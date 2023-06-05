// See https://aka.ms/new-console-template for more information
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;
using System.Numerics;
using SkiaSharp;
using Agro.GIS;
using Agro.LibCore;
using ConsoleApp1;
using SketchMap;
//using NPOI.XWPF.UserModel;
using DocumentFormat.OpenXml.Packaging;
using SketchMapConsole;

if (false)
{
    OpenXmlTest.ToPdf();// outFile);
    return;
}

//new QuestPDFTest().Test();
//return;
//var outFile=NPOITest();
//OpenXmlTest.ToPdf();// outFile);
//SketchMapExportTest.Test();
if (args.Length > 0)
{
    foreach (var arg in args)
    {
        Console.WriteLine($"Argument={arg}");
    }
    new Exporter().Test(args);
}
else
{
    Console.WriteLine("No arguments");
}

//static void NPOIAddPic(XWPFTableCell cell,string picFile,double width,double height)
//{
//    //var cell = table.GetRow(row).GetCell(col);
//    //cell.SetText("aaa");
//    while (cell.Paragraphs.Count > 0)
//    {
//        cell.RemoveParagraph(0);
//    }

//    var r2 = cell.AddParagraph().CreateRun();
//    var widthEmus = width * 10 * 36000;// (int)(400.0 * 9525/2);
//    var heightEmus = height* 10 * 36000;// (int)(300.0 * 9525/2);


//    //var picFile = @"D:\tmp\新建文件夹 (2)\Test\220581100007010001\2205811000070100002.jpg";
//    using FileStream picData = new(picFile, FileMode.Open, FileAccess.Read);
//    r2.AddPicture(picData, (int)PictureType.JPEG, "image1", (int)widthEmus, (int)heightEmus);
//}

//static void NPOIDeleteTable(XWPFDocument doc,XWPFTable table)
//{
//    int tablesIndex = doc.GetPosOfTable(table);
//    doc.RemoveBodyElement(tablesIndex);
//}
//static string NPOITest()
//{
//    //var template = @"Template1.docx";
//    var path = AppDomain.CurrentDomain.BaseDirectory+ @"Data\Template\";
//    var template = path+ @"农村土地承包经营权承包地块示意图.docx";

//    using var rs = File.OpenRead(template);

//    var generateFile =path+ @"output1.docx";
//    using var ws = File.Create(generateFile);
//    var doc = new XWPFDocument(rs);
//    var ti = 0;
//    foreach(var t in doc.Tables)
//    {
//        Console.WriteLine($"ti={ti++}");
//        for(var r = 0; r < t.Rows.Count; ++r)
//        {
//            var row=t.Rows[r];
//            var c = 0;
//            row.GetTableCells().ForEach(cell =>
//            {
//                var msg = $"row={r},c={c},text={cell.GetText()}";
//                Console.WriteLine(msg);
//                if (r == 3 && c == 1)
//                {
//                    cell.SetText("张三");
//                }
//                foreach (var para in cell.Paragraphs)
//                {
//                    var ctp = para.GetCTP();
//                    for (int dwI = 0; dwI < ctp.SizeOfBookmarkStartArray(); dwI++)
//                    {
//                        var bookmark = ctp.GetBookmarkStartArray(dwI);

//                        Console.WriteLine("bookmark=" + bookmark.name);
//                        //ctp.RemoveR()
//                        //if (dataMap.containsKey(bookmark.getName()))
//                        //{

//                        //XWPFRun run = para.CreateRun();
//                        //run.SetText($"{bookmark.name}");

//                        //Node firstNode = bookmark.GetDomNode();
//                        //Node nextNode = firstNode.getNextSibling();
//                        //while (nextNode != null)
//                        //{
//                        //    // 循环查找结束符
//                        //    String nodeName = nextNode.getNodeName();
//                        //    if (nodeName.equals(BOOKMARK_END_TAG))
//                        //    {
//                        //        break;
//                        //    }

//                        //    // 删除中间的非结束节点，即删除原书签内容
//                        //    Node delNode = nextNode;
//                        //    nextNode = nextNode.getNextSibling();

//                        //    ctp.GetDomNode().removeChild(delNode);
//                        //}

