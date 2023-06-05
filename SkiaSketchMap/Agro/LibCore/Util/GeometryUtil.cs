using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore
{
    public class GeometryUtil
    {
        public static IPolygon MakeSquare(double x, double y,double buffer)
        {
            var MinX = x - buffer;
            var MaxX = x + buffer;
            var MinY = y - buffer;
            var MaxY = y + buffer;
            var coords = new Coordinate[5];
            coords[0] = new Coordinate(MinX, MaxY);
            coords[1] = new Coordinate(MaxX, MaxY);
            coords[2] = new Coordinate(MaxX, MinY);
            coords[3] = new Coordinate(MinX, MinY);
            coords[4] = coords[0].Copy();//.Clone() as Coordinate;
            return MakePolygon(coords);
        }


        public static string? CanMakeGeometry(IEnumerable<Coordinate> coords, eGeometryType geometryType)
        {
            string? err = null;
            var cnt = coords.Count();
            switch (geometryType)
            {
                case eGeometryType.eGeometryPoint:
                    if (cnt > 1)
                    {
                        err = "点要素只能一个坐标值";
                        break;
                    }
                    break;
                case eGeometryType.eGeometryPolyline:
                    if (cnt < 2)
                    {
                        err = "线要素至少需要包含两个点坐标";
                        break;
                    }
                    break;
                case eGeometryType.eGeometryPolygon:
                    if (cnt < 3)
                    {
                        err = "面要素至少需要包含三个点坐标";
                        break;
                    }
                    break;
            }
            return err;
        }

        public static IPoint MakePoint(Coordinate c, SpatialReference? sr = null)
        {
            var p= new Point(c);
            p.SetSpatialReference(sr);
            return p;
        }
        public static IPoint MakePoint(double x,double y,SpatialReference? sr=null)
        {
            var g= new Point(x,y);
            g.SetSpatialReference(sr);
            return g;
        }
        public static IMultiPoint MakeMultiPoint(Coordinate[] coords, SpatialReference? sr = null)
        {
            var pts=new IPoint[coords.Length];
            for (int i = 0; i < pts.Length; ++i)
            {
                pts[i] = MakePoint(coords[i], sr);
            }
           var g= new MultiPoint(pts);
           g.SetSpatialReference(sr);
           return g;
        }
        public static ILinearRing MakeLinearRing(Coordinate[] coords, SpatialReference? sr = null)
        {
            var r = new LinearRing(coords);
            r.SetSpatialReference(sr);
            return r;
        }
        public static ILineString? MakeLineString(Coordinate[] coords, SpatialReference? sr = null)
        {
            if (coords == null || coords.Length == 1)
            {
                return null;
            }
            var g= new LineString(coords);
            g.SetSpatialReference(sr);
            return g;
        }

        public static ILineString MakeLineString(Envelope env, SpatialReference? sr = null)
        {
            var coords = new Coordinate[5];
            coords[0] = new Coordinate(env.MinX, env.MaxY);
            coords[1] = new Coordinate(env.MaxX, env.MaxY);
            coords[2] = new Coordinate(env.MaxX, env.MinY);
            coords[3] = new Coordinate(env.MinX, env.MinY);
            coords[4] = coords[0].Copy();
            return MakeLineString(coords, sr)!;
        }
        public static ILineString MakeLineString(OkEnvelope env)
        {
            return MakeLineString((Envelope)env, env.SpatialReference);
        }

		public static ILineString MakeLineString(double x1, double y1, double x2, double y2,SpatialReference? sr=null)
		{
			var coords = new Coordinate[]{
				new Coordinate(x1,y1),
				new Coordinate(x2,y2)
			};
			return MakeLineString(coords, sr)!;
		}
		public static ILineString MakeLineString(Coordinate c0,Coordinate c1, SpatialReference? sr = null)
		{
			var coords = new Coordinate[]{c0,c1};
			return MakeLineString(coords, sr)!;
		}

		public static IMultiLineString MakeMultiLineString(ILineString[] lst, SpatialReference? sr = null)
        {
            var g = new MultiLineString(lst);
            g.SetSpatialReference(sr);
            return g;
        }
        public static IMultiPolygon MakeMultiPolygon(IPolygon[] lst, SpatialReference? sr = null)
        {
            var g = new MultiPolygon(lst);
            g.SetSpatialReference(sr);
            return g;
        }
        public static IPolygon MakePolygon(Coordinate[] coords, SpatialReference? sr = null)
        {
            var g= new Polygon(MakeLinearRing(coords));
            g.SetSpatialReference(sr);
            return g;
        }
        public static IPolygon MakePolygon(ILinearRing shell, ILinearRing[]? holes, SpatialReference? sr = null)
        {
            var g= new Polygon(shell,holes);
            g.SetSpatialReference(sr);
            return g;
        }
        public static IPolygon MakePolygon(OkEnvelope env)
        {
            return MakePolygon((Envelope)env, env.SpatialReference);
        }
        public static IPolygon MakePolygon(Envelope env,SpatialReference? sr=null)
        {
            var coords = new Coordinate[5];
            coords[0] = new Coordinate(env.MinX, env.MaxY);
            coords[1] = new Coordinate(env.MaxX, env.MaxY);
            coords[2] = new Coordinate(env.MaxX, env.MinY);
            coords[3] = new Coordinate(env.MinX, env.MinY);
            coords[4] = coords[0].Copy();
            return MakePolygon(coords,sr);
        }
        public static IPolygon MakePolygon(double x1, double y1, double x2, double y2)
        {
            var MinX = Math.Min(x1, x2);
            var MaxX = Math.Max(x1, x2);
            var MinY = Math.Min(y1, y2);
            var MaxY = Math.Max(y1, y2);
            var coords = new Coordinate[5];
            coords[0] = new Coordinate(MinX, MaxY);
            coords[1] = new Coordinate(MaxX, MaxY);
            coords[2] = new Coordinate(MaxX, MinY);
            coords[3] = new Coordinate(MinX, MinY);
            coords[4] = coords[0].Copy();
            return MakePolygon(coords);
        }

        public static Envelope? ScaleAt(Envelope? env, double scale, double x, double y)
        {
            if (env == null)
                return null;
            double xmin = env.MinX;
            double ymin = env.MinY;
            double xmax = env.MaxX;
            double ymax = env.MaxY;
            xmin = (xmin - x) * scale + x;
			ymin = (ymin - y)*scale + y;
			xmax = (xmax - x)*scale +x;
			ymax = (ymax - y)*scale +y;
            return new Envelope(xmin, xmax, ymin, ymax);
        }


        /// <summary>
        /// 判断geometry是否在范围内（包括在边界上）
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static bool InEnvelope(IGeometry geometry,Envelope env)
        {
            var geoExtend = geometry.EnvelopeInternal;
            if (geoExtend.MaxX >= env.MinX && geoExtend.MaxX <= env.MaxX &&
                geoExtend.MinX >= env.MinX && geoExtend.MinX <= env.MaxX &&
                geoExtend.MaxY >= env.MinY && geoExtend.MaxY <= env.MaxY &&
                geoExtend.MinY >= env.MinY && geoExtend.MinY <= env.MaxY)
                return true;

            return false;// geometry.Intersection(clip);
        }

        public static string GeometryTypeToString(eGeometryType gt)
        {
            return gt switch
            {
                eGeometryType.eGeometryNull => "未知",
                eGeometryType.eGeometryMultipoint => "多点",
                eGeometryType.eGeometryPoint => "点",
                eGeometryType.eGeometryPolyline => "线",
                eGeometryType.eGeometryPolygon => "面",
                _ => gt.ToString(),
            };
        }

        /// <summary>
        /// 返回Polygon或MultiPolygon
        /// </summary>
        /// <param name="geo">IPolygon或IMultiPolygon</param>
        /// <param name="MinArea"></param>
        /// <returns></returns>
        public static IGeometry? MakePolygonValid(IGeometry geo,double MinArea)
        {
            if (geo.Area < MinArea)
            {
                return null;
            }
			if (!(geo is IGeometryCollection igc))
			{
				if (geo is IPolygon pgn)
				{
					return RemoveInvalidHoles(pgn, MinArea);
				}
				return geo;
				//GeometryUtil.MakeMultiPolygon()
			}
			var lst = new List<IPolygon>();
            for (int i = 0; i < igc.Count; ++i)
            {
                var g = igc.GetGeometryN(i);
                if ((g is IPolygon pgn) && !g.IsEmpty)//&&g.Area> MinArea)
                {
                    g = RemoveInvalidHoles(pgn, MinArea);
                    if (g != null && g.Area > MinArea)
                    {
                        lst.Add((IPolygon)g);
                    }
                }
            }
            if (lst.Count == 0)
            {
                return null;
            }
            else if (lst.Count == 1)
            {
                return lst[0];
            }

            var mp = GeometryUtil.MakeMultiPolygon(lst.ToArray());
            return mp;
        }
        /// <summary>
        /// 移除面积<MinArea的孔
        /// </summary>
        /// <param name="pgn"></param>
        /// <param name="MinArea"></param>
        /// <returns></returns>
        public static IPolygon RemoveInvalidHoles(IPolygon pgn, double MinArea)
        {
            if (pgn.Holes.Count() > 0)
            {
                var lst = new List<ILinearRing>();
                foreach (var h in pgn.Holes)
                {
                    var g = GeometryUtil.MakePolygon(h, null);
                    if (g.Area > MinArea)
                    {
                        lst.Add(h);
                    }
                }
                if (lst.Count == 0)
                {
                    return GeometryUtil.MakePolygon(pgn.Shell, null);
                }
                else
                {
                    return GeometryUtil.MakePolygon(pgn.Shell, lst.ToArray());
                }
            }
            return pgn;
        }

        /// <summary>
        /// 从集合g中移除非面要素部分，返回null、IPolygon或IMultiPolygon
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static IGeometry? RemoveNonPolygonParts(GeometryCollection g,double minArea)
        {
            var lst = new List<IPolygon>();
            foreach (var g0 in g.Geometries)
            {
                if (g0.Area > minArea)
                {
                    if (g0 is IPolygon pgn)
                    {
                        lst.Add(pgn);
                    }
                    else if (g0 is IMultiPolygon mp)
                    {
                        foreach (var g1 in mp.Geometries)
                        {
                            lst.Add((IPolygon)g1);
                        }
                    }
                }
            }
            if (lst.Count == 0)
            {
                return null;
            }
            if (lst.Count == 1)
            {
                return lst[0];
            }
            return MakeMultiPolygon(lst.ToArray());
        }

        /// <summary>
        /// 使得面要素的外环为顺时针，内环为逆时针方向（保持ArcGIS的方式）
        /// </summary>
        /// <param name="pgn"></param>
        /// <returns></returns>
        public static IPolygon MakeCW(IPolygon pgn)
        {
            bool fOK = true;
            if (Orientation.IsCCW(pgn.Shell.Coordinates))
            {
                fOK = false;
            }
            if (fOK)
            {
                foreach(var h in pgn.Holes)
                {
                    if (!Orientation.IsCCW(h.Coordinates))
                    {
                        fOK = false;
                        break;
                    }
                }
            }
            if (!fOK)
            {
                return (IPolygon)pgn.Reverse();
            }
            return pgn;
        }

        /// <summary>
        /// 查找狭长角
        /// </summary>
        /// <param name="g"></param>
        /// <param name="minAngle"></param>
        /// <param name="callback">中间那个点为三角形的定点</param>
        public static void FindSmallAngle(IGeometry g,double minAngle,Func<Coordinate,Coordinate,Coordinate,double,bool> callback)
        {
            if (g is IPolygon pgn)
            {
                FindSmallAngle(pgn, minAngle,callback);
            }else
            {
                System.Diagnostics.Debug.Assert(false, "todo...");
            }

        }
		public static bool IsSame(double d1, double d2, int scale)
		{
			int n1 = (int)d1;
			int n2 = (int)d2;
			if (n1 != n2) return false;
			n1 =(int)( (d1 - n1) * scale);
			n2 = (int)((d2 - n2) * scale);
			return n1 == n2;
		}
		public static bool IsSamePoint(Coordinate c1, Coordinate c2,int scale)
		{
			return IsSame(c1.X, c2.X,scale) && IsSame(c1.Y, c2.Y,scale);
		}
        public static void FindSmallAngle(IPolygon g, double minAngle, Func<Coordinate, Coordinate, Coordinate,double, bool> callback)
        {
            bool fContinue=FindSmallAngle(g.Shell.Coordinates, minAngle,callback);
            if (!fContinue)
            {
                return;
            }
            foreach (var h in g.Holes)
            {
                 fContinue= FindSmallAngle(h.Coordinates, minAngle,callback);
                if (!fContinue)
                {
                    return;
                }
            }
        }
        private static bool FindSmallAngle(Coordinate[] cds,double minAngle, Func<Coordinate, Coordinate, Coordinate,double, bool> callback)
        {
            var lst = new List<Coordinate>();
            Coordinate? preCoord = null;
            for (int i = 0; i < cds.Length - 1; ++i)
            {
                var p = cds[i];
				if (preCoord != null && IsSamePoint(p, preCoord, 10000))
				{
					continue;
				}
				lst.Add(p);
				preCoord = p;
			}
			for (int i = 0; i < lst.Count;)
            {
                var p0 = i == 0 ? lst[lst.Count - 1] : lst[i - 1];
                var p = lst[i];
                var p1 = i == lst.Count - 1 ? lst[0] : lst[i + 1];
                var d = Cgl.CalcAngle(p.X, p.Y, p0.X, p0.Y, p1.X, p1.Y);
                if (d < minAngle)
                {
                    bool fContinue=callback(p0, p, p1,d);
                    if (!fContinue)
                    {
                        return false;
                    }
                }
                ++i;
            }
            return true;
        }
        /// <summary>
        /// 移除狭长角
        /// </summary>
        /// <param name="g"></param>
        /// <param name="minAngle"></param>
        /// <returns></returns>
        public static IGeometry RemoveZeroAngle(IGeometry g, double minAngle = 1)
        {
            if (g is IPolygon pgn)
            {
                return RemoveZeroAngle(pgn, minAngle);
            }

            return g;
        }

        /// <summary>
        /// 移除狭长角
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static IPolygon RemoveZeroAngle(IPolygon g, double minAngle = 0.1)
        {
            var cds = RemoveZeroAngle(g.Shell.Coordinates, minAngle);
            if (g.Holes.Count() == 0)
            {
                var g1 = GeometryUtil.MakePolygon(cds);
                return g1;
            }
            var lst = new List<ILinearRing>();
            foreach (var h in g.Holes)
            {
                var h0 = RemoveZeroAngle(h.Coordinates, minAngle);
                lst.Add(GeometryUtil.MakeLinearRing(h0));
            }
            var shell = GeometryUtil.MakeLinearRing(cds);
            var g2 = GeometryUtil.MakePolygon(shell, lst.ToArray());
            return g2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cds">首位封闭</param>
        /// <param name="minAngle"></param>
        /// <returns></returns>
        private static Coordinate[] RemoveZeroAngle(Coordinate[] cds, double minAngle = 0.1)
        {
            var lst = new List<Coordinate>();
            Coordinate? preCoord = null;
            for(int i = 0; i < cds.Length - 1; ++i)
            {
                var p = cds[i];
                if (preCoord == null)
                {
                    lst.Add(p);
                }else
                {
                    if (p.X == preCoord.X && p.Y == preCoord.Y)
                    {
                        continue;
                    }
                    lst.Add(p);
                    preCoord = p;
                }
            }
            for (int i = 0; i < lst.Count;)
            {
                var p0 = i == 0 ? lst[lst.Count - 1] : lst[i - 1];
                var p = lst[i];
                var p1 = i == lst.Count - 1 ? lst[0] : lst[i + 1];
                var d = Cgl.CalcAngle(p.X, p.Y, p0.X, p0.Y, p1.X, p1.Y);
                if (d < minAngle)
                {
                    lst.RemoveAt(i);
                    if (i > 0)
                    {
                        --i;
                    }
                }
                else
                {
                    ++i;
                }
            }
            if (lst.Count > 0)
            {
                lst.Add(lst[0].Copy());
            }
            if (lst.Count >= 4)
            {
                return lst.ToArray();
            }
            else
            {
                Console.WriteLine("RemoveZeroAngle:lst.Count=" + lst.Count);
            }
            return cds;
        }


        public static void EnumPolygon(IGeometry g,Action<IPolygon> callback)
        {
            if(g is IPolygon pgn)
            {
                callback(pgn);
            }else
            {
				if (g is IGeometryCollection gc)
				{
					foreach (var g0 in gc.Geometries)
					{
						if (g0 is IPolygon p)
						{
							callback(p);
						}
					}
				}
			}
        }
        public static IGeometry RemoveRepeatCoords(IGeometry g, double distanceTolerance)
        {
            return TopologyPreservingSimplifier.Simplify(g, distanceTolerance);
        }
        public static IGeometry RemoveRepeatCoords(IGeometry g,Func<Coordinate,Coordinate,bool> predicate)
        {
            if (g is IPolygon pgn)
            {
                return RemoveRepeatCoords(pgn, predicate);
            }
            else if (g is IMultiPolygon mpgn)
            {
                var lst = new IPolygon[mpgn.NumGeometries];
                for(var i=0;i<lst.Length;++i)
                {
                    lst[i] = (IPolygon)RemoveRepeatCoords(mpgn.GetGeometryN(i), predicate);
                }
                return MakeMultiPolygon(lst, g.GetSpatialReference());
            }
            return g;
        }
        private static IPolygon RemoveRepeatCoords(IPolygon pgn,Func<Coordinate, Coordinate, bool> predicate)
        {
            bool fChanged = false;
            var shell = RemoveRepeatCoords(pgn.Shell,predicate);
            if (shell != pgn.Shell)
            {
                fChanged = true;
            }
            List<ILinearRing>? holes = null;
            if (pgn.Holes.Count() > 0)
            {
                holes = new List<ILinearRing>();
                foreach (var h in pgn.Holes)
                {
                    var h1 = RemoveRepeatCoords(h,predicate);
                    if (h1 != h)
                    {
                        fChanged = true;
                    }
                    holes.Add(h1);
                }
            }
            if (fChanged)
            {
                var p = GeometryUtil.MakePolygon(shell, holes == null ? null : holes.ToArray(),pgn.GetSpatialReference());
                return p;
            }
            return pgn;
        }
        private static ILinearRing RemoveRepeatCoords(ILinearRing lr, Func<Coordinate, Coordinate, bool> predicate)
        {
            var lst = new List<Coordinate>();
            var sa = lr.Coordinates;
            var p = sa[0];
            lst.Add(p);
            for (int i = 1; i < sa.Length - 1; ++i)
            {
                var p1 = sa[i];
                if (!predicate(p, p1))
                {
                    lst.Add(p1);
                    p = p1;
                }
            }
            lst.Add(new Coordinate(lst[0]));
            if (lst.Count >= 4)
            {
                return GeometryUtil.MakeLinearRing(lst.ToArray());
            }
            return lr;
        }

        //public static bool HasSelfIntersect(IGeometry g)
        //{
        //    return Topology.HasSelfIntersect(g.AsBinary());
        //}

		/// <summary>
		/// yxm 2019-4-24
		/// </summary>
		/// <param name="coords"></param>
		public static void Reverse(Coordinate[] coords)
		{
			int k = coords.Length >> 1;
			for (int i = 0; i < k; ++i)
			{
				int j = coords.Length - 1 - i;
                (coords[j], coords[i]) = (coords[i], coords[j]);
            }
        }
		/// <summary>
		/// 是否逆时针排序
		/// yxm 2019-4-24
		/// </summary>
		/// <param name="coords"></param>
		/// <returns></returns>
		public static bool IsCCW(Coordinate[] coords)
		{
			return ComputeArea(coords) > 0 ? true : false;
		}
		/// <summary>
		/// 计算面积
		/// yxm 2019-4-24
		/// </summary>
		/// <param name="coords"></param>
		/// <returns></returns>
		public static double ComputeArea(Coordinate[] coords)
		{
			double area = 0.0f;

			//for (Iterator p = end - 1, q = begin; q < end; p = q++)
			for (int ip = coords.Length - 1, iq = 0; iq < coords.Length; ip = iq++)
			{
				var p = coords[ip];
				var q = coords[iq];
				double a1 = p.X * q.Y;
				double a2 = q.X * p.Y;
				area += a1 - a2;
			}
			area *= 0.5f;
			return area;
		}

        public static eGeometryType ToGeometryType(IGeometry geo)
        {
            var geoType = eGeometryType.eGeometryNull;
            if (geo is IPolygon || geo is IMultiPolygon)
            {
                geoType = eGeometryType.eGeometryPolygon;
            }
            else if (geo is ILineString || geo is IMultiLineString)
            {
                geoType = eGeometryType.eGeometryPolyline;
            }
            else if (geo is IPoint || geo is IMultiPoint)
            {
                geoType = eGeometryType.eGeometryPoint;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "暂不支持的几何类型！");
            }
            return geoType;
        }

		/// <summary>
		/// yxm 2019-1-25
		/// </summary>
		/// <param name="g"></param>
		/// <param name="callback"></param>
		public static void EnumEdge(IGeometry g, Action<Coordinate, Coordinate> callback) {
			if (g is IPolygon pgn)
			{
				EnumEdge(pgn, callback);
			}
			else if (g is IMultiPolygon)
			{
				EnumPolygon(g, g0 => EnumEdge(g0, callback));
			}
			else if (g is ILineString ls)
			{
				EnumEdge(ls, callback);
			}
			else if (g is IMultiLineString mls)
			{
				foreach (var g0 in mls.Geometries)
				{
					if (g0 is ILineString p)
					{
						EnumEdge(p, callback);
					}
				}
			}
		}
		private static void EnumEdge(IPolygon polygon, Action<Coordinate, Coordinate> callback)
		{
			EnumEdge(polygon.Shell, callback);
			foreach (var h in polygon.Holes)
			{
				EnumEdge(h, callback);
			}
		}

		private static void EnumEdge(ILineString ls,Action<Coordinate,Coordinate> callback)
        {
			var lst=ls.Coordinates;
            if (lst != null)
            {
                var cnt = lst.Length;
                for (int i = 1; i < cnt; ++i)
                {
                    callback(lst[i - 1], lst[i]);
                }
            }
        }

		public static string ToGeoJson(IGeometry geometry, Action<Coordinate>? filter=null)
		{
			var tmp = new Coordinate();
			StringBuilder? str = null;
			if (geometry is IPolygon polygon)
			{
				str = new StringBuilder("{\"type\":\"Polygon\",\"coordinates\":");
				str.Append(ToGeoJson(polygon, filter, tmp));
			}
			else if (geometry is IMultiPolygon mp)
			{
				str = new StringBuilder("{\"type\":\"MultiPolygon\",\"coordinates\":");
				str.Append(ToGeoJson(mp, filter, tmp));
			}
			else if (geometry is ILineString ls)
			{
				str = new StringBuilder("{\"type\":\"LineString\",\"coordinates\":");
				str.Append(ToGeoJson(ls, filter, tmp));
			}
			else if (geometry is IMultiLineString mls)
			{
				str = new StringBuilder("{\"type\":\"MultiLineString\",\"coordinates\":");
				str.Append(ToGeoJson(mls, filter, tmp));
			}
			else if (geometry is IPoint pt)
			{
				str = new StringBuilder("{\"type\":\"Point\",\"coordinates\":");
				tmp.X = pt.X;
				tmp.Y = pt.Y;
				filter?.Invoke(tmp);
				str.Append("[" + tmp.X + "," + tmp.Y + "]");
			}
			else if (geometry is IMultiPoint mpt)
			{
				str = new StringBuilder("{\"type\":\"MultiPoint\",\"coordinates\":");
				str.Append("[");
				int i = 0;
				foreach (var c in mpt.Coordinates)
				{
					tmp.SetCoords(c);
					filter?.Invoke(tmp);
					if (i>0)
					{
						str.Append(",");
					}
					str.Append("[" + tmp.X + "," + tmp.Y + "]");
					++i;
				}
				str.Append("]");
			}
			str!.Append("}");
			return str.ToString();
		}
		private static string ToGeoJson(IMultiPolygon mp, Action<Coordinate>? filter, Coordinate tmp)
		{
			string str ="[";
			foreach (var g0 in mp.Geometries)
			{
				if (g0 is IPolygon p)
				{
					if (str.Length > 1)
					{
						str += ",";
					}
					str += ToGeoJson(p, filter, tmp);
				}
			}
			str += "]";
			return str;
		}
		private static string ToGeoJson(IPolygon polygon, Action<Coordinate>? filter, Coordinate tmp)
		{
			string str = "[";
			str += ToGeoJson(polygon.Shell, filter,tmp);
			foreach(var h in polygon.Holes)
			{
				str +=","+ ToGeoJson(h, filter,tmp);
			}
			str += "]";
			return str;
		}
		private static string ToGeoJson(ILineString ls,Action<Coordinate>? filter, Coordinate tmp) {
			var str =new StringBuilder("[");
			var sa = ls.Coordinates;
			for (int i = 0; i < sa.Length; ++i)
			{
				var c = sa[i];
				tmp.SetCoords(c);
				filter?.Invoke(tmp);
				if (i > 0)
				{
					str.Append(",");
				}
				str.Append( "[" + tmp.X + "," + tmp.Y + "]");
			}
			str.Append("]");
			return str.ToString();
		}
		private static string ToGeoJson(IMultiLineString mp, Action<Coordinate>? filter, Coordinate tmp)
		{
			string str = "[";
			foreach (var g0 in mp.Geometries)
			{
				if (g0 is ILineString p)
				{
					if (str.Length > 1)
					{
						str += ",";
					}
					str += ToGeoJson(p, filter, tmp);
				}
			}
			str += "]";
			return str;
		}

       
	}
}
