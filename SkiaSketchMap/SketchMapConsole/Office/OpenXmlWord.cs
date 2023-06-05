using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.ExtendedProperties;
using System.Xml.Linq;
using System.Xml;

namespace SketchMap.Office
{
    public class OpenXmlWord:IDisposable
    {
        private WordprocessingDocument? _doc;
        //private IDictionary<string, BookmarkStart>? bookmarkMap = null;// new Dictionary<string, BookmarkStart>();
        public WordprocessingDocument Doc { get { return _doc!; } }
        public IEnumerable<Paragraph>? Paragraphs
        {
            get
            {
                var body = _doc?.MainDocumentPart?.Document?.Body;
                return body?.Elements<Paragraph>();
            }
        }
        public string Path { get;private set; }=string.Empty;
        public void Open(string path, bool isEditable=true)
        {
            Path = path;
            _doc = WordprocessingDocument.Open(path, isEditable);
        }
        public void DeleteLastParagraph()
        {
            var body = _doc?.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                var lastC = body.Elements<Paragraph>().Last();
                body.RemoveChild(lastC);
            }
        }
        public void DeleteTable(int index,bool delLastParagraph=true)
        {
            var body = _doc?.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                var table = GetTable(index);
                if (table != null)
                {
                    body.RemoveChild(table);

                    if (delLastParagraph)
                    {
                        DeleteLastParagraph();
                    }
                }
            }
        }
        public Table? GetTable(int index)
        {
            //var doc = _doc!;
            var body = _doc?.MainDocumentPart?.Document?.Body;
            var lst=body?.Elements<Table>();
            if (lst != null)
            {
                var i = 0;
                foreach (var t in lst)
                {
                    if (i == index)
                    {
                        return t;
                    }
                    ++i;
                }
            }
            //var table=body.GetFirstChild<Table>();
            
            //for(var i=0;table!= null ; ++i)
            //{
            //    if (i == index)
            //    {
            //        return table;
            //    }
            //    table=table.NextSibling<Table>();
            //}
            return null;
        }
        public TableRow? GetTableRow(Table table,int row)
        {
            var r = 0;
            foreach (var tr in table.Elements<TableRow>())
            {
                if (r == row)
                {
                    return tr;
                }
                ++r;
            }
            return null;
        }
        public TableCell? GetTableCell(TableRow row, int colindex)
        {
            var c = 0;
            foreach (var cell in row.Elements<TableCell>())
            {
                if (c == colindex) { return cell; }
                ++c;
            }
            return null;
        }
        public TableCell? GetTableCell(Table table, int row, int col)
        {
            var tr=GetTableRow(table, row);
            return tr==null?null:GetTableCell(tr, col);
        }
        public TableCell? GetTableCell(int tableIndex, int row, int col)
        {
            var table=GetTable(tableIndex);
            if (table == null)
            {
                return null;
            }
            var tr = GetTableRow(table, row);
            return tr == null ? null : GetTableCell(tr, col);
        }
        public void SetBookmarkValue(string bookmarkName,string value)
        {
            var lst = _doc?.MainDocumentPart?.RootElement?.Descendants<BookmarkStart>();
            if (lst != null)
            {
                foreach (var bookmarkStart in lst)
                {
                    if (bookmarkName == bookmarkStart.Name)
                    {
                        var bookmarkText = bookmarkStart.NextSibling<Run>();
                        if (bookmarkText != null)
                        {
                            var t = bookmarkText.GetFirstChild<Text>();
                            if (t != null)
                            {
                                t.Text = value;
                            }
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 设置表格单元值
        /// </summary>
        /// <param name="tableIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="colIndex"></param>
        /// <param name="value"></param>
        public void SetCellValue(int tableIndex, int rowIndex, int colIndex, string value)
        {
            var table = GetTable(tableIndex);
            if (table == null)
            {
                return;
            }
            var cell = GetTableCell(table, rowIndex, colIndex);
            if (cell == null)
            {
                return;
            }
            var lst = cell.Elements<Paragraph>()?.ToList();

            cell.Append(new Paragraph(new Run(new Text(value))));

            if (lst != null)
            {
                foreach (var it in lst)
                {
                    cell.RemoveChild(it);
                }
            }
        }

        public static Drawing? FindCellDrawing(TableCell cell)
        {
            foreach (var cp in cell.ChildElements)
            {
                if (cp is Paragraph)
                {
                    foreach (var rp in cp.ChildElements)
                    {
                        if (rp is Run)
                        {
                            foreach (var rp1 in rp)
                            {
                               if(rp1 is Drawing drawing)
                                {
                                    return drawing;
                                }
                            }
                        }
                    }

                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cell"></param>
        /// <param name="fileName"></param>
        /// <param name="width">单位：厘米</param>
        /// <param name="height">单位：厘米</param>
        public void SetCellPicture(TableCell cell, string fileName, double width, double height)
        {
            var doc = _doc!;
            var lst = cell.Elements<Paragraph>()?.ToList();

            var mainPart = doc.MainDocumentPart!;

            var imagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

            using var stream = new FileStream(fileName, FileMode.Open);
            //{
            imagePart.FeedData(stream);
            //}

            var relationshipId = mainPart.GetIdOfPart(imagePart);

            //var width = 4.49;var height= 4.83;
            var widthEmus = width * 10 * 36000;// (int)(400.0 * 9525/2);
            var heightEmus = height * 10 * 36000;// (int)(300.0 * 9525/2);
            #region  Define the reference of the image.
            var element =
                 new Drawing(
                     new DW.Inline(
                         new DW.Extent() { Cx = (int)widthEmus, Cy = (int)heightEmus },// { Cx = 990000L, Cy = 792000L },
                         new DW.EffectExtent()
                         {
                             LeftEdge = 0L,
                             TopEdge = 0L,
                             RightEdge = 0L,
                             BottomEdge = 0L
                         },
                         new DW.DocProperties()
                         {
                             Id = (UInt32Value)1U,
                             Name = "Picture 1"
                         },
                         new DW.NonVisualGraphicFrameDrawingProperties(
                             new A.GraphicFrameLocks() { NoChangeAspect = true }),
            new A.Graphic(
                             new A.GraphicData(
            new PIC.Picture(
            new PIC.NonVisualPictureProperties(
                                         new PIC.NonVisualDrawingProperties()
                                         {
                                             Id = (UInt32Value)0U,
                                             Name = "New Bitmap Image.jpg"
                                         },
                                         new PIC.NonVisualPictureDrawingProperties()),
                                     new PIC.BlipFill(
                                         new A.Blip(
                                             new A.BlipExtensionList(
                                                 new A.BlipExtension()
                                                 {
                                                     Uri =
                                                        "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                                 })
                                         )
                                         {
                                             Embed = relationshipId,
                                             CompressionState =
                                             A.BlipCompressionValues.Print
                                         },
                                         new A.Stretch(
            new A.FillRectangle())),
                                     new PIC.ShapeProperties(
                                         new A.Transform2D(
                                             new A.Offset() { X = 0L, Y = 0L },
                                             new A.Extents() { Cx = (int)widthEmus, Cy = (int)heightEmus }),// { Cx = 990000L, Cy = 792000L }),
                                         new A.PresetGeometry(
                                             new A.AdjustValueList()
                                         )
                                         { Preset = A.ShapeTypeValues.Rectangle }))
                             )
                             { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                     )
                     {
                         DistanceFromTop = (UInt32Value)0U,
                         DistanceFromBottom = (UInt32Value)0U,
                         DistanceFromLeft = (UInt32Value)0U,
                         DistanceFromRight = (UInt32Value)0U,
                         EditId = "50D07946"
                     });
            #endregion
            //cell.Append(new Paragraph(new Run(new Text("Hello, World!"))));

            var picParagraph = new Paragraph(new Run(element));

            #region 设置图片水平剧中
            var paragraphProperties1 = new ParagraphProperties();
            paragraphProperties1.Append(new Justification() { Val = JustificationValues.Center });
            picParagraph.Append(paragraphProperties1);
            #endregion


            cell.AppendChild(picParagraph);

            if (lst != null)
            {
                foreach (var it in lst)
                {
                    cell.RemoveChild(it);
                }
            }
        }

        // Extract the styles or stylesWithEffects part from a 
        // word processing document as an XDocument instance.
        public XDocument? ExtractStylesPart(bool getStylesWithEffectsPart = true)
        {
            // Declare a variable to hold the XDocument.
            XDocument? styles = null;

            var document = _doc!;
            // Open the document for read access and get a reference.
            //using (var document =
            //    WordprocessingDocument.Open(fileName, false))
            //{
            // Get a reference to the main document part.
                var docPart = document.MainDocumentPart;

                // Assign a reference to the appropriate part to the
                // stylesPart variable.
                StylesPart? stylesPart = null;
                if (getStylesWithEffectsPart)
                    stylesPart = docPart.StylesWithEffectsPart;
                else
                    stylesPart = docPart.StyleDefinitionsPart;

                // If the part exists, read it into the XDocument.
                if (stylesPart != null)
                {
                using var reader = XmlNodeReader.Create(
                  stylesPart.GetStream(FileMode.Open, FileAccess.Read));
                // Create the XDocument.
                styles = XDocument.Load(reader);
            }
            //}
            // Return the XDocument instance.
            return styles;
        }

        ///// <summary>
        ///// 向指定的表中插入单元格
        ///// </summary>
        ///// <param name="tableIndex">表号</param>
        ///// <param name="tableIndex">开始插入行</param>
        ///// <param name="rowCount">插入几行数据</param>
        //public void InsertTableRowClone(int tableIndex, int startRow, int rowCount = 0)
        //{
        //    //if (doc == null)
        //    //{
        //    //    return;
        //    //}
        //    //NodeCollection tables = doc.GetChildNodes(NodeType.Table, true);
        //    //if (tables == null || tables.Count == 0 || tableIndex >= tables.Count)
        //    //{
        //    //    return;
        //    //}
        //    //Table table = tables[tableIndex] as Table;
        //    var table=GetTable(tableIndex);
        //    if (table == null)
        //    {
        //        return;
        //    }
        //    var rows = table.Elements<TableRow>();
        //    if (startRow >=rows.Count())
        //    {
        //        return;
        //    }
        //    try
        //    {
        //        //Node node = null;
        //        int rowNumber = rows.Count();
        //        if (rowCount == 0)
        //        {
        //            for (int index = 0; index < rowNumber; index++)
        //            {
        //                var row=GetTableRow(table, index)!;
        //                var newRow=(TableRow)row.Clone();
        //                table.AddChild(newRow);
        //                //node = table.Rows[index].Clone(true);
        //                //table.Rows.Add(node);
        //            }
        //        }
        //        else
        //        {
        //            for (int index = 0; index < rowCount; index++)
        //            {
        //                var row = GetTableRow(table, startRow)!;
        //                var newRow = (TableRow)row.Clone();
        //                table.InsertAt(newRow, startRow);
        //                //node = table.Rows[startRow].Clone(true);
        //                //table.Rows.Insert(table.Rows.Count, node);
        //            }
        //        }
        //        //node = null;
        //        //table = null;
        //        //tables = null;
        //    }
        //    catch (SystemException ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //    }
        //}

        private void UpdateCellText(TableCell cell)//,string text)
        {
          
            //cell.RemoveAllChildren();
            //cell.Append(new Paragraph(new Run(new Text(text))));
            var lst = cell.Elements<Paragraph>();
            foreach (var it in lst)
            {
                var jt = it.Elements<Run>();
                foreach (var k in jt)
                {
                    foreach (var t in k.Elements<Text>())
                    {
                        //if (dic.TryGetValue(t.Text, out var txt))
                        //{
                        //    t.Text = txt;
                        //}
                        //Console.WriteLine(t.Text);
                    }
                }
            }



            //// Find the first paragraph in the table cell.
            //Paragraph p = cell.Elements<Paragraph>().First();

            //// Find the first run in the paragraph.
            //Run r = p.Elements<Run>().First();

            //// Set the text for the run.
            //Text t = r.Elements<Text>().First();
            //t.Text = text;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);  // Violates rule
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_doc != null)
                {
                    _doc.Dispose();
                    _doc = null;
                }
            }
        }
    }
}
