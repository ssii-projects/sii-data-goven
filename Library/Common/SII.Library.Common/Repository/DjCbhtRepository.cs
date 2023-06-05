using Agro.Library.Model;

namespace Agro.Library.Common.Repository
{
	public class DjCbhtRepository : CrudRepository<DjCbhtRepository,DJ_CBJYQ_CBHT>
	{
		public string IsValid(DJ_CBJYQ_CBHT en)
		{
			string err = null;
			if (string.IsNullOrEmpty(en.CBHTBM) || en.CBHTBM.Length != 19)
			{
				return $"承包合同编码{en.CBHTBM} 无效！";
			}
			return err;
		}
	}
}
