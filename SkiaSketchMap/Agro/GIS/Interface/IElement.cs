using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
//using System.Windows.Controls;
using System.Xml;


/*
yxm created at 2019/1/8 15:18:14
*/
namespace Agro.GIS
{

    public interface IElement
    {
        string? Name { get; set; }
        void Activate(IDisplay? pIDisplay,IDisplayTransformation trans);
        void Deactivate();
        //void SetupDC(System.Drawing.Graphics dc, Transform trans);
        void Draw(/*IDisplay display,IDisplayTransformation trans,*/ ICancelTracker cancel, OutputMode mode);
        void DrawCache();// IDisplay display,IDisplayTransformation trans);//, ICancelTracker cancel, OutputMode mode);
        IGeometry? Geometry { get; set; }
        bool Locked { get; set; }
        OkEnvelope? QueryBounds();// IDisplay pIDisplay,IDisplayTransformation trans);
        IPolygon? QueryOutline();// IDisplay pIDisplay,IDisplayTransformation trans);
        //void ResetDC();
        //Bitmap CacheBitmap { get; }
        //ISelectionTracker GetSelectionTracker();
        bool HitTest(double X, double Y, double Tolerance);
        void Transform(AffineTransformation trans);

        //IElementPropertyPage PropertyPage { get; set; }

        void ReadXml(XmlElement reader, SerializeSession session);
        //void ReadXml(XmlReader reader, SerializeSession session);//, string oldMapDocumentPath, string newMapDocumentPath);
        //void WriteXml(XmlWriter writer, bool fWriteDataSource = true);
    }
    /// <summary>
    /// 2019-1-2
    /// </summary>
    public interface ITextElement : IElement
    {
        ITextSymbol Symbol { get; set; }
        string Text { get; set; }
        bool ScaleText { get; set; }


    }
    public interface IMarkerElement : IElement
    {
        IMarkerSymbol Symbol { get; set; }
    }
    public interface IGroupElement
    {
        IEnumerable<IElement> Elements { get; }
    }
    public interface IElementEditCallout
    {
        bool EditingCallout { set; }
    }
    public interface IBoundsProperties
    {
        bool FixedAspectRatio { get; set; }
        bool FixedSize { get; }
    }

    public interface IGridElement
    {
        ILineSymbol LineSymbol { get; set; }
        IFillSymbol FillSymbol { get; set; }
        //object GetData(int idx);
        //void SetData(int idx, object data);
        void SetCellText(int row, int col, string text);
        int Rows { get; set; }
        int Cols { get; set; }
        //void SetTransparent();
        List<IElement> ConvertToSimpleElement(IGraphicsContainer pIGraphicsContainer, IDisplay pIDisplay);
        void SetCellMargin(int nMargin);
        int GetCellStyleID(int row, int col);
        void SetCellStyleID(int row, int col, int nStyleID);
    };
    public interface IFormulaElement
    {
        IFormulaSymbol Symbol { get; }
        //string Expr { get; set; }
        //ITextSymbol TextSymbol { get; set; }
    }
    //public interface IElementPropertyPage : IPropertyPage
    //{
    //    void PreShow(KuiDialog dlg);
    //    void Init(IActiveView av);
    //}
    //public class ElementPropertyPage : UserControl, IElementPropertyPage
    //{
    //    protected IActiveView _av;
    //    //protected IElement _element;
    //    protected readonly List<IElement> Elements = new List<IElement>();
    //    public string Title { get; set; }

    //    protected Action<KuiDialog> OnPreShow;
    //    public ElementPropertyPage()
    //    {
    //    }
    //    public virtual string Apply()
    //    {
    //        return null;
    //    }

    //    public void Init(IActiveView av)//, IElement e)
    //    {
    //        _av = av;
    //        //_element = e;
    //        Elements.Clear();
    //        Elements.AddRange(av.GraphicsContainer.SelectedElements);
    //    }

