using Agro.GIS;
using Agro.Library.Common;
using Agro.Module.DataExchange;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Data;
using static Agro.Library.Handle.ImportShapeAndMdb.ShapeFileImportBase;
using GeoAPI.Geometries;
//using Microsoft.SqlServer.Types;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	/// <summary>
	/// 导入调查地块（汇交后的地块数据）
	/// 格式与汇交数据的矢量地块数据格式相同，地块编码只提供前14位编码，后5位自编码程序生成
	/// </summary>
	public class ImportDcdk : ImportShapeBase
    {
        public class MyInputParams: InputParam
        {
            public string DkShapeFile;
            public MyInputParams(string dkShpFile)
            {
                DkShapeFile = dkShpFile;
            }
        }
        class DkbmBuilder
        {
            private readonly Dictionary<string, int> _dicDkbmLsh = new Dictionary<string, int>();
            public int GetNextLsh(IWorkspace db, string dkbmPrefix)
            {
                if (dkbmPrefix.Length > 14)
                {
                    dkbmPrefix = dkbmPrefix.Substring(0, 14);
                }
                int n;
                if(!_dicDkbmLsh.TryGetValue(dkbmPrefix,out n))
                {
                    n = -1;
                    var sql = "select max(dkbm) from dlxx_dk where dkbm like '" + dkbmPrefix + "%'";
                    db.QueryCallback(sql, r =>
                    {
                        if (!r.IsDBNull(0))
                        {
                            var s=SafeConvertAux.ToStr(r.GetValue(0));
                            n=SafeConvertAux.ToInt32(s.Substring(14))+1;
                            _dicDkbmLsh[dkbmPrefix] = n;
                        }
                        return false;
                    });
                }else
                {
                    _dicDkbmLsh[dkbmPrefix] = ++n;
                }
                return n;
            }
            public void Clear()
            {
                _dicDkbmLsh.Clear();
            }
        }
        class DkbmCache
        {
            private readonly List<string> _dicShpRow2Dkbm = new List<string>();
            private readonly DkbmBuilder _builder = new DkbmBuilder();
            public string GetDkbmByShpRow(int row)
            {
                return _dicShpRow2Dkbm[row];
            }
            public string build(IWorkspace db, ShapeFile shp,int row,int iDkbmField)
            {
                var s=shp.GetFieldString(row, iDkbmField);
                if (s == null)
                {
                    return "shape 文件中第" + (row + 1) + "块地无地块编码！";
                }
                if (s.Length < 14)
                {
                    return "shape 文件中第" + (row + 1) + "块地的地块编码长度小于14位！";
                }
                s = s.Substring(0, 14);
                var n=_builder.GetNextLsh(db, s);
                if (n == -1)
                {
                    return "shape 文件中第" + (row + 1) + "块地的地块编码的所在地域在数据库中不存在！";
                }
                var s1 = (100000 + n).ToString().Substring(1);
                s += s1;
                _dicShpRow2Dkbm.Add(s);
                return null;
            }
            public void Clear()
            {
                _dicShpRow2Dkbm.Clear();
            }
        }


        private readonly DkbmCache dkbmCache = new DkbmCache();

        public ImportDcdk()
            : base(TableNameConstants.DK, "DK")
        {
            base._shapeFileName = "Shape";
            this.OnPreImport += (prm, lst) =>
            {
                lst.Add(new ShapeFileImportBase.ImportFieldMap("ID", -1, eFieldType.eFieldTypeString));
            };
            int iDkbmCol = -1;
            int iIDCol = -1;

            base.OnDataTableCreated += dt =>
            {
                DataTable d = dt;
                for (int c = 0; c < d.Columns.Count; ++c)
                {
                    switch (d.Columns[c].ColumnName)
                    {
                        case "DKBM":iDkbmCol = c;break;
                        case "ID": iIDCol = c; break;
                    }
                };
            };
            base.OnRowDataFilled += (dr,iShpOID) =>
            {
                dr[iIDCol]= Guid.NewGuid().ToString();
                var sDkbm = dkbmCache.GetDkbmByShpRow(iShpOID);
                dr[iDkbmCol] = sDkbm;
            };
        }

        public void Import(string dkShpFile, Action<string> reportError, Action<string> reportInfo, Action<double> reportProgress,ICancelTracker cancel,Action<string> reportWaring)
        {
            var ri = new ReportInfo();
            ri.reportProgress += i =>reportProgress((int)i);
            ri.reportError += msg =>reportError(msg);
            ri.reportInfo += msg =>reportInfo(msg);
			ri.reportWarning += msg => reportWaring?.Invoke(msg);
            var prm = new MyInputParams(dkShpFile);
            this.DoImport(prm, ri,cancel);
        }

        public override void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
        {
            var realPrm = prm as MyInputParams;
            int iShpRecordCount = 0;

			var progress = new ProgressReporter(reportInfo.reportProgress, iShpRecordCount);
			try
            {
                dkbmCache.Clear();
                var getValPrm = new GetValueParam();
                var db = MyGlobal.Workspace;
                {
                    var srid = db.GetSRID(TableName);

                    var shpFile = realPrm.DkShapeFile;// prm.dicShp1[_shpFilePrefix];
                    #region 获取shape文件总记录数
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

							progress.Reset(iShpRecordCount);

							int iDkbmField = shp.FindField("DKBM");
                            if (iDkbmField < 0)
                            {
                                throw new Exception("文件：" + shpFile + "不存在DKBM字段！");
                            }
                            var fieldType=shp.GetFieldType(iDkbmField);
                            if (fieldType != DBFFieldType.FTString)
                            {
                                throw new Exception("文件：" + shpFile + "的DKBM字段不是文本类型！");
                            }
                            for(int i = 0; i < iShpRecordCount; ++i)
                            {
                               var err= dkbmCache.build(db, shp, i, iDkbmField);
                                if (err != null)
                                {
                                    throw new Exception(err);
                                }
                            }
                        }
                    }
                    #endregion

                    var tableName = TableName;
                    var tableMeta = db.QueryTableMeta(tableName);

                    var dt = new DataTable(TableName);

                    dt.Columns.Add("SHAPE", typeof(IGeometry));

					List<ImportFieldMap> lstFieldMap = null;
					using (var shp = new ShapeFile())
					{
						shp.Open(shpFile);
						getValPrm.shp = shp;
						if (lstFieldMap == null)
						{
							lstFieldMap = CreateFieldMap(db, shp, TableName);
							lstFieldMap.RemoveAll(a =>
							{
								return StringUtil.isEqualIgnorCase(a.FieldName, "BSM");
							});
							OnPreImport(prm, lstFieldMap);
							foreach (var fmi in lstFieldMap)
							{
								dt.Columns.Add(fmi.FieldName, ToType(fmi.fieldType));
							}
							OnDataTableCreated(dt);
						}

						int cnt = shp.GetRecordCount();

						for (int i = 0; i < cnt; ++i)
						{
							var dr = dt.NewRow();

							//dr[0] = oid++;
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

							getValPrm.row = i;
							int c = 0;
							foreach (var it in lstFieldMap)
							{
								object o = null;
								getValPrm.fieldMap = it;
								getValPrm.Handled = false;
								if (it.iShpField >= 0)
								{
									o = shp.GetFieldValue(i, it.iShpField);
								}
								dr[++c] = o;
							}

							OnRowDataFilled?.Invoke(dr, i);
							dt.Rows.Add(dr);

							if (dt.Rows.Count >= 10000)
							{
								db.SqlBulkCopyByDatatable(tableMeta, dt);
								//db.GetNextObjectID(tableName, dt.Rows.Count - 1);
								dt.Rows.Clear();
							}

							progress.Step();
							//ProgressUtil.ReportProgress(reportInfo.reportProgress, iShpRecordCount, ++iProgress, ref oldProgress);
						}
					}
					if (dt.Rows.Count > 0)
					{
                        db.SqlBulkCopyByDatatable(tableMeta, dt);
                        //db.GetNextObjectID(tableName, dt.Rows.Count - 1);
                        dt.Rows.Clear();
                    }
                }
				progress.ForceFinish();
                //ProgressUtil.ReportProgress(reportInfo.reportProgress, iShpRecordCount, iShpRecordCount, ref oldProgress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                reportInfo.reportError("错误：" + ex.Message);
            }
        }
    }
}
