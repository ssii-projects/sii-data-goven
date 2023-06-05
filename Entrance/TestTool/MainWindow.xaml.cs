using Agro.GIS;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace TestTool
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void btnRun_Click(object sender, RoutedEventArgs e)
		{
			IFeatureWorkspace db;
			try
			{
				var dw = Stopwatch.StartNew();
				Console.WriteLine(DateTime.Now + " 开始修复...");
				var or=DataSourceConfig.Instance.OpenDefault();
				db = or.Workspace;
				if (db != null)
				{
					using (db)
					{
						try
						{
							db.BeginTransaction();
							var p = new Processor();
							p.Process(db);
							db.Commit();
						}
						catch (Exception e1)
						{
							db.Rollback();
							throw e1;
						}
					}
				}
				dw.Stop();
				Console.WriteLine("结束修复 耗时：" + dw.Elapsed);
				MessageBox.Show("结束修复 耗时：" + dw.Elapsed);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

	}
	public class Processor
	{
		private readonly Dictionary<string, DYCode> _dicCodes = new Dictionary<string, DYCode>();
		public void Process(IFeatureWorkspace db)
		{
			var lstCodes = new List<DYCode>();
			var sql = "select ID,BM,MC,JB,SJID,BSM from DLXX_XZDY order by BM";
			db.QueryCallback(sql, r =>
			{
				var c = new DYCode(r.GetInt32(3))
				{
					ID = r.GetString(0),
					code = r.GetString(1),
					mc = r.GetString(2),
					sjid = r.IsDBNull(4) ? null : r.GetString(4),
					BSM=r.GetInt32(5),
				};
				_dicCodes[c.ID] = c;
				lstCodes.Add(c);
				return true;
			});

			foreach (var c in lstCodes)
			{
				var sjCode = GetSjCode(c);
				if (sjCode == null)
				{
					c.Yxmc = c.mc;
				}
				else
				{
					int n = c.mc.IndexOf(sjCode.mc);
					if (n >= 0)
					{
						c.Yxmc = c.mc.Substring(n + sjCode.mc.Length);
					}
					else
					{
						c.Yxmc = c.mc;
					}
				}
			}

			string xmc = null;
			foreach (var c in lstCodes)
			{
				if (c.jb == 4)
				{
					xmc = c.Yxmc;
				}
				var kzmc = GetKzmc(c);
				//Console.WriteLine(c +"  "+GetKzmc(c));
				sql = "update DLXX_XZDY set MC='" + c.Yxmc + "',KZMC='" + kzmc + "' where BSM=" + c.BSM;
				db.ExecuteNonQuery(sql);
			}
			Console.WriteLine("xmc=" + xmc);
			RunSQLScript(db, xmc);
		}
		private DYCode GetSjCode(DYCode c)
		{

			if (c.sjid != null && _dicCodes.TryGetValue(c.sjid, out DYCode c1))
			{
				return c1;
			}
			return null;
		}

		private string GetKzmc(DYCode c)
		{
			var lst = new List<string>();// { c.Yxmc };
			var sj = GetSjCode(c);
			while (sj != null)
			{
				lst.Add(sj.Yxmc);
				sj = GetSjCode(sj);
			}
			string s = null;
			for (int i = lst.Count - 1; i >= 0; --i)
			{
				if (s == null)
				{
					s = lst[i];
				}
				else
				{
					s += lst[i];
				}
			}
			return s;
		}


		private static void ClearTableData(IFeatureWorkspace db, string tableName)
		{
			db.ExecuteNonQuery("truncate table " + tableName);
			//db.ExecuteNonQuery("delete from " + tableName);
		}

		private void ExecSQL(IFeatureWorkspace db, string sql)
		{
			Console.WriteLine(sql);
			db.ExecuteNonQuery(sql);
		}
		/// <summary>
		/// 执行SQLServer 数据库脚本
		/// </summary>
		/// <param name="db"></param>
		/// <param name="xianMC"></param>
		private void RunSQLScript(IFeatureWorkspace db, string xianMC)
		{
			#region 初始化 DJ_CBJYQ_DJB（登记簿）数据
			ClearTableData(db, "DJ_CBJYQ_DJB");
			var sql = @"INSERT INTO DJ_CBJYQ_DJB(ID,QLLX,DJLX,DJYY,CBJYQZBM,FBFBM,CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,DKSYT,CBJYQZLSH,FJ,YCBJYQZBH,DBR
, DJSJ, QSZT, DYZT, YYZT, SFYZX, QXDM, SZDY)
SELECT R.ID,0 QLLX,0 DJLX,'初始化总登记入库' DJYY,CBJYQZBM,F.FBFBM,CBFBM,CBFS,CBQX,CBQXQ,CBQXZ,DKSYT,ISNULL(CBJYQZLSH, '') CBJYQZLSH,R.DJBFJ
	,YCBJYQZBH,R.DBR,R.DJSJ,1 QSZT,0 DYZT,0 YYZT,0 SFYZX,SUBSTRING(F.FBFBM, 1, 6) QXDM,Z.ID SZDY
FROM QSSJ_CBJYQZDJB R
JOIN QSSJ_FBF F ON F.FBFBM = R.FBFBM
JOIN DLXX_XZDY Z ON Z.ID = F.SZDY";
			ExecSQL(db,sql);
			#endregion

			#region 初始化 DJ_CBJYQ_CBHT（承包合同） 数据
			ClearTableData(db, "DJ_CBJYQ_CBHT");
			sql = @"INSERT INTO DJ_CBJYQ_CBHT(ID,DJBID,CBHTBM,YCBHTBM,FBFBM,CBFBM,CBFS,CBQXQ,CBQXZ,HTZMJ,CBDKZS,QDSJ,HTZMJM,YHTZMJ,YHTZMJM)
SELECT C.ID,R.ID DJBID,C.CBHTBM,C.YCBHTBM,C.FBFBM,C.CBFBM,C.CBFS,C.CBQXQ,C.CBQXZ,C.HTZMJ,C.CBDKZS,C.QDSJ,C.HTZMJM,C.YHTZMJ,C.YHTZMJM
FROM QSSJ_CBHT C
JOIN QSSJ_CBJYQZDJB R ON C.CBHTBM=R.CBJYQZBM";
			ExecSQL(db,sql);
			#endregion

			#region 更新数据登记状态
			sql = @"UPDATE QSSJ_CBHT SET DJZT=2
FROM QSSJ_CBHT C 
WHERE EXISTS (
  SELECT ID
  FROM DJ_CBJYQ_CBHT C1
  WHERE C1.CBHTBM=C.CBHTBM)";
			ExecSQL(db,sql);
			#endregion

			//ok

			#region 初始化 DJ_CBJYQ_DKXX（承包地块） 数据
			ClearTableData(db, "DJ_CBJYQ_DKXX");
			sql = @"INSERT INTO DJ_CBJYQ_DKXX(ID,DKID,DJBID,DKBM,FBFBM,CBFBM,CBJYQQDFS,HTMJ,CBHTBM,LZHTBM,CBJYQZBM,YHTMJ,HTMJM,YHTMJM,SFQQQG,
  DKMC,YDKBM,SYQXZ,DKLB,TDLYLX,DLDJ,TDYT,SFJBNT,SCMJ,SCMJM,ELHTMJ,QQMJ,JDDMJ,DKDZ,DKNZ,DKXZ,DKBZ,DKBZXX,ZJRXM,BZ)
SELECT RL.ID,l.ID DKID,R.ID DJBID,RL.DKBM,RL.FBFBM,RL.CBFBM,RL.CBJYQQDFS,RL.HTMJ,RL.CBHTBM,RL.LZHTBM,RL.CBJYQZBM,RL.YHTMJ,RL.HTMJM,RL.YHTMJM,RL.SFQQQG,
	L.DKMC,L.YDKBM,L.SYQXZ,L.DKLB,L.TDLYLX,L.DLDJ,L.TDYT,L.SFJBNT,L.SCMJ,L.SCMJM,L.ELHTMJ,L.QQMJ,L.JDDMJ,L.DKDZ,L.DKNZ,L.DKXZ,L.DKBZ,
	L.DKBZXX,L.ZJRXM,L.DKBZXX
FROM QSSJ_CBDKXX RL
JOIN DLXX_DK L ON RL.DKBM=L.DKBM
JOIN QSSJ_CBJYQZDJB R ON RL.CBJYQZBM=R.CBJYQZBM";
			ExecSQL(db,sql);
			#endregion

			#region 更新数据登记状态
			sql = @"UPDATE DLXX_DK SET DJZT=2
FROM DLXX_DK C 
WHERE EXISTS (
  SELECT ID
  FROM DJ_CBJYQ_DKXX C1
  WHERE C1.DKBM=C.DKBM
)";
			ExecSQL(db,sql);
			#endregion

			#region 初始化 DJ_CBJYQ_CBF（登记承包方） 数据
			ClearTableData(db, "DJ_CBJYQ_CBF");
			sql = @"INSERT INTO DJ_CBJYQ_CBF(ID,DJBID,CBFBM,CBFLX,CBFMC,CBFZJLX,CBFZJHM,CBFDZ,YZBM,LXDH,CBFCYSL,CBFDCRQ,CBFDCY,CBFDCJS,GSJS,GSJSR,GSSHRQ,GSSHR)
SELECT F.ID,R.ID DJBID,F.CBFBM,F.CBFLX,F.CBFMC,F.CBFZJLX,F.CBFZJHM,F.CBFDZ,F.YZBM,F.LXDH,F.CBFCYSL,F.CBFDCRQ,F.CBFDCY,F.CBFDCJS,F.GSJS,F.GSJSR,F.GSSHRQ,F.GSSHR
FROM QSSJ_CBF F
JOIN QSSJ_CBHT C ON C.CBFBM=F.CBFBM
JOIN QSSJ_CBJYQZDJB R ON R.CBJYQZBM=C.CBHTBM";
			ExecSQL(db,sql);

			//-- 更新数据登记状态
			sql = @"UPDATE QSSJ_CBF SET DJZT=2
FROM QSSJ_CBF C
WHERE EXISTS (
  SELECT ID
  FROM DJ_CBJYQ_CBF C1
  WHERE C1.CBFBM=C.CBFBM
)";
			ExecSQL(db,sql);
			#endregion

			#region 初始化 DJ_CBJYQ_CBF_JTCY（登记承包方家庭成员） 数据
			ClearTableData(db, "DJ_CBJYQ_CBF_JTCY");
			sql = @"INSERT INTO DJ_CBJYQ_CBF_JTCY(ID,CBFBM,CBFID,CYXM,CYXB,CYZJLX,CYZJHM,YHZGX,CYBZ,SFGYR,CYBZSM,CSRQ)
SELECT P.ID,F.CBFBM,F.ID CBFID,P.CYXM,P.CYXB,P.CYZJLX,P.CYZJHM,P.YHZGX,P.CYBZ,P.SFGYR,P.CYBZSM,P.CSRQ
FROM QSSJ_CBF_JTCY P
JOIN QSSJ_CBF F ON F.CBFBM=P.CBFBM
JOIN QSSJ_CBHT C ON C.CBFBM=F.CBFBM
JOIN QSSJ_CBJYQZDJB R ON R.CBJYQZBM=C.CBHTBM";
			ExecSQL(db,sql);
			#endregion

			//ok
			#region  初始化 DJ_CBJYQ_QZ（权证） 数据
			ClearTableData(db, "DJ_CBJYQ_QZ");
			var sqlFmt = @"INSERT INTO DJ_CBJYQ_QZ
SELECT W.ID,R.ID DJBID,W.CBJYQZBM,0 ZSBS,'{0}' SQSJC, YEAR(W.FZRQ),W.FZJG FZJGSZDMC,NULL NDSXH,
	NULL YZSXH,NULL CBJYQZLSH,W.FZJG,W.FZRQ,0 DYCS,W.QZSFLQ,W.QZLQRQ,W.QZLQRXM,W.QZLQRZJLX,W.QZLQRZJHM,
	0 SFYZX,NULL ZXYY,NULL ZXRQ,NULL ZXSJ
FROM QSSJ_CBJYQZ W
JOIN QSSJ_CBJYQZDJB R ON R.CBJYQZBM=W.CBJYQZBM";
			sql = string.Format(sqlFmt, xianMC);
			ExecSQL(db,sql);
			#endregion
			//not ok

			#region  初始化 XTPZ_LSH（流水号） 数据
			ClearTableData(db, "XTPZ_LSH");
			sql = @"INSERT INTO XTPZ_LSH
SELECT NEWID() ID, 'CONTRACTOR_FAMILY' LX, F.FBFBM FZM,C.XH BH FROM QSSJ_FBF F
JOIN (
  SELECT FBFBM,MAX(CAST(RIGHT(CBFBM,3) AS INT))+1 XH FROM QSSJ_CBF
  WHERE CBFLX='1'
  GROUP BY FBFBM
) C ON F.FBFBM=C.FBFBM";
			ExecSQL(db,sql);
			sql = @"INSERT INTO XTPZ_LSH
SELECT NEWID() ID, 'CONTRACTOR_PERSON' LX, F.FBFBM FZM,8000+C.XH BH FROM QSSJ_FBF F
JOIN (
  SELECT FBFBM,MAX(CAST(RIGHT(CBFBM,3) AS INT))+1 XH FROM QSSJ_CBF
  WHERE CBFLX='2'
  GROUP BY FBFBM
) C ON F.FBFBM=C.FBFBM";
			ExecSQL(db,sql);
			sql = @"INSERT INTO XTPZ_LSH
SELECT NEWID() ID, 'CONTRACTOR_ORG' LX, F.FBFBM FZM,9000+C.XH BH FROM QSSJ_FBF F
JOIN (
  SELECT FBFBM,MAX(CAST(RIGHT(CBFBM,3) AS INT))+1 XH FROM QSSJ_CBF
  WHERE CBFLX='3'
  GROUP BY FBFBM
) C ON F.FBFBM=C.FBFBM";
			ExecSQL(db,sql);
			sql = @"INSERT INTO XTPZ_LSH
SELECT NEWID() ID, 'CONTRACTLAND' LX, F.FBFBM FZM,L.XH BH FROM QSSJ_FBF F
JOIN (
  SELECT FBFBM,MAX(CAST(RIGHT(DKBM,5) AS INT))+1 XH FROM DLXX_DK
  GROUP BY FBFBM
) L ON F.FBFBM=L.FBFBM";
			ExecSQL(db,sql);
			#endregion
		}
	}
	public class DYCode
	{
		public int BSM;
		public string ID;
		public string sjid;
		public string code;
		/// <summary>
		///  名称
		/// </summary>
		public string mc;
		/// <summary>
		/// 有效名称
		/// </summary>
		public string Yxmc;
		/// <summary>
		/// 扩展名称
		/// </summary>
		//public string kzmc
		//{
		//	get
		//	{
		//		if (sjCode == null)
		//		{
		//			return null;
		//		}
		//		return sjCode.qmc;
		//	}
		//}
		///// <summary>
		///// 全名称
		///// </summary>
		//public string qmc
		//{
		//	get
		//	{
		//		var lst = new List<string>();
		//		var sj = this;
		//		while (sj != null)
		//		{
		//			lst.Add(sj.mc);
		//			sj = sj.sjCode;
		//		}
		//		string s = null;
		//		for (int i = lst.Count - 1; i >= 0; --i)
		//		{
		//			if (s == null)
		//			{
		//				s = lst[i];
		//			}
		//			else
		//			{
		//				s += lst[i];
		//			}
		//		}
		//		return s;
		//	}
		//}
		/// <summary>
		/// 级别
		/// </summary>
		public int jb;
		///// <summary>
		///// 上级代码
		///// </summary>
		//public DYCode sjCode;
		public DYCode(int jb_)
		{
			jb = jb_;
		}
		public override string ToString()
		{
			return ID + " " + code + " " + Yxmc;
		}
	}
}
