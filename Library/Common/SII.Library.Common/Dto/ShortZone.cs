using Agro.Library.Model;

namespace Agro.Library.Common
{
	public class ShortZone
	{
		public int OID;
		public string ID;
		/// <summary>
		/// 地域编码
		/// </summary>
		public string Code;
		/// <summary>
		/// 地域名称
		/// </summary>
		public string Name;
		/// <summary>
		/// 级别
		/// </summary>
		public eZoneLevel Level;

		public ShortZone(DLXX_XZDY en)
		{
			ID = en.ID;
			Code = en.BM;
			Name = en.MC;
			Level = en.JB;
			OID = en.ObjectId;
		}
		public ShortZone(string id, string code, string name, eZoneLevel level, int oid)
		{
			ID = id;
			Code = code;
			Name = name;
			Level = level;
			OID = oid;
		}
	}
}
