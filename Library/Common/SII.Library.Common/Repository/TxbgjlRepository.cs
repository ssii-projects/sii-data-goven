using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using System.Collections.Generic;

namespace Agro.Library.Common.Repository
{
	public class TxbgjlRepository: CrudRepository<TxbgjlRepository,DLXX_DK_TXBGJL>
	{
		//public TxbgjlRepository(IWorkspace db):base(db) { }

		//public static TxbgjlRepository Instance { get; } = new TxbgjlRepository();

		public int Insert(DLXX_DK_TXBGJL entity,SubFields fields = null)
		{
			if (entity.BGFS == ETXBGLX.Xinz)
			{
				//yxm 2019-10-23 新增地块不写入图形变更记录表
				return 0;
			}
			return base.Insert(entity, fields);
		}
	}
}
