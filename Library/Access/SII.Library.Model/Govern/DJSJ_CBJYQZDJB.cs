// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;

namespace Agro.Library.Model
{
    /// <summary>
    /// 登记_承包经营权证登记簿
    /// </summary>
    [Serializable]
    [DataTable("DJ_CBJYQZDJB", AliasName = "承包经营权证登记簿")]
    public class DJ_CBJYQZDJB : ATT_CBJYQDJBEXP
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 发包方标识
        /// </summary>
        [DataColumn("FBFID", AliasName = "发包方标识")]
        public Guid FBFID { get; set; }

        /// <summary>
        /// 业务号
        /// </summary>
        [DataColumn("YWH", AliasName = "业务号")]
        public string YWH { get; set; }

        /// <summary>
        /// 权利类型
        /// </summary>
        [DataColumn("QLLX", AliasName = "权利类型")]
        public int QLLX { get; set; }

        /// <summary>
        /// 登记类型
        /// </summary>
        [DataColumn("DJLX", AliasName = "登记类型")]
        public int DJLX { get; set; }

        /// <summary>
        /// 登记原因
        /// </summary>
        [DataColumn("DJYY", AliasName = "登记原因")]
        public string DJYY { get; set; }

        /// <summary>
        /// 权属状态
        /// </summary>
        [DataColumn("QSZT", AliasName = "权属状态")]
        public int? QSZT { get; set; }

        /// <summary>
        /// 抵押状态
        /// </summary>
        [DataColumn("DYZT", AliasName = "抵押状态")]
        public int DYZT { get; set; }

        /// <summary>
        /// 异议状态
        /// </summary>
        [DataColumn("YYZT", AliasName = "异议状态")]
        public int YYZT { get; set; }

        /// <summary>
        /// 所在地域
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public Guid SZDY { get; set; }

        /// <summary>
        /// 附记
        /// </summary>
        [DataColumn("FJ", AliasName = "附记")]
        public string FJ { get; set; }

        /// <summary>
        /// 区县代码
        /// </summary>
        [DataColumn("QXDM", AliasName = "区县代码")]
        public string QXDM { get; set; }

        #endregion

        #region Ctor

        public DJ_CBJYQZDJB()
        {
            ID = Guid.NewGuid();
            FBFID = Guid.Empty;
            SZDY = Guid.Empty;
        }

        #endregion
    }
}
