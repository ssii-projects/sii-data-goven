/*
yxm created at 2019/5/5 14:04:53
*/
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// 批量数据更新任务（相对于使用B/S系统走业务流程的方式，可以一次性导入业务变更数据）
	/// 使用场景：
	///		作业单位已经完成了汇交数据导入，在这之后作业单位又调查了一些变更数据，这些数据存储在作业单位内部的系统中，
	///		该功能的目的就是定义一个类似汇交格式的数据标准，作业单位将他们的变更数据转换为这个标准，然后导入到我们的
	///		系统中。（作业单位的这些数据也可以通过B/S系统走业务流程的方式逐条录入到系统中，只是这样做数据录入效率不高）；
	/// </summary>
	class DataUpdateTask : Task
	{
		class DjbItem
		{
			public string ID;
			public string CBJYQZBM;
			public string FBFBM;
			public string CBFBM;
			public string CBFS;
			public DateTime? CBQXQ;
			public DateTime? CBQXZ;
			public string DKSYT;
			public string CBQX;
			public string YCBJYQZBH;
			public string CBJYQZLSH;
			public string DJBFJ;
			public string DBR;
			public DateTime? DJSJ;
			public string CBFMC
			{
				get
				{
					return cbhtItem?.cbfItem?.CBFMC;
				}
			}

			public CbjyqzItem cbjyqzItem;
			public CbhtItem cbhtItem;

			///// <summary>
			///// 在CBJYQZDJB中但不在承包合同中的承包方
			///// </summary>
			//public CbfItem cbfItem;

			#region 检查是否已经导入了（通过数据比较判断）
			internal bool HasImported;

			internal bool AllImported()
			{
				if (!HasImported)
				{
					return false;
				}
				if (cbjyqzItem != null && cbjyqzItem.HasImported==false)
				{
					return false;
				}
				if (cbhtItem != null && !cbhtItem.AllImported())
				{
					return false;
				}
				return true;
			}
			#endregion
		}
		class CbjyqzItem
		{
			public string CBJYQZBM;
			public string FZJG;
			public DateTime? FZRQ;
			public string QZSFLQ;
			public DateTime? QZLQRQ;
			public string QZLQRXM;
			public string QZLQRZJLX;
			public string QZLQRZJHM;

			#region DJ_CBJYQ_QZ附加字段
			/// <summary>
			/// 省区市的简称
			/// </summary>
			public string SQSJC;
			/// <summary>
			/// 颁证年份
			/// </summary>
			public int BZNF;
			/// <summary>
			/// 年度顺序号
			/// </summary>
			public int NDSXH;
			#endregion

			public DjbItem parent;

			#region 检查是否已经导入了（通过数据比较判断）
			internal bool HasImported;
			#endregion
		}
		class CbhtItem
		{
			public string CBHTBM;
			public string YCBHTBM;
			public string FBFBM;
			public string CBFBM;
			public string CBFS;
			public DateTime? CBQXQ;
			public DateTime? CBQXZ;
			public double HTZMJ;
			public int CBDKZS;
			public DateTime? QDSJ;
			public double HTZMJM;
			public double YHTZMJ;
			public double YHTZMJM;

			public CbfItem cbfItem;
			public readonly List<DkxxItem> dkxxItems = new List<DkxxItem>();

			public DjbItem parent;

			#region 检查是否已经导入了（通过数据比较判断）
			internal bool HasImported;
			/// <summary>
			/// 承包地块信息是否已导入
			/// </summary>
			internal bool DkxxHasImported;
			/// <summary>
			/// 矢量地块是否已导入
			/// </summary>
			internal bool VecDkHasImported;
			internal void CheckImported(Temp t)
			{
				var db = t._db;
				var sql = $"select {t._cbht._mdbFields} from QSSJ_CBHT where CBHTBM='{CBHTBM}'";
				db.QueryCallback(sql, r =>
				 {
					 HasImported = true;
					 var it = new CbhtItem()
					 {
						 YCBHTBM = Util.GetString(r, 1),
						 FBFBM = Util.GetString(r, 2),
						 CBFBM = Util.GetString(r, 3),
						 CBFS = Util.GetString(r, 4),
						 CBQXQ = Util.GetDateTime(r, 5),
						 CBQXZ = Util.GetDateTime(r, 6),
						 HTZMJ = Util.GetDouble(r, 7),
						 CBDKZS = Util.GetInt(r, 8),
						 QDSJ = Util.GetDateTime(r, 9),
						 HTZMJM = Util.GetDouble(r, 10),
						 YHTZMJ = Util.GetDouble(r, 11),
						 YHTZMJM = Util.GetDouble(r, 12),
					 };
					 if (!StringUtil.isEqual(it.YCBHTBM, YCBHTBM) || !StringUtil.isEqual(it.FBFBM, FBFBM) || !StringUtil.isEqual(it.CBFBM, CBFBM)
						 || !StringUtil.isEqual(it.CBFS, CBFS) || !Util.Equal(it.CBQXQ, CBQXQ) || !Util.Equal(it.CBQXZ, CBQXZ)
						 || !Util.Equal(it.HTZMJ, HTZMJ) || it.CBDKZS != CBDKZS || !Util.Equal(it.QDSJ, QDSJ) || !Util.Equal(it.HTZMJM, HTZMJM)
						 || !Util.Equal(it.YHTZMJ, YHTZMJ) || !Util.Equal(it.YHTZMJM, YHTZMJM))
					 {
						 HasImported = false;
					 }
					 return false;
				 });
				cbfItem?.CheckImported(db, t);
				dkxxItems.Sort((a, b) => {
					if (a.DKBM == b.DKBM) return Util.Compare(a.CBFBM, b.CBFBM);
					return a.DKBM.CompareTo(b.DKBM); });

				var lst = new List<DkxxItem>();
				sql = $"select {t._cbdkxx._mdbFields} from QSSJ_CBDKXX where CBHTBM='{CBHTBM}'";
				db.QueryCallback(sql, r =>
				 {
					 var it = new DkxxItem()
					 {
						 CBJYQZBM = Util.GetString(r, 0),
						 DKBM = Util.GetString(r, 1),
						 FBFBM = Util.GetString(r, 2),
						 CBFBM = Util.GetString(r, 3),
						 CBJYQQDFS = Util.GetString(r, 4),
						 HTMJ = Util.GetDouble(r, 5),
						 LZHTBM = Util.GetString(r, 6),
						 CBHTBM = Util.GetString(r, 7),
						 YHTMJ = Util.GetDouble(r, 8),
						 HTMJM = Util.GetDouble(r, 9),
						 YHTMJM = Util.GetDouble(r, 10),
						 SFQQQG = Util.GetString(r, 11),
					 };
					 lst.Add(it);
					 return true;
				 });
				if (lst.Count == dkxxItems.Count)
				{
					DkxxHasImported = true;
					lst.Sort((a, b) => {
						if (a.DKBM == b.DKBM) return Util.Compare(a.CBFBM, b.CBFBM);
						return a.DKBM.CompareTo(b.DKBM); });
					for (int i = 0; i < lst.Count; ++i)
					{
						if (!lst[i].Equal(dkxxItems[i]))
						{
							DkxxHasImported = false;
							break;
						}
					}
				}

				VecDkHasImported = true;
				foreach (var it in dkxxItems)
				{
					if (t._dkCache.Dic.TryGetValue(it.DKBM, out VecDkItem vit))
					{
						vit.CheckImported(t);
						if (vit.HasImported == false)
						{
							VecDkHasImported = false;
							break;
						}
					}
					else
					{
						throw new Exception($"CBDKXX中DKBM={it.DKBM}的记录在矢量文件中无对应记录");
					}
				}
			}

			internal bool AllImported()
			{
				var fOK= HasImported && DkxxHasImported && VecDkHasImported;
				if (cbfItem != null && !cbfItem.AllImported())
				{
					return false;
				}
				return fOK;
			}
			#endregion
		}
		class CbfItem
		{
			public string CBFBM;
			public string CBFLX;
			public string CBFMC;
			public string CBFZJLX;
			public string CBFZJHM;
			public string CBFDZ;
			public string YZBM;
			public string LXDH;
			public int CBFCYSL;
			public DateTime? CBFDCRQ;
			public string CBFDCY;
			public string CBFDCJS;
			public string GSJS;
			public string GSJSR;
			public DateTime? GSSHRQ;
			public string GSSHR;
			public int ZT;


			public CbhtItem parent;
			public readonly List<CbfJtcyItem> cbfJtcyItems = new List<CbfJtcyItem>();

			#region 检查是否已经导入了（通过数据比较判断）
			internal bool HasImported;
			/// <summary>
			/// 承包方家庭成员是否已导入
			/// </summary>
			internal bool CbfJtchHasImported;
			internal void CheckImported(IFeatureWorkspace db, Temp t)
			{
				var sql = $"select {t._cbf._mdbFields} from QSSJ_CBF where CBFBM='{CBFBM}'";
				db.QueryCallback(sql, r =>
				{
					HasImported = true;
					var it = new CbfItem()
					{
						CBFLX = Util.GetString(r, 1),
						CBFMC = Util.GetString(r, 2),
						CBFZJLX = Util.GetString(r, 3),
						CBFZJHM = Util.GetString(r, 4),
						CBFDZ = Util.GetString(r, 5),
						YZBM = Util.GetString(r, 6),
						LXDH = Util.GetString(r, 7),
						CBFCYSL = SafeConvertAux.ToInt32(r.GetValue(8)),
						CBFDCRQ = Util.GetDateTime(r, 9),
						CBFDCY = Util.GetString(r, 10),
						CBFDCJS = Util.GetString(r, 11),
						GSJS = Util.GetString(r, 12),
						GSJSR = Util.GetString(r, 13),
						GSSHRQ = Util.GetDateTime(r, 14),
						GSSHR = Util.GetString(r, 15),
					};
					if (!StringUtil.isEqual(it.CBFLX, CBFLX) || !StringUtil.isEqual(it.CBFMC, CBFMC) || !StringUtil.isEqual(it.CBFZJLX, CBFZJLX)
						|| !StringUtil.isEqual(it.CBFZJHM, CBFZJHM) || !Util.Equal(it.CBFDZ, CBFDZ) || !Util.Equal(it.YZBM, YZBM)
						|| !Util.Equal(it.LXDH, LXDH) || it.CBFCYSL != CBFCYSL || !Util.Equal(it.CBFDCRQ, CBFDCRQ) || !Util.Equal(it.CBFDCY, CBFDCY)
						|| !Util.Equal(it.CBFDCJS, CBFDCJS) || !Util.Equal(it.GSJS, GSJS) || !Util.Equal(it.GSJSR, GSJSR) || !Util.Equal(it.GSSHRQ, GSSHRQ)
						 || !Util.Equal(it.GSSHR, GSSHR))
					{
						HasImported = false;
					}
					return false;
				});
				cbfJtcyItems.Sort((a, b) =>
				{
					if (a.CYXM == b.CYXM) {
						return Util.Compare(a.CYZJHM, b.CYZJHM);
					}
					return a.CYXM.CompareTo(b.CYXM);
				});
				sql = $"select {t._cbfJtcy._mdbFields} from QSSJ_CBF_JTCY where CBFBM='{CBFBM}'";

				var lst = new List<CbfJtcyItem>();
				db.QueryCallback(sql, r =>
				 {
					 var it = new CbfJtcyItem()
					 {
						 CBFBM = r.GetString(0),
						 CYXM = Util.GetString(r, 1),
						 CYXB = Util.GetString(r, 2),
						 CYZJLX = Util.GetString(r, 3),
						 CYZJHM = Util.GetString(r, 4),
						 YHZGX = Util.GetString(r, 5),
						 CYBZ = Util.GetString(r, 6),
						 SFGYR = Util.GetString(r, 7),
						 CYBZSM = Util.GetString(r, 8),
					 };
					 lst.Add(it);
					 return true;
				 });
				if (lst.Count == cbfJtcyItems.Count)
				{
					CbfJtchHasImported = true;
					lst.Sort((a, b) =>
					{
						if (a.CYXM == b.CYXM)
						{
							return Util.Compare(a.CYZJHM, b.CYZJHM);
						}
						return a.CYXM.CompareTo(b.CYXM);
					});
					for (int i = 0; i < lst.Count; ++i)
					{
						if (!lst[i].Equal(cbfJtcyItems[i]))
						{
							CbfJtchHasImported = false;
							break;
						}
					}
				}
			}

			internal bool AllImported()
			{
				return HasImported && CbfJtchHasImported;
			}
			#endregion
		}
		class CbfJtcyItem
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
			public CbfItem parent;
			public bool Equal(CbfJtcyItem rhs)
			{
				return Util.Equal(CBFBM, rhs.CBFBM) && Util.Equal(CYXM, rhs.CYXM) && Util.Equal(CYXB, rhs.CYXB)
					&& Util.Equal(CYZJLX, rhs.CYZJLX) && Util.Equal(CYZJHM, rhs.CYZJHM) && Util.Equal(YHZGX, rhs.YHZGX)
					&& Util.Equal(CYBZ, rhs.CYBZ)&& Util.Equal(SFGYR, rhs.SFGYR) && Util.Equal(CYBZSM, rhs.CYBZSM);
			}
		}

		class DkxxItem
		{
			public string CBJYQZBM;
			public string DKBM;
			public string FBFBM;
			public string CBFBM;
			/// <summary>
			/// 承包经营权取得方式
			/// </summary>
			public string CBJYQQDFS;
			public double HTMJ;
			public string LZHTBM;
			public string CBHTBM;
			/// <summary>
			/// 原合同面积（二轮合同面积）
			/// </summary>
			public double YHTMJ;
			/// <summary>
			/// 合同面积(确权面积)
			/// </summary>
			public double HTMJM;
			public double YHTMJM;//,SFQQQG

			/// <summary>
			/// 是否确权确股
			/// </summary>
			public string SFQQQG;

			public CbhtItem parent;
			internal bool Equal(DkxxItem rhs)
			{
				return Util.Equal(CBJYQZBM, rhs.CBJYQZBM) && Util.Equal(DKBM, rhs.DKBM) && Util.Equal(FBFBM, rhs.FBFBM)
					&& Util.Equal(CBJYQQDFS, rhs.CBJYQQDFS) && Util.Equal(HTMJ, rhs.HTMJ) && Util.Equal(LZHTBM, rhs.LZHTBM)
					&& Util.Equal(CBHTBM, rhs.CBHTBM) && Util.Equal(YHTMJ, rhs.YHTMJ) && Util.Equal(HTMJM, rhs.HTMJM)
					&& Util.Equal(YHTMJM, rhs.YHTMJM);
			}
		}
		class VecDkItem
		{
			public int OID;
			public IGeometry Shape;
			public string DKBM;
			public short SFJBNT;
			public double SCMJ;
			public double SCMJM;
			public string SYQXZ;
			public string DKLB;
			public string TDLYLX;
			public string DLDJ;
			public string TDYT;
			public string DKDZ;
			public string DKXZ;
			public string DKNZ;
			public string DKBZ;
			public string ZJRXM;
			public string DKMC;
			public string DKBZXX;

			public ETXBGLX BGLX;
			/// <summary>
			/// 变更原因
			/// </summary>
			public string BGYY;
			/// <summary>
			/// 原地块编码
			/// </summary>
			public string YDKBM;
			///// <summary>
			///// 上传标志
			///// </summary>
			//public short SCBZ;

			#region  not shapefile field only for Database
			public string ID,FBFBM,CBFMC,YDKID;

			/// <summary>
			/// 数据来源
			/// （0/空：初始导入，1：新增，2：修改，3：拆分，4：合并）
			/// </summary>
			public ESjly SJLY;

			public EDjzt DJZT=EDjzt.Wdj;
			public DateTime? DJSJ;

			public string SFQQQG;
			/// <summary>
			/// 原合同面积（二轮合同面积）
			/// </summary>
			public double YHTMJ;
			/// <summary>
			/// 确权面积（合同面积亩）
			/// </summary>
			public double QQMJ;
			//public DkxxItem dkxx;// = new DkxxItem();
			//public bool HasImported = false;
			#endregion

			#region 检查是否已经导入了（通过数据比较判断）
			internal bool? HasImported=null;
			internal void CheckImported(Temp t)
			{
				if (HasImported == null)
				{
					HasImported=t._DLXX_DK.HasImported(this);
				}
			}
			#endregion

			public VecDkItem Clone()
			{
				var it = new VecDkItem
				{
					OID = this.OID,
					DJSJ=DJSJ,
					DJZT=DJZT,
					ID=ID,
					BGLX=BGLX,
					BGYY=BGYY,
					CBFMC=CBFMC,
					DKBM=DKBM,
					DKBZ=DKBZ,
					DKBZXX=DKBZXX,
					DKDZ=DKDZ,
					DLDJ=DLDJ,
					//dkxx=dkxx,
					QQMJ=QQMJ,
					YHTMJ=YHTMJ,
					SFQQQG=SFQQQG,
					SCMJ=SCMJ,
					SCMJM=SCMJM,
					SFJBNT=SFJBNT,
					ZJRXM=ZJRXM,
					DKLB=DKLB,
					DKMC=DKMC,
					DKNZ=DKNZ,
					DKXZ=DKXZ,
					FBFBM=FBFBM,
					Shape=Shape,
					SYQXZ=SYQXZ,
					//SCBZ=SCBZ,
					TDLYLX=TDLYLX,
					TDYT=TDYT,
					YDKBM=YDKBM,
					YDKID=YDKID,

				};
				return it;
			}
		}
		class VecDkCache
		{
			class TmpItem
			{
				public string SFQQQG;
				public double YHTMJ;
				public double QQMJ;
			}
			class ItemIdx
			{
				public int SFJBNT;
				public int DKBM;
				public int SCMJ;
				public int SCMJM; public int SYQXZ; public int DKLB; public int TDLYLX; public int DLDJ; public int TDYT; public int DKDZ; public int DKXZ; public int DKNZ; public int DKBZ; public int ZJRXM; public int DKMC;
				public int DKBZXX;
				public int BGLX, BGYY, YDKBM;//,SCBZ;
				
				public ItemIdx(IRow r)
				{
					SFJBNT = r.Fields.FindField(nameof(SFJBNT));
					DKBM = r.Fields.FindField(nameof(DKBM));
					SCMJ = r.Fields.FindField(nameof(SCMJ));
					SCMJM = r.Fields.FindField(nameof(SCMJM));
					SYQXZ = r.Fields.FindField(nameof(SYQXZ));
					DKLB = r.Fields.FindField(nameof(DKLB));
					TDLYLX = r.Fields.FindField(nameof(TDLYLX));
					DLDJ = r.Fields.FindField(nameof(DLDJ));
					TDYT = r.Fields.FindField(nameof(TDYT));
					DKDZ = r.Fields.FindField(nameof(DKDZ));
					DKXZ = r.Fields.FindField(nameof(DKXZ));
					DKNZ = r.Fields.FindField(nameof(DKNZ));
					DKBZ = r.Fields.FindField(nameof(DKBZ));
					ZJRXM = r.Fields.FindField(nameof(ZJRXM));
					DKMC = r.Fields.FindField(nameof(DKMC));
					DKBZXX = r.Fields.FindField(nameof(DKBZXX));
					BGLX = r.Fields.FindField(nameof(BGLX));
					BGYY = r.Fields.FindField(nameof(BGYY));
					YDKBM = r.Fields.FindField(nameof(YDKBM));
					//SCBZ = r.Fields.FindField(nameof(SCBZ));
				}
			}

			/// <summary>
			/// [DKBM,VecDkItem]
			/// </summary>
			public readonly Dictionary<string, VecDkItem> Dic = new Dictionary<string, VecDkItem>();

			public void Init(IFeatureClass fc,IWorkspace mdb)
			{
				Dic.Clear();

				var dic = new Dictionary<string, TmpItem>();
				mdb.QueryCallback("select DKBM,YHTMJ,SFQQQG,HTMJM from CBDKXX", r =>
				 {
					 var dkbm = Util.GetString(r, 0);
					 if (!dic.TryGetValue(dkbm, out TmpItem it))
					 {
						 it = new TmpItem();
						 dic[dkbm] = it;
					 }
					 it.YHTMJ += Util.GetDouble(r, 1);
					 it.SFQQQG = Util.GetString(r, 2);
					 it.QQMJ += Util.GetDouble(r, 3);
					 return true;
				 });

				//using (var fc = ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(shpFile))
				{
					var qf = new QueryFilter()
					{
						SubFields = $"DKBM,SFJBNT,SCMJ,SCMJM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,DKDZ,DKXZ,DKNZ,DKBZ,ZJRXM,DKMC,DKBZXX,BGLX,BGYY,YDKBM,{fc.ShapeFieldName},{fc.OIDFieldName}"
					};
					ItemIdx idx = null;
					fc.Search(qf, r =>
					 {
						 if (idx == null)
						 {
							 idx = new ItemIdx(r);
						 }
						 var dkbm = r.GetValue(idx.DKBM)?.ToString();
						 if (dkbm != null)
						 {
							 var it = new VecDkItem()
							 {
								 OID=r.Oid,
								 DKBM=dkbm,
								 SFJBNT = SafeConvertAux.ToShort(r.GetValue(idx.SFJBNT)),
								 SCMJ = SafeConvertAux.ToDouble(r.GetValue(idx.SCMJ)),
								 SCMJM = SafeConvertAux.ToDouble(r.GetValue(idx.SCMJM)),
								 SYQXZ = r.GetValue(idx.SYQXZ)?.ToString(),
								 DKLB = r.GetValue(idx.DKLB)?.ToString(),
								 TDLYLX = r.GetValue(idx.TDLYLX)?.ToString(),
								 DLDJ = r.GetValue(idx.DLDJ)?.ToString(),
								 TDYT = r.GetValue(idx.TDYT)?.ToString(),
								 DKDZ = r.GetValue(idx.DKDZ)?.ToString(),
								 DKXZ = r.GetValue(idx.DKXZ)?.ToString(),
								 DKNZ = r.GetValue(idx.DKNZ)?.ToString(),
								 DKBZ = r.GetValue(idx.DKBZ)?.ToString(),
								 ZJRXM = r.GetValue(idx.ZJRXM)?.ToString(),
								 DKMC = r.GetValue(idx.DKMC)?.ToString(),
								 DKBZXX = r.GetValue(idx.DKBZXX)?.ToString(),
								 BGLX = ETXBGLX.None,
								 BGYY = r.GetValue(idx.BGYY)?.ToString(),
								 YDKBM = r.GetValue(idx.YDKBM)?.ToString(),
								 //SCBZ=SafeConvertAux.ToShort(r.GetValue(idx.SCBZ)),
								 Shape = (r as IFeature).Shape
							 };
							 var oBGLX = r.GetValue(idx.BGLX);
							 if (oBGLX != null)
							 {
								 if (!short.TryParse(oBGLX.ToString(), out short n))
								 {
									 throw new Exception($"矢量地块文件的变更类型只能是空字符串或数字类型，第{it.OID}行为：{oBGLX.ToString()}");
								 }
								 it.BGLX = (ETXBGLX)SafeConvertAux.ToShort(oBGLX);
							 }

							 if (dic.TryGetValue(dkbm, out TmpItem it1))
							 {
								 it.SFQQQG = it1.SFQQQG;
								 it.YHTMJ = it1.YHTMJ;
								 it.QQMJ = it1.QQMJ;
							 }

							 Dic[dkbm] = it;
						 }
						 return true;
					 });
				}
			}
		}

		class InsertBase
		{
			protected readonly string insertSql;
			protected readonly List<SQLParam> _insertPrms = new List<SQLParam>();
			protected readonly Dictionary<string, SQLParam> _dic = new Dictionary<string, SQLParam>();
			public readonly string _tableName;
			public readonly string _mdbFields;
			protected readonly IFeatureWorkspace _db;

			protected InsertBase(IFeatureWorkspace db,string tableName,string mdbFields,string insertAdditionFields)//List<string> lstInsertField)
			{
				_db = db;
				_tableName = tableName;
				_mdbFields = mdbFields;

				var lstInsertField = new List<string>();
				if (!string.IsNullOrEmpty(insertAdditionFields))
				{
					lstInsertField.AddRange(insertAdditionFields.Split(','));
				}
				insertSql = MakeInsertSQL(lstInsertField);
			}
			private string MakeInsertSQL(List<string> lst)
			{
				if (!string.IsNullOrEmpty(_mdbFields))
				{
					var sa = _mdbFields.Split(',');
					foreach (var fieldName in sa)
					{
						lst.Add(fieldName);
					}
				}

				foreach (var s in lst)
				{
					var sp = new SQLParam() { ParamName = s };
					_insertPrms.Add(sp);
					_dic[s] = sp;
				}
				string fields = null;
				string vals = null;
				foreach (var p in _insertPrms)
				{
					var s = p.ParamName;
					var val = _db.SqlFunc.GetParamPrefix() + s;
					if (fields == null)
					{
						fields = s;
						vals = val;
					}
					else
					{
						fields += "," + s;
						vals += "," + val;
					}
				}
				var sql = $"insert into {_tableName}({fields}) values({vals})";
				return sql;
			}

			protected int Count(string wh,string tableName=null)
			{
				if (tableName == null)
				{
					tableName = _tableName;
				}
				var cnt = SafeConvertAux.ToInt32(_db.QueryOne($"select count(1) from {tableName} where {wh}"));
				return cnt;
			}
		}
		class InsertUpdateBase: InsertBase
		{
			protected readonly string updateSql;
			protected readonly List<SQLParam> _updatePrms = new List<SQLParam>();


			protected InsertUpdateBase(IFeatureWorkspace db,string tableName, string keyField,string mdbFields
				, string insertAdditionFields,string updateAdditionFields)
				:base(db,tableName,mdbFields, insertAdditionFields)
			{
				var lstUpdateField = new List<string>();
				if (!string.IsNullOrEmpty(updateAdditionFields))
				{
					lstUpdateField.AddRange(updateAdditionFields.Split(','));
				}
				updateSql = MakeUpdateSQL(keyField, lstUpdateField);
			}
			private string MakeUpdateSQL(string keyField, List<string> lst)
			{
				var sql = $"update {_tableName} set ";

				if (!string.IsNullOrEmpty(_mdbFields))
				{
					var sa = _mdbFields.Split(',');
					foreach (var s in sa)
					{
						lst.Add(s);
					}
				}
				foreach (var s in lst)
				{
					if (s != keyField)
					{
						if (_dic.TryGetValue(s, out SQLParam sp))
						{
							_updatePrms.Add(sp);
							sql += $"{s}={_db.SqlFunc.GetParamPrefix()}{s},";
						}
					}
				}

				sql = sql.TrimEnd(',');

				return sql;
			}
		}

		/// <summary>
		/// 主功能：导入到QSSJ_CBJYQZDJB
		/// 附加功能：DJ_CBJYQ_DJB
		/// </summary>
		class CBJYQZDJB : InsertUpdateBase
		{
			private readonly DJ_CBJYQ_DJB _DJ_CBJYQ_DJB;
			public CBJYQZDJB(Temp p)
				: base(p._db, "QSSJ_CBJYQZDJB", "CBJYQZBM"
					  , "CBJYQZBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,DKSYT,CBQX,YCBJYQZBH,CBJYQZLSH,DJBFJ,DBR,DJSJ",
					 "ID", null)
			{
				_DJ_CBJYQ_DJB = new DJ_CBJYQ_DJB(p);
			}

			/// <summary>
			/// 导入QSSJ_CBJYQZDJB和DJ_CBJYQ_DJB
			/// </summary>
			/// <param name="it"></param>
			public void DoImport(DjbItem it)
			{
				_dic["CBJYQZBM"].ParamValue = it.CBJYQZBM;
				_dic["FBFBM"].ParamValue = it.FBFBM;
				_dic["CBFBM"].ParamValue = it.CBFBM;
				_dic["CBFS"].ParamValue = it.CBFS;
				_dic["CBQXQ"].ParamValue = it.CBQXQ;
				_dic["CBQXZ"].ParamValue = it.CBQXZ;
				_dic["DKSYT"].ParamValue = it.DKSYT;
				_dic["CBQX"].ParamValue = it.CBQX;
				_dic["YCBJYQZBH"].ParamValue = it.YCBJYQZBH;
				_dic["CBJYQZLSH"].ParamValue = it.CBJYQZLSH;
				_dic["DJBFJ"].ParamValue = it.DJBFJ;
				_dic["DBR"].ParamValue = it.DBR;
				_dic["DJSJ"].ParamValue = it.DJSJ;

				if (!it.HasImported)
				{
					try
					{
						var wh = $"CBJYQZBM='{it.CBJYQZBM}'";
						var fInsert = Count(wh) == 0;
						if (fInsert)
						{
							_dic["ID"].ParamValue = Guid.NewGuid().ToString();
							_db.ExecuteNonQuery(insertSql, _insertPrms);
						}
						else
						{
							var sql = $"{updateSql} where {wh}";
							_db.ExecuteNonQuery(sql, _updatePrms);
						}
					}
					catch (Exception ex)
					{
						throw new Exception($"err at import {_tableName}:{ex.Message}");
					}
				}

				_DJ_CBJYQ_DJB.DoImport(it);
			}

			public static DjbItem FillItem(IDataReader r)
			{
				var it = new DjbItem
				{
					CBJYQZBM = Util.GetString(r, 0),
					FBFBM = Util.GetString(r, 1),
					CBFBM = Util.GetString(r, 2),
					CBFS = Util.GetString(r, 3),
					CBQXQ = Util.GetDateTime(r, 4),
					CBQXZ = Util.GetDateTime(r, 5),
					DKSYT = Util.GetString(r, 6),
					CBQX = Util.GetString(r, 7),
					YCBJYQZBH = Util.GetString(r, 8),
					CBJYQZLSH = Util.GetString(r, 9),
					DJBFJ = Util.GetString(r, 10),
					DBR = Util.GetString(r, 11),
					DJSJ = Util.GetDateTime(r, 12),
					//CBFMC = Util.GetString(r, 13)
				};
				return it;
			}
		}

		/// <summary>
		/// 主功能：导入到QSSJ_CBHT
		/// 附加功能：导入到DJ_CBJYQ_CBHT
		/// </summary>
		class CBHT : InsertUpdateBase
		{
			private readonly Temp _p;

			

			#region for DJ_CBJYQ_CBHT
			private readonly string _insertSql1;
			private readonly List<SQLParam> _insertPrms1 = new List<SQLParam>();
			#endregion

			public CBHT(Temp p)//,IFeatureClass dkshpFC):
				:base(p._db,"QSSJ_CBHT", "CBHTBM"
					, "CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM"
					, "ID,ZT,DJZT,CJSJ" , "ZT,DJZT")
			{
				_p = p;

				#region for DJ_CBJYQ_CBHT
				var a = _db.SqlFunc.GetParamPrefix();
				_insertSql1 = insertSql.Replace($"{_tableName}(ID,ZT,DJZT,CJSJ", "DJ_CBJYQ_CBHT(DJBID,ID").Replace($"values({a}ID,{a}ZT,{a}DJZT,{a}CJSJ", $"values({a}DJBID,{a}ID");
				var slp = new SQLParam() { ParamName = "DJBID" };
				_insertPrms1.Add(slp);
				foreach (var ip in _insertPrms)
				{
					switch (ip.ParamName)
					{
						case "ZT": case "DJZT": case "CJSJ":  break;
						default:
							_insertPrms1.Add(ip); break;
					}
				}
				_dic["DJBID"] = slp;
				#endregion
			}

			/// <summary>
			/// 1.导入QSSJ_CBHT；
			/// 2.导入DJ_CBJYQ_CBHT（如果有登记簿）
			/// </summary>
			/// <param name="it"></param>
			public void DoImport(CbhtItem it)
			{
				//var cbht = it.cbhtItem;
				if (it!=null)
				{
					Assign(it);
					try
					{
						var djb = it.parent;
						if (djb == null)
						{
							if (Count($"CBHTBM='{it.CBHTBM}' and (select count(1) from DJ_CBJYQ_DJB where ID=a.DJBID and QSZT<2)>0",
								"DJ_CBJYQ_CBHT a") > 0)
							{
								throw new Exception($"权属数据库中CBHTBM='{it.CBHTBM}'的记录无登记簿信息，但在业务系统中已做了登记！");
							}
						}
						WriteToQSSJ_CBHT(it);

						if (djb != null)
						{
							#region insert into DJ_CBJYQ_CBHT
							_dic["ID"].ParamValue = Guid.NewGuid().ToString();
							_dic["DJBID"].ParamValue =djb.ID;
							_db.ExecuteNonQuery(_insertSql1, _insertPrms1);
							#endregion
						}
						//_p._DLXX_DK.DoImport(mdb, _it, it.CBFMC, it.DJSJ);
					}
					catch (Exception ex)
					{
						throw new Exception($"err at import {_tableName}:{ex.Message}");
					}
				}
			}

			public static CbhtItem FillItem(IDataReader r, CbhtItem it)
			{
				it.CBHTBM = r.GetString(0);
				it.YCBHTBM = Util.GetString(r, 1);
				it.FBFBM = Util.GetString(r, 2);
				it.CBFBM = Util.GetString(r, 3);
				it.CBFS = Util.GetString(r, 4);
				it.CBQXQ = Util.GetDateTime(r, 5);
				it.CBQXZ = Util.GetDateTime(r, 6);
				it.HTZMJ = Util.GetDouble(r, 7);
				it.CBDKZS = Util.GetInt(r, 8);
				it.QDSJ = Util.GetDateTime(r, 9);
				it.HTZMJM = Util.GetDouble(r, 10);
				it.YHTZMJ = Util.GetDouble(r, 11);
				it.YHTZMJM = Util.GetDouble(r, 12);
				return it;
			}

			private void Assign(CbhtItem it)
			{
				int djzt = (int)(it.parent != null ? EDjzt.Ydj : EDjzt.Wdj);
				_dic["CBHTBM"].ParamValue = it.CBHTBM;
				_dic["YCBHTBM"].ParamValue = it.YCBHTBM;
				_dic["FBFBM"].ParamValue = it.FBFBM;
				_dic["CBFBM"].ParamValue = it.CBFBM;
				_dic["CBFS"].ParamValue = it.CBFS;
				_dic["CBQXQ"].ParamValue = it.CBQXQ;
				_dic["CBQXZ"].ParamValue = it.CBQXZ;
				_dic["HTZMJ"].ParamValue = it.HTZMJ;
				_dic["CBDKZS"].ParamValue = it.CBDKZS;
				_dic["QDSJ"].ParamValue = it.QDSJ;
				_dic["HTZMJM"].ParamValue = it.HTZMJM;
				_dic["YHTZMJ"].ParamValue = it.YHTZMJ;
				_dic["YHTZMJM"].ParamValue = it.YHTZMJM;
				_dic["FBFBM"].ParamValue = it.FBFBM;
				_dic["ZT"].ParamValue = 1;//有效
				_dic["DJZT"].ParamValue = djzt;//已登记
			}

			private void WriteToQSSJ_CBHT(CbhtItem it)
			{
				if (it.HasImported)
				{
					return;
				}
				var fInsert = Count($"CBHTBM='{it.CBHTBM}'") == 0;
				if (fInsert)
				{
					_dic["ID"].ParamValue = Guid.NewGuid().ToString();
					_dic["CJSJ"].ParamValue = _p.CJSJ;// DateTime.Now;
					_db.ExecuteNonQuery(insertSql, _insertPrms);
				}
				else
				{
					var sql = updateSql + $" where CBHTBM='{it.CBHTBM}'";
					_db.ExecuteNonQuery(sql, _updatePrms);
				}
			}
		}

		/// <summary>
		/// 主要功能：负责导入QSSJ_CBF_JTCY
		/// 附加功能：导入DJ_CBJYQ_CBF_JTCY
		/// </summary>
		class CBF_JTCY :InsertBase
		{
			#region for DJ_CBJYQ_CBF_JTCY
			private readonly string _insertSql1;
			private readonly List<SQLParam> _insertPrms1 = new List<SQLParam>();
			#endregion

			private readonly Temp _p;
			public CBF_JTCY(Temp p):base(p._db, "QSSJ_CBF_JTCY"
				, "CBFBM,CYXM,CYXB,CYZJLX,CYZJHM,YHZGX,CYBZ,SFGYR,CYBZSM"
				,"ID,CSRQ")
			{
				_p = p;
				#region for DJ_CBJYQ_CBF_JTCY
				var a = _db.SqlFunc.GetParamPrefix();
				_insertSql1 = insertSql.Replace("QSSJ_CBF_JTCY(ID", "DJ_CBJYQ_CBF_JTCY(CBFID,ID").Replace($"values({a}ID", $"values({a}CBFID,{a}ID");
				var slp = new SQLParam() { ParamName = "CBFID" };
				_insertPrms1.Add(slp);
				_insertPrms1.AddRange(_insertPrms);
				_dic["CBFID"] = slp;
				#endregion
			}
			public void DoImport(/*DjbItem it*/CbfItem cbf,string djCbfID)
			{
				//var cbf = it.cbhtItem?.cbfItem;
				if (cbf == null)
				{
					return;
				}
				if (!cbf.CbfJtchHasImported)
				{
					var mdb = _p._mdb;
					var sql = $"delete from {_tableName} where CBFBM='{cbf.CBFBM}'";
					_db.ExecuteNonQuery(sql);
				}
				//sql = $"select {_mdbFields} from CBF_JTCY where CBFBM='{it.CBFBM}'";
				//mdb.QueryCallback(sql, r =>
				foreach(var jt in cbf.cbfJtcyItems)
				 {
					 Assigin(jt);
					 try
					 {
						if (!cbf.CbfJtchHasImported)
						{
							_db.ExecuteNonQuery(insertSql, _insertPrms);
						}

						 #region for DJ_CBJYQ_CBF_JTCY
						 _dic["CBFID"].ParamValue = djCbfID;
						 _db.ExecuteNonQuery(_insertSql1, _insertPrms1);
						 #endregion
					 }
					 catch (Exception ex)
					 {
						 throw new Exception($"err at import QSSJ_CBF_JTCY:{ex.Message}");
					 }
					// return true;
				 }//);
			}

			public void DoImport(string CBFBM)// CbhtItem it)
			{
				var mdb = _p._mdb;
				var sql = $"delete from {_tableName} where CBFBM='{CBFBM}'";
				_db.ExecuteNonQuery(sql);
				sql = $"select {_mdbFields} from CBF_JTCY where CBFBM='{CBFBM}'";
				mdb.QueryCallback(sql, r =>
				{
					Assigin(r);
					try
					{
						_db.ExecuteNonQuery(insertSql, _insertPrms);
					}
					catch (Exception ex)
					{
						throw new Exception($"err at import QSSJ_CBF_JTCY:{ex.Message}");
					}
					return true;
				});
			}


			public static CbfJtcyItem FillItem(IDataReader r)//, CbfJtcyItem it = null)
			{
				var it = new CbfJtcyItem()
				{
					CBFBM = r.GetString(0),
					CYXM = Util.GetString(r, 1),
					CYXB = Util.GetString(r, 2),
					CYZJLX = Util.GetString(r, 3),
					CYZJHM = Util.GetString(r, 4),
					YHZGX = Util.GetString(r, 5),
					CYBZ = Util.GetString(r, 6),
					SFGYR = Util.GetString(r, 7),
					CYBZSM = Util.GetString(r, 8),
				};
				
				return it;
			}
			private void Assigin(IDataReader r)
			{
				var id = Guid.NewGuid().ToString();
				var cyzjLx = Util.GetString(r, 3);
				var CYZJHM = Util.GetString(r, 4);

				#region 根据18位身份证号码解析出生日期
				DateTime? csrq = null;
				if (cyzjLx == "1" && CYZJHM != null && CYZJHM.Length == 18)
				{
					try
					{
						var Y = CYZJHM.Substring(6, 4);
						var M = CYZJHM.Substring(10, 2);
						var D = CYZJHM.Substring(12, 2);
						csrq = DateTime.Parse($"{Y}/{M}/{D}");
					}
					catch
					{
					}
				}
				#endregion

				_dic["CSRQ"].ParamValue = csrq;
				_dic["ID"].ParamValue = id;
				_dic["CBFBM"].ParamValue = Util.GetString(r, 0);// it.CBFBM;
				_dic["CYXM"].ParamValue = Util.GetString(r, 1);
				_dic["CYXB"].ParamValue = Util.GetString(r, 2);
				_dic["CYZJLX"].ParamValue = cyzjLx;
				_dic["CYZJHM"].ParamValue = CYZJHM;
				_dic["YHZGX"].ParamValue = Util.GetString(r, 5);
				_dic["CYBZ"].ParamValue = Util.GetString(r, 6);
				_dic["SFGYR"].ParamValue = Util.GetString(r, 7);
				_dic["CYBZSM"].ParamValue = Util.GetString(r, 8);
			}

			private void Assigin(CbfJtcyItem r)
			{
				var id = Guid.NewGuid().ToString();
				var cyzjLx = r.CYZJLX;// Util.GetString(r, 3);
				var CYZJHM = r.CYZJHM;// Util.GetString(r, 4);

				#region 根据18位身份证号码解析出生日期
				DateTime? csrq = null;
				if (cyzjLx == "1" && CYZJHM != null && CYZJHM.Length == 18)
				{
					try
					{
						var Y = CYZJHM.Substring(6, 4);
						var M = CYZJHM.Substring(10, 2);
						var D = CYZJHM.Substring(12, 2);
						csrq = DateTime.Parse($"{Y}/{M}/{D}");
					}
					catch
					{
					}
				}
				#endregion

				_dic["CSRQ"].ParamValue = csrq;
				_dic["ID"].ParamValue = id;
				_dic["CBFBM"].ParamValue = r.CBFBM;// Util.GetString(r, 0);// it.CBFBM;
				_dic["CYXM"].ParamValue = r.CYXM;// Util.GetString(r, 1);
				_dic["CYXB"].ParamValue = r.CYXB;// Util.GetString(r, 2);
				_dic["CYZJLX"].ParamValue = cyzjLx;
				_dic["CYZJHM"].ParamValue = CYZJHM;
				_dic["YHZGX"].ParamValue = r.YHZGX;// Util.GetString(r, 5);
				_dic["CYBZ"].ParamValue = r.CYBZ;// Util.GetString(r, 6);
				_dic["SFGYR"].ParamValue = r.SFGYR;// Util.GetString(r, 7);
				_dic["CYBZSM"].ParamValue = r.CYBZSM;// Util.GetString(r, 8);
			}
		}

		/// <summary>
		/// 主要功能：负责导入QSSJ_CBF
		/// 附加功能：导入DJ_CBJYQ_CBF
		/// </summary>
		class CBF : InsertUpdateBase
		{
			#region for DJ_CBJYQ_CBF
			private readonly string _insertSql1;
			private readonly List<SQLParam> _insertPrms1 = new List<SQLParam>();
			#endregion

			//private readonly CbfItem _it = new CbfItem();
			private readonly Temp _p;
			public CBF(Temp p)
				:base(p._db, "QSSJ_CBF", "CBFBM"
					 ,"CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR"
					 , "ID,ZT,DJZT,CJSJ,FBFBM", "ZT,DJZT,FBFBM"
					 )
			{
				_p = p;
				#region for DJ_CBJYQ_CBF
				var a = _db.SqlFunc.GetParamPrefix();
				_insertSql1 = insertSql.Replace("QSSJ_CBF(ID,ZT,DJZT,CJSJ,FBFBM", "DJ_CBJYQ_CBF(DJBID,ID").Replace($"values({a}ID,{a}ZT,{a}DJZT,{a}CJSJ,{a}FBFBM",$"values({a}DJBID,{a}ID");
				var slp = new SQLParam() { ParamName = "DJBID" };
				_insertPrms1.Add(slp);
				foreach (var ip in _insertPrms)
				{
					switch (ip.ParamName) {
						case "ZT": case "DJZT": case "CJSJ": case "FBFBM":break;
						default:
							_insertPrms1.Add(ip);break;
					}
				}
				_dic["DJBID"] = slp;
				#endregion
			}


			public string DoImport(CbfItem it)
			{
				string djCbfID = null;

				//var cbht = it.parent;
				var djb = it.parent?.parent;
				if (djb== null)
				{
					var cnt = Count($"CBFBM='{it.CBFBM}' and and (select count(1) from DJ_CBJYQ_DJB where ID=a.DJBID and QSZT<2)>0", "DJ_CBJYQ_CBF a");
					if (cnt > 0)
					{
						throw new Exception($"权属数据库中CBFBM={it.CBFBM}的记录无登记簿信息，但其在业务系统中登记过了！");
					}
				}

				
				var cbfIt = it;// it.cbhtItem?.cbfItem;
				if (cbfIt!=null)
				{
					Assigin(cbfIt);
					_dic["FBFBM"].ParamValue = it.parent?.FBFBM;

					try
					{

						WriteToQSSJ_CBF(cbfIt);

						if (djb != null)
						{
							#region insert into DJ_CBJYQ_CBF
							djCbfID = Guid.NewGuid().ToString();
							_dic["ID"].ParamValue = djCbfID;
							_dic["DJBID"].ParamValue = djb.ID;// it.ID;
							_db.ExecuteNonQuery(_insertSql1, _insertPrms1);
							#endregion
						}
					}
					catch (Exception ex)
					{
						throw new Exception($"err at import QSSJ_CBF:{ex.Message}");
					}
				}
				else
				{
					throw new Exception($"属性数据库中的CBF表缺少承包方编码为{it.CBFBM}的记录");
				}
				return djCbfID;
			}

			/// <summary>
			/// 写入该承包方对应的权属资料
			/// </summary>
			public void WriteQSZL(CbfItem it)
			{
				var path=$"{_p._param.RootPath}权属资料/{it.CBFBM}";
				if (!Directory.Exists(path))
				{
					return;
				}
				var oPath = $"{_p._param.QszlBcPath}{it.CBFBM}";
				if (!Directory.Exists(oPath))
				{
					Directory.CreateDirectory(oPath);
				}
				FileUtil.EnumFiles(path, fi =>
				 {
					 try
					 {
						 var oFileName = $"{oPath}\\{fi.Name}";
						 if (!File.Exists(oFileName))
						 {
							 File.Copy(fi.FullName, oFileName);
						 }
					 }
					 catch
					 {
					 }
					 return true;
				 });
			}

			///// <summary>
			///// 将签了承包合同但未登记（不在CBJYQZDJB）的数据导入到QSSJ_CBF
			///// </summary>
			///// <param name="mdb"></param>
			///// <param name="it"></param>
			//public void DoImport1(CbhtItem it)
			//{
			//	var cnt = Count($"CBFBM='{it.CBFBM}'", "DJ_CBJYQ_CBF");
			//	if (cnt > 0)
			//	{
			//		throw new Exception($"CBFBM={it.CBFBM}的记录已经在业务系统中登记过了！");
			//	}
			//	var sql = $"select {_mdbFields} from CBF where CBFBM='{it.CBFBM}'";
			//	bool fOK = false;
			//	_p._mdb.QueryCallback(sql, r =>
			//	{
			//		fOK = true;
			//		FillItem(r, _it);
			//		return false;
			//	});
			//	if (fOK)
			//	{
			//		Assigin(_it);
			//		_dic["FBFBM"].ParamValue = it.FBFBM;
			//		try
			//		{
			//			WriteToQSSJ_CBF(_it);
			//		}
			//		catch (Exception ex)
			//		{
			//			throw new Exception($"err at import QSSJ_CBF:{ex.Message}");
			//		}
			//	}
			//	else
			//	{
			//		throw new Exception($"属性数据库中的CBF表缺少承包方编码为{it.CBFBM}的记录");
			//	}
			//}

			///// <summary>
			///// 将未登记和未签承包合同的数据（不在CBJYQZDJB和CBHT中）导入到QSSJ_CBF
			///// </summary>
			///// <param name="mdb"></param>
			///// <param name="callback"></param>
			//public void Import(/*DBAccess mdb,*/Action<int> callback)
			//{
			//	var lstCbhtItem = new List<CbfItem>();
			//	_p._mdb.QueryCallback($"select { _mdbFields} from CBF where CBFBM not in(select distinct CBFBM from CBJYQZDJB) and CBFBM not in (select distinct CBFBM from CBHT)", r =>
			//	{
			//		lstCbhtItem.Add(FillItem(r, new CbfItem()));
			//		return true;
			//	});
			//	int i = 0;
			//	var cbfJtcy = _p._cbfJtcy;// new CBF_JTCY(_db);
			//	foreach (var it in lstCbhtItem)
			//	{
			//		try
			//		{
			//			_db.BeginTransaction();


			//			var cnt = Count($"CBFBM='{it.CBFBM}'", "DJ_CBJYQ_CBF");
			//			if (cnt > 0)
			//			{
			//				throw new Exception($"CBFBM='{it.CBFBM}'的记录在业务系统中已登记！");
			//			}


			//			WriteToQSSJ_CBF(it);
			//			cbfJtcy.DoImport(it.CBFBM);
			//			_db.Commit();
			//		}
			//		catch (Exception ex)
			//		{
			//			_p.ReportError($"[CBFBM]='{it.CBFBM}' err:" + ex.Message);
			//			_db.Rollback();
			//		}

			//		callback(++i);
			//	}
			//}

			public static CbfItem FillItem(IDataReader r,CbfItem it=null)
			{
				if (it == null)
				{
					it = new CbfItem();
				}
				it.CBFBM = r.GetString(0);
				it.CBFLX = Util.GetString(r, 1);
				it.CBFMC = Util.GetString(r, 2);
				it.CBFZJLX = Util.GetString(r, 3);
				it.CBFZJHM = Util.GetString(r, 4);
				it.CBFDZ = Util.GetString(r, 5);
				it.YZBM = Util.GetString(r, 6);
				it.LXDH = Util.GetString(r, 7);
				it.CBFCYSL = SafeConvertAux.ToInt32(r.GetValue(8));
				it.CBFDCRQ = Util.GetDateTime(r, 9);
				it.CBFDCY = Util.GetString(r, 10);
				it.CBFDCJS = Util.GetString(r, 11);
				it.GSJS = Util.GetString(r, 12);
				it.GSJSR = Util.GetString(r, 13);
				it.GSSHRQ = Util.GetDateTime(r, 14);
				it.GSSHR = Util.GetString(r, 15);
				return it;
			}

			private void Assigin(CbfItem it)//,int djzt=2)
			{
				int djzt = (int)(it.parent != null&&it.parent.parent!=null ? EDjzt.Ydj : EDjzt.Wdj);
				_dic["CBFBM"].ParamValue = it.CBFBM;
				_dic["CBFLX"].ParamValue = it.CBFLX;
				_dic["CBFMC"].ParamValue = it.CBFMC;
				_dic["CBFZJLX"].ParamValue = it.CBFZJLX;
				_dic["CBFZJHM"].ParamValue = it.CBFZJHM;
				_dic["CBFDZ"].ParamValue = it.CBFDZ;
				_dic["YZBM"].ParamValue = it.YZBM;
				_dic["LXDH"].ParamValue = it.LXDH;
				_dic["CBFCYSL"].ParamValue = it.CBFCYSL;
				_dic["CBFDCRQ"].ParamValue = it.CBFDCRQ;
				_dic["CBFDCY"].ParamValue = it.CBFDCY;
				_dic["CBFDCJS"].ParamValue = it.CBFDCJS;
				_dic["GSJS"].ParamValue = it.GSJS;
				_dic["GSJSR"].ParamValue = it.GSJSR;
				_dic["GSSHRQ"].ParamValue = it.GSSHRQ;
				_dic["GSSHR"].ParamValue = it.GSSHR;
				_dic["ZT"].ParamValue = 1;//有效
				_dic["DJZT"].ParamValue = djzt;// 2;//已登记
				//_dic["FBFBM"].ParamValue = it.FBFBM;
			}

			private void WriteToQSSJ_CBF(CbfItem it)
			{
				if (it.HasImported)
				{
					return;
				}
				var wh = $"CBFBM='{it.CBFBM}'";
				var fInsert = Count(wh) == 0;
				if (fInsert)
				{
					_dic["ID"].ParamValue = Guid.NewGuid().ToString();
					_dic["CJSJ"].ParamValue = _p.CJSJ;// DateTime.Now;
					_db.ExecuteNonQuery(insertSql, _insertPrms);
				}
				else
				{
					var sql = updateSql + $" where {wh}";
					_db.ExecuteNonQuery(sql, _updatePrms);
				}
			}
		}

		/// <summary>
		/// 主要功能：import into QSSJ_CBJYQZ
		/// 附加功能：import into DJ_CBJYQ_QZ
		/// </summary>
		class CBJYQZ : InsertUpdateBase
		{
			#region for DJ_CBJYQ_QZ
			private readonly string _insertSql1;
			private readonly List<SQLParam> _insertPrms1 = new List<SQLParam>();
			#endregion
			private readonly Temp _p;
			public CBJYQZ(Temp p)
				: base(p._db, "QSSJ_CBJYQZ", "CBJYQZBM"
					  , "CBJYQZBM,FZJG,FZRQ,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM",
					 "ID",null)
			{
				_p = p;
				#region for DJ_CBJYQ_QZ
				var a = _db.SqlFunc.GetParamPrefix();

				var additonFields = "DJBID,SFYZX,SQSJC,BZNF,NDSXH,ZSBS,FZJGSZDMC,CBJYQZLSH,DYCS";
				var addFields1 = a + additonFields.Replace(",", $",{a}");
				_insertSql1 = insertSql.Replace($"{_tableName}(ID", $"DJ_CBJYQ_QZ({additonFields},ID").Replace($"values({a}ID", $"values({addFields1},{a}ID");

				foreach (var s in additonFields.Split(','))
				{
					var slp = new SQLParam() { ParamName = s };
					_insertPrms1.Add(slp);
					_dic[s] = slp;
				}
				_insertPrms1.AddRange(_insertPrms);
				#endregion
			}

			/// <summary>
			/// 导入QSSJ_CBJYQZ和DJ_CBJYQ_QZ
			/// </summary>
			/// <param name="it"></param>
			public void DoImport(DjbItem it)
			{
				//var mdb = _p._mdb;
				//var sql = $"select {_mdbFields} from CBJYQZ where CBJYQZBM='{it.CBJYQZBM}'";
				//mdb.QueryCallback(sql, r =>
				var qzIt = it.cbjyqzItem;
				if(qzIt!=null&&!qzIt.HasImported)
				 {
					 _dic["CBJYQZBM"].ParamValue = qzIt.CBJYQZBM;
					_dic["FZJG"].ParamValue = qzIt.FZJG;// Util.GetString(r,1);
					_dic["FZRQ"].ParamValue = qzIt.FZRQ;// Util.GetDateTime(r,2);
					_dic["QZSFLQ"].ParamValue = qzIt.QZSFLQ;// Util.GetString(r,3);
					_dic["QZLQRQ"].ParamValue = qzIt.QZLQRQ;// Util.GetDateTime(r, 4);
					_dic["QZLQRXM"].ParamValue = qzIt.QZLQRXM;// Util.GetString(r, 5);
					_dic["QZLQRZJLX"].ParamValue = qzIt.QZLQRZJLX;// Util.GetString(r, 6);
					_dic["QZLQRZJHM"].ParamValue = qzIt.QZLQRZJHM;// Util.GetString(r, 7);
					 try
					 {
						 var wh = $"CBJYQZBM='{it.CBJYQZBM}'";
						 var id =_db.QueryOne($"select id from {_tableName} where {wh}");// Count(wh) == 0;
						 if (id==null)
						 {
							 _dic["ID"].ParamValue = Guid.NewGuid().ToString();
							 _db.ExecuteNonQuery(insertSql, _insertPrms);
						 }
						 else
						 {
							 var sql = $"{updateSql} where ID='{id}'";
							 _db.ExecuteNonQuery(sql, _updatePrms);
						 }
					 }
					 catch (Exception ex)
					 {
						 throw new Exception($"err at import {_tableName}:{ex.Message}");
					}

					#region insert into DJ_CBJYQ_QZ					
					try
					{
						var lsh= QssjDjbRepository.ParseLsh(it.CBJYQZLSH);
						_dic["ID"].ParamValue = Guid.NewGuid().ToString();
						_dic["DJBID"].ParamValue = it.ID;
						_dic["SFYZX"].ParamValue = 0;
						_dic["SQSJC"].ParamValue = lsh?.SQSJC;// qzIt.SQSJC;
						_dic["BZNF"].ParamValue = lsh?.BZNF;// qzIt.BZNF;
						_dic["NDSXH"].ParamValue = lsh?.NDSXH;// qzIt.NDSXH;
						_dic["ZSBS"].ParamValue =0;//证书版式
						_dic["FZJGSZDMC"].ParamValue = qzIt.FZJG;
						_dic["CBJYQZLSH"].ParamValue = it.CBJYQZLSH;
						_dic["DYCS"].ParamValue = 0;

						_db.ExecuteNonQuery(_insertSql1, _insertPrms1);

					}
					catch (Exception ex)
					{
						throw new Exception($"err at import DJ_CBJYQ_QZ:{ex.Message}");
					}
					#endregion
				}
			}

			public static CbjyqzItem FillItem(IDataReader r)
			{
				var it = new CbjyqzItem()
				{
					CBJYQZBM = Util.GetString(r, 0),
					FZJG = Util.GetString(r, 1),
					FZRQ = Util.GetDateTime(r, 2),
					QZSFLQ = Util.GetString(r, 3),
					QZLQRQ = Util.GetDateTime(r, 4),
					QZLQRXM = Util.GetString(r, 5),
					QZLQRZJLX = Util.GetString(r, 6),
					QZLQRZJHM = Util.GetString(r, 7),
				};
				return it;
			}
		}

		/// <summary>
		/// 主功能：import into QSSJ_CBDKXX
		/// 附加功能：import into  DJ_CBJYQ_DKXX
		/// </summary>
		class CBDKXX : InsertUpdateBase
		{
			private readonly Temp _p;
			#region for DJ_CBJYQ_DKXX
			private readonly string _insertSql1;
			private readonly List<SQLParam> _insertPrms1 = new List<SQLParam>();
			#endregion

			public CBDKXX(Temp p,IFeatureWorkspace db)
				: base(db, "QSSJ_CBDKXX", "CBJYQZBM"
					  , "CBJYQZBM,DKBM,FBFBM,CBFBM,CBJYQQDFS,HTMJ,LZHTBM,CBHTBM,YHTMJ,HTMJM,YHTMJM,SFQQQG",
					 "ID",null)
			{
				_p = p;
				#region for DJ_CBJYQ_DKXX
				var a = _db.SqlFunc.GetParamPrefix();

				var additionFields = "DJBID,DKID,SFJBNT,SCMJ,SCMJM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,DKDZ,DKXZ,DKNZ,DKBZ,ZJRXM,DKMC,DKBZXX";
				var additionFields1 = a + additionFields.Replace(",", $",{a}");

				_insertSql1 = insertSql.Replace($"{_tableName}(ID", $"DJ_CBJYQ_DKXX({additionFields},ID").Replace($"values({a}ID", $"values({additionFields1},{a}ID");

				foreach (var s in additionFields.Split(','))
				{
					var slp = new SQLParam() { ParamName =s};
					_insertPrms1.Add(slp);
					_dic[s] = slp;
				}
				_insertPrms1.AddRange(_insertPrms);
				#endregion
			}
			public void DoImport(CbhtItem it)
			{
				string ids = "";
				foreach(var r in it.dkxxItems)
				{
					var dkbm = r.DKBM;// Util.GetString(r, 1);
					if (!_p._dkCache.Dic.TryGetValue(dkbm, out VecDkItem dkIt))
					{
						throw new Exception($"err at import CBDKXX:在地块矢量文件中缺少地块编码为{dkbm}的记录！");
					}
					Assign(r);

					object id = null;

					try
					{
						//var wh = $"CBJYQZBM='{it.CBJYQZBM}' and DKBM='{dkbm}'";
						var wh = $"CBHTBM='{it.CBHTBM}' and DKBM='{dkbm}'";
						id = _db.QueryOne($"select id from {_tableName} where {wh}");// Count(wh) == 0;
						if (id == null)
						{
							id = Guid.NewGuid().ToString();
							_dic["ID"].ParamValue = id;
							_db.ExecuteNonQuery(insertSql, _insertPrms);
						}
						else
						{
							var sql = $"{updateSql} where ID='{id}'";
							_db.ExecuteNonQuery(sql, _updatePrms);
						}
						if (ids != "")
						{
							ids += ",";
						}
						ids += $"'{id}'";
					}
					catch (Exception ex)
					{
						throw new Exception($"err at import {_tableName}:{ex.Message}");
					}

					if (it.parent != null)
					{
						#region insert into DJ_CBJYQ_DKXX
						try
						{
							_dic["ID"].ParamValue = Guid.NewGuid().ToString();
							_dic["DJBID"].ParamValue = it.parent.ID;
							_dic["DKID"].ParamValue = id;

							_dic["SFJBNT"].ParamValue = dkIt.SFJBNT.ToString();
							_dic["SCMJ"].ParamValue = dkIt.SCMJ;
							_dic["SCMJM"].ParamValue = dkIt.SCMJM;
							_dic["SYQXZ"].ParamValue = dkIt.SYQXZ;
							_dic["DKLB"].ParamValue = dkIt.DKLB;
							_dic["TDLYLX"].ParamValue = dkIt.TDLYLX;
							_dic["DLDJ"].ParamValue = dkIt.DLDJ;
							_dic["TDYT"].ParamValue = dkIt.TDYT;
							_dic["DKDZ"].ParamValue = dkIt.DKDZ;
							_dic["DKXZ"].ParamValue = dkIt.DKXZ;
							_dic["DKNZ"].ParamValue = dkIt.DKNZ;
							_dic["DKBZ"].ParamValue = dkIt.DKBZ;
							_dic["ZJRXM"].ParamValue = dkIt.ZJRXM;
							_dic["DKMC"].ParamValue = dkIt.DKMC;
							_dic["DKBZXX"].ParamValue = dkIt.DKBZXX;

							_db.ExecuteNonQuery(_insertSql1, _insertPrms1);
						}
						catch (Exception ex)
						{
							throw new Exception($"err at import DJ_CBJYQ_DKXX:{ex.Message}");
						}
						#endregion
					}
				}

				if (ids != "")
				{
					var wh = ids.IndexOf(',') >= 0 ? $"id not in({ids})" : $"id<>{ids}";
					var sql = $"delete from {_tableName} where CBHTBM='{it.CBHTBM}' and {wh}";
					_db.ExecuteNonQuery(sql);
				}
			}

			public static DkxxItem FillItem(IDataReader r)
			{
				var it = new DkxxItem()
				{
					CBJYQZBM=Util.GetString(r,0),
					DKBM=Util.GetString(r,1),
					FBFBM=Util.GetString(r,2),
					CBFBM=Util.GetString(r,3),
					CBJYQQDFS=Util.GetString(r,4),
					HTMJ=Util.GetDouble(r,5),
					LZHTBM=Util.GetString(r,6),
					CBHTBM=Util.GetString(r,7),
					YHTMJ=Util.GetDouble(r,8),
					HTMJM=Util.GetDouble(r,9),
					YHTMJM=Util.GetDouble(r,10),
					SFQQQG=Util.GetString(r,11),
				};
				return it;
			}
			//private void Assign(System.Data.IDataReader r)
			//{
			//	_dic["CBJYQZBM"].ParamValue = Util.GetString(r, 0);
			//	_dic["DKBM"].ParamValue = Util.GetString(r, 1);
			//	_dic["FBFBM"].ParamValue = Util.GetString(r, 2);
			//	_dic["CBFBM"].ParamValue = Util.GetString(r, 3);
			//	_dic["CBJYQQDFS"].ParamValue = Util.GetString(r, 4);
			//	_dic["HTMJ"].ParamValue = Util.GetDouble(r, 5);
			//	_dic["LZHTBM"].ParamValue = Util.GetString(r, 6);
			//	_dic["CBHTBM"].ParamValue = Util.GetString(r, 7);
			//	_dic["YHTMJ"].ParamValue = Util.GetDouble(r, 8);
			//	_dic["HTMJM"].ParamValue = Util.GetDouble(r, 9);
			//	_dic["YHTMJM"].ParamValue = Util.GetDouble(r, 10);
			//	_dic["SFQQQG"].ParamValue = Util.GetString(r, 11);
			//}

			private void Assign(DkxxItem r)
			{
				_dic["CBJYQZBM"].ParamValue = r.CBJYQZBM;// Util.GetString(r, 0);
				_dic["DKBM"].ParamValue = r.DKBM;// Util.GetString(r, 1);
				_dic["FBFBM"].ParamValue = r.FBFBM;// Util.GetString(r, 2);
				_dic["CBFBM"].ParamValue = r.CBFBM;// Util.GetString(r, 3);
				_dic["CBJYQQDFS"].ParamValue = r.CBJYQQDFS;// Util.GetString(r, 4);
				_dic["HTMJ"].ParamValue = r.HTMJ;// Util.GetDouble(r, 5);
				_dic["LZHTBM"].ParamValue = r.LZHTBM;// Util.GetString(r, 6);
				_dic["CBHTBM"].ParamValue = r.CBHTBM;// Util.GetString(r, 7);
				_dic["YHTMJ"].ParamValue = r.YHTMJ;// Util.GetDouble(r, 8);
				_dic["HTMJM"].ParamValue = r.HTMJM;// Util.GetDouble(r, 9);
				_dic["YHTMJM"].ParamValue = r.YHTMJM;// Util.GetDouble(r, 10);
				_dic["SFQQQG"].ParamValue = r.SFQQQG;// Util.GetString(r, 11);
			}
		}
		class DLXX_DK: InsertUpdateBase,IDisposable
		{
			#region for DLXX_DK_TXBGJL
			private readonly string _insertSql1;
			private readonly List<SQLParam> _insertPrms1 = new List<SQLParam>();
			#endregion
			private readonly Temp _p;
			private readonly IFeatureClass _fc;

			public DLXX_DK(Temp p)
				:base(p._db,"DLXX_DK","DKBM"
					 //,"YSDM,DKBM,"
					 ,"DKMC,SYQXZ,DKLB,TDLYLX,DLDJ,SFJBNT,SCMJ,DKDZ,DKXZ,DKNZ,DKBZ,DKBZXX,ZJRXM,SCMJM,FBFBM,CBFMC,ZHXGSJ,SJLY"
					 ,"","")
			{
				_p = p;
				//_fc = fc;
				_fc = _db.OpenFeatureClass(_tableName);
				#region for DLXX_DK_TXBGJL
				var txbgjlFields = "ID,DKID,DKBM,YDKID,YDKBM,BGFS,BGYY";
				var sql = $"insert into DLXX_DK_TXBGJL({txbgjlFields}) values(";
				foreach (var fieldName in txbgjlFields.Split(','))
				{
					sql += $"{_db.SqlFunc.GetParamPrefix()}{fieldName},";
					var sp = new SQLParam() { ParamName = fieldName };
					_insertPrms1.Add(sp);
					_dic[fieldName] = sp;
				}
				_insertSql1=$"{sql.TrimEnd(',')})";

				#endregion
			}
			public List<VecDkItem> DoImport(CbhtItem it)
			{
				var lst = new List<VecDkItem>();
				if (it.VecDkHasImported)
				{
					return lst;
				}

				var djsj = it.parent?.DJSJ;
				var CBFMC = it.cbfItem?.CBFMC;

				foreach(var jt in it.dkxxItems)
				 {
					var dkbm = jt.DKBM;
					if (lst.Find(a => { return a.DKBM == dkbm; }) == null)
					{
						if (_p._dkCache.Dic.TryGetValue(dkbm, out VecDkItem vit))
						{
							vit.FBFBM = it.FBFBM;
							//vit.QQMJ = it.;
							vit.CBFMC = CBFMC;
							vit.DJSJ = djsj;
							vit.DJZT = EDjzt.Ydj;
							lst.Add(vit);
						}
					}
				 }
				DoImport(lst);
				return lst;
			}

			/// <summary>
			///  导入无对应DKXX的矢量地块
			/// </summary>
			/// <param name="it"></param>
			public void DoImport(VecDkItem it)
			{
				if (HasImported(it, it.BGLX != ETXBGLX.Sxbg))
				{
					return;
				}
				if (it.HasImported == true)
				{
					return;
				}
				if (it.FBFBM == null)
				{
					it.FBFBM = it.DKBM.Substring(0, 14);
				}
				switch (it.BGLX)
				{
					case ETXBGLX.None:
						if (!IsDKBMExistInDB(it.DKBM))
						{
							throw new Exception($"[DLXX_DK: DKBM='{it.DKBM}']的记录在业务系统中不存在，但其BGLX不为新增类型（9）");
						}
						break;
					case ETXBGLX.Xinz:
						{
							if (IsDKBMExistInDB(it.DKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录在业务系统中存在，但其BGLX为{(int)it.BGLX}(新增)");
							}
							it.SJLY = ESjly.Xinz;
							WriteToDB(it);
						}
						break;
					case ETXBGLX.Sxbg:
						{
							if (!IsDKBMExistInDB(it.DKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录不在业务系统中，但其BGLX为{(int)it.BGLX}(属性变更)");
							}
							if (IsYDJ(it.DKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录在业务系统中已登记，但其BGLX为{(int)it.BGLX}(属性变更)");
							}
							WriteToDB(it);
						}
						break;
					case ETXBGLX.Fenge:
						{
							if (string.IsNullOrEmpty(it.YDKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录的原地块编码(YDKBM)未填，但其BGLX为{(int)it.BGLX}(分割)");
							}
							if (IsDKBMExistInDB(it.DKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录在业务系统中存在，但其BGLX为{(int)it.BGLX}(分割)");
							}
							if (!IsDKBMExistInDB(it.YDKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录的原地块编码{it.YDKBM}在业务系统中不存在，但其BGLX为{(int)it.BGLX}(分割)");
							}
							it.SJLY = ESjly.Chaifen;
							WriteToDB(it);
						}
						break;
					case ETXBGLX.Txbg:
						{
							if (string.IsNullOrEmpty(it.YDKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录的原地块编码(YDKBM)未填，但其BGLX为{(int)it.BGLX}(图形变更)");
							}
							if (IsDKBMExistInDB(it.DKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录在业务系统中存在，但其BGLX为{(int)it.BGLX}(图形变更)");
							}
							if (!IsDKBMExistInDB(it.YDKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录的原地块编码{it.YDKBM}在业务系统中不存在，但其BGLX为{(int)it.BGLX}(图形变更)");
							}
							it.SJLY = ESjly.Xiugai;
							WriteToDB(it);
						}
						break;
					case ETXBGLX.Hebing:
						{
							if (string.IsNullOrEmpty(it.YDKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录的原地块编码(YDKBM)未填，但其BGLX为{(int)it.BGLX}(合并)");
							}
							if (IsDKBMExistInDB(it.DKBM))
							{
								throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录在业务系统中存在，但其BGLX为{(int)it.BGLX}(合并)");
							}

							foreach (var yDKBM in it.YDKBM.Split(','))
							{
								if (!IsDKBMExistInDB(yDKBM))
								{
									throw new Exception($"[DLXX_DK:DKBM='{it.DKBM}']的记录的原地块编码{yDKBM}在业务系统中不存在，但其BGLX为{(int)it.BGLX}(合并)");
								}
								var vit = it.Clone();
								vit.YDKBM = yDKBM;
								vit.SJLY = ESjly.Hebing;
								WriteToDB(vit);
							}
						}
						break;
				}
			}
			private void DoImport(List<VecDkItem> lst)
			{
				foreach (var it in lst)
				{
					DoImport(it);
				}
			}
			private void WriteToDB(VecDkItem it)
			{
				if (it.BGLX == ETXBGLX.Sxbg)//属性变更，只修改属性不改变图形
				{
					//if (HasImported(it, false))
					//{
					//	return;
					//}
					_dic["DKMC"].ParamValue = it.DKMC;
					_dic["SYQXZ"].ParamValue = it.SYQXZ;
					_dic["DKLB"].ParamValue = it.DKLB;
					_dic["TDLYLX"].ParamValue = it.TDLYLX;
					_dic["DLDJ"].ParamValue = it.DLDJ;
					_dic["SFJBNT"].ParamValue = it.SFJBNT;
					_dic["SCMJ"].ParamValue = it.SCMJ;
					_dic["DKDZ"].ParamValue = it.DKDZ;
					_dic["DKXZ"].ParamValue = it.DKXZ;
					_dic["DKNZ"].ParamValue = it.DKNZ;
					_dic["DKBZ"].ParamValue = it.DKBZ;
					_dic["DKBZXX"].ParamValue = it.DKBZXX;
					_dic["ZJRXM"].ParamValue = it.ZJRXM;
					_dic["SCMJM"].ParamValue = it.SCMJM;
					_dic["FBFBM"].ParamValue = it.FBFBM;
					_dic["CBFMC"].ParamValue = it.CBFMC;
					_dic["ZHXGSJ"].ParamValue = DateTime.Now;

					var sql = $"{updateSql} where DKBM='{it.DKBM}'";
					_db.ExecuteNonQuery(sql, _updatePrms);
					return;
				}

				#region insert into DLXX_DK
				var ft = _fc.CreateFeature();
				ft.Shape = it.Shape;
				it.ID = Guid.NewGuid().ToString();
				IRowUtil.SetRowValue(ft, "ID", it.ID);
				IRowUtil.SetRowValue(ft, "DKBM", it.DKBM);
				IRowUtil.SetRowValue(ft, "DKMC", it.DKMC);
				IRowUtil.SetRowValue(ft, "SYQXZ", it.SYQXZ);
				IRowUtil.SetRowValue(ft, "DKLB", it.DKLB);
				IRowUtil.SetRowValue(ft, "TDLYLX", it.TDLYLX);
				IRowUtil.SetRowValue(ft, "DLDJ", it.DLDJ);
				IRowUtil.SetRowValue(ft, "SFJBNT", it.SFJBNT);
				IRowUtil.SetRowValue(ft, "SCMJ", it.SCMJ);
				IRowUtil.SetRowValue(ft, "DKDZ", it.DKDZ);
				IRowUtil.SetRowValue(ft, "DKXZ", it.DKXZ);
				IRowUtil.SetRowValue(ft, "DKNZ", it.DKNZ);
				IRowUtil.SetRowValue(ft, "DKBZ", it.DKBZ);
				IRowUtil.SetRowValue(ft, "DKBZXX", it.DKBZXX);
				IRowUtil.SetRowValue(ft, "ZJRXM", it.ZJRXM);
				IRowUtil.SetRowValue(ft, "SCMJM", it.SCMJM);
				IRowUtil.SetRowValue(ft, "ZT", (int)EDKZT.Youxiao);
				IRowUtil.SetRowValue(ft, "CJSJ", _p.CJSJ);
				IRowUtil.SetRowValue(ft, "CBFMC", it.CBFMC);
				IRowUtil.SetRowValue(ft, "QQMJ", it.QQMJ);
				IRowUtil.SetRowValue(ft, "FBFBM", it.FBFBM);
				IRowUtil.SetRowValue(ft, "YSDM", "211011");
				IRowUtil.SetRowValue(ft, "TDYT", it.TDYT);
				IRowUtil.SetRowValue(ft, "DJSJ", it.DJSJ);
				IRowUtil.SetRowValue(ft, "DJZT", (int)it.DJZT);
				IRowUtil.SetRowValue(ft, "SFQQQG", it.SFQQQG);
				IRowUtil.SetRowValue(ft, "ELHTMJ", it.YHTMJ);
				IRowUtil.SetRowValue(ft, "SJLY", (int)it.SJLY);
				_fc.Append(ft);
				#endregion

				#region 修改原地块的ZT
				if (!string.IsNullOrEmpty(it.YDKBM))
				{
					int oid = -1;
					var sql = $"select ID,{_fc.OIDFieldName} from {_tableName} where DKBM='{it.YDKBM}'";
					_db.QueryCallback(sql, r =>
					{
						it.YDKID = r.GetValue(0)?.ToString();
						oid = SafeConvertAux.ToInt32(r.GetValue(1));
						return false;
					});
					if (!string.IsNullOrEmpty(it.YDKID))
					{
						var sj = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
						sql = $"update {_tableName} set ZT={(int)EDKZT.Lishi},MSSJ='{sj}',ZHXGSJ='{sj}' where {_fc.OIDFieldName}={oid}";
					}
				}
				#endregion

				WriteToTXBGJL(it);


				_p._jzd.Import(it.DKBM);
				_p._jzx.Import(it.DKBM);
			}
			private void WriteToTXBGJL(VecDkItem it)
			{
				if (it.BGLX == ETXBGLX.Xinz)
				{//yxm 2019-10-23 新增地块不写入图形变更记录表
					return;
				}

				_dic["ID"].ParamValue = Guid.NewGuid().ToString();
				_dic["DKID"].ParamValue = it.ID;
				_dic["DKBM"].ParamValue = it.DKBM;
				_dic["YDKID"].ParamValue = it.YDKID??Guid.Empty.ToString();
				_dic["YDKBM"].ParamValue = it.YDKBM??"";
				_dic["BGFS"].ParamValue = (int)it.BGLX;
				_dic["BGYY"].ParamValue = it.BGYY;
				_db.ExecuteNonQuery(_insertSql1, _insertPrms1);

			}
			private bool IsDKBMExistInDB(string dkbm)
			{
				return _fc.Count($"DKBM='{dkbm}'") > 0;
			}
			/// <summary>
			/// 是否已登记
			/// </summary>
			/// <param name="dkbm"></param>
			/// <returns></returns>
			private bool IsYDJ(string dkbm)
			{
				return SafeConvertAux.ToInt32(_db.QueryOne($"select count(1) from ID_CBJYQ_DKXX where DKBM='{dkbm}'"))>0;
			}
			private string FF(string fieldName, string val)
			{
				return val == null ? $"{fieldName} is null" : $"{fieldName}='{val}'";
			}
			//private string FF(string fieldName, double val)
			//{
			//	return val == null ? $"{fieldName} is null" : $"{fieldName}={val}";
			//}
			/// <summary>
			/// 通过数据比较判断数据库中是否存在一条基本信息完全一样的记录
			/// </summary>
			/// <param name="it"></param>
			/// <returns></returns>
			internal bool HasImported(VecDkItem it,bool fCheckShapeField=true)
			{
				if (it.HasImported != null)
				{
					return (bool)it.HasImported;
				}
				bool fOK = false;
				var qf = new QueryFilter()
				{
					SubFields=fCheckShapeField?_fc.ShapeFieldName:_fc.OIDFieldName,
					WhereClause = $"DKBM='{it.DKBM}' and {FF("DKMC",it.DKMC)} and {FF("SYQXZ",it.SYQXZ)} and {FF("DKLB",it.DKLB)} and {FF("TDLYLX",it.TDLYLX)} and {FF("DLDJ",it.DLDJ)} and {FF("TDYT",it.TDYT)} and {FF("SFJBNT",it.SFJBNT.ToString())} and SCMJ={it.SCMJ} and {FF("DKDZ",it.DKDZ)} and {FF("DKXZ",it.DKXZ)} and {FF("DKNZ",it.DKNZ)} and {FF("DKBZ",it.DKBZ)} and {FF("ZJRXM",it.ZJRXM)} and SCMJM={it.SCMJM}"
				};
				try
				{
					_fc.Search(qf, r =>
					 {
						 if (!fCheckShapeField)
						 {
							 fOK = true;
						 }
						 else
						 {
							 var g = (r as IFeature).Shape;
							 if (g == null)
							 {
								 fOK = true;
							 }
							 else
							 {
								 if (it.Shape != null)
								 {
									 fOK = it.Shape.Area == g.Area && it.Shape.Length == g.Length;
								 }
							 }
						 }
						 return false;
					 });
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				return fOK;
			}

			public void Dispose()
			{
				_fc.Dispose();
			}
		}

		class DLXX_DK_JZD : IDisposable
		{
			protected IFeatureClass _srcFC;
			protected IFeatureClass _tgtFC;

			/// <summary>
			/// [dkbm,shp oid list]
			/// </summary>
			public readonly Dictionary<string, List<int>> Dic = new Dictionary<string, List<int>>();
			public DLXX_DK_JZD(IFeatureWorkspace db, string shpFile,string tableName= "DLXX_DK_JZD")
			{
				Dic.Clear();
				_tgtFC = db.OpenFeatureClass(tableName);
				_srcFC = ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(shpFile);
				{
					var qf = new QueryFilter()
					{
						SubFields = $"DKBM,{_srcFC.OIDFieldName}"
					};
					_srcFC.Search(qf, r =>
					{
						var oid = r.Oid;
						var oDKBM = r.GetValue(0);
						if (oDKBM != null)
						{
							foreach (var dkbm in oDKBM.ToString().Split('/'))
							{
								if (!Dic.TryGetValue(dkbm, out List<int> lst))
								{
									lst = new List<int>();
									Dic[dkbm] = lst;
								}
								lst.Add(oid);
							}
						}
						return true;
					});
				}
			}

			public virtual void Import(string dkbm)
			{
				if (!Dic.TryGetValue(dkbm, out List<int> lstOID))
				{
					return;
				}
				foreach (var oid in lstOID)
				{
					var srcFt=_srcFC.GetFeatue(oid);
					var tgtFt=_tgtFC.CreateFeature();
					tgtFt.Shape = srcFt.Shape;
					IRowUtil.SetRowValue(tgtFt, "ID", Guid.NewGuid().ToString());
					IRowUtil.SetRowValue(tgtFt, "YSDM", "211021");
					IRowUtil.SetRowValue(tgtFt, "DKBM", dkbm);
					IRowUtil.SetRowValue(tgtFt, "JZDH", IRowUtil.GetRowValue(srcFt, "JZDH"));
					IRowUtil.SetRowValue(tgtFt, "JZDLX", IRowUtil.GetRowValue(srcFt, "JZDLX"));
					IRowUtil.SetRowValue(tgtFt, "JBLX", IRowUtil.GetRowValue(srcFt, "JBLX"));
					IRowUtil.SetRowValue(tgtFt, "XZBZ", IRowUtil.GetRowValue(srcFt, "XZBZ"));
					IRowUtil.SetRowValue(tgtFt, "YZBZ", IRowUtil.GetRowValue(srcFt, "YZBZ"));
					_tgtFC.Append(tgtFt);
				}
			}
			public void Dispose()
			{
				if (_srcFC != null)
				{
					_srcFC.Dispose();
				}
				if (_tgtFC != null)
				{
					_tgtFC.Dispose();
				}
			}
		}
		class DLXX_DK_JZX : DLXX_DK_JZD
		{
			public DLXX_DK_JZX(IFeatureWorkspace db, string shpFile) :
				base(db, shpFile, "DLXX_DK_JZX")
			{
			}
			public override void Import(string dkbm)
			{
				if (!Dic.TryGetValue(dkbm, out List<int> lstOID))
				{
					return;
				}
				foreach (var oid in lstOID)
				{
					var srcFt = _srcFC.GetFeatue(oid);
					var tgtFt = _tgtFC.CreateFeature();
					tgtFt.Shape = srcFt.Shape;
					IRowUtil.SetRowValue(tgtFt, "ID", Guid.NewGuid().ToString());
					IRowUtil.SetRowValue(tgtFt, "YSDM", "211031");
					IRowUtil.SetRowValue(tgtFt, "DKBM", dkbm);
					IRowUtil.SetRowValue(tgtFt, "JZXH", IRowUtil.GetRowValue(srcFt, "JZXH"));
					IRowUtil.SetRowValue(tgtFt, "JXXZ", IRowUtil.GetRowValue(srcFt, "JXXZ"));
					IRowUtil.SetRowValue(tgtFt, "JZXLB", IRowUtil.GetRowValue(srcFt, "JZXLB"));
					IRowUtil.SetRowValue(tgtFt, "JZXWZ", IRowUtil.GetRowValue(srcFt, "JZXWZ"));
					IRowUtil.SetRowValue(tgtFt, "JZXSM", IRowUtil.GetRowValue(srcFt, "JZXSM"));
					IRowUtil.SetRowValue(tgtFt, "PLDWQLR", IRowUtil.GetRowValue(srcFt, "PLDWQLR"));
					IRowUtil.SetRowValue(tgtFt, "PLDWZJR", IRowUtil.GetRowValue(srcFt, "PLDWZJR"));
					IRowUtil.SetRowValue(tgtFt, "QJZDH", IRowUtil.GetRowValue(srcFt, "QJZDH"));
					IRowUtil.SetRowValue(tgtFt, "ZJZDH", IRowUtil.GetRowValue(srcFt, "ZJZDH"));
					_tgtFC.Append(tgtFt);
				}
			}
		}

		class DJ_CBJYQ_YDJB:InsertBase
		{
			public DJ_CBJYQ_YDJB(IFeatureWorkspace db):base(db, "DJ_CBJYQ_YDJB",null
				, "ID,DJBID,YDJBID,DJLX,DJXL,BGLX")
			{
			}
			public void DoImport(IFeatureWorkspace db,string djbID,string yDjbID,int djlx,int djxl)
			{
				var id = Guid.NewGuid().ToString();
				_dic["ID"].ParamValue = id;
				_dic["DJBID"].ParamValue =djbID;
				_dic["YDJBID"].ParamValue = yDjbID;
				_dic["DJLX"].ParamValue =djlx;
				_dic["DJXL"].ParamValue = djxl;
				_dic["BGLX"].ParamValue =5;
				try
				{
					db.ExecuteNonQuery(insertSql, _insertPrms);
				}
				catch (Exception ex)
				{
					throw new Exception($"err at import DJ_CBJYQ_YDJB:{ex.Message}");
				}
			}
		}
		class DJ_CBJYQ_DJB: InsertBase
		{
			private readonly Temp _p;
			public DJ_CBJYQ_DJB(Temp p)
				:base(p._db, "DJ_CBJYQ_DJB",null
					 , "ID,CBJYQZBM,FBFBM,CBFBM,CBFMC,CBFS,CBQXQ,CBQXZ,DKSYT,CBQX,YCBJYQZBH,CBJYQZLSH,FJ,DBR,DJSJ,QSZT,DJYY,DYZT,YYZT,QXDM,SZDY,QLLX,DJLX,DJXL")
			{
				_p = p;
			}

			public void DoImport(DjbItem it)
			{
				ImportTo_DJ_CBJYQ_DJB(it);
				//_p._QSSJ_CBJYQZDJB.DoImport(it);
				//var djCbfID= _p._cbf.DoImport( it);
				//_p._cbfJtcy.DoImport(it,djCbfID);
				//_p._cbht.DoImport(it);
				//_p._qz.DoImport(it);
				//_p._cbdkxx.DoImport(it);
			}

			/// <summary>
			/// 执行后：it.ID=Guid.NewGuid().ToString();
			/// </summary>
			/// <param name="it"></param>
			private void ImportTo_DJ_CBJYQ_DJB(DjbItem it)
			{
				var id = Guid.NewGuid().ToString();
				int nDJLX = 8;//更正登记
				int nDJXL = 13;//更正登记
				if (it.ID != null)
				{
					_db.ExecuteNonQuery($"update DJ_CBJYQ_DJB set QSZT=2 where ID='{it.ID}'");
					_p._ydjb.DoImport(_db, id, it.ID, nDJLX, nDJXL);
				}
				it.ID = id;
				_dic["ID"].ParamValue = id;
				_dic["CBJYQZBM"].ParamValue = it.CBJYQZBM;
				_dic["FBFBM"].ParamValue = it.FBFBM;
				_dic["CBFBM"].ParamValue = it.CBFBM;
				_dic["CBFMC"].ParamValue = it.CBFMC;
				_dic["CBFS"].ParamValue = it.CBFS;
				_dic["CBQXQ"].ParamValue = it.CBQXQ;
				_dic["CBQXZ"].ParamValue = it.CBQXZ;
				_dic["DKSYT"].ParamValue = it.DKSYT;
				_dic["CBQX"].ParamValue = it.CBQX;
				_dic["YCBJYQZBH"].ParamValue = it.YCBJYQZBH;
				_dic["CBJYQZLSH"].ParamValue = it.CBJYQZLSH;
				_dic["FJ"].ParamValue = it.DJBFJ;
				_dic["DBR"].ParamValue = it.DBR;
				_dic["DJSJ"].ParamValue = it.DJSJ;
				_dic["QSZT"].ParamValue = 1;
				_dic["DYZT"].ParamValue = 0;
				_dic["YYZT"].ParamValue = 0;
				_dic["QLLX"].ParamValue = 0;
				_dic["DJLX"].ParamValue = nDJLX;
				_dic["DJXL"].ParamValue = nDJXL;//更正登记
				_dic["DJYY"].ParamValue = "批量更新入库";
				_dic["QXDM"].ParamValue = it.FBFBM.Substring(0, 6);
				_dic["SZDY"].ParamValue = it.FBFBM.Substring(0, 12);
				_db.ExecuteNonQuery(insertSql, _insertPrms);
				//return id;
			}
		}

		class Temp:IDisposable
		{
			public readonly DLXX_DK _DLXX_DK;
			public readonly  DLXX_DK_JZD _jzd;
			public readonly DLXX_DK_JZX _jzx;
			public readonly VecDkCache _dkCache = new VecDkCache();
			public readonly DataUpdateTask p;

			public readonly DJ_CBJYQ_YDJB _ydjb;
			public readonly CBF _cbf;
			public readonly CBF_JTCY _cbfJtcy;
			public readonly CBHT _cbht;
			//public readonly QSSJ_CBJYQZDJB _QSSJ_CBJYQZDJB;
			public readonly CBJYQZ _qz;
			public readonly CBDKXX _cbdkxx;

			public readonly IWorkspace _mdb;
			public readonly IFeatureWorkspace _db;

			/// <summary>
			/// 所有登记簿
			/// [CBJYQZBM,DjbItem]
			/// </summary>
			public readonly Dictionary<string,DjbItem> djbItems = new Dictionary<string, DjbItem>();
			/// <summary>
			/// 所有承包合同
			/// [CBHTBM,CbhtItem]
			/// </summary>
			public readonly Dictionary<string,CbhtItem> cbhtItems = new Dictionary<string, CbhtItem>();

			/// <summary>
			/// 所有承包方
			/// [CBFBM,CbfItem]
			/// </summary>
			public readonly Dictionary<string,CbfItem> cbfItems = new Dictionary<string, CbfItem>();

			/// <summary>
			/// 所有不在CBDKXX表中的矢量地块数据
			/// </summary>
			public readonly List<VecDkItem> vecDkItems = new List<VecDkItem>();

			/// <summary>
			/// CBDKXX中出现的所有地块
			/// [DKBM]
			/// </summary>
			public readonly HashSet<string> setDKBMinDkxx = new HashSet<string>();

			#region 统计项
			/// <summary>
			/// 无登记簿的承包合同总数
			/// </summary>
			public int CountCbhtNoDjb;
			/// <summary>
			/// 孤立的承包方总数（既不在承包合同中也不在登记不中）
			/// </summary>
			public int CountCbfAlone;
			/// <summary>
			/// 无地块信息的矢量地块总数
			/// </summary>
			public int CountVecDkNoDkxx { get { return vecDkItems.Count; } }
			#endregion

			/// <summary>
			/// 创建时间
			/// </summary>
			public readonly DateTime CJSJ = DateTime.Now;

			public readonly DataUpdatePanel _param;
			public Temp(DataUpdateTask p_,IFeatureWorkspace db,IWorkspace mdb, DataUpdatePanel prm)//, string dkShpFile)//,  VecDkCache dkCache)
			{
				p = p_;
				_db = db;
				_param = p.PropertyPage as DataUpdatePanel;
				_mdb = mdb;
				_ydjb = new DJ_CBJYQ_YDJB(db);
				_cbf = new CBF(this);
				_cbfJtcy = new CBF_JTCY(this);
				_cbht = new CBHT(this);
				//_QSSJ_CBJYQZDJB = new QSSJ_CBJYQZDJB(db);
				_qz = new CBJYQZ(this);
				_cbdkxx = new CBDKXX(this, db);

				_DLXX_DK = new DLXX_DK(this);
				_jzd = new DLXX_DK_JZD(db, prm.dicShp["JZD"]);
				_jzx = new DLXX_DK_JZX(db, prm.dicShp["JZX"]);

				using (var fcShpDK = ShapeFileFeatureWorkspaceFactory.Instance.OpenFeatureClass2(prm.dicShp["DK"], "rb"))
				{
					_dkCache.Init(fcShpDK, mdb);
				}
			}
			public void ReportError(string err)
			{
				p.ReportError(err);
			}

			public void Dispose()
			{
				_DLXX_DK.Dispose();
				_jzd.Dispose();
				_jzx.Dispose();
			}
		}



		public DataUpdateTask()
		{
			Name = "批量数据更新";
			Description = "导入批量更新数据包";
			PropertyPage = new DataUpdatePanel();
			base.OnStart += t =>ReportInfomation($"开始{Name}");
			base.OnFinish += (t,e) => ReportInfomation($"结束{Name}，耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = PropertyPage as DataUpdatePanel;
			try
			{
				var db = MyGlobal.Workspace;

				using (var mdb = DBAccess.Open(prm.mdbFileName))
				using (var tmp = new Temp(this, db, mdb, prm))
				{
					var djb = new CBJYQZDJB(tmp);
					var lst = tmp.djbItems;

					#region Fill tmp items
					#region Fill tmp.djbItems
					//var sql = "select CBJYQZBM,FBFBM,CBJYQZDJB.CBFBM,CBFS,CBQXQ,CBQXZ,DKSYT,CBQX,YCBJYQZBH,CBJYQZLSH,DJBFJ,DBR,DJSJ,CBF.CBFMC from CBJYQZDJB left join CBF on CBF.CBFBM=CBJYQZDJB.CBFBM";
					var sql = $"select {djb._mdbFields} from CBJYQZDJB";
					mdb.QueryCallback(sql, r =>
					 {
						 var it = CBJYQZDJB.FillItem(r);
						 lst[it.CBJYQZBM] = it;
						 return true;
					 }, cancel);
					#endregion

					#region Fill cbjyqzItems
					sql = $"select {tmp._qz._mdbFields} from CBJYQZ";
					mdb.QueryCallback(sql, r =>
					{
						var it = CBJYQZ.FillItem(r);
						if (tmp.djbItems.TryGetValue(it.CBJYQZBM, out DjbItem dit))
						{
							dit.cbjyqzItem = it;
							it.parent = dit;
						}
						else
						{
							throw new Exception($"【原数据逻辑错误】CBJYQZ表中CBJYQZBM='{it.CBJYQZBM}'的记录在CBJYQZDJB中无对应记录");
						}
						return true;
					}, cancel);
					#endregion

					#region Fill tmp.cbhtItems
					sql = $"select {tmp._cbht._mdbFields} from CBHT";
					mdb.QueryCallback(sql, r =>
					 {
						 var it = new CbhtItem();
						 CBHT.FillItem(r, it);
						 tmp.cbhtItems[it.CBHTBM] = it;
						 if (lst.TryGetValue(it.CBHTBM, out DjbItem dit))
						 {
							 dit.cbhtItem = it;
							 it.parent = dit;
						 }
						 else
						 {
							 ++tmp.CountCbhtNoDjb;
						 }
						 return true;
					 }, cancel);
					#endregion

					#region Fill tmp.cbfItems
					sql = $"select {tmp._cbf._mdbFields} from CBF";
					mdb.QueryCallback(sql, r =>
					{
						var it=CBF.FillItem(r);
						tmp.cbfItems[it.CBFBM] = it;
						foreach(var kv in tmp.cbhtItems)
						{
							var cbht = kv.Value;
							if (cbht.CBFBM == it.CBFBM)
							{
								it.parent =cbht;
								cbht.cbfItem = it;
								break;
							}
						}
						if (it.parent == null)
						{
							foreach (var kv in tmp.djbItems)
							{
								if (it.CBFBM == kv.Value.CBFBM)
								{
									throw new Exception($"CBJYQZDJB中CBFBM='{it.CBFBM}'的承包方在CBHT中不存在！");
								}
							}
						}

						if (it.parent == null)
						{
							++tmp.CountCbfAlone;
						}
						return true;
					}, cancel);
					#endregion

					#region Fill cbfJtcyItems
					sql = $"select {tmp._cbfJtcy._mdbFields} from CBF_JTCY";
					mdb.QueryCallback(sql, r =>
					{
						var it=CBF_JTCY.FillItem(r);
						if (tmp.cbfItems.TryGetValue(it.CBFBM, out CbfItem cit))
						{
							it.parent = cit;
							cit.cbfJtcyItems.Add(it);
						}
						return true;
					}, cancel);
					#endregion

					#region Fill cbdkxxItems
					sql = $"select {tmp._cbdkxx._mdbFields} from CBDKXX";
					mdb.QueryCallback(sql, r =>
					{
						var it=CBDKXX.FillItem(r);
						tmp.setDKBMinDkxx.Add(it.DKBM);
						if (tmp.cbhtItems.TryGetValue(it.CBHTBM, out CbhtItem cit))
						{
							it.parent = cit;
							cit.dkxxItems.Add(it);
						}
						else
						{
							throw new Exception($"CBDKXX表中CBHTBM={it.CBHTBM}的记录在CBHT表中无对应记录");
						}
						return true;
					}, cancel);
					#endregion

					#region Fill tmp.vecDkItems
					foreach (var kv in tmp._dkCache.Dic)
					{
						if (!tmp.setDKBMinDkxx.Contains(kv.Key))
						{
							tmp.vecDkItems.Add(kv.Value);
						}
					}
					#endregion
					#endregion

					//int i = 0;
					//double iOldProgress = 0;
					var cnt = lst.Count + tmp.CountCbhtNoDjb
						+ tmp.CountCbfAlone
						+tmp.CountVecDkNoDkxx
						;

					var progress = new ProgressReporter(ReportProgress, cnt);
					//ReportProgress(0);

					#region 处理登记簿中的数据
					foreach (var kv in lst)
					{
						var it = kv.Value;
						//ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++i, ref iOldProgress);
						progress.Step();

						string id = null;
						short dyzt = 0;

						var fields = djb._mdbFields.Replace(",DJBFJ,", ",");
						var sql1 = $"select {fields},ID,DYZT from DJ_CBJYQ_DJB where CBJYQZBM ='{it.CBJYQZBM}' and qszt=1";
						db.QueryCallback(sql1, r1 =>
						{
							it.HasImported = true;
							var it1 = new DjbItem()
							{
								//CBJYQZBM = Util.GetString(r1, 0),
								FBFBM = Util.GetString(r1, 1),
								CBFBM = Util.GetString(r1, 2),
								CBFS = Util.GetString(r1, 3),
								CBQXQ = Util.GetDateTime(r1, 4),
								CBQXZ = Util.GetDateTime(r1, 5),
								DKSYT = Util.GetString(r1, 6),
								CBQX = Util.GetString(r1, 7),
								YCBJYQZBH = Util.GetString(r1, 8),
								CBJYQZLSH = Util.GetString(r1, 9),
								//DJBFJ = Util.GetString(r1, 10),
								DBR = Util.GetString(r1, 10),
								DJSJ = Util.GetDateTime(r1, 11),
							};
							if (it.FBFBM != it1.FBFBM || it.CBFBM != it1.CBFBM || it.CBFS != it1.CBFS || !Util.Equal(it.CBQXQ, it1.CBQXQ)
								|| !Util.Equal(it.CBQXZ, it1.CBQXZ) || it1.DKSYT != it.DKSYT || it.CBQX != it1.CBQX || it.YCBJYQZBH != it1.YCBJYQZBH
								|| it.CBJYQZLSH != it1.CBJYQZLSH ||it.DBR != it1.DBR || !Util.Equal(it.DJSJ, it1.DJSJ))
							{
								it.HasImported = false;
							}
							id = r1.GetString(12);
							dyzt = SafeConvertAux.ToShort(r1.GetValue(13));
							return false;
						});
						if (id != null)
						{
							if (dyzt == 1)
							{
								ReportWarning($"权证编码为{it.CBJYQZBM}的记录已抵押，不能执行导入");
								continue;
							}
						}
						it.ID = id;

						if (it.cbjyqzItem != null)
						{
							sql1 = $"select {tmp._qz._mdbFields} from QSSJ_CBJYQZ where CBJYQZBM='{it.cbjyqzItem.CBJYQZBM}'";
							db.QueryCallback(sql1, r =>
							 {
								 var jt = it.cbjyqzItem;
								 jt.HasImported = true;
								 var it1 = new CbjyqzItem()
								 {
									 //CBJYQZBM = Util.GetString(r, 0),
									 FZJG = Util.GetString(r, 1),
									 FZRQ = Util.GetDateTime(r, 2),
									 QZSFLQ = Util.GetString(r, 3),
									 QZLQRQ = Util.GetDateTime(r, 4),
									 QZLQRXM = Util.GetString(r, 5),
									 QZLQRZJLX = Util.GetString(r, 6),
									 QZLQRZJHM = Util.GetString(r, 7),
								 };
								 if (jt.FZJG != it1.FZJG || !Util.Equal(jt.FZRQ, it1.FZRQ) || jt.QZSFLQ != it1.QZSFLQ || !Util.Equal(jt.QZLQRQ, it1.QZLQRQ)
									 || jt.QZLQRXM != it1.QZLQRXM || jt.QZLQRZJLX != it1.QZLQRZJLX || jt.QZLQRZJHM != it1.QZLQRZJHM)
								 {
									 jt.HasImported = false;
								 }
								 
								 return false;
							 });
						}
						it.cbhtItem?.CheckImported(tmp);

						if (it.AllImported())
						{
							ReportInfomation($"CBJYQZBM='{it.CBJYQZBM}'的数据已经导入系统!");
						}
						else
						{
							Process(tmp, djb, it);
							//ReportInfomation($"CBJYQZBM='{it.CBJYQZBM}'的数据未导入系统!");
						}
						//Process(djb, it);
						//ProgressUtil.ReportProgress(base.ReportProgress, cnt, i, ref iOldProgress);
					}
					#endregion

					#region 处理无登记簿的承包合同
					foreach (var kv in tmp.cbhtItems)
					{
						if (kv.Value.parent == null)
						{
							Process(tmp, kv.Value);
							//ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++i, ref iOldProgress);
							progress.Step();
						}
					}
					//tmp._cbht.Import( n => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++i, ref iOldProgress));
					#endregion

					#region 处理无承包合同的承包方
					foreach (var kv in tmp.cbfItems)
					{
						if (kv.Value.parent == null)
						{
							Process(tmp, kv.Value);
							//ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++i, ref iOldProgress);
							progress.Step();
						}
					}
					//tmp._cbf.Import( n => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++i, ref iOldProgress));
					#endregion

					#region 处理不在CBDKXX中的矢量地块
					foreach (var dk in tmp.vecDkItems)
					{
						progress.Step();
						//ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++i, ref iOldProgress);
						//tmp._DLXX_DK.DoImport(dk);
						Process(tmp, dk);
					}
					#endregion

					progress.ForceFinish();
				}
			}
			catch (Exception ex)
			{
				ReportException(ex);
			}
		}

		/// <summary>
		/// 导入登记簿及相关数据（CBJYQZ、CBHT、CBF、CBF_JTCY、CBDKXX、）
		/// </summary>
		/// <param name="t"></param>
		/// <param name="djb"></param>
		/// <param name="it"></param>
		private void Process(Temp t,CBJYQZDJB djb,DjbItem it)
		{
			var db = t._db;
			try
			{
				if (it.cbhtItem == null)
				{
					throw new Exception($"CBJYQZDJB中CBJYQZBM='{it.CBJYQZBM}'的记录无对应承包合同！");
				}
				if (it.cbhtItem.cbfItem == null)
				{
					throw new Exception($"CBJYQZDJB中CBJYQZBM='{it.CBJYQZBM}'的记录无对应承包方数据！");
				}
				db.BeginTransaction();

				djb.DoImport(it);
				//t._ydjb.DoImport(t._db,it.ID,it.
				t._qz.DoImport(it);

				t._cbht.DoImport(it.cbhtItem);

				var djCbfID = t._cbf.DoImport(it.cbhtItem.cbfItem);
				t._cbfJtcy.DoImport(it.cbhtItem.cbfItem, djCbfID);

				var lstDk=t._DLXX_DK.DoImport(it.cbhtItem);
				t._cbdkxx.DoImport(it.cbhtItem);

				db.Commit();
				foreach (var dk in lstDk)
				{
					dk.HasImported = true;
				}
				t._cbf.WriteQSZL(it.cbhtItem.cbfItem);
				ReportInfomation($"成功导入登记簿数据：CBJYQZBM='{it.CBJYQZBM}'");
			}
			catch (Exception ex)
			{
				ReportError($"[CBJYQZDJB: CBJYQZBM='{it.CBJYQZBM}']"+ex.Message);
				//ReportException(ex);
				db.Rollback();
			}
		}

		/// <summary>
		/// 导入无登记簿的承包合同及相关数据
		/// </summary>
		/// <param name="t"></param>
		/// <param name="it"></param>
		private void Process(Temp t, CbhtItem it)
		{
			var db = t._db;//
			try
			{
				if (it.cbfItem == null)
				{
					throw new Exception($"CBHT中CBHTBM='{it.CBHTBM}'的记录无对应承包方数据！");
				}
				db.BeginTransaction();


				t._cbht.DoImport(it);

				var djCbfID = t._cbf.DoImport(it.cbfItem);
				t._cbfJtcy.DoImport(it.cbfItem, djCbfID);

				var lstDk=t._DLXX_DK.DoImport(it);//, it.CBFMC, it.DJSJ);
				t._cbdkxx.DoImport(it);

				db.Commit();
				foreach (var dk in lstDk)
				{
					dk.HasImported = true;
				}
				t._cbf.WriteQSZL(it.cbfItem);
			}
			catch (Exception ex)
			{
				ReportError($"[CBHT:CBHTBM='{it.CBHTBM}']:" + ex.Message);
				db.Rollback();
			}
		}

		/// <summary>
		/// 导入无承包合同的承包方数据
		/// </summary>
		/// <param name="t"></param>
		/// <param name="it"></param>
		private void Process(Temp t, CbfItem it)
		{
			var db = t._db;
			try
			{
				db.BeginTransaction();
				var djCbfID = t._cbf.DoImport(it);
				t._cbfJtcy.DoImport(it, djCbfID);
				db.Commit();
				t._cbf.WriteQSZL(it);
			}
			catch (Exception ex)
			{
				ReportError($"[CBF：CBFBM='{it.CBFBM}']" + ex.Message);
				db.Rollback();
			}
		}

		/// <summary>
		/// 导入无对应DKXX的矢量地块
		/// </summary>
		/// <param name="t"></param>
		/// <param name="it"></param>
		private void Process(Temp t, VecDkItem it)
		{
			var db = t._db;
			try
			{
				db.BeginTransaction();
				t._DLXX_DK.DoImport(it);
				db.Commit();
			}
			catch (Exception ex)
			{

				ReportError(ex.Message);// $"[DKBM]='{it.DKBM}' err:" + ex.Message);
				db.Rollback();
			}
		}

		class Util
		{

			public static string GetString(IDataReader r, int c)
			{
				return r.IsDBNull(c) ? null : r.GetString(c);
			}
			public static string GetString(System.Data.IDataReader r, int c)
			{
				return r.IsDBNull(c) ? null : r.GetString(c);
			}

			public static DateTime? GetDateTime(IDataReader r, int c)
			{
				if (r.IsDBNull(c)) return null;
				var o = r.GetValue(c);
				if (o != null)
				{
					if (DateTime.TryParse(o.ToString(), out DateTime dt))
					{
						return dt;
					}
				}
				return null;
			}
			public static DateTime? GetDateTime(System.Data.IDataReader r, int c)
			{
				if (r.IsDBNull(c)) return null;
				return r.GetDateTime(c);
			}
			public static double GetDouble(System.Data.IDataReader r, int c)
			{
				if (r.IsDBNull(c)) return 0;
				return SafeConvertAux.ToDouble(r.GetValue(c));
			}
			public static double GetDouble(IDataReader r, int c)
			{
				if (r.IsDBNull(c)) return 0;
				return SafeConvertAux.ToDouble(r.GetValue(c));
			}
			public static int GetInt(System.Data.IDataReader r, int c)
			{
				if (r.IsDBNull(c)) return 0;
				return SafeConvertAux.ToInt32(r.GetValue(c));
			}
			public static int GetInt(IDataReader r, int c)
			{
				if (r.IsDBNull(c)) return 0;
				return SafeConvertAux.ToInt32(r.GetValue(c));
			}

			public static string QueryCbfmcByCbfbm(DBAccess db, string cbfbm)
			{
				string cbfmc = null;
				db.QueryCallback($"select CBFMC from CBF where CBFBM='{cbfbm}'", r =>
				 {
					 cbfmc = GetString(r, 0);
					 return false;
				 });
				return cbfmc;
			}

			public static bool Equal(DateTime? a, DateTime? b)
			{
				if (a == null && b == null)
				{
					return true;
				}
				if (a != null && b != null)
				{
					return a.ToString() == b.ToString();
				}
				return false;
			}
			public static bool Equal(string a, string b)
			{
				if (a != null && b != null)
					return a.Trim()==b.Trim();

				return StringUtil.isEqual(a, b);
			}
			public static bool Equal(double a, double b)
			{
				return Math.Abs(a - b) < 0.00001;
			}

			public static int Compare(string a, string b)
			{
				if (a != null && b != null)
				{
					return a.CompareTo(b);
				}
				if (a == null && b == null)
				{
					return 0;
				}
				if (a == null)
				{
					return -1;
				}
				return 1;
			}
			//public static object ParamVal(object s)
			//{
			//	if (s == null)
			//	{
			//		return DBNull.Value;
			//	}
			//	return s;
			//}
		}
	}


}
