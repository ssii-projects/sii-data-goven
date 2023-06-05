// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 数据字典
    /// </summary>
    [Serializable]
    [DataTable("XTPZ_SJZD", AliasName = "数据字典")]
    public class SJZD : CDObject
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public string ID { get; set; }

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

        /// <summary>
        /// 自定义名称
        /// </summary>
        [DataColumn("ZDYMC", AliasName = "自定义名称")]
        public string ZDYMC { get; set; }

        /// <summary>
        /// 是否自定义
        /// </summary>
        [DataColumn("SFZDY", AliasName = "是否自定义")]
        public bool SFZDY { get; set; }

        /// <summary>
        /// 分组编码
        /// </summary>
        [DataColumn("FZM", AliasName = "分组编码")]
        public string FZM { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        [DataColumn("FZMC", AliasName = "分组名称")]
        public string FZMC { get; set; }

        /// <summary>
        /// 标准类别
        /// </summary>
        [DataColumn("BZLB", AliasName = "标准类别")]
        public int BZLB { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        [DataColumn("SFJY", AliasName = "是否禁用")]
        public int SFJY { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataColumn("BZ", AliasName = "备注")]
        public string BZ { get; set; }

        #endregion

        #region Ctor

        public SJZD()
        {

        }

        #endregion
    }
}
