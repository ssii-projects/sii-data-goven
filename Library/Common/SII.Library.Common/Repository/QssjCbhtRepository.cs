using Agro.Library.Model;

namespace Agro.Library.Common.Repository
{
	public class QssjCbhtRepository : CrudRepository<QssjCbhtRepository,QSSJ_CBHT>
	{
		public string IsValid(QSSJ_CBHT en)
		{
			string err = null;
			if (string.IsNullOrEmpty(en.CBHTBM) || en.CBHTBM.Length != 19)
			{
				return $"承包合同编码{en.CBHTBM} 无效！";
			}
			//var lst = DlxxXzdyRepository.Instance.Find(t => t.JB == eZoneLevel.County, DLXX_XZDY.GetFieldName(nameof(DLXX_XZDY.BM)));
			//if (lst.Find(it => en.CBJYQZBM.StartsWith(it.BM)) == null)
			//{
			//	err = $"承包经营权证编码{en.CBJYQZBM}不是本系统有效的编码，请检查是否本县数据！";
			//}
			return err;
		}
	}
}
