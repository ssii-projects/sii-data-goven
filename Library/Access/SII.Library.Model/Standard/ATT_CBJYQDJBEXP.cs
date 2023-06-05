/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 承包经营权证登记簿
    /// </summary>
    [Serializable]
    [DataTable("QSSJ_CBJYQZDJB", AliasName = "承包经营权证登记簿")]
    public class ATT_CBJYQDJBEXP
    {
        #region Propertys

        /// <summary>
        /// 承包经营权 簿(证)编码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 发包方代码(M)
        /// </summary>
        [DataColumn("FBFBM", AliasName = "发包方代码")]
        public virtual string FBFBM { get; set; }

        /// <summary>
        /// 承包方代码(M)
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方代码")]
        public virtual string CBFBM { get; set; }

        /// <summary>
        /// 承包方式(M)(eCBJYQQDFS)
        /// </summary>
        [DataColumn("CBFS", AliasName = "承包方式")]
        public virtual string CBFS { get; set; }

        /// <summary>
        /// 承包期限(M)
        /// </summary>
        [DataColumn("CBQX", AliasName = "承包期限")]
        public virtual string CBQX { get; set; }

        /// <summary>
        /// 承包期限起(M)
        /// </summary>
        [DataColumn("CBQXQ", AliasName = "承包期限起")]
        public virtual DateTime? CBQXQ { get; set; }

        /// <summary>
        /// 承包期限止(M)
        /// </summary>
        [DataColumn("CBQXZ", AliasName = "承包期限止")]
        public virtual DateTime? CBQXZ { get; set; }

        /// <summary>
        /// 地块示意图(M)
        /// </summary>
        [DataColumn("DKSYT", AliasName = "地块示意图")]
        public virtual string DKSYT { get; set; }

        /// <summary>
        /// 登簿人
        /// </summary>
        [DataColumn("DBR", AliasName = "登簿人")]
        public virtual string DBR { get; set; }

        /// <summary>
        /// 登记时间
        /// </summary>
        [DataColumn("DJSJ", AliasName = "登记时间")]
        public virtual DateTime? DJSJ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQDJBEXP()
        {
        }

        #endregion
    }
}
