using Agro.LibCore;
using QuestPDF.Drawing;
using SketchMap.Office;
using SketchMapConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Vml;
using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DOX = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.IO.Compression;
using SketchMapConsole.Util;

namespace SketchMap
{
    internal class PdfGenerator
    {
        static IContainer Block(IContainer container)
        {
            return container
                .Border(0.1f)
                .Background(Colors.White)//Colors.Grey.Lighten3)
                .ShowOnce()
                //.MinWidth(50)
                .MinHeight(0.5f, Unit.Centimetre)
                .AlignCenter()
                .AlignMiddle();
        }
        static IContainer DefCellStyle(IContainer c1, float minHeight, uint row, uint col, TableProp t)
        {
            var rows = t.rows.Count;
            var wi = 0.2f;
            var c2 = row < rows - 1 ? c1.Border(wi) : c1.BorderTop(wi).BorderBottom(wi);
            if (row == rows - 1)
            {
                if (col == 0)
                {
                    c2 = c2.BorderLeft(wi);
                }
                else if (col == t.Cols - 1)
                {
                    c2 = c2.BorderRight(wi);
                }
            }
            c2 = c2.Background(Colors.White)//Colors.Grey.Lighten3)
            .ShowOnce()
            .MinHeight(minHeight, Unit.Centimetre)
            .AlignCenter()
            .AlignMiddle();
            return c2;
        }
        private readonly OpenXmlWord doc;
        public PdfGenerator(OpenXmlWord doc)
        {
            this.doc = doc;
            //FontManager.RegisterFont(File.OpenRead($"{AppPath.FontPath}esri_40.ttf"));
            //FontManager.RegisterFont(File.OpenRead($"{AppPath.FontPath}simhei.ttf"));
            //FontManager.RegisterFont(File.OpenRead($"{AppPath.FontPath}simfang.ttf"));
            //FontManager.RegisterFont(File.OpenRead($"{AppPath.FontPath}simsun.ttc"));
            static void RegisterFont(string fontName)
            {
                FontManager.RegisterFontFromEmbeddedResource(EmbeddedResourceUtil.GetFontPath(fontName));
            }
            RegisterFont("simhei.ttf");
            RegisterFont("simsun.ttc");
        }
        public void SavePdf(string outFile)
        {
            var body = doc.Doc.MainDocumentPart!.Document.Body!;

            //var styles = doc.ExtractStylesPart(false);
            //Console.WriteLine(styles);

            var lst = new List<TableProp>();
            var tables = body.Elements<DOX.Table>();
            int tableIndex = 0;
            foreach (var table in tables)
            {
                //Console.WriteLine($"begin dump table {tableIndex}...");
                var tableProp = DumpTable(table);
                lst.Add(tableProp);
                tableProp.TableIndex = tableIndex++;
            }
            CreatePdf(lst, outFile);
        }
        private void CreatePdf(List<TableProp> lst, string outFile)
        {
            // code in your main method
            Document.Create(container =>
            {
                foreach (var tableProp in lst)
                {
                    container.Page(page =>
                    {
                        page.Size(new PageSize(230f, 152f, Unit.Millimetre));// 842f,595.4f));// PageSizes.A4);
                        page.Margin(1, Unit.Centimetre);
                        page.MarginTop(1.3f, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(FontUtil.AdjustFontFamily("宋体")));
                        CreateTable(page.Content(), tableProp);
                    });
                }
            })
            .GeneratePdf(outFile);
        }

