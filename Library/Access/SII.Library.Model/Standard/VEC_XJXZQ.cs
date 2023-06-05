/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using System;
using System.Collections.Generic;

using Agro.LibCore.Database;
using NetTopologySuite.Geometries;

namespace Agro.Library.Model
{
    /// <summary>
    /// 县级行政区
    /// </summary>
    [Serializable]
    [DataTable("XJXZQ", AliasName = "县级行政区")]
    public class VEC_XJXZQ
    {
        #region Propertys

        /// <summary>
        /// 标识码(M)
        /// </summary>
        [DataColumn("BSM", AliasName = "标识码")]
        public virtual int BSM { get; set; }

        /// <summary>
        /// 要素代码(M)(eFeatureType)
        /// </summary>
        [DataColumn("YSDM", AliasName = "要素代码")]
        public virtual string YSDM { get; set; }

        /// <summary>
        /// 行政区代码(M)
        /// </summary>
        [DataColumn("XZQDM", AliasName = "行政区代码")]
        public virtual string XZQDM { get; set; }

        /// <summary>
        ///行政区名称(M)
        /// </summary>
        [DataColumn("XZQMC", AliasName = "行政区名称")]
        public virtual string XZQMC { get; set; }

        /// <summary>
        /// 图形
        /// </summary>
        [DataColumn("Shape", AliasName = "几何图形")]
        public virtual Geometry Shape { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public VEC_XJXZQ()
        {
            YSDM = "162010";
        }

        #endregion
    }
}
