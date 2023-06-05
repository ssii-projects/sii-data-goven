using System.Drawing;
using Agro.LibCore;
using GeoAPI.Geometries;
using System.Xml;
//using SkiaSharp;

namespace Agro.GIS
{
    public interface ILocatorRectangle
    {

    }
    public interface IFrameElement 
    {
        IBorder Border { get; set; }
        IBackground Background { get; set; }
        IShadow Shadow { get; set; }
        bool DraftMode { get; set; }
        object Object { get; }
    }
    public interface IFrameDraw
    {
        void DrawBackground(/*IDisplay pIDisplay,IDisplayTransformation trans,*/ ICancelTracker CancelTracker);
        void DrawDraftMode(/*IDisplay pIDisplay, IDisplayTransformation trans,*/ ICancelTracker CancelTracker, OutputMode mode);
        void DrawForeground(/*IDisplay pIDisplay, IDisplayTransformation trans,*/ ICancelTracker CancelTracker);
        //void Thumbnail([out, retval] OLE_HANDLE* pVal);
    }
    public interface IMapFrame : IFrameElement
    {
        Map Map { get; set; }
        IGraphicsContainer Container { get; set; }
        OkExtentTypeEnum ExtentType { get; set; }
        double MapScale { get; set; }
        OkEnvelope MapBounds { get; set; }
        ILocatorRectangle LocatorRectangle(int Index);
        //void LocatorRectangle(int Index, ILocatorRectangle val);
        int LocatorRectangleCount { get; }
        void RemoveAllLocatorRectangles();
        void AddLocatorRectangle(ILocatorRectangle Locator);
        //HRESULT CreateSurroundFrame([in] IUID* CLSID, [in] IMapSurround* optionalStyle, [out, retval] IMapSurroundFrame** MapSurroundFrame);
    }

    public enum OkExtentTypeEnum
    {
        okExtentDefault,
        okExtentScale,
        okExtentBounds
    }
    public interface IMapGrid
    {
        //HRESULT Border([out, retval] IMapGridBorder** pVal);
        //[propput, helpstring("property Border")]
        //HRESULT Border([in] IMapGridBorder* newVal);
        //[helpstring("method Draw")]
        void Draw(IDisplay pIDisplay,IDisplayTransformation trans,IMapFrame pIMapFrame);
    }
    public class MapFrame: ElementBase, IMapFrame, IFrameDraw, IBoundsProperties,IDisposable
	{
        private readonly List<IMapGrid> m_arrIMapGrid=new();
        private readonly List<ILocatorRectangle> m_arrILocatorRectangle=new();
        private IGraphicsContainer m_pIContainer;
        private OkExtentTypeEnum m_ExtentType;
        private Map m_pIMap = new();
        private OkEnvelope m_pIMapBounds;

