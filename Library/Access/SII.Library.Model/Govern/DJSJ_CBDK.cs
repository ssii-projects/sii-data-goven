// (C) 2015 公司版权所有，保留所有权利
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 登记_承包地块
    /// </summary>
    [Serializable]
    [DataTable("DJ_CBDKXX", AliasName = "登记承包地块")]
    public class DJ_CBDKXX : ATT_DKEXP
    {
        #region Properties

        /// <summary>
        /// 唯一标识
        /// </summary>
        [DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public Guid ID { get; set; }

        /// <summary>
        /// 是否基本农田(M)(eWhether)
        /// </summary>
        [DataColumn("SFJBNT", AliasName = "是否基本农田")]
        public bool SFJBNT { get; set; }

        /// <summary>
        /// 地块标识
        /// </summary>
        [DataColumn("DKID", AliasName = "地块标识")]
        public Guid DKID { get; set; }

        /// <summary>
        /// 登记簿标识
        /// </summary>
        [DataColumn("QLID", AliasName = "登记簿标识")]
        public Guid QLID { get; set; }

        /// <summary>
        /// 发包方编码
        /// </summary>
        [DataColumn("FBFBM", AliasName = "发包方编码")]
        public string FBFBM { get; set; }

        /// <summary>
        /// 承包方编码
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方编码")]
        public string CBFBM { get; set; }

        /// <summary>
        /// 承包经营权取得方式
        /// </summary>
        [DataColumn("CBJYQQDFS", AliasName = "承包经营权取得方式")]
        public string CBJYQQDFS { get; set; }

        /// <summary>
        /// 原合同面积
        /// </summary>
        [DataColumn("YHTMJ", AliasName = "原合同面积")]
        public double? YHTMJ { get; set; }

        /// <summary>
        /// 原合同面积亩
        /// </summary>
        [DataColumn("YHTMJM", AliasName = "原合同面积(亩)")]
        public double? YHTMJM { get; set; }

        /// <summary>
        /// 合同面积
        /// </summary>
        [DataColumn("HTMJ", AliasName = "合同面积")]
        public double? HTMJ { get; set; }

        /// <summary>
        /// 合同面积亩
        /// </summary>
        [DataColumn("HTMJM", AliasName = "合同面积(亩)")]
        public double? HTMJM { get; set; }

        /// <summary>
        /// 承包合同编码
        /// </summary>
        [DataColumn("CBHTBM", AliasName = "承包合同编码")]
        public string CBHTBM { get; set; }

        /// <summary>
        /// 流转合同编码
        /// </summary>
        [DataColumn("LZHTBM", AliasName = "流转合同编码")]
        public string LZHTBM { get; set; }

        /// <summary>
        /// 承包经营权证编码
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
        public string CBJYQZBM { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataColumn("BZ", AliasName = "备注")]
        public string BZ { get; set; }

        /// <summary>
        /// 是否确权确股
        /// </summary>
        [DataColumn("SFQQQG", AliasName = "是否确权确股")]
        public bool SFQQQG { get; set; }

        /// <summary>
        /// 原地块编码
        /// </summary>
        [DataColumn("YDKBM", AliasName = "原地块编码")]
        public string YDKBM { get; set; }

        /// <summary>
        /// 实测面积(M)
        /// </summary>
        [DataColumn("SCMJ", AliasName = "实测面积")]
        public virtual double SCMJ { get; set; }

        /// <summary>
        /// 二轮合同面积
        /// </summary>
        [DataColumn("ELHTMJ", AliasName = "二轮合同面积")]
        public double? ELHTMJ { get; set; }

        /// <summary>
        /// 确权面积
        /// </summary>
        [DataColumn("QQMJ", AliasName = "确权面积")]
        public double? QQMJ { get; set; }

        /// <summary>
        /// 机动地面积
        /// </summary>
        [DataColumn("JDDMJ", AliasName = "机动地面积")]
        public double? JDDMJ { get; set; }

        #endregion

        #region Ctor

        public DJ_CBDKXX()
        {
            ID = Guid.NewGuid();
            DKID = Guid.Empty;
            QLID = Guid.Empty;
        }

        #endregion
    }
}
