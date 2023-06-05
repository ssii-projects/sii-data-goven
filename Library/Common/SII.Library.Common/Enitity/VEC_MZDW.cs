/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Agro.LibCore.Database;
using NetTopologySuite.Geometries;

namespace Agro.Library.Model
{
    /// <summary>
    /// 面状地物
    /// </summary>
    [Serializable]
    [DataTable("MZDW", AliasName = "面状地物")]
    public class VEC_MZDW
    {
        #region Property

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
        /// 地物名称(M)
        /// </summary>
        [DataColumn("DWMC", AliasName = "地物名称")]
        public virtual string DWMC { get; set; }

        /// <summary>
        /// 面积(O)
        /// </summary>
        [DataColumn("MJ", AliasName = "面积")]
        public virtual double? MJ { get; set; }

        /// <summary>
        /// 面积亩(O)
        /// </summary>
        [DataColumn("MJM", AliasName = "面积亩")]
        public virtual double? MJM { get; set; }

        /// <summary>
        /// 备注(O)
        /// </summary>
        [DataColumn("BZ", AliasName = "备注")]
        public virtual string BZ { get; set; }

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
        public VEC_MZDW()
        {
            YSDM = "196031";
        }

        #endregion
    }
}