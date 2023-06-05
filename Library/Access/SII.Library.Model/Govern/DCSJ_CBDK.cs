// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Library.Model
{
    /// <summary>
    /// 调查_承包地块
    /// </summary>
    [Serializable]
    [DataTable("DC_CBDK", AliasName = "调查地块")]
    public class DC_CBDK : CDObject
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 合同标识
        /// </summary>
        [DataColumn("HTID", AliasName = "合同标识")]
        public Guid HTID { get; set; }

        /// <summary>
        /// 地块标识
        /// </summary>
        [DataColumn("DKID", AliasName = "地块标识")]
        public Guid DKID { get; set; }

        /// <summary>
        /// 所在地域
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public string SZDY { get; set; }

        /// <summary>
        /// 合同面积
        /// </summary>
        [DataColumn("HTMJ", AliasName = "合同面积")]
        public double HTMJ { get; set; }

        /// <summary>
        /// 合同面积(亩)
        /// </summary>
        [DataColumn("HTMJM", AliasName = "合同面积(亩)")]
        public double? HTMJM { get; set; }

        /// <summary>
        /// 原合同面积
        /// </summary>
        [DataColumn("YHTMJ", AliasName = "原合同面积")]
        public double? YHTMJ { get; set; }

        /// <summary>
        /// 原合同面积(亩)
        /// </summary>
        [DataColumn("YHTMJM", AliasName = "原合同面积(亩)")]
        public double? YHTMJM { get; set; }

        /// <summary>
        /// 确权确股
        /// </summary>
        [DataColumn("SFQQQG", AliasName = "确权确股")]
        public bool SFQQQG { get; set; }

        #endregion

        #region Ctor

        public DC_CBDK()
        {
            ID = Guid.NewGuid();
            SFQQQG = false;
        }

        #endregion
    }
}
