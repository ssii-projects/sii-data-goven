using Agro.GIS;
using Agro.LibCore.Database;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Agro.LibCore
{
    public class SQLParam
    {
        private object _paramValue;
        public string ParamName;
        public object ParamValue
        {
            get { return _paramValue; }
            set
            {
                if (value == null)
                {
                    _paramValue = DBNull.Value;
                }
                else
                {
                    _paramValue = value;
                }
            }
        }
    }

    public interface IDataReader
  {
    int FieldCount
    {
      get;
    }
    string GetName(int col);
    object? GetValue(int col);
    bool IsDBNull(int col);
    string GetString(int col);
    short GetInt16(int col);
    int GetInt32(int col);
    byte GetByte(int col);
    bool GetBoolean(int col);
    double GetDouble(int col);
    decimal? GetDecimal(int col);
    object GetRawDataReader();
    IGeometry GetShape(int col);
    DateTime? GetDateTime(int col);

  }

  public enum TableIndexType
  {
    /// <summary>
    /// 唯一索引
    /// </summary>
    Unique,
    /// <summary>
    /// 空间索引
    /// </summary>
    SPATIAL,
  }
  public class TableIndex
  {
    public string IndexName
    {
      get; set;
    }
    public TableIndexType IndexType
    {
      get; set;
    }
    public string IndexFields
    {
      get; set;
    }
    public TableIndex(string idxName, TableIndexType idxType, string idxFields)
    {
      IndexName = idxName;
      IndexType = idxType;
      IndexFields = idxFields;
    }
  }

  public interface IWorkspace : IDisposable
  {
    eDatabaseType DatabaseType
    {
      get;
    }
    string ConnectionString
    {
      get;
    }
    string DBVersion();
    bool IsOpen();

    void QueryTables(Action<TableItem> callback);
    string QueryPrimaryKey(string tableName);
    Dictionary<string, object> QueryTableFieldDefaultValue(string tableName);
    void QueryCallback(string sql, Func<IDataReader, bool> callback, ICancelTracker cancel = null);
    void QueryCallback(string sql, Action<IDataReader> callback, ICancelTracker cancel = null);
    object QueryOne(string sql);
    int QueryOneInt(string sql);
    void TruncateTable(string tableName);
    int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null);
    object ExecuteScalar(string sql, IEnumerable<SQLParam> lstParameters = null);
    bool IsTableExists(string tableName);
    bool IsFieldExists(string tableName, string fieldName);
    void DropTable(string tableName);

    void CreateTable(string tableName, IFields fields, string aliasName = null, TableIndex[] index = null);

    #region yxm 2021-3-26
    void AddField(string tableName, IField field);
    #endregion

    #region yxm 2018-3-13
    bool IsTriggerExist(string tableName, string triggerName);
    bool IsIndexExist(string tableName, string idxName);

    List<string> QueryFields(string tableName);
    List<Field> QueryFields2(string tableName, List<Field> fields = null);
    //List<Field> QueryFields2(string tableName, List<Field> fields = null);
    Dictionary<string, Type> QueryFieldsType(string tableName);
    void SqlBulkCopyByDatatable(TableMeta TableName, DataTable dt);

    int GetSRID(string sTableName);

    int GetNextObjectID(string tableName, int num = 1);


    #endregion

    #region Spatial parts
    eGeometryType QueryShapeType(string tableName, string shapeFieldName);
    IGeometry GetShape(string tableName, string shapeFieldName, string oidFieldName, int oid);
    OkEnvelope GetFullExtent(string sTableName, string shapeFieldName);
    void UpdateShape(string tableName, string shapeFieldName, string oidFieldName, int oid, IGeometry g);
    #endregion

    ISqlFunction SqlFunc
    {
      get;
    }

    #region 事务相关
    void BeginTransaction();
    void Commit();
    void Rollback();
    #endregion

    string QueryOidFieldName(string tableName);
    string QueryShapeFieldName(string tableName);

    #region 2020-8-13
    TableMeta QueryTableMeta(string tableName);
    IRow FindRow(string tableName, IFields subFields, string wh);
    IRow GetRow(TableMeta tableMeta, int oid);
    int UpdateRow(TableMeta table, IRow row, string wh, Predicate<IField> ignoreFields = null);
    #endregion

    int UpdateRow(TableMeta table, IRow row, Predicate<IField> ignoreFields = null);
    int InsertRow(TableMeta table, IRow row, Predicate<IField> ignoreFields = null);
    IRow CreateRow(TableMeta table);

    //#region 2020-11-2
    ////int Insert<T>(T entity, SubFields subFields = null);
    //int Insert(IEntity entity, SubFields subFields = null);
    //void FillEntity(string where, IEntity entity);
    ////void Update(IEntity entity, string where, Predicate<IField> ignoreFields = null);
    //#endregion
  }

  /// <summary>
  ///描述: 定义ISqlFunction接口，映射各种数据库的SQL函数
  ///版本:1.0
  ///日期:2016/3/16 10:17:58
  ///作者:颜学铭
  /// </summary>
  public interface ISqlFunction
  {

    string Year(string expr);
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
    // string ToDouble(string expr,int precisition=38, int scale=15);
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
    ///// <summary>
    ///// sql语句截取
    ///// </summary>
    ///// <returns>返回截取字符串通配符</returns>
    //string SubStringCastOperator();

    string SubString(string expr, int iStartPos, int len);

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

    char GetParamPrefix();

    string AsBinary(string shapeField);
    string GeomFromText(IGeometry g);

  }

  ///// <summary>
  ///// 与ArcGIS保持一致
  ///// </summary>
  //public enum eGeometryType
  //{
  //  eGeometryNull = 0,
  //  eGeometryPoint = 1,
  //  eGeometryMultipoint = 2,
  //  eGeometryPolyline = 3,
  //  eGeometryPolygon = 4,
  //  eGeometryEnvelope = 5,
  //  eGeometryPath = 6,
  //  eGeometryAny = 7,
  //  eGeometryMultiPatch = 9,
  //  eGeometryRing = 11,
  //  eGeometryLine = 13,
  //  eGeometryCircularArc = 14,
  //  eGeometryBezier3Curve = 15,
  //  eGeometryEllipticArc = 16,
  //  eGeometryBag = 17,
  //  eGeometryTriangleStrip = 18,
  //  eGeometryTriangleFan = 19,
  //  eGeometryRay = 20,
  //  eGeometrySphere = 21,
  //  eGeometryTriangles = 22,
  //}

  public enum eDatabaseType
  {
    Null,
    SqlServer,
    Oracle,
    SQLite,
    Spatialite,
    ShapeFile,
    Memory,
    Access,
    MySql
  }

  public class ConnectionInfo
  {
    public string Host
    {
      get; set;
    }
    public string Port
    {
      get; set;
    }
    public string UserName
    {
      get; set;
    }
    public string Password
    {
      get; set;
    }
    public string DatabaseName
    {
      get; set;
    }

    public string IsValid(bool fCheckUserName = true)
    {
      if (string.IsNullOrEmpty(Host))
      {
        return "未输入主机";
      }
      if (fCheckUserName)
      {
        if (string.IsNullOrEmpty(UserName))
        {
          return "未输入用户名";
        }
        if (string.IsNullOrEmpty(Password))
        {
          return "未输入密码";
        }
      }
      if (string.IsNullOrEmpty(DatabaseName))
      {
        return "未输入数据库名";
      }
      return null;
    }
    internal bool IsIntegratedSecurity(eDatabaseType dbType)
    {
      return dbType == eDatabaseType.SqlServer && Host?.ToLower().StartsWith("(localdb)") == true;
    }
    public string ToConnectionString(eDatabaseType dbType)
    {
      switch (dbType)
      {
        case eDatabaseType.MySql:
          var cs = $"server={Host};uid={UserName};pwd={Password};Database={DatabaseName};Charset=utf8;AllowLoadLocalInfile=true;AllowUserVariables=true;Connection Timeout=3000";
          if (!string.IsNullOrEmpty(Port))
          {
            cs += $";port={Port}";
          }
          return cs;
        case eDatabaseType.SqlServer:
          if (IsIntegratedSecurity(dbType))
          {
            return $"Data Source={Host};Initial Catalog={DatabaseName};Integrated Security=True";
          }

          return $"Data Source={Host};Initial Catalog={DatabaseName};User ID={UserName};Password={Password}";
        default:
          throw new NotImplementedException();
      }
    }
    public void FromConnectionString(eDatabaseType dbType, string connectionString)
    {
      var dic = new Dictionary<string, string>();
      var sa = connectionString.Split(';');
      foreach (var str in sa)
      {
        var kv = str.Split('=');
        if (kv.Length == 2)
        {
          dic[kv[0].Trim().ToLower()] = kv[1].Trim();
        }
      }
      switch (dbType)
      {
        case eDatabaseType.MySql:
          Host = GetValue(dic, "server");
          Port = GetValue(dic, "port");
          UserName = GetValue(dic, "uid");
          Password = GetValue(dic, "pwd");
          DatabaseName = GetValue(dic, "database");
          break;
        case eDatabaseType.SqlServer:
          Host = GetValue(dic, "Data Source");
          //port = GetValue(dic, "port");
          UserName = GetValue(dic, "User ID");
          Password = GetValue(dic, "Password");
          DatabaseName = GetValue(dic, "Initial Catalog");
          break;
      }
    }
    private string GetValue(Dictionary<string, string> dic, string key)
    {
      if (dic.TryGetValue(key.ToLower(), out string value))
      {
        return value;
      }
      return null;
    }
  }

  public class WorkspaceBase //, IFeatureWorkspace
  {
    private readonly Dictionary<string, TableMeta> _dicTableMeta = new Dictionary<string, TableMeta>();
    protected WorkspaceBase()
    {
      Orm = new OrmObject(this as IWorkspace);
    }
    internal readonly OrmObject Orm;

    public ISqlFunction SqlFunc
    {
      get; protected set;
    }
    public virtual string DBVersion()
    {
      throw new NotImplementedException();
    }
    public virtual void QueryObjectClasses(Action<FeatureClassItem> callback)
    {
    }
    public virtual void CreateTable(string tableName, IFields fields, string aliasName = null, TableIndex[] index = null)
    {
      throw new NotImplementedException();
    }
    public virtual void AddField(string tableName, IField field)
    {
      throw new NotImplementedException();
    }
    public virtual void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null, TableIndex[] index = null)
    {
      throw new NotImplementedException();
    }
    public virtual void TruncateTable(string tableName)
    {
      this.ExecuteNonQuery($"TRUNCATE TABLE {tableName}");
    }
    public virtual int Insert(IEntity entity, SubFields subFields = null)
    {
      return Orm.Insert(entity, subFields);
    }

    public virtual void QueryTables(Action<TableItem> callback)
    {
    }

    public TableMeta QueryTableMeta(string tableName)
    {
      var key = tableName.ToLower();
      if (_dicTableMeta.TryGetValue(key, out var tableMeta))
      {
        return tableMeta;
      }
      tableMeta = new TableMeta(tableName)
      {
        PrimaryKey = QueryPrimaryKey(tableName)
      };
      _dicTableMeta[key] = tableMeta;


      var lst = QueryFields2(tableName);
      foreach (var it in lst)
      {
        tableMeta.Fields.AddField(it);
        try
        {
          switch (it.FieldType)
          {
            case eFieldType.eFieldTypeOID:
              tableMeta.OIDFieldName = it.FieldName;
              break;
            case eFieldType.eFieldTypeGeometry:
              tableMeta.ShapeFieldName = it.FieldName;
              tableMeta.ShapeType = it.GeometryType;
              var srid = GetSRID(tableName);
              if (srid > 0)
              {
                                tableMeta.SpatialReference = SpatialReferenceFactory.GetSpatialReference(srid);//.CreateFromEpsgCode(srid);
              }
              break;
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      }
      return tableMeta;
    }
    public IRow FindRow(string tableName, IFields subFields, string wh)
    {
      IRow row = null;
      string fields = null;
      var fieldCount = subFields.FieldCount;
      for (var i = 0; i < fieldCount; ++i)
      {
        var fieldName = subFields.GetField(i).FieldName;
        if (fields == null)
        {
          fields = fieldName;
        }
        else
        {
          fields += "," + fieldName;
        }
      }
      var sql = $"select {fields} from {tableName} where {wh}";
      this.QueryCallback(sql, r =>
       {
         row = new Row(subFields);
         for (int i = 0; i < fieldCount; ++i)
         {
           if (!r.IsDBNull(i))
           {
             var o = r.GetValue(i);
             row.SetValue(i, o);
           }
         }
         return false;
       });
      return row;
    }
    public IRow GetRow(TableMeta tableMeta, int oid)
    {
      return FindRow(tableMeta.TableName, tableMeta.Fields, $"{tableMeta.OIDFieldName}={oid}");
    }
    public void FillEntity(string where, IEntity entity)
    {
      var tableName = EntityUtil.GetTableName(entity);
      var fields = EntityUtil.ConvertToFields(entity.GetType());
      var row = FindRow(tableName, fields, where);
      EntityUtil.WriteToEntity(row, entity);
    }
    public int UpdateRow(TableMeta table, IRow row, Predicate<IField> ignoreFields = null)
    {
      return UpdateRow(table, row, $"{table.OIDFieldName}={row.Oid}", ignoreFields);
    }
    public virtual int UpdateRow(TableMeta table, IRow row, string wh, Predicate<IField> ignoreFields = null)
    {
      throw new NotImplementedException();
    }
    public virtual int InsertRow(TableMeta table, IRow row, Predicate<IField> ignoreFields = null)
    {
      throw new NotImplementedException();
    }
    public virtual IRow CreateRow(TableMeta table)
    {
      throw new NotImplementedException();
    }
    public virtual string QueryPrimaryKey(string tableName)
    {
      System.Diagnostics.Debug.Assert(false, "Not impl for QueryPrimaryKey");
      return null;
    }
    public virtual Dictionary<string, object> QueryTableFieldDefaultValue(string tableName)
    {
      System.Diagnostics.Debug.Assert(false, "Not impl for QueryTableFieldDefaultValue");
      return new Dictionary<string, object>();
    }
    public virtual void BeginTransaction()
    {
      //
    }

    public virtual void Commit()
    {
      //throw new NotImplementedException();
    }
    public virtual void Dispose()
    {
      //throw new NotImplementedException();
    }

    public virtual OkEnvelope GetFullExtent(string sTableName, string shapeFieldName)
    {
      throw new NotImplementedException();
    }

    public virtual int GetNextObjectID(string tableName, int num = 1)
    {
      throw new NotImplementedException();
    }

    public virtual IGeometry GetShape(string tableName, string shapeFieldName, string oidFieldName, int oid)
    {
      throw new NotImplementedException();
    }

    public virtual int GetSRID(string sTableName)
    {
      throw new NotImplementedException();
    }

    public virtual bool IsIndexExist(string tableName, string idxName)
    {
      throw new NotImplementedException();
    }

    public virtual bool IsTableExists(string tableName)
    {
      try
      {
        this.QueryOne($"select count(*) from {tableName} where 1<0");
      }
      catch// (Exception ex)
      {
        //Console.WriteLine(ex);
        return false;
      }
      return true;
    }
    public virtual bool IsFieldExists(string tableName, string fieldName)
    {
      try
      {
        var lst = QueryFields(tableName);
        foreach (var fn in lst)
        {
          if (StringUtil.isEqualIgnorCase(fieldName, fn))
          {
            return true;
          }
        }
      }
      catch
      {
      }
      return false;
    }
    public virtual bool IsTriggerExist(string tableName, string triggerName)
    {
      throw new NotImplementedException();
    }
    public virtual void DropTable(string tableName)
    {
      ExecuteNonQuery("drop table " + tableName);
    }
    public virtual int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
    {
      throw new NotImplementedException();
    }
    public virtual object ExecuteScalar(string sql, IEnumerable<SQLParam> lstParameters = null)
    {
      throw new NotImplementedException();
    }
    public virtual void QueryCallback(string sql, Func<LibCore.IDataReader, bool> callback, ICancelTracker cancel = null)
    {
      throw new NotImplementedException();
    }
    public virtual void QueryCallback(string sql, Action<IDataReader> callback, ICancelTracker cancel = null)
    {
      QueryCallback(sql, r => { callback(r); return true; }, cancel);
    }
    public virtual object QueryOne(string sql)
    {
      object o = null;
      QueryCallback(sql, r =>
      {
        o = r.IsDBNull(0) ? null : r.GetValue(0);
        return false;
      });
      return o;
    }
    public virtual int QueryOneInt(string sql)
    {
      var o = QueryOne(sql);
      if (o is int i)
      {
        return i;
      }

      var n = SafeConvertAux.ToInt32(o);
      return n;
    }
    public List<string> QueryFields(string tableName)
    {
      return QueryFields2(tableName).Select(it => it.FieldName).ToList();
    }

    public virtual List<Field> QueryFields2(string tableName, List<Field> fields = null)
    {
      throw new NotImplementedException();
    }

    public virtual Dictionary<string, Type> QueryFieldsType(string tableName)
    {
      throw new NotImplementedException();
    }

    public virtual eGeometryType QueryShapeType(string tableName, string shapeFieldName)
    {
      throw new NotImplementedException();
    }

    public virtual void Rollback()
    {
      //throw new NotImplementedException();
    }

    public virtual void SqlBulkCopyByDatatable(TableMeta TableName, System.Data.DataTable dt)
    {
      throw new NotImplementedException();
    }

    public virtual void UpdateShape(string tableName, string shapeFieldName, string oidFieldName, int oid, IGeometry g)
    {
      throw new NotImplementedException();
    }

    public virtual string QueryOidFieldName(string tableName)
    {
      throw new NotImplementedException();
    }
    public virtual string QueryShapeFieldName(string tableName)
    {
      throw new NotImplementedException();
    }
  }

  //public class WorkspaceFactory
  //{
  //  public static IWorkspace OpenSqlServerDatabase(string connectionString)
  //  {
  //    var db = new SqlServer();
  //    db.Connect(connectionString);
  //    return db;// new SqlserverWorkspace(db);// connectionString);
  //  }
  //  public static IWorkspace OpenSQLiteDatabase(string fileName)
  //  {
  //    var db = new SQLiteWorkspace();
  //    db.Open(fileName);
  //    return db;
  //  }
  //  public static IWorkspace CreateSQLiteDatabase(string fileName)
  //  {
  //    DBSQLite.CreatNewSQLite(fileName);
  //    return OpenSQLiteDatabase(fileName);
  //  }
  //  public static IWorkspace OpenSpatialiteDatabase(string fileName)
  //  {
  //    var db = new SpatialiteWorkspace();
  //    db.Open(fileName);
  //    return db;
  //  }
  //  /// <summary>
  //  /// yxm 2018-3-12
  //  /// </summary>
  //  /// <param name="connectionString"></param>
  //  /// <returns></returns>
  //  public static IWorkspace OpenOracleDatabase(string connectionString)
  //  {
  //    var db = new OracleWorkspace();
  //    db.Connect(connectionString);
  //    return db;
  //  }
  //}

  public class DBSQLiteDataReader : IDataReader
  {
    private System.Data.IDataReader r;
    public void Attach(System.Data.IDataReader r_)
    {
      r = r_;
    }
    public int FieldCount
    {
      get {
        return r.FieldCount;
      }
    }
    public string GetName(int col)
    {
      return r.GetName(col);
    }
    public DateTime? GetDateTime(int col)
    {
      if (r.IsDBNull(col)) return null;
      return r.GetDateTime(col);
    }

    public object GetValue(int col)
    {
      if (r.IsDBNull(col))
      {
        return null;
      }
      return r.GetValue(col);
    }
    public bool IsDBNull(int col)
    {
      return r.IsDBNull(col);
    }
    public string GetString(int col)
    {
      return r.IsDBNull(col) ? null : r.GetString(col);
    }
    public short GetInt16(int col)
    {
      return r.GetInt16(col);
    }
    public int GetInt32(int col)
    {
      return r.GetInt32(col);
    }
    public byte GetByte(int col)
    {
      return r.GetByte(col);
    }
    public bool GetBoolean(int col)
    {
      return r.GetBoolean(col);
    }
    public double GetDouble(int col)
    {
      return r.GetDouble(col);
    }
    public decimal? GetDecimal(int col)
    {
      if (IsDBNull(col))
      {
        return null;
      }
      return r.GetDecimal(col);
    }
    public object GetRawDataReader()
    {
      return r;
    }

    public IGeometry GetShape(int col)
    {
      if (r.GetValue(col) is byte[] bts)
      {
        return WKBHelper.FromBytes(bts);
      }
      return null;
    }
  }
  //public class SpatialiteWorkspaceBase : WorkspaceBase
  //{
  //  private readonly DBSpatialite _db = new DBSpatialite();
  //  private SQLiteTransaction _trans = null;
  //  public SpatialiteWorkspaceBase()
  //  {
  //  }
  //  public void Open(string strFileName)
  //  {
  //    _db.Open(strFileName);
  //  }
  //  public bool IsOpen()
  //  {
  //    return _db.IsOpen();
  //  }
  //  public DBSpatialite GetRawDB()
  //  {
  //    return _db;
  //  }

  //  public override void QueryObjectClasses(Action<FeatureClassItem> callback)
  //  {
  //    var oi = new FeatureClassItem();
  //    var sql = "select f_table_name,f_geometry_column from geometry_columns";
  //    QueryCallback(sql, r =>
  //    {
  //      var tableName = r.GetString(0);
  //      var shapeFieldName = r.GetString(1);
  //      var geoType = QueryShapeType(tableName, shapeFieldName);
  //      oi.TableName = tableName;
  //      oi.AliasName = tableName;
  //      oi.GeometryType = geoType;
  //      oi.ShapeFieldName = shapeFieldName;
  //      callback(oi);
  //      return true;
  //    });
  //  }
  //  #region IDatabase
  //  public eDatabaseType DatabaseType
  //  {
  //    get {
  //      return eDatabaseType.Spatialite;
  //    }
  //  }
  //  public string ConnectionString
  //  {
  //    get {
  //      return _db.ConnectionString;
  //    }
  //  }

  //  public override void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null, TableIndex[] index = null)
  //  {
  //    IField shpField = null;
  //    var sql = "create table [" + tableName + "](";
  //    for (int i = 0, j = 0; i < fields.FieldCount; ++i)
  //    {
  //      string sType = null;
  //      var field = fields.GetField(i);
  //      if (field.FieldType == eFieldType.eFieldTypeGeometry)
  //      {
  //        //shpFieldName = field.FieldName;
  //        shpField = field;
  //        continue;
  //      }
  //      else if (field.FieldType == eFieldType.eFieldTypeOID)
  //      {
  //        sType = "INTEGER PRIMARY KEY AUTOINCREMENT";
  //      }
  //      else
  //      {
  //        switch (field.FieldType)
  //        {
  //          case eFieldType.eFieldTypeBlob:
  //            sType = "blob (" + field.Length + ")";
  //            break;
  //          case eFieldType.eFieldTypeBool:
  //            sType = "BOOL";
  //            break;
  //          case eFieldType.eFieldTypeDate:
  //            sType = "DATE";
  //            break;
  //          case eFieldType.eFieldTypeDateTime:
  //            sType = "DATETIME";
  //            break;
  //          case eFieldType.eFieldTypeDouble:
  //            sType = "DOUBLE(" + field.Precision + "," + field.Scale + ")";
  //            break;
  //          case eFieldType.eFieldTypeInteger:
  //            sType = "INT";// + field.Length + ")";
  //            if (field.Length > 0)
  //            {
  //              sType += "(" + field.Length + ")";
  //            }
  //            break;
  //          case eFieldType.eFieldTypeSmallInteger:
  //            sType = "SMALLINT";// + field.Length + ")";
  //            if (field.Length > 0)
  //            {
  //              sType += "(" + field.Length + ")";
  //            }
  //            break;
  //          case eFieldType.eFieldTypeSingle:
  //            sType = "FLOAT(" + field.Precision + "," + field.Scale + ")";
  //            break;
  //          case eFieldType.eFieldTypeTime:
  //            sType = "TIME";
  //            break;
  //          case eFieldType.eFieldTypeTimeStamp:
  //            sType = "TIMESTAMP";
  //            break;
  //          default:
  //            System.Diagnostics.Debug.Assert(false, "not impl");
  //            break;
  //        }
  //      }
  //      var s = "[" + field.FieldName + "] " + sType;
  //      if (j == 0)
  //      {
  //        sql += s;
  //      }
  //      else
  //      {
  //        ++j;
  //        sql += "," + s;
  //      }
  //    }
  //    sql += ")";
  //    this.ExecuteNonQuery(sql);

  //    string gType = null;
  //    switch (shpField.GeometryType)
  //    {
  //      case eGeometryType.eGeometryPoint: gType = "POINT"; break;
  //      case eGeometryType.eGeometryPolygon: gType = "POLYGON"; break;
  //      case eGeometryType.eGeometryPolyline: gType = "LINESTRING"; break;
  //      default:
  //        System.Diagnostics.Debug.Assert(false);
  //        break;
  //    }
  //    sql = "SELECT AddGeometryColumn('" + tableName + "','" + shpField.FieldName + "'," + srid + ",'" + gType + "','XY')";
  //    this.ExecuteNonQuery(sql);
  //    _db.CreateSpatialIndex(tableName, shpField.FieldName);
  //    //return OpenFeatureClass(tableName);
  //  }
  //  public override void QueryCallback(string sql, Func<IDataReader, bool> callback, ICancelTracker cancel = null)
  //  {
  //    var ir = new DBSQLiteDataReader();
  //    _db.QueryCallback(sql, r =>
  //    {
  //      ir.Attach(r);
  //      var fContinue = callback(ir);
  //      return fContinue;
  //    }, cancel);
  //  }

  //  public override int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
  //  {
  //    return _db.ExecuteNonQuery(sql, lstParameters);
  //  }

  //  public override bool IsTableExists(string tableName)
  //  {
  //    return _db.IsTableExists(tableName);
  //  }

  //  #region yxm 2018-3-13

  //  //public override List<string> QueryFields(string tableName)
  //  //{
  //  //    return _db.QueryFields(tableName);
  //  //}
  //  public override int GetSRID(string sTableName)
  //  {
  //    return _db.QuerySRID(sTableName);
  //  }
  //  #endregion

  //  public override eGeometryType QueryShapeType(string tableName, string shapeFieldName)
  //  {
  //    return _db.QueryShapeType(tableName, shapeFieldName);
  //  }
  //  public override IGeometry GetShape(string tableName, string shapeFieldName, string oidFieldName, int oid)
  //  {
  //    return _db.GetShape(tableName, shapeFieldName, oid);
  //  }
  //  public override OkEnvelope GetFullExtent(string sTableName, string shapeFieldName)
  //  {
  //    return _db.GetFullExtent(sTableName, shapeFieldName);
  //  }
  //  public override void UpdateShape(string tableName, string shapeFieldName, string oidFieldName, int oid, IGeometry g)
  //  {
  //    var wh = oidFieldName + "=" + oid;
  //    _db.UpdateShape(tableName, shapeFieldName, wh, g);
  //  }

  //  #region 事务相关
  //  public override void BeginTransaction()
  //  {
  //    _trans = _db.BeginTransaction();
  //  }
  //  public override void Commit()
  //  {
  //    _trans?.Commit();
  //  }
  //  public override void Rollback()
  //  {
  //    _trans?.Rollback();
  //  }
  //  #endregion
  //  #endregion


  //  public override string QueryShapeFieldName(string tableName)
  //  {
  //    return _db.QueryShapeFieldName(tableName);
  //  }
  //  public override string QueryOidFieldName(string tableName)
  //  {
  //    return _db.QueryOidFieldName(tableName);
  //  }

  //  public override object ExecuteScalar(string sql, IEnumerable<SQLParam> lstParameters = null)
  //  {
  //    return _db.ExecuteScalar(sql, lstParameters);
  //  }

  //  public override void Dispose()
  //  {
  //    _db.Dispose();
  //  }
  //}

  //public class SpatialiteWorkspace : SpatialiteWorkspaceBase, IWorkspace
  //{

  //}

}