        private void SetCellPic(IContainer cell, CellProp c)
        {
            if (c.ImageData != null)
            {
                var width = c.ImageWidth;
                var height = c.ImageHeight;
                cell.Width(width, Unit.Centimetre).Height(height, Unit.Centimetre).Image(c.ImageData, ImageScaling.Resize);
            }
            //if (File.Exists(picFile))
            //{
            //    byte[] imageData = File.ReadAllBytes(picFile);
            //    cell.Width(width, Unit.Centimetre)
            //                             .Height(height, Unit.Centimetre).Image(imageData, ImageScaling.Resize);
            //}
        }
        private void SetCellText(IContainer block, CellProp c)
        {
            var bt = block.Text(c.Text);
            if (c.FontFamily.Length > 0)
            {
                var fontFamily = FontUtil.AdjustFontFamily(c.FontFamily);
                bt.FontFamily(fontFamily);
            }
            if (c.FontSize > 0)
            {
                bt.FontSize((float)c.FontSize / 2);
            }
        }
        private void CreateTable(IContainer container, TableProp tableProp)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    var dic = tableProp.GetColWidth();
                    for (uint i = 0; i < tableProp.Cols; ++i)
                    {
                        if (dic.TryGetValue(i, out var wi))
                        {
                            columns.ConstantColumn((float)wi, Unit.Centimetre);
                        }
                        else
                        {
                            columns.RelativeColumn();
                        }
                    }
                });
                for (uint iRow = 0; iRow < tableProp.rows.Count; ++iRow)
                {
                    var r = tableProp.rows[(int)iRow];
                    foreach (var c in r)
                    {
                        var cell = table.Cell().Row(iRow + 1).Column(c.Col + 1);
                        if (c.VerticalMerge == DOX.MergedCellValues.Continue)
                        {
                            cell.Element(c1 => c1.MinHeight(r.Height, Unit.Centimetre));
                            continue;
                        }
                        if (c.RowSpan > 0)
                        {
                            cell.RowSpan(c.RowSpan);
                        }
                        if (c.ColSpan > 0)
                        {
                            cell.ColumnSpan(c.ColSpan);
                        }

                        var block = cell.Element(c1 => DefCellStyle(c1, r.Height, iRow, c.Col, tableProp));
                        if (c.ImageData != null)
                        {
                            SetCellPic(block, c);
                        }
                        else if (c.Text.Length > 0)
                        {
                            SetCellText(block, c);
                        }
                    }
                }
            });
        }

        private TableProp DumpTable(DOX.Table table)
        {
            var mainPart = doc.Doc.MainDocumentPart!;
            var tableProp = new TableProp();
            var r = 0;
            foreach (var row in table.Elements<DOX.TableRow>())
            {
                var rowProp = new RowProp();
                //var str = new StringBuilder($"第{r}行：");
                var trp = row.TableRowProperties;
                if (trp != null)
                {
                    foreach (var p in trp.ChildElements)
                    {
                        if (p is DOX.TableRowHeight rh)
                        {
                            var cm = rh.Val * 21.0 / 11960;
                            //str.Append($" RowHeight={rh.Val} cm={cm}");
                            rowProp.Height = (float)cm;
                        }
                    }
                }
                //Console.WriteLine(str.ToString());

                var c = 0;
                foreach (var cell in row.Elements<DOX.TableCell>())
                {
                    var cellProp = new CellProp(c, cell.InnerText);
                    //str.Clear();
                    //str.Append($"[{r},{c}] text={cell.InnerText}");

                    if (cell.InnerText.Length > 0)
                    {
                        var cellPara = cell.Elements<DOX.Paragraph>()?.First();
                        var cellRun = cellPara?.Elements<DOX.Run>()?.First();
                        if (cellRun?.ChildElements != null)
                        {
                            foreach (var p in cellRun.ChildElements)
                            {
                                if (p is DOX.RunProperties rp)
                                {
                                    if (rp.FontSize != null)
                                    {
                                        cellProp.FontSize = (uint)SafeConvertAux.ToInt32(rp.FontSize?.Val?.Value);
                                    }
                                    var rf = rp.RunFonts;
                                    if (rf != null)
                                    {
                                        cellProp.FontFamily = rf.EastAsia?.Value ?? "宋体";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var drawing = OpenXmlWord.FindCellDrawing(cell);
                        if (drawing != null)
                        {
                            //得到图像的内嵌ID（外嵌没做处理）
                            var inline = drawing.Inline;
                            if (inline != null)
                            {
                                var extent = inline.Extent;
                                var pic = inline.Graphic.GraphicData.GetFirstChild<DocumentFormat.OpenXml.Drawing.Pictures.Picture>();
                                var embed = pic.BlipFill.Blip.Embed.Value;

                                //得到图像流
                                var part = mainPart!.GetPartById(embed);
                                using var stream = part.GetStream();

                                //流转2进制
                                byte[] bytes = new byte[stream.Length];
                                stream.Read(bytes, 0, bytes.Length);
                                cellProp.ImageData = bytes;
                                cellProp.ImageWidth = (float)(extent.Cx.Value / 360000f);
                                cellProp.ImageHeight = (float)(extent.Cy.Value / 360000f);
                            }
                        }
                    }
                    var gridSpan = 1;
                    var tcp = cell.TableCellProperties;
                    if (tcp != null)
                    {
                        foreach (var p in tcp.ChildElements)
                        {
                            if (p is DOX.TableCellWidth w)
                            {
                                var wi = SafeConvertAux.ToDouble(w.Width.Value);
                                var cm = wi * 21.0 / 11960;
                                //Console.WriteLine($"c={c},ColWidth={wi} cm={cm}");
                                //str.Append($" CellWidth[{wi}]={cm}");
                                cellProp.Width = cm;
                            }
                            else if (p is DOX.GridSpan gs)
                            {
                                gridSpan = gs.Val.Value;
                                //str.Append($" GridSpan={gridSpan}");
                                cellProp.ColSpan = (uint)gridSpan;
                            }
                            else if (p is DOX.VerticalMerge vm)
                            {
                                //str.Append($" VerticalMerge:val={vm.Val}");
                                cellProp.SetVerticalMerge(vm.Val);
                            }
                        }
                    }
                    //Console.WriteLine(str.ToString());
                    c += gridSpan;
                    rowProp.Add(cellProp);
                }
                ++r;
                tableProp.Append(rowProp);
            }
            return tableProp;
        }
    }

    internal class CellProp
    {
        public readonly uint Col;
        public double Width;
        public uint RowSpan;
        public uint ColSpan;
        public string Text = string.Empty;
        public string ImagePath = string.Empty;
        public uint FontSize;//指定以半磅 (1/144 英寸) 指定的正度量值。
        public string FontFamily = string.Empty;
        /// <summary>
        /// 图片宽度（厘米）
        /// </summary>
        public float ImageWidth;
        /// <summary>
        /// 图片高度（厘米）
        /// </summary>
        public float ImageHeight;
        public byte[] ImageData;
        public DOX.MergedCellValues? VerticalMerge { get; private set; }
        public CellProp(int col, string text)
        {
            Col = (uint)col;
            Text = text;
        }
        public void SetVerticalMerge(DOX.MergedCellValues? v)
        {
            VerticalMerge = v;
            if (v == DOX.MergedCellValues.Restart)
            {
                RowSpan = 1;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Col={Col} ColSpan={ColSpan} RowSpan={RowSpan} Width={Width}cm Text={Text} VerticalMerge={VerticalMerge}");
            if (FontSize > 0)
            {
                sb.Append($" FontSize={FontSize}");
            }
            if (FontFamily.Length > 0)
            {
                sb.Append($" FontFamily={FontFamily}");
            }
            return sb.ToString();
        }
    }
    internal class RowProp : List<CellProp>
    {
        public float Height;

        public uint Cols
        {
            get
            {
                uint cnt = 0;
                foreach (var cellProp in this)
                {
                    uint n = cellProp.Col + cellProp.ColSpan;
                    if (cnt < n) cnt = n;
                }
                return cnt;
            }
        }
        public CellProp? FindCell(uint col)
        {
            return Find(it => it.Col == col);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var cell in this)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append(cell.ToString());
            }
            return sb.ToString();
        }
    }
    internal class TableProp
    {
        public int TableIndex;
        public readonly List<RowProp> rows = new();
        public uint Cols
        {
            get
            {
                uint cnt = 0;
                foreach (var r in rows)
                {
                    if (cnt < r.Cols) cnt = r.Cols;
                }
                return cnt;
            }
        }
        public Dictionary<uint, double> GetColWidth()
        {
            var t = new TableColWidthCalculator(Cols);
            foreach (var r in rows)
            {
                t.Add(r);
            }
            var dic = t.Calc();
            return dic;
        }
        public void Append(RowProp row)
        {
            if (rows.Count > 0)
            {
                //var preRow = rows[rows.Count - 1];
                foreach (var cell in row)
                {
                    if (cell.VerticalMerge == DOX.MergedCellValues.Continue)
                    {
                        for (var i = rows.Count; --i >= 0;)
                        {
                            var c = rows[i];
                            var preCell = c.FindCell(cell.Col);
                            if (preCell?.VerticalMerge == DOX.MergedCellValues.Restart)
                            {
                                ++preCell.RowSpan;
                                break;
                            }
                        }
                        //var preCell = preRow.FindCell(cell.Col);
                        //if (preCell != null)
                        //{
                        //    ++preCell.RowSpan;
                        //}
                    }

                }
            }
            rows.Add(row);
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < rows.Count; ++i)
            {
                if (i > 0) sb.AppendLine();
                sb.Append($"第{i}行：Height={rows[i].Height}");
                sb.AppendLine();
                sb.Append(rows[i]);
            }
            //Console.WriteLine("begin dump TableProperties....");
            return sb.ToString();
        }
    }

    internal class TableColWidthCalculator
    {
        class Node
        {
            public double Width;
            public readonly List<uint> ColList = new();
            public bool IsEqual(Node other)
            {
                if (Width != other.Width || ColList.Count != other.ColList.Count) return false;
                foreach (var col in ColList)
                {
                    if (other.ColList.IndexOf(col) < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        private readonly Dictionary<uint, double> dicColWidth = new();
        private readonly List<Node> nodes = new();
        private readonly uint ColCount;
        public TableColWidthCalculator(uint colCount)
        {
            ColCount = colCount;
        }

        public void Add(RowProp row)
        {
            foreach (var c in row)
            {
                if (c.ColSpan == 0)
                {
                    dicColWidth[c.Col] = c.Width;
                }
                else
                {
                    var node = new Node()
                    {
                        Width = c.Width,
                    };
                    for (uint i = 0; i < c.ColSpan; ++i)
                    {
                        node.ColList.Add(c.Col + i);
                    }
                    if (nodes.Find(it => it.IsEqual(node)) == null)
                    {
                        nodes.Add(node);
                    }
                }
            }
        }
        public Dictionary<uint, double> Calc()
        {
            bool recalc = true;
            while (recalc && dicColWidth.Count < ColCount && nodes.Count > 0)
            {
                recalc = false;
                for (var k = nodes.Count; --k >= 0;)
                {
                    var n = nodes[k];
                    for (var i = n.ColList.Count; --i >= 0;)
                    {
                        var col = n.ColList[i];
                        if (dicColWidth.TryGetValue(col, out var width))
                        {
                            n.Width -= width;
                            n.ColList.RemoveAt(i);
                        }
                    }
                    if (n.ColList.Count == 1)
                    {
                        recalc = true;
                        dicColWidth[n.ColList[0]] = n.Width;
                        n.ColList.Clear();
                    }
                    if (n.ColList.Count == 0)
                    {
                        nodes.RemoveAt(k);
                    }
                }
            } 
            return dicColWidth;
        }
    }
}
