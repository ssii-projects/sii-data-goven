using Agro.LibCore;
using DocumentFormat.OpenXml.Vml;
//using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Drawing;
using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SketchMap;
using SketchMap.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DOX = DocumentFormat.OpenXml.Wordprocessing;

namespace SketchMapConsole
{
    internal class QuestPDFTest
    {
        // for simplicity, you can also use extension method described in the "Extending DSL" section
        static IContainer Block(IContainer container)
        {
            return container
                .Border(0.1f)
                .Background(Colors.White)//Colors.Grey.Lighten3)
                .ShowOnce()
                //.MinWidth(50)
                .MinHeight(0.5f,Unit.Centimetre)
                .AlignCenter()
                .AlignMiddle();
        }
        static IContainer DefCellStyle(IContainer c1, float minHeight,uint row,uint col)
        {
            var wi = 0.2f;
            var c2 =row<7?c1.Border(wi): c1.BorderTop(wi).BorderBottom(wi);
            if (row == 7)
            {
                if (col == 0)
                {
                    c2 = c2.BorderLeft(wi);
                }
                else if (col == 9)
                {
                    c2 = c2.BorderRight(wi);
                }
                //else
                //{
                //    c2 = c2.BorderTop(wi).BorderBottom(wi);
                //}
            }
            //else
            //{
            //    c2 = c1.Border(wi);
            //}
            c2=c2.Background(Colors.White)//Colors.Grey.Lighten3)
            .ShowOnce()
            .MinHeight(minHeight, Unit.Centimetre)
            .AlignCenter()
            .AlignMiddle();
            return c2;
        }

        public void Test()
        {
            FontManager.RegisterFont(File.OpenRead("simhei.ttf"));
            FontManager.RegisterFont(File.OpenRead("simfang.ttf"));


            var path = AppDomain.CurrentDomain.BaseDirectory + @"Data/Template/";
            var docFile = path + @"农村土地承包经营权承包地块示意图.docx";
            if (!File.Exists(docFile))
            {
                Console.WriteLine($"not find file :" + docFile);
            }
            using var doc = new OpenXmlWord();
            doc.Open(docFile, false);
            var body = doc.Doc.MainDocumentPart!.Document.Body!;

            var styles = doc.ExtractStylesPart(false);
            Console.WriteLine(styles);

            var lst = new List<TableProp>();
            var tables = body.Elements<DOX.Table>();
            int tableIndex = 0;
            foreach(var table in tables)
            {
                Console.WriteLine($"begin dump table {tableIndex}...");
                var tableProp = DumpTable(table);
                lst.Add(tableProp);
                tableProp.TableIndex= tableIndex++;
            }
            CreatePdf(lst);
            //var table = doc.GetTable(0)!;

            //var tableProp=DumpTable();
            //Console.WriteLine("begin dump TableProperties...");
            //Console.WriteLine(tableProp);
            //CreatePdf(tableProp);
        }
        private void CreatePdf(List<TableProp> lst){// tableProp) { 
            var jpgFile = @"D:\tmp\新建文件夹 (2)\Test\420502201206040018\420502201206040018.jpg";
            byte[] imageData = File.ReadAllBytes(jpgFile);
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
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("宋体"));

