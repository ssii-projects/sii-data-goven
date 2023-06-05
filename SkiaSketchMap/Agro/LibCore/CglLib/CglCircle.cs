using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 圆：由圆心和半径构成
    /// </summary>
    public struct CglCircle
    {
        /// <summary>
        /// 圆心
        /// </summary>
        public CglPoint ptCenter;// = Point.Empty;
        /// <summary>
        /// 半径
        /// </summary>
        public double r;//=0;
        /// <summary>
        /// 构造以坐标x和y为圆心r为半径的圆
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r"></param>
        public CglCircle(double x, double y, double r)
        {
            this.ptCenter = new CglPoint(x, y);
            this.r = r;
        }
        /// <summary>
        /// 用指定的中心点坐标和半径初始化Circle类的新实例
        /// </summary>
        /// <param name="ptCenter"></param>
        /// <param name="r"></param>
        public CglCircle(CglPoint ptCenter, double r)
        {
            this.ptCenter = ptCenter;
            this.r = r;
        }
        /// <summary>
        /// 求圆周上的一个点并且满足圆心到该点的方向为direction
        /// </summary>
        /// <param name="direction">单位：弧度(范围：0~2π)</param>
        /// <returns></returns>
        public CglPoint GetPoint(CglDirection direction)
        {
            return new CglPoint(ptCenter.X + r * Math.Cos(direction.Value), ptCenter.Y + r * Math.Sin(direction.Value));
        }
    }
}
