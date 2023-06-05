using Agro.LibCore;
using Agro.LibCore.CRS;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
//using NetTopologySuite.Geometries;
using ProjNet;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Collections.Generic;

namespace Agro.LibCore
{
    public class SpatialReferenceFactory
    {
		static readonly Dictionary<int, string> crsDefinition = CrsDefinition();
		static readonly CoordinateSystemServices _coordinateSystemServices= new(
			new CoordinateSystemFactory(),
			new CoordinateTransformationFactory(),
			crsDefinition);
		/// <summary>
		/// [fromSRID-toSrid,MathTransformFilter]
		/// </summary>
		static readonly Dictionary<string, MathTransformFilter?> _cachePrjFilter = new();
		static readonly Dictionary<int, SpatialReference> _cacheSpatialReference = new();

		public static MathTransformFilter? CreateProjectFilter(int fromSRID, int toSRID)
		{
			if (fromSRID == 0 || toSRID == 0) return null;
			var key = $"{fromSRID}-{toSRID}";
			if (!_cachePrjFilter.TryGetValue(key, out var filter))
			{
				filter = null;
				var transformation = _coordinateSystemServices.CreateTransformation(fromSRID, toSRID);
				if (transformation != null)
				{
					filter = new MathTransformFilter(transformation.MathTransform);
				}
				_cachePrjFilter[key] = filter;
			}
			return filter;
		}
		//public static int ParseSridFromEsriString(string esriStr)
		//{
		//	return 0;
		//}
		public static IGeometry ProjectTo(IGeometry geometry, int srid)
		{
			if (geometry.SRID == srid || srid == 0) return geometry;
			if (geometry.SRID == 0)
			{
				geometry.SRID = srid;
				return geometry;
			}
			var filter = CreateProjectFilter(geometry.SRID, srid);
			if (filter != null)
			{
				//var transformation = _coordinateSystemServices.CreateTransformation(geometry.SRID, srid);
				//var result = geometry.Copy();
				geometry.Apply(filter);// new MathTransformFilter(transformation.MathTransform));
				geometry.GeometryChanged();
			}
			return geometry;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="c"></param>
		/// <param name="src"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Coordinate Project(Coordinate c, ISpatialReference src, ISpatialReference target)
		{
			var mtf = CreateProjectFilter(src.SRID, target.SRID);
			mtf?.Transform(c, c);
			return c;
		}
		public static SpatialReference GetSpatialReference(int srid)
		{
			if (!_cacheSpatialReference.TryGetValue(srid, out var sr))
			{
				sr=new SpatialReference(srid);
				_cacheSpatialReference[srid] = sr;
			}
			return sr;
		}
        public static SpatialReference? CreateFromAuthority(string authority)
		{
			var prefix = "EPSG:";
			if (authority.StartsWith(prefix))
			{
				var srid=authority.Substring(prefix.Length);
				var sr= GetSpatialReference(SafeConvertAux.ToInt32(srid));
				return sr;
			}
            //System.Diagnostics.Debug.Assert(false);
			return null;
		}

        static Dictionary<int, string> CrsDefinition()
		{
			var dic = new AuthorityCodeHandler().ReadDefault();
			//foreach(var kv in dic)
			//{
			//	if (kv.Key == 4532)
			//	{
			//		Console.WriteLine(kv.Value);	
			//	}
			//}
			return dic;
			/*
            return new Dictionary<int, string>
			{
				// Coordinate systems:

				[4326] = GeographicCoordinateSystem.WGS84.WKT,
				[4490] = @"GEOGCS[""China Geodetic Coordinate System 2000"",DATUM[""China_2000"",SPHEROID[""CGCS2000"",6378137,298.257222101,AUTHORITY[""EPSG"",""1024""]],AUTHORITY[""EPSG"",""1043""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4490""]]",

				// This coordinate system covers the area of our data.
				// Different data requires a different coordinate system.
				[2855] =
				@"
                    PROJCS[""NAD83(HARN) / Washington North"",
                        GEOGCS[""NAD83(HARN)"",
                            DATUM[""NAD83_High_Accuracy_Regional_Network"",
                                SPHEROID[""GRS 1980"",6378137,298.257222101,
                                    AUTHORITY[""EPSG"",""7019""]],
                                AUTHORITY[""EPSG"",""6152""]],
                            PRIMEM[""Greenwich"",0,
                                AUTHORITY[""EPSG"",""8901""]],
                            UNIT[""degree"",0.01745329251994328,
                                AUTHORITY[""EPSG"",""9122""]],
                            AUTHORITY[""EPSG"",""4152""]],
                        PROJECTION[""Lambert_Conformal_Conic_2SP""],
                        PARAMETER[""standard_parallel_1"",48.73333333333333],
                        PARAMETER[""standard_parallel_2"",47.5],
                        PARAMETER[""latitude_of_origin"",47],
                        PARAMETER[""central_meridian"",-120.8333333333333],
                        PARAMETER[""false_easting"",500000],
                        PARAMETER[""false_northing"",0],
                        UNIT[""metre"",1,
                            AUTHORITY[""EPSG"",""9001""]],
                        AUTHORITY[""EPSG"",""2855""]]
                ",
				[4532]= @"PROJCS[""CGCS2000 / 3-degree Gauss-Kruger zone 44"",GEOGCS[""China Geodetic Coordinate System 2000"",DATUM[""China_2000"",SPHEROID[""CGCS2000"",6378137,298.257222101,AUTHORITY[""EPSG"",""1024""]],AUTHORITY[""EPSG"",""1043""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4490""]],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",132],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",44500000],PARAMETER[""false_northing"",0],AUTHORITY[""EPSG"",""4532""],AXIS[""X"",NORTH],AXIS[""Y"",EAST]]",
				[3857]=@"+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs"
			};*/
		}


	}

	public class MathTransformFilter : ICoordinateSequenceFilter
	{
		readonly IMathTransform _transform;

		public MathTransformFilter(IMathTransform transform)=> _transform = transform;

		public bool Done => false;
		public bool GeometryChanged => true;

		public void Filter(ICoordinateSequence seq, int i)
		{
			var result = _transform.Transform(
				new[]
				{
					seq.GetOrdinate(i, Ordinate.X),
					seq.GetOrdinate(i, Ordinate.Y)
				});
			seq.SetOrdinate(i, Ordinate.X, result[0]);
			seq.SetOrdinate(i, Ordinate.Y, result[1]);
		}

		public Coordinate Transform(Coordinate src,Coordinate dest)
		{
			var result= _transform.Transform(new[]
				{
					src.X,
					src.Y
				});
			dest.X = result[0];
			dest.Y = result[1];
			return dest;
		}
	}
}