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
    /// 控制点
    /// </summary>
    [Serializable]
    [DataTable("KZD", AliasName = "控制点")]
    public class VEC_KZD
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
        /// 控制点名称(O)
        /// </summary>
        [DataColumn("KZDMC", AliasName = "控制点名称")]
        public virtual string KZDMC { get; set; }

        /// <summary>
        /// 控制点点号(O)
        /// </summary>
        [DataColumn("KZDDH", AliasName = "控制点点号")]
        public virtual string KZDDH { get; set; }

        /// <summary>
        /// 控制点类型(M)(eKZDLX)
        /// </summary>
        [DataColumn("KZDLX", AliasName = "控制点类型")]
        public virtual string KZDLX { get; set; }

        /// <summary>
        /// 控制点等级(M)(eKZDLX)
        /// </summary>
        [DataColumn("KZDDJ", AliasName = "控制点等级")]
        public virtual string KZDDJ { get; set; }

        /// <summary>
        /// 标石类型(M)(eBSLX)
        /// </summary>
        [DataColumn("BSLX", AliasName = "标石类型")]
        public virtual string BSLX { get; set; }

        /// <summary>
        /// 标志类型(M)(eBZLX)
        /// </summary>
        [DataColumn("BZLX", AliasName = "标志类型")]
        public virtual string BZLX { get; set; }

        /// <summary>
        /// 控制点状态(O)
        /// </summary>
        [DataColumn("KZDZT", AliasName = "控制点状态")]
        public virtual string KZDZT { get; set; }

        /// <summary>
        /// 点之记(M)
        /// </summary>
        [DataColumn("DZJ", AliasName = "点之记")]
        public virtual string DZJ { get; set; }

        /// <summary>
        /// X80(C)
        /// </summary>
        [DataColumn("X80", AliasName = "X80")]
        public virtual double? X80 { get; set; }

        /// <summary>
        /// Y80(C)
        /// </summary>
        [DataColumn("Y80", AliasName = "Y80")]
        public virtual double? Y80 { get; set; }

        /// <summary>
        /// X_2000(M)
        /// </summary>
        [DataColumn("X2000", AliasName = "X_2000")]
        public virtual double X2000 { get; set; }

        /// <summary>
        /// Y_2000(M)
        /// </summary>
        [DataColumn("Y2000", AliasName = "Y_2000")]
        public virtual double Y2000 { get; set; }

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
        public VEC_KZD()
        {
            YSDM = "111000";
        }

        #endregion

    }
}