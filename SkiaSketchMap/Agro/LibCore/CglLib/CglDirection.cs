using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 方向：从X轴正方向开始逆时针旋转的角度。（X轴正方向为0度）；
    /// 单位：弧度，范围：[0~2π)
    /// </summary>
    public struct CglDirection
    {
        /// <summary>
        /// 值 单位：弧度，范围：[0~2π)
        /// </summary>
        private double _value;
        /// <summary>
        /// 值 单位：弧度。返回值范围：[0~2π)
        /// </summary>
        public double Value
        {
            get { return _value; }
            //private set;
            //get { return _value; }
            //set
            //{
            //    _value = NormalizePositive(value);
            //}
        }
        /// <summary>
        /// 用给定的弧度值构造一个新的实例
        /// </summary>
        /// <param name="value">单位：弧度，接受任意弧度值</param>
        public CglDirection(double value)
        {
            this._value =CglRadians.NormalizePositive(value);
        }
        /// <summary>
        /// 设置方向值，接受任意弧度值
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(double radiansValue)
        {
            this._value = CglRadians.NormalizePositive(radiansValue);
        }

        ///// <summary>
        ///// 将给定的弧度值转换为一个有效的方向值[0~2π)之间
        ///// </summary>
        ///// <param name="radianValue">任意弧度值</param>
        ///// <returns>[0~2π)之间</returns>
        //public static double NormalizePositive(double radianValue)
        //{
        //    double d = 2.0 * Math.PI;
        //    radianValue = radianValue % d;
        //    if (radianValue < 0)
        //        radianValue += d;
        //    return radianValue;
        //}
    }
}
