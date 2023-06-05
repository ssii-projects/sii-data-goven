using Agro.LibMapServer;
using System;

namespace Agro.Library.Model
{
	[Serializable]
	[DataTable("DLXX_DK_TXBGJL", AliasName = "地块图形变更记录")]
	public class DLXX_DK_TXBGJL : Entity<DLXX_DK_TXBGJL>
	{
		#region Properties

		/// <summary>
		/// 唯一标识
		/// </summary>
		[DataColumn("ID", Unique  = true, Nullable = false)]
		public string ID { get; set; } = Guid.NewGuid().ToString();


		[DataColumn("DKID", AliasName = "DKID")]
		public string DKID { get; set; }

		/// <summary>
		/// DKBM
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块编码")]
		public string DKBM { get; set; }

		[DataColumn("YDKID", AliasName = "YDKID")]
		public string YDKID { get; set; }

		/// <summary>
		/// 原地块编码
		/// </summary>
		[DataColumn("YDKBM", AliasName = "原地块编码")]
		public string YDKBM { get; set; }

		/// <summary>
		/// 变更方式
		/// </summary>
		[DataColumn("BGFS", AliasName = "变更方式",FieldType =eFieldType.eFieldTypeInteger)]
		public EBGLX BGFS { get; set; }

		/// <summary>
		/// 变更原因
		/// </summary>
		[DataColumn("BGYY", AliasName = "变更原因")]
		public string BGYY { get; set; }

		#endregion
	}
}
