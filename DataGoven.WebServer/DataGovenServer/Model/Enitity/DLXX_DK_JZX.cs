using Agro.LibMapServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Library.Model
{
	[Serializable]
	[DataTable("DLXX_DK_JZX", AliasName = "界址线")]
	public class DLXX_DK_JZX : Entity<DLXX_DK_JZX>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string DKBM { get; set; }
		public string JZXH { get; set; }
		public string PLDWZJR { get; set; }
		public string PLDWQLR { get; set; }
	};
}