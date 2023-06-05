using System;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// QSSJ_CBJYQZ_QZZX
	/// </summary>
	[DataTable("QSSJ_CBJYQZ_QZZX", AliasName = "QSSJ_CBJYQZ_QZZX")]
	public class QSSJ_CBJYQZ_QZZX : Entity<QSSJ_CBJYQZ_QZZX>
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
		/// 注销原因
		/// </summary>
		[DataColumn("ZXYY", AliasName = "注销原因", Nullable = false, Length = 200)]
		public string ZXYY { get; set; }

		/// <summary>
		/// 注销日期
		/// </summary>
		[DataColumn("ZXRQ", AliasName = "注销日期", Nullable = false)]
		public DateTime ZXRQ { get; set; }
	}
}
