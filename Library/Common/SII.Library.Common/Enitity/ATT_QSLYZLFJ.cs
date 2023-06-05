/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Agro.Library.Model
{
    /// <summary>
    /// 权属来源资料附件
    /// </summary>
    [Serializable]
    [DataTable("QSLYZLFJ", AliasName = "权属来源资料附件")]
    public class ATT_QSLYZLFJ
    {
        #region Property

        /// <summary>
        /// 承包经营权簿(证)编码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 资料附件编号(M)
        /// </summary>
        [DataColumn("ZLFJBH", AliasName = "资料附件编号")]
        public virtual string ZLFJBH { get; set; }

        /// <summary>
        /// 资料附件日期(M)
        /// </summary>
        [DataColumn("ZLFJRQ", AliasName = "资料附件日期")]
        public virtual DateTime? ZLFJRQ { get; set; }

        /// <summary>
        /// 资料附件名称(M)
        /// </summary>
        [DataColumn("ZLFJMC", AliasName = "资料附件名称")]
        public virtual string ZLFJMC { get; set; }

        /// <summary>
        /// 附件 
        /// </summary>
        [DataColumn("FJ", AliasName = "附件")]
        public virtual string FJ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_QSLYZLFJ()
        {
        }

        #endregion
    }
}