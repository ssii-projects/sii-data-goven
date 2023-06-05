// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 调查_发包方
    /// </summary>
    [Serializable]
    [DataTable("FBF", AliasName = "发包方")]
    public class FBF : ATT_FBF
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 原编码
        /// </summary>
        [DataColumn("YBM", AliasName = "原编码")]
        public string YBM { get; set; }

        /// <summary>
        /// 所在地域
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public Guid SZDY { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        [DataColumn("SHR", AliasName = "审核人")]
        public string SHR { get; set; }

        /// <summary>
        /// 审核日期
        /// </summary>
        [DataColumn("SHRQ", AliasName = "审核日期")]
        public DateTime? SHRQ { get; set; }

        /// <summary>
        /// 审核意见
        /// </summary>
        [DataColumn("SHYJ", AliasName = "审核意见")]
        public string SHYJ { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataColumn("BZ", AliasName = "备注")]
        public string BZ { get; set; }

        #endregion

        #region Ctor

        public FBF()
        {
            ID = Guid.NewGuid();
            SZDY = Guid.Empty;
        }

        #endregion
    }
}
