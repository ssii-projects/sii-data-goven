/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.5
 * 文 件 名：   SqlHelper
 * 创 建 人：   颜学铭
 * 创建时间：   2016/6/30 14:00:25
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.GIS;
using Agro.LibCore.Database;
using GeoAPI.Geometries;
//using System.Data.SQLite;
using System.Linq.Expressions;

using Microsoft.Data.Sqlite;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;

namespace Agro.LibCore
{
    public class SqlUtil
    {
        public static object QueryOne(IWorkspace db, string sql)
        {
            object o = null;
            db.QueryCallback(sql, r =>
            {
                o = r.GetValue(0);
                return false;
            });
            return o;
        }

        public static string QueryOneString(IWorkspace db, string sql)
        {
            var o = QueryOne(db, sql);
            return o?.ToString();
        }
        public static int QueryOneInt(IWorkspace db, string sql)
        {
            var o = QueryOne(db, sql);
            return SafeConvertAux.ToInt32(o);
        }

        public static void ConstructIn<T>(IEnumerable<T> oids, Action<string> callback, Func<T, object> selector = null, int nGroupCount = 50)
        {
            var i = 0;
            string sin = null;
            foreach (var it in oids)
            {
                if (it == null)
                {
                    continue;
                }

                object oid = it;
                if (selector != null)
                {
                    oid = selector(it);
                }
                if (oid == null) continue;
                var v = oid.ToString();
                if (oid is string)
                {
                    v = "'" + oid + "'";
                }
                sin = sin == null ? v : sin + "," + v;
                ++i;
                if (i >= nGroupCount)
                {
                    callback(sin);
                    i = 0;
                    sin = null;
                }
            }
            if (sin != null)
            {
                callback(sin);
            }
        }

		public static void Split<T>(IEnumerable<T> oids, Action<List<T>> callback, int nGroupCount = 50)
		{
			int i = 0;
			var lst = new List<T>();
			foreach (var oid in oids)
			{
				lst.Add(oid);
				++i;
				if (i >= nGroupCount)
				{
					callback(lst);
					i = 0;
					lst.Clear();
				}
			}
			if (lst.Count > 0)
			{
				callback(lst);
			}
		}

		public static string MakeFieldsString(IFields fields, Func<IField, bool> filter)
        {
            string str = null;
            for (int i = 0; i < fields.FieldCount; ++i)
            {
                var fd = fields.GetField(i);
                if (filter != null)
                {
                    bool fUse = filter(fd);
                    if (!fUse)
                    {
                        continue;
                    }
                }
                if (str == null)
                {
                    str = fd.FieldName;
                }
                else
                {
                    str += "," + fd.FieldName;
                }
            }
            return str;
        }
        /// <summary>
        /// return like "1,2,3" or null
        /// </summary>
        /// <param name="oids"></param>
        /// <returns></returns>
        public static string ConstructIn(IObjectIDSet oids)
        {
            string sin = null;
            if (oids != null)
            {
                oids.Get(oid =>
                {
                    if (sin == null)
                    {
                        sin = oid.ToString();
                    }
                    else
                    {
                        sin += "," + oid;
                    }
                    return true;
                });
            }
            return sin;
        }


        #region for SqlServer
        public static string SafeGetString(IDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return "";
            return r.GetString(c);
        }

        public static int? SafeGetInt(IDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;

            return r.GetInt32(c);
        }

        public static bool SafeGetBool(IDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return false;
            return r.GetBoolean(c);
        }
        public static double? SafeGetDouble(IDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;
            return r.GetDouble(c);
        }
        public static decimal? SafeGetDecimal(IDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;

            return r.GetDecimal(c);
        }
        public static string GetDecimalString(IDataReader r, int c)
        {
            var d = SafeGetDecimal(r, c);
            if (d == null)
                return "";
            return d.ToString();
        }
        public static short? SafeGetShort(IDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;

            return r.GetInt16(c);
        }
        #endregion

        #region for SQLite
        public static string SafeGetString(SQLiteDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return "";
            return r.GetString(c);
        }
        public static bool SafeGetBool(SQLiteDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return false;
            return r.GetBoolean(c);
        }
        public static double? SafeGetDouble(SQLiteDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;
            return r.GetDouble(c);
        }
        public static decimal? SafeGetDecimal(SQLiteDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;

            return r.GetDecimal(c);
        }
        public static string GetDecimalString(SQLiteDataReader r, int c)
        {
            var d = SafeGetDecimal(r, c);
            if (d == null)
                return "";
            return d.ToString();
        }
        public static short? SafeGetShort(SQLiteDataReader r, int c)
        {
            if (r.IsDBNull(c))
                return null;

            return r.GetInt16(c);
        }
        #endregion

        public static IQueryFilter MakeQueryFilter(string subFields,string where=null)
		{
			var qf = new QueryFilter()
			{
				SubFields = subFields,
				WhereClause = where
			};
			return qf;
		}

		public static ISpatialFilter MakeSpatialFilter<T>(eSpatialRelEnum spatialRel,Expression<Action<ListCreator, T>> fields=null, Expression<Func<T, bool>> wh=null,IGeometry geometry=null) where T : class, new()
		{
			var qf = new SpatialFilter()
			{
				WhereClause = WhereExpression<T>.Where(wh),
				SpatialRel = spatialRel,
				Geometry= geometry,
			};
			if (fields != null)
			{
				qf.SubFields = FieldsExpression<T>.Fields(fields).ToStr(",");
			}
			return qf;
		}
    }
}
