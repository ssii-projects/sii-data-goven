using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common.Util
{
	public class LayerWhereUtil
	{
		public static string UseWhere(string wh, string fieldName)
		{
			var u = MyGlobal.LoginUser;
			if (u == null) return null;
			if (!u.IsAdmin())
			{
				var postWh = $"{fieldName} like '{u.ZoneCode}%'";
				if (string.IsNullOrEmpty(wh))
				{
					wh = postWh;
				}
				else
				{
					wh = $"({wh}) and {postWh}";
				}
			}
			return wh;
		}
		public static string OnXzdyBeforeUseWhere(string wh)
		{
			return UseWhere(wh, "BM");
		}
		public static string OnDKBeforeUseWhere(string wh)
		{
			return UseWhere(wh, "DKBM");
		}
	}
}
