using Agro.LibMapServer;
using Agro.Library.Model;
using DataGovenServer.Repository;
using DataGovenServer.Service.LandDotCoil;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DataGovenServer.Service
{
	class ImportDcdkService
	{
		private static ImportDcdkService _instance;
		private readonly IFeatureWorkspace db;
		private readonly DlxxDkRepository dlxxDkRepos;
		private readonly XtpzLshRepository xtpzLshRepository;
		private readonly TxbgjlRepository bgjlRepos;
		private readonly JzxRepository jzxRepos;
		private readonly JzdRepository jzdRepos;
		private ImportDcdkService(IFeatureWorkspace db) {
			this.db = db;
			dlxxDkRepos = DlxxDkRepository.Instance(db);
			xtpzLshRepository = XtpzLshRepository.Instance(db);
			bgjlRepos = TxbgjlRepository.Instance(db);
			jzxRepos = JzxRepository.Instance(db);
			jzdRepos = JzdRepository.Instance(db);
		}
		public static ImportDcdkService Instance(IFeatureWorkspace db) {
			if (_instance == null)
			{
				_instance = new ImportDcdkService(db);
			}
			return _instance;
		}

		public void Append(IDbConnection con, EBGLX bglx, DLXX_DK dkEn, string BGYY = null)
		{
			if (string.IsNullOrEmpty(dkEn.FBFBM))
			{
				throw new Exception("FBFBM必填！");
			}
			if (0 == SafeConvertAux.ToInt32(db.QueryOne(con, $"select count(*) from QSSJ_FBF where FBFBM='{dkEn.FBFBM}'")))
			{
				throw new Exception($"FBFBM：{dkEn.FBFBM}无效！");
			}
			var sNewDKBM = xtpzLshRepository.QueryNewDKBM(con, dkEn.FBFBM);
			if (string.IsNullOrEmpty(sNewDKBM))
			{
				throw new Exception("自动生成地块编码失败，请检查发包方编码或数据库配置(XTPZ_LSH)是否正确！");
			}
			var oldDkID = dkEn.BSM;
			//var dkEn = en;
			dkEn.SJLY = FromBGLX(bglx);
			dkEn.DKBM = sNewDKBM;
			dkEn.ZT =(int)(bglx == EBGLX.Xinz ? EDKZT.Youxiao : EDKZT.Lins);
			dkEn.DJZT = (int)EDjzt.Wdj;
			if (dkEn.SCMJ <= 0)
			{
				dkEn.SCMJ = (decimal)Math.Round(dkEn.Shape.Area, 2);
			}
			if (dkEn.SCMJM == 0 || dkEn.SCMJM == null)
			{
				dkEn.SCMJM = (decimal)Math.Round((double)dkEn.SCMJ * 0.0015, 2);
			}

			#region 写入数据库（DK,JZD,JZX)
			var kjzb = ImportJzdJzx(con, dkEn.DKBM, sNewDKBM, dkEn.ZJRXM, dkEn.CBFMC, dkEn.Shape);
			dkEn.KJZB = kjzb;
			var dkOid = dlxxDkRepos.Insert(con, dkEn);
			if (dkOid != null)
			{
				dkEn.BSM = (int)dkOid;
			}
			#endregion

			#region 写入变更登记表（对非新增对象）
			if (bglx != EBGLX.Xinz)
			{
				var oldEn = dlxxDkRepos.Find(con, it => it.BSM == oldDkID, (c, t) => c(t.ID, t.DKBM));
				if (oldEn != null)
				{
					var enBgjl = new DLXX_DK_TXBGJL
					{
						YDKID = oldEn.ID,
						YDKBM = oldEn.DKBM,
						DKID = dkEn.ID,
						DKBM = dkEn.DKBM,
						BGFS = bglx,
						BGYY = BGYY
					};

					bgjlRepos.Insert(con, enBgjl);
				}
			}
			#endregion
		}
		public void Delete(IDbConnection con, int oid)
		{
			var en=dlxxDkRepos.FindNoBinary(con, t => t.BSM == oid);
			if (en == null)
			{
				throw new Exception($"BSM={oid}的记录不存在！");
			}
			var err = CanDelete(con,en);
			if (err != null) throw new Exception(err);
			dlxxDkRepos.Delete(con, t => t.BSM == oid);
			jzxRepos.Delete(con, t => t.DKBM == en.DKBM);
			jzdRepos.Delete(con, t => t.DKBM == en.DKBM);
			bgjlRepos.Delete(con, t => t.DKBM == en.DKBM);
		}
		/// <summary>
		/// yxm 2019-4-28
		/// 导入界址点和界址线
		/// </summary>
		private string ImportJzdJzx(IDbConnection con, string oriDkbm, string dkbm, string zjrXm, string cbfMc, IGeometry dkShape)
		{
			string kjzb = null;//空间坐标（/分隔）
			var param = new DotCoilBuilderParam();
			var t = new DotCoilBuilder(param);
			param.Dk.OriDkbm = oriDkbm;
			param.Dk.ZjrXm = zjrXm;
			param.Dk.CbfMc = cbfMc;
			param.Dk.Shape = dkShape;
			param.Dk.Dkbm = dkbm;

			var dkFc = db.OpenFeatureClass(DLXX_DK.GetTableName());
			var jzdFc = db.OpenFeatureClass(DLXX_DK_JZD.GetTableName());
			var jzxFc = db.OpenFeatureClass(DLXX_DK_JZX.GetTableName());
			//var jzdFCMeta = db.GetFeatureClassMeta(DLXX_DK_JZD.GetTableName());
			//var jzxFCMeta = db.GetFeatureClassMeta(DLXX_DK_JZX.GetTableName());
			//var dkFCMeta = dlxxDkRepos.tableMeta as FeatureClassMeta;
			var srid = dkFc.Meta.SRID;
			var cancel = NotCancelTracker.Instance;
			var qf = new SpatialQueryFilter()
			{
				SubFields = $"DKBM,ZJRXM,CBFMC,{dkFc.Meta.ShapeFieldName}",
				SpatialRel = SpatialRelEnum.SpatialRelEnvelopeIntersects,
				Where = "ZT =1"
			};

			#region intersect query from DLXX_DK
			//using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK"))
			{
				var env = dkShape.EnvelopeInternal.Copy();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geo = GeometryUtil.MakePolygon(env,srid);
					dkFc.SpatialQuery(con,qf,(oid,pt,r) =>
					{
						var sDKBM = SqlUtil.SafeGetString(r,0);
							if (sDKBM != param.Dk.OriDkbm)
							{
								var sZJRXM =SqlUtil.SafeGetString(r,1);
								var sCBFMC =SqlUtil.SafeGetString(r,2);
								param.AddAroundDk(pt, sZJRXM, sCBFMC);
							}
					},cancel);
				}
			}
			#endregion

			#region intersect query from DLXX_DK_JZD
			//using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK_JZD"))
			{
				qf.Where = null;
				qf.SubFields = $"JZDH,{jzdFc.Meta.ShapeFieldName}";
				qf.SpatialRel = SpatialRelEnum.SpatialRelEnvelopeIntersects;
				
				var env = dkShape.EnvelopeInternal.Copy();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geo = GeometryUtil.MakePolygon(env,srid);
					jzdFc.SpatialQuery(con,qf,(oid, geo,r) =>
					{
						if (geo is IPoint pt)
						{
							var jzdh = SqlUtil.SafeGetString(r, 0);// SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "JZDH"));
							param.AddJzd(new Coordinate(pt.X, pt.Y), jzdh);
						}
					},cancel);
				}
			}
			#endregion

			#region intersect query from DLXX_DK_JZX
			//using (var fc = (db as IFeatureWorkspace).OpenFeatureClass("DLXX_DK_JZX"))
			{
				qf.SubFields = $"JZXH,PLDWZJR,PLDWQLR,QJZDH,ZJZDH,{jzxFc.Meta.ShapeFieldName}";
					qf.SpatialRel = SpatialRelEnum.SpatialRelEnvelopeIntersects;
				var env = dkShape.EnvelopeInternal.Copy();
				env.ExpandBy(param.Tolerance);
				{//查询与env不相离的所有界址点
					qf.Geo = GeometryUtil.MakePolygon(env,srid);
					jzxFc.SpatialQuery(con,qf,(oid,geo,r) =>
					{
						if (geo is ILineString pt)
						{
							var jzxh =SqlUtil.SafeGetString(r,0);
							var pldwZjr = SqlUtil.SafeGetString(r, 1);// SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "PLDWZJR"));
							var pldwQlr = SqlUtil.SafeGetString(r, 2);// SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "PLDWQLR"));
							var qjzdh = SqlUtil.SafeGetString(r, 3);// SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "QJZDH"));
							var zjzdh = SqlUtil.SafeGetString(r, 4);// SafeConvertAux.ToStr(IRowUtil.GetRowValue(ft1, "ZJZDH"));
							param.AddJzx(pt, jzxh, pldwZjr, pldwQlr, qjzdh, zjzdh);
						}
					});
				}
			}
			#endregion

			var res = t.Build(srid);

			var xiangBM = dkbm.Substring(0, 9);
			int nMaxJzdh =xtpzLshRepository.GetMaxJzdh(con,xiangBM);
			int nMaxJzxh = xtpzLshRepository.GetMaxJzxh(con,xiangBM);
			//using (var jzdFc = db.OpenFeatureClass("DLXX_DK_JZD"))
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
					ft.Shape = GeometryUtil.MakePoint(j.Shape,srid);
					ft.SetValue("JZDH", j.Jzdh);
					ft.SetValue("DKBM", param.Dk.Dkbm);
					ft.SetValue("JZDLX", "3");
					ft.SetValue("JBLX", "6");
					ft.SetValue("YSDM", "211021");
					ft.SetValue("ID", id);
					ft.SetValue("DKBM", param.Dk.Dkbm);
					ft.SetValue("XZBZ", Math.Round(j.Shape.X, 3));
					ft.SetValue("YZBZ", Math.Round(j.Shape.Y, 3));
					jzdFc.Append(con,ft);
				}
				xtpzLshRepository.UpdateMaxJzdh(con, xiangBM, nMaxJzdh);
			}
			//using (var jzxFc = db.OpenFeatureClass("DLXX_DK_JZX"))
			{
				foreach (var j in res.lstJzx)
				{
					var ft = jzxFc.CreateFeature();
					ft.Shape = j.Shape;

					if (string.IsNullOrEmpty(j.Jzxh))
					{
						j.Jzxh = (++nMaxJzxh).ToString();
					}
					ft.SetValue("ID", Guid.NewGuid().ToString());
					ft.SetValue("DKBM", param.Dk.Dkbm);

					var qJzdh = string.IsNullOrEmpty(j.sQjzd) ? j.Qjzd.Jzdh : j.sQjzd;
					var zJzdh = string.IsNullOrEmpty(j.sZjzd) ? j.Zjzd.Jzdh : j.sZjzd;
					ft.SetValue("QJZDH", qJzdh);
					ft.SetValue("ZJZDH", zJzdh);
					ft.SetValue("JZXH", j.Jzxh);
					ft.SetValue("PLDWZJR", j.PldwZjr);
					ft.SetValue("PLDWQLR", j.PldwQlr);
					ft.SetValue("YSDM", "211031");
					ft.SetValue("JXXZ", "600009");
					ft.SetValue("JZXLB", "08");
					ft.SetValue("JZXWZ", "2");
					jzxFc.Append(con,ft);
				}
				xtpzLshRepository.UpdateMaxJzxh(con,xiangBM, nMaxJzxh);
			}

			return kjzb;
		}
		private string CanDelete(IDbConnection con, DLXX_DK en)
		{
			if (en.DJZT == (int)EDjzt.Ydj)
				return "该地块状态为已登记，不允许删除！";
			if (en.MSSJ != null)
			{
				return "该地块已灭失，不允许删除！";
			}
			if (en.SJLY == ESjly.Cs)
			{
				return "该地块为初始汇交数据导入，不允许删除！";
			}
			var lst=bgjlRepos.FindAll(con, t => t.YDKBM == en.DKBM);
			if (lst != null && lst.Count > 0)
			{				
				return $"该地块已发生变更，对应的变更地块编码（DKBM）为：[{string.Join(",", lst.Select(it=>it.DKBM))}]";
			}
			return null;
		}

		private static ESjly FromBGLX(EBGLX bglx)
		{
			var eSjly = ESjly.Cs;
			switch (bglx)
			{
				case EBGLX.Fenge: eSjly = ESjly.Chaifen; break;
				case EBGLX.Hebing: eSjly = ESjly.Hebing; break;
				case EBGLX.Xinz: eSjly = ESjly.Xinz; break;
				case EBGLX.Txbg: eSjly = ESjly.Xiugai; break;
			}
			return eSjly;
		}
	}
}
