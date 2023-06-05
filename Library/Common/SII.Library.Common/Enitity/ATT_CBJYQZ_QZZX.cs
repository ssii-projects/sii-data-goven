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
    /// 权证注销
    /// </summary>
    [Serializable]
    [DataTable("CBJYQZ_QZZX", AliasName = "权证注销")]
    public class ATT_CBJYQZ_QZZX
    {
        #region Property

        /// <summary>
        /// 承包经营权 簿(证)编码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 注销原因(M)
        /// </summary>
        [DataColumn("ZXYY", AliasName = "注销原因")]
        public virtual string ZXYY { get; set; }

        /// <summary>
        /// 注销日期(M)
        /// </summary>
        [DataColumn("ZXRQ", AliasName = "注销日期")]
        public virtual DateTime? ZXRQ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQZ_QZZX()
        {
        }

        #endregion

    }
}