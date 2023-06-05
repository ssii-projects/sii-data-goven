using Agro.Library.Common;
using System;
using System.Collections.Generic;
using Agro.LibCore.Task;
using Agro.LibCore;
using Agro.Module.DataExchange;
using System.IO;
using Agro.Library.Model;
using GeoAPI.Geometries;
//using Agro.Module.DataExchange.Properties;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	public class TaskImportUtil
	{
		#region 数据导入部分
		public class TaskImportData : Task
		{
			public InputParam _prm;
			private IImportTable _imp;
			private DateTime _starttime;
			public TaskImportData(InputParam prms, string name, IImportTable imp)
			{
				_prm = prms;
				_imp = imp;
				this.Name = name;
				base.OnStart += t => ReportInfomation($"开始{Name}");
				base.OnFinish += (t, e) =>
				{
					if (t.ErrorCount() == 0 && t.WarningCount() == 0)
					{
						var str = _imp?.RecordCount >= 0 ? $"共导入数据{_imp.RecordCount}条，" : "";
						var msg = $"结束{Name}，{str}耗时：{t.Elapsed}";
						ReportInfomation(msg);

						LogoutUtil.WriteLog(msg);
					}
				};
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					var ri = new ReportInfo();
					ri.reportProgress += i => this.ReportProgress((int)i);
					ri.reportError += msg => this.ReportError(msg);
					ri.reportInfo += msg => this.ReportInfomation(msg);
					ri.reportWarning += msg => this.ReportWarning(msg);
					_starttime = DateTime.Now;
					_imp.DoImport(_prm, ri, cancel);
					var time = DateTime.Now - _starttime;
					Report(time);
				}
				catch (Exception ex)
				{
					base.ReportException(new Exception("导入数据发生错误:" + ex.Message));
				}
			}
			#region 数据入库报告
			internal void Report(TimeSpan stime)
			{
				using (StreamWriter wt = new StreamWriter(_prm.RootPath + "\\数据入库报告.txt", true))
				{
					var wtstr = string.Format("{0}  {1}  {2}：{3}条，耗时：{4}", DateTime.Now/*.ToString("yyyy/mm/dd hh:MM:ss")*/, _prm.sXzqmc, _imp.TableName, _imp.RecordCount, stime.ToString());
					wt.WriteLine(wtstr);
				}
			}
			#endregion
		}
		/// <summary>
		/// 添加数据导入任务
		/// </summary>
		/// <param name="tg"></param>
		/// <param name="prm"></param>
		public static void AddToTaskGroup(GroupTask tg, InputParam prm,bool fCheckDataExist = true)
		{
			if (fCheckDataExist)
			{
				tg.AddTask(new TaskImportData(prm, "导入" + JCSJ_SJHZ.GetTableName(), new AgricultureDataImport()));
			}
			AddToTaskGroupForSqlServer(tg, prm,fCheckDataExist);
		}
		private static void AddToTaskGroupForSqlServer(GroupTask tg, InputParam prm, bool fCheckDataExist )
		{
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.XZQH_XZDY, new ImportXzdy(fCheckDataExist)));

			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.DK, new ImportDK(fCheckDataExist)));
			if (!prm.NotImportShapeFilePrefix.Contains("JZD"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.JZD, new ImportJzd(fCheckDataExist)));
			}
			if (!prm.NotImportShapeFilePrefix.Contains("JZX"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.JZX, new ImportJzx(fCheckDataExist)));
			}
			if (!prm.NotImportShapeFilePrefix.Contains("JBNTBHQ"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.JBNTBHQ, new ImportJbntbhq(fCheckDataExist)));
			}
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.KZD, new ImportKzd(fCheckDataExist)));
			if (!prm.NotImportShapeFilePrefix.Contains("QYJX"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.QYJX, new ImportQyjx(fCheckDataExist)));
			}
			if (!prm.NotImportShapeFilePrefix.Contains("MZDW"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.MZDW, new ImportMzdw(fCheckDataExist)));
			}
			if (!prm.NotImportShapeFilePrefix.Contains("XZDW"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.XZDW, new ImportShapeBase(TableNameConstants.XZDW, "XZDW", fCheckDataExist)));
			}
			if (!prm.NotImportShapeFilePrefix.Contains("ZJ"))
			{
				tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.ZJ, new ImportShapeBase(TableNameConstants.ZJ, "ZJ", fCheckDataExist)));
			}
			tg.AddTask(new TaskImportData(prm, $"导入{QSSJ_CBJYQZDJB.GetTableName()}", new ImportQSSJ_CBJYQZDJB(fCheckDataExist)));			
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBHT, new ImportCbht(fCheckDataExist)));			
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBDKXX, new ImportCBDKXX(fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBF, new ImportCbf(fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBF_JTCY, new ImportQSSJ_CBF_JTCY(fCheckDataExist)));

			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBJYQZ, new ImportQSSJ_CBJYQZ(fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBJYQZ_QZBF, new ImportCBJYQZ_QZBF(fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBJYQZ_QZHF, new ImportCBJYQ_QZHF(fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.CBJYQZ_QZZX, new ImportMdbTableBase(TableNameConstants.CBJYQZ_QZZX, "CBJYQZ_QZZX", fCheckDataExist)));

			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.QSLYZLFJ, new ImportMdbTableBase(TableNameConstants.QSLYZLFJ, "QSLYZLFJ", fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.LZHT, new ImportMdbTableBase(TableNameConstants.LZHT, "LZHT", fCheckDataExist)));
			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.FBF, new ImportFbf(fCheckDataExist)));

			tg.AddTask(new TaskImportData(prm, "导入" + TableNameConstants.DLXX_XZDY_EXP, new ImportXzdyExp(fCheckDataExist)));
			//tg.AddTask(new CreateIndexTask());
		}
		#endregion

		public static void ClearTableData(IWorkspace db, string tableName)
		{
			try
			{
				db.ExecuteNonQuery("truncate table " + tableName);
			}
			catch { }
		}

		public static void DropIndex(IWorkspace db, string tableName, string idxName)
		{
			if (db.IsIndexExist(tableName, idxName))
			{
				db.ExecuteNonQuery("DROP INDEX " + idxName);
			}
		}

		public static void ReCreateIndex(IWorkspace db, string tableName, string idxName, string fieldName)
		{
			DropIndex(db, tableName, idxName);
			db.ExecuteNonQuery("CREATE INDEX " + idxName + " ON " + tableName + "(" + fieldName + ")");
		}

		/// <summary>
		/// 从数据库中查询已导入的行政区编码 yxm 2018-8-24
		/// </summary>
		/// <param name="db"></param>
		/// <param name="set"></param>
		/// <returns></returns>
		internal static HashSet<string> LoadXzqBms(IWorkspace db, HashSet<string> set)
		{
			var sql = "select BM from " + TableNameConstants.XZQH_XZDY + " where BM is not null";
			db.QueryCallback(sql, r =>
			{
				set.Add(r.GetString(0));
				return true;
			});
			return set;
		}


		public static Type ToType(eFieldType fieldType)
		{
			switch (fieldType)
			{
				case eFieldType.eFieldTypeInteger:
				case eFieldType.eFieldTypeOID:
					return typeof(int);
				case eFieldType.eFieldTypeSmallInteger:
					return typeof(short);
				case eFieldType.eFieldTypeGeometry:
					return typeof(IGeometry);
				case eFieldType.eFieldTypeString:
					return typeof(string);
				case eFieldType.eFieldTypeDouble:
					//return typeof(double);
					return typeof(decimal);
				case eFieldType.eFieldTypeGUID:
					return typeof(Guid);
				default:
					System.Diagnostics.Debug.Assert(false);
					break;
			}
			return null;
		}
	}
	public class CreateIndexTask : Task
	{
		class SqlIndexItem {
			public readonly string TableName;
			public readonly string IndexName;
			public readonly string Sql;
			public SqlIndexItem(string tableName, string idxName, string sql)
			{
				TableName = tableName;
				IndexName = idxName;
				Sql = sql;
			}
		}
		public CreateIndexTask()
		{
			Name = "数据库索引创建";
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var db = MyGlobal.Workspace;
			var sa = new SqlIndexItem[]{
				new SqlIndexItem(QSSJ_CBF.GetTableName(),"IX_QSSJ_CBF_LXZT",$"CREATE NONCLUSTERED INDEX[IX_QSSJ_CBF_LXZT] ON[QSSJ_CBF]([CBFBM] ASC,[CBFLX] ASC,[ZT] ASC)"),
				new SqlIndexItem(QSSJ_CBF_JTCY.GetTableName(),"IX_QSSJ_CBF_JTCY_CYXBGYR","CREATE NONCLUSTERED INDEX [IX_QSSJ_CBF_JTCY_CYXBGYR] ON [QSSJ_CBF_JTCY]([CBFBM] ASC,[CYXB] ASC,[SFGYR] ASC)"),
				new SqlIndexItem(DLXX_DK.GetTableName(),"IX_DLXX_DK_DKLBMJ","CREATE NONCLUSTERED INDEX [IX_DLXX_DK_DKLBMJ] ON [DLXX_DK]([FBFBM] ASC,[DKLB] ASC,[SCMJM] ASC)"),
				new SqlIndexItem(DLXX_DK.GetTableName(),"IX_DLXX_DK_TDLYLXMJ","CREATE NONCLUSTERED INDEX [IX_DLXX_DK_TDLYLXMJ] ON [DLXX_DK]([FBFBM] ASC,[TDLYLX] ASC,[SCMJM] ASC)"),
				new SqlIndexItem(DLXX_DK.GetTableName(),"IX_DLXX_DK_TDYTMJ","CREATE NONCLUSTERED INDEX [IX_DLXX_DK_TDYTMJ] ON [DLXX_DK]([FBFBM] ASC,[TDYT] ASC,[SCMJM] ASC)"),
				new SqlIndexItem(DLXX_DK.GetTableName(),"IX_DLXX_DK_JBNTMJ","CREATE NONCLUSTERED INDEX [IX_DLXX_DK_JBNTMJ] ON [DLXX_DK]([FBFBM] ASC,[SFJBNT] ASC,[SCMJM] ASC)"),
			};
			Progress.Reset(sa.Length);
			foreach (var it in sa)
			{
				if (!db.IsIndexExist(it.TableName, it.IndexName))
				{
					try
					{
						db.ExecuteNonQuery(it.Sql);
					}
					catch (Exception ex)
					{
						ReportError($"create index[{it.IndexName}] error:{ex}");
					}
				}
				Progress.Step();
			}
			//var sql=Resources.createIndexSql;
			//MyGlobal.Workspace.ExecuteNonQuery(sql);
			Progress.ForceFinish();
		}
	}
}
