using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Agro.Module.DataSync
{
	/// <summary>
	/// yxm 2019-10-24
	/// 导入业务数据更新包 #I12ZJY  https://gitee.com/ssii/dashboard/issues?assignee_ids=5179231&id=I12ZJY
	/// 将从登记颁证系统导出的业务数据更新包导入到涉密数据库中，以便保持涉密数据库中数据与业务办理数据库中数据一致。
	/// </summary>
	class ImportWwDjbJsonTask : Task
	{
		class JsonData
		{
			public class DKItem
			{
				public readonly DJ_CBJYQ_DKXX DjDkxx = new DJ_CBJYQ_DKXX();
				public readonly QSSJ_CBDKXX QsDkxx = new QSSJ_CBDKXX();
				public readonly DLXX_DK DlxxDk = new DLXX_DK();
			}
			public class CbfItem
			{
				public readonly QSSJ_CBF QsCbf = new QSSJ_CBF();
				public readonly DJ_CBJYQ_CBF DjCbf = new DJ_CBJYQ_CBF();
			}
			/// <summary>
			/// 是否包含ydjbid属性
			/// </summary>
			public bool HasYdjbid = false;
			public bool HasQssjCbf = false;
			//public readonly DJ_CBJYQZ_DJB DjDjb = new DJ_CBJYQZ_DJB();
			public readonly DJ_CBJYQ_DJB Djb = new DJ_CBJYQ_DJB();
			public readonly DJ_CBJYQ_CBHT Cbht = new DJ_CBJYQ_CBHT();
			public readonly CbfItem Cbf = new CbfItem();
			public readonly List<DKItem> lstCbdkxx = new List<DKItem>();
			public readonly List<QSSJ_CBF_JTCY> lstCbfJtcy = new List<QSSJ_CBF_JTCY>();
			///// <summary>
			///// 原登记簿ID
			///// </summary>
			//public readonly HashSet<string> yDjbid = new HashSet<string>();
			public readonly List<DJ_CBJYQ_YDJB> lstYdjb = new List<DJ_CBJYQ_YDJB>();

			public void Load(string fileName)
			{

				//var dk=new CrudRepository<DLXX_DK, string>().FindFirst(t => t.ID == "faeabcb0-e311-41db-a53f-d29bf7c91039");

				var sJson=File.ReadAllText(fileName);
				//var lstDjDjbProperties = DJ_CBJYQZ_DJB.GetProperties(null, false);
				var lstDjbProperties= EntityUtil.GetAttributes<DJ_CBJYQ_DJB>();
				var lstCbhtProperties = EntityUtil.GetAttributes<DJ_CBJYQ_CBHT>();
				var lstQssjCbfProperties = EntityUtil.GetAttributes<QSSJ_CBF>();
				var lstDjCbfProperties = EntityUtil.GetAttributes<DJ_CBJYQ_CBF>();
				var lstCbdkxxProperties = EntityUtil.GetAttributes<QSSJ_CBDKXX>();
				var lstDkProperties = EntityUtil.GetAttributes<DLXX_DK>();
				var lstDjDkxxProperties = EntityUtil.GetAttributes<DJ_CBJYQ_DKXX>();
				var lstCbfJtcyProperties = EntityUtil.GetAttributes<QSSJ_CBF_JTCY>();
				var djYdjbProperties = EntityUtil.GetAttributes<DJ_CBJYQ_YDJB>();
				var obj = JObject.Parse(sJson);

				var hasDkxx = false;
				var hasJtcy = false;

				string cbfmc = null;
				string qxdm = null;
				foreach (var kv in obj)
				{
					var fieldName = kv.Key;
					if (kv.Value is JObject jo)
					{
						if (fieldName == "cbht")
						{
							foreach (var kv1 in jo)
							{
								if (kv1.Value is JValue jv1)
								{
									var fok = WriteProperty(jv1, Cbht, kv1.Key, lstCbhtProperties);
									if (!fok)
									{
										Console.WriteLine($"cbht:{kv1.Key}:{jv1.Value} not in {QSSJ_CBHT.GetTableName()}");
									}
								}
							}
						}
						else if (fieldName == "qssjcbf")
						{
							HasQssjCbf = true;
							foreach (var kv1 in jo)
							{
								if (kv1.Value is JValue jv1)
								{
									var fok1 = WriteProperty(jv1, Cbf.QsCbf, kv1.Key, lstQssjCbfProperties);
									var fok2 = WriteProperty(jv1, Cbf.DjCbf, kv1.Key, lstDjCbfProperties);
									if (!fok1&&!fok2)
									{
										Console.WriteLine($"qssfCbf:{kv1.Key}:{jv1.Value} not in {QSSJ_CBF.GetTableName()}");
									}
								}
							}
						}
					}
					else if (kv.Value is JArray ja)
					{
						switch (fieldName)
						{
							case "dkxx":
								hasDkxx = true;
								foreach (var o in ja)
								{
									if (o is JObject jo1)
									{
										var en = new DKItem();
										en.DlxxDk.CBFMC = cbfmc;
										lstCbdkxx.Add(en);
										foreach (var kv1 in jo1)
										{
											if (kv1.Value is JValue jv1)
											{
												var fok1 = WriteProperty(jv1, en.QsDkxx, kv1.Key, lstCbdkxxProperties);
												var fok2 = WriteProperty(jv1, en.DlxxDk, kv1.Key, lstDkProperties);
												var fok3 = WriteProperty(jv1, en.DjDkxx, kv1.Key, lstDjDkxxProperties);
												if (!fok1 && !fok2&&!fok3)
												{
													if (lstCbdkxx.Count == 1)
														Console.WriteLine($"dkxx:{kv1.Key}:{jv1.Value} not in {DLXX_DK.GetTableName()} and {QSSJ_CBDKXX.GetTableName()} and {DJ_CBJYQ_DKXX.GetTableName()}");
												}
											}
										}
									}
								}
								break;
							case "familyMember":
								hasJtcy = true;
								foreach (var o in ja)
								{
									if (o is JObject jo1)
									{
										var en = new QSSJ_CBF_JTCY();
										lstCbfJtcy.Add(en);
										foreach (var kv1 in jo1)
										{
											if (kv1.Value is JValue jv1)
											{
												if(!WriteProperty(jv1, en, kv1.Key, lstCbfJtcyProperties))
												{
													if (lstCbfJtcy.Count == 1)
														Console.WriteLine($"familyMember:{kv1.Key}:{jv1.Value} not in {QSSJ_CBF_JTCY.GetTableName()}");
												}
											}
										}
									}
								}
								break;
							case "ydjbs":
								{
									HasYdjbid = true;
									foreach (var o in ja)
									{
										if (o is JObject jo1)
										{
											var en = new DJ_CBJYQ_YDJB();
											lstYdjb.Add(en);
											foreach (var kv1 in jo1)
											{
												if (kv1.Value is JValue jv1)
												{
													WriteProperty(jv1, en, kv1.Key, djYdjbProperties);
												}
											}
										}
									}
								}break;
						}
					}
					else if (kv.Value is JValue jv)
					{
						if (fieldName == "cbfmc")
							cbfmc = jv.Value?.ToString();
						else if (fieldName == "dbsj")
							fieldName = "DJSJ";
						else if (fieldName == "szdy")
						{
							var s = jv.Value?.ToString();
							if (s != null && s.Length > 5)
							{
								qxdm = s.Substring(0, 6);
							}
						}
						if(!WriteProperty(jv, Djb, fieldName, lstDjbProperties))
						{
							Console.WriteLine($"DJB:{fieldName}:{jv.Value} not in {DJ_CBJYQ_DJB.GetTableName()}");
						}
					}
				}

				if (!hasJtcy || !hasDkxx)
				{
					throw new Exception("无效的数据格式");
				}

				Djb.ID = Cbht.DJBID;
				Djb.CBJYQZBM = Cbht.CBHTBM;
				Djb.CBQXQ = Cbht.CBQXQ;
				Djb.CBQXZ = Cbht.CBQXZ;
				Djb.CBFS = Cbht.CBFS;
				Djb.DJYY = "登记数据";
				Djb.QXDM = qxdm;
				Djb.QSZT = EQszt.Xians;
				Cbf.DjCbf.DJBID = Djb.ID;
				Cbf.QsCbf.FBFBM = Djb.FBFBM;
				Cbf.DjCbf.CBFCYSL= lstCbfJtcy.Count;
				Cbf.QsCbf.CBFCYSL = lstCbfJtcy.Count;
			}
			static bool WriteProperty(JValue jv, object entity, string fieldName, List<EntityProperty> entityProperties)
			{
				var p = entityProperties.Find(it => StringUtil.isEqualIgnorCase(fieldName, EntityUtil.GetFieldName(entity, it.PropertyName)));
				if (p != null)
				{
					try
					{
						var o = jv.Value;
						if (o != null)
						{
							if (p.PropertyType.IsEnum)
							{
								o = Enum.Parse(p.PropertyType, o.ToString());
							}
							else if (p.PropertyType == typeof(int)|| p.PropertyType == typeof(short))
							{
								var s = o.ToString();
								if (jv.Type == JTokenType.Boolean)
								{
									o = s == "true" ? 1 : 0;
								}
							}
							entity.SetPropertyValue(p.PropertyName, o);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						throw ex;
					}
				}

				return p != null;
			}
		}
		public ImportWwDjbJsonTask()
		{
			Name = "导入业务数据更新包";
			Description = "导入由登记颁证系统导出的业务更新数据";
			PropertyPage = new ImportNwDataPanel("业务数据更新包(*.dat) | *.dat", "FBCB9A83-476F-42D8-B90E-9179DA1B4FA2");
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) =>
			{
				if (0==t.ErrorCount())
				{
					ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
				}
			};
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = PropertyPage as ImportNwDataPanel;
			var db = MyGlobal.Workspace;
			try
			{
				var data = new JsonData();
				data.Load(prm.FileName);
				if (!data.HasYdjbid)
				{
					throw new Exception("数据格式不正确，缺少ydjbs属性！");
				}
				if (!data.HasQssjCbf)
				{
					throw new Exception("数据格式不正确，缺少qssjcbf属性！");
				}
				var djDjbRepos =DjDjbRepository.Instance;
				var djCbhtRepos = DjCbhtRepository.Instance;
				var djCbfRepos = DjCbfRepository.Instance;
				var djDkxxRepos = DjDkxxRepository.Instance;
				var djYdjbRepos = DjYdjbRepository.Instance;
				var dlxxDkRepos = DlxxDkRepository.Instance;
				var qssjDkxxRepos = QssjDkxxRepository.Instance;
				var qssjCbfRepos = QssjCbfRepository.Instance;
				var qssjCbfJtcyRepos = QssjCbfJtcyRepository.Instance;
				var bgjlRepos = TxbgjlRepository.Instance;

				var err = djDjbRepos.IsValid(data.Djb);
				if (err != null)
				{
					throw new Exception(err);
				}
				#region 处理注销登记簿
				if (data.Djb.SFYZX)
				{
					var zxDjbID = data.Djb.ID;
					if (data.lstYdjb.Count == 1 && data.lstYdjb[0].BGLX == EYwBGLX.ZhuanRang)
					{//转让注销
						zxDjbID = data.lstYdjb[0].YDJBID;
					}
					var en = djDjbRepos.Find(t => t.ID == zxDjbID, (c, t) => c(t.SFYZX));
					if (en == null)
					{
						ReportError($"未找到要注销的登记簿：ID={zxDjbID}！");
						ReportProgress(100);
						return;
					}
					if (en.SFYZX)//已注销，返回
					{
						ReportProgress(100);
						return;
					}
					en.ID = zxDjbID;
					en.SFYZX = true;
					djDjbRepos.Update(en, t => t.ID == en.ID, (c, t) => c(t.SFYZX));
					var subSql = $"select DKBM from {DJ_CBJYQ_DKXX.GetTableName()} where DJBID='{en.ID}'";
					var sql = $"update {DLXX_DK.GetTableName()} set DJZT={(int)EDjzt.Wdj},ZT={(int)EDKZT.Lishi},CBFMC=null where DKBM in ({subSql})";
					db.ExecuteNonQuery(sql);
					ReportProgress(100);
					return;
				}
				#endregion
				if (djDjbRepos.Exists(t => t.ID == data.Djb.ID))
				{
					ReportInfomation($"登记簿{data.Djb.ID}已存在！");
					ReportProgress(100);
					return;
				}
				var cnt = 2 + data.lstCbdkxx.Count + data.lstCbfJtcy.Count;

				var progress = new ProgressReporter(ReportProgress, cnt);

				var lstDkbm = QueryYDJBDkbms(data);//需要修改状态的地块（地块编码）:已登记->未登记

				db.BeginTransaction();

				#region 更新DLXX_DK中原登记簿对应地块的DJZT,ZT等属性（修改原登记簿中的地块的状态等属性）
				if (lstDkbm.Count>0)
				{
					var sin = string.Join(",", lstDkbm.TryToList().Select(it=>$"'{it}'"));
					var sql = $"update {DLXX_DK.GetTableName()} set DJZT={(int)EDjzt.Wdj},ZT={(int)EDKZT.Lishi},CBFMC=null where DKBM in({sin})";
					db.ExecuteNonQuery(sql);
					//base.ReportInfomation(sql);
				}
				#endregion

				#region 写入原登记簿数据（DJ_CBJYQ_YDJB)
				if (data.lstYdjb.Count > 0)
				{
					foreach (var en in data.lstYdjb)
					{
						DjYdjbRepository.Instance.Insert(en);
					}
				}
				#endregion

				#region 修改DJ_CBJYQ_DJB的QSZT属性
				foreach (var en in data.lstYdjb)
				{
					db.ExecuteNonQuery($"update {DJ_CBJYQ_DJB.GetTableName()} set QSZT={(int)EQszt.History} where ID='{en.YDJBID}'");
				}
				#endregion

				djDjbRepos.Insert(data.Djb);//写入登记簿数据

				#region 更新QSSJ_CBJYQZDJB
				{
					var repos = QssjDjbRepository.Instance;
					var en=repos.Find(t => t.CBJYQZBM == data.Djb.CBJYQZBM);
					if (en != null)
					{
						en.Copy(data.Djb, (c, t) => c(t.ID));
						repos.Update(en, t => t.ID == en.ID);
					}
					else
					{
						en = new QSSJ_CBJYQZDJB();
						en.Copy(data.Djb, (c, t) => c(t.ID));
						repos.Insert(en);
					}
				}
				#endregion

				progress.Step();

				djCbhtRepos.Insert(data.Cbht);//写入登记承包合同(DJ_CBJYQ_CBHT)

				#region 更新 QSSJ_CBJYQZDJB
				{
					var repos =QssjCbhtRepository.Instance;
					var en = repos.Find(t => t.CBHTBM == data.Cbht.CBHTBM);
					if (en != null)
					{
						en.Copy(data.Cbht, (c, t) => c(t.ID));
						repos.Update(en, t => t.ID == en.ID);
					}
					else
					{
						en = new QSSJ_CBHT();
						en.Copy(data.Cbht, (c, t) => c(t.ID));
						repos.Insert(en);
					}
				}
				#endregion

				progress.Step();
				
				var cbfmc = data.Djb.CBFMC;
				#region 更新DLXX_DK及QSSJ_CBDKXX
				foreach (var it in data.lstCbdkxx)
				{
					progress.Step();

					djDkxxRepos.Insert(it.DjDkxx);//写入DJ_CBJYQ_DKXX
					ReportInfomation($"写入DJ_CBJYQ_DKXX:{it.DjDkxx.DKBM}");

					#region 更新DLXX_DK中的地块
					{
						var enDk = dlxxDkRepos.FindNoBinary(t => t.DKBM == it.DlxxDk.DKBM);
						if (enDk != null)
						{
							//var jsonDk = it.DlxxDk;
							//jsonDk.ID = enDk.ID;
							enDk.DJSJ = data.Djb.DJSJ;
							enDk.ZT = EDKZT.Youxiao;
							enDk.DJZT = EDjzt.Ydj;
							enDk.CBFMC = data.Djb.CBFMC;
							//dlxxDkRepos.UpdateNoBinary(enDk, t => t.BSM == enDk.BSM);
							dlxxDkRepos.Update(enDk, t => t.BSM == enDk.BSM, (c, t) => c(t.DJSJ,t.ZT,t.DJZT,t.CBFMC));
							ReportInfomation($"更新DLXX_DK:DKBM={enDk.DKBM} ,CBFMC={enDk.CBFMC}");
						}
						else
						{
							throw new Exception($"在DLXX_DK表中未找到DKBM={it.DlxxDk.DKBM}的记录!");
						}
					}
					#endregion

					#region 更新QSSJ_CBDKXX中的地块
					{
						var jsonDk = it.QsDkxx;
						var en = qssjDkxxRepos.FindNoBinary(t => t.DKBM == jsonDk.DKBM);
						if (en != null)
						{
							en.OverWrite(jsonDk, (c, t) => c(t.ID));
							qssjDkxxRepos.UpdateNoBinary(en, t => t.ID == en.ID);
						}
						else
						{
							qssjDkxxRepos.Insert(jsonDk);
						}
					}
					#endregion
				}
				#endregion

				djCbfRepos.Insert(data.Cbf.DjCbf);//写入DJ_CBJYQ_CBF

				#region 更新QSSJ_CBF
				{
					var repos=QssjCbfRepository.Instance;
					var en = repos.Find(t => t.CBFBM == data.Cbht.CBFBM);
					if (en != null)
					{
						en.CBFCYSL = data.lstCbfJtcy.Count;
						repos.Update(en, t => t.CBFBM == en.CBFBM, (c, t) => c(t.CBFCYSL));
					}
					else
					{
						repos.Insert(data.Cbf.QsCbf);
					}
				}
				#endregion

				#region 更新承包方家庭成员数据

				qssjCbfJtcyRepos.Delete(t => t.CBFBM == data.Cbht.CBFBM);
				foreach (var it in data.lstCbfJtcy)
				{
					progress.Step();
					qssjCbfJtcyRepos.Insert(it);
				}
				#endregion

				db.Commit();
			}
			catch (Exception ex)
			{
				db.Rollback();
				ReportException(ex);
			}

			ReportProgress(100);
		}
		/// <summary>
		/// 查询需要修改状态的地块（地块编码）
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private HashSet<string> QueryYDJBDkbms(JsonData data)
		{
			var lstDkbm = new HashSet<string>();
			if (data.lstYdjb.Count == 1 && !data.Djb.SFYZX)
			{
				var en = data.lstYdjb[0];
				if (en.BGLX == EYwBGLX.YBBG || en.BGLX == EYwBGLX.GengZheng)
				{
					var sql = $"select dkbm from {DJ_CBJYQ_DKXX.GetTableName()} where DJBID ='{en.YDJBID}' and dkbm is not null";
					MyGlobal.Workspace.QueryCallback(sql, r => lstDkbm.Add(r.GetString(0)));
				}
			}
			return lstDkbm;
		}
	}

}
