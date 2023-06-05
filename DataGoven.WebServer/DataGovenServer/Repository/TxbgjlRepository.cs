using Agro.LibMapServer;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGovenServer.Repository
{
	public class TxbgjlRepository : CrudRepositoryBase<TxbgjlRepository,DLXX_DK_TXBGJL>
	{
		//public TxbgjlRepository() : base(new TableMeta(DLXX_DK_TXBGJL.GetTableName())) { }
		public new int? Insert(IDbConnection con, DLXX_DK_TXBGJL entity, SubFields fields = null)
		{
			if (entity.BGFS == EBGLX.Xinz)
			{
				//yxm 2019-10-23 新增地块不写入图形变更记录表
				return null;
			}
			return base.Insert(con,entity, fields);
		}
	}
}
