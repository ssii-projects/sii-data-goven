using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Module.DataExchange;
using GeoAPI.Geometries;
//using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using static Agro.Library.Handle.ImportShapeAndMdb.ShapeFileImportBase;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	/// <summary>
	/// 导入界址点
	/// </summary>
	public class ImportJzd : ImportShapeBase
	{
		class BuildXtpzLshJzd
		{
			//[dkbm,max jzdh]
			private readonly Dictionary<string, int> dic = new Dictionary<string, int>();
			private int _DKBM;
			private int _JZDH;
			internal void OnDataTableCreated(DataTable dt)
			{
				for (int i = dt.Columns.Count; --i >= 0;)
				{
					switch (dt.Columns[i].ColumnName.ToUpper())
					{
						case "DKBM": _DKBM = i; break;
						case "JZDH": _JZDH = i; break;
					}
				}
			}
			internal void OnDataRowAdded(DataRow dr)
			{
				var dkbm = dr[_DKBM]?.ToString();
				var jzdh = dr[_JZDH]?.ToString();
				if (dkbm?.Length > 9)
				{
					var s = jzdh;
					if (s?.Length > 1)
					{
						var ch = s[0];
						if (ch < '0' || ch > '9')
						{
							s = s.Substring(1);
						}
						var n = SafeConvertAux.ToInt32(s);
						var key = dkbm.Substring(0, 9);
						if (dic.TryGetValue(key, out int nh))
						{
							if (n > nh)
								dic[key] = n;
						}
						else
						{
							dic[key] = n;
						}
					}
				}
			}

			internal void Build(IWorkspace db)
			{
				var repos = XtpzLshRepository.Instance;// (db);
				var odb = repos.Db;
				try
				{
					if (odb != db)
					{
						repos.ChangeSource(db);
					}
					//db.ExecuteNonQuery("delete  from XTPZ_LSH where LX='JZDH'");
					foreach (var kv in dic)
					{
						repos.UpdateLsh("JZDH", kv.Key, kv.Value);
						//XTPZ_LSHUtil.InsertMaxJzdh(db, kv.Key, kv.Value);
					}
					dic.Clear();
				}
				finally
				{
					if (odb != null) repos.ChangeSource(odb);
				}
			}
		}

		private BuildXtpzLshJzd buildXtpzLsh;
		public ImportJzd(bool fCheckDataExist = true)
			: base(TableNameConstants.JZD, "JZD", fCheckDataExist)
		{

		}

		//var str = "Data Source=192.168.10.146;Initial Catalog=ARCSUMDATA;User ID=sa;Password=123456;";
		// @"C:\Users\Public\Nwt\cache\recv\李昌松\511702通川区\矢量数据\JZD5117022016.shp";
		public override void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
		{
			int iShpRecordCount = 0;

			var db = prm.Workspace;
			if (_fCheckDataExist && !PreImportCheck(db, reportInfo))
			{
				return;
			}
			var dw = Stopwatch.StartNew();

			buildXtpzLsh = new BuildXtpzLshJzd();
			try
			{
				reportInfo.reportInfo("开始导入界址点");
				var srid = db.GetSRID(TableName);

				var shpFiles = prm.dicShp1[_shpFilePrefix];
				#region 获取shape文件总记录数
				foreach (var shpFile in shpFiles)
				{
					var prjText = ShapeFileUtil.GetPrjText(shpFile);
					if (prjText != null)
					{
						var sr = ShapeFileUtil.GetSpatialReference(shpFile);
						if (sr.AuthorityCode != srid)
						{
							throw new Exception("文件：" + shpFile + "的坐标系[" + sr.AuthorityCode + "]与数据库[" + srid + "]不一致！");
						}
					}
					using (var shp = new ShapeFile())
					{
						shp.Open(shpFile);
						iShpRecordCount += shp.GetRecordCount();
						base.RecordCount = iShpRecordCount;
					}
				}
				#endregion

				var progress = new ProgressReporter(reportInfo.reportProgress, RecordCount);

				var tableName = TableName;
				var tableMeta = db.QueryTableMeta(tableName);



				db.BeginTransaction();

				//List<ImportFieldMap> lstFieldMap = null;

				foreach (var shpFile in shpFiles)
				{
					using (var dt = new DataTable(TableName))
					{
						dt.Columns.Add("SHAPE", typeof(IGeometry));
						using (var shp = new ShapeFile())
						{
							shp.Open(shpFile);

							//if (lstFieldMap == null)
							//{
							var lstFieldMap = CreateFieldMap(db, shp, TableName);
							lstFieldMap.RemoveAll(a =>
							{
								return StringUtil.isEqualIgnorCase(a.FieldName, "BSM");
							});
							ImportFieldMap ifmDkbm = null;
							foreach (var fmi in lstFieldMap)
							{
								dt.Columns.Add(fmi.FieldName, ToType(fmi.fieldType));
								if (fmi.FieldName == "DKBM")
								{
									ifmDkbm = fmi;
								}
							}
							dt.Columns.Add("ID", typeof(string));
							buildXtpzLsh.OnDataTableCreated(dt);
							//}

							int cnt = shp.GetRecordCount();

							for (int i = 0; i < cnt; ++i)
							{
								if (cancel.Cancel())
								{
									db.Rollback();
									return;
								}
								var dr = dt.NewRow();

								var g = shp.GetGeometry(i);
								bool fShapeValid = g != null;
								if (g is LineString && g.Length == 0)
								{
									fShapeValid = false;
								}
								else if ((g is Polygon || g is MultiPolygon) && g.Area == 0)
								{
									fShapeValid = false;
								}
								if (fShapeValid)
								{
									//var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
									//var sg = SqlGeometry.STGeomFromWKB(bc, srid);
									//dr[0] = sg;
									dr[0] = g;
								}
								else
								{
									dr[0] = null;
								}

								string[] saDkbm = null;
								int cDkbm = -1;
								int c = 0;
								foreach (var it in lstFieldMap)
								{
									object o = null;
									if (it.iShpField >= 0)
									{
										o = shp.GetFieldValue(i, it.iShpField);
									}
									if (it == ifmDkbm)
									{
										if (o != null && o.ToString().IndexOf('/') > 0)
										{
											saDkbm = o.ToString().Split('/');
											o = saDkbm[0];
										}
										cDkbm = c + 1;
									}
									dr[++c] = o;
								}
								dr[++c] = Guid.NewGuid().ToString();

								AddDataRow(dt, dr);
								if (saDkbm != null)
								{
									for (int j = 1; j < saDkbm.Length; ++j)
									{
										var dr1 = dt.NewRow();
										for (int k = 0; k < c; ++k)
										{
											dr1[k] = dr[k];
										}
										var sDkbm = saDkbm[j];
										dr1[cDkbm] = sDkbm;
										dr1[c] = Guid.NewGuid().ToString();
										AddDataRow(dt, dr1);
									}
								}
								if (dt.Rows.Count >= 100000)
								{
									db.SqlBulkCopyByDatatable(tableMeta, dt);
									dt.Rows.Clear();
								}

								progress.Step();
							}
						}

						if (dt.Rows.Count > 0)
						{
							db.SqlBulkCopyByDatatable(tableMeta, dt);
							dt.Rows.Clear();
						}
					}
				}

				buildXtpzLsh.Build(db);
				db.Commit();
				progress.ForceFinish();
				dw.Stop();
				reportInfo.reportInfo("结束导入界址点，耗时：" + dw.Elapsed);
			}
			catch (Exception ex)
			{
				dw.Stop();
				Console.WriteLine(ex.Message);
				reportInfo.reportError("错误：" + ex.Message);
				db.Rollback();
			}
		}

		private void AddDataRow(DataTable dt, DataRow dr)
		{
			dt.Rows.Add(dr);
			buildXtpzLsh.OnDataRowAdded(dr);
		}
	}

}
