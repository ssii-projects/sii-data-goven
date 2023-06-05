using Agro.LibCore.CglLib;
using GeoAPI.Geometries;
/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   Cgl
 * 创 建 人：   颜学铭
 * 创建时间：   2016/12/23 10:15:26
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;

namespace Agro.LibCore
{
	/// <summary>
	/// 基于GeoAPI的Cgl实现（Created at 2016-12-23 by Yanxueming）
	/// </summary>
	public class Cgl
    {
        /// <summary>
        /// 判断线段p0p1和p1p2是否重叠
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="tolerance2">容差的平方</param>
        /// <returns></returns>
        public static bool IsEdgeOverlap(Coordinate p0, Coordinate p1, Coordinate p2, double tolerance2)
        {
            #region boundbox判断
            double xmin = p1.X, xmax = p2.X;
            if (xmin > xmax)
            {
                xmin = xmax;
                xmax = p1.X;
            }
            if (p0.X < xmin || p0.X > xmax)
                return false;
            double ymin = p1.Y, ymax = p2.Y;
            if (ymin > ymax)
            {
                ymin = ymax;
                ymax = p1.Y;
            }
            if (p0.Y < ymin || p0.Y > ymax)
                return false;
            xmin = p0.X;
            xmax = p1.X;
            if (xmin > xmax)
            {
                xmin = xmax;
                xmax = p0.X;
            }
            if (p2.X < xmin || p2.X > xmax)
                return false;
            ymin = p0.Y;
            ymax = p1.Y;
            if (ymin > ymax)
            {
                ymin = ymax;
                ymax = p0.Y;
            }
            if (p2.Y < ymin || p2.Y > ymax)
                return false;
            #endregion

            if (IsPointOnLine(p1, p2, p0, tolerance2))
            {
                return true;
            }
            return IsPointOnLine(p0, p1, p2, tolerance2);
        }
        /// <summary>
        /// 判断点q是否在直线p0p1上；
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="q"></param>
        /// <param name="tolerance2">容差的平方</param>
        /// <returns></returns>
        public static bool IsPointOnLine(Coordinate p1, Coordinate p2, Coordinate q, double tolerance2)
        {
            double dx1 = p2.X - p1.X;
            double dy1 = p2.Y - p1.Y;
            double dx2 = q.X - p2.X;
            double dy2 = q.Y - p2.Y;

            double l2 = dx1 * dx1 + dy1 * dy1;
            double a = (p1.Y - q.Y) * (p2.X - p1.X) - (p1.X - q.X) * (p2.Y - p1.Y);
            return a * a < tolerance2 * l2;
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
        /// 计算点(x0,y0)到点(x1,y1)的距离的平方
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static double CalcLength2(double x0, double y0, double x1, double y1)
        {
            var dx = x0 - x1;
            var dy = y0 - y1;
            return dx * dx + dy * dy;
        }
        /// <summary>
        /// 计算点p1到p2的距离的平方
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double CalcLength2(Coordinate p1,Coordinate p2)
        {
            return CalcLength2(p1.X, p1.Y, p2.X, p2.Y);
        }

        public static bool IsSamePoint(Coordinate p0,Coordinate p1,double distance2)
        {
            double dx = p0.X - p1.X;
            double dy = p0.Y - p1.Y;
            double d = dx * dx + dy * dy;
            return d < distance2;
        }

        /// <summary>
        /// 求点p在直线[lineFromPoint,lineToPoint]上的投影点
        /// </summary>
        /// <param name="lineFromPoint"></param>
        /// <param name="lineToPoint"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Coordinate GetLineProjectionPoint(Coordinate lineFromPoint,Coordinate lineToPoint,Coordinate p)
        {
            var line = new CglLine(new CglPoint(lineFromPoint.X, lineFromPoint.Y), new CglPoint(lineToPoint.X, lineToPoint.Y));
            var l1=CglAlgorithm.GetPerpendicularLine(line, new CglPoint(p.X, p.Y));
            var p1=(CglPoint)CglAlgorithm.intersect_ll(line, l1);
            return new Coordinate(p1.X, p1.Y);
        }

		///// <summary>
		///// 判断点(x,y)在线段(x1,y1),(x2,p2)的方位，0表示在直线上
		///// </summary>
		///// <param name="x1"></param>
		///// <param name="y1"></param>
		///// <param name="x2"></param>
		///// <param name="y2"></param>
		///// <param name="x"></param>
		///// <param name="y"></param>
		///// <param name="tolerance2"></param>
		///// <returns></returns>
		//public static int Orientation_index(double x1, double y1, double x2, double y2, double x, double y, double tolerance2)
		//{
		//	return CglClib.Orientation_index(x1, y1, x2, y2, x, y, tolerance2);
		//}
	}
}
