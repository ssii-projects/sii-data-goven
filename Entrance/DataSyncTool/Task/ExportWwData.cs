/*
yxm created at 2019/5/17 14:05:33
*/
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using System;
using System.Data.SQLite;

namespace DataSyncTool
{
	/// <summary>
	/// 导出外网数据
	/// </summary>
	class ExportWwData : Task
	{
		class Tables
		{
			public readonly DLXX_DK dk;
			public readonly DJ_CBJYQ_DJB djDjb;
			public readonly DJ_CBJYQ_CBF djCbf;
			public readonly DJ_CBJYQ_CBF_JTCY djCbfJtcy;
			public readonly DJ_CBJYQ_CBHT djCbht;
			public readonly DJ_CBJYQ_DKSYT djDksyt;
			public readonly DJ_CBJYQ_DKXX djDkxx;
			public readonly DJ_CBJYQ_LZDK djLzdk;
			public readonly DJ_CBJYQ_LZHT djLzht;
			public readonly DJ_CBJYQ_QSLYZL djQslyzl;
			public readonly DJ_CBJYQ_QZ djQz;
			public readonly DJ_CBJYQ_QZBF djQzbf;
			public readonly DJ_CBJYQ_QZHF djQzhf;
			public readonly DJ_CBJYQ_QZ_DYJL djQzdyjl;
			public readonly DJ_CBJYQ_WTFK djWtfk;
			public readonly DJ_CBJYQ_YDJB djYdjb;
			public readonly QSSJ_CBDKXX qssjDkxx;
			public readonly QSSJ_CBF qssjCbf;
			public readonly QSSJ_CBF_BGJL qssjCbfBgjl;
			public readonly QSSJ_CBF_JTCY qssjCbfJtcy;
			public readonly QSSJ_CBHT qssjCbht;
			public readonly QSSJ_CBJYQZ qssjQz;
			public readonly QSSJ_CBJYQZDJB qssjDjb;
			public readonly QSSJ_CBJYQZ_QZBF qssjQzbf;
			public readonly QSSJ_CBJYQZ_QZHF qssjQzhf;
			public readonly QSSJ_CBJYQZ_QZZX qssjQzzx;
			public readonly QSSJ_LZHT qssjLzht;
			public readonly QSSJ_FBF qssjFbf;
			public Tables(ISqlFunction sqlFunc)
			{
				dk = new DLXX_DK(sqlFunc);
				djDjb = new DJ_CBJYQ_DJB(sqlFunc);
				djCbf = new DJ_CBJYQ_CBF(sqlFunc);
				djCbfJtcy = new DJ_CBJYQ_CBF_JTCY(sqlFunc);
				djCbht = new DJ_CBJYQ_CBHT(sqlFunc);
				djDksyt = new DJ_CBJYQ_DKSYT(sqlFunc);
				djDkxx = new DJ_CBJYQ_DKXX(sqlFunc);
				djLzdk = new DJ_CBJYQ_LZDK(sqlFunc);
				djLzht = new DJ_CBJYQ_LZHT(sqlFunc);
				djQslyzl = new DJ_CBJYQ_QSLYZL(sqlFunc);
				djQz = new DJ_CBJYQ_QZ(sqlFunc);
				djQzbf = new DJ_CBJYQ_QZBF(sqlFunc);
				djQzhf = new DJ_CBJYQ_QZHF(sqlFunc);
				djQzdyjl = new DJ_CBJYQ_QZ_DYJL(sqlFunc);
				djWtfk = new DJ_CBJYQ_WTFK(sqlFunc);
				djYdjb = new DJ_CBJYQ_YDJB(sqlFunc);
				qssjDkxx = new QSSJ_CBDKXX(sqlFunc);
				qssjCbf = new QSSJ_CBF(sqlFunc);
				qssjCbfBgjl = new QSSJ_CBF_BGJL(sqlFunc);
				qssjCbfJtcy = new QSSJ_CBF_JTCY(sqlFunc);
				qssjCbht = new QSSJ_CBHT(sqlFunc);
				qssjQz = new QSSJ_CBJYQZ(sqlFunc);
				qssjDjb = new QSSJ_CBJYQZDJB(sqlFunc);
				qssjQzbf = new QSSJ_CBJYQZ_QZBF(sqlFunc);
				qssjQzhf = new QSSJ_CBJYQZ_QZHF(sqlFunc);
				qssjQzzx = new QSSJ_CBJYQZ_QZZX(sqlFunc);
				qssjFbf = new QSSJ_FBF(sqlFunc);
				qssjLzht = new QSSJ_LZHT(sqlFunc);
			}
		}
		public ExportWwData()
		{
			Name = "导出外网数据包";
			Description = "导出外网已登记的脱密数据";
			PropertyPage = new ExportNwDataPanel("外网交换包(*.wdb)|*.wdb");
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) => ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = PropertyPage as ExportNwDataPanel;
			var db = prm.db;
			var fileName = prm.SaveFileName;
			//SQLiteTransaction trans = null;

