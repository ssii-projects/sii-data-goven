using System;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{
	/// <summary>
	/// QSSJ_LZHT
	/// </summary>
	[DataTable("QSSJ_LZHT", AliasName = "QSSJ_LZHT")]
	public class QSSJ_LZHT : Entity<QSSJ_LZHT>
	{
		/// <summary>
		/// 标识
		/// </summary>
		[DataColumn("ID", AliasName = "标识", Nullable = false, Length = 38)]
		public string ID { get; set; }

		/// <summary>
		/// 承包合同代码
		/// </summary>
		[DataColumn("CBHTBM", AliasName = "承包合同代码", Nullable = false, Length = 19)]
		public string CBHTBM { get; set; }

		/// <summary>
		/// 流转合同代码
		/// </summary>
		[DataColumn("LZHTBM", AliasName = "流转合同代码", Nullable = false, Length = 19)]
		public string LZHTBM { get; set; }

		/// <summary>
		/// 承包方代码
		/// </summary>
		[DataColumn("CBFBM", AliasName = "承包方代码", Nullable = false, Length = 18)]
		public string CBFBM { get; set; }

		/// <summary>
		/// 受让方代码
		/// </summary>
		[DataColumn("SRFBM", AliasName = "受让方代码", Nullable = false, Length = 18)]
		public string SRFBM { get; set; }

		/// <summary>
		/// 流转方式
		/// </summary>
		[DataColumn("LZFS", AliasName = "流转方式", Nullable = false, Length = 3)]
		public string LZFS { get; set; }

		/// <summary>
		/// 流转期限
		/// </summary>
		[DataColumn("LZQX", AliasName = "流转期限", Nullable = false, Length = 10)]
		public string LZQX { get; set; }

		/// <summary>
		/// 流转期限开始日期
		/// </summary>
		[DataColumn("LZQXKSRQ", AliasName = "流转期限开始日期", Nullable = false)]
		public DateTime LZQXKSRQ { get; set; }

		/// <summary>
		/// 流转期限结束日期
		/// </summary>
		[DataColumn("LZQXJSRQ", AliasName = "流转期限结束日期", Nullable = false)]
		public DateTime LZQXJSRQ { get; set; }

		/// <summary>
		/// 流转面积
		/// </summary>
		[DataColumn("LZMJ", AliasName = "流转面积", Nullable = false, Precision = 15, Scale = 2)]
		public decimal LZMJ { get; set; }

		/// <summary>
		/// 流转面积（亩）
		/// </summary>
		[DataColumn("LZMJM", AliasName = "流转面积（亩）", Precision = 15, Scale = 4)]
		public decimal? LZMJM { get; set; }

		/// <summary>
		/// 流转地块数
		/// </summary>
		[DataColumn("LZDKS", AliasName = "流转地块数")]
		public int? LZDKS { get; set; }

		/// <summary>
		/// 流转前土地用途
		/// </summary>
		[DataColumn("LZQTDYT", AliasName = "流转前土地用途", Length = 1)]
		public string LZQTDYT { get; set; }

		/// <summary>
		/// 流转后土地用途
		/// </summary>
		[DataColumn("LZHTDYT", AliasName = "流转后土地用途", Length = 1)]
		public string LZHTDYT { get; set; }

		/// <summary>
		/// 流转费用说明
		/// </summary>
		[DataColumn("LZJGSM", AliasName = "流转费用说明", Nullable = false, Length = 100)]
		public string LZJGSM { get; set; }

		/// <summary>
		/// 合同签订日期
		/// </summary>
		[DataColumn("HTQDRQ", AliasName = "合同签订日期", Nullable = false)]
		public DateTime HTQDRQ { get; set; }
	}
}