                        //page.Header()
                        //    .Text("Hello PDF!")
                        //    .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                        var content = page.Content();
                        CreateTable(content, tableProp);
                    });
                }
                //container.Page(page =>
                //{
                //    page.Size(new PageSize(230f, 152f, Unit.Millimetre));// 842f,595.4f));// PageSizes.A4);
                //    page.Margin(2, Unit.Centimetre);
                //    page.PageColor(Colors.White);
                //    page.DefaultTextStyle(x => x.FontSize(20));

                //    page.Header()
                //        .Text("Hello PDF!")
                //        .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                //    page.Content().Table(table =>
                //    {
                //        table.ColumnsDefinition(columns =>
                //        {
                //            columns.RelativeColumn();
                //            columns.RelativeColumn();
                //            columns.RelativeColumn();
                //            columns.RelativeColumn();
                //        });

                //        table.Cell().RowSpan(2).ColumnSpan(2).Element(Block).Text("1");
                //        table.Cell().ColumnSpan(2).Element(Block).Text("2");
                //        table.Cell().Element(Block).Text("3");
                //        table.Cell().Element(Block).Text("4");
                //        table.Cell().RowSpan(2).Element(Block).Text("5");
                //        table.Cell().ColumnSpan(2).Element(Block).Text("6");
                //        table.Cell().RowSpan(2).Element(Block).Text("7");
                //        table.Cell().Element(Block).Text("8");
                //        table.Cell().Element(Block).Text("9");

                //        // the Block() method, that defines default cell style, is omitted
                //    });

                //    //page.Content()
                //    //    .PaddingVertical(1, Unit.Centimetre)
                //    //    .Column(x =>
                //    //    {
                //    //        x.Spacing(20);

                //    //        x.Item().Text(Placeholders.LoremIpsum());
                //    //        x.Item().Image(Placeholders.Image(200, 100));
                //    //    });

                //    page.Footer()
                //        .AlignCenter()
                //        .Text(x =>
                //        {
                //            x.Span("Page ");
                //            x.CurrentPageNumber();
                //        });
                //});
            })
            .GeneratePdf("hello.pdf");
        }

        private void SetCellPic(IContainer cell,string picFile,float width,float height)
        {
            if (File.Exists(picFile))
            {
                //var jpgFile = @"D:\tmp\新建文件夹 (2)\Test\420502201206040018\420502201206040018.jpg";
                byte[] imageData = File.ReadAllBytes(picFile);
                cell.Width(width, Unit.Centimetre)
                                         .Height(height, Unit.Centimetre).Image(imageData);
            }
        }
        private void CreateTable(IContainer container, TableProp tableProp)
        {
            var picFile1 = @"D:\tmp\新建文件夹 (2)\Test\420502201206040018\420502201206040018.jpg";
            //var picFile1 = @"D:\tmp\新建文件夹 (2)\杨守金\Jpeg\2205811000070100005.jpg";
            container.Table(table =>
                                     {
                                         table.ColumnsDefinition(columns =>
                                         {
                                             var dic = tableProp.GetColWidth();
                                             //if (tableProp.TableIndex == 0)
                                             //{
                                             //    dic[1] = 0.24757525;
                                             //    //dic[3] = dic[4];
                                             //    dic[8] = 4.7285117 - dic[9];
                                             //    dic[7] = 5.72408 - dic[8];
                                             //    dic[6] = 4.7285117 - dic[7];
                                             //    dic[5] = 4.47918 - dic[6];
                                             //}
                                             //else
                                             //{
                                             //    dic[1] = 4.977842809364549 - dic[0];
                                             //    dic[2] = 4.231605351170568 - dic[1];
                                             //    dic[4]= 5.227173913043479 - dic[2]-dic[3];
                                             //    dic[5] = 4.231605351170568 - dic[4];
                                             //    dic[6] = 5.476505016722408 - dic[5];
                                             //    dic[7] = 5.724080267558528 - dic[6];
                                             //}
                                             for (uint i =0;i<tableProp.Cols;++i)
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
                                             /*
                                             columns.RelativeColumn();
                                             columns.RelativeColumn();
                                             columns.RelativeColumn();
                                             columns.RelativeColumn();
                                             columns.RelativeColumn();*/
                                         });
                                         uint k = 0;
                                         foreach(var r in tableProp.rows)
                                         {
                                             foreach(var c in r)
                                             {
                                                 var cell=table.Cell().Row(k+1).Column(c.Col+1);
                                                 if(c.VerticalMerge==DOX.MergedCellValues.Continue)
                                                 {
                                                     cell.Element(c1 =>c1.MinHeight(r.Height, Unit.Centimetre));
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
                                              
                                                 var block = cell.Element(c1 =>DefCellStyle(c1,r.Height,k,c.Col));
                                                 if (k == 1 && c.Col == 0)
                                                 {
                                                     var wi = 6.37f;
                                                     var hi = 6.37f;
                                                     if (tableProp.TableIndex > 0)
                                                     {
                                                         wi = 3.53f;
                                                         hi = 3.53f;
                                                     }
                                                     SetCellPic(block, picFile1,wi,hi);
                                                 }
                                                 else
                                                 {
                                                     var text = $"[{k},{c.Col}]";
                                                     if (c.Text.Length > 0)
                                                     {
                                                         text += c.Text;
                                                     }
                                                     var bt = block.Text(text);
                                                     if (c.FontFamily.Length > 0)
                                                     {
                                                         bt.FontFamily(c.FontFamily);
                                                     }
                                                     //else
                                                     //{
                                                     //    bt.FontFamily("Calibri");
                                                     //}
                                                     if (c.FontSize > 0)
                                                     {
                                                         bt.FontSize((float)c.FontSize/2);
                                                     }
                                                     //.FontFamily(/*"黑体"*/c.FontFamily).FontSize((float)c.FontSize/2);
                                                 }



                                             }
                                             ++k;
                                         }
                                     });
        }

        private static TableProp DumpTable(DOX.Table table)
        {
            var tableProp=new TableProp();
            var r = 0;
            foreach (var row in table.Elements<DOX.TableRow>())
            {
                var rowProp = new RowProp();
                var str = new StringBuilder($"第{r}行：");
                //Console.WriteLine($"第{r}行：");
                var trp = row.TableRowProperties;
                if (trp != null)
                {
                    foreach(var p in trp.ChildElements)
                    {
                        if(p is DOX.TableRowHeight rh)
                        {
                            var cm = rh.Val * 21.0 / 11960;
                            str.Append($" RowHeight={rh.Val} cm={cm}");
                            rowProp.Height = (float)cm;
                        }
                    }
                    //var rh = row.TableRowProperties?.Elements<DOX.TableRowHeight>()?.First();
                    //if (rh != null)
                    //{
                    //    var cm = rh.Val * 21.0 / 11960;
                    //    Console.WriteLine($"RowHeight={rh.Val} cm={cm}");
                    //}
                }
                Console.WriteLine(str.ToString());

                var c = 0;
                foreach (var cell in row.Elements<DOX.TableCell>()) 
                {
                    var cellProp = new CellProp(c,cell.InnerText);
                    str.Clear();
                    str.Append($"[{r},{c}] text={cell.InnerText}");

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
                                        cellProp.FontSize = (uint)SafeConvertAux.ToInt32(rp.FontSize.Val.Value);
                                    }
                                    var rf= rp.RunFonts;
                                    if (rf != null)
                                    {
                                        cellProp.FontFamily = rf.EastAsia?.Value??"宋体";
                                    }
                                    //Console.WriteLine(rf);
                                }
                            }
                        }
                        //var cellRunProp = cellRun?.Elements<DOX.RunProperties>()?.First();
                        //if (cellRunProp?.FontSize != null)
                        //{
                        //    cellProp.FontSize = (uint)SafeConvertAux.ToInt32(cellRunProp.FontSize.Val.Value);
                        //}
                    }
                    //str =new StringBuilder($"[{r},{c}] text={cell.InnerText}");
                    //Console.WriteLine($"[{r},{c}] text={cell.InnerText}");
                    var gridSpan = 1;
                    var tcp=cell.TableCellProperties;
                    if(tcp != null)
                    {
                        foreach(var p in tcp.ChildElements)
                        {
                            if(p is DOX.TableCellWidth w)
                            {
                                var wi = SafeConvertAux.ToDouble(w.Width.Value);
                                var cm = wi * 21.0 / 11960;
                                //Console.WriteLine($"c={c},ColWidth={wi} cm={cm}");
                                str.Append($" CellWidth[{wi}]={cm}");
                                cellProp.Width = cm;
                            }else if(p is DOX.GridSpan gs)
                            {
                                gridSpan = gs.Val.Value;
                                str.Append($" GridSpan={gridSpan}");
                                cellProp.ColSpan = (uint)gridSpan;
                            }else if(p is DOX.VerticalMerge vm)
                            {
                                str.Append($" VerticalMerge:val={vm.Val}");
                                cellProp.SetVerticalMerge(vm.Val);
                            }
                        }
                        //var cw = tcp.Elements<DOX.TableCellWidth>()?.First();
                        //if (cw != null)
                        //{
                        //    var wi = SafeConvertAux.ToDouble(cw.Width.Value);
                        //    var cm = wi * 21.0 / 11960;
                        //    Console.WriteLine($"c={c},ColWidth={wi} cm={cm}");
                        //}
                    }
                    Console.WriteLine(str.ToString());
                    //++c;
                    c += gridSpan;

                    //cellProp.Col= c;
                    rowProp.Add(cellProp);
                }
                ++r;
                tableProp.Append(rowProp);
            }

            return tableProp;
        }
    }
}
