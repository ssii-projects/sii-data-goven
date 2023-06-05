using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using System;


/*
yxm created at 2019/5/23 14:48:26
*/
namespace Agro.Module.DataSync
{
	public class ImportNwData : Task
	{
		//private readonly IWorkspace _tgtDb;
		//private readonly IWorkspace _srcDb;
		public ImportNwData()//IWorkspace srcDb, IWorkspace tgtDb)
		{
			Name = "导入内网交换包";
			Description = "导入内网已登记的数据";
			//_srcDb = srcDb;
			//_tgtDb = tgtDb;
			PropertyPage = new ImportNwDataPanel();
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) => ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var _tgtDb = MyGlobal.Workspace;
			var sqlFunc = _tgtDb.SqlFunc;
			var lst = new InsertBase[]{
				new DlxxDk(sqlFunc),
				new DlxxDkJzd(sqlFunc),
				new DlxxDkJzx(sqlFunc),
				new DlxxDkTxbgjl(sqlFunc),
				#region yxm 2019-8-19
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
				#endregion
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
							t.ImportNwData(this, _srcDb, _tgtDb, cancel, () =>progress.Step());
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
