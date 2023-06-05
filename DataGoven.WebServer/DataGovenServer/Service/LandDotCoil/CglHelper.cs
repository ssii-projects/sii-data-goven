using Agro.LibCore.CglLib;
using Agro.LibMapServer;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGovenServer.Service.LandDotCoil
{
	public class CglHelper
	{
		/// <summary>
		/// π
		/// </summary>
		public const double PI = Math.PI;//3.1415926535897932384626433832795;
										 /// <summary>
										 /// 容差
										 /// </summary>
		public const double intersection_epsilon = 1.0e-30;


		/// <summary>
		/// 线段ls延长distance距离然后作逆时针旋转angle度得到返回点，返回null表示失败（起点和终点相同）
		/// </summary>
		/// <param name="ptFromX"></param>
		/// <param name="ptFromY"></param>
		/// <param name="ptToX"></param>
		/// <param name="ptToY"></param>
		/// <param name="angle"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static Coordinate deflection_distance(Coordinate ptFrom, Coordinate ptTo, double angle, double distance)
		{
			double s1_x = ptTo.X - ptFrom.X;
			double s1_y = ptTo.Y - ptFrom.Y;
			double d = Math.Sqrt(s1_x * s1_x + s1_y * s1_y);
			if (IsZero(d))
			{
				return null;
			}
			else
			{
				angle = angle * PI / 180.0f;//角度转换为弧度
				double r_normal = 1.0f / d;
				s1_x *= r_normal;
				s1_y *= r_normal;
				double tempx = s1_x * Math.Cos(angle) - s1_y * Math.Sin(angle);
				double tempy = s1_x * Math.Sin(angle) + s1_y * Math.Cos(angle);
				s1_x = tempx * distance;
				s1_y = tempy * distance;

				double p3X = ptTo.X + s1_x;
				double p3Y = ptTo.Y + s1_y;
				return new Coordinate(p3X, p3Y);
			}
		}

		/// <summary>
		/// 向两边各缓冲distance距离和得到的Polygon对象
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static IPolygon Buffer(Coordinate ptFrom, Coordinate ptTo, double distance)
		{
			var c0 = deflection_distance(ptFrom, ptTo, 90, distance);
			if (c0 == null)
				return null;
			var coords = new Coordinate[5];
			coords[0] = c0;
			coords[1] = deflection_distance(ptFrom, ptTo, -90, distance);
			coords[2] = deflection_distance(ptTo, ptFrom, 90, distance);
			coords[3] = deflection_distance(ptTo, ptFrom, -90, distance);
			coords[coords.Length - 1] = coords[0];
			return new Polygon(new LinearRing(coords));
		}

		/// <summary>
		/// 向右边缓冲distance距离和得到的Polygon对象
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static IPolygon BufferRight(Coordinate ptFrom, Coordinate ptTo, double distance)
		{
			var c1 = deflection_distance(ptFrom, ptTo, -90, distance);
			var c2 = deflection_distance(ptTo, ptFrom, 90, distance);
			if (c1 == null || c2 == null)
				return null;
			var coords = new Coordinate[5];
			coords[0] = ptTo;// deflection_distance(ptFrom, ptTo, 90, distance);
			coords[1] = c1;
			coords[2] = c2;
			coords[3] = ptFrom;// deflection_distance(ptTo, ptFrom, -90, distance);
			coords[coords.Length - 1] = coords[0];
			return new Polygon(new LinearRing(coords));
		}
		/// <summary>
		/// 向左边缓冲distance距离和得到的Polygon对象
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static IPolygon BufferLeft(Coordinate ptFrom, Coordinate ptTo, double distance)
		{
			var c1 = deflection_distance(ptFrom, ptTo, 90, distance);
			var c2 = deflection_distance(ptTo, ptFrom, -90, distance);
			if (c1 == null || c2 == null)
				return null;
			var coords = new Coordinate[5];
			coords[0] = ptTo;// deflection_distance(ptFrom, ptTo, 90, distance);
			coords[1] = c1;
			coords[2] = c2;
			coords[3] = ptFrom;// deflection_distance(ptTo, ptFrom, -90, distance);
			coords[coords.Length - 1] = coords[0];
			return new Polygon(new LinearRing(coords));
		}


		/// <summary>
		/// 向右边缓冲distance距离和得到的Polygon对象
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static ILineString OffsetRight(Coordinate ptFrom, Coordinate ptTo, double distance)
		{
			var c1 = deflection_distance(ptFrom, ptTo, -90, distance);
			var c2 = deflection_distance(ptTo, ptFrom, 90, distance);
			if (c1 == null || c2 == null)
				return null;
			var coords = new Coordinate[2];
			coords[0] = c1;
			coords[1] = c2;
			return new LineString(coords);
		}

		/// <summary>
		/// 向左边缓冲distance距离和得到的Polygon对象
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static ILineString OffsetLeft(Coordinate ptFrom, Coordinate ptTo, double distance)
		{
			var c1 = deflection_distance(ptFrom, ptTo, 90, distance);
			var c2 = deflection_distance(ptTo, ptFrom, -90, distance);
			if (c1 == null || c2 == null)
				return null;
			var coords = new Coordinate[2];
			coords[0] = c1;
			coords[1] = c2;
			return new LineString(coords);
		}


		/// <summary>
		/// 判断浮点数是否为0
		/// </summary>
		/// <param name="d"></param>
		/// <param name="tolerance">容差</param>
		/// <returns></returns>
		public static bool IsZero(double d, double tolerance = intersection_epsilon)
		{
			return Math.Abs(d) < tolerance;// intersection_epsilon;
		}

		public static bool equal(double d1, double d2, double tolerance = intersection_epsilon)
		{
			return IsZero(d1 - d2, tolerance);
		}

		/// <summary>
		/// 粗略判断两个点是否重叠
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public static bool isSamePoint(Coordinate p1, Coordinate p2, double tolerance = intersection_epsilon)
		{
			return equal(p1.X, p2.X, tolerance) && equal(p1.Y, p2.Y, tolerance);
		}

		public static bool IsSame2(double x1, double y1, double x2, double y2, double tolerance, double tolerance2)
		{
			if (!equal(x1, x2, tolerance))
				return false;
			if (!equal(y1, y2, tolerance))
				return false;
			return GetDistance2(x1, y1, x2, y2) <= tolerance2;
		}
		/// <summary>
		/// 精确判断两个点是否重叠（isSamePoint2）
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="tolerance2">容差的平方</param>
		/// <returns></returns>
		public static bool IsSame2(double x1, double y1, double x2, double y2, double tolerance2)
		{
			return GetDistance2(x1, y1, x2, y2) <= tolerance2;
		}
		/// <summary>
		/// 精确判断两个点是否重叠（isSamePoint2）
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="tolerance2">容差的平方</param>
		/// <returns></returns>
		public static bool IsSame2(Coordinate p1, Coordinate p2, double tolerance2)
		{
			return IsSame2(p1.X, p1.Y, p2.X, p2.Y, tolerance2);
		}
		public static bool IsSame2(Coordinate p1, Coordinate p2, double tolerance, double tolerance2)
		{
			return IsSame2(p1.X, p1.Y, p2.X, p2.Y, tolerance, tolerance2);
		}
		//求两个点距离的平方
		public static double GetDistance2(double x1, double y1, double x2, double y2)
		{
			double dx = x1 - x2;
			double dy = y1 - y2;
			return dx * dx + dy * dy;
		}

		//求两个点距离的平方
		public static double GetDistance2(Coordinate p1, Coordinate p2)
		{
			return GetDistance2(p1.X, p1.Y, p2.X, p2.Y);
		}

		/// <summary>
		/// 点是否在线段上
		/// </summary>
		/// <param name="ls"></param>
		/// <param name="p"></param>
		/// <param name="tolerance"></param>
		/// <returns></returns>
		public static bool IsPointOnLine(Coordinate ptFrom, Coordinate ptTo, Coordinate p, double tolerance2)
		{
			int o = Orientation_index(ptFrom.X, ptFrom.Y, ptTo.X, ptTo.Y, p.X, p.Y, tolerance2);
			return o == 0;
		}

		/// <summary>
		/// 求点p在直线（ptFrom,ptTo）上的投影点；
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Coordinate GetProjectionPoint(Coordinate ptFrom, Coordinate ptTo, Coordinate p)
		{
			var c = Cgl.GetLineProjectionPoint(ptFrom, ptTo, p);// CglAlgorithm.GetProjectionPoint(toCglPoint(ptFrom), toCglPoint(ptTo), toCglPoint(p));
			return new Coordinate(c.X, c.Y);
		}
		/// <summary>
		/// 球线段[q0,q1]在线段[p0,p1]上的投影线段
		/// 有可能无投影或一个点或一条线段
		/// </summary>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="q0"></param>
		/// <param name="q1"></param>
		/// <returns></returns>
		public static Tuple<Coordinate, Coordinate> GetProjectionLineSegment(Coordinate p0, Coordinate p1, Coordinate q0, Coordinate q1)
		{
			var ls = new CglLineSegment(toCglPoint(p0), toCglPoint(p1));
			return GetProjectionLineSegment(ls, q0, q1);
		}

		public static Tuple<Coordinate, Coordinate> GetProjectionLineSegment(CglLineSegment ls, Coordinate q0, Coordinate q1)
		{
			//var ls = new CglLineSegment(toCglPoint(p0), toCglPoint(p1));
			var line = new CglLine(ls);
			var c0 = CglAlgorithm.GetProjectionPoint(line, toCglPoint(q0));
			var c1 = CglAlgorithm.GetProjectionPoint(line, toCglPoint(q1));
			var pos1 = CglAlgorithm.GetPosition(ls, c0);
			var pos2 = CglAlgorithm.GetPosition(ls, c1);
			if (pos1 == CglPositionType.PT_LEFTSIDE && pos2 == CglPositionType.PT_LEFTSIDE
				|| pos1 == CglPositionType.PT_RIGHTSIDE && pos2 == CglPositionType.PT_RIGHTSIDE)
				return null;
			var p0 = new Coordinate(ls.ptFrom.X, ls.ptFrom.Y);
			var p1 = new Coordinate(ls.ptTo.X, ls.ptTo.Y);
			Coordinate r0 = null, r1 = null;
			if (pos1 == CglPositionType.PT_LEFTSIDE)
			{
				r0 = p0;
			}
			else if (pos1 == CglPositionType.PT_RIGHTSIDE)
			{
				r0 = p1;
			}
			else
			{
				r0 = new Coordinate(c0.X, c0.Y);
			}
			if (pos2 == CglPositionType.PT_LEFTSIDE)
			{
				r1 = p0;
			}
			else if (pos2 == CglPositionType.PT_RIGHTSIDE)
			{
				r1 = p1;
			}
			else
			{
				r1 = new Coordinate(c1.X, c1.Y);
			}
			return new Tuple<Coordinate, Coordinate>(r0, r1);
		}

		public static CglPoint toCglPoint(Coordinate c)
		{
			return new CglPoint(c.X, c.Y);
		}

		/// <summary>
		/// 计算角p1p0p2的夹角，返回度[0~180)
		/// </summary>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <returns></returns>
		public static double CalcAngle(double x0, double y0, double x1, double y1, double x2, double y2)
		{
			var v1x = x1 - x0;
			var v1y = y1 - y0;
			var v2x = x2 - x0;
			var v2y = y2 - y0;

			var multi = v1x * v2x + v1y * v2y;
			var v1mod = Math.Sqrt(v1x * v1x + v1y * v1y);
			var v2mode = Math.Sqrt(v2x * v2x + v2y * v2y);

			var angle = Math.Acos(Math.Round(multi / v1mod / v2mode, 6)) * 180 / Math.PI;
			return angle;
		}

		/// <summary>
		/// 计算角p1p0p2的夹角，返回度[0~180)
		/// </summary>
		/// <param name="cen"></param>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <returns></returns>
		public static double CalcAngle(Coordinate cen, Coordinate first, Coordinate second)
		{
			return CalcAngle(cen.X, cen.Y, first.X, first.Y, second.X, second.Y);
		}

		//[DllImport("tGISC.dll", EntryPoint = "TGIS_Cgl_Orientation_index", CallingConvention = CallingConvention.Cdecl)]
		//private static extern int TGIS_Cgl_Orientation_index(double x1, double y1, double x2, double y2, double x, double y, double tolerance2);

		/// <summary>
		/// 判断点(x,y)在线段(x1,y1),(x2,p2)的方位，0表示在直线上
		/// </summary>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <param name="x2"></param>
		/// <param name="y2"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="tolerance2"></param>
		/// <returns></returns>
		public static int Orientation_index(double x1, double y1, double x2, double y2, double x, double y, double tolerance2)
		{
			return Cgl.Orientation_index(x1, y1, x2, y2, x, y, tolerance2);
		}

	

	}
}
