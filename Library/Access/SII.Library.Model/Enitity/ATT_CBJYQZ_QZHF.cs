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
    /// 权证换发
    /// </summary>
    [Serializable]
    [DataTable("CBJYQZ_QZHF", AliasName = "权证换发")]
    public class ATT_CBJYQZ_QZHF
    {
        #region Property

        /// <summary>
        /// 承包经营权 簿(证)编码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 权证换发原因(M)
        /// </summary>
        [DataColumn("QZHFYY", AliasName = "权证换发原因")]
        public virtual string QZHFYY { get; set; }

        /// <summary>
        /// 换发日期(M)
        /// </summary>
        [DataColumn("HFRQ", AliasName = "换发日期")]
        public virtual DateTime? HFRQ { get; set; }

        /// <summary>
        /// 权证换发领取日期(M)
        /// </summary>
        [DataColumn("QZHFLQRQ", AliasName = "权证换发领取日期")]
        public virtual DateTime? QZHFLQRQ { get; set; }

        /// <summary>
        /// 权证换发领取人姓名(M)
        /// </summary>
        [DataColumn("QZHFLQRXM", AliasName = "权证换发领取人姓名")]
        public virtual string QZHFLQRXM { get; set; }

        /// <summary>
        /// 权证换发领取人证件类型(M)
        /// </summary>
        [DataColumn("HFLQRZJLX", AliasName = "权证换发领取人证件类型")]
        public virtual string HFLQRZJLX { get; set; }

        /// <summary>
        /// 权证换发领取人证件号码(M)
        /// </summary>
        [DataColumn("HFLQRZJHM", AliasName = "权证换发领取人证件号码")]
        public virtual string HFLQRZJHM { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQZ_QZHF()
        {
        }

        #endregion
    }
}