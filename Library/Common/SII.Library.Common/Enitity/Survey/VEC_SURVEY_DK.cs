using Agro.LibCore.Database;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Model
{

	/// <summary>
	/// 外部实体类（用于ShapeFile或外部Sqlite数据库）
	/// </summary>
	[DataTable(AliasName ="调查地块")]
	public class VEC_SURVEY_DK: Entity<VEC_SURVEY_DK>
	{
		[DataColumn(Unique =true,Insertable =false,Updatable =false)]
		public int ObjectID { get; set; }

		[DataColumn(Length =19,AliasName ="地块编码")]
		public string DKBM { get; set; }

		[DataColumn(Length = 50,AliasName ="地块名称")]
		public string DKMC { get; set; }

		[DataColumn(Length = 2,AliasName ="地块类别")]
		public string DKLB { get; set; }

		[DataColumn(Length = 3,AliasName ="土地利用类型")]
		public string TDLYLX { get; set; }

		[DataColumn(Length = 2,AliasName ="地力等级")]
		public string DLDJ { get; set; }

		[DataColumn(Length = 1,AliasName ="土地用途")]
		public string TDYT { get; set; }

		[DataColumn(Length = 1,AliasName ="是否基本农田")]
		public string SFJBNT { get; set; }

		[DataColumn(Length = 50,AliasName ="地块东至")]
		public string DKDZ { get; set; }

		[DataColumn(Length = 50,AliasName ="地块西至")]
		public string DKXZ { get; set; }

		[DataColumn(Length = 50,AliasName ="地块南至")]
		public string DKNZ { get; set; }

		[DataColumn(Length = 50,AliasName ="地块备注")]
		public string DKBZ { get; set; }

		[DataColumn(Length = 254,AliasName ="地块备注信息")]
		public string DKBZXX { get; set; }

		[DataColumn(Length = 50,AliasName ="承包方名称")]
		public string CBFMC { get; set; }

		[DataColumn(Precision = 15, Scale = 2,AliasName ="实测面积")]
		public decimal? SCMJ { get; set; }

		[DataColumn(Precision = 15, Scale = 2,AliasName ="实测面积亩")]
		public decimal? SCMJM { get; set; }

		[DataColumn(Length = 14,AliasName ="发包方编码")]
		public string FBFDM { get; set; }

		[DataColumn(Length = 2,AliasName ="所有权性质")]
		public string SYQXZ { get; set; }

		[DataColumn(Length = 100,AliasName ="指界人姓名")]
		public string ZJRXM { get; set; }

		[DataColumn(Length = 10,AliasName ="变更类型")]
		public string BGLX { get; set; }//", 10, -1);

		[DataColumn(Length = 254,AliasName ="变更原因")]
		public string BGYY { get; set; }//", 254, -1);

		[DataColumn(Length = 30,AliasName = "调查编码")]
		public string DCBM { get; set; }//", 30, -1);

		[DataColumn(Length = 2,AliasName ="上传标志")]
		public string SCBZ { get; set; }//", 2, -1);

		[DataColumn(GeometryType =LibCore.eGeometryType.eGeometryPolygon)]
		public IGeometry Shape { get; set; }
	}
}
