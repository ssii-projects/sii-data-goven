using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    public class Vector3D
    {
        public double   x;
        public double   y;
        public double   z;
        
        public Vector3D() {}
        
        public Vector3D(double r, double s, double t)
        {
            x = r;
            y = s;
            z = t;
        }
        public Vector3D(Vector3D v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
        public Vector3D Set(double r, double s, double t)
        {
            x = r;
            y = s;
            z = t;
            return this;
        }

        public Vector3D Add(Vector3D v)
        {
            x += v.x;
            y += v.y;
            z += v.z;
            return this;
        }

        
        public Vector3D Minus(Vector3D v)
        {
            x -= v.x;
            y -= v.y;
            z -= v.z;
            return this;
        }
        
       /// <summary>
       /// 乘
       /// </summary>
       /// <param name="t"></param>
       /// <returns></returns>
        public Vector3D Mul(double t)
        {
            x *= t;
            y *= t;
            z *= t;
            return this;
        }
        /// <summary>
        /// 除
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3D Div(double t)
        {
            double f = 1.0F / t;
            x *= f;
            y *= f;
            z *= f;
            return this;
        }
        /// <summary>
        /// 取余数
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public Vector3D Mod(Vector3D v)
        {
            double       r, s;
            
            r = y * v.z - z * v.y;
            s = z * v.x - x * v.z;
            z = x * v.y - y * v.x;
            x = r;
            y = s;
            
            return this;
        }
        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3D Overrite(Vector3D v)
        {
            x *= v.x;
            y *= v.y;
            z *= v.z;
            return this;
        }
        public Vector3D Clone()
        {
            return new Vector3D(this);
        }
        public Vector3D Negate()
        {
            x=-x;
            y=-y;
            z=-z;
            return this;
        }
        
        //Vector3D operator +(Vector3D v)
        //{
        //    return (Vector3D(x + v.x, y + v.y, z + v.z));
        //}
        
        //Vector3D operator +(Vector2D v)
        //{
        //    return (Vector3D(x + v.x, y + v.y, z));
        //}
        
        //Vector3D operator -(Vector3D v)
        //{
        //    return (Vector3D(x - v.x, y - v.y, z - v.z));
        //}
        
        //Vector3D operator -(Vector2D v)
        //{
        //    return (Vector3D(x - v.x, y - v.y, z));
        //}
        
        //Vector3D operator *(double t)
        //{
        //    return (Vector3D(x * t, y * t, z * t));
        //}
        
        //Vector3D operator /(double t)
        //{
        //    double f = 1.0F / t;
        //    return (Vector3D(x * f, y * f, z * f));
        //}
        
        //double operator *(Vector3D v)
        //{
        //    return (x * v.x + y * v.y + z * v.z);
        //}
        
        //double operator *(Vector2D v)
        //{
        //    return (x * v.x + y * v.y);
        //}
        
        //Vector3D operator %(Vector3D v)
        //{
        //    return (Vector3D(y * v.z - z * v.y, z * v.x - x * v.z,
        //            x * v.y - y * v.x));
        //}
        
        //Vector3D operator (Vector3D v)
        //{
        //    return (Vector3D(x * v.x, y * v.y, z * v.z));
        //}
        
        public bool IsEqual(Vector3D v)
        {
            return ((x == v.x) && (y == v.y) && (z == v.z));
        }
        
        public bool IsNotEqual(Vector3D v)
        {
            return ((x != v.x) || (y != v.y) || (z != v.z));
        }
        
        public Vector3D Normalize()
        {
            return Div(Math.Sqrt(x * x + y * y + z * z));
            //return (*this /= sqrtf(x * x + y * y + z * z));
        }

        public Vector3D RotateAboutX(double angle)
        {
            double s =Math.Sin(angle);
            double c =Math.Cos(angle);

            double ny = c * y - s * z;
            double nz = c * z + s * y;

            y = ny;
            z = nz;
            return this;
        }
        public Vector3D RotateAboutY(double angle)
        {
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            double nx = c * x + s * z;
            double nz = c * z - s * x;

            x = nx;
            z = nz;
            return this;
        }
        public Vector3D RotateAboutZ(double angle)
        {
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            double nx = c * x - s * y;
            double ny = c * y + s * x;

            x = nx;
            y = ny;
            return this;
        }
        public Vector3D RotateAboutAxis(double angle, Vector3D axis)
        {
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);
            double k = 1.0F - c;

            double nx = x * (c + k * axis.x * axis.x) + y * (k * axis.x * axis.y - s * axis.z)
                    + z * (k * axis.x * axis.z + s * axis.y);
            double ny = x * (k * axis.x * axis.y + s * axis.z) + y * (c + k * axis.y * axis.y)
                    + z * (k * axis.y * axis.z - s * axis.x);
            double nz = x * (k * axis.x * axis.z - s * axis.y) + y * (k * axis.y * axis.z + s * axis.x)
                    + z * (c + k * axis.z * axis.z);

            x = nx;
            y = ny;
            z = nz;
            return this;
        }
    }
}
