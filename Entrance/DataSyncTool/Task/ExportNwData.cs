using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Data.SQLite;



/*
yxm created at 2019/5/16 14:40:42
*/
namespace DataSyncTool
{
	/// <summary>
	/// 导出内网数据
	/// </summary>
	class ExportNwData : Task
	{
		class Tables
		{
			public readonly DLXX_DK dk;
			public readonly DLXX_DK_JZD jzd;
			public readonly DLXX_DK_JZX jzx;
			public readonly DLXX_DK_TXBGJL txbgjl;
			public Tables(IWorkspace tgtDb)
			{
				dk = new DLXX_DK(tgtDb.SqlFunc);
				jzd = new DLXX_DK_JZD(tgtDb.SqlFunc);
				jzx = new DLXX_DK_JZX(tgtDb.SqlFunc);
				txbgjl = new DLXX_DK_TXBGJL(tgtDb.SqlFunc);
			}
		}
		public ExportNwData()
		{
			Name = "导出内网数据包";
			Description = "导出内网未登记的脱密数据";
			PropertyPage = new ExportNwDataPanel();
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) => ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = PropertyPage as ExportNwDataPanel;
			var db = prm.db;
			var fileName = prm.SaveFileName;
			//SQLiteTransaction trans = null;


			int n = 0, cnt = 0;
			double oldProgress = 0;
			using (var tgtDb = CreateSqliteDBUtil.CreateNwSyncDB(fileName))
			{
				try
				{
					var tmp = new Tables(tgtDb);

					var sDkWh = $"ZT={(int)EDKZT.Lins}";
					var whZhxgsj = $"convert(date,CJSJ)>=convert(date,'{prm.LastRiqi}')";
					tmp.dk.QueryDatas(db, cancel, $"{sDkWh} and {whZhxgsj}");

					if (tmp.dk.rows.Count == 0)
					{
						ReportProgress(100);
					}
					else
					{
						tgtDb.BeginTransaction();

						var wh = $"DKBM in (select distinct DKBM from DLXX_DK where {sDkWh})";
						tmp.jzd.QueryDatas(db, cancel, wh);
						tmp.jzx.QueryDatas(db, cancel, wh);
						tmp.txbgjl.QueryDatas(db, cancel, wh);

						cnt = tmp.dk.rows.Count + tmp.jzd.rows.Count + tmp.jzx.rows.Count + tmp.txbgjl.rows.Count;

						ExportUtil.DoExport(tgtDb, tmp.dk, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.jzd, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.jzx, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.txbgjl, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));

						tgtDb.Commit();
					}
					ReportInfomation($"共导出数据{cnt}条");
				}
				catch (Exception ex)
				{
					UIHelper.ShowExceptionMessage(ex);
					tgtDb.Rollback();
				}
			}

		}
	}
	class Row : List<object>
	{
	}
	class InsertBase
	{
		internal readonly string insertSql;
		internal readonly List<SQLParam> _insertPrms = new List<SQLParam>();
		internal readonly string SubFields;
		internal readonly string TableName;

		internal readonly List<Row> rows = new List<Row>();
		protected InsertBase(ISqlFunction sqlFunc, string tableName,string subFields)
		{
			TableName = tableName;
			SubFields = subFields;

			var a = sqlFunc.GetParamPrefix();
			var sql = $"insert into {TableName}({subFields})values(";
			foreach (var fieldName in subFields.Split(','))
			{
				_insertPrms.Add(new SQLParam() { ParamName = fieldName });
				sql += $"{a}{fieldName},";
			}
			sql = $"{sql.TrimEnd(',')})";
			insertSql = sql;
		}

		public int RowCount { get { return rows.Count; } }
		public void QueryDatas(IFeatureWorkspace db, ICancelTracker cancel, string wh)
		{
			var sql = $"select {SubFields} from {TableName}";
			if (wh != null)
			{
				sql += $" where {wh}";
			}
			ExportUtil.QueryDatas(db, this, sql, cancel);
		}

		/// <summary>
		///导入外网数据 (将SQLite中的数据导入到SQLServer中)
		///将外网导出的数据导入到内网数据库
		/// </summary>
		/// <param name="srcDb"></param>
		/// <param name="tgtDb"></param>
		/// <param name="cancel"></param>
		public void ImportWwData(ITask task, IWorkspace srcDb, IWorkspace tgtDb,ICancelTracker cancel,Action calback)
		{
			var srcRows = new List<Row>();
			var tgtRows = new Dictionary<string, Row>();

			ExportUtil.QueryDatas(srcDb, this, $"select {SubFields} from {TableName}", cancel);
			srcRows.AddRange(this.rows);
			rows.Clear();

			int iIDField = _insertPrms.FindIndex(p =>
			{
				return p.ParamName == "ID";
			});
			var lstIDs = new List<string>();
			foreach (var r in srcRows)
			{
				lstIDs.Add(r[iIDField].ToString());
			}

			SqlHelper.ConstructIn(lstIDs, sin =>
			 {
				 var sql = $"select {SubFields} from {TableName} where ID in({sin})";
				 ExportUtil.QueryDatas(tgtDb, this, sql, cancel);
				 foreach (var r in rows)
				 {
					 var id = r[iIDField].ToString();
					 tgtRows[id] = r;
				 }
				 rows.Clear();
			 });

			var saFieldType = new Type[_insertPrms.Count];
			var dicFieldType=tgtDb.QueryFieldsType(TableName);
			for (int i = 0; i < saFieldType.Length; ++i)
			{
				saFieldType[i] = dicFieldType[_insertPrms[i].ParamName];
			}

			var dicUpdateParams = new Dictionary<string, SQLParam>();
			var updateParams = new List<SQLParam>();
			var updateSql = $"update {TableName} set ";
			var a = tgtDb.SqlFunc.GetParamPrefix();
			foreach (var p in _insertPrms)
			{
				if (p.ParamName == "ID")
					continue;
				updateSql += $"{p.ParamName}={a}{p.ParamName},";
				updateParams.Add(p);
				dicUpdateParams[p.ParamName] = p;
			}
			updateSql =$"{updateSql.TrimEnd(',')}";

			int nInserts = 0, nUpdates = 0;
			foreach (var r in srcRows)
			{
				calback();
				var id = r[iIDField].ToString();
				if (!tgtRows.TryGetValue(id, out Row tr))
				{
					for (int i = 0; i < r.Count; ++i)
					{
						this._insertPrms[i].ParamValue = r[i];
					}
					tgtDb.ExecuteNonQuery(this.insertSql, this._insertPrms);
					++nInserts;
				}
				else
				{
					if (!IsEqual(r, tr,saFieldType))
					{//update
						var sql = $"{updateSql} where ID='{id}'";
						for (int i = 0; i < r.Count; ++i)
						{
							var fieldName = _insertPrms[i].ParamName;
							if (dicUpdateParams.TryGetValue(fieldName, out SQLParam p))
							{
								p.ParamValue = r[i];
							}
						}
						tgtDb.ExecuteNonQuery(sql, updateParams);
						Console.WriteLine(sql);
						++nUpdates;
					}
				}
			}

			var msg = $"结束导入{TableName}";
			if (nInserts > 0)// || nUpdates > 0)
			{
				msg += $"，插入{nInserts}条数据";
			}
			if (nUpdates > 0)
			{
				msg += $"，修改{nUpdates}条数据";
			}
			task.ReportInfomation(msg);
		}

		/// <summary>
		///导入内网数据 (将SQLite中的数据导入到SQLServer中)
		///将内网导出的数据导入到外网数据库
		/// </summary>
		/// <param name="srcDb"></param>
		/// <param name="tgtDb"></param>
		/// <param name="cancel"></param>
		public void ImportNwData(ITask task, IWorkspace srcDb, IWorkspace tgtDb, ICancelTracker cancel, Action callback)
		{
			var tgtIds = new HashSet<string>();

			ExportUtil.QueryDatas(srcDb, this, $"select {SubFields} from {TableName}", cancel);
			if (cancel.Cancel()) return;

			int iIDField = _insertPrms.FindIndex(p =>
			{
				return p.ParamName == "ID";
			});
			var lstIDs = new List<string>();
			foreach (var r in rows)
			{
				lstIDs.Add(r[iIDField].ToString());
			}

			SqlHelper.ConstructIn(lstIDs, sin =>
			{
				var sql = $"select ID from {TableName} where ID in({sin})";
				tgtDb.QueryCallback(sql, r =>
				 {
					 tgtIds.Add(r.GetString(0));
					 return true;
				 },cancel);
				if (cancel.Cancel()) return;
			});

			int nInserts = 0;
			foreach (var r in rows)
			{
				callback();
				var id = r[iIDField].ToString();
				if (!tgtIds.Contains(id))
				{
					for (int i = 0; i < r.Count; ++i)
					{
						this._insertPrms[i].ParamValue = r[i];
					}
					tgtDb.ExecuteNonQuery(this.insertSql, this._insertPrms);
					++nInserts;
				}
				if (cancel.Cancel()) return;
			}

			var msg = $"结束导入{TableName}";
			if (nInserts > 0)
			{
				msg += $"，插入{nInserts}条数据";
			}
			task.ReportInfomation(msg);
		}

		private static bool IsEqual(Row a, Row b,Type[] types)
		{
			for (int i = 0; i < a.Count; ++i)
			{
				var oa = a[i];
				var ob = b[i];
				if (oa == null && ob == null)
					continue;
				if (oa == null || ob == null)
					return false;
				var type = types[i];
				if (type == typeof(double) || type == typeof(float))
				{
					var da = double.Parse(oa.ToString());
					var db = double.Parse(ob.ToString());
					if (da != db)
					{
						return false;
					}
				}
				else if (type == typeof(int) || type == typeof(short))
				{
					var da = int.Parse(oa.ToString());
					var db = int.Parse(ob.ToString());
					if (da != db)
					{
						return false;
					}
				}
				else if (type == typeof(DateTime))
				{
					var da = DateTime.Parse(oa.ToString());
					var db = DateTime.Parse(ob.ToString());
					if (da.Year!=db.Year||da.Month!=db.Month||da.Day!=db.Day
						||da.Hour!=db.Hour||da.Minute!=db.Minute||da.Second!=db.Second)
					{
						return false;
					}
				}
				else
				{
					if (oa.ToString().Trim() != ob.ToString().Trim())
						return false;
				}
			}
			return true;
		}
	}
	class DLXX_DK : InsertBase
	{
		public DLXX_DK(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK",
			"ID,YSDM,FBFBM,DKBM,DKMC,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,SFQQQG,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,CBFMC,ZJRXM,KJZB,ZT,DJZT,DYZT,YYZT,LZZT,ZHXGSJ,CJSJ,DJSJ,MSSJ,SJLY")
		{
		}
	}
	class DLXX_DK_JZD : InsertBase
	{
		public DLXX_DK_JZD(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK_JZD",
			"ID,YSDM,DKBM,JZDH,JZDLX,JBLX,XZBZ,YZBZ")
		{
		}
	}
	class DLXX_DK_JZX : InsertBase
	{
		public DLXX_DK_JZX(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK_JZX",
			"ID,YSDM,DKBM,JZXH,JXXZ,JZXLB,JZXWZ,JZXSM,PLDWQLR,PLDWZJR,QJZDH,ZJZDH")
		{
		}
	}
	class DLXX_DK_TXBGJL : InsertBase
	{
		public DLXX_DK_TXBGJL(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK_TXBGJL",
			"ID,DKID,DKBM,YDKID,YDKBM,BGFS,BGYY")
		{
		}
	}

}
