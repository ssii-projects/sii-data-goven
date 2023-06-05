/// <summary>
///项目:  Lucky.AbstractEntity
///描述:  定义所有实体的基类 
///版本:1.0
///日期:2012/5/4
///作者:颜学铭
///更新:
/// </summary>
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Agro.GIS;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
namespace Agro.LibCore.Database
{

  /**
 * 说明：主要用于为含有DataTable和DataColumn属性的类提供辅助方法
 * T 类型示例：
[DataTable("SEC_ID_USER", AliasName = "用户")]
public class User : Entity<User>
{
  [DataColumn("Username", AliasName = "用户名")]
  public string Name { get; set; }
  public string Password { get; set; }
}
*/
  public interface IEntity : ICloneable
  {
  }
  public class Entity<T> : IEntity where T : class, new()
  {
    public static string GetTableName()
    {
      return EntityUtil.GetTableName<T>();
    }
    public static string GetTableAliasName()
    {
      return EntityUtil.GetTableAliasName<T>();
    }

    public static string MakeWhere(Expression<Func<T, bool>> predicate)
    {
      return WhereExpression<T>.Where(predicate);
    }
    public static string GetFieldsString(Predicate<EntityProperty> predicate = null)
    {
      return EntityUtil.GetFieldsString<T>(predicate);
    }
    public static bool IsTableExists(IWorkspace db)
    {
      return db.IsTableExists(GetTableName());
    }

    public static void CreateTable(IWorkspace db, Action<IFields> hook = null, TableIndex[] index = null)
    {
      EntityUtil.CreateTable<T>(db, hook, index);
    }

    /// <summary>
    /// yxm 2021-3-26 修复表：数据库中表与实体定义进行对比，添加缺少的字段，当前仅支持文本类型
    /// </summary>
    /// <param name="db"></param>
    public static void RepairTable(IWorkspace db)
    {
      if (!IsTableExists(db))
      {
        return;
      }
      var tableName = GetTableName();
      var lstFields = db.QueryFields2(tableName);
      var attributes = GetAttributes();
      foreach (var p in attributes)
      {
        if (lstFields.Find(it => StringUtil.isEqualIgnorCase(it.FieldName, p.FieldName)) == null)
        {
          var field = EntityUtil.ToField(p);
          db.AddField(tableName, field);
        }
      }

    }


    public static void TryCreateTable(IWorkspace db, Action<IFields> hook = null, TableIndex[] index = null)
    {
      if (!db.IsTableExists(GetTableName()))
      {
        CreateTable(db, hook, index);
      }
    }
    public static IFeatureClass CreateFeatureClass(IFeatureWorkspace db, int srid, Action<IFields> hook = null, TableIndex[] index = null)
    {
      return EntityUtil.CreateFeatureClass<T>(db, srid, hook, index);
    }
    public static void TryCreateFeatureClass(IFeatureWorkspace db, int srid, Action<IFields> hook = null, TableIndex[] index = null)
    {
      if (!db.IsTableExists(GetTableName()))
      {
        using (CreateFeatureClass(db, srid, hook, index))
        {
        }
      }
    }
    public static IFields ConvertToFields(Predicate<EntityProperty> predicate = null)
    {
      return EntityUtil.ConvertToFields(typeof(T), predicate);
    }
    public static string GetFieldName(string propertyName)
    {
      return EntityUtil.GetFieldName<T>(propertyName);
    }
    public static string GetAliasName(string propertyName)
    {
      return EntityUtil.GetAliasName<T>(propertyName);
    }
    public static DataTable DataTable(Expression<Action<ListCreator, T>> subFields)
    {
      var dt = new DataTable(GetTableName());

      var properties = GetAttributes();
      var lst = FieldsExpression<T>.Fields(subFields);
      foreach (var fieldName in lst)
      {
        var p = properties.Find(it => StringUtil.isEqualIgnorCase(it.FieldName, fieldName));

        Type columnType = p.PropertyType;

        // We need to check whether the property is NULLABLE
        if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
          // If it is NULLABLE, then get the underlying type. eg if "Nullable<int>" then this will return just "int"
          columnType = p.PropertyType.GetGenericArguments()[0];
        }

        dt.Columns.Add(fieldName, columnType);
      }
      return dt;
    }
    public static DataTableAttribute GetTableAttribute()
    {
      return EntityUtil.GetTableAttribute(typeof(T));
    }

