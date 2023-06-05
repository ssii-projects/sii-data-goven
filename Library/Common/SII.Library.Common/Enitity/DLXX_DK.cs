/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using System;
using Agro.LibCore;
using Agro.LibCore.Database;
using GeoAPI.Geometries;

namespace Agro.Library.Model
{
	/// <summary>
	/// 地块
	/// </summary>
	[Serializable]
    [DataTable("DLXX_DK", AliasName = "地块")]
    public class DLXX_DK : Entity<DLXX_DK>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 标识码(M)
		/// </summary>
		[DataColumn("BSM", AliasName = "标识码", Insertable = false, Updatable = false)]
        public  int BSM { get; set; }

		/// <summary>
		/// 要素代码(M)(eFeatureType)
		/// </summary>
		[DataColumn("YSDM", AliasName = "要素代码")]
		public  string YSDM { get; set; } = "211011";

		/// <summary>
		/// 地块代码(M)
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块代码")]
		public  string DKBM { get; set; }

		/// <summary>
		/// 地块名称(M)
		/// </summary>
		[DataColumn("DKMC", AliasName = "地块名称")]
		public  string DKMC { get; set; }

		/// <summary>
		/// 所有权性质(O)(eSYQXZ)
		/// </summary>
		[DataColumn("SYQXZ", AliasName = "所有权性质")]
		public  string SYQXZ { get; set; }

		/// <summary>
		/// 地块类别(M)(eDKLB)
		/// </summary>
		[DataColumn("DKLB", AliasName = "地块类别")]
		public  string DKLB { get; set; }

		/// <summary>
		/// 土地利用类型(O)
		/// </summary>
		[DataColumn("TDLYLX", AliasName = "土地利用类型")]
		public  string TDLYLX { get; set; }

		/// <summary>
		/// 地力等级(M)(eDLDJ)
		/// </summary>
		[DataColumn("DLDJ", AliasName = "地力等级")]
		public  string DLDJ { get; set; }

		/// <summary>
		/// 土地用途(M)(eTDYT)
		/// </summary>
		[DataColumn("TDYT", AliasName = "土地用途")]
		public  string TDYT { get; set; }

		/// <summary>
		/// 地块东至(O)
		/// </summary>
		[DataColumn("DKDZ", AliasName = "地块东至")]
		public  string DKDZ { get; set; }

		/// <summary>
		/// 地块西至(O)
		/// </summary>
		[DataColumn("DKXZ", AliasName = "地块西至")]
		public  string DKXZ { get; set; }

		/// <summary>
		/// 地块南至(O)
		/// </summary>
		[DataColumn("DKNZ", AliasName = "地块南至")]
		public  string DKNZ { get; set; }

		/// <summary>
		/// 地块北至(O)
		/// </summary>
		[DataColumn("DKBZ", AliasName = "地块北至")]
		public  string DKBZ { get; set; }

		/// <summary>
		/// 地块备注信息
		/// </summary>
		[DataColumn("DKBZXX", AliasName = "地块备注信息")]
		public  string DKBZXX { get; set; }

		/// <summary>
		/// 指界人姓名
		/// </summary>
		[DataColumn("ZJRXM", AliasName = "指界人姓名")]
		public  string ZJRXM { get; set; }

		/// <summary>
		/// 实测面积(M)
		/// </summary>
		[DataColumn("SCMJ", AliasName = "实测面积")]
        public  decimal SCMJ { get; set; }

        /// <summary>
        /// 实测面积亩
        /// </summary>
        [DataColumn("SCMJM", AliasName = "实测面积(亩)")]
        public  decimal? SCMJM { get; set; }

        /// <summary>
        /// 空间坐标
        /// </summary>
        [DataColumn("KJZB", AliasName = "空间坐标")]
        public  string KJZB { get; set; }

        /// <summary>
        /// 图形
        /// </summary>
        [DataColumn("Shape", GeometryType = eGeometryType.eGeometryPolygon)]
        public  IGeometry Shape { get; set; }

		[DataColumn("SFJBNT", AliasName = "是否基本农田")]
		public  string SFJBNT { get; set; }

		[DataColumn(AliasName ="发包方编码")]
		public string FBFBM { get; set; }

		[DataColumn(AliasName ="原地块编码")]
		public string YDKBM { get; set; }

		[DataColumn(AliasName ="二轮合同面积")]
		public decimal? ELHTMJ { get; set; }

		[DataColumn(AliasName ="确权面积")]
		public decimal? QQMJ { get; set; }

		[DataColumn(Precision = 15, Scale = 4, AliasName = "机动地面积", Nullable = true)]
		public decimal? JDDMJ { get; set; }

		[DataColumn(AliasName ="是否确权确股")]
		public string SFQQQG { get; set; }

		[DataColumn(AliasName ="承包方名称")]
		public string CBFMC { get; set; }


		public EDKZT ZT { get; set; } =EDKZT.Youxiao;


		[DataColumn(AliasName = "登记状态",FieldType =eFieldType.eFieldTypeInteger)]
		public EDjzt DJZT { get; set; } = EDjzt.Wdj;

		[DataColumn(AliasName ="抵押状态")]
		public bool DYZT { get; set; }
		[DataColumn(AliasName ="异议状态")]
		public bool YYZT { get; set; }
		[DataColumn(AliasName ="流转状态")]
		public bool LZZT { get; set; }

		[DataColumn(AliasName = "创建时间")]
		public DateTime CJSJ { get; set; } = DateTime.Now;

		[DataColumn(AliasName ="登记时间")]
		public DateTime? DJSJ { get; set; }

		[DataColumn(AliasName ="灭失时间")]
		public DateTime? MSSJ { get; set; }

		[DataColumn(AliasName = "最后修改时间")]
		public DateTime? ZHXGSJ { get; set; }


		[DataColumn(AliasName ="数据来源")]
		public ESjly SJLY { get; set; }

		[DataColumn("ZYY", AliasName = "作业员", FieldType = LibCore.eFieldType.eFieldTypeString, Length = 50, Tag = Tag_VEC_SURVEY_DK.SQLite)]
		public string ZYY { get; set; }
	};
}