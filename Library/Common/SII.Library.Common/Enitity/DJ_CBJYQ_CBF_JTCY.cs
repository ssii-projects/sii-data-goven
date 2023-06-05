using Agro.LibCore.Database;
using System;

namespace Agro.Library.Model
{
	public class DJ_CBJYQ_CBF_JTCY : Entity<DJ_CBJYQ_CBF_JTCY>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string DJBID { get; set; }
		public string CBFID { get; set; }
		public string CBFBM { get; set; }
		public string CYXM { get; set; }
		public string CYXB { get; set; }
		public string CYZJLX { get; set; }
		public string CYZJHM { get; set; }
		public DateTime? CSRQ { get; set; }
		public string YHZGX { get; set; }
		public string CYBZ { get; set; }
		public string SFGYR { get; set; }
		public string CYBZSM { get; set; }
	}
}
