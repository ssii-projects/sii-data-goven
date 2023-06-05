using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Agro.Module.DataExchange.XmlTask
{
	class XmlData
	{
		public readonly Head Head;
		public readonly BusinessData BusinessData;
		internal readonly OriginalData OriginalData=new OriginalData();
		internal readonly OriginalData ChangeData=new OriginalData();

		internal readonly Task _task;
		internal readonly IFeatureWorkspace _db;

		public XmlData(Task task, IFeatureWorkspace db,Head head)
		{
			_task = task;
			_db = db;
			Head = head;
			BusinessData = BusinessDataFactory.Create(head.SJLX);
			BusinessData.FinalConstruct(this);// task,db);
		}
		public void Query(ExportXmlPanel.Item it)
		{
			var sql = $"select max(YWH) from DJ_YW_SLSQ where YWH<'{it.Ywh}'";
			BusinessData.QYWLSH = SafeConvertAux.ToStr(_db.QueryOne(sql));
			BusinessData.YWLSH = it.Ywh;
			var lstDjbID=QueryDjb(it);
			if (BusinessData.HasOriginData())
			{
				QueryOriginalData(lstDjbID);
			}
			if (BusinessData.HasChangeData())
			{
				QueryChangeData(lstDjbID);
			}
			BusinessData.Query(it,lstDjbID,OriginalData,ChangeData);
		}
		public void ReadXml(XmlDocument xml)
		{
			BusinessData.ReadXml(xml);
			ReadOriginData(xml);
			BusinessData.ReadChangeData(xml, xml.SelectSingleNode("/SUBMIT/CHANGE_DATA"));
		}
		public void Import()
		{
			try
			{
				var enSLSQ = new DJ_YW_SLSQ()
				{
					DJXL= XmlExchangeUtil.SJLXToDjxl(Head.SJLX)
				};
				Head.SQID = enSLSQ.ID;

				BusinessData.FillEntities(enSLSQ);

				_db.BeginTransaction();
				BusinessData.Import(enSLSQ);
				_db.Commit();
			}
			catch (Exception ex)
			{
				_db.Rollback();
				_task.ReportException(ex);
			}
		}
		public void WriteXml(XmlDocument xml, int srid)
		{
			Head.WriteXml(xml, xml.SelectSingleNode(" / SUBMIT/HEAD"));
			BusinessData.WriteXml(xml, xml.SelectSingleNode("/SUBMIT/BUSINESS_DATA"),srid);
			BusinessData.CheckExport(OriginalData, ChangeData);
			OriginalData.WriteXml(xml, xml.SelectSingleNode("/SUBMIT/ORIGINAL_DATA"), srid.ToString(),true);
			if (ChangeData.lstDjb.Count > 0) {
				BusinessData.ModifyCzfs(ChangeData);

				bool fExportFBF = true;
				if (OriginalData.lstDjb.Count > 0 && OriginalData.lstDjb[0].Fbf != null && ChangeData.lstDjb[0].Fbf != null && OriginalData.lstDjb[0].Fbf.IsXmlDataEqual(ChangeData.lstDjb[0].Fbf))
				{
					fExportFBF = false;
				}
				ChangeData.WriteXml(xml, xml.SelectSingleNode("/SUBMIT/CHANGE_DATA"), srid.ToString(),fExportFBF);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="it"></param>
		/// <returns> [登记簿ID,原登记簿ID]</returns>
		List<DJBIDPair> QueryDjb(ExportXmlPanel.Item it)
		{
			var lstDjbID = new List<DJBIDPair>();
			var sql = $"select a.FBFBM,a.ID DJBID,d.YDJBID from DJ_CBJYQ_DJB a  left join DJ_CBJYQ_YDJB d on a.ID=d.DJBID where a.ID in (select QLID from DJ_YW_SQQL where SQID='{it.ID}')";
			_db.QueryCallback(sql, r =>
			{
				BusinessData.FBFBM = GetStr(r, 0);
				var id = new DJBIDPair()
				{
					DJBID = GetStr(r, 1),
					YDJBID = GetStr(r, 2),
				};
				if (it.DJXL <= 0 || it.DJXL>5)
				{
					id.YDJBID = null;
				}
				lstDjbID.Add(id);
				return true;
			});
			//foreach (var id in lstDjbID)
			//{
			//	if (id.YDJBID != null)
			//	{
			//		sql = $"select YWH from DJ_CBJYQ_DJB where ID='{id.YDJBID}'";
			//		id.YYWH = SafeConvertAux.ToStr(_db.QueryOne(sql));
			//	}
			//}
			return lstDjbID;
		}

		void ReadOriginData(XmlDocument xml)
		{
			ReadData(xml, xml.SelectSingleNode("/SUBMIT/ORIGINAL_DATA"), OriginalData);
		}
		//void ReadChangeData(XmlDocument xml)
		//{
		//	ReadData(xml, xml.SelectSingleNode("/SUBMIT/CHANGE_DATA"),ChangeData);
		//}
		internal void ReadData(XmlDocument xml, XmlNode parent, OriginalData od)
		{
			//var od = new OriginalData();
			var lstFbf = new List<Fbf>();
			var lstCbf = new List<Cbf>();
			var lstJtcy = new List<Jtcy>();
			var lstCbht = new List<Cbht>();
			var lstDjb = new List<Cbjyqzdjb>();
			var lstQz = new List<Cbjyqz>();
			var lstDk = new List<Cbdk>();
			var lstDkxx = new List<Cbdkxx>();
			var lstJzd = new List<Jzd>();
			var lstJzx = new List<Jzx>();
			foreach (var cn in parent.ChildNodes)
			{
				if (cn is XmlElement xe)
				{
					switch (xe.Name)
					{
						case "FBF":
							{
								var it = new Fbf();
								it.ReadXml(xe);
								lstFbf.Add(it);
							}
							break;
						case "CBF":
							{
								var it = new Cbf();
								it.ReadXml(xe);
								lstCbf.Add(it);
							}
							break;
						case "CBHT":
							{
								var it = new Cbht();
								it.ReadXml(xe);
								lstCbht.Add(it);
							}
							break;
						case "CBJYQZDJB":
							{
								var it = new Cbjyqzdjb();
								it.ReadXml(xe);
								lstDjb.Add(it);
							}
							break;
						case "CBJYQZ":
							{
								var it = new Cbjyqz();
								it.ReadXml(xe);
								lstQz.Add(it);
							}
							break;
#if DEBUG
						case "CBFJTCY":
							{
								foreach (var n in xe.ChildNodes)
								{
									if (n is XmlElement e)
									{
										var it = new Jtcy();
										it.ReadXml(e);
										lstJtcy.Add(it);
									}
								}
							}
							break;
						case "CBDKS":
							{
								foreach (var n in xe.ChildNodes)
								{
									if (n is XmlElement e)
									{
										var it = new Cbdk();
										it.ReadXml(e);
										lstDk.Add(it);
									}
								}
							}
							break;
						case "CBDKXXS":
							{
								foreach (var n in xe.ChildNodes)
								{
									if (n is XmlElement e)
									{
										var it = new Cbdkxx();
										it.ReadXml(e);
										lstDkxx.Add(it);
									}
								}
							}
							break;
#endif
						case "JTCY":
							{
								var it = new Jtcy();
								it.ReadXml(xe);
								lstJtcy.Add(it);
							}
							break;
						case "CBDK":
							{
								var it = new Cbdk();
								it.ReadXml(xe);
								lstDk.Add(it);
							}
							break;
						case "CBDKXX":
							{
								var it = new Cbdkxx();
								it.ReadXml(xe);
								lstDkxx.Add(it);
							}
							break;
						case "DKJZD":
							{
								foreach (var n in xe.ChildNodes)
								{
									if (n is XmlElement e)
									{
										var it = new Jzd();
										it.ReadXml(e);
										lstJzd.Add(it);
									}
								}
							}
							break;
						case "DKJZX":
							{
								foreach (var n in xe.ChildNodes)
								{
									if (n is XmlElement e)
									{
										var it = new Jzx();
										it.ReadXml(e);
										lstJzx.Add(it);
									}
								}
							}
							break;
						case "TYZB":
							od.TYZB = xe.InnerText;
							break;
					}
				}
			}


			foreach (var djb in lstDjb)
			{
				djb.Fbf = lstFbf.Find(a => { return a.FBFBM == djb.FBFBM; });
				//if (djb.Fbf != null) lstFbf.Remove(djb.Fbf);
				djb.Cbf = lstCbf.Find(a => { return a.CBFBM == djb.CBFBM; });
				if (djb.Cbf != null) lstCbf.Remove(djb.Cbf);
				djb.Cbht = lstCbht.Find(a => { return a.CBHTBM == djb.CBJYQZBM; });
				if (djb.Cbht != null)
				{
					lstCbht.Remove(djb.Cbht);
				}
				if (!string.IsNullOrEmpty(djb.CBFBM))
				{
					foreach (var it in lstJtcy)
					{
						if (it.CBFBM == djb.CBFBM) djb.jtcies.Add(it);
					}
					lstJtcy.RemoveAll(a => { return a.CBFBM == djb.CBFBM; });
				}

				if (!string.IsNullOrEmpty(djb.CBJYQZBM))
				{
					djb.Cbjyqz = lstQz.Find(a => { return a.CBJYQZBM == djb.CBJYQZBM; });
					lstQz.RemoveAll(a => { return a.CBJYQZBM == djb.CBJYQZBM; });

					foreach (var it in lstDkxx)
					{
						if (it.CBJYQZBM == djb.CBJYQZBM) djb.Cbdkxx.Add(it);
						var dk = lstDk.Find(a => { return a.Dk?.DKBM == it.DKBM; });
						if (dk != null)
						{
							djb.Cbdk.Add(dk);
							lstDk.Remove(dk);
							foreach (var jt in lstJzd)
							{
								if (jt.DKBM == dk.Dk?.DKBM) djb.DkJzd.Add(jt);
							}
							lstJzd.RemoveAll(a => { return a.DKBM == dk.Dk?.DKBM; });
							foreach (var jt in lstJzx)
							{
								if (jt.DKBM == dk.Dk?.DKBM) djb.DkJzx.Add(jt);
							}
							lstJzx.RemoveAll(a => { return a.DKBM == dk.Dk?.DKBM; });
						}
					}
					lstDkxx.RemoveAll(a => { return a.CBJYQZBM == djb.CBJYQZBM; });
				}

				od.lstDjb.Add(djb);
			}
		}

		private void QueryOriginalData(List<DJBIDPair> lstDjbID)
		{
			foreach (var v in lstDjbID)
			{
				if (v.YDJBID != null)
				{
					QueryData(v.YDJBID, OriginalData);
				}
			}
		}
		private void QueryChangeData(List<DJBIDPair> lstDjbID)
		{
			foreach (var v in lstDjbID)
			{
				QueryData(v.DJBID, ChangeData);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="db"></param>
		/// <param name="djbID">登记簿ID</param>
		/// <param name="yDjbID">原登记簿ID</param>
		/// <param name="data"></param>
		void QueryData(string djbID, OriginalData data)
		{
			if (data.lstDjb.Find(a => { return a.DJBID == djbID; }) != null)
			{
				return;
			}
			var db = _db;
			Cbjyqzdjb djb = null;
			//var sql = $"select CBJYQZBM,a.FBFBM,a.CBFBM,a.CBFS,CBQX,a.CBQXQ,a.CBQXZ,CBJYQZLSH,FJ,YCBJYQZBH,DBR,DJSJ,b.CBHTBM from DJ_CBJYQ_DJB a left join DJ_CBJYQ_CBHT b on a.ID=b.DJBID where a.ID='{djbID}'";
			var sql = $" select CBJYQZBM,FBFBM,CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,CBJYQZLSH,FJ,YCBJYQZBH,DBR,DJSJ from DJ_CBJYQ_DJB  where ID='{djbID}'";
			db.QueryCallback(sql, r =>
			{
				int i = -1;
				djb = new Cbjyqzdjb
				{
					DJBID = djbID,
					CBJYQZBM = GetStr(r, ++i),
					FBFBM = GetStr(r, ++i),
					CBFBM = GetStr(r, ++i),
					CBFS = GetStr(r, ++i),
					CBQX = GetStr(r, ++i),
					CBQXQ = GetDate(r, ++i),
					CBQXZ = GetDate(r, ++i),
					CBJYQZLSH = GetStr(r, ++i),
					DJBFJ = GetStr(r, ++i),
					YCBJYQZBH = GetStr(r, ++i),
					DBR = GetStr(r, ++i),
					DJSJ = GetDate(r, ++i),
					//CBHTBM=GetStr(r,++i),
				};
				data.lstDjb.Add(djb);
				return false;
			});
			if (djb == null)
				return;

			if (!string.IsNullOrEmpty(djb.FBFBM))//&& djb.Fbf.Find(a=> { return a.FBFBM == sFBFBM; })==null)
			{
				sql = $"select FBFMC,FBFFZRXM,FZRZJLX,FZRZJHM,LXDH,FBFDZ,YZBM,FBFDCY,FBFDCRQ,FBFDCJS from QSSJ_FBF where FBFBM='{djb.FBFBM}'";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					djb.Fbf = new Fbf
					{
						FBFBM = djb.FBFBM,
						FBFMC = GetStr(r, ++i),
						FBFFZRXM = GetStr(r, ++i),
						FZRZJLX = GetStr(r, ++i),
						FZRZJHM = GetStr(r, ++i),
						LXDH = GetStr(r, ++i),
						FBFDZ = GetStr(r, ++i),
						YZBM = GetStr(r, ++i),
						FBFDCY = GetStr(r, ++i),
						FBFDCRQ = GetDate(r, ++i),
						FBFDCJS = GetStr(r, ++i)
					};
					return false;
				});
			}
			if (!string.IsNullOrEmpty(djb.CBFBM))//&&data.Cbf.Find(a=> { return a.CBFBM == sCBFBM; })==null)
			{
				sql = $"select CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR from QSSJ_CBF where CBFBM='{djb.CBFBM}'";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					djb.Cbf = new Cbf
					{
						CBFBM = djb.CBFBM,
						CBFLX = GetStr(r, ++i),
						CBFMC = GetStr(r, ++i),
						CBFZJLX = GetStr(r, ++i),
						CBFZJHM = GetStr(r, ++i),
						CBFDZ = GetStr(r, ++i),
						YZBM = GetStr(r, ++i),
						LXDH = GetStr(r, ++i),
						CBFCYSL = GetStr(r, ++i),
						CBFDCRQ = GetDate(r, ++i),
						CBFDCY = GetStr(r, ++i),
						CBFDCJS = GetStr(r, ++i),
						GSJS = GetStr(r, ++i),
						GSJSR = GetStr(r, ++i),
						GSSHRQ = GetDate(r, ++i),
						GSSHR = GetStr(r, ++i)
					};
					//data.Cbf.Add(it);
					return false;
				});

				sql = $"select CYXM,CYXB,CYZJLX,CYZJHM,YHZGX,CYBZ,SFGYR,CYBZSM,ID from QSSJ_CBF_JTCY where CBFBM='{djb.CBFBM}'";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					var jtcy = new Jtcy
					{
						CBFBM = djb.CBFBM,
						CYXM = GetStr(r, ++i),
						CYXB = GetStr(r, ++i),
						CYZJLX = GetStr(r, ++i),
						CYZJHM = GetStr(r, ++i),
						YHZGX = GetStr(r, ++i),
						CYBZ = GetStr(r, ++i),
						SFGYR = GetStr(r, ++i),
						CYBZSM = GetStr(r, ++i),
						ID = GetStr(r, ++i)
					};

					djb.jtcies.Add(jtcy);

					return true;
				});
			}
			if (!string.IsNullOrEmpty(djb.CBHTBM))//&&data.Cbht.Find(a=> { return a.CBHTBM == sCBHTBM; })==null)
			{
				sql = $"select YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM from QSSJ_CBHT where CBHTBM='{djb.CBHTBM}'";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					djb.Cbht = new Cbht
					{
						CBHTBM = djb.CBHTBM,
						YCBHTBM = GetStr(r, ++i),
						FBFBM = GetStr(r, ++i),
						CBFBM = GetStr(r, ++i),
						CBFS = GetStr(r, ++i),
						CBQXQ = GetDate(r, ++i),
						CBQXZ = GetDate(r, ++i),
						HTZMJ = GetDouble(r, ++i),
						CBDKZS = GetInt(r, ++i),
						QDSJ = GetDate(r, ++i),
						HTZMJM = GetDouble(r, ++i),
						YHTZMJ = GetDouble(r, ++i),
						YHTZMJM = GetDouble(r, ++i)
					};
					//data.Cbht.Add(it);
					return false;
				});
			}
			if (!string.IsNullOrEmpty(djb.CBJYQZBM))//&&!string.IsNullOrEmpty(ywh))
			{
				//sql = $"select FZJG,FZRQ,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM,ID,SFYZX from DJ_CBJYQ_QZ where  CBJYQZBM='{djb.CBJYQZBM}' and DJBID='{djbID}' and YWH='{ywh}'";
				sql = $"select FZJG,FZRQ,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM,ID,SFYZX from DJ_CBJYQ_QZ where  CBJYQZBM='{djb.CBJYQZBM}' and DJBID='{djbID}' order by FZRQ desc";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					djb.Cbjyqz=new Cbjyqz
					{
						CBJYQZBM = djb.CBJYQZBM,
						FZJG = GetStr(r, ++i),
						FZRQ = GetDate(r, ++i),
						QZSFLQ = GetStr(r, ++i),
						QZLQRQ = GetDate(r, ++i),
						QZLQRXM = GetStr(r, ++i),
						QZLQRZJLX = GetStr(r, ++i),
						QZLQRZJHM = GetStr(r, ++i),
						ID = GetStr(r, ++i),
						SFYZX = SafeConvertAux.ToInt32(r.GetValue(++i)) == 1,
						YWH=BusinessData.YWLSH,
					};
					return false;
				});
			}

			var lstDkbm = new HashSet<string>();
			sql = $"select DKBM,FBFBM,CBFBM,CBJYQQDFS,HTMJ,CBHTBM,LZHTBM,CBJYQZBM,YHTMJ,HTMJM,YHTMJM,SFQQQG from DJ_CBJYQ_DKXX where DJBID='{djbID}' and DKBM is not null";
			db.QueryCallback(sql, r =>
			{
				var dkbm = r.GetString(0);
				if (djb.Cbdkxx.Find(a => { return a.DKBM == dkbm; }) == null)
				{
					lstDkbm.Add(dkbm);
					int i = 0;
					var it = new Cbdkxx()
					{
						DKBM = dkbm,
						FBFBM = GetStr(r, ++i),
						CBFBM = GetStr(r, ++i),
						CBJYQQDFS = GetStr(r, ++i),
						HTMJ = GetDouble(r, ++i),
						CBHTBM = GetStr(r, ++i),
						LZHTBM = GetStr(r, ++i),
						CBJYQZBM = GetStr(r, ++i),
						YHTMJ = GetDouble(r, ++i),
						HTMJM = GetDouble(r, ++i),
						YHTMJM = GetDouble(r, ++i),
						SFQQQG = GetStr(r, ++i),
					};
					djb.Cbdkxx.Add(it);
				}
				return true;
			});
			foreach (var dkbm in lstDkbm)
			{
				var it = new Cbdk();
				djb.Cbdk.Add(it);

				var wh = $"DKBM='{dkbm}'";
				sql = $"select YDKBM,BSM,YSDM,DKMC,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,DKDZ,DKXZ,DKNZ,DKBZ,DKBZXX,ZJRXM,KJZB,SCMJM,SHAPE from DLXX_DK where {wh}";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					it.YCBDKBM = GetStr(r, ++i);
					it.Dk = new Dk()
					{
						DKBM = dkbm,
						BSM = GetStr(r, ++i),
						YSDM = GetStr(r, ++i),
						DKMC = GetStr(r, ++i),
						SYQXZ = GetStr(r, ++i),
						DKLB = GetStr(r, ++i),
						TDLYLX = GetStr(r, ++i),
						DLDJ = GetStr(r, ++i),
						TDYT = GetStr(r, ++i),
						SFJBNT = GetStr(r, ++i),
						SCMJ = GetDouble(r, ++i),
						DKDZ = GetStr(r, ++i),
						DKXZ = GetStr(r, ++i),
						DKNZ = GetStr(r, ++i),
						DKBZ = GetStr(r, ++i),
						DKBZXX = GetStr(r, ++i),
						ZJRXM = GetStr(r, ++i),
						KJZB = GetStr(r, ++i),
						SCMJM = GetDouble(r, ++i),
					};
					var o = r.GetValue(++i);
					if (o is IGeometry geo)
					{
						it.SHP = geo.AsText();
					}
					return false;
				});

				if (it.Dk != null)
				{
					sql = $"select CBFBM from DJ_CBJYQ_DKXX where DKBM='{it.Dk.DKBM}' and DJBID in(select YDJBID from DJ_CBJYQ_YDJB where DJBID='{djbID}')";
					it.YCBFBM = SafeConvertAux.ToStr(db.QueryOne(sql));
				}

				sql = $"select BSM,YSDM,JZDH,JZDLX,JBLX,DKBM,XZBZ,YZBZ,SHAPE from DLXX_DK_JZD where {wh}";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					var it1 = new Jzd()
					{
						BSM = GetStr(r, ++i),
						YSDM = GetStr(r, ++i),
						JZDH = GetStr(r, ++i),
						JZDLX = GetStr(r, ++i),
						JBLX = GetStr(r, ++i),
						DKBM = GetStr(r, ++i),
						XZBZ = GetStr(r, ++i),
						YZBZ = GetStr(r, ++i),
					};
					djb.DkJzd.Add(it1);
					if (r.GetValue(++i) is IGeometry geo)
					{
						it1.SHP = geo.AsText();
					}
					return true;
				});

				sql = $"	select BSM,YSDM,JXXZ,JZXLB,JZXWZ,JZXSM,PLDWQLR,PLDWZJR,JZXH,QJZDH,ZJZDH,DKBM,SHAPE from DLXX_DK_JZX where {wh}";
				db.QueryCallback(sql, r =>
				{
					int i = -1;
					var it1 = new Jzx()
					{
						BSM = GetStr(r, ++i),
						YSDM = GetStr(r, ++i),
						JXXZ = GetStr(r, ++i),
						JZXLB = GetStr(r, ++i),
						JZXWZ = GetStr(r, ++i),
						JZXSM = GetStr(r, ++i),
						PLDWQLR = GetStr(r, ++i),
						PLDWZJR = GetStr(r, ++i),
						JZXH = GetStr(r, ++i),
						QJZDH = GetStr(r, ++i),
						ZJZDH = GetStr(r, ++i),
						DKBM = GetStr(r, ++i),
					};
					if (r.GetValue(++i) is IGeometry geo)
					{
						it1.SHP = geo.AsText();
					}
					djb.DkJzx.Add(it1);
					return true;
				});
			}
		}


		public static string GetStr(IDataReader r, int i)
		{
			return r.IsDBNull(i) ? null : SafeConvertAux.ToStr(r.GetValue(i));
		}
		public static double GetDouble(IDataReader r, int i)
		{
			return r.IsDBNull(i) ? 0 : SafeConvertAux.ToDouble(r.GetValue(i));
		}
		public static int GetInt(IDataReader r, int i)
		{
			return r.IsDBNull(i) ? 0 : SafeConvertAux.ToInt32(r.GetValue(i));
		}
		public static DateTime? GetDate(IDataReader r, int i)
		{
			if (!r.IsDBNull(i))
			{
				var s = r.GetValue(i).ToString();
				if (DateTime.TryParse(s, out DateTime dt))
				{
					return dt;
				}
			}
			return null;
		}
	}

	class DJBIDPair
	{
		/// <summary>
		/// 登记簿ID
		/// </summary>
		public string DJBID;
		/// <summary>
		/// 原登记簿ID
		/// </summary>
		public string YDJBID;
		///// <summary>
		///// 原登记簿业务号
		///// </summary>
		//public string YYWH;
	}

	class Head:XmlSerial
	{
		#region XmlData
		/// <summary>
		/// 发送方（接入码）
		/// </summary>
		public string FSF;
		public readonly string FSSJ;

		/// <summary>
		/// 数据类型
		/// </summary>
		public int SJLX;

		/// <summary>
		/// 数据标识码
		/// </summary>
		public readonly string SJBZM;
		#endregion

		/// <summary>
		/// DJ_YW_SLSQ.ID
		/// </summary>
		internal string SQID;
		internal string SZDY;
		public Head() {
			SJBZM=Guid.NewGuid().ToString();
			FSSJ = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "FSF", FSF);
			AppendTextElementChild(xml, xn, "FSSJ", FSSJ);
			AppendTextElementChild(xml, xn, "SJLX", SJLX.ToString());
			AppendTextElementChild(xml, xn, "SJBZM", SJBZM);
		}
	}
	class OriginalData
	{
		public readonly List<Cbjyqzdjb> lstDjb = new List<Cbjyqzdjb>();
		/// <summary>
		/// 投影坐标
		/// </summary>
		public string TYZB;
		public void WriteXml(XmlDocument xml, XmlNode xn, string srid,bool fExportFbf)
		{
			if (lstDjb.Count == 0)
			{
				return;
			}
			TYZB = srid;

			var jtcies = GetAllExportedJcty();
			var Cbdk = GetAllCbdk();
			var Cbdkxx = GetAllCbdkxx();
			var DkJzd = GetAllJzd();
			var DkJzx = GetAllJzx();
			
			if (fExportFbf && lstDjb[0].Fbf?.ExportXml==true)
			{				
				lstDjb[0].Fbf?.WriteXml(xml, AppendChild(xml, xn, "FBF"));
			}
			foreach (var it in lstDjb)
			{
				if (it.Cbf.ExportXml == true)
				{
					it.Cbf?.WriteXml(xml, AppendChild(xml, xn, "CBF"));
				}
			}

			
#if DEBUG
			if (jtcies.Count > 0)
			{
				var xe = xml.CreateElement("CBFJTCY");
				xn.AppendChild(xe);
				foreach (var jtcy in jtcies)
				{
					jtcy.WriteXml(xml, AppendChild(xml, xe, "JTCY"));
				}
			}
#else
			foreach (var jtcy in jtcies)
			{
				jtcy.WriteXml(xml, AppendChild(xml, xn, "JTCY"));
			}
#endif


			foreach (var it in lstDjb)
			{
				if (it.Cbht != null && it.Cbht.ExportXml)
				{
					it.Cbht.WriteXml(xml, AppendChild(xml, xn, "CBHT"));
				}
			}
			foreach (var it in lstDjb)
			{
				if (it.ExportXml)
				{
					it.WriteXml(xml, AppendChild(xml, xn, "CBJYQZDJB"));
				}
			}
			foreach (var it in lstDjb)
			{
				if (it.Cbjyqz != null && it.Cbjyqz.ExportXml)
				{
					it.Cbjyqz.WriteXml(xml, AppendChild(xml, xn, "CBJYQZ"));
				}
			}


#if DEBUG
			if (Cbdk.Count > 0)
			{
				var xe = xml.CreateElement("CBDKS");
				xn.AppendChild(xe);
				foreach (var it in Cbdk)
					it.WriteXml(xml, AppendChild(xml, xe, "CBDK"));
			}
#else
			foreach (var it in Cbdk)
				it.WriteXml(xml, AppendChild(xml, xn, "CBDK"));
#endif



#if DEBUG
			if (Cbdkxx.Count > 0)
			{
				var xe = xml.CreateElement("CBDKXXS");
				xn.AppendChild(xe);
				foreach (var it in Cbdkxx)
					it.WriteXml(xml, AppendChild(xml, xe, "CBDKXX"));
			}
#else
			foreach (var it in Cbdkxx)
				it.WriteXml(xml, AppendChild(xml, xn, "CBDKXX"));
#endif

			if (DkJzd.Count > 0)
			{
				var xe = xml.CreateElement("DKJZD");
				xn.AppendChild(xe);
				foreach (var it in DkJzd)
				{
					it.WriteXml(xml, AppendChild(xml, xe, "JZD"));
				}
			}
			if (DkJzx.Count > 0)
			{
				var xe = xml.CreateElement("DKJZX");
				xn.AppendChild(xe);
				foreach (var it in DkJzx)
				{
					it.WriteXml(xml, AppendChild(xml, xe, "JZX"));
				}
			}

			if (xn.ChildNodes.Count > 0)
			{
				AppendText(xml, xn, "TYZB", TYZB);
			}
		}
		XmlElement AppendChild(XmlDocument xml, XmlNode xn, string name)
		{
			var xe = xml.CreateElement(name);
			xn.AppendChild(xe);
			return xe;
		}
		void AppendText(XmlDocument xml, XmlNode xn, string name, string text)
		{
			var xe = xml.CreateElement(name);
			if (!string.IsNullOrEmpty(text))
			{
				xe.InnerText = text;
			}
			xn.AppendChild(xe);
		}

		List<Jzx> GetAllJzx()
		{
			var lst = new List<Jzx>();
			foreach (var it in lstDjb)
			{
				foreach (var j in it.DkJzx)
				{
					if (lst.Find(a => { return a.BSM == j.BSM; }) == null)
					{
						lst.Add(j);
					}
				}
			}
			return lst;
		}
		List<Jzd> GetAllJzd()
		{
			var lst = new List<Jzd>();
			foreach (var it in lstDjb)
			{
				foreach (var j in it.DkJzd)
				{
					if (lst.Find(a => { return a.BSM == j.BSM; }) == null)
					{
						lst.Add(j);
					}
				}
			}
			return lst;
		}
		List<Cbdkxx> GetAllCbdkxx()
		{
			var lst = new List<Cbdkxx>();
			foreach (var it in lstDjb)
			{
				foreach (var j in it.Cbdkxx)
				{
					if (lst.Find(a => { return a.DKBM == j.DKBM; }) == null)
					{
						lst.Add(j);
					}
				}
			}
			return lst;
		}
		List<Cbdk> GetAllCbdk()
		{
			var lst = new List<Cbdk>();
			foreach (var it in lstDjb)
			{
				foreach (var j in it.Cbdk)
				{
					if (lst.Find(a => { return a.Dk.DKBM == j.Dk.DKBM; }) == null)
					{
						lst.Add(j);
					}
				}
			}
			return lst;
		}
		List<Jtcy> GetAllExportedJcty()
		{
			var jtcies = new List<Jtcy>();
			foreach (var it in lstDjb)
			{
				if (it.ExportJcties)
				{
					foreach (var j in it.jtcies)
					{
						if (jtcies.Find(a => { return a.ID == j.ID; }) == null)
						{
							jtcies.Add(j);
						}
					}
				}
			}
			return jtcies;
		}
	}

	class XmlSerial
	{
		/// <summary>
		/// 是否需要导出
		/// </summary>
		internal bool ExportXml = true;

		public virtual void WriteXml(XmlDocument xml, XmlNode xn)
		{
		}
		public virtual void ReadXml(XmlNode xn)
		{
		}
		protected static void AppendTextElementChild(XmlDocument xml, XmlNode parent, string name, string text)
		{
			var xe = xml.CreateElement(name);
			if (!string.IsNullOrEmpty(text))
			{
				xe.InnerText = text;
			}
			parent.AppendChild(xe);
		}
	}
	class Fbf : XmlSerial
	{
		public string FBFBM;
		public string FBFMC;//>xx县xxxxxx村村民委员会</FBFMC>
		public string FBFFZRXM;//>xx</FBFFZRXM>
		public string FZRZJLX;//>*</FZRZJLX>
		public string FZRZJHM;//>******************</FZRZJHM>
		public string LXDH;//>***********</LXDH>
		public string FBFDZ;//>xx省xxxxxx村村民委员会</FBFDZ>
		public string YZBM;//>******</YZBM>
		public string FBFDCY;// />
		public DateTime? FBFDCRQ;// />
		public string FBFDCJS;// />
		public string CZFS = "01";//>***</CZFS>



		public bool IsXmlDataEqual(Fbf rhs)
		{
			return FBFBM == rhs.FBFBM && FBFMC == rhs.FBFMC && FBFFZRXM == rhs.FBFFZRXM && FZRZJLX == rhs.FZRZJLX
				&& FZRZJHM == rhs.FZRZJHM && LXDH == rhs.LXDH && FBFDZ == rhs.FBFDZ && YZBM == rhs.YZBM
				&& FBFDCY == rhs.FBFDCY && FBFDCJS == rhs.FBFDCJS;
		}
		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement xe)
				{
					switch (xe.Name)
					{
						case "FBFBM":FBFBM = xe.InnerText;break;
						case "FBFMC": FBFMC = xe.InnerText; break;
						case "FBFFZRXM": FBFFZRXM = xe.InnerText; break;
						case "FZRZJLX": FZRZJLX = xe.InnerText; break;
						case "FZRZJHM": FZRZJHM = xe.InnerText; break;
						case "LXDH": LXDH = xe.InnerText; break;
						case "FBFDZ": FBFDZ = xe.InnerText; break;
						case "YZBM": YZBM = xe.InnerText; break;
						case "FBFDCY": FBFDCY = xe.InnerText; break;
						case "FBFDCRQ": if(DateTime.TryParse(xe.InnerText,out DateTime rq)) FBFDCRQ=rq; break;
						case "FBFDCJS": FBFDCJS = xe.InnerText; break;
						case "CZFS": CZFS = xe.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "FBFBM", FBFBM);
			AppendTextElementChild(xml, xn, "FBFMC", FBFMC);
			AppendTextElementChild(xml, xn, "FBFFZRXM", FBFFZRXM);
			AppendTextElementChild(xml, xn, "FZRZJLX", FZRZJLX);
			AppendTextElementChild(xml, xn, "FZRZJHM", FZRZJHM);
			AppendTextElementChild(xml, xn, "LXDH", LXDH);
			AppendTextElementChild(xml, xn, "FBFDZ", FBFDZ);
			AppendTextElementChild(xml, xn, "YZBM", YZBM);
			AppendTextElementChild(xml, xn, "FBFDCY", FBFDCY);
			AppendTextElementChild(xml, xn, "FBFDCRQ", FBFDCRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "FBFDCJS", FBFDCJS);
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
		}
	}
	class Cbf : XmlSerial
	{
		public string CBFBM;//>******************</CBFBM>
		public string CBFLX;//>*</CBFLX>
		public string CBFMC;//>xxx</CBFMC>
		public string CBFZJLX;//>*</CBFZJLX>
		public string CBFZJHM;//>******************</CBFZJHM>
		public string CBFDZ;//>xx省xxxxxxxxx村xxx组</CBFDZ>
		public string YZBM;//>******</YZBM>
		public string LXDH;// />
		public string CBFCYSL;//>*</CBFCYSL>
		public DateTime? CBFDCRQ;// />
		public string CBFDCY;// />
		public string CBFDCJS;//></CBFDCJS>
		public string GSJS;// />
		public string GSJSR;// />
		public DateTime? GSSHRQ;// />
		public string GSSHR;// />
		public string CZFS = "01";//>***</CZFS>

		internal string ID = Guid.NewGuid().ToString();
		public bool IsXmlDataEqual(Cbf rhs)
		{
			var fEqual = CBFBM == rhs.CBFBM && CBFLX == rhs.CBFLX&& CBFMC==rhs.CBFMC&& CBFZJLX==rhs.CBFZJLX
				&& CBFZJHM==rhs.CBFZJHM&& CBFDZ==rhs.CBFDZ&& YZBM==rhs.YZBM&& LXDH==rhs.LXDH
				&& CBFCYSL==rhs.CBFCYSL&& CBFDCY==rhs.CBFDCY&& CBFDCJS==rhs.CBFDCJS&& GSJS==rhs.GSJS
				&& GSJSR==rhs.GSJSR&& CBFDCRQ ==rhs.CBFDCRQ&& GSSHRQ==rhs.GSSHRQ;

			return fEqual;
		}
		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBFMC": CBFMC = e.InnerText; break;
						case "CBFZJLX": CBFZJLX = e.InnerText; break;
						case "CBFZJHM": CBFZJHM = e.InnerText; break;
						case "CBFDZ": CBFDZ = e.InnerText; break;
						case "YZBM": YZBM = e.InnerText; break;
						case "LXDH": LXDH = e.InnerText; break;
						case "CBFCYSL": CBFCYSL = e.InnerText; break;
						case "CBFDCRQ": if (DateTime.TryParse(e.InnerText, out DateTime dt)) CBFDCRQ = dt; break;
						case "CBFDCY": CBFDCY = e.InnerText; break;
						case "CBFDCJS": CBFDCJS = e.InnerText; break;
						case "GSJS": GSJS = e.InnerText; break;
						case "GSJSR": GSJSR = e.InnerText; break;
						case "GSSHRQ": if (DateTime.TryParse(e.InnerText, out dt)) GSSHRQ = dt; break;
						case "GSSHR": GSSHR = e.InnerText; break;
						case "CZFS": CZFS = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBFMC", CBFMC);
			AppendTextElementChild(xml, xn, "CBFZJLX", CBFZJLX);
			AppendTextElementChild(xml, xn, "CBFZJHM", CBFZJHM);
			AppendTextElementChild(xml, xn, "CBFDZ", CBFDZ);
			AppendTextElementChild(xml, xn, "YZBM", YZBM);
			AppendTextElementChild(xml, xn, "LXDH", LXDH);
			AppendTextElementChild(xml, xn, "CBFCYSL", CBFCYSL);
			AppendTextElementChild(xml, xn, "CBFDCRQ", CBFDCRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "CBFDCY", CBFDCY);
			AppendTextElementChild(xml, xn, "CBFDCJS", CBFDCJS);
			AppendTextElementChild(xml, xn, "GSJS", GSJS);
			AppendTextElementChild(xml, xn, "GSJSR", GSJSR);
			AppendTextElementChild(xml, xn, "GSSHRQ", GSSHRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "GSSHR", GSSHR);
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
		}
	}
	class Jtcy : XmlSerial
	{
		public string CBFBM;
		public string CYXM;
		public string CYXB;
		public string CYZJLX;
		public string CYZJHM;
		public string YHZGX;
		public string CYBZ;
		public string SFGYR;
		public string CYBZSM;
		public string CZFS = "01";
		internal string ID;

		public bool IsXmlEqual(Jtcy rhs)
		{
			return CBFBM == rhs.CBFBM && CYXM == rhs.CYXM && CYXB == rhs.CYXB && CYZJLX == rhs.CYZJLX && CYZJHM == rhs.CYZJHM
				&& YHZGX == rhs.YHZGX && CYBZ == rhs.CYBZ && SFGYR == rhs.SFGYR && CYBZSM == rhs.CYBZSM;
		}
		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CYXM": CYXM = e.InnerText; break;
						case "CYXB": CYXB = e.InnerText; break;
						case "CYZJLX": CYZJLX = e.InnerText; break;
						case "CYZJHM": CYZJHM = e.InnerText; break;
						case "YHZGX": YHZGX = e.InnerText; break;
						case "CYBZ": CYBZ = e.InnerText; break;
						case "SFGYR": SFGYR = e.InnerText; break;
						case "CYBZSM": CYBZSM = e.InnerText; break;
						case "CZFS": CZFS = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CYXM", CYXM);
			AppendTextElementChild(xml, xn, "CYXB", CYXB);
			AppendTextElementChild(xml, xn, "CYZJLX", CYZJLX);
			AppendTextElementChild(xml, xn, "CYZJHM", CYZJHM);
			AppendTextElementChild(xml, xn, "YHZGX", YHZGX);
			AppendTextElementChild(xml, xn, "CYBZ", CYBZ);
			AppendTextElementChild(xml, xn, "SFGYR", SFGYR);
			AppendTextElementChild(xml, xn, "CYBZSM", CYBZSM);
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
		}

		public string ToCompairString()
		{
			return $"{CBFBM}_{CYXM}_{CYXB}_{CYZJLX}_{CYZJHM}";
		}
	}
	class Cbht : XmlSerial
	{
		public string CBHTBM;
		public string YCBHTBM;
		public string FBFBM;
		public string CBFBM;
		public string CBFS;
		public DateTime? CBQXQ;//>yyyy-MM-dd HH:mm:ss</CBQXQ>
		public DateTime? CBQXZ;//>yyyy-MM-dd HH:mm:ss</CBQXZ>
		public double HTZMJ;//>*****.**</HTZMJ>
		public int CBDKZS;
		public DateTime? QDSJ;//>yyyy-MM-dd HH:mm:ss</QDSJ>
		public double HTZMJM;
		public double YHTZMJ;
		public double YHTZMJM;
		public string CZFS = "01";

		internal string ID = Guid.NewGuid().ToString();
		public bool IsXmlDataEqual(Cbht rhs)
		{
			return CBHTBM == rhs.CBHTBM && YCBHTBM == rhs.YCBHTBM && FBFBM == rhs.FBFBM && CBFS == rhs.CBFS && CBQXQ == rhs.CBQXQ
				&& CBQXZ == rhs.CBQXZ && HTZMJ.ToString() == rhs.HTZMJ.ToString() && CBDKZS == rhs.CBDKZS && QDSJ == rhs.QDSJ
				&& HTZMJM.ToString() == rhs.HTZMJM.ToString() && YHTZMJ.ToString() == rhs.YHTZMJ.ToString()
				&& YHTZMJM.ToString() == rhs.YHTZMJM.ToString();
		}
		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBHTBM": CBHTBM = e.InnerText; break;
						case "YCBHTBM": YCBHTBM = e.InnerText; break;
						case "FBFBM": FBFBM = e.InnerText; break;
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBFS": CBFS = e.InnerText; break;
						case "CBQXQ": if (DateTime.TryParse(e.InnerText, out DateTime dt)) CBQXQ = dt; break;
						case "CBQXZ": if (DateTime.TryParse(e.InnerText, out dt)) CBQXZ = dt; break;
						case "HTZMJ": if (double.TryParse(e.InnerText, out double d)) HTZMJ = d; break;
						case "CBDKZS": if (int.TryParse(e.InnerText, out int i)) CBDKZS = i; break;
						case "QDSJ": if (DateTime.TryParse(e.InnerText, out dt)) QDSJ = dt; break;
						case "HTZMJM": if (double.TryParse(e.InnerText, out d)) HTZMJM = d; break;
						case "YHTZMJ": if (double.TryParse(e.InnerText, out d)) YHTZMJ = d; break;
						case "YHTZMJM": if (double.TryParse(e.InnerText, out d)) YHTZMJM = d; break;
						case "CZFS": CZFS = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "YCBHTBM", YCBHTBM);
			AppendTextElementChild(xml, xn, "FBFBM", FBFBM);
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBFS", CBFS);
			AppendTextElementChild(xml, xn, "CBQXQ", CBQXQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "CBQXZ", CBQXZ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "HTZMJ", HTZMJ.ToString());
			AppendTextElementChild(xml, xn, "CBDKZS", CBDKZS.ToString());
			AppendTextElementChild(xml, xn, "QDSJ", QDSJ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "HTZMJM", HTZMJM.ToString());
			AppendTextElementChild(xml, xn, "YHTZMJ", YHTZMJ.ToString());
			AppendTextElementChild(xml, xn, "YHTZMJM", YHTZMJM.ToString());
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
		}
	}
	class Cbjyqzdjb : XmlSerial
	{
		internal string DJBID=Guid.NewGuid().ToString();

		public string CBJYQZBM;
		public string FBFBM;
		public string CBFBM;
		public string CBFS;
		public string CBQX;
		public DateTime? CBQXQ;//>yyyy-MM-dd HH:mm:ss</CBQXQ>
		public DateTime? CBQXZ;//>yyyy-MM-dd HH:mm:ss</CBQXZ>
		public string CBJYQZLSH;
		public string DJBFJ;
		public string YCBJYQZBH;
		public string DBR;
		public DateTime? DJSJ;//>yyyy-MM-dd HH:mm:ss</DJSJ>
		public string CBHTBM { get { return CBJYQZBM; } }
		public string CZFS = "01";

		public Fbf Fbf;
		public Cbf Cbf;
		public readonly List<Jtcy> jtcies = new List<Jtcy>();
		public Cbht Cbht;
		//public readonly List<Cbjyqz> Cbjyqz=new List<Cbjyqz>();
		public Cbjyqz Cbjyqz;// = new List<Cbjyqz>();
		public readonly List<Cbdk> Cbdk = new List<Cbdk>();
		public readonly List<Cbdkxx> Cbdkxx = new List<Cbdkxx>();
		public readonly List<Jzd> DkJzd = new List<Jzd>();
		public readonly List<Jzx> DkJzx = new List<Jzx>();

		/// <summary>
		/// 是否需要导出家庭成员
		/// </summary>
		public bool ExportJcties = true;

		public void AssignXmlData(Cbjyqzdjb rhs)
		{
			CBJYQZBM = rhs.CBJYQZBM;
			FBFBM = rhs.FBFBM;
			CBFBM = rhs.CBFBM;
			CBFS = rhs.CBFS;
			CBQX = rhs.CBQX;
			CBQXQ = rhs.CBQXQ;
			CBQXZ = rhs.CBQXZ;
			CBJYQZLSH = rhs.CBJYQZLSH;
			DJBFJ = rhs.DJBFJ;
			YCBJYQZBH = rhs.YCBJYQZBH;
			DBR = rhs.DBR;
			DJSJ = rhs.DJSJ;
		}
		public bool IsXmlDataEqual(Cbjyqzdjb rhs)
		{
			return CBJYQZBM == rhs.CBJYQZBM && FBFBM == rhs.FBFBM && CBFBM == rhs.CBFBM && CBFS == rhs.CBFS && CBQX == rhs.CBQX
					&& CBQXQ == rhs.CBQXQ && CBQXZ == rhs.CBQXZ && CBJYQZLSH == rhs.CBJYQZLSH && DJBFJ == rhs.DJBFJ
					&& YCBJYQZBH == rhs.YCBJYQZBH && DBR == rhs.DBR;
		}
		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "FBFBM": FBFBM = e.InnerText; break;
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBFS": CBFS = e.InnerText; break;
						case "CBQX": CBQX = e.InnerText; break;
						case "CBQXQ": if (DateTime.TryParse(e.InnerText, out DateTime dt)) CBQXQ = dt; break;
						case "CBQXZ": if (DateTime.TryParse(e.InnerText, out dt)) CBQXZ = dt; break;
						case "CBJYQZLSH": CBJYQZLSH = e.InnerText; break;
						case "DJBFJ": DJBFJ = e.InnerText; break;
						case "YCBJYQZBH": YCBJYQZBH = e.InnerText; break;
						case "DBR": DBR = e.InnerText; break;
						case "DJSJ": if (DateTime.TryParse(e.InnerText, out dt)) DJSJ = dt; break;
						case "CZFS": CZFS = e.InnerText; break;
					}
				}
			}
		}

		
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "FBFBM", FBFBM);
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBFS", CBFS);
			AppendTextElementChild(xml, xn, "CBQX", CBQX);
			AppendTextElementChild(xml, xn, "CBQXQ", CBQXQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "CBQXZ", CBQXZ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "CBJYQZLSH", CBJYQZLSH);
			AppendTextElementChild(xml, xn, "DJBFJ", DJBFJ);
			AppendTextElementChild(xml, xn, "YCBJYQZBH", YCBJYQZBH);
			AppendTextElementChild(xml, xn, "DBR", DBR);
			AppendTextElementChild(xml, xn, "DJSJ", DJSJ?.ToString("yyyy-MM-dd HH:mm:ss"));
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
		}
	}
	class Cbjyqz : XmlSerial
	{
		public string CBJYQZBM;
		public string FZJG;
		public DateTime? FZRQ;//>yyyy-MM-dd HH:mm:ss</FZRQ>
		public string QZSFLQ;
		public DateTime? QZLQRQ;//>yyyy-MM-dd HH:mm:ss</QZLQRQ>
		public string QZLQRXM;
		public string QZLQRZJLX;
		public string QZLQRZJHM;
		public string CZFS = "01";

		internal string ID = Guid.NewGuid().ToString();
		/// <summary>
		/// 是否已注销
		/// </summary>
		internal bool SFYZX;
		/// <summary>
		/// 业务号
		/// </summary>
		internal string YWH;

		public bool IsXmlDataEqual(Cbjyqz rhs)
		{
			return CBJYQZBM == rhs.CBJYQZBM && FZJG == rhs.FZJG && FZRQ == rhs.FZRQ && QZSFLQ == rhs.QZSFLQ && QZLQRQ == rhs.QZLQRQ
				&& QZLQRXM == rhs.QZLQRXM && QZLQRZJLX == rhs.QZLQRZJLX && QZLQRZJHM == rhs.QZLQRZJHM;
		}
		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "FZJG": FZJG = e.InnerText; break;
						case "FZRQ": if (DateTime.TryParse(e.InnerText, out DateTime dt)) FZRQ = dt; break;
						case "QZSFLQ": QZSFLQ = e.InnerText; break;
						case "QZLQRQ": if (DateTime.TryParse(e.InnerText, out dt)) QZLQRQ = dt; break;
						case "QZLQRXM": QZLQRXM = e.InnerText; break;
						case "QZLQRZJLX": QZLQRZJLX = e.InnerText; break;
						case "QZLQRZJHM": QZLQRZJHM = e.InnerText; break;
						case "CZFS": CZFS = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "FZJG", FZJG);
			AppendTextElementChild(xml, xn, "FZRQ", FZRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "QZSFLQ", QZSFLQ);
			AppendTextElementChild(xml, xn, "QZLQRQ", QZLQRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "QZLQRXM", QZLQRXM);
			AppendTextElementChild(xml, xn, "QZLQRZJLX", QZLQRZJLX);
			AppendTextElementChild(xml, xn, "QZLQRZJHM", QZLQRZJHM);
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
		}
	}
	class Dk : XmlSerial
	{
		public string BSM;
		public string YSDM;
		public string DKBM;
		public string DKMC;
		public string SYQXZ;
		public string DKLB;
		public string TDLYLX;
		public string DLDJ;
		public string TDYT;
		public string SFJBNT;
		public double SCMJ;
		public string DKDZ;
		public string DKXZ;
		public string DKNZ;
		public string DKBZ;
		public string DKBZXX;
		public string ZJRXM;
		public string KJZB;
		public double SCMJM;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "BSM": BSM = e.InnerText; break;
						case "YSDM": YSDM = e.InnerText; break;
						case "DKBM": DKBM = e.InnerText; break;
						case "DKMC": DKMC = e.InnerText; break;
						case "SYQXZ": SYQXZ = e.InnerText; break;
						case "DKLB": DKLB = e.InnerText; break;
						case "TDLYLX": TDLYLX = e.InnerText; break;
						case "DLDJ": DLDJ = e.InnerText; break;
						case "TDYT": TDYT = e.InnerText; break;
						case "SFJBNT": SFJBNT = e.InnerText; break;
						case "SCMJ": if (double.TryParse(e.InnerText, out double d)) SCMJ = d; break;
						case "DKDZ": DKDZ = e.InnerText; break;
						case "DKXZ": DKXZ = e.InnerText; break;
						case "DKNZ": DKNZ = e.InnerText; break;
						case "DKBZ": DKBZ = e.InnerText; break;
						case "DKBZXX": DKBZXX = e.InnerText; break;
						case "ZJRXM": ZJRXM = e.InnerText; break;
						case "KJZB": KJZB = e.InnerText; break;
						case "SCMJM": if (double.TryParse(e.InnerText, out d)) SCMJM = d; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "BSM", BSM);
			AppendTextElementChild(xml, xn, "YSDM", YSDM);
			AppendTextElementChild(xml, xn, "DKBM", DKBM);
			AppendTextElementChild(xml, xn, "DKMC", DKMC);
			AppendTextElementChild(xml, xn, "SYQXZ", SYQXZ);
			AppendTextElementChild(xml, xn, "DKLB", DKLB);
			AppendTextElementChild(xml, xn, "TDLYLX", TDLYLX);
			AppendTextElementChild(xml, xn, "DLDJ", DLDJ);
			AppendTextElementChild(xml, xn, "TDYT", TDYT);
			AppendTextElementChild(xml, xn, "SFJBNT", SFJBNT);
			AppendTextElementChild(xml, xn, "SCMJ", SCMJ.ToString());
			AppendTextElementChild(xml, xn, "DKDZ", DKDZ);
			AppendTextElementChild(xml, xn, "DKXZ", DKXZ);
			AppendTextElementChild(xml, xn, "DKNZ", DKNZ);
			AppendTextElementChild(xml, xn, "DKBZ", DKBZ);
			AppendTextElementChild(xml, xn, "DKBZXX", DKBZXX);
			AppendTextElementChild(xml, xn, "ZJRXM", ZJRXM);
			AppendTextElementChild(xml, xn, "KJZB", KJZB);
			AppendTextElementChild(xml, xn, "SCMJM", SCMJM.ToString());
		}
	}
	class Cbdk : XmlSerial
	{
		/// <summary>
		/// 操作方式
		/// </summary>
		public string CZFS = "01";
		public string YCBDKBM;
		public string YCBFBM;
		//public string DKBM;
		public Dk Dk;
		public string SHP;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "DK":
							{
								Dk = new Dk();
								Dk.ReadXml(e);
							}break;
						case "CZFS": CZFS = e.InnerText; break;
						case "YCBDKBM": YCBDKBM = e.InnerText; break;
						case "YCBFBM": YCBFBM = e.InnerText; break;
						case "SHP": SHP = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			var en = xml.CreateElement("DK");
			xn.AppendChild(en);
			Dk?.WriteXml(xml, en);
			AppendTextElementChild(xml, xn, "CZFS", CZFS);
			AppendTextElementChild(xml, xn, "YCBDKBM", YCBDKBM);
			AppendTextElementChild(xml, xn, "YCBFBM", YCBFBM);
			AppendTextElementChild(xml, xn, "SHP", SHP);
		}
	}
	class Cbdkxx : XmlSerial
	{
		public string DKBM;
		public string FBFBM;
		public string CBFBM;
		public string CBJYQQDFS;
		public double HTMJ;//>**.**</HTMJ>
		public string CBHTBM;
		public string LZHTBM;
		public string CBJYQZBM;
		public double YHTMJ;
		public double HTMJM;
		public double YHTMJM;
		/// <summary>
		/// 1:是;2:否
		/// </summary>
		public string SFQQQG;

		internal string ID = Guid.NewGuid().ToString();
		public override void ReadXml(XmlNode xn)
		{
			foreach(var n in xn.ChildNodes) {
				if (n is XmlElement e)
				{
					switch (e.Name)
					{
						case "DKBM": DKBM = e.InnerText; break;
						case "FBFBM": FBFBM = e.InnerText; break;
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBJYQQDFS": CBJYQQDFS = e.InnerText; break;
						case "HTMJ": if (double.TryParse(e.InnerText, out double d)) HTMJ = d; break;
						case "CBHTBM": CBHTBM = e.InnerText; break;
						case "LZHTBM": LZHTBM = e.InnerText; break;
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "YHTMJ": if (double.TryParse(e.InnerText, out d)) YHTMJ = d; break;
						case "HTMJM": if (double.TryParse(e.InnerText, out d)) HTMJM = d; break;
						case "YHTMJM": if (double.TryParse(e.InnerText, out d)) YHTMJM = d; break;
						case "SFQQQG": SFQQQG = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "DKBM", DKBM);
			AppendTextElementChild(xml, xn, "FBFBM", FBFBM);
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBJYQQDFS", CBJYQQDFS);
			AppendTextElementChild(xml, xn, "HTMJ", HTMJ.ToString());
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "LZHTBM", LZHTBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "YHTMJ", YHTMJ.ToString());
			AppendTextElementChild(xml, xn, "HTMJM", HTMJM.ToString());
			AppendTextElementChild(xml, xn, "YHTMJM", YHTMJM.ToString());
			AppendTextElementChild(xml, xn, "SFQQQG", string.IsNullOrEmpty(SFQQQG)?"2":SFQQQG);
		}
	}
	class Jzd : XmlSerial
	{
		public string BSM;
		public string YSDM;
		public string JZDH;
		public string JZDLX;
		public string JBLX;
		public string DKBM;
		public string XZBZ;//>******.**</XZBZ>
		public string YZBZ;//>*******.**</YZBZ>
		public string SHP;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "BSM": BSM = e.InnerText; break;
						case "YSDM": YSDM = e.InnerText; break;
						case "JZDH": JZDH = e.InnerText; break;
						case "JZDLX": JZDLX = e.InnerText; break;
						case "JBLX": JBLX = e.InnerText; break;
						case "DKBM": DKBM = e.InnerText; break;
						case "XZBZ": XZBZ = e.InnerText; break;
						case "YZBZ": YZBZ = e.InnerText; break;
						case "SHP": SHP = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "BSM", BSM);
			AppendTextElementChild(xml, xn, "YSDM", YSDM);
			AppendTextElementChild(xml, xn, "JZDH", JZDH);
			AppendTextElementChild(xml, xn, "JZDLX", JZDLX);
			AppendTextElementChild(xml, xn, "JBLX", JBLX);
			AppendTextElementChild(xml, xn, "DKBM", DKBM);
			AppendTextElementChild(xml, xn, "XZBZ", XZBZ);
			AppendTextElementChild(xml, xn, "YZBZ", YZBZ);
			AppendTextElementChild(xml, xn, "SHP", SHP);
		}
	}
	//class DkJzd
	//{
	//	public readonly List<Jzd> lstJzd=new List<Jzd>();
	//	//public string SHP;//wkt
	//}
	class Jzx : XmlSerial
	{
		public string BSM;
		public string YSDM;
		public string JXXZ;
		public string JZXLB;
		public string JZXWZ;
		public string JZXSM;
		public string PLDWQLR;
		public string PLDWZJR;
		public string JZXH;
		public string QJZDH;
		public string ZJZDH;
		public string DKBM;
		public string SHP;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "BSM": BSM = e.InnerText; break;
						case "YSDM": YSDM = e.InnerText; break;
						case "JXXZ": JXXZ = e.InnerText; break;
						case "JZXLB": JZXLB = e.InnerText; break;
						case "JZXWZ": JZXWZ = e.InnerText; break;
						case "JZXSM": JZXSM = e.InnerText; break;
						case "PLDWQLR": PLDWQLR = e.InnerText; break;
						case "PLDWZJR": PLDWZJR = e.InnerText; break;
						case "JZXH": JZXH = e.InnerText; break;
						case "QJZDH": QJZDH = e.InnerText; break;
						case "ZJZDH": ZJZDH = e.InnerText; break;
						case "DKBM": DKBM = e.InnerText; break;
						case "SHP": SHP = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "BSM", BSM);
			AppendTextElementChild(xml, xn, "YSDM", YSDM);
			AppendTextElementChild(xml, xn, "JXXZ", JXXZ);
			AppendTextElementChild(xml, xn, "JZXLB", JZXLB);
			AppendTextElementChild(xml, xn, "JZXWZ", JZXWZ);
			AppendTextElementChild(xml, xn, "JZXSM", JZXSM);
			AppendTextElementChild(xml, xn, "PLDWQLR", PLDWQLR);
			AppendTextElementChild(xml, xn, "PLDWZJR", PLDWZJR);
			AppendTextElementChild(xml, xn, "JZXH", JZXH);
			AppendTextElementChild(xml, xn, "QJZDH", QJZDH);
			AppendTextElementChild(xml, xn, "ZJZDH", ZJZDH);
			AppendTextElementChild(xml, xn, "DKBM", DKBM);
			AppendTextElementChild(xml, xn, "SHP", SHP);

		}
	}
	//class DkJzx
	//{
	//	public readonly List<Jzx> lstJzx = new List<Jzx>();
	//}
}
