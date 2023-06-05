using System;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// QSSJ_QSLYZLFJ
	/// </summary>
	[DataTable("QSSJ_QSLYZLFJ", AliasName = "QSSJ_QSLYZLFJ")]
	public class QSSJ_QSLYZLFJ : Entity<QSSJ_QSLYZLFJ>
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
		/// 资料附件编号
		/// </summary>
		[DataColumn("ZLFJBH", AliasName = "资料附件编号", Nullable = false, Length = 20)]
		public string ZLFJBH { get; set; }

		/// <summary>
		/// 资料附件名称
		/// </summary>
		[DataColumn("ZLFJMC", AliasName = "资料附件名称", Nullable = false, Length = 100)]
		public string ZLFJMC { get; set; }

		/// <summary>
		/// 资料附件日期
		/// </summary>
		[DataColumn("ZLFJRQ", AliasName = "资料附件日期", Nullable = false)]
		public DateTime ZLFJRQ { get; set; }

		/// <summary>
		/// 附件
		/// </summary>
		[DataColumn("FJ", AliasName = "附件", Nullable = false, Length = 254)]
		public string FJ { get; set; }
	}
}
