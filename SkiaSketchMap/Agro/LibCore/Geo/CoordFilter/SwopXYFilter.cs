using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore.GIS
{
    public class SwopXYFilter : ICoordinateFilter
    {
        public void Filter(Coordinate coord)
        {
            (coord.X, coord.Y) = (coord.Y, coord.X);
        }
        public void Apply(IGeometry g)
        {
            g.Apply(this);
            g.GeometryChanged();
        }
    }
}