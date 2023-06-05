using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Model;
using System;
using System.Collections.Generic;



/*
yxm created at 2019/5/16 14:40:42
*/
namespace Agro.Module.DataSync
{
	/// <summary>
	/// 导出内网数据
	/// </summary>
	class ExportNwData : Task
	{
		class Tables
		{
			public readonly DlxxDk dk;
			public readonly DlxxDkJzd jzd;
			public readonly DlxxDkJzx jzx;
			public readonly DlxxDkTxbgjl txbgjl;

			#region yxm 2019-8-16
			public readonly DjCbjyqDjb djDjb;
			public readonly DjCbjyqCbf djCbf;
			public readonly DjCbjyqCbfJjcy djCbfJtcy;
			public readonly DjCbjyqCbht djCbht;
			public readonly DjCbjyqDksyt djDksyt;
			public readonly DjCbjyqDkxx djDkxx;
			public readonly DjCbjyqLzdk djLzdk;
			public readonly DjCbjyqLzht djLzht;
			public readonly DjCbjyqQslyzl djQslyzl;
			public readonly DjCbjyqQz djQz;
			public readonly DjCbjyqQzbf djQzbf;
			public readonly DjCbjyqQzhf djQzhf;
			public readonly DjCbjyqQzDyjl djQzdyjl;
			//public readonly DJ_CBJYQ_WTFK djWtfk;
			public readonly DjCbjyqYdjb djYdjb;
			public readonly DjYwSlsq djYwSlsj;
			public readonly QssjCbdkxx qssjDkxx;
			public readonly QssjCbf qssjCbf;
			public readonly QssjCbfBgjl qssjCbfBgjl;
			public readonly QssjCbfJtcy qssjCbfJtcy;
			public readonly QssjCbht qssjCbht;
			public readonly QssjCbjyqz qssjQz;
			public readonly QssjCbjyqzdjb qssjDjb;
			public readonly QssjCbjyqzQzbf qssjQzbf;
			public readonly QssjCbjyqzQzhf qssjQzhf;
			public readonly QssjCbjyqzQzzx qssjQzzx;
			public readonly QssjLzht qssjLzht;
			public readonly QssjFbf qssjFbf;
			#endregion

