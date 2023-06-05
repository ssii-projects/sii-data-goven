using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using Agro.Library.Common.Service;
using Agro.Library.Model;
using Agro.Module.DataExchange;
using GeoAPI.Geometries;
//using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using static Agro.Library.Handle.ImportShapeAndMdb.ShapeFileImportBase;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	public class ImportDK : ImportShapeBase
	{
		internal class Item
		{
			/// <summary>
			/// 承包方名称
			/// </summary>
			public string sCbfMc;
			/// <summary>
			/// 发包方编码
			/// </summary>
			public string sFbfBm;
			public EDjzt djzt = EDjzt.Wdj;
			public DateTime? Djsj;
			public decimal? Elhtmjm;
			public decimal? Qqmjm;
			public string Sfqqqg;
		}

		//class ContractLandLsh
		//{
		//	private readonly Dictionary<string, int> dic = new Dictionary<string, int>();
		//	public void Add(string fbfBm, string dkBm)
		//	{
		//		int bh = SafeConvertAux.ToInt32(dkBm.Right(5)) + 1;
		//		var key = fbfBm;
		//		if (dic.TryGetValue(key, out int n))
		//		{
		//			if (bh > n)
		//			{
		//				dic[key] = bh;
		//			}
		//		}
		//		else
		//		{
		//			dic[key] = bh;
		//		}
		//	}

		//	public void UpdateLsh(IWorkspace db)
		//	{
		//		var repos = new XtpzLshRepository(db);
		//		foreach (var kv in dic)
		//		{
		//			repos.UpdateLsh("CONTRACTLAND", kv.Key, kv.Value);
		//		}
		//		dic.Clear();
		//	}
		//}

		class Idx
		{
			public int iSCMJField = -1;//实测面积字段
			public int iSCMJMField = -1;//实测面积亩
			public int iCbfmc = -1;
			public int iDkbmCol = -1;
			public int iIDCol = -1;
			public int iFbfBmCol = -1;
			//public int iDjsj = -1;
			public int iCjsj = -1;
			public int DJZT;
			public int DJSJ;
			public int ELHTMJ;
			public int QQMJ;
			public int SFQQQG;
			public int SFJBNT;
			public int ZT;

			public void Reset(DataTable dt)
			{
				for (int i = 0; i < dt.Columns.Count; ++i)
				{
					var c = dt.Columns[i];
					switch (c.ColumnName)
					{
						case "CBFMC": iCbfmc = i; break;
						case "ID": iIDCol = i; break;
						case "FBFBM": iFbfBmCol = i; break;
						case "DKBM": iDkbmCol = i; break;
						case "SCMJ": iSCMJField = i; break;
						case "SCMJM": iSCMJMField = i; break;
						//case "DJSJ":iDjsj = i;break;
						case "CJSJ": iCjsj = i; break;
						case "DJZT": DJZT = i; break;
						case "DJSJ": DJSJ = i; break;
						case "ELHTMJ": ELHTMJ = i; break;
						case "QQMJ": QQMJ = i; break;
						case "SFQQQG": SFQQQG = i; break;
						case "SFJBNT": SFJBNT = i; break;
						case "ZT":ZT= i; break;
					}
				}

				System.Diagnostics.Debug.Assert(iCjsj >= 0);
			}
		}
		static object DbVal(object o)
		{
			return o ?? DBNull.Value;
		}

		private readonly XtpzLshService lshService = new XtpzLshService();
		public ImportDK(bool fCheckDataExist = true)
			: base(TableNameConstants.DK, "DK", fCheckDataExist)
		{
			base._shapeFileName = "Shape";

			var idx = new Idx();
			DateTime kssj = DateTime.Now.AddDays(-1);

			bool Exist(List<ImportFieldMap> lst, string fieldName)
			{
				return lst.Find(t => StringUtil.isEqualIgnorCase(t.FieldName, fieldName)) != null;
			}

			var setFbfbm = new HashSet<string>();
			//var setYdjDkbm = new Dictionary<string, object>();
			

			Dictionary<string, Item> dicDkbm2Cfmc = null;

			this.OnPreImport += (prm, lst) =>
			{
				if (!Exist(lst, "CBFMC"))
				{
					lst.Add(new ImportFieldMap("CBFMC", -1, eFieldType.eFieldTypeString));
				}
				lst.Add(new ImportFieldMap("ID", -1, eFieldType.eFieldTypeString));
				if (!Exist(lst, "FBFBM"))
				{
					lst.Add(new ImportFieldMap("FBFBM", -1, eFieldType.eFieldTypeString));
				}
				lst.Add(new ImportFieldMap("CJSJ", -1, eFieldType.eFieldTypeDateTime));
				if (!Exist(lst, "SCMJM"))
					lst.Add(new ImportFieldMap("SCMJM", -1, eFieldType.eFieldTypeDouble));

				dicDkbm2Cfmc = QueryDkbm2Item(prm.mdbFileName, setFbfbm);//, setYdjDkbm);

				kssj = new DateTime(prm.Year, 1, 1);
			};


			this.OnDataTableCreated += dt =>
			{
				dt.Columns.Add("DJZT", typeof(int));
				dt.Columns.Add("DJSJ", typeof(DateTime));
				dt.Columns.Add("ELHTMJ", typeof(decimal));
				dt.Columns.Add("QQMJ", typeof(decimal));
				dt.Columns.Add("SFQQQG", typeof(string));
                dt.Columns.Add("ZT", typeof(int));
                idx.Reset(dt);
			};

			this.OnRowDataFilled += (dr, iShpOID) =>
			{
				var sDkbm = dr[idx.iDkbmCol]?.ToString();
				string sFbfBm = null;
				Item it;
				#region fill 承包方名称和发包方编码
				if (sDkbm != null)
				{
					dr[idx.iIDCol] = sDkbm;// Guid.NewGuid().ToString();
					var o = dr[idx.iFbfBmCol];
					sFbfBm = o != null && o != DBNull.Value ? o.ToString() : null;
					if (dicDkbm2Cfmc.TryGetValue(sDkbm, out it))
					{
						dr[idx.iCbfmc] = it.sCbfMc;
						if (sFbfBm == null)
						{
							var fbfbm = string.IsNullOrEmpty(it.sFbfBm) ? sDkbm.Substring(0, 14) : it.sFbfBm;
							dr[idx.iFbfBmCol] = fbfbm;
							sFbfBm = fbfbm;
						}
					}
					else
					{
						it = null;
						if (sFbfBm == null)
						{
							sFbfBm = sDkbm.Substring(0, 14);
							dr[idx.iFbfBmCol] = sFbfBm;
							if (!setFbfbm.Contains(sFbfBm))
							{
								throw new Exception("地块编码为" + sDkbm + "的地块无发包方编码");
							}
						}
					}
				}
				else
				{
					throw new Exception("第" + iShpOID + "行的地块无地块编码");
				}
				#endregion

				EDjzt djzt =it?.djzt??EDjzt.Wdj;
				
				dr[idx.DJZT] = (int)( djzt);
				dr[idx.ELHTMJ] = DbVal(it?.Elhtmjm);
				dr[idx.QQMJ] = DbVal(it?.Qqmjm);
				dr[idx.SFQQQG] =DbVal(it?.Sfqqqg);
				dr[idx.DJSJ] = DbVal(it?.Djsj);
				dr[idx.ZT] = (int)EDKZT.Youxiao;

                if (dr[idx.SFJBNT] == null || dr[idx.SFJBNT] == DBNull.Value)
				{
					dr[idx.SFJBNT] = "";
				}

				#region fill 实测面积亩

				var sSCMJM = SafeConvertAux.ToDouble(dr[idx.iSCMJMField]);
				if (sSCMJM <= 0)
				{
					sSCMJM = Math.Round(SafeConvertAux.ToDouble(dr[idx.iSCMJField]) * 0.0015, 2);
					dr[idx.iSCMJMField] = sSCMJM;
				}
				#endregion

				#region 开始时间
				dr[idx.iCjsj] = kssj;
				#endregion

				lshService.AddDk(sFbfBm, sDkbm);
			};
		}

		public override void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
		{
			int iShpRecordCount = 0;

			var getValPrm = new GetValueParam();
			var db = prm.Workspace;
			if (_fCheckDataExist && !PreImportCheck(db, reportInfo))
			{
				return;
			}
			DataTable dt = null;
			try
			{
				var srid = db.GetSRID(TableName);
				var spatialReference = SpatialReferenceFactory.CreateFromEpsgCode(srid);

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

				var tableName = TableName;
				var tableMeta = db.QueryTableMeta(tableName);
				var progress = new ProgressReporter(reportInfo.reportProgress, iShpRecordCount);
				db.BeginTransaction();


				foreach (var shpFile in shpFiles)
				{
                    using var shp = new ShapeFile();
                    shp.Open(shpFile);
                    getValPrm.shp = shp;

                    using (dt = new DataTable(TableName))
                    {
                        dt.Columns.Add("SHAPE", typeof(IGeometry));

                        var lstFieldMap = CreateFieldMap(db, shp, TableName);
                        lstFieldMap.RemoveAll(a =>
                        {
                            return StringUtil.isEqualIgnorCase(a.FieldName, "BSM");
                        });
                        OnPreImport(prm, lstFieldMap);
                        foreach (var fmi in lstFieldMap)
                        {
                            dt.Columns.Add(fmi.FieldName, ToType(fmi.fieldType));
                        }
                        OnDataTableCreated?.Invoke(dt);

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
                            //	g = g.Project(spatialReference);
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
                                object? o = null;
                                getValPrm.fieldMap = it;
                                getValPrm.Handled = false;
                                if (it.iShpField >= 0)
                                {
                                    o = shp.GetFieldValue(i, it.iShpField);
                                }
                                dr[++c] = o ?? DBNull.Value;
                            }

                            OnRowDataFilled?.Invoke(dr, i);
                            dt.Rows.Add(dr);

                            if (dt.Rows.Count >= 10000)
                            {
                                db.SqlBulkCopyByDatatable(tableMeta, dt);
                                dt.Rows.Clear();
                            }

                            progress.Step();
                        }

                        if (dt.Rows.Count > 0)
                        {
                            db.SqlBulkCopyByDatatable(tableMeta, dt);
                            dt.Rows.Clear();
                        }
                    }
                }


				lshService.UpdateLsh(db);
				db.Commit();
				progress.ForceFinish();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				reportInfo.reportError("错误：" + ex.Message);
				db.Rollback();
			}
		}

		/// <summary>
		/// [dkbm,
		/// </summary>
		/// <param name="mdbFile"></param>
		/// <param name="setFbfbm"></param>
		/// <returns></returns>
		internal static Dictionary<string, Item> QueryDkbm2Item(string mdbFile, HashSet<string> setFbfbm)//, Dictionary<string, object> setYdjdkbm)
		{
			var dic = new Dictionary<string, Item>();
			using (var db = DBAccess.Open(mdbFile))
			{
				var sql = "select a.dkbm,b.cbfmc,a.FBFBM,a.YHTMJM,a.HTMJM,a.SFQQQG from CBDKXX a left join CBF b on a.CBFBM=b.CBFBM where a.dkbm is not null and b.cbfmc is not null";
				#region 查找承包方名称
				db.QueryCallback(sql, r =>
				{
					var dkbm = r.GetString(0);
					var cbfmc = r.GetString(1);
					var fbfbm = r.IsDBNull(2) ? null : r.GetString(2);
					decimal? yhtmj = null;
					if(!r.IsDBNull(3))yhtmj=(decimal)SafeConvertAux.ToDouble(r.GetValue(3));
					decimal? htmj = null;
					if (!r.IsDBNull(4)) htmj =(decimal)SafeConvertAux.ToDouble(r.GetValue(4));
					var sfqqqg = r.IsDBNull(5) ? null : r.GetString(5);
					if (!dic.TryGetValue(dkbm, out Item item))
					{
						dic[dkbm] = new Item() { sCbfMc = cbfmc, sFbfBm = fbfbm, Elhtmjm = yhtmj,Qqmjm=htmj, Sfqqqg=sfqqqg};
					}
					else
					{
						if (!string.IsNullOrEmpty(item.sCbfMc))
						{
							var s = item.sCbfMc + "/" + cbfmc;
							if (s.Length < 180)// 200)
							{//承包方名称字段长度为200
								item.sCbfMc = s;// "/" + cbfmc;
							}
						}
						else
						{
							item.sCbfMc = cbfmc;
						}
					}
				});
				#endregion

				sql = "select distinct fbfbm from fbf where fbfbm is not null";
				db.QueryCallback(sql, r =>
				{
					var fbfbm = r.GetString(0);
					setFbfbm.Add(fbfbm);
					return true;
				});


				var dicQzbm2Djsj = new Dictionary<string, object>();
				sql = "select CBJYQZBM,DJSJ from CBJYQZDJB";
				db.QueryCallback(sql, r =>
				 {
						 dicQzbm2Djsj[r.GetString(0)] = r.GetValue(1);
				 });
				sql = "select DKBM,CBJYQZBM from CBDKXX";
				db.QueryCallback(sql, r =>
				 {
					 var dkbm = r.GetString(0);
					 var qzbm = r.GetString(1);
					 if (dicQzbm2Djsj.TryGetValue(qzbm, out object o))
					 {
						 if (dic.TryGetValue(dkbm, out Item it))
						 {
							 it.djzt = EDjzt.Ydj;
							 if (o is DateTime dt)
								 it.Djsj = dt;
						 }
						 //setYdjdkbm[dkbm] = o;
					 }
				 });
			}
			return dic;
		}
	}

	class DkRepos
	{
		class ShapeFileItem
		{
			private readonly Dictionary<string, int> dicDkbm2Oid = new Dictionary<string, int>();
			private readonly IFeatureClass featureClass;

			public ShapeFileItem(string shpFile)
			{
				featureClass=ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(shpFile);
				int iDKBMField = -1;
				featureClass.Search(SqlUtil.MakeQueryFilter("DKBM," + featureClass.OIDFieldName), r =>
				   {
					   if (iDKBMField == -1)
					   {
						   iDKBMField = r.Fields.FindField("DKBM");
					   }
					   var dkbm=r.GetValue(iDKBMField)?.ToString();
					   if (dkbm == null)
						   throw new Exception($"{shpFile}中的DKBM不允许null值");
					   dicDkbm2Oid[dkbm] = r.Oid;
				   });
			}
			public IFeature GetFeatureByDkbm(string dkbm)
			{
				if (dicDkbm2Oid.TryGetValue(dkbm, out int oid))
				{
					return featureClass.GetFeatue(oid);
				}
				return null;
			}

			public void Dispose()
			{
				dicDkbm2Oid.Clear();
				featureClass.Dispose();
			}
		}

		private readonly List<ShapeFileItem> _lst = new List<ShapeFileItem>();

		public DkRepos(InputParam prm)
		{
			var shpFiles = prm.dicShp1["DK"];
			foreach (var shpFile in shpFiles)
			{
				_lst.Add(new ShapeFileItem(shpFile));
			}
		}

		public IFeature GetFeatureByDkbm(string dkbm)
		{
			foreach (var it in _lst)
			{
				var ft = it.GetFeatureByDkbm(dkbm);
				if (ft != null)
					return ft;
			}
			return null;
		}

		public void Dispose()
		{
			foreach (var it in _lst)
			{
				it.Dispose();
			}
			_lst.Clear();
		}
	}

}
