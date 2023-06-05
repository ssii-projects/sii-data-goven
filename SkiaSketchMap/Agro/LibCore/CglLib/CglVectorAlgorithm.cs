using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 矢量算法
    /// 参考：计算几何算法概览.docx
    /// 该算法考虑了浮点数的精度
    /// </summary>
    public class CglVectorAlgorithm
    {
        /// <summary>
        /// 容差（精度）
        /// </summary>
        public const double intersection_epsilon = 1.0e-30;
        private double tolerance = intersection_epsilon;
        public CglVectorAlgorithm(double tolerance=intersection_epsilon)
        {
            this.tolerance =Math.Abs(tolerance);
        }
        /// <summary>
        /// 判断浮点数d是否为0（在算法的精度范围内）
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool IsZero(double d)
        {
            return d >= -tolerance && d <= tolerance;
        }
        /// <summary>
        /// 矢量减法
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static CglPoint Minus(CglPoint p, CglPoint q)
        {
            return new CglPoint(p.X - q.X, p.Y - q.Y);
        }
        /// <summary>
        /// 矢量加法
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static CglPoint Plus(CglPoint p, CglPoint q)
        {
            return new CglPoint(p.X + q.X, p.Y + q.Y);
        }
        /// <summary>
        /// 矢量叉积
        /// 　若 P × Q > 0 , 则P在Q的顺时针方向。
        /// 　若 P × Q < 0 , 则P在Q的逆时针方向。
        /// 　若 P × Q = 0 , 则P与Q共线，但可能同向也可能反向。
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static double Multiply(CglPoint p, CglPoint q)
        {
            return p.X * q.Y - q.X * p.Y;
        }

        /// <summary>
        /// 折线段的拐向判断：
　　 /// 折线段的拐向判断方法可以直接由矢量叉积的性质推出。
       /// 对于有公共端点的线段p0p1和p1p2，通过计算(p2 - p0) × (p1 - p0)的符号便可以确定折线段的拐向：
　　 /// 若(p2 - p0) × (p1 - p0) > 0,则p0p1在p1点拐向右侧后得到p1p2。
　　 /// 若(p2 - p0) × (p1 - p0) < 0,则p0p1在p1点拐向左侧后得到p1p2。
        /// 若(p2 - p0) × (p1 - p0) = 0,则p0、p1、p2三点共线。
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double CalcLineTrend(CglPoint p0, CglPoint p1, CglPoint p2)
        {
            return (p2 - p0) * (p1 - p0);
        }
        /// <summary>
        /// 判断点是否在线段上：
        /// 设点为Q，线段为P1P2 ，判断点Q在该线段上的依据是：
        /// ( Q - P1 ) × ( P2 - P1 ) = 0 且 Q 在以 P1，P2为对角顶点的矩形内。
        /// 前者保证Q点在直线P1P2上，后者是保证Q点不在线段P1P2的延长线或反向延长线上
        /// ，对于这一步骤的判断可以用以下过程实现：
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public bool OnSegment(CglPoint p1, CglPoint p2, CglPoint q)
        {
            double d = (q - p1) * (p2 - p1);
            if (!IsZero(d))
                return false;
            double minx = p1.X;
            double maxx = p2.X;
            if (minx > maxx)
            {
                minx = maxx;
                maxx = p1.X;
            }
            double miny = p1.Y;
            double maxy = p2.Y;
            if (miny > maxy)
            {
                miny = maxy;
                maxy = p1.Y;
            }
            minx -= tolerance;
            maxx += tolerance;
            miny -= tolerance;
            maxy += tolerance;
            return q.X >= minx && q.X <= maxx && q.Y >= miny && q.Y <= maxy;
        }
        /// <summary>
        /// 判断两线段是否相交：
　　 /// 我们分两步确定两条线段是否相交：
　　 /// (1)快速排斥试验（？我感觉意义不大，增加的指令集的开销可能大于现今CPU处理浮点数的开销）
　　 /// 设以线段 P1P2 为对角线的矩形为R， 设以线段 Q1Q2 为对角线的矩形为T，如果R和T不相交，显然两线段不会相交。
　　 /// (2)跨立试验
        /// 如果两线段相交，则两线段必然相互跨立对方。
        /// 若P1P2跨立Q1Q2 ，则矢量 ( P1 - Q1 ) 和( P2 - Q1 )位于矢量( Q2 - Q1 ) 的两侧，
        /// 即( P1 - Q1 ) × ( Q2 - Q1 ) * ( P2 - Q1 ) × ( Q2 - Q1 ) < 0。
        /// 上式可改写成( P1 - Q1 ) × ( Q2 - Q1 ) * ( Q2 - Q1 ) × ( P2 - Q1 ) > 0。
        /// 当 ( P1 - Q1 ) × ( Q2 - Q1 ) = 0 时，说明 ( P1 - Q1 ) 和 ( Q2 - Q1 )共线，但是因为已经通过快速排斥试验，
        /// 所以 P1 一定在线段 Q1Q2上；
        /// 同理，( Q2 - Q1 ) ×(P2 - Q1 ) = 0 说明 P2 一定在线段 Q1Q2上。
        /// 所以判断P1P2跨立Q1Q2的依据是：( P1 - Q1 ) × ( Q2 - Q1 ) * ( Q2 - Q1 ) × ( P2 - Q1 ) >= 0。
        /// 同理判断Q1Q2跨立P1P2的依据是：( Q1 - P1 ) × ( P2 - P1 ) * ( P2 - P1 ) × ( Q2 - P1 ) >= 0。
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public bool SegmentIntersects(CglPoint P1, CglPoint P2, CglPoint Q1, CglPoint Q2)
        {
            if (!CglEnvelope.Intersects(P1, P2, Q1,Q2))
                return false;
            double d1 = ((P1 - Q1) * (Q2 - Q1)) * ((Q2 - Q1) * (P2 - Q1));
            if (!IsZero(d1) && d1 < 0)
                return false;
            double d2=(( Q1 - P1 ) *( P2 - P1 )) *( ( P2 - P1 ) * ( Q2 - P1 ));
            if (!IsZero(d2) && d2 < 0)
                return false;
            return true;
        }
        /// <summary>
        /// 功能：判断线段和矩形是否相交　
        /// 先判断线段的俩个端点是否在矩形的内部，在就必然相交
        /// 其次判断线段的包围盒是否和矩形相交，不相交的话线段和矩形肯定也不相交   
        /// 最后判断，矩形的四个顶点是否位于线段的两侧，是则必然相交，否则就不相交
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool Intersects(CglEnvelope rec, CglPoint p1,CglPoint p2)
        {
            if (rec.IsContain(p1, tolerance) || rec.IsContain(p2, tolerance))
                return true;
            if (!rec.Intersects(new CglEnvelope(p1,p2),tolerance))
                return false;
            double t1 = (p2.Y - p1.Y) * (rec.MinX - p1.X) - (p2.X - p1.X) * (rec.MinY - p1.Y);
            double t2 = (p2.Y - p1.Y) * (rec.MinX - p1.X) - (p2.X - p1.X) * (rec.MaxY - p1.Y);
            double t3 = (p2.Y - p1.Y) * (rec.MaxX - p1.X) - (p2.X - p1.X) * (rec.MinY - p1.Y);
            double t4 = (p2.Y - p1.Y) * (rec.MaxX - p1.X) - (p2.X - p1.X) * (rec.MaxY - p1.Y);
            if (t1 >= 0 && t2 >= 0 && t3 >= 0 && t4 >= 0)
                return false;
            if (t1 <= 0 && t2 <= 0 && t3 <= 0 && t4 <= 0)
                return false;
            return true;
        }
	}
}
