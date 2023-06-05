// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 登记_承包经营权证
    /// </summary>
    [Serializable]
    [DataTable("DJ_CBJYQZ", AliasName = "承包经营权证")]
    public class DJ_CBJYQZ : ATT_CBJYQZEXP
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 权证登记簿标识
        /// </summary>
        [DataColumn("QLID", AliasName = "登记簿标识")]
        public Guid QLID { get; set; }

        /// <summary>
        /// 权证是否领取(M)(eSF)
        /// </summary>
        [DataColumn("QZSFLQ", AliasName = "权证是否领取")]
        public bool QZSFLQ { get; set; }

        /// <summary>
        /// 证书版式
        /// </summary>
        [DataColumn("ZSBS", AliasName = "证书版式")]
        public int ZSBS { get; set; }

        /// <summary>
        /// 省区市的简称
        /// </summary>
        [DataColumn("SQSJC", AliasName = "省区市的简称")]
        public string SQSJC { get; set; }

        /// <summary>
        /// 颁证年份
        /// </summary>
        [DataColumn("BZNF", AliasName = "颁证年份")]
        public int BZNF { get; set; }

        /// <summary>
        /// 发证机关所在地名称
        /// </summary>
        [DataColumn("FZJGSZDMC", AliasName = "发证机关所在地名称")]
        public string FZJGSZDMC { get; set; }

        /// <summary>
        /// 年度顺序号
        /// </summary>
        [DataColumn("NDSXH", AliasName = "年度顺序号")]
        public int NDSXH { get; set; }

        /// <summary>
        /// 印制顺序号
        /// </summary>
        [DataColumn("YZSXH", AliasName = "印制顺序号")]
        public string YZSXH { get; set; }

        /// <summary>
        /// 打印次数
        /// </summary>
        [DataColumn("DYCS", AliasName = "打印次数")]
        public int DYCS { get; set; }

        /// <summary>
        /// 是否已注销
        /// </summary>
        [DataColumn("SFYZX", AliasName = "是否已注销")]
        public bool SFYZX { get; set; }

        /// <summary>
        /// 注销原因
        /// </summary>
        [DataColumn("ZXYY", AliasName = "注销原因")]
        public string ZXYY { get; set; }

        /// <summary>
        /// 注销日期
        /// </summary>
        [DataColumn("ZXRQ", AliasName = "注销日期")]
        public DateTime? ZXRQ { get; set; }

        #endregion

        #region Ctor

        public DJ_CBJYQZ()
        {
            ID = Guid.NewGuid();
        }

        #endregion
    }
}
