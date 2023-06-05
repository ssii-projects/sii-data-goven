using Agro.GIS;
using Agro.LibCore;
using System;

namespace Agro.Library.Common.Repository
{
	public class CrudRepository<U,T> : LibCore.Repository.CrudRepository<T> where T : class,new() 
		where U:new()
	{
		private IWorkspace? _db;
		public void FinalConstruct(IWorkspace db)
		{
			var fSourceChanged = _db != null && _db != db;
			_db = db;
			if (fSourceChanged)
			{
				base.ChangeSource(db);
			}
		}
		//public IWorkspace Db { get { return _db; } }
		//public CrudRepository()//IFeatureWorkspace db=null) : base(db??MyGlobal.Workspace)
		//{
		//	MyGlobal.OnDatasourceChanged += () =>
		//	{
		//		if (_db == null)
		//		{
		//			ChangeSource(MyGlobal.Workspace);
		//		}
		//	};
		//	//Console.WriteLine($"new CrudRepository():MyGlobal.Workspace is {MyGlobal.Workspace}");
		//}
		public static U Instance { get; } = new U();

		public override IWorkspace Db =>_db??MyGlobal.Workspace;
	}
}
