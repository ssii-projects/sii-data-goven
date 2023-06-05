using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Agro.Library.Common
{
	public interface IRepairDB
	{
		void DoRepair(IWorkspace db, bool fUseTransaction = true);
	}

	public class RepairTableJZD : IRepairDB
	{
		public void DoRepair(IWorkspace db, bool fUseTransaction = true)
		{
			var cnt = db.QueryOneInt(" select count(*) from XTPZ_LSH where LX='JZDH'");
			if(0== cnt)
			//if (null == db.QueryOne(" select top(1) LX from XTPZ_LSH where LX='JZDH'"))
			{
				{
					var sw = Stopwatch.StartNew();
					var dic = new Dictionary<string, int>();
					var sql = "select DKBM,JZDH from DLXX_DK_JZD";
					db.QueryCallback(sql, r =>
					{
						if (!r.IsDBNull(0) && !r.IsDBNull(1))
						{
							var dkbm = r.GetString(0);
							if (dkbm.Length > 9)
							{
								var s = r.GetString(1);
								if (s.Length > 1)
								{
									var ch = s[0];
									if (ch < '0' || ch > '9')
									{
										s = s.Substring(1);
									}
									var n = SafeConvertAux.ToInt32(s);
									var key = dkbm.Substring(0, 9);
									if (dic.TryGetValue(key, out int nh))
									{
										if (n > nh)
											dic[key] = n;
									}
									else
									{
										dic[key] = n;
									}
								}
							}
						}
						return true;
					});

					void insert()
					{
						foreach (var kv in dic)
						{
							XTPZ_LSHUtil.InsertMaxJzdh(db, kv.Key, kv.Value);
						}
					}
					if (fUseTransaction)
					{
						Try.Catch(() => DBUtil.UseTransaction(db, () => insert()), false);
					}
					else
					{
						insert();
					}

					sw.Stop();
					Console.WriteLine($"int XTPZ_LSH time:{sw.Elapsed}");
				}
			}
		}
	}
	public class RepairTableJzx : IRepairDB
	{
		public void DoRepair(IWorkspace db, bool fUseTransaction = true)
		{
            var cnt = db.QueryOneInt(" select count(*) from XTPZ_LSH where LX='JZXH'");
            if (0==cnt)//null == db.QueryOne(" select top(1) LX from XTPZ_LSH where LX='JZXH'"))
			{
				var sw = Stopwatch.StartNew();
				var dic = new Dictionary<string, int>();
				var sql = "select DKBM,JZXH from DLXX_DK_JZX";
				db.QueryCallback(sql, r =>
				{
					if (!r.IsDBNull(0) && !r.IsDBNull(1))
					{
						var dkbm = r.GetString(0);
						if (dkbm.Length > 9)
						{
							var s = r.GetString(1);
							if (s.Length > 1)
							{
								var ch = s[0];
								if (ch < '0' || ch > '9')
								{
									s = s.Substring(1);
								}
								var n = SafeConvertAux.ToInt32(s);
								var key = dkbm.Substring(0, 9);
								if (dic.TryGetValue(key, out int nh))
								{
									if (n > nh)
										dic[key] = n;
								}
								else
								{
									dic[key] = n;
								}
							}
						}
					}
					return true;
				});

				void insert()
				{
					foreach (var kv in dic)
					{
						XTPZ_LSHUtil.InsertMaxJzxh(db, kv.Key, kv.Value);
					}
				}
				if (fUseTransaction)
				{
					Try.Catch(() => DBUtil.UseTransaction(db, () => insert()), false);
				}
				else
				{
					insert();
				}

				sw.Stop();
				Console.WriteLine($"init XTPZ_LSH JZXH time:{sw.Elapsed}");
			}
		}
	}

	/// <summary>
	/// yxm 2018-3-15 数据库修复实用类 
	/// </summary>
	public class RepairDBUtil
	{
		class RepairTableSJHZ : IRepairDB
		{
			public void DoRepair(IWorkspace db, bool fUseTransaction = true)
			{
                switch (db.DatabaseType)
                {
                    case eDatabaseType.Oracle: RepairOracleDB(db); break;
                    case eDatabaseType.SqlServer: RepairSQLSeverDB(db); break;
                    case eDatabaseType.MySql: RepairMySqlDB(db); break;
                }
            }
			private void RepairOracleDB(IWorkspace db)
			{
				string sql = null;
				#region 修复数据汇总(SJHZ)表
				if (!db.IsTableExists("SJHZ"))
				{
					sql = @" CREATE TABLE SJHZ 
   (	ID VARCHAR2(38 BYTE) NOT NULL ENABLE, 
	BM VARCHAR2(14 BYTE) NOT NULL ENABLE, 
	DJ NUMBER(*,0) NOT NULL ENABLE, 
	QSDWDM VARCHAR2(14 BYTE) NOT NULL ENABLE, 
	QSDWMC NVARCHAR2(150), 
	FBFZS NUMBER(*,0), 
	CBDKZS NUMBER(*,0), 
	CBDKZMJ NUMBER(18,3), 
	FCBDKZS NUMBER(*,0), 
	FCBDZMJ NUMBER(18,3), 
	BFQZZS NUMBER(*,0), 
	NYYTZMJ NUMBER(18,3), 
	ZZYZMJ NUMBER(18,3), 
	LYZMJ NUMBER(18,3), 
	XMYZMJ NUMBER(18,3), 
	YYZMJ NUMBER(18,3), 
	FNYYTZMJ NUMBER(18,3), 
	NYYFNYYTZMJ NUMBER(18,3), 
	JTTDSYQMJ NUMBER(18,3), 
	CMXZSYMJ NUMBER(18,3), 
	CJJTJJZZMJ NUMBER(18,3), 
	XJJTJJZZMJ NUMBER(18,3), 
	QTJTJJZZMJ NUMBER(18,3), 
	GYTDSYQMJ NUMBER(18,3), 
	SYQMJ NUMBER(18,3), 
	ZLDMJ NUMBER(18,3), 
	JDDMJ NUMBER(18,3), 
	KHDMJ NUMBER(18,3), 
	QTJTTDMJ NUMBER(18,3), 
	FCBDMJHJ NUMBER(18,3), 
	JBNTMJ NUMBER(18,3), 
	FJBNTMJ NUMBER(18,3), 
	JBNTMJHJ NUMBER(18,3), 
	JTCBQZS NUMBER(*,0), 
	QTCBQZS NUMBER(*,0), 
	BFQZSL NUMBER(*,0), 
	BFQZMJ NUMBER(18,3), 
	CBFZS NUMBER(*,0), 
	CBNHS NUMBER(*,0), 
	CBNHCYS NUMBER(*,0), 
	QTFSCBHJ NUMBER(*,0), 
	DWCBZS NUMBER(*,0), 
	GRCBZS NUMBER(*,0)
   )";
					db.ExecuteNonQuery(sql);
				}
				#endregion
			}
			private void RepairSQLSeverDB(IWorkspace db)
			{
				string sql = null;
				#region 修复数据汇总(SJHZ)表
				if (!db.IsTableExists("SJHZ"))
				{
					sql = @"CREATE TABLE [SJHZ](
	[ID] [uniqueidentifier] NOT NULL,
	[BM] [varchar](14) NOT NULL,
	[DJ] [int] NOT NULL,
	[QSDWDM] [varchar](14) NOT NULL,
	[QSDWMC] [nvarchar](150) NULL,
	[FBFZS] [int] NULL,
	[CBDKZS] [int] NULL,
	[CBDKZMJ] [numeric](18, 3) NULL,
	[FCBDKZS] [int] NULL,
	[FCBDZMJ] [numeric](18, 3) NULL,
	[BFQZZS] [int] NULL,
	[NYYTZMJ] [numeric](18, 3) NULL,
	[ZZYZMJ] [numeric](18, 3) NULL,
	[LYZMJ] [numeric](18, 3) NULL,
	[XMYZMJ] [numeric](18, 3) NULL,
	[YYZMJ] [numeric](18, 3) NULL,
	[FNYYTZMJ] [numeric](18, 3) NULL,
	[NYYFNYYTZMJ] [numeric](18, 3) NULL,
	[JTTDSYQMJ] [numeric](18, 3) NULL,
	[CMXZSYMJ] [numeric](18, 3) NULL,
	[CJJTJJZZMJ] [numeric](18, 3) NULL,
	[XJJTJJZZMJ] [numeric](18, 3) NULL,
	[QTJTJJZZMJ] [numeric](18, 3) NULL,
	[GYTDSYQMJ] [numeric](18, 3) NULL,
	[SYQMJ] [numeric](18, 3) NULL,
	[ZLDMJ] [numeric](18, 3) NULL,
	[JDDMJ] [numeric](18, 3) NULL,
	[KHDMJ] [numeric](18, 3) NULL,
	[QTJTTDMJ] [numeric](18, 3) NULL,
	[FCBDMJHJ] [numeric](18, 3) NULL,
	[JBNTMJ] [numeric](18, 3) NULL,
	[FJBNTMJ] [numeric](18, 3) NULL,
	[JBNTMJHJ] [numeric](18, 3) NULL,
	[JTCBQZS] [int] NULL,
	[QTCBQZS] [int] NULL,
	[BFQZSL] [int] NULL,
	[BFQZMJ] [numeric](18, 3) NULL,
	[CBFZS] [int] NULL,
	[CBNHS] [int] NULL,
	[CBNHCYS] [int] NULL,
	[QTFSCBHJ] [int] NULL,
	[DWCBZS] [int] NULL,
	[GRCBZS] [int] NULL
) ON [PRIMARY]";
					db.ExecuteNonQuery(sql);
				}
				#endregion

				#region 修改DLXX_DK表 yxm 2018-12-18
				if (!db.IsFieldExists("DLXX_DK", "DJSJ"))
				{
					db.ExecuteNonQuery("alter table DLXX_DK add DJSJ  datetime2(7) null");
				}
				if (!db.IsFieldExists("DLXX_DK", "MSSJ"))
				{
					db.ExecuteNonQuery("alter table DLXX_DK add MSSJ  datetime2(7) null");
				}
				if (!db.IsFieldExists("DLXX_DK", "SJLY"))
				{
					db.ExecuteNonQuery("alter table DLXX_DK add SJLY  int null default 0");
				}
				#endregion
			}

            private void RepairMySqlDB(IWorkspace db)
            {
				string sql;
                #region 修复数据汇总(SJHZ)表
                if (!db.IsTableExists("SJHZ"))
                {
					sql = @"CREATE TABLE SJHZ(
	ID varchar(38) NOT NULL,
	BM varchar(14) NOT NULL,
	DJ int NOT NULL,
	QSDWDM varchar(14) NOT NULL,
	QSDWMC nvarchar(150) NULL,
	FBFZS int NULL,
	CBDKZS int NULL,
	CBDKZMJ numeric(18, 3) NULL,
	FCBDKZS int NULL,
	FCBDZMJ numeric(18, 3) NULL,
	BFQZZS int NULL,
	NYYTZMJ numeric(18, 3) NULL,
	ZZYZMJ numeric(18, 3) NULL,
	LYZMJ numeric(18, 3) NULL,
	XMYZMJ numeric(18, 3) NULL,
	YYZMJ numeric(18, 3) NULL,
	FNYYTZMJ numeric(18, 3) NULL,
	NYYFNYYTZMJ numeric(18, 3) NULL,
	JTTDSYQMJ numeric(18, 3) NULL,
	CMXZSYMJ numeric(18, 3) NULL,
	CJJTJJZZMJ numeric(18, 3) NULL,
	XJJTJJZZMJ numeric(18, 3) NULL,
	QTJTJJZZMJ numeric(18, 3) NULL,
	GYTDSYQMJ numeric(18, 3) NULL,
	SYQMJ numeric(18, 3) NULL,
	ZLDMJ numeric(18, 3) NULL,
	JDDMJ numeric(18, 3) NULL,
	KHDMJ numeric(18, 3) NULL,
	QTJTTDMJ numeric(18, 3) NULL,
	FCBDMJHJ numeric(18, 3) NULL,
	JBNTMJ numeric(18, 3) NULL,
	FJBNTMJ numeric(18, 3) NULL,
	JBNTMJHJ numeric(18, 3) NULL,
	JTCBQZS int NULL,
	QTCBQZS int NULL,
	BFQZSL int NULL,
	BFQZMJ numeric(18, 3) NULL,
	CBFZS int NULL,
	CBNHS int NULL,
	CBNHCYS int NULL,
	QTFSCBHJ int NULL,
	DWCBZS int NULL,
	GRCBZS int NULL
)";
                    db.ExecuteNonQuery(sql);
                }
                #endregion
            }
        }

		class RepairLogTable : IRepairDB
		{
			private static readonly string TableName = "CS_LOG";
			public void DoRepair(IWorkspace db, bool fUseTransaction = true)
			{
				if (!db.IsTableExists(TableName))
				{
					#region 创建日志表
					switch (db.DatabaseType)
					{

						case eDatabaseType.Oracle:
							{
								var sql = $"create table {TableName}(ID VARCHAR2(38) NOT NULL,LOGTYPE VARCHAR2(30) NOT NULL,USERNAME VARCHAR2(20) NOT NULL,LOGTIME  TIMESTAMP default systimestamp,LOGINFO CLOB)";
								db.ExecuteNonQuery(sql);
							}
							break;
						case eDatabaseType.SqlServer:
							{
								var sql = $"create table {TableName}(ID VARCHAR(38) NOT NULL,LOGTYPE VARCHAR(30) NOT NULL,USERNAME VARCHAR(20) NOT NULL,LOGTIME  DATETIME default getdate(),LOGINFO TEXT)";
								db.ExecuteNonQuery(sql);
							}
							break;
						case eDatabaseType.MySql:
							{
								var sql = $"create table {TableName}(ID VARCHAR(38) NOT NULL,LOGTYPE VARCHAR(30) NOT NULL,USERNAME VARCHAR(20) NOT NULL,LOGTIME  TIMESTAMP DEFAULT CURRENT_TIMESTAMP  ,LOGINFO TEXT)";
								db.ExecuteNonQuery(sql);
							}
							break;
					}
					#endregion
				}
			}
		}
		class RepairCS_USER_PERMISSION : IRepairDB
		{
			private static readonly string TableName = "CS_USER_PERMISSION";
			public void DoRepair(IWorkspace db, bool fUseTransaction = true)
			{
				if (!db.IsTableExists(TableName))
				{
					#region 创建表 CS_USER_PERMISSION
					switch (db.DatabaseType)
					{

						case eDatabaseType.Oracle:
							{
								var sql = "create table CS_USER_PERMISSION(USERNAME VARCHAR2(20) NOT NULL,FUNCTION_ID VARCHAR2(50) NOT NULL,FLAG int NOT NULL)";
								db.ExecuteNonQuery(sql);
							}
							break;
						case eDatabaseType.SqlServer:
							{
								var sql = "create table CS_USER_PERMISSION(USERNAME VARCHAR(20) NOT NULL,FUNCTION_ID VARCHAR(50) NOT NULL,FLAG int NOT NULL)";
								db.ExecuteNonQuery(sql);
							}
							break;
                        case eDatabaseType.MySql:
                            {
                                var sql = "create table CS_USER_PERMISSION(USERNAME VARCHAR(20) NOT NULL,FUNCTION_ID VARCHAR(50) NOT NULL,FLAG int NOT NULL)";
                                db.ExecuteNonQuery(sql);
                            }
                            break;
                    }
					#endregion
				}
			}
		}
		class RepairCS_SYSINFO : IRepairDB
		{
			private static readonly string TableName = "CS_SYSINFO";
			public void DoRepair(IWorkspace db, bool fUseTransaction = true)
			{
				if (!db.IsTableExists(TableName))
				{
					#region 创建表 CS_USER_PERMISSION
					switch (db.DatabaseType)
					{
						case eDatabaseType.SqlServer:
							{
								var sql = $"create table {TableName}(ID VARCHAR(38) NOT NULL PRIMARY KEY,VALUE VARCHAR(250))";
								db.ExecuteNonQuery(sql);
							}
							break;
						case eDatabaseType.MySql:
							{
                                var sql = $"create table {TableName}(ID VARCHAR(38) NOT NULL PRIMARY KEY,VALUE VARCHAR(250))";
                                db.ExecuteNonQuery(sql);
                            }
							break;
					}
					#endregion
				}
			}
		}
		public class RepairDJ_CBJYQ_DJB : IRepairDB
		{
			public void DoRepair(IWorkspace db, bool fUseTransaction = true)
			{
				var tbName = DJ_CBJYQ_DJB.GetTableName();
				if (!db.IsFieldExists(tbName, "FBFMC"))
				{//2019-11-19 DJ_CBJYQ_DJB 添加 FBFMC 和 FBFFZRXM 字段
					db.ExecuteNonQuery($"ALTER TABLE {tbName} ADD FBFMC nvarchar(50)");
					db.ExecuteNonQuery($"ALTER TABLE {tbName} ADD FBFFZRXM nvarchar(50)");
				}
			}
		}


		public static void Repair(IWorkspace db)
		{
			//try
			//{
				var sa = new IRepairDB[]{
					new RepairTableSJHZ(),new RepairTableJZD(),new RepairTableJzx()
					,new RepairLogTable(),new RepairCS_USER_PERMISSION(),new RepairDJ_CBJYQ_DJB()
					,new RepairCS_SYSINFO()
				};
				foreach (var it in sa)
				{
					it.DoRepair(db);
				}
				DLXX_DK.RepairTable(db);
				if (!DC_DLXX_DK.IsTableExists(db))
				{
					var srid = db.GetSRID(DLXX_XZDY.GetTableName());
					DC_DLXX_DK.CreateFeatureClass(db as IFeatureWorkspace, srid, it =>
					 {
						 var field = it.FindField(x => x.FieldName == "DKBM") as Field;
						 field.PrimaryKey = true;
						 field.IsNullable = false;
						 FieldsUtil.AddOidField(it, "BSM", "标识码");
						 //FieldsUtil.AddDateTimeField(it, "XGSJ", "修改时间");
						 //FieldsUtil.AddDoubleField(it, "JDDMJ", 15, 4, "其它面积");
						 FieldsUtil.AddIntField(it, "USED").DefaultValue = 0;
						 //FieldsUtil.AddIntField(it, "DJZT");
					 });
				}
				else
				{
					DC_DLXX_DK.RepairTable(db);
				}
				if (!DC_QSSJ_CBF_JTCY.IsTableExists(db))
				{
					DC_QSSJ_CBF_JTCY.CreateTable(db);
				}
				if (!DC_QSSJ_CBF.IsTableExists(db))
				{
					DC_QSSJ_CBF.CreateTable(db, it =>
					 {
						 (it.GetField(it.FindField(nameof(DC_QSSJ_CBF.CBFBM))) as Field).Length = 18;
						 FieldsUtil.AddIntField(it, "USED").DefaultValue = 0;
					 });
				}
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.Message);
			//	MessageBox.Show(ex.Message);
			//}
		}
	}

	public class MyDBUtil
	{
		/// <summary>
		/// 检查是否有效的承包经营权数据库
		/// if ok return null else return errinfo
		/// yxm 2018-11-30
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		public static string IsValidDB(IWorkspace db)
		{
			var sa = new string[] { "DLXX_XZDY", "DLXX_DK", "DLXX_DK_JZD", "DLXX_DK_JZX" };
			foreach (var tbName in sa)
			{
				if (!db.IsTableExists(tbName))
				{
					var err = "表：" + tbName + "不存在，请检查是否承包经营权数据库！";
					return err;
				}
			}
			return null;
		}


	}
	public class XTPZ_LSHUtil
	{
		#region JZD
		public static int GetMaxJzdh(IWorkspace db, string xiangBM)
		{
			var sql = $"select BH from XTPZ_LSH where LX='JZDH' and FZM='{xiangBM}'";
			var o = db.QueryOne(sql);
			if (o == null)
			{
				int nMax = 0;
				InsertMaxJzdh(db, xiangBM, nMax);
				return nMax;
			}
			return SafeConvertAux.ToInt32(o);
		}
		public static void UpdateMaxJzdh(IWorkspace db, string xiangBM, int nJzdh)
		{
			var sql = $"update XTPZ_LSH set BH={nJzdh} where LX='JZDH' and FZM='{xiangBM}'";
			db.ExecuteNonQuery(sql);
		}
		public static void InsertMaxJzdh(IWorkspace db, string xiangBM, int nJzdh)
		{
			var sql = $"insert into XTPZ_LSH(ID,LX,FZM,BH) values('{Guid.NewGuid().ToString()}','JZDH','{xiangBM}',{nJzdh})";
			db.ExecuteNonQuery(sql);
		}
		#endregion JZD

		#region JZX
		public static int GetMaxJzxh(IWorkspace db, string xiangBM)
		{
			var sql = $"select BH from XTPZ_LSH where LX='JZXH' and FZM='{xiangBM}'";
			var o = db.QueryOne(sql);
			if (o == null)
			{
				int nMax = 0;
				InsertMaxJzxh(db, xiangBM, nMax);
				return nMax;
			}
			return SafeConvertAux.ToInt32(o);
		}
		public static void UpdateMaxJzxh(IWorkspace db, string xiangBM, int nJzxh)
		{
			var sql = $"update XTPZ_LSH set BH={nJzxh} where LX='JZXH' and FZM='{xiangBM}'";
			db.ExecuteNonQuery(sql);
		}
		public static void InsertMaxJzxh(IWorkspace db, string xiangBM, int nJzxh)
		{
			var sql = $"insert into XTPZ_LSH(ID,LX,FZM,BH) values('{Guid.NewGuid().ToString()}','JZXH','{xiangBM}',{nJzxh})";
			db.ExecuteNonQuery(sql);
		}
		#endregion JZX
	}
}
