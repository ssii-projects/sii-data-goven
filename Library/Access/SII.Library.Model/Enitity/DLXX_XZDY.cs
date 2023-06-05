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
    /// 行政地域
    /// </summary>
    [Serializable]
    [DataTable("DLXX_XZDY", AliasName = "行政地域")]
    public class DLXX_XZDY : Entity<DLXX_XZDY>
	{
		#region Properties

		/// <summary>
		/// 唯一标识
		/// </summary>
		[DataColumn("ID", PrimaryKey = true, Nullable = false)]
        public string ID { get; set; }

        /// <summary>
        /// 标识码
        /// </summary>
        [DataColumn("BSM", AliasName = "标识码", Auto = true)]
        public int ObjectId { get; set; }

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
        /// 简称
        /// </summary>
        [DataColumn("JC", AliasName = "简称")]
        public string JC { get; set; }

        /// <summary>
        /// 扩展名称
        /// </summary>
        [DataColumn("KZMC", AliasName = "扩展名称")]
        public string KZMC { get; set; }

        /// <summary>
        /// 扩展编码
        /// </summary>
        [DataColumn("KZBM", AliasName = "扩展编码")]
        public string KZBM { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        [DataColumn("JB", AliasName = "级别")]
        public int JB { get; set; }

        /// <summary>
        /// 上级标识
        /// </summary>
        [DataColumn("SJID", AliasName = "上级标识")]
        public string SJID { get; set; }

        /// <summary>
        /// 控制面积
        /// </summary>
        [DataColumn("KZMJ", AliasName = "控制面积")]
        public double KZMJ { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DataColumn("BZ", AliasName = "备注")]
        public string BZ { get; set; }

        /// <summary>
        /// 几何图形
        /// </summary>
        [DataColumn("SHAPE", AliasName = "几何图形")]
        public Geometry SHAPE { get; set; }

        #endregion

        #region Ctor

        public DLXX_XZDY()
        {
            ID = Guid.NewGuid().ToString();
            //SJID = Guid.Empty;
        }

        #endregion
    }
}
