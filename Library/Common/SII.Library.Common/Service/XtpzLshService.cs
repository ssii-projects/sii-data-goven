using Agro.LibCore;
using Agro.Library.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common.Service
{
	public class XtpzLshService
	{
		private readonly Dictionary<string, int> dic = new Dictionary<string, int>();
		public void AddCbf(string fbfBm, string cbfLx, string cbfBm)
		{
			int bh;
			string lx;
			switch (cbfLx.Trim())
			{
				case "1"://CONTRACTOR_FAMILY
					lx = "CONTRACTOR_FAMILY";
					bh = SafeConvertAux.ToInt32(cbfBm.Right(3)) + 1;
					break;
				case "2"://CONTRACTOR_PERSON
					lx = "CONTRACTOR_PERSON";
					bh = SafeConvertAux.ToInt32(cbfBm.Right(3)) + 8000 + 1;
					break;
				case "3"://CONTRACTOR_ORG
					lx = "CONTRACTOR_ORG";
					bh = SafeConvertAux.ToInt32(cbfBm.Right(3)) + 9000 + 1;
					break;
				default:
					throw new Exception($"CBFLX值只能为（1,2,3）中的一种，当前值为：{cbfLx}");
			}
			var key = MakeKey(lx,fbfBm);
			if (dic.TryGetValue(key, out int n))
			{
				if (bh > n)
				{
					dic[key] = bh;
				}
			}
			else
			{
				dic[key] = bh;
			}
		}
		public void AddDk(string fbfBm, string dkBm)
		{
			int bh = SafeConvertAux.ToInt32(dkBm.Right(5)) + 1;
			var key =MakeKey("CONTRACTLAND", fbfBm);
			if (dic.TryGetValue(key, out int n))
			{
				if (bh > n)
				{
					dic[key] = bh;
				}
			}
			else
			{
				dic[key] = bh;
			}
		}

		public void AddCertificate(string xianBM, int year, int bh)
		{
			var key =MakeKey("CONTRACTLAND_CERTIFICATE", xianBM + "_" + year);
			if (dic.TryGetValue(key, out int n))
			{
				if (bh > n)
				{
					dic[key] = bh;
				}
			}
			else
			{
				dic[key] = bh;
			}
		}
		public void UpdateLsh(IWorkspace db)
		{
			var repos = XtpzLshRepository.Instance;// new XtpzLshRepository(db);
			var odb = repos.Db;
			repos.ChangeSource(db);
			try
			{
				foreach (var kv in dic)
				{
					var sa = kv.Key.Split('~');
					var lx = sa[1];
					var fzm = sa[0];
					repos.UpdateLsh(lx, fzm, kv.Value);
				}
				dic.Clear();
			}
			finally
			{
				repos.ChangeSource(odb);
			}
		}

		private string MakeKey(string lx, string fzm)
		{
			return fzm + "~" + lx;
		}
	}
}
