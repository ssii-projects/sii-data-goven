using Agro.LibMapServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGovenServer.Repository
{
    public class CrudRepositoryBase<T,E>: CrudRepository<E>where E:class,new() where T:class, ICrudRepository, new()
	{
		private static T _instance;
		protected CrudRepositoryBase()
		{
			
		}
		public static T Instance(IWorkspace db)
		{
			if (_instance == null)
			{
				_instance = new T();
				_instance.FinalConstruct(db);
				
			}
			return _instance;
		}
	}
}
