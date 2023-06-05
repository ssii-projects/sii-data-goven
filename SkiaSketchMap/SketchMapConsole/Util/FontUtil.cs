using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchMapConsole.Util
{
    internal static class FontUtil
    {
        public static string AdjustFontFamily(string fontFamily)
        {
            if (fontFamily == "黑体")
            {
                fontFamily = "SimHei";
            }
            else if (fontFamily == "宋体")
            {
                fontFamily = "SimSun";// "FangSong";
            }
            return fontFamily;
        }
    }
}
