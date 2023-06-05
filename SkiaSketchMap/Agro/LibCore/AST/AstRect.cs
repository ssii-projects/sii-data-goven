using System.Drawing;

namespace Agro.LibCore.AST
{
    public class MyRect
    {
        
        public double left;
        public double top;
        public double right;
        public double bottom;
        public MyRect(double l = 0, double t = 0, double width = 0, double height = 0)
        {
            SetRect(l, t, width, height);
        }
        public MyRect(MyRect rhs)
        {
            left = rhs.left;
            top = rhs.top;
            right = rhs.right;
            bottom = rhs.bottom;
        }
        public MyRect Clone()
        {
            return new MyRect(this);
        }
        public Point CenterPoint()
        {
            return new Point((int)(left + right) / 2, (int)(top + bottom) / 2);
        }
        public Point LeftTop()
        {
            return new Point((int)left, (int)top);
        }
        public double Width()
        {
                return right - left;
        }
        public double Height()
        {
               return bottom - top;
        }
        public void CenterAt(double x, double y)
        {
            double dx = x - (left + right) / 2;
            double dy = y - (top + bottom) / 2;
            OffsetRect(dx, dy);
        }
        public void CenterAt(Point pt)
        {
            CenterAt(pt.X, pt.Y);
            //double dx =pt.X - (left + right) / 2;
            //double dy = pt.Y - (top + bottom) / 2;
            //OffsetRect(dx, dy);
        }
        public void SetRect(MyRect rhs)
        {
            if (rhs != null)
            {
                SetRect(rhs.left, rhs.top, rhs.Width(), rhs.Height());
            }
        }
        public void SetRect(double l, double t, double width, double height)
        {
            left = l;
            top = t;
            right = l + width;
            bottom = t + height;
        }
        public void OffsetRect(double dx, double dy)
        {
            left += dx;
            right += dx;
            top += dy;
            bottom += dy;
        }
        //public System.Windows.Rect ToRect()
        //{
        //    return new System.Windows.Rect(left, top, Width(), Height());
        //}
        public System.Drawing.Rectangle ToRectangle()
        {
            return new System.Drawing.Rectangle((int)left, (int)top, (int)Width(), (int)Height());
        }
        public bool PointInRect(double x, double y)
        {
            return x > left && x < right && y > top && y < bottom;
        }
    }
}
