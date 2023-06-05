// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 行政地域
    /// </summary>
    [Serializable]
    [DataTable("XZDY", AliasName = "行政地域")]
    public class ATT_XZDY : CDObject
    {
        #region Properties

        /// <summary>
        /// 名称
        /// </summary>
        [DataColumn("MC", AliasName = "名称")]
        public string MC { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        [DataColumn("BM", AliasName = "编码")]
        public string BM { get; set; }

        #endregion

        #region Ctor

        public ATT_XZDY()
        {
        }

        #endregion
    }
}
