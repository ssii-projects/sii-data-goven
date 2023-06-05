using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System;
using System.Drawing;


namespace Agro.GIS
{
	public interface ITransformation
    {

    }
    public interface IDisplayTransformation
    {
        /// <summary>
        /// Indicates if resolution is tied to visible bounds. If true, zooming in magnifies contents (i.e., zoom in on page).
        /// ZoomResolution is a flag that tells the DisplayTransformation whether or not to automatically adjust the device resolution (pixels per inch) based on the current zoom level.  This is useful when you want to define a page display.  With this kind of display, graphics are drawn at their actual size, i.e., scale is 1:1.  Zooming in really means magnifying the graphics when drawing them to a computer monitor.  When a map page is exported or printed, the zoom level is always 100% and the extent is defined by the page.  Use ZoomResolution to specify this page-style behavior.  With ZoomResolution set to true, the DisplayTransformation monitors the current zoom level.  When the display is zoomed to 100%, a higher effective resolution is calculated.  This has the effect of automatically magnifying all drawing.  The Resolution property always returns the effective resolution.  ScaleRatio always returns 1:1.
        /// Note, when ZoomResolution is true, in order to get the physical device resolution, you must temporarily set ZoomResolution to false, get the Resolution, and then restore the ZoomResolution to true.
        /// To find the current zoom percentage, simply divide the effective resolution by the physical resolution and multiply by 100.  This is what IPageLayout::ZoomPercent does.
        /// If you don't want graphics to scale as you zoom in, set this flag to false.  For example, the Map object sets its DisplayTransformation's ZoomResolution property to false so that the contents of the map are not magnified when you zoom in on them.  The PageLayout sets its DisplayTransformaion's ZoomResolution property to true.
        /// </summary>
        bool ZoomResolution { get; set; }

        /// <summary>
        /// Resolution of the device in dots (pixels) per inch.
        /// In layout view, when the Page is zoomed 1:1, the resolution equals the device resolution.  When the Page is zoomed to > 100%, the resolution is set higher so that graphics are magnified.
        /// In data view, the Map object always has its resolution set to that of the display.Graphics are not magnified in this view when you zoom in.
        /// The units are always in pixels per inch.
        /// After setting the resolution, the ITransformEvents::ResolutionUpdated event is fired.
        /// </summary>
        float Resolution { get; set; }

        /// <summary>
        /// Scale between FittedBounds and DeviceFrame.
        /// Several objects that manage a DisplayTransformation, such as the Map object, use this property to control the map scale.  
        /// Map scale is the relationship between the dimensions of features on a map and the geographic objects they represent on the earth, 
        /// commonly expressed as a fraction or a ratio. 
        /// A map scale of 1/100,000 or 1:100,000 means that one unit of measure on the map equals 100,000 of the same units on the earth. 
        /// When you zoom in or out in ArcMap's data view, you are changing this property on the Map's DisplayTransformation .
        /// The Map object has a short-cut(IMap::MapScale) directly to this property on its DisplayTransformation object.
        /// The PageLayout object does not use this property the same way as the Map object. 
        /// In ArcMap's layout view, zooming in is akin to using a magnifying glass to zoom in on the page where the map is displayed. 
        /// When you zoom in, you are magnifying what is actually going to print.  
        /// To get this effect, the PageLayout objects always keeps its ReferenceScale set to zero and its ScaleRatio set to 1:1 (meaning that 1 inch on the screen equals one inch on the printer).
        /// </summary>
        double ScaleRatio { get; set; }

        double Rotation { get; set; }

        /// <summary>
        /// Reference scale for computing scaled symbol sizes.
        /// All symbols are drawn relative to the scale value set in this property.  By default this value is zero.  Symbol size is true at the reference scale.  When the reference scale is 0, symbols are always drawn at the same size regardless of the map scale (ScaleRatio).  For example, if you set your labels to display with 10 pt font, they will appear as 10 pt text at any map scale.  When the reference scale is greater than 0, symbols are drawn at a size relative to the map scale.  For example, if you set your labels to display with 10 pt font and have a reference scale of 10,000, the labels will appear as 10 pt only at that map scale; as you zoom in, the size of all symbols increases because the size of the symbols is based on the smaller scale.
        /// The Map object has a short-cut(IMap::ReferenceScale) directly to the ReferenceScale property on its DisplayTransformation object.
        /// The PageLayout object does not use ReferenceScale and ScaleRatio the same way as the Map object.  In layout view, zooming in is more like a magnifying glass looking at what is going to print.When you zoom in, you only magnify what is actually going to print.To get this effect, the ReferenceScale is always set to zero and the ScaleRatio is always set to 1:1 (i.e., 1 inch on the screen equals one inch on the printer).  In data view however, zooming in or out, changes what gets printed - WYSIWYG(What You See Is What You Get).
        /// </summary>
        double ReferenceScale { get; set; }

