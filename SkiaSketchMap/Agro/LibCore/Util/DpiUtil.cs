using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
    public class DpiUtil
    {
        /// <summary>
        /// 一英寸等于72磅（point）
        /// </summary>
        public static int POINTS_PER_INCH = 72; //72.27

        /// <summary>
        /// 磅转换为像素
        /// </summary>
        /// <param name="x">单位：磅</param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public static float POINTS2PIXELS2(double x,float dpi)
        {
            double dInches = x / POINTS_PER_INCH;
            var outSize = dInches * dpi;
            outSize =Math.Floor(outSize + 0.5);
            return (float)outSize;
        }
    }
}
