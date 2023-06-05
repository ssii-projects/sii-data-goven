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
    /// 界址线
    /// </summary>
    [Serializable()]
    [DataTable("JZX", AliasName = "界址线")]
    public class VEC_JZX
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
        /// 界线性质(O)(eJXXZ)
        /// </summary>
        [DataColumn("JXXZ", AliasName = "界线性质")]
        public virtual string JXXZ { get; set; }

        /// <summary>
        /// 界址线类别(O)(eQSJXLB)
        /// </summary>
        [DataColumn("JZXLB", AliasName = "界址线类别")]
        public virtual string JZXLB { get; set; }

        /// <summary>
        /// 界址线位置(M)(eQSJXWZ)
        /// </summary>
        [DataColumn("JZXWZ", AliasName = "界址线位置")]
        public virtual string JZXWZ { get; set; }

        /// <summary>
        /// 界址线说明(M)
        /// </summary>
        [DataColumn("JZXSM", AliasName = "界址线说明")]
        public virtual string JZXSM { get; set; }

        /// <summary>
        /// 毗邻地物权利人(M)
        /// </summary>
        [DataColumn("PLDWQLR", AliasName = "毗邻地物权利人")]
        public virtual string PLDWQLR { get; set; }

        /// <summary>
        /// 毗邻地物指界人(M)
        /// </summary>
        [DataColumn("PLDWZJR", AliasName = "毗邻地物指界人")]
        public virtual string PLDWZJR { get; set; }

        /// <summary>
        /// 界址线号
        /// </summary>
        [DataColumn("JZXH", AliasName = "界址线号")]
        public virtual string JZXH { get; set; }

        /// <summary>
        /// 起界址点号
        /// </summary>
        [DataColumn("QJZDH", AliasName = "起界址点号")]
        public virtual string QJZDH { get; set; }

        /// <summary>
        /// 止界址点号
        /// </summary>
        [DataColumn("ZJZDH", AliasName = "止界址点号")]
        public virtual string ZJZDH { get; set; }

        /// <summary>
        /// 地块代码
        /// </summary>
        [DataColumn("DKBM", AliasName = "地块代码")]
        public virtual string DKBM { get; set; }

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
        public VEC_JZX()
        {
            YSDM = "211031";
        }

        #endregion
    }
}
