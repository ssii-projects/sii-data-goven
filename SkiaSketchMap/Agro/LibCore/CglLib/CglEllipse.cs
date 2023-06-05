using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 椭圆：
    /// </summary>
    public struct CglEllipse
    {
        /// <summary>
        /// 圆心
        /// </summary>
        public CglPoint ptCenter;
        /// <summary>
        /// a截距
        /// </summary>
        public double a;
        /// <summary>
        /// b截距
        /// </summary>
        public double b;
        /// <summary>
        /// 旋转角
        /// </summary>
        public CglDirection angle;
        /// <summary>
        /// 构造一个标准椭圆
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public CglEllipse(double a, double b)
            : this(0, 0, a, b)
        {
        }
        /// <summary>
        /// 构造椭圆，圆心位于x,y位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public CglEllipse(double x, double y, double a, double b)
            :this(x,y,a,b,new CglDirection())
        {
            //ptCenter = new CglPoint(x, y);
            //this.a = a;
            //this.b = b;
            //this.angle = new CglDirection();
        }
        /// <summary>
        /// 利用圆心、a截距、b截距和旋转角构造新实例
        /// </summary>
        /// <param name="po"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="dir"></param>
        public CglEllipse(CglPoint po, double a, double b, CglDirection angle)
            :this(po.X,po.Y,a,b,angle)
        {
        }
        /// <summary>
        /// 利用圆心、a截距、b截距和旋转角构造新实例
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="dir"></param>
        public CglEllipse(double x, double y, double a, double b, CglDirection angle)
        {
            this.ptCenter = new CglPoint(x,y);
            this.a = a;
            this.b = b;
            this.angle = angle;
        }
        /// <summary>
        ///求圆周上的一个点并且满足圆心到该点的方向为direction
        /// 测试程序参见：CglLibTestWpf.Test1（利用插值法绘制椭圆）
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns></returns>
        public CglPoint GetPoint(CglDirection direction)
        {
            double angleRadian =direction.Value-this.angle.Value;
            angleRadian %= 2 * Math.PI;
            if (angleRadian < 0)
                angleRadian = 2 * Math.PI + angleRadian;
            CglPoint p = new CglPoint();
            #region 第一步.计算标准椭圆上的坐标
            if (angleRadian == 0.5)
                p.Y = b;
            else if (angleRadian == 1.5 * Math.PI)
                p.Y = -b;
            else
            {
                double a2 = a * a;
                double b2 = b * b;
                double ta = Math.Tan(angleRadian);
                double ta2 = ta * ta;
                p.X = Math.Sqrt(a2 * b2 / (b2 + a2 * ta2));
                p.Y = Math.Abs(ta * p.X);
                if (angleRadian > 1.5 * Math.PI)//四象限
                    p.Y = -p.Y;
                else if (angleRadian > Math.PI)
                {//三象限
                    p.X = -p.X;
                    p.Y = -p.Y;
                }
                else if (angleRadian > 0.5 * Math.PI)//二象限
                    p.X = -p.X;

            }
            #endregion 第一步.计算标准椭圆上的坐标
            #region 第二步.将坐标(x,y)在坐标原点处旋转AngleRadian度
            p.Rotate(this.angle.Value);
            #endregion
            #region 第三步.平移坐标(x,y)
            p.X += ptCenter.X;
            p.Y += ptCenter.Y;
            #endregion
            return p;
        }
    }
}