        bool SuppressEvents { get; set; }

        OkUnits Units { get; set; }

        /// <summary>
        /// Full extent in world coordinates.
        /// </summary>
        OkEnvelope Bounds { get; set; }

        /// <summary>
        /// Visible extent in world coordinates.
        /// </summary>
        OkEnvelope VisibleBounds { get; set; }

        /// <summary>
        /// The VisibleBounds is adjusted to match the aspect ratio of the DeviceFrame.  
        /// This adjusted rectangle is stored in FittedBounds and VisibleBounds stores the actual rectangle specified by the client.  
        /// In this way, the FittedBounds is the true visible extent of the display and is a read-only property.
        /// </summary>
        OkEnvelope FittedBounds { get; }
        /// <summary>
        /// Visible extent in device coordinates.
        /// Each DisplayTransformation must have its Bounds, VisibleBounds, and DeviceFrame set.  The DeviceFrame is normally the full extent of the device with the origin equal to (0, 0).  Output can also be directed to some rectangle on the device by specifying the rectangle as the device frame.
        /// For example, if a Map object is not framed, as in the case in ArcMap's data view, the Map uses the Windows API call GetClientRect to get the coordinates of the client (drawing) area of a window and stores them in this property.  This tells the Map where in the window it can draw.
        /// A DeviceFrame can be obtained from IMxDocument::ActiveView::ScreenDisplay::DisplayTransformation::DeviceFrame.It will contain the full extent of the map window.There may be a difference between the values returned in Map view or Layout view due to the possible presence of rulers..
        /// You may also cast a reference to the document's page layout (as found in IMxDocument::PageLayout) into an IActiveView. The IMxDocument::ActiveView::ScreenDisplay::DisplayTransformation::DeviceFrame will return the same data as above corresponding to the current device extent of the page layout.
        /// Finally if you cast a reference to the map (e.g.from IMxDocument::FocusMap) into an IActiveView, the IMxDocument::ActiveView::ScreenDisplay::DisplayTransformation::DeviceFrame will return the current device extent of the map.In Map view it will be the same as above, but in Layout view it will give back the size of the dataframe in which the map is drawn.
        /// Setting the DeviceFrame will fire the ITransformEvents::DeviceFrameUpdated event.  Clients typically need to know when the device frame has changed sized so they can redraw.  For example, the ScreenDisplay object listens for this event so that it may update its caches, cause a refresh, and update the scrollbars.
        /// If you are working within ArcMap, no display object's DeviceFrame property should be changed programmatically.
        /// </summary>
        Rectangle DeviceFrame { get; }
		void SetDeviceFrame(Rectangle value);
        /// <summary>
        /// Current spatial reference.
        /// </summary>
        SpatialReference SpatialReference { get;set; }
        Envelope DeviceFrameF { get; }
		void SetDeviceFrameF(Envelope env);
        #region yxm extends
        double[] MatrixEntries { get; }
        IGeometry? ToDevice(IGeometry? g);
		IGeometry? ToMap(IGeometry? g);

		/// <summary>
		/// Applies this transformation to the <paramref name="src" /> coordinate
		/// and places the results in the <paramref name="dest" /> coordinate
		/// (which may be the same as the source).
		/// </summary>
		/// <param name="src"> the coordinate to transform</param>
		/// <param name="dest"> the coordinate to accept the results</param>
		/// <returns> the <code>dest</code> coordinate</returns>
		Coordinate ToMap(Coordinate src, Coordinate dest);
		/// <summary>
		/// 返回一个新的对象
		/// </summary>
		/// <param name="env"></param>
		/// <returns></returns>
		OkEnvelope ToMap(OkEnvelope env);
		/// <summary>
		/// 返回一个新的对象
		/// </summary>
		/// <param name="env"></param>
		/// <returns></returns>
		OkEnvelope ToDevice(OkEnvelope env);
        Coordinate ToDevice(Coordinate src, Coordinate dest);

        double ToMapSizeX(double screenSize);
        double ToMapSizeY(double screenSize);
        IPolygon GetViewPolygon();

		/// <summary>
		/// 获取地图坐标到屏幕坐标的变换矩阵
		/// </summary>
		/// <returns></returns>
        AffineTransformation GetRawTransformation();
        #endregion
    }

