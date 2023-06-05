using Agro.GIS;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore.Database
{
 	 class OrmObject
	{
		private readonly IWorkspace _db;
		internal OrmObject(IWorkspace db)
		{
			_db = db;
		}

		//public List<string> Properties<T>(Expression<Action<ListCreator, T>> expression)
		//{
		//	return FieldsExpression<T>.Fields(expression);
		//}

		public int Insert(IEntity entity, SubFields subFields = null)
		{
			var se = this.GetInsertSqlByEntity(entity, subFields);
			return _db.ExecuteNonQuery(se.Sql, se.Params);
		}
		public int Update<T>(T entiry, Expression<Func<T, bool>> wh, SubFields subFields = null) where T : class, new()
		{
			var sWh = this.CreateWhere<T>(wh);
			var se = this.GetUpdateSqlByEntity(entiry, subFields, sWh);
			return _db.ExecuteNonQuery(se.Sql, se.Params);
		}
		public int Delete<T>(Expression<Func<T, bool>> whereExpression)where T:class,new()
		{
			var wh = WhereExpression<T>.Where(whereExpression);
			var sql = "DELETE FROM " + Entity<T>.GetTableName();
			if (!string.IsNullOrEmpty(wh))
				sql += " WHERE " + wh;
			return _db.ExecuteNonQuery(sql);
		}

		//public DateTime GetServerCurrentTime()
		//{
		//    throw new NotImplementedException();
		//}

		public void FindCallback<T>(string wh, Action<CancelItem<T>> callback, SubFields subFields = null, bool fRecycle = true, ICancelTracker cancel = null) where T : class, new()
		{
			var fields = subFields?.Value;
			var entity = new T();
			var tableName = Entity<T>.GetTableName();
			string strFields = null;
			if (fields == null)
			{
				fields = Entity<T>.GetAttributes().Select(t => t.FieldName);
			}
			foreach (var fieldName in fields)
			{
				if (strFields == null)
				{
					strFields = fieldName;
				}
				else
				{
					strFields += "," + fieldName;
				}
			}
			var sql = $"SELECT { strFields} FROM  {tableName}";
			if (!string.IsNullOrEmpty(wh))
			{
				sql += " WHERE  " + wh;
			}
			var it = new CancelItem<T>();
			_db.QueryCallback(sql, r =>
			{
				if (!fRecycle)
				{
					entity = new T();
				}
				FillEntity(r, entity);
				it.Item = entity;
				callback(it);
				return !it.Cancel;
				//return callback(entity);
			}, cancel);
		}

		public void FindCallback<T>(Expression<Func<T, bool>> whExpress, Action<CancelItem<T>> callback, SubFields subFields = null, bool fRecycle = true,ICancelTracker cancel=null) where T : class, new()
		{
			var wh = CreateWhere(whExpress);
			FindCallback<T>(wh, callback, subFields, fRecycle, cancel);
		}
        public void FindCallback<T>(Expression<Func<T, bool>> wh, Action<CancelItem<T>> callback, Expression<Action<ListCreator, T>> fields, bool fRecycle = true, ICancelTracker cancel = null) where T : class, new()
        {
            SubFields subFields = null;
            if (fields != null)
            {
                subFields=SubFields.Make(fields);
            }

            FindCallback(wh, callback, subFields, fRecycle, cancel);
        }

        //public List<T> Find<T>(Expression<Func<T, bool>> wh, SubFields subFields = null) where T : class, new()
        //{
        //	var lst = new List<T>();
        //	FindCallback<T>(wh, ren => { lst.Add(ren); return true; }, subFields, false);
        //	return lst;
        //}

        //public T Find<T>(Expression<Func<T, bool>> wh, SubFields subFields = null) where T : class, new()
        //{
        //	T entity = null;
        //	FindCallback<T>(wh, en =>
        //	{
        //		entity = en;
        //		return false;
        //	}, subFields);
        //	return entity;
        //}

        //public T FindFirst<T>(string wh, SubFields subFields = null) where T : class, new()
        //{
        //	T entity = null;
        //	FindCallback<T>(wh, en =>
        //	{
        //		entity = en;
        //		return false;
        //	}, subFields);
        //	return entity;
        //}

        public string CreateWhere<T>(Expression<Func<T, bool>> expression) where T : class, new()
		{
			return WhereExpression<T>.Where(expression);
		}

		/// <summary>
		///  datatable填充List集合
		/// </summary>
		/// <typeparam name="T">类型</typeparam>
		/// <param name="dt">数据库名称</param>
		/// <returns>实体集合</returns>
		private void FillEntity<T>(IDataReader row, T en) where T :class, new()
		{
			var rawObj = en;

			var fIs = rawObj.GetType().GetProperties();
			foreach (var fi in fIs)
			{
				bool fOK = false;
				object val = null;
				for (int i = 0; i < row.FieldCount; ++i)
				{
					var fieldName = row.GetName(i);
					if (StringUtil.isEqualIgnorCase(fieldName, Entity<T>.GetFieldName(fi.Name)))
					{
						val = row.GetValue(i);
						fOK = true;
						break;
					}
				}
				if (!fOK)
					continue;
				try
				{
					if (val is DBNull || val == null)
					{
						fi.SetValue(rawObj, null, null);
					}
					else if ((fi.PropertyType == typeof(decimal) || fi.PropertyType == typeof(decimal?)) && !(val is decimal))
					{
						fi.SetValue(rawObj, decimal.Parse(val.ToString()), null);
					}
					else if ((fi.PropertyType == typeof(double) || fi.PropertyType == typeof(double?)) && !(val is double))
					{
						fi.SetValue(rawObj, Convert.ToDouble(val), null);
					}
					else if ((fi.PropertyType == typeof(float) || fi.PropertyType == typeof(float?)) && !(val is float))
					{
						fi.SetValue(rawObj, Convert.ToSingle(val), null);
					}
					else if ((fi.PropertyType == typeof(int) || fi.PropertyType == typeof(int?)) && !(val is int))
					{
						fi.SetValue(rawObj, Convert.ToInt32(val), null);
					}
					else if ((fi.PropertyType == typeof(short) || fi.PropertyType == typeof(short?)) && !(val is short))
					{
						fi.SetValue(rawObj, Convert.ToInt16(val), null);
					}
					else if ((fi.PropertyType == typeof(bool) || fi.PropertyType == typeof(bool?)) && !(val is bool))
					{
						var fVal = false;
						var sVal = val.ToString();
						if (int.TryParse(sVal, out int n))
						{
							fVal = n != 0;
						}
						else if(bool.TryParse(sVal,out bool b))
						{
							fVal = b;
						}
						fi.SetValue(rawObj, fVal, null);
					}
					else if ((fi.PropertyType == typeof(DateTime) || fi.PropertyType == typeof(DateTime?)) && !(val is DateTime))
					{
						var dt = DateTime.Parse(Convert.ToString(val));
						fi.SetValue(rawObj, dt, null);
					}
					else if ((fi.PropertyType == typeof(Guid) || fi.PropertyType == typeof(Guid?)) && !(val is Guid))
					{
						fi.SetValue(rawObj, Guid.Parse(Convert.ToString(val)), null);
					}
					else
					{
						if (IsEnum(fi.PropertyType))
						{
							fi.SetValue(rawObj, Enum.ToObject(fi.PropertyType, val), null);
						}
						else
						{
							fi.SetValue(rawObj, val, null);
						}
					}
				}
				catch
				{
					throw;
				}
			}
			//rawObj.modifyFlag = EModifyFlag.MF_UNCHANGE;
		}
        private static bool IsEnum(Type t)
        {
            while (t!=null)
            {
                if (t == typeof(Enum)) return true;
                t = t.BaseType;
            }
            return false;
        }

		/// <summary>
		/// 获取对象保存sql语句
		/// </summary>
		/// <typeparam name="T">类型</typeparam>
		/// <param name="Obj">名称</param>
		/// <param name="subFields">字段</param>
		/// <returns>SqlEntity对象</returns>
		internal SqlEntity GetInsertSqlByEntity(object Obj, SubFields subFields)
		{
			var lst = GetDynamicEntityFields(Obj, subFields);
			var se = new SqlEntity();
			se.ConstructInsert(EntityUtil.GetTableName(Obj), lst, _db);
			return se;
		}

		private SqlEntity GetUpdateSqlByEntity(object Obj, SubFields subFields, string where)
		{
			var lst = GetDynamicEntityFields(Obj, subFields);
			var se = new SqlEntity();
			se.ConstructUpdate(EntityUtil.GetTableName(Obj), lst, _db.SqlFunc, where,_db);
			return se;
		}
		private List<DynamicEntityField> GetDynamicEntityFields(object Obj, SubFields subFields)
		{
			var lst = new List<DynamicEntityField>();

			var fields = subFields?.Value;

			foreach (var pi in Obj.GetType().GetProperties())
			{
				var val = pi.GetValue(Obj, null);
				var fieldName = EntityUtil.GetFieldName(Obj, pi.Name);

				if (fields!=null&& !fields.Contains(fieldName, StringComparer.CurrentCultureIgnoreCase))
				{
					continue;
					//var fConatin = false;
					//foreach (var propertyName in fields)
					//{
					//	if (fieldName.Equals(EntityUtil.GetFieldName(Obj, propertyName), StringComparison.CurrentCultureIgnoreCase))
					//	{
					//		fConatin = true;
					//		break;
					//	}
					//}
					//if (!fConatin)
					//	continue;
				}

				var ep = EntityUtil.GetAttribute(Obj, pi.Name);
                if (!ep.AutoIncrement)
                {
                    lst.Add(new DynamicEntityField(ep, val));
                }
			}
			return lst;
		}
	}

	/// <summary>
	///类:  Lucky.Orm.SqlEntity
	///描述:Orm的一个内部类，实现了插入sql语句和更新sql语句的拼写方法
	///版本:1.0
	///日期:2012/5/4
	///作者:颜学铭
	/// </summary>
	public class SqlEntity
	{
		/// <summary>
		/// ObjectID号
		/// </summary>
		public int ObjectID;
		/// <summary>
		/// Sql语句
		/// </summary>
		public string Sql
		{
			get;
			set;
		}
		/// <summary>
		/// 参数值
		/// </summary>
		public SQLParam[] Params
		{
			get;
			set;
		}
		/// <summary>
		/// 构造插入Sql语句
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="oid">ID</param>
		/// <param name="fields">字段名</param>
		/// <param name="SqlFunc">SqlFunc接口</param>
		/// <param name="strParamPrefix">前缀</param>
		internal void ConstructInsert(string tableName, IEnumerable<DynamicEntityField> fields,IWorkspace db)// ISqlFunction SqlFunc)//, string oidFieldName, bool fOidAutoBuild)
		{
			char strParamPrefix = db.SqlFunc.GetParamPrefix();
			var strFields = "";
			var strValues = "";
			var al = new List<SQLParam>();
			foreach (var field in fields)
			{
				var val = field.FieldValue;
				if (!field.FieldDefinition.Insertable || val == null || val is DBNull)
					continue;

				var fieldName = field.FieldName;

				if (strFields.Length > 0)
				{
					strFields += ",";
					strValues += ",";
				}

				strFields += fieldName;

				if (val is DateTime dt)
				{
					strValues += db.SqlFunc.ToDate(dt);
				}
				else if (val is Guid guid)
				{
					strValues += $"'{guid}'";
				}
				else if (val is Enum)
				{
					strValues += (int)val;
				}
				else if (val is int || val is short)
				{
					strValues += val.ToString();
				}
				else
				{
					if (val is IGeometry geo)
					{
                        if (db.DatabaseType == eDatabaseType.SqlServer)
                        {
                            strValues += db.SqlFunc.GeomFromText(geo);
                            continue;
                        }
                        /*
						var sr = geo.GetSpatialReference();
						System.Diagnostics.Debug.Assert(sr != null);
						if (db.DatabaseType == eDatabaseType.SqlServer)
						{
							var srid = sr.AuthorityCode;
							strValues += $"geometry::STGeomFromText('{geo.AsText()}',{srid}).MakeValid()";
							continue;
						}*/
                    }
					strValues += strParamPrefix + fieldName;
					al.Add(new SQLParam()
					{
						ParamName = fieldName,
						ParamValue = val
					});
				}
			}

			System.Diagnostics.Debug.Assert(strValues != "");
			this.Sql = $"INSERT INTO {tableName} ({strFields}) VALUES ({strValues})";
			Params = al.ToArray();
		}
		/// <summary>
		///  构造更新Sql语句
		/// </summary>
		/// <param name="tableName">表名</param>
		/// <param name="oid">ID</param>
		/// <param name="fields">字段名</param>
		/// <param name="SqlFunc">SqlFunc接口</param>
		/// <param name="strParamPrefix">前缀</param>
		internal void ConstructUpdate(string tableName, IEnumerable<DynamicEntityField> fields, ISqlFunction SqlFunc, string whereClause,IWorkspace db)
		{
			char strParamPrefix = SqlFunc.GetParamPrefix();
			string updateClause = null;
			var al = new List<SQLParam>();
			foreach (var field in fields)
			{
				var val = field.FieldValue;
				var fieldName = field.FieldName;
				if (field.FieldDefinition.Updatable)//!(val == null||val is DBNull))
				{
					string str = null;
                    if (val == null || val is DBNull)
                    {
                        str = $"{fieldName}=null";
                    }
                    else if (val is DateTime dt)
                    {
                        str = fieldName + "=" + SqlFunc.ToDate(dt);
                    }
                    else if (val is int || val is short)
                    {
                        str = fieldName + "=" + val;
                    }
                    else if (val is Enum)
                    {
                        str = fieldName + "=" + (int)val;
                    }
                    else
                    {

                        if (val is IGeometry geo)
                        {
                            if (db.DatabaseType == eDatabaseType.SqlServer)
                            {
                                str = fieldName + "=" + db.SqlFunc.GeomFromText(geo);                               
                            }
                        }
                        if (str == null)
                        {
                            str = fieldName + "=" + strParamPrefix + fieldName;
                            al.Add(new SQLParam()
                            {
                                ParamName = fieldName,
                                ParamValue = val
                            });
                        }
                    }
					if (updateClause == null)
                    {
                        updateClause = str;
                    }
                    else
					{
						updateClause += "," + str;
					}
				}
			}
			System.Diagnostics.Debug.Assert(updateClause != null);
			this.Sql = $"UPDATE { tableName } SET { updateClause } WHERE { whereClause}";
			this.Params = al.ToArray();
		}

	}

    public class SubFields
    {
        private SubFields(IEnumerable<string> v)
        {
            Value = v;
        }
        public IEnumerable<string> Value
        {
            get; private set;
        }
        public static SubFields Make<T>(Expression<Action<ListCreator, T>> properties) where T : class,new()
        {
            return new SubFields(FieldsExpression<T>.Fields(properties));
        }
        public static SubFields Make(IEnumerable<string> fields)
        {
            return new SubFields(fields);
        }
    }
}
