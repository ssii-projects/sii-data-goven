using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 弧度相关
    /// </summary>
    public static class CglRadians
    {
        /// <summary>
        /// 2PI（360度）
        /// </summary>
        public const double PITimes2 = 2.0 * Math.PI;
        /// <summary>
        /// 0.5PI（90度）
        /// </summary>
        public const double PIOver2 = 0.5 * Math.PI;
        /// <summary>
        /// 0.25PI（45度）
        /// </summary>
        public const double PIOver4 = 0.25 * Math.PI;
        /// <summary>
        /// 度转换为弧度并且确保返回值在[0~2π)之间
        /// </summary>
        /// <param name="degree">度</param>
        /// <returns></returns>
        public static double DegreeToRadians(double degree)
        {
            return NormalizePositive(degree* Math.PI / 180.0);
        }
        /// <summary>
        /// 弧度转换为度，并且确保返回值在[0~36)之间
        /// </summary>
        /// <param name="radian">弧度</param>
        /// <returns></returns>
        public static double RadiansToDegree(double radian)
        {
            return NormalizePositive(radian) * 180.0 / Math.PI;
        }
        /// <summary>
        /// 将输入的弧度规格化为[0~2π)
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double NormalizePositive(double radians)
        {
            while (radians > PITimes2)
            {
                radians -= PITimes2;
            }
            while (radians < -PITimes2)
            {
                radians += PITimes2;
            }
            if (radians < 0)
                radians += PITimes2;
            if (radians == PITimes2)
                radians = 0;
            return radians;
        }
    }
}
