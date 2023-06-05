using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 包迹对象（几何对象的最小外包矩形）
    /// </summary>
    public struct CglEnvelope
    {
        private double _minx, _maxx, _miny, _maxy;
        public double MinX { get { return _minx; } }
        public double MaxX { get { return _maxx; } }
        public double MinY { get { return _miny; } }
        public double MaxY { get { return _maxy; } }
        public CglEnvelope(CglPoint p1, CglPoint p2)
        {
            _minx = p1.X;
            _maxx = p2.X;
            _miny = p1.Y;
            _maxy = p2.Y;
            Normalize();
        }
        public CglEnvelope(double x1, double y1, double x2, double y2)
        {
            _minx = x1;
            _maxx = x2;
            _miny = y1;
            _maxy = y2;
            Normalize();
        }
        public void PutCoords(double x1, double y1, double x2, double y2)
        {
            _minx = x1;
            _maxx = x2;
            _miny = y1;
            _maxy = y2;
            Normalize();
        }
        /// <summary>
        /// 规格化
        /// 确保xmin<=xmax且ymin<=ymax;
        /// </summary>
        public void Normalize()
        {
            if (this._minx > this._maxx)
            {
                double t = this._minx;
                _minx = _maxx;
                _maxx = t;
            }
            if (_miny > _maxy)
            {
                double t = _miny;
                _miny = _maxy;
                _maxy = t;
            }
        }
        /// <summary>
        /// Returns <c>true</c> if this <c>Envelope</c> is a "null" envelope.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this <c>Envelope</c> is uninitialized
        /// or is the envelope of the empty point.
        /// </returns>
        public bool IsNull
        {
            get
            {
                return _maxx < _minx;
            }
        }
        /// <summary>
        /// 判断点p是否在包迹内部
        /// </summary>
        /// <param name="env"></param>
        /// <param name="p"></param>
        /// <param name="tolerance">确保>=0</param>
        /// <returns></returns>
        public bool IsContain(CglPoint p,double tolerance =CglAlgorithm.intersection_epsilon)
        {
            System.Diagnostics.Debug.Assert(tolerance>=0);
            return IsContain(p.X, p.Y, tolerance);
            //if (p.X < this._minx - tolerance || p.X > this._maxx + tolerance)
            //    return false;
            //if (p.Y < this._miny - tolerance || p.Y > this._maxy + tolerance)
            //    return false;
            //return true;
        }
        /// <summary>
        /// 判断点(pX,pY)是否在包迹内部
        /// </summary>
        /// <param name="pX"></param>
        /// <param name="pY"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public bool IsContain(double pX, double pY, double tolerance = CglAlgorithm.intersection_epsilon)
        {
            System.Diagnostics.Debug.Assert(tolerance >= 0);
            if (pX < this._minx - tolerance || pX > this._maxx + tolerance)
                return false;
            if (pY < this._miny - tolerance || pY > this._maxy + tolerance)
                return false;
            return true;
        }
        /// <summary>
        /// Test the envelope defined by p1-p2 for intersection
        /// with the envelope defined by q1-q2
        /// </summary>
        /// <param name="p1">One extremal point of the envelope Point.</param>
        /// <param name="p2">Another extremal point of the envelope Point.</param>
        /// <param name="q1">One extremal point of the envelope Q.</param>
        /// <param name="q2">Another extremal point of the envelope Q.</param>
        /// <returns><c>true</c> if Q intersects Point</returns>
        public static bool Intersects(CglPoint p1, CglPoint p2, CglPoint q1, CglPoint q2)
        {
            return Intersects(p1.X, p1.Y, p2.X, p2.Y, q1.X, q1.Y, q2.X, q2.Y);
            //double minp = Math.Min(p1.X, p2.X);
            //double maxq = Math.Max(q1.X, q2.X);
            //if (minp > maxq)
            //    return false;

            //double minq = Math.Min(q1.X, q2.X);
            //double maxp = Math.Max(p1.X, p2.X);
            //if (maxp < minq)
            //    return false;

            //minp = Math.Min(p1.Y, p2.Y);
            //maxq = Math.Max(q1.Y, q2.Y);
            //if (minp > maxq)
            //    return false;

            //minq = Math.Min(q1.Y, q2.Y);
            //maxp = Math.Max(p1.Y, p2.Y);
            //if (maxp < minq)
            //    return false;

            //return true;
        }
        /// <summary>
        /// Test the envelope defined by p1-p2 for intersection
        /// with the envelope defined by q1-q2
        /// </summary>
        /// <param name="px1"></param>
        /// <param name="py1"></param>
        /// <param name="px2"></param>
        /// <param name="py2"></param>
        /// <param name="qx1"></param>
        /// <param name="qy1"></param>
        /// <param name="qx2"></param>
        /// <param name="qy2"></param>
        /// <returns></returns>
        public static bool Intersects(double px1, double py1, double px2, double py2, double qx1, double qy1, double qx2, double qy2)
        {
            double minp = Math.Min(px1, px2);
            double maxq = Math.Max(qx1, qx2);
            if (minp > maxq)
                return false;

            double minq = Math.Min(qx1, qx2);
            double maxp = Math.Max(px1, px2);
            if (maxp < minq)
                return false;

            minp = Math.Min(py1, py2);
            maxq = Math.Max(qy1, qy2);
            if (minp > maxq)
                return false;

            minq = Math.Min(qy1, qy2);
            maxp = Math.Max(py1, py2);
            if (maxp < minq)
                return false;

            return true;
        }
        /// <summary>
        /// Test the envelope defined by p1-p2 for intersection
        /// with the envelope defined by q1-q2
        /// 输入参数必须满足条件：pMinX <= pMaxX && pMinY <= pMaxY&& qMinX <= qMaxX && qMinY <= qMaxY
        /// </summary>
        /// <param name="pMinX"></param>
        /// <param name="pMinY"></param>
        /// <param name="pMaxX"></param>
        /// <param name="pMaxY"></param>
        /// <param name="qMinX"></param>
        /// <param name="qMinY"></param>
        /// <param name="qMaxX"></param>
        /// <param name="qMaxY"></param>
        /// <returns></returns>
        public static bool NormalizeIntersects(double pMinX, double pMinY, double pMaxX, double pMaxY, double qMinX, double qMinY, double qMaxX, double qMaxY)
        {
            System.Diagnostics.Debug.Assert(pMinX <= pMaxX && pMinY <= pMaxY&& qMinX <= qMaxX && qMinY <= qMaxY);
            return !(pMinX > qMaxX || pMaxX < qMinX|| pMinY > qMaxY || pMaxY < qMinY);
        }
        /// <summary>
        /// Check if the region defined by <c>other</c>
        /// overlaps (intersects) the region of this <c>Envelope</c>.
        /// </summary>
        /// <param name="other"> the <c>Envelope</c> which this <c>Envelope</c> is
        /// being checked for overlapping.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <c>Envelope</c>s overlap.
        /// </returns>
        public bool Intersects(CglEnvelope other, double tolerance = CglAlgorithm.intersection_epsilon)
        {
            //if (IsNull || other.IsNull)
            //    return false;
            System.Diagnostics.Debug.Assert(tolerance >= 0);
            return !(other.MinX > _maxx+tolerance || other.MaxX < _minx-tolerance 
                || other.MinY > _maxy+tolerance || other.MaxY < _miny-tolerance);
        }
    }
}
