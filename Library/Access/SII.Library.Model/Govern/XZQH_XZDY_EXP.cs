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
    [DataTable("XZQH_XZDY_EXP", AliasName = "行政地域扩展")]
    public class XZQH_XZDY_EXP : CDObject
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 上级级别
        /// </summary>
        [DataColumn("SJJB", AliasName = "上级级别")]
        public int SJJB { get; set; }

        /// <summary>
        /// 上级标识
        /// </summary>
        [DataColumn("SJID", AliasName = "上级标识")]
        public Guid SJID { get; set; }

        /// <summary>
        /// 上级编码
        /// </summary>
        [DataColumn("SJBM", AliasName = "上级编码")]
        public string SJBM { get; set; }

        /// <summary>
        /// 子级级别
        /// </summary>
        [DataColumn("ZJJB", AliasName = "子级级别")]
        public int ZJJB { get; set; }

        /// <summary>
        /// 子级标识
        /// </summary>
        [DataColumn("ZJID", AliasName = "子级标识")]
        public Guid ZJID { get; set; }

        /// <summary>
        /// 子级编码
        /// </summary>
        [DataColumn("ZJBM", AliasName = "子级编码")]
        public string ZJBM { get; set; }

        #endregion

        #region Ctor

        public XZQH_XZDY_EXP()
        {
            ID = Guid.NewGuid();
        }

        #endregion
    }
}
