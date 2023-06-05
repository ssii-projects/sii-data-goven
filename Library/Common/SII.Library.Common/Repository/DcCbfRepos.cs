using Agro.LibCore;
using Agro.Library.Model;

namespace Agro.Library.Common.Repository
{
	public class DcCbfRepos : LibCore.Repository.CrudRepository<DC_QSSJ_CBF> 
	{
		private readonly IWorkspace _db;
		public DcCbfRepos(IWorkspace db) {
			_db = db;
		}
		public override IWorkspace Db => _db;
	}
}
