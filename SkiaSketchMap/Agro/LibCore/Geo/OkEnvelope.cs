using GeoAPI.Geometries;

namespace Agro.LibCore
{
    public class OkEnvelope : Envelope
    {
        public SpatialReference? SpatialReference
        {
            get; set;
        }
        public OkEnvelope()
        {

        }
        public OkEnvelope(OkEnvelope rhs)
            : base(rhs)
        {
            SpatialReference = rhs.SpatialReference;
        }
        public OkEnvelope(Envelope rhs, SpatialReference? sr = null)
            : base(rhs)
        {
            SpatialReference = sr;
        }
        public OkEnvelope(double x1, double x2, double y1, double y2, SpatialReference sr = null)
            : base(x1, x2, y1, y2)
        {
            SpatialReference = sr;
        }
        public OkEnvelope(IGeometry g)
        {
            Init(g.EnvelopeInternal);
            SpatialReference = g.GetSpatialReference();
        }
        public void Init(OkEnvelope rhs)
        {
            base.Init(rhs);
            SpatialReference = rhs.SpatialReference;
        }
        public IPolygon Project(SpatialReference sr)
        {
            var pgn = GeometryUtil.MakePolygon(this);
            pgn.Project(sr);
            return pgn;
        }

        public void Union(OkEnvelope rhs)
        {
            ExpandToInclude(rhs);
        }


        public new OkEnvelope Clone()
        {
            var rhs = new OkEnvelope(this)
            {
                SpatialReference = SpatialReference
            };
            return rhs;
        }
    }
}