    //    public void PreShow(KuiDialog dlg)
    //    {
    //        OnPreShow?.Invoke(dlg);
    //    }
    //}
    public abstract class ElementBase : IElement
    {
        protected IDisplay? m_pIDisplay;
        protected IDisplayTransformation _trans=DisplayTransformation.Dummy;
        protected IGeometry? _geometry;
        //protected readonly ISelectionTracker m_pISelectionTracker;
        //private IElementPropertyPage _propertyPage;
        public ElementBase()//ISelectionTracker st = null)
        {
            //m_pISelectionTracker = st;
        }

        public string? Name { get; set; }
        public virtual void Activate(IDisplay? pIDisplay,IDisplayTransformation trans)
        {
            m_pIDisplay = pIDisplay;
            _trans= trans;
            //if (m_pISelectionTracker != null)
            //{
            //    m_pISelectionTracker.Display = pIDisplay;
            //}
            RefreshTracker();
        }
        public virtual void Deactivate()
        {
            m_pIDisplay = null;
        }
        protected virtual void RefreshTracker() { }
        public bool Locked { get; set; }
        public virtual IGeometry? Geometry
        {
            get
            {
                return _geometry;
            }
            set
            {
                _geometry = value;
                RefreshTracker();
            }
        }
        public abstract void Draw(/*IDisplay dc,IDisplayTransformation trans, */ICancelTracker cancel, OutputMode mode);
        public virtual void DrawCache()//IDisplay display,IDisplayTransformation trans)
        {
            Draw(/*display,trans,*/ NotCancelTracker.Instance, OutputMode.ToScreen);
        }
        public virtual OkEnvelope? QueryBounds()//IDisplay pIDisplay,IDisplayTransformation trans)
        {
            var pIPolygon = QueryOutline();// pIDisplay,trans);
            if (pIPolygon == null) return null;
            var env = new OkEnvelope(pIPolygon.EnvelopeInternal)
            {
                SpatialReference = pIPolygon.GetSpatialReference()
            };
            return env;
        }
        public abstract IPolygon? QueryOutline();// IDisplay? pIDisplay,IDisplayTransformation trans);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="X">地图坐标</param>
        /// <param name="Y">地图坐标</param>
        /// <param name="Tolerance">地图坐标单位</param>
        /// <returns></returns>
        public virtual bool HitTest(double X, double Y, double Tolerance)
        {
            var pIOkPolygon = QueryOutline();// m_pIDisplay,_trans);
            if(pIOkPolygon == null) return false;
            double dXMin, dYMin, dXMax, dYMax;
            dXMin = X - 2 * Tolerance;
            dXMax = X + 2 * Tolerance;
            dYMin = Y - 2 * Tolerance;
            dYMax = Y + 2 * Tolerance;
            var env = GeometryUtil.MakePolygon(dXMin, dYMin, dXMax, dYMax);
            if (!pIOkPolygon.Disjoint(env))
            {
                return true;
            }
            return false;
        }

        public virtual void Transform(AffineTransformation trans)
        {
            if (trans != null && _geometry != null)
            {
                _geometry = trans.Transform(_geometry);
            }
        }

        //public IElementPropertyPage PropertyPage
        //{
        //    get
        //    {
        //        if (_propertyPage == null)
        //        {
        //            _propertyPage = GetPropertyPage();
        //        }
        //        return _propertyPage;
        //    }
        //    set
        //    {
        //        _propertyPage = value;
        //    }
        //}

        //protected virtual IElementPropertyPage GetPropertyPage()
        //{
        //    return null;
        //}
        //protected virtual void RefreshTracker()
        //{
        //    if (m_pIDisplay != null && m_pISelectionTracker != null)
        //    {
        //        var pIPolygon = this.QueryOutline(m_pIDisplay);
        //        m_pISelectionTracker.Geometry = pIPolygon;
        //    }
        //}

        #region IXmlSerializable
        //public System.Xml.Schema.XmlSchema? GetSchema()
        //{
        //    return null;
        //}

