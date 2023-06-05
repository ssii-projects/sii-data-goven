using Agro.LibCore;
using GeoAPI.Geometries;
using System;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace Agro.GIS
{
    public interface IFrameProperties
    {
        IBorder Border { get; set; }
        IBackground Background { get; set; }
        IShadow Shadow { get; set; }
    }
    public class FrameProperties:IFrameProperties
    {
        protected IShadow m_pIShadow = new SymbolShadow();
        protected IBorder? m_pIBorder = null;// new SymbolBorder();
        protected IBackground m_pIBackground = new SymbolBackground();

        public IBorder? Border { get => m_pIBorder; set { m_pIBorder = value; } }
        public IBackground Background { get { return m_pIBackground; } set { m_pIBackground = value; } }
        public IShadow Shadow { get { return m_pIShadow; } set { m_pIShadow = value; } }
    }
    public interface IPage
    {
        OkPageFormID FormID { get; set; }
        PaperOrientation Orientation { get; set; }
        OkUnits Units { get; set; }
		double Width { get; set; }
		double Height { get; set; }
        Color BackgroundColor { get; set; }
        bool DelayEvents { get; set; }
        OkPageToPrinterMapping PageToPrinterMapping { get; set; }
        Envelope? PrintableBounds { get; }
        bool IsPrintableAreaVisible { get; set; }
        bool StretchGraphicsWithPage { get; set; }

        void QuerySize(out double width, out double height);
        void PutCustomSize(double Width, double Height);
        short PrinterPageCount(IPrinter Printer,double Overlap);
        Envelope GetPageBounds(IPrinter Printer, short currentPage, double Overlap);
        Envelope GetDeviceBounds(IPrinter Printer, short currentPage, double Overlap, short Resolution);
        void DrawPaper(IDisplay Display,IDisplayTransformation trans, OutputMode mode, Color? Color=null);
        void DrawPrintableArea(IDisplay Display, IDisplayTransformation trans);
        void PrinterChanged(IPrinter Printer);

        void ReadXml(XmlElement reader);

  //      void ReadXml(System.Xml.XmlReader reader);
		//void WriteXml(System.Xml.XmlWriter writer);

	}
    public class Page : FrameProperties, IPage
    {
        private IPrinter m_pIPrinter;
        private OkPageFormID m_FormID= OkPageFormID.okPageFormLetter;
        private bool m_bIsPrintableAreaVisible = false;
        private bool m_bStretchGraphicsWithPage = false;
        private Color m_pIBackgroundColor = Color.FromArgb(255, 255, 255);
        private OkUnits m_Units= OkUnits.okInches;
        private OkPageToPrinterMapping m_PageToPrinterMapping=OkPageToPrinterMapping.okPageMappingTile;
        private bool m_bDelayEvents = false;
        private ISymbol m_pIPrintableAreaSymbol;
        private IFillSymbol _pFillSymbol;

        #region events
        public Action OnPageSizeChanged;
        public Action OnPageUnitsChanged;
        #endregion

        private readonly ISymbolFactory symbolFactory= GisGlobal.SymbolFactory;
        public Page()
        {
            //this.symbolFactory = symbolFactory;
            DelayEvents = false;
            m_pIShadow.HorizontalSpacing = 4;
            m_pIShadow.VerticalSpacing = 4;
            m_pIPrintableAreaSymbol = SymbolUtil.CreateDashLineSymbol(Color.FromArgb(100, 100, 100), 1);
            m_pIPrintableAreaSymbol = symbolFactory.CreateSimpleLineSymbol();
            _pFillSymbol = symbolFactory.CreateSimpleFillSymbol();
            _pFillSymbol.Outline.StrokeColor = Color.White;
        }

        public bool DelayEvents { get { return m_bDelayEvents; } set { m_bDelayEvents = value; } }
        public OkPageToPrinterMapping PageToPrinterMapping { get { return m_PageToPrinterMapping; } set { m_PageToPrinterMapping = value; } }
        public OkPageFormID FormID
        {
            get
            {
                return m_FormID;
            }
            set
            {
                m_FormID = value;
                if (m_FormID != OkPageFormID.okPageFormCUSTOM && m_FormID != OkPageFormID.okPageFormSameAsPrinter)
                {
                    Units = PaperSize.arrForms[(int)value].units;
                }
                else if (m_FormID == OkPageFormID.okPageFormSameAsPrinter)
                {
                    if (m_pIPrinter != null)
                    {
                        Units = m_pIPrinter.Units;
                        var pIPaper = m_pIPrinter.Paper;
                        pIPaper.QueryPaperSize(out double dWidth, out double dHeight);
						Width = dWidth;
						Height = dHeight;
                    }
                }
                if (!DelayEvents)
                {
                    Fire_PageSizeChanged();
                    Fire_PageUnitsChanged();
                }
            }
        }
        public PaperOrientation Orientation { get; set; }
        public OkUnits Units {
            get {
                return m_Units;
            }
            set {
                m_Units = value;
                if (m_bDelayEvents == false)
                {
                    Fire_PageUnitsChanged();
                }
            }
        }
		public double Width { get; set; } = 8.5;
		public double Height { get; set; } = 11;
		public Envelope? PrintableBounds {
            get
            {
                if (m_pIPrinter != null)
                {
                    return m_pIPrinter.PrintableBounds;
                }
                return null;
            }
        }
        public bool IsPrintableAreaVisible { get { return m_bIsPrintableAreaVisible; } set { m_bIsPrintableAreaVisible = value; } }
        public bool StretchGraphicsWithPage { get { return m_bStretchGraphicsWithPage; } set { m_bStretchGraphicsWithPage = value; } }
        public Color BackgroundColor { get { return m_pIBackgroundColor; } set { m_pIBackgroundColor = value; } }
        public void QuerySize(out double dWidth, out double dHeight)
        {
            //dWidth = 0;
            //dHeight = 0;

            if (m_FormID == OkPageFormID.okPageFormCUSTOM)
            {
                if (Orientation == PaperOrientation.Portrait)
                {
                    dWidth = Width;
                    dHeight = Height;
                }
                else
                {
                    dWidth = Height;
                    dHeight =Width;
                }
            }
            else if (m_FormID == OkPageFormID.okPageFormSameAsPrinter)
            {
                if (m_pIPrinter!=null)
                {
                    //ATLASSERT(m_pIPrinter);
                    var pIPaper = m_pIPrinter.Paper;
                    pIPaper.QueryPaperSize(out dWidth,out dHeight);
                }
                else
                {
                    dWidth = Width;
                    dHeight = Height;
                }
            }
            else
            {
                var arrForms = PaperSize.arrForms;
                int nFormID =(int)m_FormID;
                if (Orientation == PaperOrientation.Portrait)
                {
                    dWidth = arrForms[nFormID].fWidth;
                    dHeight = arrForms[nFormID].fHeight;
                }
                else
                {
                    dWidth = arrForms[nFormID].fHeight;
                    dHeight = arrForms[nFormID].fWidth;
                }
            }
        }
        public void PutCustomSize(double dWidth, double dHeight)
        {
            Width = dWidth;
            Height = dHeight;
            if (m_FormID == OkPageFormID.okPageFormCUSTOM && m_bDelayEvents == false)
            {
                Fire_PageSizeChanged();
            }
        }
        public short PrinterPageCount(IPrinter Printer, double Overlap)
        {
            short pageCount = 0;
            if (m_FormID ==OkPageFormID.okPageFormSameAsPrinter)
            {
                pageCount = 1;
            }
            else
            {
                QuerySize(out double width,out double height);

                var pIPaper=Printer.Paper;
                var pIEnv=pIPaper.PrintableBounds;
                double dPaperWidth = pIEnv.Width;
                double dPaperHeight=pIEnv.Height;
                OkUnits paperUnits=pIPaper.Units;
                width=UnitsUtil.ConvertUnits(m_Units, paperUnits, width);
                height=UnitsUtil.ConvertUnits(m_Units, paperUnits, height);
                pageCount = (short)(Math.Ceiling(width / dPaperWidth) * Math.Ceiling(height / dPaperHeight));
            }
            return pageCount;
        }
        public Envelope GetPageBounds(IPrinter Printer, short currentPage, double Overlap)
        {
            var pageBounds = new Envelope();
            if (m_FormID ==OkPageFormID.okPageFormSameAsPrinter)
            {
                var pIPaper=Printer.Paper;
                var pIEnv=pIPaper.PrintableBounds;
                pageBounds.Init(pIEnv);
            }
            else
            {
				QuerySize(out double width, out double height);

				var pIPaper=Printer.Paper;
                var pIEnv=pIPaper.PrintableBounds;
                double dPaperWidth = pIEnv.Width;
                double dPaperHeight = pIEnv.Height;
                OkUnits paperUnits=pIPaper.Units;
                if (m_Units != paperUnits)
                {
                    width=UnitsUtil.ConvertUnits(m_Units, paperUnits, width);
                    height=UnitsUtil.ConvertUnits(m_Units, paperUnits, height);
                }
                int iRowCount = (int)Math.Ceiling(height / dPaperHeight);
                int iColumnCount = (int)Math.Ceiling(width / dPaperWidth);
                int iRow = currentPage / iColumnCount;
                int iColumn = currentPage - iRow * iColumnCount;
                double xmin, ymin, xmax, ymax;
                xmin = dPaperWidth * iColumn;
                ymin = dPaperHeight * iRow;
                xmax = dPaperWidth * (iColumn + 1);
                ymax = dPaperHeight * (iRow + 1);
                if (m_Units != paperUnits)
                {
                    xmin=UnitsUtil.ConvertUnits(paperUnits, m_Units, xmin);
                    ymin=UnitsUtil.ConvertUnits(paperUnits, m_Units, ymin);
                    xmax=UnitsUtil.ConvertUnits(paperUnits, m_Units, xmax);
                    ymax=UnitsUtil.ConvertUnits(paperUnits, m_Units, ymax);
                }
                pageBounds.Init(xmin, xmax, ymin, ymax);
            }
            return pageBounds;
        }
        public Envelope GetDeviceBounds(IPrinter Printer, short currentPage, double Overlap, short Resolution)
        {
            var pIPaper=Printer.Paper;
            var pIEnv=pIPaper.PrintableBounds;
            OkUnits units=pIPaper.Units;
            double width = pIEnv.Width;
            double height = pIEnv.Height;
            width=UnitsUtil.ConvertUnits(units,OkUnits.okInches, width);
            height=UnitsUtil.ConvertUnits(units, OkUnits.okInches, height);
            width = Resolution;
            height = Resolution;
            var deviceBounds = new Envelope();
            deviceBounds.Init(0, width, 0, height);
            return deviceBounds;
        }
        public void DrawPaper(IDisplay Display,IDisplayTransformation trans,OutputMode mode, Color? color=null )
        {
            QuerySize(out double dWidth,out double dHeight);
            var bIsZero = ISZERO(dHeight);
            if (bIsZero)
                return ;

            var pIEnvelope=new Envelope();
            pIEnvelope.Init(0, dWidth, 0, dHeight);
			if (mode == OutputMode.ToScreen)
			{
				var pIGeometry = GeometryUtil.MakePolygon(pIEnvelope);
				m_pIShadow?.Draw(Display,trans, pIGeometry);
				m_pIBackground?.Draw(Display,trans, pIGeometry);
				m_pIBorder?.Draw(Display,trans, pIGeometry);

				DrawPrintableArea(Display,trans);
			}
			else
			{

				//var env = trans.ToDevice(pIGeometry)?.EnvelopeInternal;
                if (color!=null)
                {
                    var pIGeometry = GeometryUtil.MakePolygon(pIEnvelope);
                    _pFillSymbol.FillColor = color;
                    _pFillSymbol.SetupDC(Display, trans);
                    _pFillSymbol.Draw(pIGeometry);
                    _pFillSymbol.ResetDC();
                    //Display.FillRectangle((Color)color, new RectangleF((float)env.MinX, (float)env.MinY, (float)env.Width, (float)env.Height));
                }
			}
		}
        public void DrawPrintableArea(IDisplay Display,IDisplayTransformation trans)
        {
            if (m_FormID ==OkPageFormID.okPageFormSameAsPrinter && m_bIsPrintableAreaVisible == true)
            {
                //g_log.dump("DrawPrintableArea");
                var pIEnv = PrintableBounds;
                if (pIEnv!=null)
                {
                    m_pIPrintableAreaSymbol.SetupDC(Display, trans);
                    var g=GeometryUtil.MakePolygon(pIEnv);
                    m_pIPrintableAreaSymbol.Draw(g);
                    //Display.SetSymbol(m_pIPrintableAreaSymbol);
                    //Display.DrawRectangle(pIEnv);
                    m_pIPrintableAreaSymbol.ResetDC();
                }
            }
        }

        public void PrinterChanged(IPrinter Printer)
        {
            m_pIPrinter = Printer;
            if (m_FormID ==OkPageFormID.okPageFormSameAsPrinter && m_bDelayEvents == false)
                Fire_PageSizeChanged();
        }

		#region IXmlSerializable
		public System.Xml.Schema.XmlSchema? GetSchema()
		{
			return null;
		}

        public void ReadXml(XmlElement reader)
        {
            foreach(XmlNode n in reader.ChildNodes)
            {
                var s = n.InnerText;
                switch (n.Name)
                {
                    case "FormID":
                        {
                            this.FormID = (OkPageFormID)Enum.Parse(typeof(OkPageFormID), s);
                        }
                        break;
                    case "Orientation":
                        {
                            this.Orientation = (PaperOrientation)Enum.Parse(typeof(PaperOrientation), s);
                        }
                        break;
                    case "Units":
                        {
                            this.Units = (OkUnits)Enum.Parse(typeof(OkUnits), s);
                        }
                        break;
                    case "Width":
                        this.Width = SafeConvertAux.ToDouble(s);
                        break;
                    case "Height":
                        this.Height = SafeConvertAux.ToDouble(s);
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false, $"not process Property ${n.Name}");
                        break;
                }
            }
        }

  //      public void ReadXml(System.Xml.XmlReader reader)
		//{
		//	SerializeUtil.ReadNodeCallback(reader, "Page", name =>
		//	{
		//		switch (name) {
		//			case "FormID":
		//				{
		//					var s= reader.ReadString();
		//					this.FormID= (OkPageFormID)Enum.Parse(typeof(OkPageFormID), s);
		//				}
		//				break;
		//			case "Orientation":
		//				{
		//					var s = reader.ReadString();
		//					this.Orientation = (PaperOrientation)Enum.Parse(typeof(PaperOrientation), s);
		//				}
		//				break;
		//			case "Units":
		//				{
		//					var s = reader.ReadString();
		//					this.Units = (OkUnits)Enum.Parse(typeof(OkUnits), s);
		//				}
		//				break;
		//			case "Width":
		//				this.Width =SafeConvertAux.ToDouble( reader.ReadString());
		//				break;
		//			case "Height":
		//				this.Height = SafeConvertAux.ToDouble(reader.ReadString());
		//				break;
		//		}
		//	});			
		//}

		//public void WriteXml(System.Xml.XmlWriter writer)
		//{
		//	writer.WriteStartElement(this.GetType().Name);
		//	SerializeUtil.WriteStringElement(writer, "FormID", FormID.ToString());
		//	SerializeUtil.WriteStringElement(writer, "Orientation", Orientation.ToString());
		//	SerializeUtil.WriteStringElement(writer, "Units", Units.ToString());
		//	SerializeUtil.WriteDoubleElement(writer, "Width", Width);
		//	SerializeUtil.WriteDoubleElement(writer, "Height", Height);
		//	writer.WriteEndElement();
		//}
		#endregion

		private void Fire_PageSizeChanged()
        {
            OnPageSizeChanged();
        }
        private void Fire_PageUnitsChanged()
        {
            OnPageUnitsChanged();
        }

        private static bool ISZERO(double x)
        {
            return ((x > 0 ? x : -x) < 9.9999999999999998E-13) ? true : false;
        }
    }

	public interface IBorder
	{
		ILineSymbol? Symbol { get; set; }
		void Draw(IDisplay dc, IDisplayTransformation trans, IGeometry g); //IDisplayTransformation trans,Graphics dc,IGeometry geo);// IDisplay dc, IGeometry g);
		OkEnvelope? QueryBounds(IDisplay pIDisplay, IDisplayTransformation trans, IGeometry pIGeometry);
	}

	public interface IPrinter
    {
        IPaper Paper { get; }
        OkUnits Units { get; set; }
        Envelope PrintableBounds { get; }
    }
    public interface IPaper
    {
        Envelope PrintableBounds { get; }
        OkUnits Units { get; set; }
        void QueryPaperSize(out double width, out double height);
    }

    public interface IFrameDecoration
    {
        double HorizontalSpacing { get; set; }
        double VerticalSpacing { get; set; }
        short CornerRounding { get; set; }
    }

    public interface IShadow
    {
        double HorizontalSpacing { get; set; }
        double VerticalSpacing { get; set; }
        Envelope QueryBounds(Transform trans, IGeometry geometry);
        void Draw(IDisplay dc,IDisplayTransformation trans, IGeometry g);
    }

    public class FrameDecoration : IFrameDecoration
	{
        public FrameDecoration()
        {
        }
        public double HorizontalSpacing
        {
            get;set;
        }

        public double VerticalSpacing
        {
            get;set;
        }
        public short CornerRounding { get; set; }
	}

	public class SymbolShadow : IShadow
    {
        public double HorizontalSpacing
        {
            get;set;
        }

        public double VerticalSpacing
        {
            get; set;
        }

        public Envelope QueryBounds(Transform trans, IGeometry geometry)
        {
            throw new NotImplementedException();
        }
        public void Draw(IDisplay dc,IDisplayTransformation trans, IGeometry g)
        {
            //todo...
        }
    }

	public class SymbolBorder : FrameDecoration, IBorder
	{
		public ILineSymbol? Symbol { get; set; }
		public SymbolBorder(ILineSymbol? ls = null)
		{
			Symbol = ls;
            Symbol ??= SymbolUtil.CreateSimpleLineSymbol(Color.Black,1);// SymbolUtil.CreateDashLineSymbol(Color.Black, 1) as ILineSymbol;
		}
		public void Draw(IDisplay pIDisplay,IDisplayTransformation trans, IGeometry pIGeo)
		{
			if (Symbol == null || Symbol.StrokeColor == null || Symbol.StrokeThickness <= 0)
			{
				return;
			}
            var pIDisplayTransformation = trans;// pIDisplay.DisplayTransformation;
            var pIEnvelope = pIGeo.EnvelopeInternal;
			if (pIDisplayTransformation != null)
			{
				OkUnits units = pIDisplayTransformation.Units;
				double xvalue = UnitsUtil.ConvertUnits(OkUnits.okPoints, units, HorizontalSpacing);
				double yvalue = UnitsUtil.ConvertUnits(OkUnits.okPoints, units, VerticalSpacing);
				//xvalue *= 2;
				//yvalue *= 2;
				pIEnvelope.ExpandBy(xvalue, yvalue);
			}
			else
				pIEnvelope.ExpandBy(2 * HorizontalSpacing, 2 * VerticalSpacing);//, VARIANT_FALSE);

			var pIGeometry = GeometryUtil.MakePolygon(pIEnvelope);// GeometryUtil.MakeLineString(pIEnvelope);
																  //画背景
			var pISymbol = Symbol;
            pISymbol.SetupDC(pIDisplay,trans);//.Graphics, pIDisplayTransformation);
			pISymbol.Draw(pIGeometry);
			pISymbol.ResetDC();
		}

		public OkEnvelope? QueryBounds(IDisplay pIDisplay, IDisplayTransformation trans, IGeometry pIGeometry)
		{
			var pgn = Symbol?.QueryBoundary(pIDisplay,trans, pIGeometry);
			return pgn==null?null:new OkEnvelope(pgn);
		}

	}
	public class SymbolBackground : FrameDecoration, IBackground
    {
        public IFillSymbol FillSymbol { get; set; } = SymbolUtil.CreateSimpleFillSymbol(Color.AliceBlue, null) as IFillSymbol;//,SymbolUtil.CreateSimpleLineSymbol(Color.Beige) as ILineSymbol) as IFillSymbol;
																															 //private double m_dHorizontalSpacing, m_dVerticalSpacing;
																															 //private short m_iCornerRounding;
		public SymbolBackground(Color? fillColor=null)
		{
			if (fillColor != null)
			{
				FillSymbol.FillColor = fillColor;
			}
		}
		public void Draw(IDisplay pIDisplay,IDisplayTransformation trans, IGeometry Geometry)
        {
            System.Diagnostics.Debug.Assert(Geometry != null);
            System.Diagnostics.Debug.Assert(FillSymbol != null);

            //var pIDisplayTransformation=trans,pIDisplay.DisplayTransformation;

            var pIEnvelope = Geometry.EnvelopeInternal;
            double dXMin= pIEnvelope.MinX, dYMin= pIEnvelope.MinY, dXMax= pIEnvelope.MaxX, dYMax= pIEnvelope.MaxY;
            double unitlength = 1;

            //if (pIDisplayTransformation!=null)
            //{
                OkUnits units=trans.Units;
                unitlength=UnitsUtil.ConvertUnits(OkUnits.okPoints, units, unitlength);
            //}

            dXMin -= HorizontalSpacing * unitlength;
            dXMax += HorizontalSpacing * unitlength;
            dYMin -= VerticalSpacing * unitlength;
            dYMax += VerticalSpacing * unitlength;
            pIEnvelope.Init(dXMin, dXMax, dYMin,  dYMax);

            var pIGeometry =GeometryUtil.MakePolygon(pIEnvelope);
            var pISymbol = FillSymbol as ISymbol;
            pISymbol.SetupDC(pIDisplay,trans);//, pIDisplayTransformation);
            pISymbol.Draw(pIGeometry);
            pISymbol.ResetDC();
        }

		public OkEnvelope? QueryBounds(IDisplay pIDisplay,IDisplayTransformation trans, IGeometry pIGeometry)
		{
			var pgn=FillSymbol.QueryBoundary(pIDisplay,trans,pIGeometry);
			return pgn==null?null:new OkEnvelope(pgn);
		}
	}

    public class Display
    {
        //public Graphics g;
        public Transform trans;
        public void SetSymbol(ISymbol symbol)
        {

        }
        public void DrawRectangle(Envelope env)
        {

        }
    }

    public enum OkPageFormID
    {
        okPageFormLetter,
        okPageFormLegal,
        okPageFormTabloid,
        okPageFormC,
        okPageFormD,
        okPageFormE,
        okPageFormA5,
        okPageFormA4,
        okPageFormA3,
        okPageFormA2,
        okPageFormA1,
        okPageFormA0,
        okPageFormCUSTOM,
        okPageFormSameAsPrinter
    }

    public enum OkPageToPrinterMapping
    {
        okPageMappingCrop,
        okPageMappingScale,
        okPageMappingTile
    }
}
