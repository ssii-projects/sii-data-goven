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
    /// 村级区域
    /// </summary>
    [Serializable]
    [DataTable("CJQY", AliasName = "村级区域")]
    public class VEC_CJQY
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
        /// 村级区域代码(M)
        /// </summary>
        [DataColumn("CJQYDM", AliasName = "村级区域代码")]
        public virtual string CJQYDM { get; set; }

        /// <summary>
        /// 村级区域名称(M)
        /// </summary>
        [DataColumn("CJQYMC", AliasName = "村级区域名称")]
        public virtual string CJQYMC { get; set; }

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
        public VEC_CJQY()
        {
            YSDM = "162030";
        }

        #endregion
    }
}
