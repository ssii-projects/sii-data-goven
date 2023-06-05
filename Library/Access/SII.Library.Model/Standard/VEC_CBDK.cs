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
    /// 地块
    /// </summary>
    [Serializable]
    [DataTable("DLXX_DK", AliasName = "地块")]
    public class VEC_CBDK : ATT_DKEXP
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
        /// 实测面积(M)
        /// </summary>
        [DataColumn("SCMJ", AliasName = "实测面积")]
        public virtual double SCMJ { get; set; }

        /// <summary>
        /// 实测面积亩
        /// </summary>
        [DataColumn("SCMJM", AliasName = "实测面积(亩)")]
        public virtual double? SCMJM { get; set; }

        /// <summary>
        /// 空间坐标
        /// </summary>
        [DataColumn("KJZB", AliasName = "空间坐标")]
        public virtual string KJZB { get; set; }

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
        public VEC_CBDK()
        {
            YSDM = "211011";
        }

        #endregion

    };
}