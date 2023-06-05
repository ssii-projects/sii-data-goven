using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	[DataTable(AliasName ="登记承包经营权登记簿")]
	public class DJ_CBJYQ_DJB : EntityCBJYQDJBBase<DJ_CBJYQ_DJB>
	{
		[DataColumn(AliasName = "登记簿附件")]
		public string FJ { get; set; }

		[DataColumn(AliasName = "权利类型")]
		public EQllx QLLX { get; set; } = EQllx.Cbjyq;

		[DataColumn(AliasName ="登记类型")]
		public int DJLX { get; set; }
		public string SZDY { get; set; }

		[DataColumn(AliasName ="登记原因")]
		public string DJYY { get; set; }

		[DataColumn(AliasName ="权属状态")]
		public EQszt QSZT { get; set; }

		[DataColumn(AliasName ="抵押状态")]
		public bool DYZT { get; set; }

		[DataColumn(AliasName = "异议状态")]
		public bool YYZT { get; set; }

		[DataColumn(AliasName = "区县代码")]
		public string QXDM { get; set; }

		[DataColumn(AliasName = "承包方名称")]
		public string CBFMC { get; set; }

		public string YCBJYQZBH { get; set; }


		[DataColumn(AliasName = "发包方名称")]
		public string FBFMC { get; set; }
		[DataColumn(AliasName = "发包方负责人姓名")]
		public string FBFFZRXM { get; set; }

		[DataColumn(AliasName ="是否已注销")]
		public bool SFYZX { get; set; }
	}
}
