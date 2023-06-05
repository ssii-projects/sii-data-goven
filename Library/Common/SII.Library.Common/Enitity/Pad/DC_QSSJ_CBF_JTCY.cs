using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	/// <summary>
	/// 本地SQLite和服务端数据库共用该实体
	/// </summary>
	[DataTable("DC_QSSJ_CBF_JTCY")]
	public class DC_QSSJ_CBF_JTCY:Entity<DC_QSSJ_CBF_JTCY>
	{
		[DataColumn(Nullable=false, Length = 38)]
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 承包方代码(M)
		/// </summary>
		[DataColumn("CBFBM",Nullable=false, Length = 38, AliasName = "承包方代码")]
		public string CBFBM { get; set; }

		/// <summary>
		/// 成员姓名(M)
		/// </summary>
		[DataColumn("CYXM", Nullable = false, AliasName = "成员姓名", Length = 50)]
		public string CYXM { get; set; }

		/// <summary>
		/// 成员证件类型(M)(eZJLX)
		/// </summary>
		[DataColumn("CYZJLX",Nullable=false, AliasName = "成员证件类型", Length = 1, CodeType =CodeType.ZJLX)]
		public string CYZJLX { get; set; }

		/// <summary>
		/// 成员证件号码(M)
		/// </summary>
		[DataColumn("CYZJHM", AliasName = "成员证件号码", Length = 20)]
		public string CYZJHM { get; set; }

		/// <summary>
		/// 成员性别(M)(eSEX)
		/// </summary>
		[DataColumn("CYXB", AliasName = "成员性别", Length = 1, Nullable =false, CodeType = CodeType.XingBie)]
		public string CYXB { get; set; }

		[DataColumn(AliasName = "出生日期",FieldType =LibCore.eFieldType.eFieldTypeDate)]
		public DateTime? CSRQ { get; set; }

		/// <summary>
		/// 与户主关系(M)(eRelationShip)
		/// </summary>
		[DataColumn("YHZGX", AliasName = "与户主关系", Length = 2, CodeType =CodeType.JTGX)]
		public string YHZGX { get; set; }

		/// <summary>
		/// 成员备注(O)(eComment)
		/// </summary>
		[DataColumn("CYBZ", AliasName = "成员备注", Length = 1, CodeType = CodeType.CYBZ)]
		public string CYBZ { get; set; }

		/// <summary>
		/// 成员备注说明
		/// </summary>
		[DataColumn("CYBZSM", AliasName = "成员备注说明", Length = 254)]
		public string CYBZSM { get; set; }

		/// <summary>
		/// 是否共有人(O)(eWhether)
		/// </summary>
		[DataColumn("SFGYR", AliasName = "是否共有人", Length = 1, CodeType =CodeType.YesNo)]
		public string SFGYR { get; set; }
	}
}