        //private readonly PageLayout _pageLayout;
        //		private MapFrame()
        //		{
        //		}
        //		public MapFrame(PageLayout pl, Map map)//:base(new EnvelopeTracker())
        //        {
        //			//_pageLayout = pl;
        //            Map = map;
        //            //Border = new SymbolBorder(SymbolUtil.CreateSimpleLineSymbol(Color.Black)as ILineSymbol);
        //			var sd = pl.ScreenDisplay;
        //            base.Activate(sd,pl.Transformation);
        //			//map.OnContentChanged += (sender,bmp,rc,cct) =>
        //			//  {
        //			//	  Console.WriteLine("MapFrame ContentChanged, rc=" + rc+",sender is "+sender);
        //			//	  //if (sender is Map)
        //			//	  //{
        //			//	  // return;
        //			//	  //}
        //			//	  #region yxm 2019-1-2
        //			//	  if (cct == ContentChangeType.Pan)
        //			//	  {
        //			//		  if (Flush(bmp, true))
        //			//		  {
        //			//			  pl.FireContenChanged(this, pl.ScreenDisplay.BackImage, rc,cct);
        //			//		  }
        //			//	  }
        //			//	  else
        //			//	  {
        //			//		  pl.Invoke(()=>
        //			//		  pl.RefreshCache()
        //			//		  );
        //			//	  }
        //			//	  #endregion
        //			//	  //if (true)
        //			//	  //{
        //			//		 // if (Flush(bmp,true))
        //			//		 // {
        //			//			//  pl.FireContenChanged(this, pl.ScreenDisplay.BackImage, rc,cct);
        //			//		 // }
        //			//	  //}
        //			//	  //else
        //			//	  //{
        //			//		 // if (rc != RectangleF.Empty)
        //			//		 // {
        //			//			//  var bm = pl.ScreenDisplay.BackImage;
        //			//			//  using (var g = ImageUtil.CreateGraphics(bm))
        //			//			//  {
        //			//			//	  //RectangleF r;
        //			//			//	  g.ResetTransform();
        //			//			//	  g.SetClip(rc);
        //			//			//	  g.Clear(Color.Transparent);
        //			//			//	  g.DrawImage(bmp, rc.X, rc.Y);
        //			//			//	  g.ResetClip();

        //			//			//	  DrawForeground(new GraphicsDisplay(pl.ScreenDisplay.DisplayTransformation, g), null);
        //			//			//  }
        //			//			//  pl.FireContenChanged(this, bm, rc,cct);
        //			//		 // }
        //			//		 // else
        //			//		 // {
        //			//			//  Console.WriteLine();
        //			//		 // }
        //			//	  //}
        //			//  };

        //#if DEBUG
        //			//this.Background = new SymbolBackground(Color.LightSeaGreen);
        //#endif
        //		}

