using GeoAPI.Geometries;
/*


*
* CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
* 文 件 名：   GeometryExtension
* 创 建 人：   颜学铭
* 创建时间：   2016/11/29 10:07:30
* 版    本：   1.0.0
* 备注描述：
* 修订历史：
*/

namespace Agro.LibCore
{
    /// <summary>
    /// IGeometry的扩展类
    /// </summary>
    public static class GeometryExtension
    {
        #region IGeometry Extension
        /// <summary>
        /// 设置坐标系
        /// 技巧：修改IGeometry.SRID字段的含义，用它来存储SpatialReferenceFactory._lstSpatialReference的索引
        /// </summary>
        /// <param name="g"></param>
        /// <param name="sr"></param>
        public static void SetSpatialReference(this IGeometry g, SpatialReference? sr)
        {
            g.SRID = sr == null ? 0 : sr.SRID;
        }
        /// <summary>
        /// 获取坐标系
        /// 技巧：用IGeometry.SRID字段的值从SpatialReferenceFactory._lstSpatialReference中获取对应的坐标系实例
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static SpatialReference GetSpatialReference(this IGeometry g)
        {
            return SpatialReferenceFactory.GetSpatialReference(g.SRID);
        }
        /// <summary>
        /// 投影并返回g自身
        /// </summary>
        /// <param name="g"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IGeometry Project(this IGeometry g, SpatialReference? target)
        {
            return SpatialReference.Project(g, target);
        }
        /// <summary>
        /// return g.EnvelopeInternal.Center
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static IPoint Centre(this IGeometry g)
        {
            var pt = g.EnvelopeInternal.Centre;
            return GeometryUtil.MakePoint(pt, g.GetSpatialReference());
        }
        public static void PutCoords(this IPoint pt, double x, double y)
        {
            pt.X = x;
            pt.Y = y;
        }
        public static void QueryCoords(this IPoint pt, out double x, out double y)
        {
            x = pt.X;
            y = pt.Y;
        }

        public static void QueryCoords(this Envelope env, out double dXMin, out double dYMin, out double dXMax, out double dYMax)
        {
            dXMin = env.MinX;
            dYMin = env.MinY;
            dXMax = env.MaxX;
            dYMax = env.MaxY;
        }

        public static void PutCoords(this Envelope env, double dXMin, double dYMin, double dXMax, double dYMax)
        {
            env.Init(dXMin, dXMax, dYMin, dYMax);
        }
        #endregion

        /// <summary>
        /// 更改env的内容并返回env
        /// </summary>
        /// <param name="env"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IPolygon Project(this Envelope env, SpatialReference src, SpatialReference target)
        {
            var pgn = GeometryUtil.MakePolygon(env, src);
            return (IPolygon)pgn.Project(target);
        }
        public static void SetCoords(this Coordinate c, double x, double y)
        {
            c.X = x;
            c.Y = y;
        }
        public static void SetCoords(this Coordinate c, Coordinate val)
        {
            c.X = val.X;
            c.Y = val.Y;
        }
        /// <summary>
        /// 如果有变化则修改自身，返回输入 c
        /// </summary>
        /// <param name="c"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Coordinate Project(this Coordinate c, SpatialReference src, SpatialReference target)
        {
            return SpatialReference.Project(c, src, target);
        }
        public static Envelope ScaleAt(this Envelope env, double scale, double x, double y)
        {
            return GeometryUtil.ScaleAt(env, scale, x, y);
        }
        public static IPolygon ToPolygon(this Envelope env, SpatialReference sr = null)
        {
            return GeometryUtil.MakePolygon(env, sr);
        }

    }

    public class VertexLocation
    {
        /// <summary>
        /// 几何对象的第几个部分
        /// </summary>
        public int PartIndex;
        /// <summary>
        /// 针对Polygon：0表示shell，[1..n]表示第几个hole
        /// </summary>
        public int RingIndex;
        public int PointIndex;

    }

}
