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
    /// 基本农田保护区
    /// </summary>
    [Serializable]
    [DataTable("JBNTBHQ", AliasName = "基本农田保护区")]
    public class VEC_JBNTBHQ
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
        /// 保护区编号(O)
        /// </summary>
        [DataColumn("BHQBH", AliasName = "保护区编号")]
        public virtual string BHQBH { get; set; }

        /// <summary>
        /// 基本农田面积(O)
        /// </summary>   
        [DataColumn("JBNTMJ", AliasName = "基本农田面积")]
        public virtual double? JBNTMJ { get; set; }

        /// <summary>
        /// 基本农田面积亩
        /// </summary>   
        [DataColumn("JBNTMJM", AliasName = "基本农田面积亩")]
        public virtual double? JBNTMJM { get; set; }

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
        public VEC_JBNTBHQ()
        {
            YSDM = "251100";
        }

        #endregion
    }
}
