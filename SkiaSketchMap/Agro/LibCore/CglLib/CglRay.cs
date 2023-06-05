using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 射线：由起点和方向构成
    /// </summary>
    public struct CglRay
    {
        /// <summary>
        /// 起点
        /// </summary>
        public CglPoint ptFrom;
        /// <summary>
        /// 方向（x轴正方向逆时针旋转的弧度，范围：0~2π）
        /// </summary>
        public CglDirection direction;
        /// <summary>
        /// 用给定起点位置（x,y）和方向angleRadian构造新的实例
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction">方向值，弧度</param>
        public CglRay(double x, double y, CglDirection direction)
        {
            ptFrom = new CglPoint(x, y);
            this.direction = direction;
        }
        /// <summary>
        /// 用给定起点位置（x,y）和方向angleRadian构造新的实例
        /// </summary>
        /// <param name="p"></param>
        /// <param name="direction"></param>
        public CglRay(CglPoint p, CglDirection direction)
        {
            ptFrom = p;
            this.direction = direction;
        }
        /// <summary>
        /// 利用线段构造射线（线段的起点作为射线的起点，线段的方向作为射线的方向）
        /// </summary>
        /// <param name="ls"></param>
        public CglRay(CglLineSegment ls)
        {
            ptFrom = ls.ptFrom;
            direction = ls.GetDirection();
        }
        /// <summary>
        /// 求射线上距离起点s距离的点
        /// 测试代码：CglLibTest.Test2
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public CglPoint GetPoint(double s)
        {
            return GetPoint(this.ptFrom,this.direction, s);
            //double x = ptFrom.X + s * Math.Cos(direction.Value);
            //double y = ptFrom.Y + s * Math.Sin(direction.Value);
            //CglPoint p = new CglPoint(x, y);
            //return p;
        }
        /// <summary>
        /// 求射线与直线的交点
        /// </summary>
        /// <param name="line"></param>
        /// <param name="tolerance">必须满足>=0</param>
        /// <returns></returns>
        public CglPoint? CalcIntersectPoint(CglLine line, double tolerance =CglAlgorithm.intersection_epsilon)
        {
            return CglAlgorithm.CalcIntersectPoint(this, line, tolerance);
        }

        /// <summary>
        /// 求射线与线段的交点
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public CglPoint? CalcIntersectPoint(CglLineSegment ls, double tolerance = CglAlgorithm.intersection_epsilon)
        {
            return CglAlgorithm.CalcIntersectPoint(this, ls, tolerance);
        }
        /// <summary>
        /// 沿起点逆时针旋转radians弧度
        /// </summary>
        /// <param name="radians">单位：弧度</param>
        public void Rotate(double radians)
        {
            direction.SetValue(direction.Value + radians);
        }
        /// <summary>
        /// 求p点在dir方向上距离为s的点
        /// </summary>
        /// <param name="p"></param>
        /// <param name="dir"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static CglPoint GetPoint(CglPoint p, CglDirection dir, double s)
        {
            return GetPoint(p.X, p.Y, dir, s);
        }
        /// <summary>
        /// 求点(pX,pY)在dir方向上距离为s的点
        /// </summary>
        /// <param name="pX"></param>
        /// <param name="pY"></param>
        /// <param name="dir"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static CglPoint GetPoint(double pX,double pY, CglDirection dir, double s)
        {
            double x = pX + s * Math.Cos(dir.Value);
            double y = pY + s * Math.Sin(dir.Value);
            return new CglPoint(x, y);
        }
    }
}
