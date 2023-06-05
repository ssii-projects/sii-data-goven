using Agro.LibCore.Database;
using GeoAPI.Geometries;

namespace Agro.Library.Model
{
	/// <summary>
	/// DLXX_DK_JZX
	/// </summary>
	[DataTable("DLXX_DK_JZX", AliasName = "DLXX_DK_JZX")]
	public class DLXX_DK_JZX : Entity<DLXX_DK_JZX>
	{
		/// <summary>
		/// 要素代码
		/// </summary>
		[DataColumn("YSDM", AliasName = "要素代码", Nullable = false, Length = 6)]
		public string YSDM { get; set; }

		/// <summary>
		/// 标识
		/// </summary>
		[DataColumn("ID", AliasName = "标识", Nullable = false, Length = 38)]
		public string ID { get; set; }

		/// <summary>
		/// 地块编码
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块编码", Nullable = false, Length = 19)]
		public string DKBM { get; set; }

		/// <summary>
		/// 界址线号
		/// </summary>
		[DataColumn("JZXH", AliasName = "界址线号", Length = 10)]
		public string JZXH { get; set; }

		/// <summary>
		/// 界线性质
		/// </summary>
		[DataColumn("JXXZ", AliasName = "界线性质", Length = 6)]
		public string JXXZ { get; set; }

		/// <summary>
		/// 界址线类类别
		/// </summary>
		[DataColumn("JZXLB", AliasName = "界址线类类别", Length = 2)]
		public string JZXLB { get; set; }

		/// <summary>
		/// 界址线位置
		/// </summary>
		[DataColumn("JZXWZ", AliasName = "界址线位置", Length = 1)]
		public string JZXWZ { get; set; }

		/// <summary>
		/// 界址线说明
		/// </summary>
		[DataColumn("JZXSM", AliasName = "界址线说明", Length = 300)]
		public string JZXSM { get; set; }

		/// <summary>
		/// 毗邻地物权利人
		/// </summary>
		[DataColumn("PLDWQLR", AliasName = "毗邻地物权利人", Length = 100)]
		public string PLDWQLR { get; set; }

		/// <summary>
		/// 毗邻地物指界人
		/// </summary>
		[DataColumn("PLDWZJR", AliasName = "毗邻地物指界人", Length = 100)]
		public string PLDWZJR { get; set; }

		/// <summary>
		/// 起界址点号
		/// </summary>
		[DataColumn("QJZDH", AliasName = "起界址点号", Length = 10)]
		public string QJZDH { get; set; }

		/// <summary>
		/// 止界址点号
		/// </summary>
		[DataColumn("ZJZDH", AliasName = "止界址点号", Length = 10)]
		public string ZJZDH { get; set; }

		/// <summary>
		/// 几何对象
		/// </summary>
		[DataColumn("SHAPE", AliasName = "几何对象")]
		public IGeometry SHAPE { get; set; }

		/// <summary>
		/// BSM
		/// </summary>
		[DataColumn("BSM", AliasName = "BSM", Nullable = false)]
		public int BSM { get; set; }
	}
}