	public class DisplayTransformation : IDisplayTransformation
    {
        public static readonly DisplayTransformation Dummy = new();

        private readonly AffineTransformation m_trans = new();
        private AffineTransformation m_inv_trans;


        private OkEnvelope m_pIBounds;
        private OkEnvelope m_pIVisibleBounds;
        private OkEnvelope m_pIFittedBounds;
        private OkEnvelope m_pIDeviceFrame;
		private double m_dRotation = 0;
        private float m_dResolution=96;
		private double m_dReferenceScale = 0;
        private OkUnits m_Units=OkUnits.okUnknownUnits;
		
		public DisplayTransformation()
        {
			//using (var g = Graphics.FromHwnd(IntPtr.Zero))
			//{
			//	m_dResolution = g.DpiX;
			//}
		}

        #region IDisplayTransformation
        public OkEnvelope Bounds
        {
            get
            {
                if (m_pIBounds!=null)
                {
                    var pIEnv = new OkEnvelope(m_pIBounds);
                    return pIEnv;
                }
                return null;
            }
            set
            {
				if (value == null)
				{
					m_pIBounds = null;
					return;
				}
                if (m_pIBounds == null)
                    m_pIBounds = new OkEnvelope(value);
                else
                {
                    m_pIBounds.Init(value);                   
                }
				m_pIBounds=new OkEnvelope(m_pIBounds.Project(this.SpatialReference));

                if (SuppressEvents == false)
                    Fire_BoundsUpdated(this);
                if (m_pIVisibleBounds == null)
                    VisibleBounds=Bounds;
            }
        }
        public OkEnvelope VisibleBounds {
            get
            {
                if (m_pIVisibleBounds!=null)
                {
                    var pIEnv = new OkEnvelope(m_pIVisibleBounds);
                    return pIEnv;
                }
                return null;
            }
            set
            {
                System.Diagnostics.Debug.Assert(value != null);
                if (m_pIVisibleBounds == null)
                    m_pIVisibleBounds = new OkEnvelope(value);
                else
                {
                    m_pIVisibleBounds.Init(value);
                }

                var pISpatialRef= value.SpatialReference;
                m_pIVisibleBounds.SpatialReference=pISpatialRef;
				m_pIVisibleBounds = new OkEnvelope(m_pIVisibleBounds.Project(SpatialReference));
                Calc_matrix();
                if (SuppressEvents == false)
                    Fire_VisibleBoundsUpdated(this, true);
            }
        }

        public OkEnvelope FittedBounds {
            get
            {
                if (m_pIFittedBounds!=null)
                {
                    var pIEnv=new OkEnvelope(m_pIFittedBounds);
                    return pIEnv;
                }
                return null;
            }
        }
        public Rectangle DeviceFrame {
            get
            {
                var pIEnv=DeviceFrameF;
                if (pIEnv!=null)
                {
                    return ToRectangle(pIEnv);
                }
                return Rectangle.Empty;
            }
        }
		public void SetDeviceFrame(Rectangle value)
		{
			var pIEnv = new OkEnvelope(new Envelope(value.Left, value.Right, value.Top, value.Bottom));
			SetDeviceFrameF(pIEnv);
		}
		public Envelope DeviceFrameF
        {
            get
            {
                if (m_pIDeviceFrame != null)
                {
                    var pIEnv = new OkEnvelope(m_pIDeviceFrame);
                    return pIEnv;
                }
                return null;
            }
        }
		public void SetDeviceFrameF(Envelope value)
		{
			if (m_pIDeviceFrame == null)
				m_pIDeviceFrame = new OkEnvelope(value);
			else
			{
				m_pIDeviceFrame.Init(value);
			}
			Calc_matrix();
			if (SuppressEvents == false)
				Fire_DeviceFrameUpdated(this, true);
		}
		public double ScaleRatio {
            get
            {
                if (ZoomResolution == true)
                {
                    return 1.0;
                }
                var Scale = m_trans.MatrixEntries[0];
                if (m_pIDeviceFrame!=null)
                {
                    double screen_width=m_pIDeviceFrame.Width;
                    double map_width = ToMapSizeX(screen_width);
                    if (ISZERO(screen_width) || ISZERO(map_width))
                        return 0;
                    var units=Units;
                    map_width=UnitsUtil.ConvertUnits(units,OkUnits.okInches, map_width);
                    map_width *= m_dResolution;
                    Scale = map_width / screen_width;
                }
                return Scale;
            }
            set
            {
                if (ZoomResolution == true)
                    return ;
                var Scale = value;
                if (m_pIFittedBounds!=null && m_pIDeviceFrame!=null)
                {
                    var units=Units;
                    var mapUnitsPerPixel = Scale;
                    mapUnitsPerPixel /= m_dResolution;
                    mapUnitsPerPixel=UnitsUtil.ConvertUnits(OkUnits.okInches, units, mapUnitsPerPixel);

                    var c = m_pIFittedBounds.Centre;
                    double center_x=c.X, center_y=c.Y;

                    double pixel_width = m_pIDeviceFrame.Width;
                    double pixel_height= m_pIDeviceFrame.Height;

                    double width = pixel_width * mapUnitsPerPixel;
                    double height = pixel_height * mapUnitsPerPixel;

                    var pIVisibleBounds = new OkEnvelope(new Envelope(center_x - width / 2, center_x + width / 2, center_y - height / 2, center_y + height / 2));
                    VisibleBounds=pIVisibleBounds;
                }
            }
        }

