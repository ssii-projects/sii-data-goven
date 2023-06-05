/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;


namespace Agro.Library.Model
{
	public class EntityCBJYQDJBBase<T> : Entity<T> where T : class, new()
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 承包经营权 簿(证)编码(M)
		/// </summary>
		[DataColumn("CBJYQZBM", AliasName = "承包经营权证编码")]
		public string CBJYQZBM { get; set; }

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
		/// 承包方式(M)(eCBJYQQDFS)
		/// </summary>
		[DataColumn("CBFS", AliasName = "承包方式")]
		public string CBFS { get; set; }

		/// <summary>
		/// 承包期限(M)
		/// </summary>
		[DataColumn("CBQX", AliasName = "承包期限")]
		public string CBQX { get; set; }

		/// <summary>
		/// 承包期限起(M)
		/// </summary>
		[DataColumn("CBQXQ", AliasName = "承包期限起")]
		public DateTime? CBQXQ { get; set; }

		/// <summary>
		/// 承包期限止(M)
		/// </summary>
		[DataColumn("CBQXZ", AliasName = "承包期限止")]
		public DateTime? CBQXZ { get; set; }

		/// <summary>
		/// 地块示意图(M)
		/// </summary>
		[DataColumn("DKSYT", AliasName = "地块示意图")]
		public string DKSYT { get; set; }

		/// <summary>
		/// 登簿人
		/// </summary>
		[DataColumn("DBR", AliasName = "登簿人")]
		public string DBR { get; set; }

		/// <summary>
		/// 登记时间
		/// </summary>
		[DataColumn("DJSJ", AliasName = "登记时间")]
		public DateTime? DJSJ { get; set; }

		[DataColumn(AliasName = "承包经营权证流水号")]
		public string CBJYQZLSH { get; set; }
	}

	/// <summary>
	/// 承包经营权证登记簿
	/// </summary>
	[Serializable]
    [DataTable("QSSJ_CBJYQZDJB", AliasName = "承包经营权证登记簿")]
    public class QSSJ_CBJYQZDJB : EntityCBJYQDJBBase<QSSJ_CBJYQZDJB >
	{
		[DataColumn(AliasName = "登记簿附件")]
		public string DJBFJ { get; set; }
	}
}
