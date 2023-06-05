using Agro.LibCore;
using Agro.LibCore.AST;
using Agro.LibCore.Xml;
using GeoAPI.Geometries;
using System.Drawing;
using System.Xml;

namespace Agro.GIS
{
    public enum DashStyle
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
        Custom
    }
    public enum LineJoin
    {
        Miter,
        Bevel,
        Round,
        MiterClipped
    }

    public class PenProperty
    {
        /// <summary>
        /// 获取或设置线的颜色。
        /// </summary>
        public Color? StrokeColor
        {
            get;
            set;
        }
        /// <summary>
        /// 获取或设置线的宽度。
        /// </summary>
        public double StrokeThickness
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置线的风格。
        /// </summary>
        public DashStyle DashStyle;

        public LineJoin LineJoin
        {
            get;
            set;
        }
        public PenProperty(Color? clr, double thickness = 1.0)
        {
            StrokeColor = clr;
            StrokeThickness = thickness;
            DashStyle = DashStyle.Solid;
            LineJoin = LineJoin.Round;
        }

        /// <summary>
        /// 有颜色并且笔的宽度>0
        /// </summary>
        public bool IsValid
        {
            get
            {
                return StrokeColor != null && StrokeThickness > 0;
            }
        }
        //public void SetupPaint(SKPaint paint, IDisplayTransformation dts)
        //{
        //    if (StrokeThickness > 0 && StrokeColor is Color clr)
        //    {
        //        paint.Style = SKPaintStyle.Stroke;
        //        paint.Color = ColorUtil.ToSkColor(clr);
        //        paint.StrokeWidth = DpiUtil.POINTS2PIXELS2(StrokeThickness, dts.Resolution);
        //    }
        //}
        public void ReadXml(XmlElement reader)
        {
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    var name = e.Name;
                    var s = e.InnerText;
                    switch (name)
                    {
                        case "StrokeColor":
                            StrokeColor = ColorUtil.ConvertFromString(s);// SerializeUtil.ReadColor(reader, "StrokeColor");
                            break;
                        case "StrokeThickness":
                            StrokeThickness = SafeConvertAux.ToDouble(s);// reader.ReadElementContentAsDouble();
                            break;
                        case "Style":
                            //var str = SerializeUtil.ReadStringElement(reader, "Style");
                            if (s != null)
                            {
                                EnumUtil.StrToEnum(s, ref DashStyle);
                            }
                            break;
                        case "LineJoin":
                            LineJoin = (LineJoin)SafeConvertAux.ToInt32(s);// reader.ReadElementContentAsInt();
                            break;
                    }
                }
            }
        }
        public void WriteXml(XmlWriter writer)
        {
            SerializeUtil.WriteColorElement(writer, "StrokeColor", StrokeColor);
            SerializeUtil.WriteDoubleElement(writer, "StrokeThickness", StrokeThickness);
            if (DashStyle != DashStyle.Solid)
                SerializeUtil.WriteIntElement(writer, "Style", (int)DashStyle);
            if (LineJoin != LineJoin.Round)
                SerializeUtil.WriteIntElement(writer, "LineJoin", (int)LineJoin);
        }
    }



    /// <summary>
    /// 轮廓发光效果
    /// </summary>
    public class GlowEffect
    {
        /// <summary>
        /// 获取或设置标注是否启用轮廓发光效果。
        /// </summary>
        public bool Enable
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置标注轮廓发光效果的大小（像素单位）。
        /// </summary>
        public double GlowEffectSize
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置标注轮廓发光效果的颜色。
        /// </summary>
        public Color? GlowEffectColor
        {
            get;
            set;
        }
        public GlowEffect(Color? color = null, double glowEffectSize = 3)
        {
            Enable = true;
            GlowEffectColor = color;
            GlowEffectSize = glowEffectSize;
        }

        public void ReadXml(XmlElement reader)
        {
            foreach (XmlNode n in reader.ChildNodes)
            {
                var s = n.InnerText;
                switch (n.Name)
                {
                    case "Enable":
                        Enable = "True".Equals(s);//SerializeUtil.ReadStringElement(reader, "GlowEffect"));
                        break;
                    case "GlowEffectSize":
                        GlowEffectSize = SafeConvertAux.ToDouble(s);// SerializeUtil.ReadDouble(reader, "GlowEffectSize");
                        break;
                    case "GlowEffectColor":
                        GlowEffectColor = ColorUtil.ConvertFromString(s);// SerializeUtil.ReadColor(reader, "GlowEffectColor");
                        break;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("GlowEffect");
            SerializeUtil.WriteStringElement(writer, "GlowEffect", Enable.ToString());
            SerializeUtil.WriteDoubleElement(writer, "GlowEffectSize", GlowEffectSize);
            SerializeUtil.WriteColorElement(writer, "GlowEffectColor", GlowEffectColor);
            writer.WriteEndElement();
        }
    }

    public interface ISymbol:IXmlSerializable,ICloneable
    {
        void SetupDC(IDisplay dc, IDisplayTransformation trans);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="extension"></param>
        /// <param name="fDeviceCoords">geometry是否已经被转换成设备坐标了</param>
        void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false);
        void ResetDC();

        IPolygon? QueryBoundary(/*Graphics hDC, IDisplayTransformation displayTransform*/IDisplay dc, IDisplayTransformation displayTransform, IGeometry Geometry);
        //void ReadXml(XmlElement reader);
    }
    public interface ILineSymbol : ISymbol
    {
        Color? StrokeColor { get; set; }
        double StrokeThickness { get; set; }
    }
    public interface IFillSymbol : ISymbol
    {
        Color? FillColor { get; set; }
        ILineSymbol? Outline { get; set; }
    }

    public interface ILineFillSymbol : IFillSymbol
    {
        double Angle { get; set; }
        ILineSymbol? LineSymbol { get; set; }
        double Offset { get; set; }
        double Separation { get; set; }
    }

    //public interface IPictureFillSymbol : IFillSymbol
    //{
    //    Image Image { get; set; }
    //}

    public enum eSimpleMarkerStyle
    {
        Circle = 0,
        Square = 1,
        Cross = 2,
        X = 3,
        Diamond = 4,
        Triangle = 5,
    }
    public interface IMarkerSymbol : ISymbol
    {
        double Angle { get; set; }
        Color? FillColor { get; set; }
        double Size { get; set; }
        double XOffset { get; set; }
        double YOffset { get; set; }
        PenProperty Strock { get; set; }
        eSimpleMarkerStyle Style { get; set; }
    }

    public interface IMultiLayerSymbol
    {
        int LayerCount { get; }
        ISymbol? GetLayer(int index);
    }
    /// <summary>
    /// 枚举字体的样式。
    /// </summary>
    public enum eFontStyle
    {
        /// <summary>
        /// 正常。
        /// </summary>
        Regular = 0,

        /// <summary>
        /// 粗体。
        /// </summary>
        Bold = 1,

        /// <summary>
        /// 斜体。
        /// </summary>
        Italic = 2,

        /// <summary>
        /// 下划线。
        /// </summary>
        Underline = 4,

        /// <summary>
        /// 删除线。
        /// </summary>
        Strikeout = 8,
    }

    public class FontDisp
    {
        public float FontSize { get; set; }
        public string? FontFamily { get; set; }
        public eFontStyle FontStyle;

        public bool IsValid
        {
            get
            {
                return FontSize > 0;
            }
        }
        //public Font CreateFont()
        //{
        //    var font = new Font(FontFamily, (float)(FontSize / 0.75), (FontStyle)FontStyle);
        //    return font;
        //}

        public void ReadXml(XmlElement reader)
        {
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    var name = e.Name;
                    var s = e.InnerText;
                    switch (name)
                    {
                        case "FontFamily":
                            FontFamily = s;
                            break;
                        case "FontSize":
                            FontSize = SafeConvertAux.ToFloat(s);
                            break;
                        case "FontStyle":
                            EnumUtil.StrToEnum(s, ref FontStyle);
                            break;
                        default:
                            System.Diagnostics.Debug.Assert(false, $"not process Property ${name}");
                            break;
                    }
                }
            }
        }

        // public void ReadXml(System.Xml.XmlReader reader,string endElementName="Font")
        //{
        //    SerializeUtil.ReadNodeCallback(reader, endElementName, name =>
        //    {
        //        switch (name)
        //        {
        //            case "FontFamily":
        //                FontFamily = reader.ReadString();
        //                break;
        //            case "FontSize":
        //                FontSize =SafeConvertAux.ToFloat(reader.ReadString());
        //                break;
        //            case "FontStyle":
        //                EnumUtil.StrToEnum(reader.ReadString(), ref FontStyle);
        //                break;
        //        }
        //    });
        //}
    }

    public interface ICharacterMarkerSymbol : IMarkerSymbol
    {
        int CharacterIndex { get; set; }

        FontDisp Font { get; }// set; }
    }
    public enum eLineCapStyle
    {
        eLCSButt = 0,
        eLCSRound = 1,
        eLCSSquare = 2,
    }
    public enum eLineJoinStyle
    {
        eLJSMitre = 0,
        eLJSRound = 1,
        eLJSBevel = 2,
    }
    public interface ICartographicLineSymbol : ILineSymbol
    {
        //eLineCapStyle Cap { get; set; }
        //eLineJoinStyle Join { get; set; }
        //double MiterLimit { get; set; }
        double Offset { get; set; }
    }

    public interface ITextSymbol : ISymbol
    {
        FontDisp Font { get; set; }
        Color Color { get; set; }
        /// <summary>
        /// 获取或设置标注位置偏移 X 值（像素单位）。
        /// </summary>
        double OffsetX { get; set; }
        /// <summary>
        /// 获取或设置标注位置偏移 Y 值（像素单位）。
        /// </summary>
        double OffsetY { get; set; }
        eTextHorizontalAlignment HorizontalAlignment { get; set; }
        eTextVerticalAlignment VerticalAlignment { get; set; }
        /// <summary>
        /// 获取或设置标注是否启用轮廓发光效果。
        /// </summary>
        GlowEffect GlowEffect { get; set; }
        string Text { get; set; }
        //void GetTextSize(System.Drawing.Graphics g, Transform trans, string Text, out double xSize, out double ySize);
        //ITextBackground Background { get; set; }
        bool AllowGlowEffect { get; }

        /// <summary>
        /// 是否竖排文字（从上到下显示）
        /// </summary>
        bool IsVerticalTextDirection { get; set; }

        RectangleF MeasureText(string text);
    }
    public interface IFormattedTextSymbol
    {
        ITextBackground Background { get; set; }
    }
    #region TextBackground 
    public interface ITextBackground
    {
        OkEnvelope TextBox { set; }
        IPolygon TextBoundary { set; }

        ITextSymbol TextSymbol { get; set; }

        void Draw(IDisplay g);//, Transform transform);
        IPolygon QueryBoundary(IDisplay dc);// Graphics g, /*Transform*/IDisplayTransformation transform);
    }
    public interface ICallout
    {
        IPoint AnchorPoint { get; set; }
        double LeaderTolerance { get; set; }
    }
    public enum eBalloonCalloutStyle
    {
        eBCSRectangle = 0,
        eBCSRoundedRectangle = 1,
        eBCSOval = 2,
    }
    public enum eLineCalloutStyle
    {
        eLCSBase = 0,
        eLCSMidpoint = 1,
        eLCSThreePoint = 2,
        eLCSFourPoint = 3,
        eLCSUnderline = 4,
        eLCSCustom = 5,
        eLCSCircularCW = 6,
        eLCSCircularCCW = 7,
    }
    public interface IBalloonCallout : ICallout
    {
        eBalloonCalloutStyle Style { get; set; }
        IFillSymbol Symbol { get; set; }
    }
    public interface ILineCallout : ICallout
    {
        ILineSymbol AccentBar { get; set; }
        IFillSymbol Border { get; set; }
        double Gap { get; set; }
        ILineSymbol LeaderLine { get; set; }
        eLineCalloutStyle Style { get; set; }
    }
    public class TextBackgroundBase
    {

        public OkEnvelope TextBox
        {
            set { throw new NotImplementedException(); }
        }

        public IPolygon? TextBoundary { set; protected get; }

        public ITextSymbol TextSymbol
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Draw(IDisplay dc)// g, Transform transform)
        {
            throw new NotImplementedException();
        }

        public IPolygon QueryBoundary(IDisplay dc)//Graphics g, IDisplayTransformation transform)
        {
            throw new NotImplementedException();
        }
    }
    public class CalloutTextBackgroundBase : TextBackgroundBase
    {
        public IPoint? AnchorPoint { get; set; }
        public double LeaderTolerance { get; set; }
    }
    /// <summary>
    /// A filled background that is placed behind text
    /// </summary>
    public class BalloonCallout : CalloutTextBackgroundBase, ITextBackground, IBalloonCallout
    {

        public eBalloonCalloutStyle Style
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IFillSymbol Symbol
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    /// <summary>
    /// A series of line symbols that link text to a specified location
    /// </summary>
    public class LineCallout : CalloutTextBackgroundBase, ITextBackground, ILineCallout
    {

        public ILineSymbol AccentBar
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IFillSymbol Border
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Gap
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ILineSymbol LeaderLine
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public eLineCalloutStyle Style
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    /// <summary>
    /// A marker that is placed behind text.
    /// </summary>
    public class MarkerTextBackground : TextBackgroundBase, ITextBackground
    {

    }
    /// <summary>
    /// A simple line that links text to a specified location.
    /// </summary>
    public class SimpleLineCallout : CalloutTextBackgroundBase, ITextBackground
    {

    }
    #endregion


    public interface IFormulaSymbol : ISymbol
    {
        string Expr { get; set; }
        //ASTNode? ASTNode { get; set; }
        ITextSymbol TextSymbol { get; set; }
        double PenSize { get; set; }
    }
}
