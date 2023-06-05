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
    /// 注记
    /// </summary>
    [Serializable]
    [DataTable("ZJ", AliasName = "注记")]
    public class VEC_ZJ
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
        /// 注记内容(M)
        /// </summary>
        [DataColumn("ZJNR", AliasName = "注记内容")]
        public virtual string ZJNR { get; set; }

        /// <summary>
        /// 字体(M)
        /// </summary>
        [DataColumn("ZT", AliasName = "字体")]
        public virtual string ZT { get; set; }

        /// <summary>
        /// 颜色(M)
        /// </summary>
        [DataColumn("YS", AliasName = "颜色")]
        public virtual string YS { get; set; }

        /// <summary>
        /// 磅数(O)
        /// </summary>
        [DataColumn("BS", AliasName = "磅数")]
        public virtual int BS { get; set; }

        /// <summary>
        /// 形状(O)
        /// </summary>
        [DataColumn("XZ", AliasName = "形状")]
        public virtual string XZ { get; set; }

        /// <summary>
        /// 下划线(O)
        /// </summary>
        [DataColumn("XHX", AliasName = "下划线")]
        public virtual string XHX { get; set; }

        /// <summary>
        /// 宽度(O)
        /// </summary>
        [DataColumn("KD", AliasName = "宽度")]
        public virtual double KD { get; set; }

        /// <summary>
        /// 高度(O)
        /// </summary>
        [DataColumn("GD", AliasName = "高度")]
        public virtual double GD { get; set; }

        /// <summary>
        /// 间隔(O)
        /// </summary>
        [DataColumn("JG", AliasName = "间隔")]
        public virtual double JG { get; set; }

        /// <summary>
        /// 注记点左下角X坐标(M)
        /// </summary>
        [DataColumn("ZJDZXJXZB", AliasName = "注记点左下角X坐标")]
        public virtual double ZJDZXJXZB { get; set; }

        /// <summary>
        /// 注记点左下角Y坐标(M)
        /// </summary>
        [DataColumn("ZJDZXJYZB", AliasName = "注记点左下角Y坐标")]
        public virtual double ZJDZXJYZB { get; set; }

        /// <summary>
        /// 注记方向 (M)
        /// </summary>
        [DataColumn("ZJFX", AliasName = "注记方向")]
        public virtual double ZJFX { get; set; }

        /// <summary>
        /// SHAPE 
        /// </summary>
        [DataColumn("Shape", AliasName = "几何图形")]
        public virtual Geometry Shape { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public VEC_ZJ()
        {
            YSDM = "211012";
        }

        #endregion

    }
}