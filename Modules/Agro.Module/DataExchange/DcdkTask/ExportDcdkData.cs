using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using Agro.LibCore;
using System.IO;
using Agro.Library.Common;
using Agro.GIS;
using Agro.LibCore.Database;
using Agro.Library.Model;
using Agro.Library.Common.Repository;
using static Agro.Module.DataExchange.SelectFbfPanel;
using GeoAPI.Geometries;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 导出地块数据
	/// </summary>
	public class ExportDkData : ExportDkDataBase
	{
		
        public ExportDkData()
        {
            base.Name = "导出地块数据";
            base.Description = "导出符合农业部要求格式的调查地块数据";
            base.PropertyPage = new ExportDkDataPropertyPage();
			base.OnFinish += (t,s) =>base.ReportInfomation($"耗时：{t.Elapsed}");
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = (ExportDkDataPropertyPage)base.PropertyPage;
            try
            {
                var dkShpFile = d.ExportFilePath;
                var nRecordCount=DoExportDk(d.Fbfbm, d,cancel);
				base.ReportInfomation($"共导出{nRecordCount}条记录");
            }
            catch (Exception ex)
            {
                base.ReportException(ex);
            }
        }

		private int DoExportDk(string fbfbm, ExportDkDataPropertyPage prm,  ICancelTracker cancel)
		{
			if (string.IsNullOrEmpty(fbfbm))
			{
				throw new Exception("未输入发包方编码！");
			}

			var srcRepos = DlxxDkRepository.Instance;


			int nRecordCount = srcRepos.Count(t => t.ZT == EDKZT.Youxiao && t.FBFBM.StartsWith(fbfbm));
			
			if (nRecordCount == 0)
			{
				throw new Exception("发包方编码" + fbfbm + "无效！");
			}

			var srid = srcRepos.GetSrid();

			var db = MyGlobal.Workspace;

            #region yxm 2022-12-5 检查数据库中是否存在表BDC_JR_DK，目前仅枝江市包含该业务
            var isTableBDC_JR_DKExists = db.IsTableExists(BDC_JR_DK.GetTableName());
            #endregion

            using (var tgtFc = CreateTgtFeatureClass(prm, srid,fbfbm,out IFeatureClass? fcDcDk))
			{
				try
				{
					//var progress = new ProgressReporter(ReportProgress, nRecordCount);
					Progress.Reset(nRecordCount+1, "导出地块");

					tgtFc.Workspace.BeginTransaction();

					var lstEn = new List<DLXX_DK>();
					var lstFields = EntityUtil.GetIntersectionAttributes<VEC_SURVEY_DK, DLXX_DK>((t, u) =>t.FieldName==u.FieldName).Select(t =>t.FieldName).ToList();
					lstFields.Add(nameof(DLXX_DK.FBFBM));
					if (prm.DatabaseType == eDatabaseType.SQLite)
					{
						lstFields.Add(nameof(DC_DLXX_DK.DJZT));
					}
					var subFields = SubFields.Make(lstFields);
					//srcRepos.FindCallback(t => t.ZT == EDKZT.Youxiao && t.FBFBM == fbfbm, en =>
					srcRepos.FindCallback1(t => t.ZT == EDKZT.Youxiao && t.FBFBM.StartsWith(fbfbm), it =>lstEn.Add(it.Item), subFields, false, cancel);

					var dicDkbm2BDCDYH=new Dictionary<string, string>();//[DKBM,不动产单元号] 2022-12-5

					var dicDkbm2Cbfbm = new Dictionary<string, string>();
					SqlUtil.ConstructIn(lstEn, sin =>
					   {
						   db.QueryCallback($"select DKBM,CBFBM from {QSSJ_CBDKXX.GetTableName()} where DKBM in({sin})", r =>
							{
								var dkbm = r.GetString(0);
								var cbfbm = SafeConvertAux.ToStr(r.GetValue(1));
								dicDkbm2Cbfbm[dkbm] = cbfbm;
							});

                           #region yxm 2022-12-5 如果数据库中存在表BDC_JR_DK，目前仅枝江市包含该业务
                           if (isTableBDC_JR_DKExists)
						   {
                               db.QueryCallback($"select DKBM,BDCDYH from {BDC_JR_DK.GetTableName()} where DKBM in({sin})", r =>
                               {
                                   var dkbm = r.GetString(0);
                                   var bdcDyh = SafeConvertAux.ToStr(r.GetValue(1));
                                   dicDkbm2BDCDYH[dkbm] = bdcDyh;
                               });
                           }
                           #endregion
                       }, en=>en.DKBM);

					var ft = tgtFc.CreateFeature();
					Envelope? env = null;
					foreach (var en in lstEn)
					{
						if (!dicDkbm2Cbfbm.TryGetValue(en.DKBM, out string? cbfbm))
						{
							cbfbm= null;
						}
                        IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.CBFBM), cbfbm);

                        #region yxm 2022-12-5 如果数据库中存在表BDC_JR_DK，目前仅枝江市包含该业务
                        if (isTableBDC_JR_DKExists)
						{
							if (!dicDkbm2BDCDYH.TryGetValue(en.DKBM, out string? bdcDyh))
							{
								bdcDyh = null;
							}
							IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.BDCDYH), bdcDyh);
						}
                        #endregion

                        if (en.Shape?.IsEmpty == false)
						{
							if (env == null)
							{
								env = en.Shape.EnvelopeInternal;
							}
							else
							{
								env.ExpandToInclude(en.Shape.EnvelopeInternal);
							}
						}
						EntityUtil.WriteToFeature(en, ft);
						IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.FBFDM), en.FBFBM);
						tgtFc.Append(ft);
						Progress.Step();
					}

					if (env != null&& fcDcDk!=null)
					{
						base.ExportYDk(fcDcDk, env, lstFields, cancel);
					}

					tgtFc.Workspace.Commit();



					Progress.ForceFinish();

					fcDcDk?.Dispose();
				}
				catch (Exception ex)
				{
					tgtFc.Workspace.Rollback();
					throw ex;
				}
			}

			return nRecordCount;
		}

		//private void ExportXzdy(IFeatureClass tgtFc, string fbfbm)
		//{
		//	try
		//	{
		//		var repos = DlxxXzdyRepository.Instance;
		//		var sXiang = fbfbm.Substring(0, 9);
		//		var where=DLXX_XZDY.MakeWhere(t => (int)t.JB < (int)eZoneLevel.County&&t.BM.StartsWith(sXiang));
		//		var cnt = repos.Count(where);// t => (int)t.JB < 5);
		//		Progress.Reset(cnt, "导出行政地域");
		//		//if (fbfbm.Length==14&&fbfbm.EndsWith("00"))
		//		//{
		//		//	fbfbm = fbfbm.Substring(0,fbfbm.Length - 2);
		//		//}
		//		var lstEn = repos.FindAll(where);
		//		tgtFc.Workspace.BeginTransaction();

		//		var ft = tgtFc.CreateFeature();
		//		foreach (var en in lstEn)
		//		{
		//			EntityUtil.WriteToFeature(en, ft);
		//			tgtFc.Append(ft);
		//			Progress.Step();
		//		}

		//		tgtFc.Workspace.Commit();
		//	}
		//	catch (Exception ex)
		//	{
		//		tgtFc.Workspace.Rollback();
		//	}
		//}

		private IFeatureClass CreateTgtFeatureClass(ExportDkDataPropertyPage prm, int srid,string fbfbm,out IFeatureClass? fcDcDK)
		{
			string dkFileName = prm.ExportFilePath;
			if (prm.DatabaseType==eDatabaseType.SQLite)
			{
				return base.CreateTgtFeatureClass(dkFileName, srid, fbfbm, out fcDcDK);
				//using (var ws = SqliteFeatureWorkspaceFactory.Instance.CreateWorkspace(dkFileName))
				//{
				//	void OnPreCreateFeatureClass(IFields fields)
				//	{
				//		for (int i = fields.FieldCount; --i >= 0;)
				//		{
				//			var field = fields.GetField(i);
				//			if (StringUtil.isEqualIgnorCase(field.FieldName , nameof(VEC_SURVEY_DK.ELHTMJ)))
				//			{
				//				field.AliasName = "二轮合同面积";
				//			}
				//		}
				//		if (fields.FindField("DJZT") < 0)
				//		{
				//			FieldsUtil.AddIntField(fields, "DJZT", "登记状态");
				//		}
				//	}
				//	fc = VEC_SURVEY_DK.CreateFeatureClass(ws, srid, OnPreCreateFeatureClass);
				//	fcDcDK = DC_DLXX_DK.CreateFeatureClass(ws, srid, OnPreCreateFeatureClass);
				//	XTPZ_SJZD.CreateTable(ws);
				//	XTPZ_TDLYXZFL.CreateTable(ws);
				//	CS_SYSINFO.CreateTable(ws);
				//	DC_QSSJ_CBF.CreateTable(ws);
				//	DC_QSSJ_CBF_JTCY.CreateTable(ws);
				//	DC_QSSJ_FBF.CreateTable(ws);
				//	ExportTable(ws, XTPZ_SJZD.GetTableName(), XTPZ_SJZD.GetFieldsString());
				//	ExportTable(ws, XTPZ_TDLYXZFL.GetTableName(), XTPZ_TDLYXZFL.GetFieldsString());
				//	ExportTable(ws, CS_SYSINFO.GetTableName(), CS_SYSINFO.GetFieldsString());
				//	var cbfFields = DC_QSSJ_CBF.GetFieldsString();
					
				//	ExportTable(ws, QSSJ_CBF.GetTableName(), cbfFields, DC_QSSJ_CBF.GetTableName(),$"FBFBM like '{fbfbm}%'",p=>
				//	{
				//		if (StringUtil.isEqualIgnorCase(p.ParamName, "ZHXGSJ"))
				//		{
				//			p.ParamValue = DBNull.Value;
				//		}
				//	});
				//	var cbfJtcyFields = DC_QSSJ_CBF_JTCY.GetFieldsString();
				//	ExportTable(ws, QSSJ_CBF_JTCY.GetTableName(), cbfJtcyFields, DC_QSSJ_CBF_JTCY.GetTableName(), $"CBFBM in(select CBFBM from QSSJ_CBF where FBFBM like '{fbfbm}%')");
				//	//ExportTable(ws, QSSJ_FBF.GetTableName(), DC_QSSJ_FBF.GetFieldsString(), DC_QSSJ_FBF.GetTableName(), $"FBFBM like '{fbfbm.Substring(0,12)}%'");
				//	ExportTable(ws, QSSJ_FBF.GetTableName(), DC_QSSJ_FBF.GetFieldsString(), DC_QSSJ_FBF.GetTableName(), $"FBFBM like '{fbfbm}%'");
				//	using (var fcXzdy = DLXX_XZDY.CreateFeatureClass(ws, srid))
				//	{
				//		ExportXzdy(fcXzdy, fbfbm);
				//	}
				//}
			}
			else
			{
				if (File.Exists(dkFileName))
				{
					ShapeFileUtil.DeleteShapeFile(dkFileName);
				}
				fcDcDK = null;

				var fContainFieldBDCDYH = MyGlobal.Workspace.IsTableExists(BDC_JR_DK.GetTableName());

				ShapeFileFeatureWorkspaceFactory.ParseShpFileName(dkFileName, out string cons, out string tableName);
                using var ws = ShapeFileFeatureWorkspaceFactory.Instance.OpenWorkspace(cons);

                ws.CreateFeatureClass(tableName, VEC_SURVEY_DK.ConvertToFields(it =>
				{
					if(it.Tag != null) { return false; }
					var isBdckyhField = nameof(VEC_SURVEY_DK.BDCDYH).Equals(it.FieldName, StringComparison.CurrentCultureIgnoreCase);
					if(isBdckyhField)
					{
						return fContainFieldBDCDYH;
					}
					return true;
				}), srid);
                return ws.OpenFeatureClass(tableName, "rb+");
            }
		}

		
	}

	public abstract class ExportDkDataBase : Task
	{
		protected void ExportXzdy(IFeatureClass tgtFc, string fbfbm)
		{
			try
			{
				var repos = DlxxXzdyRepository.Instance;
				var sXiang = fbfbm.Substring(0, 12);
				var where = DLXX_XZDY.MakeWhere(t => (int)t.JB < (int)eZoneLevel.County && t.BM.StartsWith(sXiang));
				var cnt = repos.Count(where);
				Progress.Reset(cnt, "导出行政地域");
				var lstEn = repos.FindAll(where);
				tgtFc.Workspace.BeginTransaction();

				var ft = tgtFc.CreateFeature();
				foreach (var en in lstEn)
				{
					EntityUtil.WriteToFeature(en, ft);
					tgtFc.Append(ft);
					Progress.Step();
				}

				tgtFc.Workspace.Commit();
			}
			catch (Exception ex)
			{
				tgtFc.Workspace.Rollback();
			}
		}

		/// <summary>
		/// 导出原地块
		/// </summary>
		/// <param name="tgtFc"></param>
		/// <param name="env"></param>
		/// <param name="subFields"></param>
		/// <param name="cancel"></param>
		protected void ExportYDk(IFeatureClass tgtFc, Envelope env, List<string> subFields, ICancelTracker cancel)
		{
			env.ExpandBy(50);
			var srcRepos = DlxxDkRepository.Instance;
			var ft1 = tgtFc.CreateFeature();
			using (var srcFc = MyGlobal.Workspace.OpenFeatureClass(DLXX_DK.GetTableName()))
			{
				var qf = new SpatialFilter()
				{
					Geometry = GeometryUtil.MakePolygon(env),
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects,
					SubFields = string.Join(",", subFields),
					WhereClause = $"ZT={(int)EDKZT.Youxiao}"
				};
				srcFc.SpatialQuery(qf, ft =>
				{
					CopyValues(ft, ft1);

					tgtFc.Append(ft1);
				}, cancel);
			}
		}

		protected void ExportTable(IFeatureWorkspace tgtWS, string tableName, string fields, string tgtTableName = null, string wh = null, Action<SQLParam> callback = null)
		{
			var srcDb = MyGlobal.Workspace;
			var lstPrms = new List<List<SQLParam>>();
			var sql = $"select count(1) from {tableName}"+(string.IsNullOrEmpty(wh)?"":$" where {wh}");
			var cnt = srcDb.QueryOneInt(sql);
			Progress.Reset(cnt, $"导出{tableName}");
			sql = $"SELECT {fields}   FROM {tableName}" + (string.IsNullOrEmpty(wh) ? "" : $" where {wh}");
			srcDb.QueryCallback(sql, r =>
			{
				var sa = fields.Split(',');
				var lst = new List<SQLParam>();
				for (int i = 0; i < sa.Length; ++i)
				{
					var p = new SQLParam()
					{
						ParamName = sa[i].Trim(),
						ParamValue = r.GetValue(i),
					};
					callback?.Invoke(p);
					lst.Add(p);
				}
				lstPrms.Add(lst);
			});

			var vals = fields;
			var a = tgtWS.SqlFunc.GetParamPrefix();
			vals = a + vals.Replace(",", $",{a}");
			sql = $"insert into {tgtTableName ?? tableName}({fields}) values({vals})";
			try
			{
				tgtWS.BeginTransaction();
				foreach (var lst in lstPrms)
				{
					tgtWS.ExecuteNonQuery(sql, lst);
					Progress.Step();
				}
				tgtWS.Commit();
			}
			catch (Exception ex)
			{
				tgtWS.Rollback();
				Console.WriteLine(ex.Message);
			}
		}

		protected IFeatureClass CreateTgtFeatureClass(string dkFileName, int srid, string fbfbm, out IFeatureClass? fcDcDK)
		{
			fcDcDK = null;
			IFeatureClass fc;
			using (var ws = SqliteFeatureWorkspaceFactory.Instance.CreateWorkspace(dkFileName))
			{
                static void OnPreCreateFeatureClass(IFields fields)
				{
					for (int i = fields.FieldCount; --i >= 0;)
					{
						var field = fields.GetField(i);
						if (StringUtil.isEqualIgnorCase(field.FieldName, nameof(VEC_SURVEY_DK.ELHTMJ)))
						{
							field.AliasName = "二轮合同面积";
						}
					}
					if (fields.FindField("DJZT") < 0)
					{
						FieldsUtil.AddIntField(fields, "DJZT", "登记状态");
					}
				}
				fc = VEC_SURVEY_DK.CreateFeatureClass(ws, srid, OnPreCreateFeatureClass);
				fcDcDK = DC_DLXX_DK.CreateFeatureClass(ws, srid, OnPreCreateFeatureClass);
				XTPZ_SJZD.CreateTable(ws);
				XTPZ_TDLYXZFL.CreateTable(ws);
				CS_SYSINFO.CreateTable(ws);
				DC_QSSJ_CBF.CreateTable(ws);
				DC_QSSJ_CBF_JTCY.CreateTable(ws);
				DC_QSSJ_FBF.CreateTable(ws);
				ExportTable(ws, XTPZ_SJZD.GetTableName(), XTPZ_SJZD.GetFieldsString());
				ExportTable(ws, XTPZ_TDLYXZFL.GetTableName(), XTPZ_TDLYXZFL.GetFieldsString());
				ExportTable(ws, CS_SYSINFO.GetTableName(), CS_SYSINFO.GetFieldsString());
				var cbfFields = DC_QSSJ_CBF.GetFieldsString();

				ExportTable(ws, QSSJ_CBF.GetTableName(), cbfFields, DC_QSSJ_CBF.GetTableName(), $"FBFBM like '{fbfbm}%'", p =>
				{
					if (StringUtil.isEqualIgnorCase(p.ParamName, "ZHXGSJ"))
					{
						p.ParamValue = DBNull.Value;
					}
				});
				var cbfJtcyFields = DC_QSSJ_CBF_JTCY.GetFieldsString();
				ExportTable(ws, QSSJ_CBF_JTCY.GetTableName(), cbfJtcyFields, DC_QSSJ_CBF_JTCY.GetTableName(), $"CBFBM in(select CBFBM from QSSJ_CBF where FBFBM like '{fbfbm}%')");
				ExportTable(ws, QSSJ_FBF.GetTableName(), DC_QSSJ_FBF.GetFieldsString(), DC_QSSJ_FBF.GetTableName(), $"FBFBM like '{fbfbm.Substring(0, 12)}%'");
                //ExportTable(ws, QSSJ_FBF.GetTableName(), DC_QSSJ_FBF.GetFieldsString(), DC_QSSJ_FBF.GetTableName(), $"FBFBM like '{fbfbm}%'");
                using var fcXzdy = DLXX_XZDY.CreateFeatureClass(ws, srid);
                ExportXzdy(fcXzdy, fbfbm);
            }
			return fc;
		}


		private static int CopyValues(IRow from, IRow to)//, bool fCopyShapeField = true)
		{
			int nSameFields = 0;
			var toFields = to.Fields;
			for (int i = 0; i < toFields.FieldCount; ++i)
			{
				var toField = toFields.GetField(i);
				if (!toField.Editable)
				{
					continue;
				}
				var name = toField.FieldName;
				if (name == DC_DLXX_DK.GetFieldName(nameof(DC_DLXX_DK.FBFDM)))
				{
					name = "FBFBM";
				}
				int n = from.Fields.FindField(name);
				if (n < 0)
				{
					continue;
				}
				var o = from.GetValue(n);
				to.SetValue(i, o);
				++nSameFields;
			}
			return nSameFields;
		}


	}

	//class ExportYDkUtil
	//{
	//	/// <summary>
	//	/// 导出原地块
	//	/// </summary>
	//	/// <param name="tgtFc"></param>
	//	/// <param name="env"></param>
	//	/// <param name="subFields"></param>
	//	/// <param name="cancel"></param>
	//	public static void ExportYDk(IFeatureClass tgtFc, Envelope env, List<string> subFields, ICancelTracker cancel)
	//	{
	//		env.ExpandBy(50);
	//		var srcRepos = DlxxDkRepository.Instance;
	//		var ft1 = tgtFc.CreateFeature();
	//		using (var srcFc = MyGlobal.Workspace.OpenFeatureClass(DLXX_DK.GetTableName()))
	//		{
	//			var qf = new SpatialFilter()
	//			{
	//				Geometry = GeometryUtil.MakePolygon(env),
	//				SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects,
	//				SubFields = string.Join(",", subFields),
	//				WhereClause = $"ZT={(int)EDKZT.Youxiao}"
	//			};
	//			//tgtFc.Workspace.BeginTransaction();
	//			srcFc.SpatialQuery(qf, ft =>
	//			{
	//				CopyValues(ft, ft1);

	//				tgtFc.Append(ft1);
	//			}, cancel);
	//			//tgtFc.Workspace.Commit();
	//		}
	//	}

	//	private static int CopyValues(IRow from, IRow to)//, bool fCopyShapeField = true)
	//	{
	//		int nSameFields = 0;
	//		var toFields = to.Fields;
	//		for (int i = 0; i < toFields.FieldCount; ++i)
	//		{
	//			var toField = toFields.GetField(i);
	//			if (!toField.Editable)
	//			{
	//				continue;
	//			}
	//			var name = toField.FieldName;
	//			if (name == DC_DLXX_DK.GetFieldName(nameof(DC_DLXX_DK.FBFDM)))
	//			{
	//				name = "FBFBM";
	//			}
	//			int n = from.Fields.FindField(name);
	//			if (n < 0)
	//			{
	//				continue;
	//			}
	//			//var fromField = from.Fields.GetField(n);
	//			//if (!IsSameField(fromField, toField))
	//			//{
	//			//	continue;
	//			//}
	//			//var fShpField = toField.FieldType == eFieldType.eFieldTypeGeometry;
	//			//if (fShpField && !fCopyShapeField)
	//			//{
	//			//	continue;
	//			//}
	//			var o = from.GetValue(n);
	//			to.SetValue(i, o);
	//			++nSameFields;
	//		}
	//		return nSameFields;
	//	}


	//}

	/// <summary>
	/// 批量导出地块数据
	/// </summary>
	public class BatchExportDkData : GroupTask
	{
		class MyTask : ExportDkDataBase
		{
			private readonly FbfItem fbfItem;
			private readonly string outPath;
			public MyTask(FbfItem fbfItem, string outPath)
			{
				Name = fbfItem.FbfMC;
				this.fbfItem = fbfItem;
				this.outPath = outPath;
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				try
				{
					//var dkShpFile = d.ExportFilePath;
					var nRecordCount = DoExportDk(cancel);
					base.ReportInfomation($"共导出{nRecordCount}条记录");
				}
				catch (Exception ex)
				{
					base.ReportException(ex);
				}
			}

			private int DoExportDk(ICancelTracker cancel)
			{
				var fbfbm = fbfItem.FbfBM;
				var dkFileName = Path.Combine(outPath, fbfItem.FbfMC + ".dk");

				var srcRepos = DlxxDkRepository.Instance;


				int nRecordCount = srcRepos.Count(t => t.ZT == EDKZT.Youxiao && t.FBFBM.StartsWith(fbfbm));

				if (nRecordCount == 0)
				{
					throw new Exception("发包方编码" + fbfbm + "无效！");
				}

				var srid = srcRepos.GetSrid();

				using (var tgtFc = CreateTgtFeatureClass(dkFileName, srid, fbfbm, out var fcDcDk))
				{
					try
					{
						//var progress = new ProgressReporter(ReportProgress, nRecordCount);
						Progress.Reset(nRecordCount + 1, "导出地块");

						tgtFc.Workspace.BeginTransaction();

						var lstEn = new List<DLXX_DK>();
						var lstFields = EntityUtil.GetIntersectionAttributes<VEC_SURVEY_DK, DLXX_DK>((t, u) => t.FieldName == u.FieldName).Select(t => t.FieldName).ToList();
						lstFields.Add(nameof(DLXX_DK.FBFBM));
						//if (prm.DatabaseType == eDatabaseType.SQLite)
						{
							lstFields.Add(nameof(DC_DLXX_DK.DJZT));
						}
						var subFields = SubFields.Make(lstFields);
						//srcRepos.FindCallback(t => t.ZT == EDKZT.Youxiao && t.FBFBM == fbfbm, en =>
						srcRepos.FindCallback1(t => t.ZT == EDKZT.Youxiao && t.FBFBM.StartsWith(fbfbm), it => lstEn.Add(it.Item), subFields, false, cancel);

						var dicDkbm2Cbfbm = new Dictionary<string, string>();
						SqlUtil.ConstructIn(lstEn, sin =>
						{
							MyGlobal.Workspace.QueryCallback($"select DKBM,CBFBM from {QSSJ_CBDKXX.GetTableName()} where DKBM in({sin})", r =>
							{
								var dkbm = r.GetString(0);
								var cbfbm = SafeConvertAux.ToStr(r.GetValue(1));
								dicDkbm2Cbfbm[dkbm] = cbfbm;
							});
						}, en => en.DKBM);

						var ft = tgtFc.CreateFeature();
						//var ft1 = fcDcDk?.CreateFeature();
						Envelope? env = null;

						foreach (var en in lstEn)
						{
							if (dicDkbm2Cbfbm.TryGetValue(en.DKBM, out string? cbfbm))
							{
								IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.CBFBM), cbfbm);
							}
							else
							{
								cbfbm = null;
							}
							EntityUtil.WriteToFeature(en, ft);
							IRowUtil.SetRowValue(ft, nameof(VEC_SURVEY_DK.FBFDM), en.FBFBM);
							if (en.Shape != null && !en.Shape.IsEmpty)
							{
								if (env == null)
								{
									env = en.Shape.EnvelopeInternal;
								}
								else
								{
									env.ExpandToInclude(en.Shape.EnvelopeInternal);
								}
							}
							//if (ft1 != null)
							//{
							//	EntityUtil.WriteToFeature(en, ft1);
							//	IRowUtil.SetRowValue(ft1, nameof(VEC_SURVEY_DK.FBFDM), en.FBFBM);
							//	IRowUtil.SetRowValue(ft1, nameof(VEC_SURVEY_DK.CBFBM), cbfbm);
							//	fcDcDk.Append(ft1);
							//}
							tgtFc.Append(ft);
							Progress.Step();
						}

						if (env != null && fcDcDk != null)
						{
							base.ExportYDk(fcDcDk, env, lstFields, cancel);
						}

						tgtFc.Workspace.Commit();
						Progress.ForceFinish();

						fcDcDk?.Dispose();
					}
					catch (Exception ex)
					{
						tgtFc.Workspace.Rollback();
						throw ex;
					}
				}

				return nRecordCount;
			}


			//private void ExportYDk(IFeatureClass tgtFc, Envelope env, List<string> subFields, ICancelTracker cancel)
			//{
			//	env.ExpandBy(50);
			//	var srcRepos = DlxxDkRepository.Instance;
			//	var ft1 = tgtFc.CreateFeature();
			//	using (var srcFc = MyGlobal.Workspace.OpenFeatureClass(DLXX_DK.GetTableName()))
			//	{
			//		var qf = new SpatialFilter()
			//		{
			//			Geometry = GeometryUtil.MakePolygon(env),
			//			SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects,
			//			SubFields = string.Join(",", subFields),
			//			WhereClause = $"ZT={(int)EDKZT.Youxiao}"
			//		};
			//		//tgtFc.Workspace.BeginTransaction();
			//		srcFc.SpatialQuery(qf, ft =>
			//		{
			//			CopyValues(ft, ft1);

			//			tgtFc.Append(ft1);
			//		}, cancel);
			//		//tgtFc.Workspace.Commit();
			//	}
			//}

			//private static int CopyValues(IRow from, IRow to)//, bool fCopyShapeField = true)
			//{
			//	int nSameFields = 0;
			//	var toFields = to.Fields;
			//	for (int i = 0; i < toFields.FieldCount; ++i)
			//	{
			//		var toField = toFields.GetField(i);
			//		if (!toField.Editable)
			//		{
			//			continue;
			//		}
			//		var name = toField.FieldName;
			//		if (name ==DC_DLXX_DK.GetFieldName(nameof(DC_DLXX_DK.FBFDM)))
			//		{
			//			name = "FBFBM";
			//		}
			//		int n = from.Fields.FindField(name);
			//		if (n < 0)
			//		{
			//			continue;
			//		}
			//		//var fromField = from.Fields.GetField(n);
			//		//if (!IsSameField(fromField, toField))
			//		//{
			//		//	continue;
			//		//}
			//		//var fShpField = toField.FieldType == eFieldType.eFieldTypeGeometry;
			//		//if (fShpField && !fCopyShapeField)
			//		//{
			//		//	continue;
			//		//}
			//		var o = from.GetValue(n);
			//		to.SetValue(i, o);
			//		++nSameFields;
			//	}
			//	return nSameFields;
			//}


			//private void ExportXzdy(IFeatureClass tgtFc, string fbfbm)
			//{
			//	try
			//	{
			//		var repos = DlxxXzdyRepository.Instance;
			//		var sXiang = fbfbm.Substring(0, 12);
			//		var where = DLXX_XZDY.MakeWhere(t => (int)t.JB < (int)eZoneLevel.County && t.BM.StartsWith(sXiang));
			//		var cnt = repos.Count(where);// t => (int)t.JB < 5);
			//		Progress.Reset(cnt, "导出行政地域");
			//		//if (fbfbm.Length==14&&fbfbm.EndsWith("00"))
			//		//{
			//		//	fbfbm = fbfbm.Substring(0,fbfbm.Length - 2);
			//		//}
			//		var lstEn = repos.FindAll(where);
			//		tgtFc.Workspace.BeginTransaction();

			//		var ft = tgtFc.CreateFeature();
			//		foreach (var en in lstEn)
			//		{
			//			EntityUtil.WriteToFeature(en, ft);
			//			tgtFc.Append(ft);
			//			Progress.Step();
			//		}

			//		tgtFc.Workspace.Commit();
			//	}
			//	catch (Exception ex)
			//	{
			//		tgtFc.Workspace.Rollback();
			//	}
			//}
			//private void ExportTable(IFeatureWorkspace tgtWS, string tableName, string fields, string tgtTableName = null, string wh = null, Action<SQLParam> callback = null)
			//{
			//	var srcDb = MyGlobal.Workspace;
			//	var lstPrms = new List<List<SQLParam>>();

			//	var cnt = srcDb.QueryOneInt($"select count(1) from {tableName}");
			//	Progress.Reset(cnt, $"导出{tableName}");
			//	var sql = $"SELECT {fields}   FROM {tableName}";
			//	if (!string.IsNullOrEmpty(wh))
			//	{
			//		sql += $" where {wh}";
			//	}
			//	srcDb.QueryCallback(sql, r =>
			//	{
			//		var sa = fields.Split(',');
			//		var lst = new List<SQLParam>();
			//		for (int i = 0; i < sa.Length; ++i)
			//		{
			//			var p = new SQLParam()
			//			{
			//				ParamName = sa[i].Trim(),
			//				ParamValue = r.GetValue(i),
			//			};
			//			callback?.Invoke(p);
			//			lst.Add(p);
			//		}
			//		lstPrms.Add(lst);
			//	});

			//	var vals = fields;
			//	var a = tgtWS.SqlFunc.GetParamPrefix();
			//	vals = a + vals.Replace(",", $",{a}");
			//	sql = $"insert into {tgtTableName ?? tableName}({fields}) values({vals})";
			//	try
			//	{
			//		tgtWS.BeginTransaction();
			//		foreach (var lst in lstPrms)
			//		{
			//			tgtWS.ExecuteNonQuery(sql, lst);
			//			Progress.Step();
			//		}
			//		tgtWS.Commit();
			//	}
			//	catch (Exception ex)
			//	{
			//		tgtWS.Rollback();
			//		Console.WriteLine(ex.Message);
			//	}
			//}

			//private IFeatureClass CreateTgtFeatureClass(string dkFileName, int srid, string fbfbm, out IFeatureClass fcDcDK)
			//{
			//	fcDcDK = null;
			//	IFeatureClass fc;
			//	using (var ws = SqliteFeatureWorkspaceFactory.Instance.CreateWorkspace(dkFileName))
			//	{
			//		void OnPreCreateFeatureClass(IFields fields)
			//		{
			//			for (int i = fields.FieldCount; --i >= 0;)
			//			{
			//				var field = fields.GetField(i);
			//				if (StringUtil.isEqualIgnorCase(field.FieldName, nameof(VEC_SURVEY_DK.ELHTMJ)))
			//				{
			//					field.AliasName = "二轮合同面积";
			//				}
			//			}
			//			if (fields.FindField("DJZT") < 0)
			//			{
			//				FieldsUtil.AddIntField(fields, "DJZT", "登记状态");
			//			}
			//		}
			//		fc = VEC_SURVEY_DK.CreateFeatureClass(ws, srid, OnPreCreateFeatureClass);
			//		fcDcDK = DC_DLXX_DK.CreateFeatureClass(ws, srid, OnPreCreateFeatureClass);
			//		XTPZ_SJZD.CreateTable(ws);
			//		XTPZ_TDLYXZFL.CreateTable(ws);
			//		CS_SYSINFO.CreateTable(ws);
			//		DC_QSSJ_CBF.CreateTable(ws);
			//		DC_QSSJ_CBF_JTCY.CreateTable(ws);
			//		DC_QSSJ_FBF.CreateTable(ws);
			//		ExportTable(ws, XTPZ_SJZD.GetTableName(), XTPZ_SJZD.GetFieldsString());
			//		ExportTable(ws, XTPZ_TDLYXZFL.GetTableName(), XTPZ_TDLYXZFL.GetFieldsString());
			//		ExportTable(ws, CS_SYSINFO.GetTableName(), CS_SYSINFO.GetFieldsString());
			//		var cbfFields = DC_QSSJ_CBF.GetFieldsString();

			//		ExportTable(ws, QSSJ_CBF.GetTableName(), cbfFields, DC_QSSJ_CBF.GetTableName(), $"FBFBM like '{fbfbm}%'", p =>
			//		{
			//			if (StringUtil.isEqualIgnorCase(p.ParamName, "ZHXGSJ"))
			//			{
			//				p.ParamValue = DBNull.Value;
			//			}
			//		});
			//		var cbfJtcyFields = DC_QSSJ_CBF_JTCY.GetFieldsString();
			//		ExportTable(ws, QSSJ_CBF_JTCY.GetTableName(), cbfJtcyFields, DC_QSSJ_CBF_JTCY.GetTableName(), $"CBFBM in(select CBFBM from QSSJ_CBF where FBFBM like '{fbfbm}%')");
			//		ExportTable(ws, QSSJ_FBF.GetTableName(), DC_QSSJ_FBF.GetFieldsString(), DC_QSSJ_FBF.GetTableName(), $"FBFBM like '{fbfbm.Substring(0, 9)}%'");
			//		using (var fcXzdy = DLXX_XZDY.CreateFeatureClass(ws, srid))
			//		{
			//			ExportXzdy(fcXzdy, fbfbm);
			//		}
			//	}
			//	return fc;
			//}


		}
		/// <summary>
		/// 批量导出地块数据
		/// </summary>
		public BatchExportDkData()
		{
			base.Name = "导出地块数据";
			//base.Description = "导出符合农业部要求格式的调查地块数据";
			base.PropertyPage = new ExportDkDataPropertyPage1(lst=>
			{
				base.ClearTasks();
				var prm = base.PropertyPage as ExportDkDataPropertyPage1;
				var outPath = prm.ExportFilePath;
				if (!Directory.Exists(outPath))
				{
					Directory.CreateDirectory(outPath);
				}
				foreach (var it in lst)
				{
					AddTask(new MyTask(it,outPath));
				}
				base.IsExpanded = true;
			});
			base.OnFinish += (t, s) => base.ReportInfomation($"耗时：{t.Elapsed}");
		}
	}
}
