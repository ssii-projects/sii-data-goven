using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using Agro.LibCore;
using Agro.Library.Common;
using GeoAPI.Geometries;
using Agro.GIS;
using Agro.Module.DataExchange.LandDotCoil;
using Agro.Library.Model;
using Agro.Library.Common.Repository;
using Agro.Module.DataExchange.Repository;
namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 导入调查地块（新增、分割、合并、图形变更、一般变更）部分
	/// yxm 2018-3-20
	/// </summary>
	public class ImportDcdkData : Task, ICloneable
	{
		public class TopChecker:IDisposable
		{
			public class DkItem
			{
				public string? ID;
				public string? DKBM;
				public readonly IGeometry Shape;
				public DkItem(IGeometry g)
				{
					Shape= g;
				}
			}
			private readonly IFeatureClass _fc;
			private readonly ISpatialFilter spatialFilter = SqlUtil.MakeSpatialFilter<DLXX_DK>(eSpatialRelEnum.eSpatialRelIntersects, (c, t) => c(t.DKBM, t.Shape,t.ID),t=>t.Shape!=null&&t.ZT==EDKZT.Youxiao&&t.MSSJ==null);
			/// <summary>
			/// [DKBM,]
			/// </summary>
			private readonly List<DkItem> _dicBuffer = new();

			/// <summary>
			/// [DKBM,]
			/// </summary>
			public readonly Dictionary<string, List<DkItem>> _dicMergeItems = new Dictionary<string, List<DkItem>>();
			/// <summary>
			/// [DKBM,]
			/// </summary>
			public readonly Dictionary<string, List<VEC_SURVEY_DK>> dicSplit = new Dictionary<string, List<VEC_SURVEY_DK>>();
			private readonly OuterDcdkRepository _srcRepos;
			public TopChecker(OuterDcdkRepository srcRepos)
			{
				_srcRepos = srcRepos;
				_fc = MyGlobal.Workspace.OpenFeatureClass(DLXX_DK.GetTableName());
			}
			public void Build(List<VEC_SURVEY_DK> lst, ICancelTracker? cancel)
			{
				_dicBuffer.Clear();
				foreach (var en in lst)
				{
					var eBglx = _srcRepos.GetBGLX(en);
					List<DkItem>? lstMergeDkbms = null;
					var mergeAreas = 0.0;
					var enArea = 0.0;
					if (eBglx == ETXBGLX.Hebing)
					{
						lstMergeDkbms = new List<DkItem>();
						enArea = en.Shape.Area;
						if (string.IsNullOrEmpty(en.DKBM))
						{
							throw new Exception($"DKBM=‘{en.DKBM}’的记录存在问题，其变更类型为‘合并’，其地块编码不能为空！");
						}
					}
					spatialFilter.Geometry = en.Shape;
					_fc.SpatialQuery(spatialFilter, ft =>
					{
						var dkbm = IRowUtil.GetRowValue(ft, "DKBM")?.ToString();
						if (_dicBuffer.Find(it=>it.DKBM==dkbm)==null)
						{
							var id = IRowUtil.GetRowValue(ft, "ID").ToString();
							var geo = eBglx==ETXBGLX.Txbg&& dkbm == en.DKBM ? en.Shape : (IGeometry)ft.Shape.Clone();
							var dkIt = new DkItem(geo)
							{
								ID = id,
								DKBM=dkbm
							};
							_dicBuffer.Add(dkIt);
							if (eBglx == ETXBGLX.Hebing)
							{
								try
								{
									var gx = en.Shape.Intersection(geo);
									if (gx.Area / geo.Area > 0.9)
									{
										lstMergeDkbms!.Add(dkIt);
										mergeAreas += gx.Area;
									}
								}
								catch (Exception ex)
								{
									Console.WriteLine(ex.Message);
								}
							}
						}
					}, cancel);

					switch (eBglx)
					{
						case ETXBGLX.Xinz:
							_dicBuffer.Add(new DkItem(en.Shape));
							break;
						case ETXBGLX.Hebing:
							if (mergeAreas / enArea < 0.9)
							{
								throw new Exception($"DKBM=‘{en.DKBM}’的记录存在问题，其变更类型为‘合并’但其图形与数据库中的不匹配！");
							}
							_dicMergeItems[en.DKBM] = lstMergeDkbms!;
							break;
						case ETXBGLX.Fenge:
							if (!dicSplit.TryGetValue(en.DKBM, out var lstFenge))
							{
								lstFenge = new List<VEC_SURVEY_DK>();
								dicSplit[en.DKBM] = lstFenge;
							}
							lstFenge.Add(en);
							break;
						case ETXBGLX.Txbg:
							{//yxm 2021-12-16 当修改的地块图形距离比较远的时候
								try
								{
									if (_dicBuffer.Find(it => it.DKBM == en.DKBM) == null)
									{
										var repos = DlxxDkRepository.Instance;
										var dkbm = en.DKBM;
										var oldEn = repos.Find(t => t.DKBM == dkbm && t.Shape != null && t.ZT == EDKZT.Youxiao && t.MSSJ == null, (c, t) => c(t.ID));
										//var qf = new QueryFilter
										//{
										//	SubFields = "ID",
										//	WhereClause = $"DKBM='{en.DKBM}' and ZT=1 and MSMJ=null and shape is not null"
										//};
										//string id = null;
										//_fc.Search(qf, r =>
										// {
										//	 id=IRowUtil.GetRowValue(r, "ID")?.ToString();
										// });
										//if (id != null)
										if (oldEn != null)
										{
											var dkIt = new DkItem(en.Shape)
											{
												ID = oldEn.ID,
												DKBM = en.DKBM
											};
											_dicBuffer.Add(dkIt);
										}
									}
								}
								catch (Exception ex)
								{
									Console.WriteLine(ex.Message);
								}
							}
							break;
					}
				}
			}
			public bool IntersectsCheck(IGeometry geo, ICancelTracker? cancel)//,Dictionary<string,IGeometry> dicReplaceGeo, string excludeDkbm = null)
			{
				foreach (var it in _dicBuffer)
				{
					var ftGeo = it.Shape;
					try
					{
						if (ftGeo!=geo && ftGeo.Intersects(geo))
						{
							var gi = ftGeo.Intersection(geo);
							if (gi?.IsEmpty == false)
							{
								if (Math.Abs(gi.Area) > 5)
								{
									return true;
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
					if (cancel?.Cancel() == true)
					{
						break;
					}
				}
				return false;
			}
			public DkItem GetDkItemByDkbm(string dkbm)
			{
				return _dicBuffer.Find(it => it.DKBM == dkbm);
			}
			public void Dispose()
			{
				_fc?.Dispose();
			}
		}

		private OuterDcdkRepository _srcRepos;

		/// <summary>
		/// [VEC_SURVEY_DK,DKBM]
		/// </summary>
		private readonly Dictionary<VEC_SURVEY_DK, string> lstUploadedRows = new Dictionary<VEC_SURVEY_DK, string>();
		private TopChecker topCheck;
		private readonly XtpzLshRepository xtpzLshRepository = XtpzLshRepository.Instance;
		private readonly TxbgjlRepository bgjlRepos = TxbgjlRepository.Instance;
		private readonly DlxxDkRepository dlxxDkRepos = DlxxDkRepository.Instance;// (MyGlobal.Workspace);
        private BdcJrDkRepos? bdcJrDkRepos = null;

        private readonly ExportDkJsonTask jsonExp = new ();
        public ImportDcdkData()
		{
			Name = "导入调查地块数据";
			Description = "导入符合农业部要求格式的调查地块数据";
			base.PropertyPage = new ImportDcdkDataPropertyPage();
			base.OnStart += s => base.ReportInfomation("开始" + Name);
			base.OnFinish += (s, e) => base.ReportInfomation("结束" + Name + "，耗时：" + base.Elapsed);
		}

		private bool JsJtcyUploaded(List<DC_QSSJ_CBF_JTCY> lst)
		{
			var fUploaded = false;
			var db = MyGlobal.Workspace;
			bool isUpload(string tableName,string sin)
			{
				var sql = $"select count(*) from {tableName} where id in({sin})";
				return db.QueryOneInt(sql) > 0;
			}
			SqlUtil.ConstructIn(lst, sin =>
			 {
				 if (!fUploaded) {
					 if (isUpload(DC_QSSJ_CBF_JTCY.GetTableName(),sin)
					 || isUpload(QSSJ_CBF_JTCY.GetTableName(), sin)
					 || isUpload(DJ_CBJYQ_CBF_JTCY.GetTableName(), sin))
					 {
						 fUploaded = true;
					 }
				 }
			 }, it =>$"{it.ID}");
			return fUploaded;
		}

		protected override void DoGo(ICancelTracker cancel_)
		{
            ICancelTracker cancel = cancel_!;
			var d = (ImportDcdkDataPropertyPage)base.PropertyPage;
			var db = MyGlobal.Workspace;
			lstUploadedRows.Clear();
            if (db.IsTableExists(BDC_JR_DK.GetTableName()))
            {
                bdcJrDkRepos = BdcJrDkRepos.Instance;
            }
            try
			{
				IWorkspace? sqliteDB = null;
				if (d.DatabaseType == eDatabaseType.SQLite)
				{
					sqliteDB = SqliteFeatureWorkspaceFactory.Instance.OpenWorkspace(d.FileName);
				}

				using (var srcFc = OuterDcdkRepository.OpenSrcFeatureClass(d.DatabaseType, d.FileName))
				{
					_srcRepos = new OuterDcdkRepository(srcFc, false);
					using (topCheck = new TopChecker(_srcRepos))
					{
						try
						{
							var lst = new List<VEC_SURVEY_DK>();
							_srcRepos.LoadChangeData(cancel, en => lst.Add(en));
							if (cancel?.Cancel() == true) return;
							topCheck.Build(lst, cancel);

							var lstCbf = new List<DC_QSSJ_CBF>();
							var lstCbfJtcy = new Dictionary<string, List<DC_QSSJ_CBF_JTCY>>();
							DcCbfRepos? sqliteCbfRepos = sqliteDB != null ? new DcCbfRepos(sqliteDB) : null;
							DcCbfJtcyRepos? sqliteCbfJtcyRepos = sqliteDB != null ? new DcCbfJtcyRepos(sqliteDB) : null;
							if (sqliteCbfRepos != null)
							{
								sqliteCbfRepos.FindCallback(t => t.ZHXGSJ != null, it => lstCbf.Add(it.Item), null, false, cancel);
								var wh = "CBFBM in (select cbfbm from DC_QSSJ_CBF where ZHXGSJ is not null)";
								sqliteCbfJtcyRepos.FindCallback(wh, it =>
								 {
									 var en = it.Item;
									 if (!lstCbfJtcy.TryGetValue(en.CBFBM, out var lst1))
									 {
										 lst1 = new List<DC_QSSJ_CBF_JTCY>();
										 lstCbfJtcy[en.CBFBM] = lst1;
									 }
									 lst1.Add(en);
								 }, null, false, cancel);
							}
							var cnt = lst.Count + lstCbf.Count + lstCbfJtcy.Count;
							if (cnt == 0)
							{
								ReportWarning("没有需要上传的数据");
								return;
							}


							if (db.DatabaseType == eDatabaseType.Oracle)
							{
								#region 修复DLXX_DK表的自增长序列
								{
									var sql = @"DECLARE 
    nBSM INTEGER;
    nCur INTEGER;
BEGIN
    SELECT 0 INTO nCur FROM DUAL;
    SELECT MAX(BSM) INTO nBSM FROM DLXX_DK;
    WHILE (nCur < nBSM)
    LOOP
        SELECT SDE.GDB_UTIL.NEXT_ROWID(sys_context('userenv', 'CURRENT_USER'),'DLXX_DK') INTO nCur FROM DUAL;
    END LOOP;
END;";
									db.ExecuteNonQuery(sql);
								}
								#endregion
							}
							try
							{
								db.BeginTransaction();
								#region 执行写入数据操作
								ImportXinz(lst, cancel);
								ImportXiugai(lst, cancel);
								ImportXiugaiQtmj(lst, cancel);
								ImportSplit(topCheck.dicSplit, cancel);
								ImportYbbg(lst, cancel);
								ImportMerge(lst, cancel);
								if (lstCbf.Count > 0)
								{
									var tgtCbfRepos = new DcCbfRepos(db);
									var tgtEnCbf = new QSSJ_CBF();
									var tgtEnCbfJtcy = new QSSJ_CBF_JTCY();
									foreach (var en in lstCbf)
									{
										if (lstCbfJtcy.TryGetValue(en.CBFBM, out var lst1))
										{
											if (!JsJtcyUploaded(lst1))
											{
												if (en.CBFBM.Length > 18)
												{//新增承包方:直接写入QSSJ_CBF及QSSJ_CBF_JTCY
													var cbflxMc = CodeUtil.Code2Name(CodeType.CBFLX, en.CBFLX);
													var newCbfbm = this.xtpzLshRepository.QueryNewCbfbm(XtpzLshRepository.CbfLx2Enum(cbflxMc), en.FBFBM);
													lstCbfJtcy.Remove(en.CBFBM);

													en.CBFBM = newCbfbm;

													foreach (var it in lst1)
													{
														it.CBFBM = newCbfbm;
														tgtEnCbfJtcy.Copy(it);
														QssjCbfJtcyRepository.Instance.Insert(tgtEnCbfJtcy);
													}
													tgtEnCbf.Copy(en);
													tgtEnCbf.DJZT = (int)EDjzt.Wdj;
													tgtEnCbf.ZT = (int)EDKZT.Youxiao;
													QssjCbfRepository.Instance.Insert(tgtEnCbf);
												}
												else
												{
													tgtCbfRepos.Delete(t => t.CBFBM == en.CBFBM);
													tgtCbfRepos.Insert(en);
												}
											}
										}
										Progress.Step();
									}
								}
								if (lstCbfJtcy.Count > 0)
								{
									var tgtCbfJtcyRepos = new DcCbfJtcyRepos(db);
									foreach (var kv in lstCbfJtcy)
									{
										var cbfbm = kv.Key;
										tgtCbfJtcyRepos.Delete(t => t.CBFBM == cbfbm);
										foreach (var en in kv.Value)
										{
											tgtCbfJtcyRepos.Insert(en);
										}
										Progress.Step();
									}
								}

								#endregion
								db.Commit();

								Try.Catch(() =>
								{
									//var lstDkbms = new List<string>();
									foreach (var kv in lstUploadedRows)
									{
										var en = kv.Key;
										//var dkEn = kv.Value;
										en.SCBZ = "1";
										en.DKBM = kv.Value;// kv.Value.DKBM;
										_srcRepos.Update(en, (c, t) => c(t.SCBZ, t.DKBM));

										//lstDkbms.Add(en.DKBM);
									}
									//if (lstDkbms.Count > 0)
									//{
									//	jsonExp.DoExport(d.FileName+".json", lstDkbms, new NotCancelTracker());
									//}
								}, false);
							}
							catch (Exception ex)
							{
								db.Rollback();
								throw ex;
							}
						}
						catch (SystemException ex)
						{
							this.ReportError("导入数据发生错误:" + ex.Message);
						}
						//#region 导出JSON
						//File.WriteAllText(d.FileName + ".ndt", jsonObj.ToString());
						//#endregion
					}
				}

				sqliteDB?.Dispose();
				Progress.ForceFinish();
			}
			catch (Exception ex)
			{
				ReportException(ex);
			}
		}

		/// <summary>
		/// 上传新增地块
		/// </summary>
		/// <param name="lst"></param>
		/// <param name="cancel"></param>
		void ImportXinz(List<VEC_SURVEY_DK> lst,ICancelTracker cancel)
		{
			foreach (var en in lst)
			{
				if (_srcRepos.GetBGLX(en) != ETXBGLX.Xinz) continue;
				if (topCheck.IntersectsCheck(en.Shape,cancel))
				{
					throw new Exception($"第{en.GetObjectID()}条记录存在问题，该新增地块图形位置与其它地块位置有覆盖！");
				}
				var dkEn = Append(en);
				lstUploadedRows[en]=dkEn.DKBM;
				if (cancel.Cancel()==true) return;
			}
		}

		/// <summary>
		/// 修改其它面积
		/// </summary>
		/// <param name="lst"></param>
		/// <param name="cancel"></param>
		void ImportXiugaiQtmj(List<VEC_SURVEY_DK> lst, ICancelTracker cancel)//,Dictionary<string, IGeometry> dicReplaceGeo)
		{
			foreach (var en in lst)
			{
				if (_srcRepos.GetBGLX(en) != ETXBGLX.Xgqtmj)
				{
					continue;
				}
				UpdateJDDMJ(en.DKBM, en.JDDMJ);
				lstUploadedRows[en] =en.DKBM;
				Progress.Step();
				if (cancel.Cancel()) return;
			}
		}

		/// <summary>
		/// 修改DLXX_DK的机动地面积
		/// </summary>
		/// <param name="dkbm"></param>
		/// <param name="jddmj"></param>
		private void UpdateJDDMJ(string dkbm, decimal? jddmj)
		{
			var sJddmj = jddmj == null ? "null" : jddmj.ToString();
			var sql = $"update {DLXX_DK.GetTableName()} set JDDMJ={sJddmj} where DKBM='{dkbm}'";
			dlxxDkRepos.Db.ExecuteNonQuery(sql);
		}

		/// <summary>
		/// 上传修改地块
		/// </summary>
		/// <param name="lst"></param>
		/// <param name="cancel"></param>
		void ImportXiugai(List<VEC_SURVEY_DK> lst, ICancelTracker cancel)
		{
			foreach (var en in lst)
			{
				if (_srcRepos.GetBGLX(en) != ETXBGLX.Txbg)
				{
					continue;
				}
				var oldEn = topCheck.GetDkItemByDkbm(en.DKBM);
				if (oldEn == null)
				{
					throw new Exception($"第{ en.GetObjectID()}行记录有错，该行地块编码（DKBM）的值{en.DKBM}在数据库有效地块中不存在！");// $"地块编码{en.DKBM}在数据库中不存在，修改地块失败！");
				}
				if (topCheck.IntersectsCheck(en.Shape,cancel))
				{
					throw new Exception($"第{en.GetObjectID()}条记录存在问题，该地块图形位置与其它地块位置有覆盖！");
				}
				UpdateJDDMJ(en.DKBM, en.JDDMJ);
				var dkEn = Append(en, oldEn);
				lstUploadedRows[en]=dkEn.DKBM;
				if (cancel.Cancel()) return;
			}
		}

		/// <summary>
		/// 上传分割地块
		/// </summary>
		/// <param name="dicSplit"></param>
		/// <param name="cancel"></param>
		void ImportSplit(Dictionary<string, List<VEC_SURVEY_DK>> dicSplit,ICancelTracker cancel)
		{
			foreach (var it in dicSplit)
			{
				if (cancel.Cancel()) return;

				var sOldDkbm = it.Key;
				var oldEn = topCheck.GetDkItemByDkbm(sOldDkbm);
				if (oldEn == null)
				{
					throw new Exception($"地块编码{sOldDkbm}在数据库有效地块中不存在，分割地块失败！");
				}


				var dic = new Dictionary<VEC_SURVEY_DK, string>();
				foreach (var en in it.Value)
				{
					var dkEn = Append(en, oldEn);
					dic[en] = dkEn.DKBM;
				}
				foreach (var kv in dic)
				{
					lstUploadedRows[kv.Key] = kv.Value;
				}
			}
		}

		/// <summary>
		/// 上传一般变更地块（包含移除承包方）
		/// </summary>
		/// <param name="lst"></param>
		/// <param name="cancel"></param>
		void ImportYbbg(List<VEC_SURVEY_DK> lst, ICancelTracker cancel)
		{
			var repos = new DcDlxxDkRepos(MyGlobal.Workspace);
			var tgtEn = new DC_DLXX_DK();
			
			foreach (var en in lst)
			{
				var bglx = _srcRepos.GetBGLX(en);
				if (bglx != ETXBGLX.Sxbg && bglx != ETXBGLX.YcCbf)
				{
					continue;
				}
				tgtEn.Copy(en);
				var x=repos.Find(t => t.DKBM == tgtEn.DKBM);
				if (x != null)
				{
					repos.Update(tgtEn, t => t.DKBM == en.DKBM);
				}
				else
				{
					repos.Insert(tgtEn);
				}
				if (bglx == ETXBGLX.Sxbg)
				{
					UpdateJDDMJ(en.DKBM, en.JDDMJ);
				}
				lstUploadedRows[en] = en.DKBM;

                //ImportBDCDYH(en.DKBM, en.BDCDYH);

                if (cancel.Cancel()) return;
				Progress.Step();
			}
		}

		void ImportMerge(List<VEC_SURVEY_DK> lst, ICancelTracker cancel)
		{
			foreach (var en in lst)
			{
				if (_srcRepos.GetBGLX(en) != ETXBGLX.Hebing)
				{
					continue;
				}

				var dkEn = Append(en);
				lstUploadedRows[en] = dkEn.DKBM;
				if (cancel.Cancel()) return;
			}
		}

		private DLXX_DK Append(VEC_SURVEY_DK en, TopChecker.DkItem oldEn = null)//, bool fUseTransaction=true)
		{
			var bglx = _srcRepos.GetBGLX(en);
			var sNewDKBM = xtpzLshRepository.QueryNewDKBM(en.FBFDM);
			if (string.IsNullOrEmpty(sNewDKBM))
			{
				throw new Exception("自动生成地块编码失败，请检查发包方编码或数据库配置(XTPZ_LSH)是否正确！");
			}
			var dkEn = new DLXX_DK();
			dkEn.Copy(en, (c, t) => c(t.ID));
			dkEn.FBFBM = en.FBFDM;
			dkEn.SJLY = OuterDcdkRepository.FromBGLX(bglx);
			dkEn.DKBM = sNewDKBM;
			dkEn.ZT = bglx == ETXBGLX.Xinz ? EDKZT.Youxiao : EDKZT.Lins;
			dkEn.DJZT = EDjzt.Wdj;
			dkEn.ZHXGSJ = DateTime.Now;
			if (dkEn.SCMJM == 0 || dkEn.SCMJM == null)
			{
				dkEn.SCMJM = (decimal)Math.Round((double)dkEn.SCMJ * 0.0015, 2);
			}

			#region 写入数据库（DK,JZD,JZX)
			void insertAction()
			{
				var kjzb = ImportJzdJzx(en.DKBM, sNewDKBM, en.ZJRXM, en.CBFMC, en.Shape);
				dkEn.KJZB = kjzb;
				dlxxDkRepos.Insert(dkEn);
			}

			//if (fUseTransaction)
			//{
			//	DBUtil.UseTransaction(MyGlobal.Workspace, insertAction);
			//}
			//else
			//{
			insertAction();
			//}
			#endregion

			var eBgfs = _srcRepos.GetBGLX(en);
			#region 写入变更登记表（对非新增对象）
			if (eBgfs == ETXBGLX.Hebing)
			{
				if (topCheck._dicMergeItems.TryGetValue(en.DKBM, out var lstMergeItems))
				{
					foreach (var it in lstMergeItems)
					{
						var enBgjl = new DLXX_DK_TXBGJL
						{
							YDKID = it.ID,
							YDKBM = it.DKBM,
							DKID = dkEn.ID,
							DKBM = dkEn.DKBM,
							BGFS = eBgfs,// ETXBGLX.Fenge,
							BGYY = en.BGYY
						};

						bgjlRepos.Insert(enBgjl);
					}
				}
			}
			else if (oldEn != null)
			{
				var enBgjl = new DLXX_DK_TXBGJL
				{
					YDKID = oldEn.ID,
					YDKBM = oldEn.DKBM,
					DKID = dkEn.ID,
					DKBM = dkEn.DKBM,
					BGFS = eBgfs,// ETXBGLX.Fenge,
					BGYY = en.BGYY
				};

				bgjlRepos.Insert(enBgjl);
				//jsonObj.AddTxbgjl(ExportDkJsonTask.FromEntity(enBgjl));
			}
			#endregion

			#region 写入不动产单元（承包地）BDC_JR_DK 2022-11-25
			ImportBDCDYH(dkEn.DKBM, en.BDCDYH,dkEn.ID);
            #endregion

            Progress.Step();
			return dkEn;
		}
        /// <summary>
        /// 写入不动产单元（承包地）BDC_JR_DK 2022-11-25
        /// </summary>
        /// <param name="dkbm"></param>
        /// <param name="BDCDYH"></param>
        private void ImportBDCDYH(string dkbm,string? BDCDYH,string? id=null)
		{
            try
            {
                if (bdcJrDkRepos != null && !string.IsNullOrEmpty(BDCDYH?.Trim()))
                {
                    var bdcJrDkEn = new BDC_JR_DK()
                    {
                        DKBM = dkbm,
                        BDCDYH = BDCDYH
                    };
                    if (!string.IsNullOrEmpty(id))
                    {
						bdcJrDkEn.ID = id;
                    }
                    bdcJrDkRepos.Delete(it => it.DKBM == dkbm);
                    bdcJrDkRepos.Insert(bdcJrDkEn);
                }
            }
            catch { }
        }

        /// <summary>
        /// yxm 2019-4-28
        /// 导入界址点和界址线
        /// </summary>
        private string ImportJzdJzx(string oriDkbm, string dkbm, string zjrXm, string cbfMc, IGeometry dkShape)
		{
			string? kjzb = null;//空间坐标（/分隔）
			var param = new DotCoilBuilderParam();
			var t = new DotCoilBuilder(param);
			param.Dk.OriDkbm = oriDkbm;
			param.Dk.ZjrXm = zjrXm;
			param.Dk.CbfMc = cbfMc;
			param.Dk.Shape = dkShape;
			param.Dk.Dkbm = dkbm;
			var db = MyGlobal.Workspace;

			#region intersect query from DLXX_DK
			using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK"))
			{
				var qf = new SpatialFilter()
				{
					SubFields = "DKBM,ZJRXM,CBFMC," + fc.ShapeFieldName,
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects,
					WhereClause = "ZT =1"
				};
				var env = dkShape.EnvelopeInternal.Clone();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geometry = GeometryUtil.MakePolygon(env);
					fc.SpatialQuery(qf, ft1 =>
					{
						if (ft1.Shape is IGeometry pt)
						{
							var sDKBM = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "DKBM"));
							if (sDKBM != param.Dk.OriDkbm)
							{
								var sZJRXM = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "ZJRXM"));
								var sCBFMC = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "CBFMC"));
								param.AddAroundDk(pt, sZJRXM, sCBFMC);
							}
						}
					});
				}
			}
			#endregion

			#region intersect query from DLXX_DK_JZD
			using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK_JZD"))
			{
				var qf = new SpatialFilter()
				{
					SubFields = "JZDH," + fc.ShapeFieldName,
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects
				};
				var env = dkShape.EnvelopeInternal.Clone();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geometry = GeometryUtil.MakePolygon(env);
					fc.SpatialQuery(qf, ft1 =>
					{
						if (ft1.Shape is IPoint pt)
						{
							var jzdh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "JZDH"));
							param.AddJzd(new Coordinate(pt.X, pt.Y), jzdh);
						}
					});
				}
			}
			#endregion

			#region intersect query from DLXX_DK_JZX
			using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK_JZX"))
			{
				var qf = new SpatialFilter()
				{
					SubFields = "JZXH,PLDWZJR,PLDWQLR," + fc.ShapeFieldName,
					SpatialRel = eSpatialRelEnum.eSpatialRelEnvelopeIntersects
				};
				var env = dkShape.EnvelopeInternal.Clone();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geometry = GeometryUtil.MakePolygon(env);
					fc.SpatialQuery(qf, ft1 =>
					{
						if (ft1.Shape is ILineString pt)
						{
							var jzxh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "JZXH"));
							var pldwZjr = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "PLDWZJR"));
							var pldwQlr = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "PLDWQLR"));
							var qjzdh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "QJZDH"));
							var zjzdh = SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "ZJZDH"));
							param.AddJzx(pt, jzxh, pldwZjr, pldwQlr, qjzdh, zjzdh);
						}
					});
				}
			}
			#endregion

			var res = t.Build();

			var xiangBM = dkbm.Substring(0, 9);
			int nMaxJzdh = XTPZ_LSHUtil.GetMaxJzdh(db, xiangBM);
			int nMaxJzxh = XTPZ_LSHUtil.GetMaxJzxh(db, xiangBM);
			using (var jzdFc = db.OpenFeatureClass("DLXX_DK_JZD"))
			{
				foreach (var j in res.lstJzd)
				{
					if (string.IsNullOrEmpty(j.Jzdh))
					{
						j.Jzdh = $"T{++nMaxJzdh}";
					}
					if (kjzb == null)
					{
						kjzb = j.Jzdh;
					}
					else
					{
						kjzb += "/" + j.Jzdh;
					}
					var ft = jzdFc.CreateFeature();
					var id = Guid.NewGuid().ToString();
					ft.Shape = GeometryUtil.MakePoint(j.Shape);
					IRowUtil.SetRowValue(ft, "JZDH", j.Jzdh);
					IRowUtil.SetRowValue(ft, "DKBM", param.Dk.Dkbm);
					IRowUtil.SetRowValue(ft, "JZDLX", "3");
					IRowUtil.SetRowValue(ft, "JBLX", "6");
					IRowUtil.SetRowValue(ft, "YSDM", "211021");
					IRowUtil.SetRowValue(ft, "ID", id);
					IRowUtil.SetRowValue(ft, "DKBM", param.Dk.Dkbm);
					IRowUtil.SetRowValue(ft, "XZBZ", Math.Round(j.Shape.X, 3));
					IRowUtil.SetRowValue(ft, "YZBZ", Math.Round(j.Shape.Y, 3));
					jzdFc.Append(ft);

					//var jzdJson = new ExportDkJsonTask.KeyValues();
					//jsonObj.AddJzd(jzdJson);
					//for (int i = 0; i < ft.Fields.FieldCount; ++i)
					//{
					//	var o = ft.GetValue(i);
					//	if (o is IGeometry) continue;
					//	jzdJson.Add(ft.Fields.GetField(i).FieldName, o);
					//}
				}
				XTPZ_LSHUtil.UpdateMaxJzdh(db, xiangBM, nMaxJzdh);
			}
			using (var jzxFc = db.OpenFeatureClass("DLXX_DK_JZX"))
			{
				foreach (var j in res.lstJzx)
				{
					var ft = jzxFc.CreateFeature();
					ft.Shape = j.Shape;

					if (string.IsNullOrEmpty(j.Jzxh))
					{
						j.Jzxh = (++nMaxJzxh).ToString();
					}
					IRowUtil.SetRowValue(ft, "ID", Guid.NewGuid().ToString());
					IRowUtil.SetRowValue(ft, "DKBM", param.Dk.Dkbm);

					var qJzdh = string.IsNullOrEmpty(j.sQjzd) ? j.Qjzd.Jzdh : j.sQjzd;
					var zJzdh = string.IsNullOrEmpty(j.sZjzd) ? j.Zjzd.Jzdh : j.sZjzd;
					IRowUtil.SetRowValue(ft, "QJZDH", qJzdh);
					IRowUtil.SetRowValue(ft, "ZJZDH", zJzdh);

					IRowUtil.SetRowValue(ft, "JZXH", j.Jzxh);
					IRowUtil.SetRowValue(ft, "PLDWZJR", j.PldwZjr);
					IRowUtil.SetRowValue(ft, "PLDWQLR", j.PldwQlr);
					IRowUtil.SetRowValue(ft, "YSDM", "211031");
					IRowUtil.SetRowValue(ft, "JXXZ", "600009");
					IRowUtil.SetRowValue(ft, "JZXLB", "08");
					IRowUtil.SetRowValue(ft, "JZXWZ", "2");
					jzxFc.Append(ft);

					//var jzxJson = new ExportDkJsonTask.KeyValues();
					//jsonObj.AddJzx(jzxJson);
					//for (int i = 0; i < ft.Fields.FieldCount; ++i)
					//{
					//	var o = ft.GetValue(i);
					//	if (o is IGeometry) continue;
					//	jzxJson.Add(ft.Fields.GetField(i).FieldName, o);
					//}
				}
				XTPZ_LSHUtil.UpdateMaxJzxh(db, xiangBM, nMaxJzxh);
			}

			return kjzb;
		}

		public object Clone()
		{
			var clone = new ImportDcdkData();
			return clone;
		}
	}
}
