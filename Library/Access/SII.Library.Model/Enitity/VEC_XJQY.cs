/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Agro.LibCore.Database;
using NetTopologySuite.Geometries;

namespace Agro.Library.Model
{
    /// <summary>
    /// 乡级区域
    /// </summary>
    [Serializable]
    [DataTable("XJQY", AliasName = "乡级区域")]
    public class VEC_XJQY
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
        /// 乡级行政区代码(M)
        /// </summary>
        [DataColumn("XJQYDM", AliasName = "乡级行政区代码")]
        public virtual string XJQYDM { get; set; }

        /// <summary>
        /// 乡级行政区名称(M)
        /// </summary>
        [DataColumn("XJQYMC", AliasName = "乡级行政区名称")]
        public virtual string XJQYMC { get; set; }

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
        public VEC_XJQY()
        {
            YSDM = "162020";
        }

        #endregion

    }
}
