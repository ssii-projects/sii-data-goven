using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Model;

namespace Agro.Library.Common.Repository
{
	public class DcDlxxDkRepos : LibCore.Repository.CrudRepository<DC_DLXX_DK>
	{
		private readonly IWorkspace _db;
		public DcDlxxDkRepos(IFeatureWorkspace db)//:base(db.OpenFeatureClass(DC_DLXX_DK.GetTableName()))
		{
			_db = db;
		}
		public override IWorkspace Db => _db;
	}
}
