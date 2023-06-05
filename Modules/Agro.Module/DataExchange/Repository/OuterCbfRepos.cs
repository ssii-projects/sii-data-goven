using Agro.LibCore;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Module.DataExchange.Repository
{
	class OuterCbfRepos: DcCbfRepos
	{
		public OuterCbfRepos(IWorkspace db) : base(db)
		{
		}
		internal void LoadChangeData(ICancelTracker cancel, Action<DC_QSSJ_CBF> callback, bool fRecycle = false)
		{
			FindCallback(t =>t.ZHXGSJ!=null , it =>callback(it.Item), null, fRecycle, cancel);
		}
	}
}
