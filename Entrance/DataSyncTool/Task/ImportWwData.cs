using Agro.LibCore;
using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*
yxm created at 2019/5/21 15:44:53
*/
namespace DataSyncTool
{
	class ImportWwData : Task
	{
		private readonly IWorkspace _tgtDb;
		private readonly IWorkspace _srcDb;
		public ImportWwData(IWorkspace srcDb,IWorkspace tgtDb)
		{
			Name = "导入外网交换包";
			Description = "导入外网已登记的数据";
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
		new DJ_CBJYQ_DJB(sqlFunc),
		new DJ_CBJYQ_CBF(sqlFunc),
		new DJ_CBJYQ_CBF_JTCY(sqlFunc),
		new DJ_CBJYQ_CBHT(sqlFunc),
		new DJ_CBJYQ_DKSYT(sqlFunc),
		new DJ_CBJYQ_DKXX(sqlFunc),
		new DJ_CBJYQ_LZDK(sqlFunc),
		new DJ_CBJYQ_LZHT(sqlFunc),
		new DJ_CBJYQ_QSLYZL(sqlFunc),
		new DJ_CBJYQ_QZ(sqlFunc),
		new DJ_CBJYQ_QZBF(sqlFunc),
		new DJ_CBJYQ_QZHF(sqlFunc),
		new DJ_CBJYQ_QZ_DYJL(sqlFunc),
		new DJ_CBJYQ_WTFK(sqlFunc),
		new DJ_CBJYQ_YDJB(sqlFunc),
		new QSSJ_CBDKXX(sqlFunc),
		new QSSJ_CBF(sqlFunc),
		new QSSJ_CBF_BGJL(sqlFunc),
		new QSSJ_CBF_JTCY(sqlFunc),
		new QSSJ_CBHT(sqlFunc),
		new QSSJ_CBJYQZ(sqlFunc),
		new QSSJ_CBJYQZDJB(sqlFunc),
		new QSSJ_CBJYQZ_QZBF(sqlFunc),
		new QSSJ_CBJYQZ_QZHF(sqlFunc),
		new QSSJ_CBJYQZ_QZZX(sqlFunc),
		new QSSJ_LZHT(sqlFunc),
		new QSSJ_FBF(sqlFunc),
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
						t.ImportWwData(this, _srcDb, _tgtDb, cancel, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
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