        public override void Activate(IDisplay pIDisplay, IDisplayTransformation trans)
        {
            base.Activate(pIDisplay, trans);
            m_pIMap.Display = pIDisplay;
        }
        #region IFrameElement
        public IBorder Border { get; set; } = new SymbolBorder(SymbolUtil.CreateSimpleLineSymbol(Color.Black) as ILineSymbol);
		public IBackground Background { get; set; } = new SymbolBackground(Color.White);
		public IShadow Shadow { get; set; }
		public bool DraftMode { get; set; } = false;
      public object Object { get { return Map; } }
#endregion

#region IMapFrame
        public Map Map
        {
            get { return m_pIMap; }
            set { m_pIMap = value; }
        }
        public IGraphicsContainer Container { get { return m_pIContainer; } set { m_pIContainer = value; } }
        public OkExtentTypeEnum ExtentType { get { return m_ExtentType; } set { m_ExtentType = value; } }
        public double MapScale {
            get
            {
                return m_pIMap.MapScale;
            }
            set
            {
                m_pIMap.MapScale = value;
            }
        }
        public OkEnvelope MapBounds {
            get
            {
                //if (m_pIMap!=null)
                //{
                    var pIActiveView = m_pIMap as ActiveView;
                    var pISD=pIActiveView.Display;
                    var pIDT = pIActiveView.Transformation;
                    var pVal=pIDT.DeviceFrame;
                    //get_DeviceFrame(pIDT, pVal);
                    //ToMapEnvelope(pIDT, *pVal);
                    return pIDT.ToMap(new OkEnvelope(pVal.Left,pVal.Right,pVal.Top,pVal.Bottom));
                //}
                //return null;
            }
            set
            {
                m_pIMapBounds = value;
            }
        }
        public ILocatorRectangle LocatorRectangle(int Index)
        {
            return m_arrILocatorRectangle[Index];
        }
        public int LocatorRectangleCount
        {
            get
            {
                return m_arrILocatorRectangle.Count;
            }

        }
        public void RemoveAllLocatorRectangles()
        {
            m_arrILocatorRectangle.Clear();
        }
        public void AddLocatorRectangle(ILocatorRectangle Locator)
        {
            m_arrILocatorRectangle.Add(Locator);
        }
#endregion

#region IFrameDraw
        public void DrawBackground(/*IDisplay pIDisplay,IDisplayTransformation trans,*/ ICancelTracker CancelTracker)
        {
            if ((Shadow == null || Background == null)&&m_pIDisplay==null&&this.Geometry!=null)
            {
                return;
            }
            var dc = m_pIDisplay!;
            var pIEnvelope = QueryBounds();//pIDisplay,trans);
            var geo = GeometryUtil.MakePolygon(pIEnvelope!);
            Shadow?.Draw(dc,_trans, geo);
            Background?.Draw(dc,_trans, geo);
        }
        public void DrawDraftMode(/*IDisplay pIDisplay,IDisplayTransformation trans,*/ ICancelTracker CancelTracker, OutputMode mode)
        {

            if (!DraftMode)
            {
                //var pIActiveView = m_pIMap as ActiveView;
                //var pIScreenDisplay = pIActiveView.ScreenDisplay;
                var bIsCacheDirty = true;// pIScreenDisplay.IsCacheDirty();// OkScreenCache.okScreenRecording);

                if (bIsCacheDirty)
                {
                    //pIDisplay.Progress(0);
                    m_pIMap.Draw(CancelTracker, mode);
                    //if (mode != OutputMode.ToScreen)
                    //{
                    //    Flush(((SkiaDisplay)m_pIMap.ScreenDisplay).GetBitmap(), false);
                    //}
                }
                else
                {
                    //Flush(m_pIMap.ScreenDisplay.BackImage, false);
                    //var rc = m_pIMap.ScreenDisplay.ClipRect;
                    //if (rc != RectangleF.Empty)
                    //{
                    //	pIDisplay.Graphics.DrawImage(m_pIMap.ScreenDisplay.BackImage, rc.X, rc.Y);
                    //}
                    Console.WriteLine("MapFrame DrawCache");
                }

            }
            else
            {
                throw new NotImplementedException();
                //var pIEnv = Geometry.EnvelopeInternal;
                //var pITextSymbol = pIDisplay.SymbolFactory.CreateTextSymbol();// new TextSymbol();
                //pITextSymbol.Font.FontFamily = "黑体";
                //pITextSymbol.Font.FontSize = 32;
                //pITextSymbol.HorizontalAlignment = eTextHorizontalAlignment.eTHACenter;
                //pITextSymbol.VerticalAlignment = eTextVerticalAlignment.eTVACenter;
                //pITextSymbol.Text = "地图";

                //var pIPoint = GeometryUtil.MakePoint((pIEnv.MinX + pIEnv.MaxX) / 2, (pIEnv.MinY + pIEnv.MaxY) / 2);
                //var pISymbol = pITextSymbol as ISymbol;

                ////var pIDT = pIDisplay.DisplayTransformation;
                //pISymbol.SetupDC(pIDisplay,trans);//.Graphics, pIDT);
                //pISymbol.Draw(pIPoint);
                //pISymbol.ResetDC();
            }
        }
        public void DrawForeground(/*IDisplay pIDisplay,IDisplayTransformation trans,*/ICancelTracker CancelTracker)
        {
            if (Border != null&&Geometry!=null&&m_pIDisplay is IDisplay dc)
            {
                var pIEnvelope = QueryBounds();// pIDisplay,trans);
                var geo = GeometryUtil.MakePolygon(pIEnvelope!);
                Border.Draw(dc,_trans, geo);
            }
        }
#endregion

#region IElement

