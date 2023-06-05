using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using System;


/*
yxm created at 2019/5/21 15:44:53
*/
namespace Agro.Module.DataSync
{
	class ImportWwData : Task
	{
		public ImportWwData()
		{
			Name = "导入外网交换包";
			Description = "导入外网已登记的数据";
			PropertyPage = new ImportNwDataPanel("外网交换包(*.wdb) | *.wdb", "ABC4D0FE-BB09-4089-8B4A-C7D953A7824B");
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) => ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var _tgtDb = MyGlobal.Workspace;
			var sqlFunc = _tgtDb.SqlFunc;
			var lst = new InsertBase[]{
		new DlxxDk(sqlFunc),
		new DjCbjyqDjb(sqlFunc),
		new DjCbjyqCbf(sqlFunc),
		new DjCbjyqCbfJjcy(sqlFunc),
		new DjCbjyqCbht(sqlFunc),
		new DjCbjyqDksyt(sqlFunc),
		new DjCbjyqDkxx(sqlFunc),
		new DjCbjyqLzdk(sqlFunc),
		new DjCbjyqLzht(sqlFunc),
		new DjCbjyqQslyzl(sqlFunc),
		new DjCbjyqQz(sqlFunc),
		new DjCbjyqQzbf(sqlFunc),
		new DjCbjyqQzhf(sqlFunc),
		new DjCbjyqQzDyjl(sqlFunc),
		//new DJ_CBJYQ_WTFK(sqlFunc),
		new DjCbjyqYdjb(sqlFunc),
		new DjYwSlsq(sqlFunc),
		new QssjCbdkxx(sqlFunc),
		new QssjCbf(sqlFunc),
		new QssjCbfBgjl(sqlFunc),
		new QssjCbfJtcy(sqlFunc),
		new QssjCbht(sqlFunc),
		new QssjCbjyqz(sqlFunc),
		new QssjCbjyqzdjb(sqlFunc),
		new QssjCbjyqzQzbf(sqlFunc),
		new QssjCbjyqzQzhf(sqlFunc),
		new QssjCbjyqzQzzx(sqlFunc),
		new QssjLzht(sqlFunc),
		new QssjFbf(sqlFunc),
	};

			using (var _srcDb = WorkspaceFactory.OpenSQLiteDatabase((PropertyPage as ImportNwDataPanel).FileName))
			{
				//int n = 0, cnt = 0;
				//double oldProgress = 0;
				int cnt = 0;
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
					var progress = new ProgressReporter(ReportProgress, cnt);
					foreach (var t in lst)
					{
						try
						{
							_tgtDb.BeginTransaction();
							t.ImportWwData(this, _srcDb, _tgtDb, cancel, () =>progress.Step());
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
}