			public Tables(IWorkspace tgtDb)
			{
				var sqlFunc = tgtDb.SqlFunc;
				dk = new DlxxDk(tgtDb.SqlFunc);
				jzd = new DlxxDkJzd(tgtDb.SqlFunc);
				jzx = new DlxxDkJzx(tgtDb.SqlFunc);
				txbgjl = new DlxxDkTxbgjl(tgtDb.SqlFunc);

				#region yxm 2019-8-16
				djDjb = new DjCbjyqDjb(sqlFunc);
				djCbf = new DjCbjyqCbf(sqlFunc);
				djCbfJtcy = new DjCbjyqCbfJjcy(sqlFunc);
				djCbht = new DjCbjyqCbht(sqlFunc);
				djDksyt = new DjCbjyqDksyt(sqlFunc);
				djDkxx = new DjCbjyqDkxx(sqlFunc);
				djLzdk = new DjCbjyqLzdk(sqlFunc);
				djLzht = new DjCbjyqLzht(sqlFunc);
				djQslyzl = new DjCbjyqQslyzl(sqlFunc);
				djQz = new DjCbjyqQz(sqlFunc);
				djQzbf = new DjCbjyqQzbf(sqlFunc);
				djQzhf = new DjCbjyqQzhf(sqlFunc);
				djQzdyjl = new DjCbjyqQzDyjl(sqlFunc);
				//djWtfk = new DJ_CBJYQ_WTFK(sqlFunc);
				djYdjb = new DjCbjyqYdjb(sqlFunc);
				djYwSlsj = new DjYwSlsq(sqlFunc);
				qssjDkxx = new QssjCbdkxx(sqlFunc);
				qssjCbf = new QssjCbf(sqlFunc);
				qssjCbfBgjl = new QssjCbfBgjl(sqlFunc);
				qssjCbfJtcy = new QssjCbfJtcy(sqlFunc);
				qssjCbht = new QssjCbht(sqlFunc);
				qssjQz = new QssjCbjyqz(sqlFunc);
				qssjDjb = new QssjCbjyqzdjb(sqlFunc);
				qssjQzbf = new QssjCbjyqzQzbf(sqlFunc);
				qssjQzhf = new QssjCbjyqzQzhf(sqlFunc);
				qssjQzzx = new QssjCbjyqzQzzx(sqlFunc);
				qssjFbf = new QssjFbf(sqlFunc);
				qssjLzht = new QssjLzht(sqlFunc);
				#endregion
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
		static string GE(string fieldName, DateTime? rq)
		{
			return $"convert(date,{fieldName})>=convert(date,'{rq}')";
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = PropertyPage as ExportNwDataPanel;
			var db = MyGlobal.Workspace;
			var fileName = prm.SaveFileName;

			//int n = 0, cnt = 0;
			//double oldProgress = 0;
			using (var tgtDb = CreateSqliteDBUtil.CreateNwSyncDB(fileName))
			{
				try
				{
					var tmp = new Tables(tgtDb);

					var rq = prm.LastRiqi;
					//var sDkWh = $"ZT={(int)EDKZT.Lins}";
					var whZhxgsj = $"({GE("CJSJ",rq)} or {GE("DJSJ", rq)} or {GE("ZHXGSJ", rq)})";
					var dkWhere = whZhxgsj;
					if (prm.ExportUpdatedData)
					{
						dkWhere+= " and DKBM in(select DKBM from DJ_CBJYQ_DKXX where DJBID in (select ID from DJ_CBJYQ_DJB where DJYY = '批量更新入库'))";
					}

					tmp.dk.QueryDatas(db, cancel, dkWhere);
					if (tmp.dk.rows.Count > 0)
					{
						var wh = $"DKBM in (select distinct DKBM from DLXX_DK where {dkWhere})";
						tmp.jzd.QueryDatas(db, cancel, wh);
						tmp.jzx.QueryDatas(db, cancel, wh);
						tmp.txbgjl.QueryDatas(db, cancel, wh);
					}
					#region yxm 2019-8-16
					whZhxgsj = $"(convert(date,ZHXGSJ)>=convert(date,'{prm.LastRiqi}') or convert(date,DJSJ)>=convert(date,'{prm.LastRiqi}'))";
					var whDjb = $"QSZT<>{(int)EQszt.Lins} and {whZhxgsj}";

					if (prm.ExportUpdatedData)
					{
						whDjb += " and DJYY='批量更新入库'";
					}

					var whDjbIDIn = $"DJBID in (select distinct ID from DJ_CBJYQ_DJB where {whDjb})";
					tmp.djDjb.QueryDatas(db, cancel, whDjb);
					tmp.djCbf.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djCbfJtcy.QueryDatas(db, cancel, $"CBFBM in (select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.djCbht.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djDksyt.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djDkxx.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djLzdk.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djLzht.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djQslyzl.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djQz.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djQzbf.QueryDatas(db, cancel, $"QZID in(select distinct ID from DJ_CBJYQ_QZ where {whDjbIDIn})");
					tmp.djQzhf.QueryDatas(db, cancel, $"QZID in(select distinct ID from DJ_CBJYQ_QZ where {whDjbIDIn})");
					tmp.djQzdyjl.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djYdjb.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djYwSlsj.QueryDatas(db, cancel, $"AJZT=1 and YWH  in(select distinct YWH from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjDkxx.QueryDatas(db, cancel, $"DKBM in(select distinct DKBM from DJ_CBJYQ_DKXX where {whDjbIDIn})");
					tmp.qssjCbf.QueryDatas(db, cancel, $"CBFBM in(select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.qssjCbfBgjl.QueryDatas(db, cancel, $"CBFBM in(select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.qssjCbfJtcy.QueryDatas(db, cancel, $"CBFBM in(select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.qssjCbht.QueryDatas(db, cancel, $"CBHTBM in(select distinct CBHTBM from DJ_CBJYQ_CBHT where {whDjbIDIn})");
					tmp.qssjQz.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjDjb.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjQzbf.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjQzbf.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjQzzx.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjLzht.QueryDatas(db, cancel, $"CBHTBM in(select distinct CBHTBM from DJ_CBJYQ_LZHT where {whDjbIDIn})");

					whZhxgsj = $"convert(date,ZHXGSJ)>=convert(date,'{prm.LastRiqi}')";
					tmp.qssjFbf.QueryDatas(db, cancel, whZhxgsj);
					#endregion

					int cnt = tmp.dk.rows.Count + tmp.jzd.rows.Count + tmp.jzx.rows.Count + tmp.txbgjl.rows.Count
						+ tmp.djDjb.RowCount + tmp.djCbf.RowCount + tmp.djCbfJtcy.RowCount + tmp.djCbht.RowCount
						+ tmp.djDksyt.RowCount + tmp.djDkxx.RowCount + tmp.djLzdk.RowCount + tmp.djLzht.RowCount
						+ tmp.djQslyzl.RowCount + tmp.djQz.RowCount + tmp.djQzbf.RowCount + tmp.djQzhf.RowCount
						+ tmp.djQzdyjl.RowCount + /*tmp.djWtfk.RowCount +*/ tmp.djYdjb.RowCount + tmp.djYwSlsj.RowCount
						+ tmp.qssjDkxx.RowCount + tmp.qssjCbf.RowCount + tmp.qssjCbfBgjl.RowCount + tmp.qssjCbfJtcy.RowCount
						+ tmp.qssjCbht.RowCount + tmp.qssjQz.RowCount + tmp.qssjDjb.RowCount + tmp.qssjQzbf.RowCount
						+ tmp.qssjQzhf.RowCount + tmp.qssjQzzx.RowCount + tmp.qssjLzht.RowCount + tmp.qssjFbf.RowCount;

					if (cnt== 0)
					{
						ReportProgress(100);
					}
					else
					{
						var progress = new ProgressReporter(ReportProgress, cnt);
						tgtDb.BeginTransaction();

						ExportUtil.DoExport(tgtDb, tmp.dk, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.jzd, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.jzx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.txbgjl, () => progress.Step());

						#region yxm 2019-8-16
						ExportUtil.DoExport(tgtDb, tmp.djDjb, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djCbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djCbfJtcy, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djCbht, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djDksyt, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djDkxx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djLzdk, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djLzht, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQslyzl, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQz, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQzbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQzhf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQzdyjl, () => progress.Step());
						//ExportUtil.DoExport(tgtDb, tmp.djWtfk, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djYdjb, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djYwSlsj, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjDkxx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbfBgjl, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbfJtcy, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbht, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQz, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjDjb, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQzbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQzhf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQzzx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjFbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjLzht, () => progress.Step());

						#endregion


						tgtDb.Commit();
					}
					ReportInfomation($"共导出数据{cnt}条");
				}
				catch (Exception ex)
				{
					//UIHelper.ShowExceptionMessage(ex);
					base.ReportException(ex);
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

		protected readonly string PrimaryKey;
		protected InsertBase(ISqlFunction sqlFunc, string tableName, string subFields, string primaryKey="ID")
		{
			TableName = tableName;
			SubFields = subFields;
			PrimaryKey = primaryKey;

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

			string IDFieldName = PrimaryKey;

			int iIDField = _insertPrms.FindIndex(p =>
			{
				return p.ParamName == IDFieldName;// "ID";
			});
			var lstIDs = new List<string>();
			foreach (var r in srcRows)
			{
				lstIDs.Add(r[iIDField].ToString());
			}

			SqlUtil.ConstructIn(lstIDs, sin =>
			 {
				 var sql = $"select {SubFields} from {TableName} where {IDFieldName} in({sin})";
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
				if (p.ParamName ==IDFieldName)// "ID")
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
					try
					{
						tgtDb.ExecuteNonQuery(this.insertSql, this._insertPrms);
						++nInserts;
					}
					catch (Exception ex)
					{
						throw new Exception($"{IDFieldName}='{id}':"+ex.Message);
					}
				}
				else
				{
					if (!IsEqual(r, tr,saFieldType))
					{//update
						var sql = $"{updateSql} where {IDFieldName}='{id}'";
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

			SqlUtil.ConstructIn(lstIDs, sin =>
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
	class DlxxDk : InsertBase
	{
		public DlxxDk(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK",
			"ID,YSDM,FBFBM,DKBM,DKMC,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,SFQQQG,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,CBFMC,ZJRXM,KJZB,ZT,DJZT,DYZT,YYZT,LZZT,ZHXGSJ,CJSJ,DJSJ,MSSJ,SJLY"
			,"DKBM")
		{
		}
	}
	class DlxxDkJzd : InsertBase
	{
		public DlxxDkJzd(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK_JZD",
			"ID,YSDM,DKBM,JZDH,JZDLX,JBLX,XZBZ,YZBZ")
		{
		}
	}
	class DlxxDkJzx : InsertBase
	{
		public DlxxDkJzx(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK_JZX",
			"ID,YSDM,DKBM,JZXH,JXXZ,JZXLB,JZXWZ,JZXSM,PLDWQLR,PLDWZJR,QJZDH,ZJZDH")
		{
		}
	}
	class DlxxDkTxbgjl : InsertBase
	{
		public DlxxDkTxbgjl(ISqlFunction sqlFunc) : base(sqlFunc, "DLXX_DK_TXBGJL",
			"ID,DKID,DKBM,YDKID,YDKBM,BGFS,BGYY")
		{
		}
	}

}
