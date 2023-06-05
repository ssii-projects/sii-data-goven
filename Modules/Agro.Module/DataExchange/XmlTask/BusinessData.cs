using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Agro.Module.DataExchange.XmlTask
{

	abstract class BusinessData
	{
		#region XmlData
		public string XZQDM;
		public string XZDYMC;
		/// <summary>
		/// 前业务流水号
		/// </summary>
		public string QYWLSH;
		public string YWLSH;
		public DateTime? YWBLSJ;
		public string FBFBM;
		#endregion

		protected Task _task { get { return _p._task; } }
		protected IFeatureWorkspace _db { get { return _p._db; } }
		protected XmlData _p;
		public BusinessData()
		{
		}
		internal void FinalConstruct(XmlData p)
		{
			_p = p;
		}

		public void Query(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			//YWLSH = it.Ywh;
			if (XZQDM == null)
			{
				XZQDM = it.SZDY.Substring(0, 6);
				var zone = ZoneUtil.QueryZone(u=>u.BM==XZQDM);// $"BM='{XZQDM}'");
				if (zone != null)
				{
					XZDYMC = zone.Name;
				}
			}
			YWBLSJ =it.Blsj;

			DoQuery(it, lstDjbID, od, cd);
		}

		/// <summary>
		/// 根据任务参数it从数据库中查询导出数据
		/// </summary>
		/// <param name="db"></param>
		/// <param name="it"></param>
		protected abstract void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd);

		public virtual bool HasOriginData()
		{
			return true;
		}
		public virtual bool HasChangeData()
		{
			return true;
		}
		/// <summary>
		/// 检查cd中的数据是否需要导出
		/// </summary>
		/// <param name="od"></param>
		/// <param name="cd"></param>
		public virtual void CheckExport(OriginalData od,OriginalData cd)
		{
			if (od.lstDjb.Count == 0)
				return;
			foreach (var d1 in cd.lstDjb)
			{
				var d0 = od.lstDjb.Find(a => { return a.CBJYQZBM == d1.CBJYQZBM; });
				if (d0 != null)
				{
					CheckExport(d0, d1);
				}
			}
		}
		public void WriteXml(XmlDocument xml,XmlNode xn, int srid)
		{
			AppendTextElementChild(xml, xn, "XZQDM", XZQDM);
			AppendTextElementChild(xml, xn, "XZDYMC", XZDYMC);
			AppendTextElementChild(xml, xn, "QYWLSH", QYWLSH);
			AppendTextElementChild(xml, xn, "YWLSH", YWLSH);
			AppendTextElementChild(xml, xn, "YWBLSJ", YWBLSJ?.ToString("yyyy-MM-dd HH:mm:ss"));
			AppendTextElementChild(xml, xn, "FBFBM", FBFBM);
			WriteXmlExt(xml, xn);
		}
		public void ReadXml(XmlDocument xml)
		{
			var xn = xml.SelectSingleNode("/SUBMIT/BUSINESS_DATA");
			foreach (var n in xn.ChildNodes)
			{
				if (n is XmlElement xe)
				{
					switch (xe.Name)
					{
						case "XZQDM": XZQDM = xe.InnerText; break;
						case "XZDYMC": XZDYMC = xe.InnerText; break;
						case "QYWLSH": QYWLSH = xe.InnerText; break;
						case "YWLSH": YWLSH = xe.InnerText; break;
						case "YWBLSJ": if(DateTime.TryParse(xe.InnerText,out DateTime dt)) YWBLSJ=dt; break;
						case "FBFBM": FBFBM = xe.InnerText; break;
						default:
							ReadXmlExt(xe);
							break;
					}
				}
			}
		}
		public virtual void ReadChangeData(XmlDocument xml,XmlNode parent)
		{
		}
		public virtual void ModifyCzfs(OriginalData changeData)
		{
		}
		public virtual void FillEntities(DJ_YW_SLSQ en)
		{
		}
		public void Import(DJ_YW_SLSQ enSLSQ)
		{
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			#region 写入DJ_YW_SLSQ
			var fields = "YWH,ZHXGSJ,QLLX,DJLX,DJXL,ID,DJYY,SZDY,AZJT,SLRY,SLSJ,CJSJ";
			var sql = $"insert into DJ_YW_SLSQ({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";

				object o = null;
				switch (field)
				{
					case "YWH": o = YWLSH; break;
					case "ZHXGSJ": o = YWBLSJ; break;
					case "QLLX": o = enSLSQ.QLLX; break;
					case "DJLX": o = enSLSQ.DJLX; break;
					case "DJXL": o =enSLSQ.DJXL; break;
					case "ID": o = enSLSQ.ID; break;
					case "DJYY": o = enSLSQ.DJYY; break;
					case "SZDY": o = XZQDM; break;
					case "AZJT": o = enSLSQ.AJZT; break;
					case "SLRY": o = enSLSQ.SLRY; break;
					case "SLSJ": o = enSLSQ.SLSJ; break;
					case "CJSJ": o = enSLSQ.CJSJ; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
			#endregion
			DoImport(enSLSQ);
		}

		protected virtual void DoImport(DJ_YW_SLSQ enSLSQ)
		{
		}


		/// <summary>
		/// 写入DJ_YW_SQQL
		/// </summary>
		/// <param name="SQID">DJ_YW_SLSQ.ID</param>
		/// <param name="QLID">DJ_CBJYQ_DJB.ID</param>
		protected void WriteDJ_YW_SQQL(string SQID,string QLID)
		{
			var sql = $"insert into DJ_YW_SQQL(ID,SQID,QLID) values('{Guid.NewGuid().ToString()}','{SQID}','{QLID}')";
			_db.ExecuteNonQuery(sql);
		}

		/// <summary>
		/// 默认根据ChangeData.lstDjb数据写入DJ_YW_SQQL
		/// </summary>
		/// <param name="enSLSQ"></param>
		protected void DefWriteDJ_YW_SQQL(DJ_YW_SLSQ enSLSQ)
		{
			foreach (var djb in _p.ChangeData.lstDjb)
			{
				WriteDJ_YW_SQQL(enSLSQ.ID, djb.DJBID);
			}
		}

		protected DJ_CBJYQ_DJB WriteDJ_CBJYQ_DJB(Cbjyqzdjb djb)
		{
			var en = new DJ_CBJYQ_DJB(djb);
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "ID,YWH,QLLX,DJLX,DJXL,DJYY,CBJYQZBM,FBFBM,CBFBM,CBFMC,CBFS,CBQX,CBQXQ,CBQXZ,CBJYQZLSH,YCBJYQZBH,DKSYT,DBR,DJSJ,QSZT,DYZT,YYZT,SFYZX,QXDM,SZDY,ZHXGSJ,FJ,LSBB";
			var sql = $"insert into DJ_CBJYQ_DJB({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
				object o = null;
				switch (field)
				{
					case "ID": o = djb.DJBID; break;
					case "YWH": o = YWLSH; break;
					case "QLLX": o = 0; break;
					case "DJLX": o = XmlExchangeUtil.DJXLToDJLX(djxl); break;
					case "DJXL": o = djxl; break;
					case "DJYY": o = "首次登记"; break;
					case "CBJYQZBM": o = djb.CBJYQZBM; break;
					case "FBFBM": o = djb.FBFBM; break;
					case "CBFBM": o = djb.CBFBM; break;
					case "CBFMC": o = djb.Cbf?.CBFMC; break;
					case "CBFS": o = djb.Cbht?.CBFS; break;
					case "CBQX": o = djb.CBQX; break;
					case "CBQXQ": o = djb.CBQXQ; break;
					case "CBQXZ": o = djb.CBQXZ; break;
					case "CBJYQZLSH": o = djb.CBJYQZLSH; break;
					case "YCBJYQZBH": o = djb.YCBJYQZBH; break;
					case "DKSYT": o =null; break;
					case "DBR": o = djb.DBR; break;
					case "DJSJ": o = djb.DJSJ; break;
					case "QSZT": o =(int)EQszt.Xians; break;
					case "DYZT": o =0; break;
					case "YYZT": o =0; break;
					case "SFYZX": o =0; break;
					case "QXDM": o =djb.FBFBM; break;
					case "SZDY": o =XmlExchangeUtil.Fbfbm2SZDY(djb.FBFBM); break;
					case "ZHXGSJ": o =DateTime.Now; break;
					case "fj": o =null; break;
					case "lsbb": o = null; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
			return en;
		}
		protected void WriteDJ_CBJYQ_QZ(Cbjyqzdjb djb)
		{
			var qz = djb.Cbjyqz;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "ID,DJBID,CBJYQZBM,ZSBS,SQSJC,BZNF,FZJGSZDMC,NDSXH,YZSXH,CBJYQZLSH,FZJG,FZRQ,DYCS,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM,SFYZX,ZXYY,ZXRQ,ZXSJ";
			var sql = $"insert into DJ_CBJYQ_QZ({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
				object o = null;
				switch (field)
				{
					case "ID": o = qz.ID; break;
					case "DJBID": o = djb.DJBID; break;
					case "CBJYQZBM": o = qz.CBJYQZBM; break;
					case "ZSBS": o = 0; break;
					case "SQSJC": o = null; break;
					case "BZNF": o = djb.DJSJ?.Year; break;
					case "FZJGSZDMC": o = qz.FZJG; break;
					case "NDSXH": o =null; break;
					case "YZSXH": o =null; break;
					case "CBJYQZLSH": o =null; break;
					case "FZJG": o = qz.FZJG; break;
					case "FZRQ": o = qz.FZRQ; break;
					case "DYCS": o = 0; break;
					case "QZSFLQ": o = 2; break;
					case "QZLQRQ": o =null; break;
					case "QZLQRXM": o = qz.QZLQRXM; break;
					case "QZLQRZJLX": o = qz.QZLQRZJLX; break;
					case "QZLQRZJHM": o = qz.QZLQRZJHM; break;
					case "SFYZX": o =0; break;
					case "ZXYY": o =null; break;
					case "ZXRQ": o =null; break;
					case "ZXSJ": o = null; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o ?? DBNull.Value 
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
		}
		protected void UpdateQSSJ_CBJYQZ(Cbjyqzdjb djb)
		{
			var qz = djb.Cbjyqz;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "IFZJG,FZRQ,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM";
			var saFields = fields.Split(',');
			if (SafeConvertAux.ToInt32(_db.QueryOne($"select count(1) from QSSJ_CBJYQZ where CBJYQZBM='{djb.CBJYQZBM}'")) == 0)
			{//
				var sql = $"insert into QSSJ_CBJYQZ({fields}) values(";
				foreach (var field in saFields)
				{
					sql += $"{a}{field},";
					object o = null;
					switch (field)
					{
						case "FZJG": o = qz.FZJG; break;
						case "FZRQ": o = qz.FZRQ; break;
						case "QZSFLQ": o = qz.QZSFLQ; break;
						case "QZLQRQ": o = qz.QZLQRQ; break;
						case "QZLQRXM": o = qz.QZLQRXM; break;
						case "QZLQRZJLX": o = qz.QZLQRZJLX; break;
						case "QZLQRZJHM": o = qz.QZLQRZJHM; break;
					}
					var prm = new SQLParam()
					{
						ParamName = field,
						ParamValue = o ?? DBNull.Value
					};
					sqlPrms.Add(prm);
				}
				sql = sql.TrimEnd(',') + ")";
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
			else
			{
				var sql = $"update QSSJ_CBJYQZ({fields}) set(";
				foreach (var field in saFields)
				{
					sql += $"{field}={a}{field},";
					object o = null;
					switch (field)
					{
						case "FZJG": o = qz.FZJG; break;
						case "FZRQ": o = qz.FZRQ; break;
						case "QZSFLQ": o = qz.QZSFLQ; break;
						case "QZLQRQ": o = qz.QZLQRQ; break;
						case "QZLQRXM": o = qz.QZLQRXM; break;
						case "QZLQRZJLX": o = qz.QZLQRZJLX; break;
						case "QZLQRZJHM": o = qz.QZLQRZJHM; break;
					}
					var prm = new SQLParam()
					{
						ParamName = field,
						ParamValue = o ?? DBNull.Value
					};
					sqlPrms.Add(prm);
				}
				sql = sql.TrimEnd(',') + $") where CBJYQZBM='{djb.CBJYQZBM}'";
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}
		protected void WriteDJ_CBJYQ_CBHT(Cbjyqzdjb djb)
		{
			var ht = djb.Cbht;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "ID,DJBID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM";
			var sql = $"insert into DJ_CBJYQ_CBHT({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
				object o = null;
				switch (field)
				{
					case "ID": o = ht.ID; break;
					case "DJBID": o =djb.DJBID; break;
					case "CBHTBM": o =djb.CBHTBM ; break;
					case "YCBHTBM": o = ht.YCBHTBM; break;
					case "FBFBM": o = djb.FBFBM; break;
					case "CBFBM": o = djb.CBFBM; break;
					case "CBFS": o =ht.CBFS; break;
					case "CBQXQ": o = ht.CBQXQ; break;
					case "CBQXZ": o = ht.CBQXZ; break;
					case "HTZMJ": o = ht.HTZMJ; break;
					case "CBDKZS": o =ht.CBDKZS; break;
					case "QDSJ": o =ht.QDSJ; break;
					case "HTZMJM": o = ht.HTZMJM; break;
					case "YHTZMJ": o = ht.YHTZMJ; break;
					case "YHTZMJM": o = ht.YHTZMJM; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o ?? DBNull.Value
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
		}
		protected void UpdateQSSJ_CBHT(Cbjyqzdjb djb)
		{
			var now = DateTime.Now;
			var qz = djb.Cbht;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM,ZT,DJZT,CJSJ,ZHXGSJ";
			var saFields = fields.Split(',');
			if (SafeConvertAux.ToInt32(_db.QueryOne($"")) == 0)
			{
				var sql = $"insert into QSSJ_CBHT({fields}) values(";
				foreach (var field in saFields)
				{
					sql += $"{a}{field},";
					var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
					object o = null;
					switch (field)
					{
						case "YCBHTBM": o = qz.YCBHTBM; break;
						case "FBFBM": o = qz.FBFBM; break;
						case "CBFBM": o = qz.CBFBM; break;
						case "CBFS": o = qz.CBFS; break;
						case "CBQXQ": o = qz.CBQXQ; break;
						case "CBQXZ": o = qz.CBQXZ; break;
						case "HTZMJ": o = qz.HTZMJ; break;
						case "CBDKZS": o = qz.CBDKZS; break;
						case "QDSJ": o = qz.QDSJ; break;
						case "HTZMJM": o = qz.HTZMJM; break;
						case "YHTZMJ": o = qz.YHTZMJ; break;
						case "YHTZMJM": o = qz.YHTZMJM; break;
						case "ZT": o = 1; break;
						case "DJZT": o = 2; break;
						case "CJSJ": o = now; break;
						case "ZHXGSJ": o = now; break;
					}
					var prm = new SQLParam()
					{
						ParamName = field,
						ParamValue = o ?? DBNull.Value
					};
					sqlPrms.Add(prm);
				}
				sql = sql.TrimEnd(',') + ")";
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
			else
			{
				var sql = $"update QSSJ_CBHT({fields}) set(";
				foreach (var field in saFields)
				{
					sql += $"{field}={a}{field},";
					var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
					object o = null;
					switch (field)
					{
						case "YCBHTBM": o = qz.YCBHTBM; break;
						case "FBFBM": o = qz.FBFBM; break;
						case "CBFBM": o = qz.CBFBM; break;
						case "CBFS": o = qz.CBFS; break;
						case "CBQXQ": o = qz.CBQXQ; break;
						case "CBQXZ": o = qz.CBQXZ; break;
						case "HTZMJ": o = qz.HTZMJ; break;
						case "CBDKZS": o = qz.CBDKZS; break;
						case "QDSJ": o = qz.QDSJ; break;
						case "HTZMJM": o = qz.HTZMJM; break;
						case "YHTZMJ": o = qz.YHTZMJ; break;
						case "YHTZMJM": o = qz.YHTZMJM; break;
						case "ZT": o = 1; break;
						case "DJZT": o = 2; break;
						case "CJSJ": o = now; break;
						case "ZHXGSJ": o = now; break;
					}
					var prm = new SQLParam()
					{
						ParamName = field,
						ParamValue = o ?? DBNull.Value
					};
					sqlPrms.Add(prm);
				}
				sql = sql.TrimEnd(',') + $") where CBHTBM='{djb.CBHTBM}'";
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}
		protected void WriteDJ_CBJYQ_CBF(Cbjyqzdjb djb)
		{
			var ht = djb.Cbf;
			if (ht == null) return;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "ID,DJBID,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR";
			var sql = $"insert into DJ_CBJYQ_CBF({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
				object o = null;
				switch (field)
				{
					case "ID": o = ht.ID; break;
					case "DJBID": o = djb.DJBID; break;
					case "CBFBM": o = djb.CBFBM; break;
					case "CBFLX": o = ht.CBFLX; break;
					case "CBFMC": o = ht.CBFMC; break;
					case "CBFZJLX": o = ht.CBFZJLX; break;
					case "CBFZJHM": o = ht.CBFZJHM; break;
					case "CBFDZ": o = ht.CBFDZ; break;
					case "YZBM": o = ht.YZBM; break;
					case "LXDH": o = ht.LXDH; break;
					case "CBFCYSL": o = ht.CBFCYSL; break;
					case "CBFDCRQ": o = ht.CBFDCRQ; break;
					case "CBFDCY": o = ht.CBFDCY; break;
					case "CBFDCJS": o = ht.CBFDCJS; break;
					case "GSJS": o = ht.GSJS; break;
					case "GSJSR": o = ht.GSJSR; break;
					case "GSSHRQ": o = ht.GSSHRQ; break;
					case "GSSHR": o = ht.GSSHR; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o ?? DBNull.Value
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
		}
		protected void UpdateQSSJ_CBF(Cbjyqzdjb djb)
		{
			var now = DateTime.Now;
			var qz = djb.Cbf;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var fields = "FBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR,ZT,DJZT,CJSJ,ZHXGSJ";
			var saFields = fields.Split(',');
			if (SafeConvertAux.ToInt32(_db.QueryOne($"select count(1) from QSSJ_CBF where CBFBM='{djb.CBFBM}'")) == 0)
			{
				var sql = $"insert into QSSJ_CBF({fields}) values(";
				foreach (var field in saFields)
				{
					sql += $"{a}{field},";
					var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
					object o = null;
					switch (field)
					{
						case "FBFBM": o = djb.FBFBM; break;
						case "CBFLX": o = qz.CBFLX; break;
						case "CBFMC": o = qz.CBFMC; break;
						case "CBFZJLX": o = qz.CBFZJLX; break;
						case "CBFZJHM": o = qz.CBFZJHM; break;
						case "CBFDZ": o = qz.CBFDZ; break;
						case "YZBM": o = qz.YZBM; break;
						case "LXDH": o = qz.LXDH; break;
						case "CBFCYSL": o = qz.CBFCYSL; break;
						case "CBFDCRQ": o = qz.CBFDCRQ; break;
						case "CBFDCY": o = qz.CBFDCY; break;
						case "CBFDCJS": o = qz.CBFDCJS; break;
						case "GSJS": o = qz.GSJS; break;
						case "GSJSR": o = qz.GSJSR; break;
						case "GSSHRQ": o = qz.GSSHRQ; break;
						case "GSSHR": o = qz.GSSHR; break;
						case "ZT": o = 1; break;
						case "DJZT": o = 2; break;
						case "CJSJ": o = now; break;
						case "ZHXGSJ": o = now; break;
					}
					var prm = new SQLParam()
					{
						ParamName = field,
						ParamValue = o ?? DBNull.Value
					};
					sqlPrms.Add(prm);
				}
				sql = sql.TrimEnd(',') + ")";
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
			else
			{
				var sql = $"update QSSJ_CBF({fields}) set(";
				foreach (var field in saFields)
				{
					sql += $"{field}={a}{field},";
					var djxl = XmlExchangeUtil.SJLXToDjxl(_p.Head.SJLX);
					object o = null;
					switch (field)
					{
						case "FBFBM": o = djb.FBFBM; break;
						case "CBFLX": o = qz.CBFLX; break;
						case "CBFMC": o = qz.CBFMC; break;
						case "CBFZJLX": o = qz.CBFZJLX; break;
						case "CBFZJHM": o = qz.CBFZJHM; break;
						case "CBFDZ": o = qz.CBFDZ; break;
						case "YZBM": o = qz.YZBM; break;
						case "LXDH": o = qz.LXDH; break;
						case "CBFCYSL": o = qz.CBFCYSL; break;
						case "CBFDCRQ": o = qz.CBFDCRQ; break;
						case "CBFDCY": o = qz.CBFDCY; break;
						case "CBFDCJS": o = qz.CBFDCJS; break;
						case "GSJS": o = qz.GSJS; break;
						case "GSJSR": o = qz.GSJSR; break;
						case "GSSHRQ": o = qz.GSSHRQ; break;
						case "GSSHR": o = qz.GSSHR; break;
						case "ZT": o = 1; break;
						case "DJZT": o = 2; break;
						case "CJSJ": o = now; break;
						case "ZHXGSJ": o = now; break;
					}
					var prm = new SQLParam()
					{
						ParamName = field,
						ParamValue = o ?? DBNull.Value
					};
					sqlPrms.Add(prm);
				}
				sql = sql.TrimEnd(',') + $") where CBFBM='{djb.CBFBM}'";
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}

		protected void WriteDJ_CBJYQ_CBF_JTCY(Cbjyqzdjb djb)
		{
			var cbf = djb.Cbf;
			if (cbf == null) return;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var dicPrms = new Dictionary<string, SQLParam>();
			var fields = "ID,CBFBM,CBFID,CYXM,CYXB,CYZJLX,CYZJHM,CSRQ,YHZGX,CYBZ,SFGYR,CYBZSM,DJBID";
			var sql = $"insert into DJ_CBJYQ_CBF({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
					//ParamValue = o ?? DBNull.Value
				};
				dicPrms[field] = prm;
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";

			foreach (var j in djb.jtcies)
			{
				foreach (var kv in dicPrms)
				{
					object o = null;
					switch (kv.Key)
					{
						case "ID": o = j.ID; break;
						case "CBFBM":o = j.CBFBM; break;
						case "CBFID":o = cbf.ID;break;
						case "CYXM":o = j.CYXM;break;
						case "CYXB":o = j.CYXB;break;
						case "CYZJLX":o = j.CYZJLX;break;
						case "CYZJHM":o = j.CYZJHM;break;
						case "CSRQ":
							System.Diagnostics.Debug.Assert(false, "?");
							break;
						case "YHZGX":o = j.YHZGX;break;
						case "CYBZ":o = j.CYBZ;break;
						case "SFGYR":o = j.SFGYR;break;
						case "CYBZSM":o = j.CYBZSM;break;
						case "DJBID":o = djb.DJBID;break;
					}
					kv.Value.ParamValue = o;
				}
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}

		protected void UpdateQSSJ_CBF_JTCY(Cbjyqzdjb djb)
		{
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var dicPrms = new Dictionary<string, SQLParam>();
			var fields = "CBFBM,CYXM,CYXB,CYZJLX,CYZJHM,CSRQ,YHZGX,CYBZ,SFGYR,CYBZSM";
			var saFields = fields.Split(',');
			var sUpdate = $"update QSSJ_CBF_JTCY({fields}) set(";
			foreach (var field in saFields)
			{
				sUpdate += $"{field}={a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
				};
				dicPrms[field] = prm;
				sqlPrms.Add(prm);
			}
			sUpdate = sUpdate.TrimEnd(',');

			var lstID = new List<string>();
			_db.QueryCallback($"select ID from QSSJ_CBF_JTCY where CBFBM='{djb.CBFBM}'",r=>
			{
				lstID.Add(r.GetString(0));
				return true;
			});
			int cnt = Math.Min(lstID.Count, djb.jtcies.Count);
			for(int i=0;i<cnt;++i)
			{
				string id = lstID[i];
				var j = djb.jtcies[i];
				foreach (var field in saFields)
				{
					object o = null;
					switch (field)
					{
						case "CBFBM": o = djb.CBFBM; break;
						case "CYXM": o = j.CYXM; break;
						case "CYXB": o = j.CYXB; break;
						case "CYZJLX": o = j.CYZJLX; break;
						case "CYZJHM": o = j.CYZJHM; break;
						case "CSRQ":
							//o = j.CSRQ;
							System.Diagnostics.Debug.Assert(false, "?");
							break;
						case "YHZGX": o = j.YHZGX; break;
						case "CYBZ": o = j.CYBZ; break;
						case "SFGYR": o = j.SFGYR; break;
						case "CYBZSM": o = j.CYBZSM; break;
					}
					dicPrms[field].ParamValue = o;
				}
				var sql=sUpdate  + $") where ID='{id}'";
				_db.ExecuteNonQuery(sUpdate, sqlPrms);
			}
			if (cnt < lstID.Count)
			{
				string sin = null;
				for (int i = cnt; i < lstID.Count; ++i)
				{
					if (sin == null) sin = $"'{lstID[i]}'";
					else sin += $",'{lstID[i]}'";
				}
				_db.ExecuteNonQuery($"delete from QSSJ_CBF_JTCY where ID in({sin})");
			}
			else if (cnt < djb.jtcies.Count)
			{
				var sql = $"insert into QSSJ_CBF_JTCY({fields}) values(";
				foreach (var field in saFields)
				{
					sql += $"{a}{field},";
				}
				sql = sql.TrimEnd(',') + ")";
				for (int i = cnt; i < djb.jtcies.Count; ++i)
				{
					var j = djb.jtcies[i];
					foreach (var field in saFields)
					{
						object o = null;
						switch (field)
						{
							case "CBFBM": o = djb.CBFBM; break;
							case "CYXM": o = j.CYXM; break;
							case "CYXB": o = j.CYXB; break;
							case "CYZJLX": o = j.CYZJLX; break;
							case "CYZJHM": o = j.CYZJHM; break;
							case "CSRQ":
								//o = j.CSRQ;
								System.Diagnostics.Debug.Assert(false, "?");
								break;
							case "YHZGX": o = j.YHZGX; break;
							case "CYBZ": o = j.CYBZ; break;
							case "SFGYR": o = j.SFGYR; break;
							case "CYBZSM": o = j.CYBZSM; break;
						}
						dicPrms[field].ParamValue = o;
					}
				}
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}

		protected void WriteDJ_CBJYQ_DKXX(Cbjyqzdjb djb)
		{
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var dicPrms = new Dictionary<string, SQLParam>();
			var fields = "ID,DKID,DJBID,DKBM,DKMC,FBFBM,CBFBM,CBJYQQDFS,HTMJ,CBHTBM,LZHTBM,CBJYQZBM,YHTMJ,HTMJM,YHTMJM,SFQQQG,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,ZJRXM,DYZT,YYZT,LZZT,BZ";
			var sql = $"insert into DJ_CBJYQ_DKXX({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
				};
				dicPrms[field] = prm;
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";

			var dic = new Dictionary<string, DkItem>();
			string sin = null;
			foreach (var j in djb.Cbdkxx) {
				if (sin == null) sin = $"'{j.DKBM}'";
				else sin += $",'{j.DKBM}'";
			}
			_db.QueryCallback($"select DKBM,ID,DKMC,YDKBM,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ from DLXX_DK where DKBM in({sin})",r=>
			{
				int i = 0;
				dic[r.GetString(0)] = new DkItem()
				{
					DKID = r.GetString(++i),
					DKMC = r.GetString(++i),
					YDKBM=GetStr(r,++i),
					SYQXZ=GetStr(r,++i),
					DKLB=GetStr(r,++i),
					TDLYLX=GetStr(r,++i),
					DLDJ=GetStr(r,++i),
					TDYT=GetStr(r,++i),
					SFJBNT=GetStr(r,++i),
					SCMJ=GetDouble(r,++i),
					SCMJM=GetDouble(r,++i),
					ELHTMJ=GetDouble(r,++i),
					QQMJ=GetDouble(r,++i),
					JDDMJ=GetDouble(r,++i),
				};
				return true;
			});
				foreach (var j in djb.Cbdkxx)
			{
				if (!dic.TryGetValue(j.DKBM, out DkItem dkItem)) {
					dkItem = null;
				}
				var dk=djb.Cbdk.Find(x => { return j.DKBM == x.Dk.DKBM; })?.Dk;
				foreach (var kv in dicPrms)
				{
					object o = null;
					switch (kv.Key)
					{
						case "ID": o = j.ID; break;
						case "DKID": o =dkItem?.DKID;break;
						case "DJBID":o = djb.DJBID;break;
						case "DKBM":o = j.DKBM;break;
						case "DKMC":o = dkItem?.DKMC;break;
						case "FBFBM":o = j.FBFBM;break;
						case "CBFBM":o = j.CBFBM;break;
						case "CBJYQQDFS":o = j.CBJYQQDFS;break;
						case "HTMJ":o = j.HTMJ;break;
						case "CBHTBM":o = j.CBHTBM;break;
						case "LZHTBM":o = j.LZHTBM;break;
						case "CBJYQZBM":o = j.CBJYQZBM;break;
						case "YHTMJ":o = j.YHTMJ;break;
						case "HTMJM":o = j.HTMJM;break;
						case "YHTMJM":o = j.YHTMJM;break;
						case "SFQQQG":o = j.SFQQQG;break;
						case "YDKBM":o = dkItem?.YDKBM;break;
						case "SYQXZ":o = dkItem?.SYQXZ;break;
						case "DKLB":o = dk?.DKLB;break;
						case "TDLYLX":o = dkItem?.TDLYLX;break;
						case "DLDJ":o = dkItem?.DLDJ;break;
						case "TDYT":o = dkItem?.TDYT;break;
						case "SFJBNT":o = dkItem?.SFJBNT;break;
						case "SCMJ":o = dkItem?.SCMJ;break;
						case "SCMJM":o = dkItem?.SCMJM;break;
						case "ELHTMJ":o = dkItem?.ELHTMJ;break;
						case "QQMJ":o = dkItem?.QQMJ;break;
						case "JDDMJ":o = dkItem?.JDDMJ;break;
						case "DKDZ":o = dk?.DKDZ;break;
						case "DKNZ":o = dk?.DKNZ;break;
						case "DKXZ":o = dk?.DKXZ;break;
						case "DKBZ":o = dk?.DKBZ;break;
						case "DKBZXX":o = dk?.DKBZXX;break;
						case "ZJRXM":o = dk?.ZJRXM;break;
						case "DYZT":o = 0;break;
						case "YYZT":o = 0;break;
						case "LZZT":o = 0;break;
						case "BZ":o = null;break;
					}
					kv.Value.ParamValue = o;
				}
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}

		protected void UpdateQSSJ_CBDKXX(Cbjyqzdjb djb)
		{
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var dicPrms = new Dictionary<string, SQLParam>();
			var fields = "DKBM,FBFBM,CBFBM,CBJYQQDFS,CBHTBM,LZHTBM,CBJYQZBM,HTMJ,YHTMJ,HTMJM,YHTMJM,SFQQQG";
			var saFields = fields.Split(',');
			var sUpdate = $"update QSSJ_CBDKXX({fields}) set(";
			foreach (var field in saFields)
			{
				sUpdate += $"{field}={a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
				};
				dicPrms[field] = prm;
				sqlPrms.Add(prm);
			}
			sUpdate = sUpdate.TrimEnd(',');

			string sin = null;
			foreach (var dk in djb.Cbdkxx)
			{
				if (sin == null) sin = $"'{dk.DKBM}'";
				else sin += $",'{dk.DKBM}'";
			}

			var lstID = new List<string>();
			_db.QueryCallback($"select ID from QSSJ_CBDKXX where DKBM in({sin})", r =>
			{
				lstID.Add(r.GetString(0));
				return true;
			});
			int cnt = Math.Min(lstID.Count, djb.Cbdkxx.Count);
			for (int i = 0; i < cnt; ++i)
			{
				string id = lstID[i];
				var j = djb.Cbdkxx[i];
				foreach (var field in saFields)
				{
					object o = null;
					switch (field)
					{
						case "DKBM": o = j.DKBM; break;
						case "FBFBM": o = j.FBFBM; break;
						case "CBFBM": o = j.CBFBM; break;
						case "CBJYQQDFS": o = j.CBJYQQDFS; break;
						case "CBHTBM": o = j.CBHTBM; break;
						case "LZHTBM": o = j.LZHTBM; break;
						case "CBJYQZBM": o = j.CBJYQZBM; break;
						case "HTMJ": o = j.HTMJ; break;
						case "YHTMJ": o = j.YHTMJ; break;
						case "HTMJM": o = j.HTMJM; break;
						case "YHTMJM": o = j.YHTMJM; break;
						case "SFQQQG": o = j.SFQQQG; break;
					}
					dicPrms[field].ParamValue = o;
				}
				var sql = sUpdate + $") where ID='{id}'";
				_db.ExecuteNonQuery(sUpdate, sqlPrms);
			}
			if (cnt < lstID.Count)
			{
				sin = null;
				for (int i = cnt; i < lstID.Count; ++i)
				{
					if (sin == null) sin = $"'{lstID[i]}'";
					else sin += $",'{lstID[i]}'";
				}
				_db.ExecuteNonQuery($"delete from QSSJ_CBDKXX where ID in({sin})");
			}
			else if (cnt < djb.Cbdkxx.Count)
			{
				var sql = $"insert into QSSJ_CBDKXX({fields}) values(";
				foreach (var field in saFields)
				{
					sql += $"{a}{field},";
				}
				sql = sql.TrimEnd(',') + ")";
				for (int i = cnt; i < djb.Cbdkxx.Count; ++i)
				{
					var j = djb.Cbdkxx[i];
					foreach (var field in saFields)
					{
						object o = null;
						switch (field)
						{
							case "DKBM": o = j.DKBM; break;
							case "FBFBM": o = j.FBFBM; break;
							case "CBFBM": o = j.CBFBM; break;
							case "CBJYQQDFS": o = j.CBJYQQDFS; break;
							case "CBHTBM": o = j.CBHTBM; break;
							case "LZHTBM": o = j.LZHTBM; break;
							case "CBJYQZBM": o = j.CBJYQZBM; break;
							case "HTMJ": o = j.HTMJ; break;
							case "YHTMJ": o = j.YHTMJ; break;
							case "HTMJM": o = j.HTMJM; break;
							case "YHTMJM": o = j.YHTMJM; break;
							case "SFQQQG": o = j.SFQQQG; break;
						}
						dicPrms[field].ParamValue = o;
					}
				}
				_db.ExecuteNonQuery(sql, sqlPrms);
			}

		}

		protected abstract void ReadXmlExt(XmlElement xe);
		protected abstract void WriteXmlExt(XmlDocument xml, XmlNode parent);

		protected static string GetStr(IDataReader r, int i)
		{
			return XmlData.GetStr(r, i);// r.IsDBNull(i) ? null : SafeConvertAux.ToStr(r.GetValue(i));
		}
		protected static double GetDouble(IDataReader r, int i)
		{
			return XmlData.GetDouble(r, i);
		}
		protected static int GetInt(IDataReader r, int i)
		{
			return XmlData.GetInt(r, i);
		}
		protected static DateTime? GetDate(IDataReader r, int i)
		{
			return XmlData.GetDate(r, i);
		}
		protected static void AppendTextElementChild(XmlDocument xml,XmlNode parent, string name, string text)
		{
			var xe = xml.CreateElement(name);
			if (!string.IsNullOrEmpty(text))
			{
				xe.InnerText = text;
			}
			parent.AppendChild(xe);
		}

		void CheckExport(Cbjyqzdjb oldDjb, Cbjyqzdjb newDjb)
		{
			if (newDjb.Fbf != null && oldDjb.Fbf != null && oldDjb.Fbf.IsXmlDataEqual(newDjb.Fbf))
			{
				newDjb.Fbf.ExportXml = false;
			}
			if (oldDjb.Cbf != null && newDjb.Cbf != null && oldDjb.Cbf.IsXmlDataEqual(newDjb.Cbf))
				newDjb.Cbf.ExportXml = false;
			if (oldDjb.jtcies.Count == newDjb.jtcies.Count)
			{
				int func(Jtcy a, Jtcy b)
				{
					var s1 = a.ToCompairString();
					var s2 = b.ToCompairString();
					return s1.CompareTo(s2);
				}
				oldDjb.jtcies.Sort((a, b) => { return func(a, b); });
				newDjb.jtcies.Sort((a, b) => { return func(a, b); });

				newDjb.ExportJcties = false;
				for (int i = 0; i < oldDjb.jtcies.Count; ++i)
				{
					if (!oldDjb.jtcies[i].IsXmlEqual(newDjb.jtcies[i]))
					{
						newDjb.ExportJcties = true;
						break;
					}
				}
			}
			if (oldDjb.Cbht != null && newDjb.Cbht != null && oldDjb.Cbht.IsXmlDataEqual(newDjb.Cbht))
				newDjb.Cbht.ExportXml = false;
			if (oldDjb.IsXmlDataEqual(newDjb))
				newDjb.ExportXml = false;
			if (oldDjb.Cbjyqz != null && newDjb.Cbjyqz != null && oldDjb.Cbjyqz.IsXmlDataEqual(newDjb.Cbjyqz))
				newDjb.Cbjyqz.ExportXml = false;
		}

		protected void CopyFromOld(Cbjyqzdjb od, Cbjyqzdjb nd)
		{
			if (nd.CBJYQZBM == null) nd.AssignXmlData(od);
			if (nd.Cbf == null) nd.Cbf = od.Cbf;
			if (nd.Cbht == null) nd.Cbht = od.Cbht;
			if (nd.jtcies.Count == 0) nd.jtcies.AddRange(od.jtcies);
			if (nd.Cbjyqz == null) nd.Cbjyqz = od.Cbjyqz;
		}
	}

	/// <summary>
	/// 变更登记业务基类
	/// </summary>
	abstract class BGDJBusinessData : BusinessData
	{
		protected readonly List<DJ_CBJYQ_YDJB> lstYDJB = new List<DJ_CBJYQ_YDJB>();

		/// <summary>
		/// Fill entities.lstYDJB
		/// </summary>
		/// <param name="p"></param>
		/// <param name="entities"></param>
		protected void DefFillEntities(DJ_YW_SLSQ enSLSQ)
		{
			var p = _p;
			var dic = new Dictionary<string, DJ_CBJYQ_YDJB>();
			var sql = $"select ID,CBJYQZBM,CBFBM from DJ_CBJYQ_DJB where QSZT={(int)EQszt.Xians} and CBJYQZBM in(";
			foreach (var djb in p.ChangeData.lstDjb)
			{
				sql += $"'{djb.CBJYQZBM}',";
			}
			sql = sql.TrimEnd(',') + ")";
			_db.QueryCallback(sql, r =>
			{
				var CBJYQZBM = r.GetString(1);
				dic[CBJYQZBM] = new DJ_CBJYQ_YDJB()
				{
					YDJBID = r.GetString(0),
					YCBHTBM = CBJYQZBM,
					YCBFBM = r.GetString(2),
					SLSQID = enSLSQ.ID,
					DJXL = XmlExchangeUtil.SJLXToDjxl(p.Head.SJLX)
				};
				return true;
			});

			foreach (var djb in p.ChangeData.lstDjb)
			{
				if (dic.TryGetValue(djb.CBJYQZBM, out DJ_CBJYQ_YDJB en))
				{
					en.DJBID = djb.DJBID;
					lstYDJB.Add(en);
				}
			}
		}

		/// <summary>
		/// 写入 DJ_CBJYQ_YDJB
		/// </summary>
		protected void WriteDJ_CBJYQ_YDJB()
		{
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var dicPrm = new Dictionary<string, SQLParam>();

			var fields = "ID,DJBID,YDJBID,SLSQID,DJLX,DJXL,BGLX";
			var sql = $"insert into DJ_CBJYQ_YDJB({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
				};
				dicPrm[field] = prm;
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			foreach (var it in lstYDJB)
			{
				dicPrm["ID"].ParamValue = it.ID;
				dicPrm["DJBID"].ParamValue = it.DJBID;
				dicPrm["YDJBID"].ParamValue = it.YDJBID;
				dicPrm["SLSQID"].ParamValue = it.SLSQID;
				dicPrm["DJLX"].ParamValue = it.DJLX;
				dicPrm["DJXL"].ParamValue = it.DJXL;
				dicPrm["BGLX"].ParamValue = it.BGLX;
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
		}
		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			DefWriteDJ_YW_SQQL(enSLSQ);
			WriteDJ_CBJYQ_YDJB();
			UpdateDjbQSZT();

			foreach (var djb in _p.ChangeData.lstDjb)
			{
				WriteDJ_CBJYQ_DJB(djb);
				WriteDJ_CBJYQ_QZ(djb);
				WriteDJ_CBJYQ_CBHT(djb);
				WriteDJ_CBJYQ_CBF(djb);
				WriteDJ_CBJYQ_CBF_JTCY(djb);
				WriteDJ_CBJYQ_DKXX(djb);
				UpdateQSSJ_CBJYQZ(djb);
				UpdateQSSJ_CBHT(djb);
				UpdateQSSJ_CBF(djb);
				UpdateQSSJ_CBF_JTCY(djb);
				UpdateQSSJ_CBDKXX(djb);
			}
		}
		/// <summary>
		/// 更新原登记簿的权属状态
		/// </summary>
		void UpdateDjbQSZT()
		{
			string sin = null;
			foreach (var it in lstYDJB)
			{
				if (sin == null) sin = $"'{it.YDJBID}'";
				else sin += $",'{it.YDJBID}'";
			}
			if (sin != null)
			{
				var sql = $"update DJ_CBJYQ_DJB set QSZT={(int)EQszt.History} where ID in({sin})";
				_db.ExecuteNonQuery(sql);
			}
		}
	}

	/// <summary>
	/// 含流转变更登记基类（转让和互换）
	/// </summary>
	abstract class LZBGBusinessData : BGDJBusinessData
	{
		protected readonly List<DJ_CBJYQ_LZHT> lstLZHT = new List<DJ_CBJYQ_LZHT>();
		protected readonly List<DJ_CBJYQ_LZDK> lstLZDK = new List<DJ_CBJYQ_LZDK>();


		public override void ReadChangeData(XmlDocument xml, XmlNode xn)
		{
			var dicDjb = new List<Tuple<Cbjyqzdjb, Cbjyqzdjb>>();
			foreach (var djb in _p.OriginalData.lstDjb)
			{
				dicDjb.Add(new Tuple<Cbjyqzdjb, Cbjyqzdjb>(djb, new Cbjyqzdjb()));
			}

			var lstCbdk = new List<Cbdk>();
			var lstJzd = new List<Jzd>();
			var lstJzx = new List<Jzx>();
			foreach (var cn in xn.ChildNodes)
			{
				if (cn is XmlElement xe)
				{
					switch (xe.Name)
					{
						case "FBF":
							{
								var it = new Fbf();
								it.ReadXml(xe);
								foreach (var t in dicDjb)
								{
									t.Item2.Fbf = it;
								}
							}
							break;
						case "CBF":
							{
								var it = new Cbf();
								it.ReadXml(xe);
								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
								djb.Cbf = it;
							}
							break;
						case "CBHT":
							{
								var it = new Cbht();
								it.ReadXml(xe);
								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
								djb.Cbht = it;
							}
							break;
						case "CBJYQZDJB":
							{
								var it = new Cbjyqzdjb();
								it.ReadXml(xe);
								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
								djb.AssignXmlData(it);
							}
							break;
						case "CBJYQZ":
							{
								var it = new Cbjyqz();
								it.ReadXml(xe);
								var djb = dicDjb.Find(a => { return a.Item1.CBJYQZBM == it.CBJYQZBM; }).Item2;
								djb.Cbjyqz = it;
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
										if (it.CZFS != ECzfs.Sc)
										{
											var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
											djb.jtcies.Add(it);
										}
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
										lstCbdk.Add(it);
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
										var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
										djb.Cbdkxx.Add(it);
									}
								}
							}
							break;
#endif
						case "JTCY":
							{
								var it = new Jtcy();
								it.ReadXml(xe);
								if (it.CZFS != ECzfs.Sc)
								{
									var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
									djb.jtcies.Add(it);
								}
							}
							break;
						case "CBDK":
							{
								var it = new Cbdk();
								it.ReadXml(xe);
								lstCbdk.Add(it);
							}
							break;
						case "CBDKXX":
							{
								var it = new Cbdkxx();
								it.ReadXml(xe);
								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
								djb.Cbdkxx.Add(it);
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
							_p.ChangeData.TYZB = xe.InnerText;
							break;
					}
				}
			}

			foreach (var t in dicDjb)
			{
				var od = t.Item1;
				var nd = t.Item2;
				CopyFromOld(od, nd);

				foreach (var it in nd.Cbdkxx)
				{
					var dk = lstCbdk.Find(a => { return a.Dk?.DKBM == it.DKBM; });
					if (dk != null)
					{
						nd.Cbdk.Add(dk);
						lstCbdk.Remove(dk);
						foreach (var jt in lstJzd)
						{
							if (jt.DKBM == dk.Dk?.DKBM) nd.DkJzd.Add(jt);
						}
						lstJzd.RemoveAll(a => { return a.DKBM == dk.Dk?.DKBM; });
						foreach (var jt in lstJzx)
						{
							if (jt.DKBM == dk.Dk?.DKBM) nd.DkJzx.Add(jt);
						}
						lstJzx.RemoveAll(a => { return a.DKBM == dk.Dk?.DKBM; });
					}
				}

				_p.ChangeData.lstDjb.Add(nd);
			}
		}
		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			base.DoImport(enSLSQ);

			string sql;
			string fields;
			var a = _db.SqlFunc.GetParamPrefix();
			var sqlPrms = new List<SQLParam>();
			var dicPrm = new Dictionary<string, SQLParam>();

			#region 写入 DJ_CBJYQ_LZHT
			fields = "ID,DJBID,YDJBID,CBHTBM,LZHTBM,CBFBM,SRFBM,LZFS,LZQX,LZQXKSRQ,LZQXJSRQ,LZMJ,LZMJM,LZDKS,LZQTDYT,LZHTDYT,LZJGSM,HTQDRQ,SZDY,CJYH,CJSJ,ZHXGYH,ZHXGSJ";
			sql = $"insert into DJ_CBJYQ_LZHT({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
				};
				dicPrm[field] = prm;
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			foreach (var it in lstLZHT)
			{
				dicPrm["ID"].ParamValue = it.ID;
				dicPrm["DJBID"].ParamValue = it.DJBID;
				dicPrm["YDJBID"].ParamValue = it.YDJBID;
				dicPrm["CBHTBM"].ParamValue = it.CBHTBM;
				dicPrm["LZHTBM"].ParamValue = it.LZHTBM;
				dicPrm["CBFBM"].ParamValue = it.CBFBM;
				dicPrm["SRFBM"].ParamValue = it.SRFBM;
				dicPrm["LZFS"].ParamValue = it.LZFS;
				dicPrm["LZQX"].ParamValue = it.LZQX;
				dicPrm["LZQXKSRQ"].ParamValue = it.LZQXKSRQ;
				dicPrm["LZQXJSRQ"].ParamValue = it.LZQXJSRQ;
				dicPrm["LZMJ"].ParamValue = it.LZMJ;
				dicPrm["LZMJM"].ParamValue = it.LZMJM;
				dicPrm["LZDKS"].ParamValue = it.LZDKS;
				dicPrm["LZQTDYT"].ParamValue = it.LZQTDYT;
				dicPrm["LZHTDYT"].ParamValue = it.LZHTDYT;
				dicPrm["LZJGSM"].ParamValue = it.LZJGSM;
				dicPrm["HTQDRQ"].ParamValue = it.HTQDRQ;
				dicPrm["SZDY"].ParamValue = it.SZDY;
				dicPrm["CJYH"].ParamValue = it.CJYH;
				dicPrm["CJSJ"].ParamValue = it.CJSJ;
				dicPrm["ZHXGYH"].ParamValue = it.ZHXGYH;
				dicPrm["ZHXGSJ"].ParamValue = it.ZHXGSJ;
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
			#endregion

			#region 写入 DJ_CBJYQ_LZDK
			sqlPrms.Clear();
			fields = "ID,DJBID,YDJBID,DJBBM,HTID,HTBM,DKID,DKBM,DKMJ,DKMJM";
			sql = $"insert into DJ_CBJYQ_LZDK({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";
				var prm = new SQLParam()
				{
					ParamName = field,
				};
				dicPrm[field] = prm;
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			foreach (var it in lstLZDK)
			{
				dicPrm["ID"].ParamValue = it.ID;
				dicPrm["DJBID"].ParamValue = it.DJBID;
				dicPrm["YDJBID"].ParamValue = it.YDJBID;
				dicPrm["DJBBM"].ParamValue = it.DJBBM;
				dicPrm["HTID"].ParamValue = it.HTID;
				dicPrm["HTBM"].ParamValue = it.HTBM;
				dicPrm["DKID"].ParamValue = it.DKID;
				dicPrm["DKBM"].ParamValue = it.DKBM;
				dicPrm["DKMJ"].ParamValue = it.DKMJ;
				dicPrm["DKMJM"].ParamValue = it.DKMJM;
				_db.ExecuteNonQuery(sql, sqlPrms);
			}
			#endregion

			var djb = _p.ChangeData.lstDjb[0];
			WriteDJ_CBJYQ_QZ(djb);
			UpdateQSSJ_CBJYQZ(djb);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ydjb"></param>
		/// <param name="zrDjb"></param>
		/// <param name="DKBM"></param>
		/// <param name="entities"></param>
		protected void FillEntitiesLzhtAndLzdk(DJ_CBJYQ_YDJB ydjb, Cbjyqzdjb zrDjb, string DKBM)//, DJ_YW_SLSQ slsqEn)// Entities entities)
		{
			var lzht = new DJ_CBJYQ_LZHT
			{
				DJBID = zrDjb.DJBID,
				YDJBID = ydjb.YDJBID,
				CBHTBM = ydjb.YCBHTBM,
				LZHTBM = zrDjb.CBFBM,
				CBFBM = ydjb.YCBFBM,
				SRFBM = zrDjb.CBFBM,
				LZFS = (int)ECbjyqQDFS.Zr,
				LZJGSM = "无",
				SZDY = FBFBM
			};

			lzht.SZDY = XmlExchangeUtil.Fbfbm2SZDY(FBFBM);

			if (YWBLSJ != null)
			{
				lzht.LZQXKSRQ = (DateTime)YWBLSJ;
			}
			lzht.HTQDRQ = lzht.LZQXKSRQ;
			var o = _db.QueryOne($"select CBQXZ from QSSJ_CBHT where CBHTBM='{ydjb.YCBHTBM}'");
			if (o != null)
			{
				lzht.LZQXJSRQ = DateTime.Parse(o.ToString());
			}
			lzht.LZQX = XmlExchangeUtil.GetQX(lzht.LZQXKSRQ, lzht.LZQXJSRQ);

			if (DKBM != null)
			{
				var sa = DKBM.Split(';');

				string sin = "'" + DKBM.Replace(";", "','") + "'";
				var sql = $"select SUM(HTMJ),SUM(HTMJM) from DJ_CBJYQ_DKXX where DJBID='{lzht.YDJBID}' and DKBM in({sin})";
				_db.QueryCallback(sql, r =>
				{
					lzht.LZMJ = SafeConvertAux.ToDouble(r.GetValue(0));
					lzht.LZMJM = SafeConvertAux.ToDouble(r.GetValue(1));
					return false;
				});
				lzht.LZDKS = sa.Length;

				#region LZDK

				foreach (var dkbm in sa)
				{
					var en = new DJ_CBJYQ_LZDK
					{
						DJBID = zrDjb.DJBID,
						YDJBID = ydjb.YDJBID,
						DJBBM = zrDjb.CBJYQZBM,
						HTID = lzht.ID,
						HTBM = lzht.LZHTBM,
						DKBM = dkbm,
					};
					lstLZDK.Add(en);
					sql = $"select HTMJ,HTMJM,DKID from DJ_CBJYQ_DKXX where DKBM='{dkbm}' and DJBID='{lzht.YDJBID}'";
					_db.QueryCallback(sql, r =>
					{
						en.DKMJ = SafeConvertAux.ToDouble(r.GetValue(0));
						en.DKMJM = SafeConvertAux.ToDouble(r.GetValue(1));
						en.DKID = r.GetString(2);
						return false;
					});
				}
				#endregion
			}
			lstLZHT.Add(lzht);
		}

	}

	abstract class QZBusinessData : BusinessData
	{
		public string CBFBM;
		public string CBHTBM;
		public string CBJYQZBM;
		public override bool HasOriginData()
		{
			return false;
		}
		//public override void FillEntities(DJ_YW_SLSQ en)
		//{
		//	if (_p.ChangeData.lstDjb.Count == 1)
		//	{
		//		var d = _p.ChangeData.lstDjb[0];
		//		CBFBM = d.CBFBM;
		//		CBHTBM = d.CBHTBM;
		//		CBJYQZBM = d.CBJYQZBM;
		//	}
		//}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			if (cd.lstDjb.Count == 1)
			{
				var d = cd.lstDjb[0];
				CBFBM = d.CBFBM;
				CBHTBM = d.CBHTBM;
				CBJYQZBM = d.CBJYQZBM;
			}
		}
		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			base.DefWriteDJ_YW_SQQL(enSLSQ);

			var djb = _p.ChangeData.lstDjb[0];
			djb.CBJYQZBM =djb.Cbjyqz.CBJYQZBM;
			bool fOK = false;
			var sql = $"select ID,DJSJ from DJ_CBJYQ_DJB where CBJYQZBM='{djb.CBJYQZBM}' and QSZT={(int)EQszt.Xians}";
			_db.QueryCallback(sql, r =>
			{
				djb.DJBID = r.GetString(0);
				djb.DJSJ = GetDate(r, 1);
				fOK = true;
				return false;
			});
			if (fOK)
			{
				sql = $"update DJ_CBJYQ_QZ set SFYZX={(int)ESfyzx.YZX} where DJBID='{djb.DJBID}' and CBJYQZBM='{djb.CBJYQZBM}' and SJYZX={(int)ESfyzx.WZX}";
				_db.ExecuteNonQuery(sql);
				base.WriteDJ_CBJYQ_QZ(djb);
				base.UpdateQSSJ_CBJYQZ(djb);
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			AppendTextElementChild(xml, parent, "CBFBM", CBFBM);
			AppendTextElementChild(xml, parent, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, parent, "CBJYQZBM", CBJYQZBM);
		}
		protected override void ReadXmlExt(XmlElement xe)
		{
			switch (xe.Name)
			{
				case "CBFBM": CBFBM = xe.InnerText; break;
				case "CBHTBM": CBHTBM = xe.InnerText; break;
				case "CBJYQZBM": CBJYQZBM = xe.InnerText; break;
			}
		}
	}

	class BusinessDataFactory
	{
		public static BusinessData Create(/*Task task, IFeatureWorkspace db,*/ int sjlx)
		{
			BusinessData bd=null;
			switch (sjlx)
			{
				case 101:bd =new ScBusinessData(); break;//首次登记 / 初始登记
				case 102:bd = new ZrBusinessData(); break;//转让登记
				case 103:bd = new YbBusinessData(); break;//一般登记 / 其它变更登记
				case 104:bd = new HuhBusinessData(); break;//互换登记
				case 105:bd = new FhBusinessData(); break;//分户登记
				case 106:bd = new HehBusinessData(); break;//合户登记
				case 107:bd = new ZxBusinessData(); break;//注销登记
				case 201:bd = new QzbfBusinessData(); break;//权证补发
				case 202:bd = new QzhfBusinessData(); break;//权证换发
			}
			//bd.Init(task,db);
			return bd;
		}
		/// <summary>
		/// 是否有效的数据类型
		/// </summary>
		/// <param name="sjlx"></param>
		/// <returns></returns>
		public static bool IsValidSjlx(int sjlx)
		{
			return sjlx >= 101 && sjlx < 108 || sjlx == 201 || sjlx == 202;
		}
	}

	/// <summary>
	/// 首次登记
	/// </summary>
	class ScBusinessData : BusinessData
	{
		public string CBFBM;
		public string CBHTBM;
		public string CBJYQZBM;

		public override bool HasOriginData()
		{
			return false;
		}
		public override void ReadChangeData(XmlDocument xml, XmlNode parent)
		{
			_p.ReadData(xml, parent, _p.ChangeData);
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			var djb=changeData.lstDjb[0];
			djb.Cbf.CZFS = ECzfs.Zj;
			foreach (var j in djb.jtcies)
			{
				j.CZFS = ECzfs.Zj;
			}
			djb.Cbht.CZFS = ECzfs.Zj;
			djb.CZFS = ECzfs.Zj;
			djb.Cbjyqz.CZFS = ECzfs.Zj;
		}

		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			if (_p.ChangeData.lstDjb.Count != 1)
			{
				_task.ReportError("Xml文件内容有误");
				return;
			}
			DefWriteDJ_YW_SQQL(enSLSQ);
			var djb = _p.ChangeData.lstDjb[0];
			WriteDJ_CBJYQ_DJB(djb);
			WriteDJ_CBJYQ_QZ(djb);
			WriteDJ_CBJYQ_CBHT(djb);
			WriteDJ_CBJYQ_CBF(djb);
			WriteDJ_CBJYQ_CBF_JTCY(djb);
			WriteDJ_CBJYQ_DKXX(djb);
			UpdateQSSJ_CBJYQZ(djb);
			UpdateQSSJ_CBHT(djb);
			UpdateQSSJ_CBF(djb);
			UpdateQSSJ_CBF_JTCY(djb);
			UpdateQSSJ_CBDKXX(djb);
		}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			if (cd.lstDjb.Count > 0)
			{
				var d = cd.lstDjb[0];
				CBFBM =d.CBFBM;
				CBHTBM =d.CBHTBM;
				CBJYQZBM = d.CBJYQZBM;
			}
		}

		protected override void WriteXmlExt(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
		}

		protected override void ReadXmlExt(XmlElement xe)
		{
			switch (xe.Name)
			{
				case "CBFBM":CBFBM = xe.InnerText;break;
				case "CBHTBM": CBHTBM = xe.InnerText; break;
				case "CBJYQZBM": CBJYQZBM = xe.InnerText; break;
			}
		}
	}

	/// <summary>
	/// 一般变更登记
	/// </summary>
	class YbBusinessData : BGDJBusinessData
	{
		public string CBFBM;
		public string CBHTBM;
		public string CBJYQZBM;

		public override void FillEntities(DJ_YW_SLSQ en)
		{
			DefFillEntities(en);
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			var djb = changeData.lstDjb[0];
			djb.Cbf.CZFS = ECzfs.Xg;
			djb.Cbht.CZFS = ECzfs.Xg;
			djb.CZFS = ECzfs.Xg;
			djb.Cbjyqz.CZFS = ECzfs.Xg;

			if (djb.ExportJcties)
			{
				var od = _p.OriginalData.lstDjb[0];
				foreach (var j in djb.jtcies)
				{
					var j1 = od.jtcies.Find(a => { return a.ToCompairString() == j.ToCompairString(); });
					if (j1 != null)
					{
						if (!j1.IsXmlEqual(j))
						{
							j.CZFS = ECzfs.Xg;
						}
					}
					else
					{
						j.CZFS = ECzfs.Zj;
					}
				}
				foreach (var j in od.jtcies)
				{
					if (djb.jtcies.Find(a => { return a.ToCompairString() == j.ToCompairString(); }) == null)
					{
						j.CZFS = ECzfs.Sc;
						djb.jtcies.Add(j);
					}
				}
			}
		}
		public override void ReadChangeData(XmlDocument xml,XmlNode xn)
		{
			if (_p.OriginalData.lstDjb.Count != 1)
			{
				_task.ReportError("Xml数据内容有误，缺少原登记簿数据");
				return;
			}
			var od = _p.OriginalData.lstDjb[0];
			var djb = new Cbjyqzdjb();
			foreach (var cn in xn.ChildNodes)
			{
				if (cn is XmlElement xe)
				{
					switch (xe.Name)
					{
						case "FBF":
							{
								var it = new Fbf();
								it.ReadXml(xe);
								djb.Fbf = it;
							}
							break;
						case "CBF":
							{
								var it = new Cbf();
								it.ReadXml(xe);
								djb.Cbf = it;
							}
							break;
						case "CBHT":
							{
								var it = new Cbht();
								it.ReadXml(xe);
								djb.Cbht = it;
							}
							break;
						case "CBJYQZDJB":
							{
								djb.ReadXml(xe);
							}
							break;
						case "CBJYQZ":
							{
								var it = new Cbjyqz();
								it.ReadXml(xe);
								djb.Cbjyqz = it;
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
										if (it.CZFS != ECzfs.Sc)
										{
											djb.jtcies.Add(it);
										}
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
										djb.Cbdk.Add(it);
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
										djb.Cbdkxx.Add(it);
									}
								}
							}
							break;
#endif
						case "JTCY":
							{
								var it = new Jtcy();
								it.ReadXml(xe);
								if (it.CZFS != ECzfs.Sc)
								{
									djb.jtcies.Add(it);
								}
							}
							break;
						case "CBDK":
							{
								var it = new Cbdk();
								it.ReadXml(xe);
								djb.Cbdk.Add(it);
							}
							break;
						case "CBDKXX":
							{
								var it = new Cbdkxx();
								it.ReadXml(xe);
								djb.Cbdkxx.Add(it);
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
										djb.DkJzd.Add(it);
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
										djb.DkJzx.Add(it);
									}
								}
							}
							break;
						case "TYZB":
							_p.ChangeData.TYZB = xe.InnerText;
							break;
					}
				}
			}
			CopyFromOld(od, djb);
			_p.ChangeData.lstDjb.Add(djb);
		}
		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			if (_p.ChangeData.lstDjb.Count != 1)
			{
				_task.ReportError("Xml文件内容有误");
				return;
			}
			base.DoImport(enSLSQ);

			//DefWriteDJ_YW_SQQL(enSLSQ);

		}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			if (cd.lstDjb.Count > 0)
			{
				var d = cd.lstDjb[0];
				CBFBM = d.Cbf?.CBFBM;
				CBHTBM = d.Cbht?.CBHTBM;

				var qz = d.Cbjyqz;//.Find(a => { return !a.SFYZX; });
				CBJYQZBM = qz?.CBJYQZBM;
			}
		}

		protected override void WriteXmlExt(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
		}

		protected override void ReadXmlExt(XmlElement xe)
		{
			switch (xe.Name)
			{
				case "CBFBM": CBFBM = xe.InnerText; break;
				case "CBHTBM": CBHTBM = xe.InnerText; break;
				case "CBJYQZBM": CBJYQZBM = xe.InnerText; break;
			}
		}
	}
	/// <summary>
	/// 转让登记业务数据
	/// </summary>
	class ZrBusinessData : LZBGBusinessData
	{
		/// <summary>
		/// 转出承包方代码
		/// </summary>
		public string ZCCBFBM;
		/// <summary>
		/// 转出承包经营权证（登记簿）代码
		/// </summary>
		public string ZCCBJYQZBM;
		/// <summary>
		/// 转入承包方代码
		/// </summary>
		public string ZRCBFBM;
		/// <summary>
		/// 转入承包经营权证（登记簿）代码
		/// </summary>
		public string ZRCBJYQZBM;
		/// <summary>
		/// 承包合同代码（多个承包合同代码以分号分隔。）（转入方的承包合同代码）
		/// </summary>
		public string CBHTBM;
		/// <summary>
		/// 地块代码(多个承包合同代码以分号分隔)
		/// </summary>
		public string DKBM;

		public override void FillEntities(DJ_YW_SLSQ en)// Entities entities)
		{
			var p = _p;
			DefFillEntities(en);

			var zrDjb = p.ChangeData.lstDjb.Find(a => { return a.CBJYQZBM == ZRCBJYQZBM; });
			var ydjb = lstYDJB.Find(a => { return a.YCBFBM == ZCCBFBM; });
			FillEntitiesLzhtAndLzdk(ydjb, zrDjb, DKBM);
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			foreach (var d in changeData.lstDjb)
			{
				d.CZFS = ECzfs.Zj;
				d.Cbjyqz.CZFS = ECzfs.Zj;
				d.Cbht.CZFS = ECzfs.Zj;
			}
		}

		protected override void ReadXmlExt(XmlElement e)
		{
			switch (e.Name)
			{
				case "ZCCBFBM": ZCCBFBM = e.InnerText; break;
				case "ZCCBJYQZBM": ZCCBJYQZBM = e.InnerText; break;
				case "ZRCBFBM": ZRCBFBM = e.InnerText; break;
				case "ZRCBJYQZBM": ZRCBJYQZBM = e.InnerText; break;
				case "CBHTBM": CBHTBM = e.InnerText; break;
				case "DKBM": DKBM = e.InnerText; break;
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			AppendTextElementChild(xml, parent, "ZCCBFBM", ZCCBFBM);
			AppendTextElementChild(xml, parent, "ZCCBJYQZBM", ZCCBJYQZBM);
			AppendTextElementChild(xml, parent, "ZRCBFBM", ZRCBFBM);
			AppendTextElementChild(xml, parent, "ZRCBJYQZBM", ZRCBJYQZBM);
			AppendTextElementChild(xml, parent, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, parent, "DKBM", DKBM);
		}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			var task = _task;
			if (lstDjbID.Count == 0)
				return;
			if (lstDjbID.Count != 2)
			{
				task.ReportWarning($"转让登记业务导出只支持一个转出方和一个转入方");
				return;
			}
			var db = _db;
			string zcDJBID = null;
			string zrDJBID = null;
			string lzhtID = null;
			string sql = null;

			sql = $"select DJBID,YDJBID,ID from DJ_CBJYQ_LZHT where DJBID in('{lstDjbID[0].DJBID}','{lstDjbID[1].DJBID}')";
			db.QueryCallback(sql, r =>
			{
				zrDJBID = GetStr(r, 0);
				zcDJBID = GetStr(r, 1);
				lzhtID = GetStr(r, 2);
				return false;
			});
			

			if (zrDJBID == null)
			{
				task.ReportWarning("未找到转入方");
				return;
			}
			if (zcDJBID == null)
			{
				task.ReportWarning("未找到转出方");
				return;
			}

			Query(db, zcDJBID, out ZCCBFBM, out ZCCBJYQZBM, out CBHTBM);
			Query(db, zrDJBID, out ZRCBFBM, out ZRCBJYQZBM, out CBHTBM);

			sql = $"select DKBM from DJ_CBJYQ_LZDK where HTID='{lzhtID}'";
			db.QueryCallback(sql, r =>
			 {
				 var dkbm = GetStr(r, 0);
				 if (!string.IsNullOrEmpty(dkbm))
				 {
					 if (DKBM == null) DKBM = dkbm;
					 else DKBM += $";{dkbm}";
				 }
				 return true;
			 });


		}

		void Query(IFeatureWorkspace db, string djbID, out string sCBFBM, out string sCBJYQZBM, out string sCBHTBM)
		{
			string cbfBM = null;
			string cbjyqzBM = null;
			string cbhtBM = null;
			var sql = $"select a.CBFBM,a.CBJYQZBM,b.CBHTBM from DJ_CBJYQ_DJB a left join DJ_CBJYQ_CBHT b on a.ID=b.DJBID where a.ID='{djbID}'";
			db.QueryCallback(sql, r =>
			 {
				 cbfBM = GetStr(r, 0);
				 cbjyqzBM = GetStr(r, 1);
				 cbhtBM = GetStr(r, 2);
				 return false;
			 });
			sCBFBM = cbfBM;
			sCBJYQZBM = cbjyqzBM;
			sCBHTBM = cbhtBM;
		}
	}

	/// <summary>
	/// 互换登记业务数据
	/// </summary>
	class HuhBusinessData : LZBGBusinessData
	{
		public readonly JF JF = new JF();
		public readonly YF YF = new YF();

		public override void FillEntities(DJ_YW_SLSQ en)// Entities entities)
		{
			var p = _p;
			DefFillEntities(en);
			if (lstYDJB.Count != 2)
			{
				_task.ReportError("Xml文件内容有误！");
				return;
			}

			#region LZHT
			for (int i=0;i<2;++i)
			{//a->b(a转让给b)
				var a = lstYDJB[i];
				var b = i == 0 ? lstYDJB[1] : lstYDJB[0];
				var aF = JF.CBFBM == a.YCBFBM ? JF : YF;
				var db1 = p.ChangeData.lstDjb.Find(x => { return x.CBFBM == b.YCBFBM; });
				if (db1 == null)
				{
					_task.ReportError("Xml数据有误");
					return;
				}
				FillEntitiesLzhtAndLzdk(a, db1, aF.DKBM);//, entities);
				
				/*
				var lzht = new DJ_CBJYQ_LZHT()
				{
					YDJBID = a.YDJBID,
					DJBID = b.DJBID,
					LZFS = (int)ECbjyqQDFS.Huh,
					LZJGSM = "无"
				};

				lzht.CBHTBM = da0.CBHTBM;
				lzht.LZHTBM = db1.CBFBM;
				lzht.CBFBM = da0.CBFBM;
				lzht.SRFBM = db1.CBFBM;
				lzht.SZDY = FBFBM;
				if (FBFBM.Length == 14 && FBFBM.EndsWith("00"))
				{
					lzht.SZDY = FBFBM.Substring(0, 12);
				}

				if (YWBLSJ != null)
				{
					lzht.LZQXKSRQ = (DateTime)YWBLSJ;
				}
				lzht.HTQDRQ = lzht.LZQXKSRQ;
				var o = _db.QueryOne($"select CBQXZ from QSSJ_CBHT where CBHTBM='{a.YCBHTBM}'");
				if (o != null)
				{
					lzht.LZQXJSRQ = DateTime.Parse(o.ToString());
				}
				lzht.LZQX = XmlExchangeUtil.GetQX(lzht.LZQXKSRQ, lzht.LZQXJSRQ);

				if (aF.DKBM != null)
				{
					string sin = "'" + aF.DKBM.Replace(";", "','") + "'";
					var sql = $"select SUM(HTMJ),SUM(HTMJM) from DJ_CBJYQ_DKXX where DJBID='{lzht.YDJBID}' and DKBM in({sin})";
					_db.QueryCallback(sql, r =>
					{
						lzht.LZMJ = SafeConvertAux.ToDouble(r.GetValue(0));
						lzht.LZMJM = SafeConvertAux.ToDouble(r.GetValue(1));
						return false;
					});

					var sa = aF.DKBM.Split(';');
					lzht.LZDKS = sa.Length;

					#region LZDK
					foreach (var dkbm in sa)
					{
						var zrDjb = p.ChangeData.lstDjb.Find(x => { return x.CBJYQZBM == db1.CBJYQZBM; });
						var ydjb = entities.lstYDJB.Find(x => { return x.YCBFBM == da0.CBFBM; });
						var en = new DJ_CBJYQ_LZDK
						{
							DJBID = zrDjb.DJBID,
							YDJBID = ydjb.YDJBID,
							DJBBM = zrDjb.CBJYQZBM,
							HTID = lzht.ID,
							HTBM = lzht.LZHTBM,
							DKBM = dkbm,
						};
						entities.lstLZDK.Add(en);
						sql = $"select HTMJ,HTMJM,DKID from DJ_CBJYQ_DKXX where DKBM='{dkbm}' and DJBID='{lzht.YDJBID}'";
						_db.QueryCallback(sql, r =>
						{
							en.DKMJ = SafeConvertAux.ToDouble(r.GetValue(0));
							en.DKMJM = SafeConvertAux.ToDouble(r.GetValue(1));
							en.DKID = r.GetString(2);
							return false;
						});
					}
					#endregion
				}
				entities.lstLZHT.Add(lzht);
				*/
			}
			#endregion
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			foreach (var d in changeData.lstDjb)
			{
				d.CZFS = ECzfs.Zj;
				d.Cbjyqz.CZFS = ECzfs.Zj;
			}
		}
//		public override void ReadChangeData(XmlDocument xml, XmlNode xn)
//		{
//			var dicDjb = new List<Tuple<Cbjyqzdjb, Cbjyqzdjb>>();
//			foreach (var djb in _p.OriginalData.lstDjb)
//			{
//				dicDjb.Add(new Tuple<Cbjyqzdjb, Cbjyqzdjb>(djb, new Cbjyqzdjb()));
//			}

//			var lstCbdk = new List<Cbdk>();
//			var lstJzd = new List<Jzd>();
//			var lstJzx = new List<Jzx>();
//			foreach (var cn in xn.ChildNodes)
//			{
//				if (cn is XmlElement xe)
//				{
//					switch (xe.Name)
//					{
//						case "FBF":
//							{
//								var it = new Fbf();
//								it.ReadXml(xe);
//								foreach (var t in dicDjb)
//								{
//									t.Item2.Fbf = it;
//								}
//							}
//							break;
//						case "CBF":
//							{
//								var it = new Cbf();
//								it.ReadXml(xe);
//								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//								djb.Cbf = it;
//							}
//							break;
//						case "CBHT":
//							{
//								var it = new Cbht();
//								it.ReadXml(xe);
//								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//								djb.Cbht = it;
//							}
//							break;
//						case "CBJYQZDJB":
//							{
//								var it = new Cbjyqzdjb();
//								it.ReadXml(xe);
//								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//								djb.AssignXmlData(it);
//							}
//							break;
//						case "CBJYQZ":
//							{
//								var it = new Cbjyqz();
//								it.ReadXml(xe);
//								var djb = dicDjb.Find(a => { return a.Item1.CBJYQZBM == it.CBJYQZBM; }).Item2;
//								djb.Cbjyqz = it;
//							}
//							break;
//#if DEBUG
//						case "CBFJTCY":
//							{
//								foreach (var n in xe.ChildNodes)
//								{
//									if (n is XmlElement e)
//									{
//										var it = new Jtcy();
//										it.ReadXml(e);
//										if (it.CZFS != ECzfs.Sc)
//										{
//											var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//											djb.jtcies.Add(it);
//										}
//									}
//								}
//							}
//							break;
//						case "CBDKS":
//							{
//								foreach (var n in xe.ChildNodes)
//								{
//									if (n is XmlElement e)
//									{
//										var it = new Cbdk();
//										it.ReadXml(e);
//										lstCbdk.Add(it);
//									}
//								}
//							}
//							break;
//						case "CBDKXXS":
//							{
//								foreach (var n in xe.ChildNodes)
//								{
//									if (n is XmlElement e)
//									{
//										var it = new Cbdkxx();
//										it.ReadXml(e);
//										var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//										djb.Cbdkxx.Add(it);
//									}
//								}
//							}
//							break;
//#endif
//						case "JTCY":
//							{
//								var it = new Jtcy();
//								it.ReadXml(xe);
//								if (it.CZFS != ECzfs.Sc)
//								{
//									var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//									djb.jtcies.Add(it);
//								}
//							}
//							break;
//						case "CBDK":
//							{
//								var it = new Cbdk();
//								it.ReadXml(xe);
//								lstCbdk.Add(it);
//							}
//							break;
//						case "CBDKXX":
//							{
//								var it = new Cbdkxx();
//								it.ReadXml(xe);
//								var djb = dicDjb.Find(a => { return a.Item1.CBFBM == it.CBFBM; }).Item2;
//								djb.Cbdkxx.Add(it);
//							}
//							break;
//						case "DKJZD":
//							{
//								foreach (var n in xe.ChildNodes)
//								{
//									if (n is XmlElement e)
//									{
//										var it = new Jzd();
//										it.ReadXml(e);
//										lstJzd.Add(it);
//									}
//								}
//							}
//							break;
//						case "DKJZX":
//							{
//								foreach (var n in xe.ChildNodes)
//								{
//									if (n is XmlElement e)
//									{
//										var it = new Jzx();
//										it.ReadXml(e);
//										lstJzx.Add(it);
//									}
//								}
//							}
//							break;
//						case "TYZB":
//							_p.ChangeData.TYZB = xe.InnerText;
//							break;
//					}
//				}
//			}

//			foreach (var t in dicDjb)
//			{
//				var od = t.Item1;
//				var nd = t.Item2;
//				CopyFromOld(od, nd);

//				foreach (var it in nd.Cbdkxx)
//				{
//					var dk = lstCbdk.Find(a => { return a.Dk?.DKBM == it.DKBM; });
//					if (dk != null)
//					{
//						nd.Cbdk.Add(dk);
//						lstCbdk.Remove(dk);
//						foreach (var jt in lstJzd)
//						{
//							if (jt.DKBM == dk.Dk?.DKBM) nd.DkJzd.Add(jt);
//						}
//						lstJzd.RemoveAll(a => { return a.DKBM == dk.Dk?.DKBM; });
//						foreach (var jt in lstJzx)
//						{
//							if (jt.DKBM == dk.Dk?.DKBM) nd.DkJzx.Add(jt);
//						}
//						lstJzx.RemoveAll(a => { return a.DKBM == dk.Dk?.DKBM; });
//					}
//				}

//				_p.ChangeData.lstDjb.Add(nd);
//			}
//		}

		protected override void ReadXmlExt(XmlElement xe)
		{
			switch (xe.Name)
			{
				case "JF":JF.ReadXml(xe);break;
				case "YF":YF.ReadXml(xe);break;
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			var ce=xml.CreateElement("JF");
			parent.AppendChild(ce);
			JF.WriteXml(xml, ce);
			ce = xml.CreateElement("YF");
			parent.AppendChild(ce);
			YF.WriteXml(xml, ce);
		}

		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			var task = _task;
			if (lstDjbID.Count == 0)
				return;
			if (lstDjbID.Count != 2)
			{
				task.ReportWarning("互换登记业务只能包含甲方和乙方两个两个登记簿");
				return;
			}
			string jfDJBID = lstDjbID[0].DJBID;
			string yfDJBID = lstDjbID[1].DJBID;
			Query(jfDJBID, JF);
			Query(yfDJBID, YF);
		}

		void Query(string djbID, JF jf)
		{
			var sql = $"select FBFBM, CBJYQZBM, CBFBM from DJ_CBJYQ_DJB where ID = '{djbID}'";
			_db.QueryCallback(sql, r =>
			 {
				 int i = -1;
				 jf.FBFBM = GetStr(r, ++i);
				 jf.CBJYQZBM = GetStr(r, ++i);
				 jf.CBFBM = GetStr(r, ++i);
				 return false;
			 });

			sql = $"select DKBM from DJ_CBJYQ_LZDK where HTID=(select ID from DJ_CBJYQ_LZHT where DJBID='{djbID}')";
			_db.QueryCallback(sql, r =>
			 {
				 var dkbm = r.GetString(0);
				 if (jf.DKBM == null) jf.DKBM = dkbm;
				 else jf.DKBM += $";{dkbm}";
				 return true;
			 });
		}
	}

	/// <summary>
	/// 转让登记-甲方
	/// </summary>
	class JF:XmlSerial
	{
		/// <summary>
		/// 地块代码(有多个地块代码时以分号隔开)
		/// </summary>
		public string DKBM;
		/// <summary>
		/// 发包方代码
		/// </summary>
		public string FBFBM;
		/// <summary>
		/// 承包经营权证（登记簿）代码
		/// </summary>
		public string CBJYQZBM;
		/// <summary>
		/// 承包方代码
		/// </summary>
		public string CBFBM;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "DKBM": DKBM = e.InnerText; break;
						case "FBFBM": FBFBM = e.InnerText; break;
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "CBFBM": CBFBM = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "DKBM", DKBM);
			AppendTextElementChild(xml, xn, "FBFBM", FBFBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
		}
	}
	/// <summary>
	/// 转让登记-乙方
	/// </summary>
	class YF:JF
	{
	}

	/// <summary>
	/// 分户登记业务数据
	/// </summary>
	class FhBusinessData : BGDJBusinessData
	{
		/// <summary>
		/// 分户前承包方代码
		/// </summary>
		public string FHQCBFBM;
		/// <summary>
		/// 分户前承包合同代码
		/// </summary>
		public string FHQCBHTBM;
		/// <summary>
		/// 分户前承包经营权证（登记簿）代码
		/// </summary>
		public string FHQCBJYQZBM;
		/// <summary>
		/// 分户后承包方数据
		/// </summary>
		public readonly List<FHHCBF> FHHCBF = new List<FHHCBF>();


		public override void FillEntities(DJ_YW_SLSQ en)
		{
			var p = _p;
			#region Fill entities.lstYDJB
			var sql = $"select ID from DJ_CBJYQ_DJB where QSZT={(int)EQszt.Xians} and CBJYQZBM='{p.OriginalData.lstDjb[0].CBJYQZBM}'";
			var sYDJBID=SafeConvertAux.ToStr(_db.QueryOne(sql));

			foreach (var djb in p.ChangeData.lstDjb)
			{
				var enYDJB1 = new DJ_CBJYQ_YDJB()
				{
					DJBID = p.ChangeData.lstDjb[0].DJBID,
					SLSQID =en.ID,
					DJXL = XmlExchangeUtil.SJLXToDjxl(p.Head.SJLX),
					YDJBID= sYDJBID
				};
				lstYDJB.Add(enYDJB1);
			}
			#endregion
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			foreach (var d in changeData.lstDjb)
			{
				d.CZFS = ECzfs.Zj;
				d.Cbjyqz.CZFS = ECzfs.Zj;
				d.Cbf.CZFS = ECzfs.Zj;
				foreach (var j in d.jtcies)
				{
					j.CZFS = ECzfs.Zj;
				}
				d.Cbht.CZFS = ECzfs.Zj;
			}
		}
		public override void ReadChangeData(XmlDocument xml, XmlNode parent)
		{
			_p.ReadData(xml, parent, _p.ChangeData);
		}

		protected override void ReadXmlExt(XmlElement e)
		{
			switch (e.Name)
			{
				case "FHQCBFBM": FHQCBFBM = e.InnerText; break;
				case "FHQCBHTBM": FHQCBHTBM = e.InnerText; break;
				case "FHQCBJYQZBM": FHQCBJYQZBM = e.InnerText; break;
				case "FHHCBF":
					{
						var it = new FHHCBF();
						it.ReadXml(e);
						FHHCBF.Add(it);
					}
					break;
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			AppendTextElementChild(xml, parent, "FHQCBFBM", FHQCBFBM);
			AppendTextElementChild(xml, parent, "FHQCBHTBM", FHQCBHTBM);
			AppendTextElementChild(xml, parent, "FHQCBJYQZBM", FHQCBJYQZBM);
			foreach (var it in FHHCBF)
			{
				var xe=xml.CreateElement("FHHCBF");
				parent.AppendChild(xe);
				it.WriteXml(xml, xe);
			}
		}

		protected override void DoQuery(ExportXmlPanel.Item pit, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			if (od.lstDjb.Count > 0)
			{
				var d = od.lstDjb[0];
				FHQCBFBM = d.Cbf?.CBFBM;
				FHQCBHTBM = d.Cbht?.CBHTBM;
				FHQCBJYQZBM = d.Cbjyqz?.CBJYQZBM;//.Find(a => { return !a.SFYZX; })?.CBJYQZBM;
			}
			foreach (var it in cd.lstDjb)
			{
				if (it.Cbf != null)
				{
					var hcbf = new FHHCBF()
					{
						CBFBM = it.CBFBM,
						CBHTBM = it.CBHTBM,
						CBJYQZBM = it.CBHTBM
					};
					foreach (var jt in it.Cbdkxx)
					{
						if (jt.CBFBM == it.CBFBM)
						{
							if (!hcbf.DKBM.Contains(jt.DKBM))
							{
								hcbf.DKBM.Add(jt.DKBM);
							}
						}
					}

					FHHCBF.Add(hcbf);
				}
			}


		}
	}
	/// <summary>
	/// 分户业务分户后承包方表
	/// </summary>
	class FHHCBF:XmlSerial
	{
		public string CBFBM;
		public string CBHTBM;
		/// <summary>
		/// 承包经营权证（登记簿）代码
		/// </summary>
		public string CBJYQZBM;

		/// <summary>
		/// 多个地块编码用分号隔开
		/// </summary>
		public readonly List<string> DKBM=new List<string>();

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBHTBM": CBHTBM = e.InnerText; break;
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "DKBM": DKBM.Add(e.InnerText); break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			foreach (var dkbm in DKBM)
			{
				AppendTextElementChild(xml, xn, "DKBM", dkbm);
			}
		}
	}
	/// <summary>
	/// 合户登记业务数据
	/// </summary>
	class HehBusinessData : BGDJBusinessData
	{
		/// <summary>
		/// 合户前承包方数据
		/// </summary>
		public readonly List<HHQCBF> HHQCBF = new List<HHQCBF>();
		/// <summary>
		/// 合户后承包方数据
		/// </summary>
		public readonly HHHCBF HHHCBF = new HHHCBF();

		public override void FillEntities(DJ_YW_SLSQ en)// Entities entities)
		{
			var p = _p;
			if (p.ChangeData.lstDjb.Count != 1)
			{
				_task.ReportError($"CHANGE_DATA节点下的CBJYQZDJB个数为{p.ChangeData.lstDjb.Count}，针对合户登记应为1");
				return;
			}
			#region Fill entities.lstYDJB
			var djbID = p.ChangeData.lstDjb[0].DJBID;
			var djxl = XmlExchangeUtil.SJLXToDjxl(p.Head.SJLX);

			var sql = $"select ID from DJ_CBJYQ_DJB where QSZT={(int)EQszt.Xians} and CBJYQZBM in(";
			foreach (var djb in p.OriginalData.lstDjb)
			{
				sql += $"'{djb.CBJYQZBM}',";
			}
			sql = sql.TrimEnd(',') + ")";
			_db.QueryCallback(sql, r =>
			{
				var sYDJBID = r.GetString(0);
				var enYDJB1 = new DJ_CBJYQ_YDJB()
				{
					DJBID = djbID,
					SLSQID = en.ID,
					DJXL = djxl,
					YDJBID = sYDJBID
				};
				lstYDJB.Add(enYDJB1);
				return true;
			});

			#endregion
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			foreach (var d in changeData.lstDjb)
			{
				d.CZFS = ECzfs.Zj;
				d.Cbjyqz.CZFS = ECzfs.Zj;
				d.Cbf.CZFS = ECzfs.Zj;
				foreach (var j in d.jtcies)
				{
					j.CZFS = ECzfs.Zj;
				}
				d.Cbht.CZFS = ECzfs.Zj;
			}
		}
		public override void ReadChangeData(XmlDocument xml, XmlNode parent)
		{
			_p.ReadData(xml, parent, _p.ChangeData);
		}
		protected override void DoQuery(ExportXmlPanel.Item pit, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			foreach (var it in od.lstDjb)
			{
				var qcbf = new HHQCBF()
				{
					CBFBM = it.CBFBM,
					CBHTBM = it.CBHTBM,
					CBJYQZBM = it.CBJYQZBM,
				};
				HHQCBF.Add(qcbf);
			}

			if (cd.lstDjb.Count == 1)
			{
				var d = cd.lstDjb[0];
				HHHCBF.CBFBM = d.CBFBM;
				HHHCBF.CBHTBM = d.CBHTBM;
				HHHCBF.CBJYQZBM = d.CBJYQZBM;

				foreach (var it in d.Cbdk)
				{
					if (HHHCBF.DKBM == null) HHHCBF.DKBM = it.Dk.DKBM;
					else HHHCBF.DKBM += $";{it.Dk.DKBM}";
				}
			}
		}
		protected override void ReadXmlExt(XmlElement xe)
		{
			switch (xe.Name)
			{
				case "HHQCBF":
					var it = new HHQCBF();
					HHQCBF.Add(it);
					it.ReadXml(xe);
					break;
				case "HHHCBF":
					HHHCBF.ReadXml(xe);
					break;
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			foreach (var it in HHQCBF)
			{
				var xe = xml.CreateElement("HHQCBF");
				parent.AppendChild(xe);
				it.WriteXml(xml, xe);
			}
			var xe1 = xml.CreateElement("HHHCBF");
			parent.AppendChild(xe1);
			HHHCBF.WriteXml(xml, xe1);
		}
	}
	/// <summary>
	/// 合户业务合户前承包方表
	/// </summary>
	class HHQCBF:XmlSerial
	{
		public string CBFBM;
		public string CBHTBM;
		public string CBJYQZBM;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBHTBM": CBHTBM = e.InnerText; break;
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
		}
	}
	/// <summary>
	/// 合户业务合户后承包方表
	/// </summary>
	class HHHCBF: XmlSerial
	{
		public string CBFBM;
		public string CBHTBM;
		public string CBJYQZBM;
		/// <summary>
		/// 多个地块逗号分隔
		/// </summary>
		public string DKBM;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBFBM": CBFBM = e.InnerText; break;
						case "CBHTBM": CBHTBM = e.InnerText; break;
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "DKBM": DKBM = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBFBM", CBFBM);
			AppendTextElementChild(xml, xn, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "DKBM", DKBM);
		}
	}
	/// <summary>
	/// 注销登记业务数据
	/// </summary>
	class ZxBusinessData : BusinessData
	{
		/// <summary>
		/// 注销原因
		/// </summary>
		public string ZXYY;

		public string CBFBM;
		public string CBHTBM;
		public string CBJYQZBM;

		/// <summary>
		/// 被注销的登记簿ID
		/// </summary>
		private string DJBID;

		public override bool HasOriginData()
		{
			return false;
		}
		public override void FillEntities(DJ_YW_SLSQ en)
		{
			en.DJYY = ZXYY;

			var sql = $"select ID from DJ_CBJYQ_DJB where QSZT={(int)EQszt.Xians} and CBJYQZBM='{CBJYQZBM}'";
			DJBID =SafeConvertAux.ToStr(_db.QueryOne(sql));
		}
		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			WriteDJ_YW_SQQL(enSLSQ.ID, DJBID);
			var sql = $"update DJ_CBJYQ_DJB set QSZT={(int)EQszt.History} where ID='{DJBID}'";
			_db.ExecuteNonQuery(sql);
		}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			if (cd.lstDjb.Count != 1)
			{
				_task.ReportError("业务数据有误，注销的登记簿数量不为1");
				return;
			}
			ZXYY = it.DJYY;
			//od.lstDjb.Clear();
			var djb = cd.lstDjb[0];
			CBFBM = djb.CBFBM;
			CBHTBM = djb.CBHTBM;
			CBJYQZBM = djb.CBJYQZBM;
			cd.lstDjb.Clear();
		}
		protected override void ReadXmlExt(XmlElement xe)
		{
			switch (xe.Name)
			{
				case "ZXYY": ZXYY = xe.InnerText; break;
				case "CBFBM": CBFBM = xe.InnerText; break;
				case "CBHTBM": CBHTBM = xe.InnerText; break;
				case "CBJYQZBM": CBJYQZBM = xe.InnerText; break;
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			AppendTextElementChild(xml, parent, "ZXYY", ZXYY);
			AppendTextElementChild(xml, parent, "CBFBM", CBFBM);
			AppendTextElementChild(xml, parent, "CBHTBM", CBHTBM);
			AppendTextElementChild(xml, parent, "CBJYQZBM", CBJYQZBM);
		}
	}

	/// <summary>
	/// 权证补发
	/// </summary>
	class QzbfBusinessData : QZBusinessData
	{
		public QZBF QZBF;
		private DJ_CBJYQ_QZBF enQZBF;
		public override void FillEntities(DJ_YW_SLSQ en)
		{
			base.FillEntities(en);
			var p = _p;
			if (p.ChangeData.lstDjb.Count != 1)
			{
				_task.ReportError("Xml格式有误");
				return;
			}
			var djb = p.ChangeData.lstDjb[0];
			enQZBF = new DJ_CBJYQ_QZBF(QZBF)
			{
				QZID = djb.Cbjyqz.ID,//[0].ID,
				SQID = p.Head.SQID,
				YWH=YWLSH,
				DJBID=djb.DJBID
			};
		}
		public override void CheckExport(OriginalData od, OriginalData cd)
		{
			var djb=cd.lstDjb[0];
			djb.ExportJcties = false;
			djb.ExportXml = false;
			djb.Fbf.ExportXml = false;
			djb.Cbf.ExportXml = false;
			djb.Cbht.ExportXml = false;
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			var djb = changeData.lstDjb[0];
			djb.Cbjyqz.CZFS = ECzfs.Zj;
		}
		public override void ReadChangeData(XmlDocument xml, XmlNode parent)
		{
			var xn=parent.SelectSingleNode("CBJYQZ");
			var djb = new Cbjyqzdjb
			{
				Cbjyqz = new Cbjyqz()
			};
			djb.Cbjyqz.ReadXml(xn);
			_p.ChangeData.lstDjb.Add(djb);
		}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			base.DoQuery(it, lstDjbID, od, cd);
			var sql = $"select CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM from DJ_CBJYQ_QZBF where YWH='{it.Ywh}'";
			_db.QueryCallback(sql, r =>
			{
				int i = -1;
				QZBF = new QZBF()
				{
					CBJYQZBM = GetStr(r, ++i),
					QZBFYY = GetStr(r, ++i),
					BFRQ = GetDate(r, ++i),
					QZBFLQRQ = GetDate(r, ++i),
					QZBFLQRXM = GetStr(r, ++i),
					BFLQRZJLX = GetStr(r, ++i),
					BFLQRZJHM = GetStr(r, ++i),
				};
				return false;
			});

			//cd.lstDjb.Clear();
		}

		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			base.DoImport(enSLSQ);
			var a = _db.SqlFunc.GetParamPrefix();
			var b = enQZBF;
			var bf = b.qZBF;
			var sqlPrms = new List<SQLParam>();
			var fields = "ID,QZID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM,SQID,YWH,DJBID";
			var saFields = fields.Split(',');
			var sql = $"insert into DJ_CBJYQ_QZBF({fields}) values(";
			foreach (var field in saFields)
			{
				sql += $"{a}{field},";

				object o = null;
				switch (field)
				{
					case "ID": o = b.ID; break;
					case "QZID": o = b.QZID; break;
					case "CBJYQZBM": o = bf.CBJYQZBM; break;
					case "QZBFYY": o = bf.QZBFYY; break;
					case "BFRQ": o = bf.BFRQ; break;
					case "QZBFLQRQ": o = bf.QZBFLQRQ; break;
					case "QZBFLQRXM": o = bf.QZBFLQRXM; break;
					case "BFLQRZJLX": o = bf.BFLQRZJLX; break;
					case "BFLQRZJHM": o = bf.BFLQRZJHM; break;
					case "SQID": o = b.SQID; break;
					case "YWH": o = b.YWH; break;
					case "DJBID": o = b.DJBID; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
		}

		protected override void ReadXmlExt(XmlElement xe)
		{
			base.ReadXmlExt(xe);
			if (xe.Name == "QZBF")
			{
				QZBF = new QZBF();
				QZBF.ReadXml(xe);
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			base.WriteXmlExt(xml, parent);
			if (QZBF != null)
			{
				var xe = xml.CreateElement("QZBF");
				parent.AppendChild(xe);
				QZBF.WriteXml(xml, xe);
			}
		}
	}
	class QZBF : XmlSerial
	{
		public string CBJYQZBM;
		/// <summary>
		/// 权证补发原因
		/// </summary>
		public string QZBFYY;
		/// <summary>
		/// 补发日期
		/// </summary>
		public DateTime? BFRQ;
		/// <summary>
		/// 权证补发领取日期
		/// </summary>
		public DateTime? QZBFLQRQ;
		/// <summary>
		/// 权证补发领取人姓名
		/// </summary>
		public string QZBFLQRXM;
		/// <summary>
		/// 权证补发领取人证件类型
		/// </summary>
		public string BFLQRZJLX;
		/// <summary>
		/// 权证补发领取人证件号码
		/// </summary>
		public string BFLQRZJHM;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "QZBFYY": QZBFYY = e.InnerText; break;
						case "BFRQ": if(DateTime.TryParse(e.InnerText,out DateTime dt)) BFRQ=dt; break;
						case "QZBFLQRQ": if(DateTime.TryParse(e.InnerText,out dt)) QZBFLQRQ=dt; break;
						case "QZBFLQRXM": QZBFLQRXM = e.InnerText; break;
						case "BFLQRZJLX": BFLQRZJLX = e.InnerText; break;
						case "BFLQRZJHM": BFLQRZJHM = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "QZBFYY", QZBFYY);
			AppendTextElementChild(xml, xn, "BFRQ", BFRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "QZBFLQRQ", QZBFLQRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "QZBFLQRXM", QZBFLQRXM);
			AppendTextElementChild(xml, xn, "BFLQRZJLX", BFLQRZJLX);
			AppendTextElementChild(xml, xn, "BFLQRZJHM", BFLQRZJHM);
		}
	}
	/// <summary>
	/// 权证换发
	/// </summary>
	class QzhfBusinessData : QZBusinessData
	{
		public QZHF QZHF;
		private DJ_CBJYQ_QZHF enQZHF;
		public override void FillEntities(DJ_YW_SLSQ en)
		{
			base.FillEntities(en);
			var p = _p;
			if (p.ChangeData.lstDjb.Count != 1)
			{
				_task.ReportError("Xml格式有误");
				return;
			}
			var djb = p.ChangeData.lstDjb[0];
			enQZHF = new DJ_CBJYQ_QZHF(QZHF)
			{
				QZID = djb.Cbjyqz.ID,
				SQID = p.Head.SQID,
				YWH = YWLSH,
				DJBID = djb.DJBID
			};
		}

		public override void CheckExport(OriginalData od, OriginalData cd)
		{
			var djb = cd.lstDjb[0];
			djb.ExportJcties = false;
			djb.ExportXml = false;
			djb.Fbf.ExportXml = false;
			djb.Cbf.ExportXml = false;
			djb.Cbht.ExportXml = false;
		}
		public override void ModifyCzfs(OriginalData changeData)
		{
			var djb = changeData.lstDjb[0];
			djb.Cbjyqz.CZFS = ECzfs.Zj;
		}
		public override void ReadChangeData(XmlDocument xml, XmlNode parent)
		{
			var xn = parent.SelectSingleNode("CBJYQZ");
			var djb = new Cbjyqzdjb
			{
				Cbjyqz = new Cbjyqz()
			};
			djb.Cbjyqz.ReadXml(xn);
			_p.ChangeData.lstDjb.Add(djb);
		}
		protected override void DoQuery(ExportXmlPanel.Item it, List<DJBIDPair> lstDjbID, OriginalData od, OriginalData cd)
		{
			base.DoQuery(it, lstDjbID, od, cd);
			var sql = $"select CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM from DJ_CBJYQ_QZHF where YWH='{it.Ywh}'";
			_db.QueryCallback(sql, r =>
			{
				int i = -1;
				QZHF = new QZHF()
				{
					CBJYQZBM = GetStr(r, ++i),
					QZHFYY = GetStr(r, ++i),
					HFRQ = GetDate(r, ++i),
					QZHFLQRQ = GetDate(r, ++i),
					QZHFLQRXM = GetStr(r, ++i),
					HFLQRZJLX = GetStr(r, ++i),
					HFLQRZJHM = GetStr(r, ++i),
				};
				return false;
			});
		}
		protected override void DoImport(DJ_YW_SLSQ enSLSQ)
		{
			base.DoImport(enSLSQ);
			var a = _db.SqlFunc.GetParamPrefix();
			var b = enQZHF;
			var bf = b.qZHF;
			var sqlPrms = new List<SQLParam>();
			var fields = "ID,QZID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM,SQID,YWH,DJBID";
			var sql = $"insert into DJ_CBJYQ_QZHF({fields}) values(";
			foreach (var field in fields.Split(','))
			{
				sql += $"{a}{field},";

				object o = null;
				switch (field)
				{
					case "ID": o = b.ID; break;
					case "QZID": o = b.QZID; break;
					case "CBJYQZBM": o = bf.CBJYQZBM; break;
					case "QZBFYY": o = bf.QZHFYY; break;
					case "BFRQ": o = bf.HFRQ; break;
					case "QZBFLQRQ": o = bf.QZHFLQRQ; break;
					case "QZBFLQRXM": o = bf.QZHFLQRXM; break;
					case "BFLQRZJLX": o = bf.HFLQRZJLX; break;
					case "BFLQRZJHM": o = bf.HFLQRZJHM; break;
					case "SQID": o = b.SQID; break;
					case "YWH": o = b.YWH; break;
					case "DJBID": o = b.DJBID; break;
				}
				var prm = new SQLParam()
				{
					ParamName = field,
					ParamValue = o
				};
				sqlPrms.Add(prm);
			}
			sql = sql.TrimEnd(',') + ")";
			_db.ExecuteNonQuery(sql, sqlPrms);
		}
		protected override void ReadXmlExt(XmlElement xe)
		{
			base.ReadXmlExt(xe);
			if (xe.Name == "QZHF")
			{
				QZHF = new QZHF();
				QZHF.ReadXml(xe);
			}
		}
		protected override void WriteXmlExt(XmlDocument xml, XmlNode parent)
		{
			base.WriteXmlExt(xml, parent);
			if (QZHF != null)
			{
				var xe = xml.CreateElement("QZHF");
				parent.AppendChild(xe);
				QZHF.WriteXml(xml, xe);
			}
		}
	}
	class QZHF : XmlSerial
	{
		public string CBJYQZBM;
		public string QZHFYY;
		public DateTime? HFRQ;
		public DateTime? QZHFLQRQ;
		public string QZHFLQRXM;
		public string HFLQRZJLX;
		public string HFLQRZJHM;

		public override void ReadXml(XmlNode xn)
		{
			foreach (var o in xn.ChildNodes)
			{
				if (o is XmlElement e)
				{
					switch (e.Name)
					{
						case "CBJYQZBM": CBJYQZBM = e.InnerText; break;
						case "QZHFYY": QZHFYY = e.InnerText; break;
						case "HFRQ": if(DateTime.TryParse(e.InnerText,out DateTime dt)) HFRQ=dt; break;
						case "QZHFLQRQ": if(DateTime.TryParse(e.InnerText,out dt)) QZHFLQRQ=dt; break;
						case "QZHFLQRXM": QZHFLQRXM = e.InnerText; break;
						case "HFLQRZJLX": HFLQRZJLX = e.InnerText; break;
						case "HFLQRZJHM": HFLQRZJHM = e.InnerText; break;
					}
				}
			}
		}
		public override void WriteXml(XmlDocument xml, XmlNode xn)
		{
			AppendTextElementChild(xml, xn, "CBJYQZBM", CBJYQZBM);
			AppendTextElementChild(xml, xn, "QZHFYY", QZHFYY);
			AppendTextElementChild(xml, xn, "HFRQ", HFRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "QZHFLQRQ", QZHFLQRQ?.ToString("yyyy-MM-dd"));
			AppendTextElementChild(xml, xn, "QZHFLQRXM", QZHFLQRXM);
			AppendTextElementChild(xml, xn, "HFLQRZJLX", HFLQRZJLX);
			AppendTextElementChild(xml, xn, "HFLQRZJHM", HFLQRZJHM);
		}
	}

	class ECzfs {
		/// <summary>
		/// 无操作
		/// </summary>
		public const string WCZ = "01";
		/// <summary>
		/// 增加
		/// </summary>
		public const string Zj = "02";
		/// <summary>
		/// 修改
		/// </summary>
		public const string Xg = "03";
		/// <summary>
		/// 删除
		/// </summary>
		public const string Sc = "04";
	}
}