			#region 创建索引TRI_QSSJ_FBF
			if (!db.IsTriggerExist("QSSJ_FBF", "TRI_QSSJ_FBF"))
			{
				db.ExecuteNonQuery(@"CREATE TRIGGER TRI_QSSJ_FBF
ON [dbo].[QSSJ_FBF]
AFTER UPDATE 
AS
if update(FBFMC) or update(FBFFZRXM) or update(FZRZJLX) or update(FZRZJHM) or update(LXDH) or update(FBFDZ) or update(YZBM) or update(YBM) or update(SZDY) or update(FBFDCY) or update(FBFDCRQ) or update(FBFDCJS) or update(SHR) or update(SHRQ) or update(SHYJ) or update(BZ)
BEGIN
	SET NOCOUNT ON;
    UPDATE dbo.[QSSJ_FBF]
    SET  ZHXGSJ=SYSDATETIME()
    WHERE ID IN (SELECT DISTINCT ID FROM inserted)
END");
			}
			#endregion


			int n = 0, cnt = 0;
			double oldProgress = 0;
			using (var tgtDb = CreateSqliteDBUtil.CreateWwSyncDB(fileName))
			{
				try
				{
					var tmp = new Tables(tgtDb.SqlFunc);
					var whZhxgsj = $"convert(date,ZHXGSJ)>=convert(date,'{prm.LastRiqi}')";
					var whDjb = $"QSZT<>{(int)EQszt.Lins} and {whZhxgsj}";
					var whDjbIDIn = $"DJBID in (select distinct ID from DJ_CBJYQ_DJB where {whDjb})";
					tmp.djDjb.QueryDatas(db, cancel, whDjb);
					tmp.dk.QueryDatas(db, cancel, $"ZT={(int)EDKZT.Youxiao} and {whZhxgsj}");
					tmp.djCbf.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djCbfJtcy.QueryDatas(db, cancel, $"CBFBM in (select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.djCbht.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djDksyt.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djDkxx.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djLzdk.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djLzht.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djQslyzl.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djQz.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djQzbf.QueryDatas(db, cancel, $"QZID in(select distinct ID from DJ_CBJYQ_QZ where {whDjbIDIn})");
					tmp.djQzhf.QueryDatas(db, cancel, $"QZID in(select distinct ID from DJ_CBJYQ_QZ where {whDjbIDIn})");
					tmp.djQzdyjl.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djWtfk.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djYdjb.QueryDatas(db, cancel, whDjbIDIn);
					tmp.qssjDkxx.QueryDatas(db, cancel, $"DKBM in(select distinct DKBM from DJ_CBJYQ_DKXX where {whDjbIDIn})");
					tmp.qssjCbf.QueryDatas(db, cancel, $"CBFBM in(select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.qssjCbfBgjl.QueryDatas(db, cancel, $"CBFBM in(select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.qssjCbfJtcy.QueryDatas(db, cancel, $"CBFBM in(select distinct CBFBM from DJ_CBJYQ_CBF where {whDjbIDIn})");
					tmp.qssjCbht.QueryDatas(db, cancel, $"CBHTBM in(select distinct CBHTBM from DJ_CBJYQ_CBHT where {whDjbIDIn})");
					tmp.qssjQz.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjDjb.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjQzbf.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjQzbf.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjQzzx.QueryDatas(db, cancel, $"CBJYQZBM in(select distinct CBJYQZBM from DJ_CBJYQ_DJB where {whDjb})");
					tmp.qssjLzht.QueryDatas(db, cancel, $"CBHTBM in(select distinct CBHTBM from DJ_CBJYQ_LZHT where {whDjbIDIn})");
					tmp.qssjFbf.QueryDatas(db, cancel, whZhxgsj);
					cnt = tmp.dk.RowCount + tmp.djDjb.RowCount + tmp.djCbf.RowCount + tmp.djCbfJtcy.RowCount + tmp.djCbht.RowCount
						+ tmp.djDksyt.RowCount + tmp.djDkxx.RowCount + tmp.djLzdk.RowCount + tmp.djLzht.RowCount
						+ tmp.djQslyzl.RowCount + tmp.djQz.RowCount + tmp.djQzbf.RowCount + tmp.djQzhf.RowCount
						+ tmp.djQzdyjl.RowCount + tmp.djWtfk.RowCount + tmp.djYdjb.RowCount + tmp.qssjDkxx.RowCount
						+ tmp.qssjCbf.RowCount + tmp.qssjCbfBgjl.RowCount + tmp.qssjCbfJtcy.RowCount + tmp.qssjCbht.RowCount
						+ tmp.qssjQz.RowCount + tmp.qssjDjb.RowCount + tmp.qssjQzbf.RowCount + tmp.qssjQzhf.RowCount
						+ tmp.qssjQzzx.RowCount + tmp.qssjLzht.RowCount + tmp.qssjFbf.RowCount;
					if (cnt == 0)
					{
						ReportProgress(100);
					}
					else
					{
						tgtDb.BeginTransaction();
						ExportUtil.DoExport(tgtDb, tmp.dk, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djDjb, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djCbf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djCbfJtcy, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djCbht, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djDksyt, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djDkxx, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djLzdk, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djLzht, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djQslyzl, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djQz, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djQzbf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djQzhf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djQzdyjl, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djWtfk, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.djYdjb, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjDkxx, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjCbf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjCbfBgjl, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjCbfJtcy, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjCbht, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjQz, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjDjb, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjQzbf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjQzhf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjQzzx, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjFbf, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						ExportUtil.DoExport(tgtDb, tmp.qssjLzht, () => ProgressUtil.ReportProgress(base.ReportProgress, cnt, ++n, ref oldProgress));
						tgtDb.Commit();
					}
					ReportInfomation($"共导出数据{cnt}条");
				}
				catch (Exception ex)
				{
					UIHelper.ShowExceptionMessage(ex);
					tgtDb.Rollback();
				}
			}

		}
	}

	class DJ_CBJYQ_DJB : InsertBase
	{
		public DJ_CBJYQ_DJB(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_DJB",
			"ID,YWH,QLLX,DJLX,DJXL,DJYY,CBJYQZBM,FBFBM,CBFBM,CBFMC,CBFS,CBQX,CBQXQ,CBQXZ,CBJYQZLSH,YCBJYQZBH,DKSYT,DBR,DJSJ,QSZT,DYZT,YYZT,SFYZX,QXDM,SZDY,ZHXGSJ,FJ")
		{
		}
	}
	class DJ_CBJYQ_CBF : InsertBase
	{
		public DJ_CBJYQ_CBF(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_CBF",
			"ID,DJBID,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR")
		{
		}
	}
	class DJ_CBJYQ_CBF_JTCY : InsertBase
	{
		public DJ_CBJYQ_CBF_JTCY(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_CBF_JTCY",
			"ID,CBFBM,CBFID,CYXM,CYXB,CYZJLX,CYZJHM,CSRQ,YHZGX,CYBZ,SFGYR,CYBZSM")
		{
		}
	}
	class DJ_CBJYQ_CBHT : InsertBase
	{
		public DJ_CBJYQ_CBHT(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_CBHT",
			"ID,DJBID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM")
		{
		}
	}
	class DJ_CBJYQ_DKSYT : InsertBase
	{
		public DJ_CBJYQ_DKSYT(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_DKSYT",
			"ID,DJBID,SXH,FJ")
		{
		}
	}
	class DJ_CBJYQ_DKXX : InsertBase
	{
		public DJ_CBJYQ_DKXX(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_DKXX",
			"ID,DKID,DJBID,DKBM,DKMC,FBFBM,CBFBM,CBJYQQDFS,HTMJ,CBHTBM,LZHTBM,CBJYQZBM,YHTMJ,HTMJM,YHTMJM,SFQQQG,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,ZJRXM,DYZT,YYZT,LZZT,BZ")
		{
		}
	}
	class DJ_CBJYQ_LZDK : InsertBase
	{
		public DJ_CBJYQ_LZDK(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_LZDK",
			"ID,DJBID,YDJBID,DJBBM,HTID,HTBM,DKID,DKBM,DKMJ,DKMJM")
		{
		}
	}
	class DJ_CBJYQ_LZHT : InsertBase
	{
		public DJ_CBJYQ_LZHT(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_LZHT",
			"ID,DJBID,YDJBID,CBHTBM,LZHTBM,CBFBM,SRFBM,LZFS,LZQX,LZQXKSRQ,LZQXJSRQ,LZMJ,LZMJM,LZDKS,LZQTDYT,LZHTDYT,LZJGSM,HTQDRQ,SZDY,CJYH,CJSJ,ZHXGYH,ZHXGSJ")
		{
		}
	}
	class DJ_CBJYQ_QSLYZL : InsertBase
	{
		public DJ_CBJYQ_QSLYZL(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QSLYZL",
			"ID,FJID,DJBID,CBJYQZBM,ZLFJBH,ZLFJMC,ZLFJRQ,FJ")
		{
		}
	}
	class DJ_CBJYQ_QZ : InsertBase
	{
		public DJ_CBJYQ_QZ(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZ",
			"ID,DJBID,CBJYQZBM,ZSBS,SQSJC,BZNF,FZJGSZDMC,NDSXH,YZSXH,CBJYQZLSH,FZJG,FZRQ,DYCS,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM,SFYZX,ZXYY,ZXRQ,ZXSJ")
		{
		}
	}
	class DJ_CBJYQ_QZBF : InsertBase
	{
		public DJ_CBJYQ_QZBF(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZBF",
			"ID,QZID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM")
		{
		}
	}
	class DJ_CBJYQ_QZHF : InsertBase
	{
		public DJ_CBJYQ_QZHF(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZHF",
			"ID,QZID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM")
		{
		}
	}
	class DJ_CBJYQ_QZ_DYJL : InsertBase
	{
		public DJ_CBJYQ_QZ_DYJL(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZ_DYJL",
			"ID,QZID,DJBID,CBJYQZBM,CBJYQZLSH,YZSXH,DYSJ,DYR")
		{
		}
	}
	class DJ_CBJYQ_WTFK : InsertBase
	{
		public DJ_CBJYQ_WTFK(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_WTFK",
			"ID,DJBID,CBJYQZBM,CBFMC,FKRXM,FKRDH,SFYWT,WTLX,FKNR,FBFBM,SZDY,CJSJ,ZHXGSJ")
		{
		}
	}
	class DJ_CBJYQ_YDJB : InsertBase
	{
		public DJ_CBJYQ_YDJB(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_YDJB",
			"ID,DJBID,YDJBID,SLSQID,DJLX,DJXL,BGLX,BGCS")
		{
		}
	}

	class QSSJ_CBDKXX : InsertBase
	{
		public QSSJ_CBDKXX(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBDKXX",
			"ID,DKBM,FBFBM,CBFBM,CBJYQQDFS,CBHTBM,LZHTBM,CBJYQZBM,HTMJ,YHTMJ,HTMJM,YHTMJM,SFQQQG")
		{
		}
	}
	class QSSJ_CBF : InsertBase
	{
		public QSSJ_CBF(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBF",
			"ID,FBFBM,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR,ZT,DJZT,CJSJ,ZHXGSJ")
		{
		}
	}
	class QSSJ_CBF_BGJL : InsertBase
	{
		public QSSJ_CBF_BGJL(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBF_BGJL",
			"ID,CBFID,CBFBM,YCBFID,YCBFBM,BGFS")
		{
		}
	}
	class QSSJ_CBF_JTCY : InsertBase
	{
		public QSSJ_CBF_JTCY(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBF_JTCY",
			"ID,CBFBM,CYXM,CYXB,CYZJLX,CYZJHM,CSRQ,YHZGX,CYBZ,SFGYR,CYBZSM")
		{
		}
	}
	class QSSJ_CBHT : InsertBase
	{
		public QSSJ_CBHT(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBHT",
			"ID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM,ZT,DJZT,CJSJ,ZHXGSJ")
		{
		}
	}
	class QSSJ_CBJYQZ : InsertBase
	{
		public QSSJ_CBJYQZ(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ",
			"ID,CBJYQZBM,FZJG,FZRQ,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM")
		{
		}
	}
	class QSSJ_CBJYQZDJB : InsertBase
	{
		public QSSJ_CBJYQZDJB(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZDJB",
			"ID,CBJYQZBM,FBFBM,CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,DKSYT,CBJYQZLSH,DJBFJ,YCBJYQZBH,DBR,DJSJ")
		{
		}
	}
	class QSSJ_CBJYQZ_QZBF : InsertBase
	{
		public QSSJ_CBJYQZ_QZBF(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ_QZBF",
			"ID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM")
		{
		}
	}
	class QSSJ_CBJYQZ_QZHF : InsertBase
	{
		public QSSJ_CBJYQZ_QZHF(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ_QZHF",
			"ID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM")
		{
		}
	}
	class QSSJ_CBJYQZ_QZZX : InsertBase
	{
		public QSSJ_CBJYQZ_QZZX(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ_QZZX",
			"ID,CBJYQZBM,ZXYY,ZXRQ")
		{
		}
	}

	class QSSJ_FBF : InsertBase
	{
		public QSSJ_FBF(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_FBF",
			"ID,FBFBM,FBFMC,FBFFZRXM,FZRZJLX,FZRZJHM,LXDH,FBFDZ,YZBM,YBM,SZDY,FBFDCY,FBFDCRQ,FBFDCJS,SHR,SHRQ,SHYJ,BZ,ZHXGSJ")
		{
		}
	}
	class QSSJ_LZHT : InsertBase
	{
		public QSSJ_LZHT(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_LZHT",
			"ID,CBHTBM,LZHTBM,CBFBM,SRFBM,LZFS,LZQX,LZQXKSRQ,LZQXJSRQ,LZMJ,LZMJM,LZDKS,LZQTDYT,LZHTDYT,LZJGSM,HTQDRQ")
		{
		}
	}
}
