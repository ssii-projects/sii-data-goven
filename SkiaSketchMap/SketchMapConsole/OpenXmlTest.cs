using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Xml.Linq;
using SkiaSharp;
using System.Security.Policy;
//using SelectPdf;
using SketchMap.Office;
using System.Diagnostics.Contracts;
//using GemBox.Document;
//using OpenXmlPowerTools;
//using GM = GemBox.Document;
//using PdfSharpCore.Drawing;
//using PdfSharpCore.Fonts;
//using PdfSharpCore.Utils;
//using PdfSharpCore.Pdf;
//using GemBox.Document;

namespace SketchMapConsole
{
    internal class OpenXmlTest
    {
        private static Dictionary<string, string> dic = new()
        {
            {"[CBFMC]","承包方名称" },
            {"[CBDKZS]","5" },
            {"[CBDKZMJ]","0.49" },
            {"[CBFBM]","220581100007010001" }
        };
        private static void UpdateCellText(TableCell cell)//,string text)
        {
            //cell.RemoveAllChildren();
            //cell.Append(new Paragraph(new Run(new Text(text))));
            var lst = cell.Elements<Paragraph>();
            foreach(var it in lst)
            {
                var jt=it.Elements<Run>();
                foreach(var k in jt)
                {
                    foreach(var t in k.Elements<Text>())
                    {
                        if(dic.TryGetValue(t.Text,out var txt))
                        {
                            t.Text= txt;
                        }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="cell"></param>
        /// <param name="fileName"></param>
        /// <param name="width">单位：厘米</param>
        /// <param name="height">单位：厘米</param>
        private static void AddPictureToCell(WordprocessingDocument doc, TableCell cell, string fileName, double width, double height)
        {
            var lst = cell.Elements<Paragraph>()?.ToList();

            //cell.RemoveAllChildren();
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
                                             new A.Extents() { Cx = 990000L, Cy = 792000L }),
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
            cell.Append(picParagraph);

            if (lst != null)
            {
                foreach (var it in lst)
                {
                    cell.RemoveChild(it);
                }
            }
        }
        private static void DeleteLastPage(WordprocessingDocument doc)
        {
            var body = doc.MainDocumentPart.Document.Body!;
            var table = body.Elements<Table>().Last();

            body.RemoveChild(table);

            //Console.WriteLine(body.LastChild);
            var lastC = body.Elements<Paragraph>().Last();

            body.RemoveChild(lastC);
        }
        public static void ToPdf()//string docFile)
        {
            if (true)
            {
                DumpTable();
                return;

            }
            var outFile = BuildDoc();

            //var outFile = "result.docx";


            //using (DocumentConverter documentConverter = new DocumentConverter())
            //{
            //    var inFile = Path.Combine(ImagesPath.Path, @"input.xlsx");
            //    var outFile = Path.Combine(ImagesPath.Path, @"output.pdf");
            //    var format = DocumentFormat.Pdf;
            //    var jobData = DocumentConverterJobs.CreateJobData(inFile, outFile, format);
            //    jobData.JobName = "XLSX conversion to PDF";
            //    var documentWriter = new DocumentWriter();
            //    documentConverter.SetDocumentWriterInstance(documentWriter); // Handles any annotations or comments in the input spreadsheet var renderingEngine = new AnnWinFormsRenderingEngine();      
            //    documentConverter.SetAnnRenderingEngineInstance(renderingEngine);
            //    var job = documentConverter.Jobs.CreateJob(jobData);
            //    documentConverter.Jobs.RunJob(job);
            //    if (job.Status == DocumentConverterJobStatus.Success)
            //        Console.WriteLine("Success");
            //    else
            //    {
            //        Console.WriteLine("{0} Errors", job.Status);
            //        foreach (var error in job.Errors)
            //            Console.WriteLine("  {0} at {1}: {2}", error.Operation,
            //           error.InputDocumentPageNumber, error.Error.Message);
            //    }
            //}


            //var htmlFile = "test.html";

            //string line = "";
            //var testStr = new StringBuilder();
            //using (StreamReader sr = new StreamReader(htmlFile))
            //{
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        testStr.Append(line);
            //    }
            //}

            //HtmlToPdf converter = new HtmlToPdf();

            //// convert the url to pdf
            //PdfDocument doc = converter.ConvertHtmlString(testStr.ToString());

            //for (int i = 0; i < 10; i++)
            //{
            //    testStr.Replace("#ImageUrl#", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "文件夹", "文件夹下的图片"));//由于html中图片，使用相对地址解析不出来，所以使用替换方式去解决
            //    var docStr = converter.ConvertHtmlString(testStr.ToString());
            //    doc.Append(docStr);
            //}

            //// save pdf document
            //doc.Save("test.pdf");

            //// close pdf document
            //doc.Close();



            //GlobalFontSettings.FontResolver = new FontResolver();

            //var document = new PdfDocument();
            //var page = document.AddPage();

            //var gfx = XGraphics.FromPdfPage(page);
            //var font = new XFont("Arial", 20, XFontStyle.Bold);

            //var textColor = XBrushes.Black;
            //var layout = new XRect(20, 20, page.Width, page.Height);
            //var format = XStringFormats.Center;

            //gfx.DrawString("Hello World!", font, textColor, layout, format);

            //document.Save("helloworld.pdf");

            //GM.ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            //var doc =GM.DocumentModel.Load(outFile);

            //doc.Save("result1.pdf");

            //var doc1 = new Spire.Doc.Document();
            //doc1.LoadFromFile(outFile);

            ////将文档保存为PDF格式
            //doc1.SaveToFile("result1.pdf");//, topdf);//文件路径可自定义
            ////System.Diagnostics.Process.Start("result.pdf");
            ///

        }
        private static void DumpTable()//Table table)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + @"Data/Template/";
            var docFile = path + @"农村土地承包经营权承包地块示意图.docx";
            if (!File.Exists(docFile))
            {
                Console.WriteLine($"not find file :" + docFile);
            }
            //var outFile = "result.docx";
            //if (File.Exists(outFile))
            //{
            //    File.Delete(outFile);
            //}
            //File.Copy(docFile, outFile);
            //Console.WriteLine("toPdf...");
            //using var doc = WordprocessingDocument.Open(outFile, true);
            using var doc = new OpenXmlWord();
            doc.Open(docFile, false);
            //doc.Dispose();

            //var parts=doc.GetAllParts();
            //foreach(var it in parts)
            //{
            //    Console.WriteLine(it.ToString());
            //}
            var body = doc.Doc.MainDocumentPart.Document.Body!;


            var table = doc.GetTable(0)!;

            var r = 0;
            foreach (var row in table.Elements<TableRow>())
            {
                Console.WriteLine($"第{r}行：");
                var rh = row.TableRowProperties?.Elements<TableRowHeight>()?.First();
                if (rh != null)
                {
                    var cm = rh.Val * 21.0 / 11960;
                    Console.WriteLine($"RowHeight={rh.Val} cm={cm}");
                }
                var c = 0;
                foreach (var cell in row.Elements<TableCell>())
                {
                    Console.WriteLine($"[{r},{c}] text={cell.InnerText}");
                    ++c;
                }
                ++r;
            }
        }

        private static string BuildDoc() { 
            var path = AppDomain.CurrentDomain.BaseDirectory + @"Data/Template/";
            var docFile =path + @"农村土地承包经营权承包地块示意图.docx";
            if (!File.Exists(docFile))
            {
                Console.WriteLine($"not find file :" + docFile);
            }
            var outFile = "result.docx";
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            File.Copy(docFile, outFile);
            Console.WriteLine("toPdf...");
            //using var doc = WordprocessingDocument.Open(outFile, true);
            using var doc = new OpenXmlWord();
            doc.Open(outFile, true);
            //doc.Dispose();

            //var parts=doc.GetAllParts();
            //foreach(var it in parts)
            //{
            //    Console.WriteLine(it.ToString());
            //}
            var body =doc.Doc.MainDocumentPart.Document.Body!;


            var table = doc.GetTable(0);
            //body.RemoveChild(body.LastOrDefault<Paragraph>());
            //var r = 0;
            //DumpTable(table);


            //var tables=body.Elements<Table>().ToList();
            //var table = body.Elements<Table>().First();// tables.First();

            //var bookmarks= body.Elements<BookmarkStart>();

            //IDictionary<string, BookmarkStart> bookmarkMap = new Dictionary<string, BookmarkStart>();

            //foreach (BookmarkStart bookmarkStart in doc.MainDocumentPart.RootElement.Descendants<BookmarkStart>())
            //{
            //    bookmarkMap[bookmarkStart.Name] = bookmarkStart;
            //}

            //foreach (BookmarkStart bookmarkStart in bookmarkMap.Values)
            //{
            //    Run bookmarkText = bookmarkStart.NextSibling<Run>();
            //    if (bookmarkText != null)
            //    {
            //        var t = bookmarkText.GetFirstChild<Text>();
            //       t.Text = "blah";
            //    }
            //}

            #region test append pages
            if (true)
            {

                int landCount = 30;// Contractor.Lands.Length - 18;
                int pageCount = landCount / 12;
                pageCount = landCount % 12 == 0 ? pageCount : (pageCount + 1);
                for (int i = 0; i < pageCount; i++)
                {
                    var t = (Table)doc.GetTable(1)!.Clone();
                    body.AppendChild(t);
                    //InsertTableRowClone(1, 0);
                }
                int totalPage = pageCount + 2;
                doc.SetCellValue(0, 7, 5, "1/" + totalPage.ToString());
                for (int index = 0; index < pageCount + 1; index++)
                {
                    doc.SetCellValue(index+1, 4, 5, (index + 2).ToString() + "/" + totalPage.ToString());
                }
            }
            #endregion

            //var t1=(Table)doc.GetTable(1).Clone();
            //body.AppendChild(t1);
            //doc.InsertTableRowClone(1,0);


            //#region delete last page
            //doc.DeleteTable(1);
            //var lastC = body.Elements<Paragraph>().Last();
            //body.RemoveChild(lastC);
            //#endregion

            //DeleteLastPage(doc);

            //body.RemoveChild(tables[1]);

            //Console.WriteLine(body.LastChild);
            //var lastC=body.Elements<Paragraph>().Last();

            //body.RemoveChild(lastC);

            //var picFile1 = @"D:\tmp\新建文件夹 (2)\Test\220581100007010001\220581100007010001.jpg"; 
            //var picFile = @"D:\tmp\新建文件夹 (2)\Test\220581100007010001\2205811000070100002.jpg";

            var picFile1 = @"220581100007010001.jpg";
            var picFile = @"2205811000070100002.jpg";


            //var table = doc.GetTable(0);
            //body.RemoveChild(body.LastOrDefault<Paragraph>());
            var r = 0;
            foreach(var row in table.Elements<TableRow>())
            {
                var lst=row.TableRowProperties?.Elements<TableRowHeight>();
                var c = 0;
                foreach(var cell in row.Elements<TableCell>())
                {
                    Console.WriteLine($"[{r},{c}] text={cell.InnerText}");
                    UpdateCellText(cell);
                    if ((r == 1||r==2) && c>0)
                    {
                        doc.SetCellPicture(cell, picFile, 4.49, 4.83);
                        //AddPictureToCell(doc, cell, picFile, 4.49, 4.83);
                    }
                    else if (r == 1 && c == 0)
                    {
                        doc.SetCellPicture(cell, picFile1, 6.37, 6.37);
                        //AddPictureToCell(doc, cell, picFile1, 6.37, 6.37);
                    }
                    ++c;
                }
                ++r;
            }



            /*
            // Find the second row in the table.
            TableRow row = table.Elements<TableRow>().ElementAt(0);

            // Find the third cell in the row.
            TableCell cell = row.Elements<TableCell>().ElementAt(0);

            cell.Append(new Paragraph(new Run(new Text("Hello, World!"))));
            */

            //// Find the first paragraph in the table cell.
            //Paragraph p = cell.Elements<Paragraph>().First();

            //// Find the first run in the paragraph.
            //Run r = p.Elements<Run>().First();

            //// Set the text for the run.
            //Text t = r.Elements<Text>().First();
            //t.Text = "this is a test";

            //var outFile = "resultxx.docx";
            //doc.SaveAs(outFile);


            //var settings = new OpenXmlPowerTools.HtmlConverterSettings();
            //XElement html = OpenXmlPowerTools.HtmlConverter.ConvertToHtml(doc, settings);

            //Console.WriteLine(html.ToString());
            //using var writer = File.CreateText("test.html");
            //writer.WriteLine(html.ToString());
            //writer.Dispose();

            return outFile;

            //var doc1 = new Spire.Doc.Document();
            //doc1.LoadFromFile(docFile);

            ////将文档保存为PDF格式
            //doc1.SaveToFile("result.pdf");//, topdf);//文件路径可自定义
            ////System.Diagnostics.Process.Start("result.pdf");
        }
    }
}
