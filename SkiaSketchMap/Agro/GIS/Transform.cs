using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using  System.Drawing;

namespace Agro.GIS
{
    public class Transform
    {
        private IPolygon? _mapExtent;
        private IPolygon? _adjustExtent;// = new Envelope();
        
        private Rectangle? _rcView;
        private IPolygon? _rcViewPolygon;
        private readonly AffineTransformation _tf = new();
        private AffineTransformation? _invertTrans;

        /// <summary>
        /// Resolution of the device in dots (pixels) per inch
        /// </summary>
        public float Resolution { get; set; } = 96;

        /// <summary>
        /// 是地图坐标系的快捷方式
        /// </summary>
        public SpatialReference? SpatialReference
        {
            get
            {
                return _mapExtent?.GetSpatialReference();
            }
        }

        public Transform()
        {
            //using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            //Resolution = g.DpiX;
        }

        public void SetMapExtent(IPolygon mapBounds)
        {
            _mapExtent = mapBounds;
            Calc();
        }
        public void SetDeviceFrame(Rectangle lprc)
        {
            _rcView = lprc;
            ResetDeviceFramePolygon();
            Calc();
        }
        public Rectangle? GetViewRect()
        {
            return _rcView;
        }
        /// <summary>
        /// 返回屏幕坐标
        /// </summary>
        /// <returns></returns>
        public IPolygon? GetViewPolygon()
        {
            return _rcViewPolygon;
        }
        public IPolygon? GetAdjustMapExtent()
        {
            return _adjustExtent;
        }
        public void Init(IPolygon mapBounds, Rectangle lprc)
        {
            _mapExtent = mapBounds;
            _rcView = lprc;
            ResetDeviceFramePolygon();
            Calc();
        }
        public double Scale
        {
            get{
                return _tf.MatrixEntries[0];
            }
            
        }

        /// <summary>
        /// 返回一个新的对象
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public IGeometry? ToDevice(IGeometry? g)
        {
            if (g == null || _tf == null)
            {
                return null;
            }
            var sr = g.GetSpatialReference();
            g.Project(this.SpatialReference);
            var g1= _tf.Transform(g);
            g.Project(sr);
            return g1;
        }
        /// <summary>
        /// 返回新的几何对象
        /// </summary>
        /// <param name="g">屏幕坐标</param>
        /// <returns></returns>
        public IGeometry? ToMap(IGeometry? g)
        {
            if (_invertTrans == null||g==null)
            {
                return null;
            }
            g= _invertTrans.Transform(g);
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
            return _invertTrans?.Transform(src, dest)??dest;
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
            return _tf.Transform(src, dest);
        }
        public double ToMapSizeX(double screenSize)
        {
            var c0 = new Coordinate();
            var c1 = new Coordinate(screenSize, screenSize);
            ToMap(c0, c0);
            ToMap(c1, c1);
            return c1.X-c0.X;
        }
        public double ToMapSizeY(double screenSize)
        {
            var c0 = new Coordinate();
            var c1 = new Coordinate(screenSize, screenSize);
            ToMap(c0, c0);
            ToMap(c1, c1);
            return c1.Y - c0.Y;
        }

        public double[] MatrixEntries
        {
            get
            {
                return _tf.MatrixEntries;
            }
        }
        internal AffineTransformation GetRawTransformation()
        {
            return _tf;
        }
        private void Calc()
        {
            if (_mapExtent == null || _rcView == null)
            {
                return;
            }
            var rc = (Rectangle)_rcView;
            if (rc.Width == 0 || rc.Height == 0)
                return;

            var mapBounds = _mapExtent!.EnvelopeInternal;
            if (mapBounds.Area == 0)
            {
                var dlt = 0.01;
                mapBounds = new Envelope(mapBounds.MinX - dlt, mapBounds.MaxX + dlt, mapBounds.MinY - dlt, mapBounds.MaxY + dlt);
            }

            double scalex = rc.Width / mapBounds.Width;
            double scaley = rc.Height / mapBounds.Height;
            double scale = scalex < scaley ? scalex : scaley;
            //Log.WriteLine("pre " + _tf);
            _tf.SetToIdentity();
            _tf.Scale(scale, -scale);
            double dx=(rc.X+rc.Width/2-(mapBounds.MinX+mapBounds.MaxX)*scale/2);
            double dy=((mapBounds.MinY+mapBounds.MaxY)*scale/2+rc.Y+rc.Height/2);
            _tf.Translate(dx, dy);
            _invertTrans=_tf.GetInverse();

            
            var c0 = new Coordinate(rc.Left, rc.Top);
            var c1 = new Coordinate(rc.Right, rc.Bottom);
            ToMap(c0, c0);
            ToMap(c1, c1);

            _adjustExtent = GeometryUtil.MakePolygon( c0.X,c0.Y,c1.X,c1.Y);
            _adjustExtent.SetSpatialReference(_mapExtent.GetSpatialReference());

        }

        private void ResetDeviceFramePolygon()
        {
            if(_rcView==null)return;
            var lprc = (Rectangle)_rcView;
            var coords = new Coordinate[5];
            coords[0] = new Coordinate(lprc.Left, lprc.Top);
            coords[1] = new Coordinate(lprc.Right, lprc.Top);
            coords[2] = new Coordinate(lprc.Right, lprc.Bottom);
            coords[3] = new Coordinate(lprc.Left, lprc.Bottom);
            coords[4] = coords[0].Copy();
            _rcViewPolygon = GeometryUtil.MakePolygon(coords);
        }
    }
}
