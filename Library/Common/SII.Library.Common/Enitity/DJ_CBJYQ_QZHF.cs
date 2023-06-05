using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Model
{
	public class DJ_CBJYQ_QZHF : Entity<DJ_CBJYQ_QZHF>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string SQID { get; set; }
		public string YWH { get; set; }
		public string DJBID { get; set; }
		public string QZID { get; set; }
		public string CBJYQZBM { get; set; }
		public string QZHFYY { get; set; }
		public DateTime? HFRQ { get; set; }
		public DateTime? QZHFLQRQ { get; set; }
		public string QZHFLQRXM { get; set; }
		public string HFLQRZJLX { get; set; }
		public string HFLQRZJHM { get; set; }
	}
}
