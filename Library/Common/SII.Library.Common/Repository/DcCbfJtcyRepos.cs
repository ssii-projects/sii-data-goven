using Agro.LibCore;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common.Repository
{
	public class DcCbfJtcyRepos : LibCore.Repository.CrudRepository<DC_QSSJ_CBF_JTCY>
	{
		private readonly IWorkspace _db;
		public DcCbfJtcyRepos(IWorkspace db)// : base(db)
		{
			_db = db;
		}

		public override IWorkspace Db => _db;
	}
}
