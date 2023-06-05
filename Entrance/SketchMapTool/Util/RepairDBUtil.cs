using SII.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SII.FrameApp
{
    /// <summary>
    /// yxm 2018-3-15 数据库修复实用类 
    /// </summary>
    class RepairDBUtil
    {
        class RepairTableSJHZ
        {
            public void DoRepair(IWorkspace db)
            {
                var fOracle = db.DatabaseType == eDatabaseType.Oracle;
                if (fOracle)
                {
                    RepairOracleDB(db);
                }
                else
                {
                    RepairSQLSeverDB(db);
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
            }
        }

        public static void Repair(IWorkspace db)
        {
            try
            {
                new RepairTableSJHZ().DoRepair(db);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
