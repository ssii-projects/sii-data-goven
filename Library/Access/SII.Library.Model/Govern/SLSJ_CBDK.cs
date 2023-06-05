// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Agro.LibCore.Database;
using NetTopologySuite.Geometries;

namespace Agro.Library.Model
{
    /// <summary>
    /// 地块
    /// </summary>
    [Serializable]
    [DataTable("DK", AliasName = "地块")]
    public class DK : VEC_CBDK
    {
        #region Properties

        /// <summary>
        /// 标识码(M)
        /// </summary>
        [DataColumn("BSM", AliasName = "标识码",Auto = true)]
        public override int BSM { get; set; }

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
        /// 所在地域(M)
        /// </summary>
        [DataColumn("SZDY", AliasName = "所在地域")]
        public Guid SZDY { get; set; }

        /// <summary>
        /// 二轮合同面积
        /// </summary>
        [DataColumn("ELHTMJ", AliasName = "二轮合同面积")]
        public double ELHTMJ { get; set; }

        /// <summary>
        /// 确权面积
        /// </summary>
        [DataColumn("QQMJ", AliasName = "确权面积")]
        public double QQMJ { get; set; }

        /// <summary>
        /// 确权确股
        /// </summary>
        [DataColumn("QQQG", AliasName = "确权确股")]
        public bool QQQG { get; set; }

        /// <summary>
        /// 是否基本农田(M)(eWhether)
        /// </summary>
        [DataColumn("SFJBNT", AliasName = "是否基本农田")]
        public bool SFJBNT { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [DataColumn("ZT", AliasName = "状态")]
        public bool ZT { get; set; }

        /// <summary>
        /// 登记状态
        /// </summary>
        [DataColumn("DJZT", AliasName = "登记状态")]
        public bool DJZT { get; set; }

        /// <summary>
        /// 抵押状态
        /// </summary>
        [DataColumn("DYZT", AliasName = "抵押状态")]
        public bool DYZT { get; set; }

        /// <summary>
        /// 异议状态
        /// </summary>
        [DataColumn("YYZT", AliasName = "异议状态")]
        public bool YYZT { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [DataColumn("ZHXGSJ", AliasName = "最后修改时间")]
        public DateTime? ZHXGSJ { get; set; }

        /// <summary>
        /// 承包方名称
        /// </summary>
        [DataColumn("CBFMC", AliasName = "承包方名称")]
        public string CBFMC { get; set; }

        /// <summary>
        /// 几何图形
        /// </summary>
        [DataColumn("SHAPE", AliasName = "几何图形")]
        public override Geometry Shape { get; set; }

        #endregion

        #region Ctor

        public DK()
        {
            ID = Guid.NewGuid();
            SZDY = Guid.Empty;
            YSDM = "211011";
            ZT = true;
        }

        #endregion
    }
}
