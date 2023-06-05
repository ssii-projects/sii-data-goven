using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	public class EntityCBHTBase<T> : Entity<T> where T:class, new()
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// 承包合同代码(M)
		/// </summary>
		[DataColumn("CBHTBM", AliasName = "承包合同代码")]
		public string CBHTBM { get; set; }

		/// <summary>
		/// 原承包合同代码(C)
		/// </summary>
		[DataColumn("YCBHTBM", AliasName = "原承包合同代码")]
		public string YCBHTBM { get; set; }

		/// <summary>
		/// 发包方代码(M)
		/// </summary>
		[DataColumn("FBFBM", AliasName = "发包方代码")]
		public string FBFBM { get; set; }

		/// <summary>
		/// 承包方代码(M)
		/// </summary>
		[DataColumn("CBFBM", AliasName = "承包方代码")]
		public string CBFBM { get; set; }

		/// <summary>
		/// 承包方式(M)(eCBJYQQDFS)
		/// </summary>
		[DataColumn("CBFS", AliasName = "承包方式")]
		public string CBFS { get; set; }

		/// <summary>
		/// 承包期限起(M)
		/// </summary>
		[DataColumn("CBQXQ", AliasName = "承包期限起")]
		public DateTime? CBQXQ { get; set; }

		/// <summary>
		/// 承包期限止(M)
		/// </summary>
		[DataColumn("CBQXZ", AliasName = "承包期限止")]
		public DateTime? CBQXZ { get; set; }

		/// <summary>
		/// 承包确权(合同)总面积(M)
		/// </summary>
		[DataColumn("HTZMJ", AliasName = "承包确权(合同)总面积")]
		public decimal HTZMJ { get; set; }

		/// <summary>
		/// 承包地块总数 (M)
		/// </summary>
		[DataColumn("CBDKZS", AliasName = "承包地块总数")]
		public int CBDKZS { get; set; }

		/// <summary>
		/// 签订时间（M）
		/// </summary>
		[DataColumn("QDSJ", AliasName = "签订时间")]
		public DateTime? QDSJ { get; set; }
		/// <summary>
		/// 承包确权(合同)总面积亩(M)
		/// </summary>
		[DataColumn("HTZMJM", AliasName = "承包确权(合同)总面积亩")]
		public decimal? HTZMJM { get; set; }

		/// <summary>
		/// 原承包合同总面积(C)
		/// </summary>
		[DataColumn("YHTZMJ", AliasName = "原承包合同总面积")]
		public decimal? YHTZMJ { get; set; }

		/// <summary>
		/// 原承包合同总面积亩(C)
		/// </summary>
		[DataColumn("YHTZMJM", AliasName = "原承包合同总面积亩")]
		public decimal? YHTZMJM { get; set; }
	}

	[DataTable(AliasName ="登记-承包合同")]
	public class DJ_CBJYQ_CBHT:EntityCBHTBase<DJ_CBJYQ_CBHT>
	{
		public string DJBID { get; set; }
	}
}