    //    public override IGeometry Geometry
    //    {
    //        get
    //        {
				//return base.Geometry;// m_pIGeo;
    //        }
    //        set
    //        {
    //            base.Geometry = value;
    //            //EnvelopeChanged();
    //        }
    //    }
        public override OkEnvelope? QueryBounds()//IDisplay pIDisplay,IDisplayTransformation trans)
        {
            if (this.Geometry != null)
            {
                var env = new OkEnvelope(this.Geometry.EnvelopeInternal,Geometry.GetSpatialReference());
                return env;
            }
            return null;
        }
        public override IPolygon? QueryOutline()// IDisplay pIDisplay,IDisplayTransformation trans)
        {
            if (this.Geometry != null)
            {
                var pgn = GeometryUtil.MakePolygon(this.Geometry.EnvelopeInternal,this.Geometry.GetSpatialReference());
                return pgn;
            }
            return null;
        }
        public override void Draw(ICancelTracker cancel, OutputMode mode)
        {
            if (m_pIDisplay is IDisplay dc)
            {
                //try
                //{
                //    dc.SaveDC();                  
                    var rc1 = m_pIMap.Transformation.DeviceFrame;
                    if (rc1.Right < 0 || rc1.Bottom < 0 || rc1.Left > dc.Width || rc1.Top > dc.Height)
                    {
                        return;
                    }
                    //dc.SetClipRect(rc1);
                    var trans = _trans;
                    try
                    {
                        int left = Math.Max(0, rc1.Left);
                        int top = Math.Max(0, rc1.Top);
                        int right = Math.Min(rc1.Right, dc.Width);
                        int bottom = Math.Min(rc1.Bottom, dc.Height);
                        //g.SetClip(new Rectangle(left, top, right - left, bottom - top));
                        //g.SetClip()
                        var pIEnvelope = QueryBounds();// dc, trans);
                        var geo = GeometryUtil.MakePolygon(pIEnvelope);


                        DrawBackground(/*dc, trans,*/ cancel);

                        #region yxm 2018-9-29
                        //InvokeUtil.Invoke(_pageLayout.Host.Container, () =>
                        // {
                        //	 _pageLayout.OnContentChanged?.Invoke(dc.BackImage);
                        // });

                        #endregion


                        DrawDraftMode(/*pIDisplay, trans,*/ cancel, mode);

                        for (int i = 0; i < m_arrIMapGrid.Count; i++)
                        {
                            m_arrIMapGrid[i].Draw(dc, trans, this);
                        }

                    }
                    finally
                    {
                        try
                        {
                            //g.SetClip(ClipBounds);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("MapFrame.Draw:" + ex.Message);
                        }
                    }
                    DrawForeground(/*pIDisplay, trans,*/ cancel);
                //}
                //finally
                //{
                //    dc.RestorDC();
                //}
            }
        }
		public override void DrawCache()//IDisplay display,IDisplayTransformation trans)
		{
            throw new NotImplementedException();
            /*
			ICancelTracker cancel = null;
			var dc = display;
			var rc1 = m_pIMap.ScreenDisplay.DisplayTransformation.DeviceFrame;
			if (rc1.Right < 0 || rc1.Bottom < 0 || rc1.Left > dc.Width || rc1.Top > dc.Height)
			{
				return;
			}
			try
			{
				int left = Math.Max(0, rc1.Left);
				int top = Math.Max(0, rc1.Top);
				int right = Math.Min(rc1.Right, dc.Width);
				int bottom = Math.Min(rc1.Bottom, dc.Height);
				//g.SetClip(new Rectangle(left, top, right - left, bottom - top));
				//g.SetClip()
				var pIEnvelope = QueryBounds(display);
				var geo = GeometryUtil.MakePolygon(pIEnvelope);


				DrawBackground(display, cancel);

				if (DraftMode)
				{
					DrawDraftMode(display, cancel,OutputMode.ToScreen);
				}
				else
				{
					var rc = m_pIMap.ScreenDisplay.ClipRect;
					if (rc != RectangleF.Empty)
					{
						dc.Graphics.DrawImage(m_pIMap.ScreenDisplay.BackImage, rc.X, rc.Y);
					}
				}
				for (int i = 0; i < m_arrIMapGrid.Count; i++)
				{
					m_arrIMapGrid[i].Draw(display, this);
				}

			}
			finally
			{
				try
				{
					//g.SetClip(ClipBounds);
				}
				catch (Exception ex)
				{
					Console.WriteLine("MapFrame.Draw:" + ex.Message);
				}
			}
			DrawForeground(display, cancel);
            */
		}
		//public override void Transform(AffineTransformation trans)
		//{
		//	base.Transform(trans);
		//	EnvelopeChanged();
		//}
#endregion

#region IBoundsProperties
		public	bool FixedAspectRatio {get;set;
			//get
			//{
			//	var pISelectionTracker =m_pISelectionTracker;
			//	var pIDisplayFeedback=pISelectionTracker.QueryResizeFeedback();
			//	var pIResizeEnvelopeFeedback = pIDisplayFeedback as IResizeEnvelopeFeedback;
			//	var constraints = pIResizeEnvelopeFeedback.Constraint;
			//	return constraints == OkEnvelopeConstraints.okEnvelopeConstraintsAspect;
			//}
			//set
			//{
			//	var pISelectionTracker =m_pISelectionTracker;
			//	var pIDisplayFeedback=pISelectionTracker.QueryResizeFeedback();
			//	var pIResizeEnvelopeFeedback = pIDisplayFeedback as IResizeEnvelopeFeedback;
			//	pIResizeEnvelopeFeedback.Constraint=value ?OkEnvelopeConstraints.okEnvelopeConstraintsAspect : OkEnvelopeConstraints.okEnvelopeConstraintsNone;
			//}
		}
		public	bool FixedSize { get; }
        #endregion

