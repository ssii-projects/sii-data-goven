using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	/// <summary>
	/// 原地块数据（参考图层）
	/// </summary>
	[DataTable("DC_DLXX_DK", AliasName = "导出地块")]
	public class DC_DLXX_DK: VEC_SURVEY_DKBase<DC_DLXX_DK>
	{
		/// <summary>
		/// 登记状态
		/// </summary>
		[DataColumn("DJZT", AliasName = "登记状态", Nullable = false, FieldType = LibCore.eFieldType.eFieldTypeInteger, CodeType = CodeType.DJZT)]
		public int DJZT { get; set; } = (int)EDjzt.Wdj;

		[DataColumn("ZYY", AliasName = "作业员", FieldType = LibCore.eFieldType.eFieldTypeString, Length = 50, Tag = Tag_VEC_SURVEY_DK.SQLite)]
		public string ZYY { get; set; }


	}
}
