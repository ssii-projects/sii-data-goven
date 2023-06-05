/*
 * (C) 2017 xx公司版权所有，保留所有权利
 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   IImportShape
 * 创 建 人：   颜学铭
 * 创建时间：   2017/5/26 18:07:34
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.Library.Common;
using Agro.Module.DataExchange;
using Agro.Module.PreproccessTool;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
    public interface IImportTable
    {
        string TableName { get;  }
        int RecordCount{get;}
        void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel);
    }

    public class ImportTableBase
    {
        public string TableName
        {
            get;
            private set;
        }
        public int RecordCount
        {
            get;
            set;
        }
        protected ImportTableBase(string tableName)
        {
            TableName = tableName;
        }
        /// <summary>
        /// 判断表中是否存在数据，若存在数据则不执行导入
        /// return false表示检查未通过（有数据）
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        protected bool PreImportCheck(IWorkspace db, ReportInfo reportInfo)
        {
            #region 判断表中是否存在数据，若存在数据则不执行导入
            var sql = "select count(*) from " + TableName;
            var cnt = SafeConvertAux.ToInt32(SqlUtil.QueryOne(db, sql));
            if (cnt > 0)
            {
                reportInfo.reportProgress(100);
                reportInfo.reportWarning("表" + TableName + "中已存在数据，不允许执行此操作！");
                return false;
            }
            #endregion
            return true;
        }

        public static void ClearTableData(IWorkspace db, string tableName)
        {
            db.ExecuteNonQuery("truncate table " + tableName);
            //db.ExecuteNonQuery("delete from " + tableName);
        }
        protected void ClearTableData(IWorkspace db)//, string tableName)
        {
            ClearTableData(db, TableName);
           // db.ExecuteNonQuery("truncate table " + TableName);
        }
        protected static bool IsTableHasData(IWorkspace db, string tableName)
        {
            bool fHasData = false;
            var sql = "select count(1) from " + tableName;
            db.QueryCallback(sql, r =>
            {
                fHasData = SafeConvertAux.ToInt32(r.GetValue(0)) > 0;
                return false;
            });
            return fHasData;
        }
        protected bool IsTableHasData(IWorkspace db)
        {
            return IsTableHasData(db, TableName);
        }

    }
    //public class ReportInfo
    //{
    //    public Action<double> reportProgress;
    //    public Action<string> reportError;
    //    public Action<string> reportInfo;
    //}
}
