using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 线段
    /// </summary>
    public struct CglLineSegment
    {
        /// <summary>
        /// 起点
        /// </summary>
        public CglPoint ptFrom;
        /// <summary>
        /// 终点
        /// </summary>
        public CglPoint ptTo;// = Point.Empty;

        public CglLineSegment(double x1, double y1, double x2, double y2)
        {
            ptFrom = new CglPoint(x1, y1);
            ptTo = new CglPoint(x2, y2);
        }
        /// <summary>
        /// 用指定坐标初始化 LineSegment 类的新实例。
        /// </summary>
        /// <param name="pf"></param>
        /// <param name="pt"></param>
        public CglLineSegment(CglPoint pf, CglPoint pt)
        {
            ptFrom = pf;
            ptTo = pt;
        }
        /// <summary>
        /// 用指定坐标修改 LineSegment 类的属性。
        /// </summary>
        /// <param name="pf"></param>
        /// <param name="pt"></param>
        public void Modify(CglPoint pf, CglPoint pt)
        {
            ptFrom = pf;
            ptTo = pt;
        }
        /// <summary>
        /// 修改线段的起点和终点
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void PutCoords(double x1, double y1, double x2, double y2)
        {
            ptFrom.X = x1;
            ptFrom.Y = y1;
            ptTo.X = x2;
            ptTo.Y = y2;
        }
        /// <summary>
        /// 计算线段的长度
        /// </summary>
        /// <returns></returns>
        public double GetLength()
        {
            return CglAlgorithm.GetDistance(ptFrom, ptTo);
        }
        ///// <summary>
        ///// 求线段的方向（与坡向的定义一致）
        ///// 定义：以正北方（Y轴反方向）为0°，顺时针递增，南为90°，西为180°，北为270°等，范围在0°～359°59′59″之间
        ///// 测试程序见：CglLibTest.Test1
        ///// </summary>
        ///// <returns></returns>
        //public double GetAspectDegree()
        //{
        //    double d = 0;
        //    double dx=ptTo.X-ptFrom.X;
        //    double dy=ptTo.Y-ptFrom.Y;
        //    if (Algorithm.IsZero(dy))
        //    {
        //        d = dx < 0 ? 90 : 270;
        //    }
        //    else
        //    {
        //        if (dx > 0)
        //        {
        //            if(dy>0)//一象限
        //                d = 180+Math.Atan(Math.Abs(dx / dy)) * 180.0 / Math.PI;
        //            else//四象限
        //                d=270+Math.Atan(-dy / dx) * 180.0 / Math.PI;
        //        }
        //        else if (dx < 0)
        //        {
        //            if (dy < 0)//三象限
        //                d = Math.Atan(dx / dy) * 180.0 / Math.PI;
        //            else//二象限
        //                d = 90 + Math.Atan(dy / -dx) * 180.0 / Math.PI;
        //        }
        //        else
        //        {
        //            d = dy < 0 ? 0 :180;
        //        }
        //    }
        //    return d;
        //}
        /// <summary>
        /// 求线段的方向（线段起点到线段终点的方向）
        /// </summary>
        /// <returns></returns>
        public CglDirection GetDirection()
        {
            return GetDirection(this.ptFrom, this.ptTo);
            /*
            double d = 0;
            double dx = ptTo.X - ptFrom.X;
            double dy = ptTo.Y - ptFrom.Y;
            if (CglAlgorithm.IsZero(dy))
            {
                d = dx < 0 ? Math.PI : 0;
            }
            else
            {
                if (dx > 0)
                {
                    if (dy > 0)//一象限
                        d = Math.Atan(Math.Abs(dy / dx));
                    else//四象限
                        d = 2 * Math.PI - Math.Atan(-dy / dx);
                }
                else if (dx < 0)
                {
                    if (dy < 0)//三象限
                        d = Math.PI + Math.Atan(dy / dx);
                    else//二象限
                        d = Math.PI - Math.Atan(dy / -dx);
                }
                else
                {
                    d = dy < 0 ? 1.5 * Math.PI : 0.5 * Math.PI;
                }
            }
            return new CglDirection(d);
            */
        }
        ///// <summary>
        ///// 求线段的方向（线段起点到线段终点的方向）
        ///// </summary>
        ///// <returns></returns>
        //public Direction GetDirection()
        //{
        //    return GetAngleRadian() * 180.0 / Math.PI;
        //}
        /// <summary>
        /// 将线段正向延长distance距离
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool Expand(double distance)
        {
            double s1_x = this.ptTo.X - this.ptFrom.X;
            double s1_y = this.ptTo.Y - this.ptFrom.Y;
            double d = Math.Sqrt(s1_x * s1_x + s1_y * s1_y);
            if (CglAlgorithm.IsZero(d))
            {
                return false;
            }
            else
            {
                this.ptTo.X = this.ptTo.X + s1_x * distance / d;
                this.ptTo.Y = this.ptTo.Y + s1_y * distance / d;
                return true;
            }
        }
        /// <summary>
        /// 是否与p1与p2构成的线段相交
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool Intersects(CglPoint p1, CglPoint p2)
        {
            //if (!CglEnvelope.Intersects(ptFrom, ptTo, p1, p2))
            //    return false;
            CglPoint? p= CglAlgorithm.intersect_ll(this, new CglLineSegment(p1, p2));
            return p != null;
        }
        /// <summary>
        /// 求点p1到点p2的方向
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static CglDirection GetDirection(CglPoint p1, CglPoint p2)
        {
            double d = 0;
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            if (CglAlgorithm.IsZero(dy))
            {
                d = dx < 0 ? Math.PI : 0;
            }
            else
            {
                if (dx > 0)
                {
                    if (dy > 0)//一象限
                        d = Math.Atan(Math.Abs(dy / dx));
                    else//四象限
                        d = 2 * Math.PI - Math.Atan(-dy / dx);
                }
                else if (dx < 0)
                {
                    if (dy < 0)//三象限
                        d = Math.PI + Math.Atan(dy / dx);
                    else//二象限
                        d = Math.PI - Math.Atan(dy / -dx);
                }
                else
                {
                    d = dy < 0 ? 1.5 * Math.PI : 0.5 * Math.PI;
                }
            }
            return new CglDirection(d);
        }
    }
}