        public double ReferenceScale {
            get
            {
                if (ZoomResolution == true)
                {
                    return 0.0;
                }
                return m_dReferenceScale;
            }
            set
            {
                if (ZoomResolution == true)
                    return ;
                m_dReferenceScale = value;
            }
        }

		public bool SuppressEvents { get; set; } = false;
		public bool ZoomResolution { get; set; } = false;
		public float Resolution {
            get
            {
				if (ZoomResolution == false)
                    return m_dResolution;
                else
                {
                    ZoomResolution = false;
                    double scaleRatio=ScaleRatio;
                    var pDpi = m_dResolution / scaleRatio;
                    ZoomResolution = true;
                    return (float)pDpi;
                }
            }
            set
            {
                m_dResolution = value;
                if (SuppressEvents == false)
                    Fire_ResolutionUpdated(this);
            }
        }

        public double Rotation {
            get { return m_dRotation; }
            set
            {
                m_dRotation = value;
                Calc_matrix();
                if (SuppressEvents == false)
                    Fire_RotationUpdated(this);
            }
        }

        public OkUnits Units {
            get { return m_Units; }
            set
            {
                m_Units = value;
                if (SuppressEvents == false)
                    Fire_UnitsUpdated(this);
            }
        }

		public SpatialReference SpatialReference { get; set; }
		#endregion


		#region yxm extends
		/// <summary>
		/// 返回一个新的对象
		/// </summary>
		/// <param name="g"></param>
		/// <returns></returns>
		public IGeometry ToDevice(IGeometry g)
        {
            if (g == null || m_trans == null)
            {
                return null;
            }
            var sr = g.GetSpatialReference();
            g.Project(this.SpatialReference);
            var g1 = m_trans.Transform(g);
            g.Project(sr);
            return g1;
        }
        /// <summary>
        /// 返回新的几何对象
        /// </summary>
        /// <param name="g">屏幕坐标</param>
        /// <returns></returns>
        public IGeometry ToMap(IGeometry g)
        {
            if (m_inv_trans == null || g == null)
            {
                return null;
            }
            g = m_inv_trans.Transform(g);
            g.SetSpatialReference(this.SpatialReference);
            return g;
        }
        /// <summary>
        /// Applies this transformation to the <paramref name="src" /> coordinate
        /// and places the results in the <paramref name="dest" /> coordinate
        /// (which may be the same as the source).
        /// </summary>
        /// <param name="src"> the coordinate to transform</param>
        /// <param name="dest"> the coordinate to accept the results</param>
        /// <returns> the <code>dest</code> coordinate</returns>
        ///
        public Coordinate ToMap(Coordinate src, Coordinate dest)
        {
            return m_inv_trans?.Transform(src, dest);
        }
        /// <summary>
        /// Applies this transformation to the <paramref name="src" /> coordinate
        /// and places the results in the <paramref name="dest" /> coordinate
        /// (which may be the same as the source).
        /// </summary>
        /// <param name="src"> the coordinate to transform</param>
        /// <param name="dest"> the coordinate to accept the results</param>
        /// <returns> the <code>dest</code> coordinate</returns>
        ///
        public Coordinate ToDevice(Coordinate src, Coordinate dest)
        {
            return m_trans.Transform(src, dest);
        }
		/// <summary>
		/// 返回一个新的对象
		/// </summary>
		/// <param name="env"></param>
		/// <returns></returns>
        public OkEnvelope ToMap(OkEnvelope env)
        {
            var pgn=env.ToPolygon();
            pgn = ToMap(pgn) as IPolygon;
			var env1 = new OkEnvelope(pgn)
			{
				SpatialReference = this.SpatialReference
			};
			return env1;
        }
		public OkEnvelope ToDevice(OkEnvelope env)
		{
			var pgn = env.ToPolygon();
			pgn = ToDevice(pgn) as IPolygon;
			var env1 = new OkEnvelope(pgn)
			{
				SpatialReference = this.SpatialReference
			};
			return env1;
		}
		public double ToMapSizeX(double screenSize)
        {
            var c0 = new Coordinate();
            var c1 = new Coordinate(screenSize, screenSize);
            ToMap(c0, c0);
            ToMap(c1, c1);
            return c1.X - c0.X;
        }
        public double ToMapSizeY(double screenSize)
        {
            var c0 = new Coordinate();
            var c1 = new Coordinate(screenSize, screenSize);
            ToMap(c0, c0);
            ToMap(c1, c1);
            return c1.Y - c0.Y;
        }
        public IPolygon GetViewPolygon()
        {
            var env=DeviceFrameF;
            if (env != null)
            {
                return GeometryUtil.MakePolygon(env);
            }
            return null;
        }

