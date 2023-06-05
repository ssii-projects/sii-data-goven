using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.LibCore
{
	public interface ISpatialReference
	{
		int SRID { get; set; }
		//IGeometry Project(IGeometry g, ISpatialReference dest)
	}
	public class SpatialReference : ISpatialReference
	{
        /// <summary>
        /// like: EPSG:4532
        /// </summary>
        public string? Authority;
        public int SRID { get; set; }
		public SpatialReference(int srid)
		{
			SRID = srid;
		}

		public static IGeometry Project(IGeometry g, ISpatialReference? dest)
		{
			return dest==null?g:SpatialReferenceFactory.ProjectTo(g, dest.SRID);
		}
		public static Coordinate Project(Coordinate c, ISpatialReference src, ISpatialReference dest)
		{
			return SpatialReferenceFactory.Project(c, src, dest);
		}
	}
}
