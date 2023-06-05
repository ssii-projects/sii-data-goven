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
    /// 权证补发
    /// </summary>
    [Serializable]
    [DataTable("CBJYQZ_QZBF", AliasName = "权证补发")]
    public class ATT_CBJYQZ_QZBF
    {
        #region Property

        /// <summary>
        /// 承包经营权 簿(证)编码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 权证补发原因(M)
        /// </summary>
        [DataColumn("QZBFYY", AliasName = "权证补发原因")]
        public virtual string QZBFYY { get; set; }

        /// <summary>
        /// 补发日期(M)
        /// </summary>
        [DataColumn("BFRQ", AliasName = "补发日期")]
        public virtual DateTime? BFRQ { get; set; }

        /// <summary>
        /// 权证补发领取日期(M)
        /// </summary>
        [DataColumn("QZBFLQRQ", AliasName = "权证补发领取日期")]
        public virtual DateTime? QZBFLQRQ { get; set; }

        /// <summary>
        /// 权证补发领取人姓名(M)
        /// </summary>
        [DataColumn("QZBFLQRXM", AliasName = "权证补发领取人姓名")]
        public virtual string QZBFLQRXM { get; set; }

        /// <summary>
        /// 权证补发领取人证件类型(M)
        /// </summary>
        [DataColumn("BFLQRZJLX", AliasName = "权证补发领取人证件类型")]
        public virtual string BFLQRZJLX { get; set; }

        /// <summary>
        /// 权证补发领取人证件号码(M)
        /// </summary>
        [DataColumn("BFLQRZJHM", AliasName = "权证补发领取人证件号码")]
        public virtual string BFLQRZJHM { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQZ_QZBF()
        {
        }

        #endregion
    }
}