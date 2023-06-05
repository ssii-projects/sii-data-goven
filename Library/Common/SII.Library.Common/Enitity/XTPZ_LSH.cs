using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	[DataTable(AliasName ="系统配置-流水号")]
	public class XTPZ_LSH:Entity<XTPZ_LSH>
	{
		[DataColumn(Unique = true,Length =38)]
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 类型，默认"CONTRACTLAND"（表示承包经营权）
		/// </summary>
		[DataColumn(AliasName ="类型",Length =100)]
		public string LX { get; set; } = "CONTRACTLAND";

		[DataColumn(AliasName = "分组码", Length =100)]
		public string FZM { get; set; }

		[DataColumn(AliasName = "当前流水号")]
		public int BH { get; set; }
	}
}
