using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    /// 正方形
    /// </summary>
    public struct CglSquare
    {
        public double Size;
        public double MinX;
        public double MinY;
        public double MaxX { get { return MinX + Size; } }
        public double MaxY { get { return MinY + Size; } }
        /// <summary>
        /// 判断是否与线段相交
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public bool IsIntersect(CglLineSegment ls)
        {
            return IsIntersect(ls.ptFrom.X, ls.ptFrom.Y, ls.ptTo.X, ls.ptTo.Y);
            //double lsMinX = Math.Min(ls.ptFrom.X, ls.ptTo.X);
            //double lsMaxX = Math.Max(ls.ptFrom.X, ls.ptTo.X);
            //double lsMinY = Math.Min(ls.ptFrom.Y, ls.ptTo.Y);
            //double lsMaxY = Math.Max(ls.ptFrom.Y, ls.ptTo.Y);
            //return CglEnvelope.NormalizeIntersects(MinX, MinY, MaxX, MaxY, lsMinX, lsMinY, lsMaxX, lsMaxY);
        }
        /// <summary>
        /// 判断是否与线段(p-q)相交
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="qx"></param>
        /// <param name="qy"></param>
        /// <returns></returns>
        public bool IsIntersect(double px, double py, double qx, double qy)
        {
            return CglEnvelope.NormalizeIntersects(MinX, MinY, MaxX, MaxY
                , Math.Min(px, qx), Math.Min(py, qy), Math.Max(px, qx), Math.Max(py, qy));
        }
    }
}
