using Agro.LibCore.Database;
using GeoAPI.Geometries;

namespace Agro.Library.Model
{
	/// <summary>
	/// DLXX_DK_JZD
	/// </summary>
	[DataTable("DLXX_DK_JZD", AliasName = "DLXX_DK_JZD")]
	public class DLXX_DK_JZD : Entity<DLXX_DK_JZD>
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
		/// 界址点号
		/// </summary>
		[DataColumn("JZDH", AliasName = "界址点号", Nullable = false, Length = 10)]
		public string JZDH { get; set; }

		/// <summary>
		/// 界址点类型
		/// </summary>
		[DataColumn("JZDLX", AliasName = "界址点类型", Nullable = false, Length = 1)]
		public string JZDLX { get; set; }

		/// <summary>
		/// 界标类型
		/// </summary>
		[DataColumn("JBLX", AliasName = "界标类型", Length = 1)]
		public string JBLX { get; set; }

		/// <summary>
		/// X坐标值
		/// </summary>
		[DataColumn("XZBZ", AliasName = "X坐标值", Nullable = false, Precision = 13, Scale = 3)]
		public decimal XZBZ { get; set; }

		/// <summary>
		/// Y坐标值
		/// </summary>
		[DataColumn("YZBZ", AliasName = "Y坐标值", Nullable = false, Precision = 13, Scale = 3)]
		public decimal YZBZ { get; set; }

		/// <summary>
		/// 几何对象
		/// </summary>
		[DataColumn("SHAPE", AliasName = "几何对象",GeometryType =LibCore.eGeometryType.eGeometryPoint)]
		public IGeometry SHAPE { get; set; }

		/// <summary>
		/// BSM
		/// </summary>
		[DataColumn("BSM", AliasName = "BSM", Nullable = false)]
		public int BSM { get; set; }
	}
}
