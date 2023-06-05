/*
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   IWorkspaceFactory
 * 创 建 人：   颜学铭
 * 创建时间：   2016/12/14 10:36:14
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Agro.LibCore
{
  public class TableItem
  {
    public string? TableName
    {
      get; set;
    }
    public string? AliasName
    {
      get; set;
    }
  }
  public class FeatureClassItem : TableItem
  {
    public string? ShapeFieldName
    {
      get; set;
    }
    public eGeometryType GeometryType
    {
      get; set;
    }
    public FeatureClassItem Clone()
    {
      return new FeatureClassItem()
      {
        TableName = TableName,
        AliasName = AliasName,
        ShapeFieldName = ShapeFieldName,
        GeometryType = GeometryType
      };
    }
  }
  public interface IFeatureWorkspace : IWorkspace//IDisposable
  {
    IFeatureClass OpenFeatureClass(string tableName, string lpszAccess = "rb");
    void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null, TableIndex[] index = null);
    //int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null);
    //void QueryCallback(string sql, Func<IDataReader, bool> callback, ICancelTracker cancel = null);
    void QueryObjectClasses(Action<FeatureClassItem> callback);

    IFeatureWorkspaceFactory WorkspaceFactory
    {
      get;
    }
  }
  public interface IFeatureWorkspaceFactory
  {
    IFeatureWorkspace OpenWorkspace(string connectionString);
    IFeatureWorkspace CreateWorkspace(string connectionString);
    IFeatureClass OpenFeatureClass(string connectionString, string tableName, string lpszAccess = "rb");
    void QueryObjectClasses(string connectionString, Action<FeatureClassItem> callback);
  }
    //public class WorkspaceT
    //{
    //    internal int ReferenceCount;
    //    public string ConnectionString
    //    {
    //        get; set;
    //    }
    //    internal void Open(string connectionString)
    //    {
    //        ConnectionString = connectionString;
    //    }
    //}
    public abstract class FeatureWorkspaceFactoryBase
    {
        public virtual IFeatureWorkspace CreateWorkspace(string connectionString)
        {
            return null;
        }
        public abstract IFeatureWorkspace OpenWorkspace(string connectionString);

        /// <summary>
        /// 建立数据库连接
        /// </summary>
        /// <param name="connectionString">like @"e:\test.db3"</param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IFeatureClass OpenFeatureClass(string connectionString, string tableName, string lpszAccess = "rb")
        {
            using var ws = OpenWorkspace(connectionString);
            var fc = ws.OpenFeatureClass(tableName, lpszAccess);
            return fc;
        }

        public void QueryObjectClasses(string connectionString, Action<FeatureClassItem> callback)
        {
            using var db = OpenWorkspace(connectionString);
            db.QueryObjectClasses(callback);
        }

    }

    public class MemoryWorkspaceFactory : FeatureWorkspaceFactoryBase, IFeatureWorkspaceFactory
    {
        public class ValueItem : SQLiteWorkspace, IFeatureWorkspace
        {
            public int ReferenceCount;
            private readonly string DBKey;
            public ValueItem(IFeatureWorkspaceFactory wf, string dBKey)
            {
                WorkspaceFactory = wf;
                DBKey = dBKey;
            }
            #region IFeatureWorkspace

            public IFeatureClass OpenFeatureClass(string tableName, string lpszAccess = "rb")
            {
                var fc = new SqliteFeatureClass(this, (fc1) =>
                {
                    Dispose();
                });
                fc.Open(tableName);
                ++ReferenceCount;
                return fc;
            }

            public IFeatureWorkspaceFactory WorkspaceFactory
            {
                get; private set;
            }

            #endregion
            public override void Dispose()
            {
                if (ReferenceCount > 0)
                {
                    if (--ReferenceCount == 0)
                    {
                        base.Dispose();
                        Instance._dic.Remove(DBKey);// base.ConnectionString);
                    }
                }
            }
        }
        private readonly Dictionary<string, ValueItem> _dic = new Dictionary<string, ValueItem>();
        private MemoryWorkspaceFactory()
        {

        }
        public static readonly MemoryWorkspaceFactory Instance = new();

        public override IFeatureWorkspace OpenWorkspace(string dbName)// connectionString)
        {
            //var connectionString =  ":memory:";// $"{dbName};Mode=Memory;Cache=Shared";
            if (!_dic.TryGetValue(dbName, out var vi))
            {
                vi = new ValueItem(this,dbName);
                vi.Open(":memory:");// connectionString);
                var db = vi.GetRawDB();
                if (!db.IsTableExists("ime_FeatureDataset"))
                {
                    GeoDBSqlite.CreateSysTables.Create(db);
                }
                _dic[dbName] = vi;
            }
            ++vi.ReferenceCount;
            return vi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">形如：@"e:\test.db3"</param>
        /// <returns></returns>
        public override IFeatureWorkspace CreateWorkspace(string connectionString)
        {
            //GeoDBSqlite.CreateDatabase(connectionString);
            return OpenWorkspace(connectionString);
        }
    }

    //public class MemoryWorkspaceFactory : FeatureWorkspaceFactoryBase, IFeatureWorkspaceFactory
    //{
    //  public class MemoryFeature : Feature
    //  {
    //    public MemoryFeature(IFields fields = null)
    //        : base(fields)
    //    {
    //    }
    //    internal void UpdateFields(IFields allFields, IEnumerable<string> subFields)
    //    {
    //      if (base.Fields is Fields fields)
    //      {
    //        fields.Clear();
    //      }
    //      else
    //      {
    //        fields = new Fields();
    //      }

    //      foreach (var fieldName in subFields)
    //      {
    //        int iField = allFields.FindField(fieldName);
    //        if (iField >= 0)
    //        {
    //          var pField = allFields.GetField(iField);
    //          fields.AddField(pField);
    //        }
    //        else
    //        {
    //          throw new Exception("字段" + fieldName + "不存在！");
    //        }
    //      }
    //      base.Fields = fields;
    //    }


    //  }
    //  public class MemoryFeatureClass : FeatureClassBase, IFeatureClass, IFeatureClassRender
    //  {
    //    public int ReferenceCount;
    //    private ValueItem _workspace
    //    {
    //      get {
    //        return Workspace as ValueItem;
    //      }
    //    }
    //    private readonly Dictionary<int, IFeature> _features = new Dictionary<int, IFeature>();
    //    private int _lastOID = 0;
    //    private OkEnvelope _fullExtent;
    //    private int _srid;
    //    public MemoryFeatureClass(ValueItem vi, string tableName, IFields fields, int srid, string tableAliasName = null)
    //        : base(vi)
    //    {
    //      _srid = srid;
    //      if (srid > 0)
    //      {
    //        this.SpatialReference = SpatialReferenceFactory.GetSpatialReference(srid);
    //      }
    //      //this.ShapeType = FieldsUtil.QueryShapeType(fields);
    //      for (int i = 0; i < fields.FieldCount; ++i)
    //      {
    //        var field = fields.GetField(i);
    //        if (field.FieldType == eFieldType.eFieldTypeGeometry)
    //        {
    //          this.ShapeType = field.GeometryType;
    //          ShapeFieldName = field.FieldName;
    //        }
    //        else if (field.FieldType == eFieldType.eFieldTypeOID)
    //        {
    //          OIDFieldName = field.FieldName;
    //        }
    //        if (OIDFieldName != null && ShapeFieldName != null)
    //        {
    //          break;
    //        }
    //      }
    //      TableMeta = new TableMeta(tableName)
    //      {
    //        OIDFieldName = this.OIDFieldName,
    //        ShapeFieldName = this.ShapeFieldName,
    //        SpatialReference = this.SpatialReference,
    //        Fields = fields,
    //        ShapeType = this.ShapeType
    //      };
    //    }

    //    public string ClassName
    //    {
    //      get {
    //        return this.GetType().Name;
    //      }
    //    }
    //    public void CancelTask()
    //    {
    //      //throw new NotImplementedException();
    //    }

    //    public IFeature CreateFeature()
    //    {
    //      var feature = new MemoryFeature(Fields);
    //      feature.ResetValueList();
    //      feature.Oid = 0;
    //      //feature.Oid = ++_lastOID;
    //      //_features[feature.Oid] = feature;
    //      return feature;
    //    }

    //    public override int Delete(IRow row)
    //    {
    //      if (_features.ContainsKey(row.Oid))
    //      {
    //        _features.Remove(row.Oid);
    //        if (row.Oid == _lastOID)
    //        {
    //          --_lastOID;
    //        }
    //        return 1;
    //      }
    //      return 0;
    //    }
    //    public override int Append(IRow row)
    //    {
    //      if (!(row is IFeature feature))
    //      {
    //        throw new Exception("row 必须为IFeature类型");
    //      }
    //      feature.Oid = ++_lastOID;
    //      _features[feature.Oid] = feature;
    //      return 1;
    //    }
    //    public override int Append(IEnumerable<IRow> adds, bool fUseTransaction = true)
    //    {
    //      int nRes = 0;
    //      if (adds != null)
    //      {
    //        foreach (var r in adds)
    //        {
    //          nRes += Append(r);
    //        }
    //      }
    //      return nRes;
    //    }
    //    //public override int Update(IRow row,bool fClearChangedValues=true)
    //    public override int Update(RowUpdateData updateValues)
    //    {
    //      //System.Diagnostics.Debug.Assert(row is MemoryFeature);
    //      //var feature = row as MemoryFeature;
    //      if (_features.ContainsKey(updateValues.Oid))
    //      {
    //        //_features[row.Oid] = row as IFeature;
    //        //var feature=_features[row.Oid];
    //        //IRowUtil.CopyValues(row, feature);
    //        return 1;
    //      }
    //      return 0;
    //    }
    //    //public override int Edit(IEnumerable<IRow> adds, IEnumerable<IRow> updates, IEnumerable<IRow> deletes)
    //    //{
    //    //    int nRes = 0;
    //    //    try
    //    //    {
    //    //        #region update
    //    //        foreach (var r in updates)
    //    //        {
    //    //            nRes += Update(r);
    //    //        }
    //    //        #endregion

    //    //        #region delete rows
    //    //        foreach (var r in deletes)
    //    //        {
    //    //            nRes += Delete(r);
    //    //        }
    //    //        #endregion

    //    //        #region insert
    //    //        foreach (var r in adds)
    //    //        {
    //    //            nRes += Append(r);
    //    //        }
    //    //        #endregion

    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        throw ex;
    //    //    }

    //    //    return nRes;
    //    //}
    //    public override int ClearAll()
    //    {
    //      var cnt = _features.Count;
    //      _features.Clear();
    //      _lastOID = 0;
    //      return cnt;
    //    }
    //    public override void UpdateShape(int oid, IGeometry g)
    //    {
    //      if (_features.TryGetValue(oid, out IFeature ft))
    //      {
    //        ft.Shape = g;
    //      }
    //    }

    //    public void Dispose()
    //    {
    //      if (ReferenceCount > 0)
    //      {
    //        if (--ReferenceCount == 0)
    //        {
    //          _workspace._dicTableName2FC.Remove(this.TableName);
    //        }
    //      }
    //    }

    //    public override int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
    //    {
    //      throw new NotImplementedException();
    //    }

    //    public OkEnvelope GetFullExtent()
    //    {
    //      return _fullExtent;
    //    }

    //    public override IGeometry GetShape(int oid)
    //    {
    //      if (_features.TryGetValue(oid, out IFeature ft))
    //      {
    //        return ft.Shape;
    //      }
    //      return null;
    //    }

    //    public override IFeature GetFeatue(int oid)
    //    {
    //      if (_features.TryGetValue(oid, out IFeature ft))
    //      {
    //        return ft;
    //      }
    //      return null;
    //    }
    //    private PredicateNodeHelper PrepareSearch(IQueryFilter filter, out MemoryFeature row)
    //    {
    //      PredicateNodeHelper pnh = null;// new PredicateNodeHelper();
    //      if (!string.IsNullOrEmpty(filter.WhereClause))
    //      {
    //        var ast = AstLabelExpress.BuildAST(filter.WhereClause);
    //        if (ast != null)
    //        {
    //          pnh = new PredicateNodeHelper();
    //          pnh.ParseFields(ast);
    //        }
    //      }

    //      var subFields = new List<string>();
    //      if (filter.SubFields == null)
    //      {
    //        var n = Fields.FieldCount;// shp.GetFieldCount();
    //        for (var i = 0; i < n; ++i)
    //        {
    //          subFields.Add(Fields.GetField(i).FieldName);// shp.GetFieldName(i));
    //        }
    //      }
    //      else
    //      {
    //        var sa = filter.SubFields.Split(',');
    //        foreach (var fieldName in sa)
    //        {
    //          subFields.Add(fieldName);
    //        }
    //      }
    //      if (pnh != null)
    //      {
    //        foreach (var fn in pnh.fields)
    //        {
    //          int n = subFields.FindIndex(s =>
    //          {
    //            return StringUtil.isEqualIgnorCase(s, fn);
    //          });
    //          if (n < 0)
    //          {
    //            subFields.Add(fn);
    //          }
    //        }
    //      }

    //      row = new MemoryFeature();
    //      row.UpdateFields(this.Fields, subFields);
    //      row.ResetValueList();

    //      if (pnh != null)
    //      {
    //        pnh.Prepare(row);
    //      }
    //      return pnh;
    //    }
    //    public override void Search(IQueryFilter filter, Func<IRow, bool> callback)
    //    {
    //      var pnh = PrepareSearch(filter, out var row);
    //      foreach (var kv in _features)
    //      {
    //        var oid = kv.Key;
    //        if (filter.Oids != null && !filter.Oids.Contains(oid))
    //        {
    //          continue;
    //        }
    //        IRowUtil.CopyValues(kv.Value, row);
    //        row.Oid = kv.Value.Oid;
    //        if (pnh != null && !pnh.IsOK())
    //        {
    //          continue;
    //        }

    //        var fContinue = callback(row);
    //        if (!fContinue)
    //        {
    //          return;
    //        }
    //      }
    //    }

    //    public void SearchAsync(IQueryFilter filter, Func<IRow, bool> callback)
    //    {
    //      Search(filter, callback);
    //    }

    //    public override int Count(string where)
    //    {
    //      var filter = new QueryFilter
    //      {
    //        SubFields = this.OIDFieldName,
    //        WhereClause = where
    //      };
    //      var cnt = 0;
    //      Search(filter, r =>
    //      {
    //        ++cnt;
    //        return true;
    //      });
    //      return cnt;
    //    }

    //    public override bool SpatialQuery(ISpatialFilter filter, Func<IFeature, bool> callback, ICancelTracker cancel = null, bool fRecycle = true)
    //    {
    //      return DoSpatialQuery(filter, cancel, callback, fRecycle);
    //    }

    //    public bool SpatialQueryAsync(ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callback)
    //    {
    //      return DoSpatialQuery(filter, cancel, callback, true);
    //    }
    //    private bool DoSpatialQuery(ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callback, bool fRecycle = true)
    //    {
    //      var pnh = PrepareSearch(filter, out var row);

    //      var geo = filter.Geometry;
    //      geo.Project(this.SpatialReference);
    //      var geoEnv = geo.EnvelopeInternal;
    //      foreach (var kv in _features)
    //      {
    //        var g = kv.Value.Shape;
    //        if (g == null)
    //        {
    //          continue;
    //        }
    //        var oid = kv.Key;
    //        if (filter.Oids != null && !filter.Oids.Contains(oid))
    //        {
    //          continue;
    //        }
    //        IRowUtil.CopyValues(kv.Value, row);
    //        row.Oid = kv.Value.Oid;
    //        if (pnh != null && !pnh.IsOK())
    //        {
    //          continue;
    //        }
    //        var fok = false;
    //        switch (filter.SpatialRel)
    //        {
    //          case eSpatialRelEnum.eSpatialRelEnvelopeIntersects:
    //            fok = geoEnv.Intersects(g.EnvelopeInternal);
    //            break;
    //          case eSpatialRelEnum.eSpatialRelIntersects:
    //            fok = geo.Intersects(g);
    //            break;
    //          case eSpatialRelEnum.eSpatialRelWithin:
    //            fok = geo.Within(g);
    //            break;
    //          case eSpatialRelEnum.eSpatialRelContains:
    //            fok = geo.Contains(g);
    //            break;
    //          default:
    //            System.Diagnostics.Debug.Assert(false);
    //            break;
    //        }
    //        if (fok)
    //        {
    //          IFeature feature = row;
    //          if (!fRecycle)
    //          {
    //            feature = row.Clone() as IFeature;
    //          }
    //          var fContinue = callback(feature);
    //          if (!fContinue)
    //          {
    //            return false;
    //          }
    //        }
    //      }
    //      return true;
    //    }
    //    public override void RecalcFullExtent()
    //    {
    //      _fullExtent = null;
    //      Envelope env = null;
    //      foreach (var kv in _features)
    //      {
    //        var g = kv.Value.Shape;
    //        if (g != null)
    //        {
    //          var e = g.EnvelopeInternal;
    //          if (env == null)
    //          {
    //            env = e;
    //          }
    //          else
    //          {
    //            env = env.ExpandedBy(e);
    //          }
    //        }
    //      }
    //      if (env != null)
    //      {
    //        _fullExtent = new OkEnvelope(env, this.SpatialReference);
    //      }
    //    }
    //  }

    //  public class ValueItem : WorkspaceBase, IFeatureWorkspace
    //  {
    //    public int ReferenceCount;

    //    internal readonly Dictionary<string, MemoryFeatureClass> _dicTableName2FC = new Dictionary<string, MemoryFeatureClass>();
    //    internal ValueItem(IFeatureWorkspaceFactory wf)
    //    {
    //      WorkspaceFactory = wf;
    //    }
    //    public string ConnectionString
    //    {
    //      get; set;
    //    }
    //    internal void Open(string connectionString)
    //    {
    //      ConnectionString = connectionString;
    //    }
    //    public eDatabaseType DatabaseType
    //    {
    //      get {
    //        return eDatabaseType.Memory;
    //      }
    //    }
    //    public bool IsOpen()
    //    {
    //      return true;
    //    }
    //    public override void DropTable(string tableName)
    //    {
    //      _dicTableName2FC.Remove(tableName);
    //    }
    //    public override void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null, TableIndex[] index = null)
    //    {
    //      if (_dicTableName2FC.ContainsKey(tableName))
    //      {
    //        throw new Exception("表存在！");
    //      }
    //      var fc = new MemoryFeatureClass(this, tableName, fields, srid, tableAliasName);
    //      _dicTableName2FC[tableName] = fc;
    //      //return OpenFeatureClass(tableName);
    //    }
    //    /// <summary>
    //    /// 使用完后需要调用Dispose
    //    /// </summary>
    //    /// <param name="tableName"></param>
    //    /// <param name="lpszAccess"></param>
    //    /// <returns></returns>
    //    public IFeatureClass OpenFeatureClass(string tableName, string lpszAccess = "rb")
    //    {
    //      if (_dicTableName2FC.TryGetValue(tableName, out var fc))
    //      {
    //        ++fc.ReferenceCount;
    //        ++ReferenceCount;
    //        return fc;
    //      }
    //      return null;
    //    }
    //    public IFeatureWorkspaceFactory WorkspaceFactory
    //    {
    //      get; private set;
    //    }
    //    public override void Dispose()
    //    {
    //      if (ReferenceCount > 0)
    //      {
    //        if (--ReferenceCount == 0)
    //        {
    //          MemoryWorkspaceFactory.Instance._dic.Remove(this.ConnectionString);
    //        }
    //      }
    //    }

    //    public override bool IsTableExists(string tableName)
    //    {
    //      return _dicTableName2FC.ContainsKey(tableName);
    //    }
    //  }

    //  private readonly Dictionary<string, ValueItem> _dic = new Dictionary<string, ValueItem>();

    //  public static readonly MemoryWorkspaceFactory Instance = new MemoryWorkspaceFactory();

    //  public override IFeatureWorkspace OpenWorkspace(string connectionString)
    //  {
    //    if (!_dic.TryGetValue(connectionString, out var vi))
    //    {
    //      vi = new ValueItem(this);
    //      _dic[connectionString] = vi;
    //      vi.Open(connectionString);
    //    }
    //    ++vi.ReferenceCount;
    //    return vi;
    //  }
    //}
}
