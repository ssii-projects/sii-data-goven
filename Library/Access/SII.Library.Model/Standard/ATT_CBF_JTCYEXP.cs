/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;


namespace Agro.Library.Model
{
    /// <summary>
    /// 家庭成员
    /// </summary>
    [Serializable]
    [DataTable("CBF_JTCY", AliasName = "家庭成员")]
    public class ATT_CBF_JTCYEXP
    {
        #region Propertys

        /// <summary>
        /// 承包方代码(M)
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方代码")]
        public virtual string CBFBM { get; set; }

        /// <summary>
        /// 成员姓名(M)
        /// </summary>
        [DataColumn("CYXM", AliasName = "成员姓名")]
        public virtual string CYXM { get; set; }

        /// <summary>
        /// 成员性别(M)(eSEX)
        /// </summary>
        [DataColumn("CYXB", AliasName = "成员性别")]
        public virtual string CYXB { get; set; }

        /// <summary>
        /// 成员证件类型(M)(eZJLX)
        /// </summary>
        [DataColumn("CYZJLX", AliasName = "成员证件类型")]
        public virtual string CYZJLX { get; set; }

        /// <summary>
        /// 成员证件号码(M)
        /// </summary>
        [DataColumn("CYZJHM", AliasName = "成员证件号码")]
        public virtual string CYZJHM { get; set; }

        /// <summary>
        /// 与户主关系(M)(eRelationShip)
        /// </summary>
        [DataColumn("YHZGX", AliasName = "与户主关系")]
        public virtual string YHZGX { get; set; }

        /// <summary>
        /// 成员备注(O)(eComment)
        /// </summary>
        [DataColumn("CYBZ", AliasName = "成员备注")]
        public virtual string CYBZ { get; set; }

        /// <summary>
        /// 成员备注说明
        /// </summary>
        [DataColumn("CYBZSM", AliasName = "成员备注说明")]
        public virtual string CYBZSM { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBF_JTCYEXP()
        {
        }

        #endregion

    }
}
