using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Module.DataExchange;
using GeoAPI.Geometries;
//using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using static Agro.Library.Handle.ImportShapeAndMdb.ShapeFileImportBase;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	/// <summary>
	/// 导入界址线
	/// </summary>
	public class ImportJzx : ImportShapeBase
	{
		class BuildXtpzLshJzx
		{
			//[dkbm,max jzxh]
			private readonly Dictionary<string, int> dic = new Dictionary<string, int>();
			private int _DKBM;
			private int _JZXH;
			internal void OnDataTableCreated(DataTable dt)
			{
				for (int i = dt.Columns.Count; --i >= 0;)
				{
					switch (dt.Columns[i].ColumnName.ToUpper())
					{
						case "DKBM": _DKBM = i; break;
						case "JZXH": _JZXH = i; break;
					}
				}
			}
			internal void OnDataRowAdded(DataRow dr)
			{
				var dkbm = dr[_DKBM]?.ToString();
				var jzdh = dr[_JZXH]?.ToString();
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
				var repos = XtpzLshRepository.Instance;
				var odb = repos.Db;
				repos.ChangeSource(db);
				try
				{
					//db.ExecuteNonQuery("delete  from XTPZ_LSH where LX='JZXH'");
					foreach (var kv in dic)
					{
						repos.UpdateLsh("JZXH", kv.Key, kv.Value);
						//XTPZ_LSHUtil.InsertMaxJzxh(db, kv.Key, kv.Value);
					}
					dic.Clear();
				}
				finally
				{
					repos.ChangeSource(odb);
				}
			}
		}

		private BuildXtpzLshJzx buildXtpzLsh;
		public ImportJzx(bool fCheckDataExist = true)
						: base(TableNameConstants.JZX, "JZX", fCheckDataExist)
		{

		}

		public override void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
		{
			int iShpRecordCount = 0;

			var db = prm.Workspace;

			if (_fCheckDataExist && !PreImportCheck(db, reportInfo))
			{
				return;
			}

			try
			{
				buildXtpzLsh = new BuildXtpzLshJzx();
				var dw = Stopwatch.StartNew();
				reportInfo.reportInfo("开始导入界址线");

				var tableName = TableName;
				var tableMeta = db.QueryTableMeta(tableName);
				var srid = db.GetSRID(tableName);

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

				var progress = new ProgressReporter(reportInfo.reportProgress, iShpRecordCount);


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
								//if (g != null)
								//{
								//	var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
								//	var sg = SqlGeometry.STGeomFromWKB(bc, srid);
								//	dr[0] = sg;
								//}
								//else
								//{
								//	dr[0] = null;
								//}
								dr[0] = g;

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
										//dr1[0] = oid++;
										for (int k = 0; k < c; ++k)
										{
											dr1[k] = dr[k];
										}
										var sDkbm = saDkbm[j];
										dr1[cDkbm] = sDkbm;
										dr1[c] = Guid.NewGuid().ToString();
										//dt.Rows.Add(dr1);
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
				reportInfo.reportInfo("结束导入界址线，耗时：" + dw.Elapsed);
			}
			catch (Exception ex)
			{
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

		//var str = "Data Source=192.168.10.146;Initial Catalog=ARCSUMDATA;User ID=sa;Password=123456;";
		// @"C:\Users\Public\Nwt\cache\recv\李昌松\511702通川区\矢量数据\JZD5117022016.shp";
		//public override void DoImport(string connectionString, InputParams prm, ReportInfo reportInfo
		//     , bool fClearAllOldData = false)
		//{
		//    try
		//    {
		//        //var sw = Stopwatch.StartNew();
		//        var dkShpFile = prm.dicShp[_shpFilePrefix];// GetShapeFileName(prm);//.kzdShpFileName;
		//        using (var shp = new ShapeFile())
		//        // using (var db = new SqlServer())
		//        using (var db = DataBaseSource.GetDatabase())
		//        {
		//            if (!PreImportCheck(db, reportInfo))
		//            {
		//                return;
		//            }
		//            //db.Connect(connectionString);
		//            shp.Open(dkShpFile);
		//            var sTableName = TableName;
		//            var imp = new ShapeFileToSQLServer();
		//            var fm = ShapeFileImportBase.CreateFieldMap(db, shp, sTableName);
		//            fm.Add(new ShapeFileImportBase.ImportFieldMap("ID", -1, eFieldType.eFieldTypeGUID));
		//            int iIDCol = -1;
		//            imp.DoImport(db, sTableName, shp, fm, (pr, c, dr) =>
		//            {
		//                if (iIDCol == -1)
		//                {
		//                    if (pr.fieldMap.FieldName == "ID")
		//                    {
		//                        iIDCol = c;
		//                    }
		//                }
		//                if (iIDCol == c)
		//                {
		//                    pr.Handled = true;
		//                    return Guid.NewGuid();
		//                }
		//                pr.Handled = false;
		//                return null;
		//            }, reportInfo, fClearAllOldData, "OBJECTID", "Shape");
		//        }
		//        //sw.Stop();
		//        //reportInfo.reportInfo("耗时：" + sw.Elapsed);
		//    }
		//    catch (Exception ex)
		//    {
		//        Console.WriteLine(ex.Message);
		//        reportInfo.reportError("错误：" + ex.Message);
		//    }
		//}
	}

}
