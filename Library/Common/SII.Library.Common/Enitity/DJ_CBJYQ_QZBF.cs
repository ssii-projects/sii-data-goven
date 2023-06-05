using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	public class DJ_CBJYQ_QZBF : Entity<DJ_CBJYQ_QZBF>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string SQID { get; set; }
		public string YWH { get; set; }
		public string DJBID { get; set; }
		public string QZID { get; set; }
		public string CBJYQZBM { get; set; }
		public string QZBFYY { get; set; }
		public DateTime? BFRQ { get; set; }
		public string QZBFLQRQ { get; set; }
		public string QZBFLQRXM { get; set; }
		public string BFLQRZJLX { get; set; }
		public string BFLQRZJHM { get; set; }
	}
}