    public static EntityProperty GetAttribute(string propertyName)
    {
      return EntityUtil.GetAttribute<T>(propertyName);
    }

    public static List<EntityProperty> GetAttributes(bool fContainBinaryField = true, List<EntityProperty> lst = null)
    {
      return EntityUtil.GetAttributes(typeof(T), fContainBinaryField, lst);
    }
    //public List<EntityProperty> GetAttributes(bool fContainBinaryField = true, List<EntityProperty> lst = null)
    //{
    //    return EntityUtil.GetAttributes(typeof(T), fContainBinaryField, lst);
    //}

    public static SubFields GetSubFields(Expression<Action<ListCreator, T>> expression)
    {
      return SubFields.Make(expression);
    }
    public static SubFields GetSubFields(bool fContainBinaryField = true)
    {
      return SubFields.Make(GetAttributes(fContainBinaryField).Select(t => t.FieldName));
    }
    public static SubFields GetSubFields(Predicate<EntityProperty> predicate)
    {
      return SubFields.Make(GetAttributes().Where(it => predicate(it)).Select(t => t.FieldName));
    }

    ///// <summary>
    ///// 用于方便的构造IOrmObject.LoadEntities方法中的subFields参数
    ///// </summary>
    ///// <param name="fContainBinaryField">是否包含二进制字段</param>
    ///// <returns>字段集合</returns>
    //public static IEnumerable<string> GetProperties(bool fContainBinaryField = true)
    //{
    //	return EntityUtil.GetProperties(typeof(T), fContainBinaryField).Select(t=>t.PropertyName);
    //}
    //public static List<string> GetFieldsByProperties(IEnumerable<string> properties)
    //{
    //	return EntityUtil.GetFieldsByProperties(typeof(T), properties);
    //}
    /// <summary>
    /// 清空属性
    /// </summary>
    public static void Clear(T entity)
    {
      var type = entity.GetType();
      foreach (System.Reflection.PropertyInfo fi in type.GetProperties())
      {
        if (fi.PropertyType == typeof(double) || fi.PropertyType == typeof(float)
          || fi.PropertyType == typeof(short) || fi.PropertyType == typeof(int)
          )
        {
          fi.SetValue(entity, (short)0, null);
        }
        else
        {
          fi.SetValue(entity, null, null);
        }
      }

    }

