/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using GeoAPI.Geometries;
using System;


namespace Agro.Module.SketchMap
{
    /// <summary>
    /// 地块
    /// </summary>
    [Serializable]
    [DataTable("DLXX_XZDY", AliasName = "地块")]
    public class DLXX_XZDY
    {
        #region Property

        /// <summary>
        /// 名称(M)
        /// </summary>
        [DataColumn("MC", AliasName = "名称")]
        public virtual string MC { get; set; }

        /// <summary>
        /// 编码(M)(eFeatureType)
        /// </summary>
        [DataColumn("BM", AliasName = "编码")]
        public virtual string BM { get; set; }

        /// <summary>
        /// 图形
        /// </summary>
        [DataColumn("SHAPE", AliasName = "几何图形")]
        public virtual IGeometry Shape { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public DLXX_XZDY()
        {
        }

        #endregion

    };
}