//                        //if (nextNode == null)
//                        //{
//                        //    // 始终找不到结束标识的，就在书签前面添加
//                        //    ctp.getDomNode().insertBefore(run.getCTR().getDomNode(), firstNode);
//                        //}
//                        //else
//                        //{
//                        //    // 找到结束符，将新内容添加到结束符之前，即内容写入bookmark中间
//                        //    ctp.getDomNode().insertBefore(run.getCTR().getDomNode(), nextNode);
//                        //}
//                        //}
//                        Console.WriteLine(para.ToString());
//                        //foreach (var placeholder in placeHolderDictionary)
//                        //{
//                        //    if (para.ParagraphText.Contains(placeholder))
//                        //    {
//                        //        para.ReplaceText(placeholder, "Nissl");
//                        //    }
//                        //}
//                    }
//                }


//                ++c;
//            });
//        }
//    }


//    foreach (var para in doc.Paragraphs)
//    {
//        var ctp = para.GetCTP();
//        for (int dwI = 0; dwI < ctp.SizeOfBookmarkStartArray(); dwI++)
//        {
//            var bookmark = ctp.GetBookmarkStartArray(dwI);
//            Console.WriteLine(bookmark.name);
//            //if (dataMap.containsKey(bookmark.getName()))
//            //{

//            //    XWPFRun run = xwpfParagraph.createRun();
//            //    run.setText(dataMap.get(bookmark.getName()));

//            //    Node firstNode = bookmark.getDomNode();
//            //    Node nextNode = firstNode.getNextSibling();
//            //    while (nextNode != null)
//            //    {
//            //        // 循环查找结束符
//            //        String nodeName = nextNode.getNodeName();
//            //        if (nodeName.equals(BOOKMARK_END_TAG))
//            //        {
//            //            break;
//            //        }

//            //        // 删除中间的非结束节点，即删除原书签内容
//            //        Node delNode = nextNode;
//            //        nextNode = nextNode.getNextSibling();

//            //        ctp.getDomNode().removeChild(delNode);
//            //    }

//            //    if (nextNode == null)
//            //    {
//            //        // 始终找不到结束标识的，就在书签前面添加
//            //        ctp.getDomNode().insertBefore(run.getCTR().getDomNode(), firstNode);
//            //    }
//            //    else
//            //    {
//            //        // 找到结束符，将新内容添加到结束符之前，即内容写入bookmark中间
//            //        ctp.getDomNode().insertBefore(run.getCTR().getDomNode(), nextNode);
//            //    }
//            //}
//            Console.WriteLine(para.ToString());
//            //foreach (var placeholder in placeHolderDictionary)
//            //{
//            //    if (para.ParagraphText.Contains(placeholder))
//            //    {
//            //        para.ReplaceText(placeholder, "Nissl");
//            //    }
//            //}
//        }
//    }


//    var table = doc.Tables[0];
//    //var cell = table.GetRow(1).GetCell(1);
//    var picFile = @"D:\tmp\新建文件夹 (2)\Test\220581100007010001\2205811000070100002.jpg";
//    for(var r = 1; r <= 2; ++r)
//    {
//        for(var c = 1; c < 4; ++c)
//        {
//            NPOIAddPic(table.GetRow(r).GetCell(c), picFile, 4.49, 4.83);
//        }
//    }
   

//    picFile = @"D:\tmp\新建文件夹 (2)\Test\220581100007010001\220581100007010001.jpg";
//    var cell = table.GetRow(1).GetCell(0);
//    NPOIAddPic(cell, picFile,6.37, 6.37);

//    if (false)
//    {
//        NPOIDeleteTable(doc, doc.Tables[1]);

//        var summary = doc.BodyElements.Count;
//        var ss = doc.BodyElements[summary - 1];
//        doc.RemoveBodyElement(summary - 1);
//    }

//    //int tablesIndex = doc.GetPosOfTable(doc.Tables[1]);
//    //doc.RemoveBodyElement(tablesIndex);
//    /*
//    //cell.SetText("aaa");
//    while(cell.Paragraphs.Count> 0)
//    {
//        cell.RemoveParagraph(0);
//    }
   
//    var r2 =cell.AddParagraph().CreateRun();
//    var widthEmus = 4.49 * 10 * 36000;// (int)(400.0 * 9525/2);
//    var heightEmus = 4.83*10* 36000;// (int)(300.0 * 9525/2);
   

   
//    using (FileStream picData = new(picFile, FileMode.Open, FileAccess.Read))
//    {
//        var pic=r2.AddPicture(picData, (int)PictureType.JPEG, "image1", (int)widthEmus, (int)heightEmus);
//    }*/

   
//    doc.Write(ws);

