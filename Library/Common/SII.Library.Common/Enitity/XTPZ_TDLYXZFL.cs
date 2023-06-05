using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// XTPZ_TDLYXZFL
	/// </summary>
	[DataTable("XTPZ_TDLYXZFL", AliasName = "系统配置-土地利用类型")]
	public class XTPZ_TDLYXZFL : Entity<XTPZ_TDLYXZFL>
	{
		/// <summary>
		/// 标识
		/// </summary>
		[DataColumn("ID", AliasName = "标识", Nullable = false, Length = 38)]
		public string ID { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[DataColumn("MC", AliasName = "名称", Nullable = false, Length = 15)]
		public string MC { get; set; }

		/// <summary>
		/// 编码
		/// </summary>
		[DataColumn("BM", AliasName = "编码", Nullable = false, Length = 3)]
		public string BM { get; set; }

		/// <summary>
		/// 上级编码
		/// </summary>
		[DataColumn("SJBM", AliasName = "上级编码", Length = 3)]
		public string SJBM { get; set; }

		/// <summary>
		/// 三大类
		/// </summary>
		[DataColumn("SDL", AliasName = "三大类", Length = 4)]
		public string SDL { get; set; }

		/// <summary>
		/// 含义
		/// </summary>
		[DataColumn("HY", AliasName = "含义", Length = 100)]
		public string HY { get; set; }
	}
}
