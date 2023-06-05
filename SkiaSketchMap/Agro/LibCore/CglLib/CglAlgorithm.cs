using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// CGL库，包括几何对象（点、线、线段和园）的结构和几何对象之间的关系算法
    /// </summary>
    public class CglAlgorithm
    {
        /// <summary>
        /// 容差
        /// </summary>
        public const double intersection_epsilon = 1.0e-30;
        /// <summary>
        /// π
        /// </summary>
        public const double PI = Math.PI;//3.1415926535897932384626433832795;
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
        /// <summary>
        /// 判断是否相等
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static bool IsEqual(double d1, double d2, double tolerance = intersection_epsilon)
        {
            //if (d1 == null && d2 == null)
            //    return true;
            //else if (d1 == null || d2 == null)
            //    return false;
            //else
            return IsZero((double)d1 - (double)d2, tolerance);
        }

        /// <summary>
        ///求两直线的交点
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns>null表示无交点</returns>
        public static CglPoint? intersect_ll(CglLine l1, CglLine l2)
        {
            CglPoint? pt = null;
            if (IsLineParallel(l1, l2))
                return null;
            if (l1.k != null)
            {
                double x = l2.k != null ? ((l2.b - l1.b) / ((double)l1.k - (double)l2.k)) : l2.b;
                pt = new CglPoint(x, (double)l1.k * x + l1.b);
            }
            else
            {
                pt = new CglPoint(l1.b, (double)l2.k * l1.b + l2.b);
            }
            return pt;
        }
        /// <summary>
        /// 求两线段的交点
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        public static CglPoint? intersect_ll(CglLineSegment l1, CglLineSegment l2, double tolerance = CglVectorAlgorithm.intersection_epsilon)
        {
            if (!new CglVectorAlgorithm(tolerance).SegmentIntersects(l1.ptFrom, l1.ptTo, l2.ptFrom, l2.ptTo))
            {
                return null;
            }
            CglPoint? p=intersect_ll(new CglLine(l1), new CglLine(l2));
            System.Diagnostics.Debug.Assert(p != null);
            //if (p != null)
            //{
            //    if (!new CglEnvelope(l1.ptFrom, l1.ptTo).IsContain((CglPoint)p)
            //        || !new CglEnvelope(l2.ptFrom, l2.ptTo).IsContain((CglPoint)p))
            //    {
            //        return null;
            //    }
            //    //CglLineSegment ls = l1;
            //    //double minx = Math.Min(ls.ptFrom.X, ls.ptTo.X);
            //    //double maxx = Math.Max(ls.ptFrom.X, ls.ptTo.X);
            //    //double miny = Math.Min(ls.ptFrom.Y, ls.ptTo.Y);
            //    //double maxy = Math.Max(ls.ptFrom.Y, ls.ptTo.Y);
            //    //CglPoint pt = (CglPoint)p;
            //    //if (!(pt.X >= minx && pt.X <= maxx && pt.Y >= miny && pt.Y <= maxy))
            //    //    return null;
            //}
            return p;
        }
        /// <summary>
        /// 求直线与线段的交点
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static CglPoint? intersect_ll(CglLine l1, CglLineSegment ls, double tolerance = intersection_epsilon)
        {
            CglPoint? p = intersect_ll(l1, new CglLine(ls));
            if (p != null)
            {
                CglPoint pt = (CglPoint)p;
                if (!new CglEnvelope(ls.ptFrom, ls.ptTo).IsContain(pt, tolerance))
                    return null;
            }
            return p;
        }
        /// <summary>
        /// 求射线与直线的交点
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static CglPoint? CalcIntersectPoint(CglRay ray, CglLine line, double tolerance = intersection_epsilon)
        {
            CglLine l = new CglLine(ray);
            CglPoint? p = intersect_ll(l, line);
            if (p != null)
            {
                CglPoint pt = (CglPoint)p;
                if(IsEqual(pt.X,ray.ptFrom.X,tolerance)&&IsEqual(pt.Y,ray.ptFrom.Y,tolerance))
                    return p;
                double delta = CglLineSegment.GetDirection(ray.ptFrom, pt).Value - ray.direction.Value;
                if (Math.Abs(delta)<0.01)
                    return p;
            }
            return null;
        }
        /// <summary>
        /// 求射线与线段的交点
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="ls"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static CglPoint? CalcIntersectPoint(CglRay ray, CglLineSegment ls, double tolerance = intersection_epsilon)
        {
            CglPoint? p = CalcIntersectPoint(ray, new CglLine(ls), tolerance);
            if (p != null)
            {
                CglPoint pt = (CglPoint)p;
                if (!new CglEnvelope(ls.ptFrom, ls.ptTo).IsContain(pt, tolerance))
                    return null;
            }
            return p;
        }
        /// <summary>
        /// 两直线是否平行
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        public static bool IsLineParallel(CglLine l1, CglLine l2)
        {
            return l1.k == null && l2.k == null
                || l1.k != null && l2.k != null && CglAlgorithm.IsEqual((double)l1.k, (double)l2.k);
        }
        /// <summary>
        ///求直线l与圆c相交的交点，
        /// </summary>
        /// <param name="l"></param>
        /// <param name="cr"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns>return 0，无交点;return 1,一个交点;return 2两个交点</returns>
        public static int intersect_lc(CglLine l, CglCircle cr, out CglPoint pt1, out CglPoint pt2)
        {
            pt1 = CglPoint.Empty;
            pt2 = CglPoint.Empty;
            if (l.k == null)
            {
                double dx = l.b - cr.ptCenter.X;
                double dx2 = dx * dx;
                double r2 = cr.r * cr.r;
                if (r2 < dx2)
                    return 0;
                if (IsEqual(r2, dx2))
                {
                    pt1 = new CglPoint(l.b, cr.ptCenter.Y);
                    return 1;
                }
                pt1 = new CglPoint(l.b, -Math.Sqrt(r2 - dx2) + cr.ptCenter.Y);
                pt2 = new CglPoint(l.b, Math.Sqrt(r2 - dx2) + cr.ptCenter.Y);
                return 2;
            }
            double k = (double)l.k;
            double a = 1 + k * k;
            double b = 2 * (k * l.b - cr.ptCenter.X - k * cr.ptCenter.Y);
            double c = cr.ptCenter.X * cr.ptCenter.X + l.b * l.b + cr.ptCenter.Y * cr.ptCenter.Y - cr.r * cr.r - 2 * l.b * cr.ptCenter.Y;
            if (IsZero(a))
            {
                if (IsZero(b))
                {
                    return 0;//无交点
                }
                else
                {
                    pt1 = new CglPoint(-c / b, k * pt1.X + l.b);
                    return 1;
                }
            }
            else
            {
                double b2 = b * b;
                double ac = 4 * a * c;
                if (b2 < ac)
                {
                    return 0;
                }
                double sq = Math.Sqrt(b2 - ac);
                double x = (-b + sq) / (2 * a);
                pt1 = new CglPoint(x, k * x + l.b);
                x = (-b - sq) / (2 * a);
                pt2 = new CglPoint(x, k * x + l.b);
                return 2;
            }
        }

        /// <summary>
        /// 求两个圆的交点
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>0，无交点;1,一个交点;2两个交点</returns>
        public static int intersect_cc(CglCircle c1, CglCircle c2, CglPoint p1, CglPoint p2)
        {
            double dx = c1.ptCenter.X - c2.ptCenter.X;
            double dy = c1.ptCenter.Y - c2.ptCenter.Y;
            double sx = c1.ptCenter.X + c2.ptCenter.X;
            double sy = c1.ptCenter.Y + c2.ptCenter.Y;
            if (IsZero(dy))
            {
                if (IsZero(dx))
                {
                    return 0;
                }
                p1.X = (c2.r * c2.r - c1.r * c1.r + dx * sx) / (2.0f * dx);
                p2.X = p1.X;

                double sq = Math.Sqrt(c1.r * c1.r - (p1.X - c1.ptCenter.X) * (p1.X - c1.ptCenter.X));
                p1.Y = c1.ptCenter.Y + sq;
                p2.Y = c1.ptCenter.Y - sq;
                return IsZero(sq) ? 1 : 2;
            }
            else
            {
                if (IsZero(dy))
                {
                    return 0;
                }
                double m = (c2.r * c2.r - c1.r * c1.r + dx * sx + dy * sy) / (2.0f * dy);
                double n = dx / dy;
                double h = m - c1.ptCenter.Y;
                double a = n * n + 1;
                double b = -2 * (c1.ptCenter.X + h * n);
                double c = c1.ptCenter.X * c1.ptCenter.X + h * h - c1.r * c1.r;
                if (b * b < 4 * a * c)
                    return 0;
                double sq = Math.Sqrt(b * b - 4 * a * c);
                p1.X = (-b + sq) / (2.0f * a);
                p1.Y = m - n * p1.X;
                p2.X = (-b - sq) / (2.0f * a);
                p2.Y = m - n * p2.X;
                return IsZero(sq) ? 1 : 2;
            }
        }

        /// <summary>
        /// 求过一点p与直线l垂直的直线
        /// </summary>
        /// <param name="l"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static CglLine GetPerpendicularLine(CglLine l, CglPoint p)
        {
            CglLine retLine = new CglLine();
            if (l.k == null)
            {
                retLine.k = 0;
                retLine.b = p.Y;
            }
            else
            {
                double k = (double)l.k;
                if (IsZero(k))
                {
                    retLine.k = null;
                    retLine.b = p.X;
                }
                else
                {
                    k = -1.0f / k;
                    retLine.b = p.Y - k * p.X;
                    retLine.k = k;
                }
            }
            return retLine;
        }
        /// <summary>
        /// 求过一点p与线段ls垂直的直线
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static CglLine GetPerpendicularLine(CglLineSegment ls, CglPoint p)
        {
            return GetParallelLine(new CglLine(ls), p);
        }

        /// <summary>
        /// 求过点p且与l平行的直线
        /// </summary>
        /// <param name="l"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static CglLine GetParallelLine(CglLine l, CglPoint p)
        {
            CglLine retLine = l;
            retLine.k = l.k;
            if (l.k == null)
            {
                retLine.b = p.X;
            }
            else
            {
                retLine.b = p.Y - (double)l.k * p.X;
            }
            return retLine;
        }

        /// <summary>
        /// 求与线段ls平行且距离为distance的直线，结果存l1和l2
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="distance"></param>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        public static void GetParallelDistance(CglLineSegment ls, double distance, out CglLine l1, out CglLine l2)
        {
            CglLine l = new CglLine(ls);
            CglLine lp = GetPerpendicularLine(l, ls.ptTo);
            CglPoint p1, p2;
            intersect_lc(lp, new CglCircle(ls.ptTo, distance), out p1, out p2);
            l1 = GetParallelLine(l, p1);
            l2 = GetParallelLine(l, p2);
        }

        /// <summary>
        ///线段ls延长distance距离然后作逆时针旋转angle度得到返回点
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="angle">单位为度</param>
        /// <param name="distance"></param>
        /// <param name="p3"></param>
        /// <returns></returns>
        public static bool deflection_distance(CglLineSegment ls, double angle, double distance, CglPoint p3)
        {
            double s1_x = ls.ptTo.X - ls.ptFrom.X;
            double s1_y = ls.ptTo.Y - ls.ptFrom.Y;
            double d = Math.Sqrt(s1_x * s1_x + s1_y * s1_y);
            if (IsZero(d))
            {
                return false;
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
                p3.X = ls.ptTo.X + s1_x;
                p3.Y = ls.ptTo.Y + s1_y;
                return true;
            }
        }

        /// <summary>
        /// 判断点是否在直线上
        /// </summary>
        /// <param name="l"></param>
        /// <param name="p"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsPointOnLine(CglLine l, CglPoint p, double tolerance)
        {
            System.Diagnostics.Debug.Assert(tolerance >= 0);
            double delta = l.k != null ? (p.Y - (double)l.k * p.X - l.b) : (p.X - l.b);
            return Math.Abs(delta) <= tolerance;
        }
        /// <summary>
        /// 点是否在线段上
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="p"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsPointOnLineSegment(CglLineSegment ls, CglPoint p, double tolerance)
        {
            if (IsPointOnLine(new CglLine(ls), p, tolerance))
            {
                double minx = Math.Min(ls.ptFrom.X, ls.ptTo.X);
                double maxx = Math.Max(ls.ptFrom.X, ls.ptTo.X);
                double miny = Math.Min(ls.ptFrom.Y, ls.ptTo.Y);
                double maxy = Math.Max(ls.ptFrom.Y, ls.ptTo.Y);
                return p.X >= minx && p.X <= maxx && p.Y >= miny && p.Y <= maxy;
            }
            return false;
        }
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetDistance(CglPoint p1, CglPoint p2)
        {
            return GetDistance(p1.X, p1.Y, p2.X, p2.Y);
        }
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            double dx = (y1 - y2);
            double dy = (x1 - x2);
            return Math.Sqrt(dx * dx + dy * dy);
        }
        /// <summary>
        /// 求点到线段的距离
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="p"></param>
        /// <param name="pbOnLineSegment">点p是否在线段ls上</param>
        /// <returns></returns>
        public double GetDistance(CglLineSegment ls, CglPoint p, out bool pbOnLineSegment)
        {
            CglLine l = new CglLine(ls);
            CglLine ltmp = GetPerpendicularLine(l, p);
            CglPoint p1=(CglPoint)intersect_ll(l, ltmp);
            pbOnLineSegment = IsPointOnLineSegment(ls, p1, intersection_epsilon);
            if (pbOnLineSegment)
                return GetDistance(p, p1);
            return Math.Min(GetDistance(p, ls.ptFrom), GetDistance(p, ls.ptFrom));
        }
        /// <summary>
        /// 判断点p和线段ls的位置关系
        /// 判断准则：
        /// 设p1为p点在过线段ls的直线L上的投影点（过p点与直线L的垂线与L的交点）;
        /// 若p1在线段ls的延长线上则称点p在ls的右边（返回PositionType.PT_RIGHTSIDE）;
        /// 若p1在线段ls反方向的延长线上则称点p在ls的左边（返回PositionType.PT_LEFTSIDE）;
        /// 若p1在线段的起点到终点之间则称点p在ls上（返回PositionType.PT_INNER）;
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static CglPositionType GetPosition(CglLineSegment ls, CglPoint p)
        {
            if (ls.ptFrom.X < ls.ptTo.X)
            {
                if (p.X < ls.ptFrom.X)
                    return CglPositionType.PT_LEFTSIDE;
                else if (p.X > ls.ptTo.X)
                    return CglPositionType.PT_RIGHTSIDE;
                else
                    return CglPositionType.PT_INNER;
            }
            else if (ls.ptFrom.X > ls.ptTo.X)
            {
                if (p.X > ls.ptFrom.X)
                    return CglPositionType.PT_LEFTSIDE;
                else if (p.X < ls.ptTo.X)
                    return CglPositionType.PT_RIGHTSIDE;
                else
                    return CglPositionType.PT_INNER;
            }
            else if (ls.ptFrom.Y < ls.ptTo.Y)
            {
                if (p.Y < ls.ptFrom.Y)
                    return CglPositionType.PT_LEFTSIDE;
                else if (p.Y > ls.ptTo.Y)
                    return CglPositionType.PT_RIGHTSIDE;
                else
                    return CglPositionType.PT_INNER;
            }
            else if (ls.ptFrom.Y > ls.ptTo.Y)
            {
                if (p.Y > ls.ptFrom.Y)
                    return CglPositionType.PT_LEFTSIDE;
                else if (p.Y < ls.ptTo.Y)
                    return CglPositionType.PT_RIGHTSIDE;
                else
                    return CglPositionType.PT_INNER;
            }
            return CglPositionType.PT_RIGHTSIDE;
        }
        ///// <summary>
        ///// 求两个矢量p和q的夹角（单位：弧度）
        ///// </summary>
        ///// <param name="p"></param>
        ///// <param name="q"></param>
        ///// <returns></returns>
        //public static double GetVectorAngleRadian(CglPoint p, CglPoint q)
        //{
        //    double n = p.X * q.X + p.Y * q.Y;
        //    double m = Math.Sqrt(p.X * p.X + p.Y * p.Y) * Math.Sqrt(q.X * q.X + q.Y * q.Y);
        //    if (IsZero(m))
        //        return 0;
        //    return Math.Acos(n / m);
        //}
        /// <summary>
        /// 求坐标x,y的象限位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static CglQuadrantType GetQuadrant(double x, double y)
        {
            if (IsZero(x))
            {
                if (IsZero(y))
                {
                    return CglQuadrantType.OriginPoint;
                }
                return y > 0 ? CglQuadrantType.YPositive : CglQuadrantType.YNegative;
            }
            if (IsZero(y))
                return x > 0 ? CglQuadrantType.XPositive : CglQuadrantType.XNegative;
            if (x > 0)
                return y > 0 ? CglQuadrantType.One : CglQuadrantType.Four;
            else
                return y > 0 ? CglQuadrantType.Two : CglQuadrantType.Tree;
        }

        /// <summary>
        /// 判断线段与矩形是否相交
        /// </summary>
        /// <param name="env"></param>
        /// <param name="ls"></param>
        /// <returns></returns>
        public static bool Intersects(CglEnvelope env, CglLineSegment ls, double tolerance=CglVectorAlgorithm.intersection_epsilon)
        {
            return new CglVectorAlgorithm(tolerance).Intersects(env, ls.ptFrom, ls.ptTo);
            //CglEnvelope env1 = new CglEnvelope(ls.ptFrom, ls.ptTo);
            //if (!env.Intersects(env1))
            //    return false;
            //if (env.IsContain(ls.ptFrom) || env.IsContain(ls.ptTo))
            //    return true;
            //CglPoint p0=new CglPoint(env.MinX,env.MinY);
            //CglPoint p1=new CglPoint(env.MaxX,env.MinY);
            //CglPoint p2=new CglPoint(env.MaxX,env.MaxY);
            //CglPoint p3=new CglPoint(env.MinX,env.MaxY);
            //return ls.Intersects(p0, p1) || ls.Intersects(p1, p2) || ls.Intersects(p2, p3) || ls.Intersects(p3, p0);
        }
		///// <summary>
		///// 弧度转换为度
		///// </summary>
		///// <param name="radian">弧度</param>
		///// <returns></returns>
		//public static double RadiansToDegree(double radian)
		//{
		//    return radian * 180.0 / Math.PI;
		//}
		///// <summary>
		///// 度转换为弧度
		///// </summary>
		///// <param name="degree">度</param>
		///// <returns></returns>
		//public static double DegreeToRadians(double degree)
		//{
		//    return (degree % 360) * Math.PI / 180.0;
		//}


		/// <summary>
		/// 求点p在直线（ptFrom,ptTo）上的投影点；
		/// yxm 2019-4-24
		/// </summary>
		/// <param name="ptFrom"></param>
		/// <param name="ptTo"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static CglPoint GetProjectionPoint(CglPoint ptFrom, CglPoint ptTo, CglPoint p)
		{
			return GetProjectionPoint(new CglLine(ptFrom, ptTo), p);
		}
		/// <summary>
		/// 求点p在直线line上的投影点；
		///  yxm 2019-4-24
		/// </summary>
		/// <param name="line"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static CglPoint GetProjectionPoint(CglLine line, CglPoint p)
		{
			var l2 = GetPerpendicularLine(line, p);
			return (CglPoint)intersect_ll(line, l2);
		}
		/// <summary>
		/// 求过一点p与直线l垂直的直线
		/// yxm 2019-4-24
		/// </summary>
		/// <param name="l"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static CglLine GetPerpendicularLine(CglLine l, CglPoint p, double tolerance = intersection_epsilon)
		{
			CglLine retLine = new CglLine();
			if (l.k == null)
			{
				retLine.k = 0;
				retLine.b = p.Y;
			}
			else
			{
				double k = (double)l.k;
				if (IsZero(k, tolerance))
				{
					retLine.k = null;
					retLine.b = p.X;
				}
				else
				{
					k = -1.0f / k;
					retLine.b = p.Y - k * p.X;
					retLine.k = k;
				}
			}
			return retLine;
		}
	}
}
