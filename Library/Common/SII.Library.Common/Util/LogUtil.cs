using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common.Util
{
    public class LogUtil
    {
        /// <summary>
        /// 写登录日志
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sUserName"></param>
        public static void WriteLoginLog(IWorkspace db, string sUserName)
        {
            var sql = "insert into CS_LOG(ID,LOGTYPE,USERNAME,LOGINFO) values('" + Guid.NewGuid().ToString() + "','系统日志','" + sUserName + "','登录系统')";
            db.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 写系统日志
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sModuleName"></param>
        public static void WriteSystemLog(IWorkspace db, string sModuleName)
        {
            var sUserName =MyGlobal.LoginUser.Name;
            sModuleName =""+ sModuleName.Replace("'", "''");
            var sql = "insert into CS_LOG(ID,LOGTYPE,USERNAME,LOGINFO) values('" + Guid.NewGuid().ToString() + "','系统日志','" + sUserName + "','" + sModuleName + "')";
            db.ExecuteNonQuery(sql);
        }
        /// <summary>
        /// 写异常日志
        /// </summary>
        /// <param name="db"></param>
        /// <param name="err"></param>
        public static void WriteExceptionLog(IWorkspace db, string err)
        {
            var sUserName = MyGlobal.LoginUser.Name;
            err = err.Replace("'", "''");
            var sql = "insert into CS_LOG(ID,LOGTYPE,USERNAME,LOGINFO) values('" + Guid.NewGuid().ToString() + "','异常日志','" + sUserName + "','"+err+"')";
            db.ExecuteNonQuery(sql);
        }
    }
}