    /// <summary>
    /// 比较两个实体的差异（返回有差异的字段的字段名和oldEntity的字段值）
    /// 返回[小写字段名，字段值]映射
    /// 不对二进制字段进行比较
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="oldEntity"></param>
    /// <param name="newEntity"></param>
    /// <returns>[小写字段名，字段值]</returns>
    public Dictionary<string, object> CheckDifference<U>(T oldEntity, U newEntity)
    {
      var dic = new Dictionary<string, object>();
      bool fContainBinaryField = false;
      var type = oldEntity.GetType();
      foreach (var field in type.GetProperties())
      {
        var fieldName = GetFieldName(field.Name);
        if (!fContainBinaryField
          && (field.PropertyType == typeof(byte[])
              || field.PropertyType == typeof(Geometry)))
          continue;
        object oldVal = oldEntity.GetPropertyValue(fieldName);
        object newVal = newEntity.GetPropertyValue(fieldName);
        if (SafeConvertAux.ToStr(oldVal) != SafeConvertAux.ToStr(newVal))
          dic[fieldName] = oldVal;
      }
      return dic;
    }
    /// <summary>
    /// 从同种类型的实体对象拷贝数据；
    /// 比Copy效率更高
    /// </summary>
    /// <param name="rhs"></param>
    /// <param name="exclude">需要排除的属性</param>
    public void OverWrite(T rhs, Expression<Action<ListCreator, T>> exclude = null)
    {
      if (rhs == null || rhs == this)
        return;
      var excludeProperties = FieldsExpression<T>.Properties(exclude);
      var props = rhs.GetType().GetProperties();
      foreach (var pi in props)
      {
        if (excludeProperties?.Find(it => it.PropertyName == pi.Name) != null)
        {
          continue;
        }
        var o = pi.GetValue(rhs, null);
        this.SetPropertyValue(pi.Name, o);
      }
    }
    /// <summary>
    /// 从另外一种类型的实体对象拷贝数据；
    /// 拷贝规则：字段名相同并且类型相同；
    /// 说明：如果U和T的类型相同，则不推荐使用该方法（推荐使用T的OverWrite方法）
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="rhs"></param>
    /// <param name="exclude">需要排除的属性</param>
    public void Copy<U>(U rhs, Expression<Action<ListCreator, T>> exclude = null) where U : class
    {
      if (rhs == null || rhs == this)
        return;
      var tType = this.GetType();
      var rType = rhs.GetType();
      if (tType == rType)
      {
        OverWrite(rhs as T, exclude);
        return;
      }
      var rhsProps = rType.GetProperties();

      var excludeProperties = FieldsExpression<T>.Properties(exclude);
      foreach (var pi in rhsProps)
      {
        if (excludeProperties?.Find(it => it.PropertyName == pi.Name) != null)
        {
          continue;
        }
        var fi = tType.GetProperty(pi.Name);
        if (fi?.PropertyType == pi.PropertyType)
        {
          var o = pi.GetValue(rhs, null);
          this.SetPropertyValue(fi.Name, o);
        }
      }
    }

