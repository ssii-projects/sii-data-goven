using Captain.NetAux.Database;
using DotNetSharp;
using DotNetSharp.Data;
using DotNetSharp.Data.SQLite;
using System;
using DotNetSharp.Data.SqlServer;
using DotNetSharp.Data.Oracle;

namespace Captain.Library.Common
{
    /// <summary>
    /// 数据源
    /// </summary>
    public class DataBaseSource
    {
        /// <summary>
        /// 连接数据库失败
        /// </summary>
        public const string ConnectionError = "连接数据库失败,请检查数据库连接路径是否有效!";

        /// <summary>
        /// 获得当前数据源
        /// </summary>
        public static IDbContext GetDataBaseSource()
        {
            IDbContext dsNew = null;
            try
            {
                dsNew = DataSource.Create<IDbContext>(TheBns.Current.GetDataSourceName());
            }
            catch (Exception ex)
            {
                Captain.Library.Log.Log.WriteException("", "创建数据库实例失败!", ex.Message + ex.StackTrace);
                dsNew = null;
            }
            return dsNew;
        }
        public static IDatabase GetDatabase()
        {
            try
            {
                var sourceName = TheBns.Current.GetDataSourceName();
                using (var ds = DataSource.Create<IDbContext>(sourceName))
                {
                    if(ds.DataSource is IProviderDbCSQLServer)
                    {
                        var db=DatabaseFactory.CreateSqlServerDatabase(ds.ConnectionString);
                        return db;
                    }
                    if (ds.DataSource is IProviderDbCOracle)
                    {
                        var db = DatabaseFactory.CreateOracleDatabase(ds.ConnectionString);
                        return db;
                    }
                }
            }
            catch (Exception ex)
            {
                Captain.Library.Log.Log.WriteException("", "创建数据库实例失败!", ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
        /// <summary>
        /// 获得当前数据源
        /// </summary>
        public static IDbContext GetDataBaseSource(string dataSourceName)
        {
            IDbContext dsNew = null;
            try
            {
                dsNew = DataSource.Create<IDbContext>(dataSourceName);
            }
            catch (Exception ex)
            {
                Captain.Library.Log.Log.WriteException("", "创建数据库实例失败!", ex.Message + ex.StackTrace);
                dsNew = null;
            }
            return dsNew;
        }

        /// <summary>
        /// 获得当前数据源
        /// </summary>
        public static IDbContext GetDataBaseSourceByPath(string fileName)
        {
            IDbContext dsNew = null;
            try
            {
                dsNew = ProviderDbCSQLite.CreateDataSourceByFileName(fileName) as IDbContext;
            }
            catch (Exception ex)
            {
                Captain.Library.Log.Log.WriteException("", "创建数据库实例失败!", ex.Message + ex.StackTrace);
                dsNew = null;
                throw ex;
            }
            return dsNew;
        }
    }
}
