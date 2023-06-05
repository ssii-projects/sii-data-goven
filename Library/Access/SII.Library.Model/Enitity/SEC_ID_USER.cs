using System;
using Agro.LibCore.Database;

namespace Agro.Library.Model
{

	[Serializable]
	[DataTable("SEC_ID_USER", AliasName = "用户")]
	public class SEC_ID_USER : Entity<SEC_ID_USER >
	{
		public string ID { get; set; }

		[DataColumn("Username", AliasName = "用户名")]
		public string Name { get; set; }

		public string Password { get; set; }

		[DataColumn("SecurityStamp", AliasName = "地域代码")]
		public string ZoneCode { get; set; }

		public bool Buildin { get; set; } = false;
		public bool EmailConfirmed { get; set; } = false;
		public bool PhoneNumberConfirmed { get; set; } = false;
		public bool LockoutEnabled { get; set; } = false;
		public int AccessFailedCount { get; set; } = 0;
		public bool Disabled { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;
		public bool IsAdmin()
		{
			return "admin" == Name;
		}
	}
}
