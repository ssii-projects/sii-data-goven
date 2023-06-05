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
    /// 承包方
    /// </summary>
    [Serializable]
    [DataTable("QSSJ_CBF", AliasName = "承包方")]
    public class ATT_CBF
    {
        #region Property

        /// <summary>
        /// 承包方代码(M)
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方代码")]
        public virtual string CBFBM { get; set; }

        /// <summary>
        /// 承包方类型(M)(eCBFLX)
        /// </summary>
        [DataColumn("CBFLX", AliasName = "承包方类型")]
        public virtual string CBFLX { get; set; }

        /// <summary>
        /// 承包方(代表)名称(M)
        /// </summary>
        [DataColumn("CBFMC", AliasName = "承包方(代表)名称")]
        public virtual string CBFMC { get; set; }

        /// <summary>
        /// 承包方(代表)证件类型(eZJLX)(M)
        /// </summary>
        [DataColumn("CBFZJLX", AliasName = "承包方(代表)证件类型")]
        public virtual string CBFZJLX { get; set; }

        /// <summary>
        /// 承包方(代表)证件号码(M)
        /// </summary>
        [DataColumn("CBFZJHM", AliasName = "承包方(代表)证件号码")]
        public virtual string CBFZJHM { get; set; }

        /// <summary>
        /// 承包方地址(M)
        /// </summary>
        [DataColumn("CBFDZ", AliasName = "承包方地址")]
        public virtual string CBFDZ { get; set; }

        /// <summary>
        /// 邮政编码(M)
        /// </summary>
        [DataColumn("YZBM", AliasName = "邮政编码")]
        public virtual string YZBM { get; set; }

        /// <summary>
        /// 联系电话(O)
        /// </summary>
        [DataColumn("LXDH", AliasName = "联系电话")]
        public virtual string LXDH { get; set; }

        /// <summary>
        /// 承包方成员数量(M)
        /// </summary>
        [DataColumn("CBFCYSL", AliasName = "承包方成员数量")]
        public virtual int CBFCYSL { get; set; }

        /// <summary>
        /// 承包方调查日期(M)
        /// </summary>
        [DataColumn("CBFDCRQ", AliasName = "承包方调查日期")]
        public virtual DateTime? CBFDCRQ { get; set; }

        /// <summary>
        /// 承包方调查员(M)
        /// </summary>
        [DataColumn("CBFDCY", AliasName = "承包方调查员")]
        public virtual string CBFDCY { get; set; }

        /// <summary>
        /// 承包方调查记事(C)
        /// </summary>
        [DataColumn("CBFDCJS", AliasName = "承包方调查记事")]
        public virtual string CBFDCJS { get; set; }

        /// <summary>
        /// 公示记事(C)
        /// </summary>
        [DataColumn("GSJS", AliasName = "公示记事")]
        public virtual string GSJS { get; set; }

        /// <summary>
        /// 公示记事人(M)
        /// </summary>
        [DataColumn("GSJSR", AliasName = "公示记事人")]
        public virtual string GSJSR { get; set; }

        /// <summary>
        /// 公示审核日期(M)
        /// </summary>
        [DataColumn("GSSHRQ", AliasName = "公示审核日期")]
        public virtual DateTime? GSSHRQ { get; set; }

        /// <summary>
        /// 公示审核人(M)
        /// </summary>
        [DataColumn("GSSHR", AliasName = "公示审核人")]
        public virtual string GSSHR { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBF()
        {
        }

        #endregion

    }
}