//    return generateFile;

//}


static void Test()
{

    Console.WriteLine("Hello, World!");
    var c = System.Drawing.Color.FromArgb(215, 225, 0);
    var str = $"#{c.A:x2}{c.R:x2}{c.G:x2}{c.B:x2}".ToUpper();
    str = System.Drawing.ColorTranslator.ToHtml(c);
    Console.WriteLine(str);// System.Drawing.Color.FromArgb(255, 255, 0).ToString());
    if (true)
    {
        SketchMapExportTest.Test();
        //var pageLayout = new PageLayout();
        //pageLayout.OpenDocument(@"./地块四至示意图.kpd", false);
        //pageLayout.SaveToImage("./ttx.jpg", 300, i => { }, NotCancelTracker.Instance);
    }
    int width = 1200;// 9921;// 640;
    int height = 600;// 9921;// 480;

    // Creates a new image with empty pixel data. 
    using Image<Rgba32> image = new(width, height);

    //IPath yourPolygon = new Star(x: 100.0f, y: 100.0f, prongs: 5, innerRadii: 20.0f, outerRadii: 30.0f);

    //image.Mutate(x => x.Fill(Color.Red, yourPolygon));

    if (false)
    {
        // The options are optional
        var options = new DrawingOptions();
        IBrush brush = Brushes.Horizontal(Color.Red, Color.Blue);
        IPen pen = Pens.DashDot(Color.Green, 5);
        IPath yourPolygon = new Star(x: 100.0f, y: 100.0f, prongs: 5, innerRadii: 20.0f, outerRadii: 30.0f);

        // draws a star with Horizontal red and blue hatching with a dash dot pattern outline.
        image.Mutate(x => x.Fill(options, brush, yourPolygon)
                           .Draw(options, pen, yourPolygon));


    }
    if (true)
    {
        // The options are optional
        //var options = new TextGraphicsOptions()
        //{
        //    ApplyKerning = true,
        //    TabWidth = 8, // a tab renders as 8 spaces wide
        //    WrapTextWidth = 100, // greater than zero so we will word wrap at 100 pixels wide
        //    HorizontalAlignment = HorizontalAlignment.Right // right align
        //};

        var brush = Brushes.Horizontal(Color.Red, Color.Blue);
        IPen? pen = null;// Pens.DashDot(Color.Green, 5);
        string text = "字体的路径";// "sample text";
                              //var font = new Font(new FontFamily("微软雅黑"));

        var fonts = new FontCollection();
        var fontFamily = fonts.Add($"{AppPath.FontPath}SIMHEI.TTF"); //字体的路径（电脑自带字体库，去copy出来）
        var font = new Font(fontFamily, 100, FontStyle.Bold);

        try
        {
            // draws a star with Horizontal red and blue hatching with a dash dot pattern outline.
            image.Mutate(x => x.DrawText(text, font, brush, pen, new PointF(100, 300)));
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }

    }

    {
        var simplePath = new Polygon(new LinearLineSegment(
                          new Vector2(10, 10),
                          new Vector2(200, 150),
                          new Vector2(50, 300)));

        var hole1 = new Polygon(new LinearLineSegment(
                        new Vector2(37, 85),
                        new Vector2(93, 85),
                        new Vector2(65, 137)));

        var options = new DrawingOptions();
        IBrush brush = Brushes.Horizontal(Color.Red, Color.Blue);

        image.Mutate(x => x.Fill(options, brush, simplePath.Clip(hole1)));

    }

    image.SaveAsJpeg("./test111.jpg");

    var hi = 708;//7086
    var bmp = new SKBitmap(hi, hi);
    using var canvas = new SKCanvas(bmp);
    SkiaSharpTest(canvas, bmp);
    SkiaSharpTest1(canvas, bmp);

    //保存成图片文件
    using SKImage img = SKImage.FromBitmap(bmp);
    using SKData p = img.Encode(SKEncodedImageFormat.Jpeg, 100);
    //return p.ToArray();
    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test1.jpg");//, name);
    Console.WriteLine(path);
    using var stream = File.Create(path);
    stream.Write(p.ToArray(), 0, p.ToArray().Length);
}

