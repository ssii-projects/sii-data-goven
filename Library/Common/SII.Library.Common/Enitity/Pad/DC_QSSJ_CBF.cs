using Agro.LibCore;
using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	/// <summary>
	/// 本地SQLite和服务端数据库共用该实体
	/// </summary>
	[DataTable("DC_QSSJ_CBF", AliasName = "调查数据-承包方")]
	public class DC_QSSJ_CBF:Entity<DC_QSSJ_CBF>
	{
		[DataColumn("ID", AliasName = "ID", Length = 38)]
		public string ID { get; set; } = Guid.NewGuid().ToString();

		[DataColumn(AliasName ="发包方编码", Length = 14,Nullable =false)]
		public string FBFBM { get; set; }
		/// <summary>
		/// 承包方代码(M)
		/// </summary>
		[DataColumn("CBFBM", AliasName = "承包方代码", Length = 38,Nullable =false)]
		public string CBFBM { get; set; }

		/// <summary>
		/// 承包方类型(M)(eCBFLX)
		/// </summary>
		[DataColumn("CBFLX",Nullable=false, Length = 1, AliasName = "承包方类型",CodeType = CodeType.CBFLX)]
		public string CBFLX { get; set; }

		/// <summary>
		/// 承包方(代表)名称(M)
		/// </summary>
		[DataColumn("CBFMC", AliasName = "承包方名称", Length = 50)]
		public string CBFMC { get; set; }

		/// <summary>
		/// 承包方(代表)证件类型(eZJLX)(M)
		/// </summary>
		[DataColumn("CBFZJLX", Length = 1, AliasName = "承包方证件类型", CodeType =CodeType.ZJLX)]
		public string CBFZJLX { get; set; }

		/// <summary>
		/// 承包方(代表)证件号码(M)
		/// </summary>
		[DataColumn("CBFZJHM", Length = 20, AliasName = "承包方证件号码")]
		public string CBFZJHM { get; set; }

		/// <summary>
		/// 承包方地址(M)
		/// </summary>
		[DataColumn("CBFDZ", AliasName = "承包方地址", Length = 100)]
		public string CBFDZ { get; set; }

		/// <summary>
		/// 邮政编码(M)
		/// </summary>
		[DataColumn("YZBM", AliasName = "邮政编码", Length = 6)]
		public string YZBM { get; set; }

		/// <summary>
		/// 联系电话(O)
		/// </summary>
		[DataColumn("LXDH", AliasName = "联系电话", Length = 20)]
		public string LXDH { get; set; }

		/// <summary>
		/// 承包方成员数量(M)
		/// </summary>
		[DataColumn("CBFCYSL", AliasName = "承包方成员数量")]
		public int CBFCYSL { get; set; }

		/// <summary>
		/// 承包方调查日期(M)
		/// </summary>
		[DataColumn("CBFDCRQ", AliasName = "承包方调查日期", FieldType = eFieldType.eFieldTypeDate)]
		public DateTime? CBFDCRQ { get; set; }

		/// <summary>
		/// 承包方调查员(M)
		/// </summary>
		[DataColumn("CBFDCY", AliasName = "承包方调查员", Length = 50)]
		public string CBFDCY { get; set; }

		/// <summary>
		/// 承包方调查记事(C)
		/// </summary>
		[DataColumn("CBFDCJS", AliasName = "承包方调查记事", Length = 300)]
		public string CBFDCJS { get; set; }

		/// <summary>
		/// 公示记事(C)
		/// </summary>
		[DataColumn("GSJS", AliasName = "公示记事", Length = 300)]
		public string GSJS { get; set; }

		/// <summary>
		/// 公示记事人(M)
		/// </summary>
		[DataColumn("GSJSR", AliasName = "公示记事人", Length = 50)]
		public string GSJSR { get; set; }

		/// <summary>
		/// 公示审核日期(M)
		/// </summary>
		[DataColumn("GSSHRQ", AliasName = "公示审核日期",FieldType =eFieldType.eFieldTypeDate)]
		public DateTime? GSSHRQ { get; set; }

		/// <summary>
		/// 公示审核人(M)
		/// </summary>
		[DataColumn("GSSHR", AliasName = "公示审核人", Length = 50)]
		public string GSSHR { get; set; }

		[DataColumn(Nullable=false)]
		public int ZT { get; set; }

		[DataColumn(Nullable=false,FieldType =eFieldType.eFieldTypeInteger, CodeType = CodeType.DJZT)]
		public EDjzt DJZT { get; set; }

		[DataColumn(Nullable=false, FieldType =eFieldType.eFieldTypeDateTime)]
		public DateTime? CJSJ { get; set; }

		[DataColumn(FieldType = eFieldType.eFieldTypeDateTime)]
		public DateTime? ZHXGSJ { get; set; }
	}
}
