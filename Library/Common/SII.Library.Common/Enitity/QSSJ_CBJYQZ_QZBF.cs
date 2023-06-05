using System;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// QSSJ_CBJYQZ_QZBF
	/// </summary>
	[DataTable("QSSJ_CBJYQZ_QZBF", AliasName = "QSSJ_CBJYQZ_QZBF")]
	public class QSSJ_CBJYQZ_QZBF : Entity<QSSJ_CBJYQZ_QZBF>
	{
		/// <summary>
		/// 标识
		/// </summary>
		[DataColumn("ID", AliasName = "标识", Nullable = false, Length = 38)]
		public string ID { get; set; }

		/// <summary>
		/// 承包经营权证(登记簿)代码
		/// </summary>
		[DataColumn("CBJYQZBM", AliasName = "承包经营权证(登记簿)代码", Nullable = false, Length = 19)]
		public string CBJYQZBM { get; set; }

		/// <summary>
		/// 权证补发原因
		/// </summary>
		[DataColumn("QZBFYY", AliasName = "权证补发原因", Nullable = false, Length = 200)]
		public string QZBFYY { get; set; }

		/// <summary>
		/// 补发日期
		/// </summary>
		[DataColumn("BFRQ", AliasName = "补发日期", Nullable = false)]
		public DateTime BFRQ { get; set; }

		/// <summary>
		/// 权证补发领取日期
		/// </summary>
		[DataColumn("QZBFLQRQ", AliasName = "权证补发领取日期", Nullable = false)]
		public DateTime QZBFLQRQ { get; set; }

		/// <summary>
		/// 权证补发领取人姓名
		/// </summary>
		[DataColumn("QZBFLQRXM", AliasName = "权证补发领取人姓名", Nullable = false, Length = 50)]
		public string QZBFLQRXM { get; set; }

		/// <summary>
		/// 权证补发领取人证件类型
		/// </summary>
		[DataColumn("BFLQRZJLX", AliasName = "权证补发领取人证件类型", Nullable = false, Length = 1)]
		public string BFLQRZJLX { get; set; }

		/// <summary>
		/// 权证补发领取人证件号码
		/// </summary>
		[DataColumn("BFLQRZJHM", AliasName = "权证补发领取人证件号码", Nullable = false, Length = 20)]
		public string BFLQRZJHM { get; set; }
	}
}
