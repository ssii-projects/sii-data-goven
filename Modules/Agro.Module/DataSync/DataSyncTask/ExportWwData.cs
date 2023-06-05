/*
yxm created at 2019/5/17 14:05:33
*/
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Model;
using System;

namespace Agro.Module.DataSync
{
	/// <summary>
	/// 导出外网数据
	/// </summary>
	public class ExportWwData : Task
	{
		class Tables
		{
			public readonly DlxxDk dk;
			public readonly DjCbjyqDjb djDjb;
			public readonly DjCbjyqCbf djCbf;
			public readonly DjCbjyqCbfJjcy djCbfJtcy;
			public readonly DjCbjyqCbht djCbht;
			public readonly DjCbjyqDksyt djDksyt;
			public readonly DjCbjyqDkxx djDkxx;
			public readonly DjCbjyqLzdk djLzdk;
			public readonly DjCbjyqLzht djLzht;
			public readonly DjCbjyqQslyzl djQslyzl;
			public readonly DjCbjyqQz djQz;
			public readonly DjCbjyqQzbf djQzbf;
			public readonly DjCbjyqQzhf djQzhf;
			public readonly DjCbjyqQzDyjl djQzdyjl;
			//public readonly DJ_CBJYQ_WTFK djWtfk;
			public readonly DjCbjyqYdjb djYdjb;
			public readonly DjYwSlsq djYwSlsj;
			public readonly QssjCbdkxx qssjDkxx;
			public readonly QssjCbf qssjCbf;
			public readonly QssjCbfBgjl qssjCbfBgjl;
			public readonly QssjCbfJtcy qssjCbfJtcy;
			public readonly QssjCbht qssjCbht;
			public readonly QssjCbjyqz qssjQz;
			public readonly QssjCbjyqzdjb qssjDjb;
			public readonly QssjCbjyqzQzbf qssjQzbf;
			public readonly QssjCbjyqzQzhf qssjQzhf;
			public readonly QssjCbjyqzQzzx qssjQzzx;
			public readonly QssjLzht qssjLzht;
			public readonly QssjFbf qssjFbf;

			public Tables(ISqlFunction sqlFunc)
			{
				dk = new DlxxDk(sqlFunc);
				djDjb = new DjCbjyqDjb(sqlFunc);
				djCbf = new DjCbjyqCbf(sqlFunc);
				djCbfJtcy = new DjCbjyqCbfJjcy(sqlFunc);
				djCbht = new DjCbjyqCbht(sqlFunc);
				djDksyt = new DjCbjyqDksyt(sqlFunc);
				djDkxx = new DjCbjyqDkxx(sqlFunc);
				djLzdk = new DjCbjyqLzdk(sqlFunc);
				djLzht = new DjCbjyqLzht(sqlFunc);
				djQslyzl = new DjCbjyqQslyzl(sqlFunc);
				djQz = new DjCbjyqQz(sqlFunc);
				djQzbf = new DjCbjyqQzbf(sqlFunc);
				djQzhf = new DjCbjyqQzhf(sqlFunc);
				djQzdyjl = new DjCbjyqQzDyjl(sqlFunc);
				//djWtfk = new DJ_CBJYQ_WTFK(sqlFunc);
				djYdjb = new DjCbjyqYdjb(sqlFunc);
				djYwSlsj = new DjYwSlsq(sqlFunc);
				qssjDkxx = new QssjCbdkxx(sqlFunc);
				qssjCbf = new QssjCbf(sqlFunc);
				qssjCbfBgjl = new QssjCbfBgjl(sqlFunc);
				qssjCbfJtcy = new QssjCbfJtcy(sqlFunc);
				qssjCbht = new QssjCbht(sqlFunc);
				qssjQz = new QssjCbjyqz(sqlFunc);
				qssjDjb = new QssjCbjyqzdjb(sqlFunc);
				qssjQzbf = new QssjCbjyqzQzbf(sqlFunc);
				qssjQzhf = new QssjCbjyqzQzhf(sqlFunc);
				qssjQzzx = new QssjCbjyqzQzzx(sqlFunc);
				qssjFbf = new QssjFbf(sqlFunc);
				qssjLzht = new QssjLzht(sqlFunc);
			}
		}
		public ExportWwData()
		{
			Name = "导出外网数据包";
			Description = "导出外网已登记的脱密数据";
			PropertyPage = new ExportWwDataPanel();// "外网交换包(*.wdb)|*.wdb", "DB07E317-E3B3-4D2A-8D44-584DC0B268FA");
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) => ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = PropertyPage as ExportWwDataPanel;
			var db = MyGlobal.Workspace;// prm.db;
			var fileName = prm.SaveFileName;
			//SQLiteTransaction trans = null;

