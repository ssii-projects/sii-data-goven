using GeoAPI.Geometries;
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Data;
//using Microsoft.SqlServer.Types;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 合库任务（）
	/// yxm 2018-11-29
	/// </summary>
	public class ImportSDEDataTask : GroupTask
  {
		class ImportTableTask : Task
		{
			protected IWorkspace _srcDB,_tgtDB;
			protected readonly string _tableName;
			protected readonly string _keyFieldName;
			protected List<Field> _fields;
			public ImportTableTask(string tableName,string keyFieldName)
			{
				base.Name = "导入"+tableName;

				_tableName = tableName;
				_keyFieldName = keyFieldName;

				base.OnStart += s => base.ReportInfomation("开始" + Name);
				base.OnFinish += (s, e) => base.ReportInfomation("结束" + Name + "，耗时：" + base.Elapsed);
			}
			public void Init(IWorkspace db)
			{
				_srcDB = db;
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				bool fBrgionTransaction = false;
				try
				{
					_tgtDB = MyGlobal.Workspace;
					_fields = _srcDB.QueryFields2(_tableName);
					if (HasImported())
					{
						base.ReportWarning("数据已存在");
						base.ReportProgress(100);
						return;
					}
					var tgtTableMeta = _tgtDB.QueryTableMeta(_tableName);

					#region 排除名称不同的字段（考虑测试等原因后期可能添加过字段）
					var lstField = _tgtDB.QueryFields2(_tableName);
					for (int i = _fields.Count - 1; i >= 0; --i)
					{
						//if (_fields[i].FieldType == eFieldType.eFieldTypeOID
						//	|| StringUtil.isEqualIgnorCase(_fields[i].FieldName, "BSM"))
						//{
						//	_fields.RemoveAt(i);
						//	continue;
						//}
						var fi = lstField.Find(a => { return StringUtil.isEqualIgnorCase(a.FieldName, _fields[i].FieldName); });
						if (fi == null)
						{
							_fields.RemoveAt(i);
						}
					}
					#endregion


					var dt = GetDataTable(out string subFields,out List<Field> fields);
					int srid = _srcDB.GetSRID("DLXX_DK");
					int srid1 = _tgtDB.GetSRID("DLXX_DK");
					if (srid != srid1)
					{
						base.ReportError("元数据库坐标系[" + srid + "]与目的数据库坐标系[" + srid1 + "]不一致");
						return;
					}
					var cnt = SafeConvertAux.ToInt32(_srcDB.QueryOne("select count(1) from " + _tableName));

					_tgtDB.BeginTransaction();
					fBrgionTransaction = true;
					double oldProgress = 0;
					int iProgress = 0;
					var sql = "select " + subFields + " from " + _tableName;
					_srcDB.QueryCallback(sql, r =>
					 {
						 var dr = dt.NewRow();
						 for (int i = 0; i < fields.Count; ++i)
						 {
							 var o = r.GetValue(i);
							 if (o != null)
							 {
								 switch (fields[i].FieldType)
								 {
									 case eFieldType.eFieldTypeGeometry:
										 {
											 //SqlGeometry sg = null;
											 //if (o is IGeometry g)
											 //{
												// var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
												// sg = SqlGeometry.STGeomFromWKB(bc, srid);
											 //}
											 //o = sg;
										 }
										 break;
								 }
							 }
							 if(o==null)
							 {
								 o = DBNull.Value;
							 }

							 dr[i] = o;
						 }
						 dt.Rows.Add(dr);
						 if (dt.Rows.Count > 50000)
						 {
							 _tgtDB.SqlBulkCopyByDatatable(tgtTableMeta, dt);
							 dt.Rows.Clear();
						 }
						 ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++iProgress, ref oldProgress);
						 return true;
					 }, cancel);

					if (dt.Rows.Count > 0)
					{
						_tgtDB.SqlBulkCopyByDatatable(tgtTableMeta, dt);
						dt.Rows.Clear();
					}
					ProgressUtil.ReportProgress(base.ReportProgress, cnt, cnt, ref oldProgress);
					_tgtDB.Commit();
				}
				catch (Exception ex)
				{

					base.ReportException(ex);
					if (fBrgionTransaction)
					{
						_tgtDB.Rollback();
					}
				}
			}
			protected DataTable GetDataTable(out string subFields,out List<Field> fields)
			{
				subFields = null;
				fields = new List<Field>();
				var dt = new DataTable(_tableName);
				for (int i = 0; i < _fields.Count; ++i)
				{
					var fi = _fields[i];
					if (fi.FieldType == eFieldType.eFieldTypeOID)
					{
						continue;
					}
					if (StringUtil.isEqualIgnorCase(fi.FieldName, "BSM"))
					{
						continue;
					}
					var t = FieldsUtil.ToDataTableType(fi.FieldType);
					if (fi.FieldType == eFieldType.eFieldTypeGeometry)
					{
						t = typeof(IGeometry);
					}
					dt.Columns.Add(new DataColumn(fi.FieldName, t));
					fields.Add(fi);
					if (subFields == null)
					{
						subFields = fi.FieldName;
					}
					else
					{
						subFields += "," + fi.FieldName;
					}
				}
				return dt;
			}
			protected bool HasImported()
			{
				if (string.IsNullOrEmpty(_keyFieldName))
				{
					return false;
				}
				bool fStrField = false;
				var field = _fields.Find(a => { return StringUtil.isEqualIgnorCase(a.FieldName, _keyFieldName); });
				if (field != null && field.FieldType == eFieldType.eFieldTypeString) {
					fStrField = true;
				}

				var sql = "select TOP(1) "+_keyFieldName+" from "+_tableName;
				if (StringUtil.isEqualIgnorCase(_tableName, "DLXX_XZDY"))
				{
					sql += " where JB=4";
				}
				var v = SafeConvertAux.ToStr(_srcDB.QueryOne(sql));
				sql = "select count(1) from " + _tableName + " where " + _keyFieldName + "=";
				if (fStrField)
				{
					sql += "'" + v + "'";
				}
				else
				{
					sql += v;
				}

				var n = SafeConvertAux.ToInt32(_tgtDB.QueryOne(sql));
				return n>0;
			}
		}
		class ImportTableXzdy : ImportTableTask
		{
			public ImportTableXzdy() : base("DLXX_XZDY", "BM")
			{
			}

			protected override void DoGo(ICancelTracker cancel)
			{
				bool fBrgionTransaction = false;
				try
				{
					_tgtDB = MyGlobal.Workspace;
					_fields = _srcDB.QueryFields2(_tableName);
					if (HasImported())
					{
						base.ReportWarning("数据已存在");
						base.ReportProgress(100);
						return;
					}

					#region 排除不同的字段（考虑测试等原因后期可能添加过字段）
					var lstField = _tgtDB.QueryFields2(_tableName);
					for (int i = _fields.Count - 1; i >= 0; --i)
					{
						var fi = lstField.Find(a => { return StringUtil.isEqualIgnorCase(a.FieldName, _fields[i].FieldName); });
						if (fi == null)
						{
							_fields.RemoveAt(i);
						}
					}
					#endregion
					var dt = GetDataTable(out string subFields,out List<Field> fields);
					int srid = _srcDB.GetSRID("DLXX_DK");
					int srid1 = _tgtDB.GetSRID("DLXX_DK");
					if (srid != srid1)
					{
						base.ReportError("元数据库坐标系[" + srid + "]与目的数据库坐标系[" + srid1 + "]不一致");
						return;
					}
					var wh = " where JB<=4";

					var sql = "select count(1) from " + _tableName;
					var cnt=SafeConvertAux.ToInt32( _tgtDB.QueryOne("select count(1) from "+ _tableName));
					if (cnt == 0)
					{
						wh = "";
					}

					cnt = SafeConvertAux.ToInt32(_srcDB.QueryOne(sql+wh));

					var tableMeta = _tgtDB.QueryTableMeta(_tableName);

					_tgtDB.BeginTransaction();
					fBrgionTransaction = true;
					double oldProgress = 0;
					int iProgress = 0;
					sql = "select " + subFields + " from " + _tableName+wh;
					_srcDB.QueryCallback(sql, r =>
					{
						var dr = dt.NewRow();
						for (int i = 0; i < fields.Count; ++i)
						{
							var o = r.GetValue(i);
							if (o != null)
							{
								switch (fields[i].FieldType)
								{
									case eFieldType.eFieldTypeGeometry:
										//{
										//	SqlGeometry sg = null;
										//	if (o is IGeometry g)
										//	{
										//		var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
										//		sg = SqlGeometry.STGeomFromWKB(bc, srid);
										//	}
										//	o = sg;
										//}
										break;
								}
							}
							if (o == null)
							{
								o = DBNull.Value;
							}

							dr[i] = o;
						}
						dt.Rows.Add(dr);
						if (dt.Rows.Count > 50000)
						{
							_tgtDB.SqlBulkCopyByDatatable(tableMeta, dt);
							dt.Rows.Clear();
						}
						ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++iProgress, ref oldProgress);
						return true;
					}, cancel);

					if (dt.Rows.Count > 0)
					{
						_tgtDB.SqlBulkCopyByDatatable(tableMeta, dt);
						dt.Rows.Clear();
					}
					ProgressUtil.ReportProgress(base.ReportProgress, cnt, cnt, ref oldProgress);
					_tgtDB.Commit();
				}
				catch (Exception ex)
				{

					base.ReportException(ex);
					if (fBrgionTransaction)
					{
						_tgtDB.Rollback();
					}
				}
			}


		}
		class ImportDBTask : Agro.LibCore.Task.GroupTask
		{
			private readonly string _conStr;
			public ImportDBTask(string conStr)
			{
				_conStr = conStr;
				using (var db = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(conStr))
				{
					var sql = "select MC from DLXX_XZDY where JB=4";
					var xmc = SafeConvertAux.ToStr(db.QueryOne(sql));
					this.Name = "导入"+xmc+"数据";
				}
				base.AddTask(new ImportTableXzdy());
				base.AddTask(new ImportTableTask("DLXX_DK", "DKBM"));
				base.AddTask(new ImportTableTask("DLXX_DK_JZD", "DKBM"));
				base.AddTask(new ImportTableTask("DLXX_DK_JZX", "DKBM"));
				base.AddTask(new ImportTableTask("DLXX_DK_TXBGJL", "DKBM"));
				base.AddTask(new ImportTableTask("DLXX_DZDW", "QXDM"));
				base.AddTask(new ImportTableTask("DLXX_JBNTBHQ", null));
				base.AddTask(new ImportTableTask("DLXX_KZD", null));
				base.AddTask(new ImportTableTask("DLXX_MZDW", "QXDM"));
				base.AddTask(new ImportTableTask("DLXX_QYJX", "SZDY"));
				base.AddTask(new ImportTableTask("DLXX_XZDW", "QXDM"));
				base.AddTask(new ImportTableTask("DLXX_XZDY_EXP", "ZJBM"));
				base.AddTask(new ImportTableTask("DLXX_ZJ", "QXDM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_CBF", "CBFBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_CBF_JTCY", "CBFBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_CBHT", "CBHTBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_DJB", "QXDM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_DKSYT",null));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_DKXX", "DKBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_LZDK", "DKBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_LZHT", "SZDY"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_QSLYZL", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_QZ", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_QZ_DYJL", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_QZBF", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_QZHF", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("DJ_CBJYQ_YDJB", null));
				base.AddTask(new ImportTableTask("DJ_DYAQ", null));
				base.AddTask(new ImportTableTask("DJ_DYAQ_DYHT", null));
				base.AddTask(new ImportTableTask("DJ_DYAQ_DYR", null));
				base.AddTask(new ImportTableTask("DJ_DYAQ_JKR", null));
				base.AddTask(new ImportTableTask("DJ_DYAQ_QLR", null));
				base.AddTask(new ImportTableTask("DJ_JFZC", "QXDM"));
				base.AddTask(new ImportTableTask("DJ_JFZC_BSQR", null));
				base.AddTask(new ImportTableTask("QSSJ_CBDKXX", "DKBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBF", "CBFBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBF_BGJL", "CBFBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBF_BGJL", "CBFBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBF_JTCY", "CBFBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBHT", "CBHTBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBJYQZ", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBJYQZ_QZBF", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBJYQZ_QZHF", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBJYQZ_QZZX", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("QSSJ_CBJYQZDJB", "CBJYQZBM"));
				base.AddTask(new ImportTableTask("QSSJ_FBF", "FBFBM"));
				base.AddTask(new ImportTableTask("QSSJ_LZHT", "LZHTBM"));
				base.AddTask(new ImportTableTask("QSSJ_QSLYZLFJ", "CBJYQZBM"));
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				using (var db = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(_conStr))
				{
					foreach (var t in base.Tasks)
					{
						if (t is ImportTableTask it)
						{
							it.Init(db);
						}
					}
					base.DoGo(cancel);
				}
			}
		}
		public ImportSDEDataTask(TaskPage page)
		{
			base.Name = "合库工具";
			base.Description = "导入县级承包经营权数据";
			base.IsExpanded = true;
			base.PropertyPage = new ImportSDEDataPanel(lst=>
			{
				this.ClearTasks();
				foreach (var s in lst)
				{
					AddTask(new ImportDBTask(s));
				}
			});

			base.OnStart += t =>base.ReportInfomation("开始" + Name);
			base.OnFinish += (t,e) =>base.ReportInfomation("结束" + Name + "，耗时：" + base.Elapsed);
		}
	}
}
