using Agro.LibCore;
using System;
using System.Linq;


/*
yxm created at 2019/5/17 14:19:10
*/
namespace Agro.Module.DataSync
{
	class ExportUtil
	{
		public static void QueryDatas(IWorkspace db, InsertBase tb, string sql, ICancelTracker cancel)
		{
			db.QueryCallback(sql, r =>
			{
				var row = new Row();
				for (int i = 0; i < tb._insertPrms.Count; ++i)
				{
					row.Add(r.IsDBNull(i) ? null : r.GetValue(i));
				}
				tb.rows.Add(row);
				return true;
			}, cancel);
		}
		public static void DoExport(IWorkspace tgtDb, InsertBase tb, Action callback)
		{
			foreach (var r in tb.rows)
			{
				for (int i = 0; i < r.Count; ++i)
				{
					tb._insertPrms[i].ParamValue = r[i];
				}
				tgtDb.ExecuteNonQuery(tb.insertSql, tb._insertPrms);
				callback();
			}
		}
	}
}
