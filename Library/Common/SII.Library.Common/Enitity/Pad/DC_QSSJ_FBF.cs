using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// QSSJ_FBF
	/// </summary>
	[DataTable("DC_QSSJ_FBF", AliasName = "QSSJ_FBF")]
	public class DC_QSSJ_FBF : Entity<DC_QSSJ_FBF>
	{
		/// <summary>
		/// FBFBM
		/// </summary>
		[DataColumn("FBFBM", AliasName = "FBFBM", Length = 14)]
		public string FBFBM { get; set; }

		/// <summary>
		/// FBFMC
		/// </summary>
		[DataColumn("FBFMC", AliasName = "FBFMC", Length = 50)]
		public string FBFMC { get; set; }

		/// <summary>
		/// SZDY
		/// </summary>
		[DataColumn("SZDY", AliasName = "SZDY", Length = 38)]
		public string SZDY { get; set; }
	}

}