        //protected override IElementPropertyPage GetPropertyPage()
        //{
        //	return new MapFramePropertyPage();
        //}
        protected override void ReadXmlElement(XmlElement e, SerializeSession session)
        {
            var name = e.Name;
            switch (name)
            {
                case "Map":
                    {
                        this.Map.ReadXml(e, session);// newMapDocumentPath,oldMapDocumentPath);
                    }
                    break;
                case "Border":
                    {
                        if (SerializeUtil.ReadBorder(e, Border) is IBorder bdr)
                        {
                            Border = bdr;
                        }
                    }
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, $"not process Property ${name}");
                    break;
            }
        }

  //      public override void ReadXml(XmlReader reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
		//{
		//	SerializeUtil.ReadNodeCallback(reader, this.GetType().Name, name =>
		//	{
		//		switch (name) {
		//			case "Map":
		//				{
		//					this.Map.ReadXml(reader, session);// newMapDocumentPath,oldMapDocumentPath);
		//				}break;
		//			case "Border":
		//				{
		//					Border=SerializeUtil.ReadBorder(reader, name, Border);
		//				}break;
		//			default:
		//				base.ReadXmlElement(reader, name);
		//				break;
		//		}
		//	});
		//}
		//public override void WriteXml(System.Xml.XmlWriter writer, bool fWriteDataSource = true)
		//{
		//	writer.WriteStartElement(this.GetType().Name);
		//	base.WriteInnerXml(writer);
		//	this.Map.WriteXml(writer);
		//	SerializeUtil.WriteBorder(writer, "Border", Border);
		//	writer.WriteEndElement();
		//}

