using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// XTPZ_SJZD
	/// </summary>
	[DataTable("XTPZ_SJZD", AliasName = "系统配置-数据字典")]
	public class XTPZ_SJZD : Entity<XTPZ_SJZD>
	{
		/// <summary>
		/// 标识
		/// </summary>
		[DataColumn("ID", AliasName = "标识", Nullable = false, Length = 38)]
		public string ID { get; set; }

		/// <summary>
		/// 名称
		/// </summary>
		[DataColumn("MC", AliasName = "名称", Nullable = false, Length = 50)]
		public string MC { get; set; }

		/// <summary>
		/// 编码
		/// </summary>
		[DataColumn("BM", AliasName = "编码", Nullable = false, Length = 20)]
		public string BM { get; set; }

		/// <summary>
		/// 级别
		/// </summary>
		[DataColumn("JB", AliasName = "级别")]
		public int? JB { get; set; }

		/// <summary>
		/// 上级标识
		/// </summary>
		[DataColumn("SJID", AliasName = "上级标识", Length = 38)]
		public string SJID { get; set; }

		/// <summary>
		/// 是否自定义
		/// </summary>
		[DataColumn("SFZDY", AliasName = "是否自定义", Nullable = false)]
		public bool SFZDY { get; set; }

		/// <summary>
		/// 自定义名称
		/// </summary>
		[DataColumn("ZDYMC", AliasName = "自定义名称", Length = 50)]
		public string ZDYMC { get; set; }

		/// <summary>
		/// 是否禁用
		/// </summary>
		[DataColumn("SFJY", AliasName = "是否禁用", Nullable = false)]
		public bool SFJY { get; set; }

		/// <summary>
		/// 序号
		/// </summary>
		[DataColumn("XH", AliasName = "序号")]
		public int? XH { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		[DataColumn("BZ", AliasName = "备注", Length = 300)]
		public string BZ { get; set; }
	}
}
