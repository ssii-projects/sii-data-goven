using Agro.LibCore;
using Agro.LibCore.AST;
using Agro.LibCore.Xml;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Agro.GIS
{
    public abstract class Symbol<T> : IXmlSerializable, ICloneable where T:IXmlSerializable,new()
    {
        //private static readonly SkiaDisplay dummyDC = new();
        private static readonly DisplayTransformation dummyTrans = new();
        //protected System.Drawing.Graphics _dc;
        protected IDisplay? _dc;// = dummyDC;
        protected IDisplayTransformation _trans = dummyTrans;// { get { return _dc.DisplayTransformation; } }
        //protected readonly SKPaint _paint = new() { IsAntialias = true };
        #region ISymbol
        protected Symbol()
        {

        }
        public virtual void SetupDC(IDisplay dc, IDisplayTransformation trans)
        {
            _dc = dc;
            _trans = trans;
        }
        public void ResetDC()
        {
            //_dc = null;
            //_trans = null;
        }
        public virtual IPolygon? QueryBoundary(IDisplay dc, IDisplayTransformation displayTransform, IGeometry Geometry)
        {
            return null;
        }
        #endregion


        public abstract void ReadXml(XmlElement reader);

        public virtual void WriteXml(XmlWriter writer)
        {
            //System.Diagnostics.Debug.Assert(false);
            throw new NotImplementedException();
        }

        protected virtual void ReadXmlElement(XmlElement e)
        {
            System.Diagnostics.Debug.Assert(false, $"not process Property ${e.Name}");
        }

        public object Clone()
        {
            return CloneUtil.Clone<T>(this);
        }
    }

    public abstract class LineSymbol<T> : Symbol<T>,ILineSymbol where T:IXmlSerializable,new()
    {
        //protected readonly PenProperty _stroke = new(Color.FromArgb(255, 128, 128, 128), 1);

        #region ILineSymbol
        public Color? StrokeColor { get; set; } = Color.FromArgb(255, 128, 128, 128);
        //{
        //    get
        //    {
        //        return _stroke.StrokeColor;
        //    }
        //    set
        //    {
        //        _stroke.StrokeColor = value;
        //    }
        //}
        public double StrokeThickness { get; set; } = 1;

        public abstract void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false);

        //{
        //    get
        //    {
        //        return _stroke.StrokeThickness;
        //    }
        //    set
        //    {
        //        _stroke.StrokeThickness = value;
        //    }
        //}
        #endregion

        public override void ReadXml(XmlElement reader)
        {
            //_stroke.ReadXml(reader);
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
                        //case "Style":
                        //    //var str = SerializeUtil.ReadStringElement(reader, "Style");
                        //    if (s != null)
                        //    {
                        //        EnumUtil.StrToEnum(s, ref DashStyle);
                        //    }
                        //    break;
                        //case "LineJoin":
                        //    LineJoin = (LineJoin)SafeConvertAux.ToInt32(s);// reader.ReadElementContentAsInt();
                        //    break;
                        default:
                            ReadXmlElement(e);
                            break;
                    }
                }
            }
        }
        public override void WriteXml(XmlWriter writer)
        {
            SerializeUtil.WriteColorElement(writer, "StrokeColor", StrokeColor);
            SerializeUtil.WriteDoubleElement(writer, "StrokeThickness", StrokeThickness);
            //if (DashStyle != DashStyle.Solid)
            //    SerializeUtil.WriteIntElement(writer, "Style", (int)DashStyle);
            //if (LineJoin != LineJoin.Round)
            //    SerializeUtil.WriteIntElement(writer, "LineJoin", (int)LineJoin);
        }
    }

    public abstract class FillSymbol<T> : Symbol<T> where T:IXmlSerializable,new ()
    {
        protected readonly ISymbolFactory symbolFactory;
        protected FillSymbol(ISymbolFactory f)
        {
            symbolFactory = f;
            //FillColor = Color.LightGray;
        }

        #region IFillSymbol
        /// <summary>
        /// 获取或设置面的背景填充色。
        /// </summary>
        public Color? FillColor
        {
            get;
            set;
        } = Color.LightGray;
        public ILineSymbol? Outline { get; set; }
        #endregion

        public override void ReadXml(XmlElement reader)
        {
            FillColor = null;
            Outline = null;
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    var name = e.Name;
                    switch (name)
                    {
                        case "BackgroundColor":
                            FillColor = ColorUtil.ConvertFromString(e.InnerText);// SerializeUtil.ReadColor(reader, "BackgroundColor");
                            break;
                        case "Border":
                            Outline = SerializeUtil.ReadSymbol(e, symbolFactory) as ILineSymbol;
                            break;
                        default:
                            ReadXmlElement(e);
                            break;
                    }
                }
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            SerializeUtil.WriteColorElement(writer, "BackgroundColor", FillColor);
            if (Outline != null)
            {
                writer.WriteStartElement("Border");
                writer.WriteStartAttribute("type");
                writer.WriteString(Outline.GetType().Name);
                writer.WriteEndAttribute();
                Outline.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
    }

    public abstract class MarkerSymbol<T> : Symbol<T> where T : IXmlSerializable, new()
    {
        private double _size;
        public double Angle { get; set; }
        /// <summary>
        /// 获取或设置点的背景填充色。
        /// </summary>
        public Color? FillColor
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置点的边框
        /// </summary>
        public PenProperty Strock
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置点的大小。
        /// </summary>
        public double Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                OnSetSize(value);
            }
        }

        public double XOffset { get; set; }
        public double YOffset { get; set; }

        public const double DefaultSize = 6.0;

        /// <summary>
        /// 获取或设置点的风格。
        /// </summary>
        public eSimpleMarkerStyle Style { get; set; }
        protected MarkerSymbol()
        {
            FillColor = Color.FromArgb(255, 240, 240, 240);
            Strock = new PenProperty(Color.FromArgb(255, 128, 128, 128));
            Size = DefaultSize;
            Style = eSimpleMarkerStyle.Circle;

        }

        protected virtual void OnSetSize(double size)
        {

        }
        //protected void ReleaseDC()
        //{
        //    //DisposeUtil.SafeDispose(ref _fillBrush);
        //    //DisposeUtil.SafeDispose(ref _pen);
        //}
        public override void ReadXml(XmlElement reader)
        {
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    var s = e.InnerText;
                    var name = e.Name;
                    switch (name)
                    {
                        case "BackgroundColor":
                            FillColor = ColorUtil.ConvertFromString(s);// SerializeUtil.ReadColor(reader, "BackgroundColor");
                            break;
                        case "Strock":
                            Strock.ReadXml(e);
                            break;
                        case "Size":
                            Size = SafeConvertAux.ToDouble(s);// reader.ReadElementContentAsDouble();
                            break;
                        case "Style":
                            Style = (eSimpleMarkerStyle)SafeConvertAux.ToInt32(s);// reader.ReadElementContentAsInt();
                            break;
                        case "Angle":
                            Angle = SafeConvertAux.ToDouble(s);// reader.ReadElementContentAsDouble();
                            break;
                        case "XOffset":
                            XOffset = SafeConvertAux.ToDouble(s);// reader.ReadElementContentAsDouble();
                            break;
                        case "YOffset":
                            YOffset = SafeConvertAux.ToDouble(s);// reader.ReadElementContentAsDouble();
                            break;
                        case "StrokeColor":
                            Strock.StrokeColor = ColorUtil.ConvertFromString(s);
                            break;
                        case "StrokeThickness":
                            Strock.StrokeThickness = SafeConvertAux.ToDouble(s);
                            break;
                        //case "Style":
                        //	Strock.DashStyle = (DashStyle)SafeConvertAux.ToInt32(s);
                        //	break;
                        case "LineJoin":
                            Strock.LineJoin = (LineJoin)SafeConvertAux.ToInt32(s);
                            break;
                        default:
                            ReadXmlElement(e);
                            break;
                    }
                }
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            WriteInnerXml(writer);
        }

        protected void WriteInnerXml(XmlWriter writer, bool fWriteSize = true, bool fWriteStyle = true)
        {
            SerializeUtil.WriteColorElement(writer, "BackgroundColor", FillColor);
            Strock?.WriteXml(writer);
            if (fWriteSize)
            {
                SerializeUtil.WriteDoubleElement(writer, "Size", Size);
            }
            if (fWriteStyle)
            {
                SerializeUtil.WriteIntElement(writer, "Style", (int)Style);
            }
            if (Angle != 0)
            {
                SerializeUtil.WriteDoubleElement(writer, "Angle", Angle);
            }
            if (XOffset != 0)
            {
                SerializeUtil.WriteDoubleElement(writer, "XOffset", XOffset);
            }
            if (YOffset != 0)
            {
                SerializeUtil.WriteDoubleElement(writer, "YOffset", YOffset);
            }
        }

    }


    public abstract class CharacterMarkerSymbolBase<T> : MarkerSymbol<T>, ICharacterMarkerSymbol where T:IXmlSerializable,new()
    {
        private readonly FontDisp _fontDesp = new();

        #region ICharacterMarkerSymbol
        /// <summary>
        /// 字符集索引
        /// </summary>
        public int CharacterIndex
        {
            get;
            set;
        }
        public FontDisp Font
        {
            get
            {
                return _fontDesp;
            }
            //set
            //{
            //	_fontDesp = value;
            //	this.Size = value?.FontSize??0;
            //}
        }
        #endregion

        public GlowEffect? GlowEffect { get; set; }

        public CharacterMarkerSymbolBase()
        {
            //this.Font = new FontDisp();
            base.FillColor = Color.Black;
            //Font.BackgroundColor = System.Windows.Media.Colors.Black;
            Font.FontSize = 18;
            Font.FontFamily = "ESRI Default Marker";

            this.Size = Font.FontSize;
            GlowEffect = new GlowEffect(Color.White, 3);

            //_format.Alignment = StringAlignment.Center;
            //_format.LineAlignment = StringAlignment.Center;
        }

        public abstract void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false);


        public override void WriteXml(XmlWriter writer)
        {
            SerializeUtil.WriteIntElement(writer, "CharacterIndex", CharacterIndex);

            SerializeUtil.WriteFont(writer, "Font", Font);
            base.WriteInnerXml(writer, false, false);
            if (GlowEffect != null)
            {
                GlowEffect.WriteXml(writer);
            }
        }

        protected override void ReadXmlElement(XmlElement e)
        {
            switch (e.Name)
            {
                case "CharacterIndex":
                    CharacterIndex = SafeConvertAux.ToInt32(e.InnerText); break;
                case "Font":
                    Font.ReadXml(e);
                    break;
                case "GlowEffect":
                    GlowEffect ??= new();
                    GlowEffect.ReadXml(e);
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, $"not process Property ${e.Name}");
                    break;
            }
        }

        protected PointF AdjustTextOrigin(PointF origin)
        {
            origin.X = origin.X + (float)base.XOffset;
            origin.Y = origin.Y + (float)base.YOffset;
            return origin;
        }

        protected override void OnSetSize(double size)
        {
            if (this.Font != null)
            {
                this.Font.FontSize = (float)size;
            }
        }
    }


    /// <summary>
    /// 提供多层渲染的渲染器。
    /// </summary>
    public class MultiLayerSymbol<T,TSymbol> : IMultiLayerSymbol, IXmlSerializable where TSymbol : class, ISymbol where T:IXmlSerializable,new()
    {
        //
        //private IDisplay? _dc;
        private IDisplayTransformation? _trans;// { get { return _dc?.DisplayTransformation; } }
        private readonly List<Tuple<TSymbol, bool>> _layers = new();
        private readonly ISymbolFactory symbolFactory;
        protected MultiLayerSymbol(ISymbolFactory f)
        {
            symbolFactory = f;
        }
        public int LayerCount { get { return _layers.Count; } }
        public void AddLayer(TSymbol fillLayer, bool fVisible = true)
        {
            _layers.Add(new Tuple<TSymbol, bool>(fillLayer, fVisible));
        }
        public void ClearLayers()
        {
            _layers.Clear();
        }
        public void DeleteLayer(ISymbol fillLayer)
        {
            var i = _layers.FindIndex(a =>
            {
                var t = a.Item1;
                return t == fillLayer;
            });
            if (i >= 0)
            {
                _layers.RemoveAt(i);
            }
        }
        public ISymbol? GetLayer(int index)
        {
            if (index >= 0 && index < _layers.Count)
                return _layers[index].Item1;
            return null;
        }
        public void MoveLayer(TSymbol fillLayer, int toIndex)
        {
        }

        #region ILayerVisible
        public bool GetLayerVisible(int index)
        {
            if (index >= 0 && index < _layers.Count)
                return _layers[index].Item2;
            return false;
        }
        public void SetLayerVisible(int index, bool fVis)
        {
            if (index >= 0 && index < _layers.Count)
            {
                _layers[index] = new Tuple<TSymbol, bool>(_layers[index].Item1, fVis);
            }
        }
        #endregion


        public void SetupDC(IDisplay dc, IDisplayTransformation trans)
        {
            //_dc = dc;
            _trans = trans;
            for (int i = _layers.Count - 1; i >= 0; --i)
            {
                if (!_layers[i].Item2)
                    continue;
                var symbol = _layers[i].Item1;
                symbol.SetupDC(dc, trans);// trans);
            }
        }
        public void ResetDC()
        {
            for (int i = _layers.Count - 1; i >= 0; --i)
            {
                if (!_layers[i].Item2)
                    continue;
                var symbol = _layers[i].Item1;
                symbol.ResetDC();
            }
        }
        public void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
        {
            //System.Diagnostics.Debug.Assert(false);
            if (geometry != null)
            {
                if (_trans != null)
                {
                    if (!fDeviceCoords)
                    {
                        geometry = _trans.ToDevice(geometry)!;
                    }
                }
                for (int i = _layers.Count - 1; i >= 0; --i)
                {
                    if (!_layers[i].Item2)
                        continue;

                    var symbol = _layers[i].Item1;
                    symbol.Draw(geometry, extension, true);
                }
            }
        }

        public IPolygon? QueryBoundary(IDisplay dc,IDisplayTransformation trans, IGeometry geo)
        {
            throw new NotImplementedException();
        }

        public virtual void ReadXml(XmlElement reader)
        {
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    var name = e.Name;
                    if (name == "Layer")
                    {
                        var sVisible = e.GetAttribute("Visible");
                        foreach (var o1 in e.ChildNodes)
                        {
                            if (o1 is XmlElement n)
                            {
                                switch (n.Name)
                                {
                                    case "Symbol":
                                        var symbol = SerializeUtil.ReadSymbol(n, symbolFactory) as TSymbol;
                                        if (symbol != null)
                                        {
                                            AddLayer(symbol, sVisible == null || sVisible != "False");
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var s in _layers)
            {
                writer.WriteStartElement("Layer");
                if (!s.Item2)
                {
                    SerializeUtil.WriteAttribute(writer, "Visible", "False");
                }
                SerializeUtil.WriteSymbol(writer, "Symbol", s.Item1);
                writer.WriteEndElement();
            }
        }

        public object Clone()
        {
            return CloneUtil.Clone<T>(this);
        }
    }


    public class MultiLayerFillSymbolBase<T>: MultiLayerSymbol<T,IFillSymbol>, IFillSymbol where T:   IXmlSerializable,new()
    {
        public MultiLayerFillSymbolBase(ISymbolFactory f) : base(f) { }
        public Color? FillColor
        {
            get
            {
                return base.GetLayer(0) is IFillSymbol symbol ? symbol.FillColor : null;
            }
            set
            {
                if (base.GetLayer(0) is IFillSymbol symbol)
                {
                    symbol.FillColor = value;
                }
            }
        }

        public ILineSymbol? Outline
        {
            get
            {
                return base.GetLayer(0) is IFillSymbol symbol ? symbol.Outline : null;
            }
            set
            {
                if (base.GetLayer(0) is IFillSymbol symbol)
                {
                    symbol.Outline = value;
                }
            }
        }
    }

    public class MultiLayerMarkerSymbolBase<T> : MultiLayerSymbol<T,IMarkerSymbol>, IMarkerSymbol where T:   IXmlSerializable,new ()
    {
        public MultiLayerMarkerSymbolBase(ISymbolFactory f) : base(f) { }
        public double Angle
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.Angle : 0;
            }
            set
            {
                if (base.GetLayer(0) is IMarkerSymbol symbol)
                {
                    symbol.Angle = value;
                }
            }
        }

        public Color? FillColor
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.FillColor : null;
            }
            set
            {
                if (base.GetLayer(0) is IMarkerSymbol symbol)
                {
                    symbol.FillColor = value;
                }
            }
        }

        public double Size
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.Size : 0;
            }
            set
            {
                for (int i = 0; i < base.LayerCount; ++i)
                {
                    if (base.GetLayer(i) is IMarkerSymbol symbol)
                    {
                        symbol.Size = value;
                    }
                }
            }
        }

        public double XOffset
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.XOffset : 0;
            }
            set
            {
                if (base.GetLayer(0) is IMarkerSymbol symbol)
                {
                    symbol.XOffset = value;
                }
            }
        }

        public double YOffset
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.YOffset : 0;
            }
            set
            {
                if (base.GetLayer(0) is IMarkerSymbol symbol)
                {
                    symbol.YOffset = value;
                }
            }
        }

        public PenProperty Strock
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.Strock : new PenProperty(null);
            }
            set
            {
                if (base.GetLayer(0) is IMarkerSymbol symbol)
                {
                    symbol.Strock = value;
                }
            }
        }

        public eSimpleMarkerStyle Style
        {
            get
            {
                return base.GetLayer(0) is IMarkerSymbol symbol ? symbol.Style : eSimpleMarkerStyle.Circle;
            }
            set
            {
                if (base.GetLayer(0) is IMarkerSymbol symbol)
                {
                    symbol.Style = value;
                }
            }
        }
        //public object Clone()
        //{
        //    return CloneUtil.Clone<T>(this);
        //}
    }

    public class MultiLayerLineSymbolBase<T> : MultiLayerSymbol<T,ILineSymbol>, ILineSymbol where T:IXmlSerializable,new()
    {
        public MultiLayerLineSymbolBase(ISymbolFactory f) : base(f) { }
        public Color? StrokeColor
        {
            get
            {
                return base.GetLayer(0) is ILineSymbol symbol ? symbol.StrokeColor : null;
            }
            set
            {
                if (base.GetLayer(0) is ILineSymbol symbol)
                {
                    symbol.StrokeColor = value;
                }
            }
        }

        public double StrokeThickness
        {
            get
            {
                return base.GetLayer(0) is ILineSymbol symbol ? symbol.StrokeThickness : 0;
            }
            set
            {
                if (base.GetLayer(0) is ILineSymbol symbol)
                {
                    symbol.StrokeThickness = value;
                }
            }
        }

        //public object Clone()
        //{
        //    return CloneUtil.Clone<MultiLayerLineSymbol>(this);
        //}
    }

    /// <summary>
    /// 制图线符号
    /// </summary>
    public abstract class CartographicLineSymbolBase<T> : LineSymbol<T>, ICartographicLineSymbol where T: IXmlSerializable,new()
    {
        public Template Template { get; private set; } = new();

        #region ICartographicLineSymbol
        /// <summary>
        /// 画笔Y方向的偏移量
        /// </summary>
        public double Offset
        {
            get;
            set;
        }
        #endregion

        //public CartographicLineSymbol()
        //{
        //    Template = new Template();
        //    Offset = 0;
        //}

        #region ISymbol

        //public abstract void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false);
 

        #endregion

        protected override void ReadXmlElement(XmlElement e)
        {
            switch (e.Name)
            {
                case "Offset":
                    Offset = SafeConvertAux.ToDouble(e.InnerText);// reader.ReadElementContentAsDouble();// SerializeUtil.ReadDouble(reader, "Offset");
                    break;
                case "Template":
                    Template.ReadXml(e);
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, $"not process Property ${e.Name}");
                    break;
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            SerializeUtil.WriteDoubleElement(writer, "Offset", Offset);
            writer.WriteStartElement("Template");
            Template.WriteXml(writer);
            writer.WriteEndElement();
        }

    }

    /// <summary>
    /// 用于创建DashPattern的模板
    /// </summary>
    public class Template
    {
        struct PatternElement
        {
            public int mark_;
            public int gap_;
        }
        private List<PatternElement> m_arrPatternElement = new List<PatternElement>();
        //private double m_dInterval=1.0;
        public Template()
        {
            Interval = 1.0;
        }
        public int GetPatternElementCount()
        {
            return m_arrPatternElement.Count;
        }
        public bool GetPatternElement(int Index, out double mark, out double Gap)
        {
            mark = Gap = 0;
            if (Index >= 0 && Index < m_arrPatternElement.Count)
            {
                mark = m_arrPatternElement[Index].mark_;
                Gap = m_arrPatternElement[Index].gap_;
                return true;
            }
            return false;
        }
        public void AddPatternElement(double mark, double gap)
        {
            //add a PatternElement into m_arrPatternElement
            var pPatternElement = new PatternElement();
            pPatternElement.mark_ = (int)mark;
            pPatternElement.gap_ = (int)gap;
            m_arrPatternElement.Add(pPatternElement);
        }
        public void DeletePatternElement(int Index)
        {
            if (!(Index < 0 || Index >= m_arrPatternElement.Count))
            {
                m_arrPatternElement.RemoveAt(Index);
            }
        }
        public void MovePatternElement(int fromIndex, int toIndex)
        {
            var pPatternElement = m_arrPatternElement[toIndex];
            m_arrPatternElement[toIndex] = m_arrPatternElement[fromIndex];
            m_arrPatternElement[fromIndex] = pPatternElement;
        }
        public void ClearPatternElements()
        {
            m_arrPatternElement.Clear();
        }
        public double Interval { get; set; }
        public bool IsOK()
        {
            return m_arrPatternElement.Count > 0;
        }
        public float[] CreateDashPattern()
        {
            if (!IsOK())
                return null;
            var lst = new List<float>();
            for (int j = 0; j < m_arrPatternElement.Count; j++)
            {
                var p = m_arrPatternElement[j];
                double mark = p.mark_ * Interval;
                double gap = p.gap_ * Interval;
                if (mark > 0 && gap > 0)
                {
                    lst.Add((float)mark);
                    lst.Add((float)gap);
                }
            }
            if (lst.Count == 0)
                return null;
            return lst.ToArray();
        }
        //public System.Drawing.TextureBrush CreateBrush(double height, double offset, System.Drawing.Color markColor)
        //{
        //	int Width = 0;
        //	for (int j = 0; j < m_arrPatternElement.Count; j++)
        //	{
        //		var p = m_arrPatternElement[j];
        //		double mark = p.mark_ * Interval;
        //		double gap = p.gap_ * Interval;
        //		Width += (int)(mark + gap);
        //	}
        //	int Height = (int)(height + 0.9);
        //	//if (Height == 0 && height > 0)
        //	//{
        //	//    Height = 1;
        //	//}
        //	if (Width == 0 || Height <= 0)
        //		return null;
        //	//if (Height <= 0)
        //	//    Height = 1;
        //	var bm = new System.Drawing.Bitmap(Width, Height);
        //	using (var g = System.Drawing.Graphics.FromImage(bm))
        //	{
        //		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        //		using (var pen = new System.Drawing.Pen(markColor, (float)Height))
        //		{
        //			pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
        //			int x = 0;
        //			for (int j = 0; j < m_arrPatternElement.Count; j++)
        //			{
        //				var p = m_arrPatternElement[j];
        //				double mark = p.mark_ * Interval;
        //				double gap = p.gap_ * Interval;
        //				if (mark > 0)
        //				{
        //					int x1 = x + (int)mark;
        //					g.DrawLine(pen, new System.Drawing.Point(x, 0), new System.Drawing.Point(x1, 0));
        //				}
        //				x += (int)(mark + gap);
        //			}
        //		}
        //	}
        //	var brush = new System.Drawing.TextureBrush(bm);
        //	return brush;
        //}

        public void ReadXml(XmlElement reader)
        {
            double? mark = null, gap = null;
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    switch (e.Name)
                    {
                        case "Interval": Interval = SafeConvertAux.ToDouble(e.InnerText); break;
                        case "Mark": mark = SafeConvertAux.ToDouble(e.InnerText); break;
                        case "Gap": gap = SafeConvertAux.ToDouble(e.InnerText); break;
                    }
                    //Interval = SafeConvertAux.ToDouble(e.InnerText);// (double)SerializeUtil.ReadDouble(reader, "Interval");
                    //while (true)
                    //{
                    //	if (!reader.IsStartElement("Mark"))
                    //		break;
                    //	var mark = SerializeUtil.ReadInt(reader, "Mark");
                    //	if (!reader.IsStartElement("Gap"))
                    //		break;
                    //	var gap = SerializeUtil.ReadInt(reader, "Gap");
                    //	AddPatternElement((double)mark, (double)gap);
                    //}
                }
            }
            if (mark != null && gap != null)
            {
                AddPatternElement((double)mark, (double)gap);
            }
        }

        //      #region Methods - Serialize

        //      public void ReadXml(System.Xml.XmlReader reader)
        //{
        //	reader.Read();

        //	Interval = (double)SerializeUtil.ReadDouble(reader, "Interval");
        //	while (true)
        //	{
        //		if (!reader.IsStartElement("Mark"))
        //			break;
        //		var mark = SerializeUtil.ReadInt(reader, "Mark");
        //		if (!reader.IsStartElement("Gap"))
        //			break;
        //		var gap = SerializeUtil.ReadInt(reader, "Gap");
        //		AddPatternElement((double)mark, (double)gap);
        //	}

        //	reader.ReadEndElement();
        //}

        public void WriteXml(XmlWriter writer)
        {
            SerializeUtil.WriteDoubleElement(writer, "Interval", Interval);

            foreach (var s in m_arrPatternElement)
            {
                SerializeUtil.WriteIntElement(writer, "Mark", s.mark_);
                SerializeUtil.WriteIntElement(writer, "Gap", s.gap_);
            }
        }

        //#endregion
    }
}
