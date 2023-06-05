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
    /// 承包经营权证
    /// </summary>
    [Serializable]
    [DataTable("CBJYQZ", AliasName = "承包经营权证")]
    public class ATT_CBJYQZEXP
    {
        #region Property

        /// <summary>
        /// 承包经营权 簿(证)编码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 发证机关(M)
        /// </summary>
        [DataColumn("FZJG", AliasName = "发证机关")]
        public virtual string FZJG { get; set; }

        /// <summary>
        /// 发证日期(M)
        /// </summary>
        [DataColumn("FZRQ", AliasName = "发证日期")]
        public virtual DateTime? FZRQ { get; set; }

        /// <summary>
        /// 权证领取日期(C)
        /// </summary>
        [DataColumn("QZLQRQ", AliasName = "权证领取日期")]
        public virtual DateTime? QZLQRQ { get; set; }

        /// <summary>
        /// 权证领取人姓名(C)
        /// </summary>
        [DataColumn("QZLQRXM", AliasName = "权证领取人姓名")]
        public virtual string QZLQRXM { get; set; }

        /// <summary>
        /// 权证领取人证件类型(C)
        /// </summary>
        [DataColumn("QZLQRZJLX", AliasName = "权证领取人证件类型")]
        public virtual string QZLQRZJLX { get; set; }

        /// <summary>
        /// 权证领取人证件号码(C)
        /// </summary>
        [DataColumn("QZLQRZJHM", AliasName = "权证领取人证件号码")]
        public virtual string QZLQRZJHM { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQZEXP()
        {
        }

        #endregion
    }
}