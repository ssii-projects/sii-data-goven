/*
 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.5
 * 文 件 名：   DatabaseFactory
 * 创 建 人：   颜学铭
 * 创建时间：   2016/3/16 10:24:27
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agro.LibCore;

namespace Agro.Module.ImportData
{
    /// <summary>
    /// 创建IDatabase的工厂类
    /// </summary>
    public class DatabaseFactory
    {
        /// <summary>
        /// 创建Access类型的IDataBase实例
        /// </summary>
        /// <param name="mdbFile"></param>
        /// <returns></returns>
        public static IDataBase CreateAccess(string mdbFile)
        {
            return new ImpAccess(mdbFile);
        }
        //public static IDataBase OpenSqlServerWorkspace(string host, string dbName, string user, string pwd)
        //{
        //    return new ImpSQLServer(host, dbName, user, pwd);
        //}
    }

    /// <summary>
    ///接口: IDataBase
    ///描述: 定义了IDataBase接口，执行sql，事物相关、获得下一个ID的方法
    ///版本:1.0
    ///日期:2016/3/16 10:13:32
    ///作者:颜学铭
    /// </summary>
    public interface IDataBase:IDisposable
    {

        /// <summary>
        ///  执行一条SQL语句,可进行增，删，改
        /// </summary>
        /// <param name="se">sql 实体包括sql语句和param</param>
        /// <returns>返回受影响的行</returns>
        int ExecuteSql(string sql, KeyValuePair<String, Object>[] Params=null);

        /// <summary>
        /// 执行一条SQL语句,返回一个数据集
        /// </summary>
        /// <param name="sql">sql 实体包括sql</param>
        /// <param name="Params">可以为null</param>
        /// <returns>返回符合条件的数据集合</returns>
        DataSet QueryDataSet(string sql, KeyValuePair<String, Object>[] Params=null);
        DataTable QueryDataTable(string sql, KeyValuePair<String, Object>[] Params = null);

        //开始一个数据库事务
        void BeginTransaction();

        //提交一个事务
        void CommitTransaction();

        //回滚当前事务
        void RollbackTransaction();

        /// <summary>
        /// 判断当前是否开启了事务
        /// </summary>
        /// <returns>布尔值</returns>
        bool IsTransactionBegining();
        /// <summary>
        /// 获得下一个ID
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>ID</returns>
        int GetNextObjectID(String tableName);

        /// <summary>
        /// 获取表的字段名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        List<string> QueryFields(string tableName);

        /// <summary>
        /// 获得前缀
        /// </summary>
        /// <returns>字符</returns>
        char GetParamPrefix();
        /// <summary>
        /// ISqlFunction 接口
        /// </summary>
        ISqlFunction SqlFunc { get; }
        //ConnectionInfo ConInfo { get; }
        System.Data.IDataReader QueryReader(String sql);
    }

    /// <summary>
    ///描述: 定义ISqlFunction接口，映射各种数据库的SQL函数
    ///版本:1.0
    ///日期:2016/3/16 10:17:58
    ///作者:颜学铭
    /// </summary>
    public interface ISqlFunction
    {
        /// <summary>
        /// 取用int表示日期的年的函数
        /// </summary>
        /// <param name="intDateField">intdate字符串</param>
        /// <returns>年</returns>
        string IntDate_Year(string intDateField);
        /// <summary>
        /// 取用int表示日期的月的函数
        /// </summary>
        /// <param name="intDateField">intdate字符串</param>
        /// <returns>月</returns>
        string IntDate_Month(string intDateField);
        /// <summary>
        /// 取用double表示日期的年的函数
        /// </summary>
        /// <param name="doubleDateField">doubleDateField字符串</param>
        /// <returns></returns>
        string DoubleDate_Year(string doubleDateField);
        /// <summary>
        /// 取用double表示日期的月的函数
        /// </summary>
        /// <param name="doubleDateField">doubleDateField字符串</param>
        /// <returns>月</returns>
        string DoubleDate_Month(string doubleDateField);
        /// <summary>
        /// 转换为date
        /// </summary>
        /// <param name="dateTime">日期</param>
        /// <returns>日期</returns>
        string ToDate(DateTime dateTime);
        /// <summary>
        /// 返回与指定数值表达式对应的字符
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns>字符</returns>
        string ToStr(string field);
        string ToDouble(string expr);
        /// <summary>
        /// 返回文本字段中值的长度
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns>字符串长度</returns> 
        string StrLen(string field);
        /// <summary>
        /// 返回字符串在另一个字符串中的起始位置 ， subStr是要到field中寻找的字符中 
        /// </summary>
        /// <param name="field">目标字段</param>
        /// <param name="subStr">处理字段</param>
        /// <returns>subStr在field中的位置</returns> 
        string StrPos(string field, string subStr);
        /// <summary>
        /// 截取字符串中的空格
        /// </summary>
        /// <param name="strField">字符串</param>
        /// <returns>去掉空格的字符串</returns>
        string Trim(string strField);
        /// <summary>
        /// sql语句拼接
        /// </summary>
        /// <returns>返回通配符</returns>
        string ConcactOperator();
        /// <summary>
        /// sql语句截取
        /// </summary>
        /// <returns>返回截取字符串通配符</returns>
        string SubStringCastOperator();
        /// <summary>
        /// 模糊查询
        /// </summary>
        /// <returns>模糊查询通配符</returns>
        string LikeCastOperator();
        /// <summary>
        /// 根据表达式返回特定的值
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="trueSql">如果为真语句</param>
        /// <param name="falseSql">如果为假语句</param>
        /// <returns>符合条件的sql语句</returns>
        string if_else(string condition, string trueSql, string falseSql);
        /// <summary>
        /// 返回由逻辑测试确定的两个数值或字符串值之一
        /// </summary> 
        /// <param name="caseSql">case语句</param>
        /// <param name="dicWhen">when字典</param>
        /// <param name="elseSql">else语句</param>
        /// <returns>符合条件的sql语句</returns>
        string CaseWhen(string caseSql, Dictionary<string, string> dicWhen, string elseSql);
        /// <summary>
        /// 返回数据库当前时间的sql函数
        /// </summary>
        /// <returns>当前时间</returns>
        string GetServerCurrentTime();
        /// <summary>
        /// 返回小数点保留位数的函数（按四舍五入）
        /// </summary>
        /// <param name="fieldExpr"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        string Round(string fieldExpr, int n);
    }

    public class DatabaseUtil
    {
        public static int QueryOneInt(IDataBase db, string sql)
        {
            using (var dr = db.QueryReader(sql))
            {
                if (dr.Read())
                {
                    var o = dr.GetValue(0);
                    return SafeConvertAux.ToInt32(o);
                }
                return 0;
            }
        }
    }

    /// <summary>
    ///描述: IDataBase接口实现类(针对Access数据库）
    ///版本:1.0
    ///作者:颜学铭
    ///<summary>
    internal class ImpAccess : IDataBase
    {
        /// <summary>
        /// OleDbCommand 命令
        /// </summary>
        private readonly OleDbCommand _cmd = null;
        /// <summary>
        /// OleDbConnection 命令
        /// </summary>
        private readonly OleDbConnection _conn = null;
        /// <summary>
        /// ISqlFunction 对象
        /// </summary>
        private readonly ISqlFunction _sqlFunc;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="mdbFile">数据库名称</param>
        internal ImpAccess(string mdbFile)
        {
            if (!System.IO.File.Exists(mdbFile))
                throw new Exception("文件：" + mdbFile + "不存在！");
            //ConInfo = new ConnectionInfo() { Server = mdbFile, dbType = Database.Constants.DBType.Access };
            string ADOConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mdbFile + ";Jet OLEDB:Database Password=amdin";
            _conn = new OleDbConnection(ADOConnectionString);
            _cmd = new OleDbCommand() { Connection = _conn };
            var fUsedInADO = true;
            _sqlFunc = new AccessSqlFunction(fUsedInADO ? "%" : "*");
        }

        #region IDataBase
        /// <summary>
        /// 返回下一个ObjectID，为当前搜索中最大值的下一个
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>ID</returns>
        public int GetNextObjectID(String tableName)
        {
            DataSet ds = QueryDataSet("SELECT MAX(objectid) FROM " + tableName, null);
            if (ds.Tables[0].Rows.Count == 0)
                return 1;
            Object val = ds.Tables[0].Rows[0][0];
            if (val == DBNull.Value)
                return 1;
            int oid = Convert.ToInt32(val) + 1;
            return oid;
        }
        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="Params">参数</param>
        /// <returns>受影响的行</returns>
        public int ExecuteSql(string sql, KeyValuePair<String, Object>[] Params=null)
        {
            lock (_conn)
            {
                int result = 0;
                bool fClose = _conn.State != ConnectionState.Open;
                try
                {
                    if (fClose)
                        _conn.Open();
                    PrepareCommand(sql, Params);
                    result = _cmd.ExecuteNonQuery();
                }
                finally
                {
                    if (fClose)
                    {
                        _conn.Close();
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 返回DataSet
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="Params">参数</param>
        /// <returns>DataSet</returns>
        public DataSet QueryDataSet(string sql, KeyValuePair<String, Object>[] Params=null)
        {
            lock (_conn)
            {
                DataSet ds = new DataSet();
                bool fClose = _conn.State != ConnectionState.Open;
                try
                {
                    if (fClose)
                        _conn.Open();
                    PrepareCommand(sql, Params);

                    OleDbDataAdapter da = new OleDbDataAdapter(_cmd);

                    //Excute Command ,then fill DataSet
                    da.Fill(ds);
                }
                finally
                {
                    if (fClose)
                        _conn.Close();
                }

                return ds;
            }
        }
        public DataTable QueryDataTable(string sql, KeyValuePair<String, Object>[] Params = null)
        {
            var ds = QueryDataSet(sql, Params);
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }
        public System.Data.IDataReader QueryReader(String sql)
        {
            lock (_conn)
            {
                bool fClose = _conn.State != ConnectionState.Open;
                try
                {
                    if (fClose)
                        _conn.Open();
                    PrepareCommand(sql, null);
                    return _cmd.ExecuteReader();
                    //OleDbDataAdapter da = new OleDbDataAdapter(_cmd);

                    ////Excute Command ,then fill DataSet
                    //da.Fill(ds);
                }
                finally
                {
                    //if (fClose)
                    //    _conn.Close();
                }
            }
        }
        /// <summary>
        /// 获取表的字段名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<string> QueryFields(string tableName)
        {
            var lst = new List<string>();
            var sql = "select * from " + tableName + " where 1<0";
            using (var r = QueryReader(sql))
            {
                for (int i = 0; i < r.FieldCount; ++i)
                {
                    lst.Add(r.GetName(i));
                }
            }
            //using (var cmd = new SqlCommand(sql, _conn))
            //{
            //    using (var r = cmd.ExecuteReader())
            //    {
            //        for (int i = 0; i < r.FieldCount; ++i)
            //        {
            //            lst.Add(r.GetName(i));
            //        }
            //    }
            //}
            return lst;
        }
        ///// <summary>
        ///// 获取表的字段名
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public List<string> QueryFields(string tableName)
        //{
        //    var lst = new List<string>();
        //    var sql = "select * from " + tableName + " where 1<0";
        //    using (var cmd = new SqlCommand(sql, _con))
        //    {
        //        using (var r = cmd.ExecuteReader())
        //        {
        //            for (int i = 0; i < r.FieldCount; ++i)
        //            {
        //                lst.Add(r.GetName(i));
        //            }
        //        }
        //    }
        //    return lst;
        //}
        //public DateTime GetServerCurrentTime()
        //{
        //    DataSet ds=ExecuteDataSet("SELECT NOW()",null);
        //    object val = ds.Tables[0].Rows[0][0];
        //    return Convert.ToDateTime(val);
        //}
        #region 事务
        /// <summary>
        /// 开始事务，打开连接
        /// </summary>
        public void BeginTransaction()
        {
            System.Diagnostics.Debug.Assert(_conn.State != ConnectionState.Open);
            _conn.Open();
            _cmd.Transaction = _conn.BeginTransaction();
        }
        /// <summary>
        /// 提交事务，关闭连接
        /// </summary>
        public void CommitTransaction()
        {
            System.Diagnostics.Debug.Assert(_conn.State == ConnectionState.Open);
            _cmd.Transaction.Commit();
            _cmd.Transaction = null;
            _conn.Close();
        }
        /// <summary>
        /// 回滚事务，关闭连接
        /// </summary>
        public void RollbackTransaction()
        {
            System.Diagnostics.Debug.Assert(_conn.State == ConnectionState.Open);
            _cmd.Transaction.Rollback();
            _cmd.Transaction = null;
            _conn.Close();
        }
        /// <summary>
        /// 查询cmd是否有事务
        /// </summary>
        /// <returns>真假</returns>
        public bool IsTransactionBegining()
        {
            return _cmd.Transaction != null;
        }
        #endregion
        /// <summary>
        /// 返回参数标识符
        /// </summary>
        /// <returns>'@'</returns>
        public char GetParamPrefix()
        {
            return '@';
        }
        /// <summary>
        /// ISqlFunction 接口
        /// </summary>
        public ISqlFunction SqlFunc { get { return _sqlFunc; } }
        //public ConnectionInfo ConInfo { get; private set; }
        public void Dispose()
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
            if (_conn != null)
            {
                _conn.Dispose();
            }
        }
        #endregion IDataBase

        /// <summary>
        /// 重新设置_cmd
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="Params">参数</param>
        private void PrepareCommand(string sql, KeyValuePair<String, Object>[] Params)
        {
            //Set up the command
            _cmd.Parameters.Clear();
            //_cmd.Transaction = _Transaction;
            _cmd.CommandText = sql;
            _cmd.CommandType = CommandType.Text;

            // Bind the parameters passed in
            if (Params != null)
            {
                foreach (KeyValuePair<String, Object> parm in Params)
                {
                    OleDbParameter sp = new OleDbParameter(parm.Key, parm.Value);
                    _cmd.Parameters.Add(sp);
                }
            }
        }
    }
    /// <summary>
    ///描述: 实现Access数据库的常用函数.ISqlFunction实现类（针对Access数据库）
    ///版本:1.0
    ///作者:颜学铭
    /// </summary>
    public class AccessSqlFunction : ISqlFunction
    {
        /// <summary>
        /// sql中like语句的通配符
        /// </summary>
        private string _likeCastOp = "%";
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="likeCastOp">基于ADO方式的传"%"，基于ArcGIS Engine方式的传"*"</param>
        internal AccessSqlFunction(string likeCastOp = "%")
        {
            _likeCastOp = likeCastOp;
        }
        #region ISqlFunction 默认针对Access的实现
        /// <summary>
        /// 年度转换为int
        /// </summary> 
        /// <param name="intDateField">日期字符串</param>
        /// <returns>Int时间</returns>
        public string IntDate_Year(string intDateField)
        {
            return " Int(" + intDateField + "/10000)";
        }
        /// <summary>
        /// 月份转换为int
        /// </summary> 
        /// <param name="intDateField">日期字符串</param>
        /// <returns>Int时间</returns>
        public string IntDate_Month(string intDateField)
        {
            return " Int((" + intDateField + " mod 10000)/100)";
        }
        /// <summary>
        /// 取用double表示日期的年的函数
        /// </summary>
        /// <param name="doubleDateField">doubleDateField字符串</param>
        /// <returns></returns>
        public string DoubleDate_Year(string doubleDateField)
        {
            return " Int(" + doubleDateField + "/10000000000)";
        }
        /// <summary>
        /// 取用double表示日期的月的函数
        /// </summary>
        /// <param name="doubleDateField">doubleDateField字符串</param>
        /// <returns>月</returns>
        public string DoubleDate_Month(string doubleDateField)
        {
            return " Int((" + doubleDateField + " mod 10000000000)/100000000)";
            // " floor(mod(" + doubleDateField + ",10000000000)/100000000)";
        }
        /// <summary>
        /// 年度转换为时间
        /// </summary> 
        /// <param name="dateTimeField">时间字符串</param>
        /// <returns>时间</returns>
        public string Year(string dateTimeField)
        {
            return " year(" + dateTimeField + ")";
        }
        /// <summary>
        /// 月份转换为时间
        /// </summary> 
        /// <param name="dateTimeField">时间字符串</param>
        /// <returns>时间</returns>
        public string Month(string dateTimeField)
        {
            return " month(" + dateTimeField + ")";
        }
        /// <summary>
        /// 年度转换为int
        /// </summary> 
        /// <param name="dateTimeField">时间字符串</param>
        /// <returns>时间</returns>
        public string Day(string dateTimeField)
        {
            return " day(" + dateTimeField + ")";
        }
        /// <summary>
        /// 将年度转换为日期格式
        /// </summary> 
        /// <param name="dateTime">时间字符串</param>
        /// <returns>日期格式时间</returns>
        public string ToDate(DateTime dateTime)
        {
            return " CDate('" + dateTime.ToString() + "')";
        }
        /// <summary>
        /// 将数字转换成字符串
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns>字符串字段</returns> 
        public string ToStr(string field)
        {
            return " str(" + field + ")";
        }
        public string ToDouble(string expr)
        {
            return " CDbl(" + expr + ")";
        }
        /// <summary>
        /// 返回字符串长度
        /// </summary> 
        /// <param name="field">字段</param>
        /// <returns>字段长度</returns> 
        public string StrLen(string field)
        {
            return " len(" + field + ")";
        }

        /// <summary>
        /// 查询子串在查询中的位置
        /// </summary> 
        /// <param name="field">目标字段</param>
        /// <param name="subStr">处理字段</param>
        /// <returns>subStr在field中的位置</returns> 
        public string StrPos(string field, string subStr)
        {
            return " InStr(" + field + "," + subStr + ")";
        }
        /// <summary>
        /// 截取字符串两头的空格
        /// </summary>
        /// <param name="strField">字符串</param>
        /// <returns>去掉空格的字符串</returns> 
        public string Trim(string strField)
        {
            return " trim(" + strField + ")";
        }
        /// <summary>
        /// sql语句拼接
        /// </summary>
        /// <returns>返回通配符“+”</returns>
        public string ConcactOperator()
        {
            return "+";
        }
        /// <summary>
        /// 截取字符串通配符
        /// </summary>
        /// <returns>返回截取字符串通配符MID</returns>
        public string SubStringCastOperator()
        {
            return "MID";
        }
        /// <summary>
        /// 模糊查询
        /// </summary> 
        /// <returns>模糊查询通配符</returns>
        public string LikeCastOperator()
        {
            return _likeCastOp;
        }
        /// <summary>
        /// 根据表达式返回特定的值
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="trueSql">如果为真语句</param>
        /// <param name="falseSql">如果为假语句</param>
        /// <returns>符合条件的sql语句</returns>
        public string if_else(string condition, string trueSql, string falseSql)
        {
            return " iif(" + condition + "," + trueSql + "," + falseSql + ")";
        }
        /// <summary>
        /// 返回由逻辑测试确定的两个数值或字符串值之一
        /// </summary> 
        /// <param name="caseSql">case语句</param>
        /// <param name="dicWhen">when字典</param>
        /// <param name="elseSql">else语句</param>
        /// <returns>符合条件的sql语句</returns>
        public string CaseWhen(string caseSql, Dictionary<string, string> dicWhen, string elseSql)
        {
            string s = "";
            foreach (KeyValuePair<string, string> kv in dicWhen)
            {
                if (s != "")
                    s += ",";
                s += " iif((" + caseSql + "=" + kv.Key + ")," + kv.Value;
            }
            s += "," + elseSql;
            for (int i = 0; i < dicWhen.Count; ++i)
                s += ")";
            return s;
        }
        /// <summary>
        /// 返回数据库当前时间的sql函数
        /// </summary>
        /// <returns>数据库当前时间</returns>
        public string GetServerCurrentTime()
        {
            return "SELECT NOW()";
        }
        /// <summary>
        /// 返回小数点保留位数的函数（按四舍五入）
        /// </summary>
        /// <param name="fieldExpr"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public string Round(string fieldExpr, int n)
        {
            return "round(" + fieldExpr + "," + n + ")";
        }
        #endregion ISqlFunction
    }


    //internal class ImpSQLServer : IDataBase
    //{
    //    /// <summary>
    //    /// ISqlFunction 对象
    //    /// </summary>
    //    private readonly ISqlFunction _sqlFunc = null;
    //    /// <summary>
    //    /// SqlConnection 命令
    //    /// </summary>
    //    private SqlConnection _conn = null;
    //    /// <summary>
    //    /// SqlCommand 命令
    //    /// </summary>
    //    private SqlCommand _cmd = null;

    //    /// <summary>
    //    /// 构造方法
    //    /// </summary>
    //    /// <param name="host">ip</param>
    //    /// <param name="dbName">表名</param>
    //    /// <param name="user">用户</param>
    //    /// <param name="pwd">密码</param>
    //    internal ImpSQLServer(string host, string dbName, string user, string pwd)
    //    {
    //        //ConInfo = new ConnectionInfo()
    //        //{
    //        //    dbType = Database.Constants.DBType.Oracle,
    //        //    Server = host,
    //        //    DataBase = dbName,
    //        //    User = user,
    //        //    Pwd = pwd,
    //        //};
    //        //_sqlFunc = SqlFunctionFactory.CreateSqlFunction(DBType.SQLServer);
    //        string ADOConnectionString = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}", host, dbName, user, pwd);

    //        _conn = new SqlConnection(ADOConnectionString);
    //        _cmd = new SqlCommand() { Connection = _conn };
    //    }

    //    #region IDataBase implement
    //    /// <summary>
    //    /// 返回下一个ObjectID，为当前搜索中最大值的下一个
    //    /// </summary>
    //    /// <param name="tableName">表名</param>
    //    /// <returns>ID</returns> 
    //    /// <exception cref="Exception">表不存在</exception>
    //    public int GetNextObjectID(String tableName)
    //    {
    //        lock (_conn)
    //        {
    //            bool fClose = _conn.State != ConnectionState.Open;
    //            int oid = 0;
    //            try
    //            {
    //                if (fClose)
    //                    _conn.Open();
    //                //string sql = "SELECT registration_id FROM sde_table_registry  WHERE owner = 'SDE' AND table_name = '"+tableName.ToUpper()+"'";
    //                string sql = "SELECT registration_id FROM sde_table_registry  WHERE owner = CURRENT_USER AND table_name = '" + tableName.ToUpper() + "'";
    //                DataSet ds =QueryDataSet(sql, null);
    //                Object val = ds.Tables[0].Rows[0][0];
    //                if (val == DBNull.Value)
    //                    throw new Exception("表" + tableName + "不存在！");
    //                string spName = "i" + val + "_get_ids";
    //                SqlParameter[] Params = new SqlParameter[]
    //            {
    //                new SqlParameter("@id_type", 2),
    //                new SqlParameter("@num_requested_ids", 1),
    //                new SqlParameter(){ParameterName="@base_id",Value= -1,SqlDbType=SqlDbType.Int,Direction=ParameterDirection.Output},
    //                new SqlParameter(){ParameterName="@num_obtained_ids",Value= -1,SqlDbType=SqlDbType.Int,Direction=ParameterDirection.Output},
    //            };
    //                PrepareCommand(spName, Params, CommandType.StoredProcedure);
    //                _cmd.ExecuteNonQuery();
    //                val = _cmd.Parameters["@base_id"].Value.ToString();
    //                oid = Convert.ToInt32(val);
    //            }
    //            catch
    //            {
    //                throw;
    //            }
    //            finally
    //            {
    //                if (fClose)
    //                    _conn.Close();
    //            }
    //            return oid;
    //        }
    //        /*
    //        //临时方案，有并发问题，待修改
    //        DataSet ds = ExecuteDataSet("SELECT MAX(objectid) FROM " + tableName, null);
    //        if (ds.Tables[0].Rows.Count == 0)
    //            return 1;
    //        Object val = ds.Tables[0].Rows[0][0];
    //        if (val == DBNull.Value)
    //            return 1;
    //        int oid = Convert.ToInt32(val) + 1;
    //        return oid;
    //        */
    //    }

    //    /// <summary>
    //    /// 执行查询
    //    /// </summary>
    //    /// <param name="sql">sql语句</param>
    //    /// <param name="Params">参数</param>
    //    /// <returns>number</returns>
    //    public int ExecuteSql(string sql, KeyValuePair<String, Object>[] Params)
    //    {
    //        int result = 0;
    //        bool fClose = _conn.State != ConnectionState.Open;
    //        try
    //        {
    //            if (fClose)
    //                _conn.Open();
    //            PrepareCommand(sql, ConvertParams(Params));
    //            result = _cmd.ExecuteNonQuery();
    //        }
    //        finally
    //        {
    //            // Detach the SqlParameters from the command object, so they can be used again
    //            //_cmd.Parameters.Clear();
    //            if (fClose)
    //            {
    //                _conn.Close();
    //            }
    //        }

    //        return result;
    //    }
    //    /// <summary>
    //    /// 返回DataSet
    //    /// </summary>
    //    /// <param name="sql">sql语句</param>
    //    /// <param name="Params">参数</param>
    //    /// <returns>DataSet</returns>
    //    public DataSet QueryDataSet(string sql, KeyValuePair<String, Object>[] Params)
    //    {
    //        lock (_conn)
    //        {
    //            bool fClose = _conn.State != ConnectionState.Open;
    //            DataSet ds = new DataSet();
    //            try
    //            {
    //                if (fClose)
    //                    _conn.Open();
    //                PrepareCommand(sql, ConvertParams(Params));
    //                new SqlDataAdapter(_cmd).Fill(ds);
    //            }
    //            finally
    //            {
    //                // Detach the SqlParameters from the command object, so they can be used again
    //                //_cmd.Parameters.Clear();
    //                if (fClose)
    //                    _conn.Close();
    //            }
    //            return ds;
    //        }
    //    }

    //    public DataTable QueryDataTable(string sql, KeyValuePair<string, object>[] Params = null)
    //    {
    //        var ds = QueryDataSet(sql, Params);
    //        if (ds != null && ds.Tables.Count > 0)
    //            return ds.Tables[0];
    //        return null;
    //    }

    //    public System.Data.IDataReader QueryReader(String sql)
    //    {
    //        return null;
    //    }

    //    #region 事务
    //    /// <summary>
    //    /// 开始事务，开始连接
    //    /// </summary>
    //    public void BeginTransaction()
    //    {
    //        System.Diagnostics.Debug.Assert(_conn.State != ConnectionState.Open);
    //        _conn.Open();
    //        _cmd.Transaction = _conn.BeginTransaction();
    //    }
    //    /// <summary>
    //    /// 提交事务，关闭连接
    //    /// </summary>
    //    public void CommitTransaction()
    //    {
    //        System.Diagnostics.Debug.Assert(_conn.State == ConnectionState.Open);
    //        _cmd.Transaction.Commit();
    //        _cmd.Transaction = null;
    //        _conn.Close();
    //    }
    //    /// <summary>
    //    /// 回滚事务，关闭连接
    //    /// </summary>
    //    public void RollbackTransaction()
    //    {
    //        System.Diagnostics.Debug.Assert(_conn.State == ConnectionState.Open);
    //        _cmd.Transaction.Rollback();
    //        _cmd.Transaction = null;
    //        _conn.Close();
    //    }
    //    /// <summary>
    //    /// 查询是否有事务
    //    /// </summary>
    //    /// <returns>布尔值</returns>
    //    public bool IsTransactionBegining()
    //    {
    //        return _cmd.Transaction != null;
    //    }
    //    #endregion
    //    /// <summary>
    //    /// 返回参数前缀
    //    /// </summary>
    //    /// <returns>'@'</returns>
    //    public char GetParamPrefix()
    //    {
    //        return '@';
    //    }
    //    /// <summary>
    //    /// ISqlFunction 接口
    //    /// </summary>
    //    public ISqlFunction SqlFunc { get { return _sqlFunc; } }
    //    //public ConnectionInfo ConInfo { get; private set; }

    //    public void Dispose()
    //    {

    //    }
    //    #endregion IDataBase implement

    //    #region 私有函数
    //    /// <summary>
    //    /// 重新设置_cmd
    //    /// </summary>
    //    /// <param name="sql">sql语句</param>
    //    /// <param name="Params">参数</param>
    //    private void PrepareCommand(string sql, SqlParameter[] Params, /*KeyValuePair<String, Object>[] Params,*/CommandType cmdType = CommandType.Text)
    //    {
    //        //Set up the command
    //        _cmd.Parameters.Clear();
    //        //_cmd.Transaction = _Transaction;
    //        //_cmd.Connection = _conn;
    //        _cmd.CommandText = sql;
    //        _cmd.CommandType = cmdType;// CommandType.Text;

    //        // Bind the parameters passed in
    //        if (Params != null)
    //        {
    //            foreach (SqlParameter sp in Params)//KeyValuePair<String, Object> parm in Params)
    //            {
    //                //SqlParameter sp = new SqlParameter(parm.Key, parm.Value);
    //                _cmd.Parameters.Add(sp);
    //            }
    //        }
    //    }
    //    /// <summary>
    //    /// 转换参数
    //    /// </summary>
    //    /// <param name="Params">值</param>
    //    /// <returns>SqlParameter[]</returns>
    //    private SqlParameter[] ConvertParams(KeyValuePair<String, Object>[] Params)
    //    {
    //        SqlParameter[] sps = null;
    //        if (Params != null)
    //        {
    //            sps = new SqlParameter[Params.Length];
    //            for (int i = 0; i < Params.Length; ++i)
    //            {
    //                sps[i] = new SqlParameter(Params[i].Key, Params[i].Value);
    //            }
    //        }
    //        return sps;
    //    }
    //    #endregion 私有函数




    //}
}
