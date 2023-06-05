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
    [DataTable("CBJYQZDJB", AliasName = "承包经营权证登记簿")]
    public class ATT_CBJYQZDJB : ATT_CBJYQDJBEXP
    {
        #region Propertys

        /// <summary>
        /// 原经营权证编号 (0)
        /// </summary>
        [DataColumn("YCBJYQZBH", AliasName = "原经营权证编号")]
        public virtual string YCBJYQZBH { get; set; }

        /// <summary>
        /// 承包经营权证流水号（M）
        /// </summary>
        [DataColumn("CBJYQZLSH", AliasName = "承包经营权证流水号")]
        public virtual string CBJYQZLSH { get; set; }

        /// <summary>
        /// 登记簿附记
        /// </summary>
        [DataColumn("DJBFJ", AliasName = "登记簿附记")]
        public virtual string DJBFJ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBJYQZDJB()
        {
        }

        #endregion
    }
}
