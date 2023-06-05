using Agro.GIS;
using Agro.LibCore.Database;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Agro.LibCore.Repository
{
  public abstract class CrudRepository<T> where T : class, new()
  {
    class GeometryGetter
    {
      //private IWorkspace _db;
      private string? ShapeFieldName;
      private string? OidFieldName;
      private readonly string TableName;
      private readonly CrudRepository<T> _p;
      internal GeometryGetter(CrudRepository<T> p, string tableName)
      {
        _p = p;
        TableName = tableName;
      }
      internal IGeometry? GetShape(int oid)
      {
        if (ShapeFieldName == null)
        {
          InitShapeAndOidField();
        }
        if (ShapeFieldName == "")
        {
          return null;
        }

        var geo = _p.Db.GetShape(TableName, ShapeFieldName!, OidFieldName!, oid);
        return geo;
      }

      private void InitShapeAndOidField()
      {
        var _db = _p.Db;
        if (_db != null)
        {
          ShapeFieldName = "";
          try
          {
            OidFieldName = _db.QueryOidFieldName(TableName);
            ShapeFieldName = _db.QueryShapeFieldName(TableName);
          }
          catch (Exception ex)
          {
            Console.WriteLine(ex.ToString());
          }
        }
      }
    }
    public abstract IWorkspace Db
    {
      get;// private set;
    }
    private OrmObject Orm
    {
      get {
        return (Db as WorkspaceBase).Orm;
      }
    }

    private readonly GeometryGetter _geoGetter;

    private TableMeta _tableMeta;
    public TableMeta TableMeta
    {
      get {
        if (_tableMeta == null)
        {
          _tableMeta = Db.QueryTableMeta(TableName);
        }
        return _tableMeta;
      }
      //private set;
    }
    protected CrudRepository()//IWorkspace db)
    {
      //ChangeSource(db);
      _geoGetter = new GeometryGetter(this, Entity<T>.GetTableName());
    }
    public void ChangeSource(IWorkspace db)
    {
      if (db != Db)
      {
        //Db = db;
        //_geoGetter.Reset(db);
        _tableMeta = db.QueryTableMeta(TableName);
      }
    }
    public string TableName
    {
      get {
        return Entity<T>.GetTableName();
      }
    }

    public void TruncateTable()
    {
      Db.TruncateTable(TableName);
    }
    /// <summary>
    /// 根据subFields构造一个DataTable结构
    /// </summary>
    /// <param name="subFields"></param>
    /// <returns></returns>
    public DataTable DataTable(Expression<Action<ListCreator, T>> subFields)
    {
      return Entity<T>.DataTable(subFields);
    }
    public void SqlBulkCopy(DataTable dt, bool useInnerTransaction = false)
    {
      if (useInnerTransaction)
      {
        try
        {
          Db.BeginTransaction();
          Db.SqlBulkCopyByDatatable(TableMeta, dt);
          Db.Commit();
        }
        catch (Exception ex)
        {
          Db.Rollback();
          throw ex;
        }
      }
      else
      {
        Db.SqlBulkCopyByDatatable(TableMeta, dt);
      }
    }

    //public List<string> GetProperties(bool fContainBinaryField = true)
    //{
    //	return Entity<T>.GetProperties(fContainBinaryField);
    //}
    //public IEnumerable<string> GetProperties(Expression<Action<ListCreator, T>> expression)
    //{
    //	return FieldsExpression<T>.Fields(expression);
    //}
    public int Insert(IEntity entity, SubFields fields = null)
    {
      return Orm.Insert(entity, fields);
    }
    public int Insert(IEntity entity, Expression<Action<ListCreator, T>> fields)
    {
      return Insert(entity, SubFields.Make(fields));
    }

    public int Update(T entiry, Expression<Func<T, bool>> wh, SubFields fields = null)
    {
      return Orm.Update(entiry, wh, fields);
    }
    public int Update(T entiry, Expression<Func<T, bool>> wh, Expression<Action<ListCreator, T>> fields)
    {
      return Update(entiry, wh, SubFields.Make(fields));
    }

    public int UpdateNoBinary(T entiry, Expression<Func<T, bool>> wh)
    {
      return Update(entiry, wh, Entity<T>.GetSubFields(false));
    }

    public int Delete(Expression<Func<T, bool>> wh)
    {
      return Orm.Delete<T>(wh);
    }

    public T Find(Expression<Func<T, bool>> wh, SubFields fields = null)
    {
      T entity = null;
      Orm.FindCallback<T>(wh, it =>
      {
        entity = it.Item;
        it.Cancel = true;
      }, fields);
      return entity;
    }
    public T Find(Expression<Func<T, bool>> wh, Expression<Action<ListCreator, T>> fields)
    {
      return Find(wh, SubFields.Make(fields));
    }

    public T FindNoBinary(Expression<Func<T, bool>> wh)
    {
      return Find(wh, Entity<T>.GetSubFields(false));
    }
    public List<T> FindAll(string wh, SubFields fields = null)
    {
      var lst = new List<T>();
      FindCallback(wh, it => lst.Add(it.Item), fields, false);
      return lst;
    }
    public List<T> FindAll(string wh, Expression<Action<ListCreator, T>> fields)
    {
      return FindAll(wh, SubFields.Make(fields));
    }
    public List<T> FindAll(Expression<Func<T, bool>> wh, SubFields fields = null)
    {
      var lst = new List<T>();
      Orm.FindCallback<T>(wh, t => lst.Add(t.Item), fields, false);
      return lst;
    }
    public List<T> FindAll(Expression<Func<T, bool>> wh, Expression<Action<ListCreator, T>> fields)
    {
      return FindAll(wh, SubFields.Make(fields));
    }
    public List<T> FindAllNoBinary(Expression<Func<T, bool>> wh)
    {
      return FindAll(wh, Entity<T>.GetSubFields(false));
    }
    public void FindNoBinary(Expression<Func<T, bool>> wh, Action<CancelItem<T>> callback)
    {
      Orm.FindCallback<T>(wh, callback, Entity<T>.GetSubFields(false));
    }
    public void FindCallback(string wh, Action<CancelItem<T>> callback, SubFields fields = null, bool fRecycle = true, ICancelTracker cancel = null)
    {
      Orm.FindCallback<T>(wh, callback, fields, fRecycle, cancel);
    }
    public void FindCallback1(Expression<Func<T, bool>> wh, Action<CancelItem<T>> callback, SubFields fields = null, bool fRecycle = true, ICancelTracker cancel = null)
    {
      Orm.FindCallback<T>(wh, callback, fields, fRecycle, cancel);
    }
    public void FindCallback(Expression<Func<T, bool>> wh, Action<CancelItem<T>> callback, Expression<Action<ListCreator, T>> fields = null, bool fRecycle = true, ICancelTracker cancel = null)
    {
      Orm.FindCallback(wh, callback, fields, fRecycle, cancel);
    }
    //public void FindCallback(Expression<Func<T, bool>> wh, Action<T> callback, SubFields fields = null, bool fRecycle = true, ICancelTracker cancel = null)
    //{
    //	FindCallback(wh, t =>callback(t), fields, fRecycle, cancel);
    //}



    public object FindBy(string propertyName, Expression<Func<T, bool>> wh)
    {
      var sWh = Where(wh);
      var sql = $"select {FieldName(propertyName)} from {TableName}";
      if (!string.IsNullOrEmpty(sWh))
      {
        sql += $" where {sWh}";
      }
      return Db.QueryOne(sql);
    }
    public int FindIntBy(string propertyName, Expression<Func<T, bool>> wh)
    {
      return SafeConvertAux.ToInt32(FindBy(propertyName, wh));
    }
    public List<object> FindDistinctBy(Expression<Func<T, object>> field, Expression<Func<T, bool>> wh, OrderByType orderBy = OrderByType.ASC, ICancelTracker cancel = null)
    {
      var propertyName = FieldsExpression<T>.Field(field).PropertyName;
      var lst = new List<object>();
      var sWh = Where(wh);
      var fieldName = FieldName(propertyName);
      var sql = $"select distinct {fieldName} from {TableName} where {sWh}";
      if (orderBy != OrderByType.None)
      {
        sql += $" order by {fieldName}";
        if (orderBy == OrderByType.DESC)
          sql += " desc";
      }
      Db.QueryCallback(sql, r =>
       {
         lst.Add(r.GetValue(0));
       }, cancel);
      return lst;
    }

    public int Count(Expression<Func<T, bool>> wh = null)
    {
      return FindIntBy("count(*)", wh);
    }
    public int Count(string wh)
    {
      var sql = $"select count(*) from {TableName}";
      if (!string.IsNullOrEmpty(wh))
      {
        sql += $"  where {wh}";
      }
      return SafeConvertAux.ToInt32(Db.QueryOne(sql));
    }
    public bool Exists(Expression<Func<T, bool>> wh)
    {
      return FindIntBy("count(1)", wh) > 0;
    }




    public IGeometry GetShape(int oid)
    {
      return _geoGetter.GetShape(oid);
    }

    public int GetSrid()
    {
      int srid = 0;
      Try.Catch(() =>
      {
        if (Db is IFeatureWorkspace fws)
        {
          srid = fws.GetSRID(TableName);
        }
      }, false);
      return srid;
    }

    public void SpatialQuery(IFeatureClass fc, ISpatialFilter qf, Func<T, bool> callback, bool fRecycle = true, ICancelTracker cancel = null)
    {
      T en = new T();
      fc.SpatialQuery(qf, ft =>
      {
        if (!fRecycle) en = new T();
        EntityUtil.WriteToEntity(ft, en);
        return callback(en);
      }, cancel, fRecycle);
    }
    public void SpatialQuery(IFeatureClass fc, ISpatialFilter qf, Action<T> callback, bool fRecycle = true, ICancelTracker cancel = null)
    {
      SpatialQuery(fc, qf, en => { callback(en); return true; }, fRecycle, cancel);
    }


    public string Where(Expression<Func<T, bool>> wh)
    {
      return Orm.CreateWhere(wh);
    }

    /// <summary>
    /// 根据属性名获取字段名
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected string FieldName(string propertyName)
    {
      return Entity<T>.GetFieldName(propertyName);
    }
  }

  public class SimpleCrudRepository<T> : CrudRepository<T> where T : class, new()
  {
    private readonly IWorkspace _db;
    public override IWorkspace Db
    {
      get {
        return _db;
      }
    }

    public SimpleCrudRepository(IWorkspace db)
    {
      _db = db;
    }
  }

  public enum OrderByType
  {
    /// <summary>
    /// 不排序
    /// </summary>
    None,
    /// <summary>
    /// 升序
    /// </summary>
    ASC,
    /// <summary>
    /// 降序
    /// </summary>
    DESC,
  }
}
