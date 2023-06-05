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
    [DataTable("QSSJ_CBJYQZ", AliasName = "承包经营权证")]
    public class ATT_CBJYQZ : ATT_CBJYQZEXP
    {
        #region Property

        /// <summary>
        /// 权证是否领取(M)(eSF)
        /// </summary>
        [DataColumn("QZSFLQ", AliasName = "权证是否领取")]
        public virtual string QZSFLQ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQZ()
        {
        }

        #endregion
    }
}