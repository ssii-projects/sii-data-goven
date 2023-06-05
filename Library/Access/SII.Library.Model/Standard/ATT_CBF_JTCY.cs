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
    [DataTable("QSSJ_CBF_JTCY", AliasName = "家庭成员")]
    public class ATT_CBF_JTCY : ATT_CBF_JTCYEXP
    {
        #region Propertys

        /// <summary>
        /// 是否共有人(O)(eWhether)
        /// </summary>
        [DataColumn("SFGYR", AliasName = "是否共有人")]
        public virtual string SFGYR { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBF_JTCY()
        {
        }

        #endregion

    }
}
