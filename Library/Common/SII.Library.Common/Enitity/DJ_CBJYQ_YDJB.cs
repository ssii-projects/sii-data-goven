using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Model
{
	/// <summary>
	/// 原登记簿
	/// </summary>
	[Serializable]
	[DataTable("DJ_CBJYQ_YDJB", AliasName = "原登记簿")]
	public class DJ_CBJYQ_YDJB : Entity<DJ_CBJYQ_YDJB>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 登记簿ID
		/// </summary>
		[DataColumn("DJBID", AliasName = "登记簿ID")]
		public string DJBID { get; set; }

		/// <summary>
		/// 原登记簿ID
		/// </summary>
		[DataColumn("YDJBID", AliasName = "原登记簿ID")]
		public string YDJBID { get; set; }

		/// <summary>
		/// 受理申请ID
		/// </summary>
		[DataColumn("SLSQID", AliasName = "受理申请ID")]
		public string SLSQID { get; set; }

		/// <summary>
		/// 登记类型
		/// </summary>
		[DataColumn("DJLX", AliasName = "登记类型")]
		public int DJLX { get; set; }

		/// <summary>
		/// 登记小类
		/// </summary>
		[DataColumn("DJXL", AliasName = "登记小类")]
		public int DJXL { get; set; }

		/// <summary>
		/// 变更类型
		/// </summary>
		[DataColumn("BGLX", AliasName = "变更类型")]
		public EYwBGLX BGLX { get; set; }

		/// <summary>
		/// 变更参数
		/// </summary>
		[DataColumn("BGCS", AliasName = "变更参数")]
		public string BGCS { get; set; }
	}
}