        public double[] MatrixEntries
        {
            get
            {
                return m_trans.MatrixEntries;
            }
        }

        public AffineTransformation GetRawTransformation()
        {
            return m_trans;
        }
        #endregion

        private void Calc_matrix()
        {
            if (m_pIVisibleBounds == null || m_pIDeviceFrame == null)
                return;

            if (!Calc_fitted_bounds())
                return;

            double width_map = m_pIFittedBounds.Width;
            double width_screen = m_pIDeviceFrame.Width;
            double scale = width_screen / width_map;

            var c = m_pIFittedBounds.Centre;
            double map_centerx=c.X, map_centery=c.Y;
            c = m_pIDeviceFrame.Centre;
            double screen_centerx=c.X, screen_centery=c.Y;

            m_trans.SetToIdentity();
            m_trans.Translate(-map_centerx, -map_centery);
            m_trans.Scale(scale, -scale);
            m_trans.Rotate(m_dRotation * 3.1415926 / 180.0);
            m_trans.Translate(screen_centerx, screen_centery);
            m_inv_trans = m_trans.GetInverse();
        }
        bool Calc_fitted_bounds()
        {
            if (m_pIVisibleBounds == null)
                return false;
            if (m_pIFittedBounds == null)
            {
                m_pIFittedBounds = new OkEnvelope();
            }

            double width_map = m_pIVisibleBounds.Width;
            double height_map= m_pIVisibleBounds.Height;
            double width_screen = m_pIDeviceFrame.Width;
            double height_screen= m_pIDeviceFrame.Height;

            if (ISZERO(width_map) || ISZERO(height_map) || ISZERO(width_screen) || ISZERO(height_screen))
                return false;

            double scale_map = width_map / height_map;
            double scale_screen = width_screen / height_screen;

            if (scale_map > scale_screen)
                height_map = width_map * (height_screen / width_screen);
            else if (scale_map < scale_screen)
                width_map = height_map * (width_screen / height_screen);

            var c=m_pIVisibleBounds.Centre;
            double center_x=c.X, center_y=c.Y;
            m_pIFittedBounds.Init(center_x - width_map / 2, center_x + width_map / 2, center_y - height_map / 2,  center_y + height_map / 2);
            m_pIFittedBounds.SpatialReference = m_pIVisibleBounds.SpatialReference;
			m_pIFittedBounds=new OkEnvelope(m_pIFittedBounds.Project(this.SpatialReference));
            return true;
        }
        private static bool ISZERO(double x)
        {
            return ((x > 0 ? x : -x) < 9.9999999999999998E-13) ? true : false;
        }
        //private static void AssignEnvelope(Envelope src, Rectangle dest)
        //{
        //    dest.X = (int)src.MinX;
        //    dest.Width = (int)src.Width;
        //    dest.Y = (int)src.MinY;
        //    dest.Height = (int)src.Height;
        //}
        private static Rectangle ToRectangle(Envelope src)
        {
            return new Rectangle((int)src.MinX, (int)src.MinY, (int)src.Width, (int)src.Height);
        }
        private void Fire_BoundsUpdated(IDisplayTransformation trans)
        {

        }
        private void Fire_VisibleBoundsUpdated(IDisplayTransformation trans,bool sizeChanged)
        {

        }
        private void Fire_DeviceFrameUpdated(IDisplayTransformation trans,bool sizeChanged)
        {

        }
        private void Fire_ResolutionUpdated(IDisplayTransformation trans)
        {

        }
        private void Fire_RotationUpdated(IDisplayTransformation trans)
        {

        }
        private void Fire_UnitsUpdated(IDisplayTransformation trans)
        {

        }
    }
}
