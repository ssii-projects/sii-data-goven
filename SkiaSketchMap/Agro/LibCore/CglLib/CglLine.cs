using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 直线
    /// </summary>
    public struct CglLine
    {
        /// <summary>
        /// 斜率：直线斜率公式y=k*x+b中的k
        /// 当斜率k为null时表示斜率无效，此时直线方程简化为x=b;
        /// </summary>
        public double? k;
        /// <summary>
        /// 直线斜率公式y=k*x+b中的b
        /// 当斜率k为null时表示斜率无效，此时直线方程简化为x=b;
        /// </summary>
        public double b;

        /// <summary>
        /// 用指定的k和b初始化Line类的新实例
        /// </summary>
        /// <param name="k">斜率</param>
        /// <param name="b">截距（当x=0时，y的值）</param>
        public CglLine(double? k, double b)
        {
            this.k = k;
            this.b = b;
        }
        /// <summary>
        /// 用指定的两点初始化Line类的新实例
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public CglLine(CglPoint p1, CglPoint p2)
        {
            if (CglAlgorithm.IsEqual(p1.X, p2.X))
            {
                //_angleRadian = 0.5 * Math.PI;
                k = null;
                b = p1.X;
            }
            else
            {
                k = (p2.Y - p1.Y) / (p2.X - p1.X);
                //_angleRadian = Math.Atan(k1);
                b = p1.Y - (double)k * p1.X;
                //k = k1;
            }
        }
        /// <summary>
        /// 用指定的线段初始化Line类的新实例
        /// </summary>
        /// <param name="rhs"></param>
        public CglLine(CglLineSegment rhs)
            : this(rhs.ptFrom, rhs.ptTo)
        {
        }
        /// <summary>
        /// 用射线构造直线
        /// </summary>
        /// <param name="r"></param>
        public CglLine(CglRay r)
        {
            if (r.direction.Value == CglRadians.PIOver2 || r.direction.Value == 1.5 * Math.PI)
            {
                k = null;
                b = r.ptFrom.X;
            }
            else
            {
                k = Math.Tan(r.direction.Value);
                b = r.ptFrom.Y - (double)k * r.ptFrom.X;
            }
        }
        /// <summary>
        ///求直线与直线的交点
        /// </summary>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public CglPoint? CalcIntersectPoint(CglLine rhs)
        {
            return CglAlgorithm.intersect_ll(this, rhs);
        }
        /// <summary>
        /// 求直线与线段的交点
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public CglPoint? CalcIntersectPoint(CglLineSegment ls)
        {
            return CglAlgorithm.intersect_ll(this,ls);
        }
    }
}
