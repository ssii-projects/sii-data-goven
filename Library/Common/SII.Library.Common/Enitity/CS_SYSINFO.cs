using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	public class CS_SYSINFO:Entity<CS_SYSINFO>
	{
		/// <summary>
		/// ID
		/// </summary>
		[DataColumn("ID", AliasName = "ID", Nullable = false, Length = 38)]
		public string ID { get; set; }

		/// <summary>
		/// VALUE
		/// </summary>
		[DataColumn("VALUE", AliasName = "VALUE", Length = 250)]
		public string Value { get; set; }
	}
}