		internal void UpdateLayout()
        {
            EnvelopeChanged();
        }
        //private int _left, _top;
        private void EnvelopeChanged()
        {
			//var pSD = _pageLayout.ScreenDisplay;

			if (Geometry == null || m_pIDisplay == null)
                return;

            //var pIEnv = m_pIGeo.EnvelopeInternal;//.get_Envelope(&pIEnv);
            //var pIDT = _pageLayout.Transformation;// pSD.DisplayTransformation;
            //if (pIDT == null) return;
            var pIEnv=_trans.ToDevice(Geometry!)!.EnvelopeInternal;
            //ToDeviceEnvelope(pIDT, pIEnv);

            var map = m_pIMap;
            //var pIAV = m_pIMap;
            //var pISD = m_pIMap.ScreenDisplay;// (&pISD);
                                             //var pIDT1 = pISD.DisplayTransformation;// (&pIDT1);
                                             ////put_DeviceFrame(pIDT1, pIEnv);
                                             //pIDT1.SetDeviceFrameF(pIEnv);

            //_left = (int)pIEnv.MinX;
            //_top=(int)pIEnv.MinY;
            //var rc = new Rectangle(/*(int)pIEnv.MinX, (int)pIEnv.MinY*/0,0, (int)pIEnv.Width, (int)pIEnv.Height);
            var rc = new Rectangle((int)pIEnv.MinX, (int)pIEnv.MinY, (int)pIEnv.Width, (int)pIEnv.Height);

            map.Transformation.SetDeviceFrame(rc);
            //pISD.Resize(rc.Width, rc.Height);//.SetDeviceFrame(rc,true);
            //pISD.DisplayTransformation.Resolution = pSD.DisplayTransformation.Resolution;
            map.Transformation.Resolution= _trans.Resolution;

            #region yxm 2018-9-30
            var dff=_trans.DeviceFrameF;
            if (dff != null)
            {
                var left = (float)Math.Max(dff.MinX, pIEnv.MinX);
                var top = (float)Math.Max(dff.MinY, pIEnv.MinY);
                var r = (float)Math.Min(dff.MaxX, pIEnv.MaxX);
                var b = (float)Math.Min(dff.MaxY, pIEnv.MaxY);
                var clipRc = r <= left || b <= top ? RectangleF.Empty : new RectangleF(left, top, r - left, b - top);
                map.SetClipRect(clipRc);
            }
            #endregion

            map.Invalidate();
            Fire_MapFrameResized(this);
        }

        //private bool Flush(SKBitmap bmp, bool fDrawForground)
        //{
        //    var pl = _pageLayout;
        //    var canvas=pl.ScreenDisplay.GetCanvas();
        //    //canvas.DrawCircle(10, 10, 20, new SKPaint()
        //    //{
        //    //    Color=SKColor.Parse("#FF0000"),
        //    //    Style=SKPaintStyle.Stroke,
        //    //    StrokeWidth=2,
        //    //});
        //    //var rc=m_pIMap.ScreenDisplay.DisplayTransformation.DeviceFrame;
        //    canvas.DrawBitmap(bmp, _left,_top);

        //    ((SkiaDisplay)m_pIMap.ScreenDisplay).SaveToFile("./ttx1.jpg");
        //    /*
        //    var rc = m_pIMap.ScreenDisplay.ClipRect;
        //    //var bmp = m_pIMap.ScreenDisplay.BackImage;//


        //    if (rc != RectangleF.Empty)
        //    {
        //        var bm = pl.ScreenDisplay.BackImage;
        //        using (var g = ImageUtil.CreateGraphics(bm))
        //        {
        //            //RectangleF r;
        //            g.ResetTransform();
        //            g.SetClip(rc);
        //            //g.Clear(Color.Transparent);

        //            var dc = new GraphicsDisplay(pl.Transformation, g);
        //            this.DrawBackground(dc, null);
        //            g.DrawImage(bmp, rc.X, rc.Y);
        //            //#if DEBUG
        //            //					bmp.Save("d:/focusmap.jpg");
        //            //#endif
        //            g.ResetClip();
        //            if (fDrawForground)
        //            {
        //                DrawForeground(new GraphicsDisplay(pl.ScreenDisplay.DisplayTransformation, g), null);
        //            }
        //        }
        //        //pl.FireContenChanged(this, bm, rc);
        //        return true;
        //    }
        //    */
        //    return false;

        //}

        private void Fire_MapFrameResized(IMapFrame mapFrame)
        {

        }

		public void Dispose()
		{
			Map.Dispose();
		}
	}
}