			#region 创建索引TRI_QSSJ_FBF
			if (!db.IsTriggerExist("QSSJ_FBF", "TRI_QSSJ_FBF"))
			{
				db.ExecuteNonQuery(@"CREATE TRIGGER TRI_QSSJ_FBF
ON [QSSJ_FBF]
AFTER UPDATE 
AS
if update(FBFMC) or update(FBFFZRXM) or update(FZRZJLX) or update(FZRZJHM) or update(LXDH) or update(FBFDZ) or update(YZBM) or update(YBM) or update(SZDY) or update(FBFDCY) or update(FBFDCRQ) or update(FBFDCJS) or update(SHR) or update(SHRQ) or update(SHYJ) or update(BZ)
BEGIN
	SET NOCOUNT ON;
    UPDATE [QSSJ_FBF]
    SET  ZHXGSJ=SYSDATETIME()
    WHERE ID IN (SELECT DISTINCT ID FROM inserted)
END");
			}
			#endregion


			//int n = 0, cnt = 0;
			//double oldProgress = 0;
			using (var tgtDb = CreateSqliteDBUtil.CreateWwSyncDB(fileName))
			{
				try
				{
					var tmp = new Tables(tgtDb.SqlFunc);
					var whZhxgsj = $"convert(date,ZHXGSJ)>=convert(date,'{prm.LastRiqi}')";
					//var whZhxgsj = $"(convert(date,CJSJ)>=convert(date,'{prm.LastRiqi}') or convert(date,DJSJ)>=convert(date,'{prm.LastRiqi}') or convert(date,ZHXGSJ)>=convert(date,'{prm.LastRiqi}'))";

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
					//tmp.djWtfk.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djYdjb.QueryDatas(db, cancel, whDjbIDIn);
					tmp.djYwSlsj.QueryDatas(db, cancel, $"AJZT=1 and YWH  in(select distinct YWH from DJ_CBJYQ_DJB where {whDjb})");
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
					int cnt = tmp.dk.RowCount + tmp.djDjb.RowCount + tmp.djCbf.RowCount + tmp.djCbfJtcy.RowCount + tmp.djCbht.RowCount
						+ tmp.djDksyt.RowCount + tmp.djDkxx.RowCount + tmp.djLzdk.RowCount + tmp.djLzht.RowCount
						+ tmp.djQslyzl.RowCount + tmp.djQz.RowCount + tmp.djQzbf.RowCount + tmp.djQzhf.RowCount
						+ tmp.djQzdyjl.RowCount + /*tmp.djWtfk.RowCount +*/ tmp.djYdjb.RowCount +tmp.djYwSlsj.RowCount
						+ tmp.qssjDkxx.RowCount+ tmp.qssjCbf.RowCount + tmp.qssjCbfBgjl.RowCount + tmp.qssjCbfJtcy.RowCount 
						+ tmp.qssjCbht.RowCount+ tmp.qssjQz.RowCount + tmp.qssjDjb.RowCount + tmp.qssjQzbf.RowCount 
						+ tmp.qssjQzhf.RowCount+ tmp.qssjQzzx.RowCount + tmp.qssjLzht.RowCount + tmp.qssjFbf.RowCount;
					if (cnt == 0)
					{
						ReportProgress(100);
					}
					else
					{
						var progress = new ProgressReporter(ReportProgress, cnt);
						tgtDb.BeginTransaction();
						ExportUtil.DoExport(tgtDb, tmp.dk, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djDjb, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djCbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djCbfJtcy, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djCbht, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djDksyt, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djDkxx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djLzdk, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djLzht, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQslyzl, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQz, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQzbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQzhf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djQzdyjl, () => progress.Step());
						//ExportUtil.DoExport(tgtDb, tmp.djWtfk, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djYdjb, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.djYwSlsj, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjDkxx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbfBgjl, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbfJtcy, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjCbht, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQz, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjDjb, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQzbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQzhf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjQzzx, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjFbf, () => progress.Step());
						ExportUtil.DoExport(tgtDb, tmp.qssjLzht, () => progress.Step());
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

	class DjCbjyqDjb : InsertBase
	{
		public DjCbjyqDjb(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_DJB",
			"ID,YWH,QLLX,DJLX,DJXL,DJYY,CBJYQZBM,FBFBM,CBFBM,CBFMC,CBFS,CBQX,CBQXQ,CBQXZ,CBJYQZLSH,YCBJYQZBH,DKSYT,DBR,DJSJ,QSZT,DYZT,YYZT,SFYZX,QXDM,SZDY,ZHXGSJ,FJ")
		{
		}
	}
	class DjCbjyqCbf : InsertBase
	{
		public DjCbjyqCbf(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_CBF",
			"ID,DJBID,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR")
		{
		}
	}
	class DjCbjyqCbfJjcy : InsertBase
	{
		public DjCbjyqCbfJjcy(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_CBF_JTCY",
			"ID,CBFBM,CBFID,CYXM,CYXB,CYZJLX,CYZJHM,CSRQ,YHZGX,CYBZ,SFGYR,CYBZSM")
		{
		}
	}
	class DjCbjyqCbht : InsertBase
	{
		public DjCbjyqCbht(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_CBHT",
			"ID,DJBID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM")
		{
		}
	}
	class DjCbjyqDksyt : InsertBase
	{
		public DjCbjyqDksyt(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_DKSYT",
			"ID,DJBID,SXH,FJ")
		{
		}
	}
	class DjCbjyqDkxx : InsertBase
	{
		public DjCbjyqDkxx(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_DKXX",
			"ID,DKID,DJBID,DKBM,DKMC,FBFBM,CBFBM,CBJYQQDFS,HTMJ,CBHTBM,LZHTBM,CBJYQZBM,YHTMJ,HTMJM,YHTMJM,SFQQQG,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,ZJRXM,DYZT,YYZT,LZZT,BZ")
		{
		}
	}
	class DjCbjyqLzdk : InsertBase
	{
		public DjCbjyqLzdk(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_LZDK",
			"ID,DJBID,YDJBID,DJBBM,HTID,HTBM,DKID,DKBM,DKMJ,DKMJM")
		{
		}
	}
	class DjCbjyqLzht : InsertBase
	{
		public DjCbjyqLzht(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_LZHT",
			"ID,DJBID,YDJBID,CBHTBM,LZHTBM,CBFBM,SRFBM,LZFS,LZQX,LZQXKSRQ,LZQXJSRQ,LZMJ,LZMJM,LZDKS,LZQTDYT,LZHTDYT,LZJGSM,HTQDRQ,SZDY,CJYH,CJSJ,ZHXGYH,ZHXGSJ")
		{
		}
	}
	class DjCbjyqQslyzl : InsertBase
	{
		public DjCbjyqQslyzl(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QSLYZL",
			"ID,FJID,DJBID,CBJYQZBM,ZLFJBH,ZLFJMC,ZLFJRQ,FJ")
		{
		}
	}
	class DjCbjyqQz : InsertBase
	{
		public DjCbjyqQz(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZ",
			"ID,DJBID,CBJYQZBM,ZSBS,SQSJC,BZNF,FZJGSZDMC,NDSXH,YZSXH,CBJYQZLSH,FZJG,FZRQ,DYCS,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM,SFYZX,ZXYY,ZXRQ,ZXSJ")
		{
		}
	}
	class DjCbjyqQzbf : InsertBase
	{
		public DjCbjyqQzbf(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZBF",
			"ID,QZID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM")
		{
		}
	}
	class DjCbjyqQzhf : InsertBase
	{
		public DjCbjyqQzhf(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZHF",
			"ID,QZID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM")
		{
		}
	}
	class DjCbjyqQzDyjl : InsertBase
	{
		public DjCbjyqQzDyjl(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_QZ_DYJL",
			"ID,QZID,DJBID,CBJYQZBM,CBJYQZLSH,YZSXH,DYSJ,DYR")
		{
		}
	}

	class DjCbjyqYdjb : InsertBase
	{
		public DjCbjyqYdjb(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_CBJYQ_YDJB",
			"ID,DJBID,YDJBID,SLSQID,DJLX,DJXL,BGLX,BGCS")
		{
		}
	}

	class QssjCbdkxx : InsertBase
	{
		public QssjCbdkxx(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBDKXX",
			"ID,DKBM,FBFBM,CBFBM,CBJYQQDFS,CBHTBM,LZHTBM,CBJYQZBM,HTMJ,YHTMJ,HTMJM,YHTMJM,SFQQQG")
		{
		}
	}
	class QssjCbf : InsertBase
	{
		public QssjCbf(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBF",
			"ID,FBFBM,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR,ZT,DJZT,CJSJ,ZHXGSJ"
			,"CBFBM")
		{
		}
	}
	class QssjCbfBgjl : InsertBase
	{
		public QssjCbfBgjl(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBF_BGJL",
			"ID,CBFID,CBFBM,YCBFID,YCBFBM,BGFS")
		{
		}
	}
	class QssjCbfJtcy : InsertBase
	{
		public QssjCbfJtcy(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBF_JTCY",
			"ID,CBFBM,CYXM,CYXB,CYZJLX,CYZJHM,CSRQ,YHZGX,CYBZ,SFGYR,CYBZSM")
		{
		}
	}
	class QssjCbht : InsertBase
	{
		public QssjCbht(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBHT",
			"ID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM,ZT,DJZT,CJSJ,ZHXGSJ"
			,"CBHTBM")
		{
		}
	}
	class QssjCbjyqz : InsertBase
	{
		public QssjCbjyqz(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ",
			"ID,CBJYQZBM,FZJG,FZRQ,QZSFLQ,QZLQRQ,QZLQRXM,QZLQRZJLX,QZLQRZJHM")
		{
		}
	}
	class QssjCbjyqzdjb : InsertBase
	{
		public QssjCbjyqzdjb(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZDJB",
			"ID,CBJYQZBM,FBFBM,CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,DKSYT,CBJYQZLSH,DJBFJ,YCBJYQZBH,DBR,DJSJ"
			, "CBJYQZBM")
		{
		}
	}
	class QssjCbjyqzQzbf : InsertBase
	{
		public QssjCbjyqzQzbf(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ_QZBF",
			"ID,CBJYQZBM,QZBFYY,BFRQ,QZBFLQRQ,QZBFLQRXM,BFLQRZJLX,BFLQRZJHM")
		{
		}
	}
	class QssjCbjyqzQzhf : InsertBase
	{
		public QssjCbjyqzQzhf(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ_QZHF",
			"ID,CBJYQZBM,QZHFYY,HFRQ,QZHFLQRQ,QZHFLQRXM,HFLQRZJLX,HFLQRZJHM")
		{
		}
	}
	class QssjCbjyqzQzzx : InsertBase
	{
		public QssjCbjyqzQzzx(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_CBJYQZ_QZZX",
			"ID,CBJYQZBM,ZXYY,ZXRQ")
		{
		}
	}

	class QssjFbf : InsertBase
	{
		public QssjFbf(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_FBF",
			"ID,FBFBM,FBFMC,FBFFZRXM,FZRZJLX,FZRZJHM,LXDH,FBFDZ,YZBM,YBM,SZDY,FBFDCY,FBFDCRQ,FBFDCJS,SHR,SHRQ,SHYJ,BZ,ZHXGSJ"
			, "FBFBM")
		{
		}
	}
	class QssjLzht : InsertBase
	{
		public QssjLzht(ISqlFunction sqlFunc) : base(sqlFunc, "QSSJ_LZHT",
			"ID,CBHTBM,LZHTBM,CBFBM,SRFBM,LZFS,LZQX,LZQXKSRQ,LZQXJSRQ,LZMJ,LZMJM,LZDKS,LZQTDYT,LZHTDYT,LZJGSM,HTQDRQ")
		{
		}
	}

	class DjYwSlsq : InsertBase
	{
		public DjYwSlsq(ISqlFunction sqlFunc) : base(sqlFunc, "DJ_YW_SLSQ",
			"ID,YWH,QLLX,TXQLLX,DJLX,DJXL,DJSX,YWMC,SLRY,SLSJ,ZL,TZRXM,TZFS,TZRDH,TZRYDDH,TZRDZYJ,SFWTAJ,JSSJ,DJYY,AJZT,LCJDSL,DQJDXH,YWZT,SZDY,CJSJ,CJYH,ZHXGSJ,ZHXGYH,BZ")
		{
		}
	}
}
