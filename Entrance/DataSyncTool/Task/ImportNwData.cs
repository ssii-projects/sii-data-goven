using Agro.LibCore;
using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*
yxm created at 2019/5/23 14:48:26
*/
namespace DataSyncTool
{
	class ImportNwData : Task
	{
		private readonly IWorkspace _tgtDb;
		private readonly IWorkspace _srcDb;
		public ImportNwData(IWorkspace srcDb, IWorkspace tgtDb)
		{
			Name = "导入内网交换包";
			Description = "导入内网已登记的数据";
			_srcDb = srcDb;
			_tgtDb = tgtDb;

			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) => ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var sqlFunc = _tgtDb.SqlFunc;
			var lst = new InsertBase[]{
				new DLXX_DK(sqlFunc),
				new DLXX_DK_JZD(sqlFunc),
				new DLXX_DK_JZX(sqlFunc),
				new DLXX_DK_TXBGJL(sqlFunc),
			};

			int n = 0, cnt = 0;
			double oldProgress = 0;
			foreach (var t in lst)
			{
				cnt += SafeConvertAux.ToInt32(_srcDb.QueryOne($"select count(1) from {t.TableName}"));
			}

			if (cnt == 0)
			{
				ReportProgress(100);
			}
			else
			{
				foreach (var t in lst)
				{
					try
					{
						_tgtDb.BeginTransaction();
						t.ImportNwData(this, _srcDb, _tgtDb, cancel, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						_tgtDb.Commit();
					}
					catch (Exception ex)
					{
						ReportException(ex);
						_tgtDb.Rollback();
					}
				}
			}
		}
	}
}