static double POINTS2PIXELS2(double x, double dpi)
{
    double dInches = x / 72;// POINTS_PER_INCH;
    var outSize = dInches * (double)dpi;
    outSize = Math.Floor(outSize + 0.5);
    return outSize;
}

static void SkiaSharpTest(SKCanvas canvas, SKBitmap bmp)
{
    //var imageName = "test1";
    //var name = imageName + ".jpg";
    //var hi = 708;//7086
    //var bmp = new SKBitmap(hi, hi);
    var str = "测试";// imageName;

    str= ((char)177).ToString();

    //using var canvas = new SKCanvas(bmp);
    var r = new Random();
    int num = r.Next(0, 9);


    // 获取宋体在字体集合中的下标
    var lst=SKFontManager.Default.FontFamilies.ToList();
    if (lst != null)
    {
        foreach(var f in lst)
        {
            Console.WriteLine(f);
        }
    }
    //var index = SKFontManager.Default.FontFamilies.ToList().IndexOf("Microsoft YaHei Boot");// "宋体");
    var index = SKFontManager.Default.FontFamilies.ToList().IndexOf("ESRI North");// "宋体");
    //if(index<0) index = 0;
    // 创建宋体字形
    var songtiTypeface = SKFontManager.Default.GetFontStyles(index).CreateTypeface(0);

    //canvas.DrawColor(colors[num]); // colors是图片背景颜色集合，这里代码就不贴出来了，随机找一个
    using var sKPaint = new SKPaint();
    sKPaint.Color = SKColors.Red;//字体颜色
    sKPaint.TextSize =(float)POINTS2PIXELS2(30,300);//字体大小
    sKPaint.IsAntialias = true;//开启抗锯齿
    sKPaint.StrokeWidth = 12;

    sKPaint.Typeface = SKFontManager.Default.MatchFamily("ESRI North");// songtiTypeface;// SKTypeface.FromFamilyName("SimSun");//, SKTypefaceStyle.Bold);//字体
    var size = new SKRect();
    sKPaint.MeasureText(str, ref size);//计算文字宽度以及高度
    float temp = (128 - size.Size.Width) / 2;
    float temp1 = (128 - size.Size.Height) / 2;
    //canvas.DrawText(str, temp, temp1 - size.Top, sKPaint);//画文字
    canvas.DrawText(str, 0, size.Size.Height, sKPaint);
    //return stream;
}

static void SkiaSharpTest1(SKCanvas canvas, SKBitmap info1)
{
    //var hi = 708;//7086
    //var bmp = new SKBitmap(hi, hi);
    //var str = "测试";// imageName;

    var info = new System.Drawing.Rectangle(0,0,info1.Width,info1.Height);

    //using var canvas = new SKCanvas(bmp);

    //var info = canvas.g;
    // Create the path
    var path = new SKPath();
    path.FillType = SKPathFillType.EvenOdd;

    var n = 0;
    if (n == 0)
    {
        // Define the first contour
        path.MoveTo(0.5f * info.Width, 0.1f * info.Height);
        path.LineTo(0.2f * info.Width, 0.4f * info.Height);
        path.LineTo(0.8f * info.Width, 0.4f * info.Height);
        path.LineTo(0.5f * info.Width, 0.1f * info.Height);

        // Define the second contour
        path.MoveTo(0.5f * info.Width, 0.6f * info.Height);
        path.LineTo(0.2f * info.Width, 0.9f * info.Height);
        path.LineTo(0.8f * info.Width, 0.9f * info.Height);
        path.Close();
    }else if (n == 1)
    {
        path.MoveTo(10, 10);
        path.LineTo(200, 150);
        path.LineTo(50, 300);
        path.Close();

        path.MoveTo(37, 85);
        path.LineTo(93, 85);
        path.LineTo(65, 137);
        path.Close();
    }

    // Create two SKPaint objects
    SKPaint strokePaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = SKColors.Magenta,
        StrokeWidth = 5,

    };

    SKPaint fillPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = SKColors.Cyan,
        //BlendMode=SKBlendMode.Xor
    };

    // Fill and stroke the path
    canvas.DrawPath(path, fillPaint);
    canvas.DrawPath(path, strokePaint);
}