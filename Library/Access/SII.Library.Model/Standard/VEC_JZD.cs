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
    /// 界址点
    /// </summary>
    [Serializable]
    [DataTable("JZD", AliasName = "界址点")]
    public class VEC_JZD
    {
        #region Property

        /// <summary>
        /// 标识码（M）
        /// </summary>
        [DataColumn("BSM", AliasName = "标识码")]
        public virtual int BSM { get; set; }

        /// <summary>
        /// 要素代码(M)(eFeatureType)
        /// </summary>
        [DataColumn("YSDM", AliasName = "要素代码")]
        public virtual string YSDM { get; set; }

        /// <summary>
        /// 界址点号(M)
        /// </summary>
        [DataColumn("JZDH", AliasName = "界址点号")]
        public virtual string JZDH { get; set; }

        /// <summary>
        /// 界标类型(O)(eJBLX)
        /// </summary>
        [DataColumn("JBLX", AliasName = "界标类型")]
        public virtual string JBLX { get; set; }

        /// <summary>
        /// 界址点类型(O)(eJZDLX)
        /// </summary>
        [DataColumn("JZDLX", AliasName = "界址点类型")]
        public virtual string JZDLX { get; set; }

        /// <summary>
        /// 地块代码
        /// </summary>
        [DataColumn("DKBM", AliasName = "地块代码")]
        public virtual string DKBM { get; set; }

        /// <summary>
        /// X坐标值
        /// </summary>
        [DataColumn("XZBZ", AliasName = "X坐标值")]
        public virtual double XZBZ { get; set; }

        /// <summary>
        /// Y坐标值
        /// </summary>
        [DataColumn("YZBZ", AliasName = "Y坐标值")]
        public virtual double YZBZ { get; set; }

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
        public VEC_JZD()
        {
            YSDM = "211021";
        }

        #endregion
    }
}