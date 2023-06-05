using System;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// QSSJ_CBJYQZ_QZHF
	/// </summary>
	[DataTable("QSSJ_CBJYQZ_QZHF", AliasName = "QSSJ_CBJYQZ_QZHF")]
	public class QSSJ_CBJYQZ_QZHF : Entity<QSSJ_CBJYQZ_QZHF>
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
		/// 权证换发原因
		/// </summary>
		[DataColumn("QZHFYY", AliasName = "权证换发原因", Nullable = false, Length = 200)]
		public string QZHFYY { get; set; }

		/// <summary>
		/// 换发日期
		/// </summary>
		[DataColumn("HFRQ", AliasName = "换发日期", Nullable = false)]
		public DateTime HFRQ { get; set; }

		/// <summary>
		/// 权证换发领取日期
		/// </summary>
		[DataColumn("QZHFLQRQ", AliasName = "权证换发领取日期", Nullable = false)]
		public DateTime QZHFLQRQ { get; set; }

		/// <summary>
		/// 权证换发领取人姓名
		/// </summary>
		[DataColumn("QZHFLQRXM", AliasName = "权证换发领取人姓名", Nullable = false, Length = 50)]
		public string QZHFLQRXM { get; set; }

		/// <summary>
		/// 权证换发领取人证件类型
		/// </summary>
		[DataColumn("HFLQRZJLX", AliasName = "权证换发领取人证件类型", Nullable = false, Length = 1)]
		public string HFLQRZJLX { get; set; }

		/// <summary>
		/// 权证换发领取人证件号码
		/// </summary>
		[DataColumn("HFLQRZJHM", AliasName = "权证换发领取人证件号码", Nullable = false, Length = 20)]
		public string HFLQRZJHM { get; set; }
	}
}
