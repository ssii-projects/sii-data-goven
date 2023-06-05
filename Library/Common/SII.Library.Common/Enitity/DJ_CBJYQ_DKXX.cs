using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	public class DJ_CBJYQ_DKXX : Entity<DJ_CBJYQ_DKXX>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 地块代码(M)
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块代码")]
		public string DKBM { get; set; }

		/// <summary>
		/// 发包方代码(M)
		/// </summary>
		[DataColumn("FBFBM", AliasName = "发包方代码")]
		public string FBFBM { get; set; }

		/// <summary>
		/// 承包方代码(M)
		/// </summary>
		[DataColumn("CBFBM", AliasName = "承包方代码")]
		public string CBFBM { get; set; }

		/// <summary>
		/// 承包经营权取得方式(M)（eCBJYQQDFS）
		/// </summary>
		[DataColumn("CBJYQQDFS", AliasName = "承包经营权取得方式")]
		public string CBJYQQDFS { get; set; }

		/// <summary>
		/// 承包合同代码(M)
		/// </summary>
		[DataColumn("CBHTBM", AliasName = "承包合同代码")]
		public string CBHTBM { get; set; }

		/// <summary>
		/// 确权(合同)面积(M)
		/// </summary>
		[DataColumn("HTMJ", AliasName = "确权(合同)面积")]
		public decimal HTMJ { get; set; }

		/// <summary>
		/// 确权(合同)面积(M)
		/// </summary>
		[DataColumn("HTMJM", AliasName = "确权(合同)面积亩")]
		public decimal? HTMJM { get; set; }

		/// <summary>
		/// 原合同面积(M)
		/// </summary>
		[DataColumn("YHTMJ", AliasName = "原合同面积")]
		public decimal? YHTMJ { get; set; }

		/// <summary>
		/// 原合同面积(M)
		/// </summary>
		[DataColumn("YHTMJM", AliasName = "原合同面积(亩)")]
		public decimal? YHTMJM { get; set; }

		/// <summary>
		/// 承包经营权证(登记簿)代码(M)
		/// </summary>
		[DataColumn("CBJYQZBM", AliasName = "承包经营权证代码")]
		public string CBJYQZBM { get; set; }

		/// <summary>
		/// 流转合同代码(O)
		/// </summary>
		[DataColumn("LZHTBM", AliasName = "流转合同代码")]
		public string LZHTBM { get; set; }

		/// <summary>
		/// 是否确权确股(O)
		/// </summary>
		[DataColumn("SFQQQG", AliasName = "是否确权确股")]
		public string SFQQQG { get; set; }


		public string DKID { get; set; }
		public string DJBID { get; set; }

		public string DKMC { get; set; }

		public string YDKBM { get; set; }
		public string SYQXZ { get; set; }
		public string DKLB { get; set; }
		public string TDLYLX { get; set; }
		public string DLDJ { get; set; }
		public string TDYT { get; set; }
		public string SFJBNT { get; set; }
		public decimal SCMJ { get; set; }
		public decimal SCMJM { get; set; }
		public decimal ELHTMJ { get; set; }
		public decimal QQMJ { get; set; }
		public decimal JDDMJ { get; set; }
		public string DKDZ { get; set; }
		public string DKNZ { get; set; }
		public string DKXZ { get; set; }
		public string DKBZ { get; set; }
		public string DKBZXX { get; set; }
		public string ZJRXM { get; set; }
		public bool DYZT { get; set; }
		public bool YYZT { get; set; }
		public bool LZZT { get; set; }

		public string BZ { get; set; }
	}
}
