using Agro.LibCore.Database;
using GeoAPI.Geometries;
using System;

namespace Agro.Library.Model
{
	public class SURVEY_DKBase<T> : FeatureEntity<T> where T : class, new()
	{
        [DataColumn(Length = 19, AliasName = "地块编码")]
        public string DKBM { get; set; }

        [DataColumn(Length = 50, AliasName = "地块名称", Nullable = false)]
        public string DKMC { get; set; }

        [DataColumn(Length = 2, AliasName = "地块类别", Nullable = false, CodeType = CodeType.DKLB)]
        public string DKLB { get; set; }

        [DataColumn(Length = 3, AliasName = "土地利用类型", CodeType = CodeType.TDLYLX)]
        public string TDLYLX { get; set; }

        [DataColumn(Length = 2, AliasName = "地力等级", Nullable = true, CodeType = CodeType.DLDJ)]//2020-7-13 Nullable false to true 
        public string DLDJ { get; set; }

        [DataColumn(Length = 1, AliasName = "土地用途", Nullable = false, CodeType = CodeType.TDYT)]
        public string TDYT { get; set; }

        [DataColumn(Length = 1, AliasName = "是否基本农田", Nullable = true, CodeType = CodeType.SFJBNT)]//2020-7-13 Nullable false to true 
        public string SFJBNT { get; set; }

        [DataColumn(Length = 50, AliasName = "地块东至")]
        public string DKDZ { get; set; }

        [DataColumn(Length = 50, AliasName = "地块西至")]
        public string DKXZ { get; set; }

        [DataColumn(Length = 50, AliasName = "地块南至")]
        public string DKNZ { get; set; }

        [DataColumn(Length = 50, AliasName = "地块北至")]
        public string DKBZ { get; set; }

        [DataColumn(Length = 254, AliasName = "地块备注信息")]
        public string DKBZXX { get; set; }

        [DataColumn(Length = 50, AliasName = "承包方名称")]
        public string CBFMC { get; set; }

        [DataColumn(Precision = 15, Scale = 4, AliasName = "实测面积", Nullable = false)]
        public decimal SCMJ { get; set; }

        [DataColumn(Precision = 15, Scale = 4, AliasName = "实测面积亩", Nullable = true)]
        public decimal? SCMJM { get; set; }

        [DataColumn(Precision = 15, Scale = 4, AliasName = "二轮合同面积亩", Nullable = true)]
        public decimal? ELHTMJ { get; set; }

        /// <summary>
        /// 其它面积
        /// </summary>
        [DataColumn(Precision = 15, Scale = 4, AliasName = "其它面积", Nullable = true)]
        public decimal? JDDMJ { get; set; }


        [DataColumn(Length = 14, AliasName = "发包方编码", Nullable = false)]
        public string FBFDM { get; set; }

        [DataColumn(Length = 2, AliasName = "所有权性质", CodeType = CodeType.SYQXZ)]
        public string SYQXZ { get; set; }

        [DataColumn(Length = 100, AliasName = "指界人姓名")]
        public string ZJRXM { get; set; }

        [DataColumn(Length = 10, AliasName = "变更类型")]
        public string BGLX { get; set; }

        [DataColumn(Length = 254, AliasName = "变更原因")]
        public string BGYY { get; set; }

        [DataColumn(Length = 30, AliasName = "调查编码")]
        public string DCBM { get; set; }

        [DataColumn(Length = 18, AliasName = "承包方编码")]
        public string CBFBM { get; set; }

        /// <summary>
        /// "1"：表示已上传
        /// </summary>
        [DataColumn(Length = 2, AliasName = "上传标志")]
        public string SCBZ { get; set; } = "0";

        /// <summary>
        /// 修改时间
        /// </summary>
        [DataColumn("XGSJ", AliasName = "修改时间", FieldType = LibCore.eFieldType.eFieldTypeDateTime, Tag = Tag_VEC_SURVEY_DK.SQLite)]
        public DateTime? XGSJ { get; set; } = DateTime.Now;
    }


    public class VEC_SURVEY_DKBase<T> : SURVEY_DKBase<T> where T:class, new()
	{

        [DataColumn(GeometryType = LibCore.eGeometryType.eGeometryPolygon)]
        public IGeometry Shape { get; set; }
    }


	public enum Tag_VEC_SURVEY_DK {
		SQLite
	}

	/// <summary>
	/// 外部实体类（用于ShapeFile或Sqlite数据库）
	/// </summary>
	[DataTable(AliasName = "调查地块")]
	public class VEC_SURVEY_DK : VEC_SURVEY_DKBase<VEC_SURVEY_DK>
	{
		#region 以下字段仅SQLite数据库中包含
		[DataColumn("ZYY", AliasName = "作业员", FieldType = LibCore.eFieldType.eFieldTypeString, Length = 50,Tag = Tag_VEC_SURVEY_DK.SQLite)]
		public string ZYY { get; set; }
		#endregion

		/// <summary>
		/// 不动产单元号：该字段为可选字段（可能存在也可能不存在）
		/// </summary>
		[DataColumn("BDCDYH", AliasName = "不动产单元号", Length = 28)]
		public string BDCDYH { get; set; }
	}

	///// <summary>
	///// 外部实体类（用于Sqlite数据库）
	///// </summary>
	//[DataTable("VEC_SURVEY_DK", AliasName = "调查地块")]
	//public class VEC_SURVEY_DK_SQLITE : VEC_SURVEY_DK//VEC_SURVEY_DKBase<VEC_SURVEY_DK_SQLITE>
	//{
	//	//[DataColumn("ZYY", AliasName = "作业员", FieldType = LibCore.eFieldType.eFieldTypeString, Length = 50)]
	//	//public string Zyy { get; set; }
	//}
}
