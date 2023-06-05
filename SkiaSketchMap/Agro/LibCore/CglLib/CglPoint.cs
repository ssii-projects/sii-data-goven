using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 点
    /// </summary>
    public struct CglPoint
    {
        /// <summary>
        /// 表示 Point 类的成员数据未被初始化的新实例。
        /// </summary>
        public static readonly CglPoint Empty;//=new Point();
        /// <summary>
        /// X坐标
        /// </summary>
        public double X;
        /// <summary>
        /// Y坐标
        /// </summary>
        public double Y;
        /// <summary>
        /// 用指定坐标初始化 Point 类的新实例。
        /// </summary>
        /// <param name="x">该点的水平位置</param>
        /// <param name="y">该点的垂直位置</param>
        public CglPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        ///  比较两个 Point 结构。此结果指定两个 Point 结构的 Point.X和 Point.Y 属性的值是否相等。
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(CglPoint lhs, CglPoint rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }
        /// <summary>
        /// 确定指定点的坐标是否不等
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(CglPoint lhs, CglPoint rhs)
        {
            return !(lhs.X == rhs.X && lhs.Y == rhs.Y);
        }
        #region 矢量运算
        /// <summary>
        /// 矢量减法
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static CglPoint operator -(CglPoint p, CglPoint q)
        {
            return new CglPoint(p.X - q.X, p.Y - q.Y);
        }
        /// <summary>
        /// 矢量加法
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static CglPoint operator +(CglPoint p, CglPoint q)
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
        public static double operator *(CglPoint p, CglPoint q)
        {
            return p.X * q.Y - q.X * p.Y;
        }
        #endregion 矢量运算
        /// <summary>
        /// 指定此 Point 是否包含与指定 System.Object 有相同的坐标。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is CglPoint)
            {
                CglPoint p = (CglPoint)obj;
                return X == p.X && Y == p.Y;
            }
            return false;
        }
        /// <summary>
        /// 指定此 Point 是否包含与指定 Point 有相同的坐标。
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equals(CglPoint p)
        {
            return (X == p.X) && (Y == p.Y);
        }
        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)(X * Y);
        }
        /// <summary>
        /// 将自身以坐标原点为中心逆时针方向旋转angleRadian度
        /// </summary>
        /// <param name="angleRadian">单位弧度</param>
        public void Rotate(double angleRadian)
        {
            CglLineSegment ls = new CglLineSegment(0, 0, X, Y);
            double a = angleRadian + ls.GetDirection().Value;
            double d = ls.GetLength();
            X = d * Math.Cos(a);
            Y = d * Math.Sin(a);
        }
    }
}
