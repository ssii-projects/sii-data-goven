using SkiaSharp;
using System.Drawing;

namespace SkiaMap
{
    public static class ColorExtension
    {
        public static SKColor ToSkColor(this Color clr)
        {
            //if (clr == null)
            //{
            //    return SKColor.Empty;
            //}
            //Color c = (Color)clr!;
            return new SKColor((uint)(clr.ToArgb()));
        }
    }
}
