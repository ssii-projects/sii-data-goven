/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   ColorUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/10/26 10:21:59
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/


//using SkiaSharp;
using System.Drawing;

namespace Agro.LibCore
{
    public class ColorUtil
    {
        //public static System.Drawing.Color ToDrawingColor(System.Windows.Media.Color clr)
        //{
        //    return System.Drawing.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        //}
        //public static System.Windows.Media.Color ToMediaColor(System.Drawing.Color clr)
        //{
        //    return System.Windows.Media.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strColor">like #FFA3F2F2</param>
        /// <returns></returns>
        public static Color ConvertFromString(string strColor)
        {
            //if(strColor.Length== 9)
            //{
            //    var a = strColor.Substring(1, 2);
            //}
            var clr= ColorTranslator.FromHtml(strColor);
            
            return clr;
            //return Color.White;
            //throw new NotImplementedException();
            //var mediaColor = (System.Windows.Media.Color)System.Windows.Media.
            //        ColorConverter.ConvertFromString(strColor);
            //return ToDrawingColor((System.Windows.Media.Color)mediaColor);
        }

    }
}
