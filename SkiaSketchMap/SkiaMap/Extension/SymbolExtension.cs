using Agro.GIS;
using Agro.LibCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiaMap
{
    public static class SymbolExtension
    {
        public static void SetupPaint(this ILineSymbol t,SKPaint paint,IDisplayTransformation? trans)
        {
            if (t.StrokeThickness > 0 && t.StrokeColor is Color clr)
            {
                var n = t.StrokeThickness;
                if (trans != null) n = DpiUtil.POINTS2PIXELS2(n, trans.Resolution);

                paint.Color = clr.ToSkColor();
                paint.StrokeWidth = (float)n;
            }
        }
    }
}
