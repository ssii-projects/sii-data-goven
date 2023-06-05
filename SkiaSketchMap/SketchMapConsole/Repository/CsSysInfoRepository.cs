using Agro.Library.Model;
using Agro.LibCore.Repository;
namespace Agro.Library.Common.Repository
{
	public class CsSysInfoRepository : CrudRepository<CsSysInfoRepository, CS_SYSINFO>
	{
		/// <summary>
		/// DLXX_DK.DKMJ保留小数点位数
		/// </summary>
		public static readonly string KEY_DKMJSCALE = "AB85771F-00D7-4978-9A5D-F57104159DAC";
		public string? Load(string key)
		{
			var en=base.Find(t => t.ID == key,(c,t)=>c(t.Value));
			return en?.Value;
		}
		public void Save(string key, object value)
		{
			var en=base.Find(t => t.ID == key);
			if (en != null)
			{
				en.Value = value.ToString()??"";
				Update(en, t => t.ID == key, (c, t) => c(t.Value));
			}
			else
			{
				en = new CS_SYSINFO
				{
					ID = key,
					Value = value.ToString() ?? ""
				};
				Insert(en);
			}
		}
		public int LoadInt(string key, int defaultVal=0)
		{
			var s = Load(key);
			if (s != null&&int.TryParse(s,out int d))
			{
				return d;
			}
			return defaultVal;
		}
	}
}