        public void ReadXml(XmlElement reader, SerializeSession session)//, string oldMapDocumentPath, string newMapDocumentPath)
        {
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    var s = e.InnerText;
                    switch (e.Name)
                    {
                        case "Name":
                            Name =s;
                            break;
                        case "Locked":
                            Locked =s== "True";
                            break;
                        case "Extent":
                            {
                                IGeometry? g = null;
                                var env = SerializeUtil.ReadExtent(e);
                                if (env.Width == 0)
                                {
                                    g = GeometryUtil.MakePoint(env.Centre);
                                }
                                else
                                {
                                    g = GeometryUtil.MakePolygon(env);
                                }
                                this.Geometry = g;
                            }
                            break;
                        default:
                            ReadXmlElement(e,session);
                            break;
                    }
                }
            }
        }
        protected virtual void ReadXmlElement(XmlElement x, SerializeSession session)
        {
            throw new NotImplementedException();
        }
        //public virtual void ReadXml(System.Xml.XmlReader reader, SerializeSession session)//, string oldMapDocumentPath, string newMapDocumentPath)
        //{
        //    System.Diagnostics.Debug.Assert(false);
        //    //throw new NotImplementedException();
        //}


        //public virtual void WriteXml(System.Xml.XmlWriter writer, bool fWriteDataSource = true)
        //{

        //    System.Diagnostics.Debug.Assert(false);
        //    //throw new NotImplementedException();
        //}
        #endregion

        //protected void ReadXmlElement(XmlElement reader)//, string elementName)
        //{
        //    switch (reader.Name)
        //    {
        //        case "Name":
        //            Name = reader.InnerText;
        //            break;
        //        case "Locked":
        //            Locked = reader.InnerText == "True";
        //            break;
        //        case "Extent":
        //            {
        //                IGeometry? g = null;
        //                var env = SerializeUtil.ReadExtent(reader);
        //                if (env.Width == 0)
        //                {
        //                    g = GeometryUtil.MakePoint(env.Centre);
        //                }
        //                else
        //                {
        //                    g = GeometryUtil.MakePolygon(env);
        //                }
        //                this.Geometry = g;
        //            }
        //            break;
        //        default:
        //            {
        //                System.Diagnostics.Debug.Assert(false, $"not process propery {reader.Name}");
        //            }break;
        //    }
        //}
        //protected void ReadXmlElement(System.Xml.XmlReader reader, string elementName)
        //{
        //    switch (elementName)
        //    {
        //        case "Name":
        //            Name = reader.ReadString();
        //            break;
        //        case "Locked":
        //            Locked = reader.ReadString() == "True";
        //            break;
        //        case "Extent":
        //            {
        //                IGeometry? g = null;
        //                var env = SerializeUtil.ReadExtent(reader);
        //                if (env.Width == 0)
        //                {
        //                    g = GeometryUtil.MakePoint(env.Centre);
        //                }
        //                else
        //                {
        //                    g = GeometryUtil.MakePolygon(env);
        //                }
        //                this.Geometry = g;
        //            }
        //            break;
        //    }
        //}
        //protected void WriteInnerXml(System.Xml.XmlWriter writer)
        //{
        //    if (!string.IsNullOrEmpty(Name))
        //    {
        //        SerializeUtil.WriteStringElement(writer, "Name", Name);
        //    }
        //    SerializeUtil.WriteBoolElement(writer, "Locked", Locked);
        //    if (Geometry != null)
        //    {
        //        SerializeUtil.WriteExtent(writer, Geometry.EnvelopeInternal);
        //    }
        //}
    }

    /// <summary>
    /// yxm 2019-1-18
    /// 用于保存读取文档过程中的临时变量
    /// </summary>
    public class SerializeSession
    {
        public string? OldDocumentPath;
        public string? DocumentPath;
        internal readonly Dictionary<IMapSurround, string> _mapID = new();

        public SerializeSession(string docPath, string? oldDocPath = null)
        {
            DocumentPath = docPath;
            OldDocumentPath = oldDocPath;
        }
        internal void Clear()
        {
            _mapID.Clear();
        }
    }
}
