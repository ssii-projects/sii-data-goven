using SkiaSharp;
using System.Drawing;

namespace SkiaMap
{
    public static class SKPaintExtension
    {
        public static SKRect QueryTextBound(this SKPaint t, string text, PointF p)
        {
            //var rc = GDIPlusUtil.QueryTextBound(Text, _font, _format, origin);

            var sz = new SKRect();
            t.MeasureText(text, ref sz);
            var wi = sz.Width * 0.5;
            var hi = sz.Height;
            sz.Left = (float)(p.X - wi);
            sz.Right = (float)(p.X + wi);
            sz.Top = (float)(p.Y - hi);
            sz.Bottom = (float)(p.Y + hi);
            return sz;
            //var p = origin;
            //var env = new Envelope(p.X, p.X - size.Width / 2, p.Y, p.Y + size.Height / 2);
            //return new RectangleF(p.X-,p.Y,);
        }
    }
}
