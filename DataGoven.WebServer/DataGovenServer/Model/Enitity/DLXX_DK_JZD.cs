using Agro.LibMapServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Library.Model
{
	[Serializable]
	[DataTable("DLXX_DK_JZD", AliasName = "界址点")]
	public class DLXX_DK_JZD : Entity<DLXX_DK_JZD>
	{
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string DKBM { get; set; }
	};
}
