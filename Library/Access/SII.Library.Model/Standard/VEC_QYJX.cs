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
    /// 区域界线
    /// </summary>
    [Serializable]
    [DataTable("QYJX", AliasName = "区域界线")]
    public class VEC_QYJX
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
        /// 界线类型(M)(eJXLX)
        /// </summary>
        [DataColumn("JXLX", AliasName = "界线类型")]
        public virtual string JXLX { get; set; }

        /// <summary>
        /// 界线性质(M)(eJXXZ)
        /// </summary>
        [DataColumn("JXXZ", AliasName = "界线性质")]
        public virtual string JXXZ { get; set; }

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
        public VEC_QYJX()
        {
            YSDM = "161051";
        }

        #endregion
    }
}
