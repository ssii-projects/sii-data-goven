using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common.Repository
{
	public class DjDjbRepository : CrudRepository<DjDjbRepository,DJ_CBJYQ_DJB>
	{
		public string IsValid(DJ_CBJYQ_DJB en)
		{
			string err = null;
			if (string.IsNullOrEmpty(en.CBJYQZBM) || en.CBJYQZBM.Length != 19)
			{
				return $"承包经营权证编码{en.CBJYQZBM} 无效！";
			}
			var xbm = en.CBJYQZBM.Substring(0, 6);
			if (!DlxxXzdyRepository.Instance.Exists(t => t.JB == eZoneLevel.County && t.BM == xbm))
			{
				err = $"承包经营权证编码{en.CBJYQZBM}不是本系统有效的编码，请检查是否本县数据！";
			}
			return err;
		}
		/// <summary>
		/// 查询登记簿包含的地块
		/// </summary>
		/// <param name="djbID"></param>
		/// <returns></returns>
		public List<DJ_CBJYQ_DKXX> GetDjDKs(string djbID)
		{
			return DjDkxxRepository.Instance.FindAll(t => t.DJBID == djbID);
		}
	}
}