    public object Clone()
    {
      var rhs = new T();
      var props = rhs.GetType().GetProperties();
      foreach (var pi in props)
      {
        var o = pi.GetValue(this, null);
        rhs.SetPropertyValue(pi.Name, o);
      }
      return rhs;
    }
  }

  public interface IFeatureEntity
  {
    int GetObjectID();
    void SetObjectID(int oid);
  }
  public class FeatureEntity<T> : Entity<T>, IFeatureEntity where T : class, new()
  {
    private int _oid;
    public int GetObjectID()
    {
      return _oid;
    }
    public void SetObjectID(int oid)
    {
      _oid = oid;
    }
  }
  /// <summary>
  /// 主要为Entity<T>的实例提供辅助方法
  /// </summary>
  public static class EntityUtil
  {
    public static string GetTableName(object entity)
    {
      return GetTableName(entity.GetType());
    }

    public static string GetFieldName(object entity, string propertyName)
    {
      return GetFieldName(entity.GetType(), propertyName);
    }
    /// <summary>
    /// 返回逗号分隔的字段名集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static string GetFieldsString<T>(Predicate<EntityProperty> predicate = null)
    {
      var lst = GetAttributes<T>();
      if (predicate != null)
      {
        return string.Join(",", lst.Where(it => predicate(it)).Select(it => it.FieldName));
      }
      return string.Join(",", lst.Select(it => it.FieldName));
    }
    public static EntityProperty GetAttribute(object entity, string propertyName)
    {
      return GetAttribute(entity.GetType(), propertyName);
    }

    public static List<EntityProperty> GetAttributes<T>(bool fContainBinaryField = true, List<EntityProperty> lst = null)
    {
      return GetAttributes(typeof(T), fContainBinaryField, lst);
    }

    public static List<EntityProperty> GetAttributes(object entity, bool fContainBinaryField = true, List<EntityProperty> lst = null)
    {
      return GetAttributes(entity.GetType(), fContainBinaryField, lst);
    }

    public static string GetTableName<T>()
    {
      return GetTableName(typeof(T));
    }
    public static string GetTableAliasName<T>()
    {
      return GetTableAliasName(typeof(T));
    }

    public static string GetFieldName<T>(string propertyName)
    {
      return GetFieldName(typeof(T), propertyName);
    }
    public static string GetAliasName<T>(string propertyName)
    {
      return GetAliasName(typeof(T), propertyName);
    }
    public static EntityProperty GetAttribute<T>(string propertyName)
    {
      return GetAttribute(typeof(T), propertyName);
    }
    /// <summary>
    /// 必填项检查
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="en"></param>
    /// <param name="ignore"></param>
    /// <returns></returns>
    public static string CheckErrorForNull<T>(Entity<T> en, Predicate<EntityProperty> ignore = null) where T : class, new()
    {
      var lst = GetAttributes(en);
      foreach (var p in lst)
      {
        if (!p.Nullable)
        {
          if (ignore?.Invoke(p) == true)
          {
            continue;
          }
          var o = en.GetPropertyValue(p.PropertyName);
          if (o == null || p.PropertyType == typeof(string) && string.IsNullOrEmpty(o.ToString()))
          {
            return $"{p.AliasName}不能为空！";
          }
        }
      }
      return null;
    }
    internal static string GetTableName(Type t)
    {
      var tableName = t.Name;
      var da = GetTableAttribute(t);
      if (!string.IsNullOrEmpty(da?.TableName))
      {
        tableName = da.TableName.Trim();
      }
      return tableName;
    }
    internal static string GetTableAliasName(Type t)
    {
      var da = GetTableAttribute(t);
      if (!string.IsNullOrEmpty(da?.AliasName))
      {
        return da.AliasName.Trim();
      }
      return t.Name; ;
    }
    internal static string GetFieldName(Type t, string propertyName)
    {
      return (GetAttribute(t, propertyName)?.FieldName ?? propertyName)?.Trim();
    }
    internal static string GetAliasName(Type t, string propertyName)
    {
      return GetAttribute(t, propertyName)?.AliasName;
    }

    //internal static List<string> GetFieldsByProperties(Type t, IEnumerable<string> properties)
    //{
    //	var fields = new List<string>();
    //	foreach (var propertyName in properties)
    //	{
    //		fields.Add(GetFieldName(t, propertyName));
    //	}
    //	return fields;
    //}

    //internal static List<string> GetProperties(Type t, bool fContainBinaryField = true)
    //{
    //	List<string> lst = new List<string>();
    //	foreach (var field in t.GetProperties())
    //	{
    //		var fieldName = field.Name;// GetFieldName(t,field.Name);
    //		if (!fContainBinaryField
    //			&& (field.PropertyType == typeof(byte[])
    //					|| field.PropertyType == typeof(Geometry)))
    //			continue;
    //		lst.Add(fieldName);
    //	}
    //	return lst;
    //}
    internal static DataTableAttribute GetTableAttribute(Type t)
    {
      return t.GetCustomAttributes(typeof(DataTableAttribute), false).FirstOrDefault() as DataTableAttribute;
    }
    internal static EntityProperty GetAttribute(Type t, string propertyName)
    {
      var fi = t.GetProperty(propertyName);
      if (fi != null)
      {
        var ep = new EntityProperty
        {
          PropertyName = propertyName,
          FieldName = propertyName,
          AliasName = propertyName,
          PropertyType = fi.PropertyType
        };
        if (fi?.GetCustomAttributes(typeof(DataColumnAttribute), false).FirstOrDefault() is DataColumnAttribute ca)
        {
          if (!string.IsNullOrEmpty(ca.Name))
          {
            ep.FieldName = ca.Name.Trim();
          }
          if (!string.IsNullOrEmpty(ca.AliasName))
          {
            ep.AliasName = ca.AliasName.Trim();
          }
          ep.Unique = ca.Unique;
          ep.Nullable = ca.Nullable;
          ep.Insertable = ca.Insertable;
          ep.Updatable = ca.Updatable;
          ep.PrimaryKey = ca.PrimaryKey;
          ep.AutoIncrement = ca.AutoIncrement;
          ep.Length = ca.Length;
          ep.Precision = ca.Precision;
          ep.Scale = ca.Scale;
          ep.GeometryType = ca.GeometryType;
          ep.CodeType = ca.CodeType;
          if (ca.FieldType != eFieldType.eFieldTypeNull)
          {
            ep.FieldType = ca.FieldType;
          }
          ep.DefaultValue = ca.DefaultValue;
          ep.Tag = ca.Tag;
        }
        return ep;
      }
      return null;
    }
    internal static List<EntityProperty> GetAttributes(Type t, bool fContainBinaryField = true, List<EntityProperty> lst = null)
    {
      if (lst == null)
      {
        lst = new List<EntityProperty>();
      }
      else
      {
        lst.Clear();
      }
      var pis = t.GetProperties();
      foreach (var pi in pis)
      {
        var fBinaryField = false;
        var fGeometryField = pi.PropertyType == typeof(Geometry) || pi.PropertyType == typeof(IGeometry);
        if (pi.PropertyType == typeof(byte[]) || fGeometryField)
        {
          fBinaryField = true;
        }
        if (!fContainBinaryField && fBinaryField)
        {
          continue;
        }
        var ca = GetAttribute(t, pi.Name);
        if (fGeometryField)
        {
          ca.FieldType = eFieldType.eFieldTypeGeometry;
        }
        lst.Add(ca);
      }
      return lst;
    }

    public static List<EntityProperty> GetIntersectionAttributes<T, U>(Func<EntityProperty, EntityProperty, bool> match = null)
    {
      var lst = new List<EntityProperty>();

      if (match == null)
      {
        match = (t, u) => t.FieldName == u.FieldName && t.PropertyType == u.PropertyType;
      }

      var ts = GetAttributes<T>();
      var us = GetAttributes<U>();
      foreach (var t in ts)
      {
        var it = us.Find(u => match(t, u));
        if (it != null)
        {
          lst.Add(it);
        }
      }
      return lst;
    }
    public static IField ToField(EntityProperty it)
    {
      return new Field()
      {
        FieldName = it.FieldName,
        AliasName = it.AliasName,
        IsNullable = it.Nullable,
        Editable = it.Insertable || it.Updatable,
        Length = it.IsStringField ? it.Length : it.Precision,
        Scale = it.Scale,
        GeometryType = it.GeometryType,
        FieldType = FieldsUtil.ToFieldType(it),
        PrimaryKey = it.PrimaryKey,
        AutoIncrement = it.AutoIncrement,
        DefaultValue = it.DefaultValue
      };
    }
    public static IFields ConvertToFields(Type t, Predicate<EntityProperty> predicate = null)
    {
      var fields = new Fields();
      var lst = GetAttributes(t);
      foreach (var it in lst)
      {
        if (predicate?.Invoke(it) != false)
        {
          var field = ToField(it);

          fields.AddField(field);
        }
      }
      return fields;
    }

    public static IFeatureClass CreateFeatureClass<T>(IFeatureWorkspace db, int srid, Action<IFields> hook = null, TableIndex[] index = null)
    {
      var type = typeof(T);
      var fields = ConvertToFields(type);
      hook?.Invoke(fields);
      var tableName = GetTableName(type);
      db.CreateFeatureClass(tableName, fields, srid, GetTableAliasName(type), index);
      return db.OpenFeatureClass(tableName, db.DatabaseType == eDatabaseType.ShapeFile ? "rb+" : "rb");
    }
    public static void CreateTable<T>(IWorkspace db, Action<IFields> hook = null, TableIndex[] index = null)
    {
      var type = typeof(T);
      var fields = ConvertToFields(type);
      hook?.Invoke(fields);
      db.CreateTable(GetTableName(type), fields, GetTableAliasName(type), index);
    }

    /// <summary>
    /// 将entity的值按字段名和字段类型相同写入feature中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <param name="feature"></param>
    public static void WriteToFeature<T>(T entity, IRow feature)
    {
      if (feature != null && entity != null)
      {
        var fields = feature.Fields;
        var lst = GetAttributes(entity.GetType());
        foreach (var it in lst)
        {
          int iField = fields.FindField(it.FieldName);
          if (iField >= 0)
          {
            if (FieldsUtil.ToFieldType(it) == fields.GetField(iField).FieldType)
            {
              feature.SetValue(iField, entity.GetPropertyValue(it.PropertyName));
            }
          }
        }
      }
    }
    /// <summary>
    ///  将feature中的值按字段名和字段类型相同写入entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="feature"></param>
    /// <param name="entity"></param>
    public static void WriteToEntity<T>(IFeature feature, T entity)
    {
      if (feature != null && entity != null)
      {
        var lst = GetAttributes(entity.GetType());
        var fields = feature.Fields;
        foreach (var it in lst)
        {
          int iField = fields.FindField(it.FieldName);
          if (iField >= 0)
          {
            if (FieldsUtil.ToFieldType(it) == fields.GetField(iField).FieldType)
            {
              var o = feature.GetValue(iField);
              if (o is IGeometry geo)
              {
                geo.SetSpatialReference(feature.Shape.GetSpatialReference());
              }
              entity.SetPropertyValue(it.PropertyName, o);
            }
          }
        }

        if (entity is IFeatureEntity fe)
        {
          fe.SetObjectID(feature.Oid);
        }
      }
    }

    public static void WriteToEntity(IRow row, IEntity entity)
    {
      if (row != null && entity != null)
      {
        var lst = GetAttributes(entity.GetType());
        var fields = row.Fields;
        foreach (var it in lst)
        {
          int iField = fields.FindField(it.FieldName);
          if (iField >= 0)
          {
            if (FieldsUtil.ToFieldType(it) == fields.GetField(iField).FieldType)
            {
              var o = row.GetValue(iField);
              if (o is IGeometry geo)
              {
                if (row is IFeature feature)
                {
                  geo.SetSpatialReference(feature.Shape.GetSpatialReference());
                }
              }
              entity.SetPropertyValue(it.PropertyName, o);
            }
          }
        }

        if (entity is IFeatureEntity fe)
        {
          fe.SetObjectID(row.Oid);
        }
      }
    }

    public static DataTable ToDataTable<T>(Predicate<EntityProperty> predicate = null)
    {
      var tableName = GetTableName<T>();
      var dt = new DataTable(tableName);
      var lst = GetAttributes<T>();
      foreach (var it in lst)
      {
        if (predicate?.Invoke(it) != false)
        {
          var type = it.PropertyType;
          if (type == typeof(DateTime?))
          {
            type = typeof(DateTime);
          }
          else if (type == typeof(Enum))
          {
            switch (it.FieldType)
            {
              case eFieldType.eFieldTypeInteger:
                type = typeof(int);
                break;
              case eFieldType.eFieldTypeSmallInteger:
                type = typeof(short);
                break;
              default:
                throw new Exception($"not impl {it.FieldType}");
            }
          }
          else if (type == typeof(double?))
          {
            type = typeof(double);
          }
          else if (type == typeof(float?))
          {
            type = typeof(float);
          }
          else if (type == typeof(decimal?))
          {
            type = typeof(decimal);
          }
          else if (type == typeof(int?))
          {
            type = typeof(int);
          }
          else if (type == typeof(short?))
          {
            type = typeof(short);
          }
          try
          {
            dt.Columns.Add(it.FieldName, type);
          }
          catch (Exception ex)
          {
            throw ex;
          }
        }
      }
      return dt;
    }

  }

  public class EntityProperty
  {
    public string PropertyName;
    public string FieldName;
    public string AliasName;
    public string CodeType;
    public Type PropertyType;

    private eFieldType fieldType = eFieldType.eFieldTypeNull;
    public eFieldType FieldType
    {
      get {
        if (fieldType == eFieldType.eFieldTypeNull)
        {
          if (PropertyType == typeof(decimal) || PropertyType == typeof(decimal?)
              || PropertyType == typeof(double) || PropertyType == typeof(double?))
          {
            fieldType = eFieldType.eFieldTypeDouble;
          }
          else if (PropertyType == typeof(float) || PropertyType == typeof(float?))
          {
            fieldType = eFieldType.eFieldTypeSingle;
          }
          else if (PropertyType == typeof(IGeometry))
          {
            fieldType = eFieldType.eFieldTypeGeometry;
          }
          else if (PropertyType == typeof(string))
          {
            fieldType = eFieldType.eFieldTypeString;
          }
          else if (PropertyType == typeof(int) || PropertyType == typeof(int?))
          {
            fieldType = eFieldType.eFieldTypeInteger;
          }
          else if (PropertyType == typeof(short) || PropertyType == typeof(short?))
          {
            fieldType = eFieldType.eFieldTypeSmallInteger;
          }
        }
        return fieldType;
      }
      set {
        fieldType = value;
      }
    }
    public object Tag;
    public object DefaultValue
    {
      get; set;
    }

    /// <summary>
    /// unique属性表示该字段是否为唯一标识，默认为false。如果表中有一个字段需要唯一标识，则既可以使用该标记，也可以使用@Table标记中的@UniqueConstraint。
    /// </summary>
    public bool Unique
    {
      get; set;
    }

    /// <summary>
    /// 表示该字段是否可以为null值，默认为true。
    /// </summary>
    public bool Nullable { get; set; } = true;

    /// <summary>
    /// 表示在使用“INSERT”脚本插入数据时，是否需要插入该字段的值。
    /// </summary>
    public bool Insertable { get; set; } = true;
    /// <summary>
    /// 表示在使用“UPDATE”脚本插入数据时，是否需要更新该字段的值。
    /// insertable和updatable属性一般多用于只读的属性，例如主键和外键等。这些字段的值通常是自动生成的。
    /// </summary>
    public bool Updatable { get; set; } = true;
    public bool PrimaryKey
    {
      get; set;
    }
    public bool AutoIncrement
    {
      get; set;
    }
    /// <summary>
    /// 不使用
    /// </summary>
    //private eFieldType ColumnType { get; set; }

    /// <summary>
    /// 表示字段的长度，当字段的类型为varchar时，该属性才有效，默认为255个字符。
    /// </summary>
    public int Length
    {
      get; set;
    }


    /// <summary>
    /// 当字段类型为double或decimal时，precision表示数值的总长度
    /// </summary>
    public int Precision
    {
      get; set;
    }
    /// <summary>
    /// 当字段类型为double或decimal时，表示小数点所占的位数
    /// </summary>
    public int Scale
    {
      get; set;
    }


    public eGeometryType GeometryType { get; set; } = eGeometryType.eGeometryNull;
    ///// <summary>
    ///// 是否自增长字段
    ///// </summary>
    //public bool Auto;

    public bool IsStringField
    {
      get {
        return PropertyType == typeof(string);
      }
    }
    public bool IsDateTimeField
    {
      get {
        return PropertyType == typeof(DateTime) || PropertyType == typeof(DateTime?);
      }
    }
    public bool IsNumericField
    {
      get {
        return PropertyType == typeof(decimal) || PropertyType == typeof(double) || PropertyType == typeof(long) || PropertyType == typeof(int) || PropertyType == typeof(short)
  || PropertyType == typeof(decimal?) || PropertyType == typeof(double?) || PropertyType == typeof(long?) || PropertyType == typeof(int?) || PropertyType == typeof(short?)
  ;
      }
    }
  }
}
