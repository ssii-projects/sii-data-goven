using Agro.GIS;
using Agro.LibCore.GIS;
using GeoAPI.Geometries;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
    public class DBMySql : WorkspaceBase, IWorkspace
    {
        private class DataReader : IDataReader
        {
            internal MySqlDataReader r;
            private readonly DBMySql p;
            public DataReader(DBMySql p)
            {
                this.p = p;
            }

            public int FieldCount
            {
                get
                {
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
            public object? GetValue(int col)
            {
                if (r.IsDBNull(col))
                {
                    return null;
                }
                var o = r.GetValue(col);
                if (o is MySqlGeometry g)
                {
                    var wkb = g.Value;
                    o = WKBHelper.FromBytes(wkb, g.SRID ?? 0);
                }
                return o;
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
                    var g = WKBHelper.FromBytes(bts);
                    p.ValidShape(g);
                    return g;
                }
                return null;
            }
        }

        protected readonly Dictionary<string, string> _conProperty = new Dictionary<string, string>();
        private MySqlConnection _con = null;
        private MySqlTransaction _sqlTransaction = null;

        private int _dbVersion;
        private readonly SwopXYFilter swopXYFilter = new SwopXYFilter();

        public string ConnectionString
        {
            get; private set;
        }
        public string UserID
        {
            get; private set;
        }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DatabaseName
        {
            get; private set;
        }
        /// <summary>
        ///   The time in seconds to wait for the command to execute. The default is 0 seconds.
        ///   0:
        /// </summary>
        public int CommandTimeout { get; set; } = 0;//0：表示不限制

        private readonly Dictionary<string, int> _dicTableSrid = new Dictionary<string, int>();
        public DBMySql()
        {
            SqlFunc = new MySqlSqlFunction();
        }

        public eDatabaseType DatabaseType
        {
            get
            {
                return eDatabaseType.MySql;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">"Data Source=192.168.2.3;Initial Catalog=YuLinTuCQCJ77;User ID=sa;Password=123456;"</param>
        public void Connect(string connectionString, string sDatabaseName = null)
        {
            Close();
            _conProperty.Clear();
            _con = new MySqlConnection(connectionString);
            ConnectionString = connectionString;
            #region parse connectionString
            var sa = connectionString.Split(';');
            foreach (var s in sa)
            {
                var sa1 = s.Split('=');
                if (sa1.Length == 2)
                {
                    var key = sa1[0].Trim();
                    var val = sa1[1].Trim();
                    _conProperty[key] = val;
                    if (sDatabaseName == null && 
                        (StringUtil.isEqualIgnorCase(key, "database")
                        || StringUtil.isEqualIgnorCase(key, "Initial Catalog")))
                    {
                        sDatabaseName = val;
                    }
                    if (StringUtil.isEqualIgnorCase(key, "uid")
                        || StringUtil.isEqualIgnorCase(key, "User ID"))
                    {
                        this.UserID = val;
                    }
                }
            }
            #endregion

            if (string.IsNullOrEmpty(sDatabaseName))
            {
                sDatabaseName = QueryOne("select Database()")?.ToString()??"";
            }

            DatabaseName = sDatabaseName;

            _con.Open();
            var str = QueryOne("select VERSION()").ToString();
            var dbVersion = SafeConvertAux.ToInt32(str.Split('.')[0]);
            _dbVersion = dbVersion;
            ((MySqlSqlFunction)SqlFunc)._dbVersion = dbVersion;

            try
            {
                ExecuteNonQuery("set global local_infile=1");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public override string DBVersion()
        {
            return QueryOne("select VERSION()").ToString();
        }
        public override void AddField(string tableName, IField field)
        {
            if (field.FieldType != eFieldType.eFieldTypeString)
            {
                base.AddField(tableName, field);
                return;
            }
            var sql = $"ALTER TABLE {tableName} ADD {field.FieldName} nvarchar({field.Length}) comment '{field.AliasName}'";
            ExecuteNonQuery(sql);
        }
        public void Close()
        {
            if (_con != null)
            {
                _con.Close();
                _con = null;
            }
        }

        public override void QueryObjectClasses(Action<FeatureClassItem> callback)
        {
            var oi = new FeatureClassItem();
            //var sql = $"SELECT distinct TABLE_NAME,COLUMN_NAME, DATA_TYPE,COLUMN_COMMENT from information_schema.columns where table_schema = '{DatabaseName}' and DATA_TYPE in ('polygon','linestring','point','geometry')";
            var sql = $"SELECT a.TABLE_NAME,COLUMN_NAME, DATA_TYPE,COLUMN_COMMENT,TABLE_COMMENT from information_schema.columns a LEFT JOIN information_schema.TABLES b on a.TABLE_NAME=b.TABLE_NAME and a.table_schema=b.table_schema  where a.table_schema = '{DatabaseName}' and a.DATA_TYPE in ('polygon','linestring','point','geometry')";
            QueryCallback(sql, r =>
            {
                var tableName = r.GetString(0);
                var alias = SqlUtil.SafeGetString(r, 4) ?? tableName;
                var shapeFieldName = SqlUtil.SafeGetString(r, 1);
                var n = r.GetString(2);
                var geoType = eGeometryType.eGeometryNull;
                if (n == "point")
                {
                    geoType = eGeometryType.eGeometryPoint;
                }
                else if (n == "linestring")
                {
                    geoType = eGeometryType.eGeometryPolyline;
                }
                else if (n == "polygon")
                {
                    geoType = eGeometryType.eGeometryPolygon;
                }
                else if (n == "geometry")
                {
                    if (!r.IsDBNull(3))
                    {
                        var cmt = r.GetString(3);
                        geoType = ParseShapeComment(cmt).GeoType;
                    }
                }
                oi.TableName = tableName;
                oi.AliasName = alias;
                oi.GeometryType = geoType;
                oi.ShapeFieldName = shapeFieldName;
                callback(oi);
                return true;
            });
        }

        public bool IsOpen()
        {
            return _con != null && _con.State == System.Data.ConnectionState.Open; // !string.IsNullOrEmpty(_connectionString);// _con != null && _con.State == System.Data.ConnectionState.Open;
        }
        public override bool IsTableExists(string tableName)
        {
            var sql = $"select count(*) from information_schema.TABLES where TABLE_NAME='{tableName}' and table_schema = '{DatabaseName}'";
            //Console.WriteLine(sql);
            var n = QueryOneInt(sql);// > 0;
            //Console.WriteLine($"retsult is {n}");
            return n > 0;
        }
        public override bool IsIndexExist(string tableName, string idxName)
        {
            var sql = $"select count(*) from information_schema.statistics where TABLE_NAME='{tableName}' and table_schema = '{DatabaseName}' AND index_name = '{idxName}'";
            return QueryOneInt(sql) > 0;
        }
        public override void CreateTable(string tableName, IFields fields, string aliasName = null, TableIndex[] index = null)
        {
            var db = this;
            if (IsTableExists(tableName))
            {
                throw new Exception("表：" + tableName + "已经存在！");
            }

            var sql = BuildCreateFeatureClassSql(tableName, aliasName, fields, eGeometryType.eGeometryNull, 0, index);

            db.ExecuteNonQuery(sql);
        }
        public override void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null, TableIndex[] index = null)
        {
            var db = this;
            if (IsTableExists(tableName))
            {
                throw new Exception("表：" + tableName + "已经存在！");
            }

            var shpField = fields.FindField(it => it.FieldType == eFieldType.eFieldTypeGeometry);
            var geoType = shpField?.GeometryType ?? eGeometryType.eGeometryNull;
            if (geoType == eGeometryType.eGeometryNull)
            {
                throw new Exception($"在创建要素类：{tableName}时，未指定几何对象类型！");
            }
            var sql = BuildCreateFeatureClassSql(tableName, tableAliasName, fields, geoType, srid, index);

            db.ExecuteNonQuery(sql);

            sql = $"CREATE SPATIAL INDEX ix_{tableName}_{shpField.FieldName} ON {tableName}({shpField.FieldName})".ToLower();
            db.ExecuteNonQuery(sql);
            //CreateFeatureClassSpatialIndex(db, tableName, geoType);
        }
        public void UpdateShapeFieldComments(string tableName, string shapeFieldName, int srid, eGeometryType geoType)
        {
            var alias = MySqlShapeFieldComment.MakeComment(geoType, srid);
            var sql = $"alter table {tableName} modify column {shapeFieldName} geometry comment '{alias}'";
            ExecuteNonQuery(sql);
        }
        public override void QueryCallback(string sql, Func<IDataReader, bool> callback, ICancelTracker cancel = null)
        {
            lock (this)
            {
#if DEBUG
                Console.WriteLine(sql);
#endif
                try
                {
                    var ir = new DataReader(this);
                    using (var cmd = new MySqlCommand(sql, _con))
                    {
                        cmd.CommandTimeout = this.CommandTimeout;
                        if (_sqlTransaction != null)
                        {
                            cmd.Transaction = _sqlTransaction;
                        }
                        using (var r = cmd.ExecuteReader())
                        {
                            try
                            {
                                while (r.Read())
                                {
                                    ir.r = r;
                                    var fContinue = callback(ir);
                                    if (!fContinue)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DBMySql QueryCallback[{sql}]:{ex.Message}");
                    throw ex;
                }
            }
        }

        public override void QueryTables(Action<TableItem> callback)
        {
            var sql = $"select TABLE_NAME,TABLE_COMMENT from information_schema.TABLES where table_schema = '{DatabaseName}'";
            QueryCallback(sql, r =>
            {
                var tableName = r.GetString(0);
                var alias = r.IsDBNull(1) ? "" : r.GetString(1);
                var oi = new TableItem
                {
                    TableName = tableName,
                    AliasName = alias,
                };
                callback(oi);
            });
        }

        public override List<Field> QueryFields2(string tableName, List<Field> fields = null)
        {
            if (fields == null)
            {
                fields = new List<Field>();
            }

            var dicFieldAlias = QueryFieldAliasName(tableName);

            Field shpField = null;
            var sql = $"select IS_NULLABLE,COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION,NUMERIC_SCALE,EXTRA,COLUMN_DEFAULT from INFORMATION_SCHEMA.columns WHERE TABLE_SCHEMA='{DatabaseName}' and TABLE_NAME='{tableName}'  order by ORDINAL_POSITION";
            QueryCallback(sql, r =>
            {
                var name = r.GetString(1);
                var type = r.GetString(2).ToUpper();
                var isNullable = r.GetString(0);//=="YES";
                var len = 0;
                try
                {
                    len = r.IsDBNull(3) ? 0 : r.GetInt32(3);
                }
                catch (Exception ex)
                {
                }
                var precision = r.IsDBNull(4) ? 0 : r.GetInt32(4);
                var scale = r.IsDBNull(5) ? 0 : r.GetInt32(5);
                var field = new Field()
                {
                    FieldName = name,
                    AliasName = name,
                    DefaultValue = r.IsDBNull(7) ? null : r.GetValue(7)
                };
                var extra = r.IsDBNull(6) ? "" : r.GetString(6);
                if (dicFieldAlias.TryGetValue(name.ToUpper(), out string fieldAlias))
                {
                    field.AliasName = fieldAlias;
                }
                field.IsNullable = isNullable == "YES";
                field.Length = precision;
                //field.Precision = precision;
                field.Scale = scale;
                switch (type)
                {
                    case "NVARCHAR":
                    case "VARCHAR":
                    case "CHAR":
                    case "NCHAR":
                    case "TEXT":
                        field.FieldType = eFieldType.eFieldTypeString;
                        field.Length = len;
                        break;
                    case "BIGINT":
                        field.FieldType = eFieldType.eFieldTypeBigInt;
                        break;
                    case "DOUBLE":
                    case "NUMERIC":
                    case "DECIMAL":
                        field.FieldType = eFieldType.eFieldTypeDouble;
                        break;
                    case "FLOAT":
                        field.FieldType = eFieldType.eFieldTypeSingle;
                        break;
                    case "INT":
                    case "INTEGER":
                        field.FieldType = eFieldType.eFieldTypeInteger;
                        break;
                    case "TINYINT":
                    case "SMALLINT":
                        field.FieldType = eFieldType.eFieldTypeSmallInteger;
                        break;
                    case "DATETIME":
                    case "DATE":
                    case "DATETIME2":
                        field.FieldType = eFieldType.eFieldTypeDate;
                        break;
                    case "TIMESTAMP":
                        field.FieldType = eFieldType.eFieldTypeTimeStamp;
                        break;
                    case "UNIQUEIDENTIFIER":
                        field.FieldType = eFieldType.eFieldTypeGUID;
                        break;
                    case "GEOMETRY":
                        field.FieldType = eFieldType.eFieldTypeGeometry; break;
                    case "POLYGON":
                        field.FieldType = eFieldType.eFieldTypeGeometry;
                        field.GeometryType = eGeometryType.eGeometryPolygon;
                        break;
                    case "LINESTRING":
                        field.FieldType = eFieldType.eFieldTypeGeometry;
                        field.GeometryType = eGeometryType.eGeometryPolyline;
                        break;
                    case "POINT":
                        //ShapeFieldName = name;
                        field.GeometryType = eGeometryType.eGeometryPoint;
                        field.FieldType = eFieldType.eFieldTypeGeometry;
                        //field.GeometryType = this.ShapeType;
                        //shpField = field;
                        break;
                    case "VARBINARY":
                    case "BINARY":
                    case "LONGBLOB":
                        field.FieldType = eFieldType.eFieldTypeBlob;
                        break;
                    case "XML":
                        field.FieldType = eFieldType.eFieldTypeXML;
                        break;
                    case "BOOLEAN":
                    case "BIT":
                        field.FieldType = eFieldType.eFieldTypeBool;
                        //field.Length = 1;
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }
                if (extra == "auto_increment")
                {
                    field.FieldType = eFieldType.eFieldTypeOID;
                    field.AutoIncrement = true;
                }
                else if (field.FieldType == eFieldType.eFieldTypeGeometry)
                {
                    shpField = field;
                    //field.AliasName = "几何对象";
                }
                if (string.IsNullOrEmpty(field.AliasName))
                {
                    field.AliasName = field.FieldName;
                }
                fields.Add(field);
            });
            if (shpField != null)
            {
                shpField.GeometryType = ParseShapeComment(shpField.AliasName).GeoType;
                shpField.AliasName = "几何对象";
            }

            return fields;
        }

        public override string QueryPrimaryKey(string tableName)
        {
            var sql = $" SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.`KEY_COLUMN_USAGE` WHERE table_name = '{tableName}' AND CONSTRAINT_SCHEMA = '{DatabaseName}' AND constraint_name = 'PRIMARY'";
            var o = QueryOne(sql);
            return o?.ToString();
        }

        /// </summary>  
        ///将DataTable转换为标准的CSV  
        /// </summary>  
        /// <param name="table">数据表</param>  
        /// <returns>返回标准的CSV</returns>  
        private static string DataTableToCsv(DataTable table, TableMeta tableMeta)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。  
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。  
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。  
            var sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in table.Rows)
            {
                for (var i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append(",");
                    var o = row[colum];
                    if (o == null || o == DBNull.Value)
                    {
                        if (colum.DataType == typeof(IGeometry))
                        {
                            var wkt = "GEOMETRYCOLLECTION EMPTY";
                            var str = $"\"{wkt}\"";
                            sb.Append(str);
                        }
                        else
                        {
                            sb.Append("\\N");
                        }
                    }
                    else if (colum.DataType == typeof(byte[]))
                    {
                        var bts = o as byte[];
                        var str = string.Join(string.Empty, bts.Select(it => it.ToString("X2")));
                        sb.Append(str);
                    }
                    else if (colum.DataType == typeof(IGeometry))
                    {
                        string wkt;
                        if (row[colum] is IGeometry g)
                        {
                            if (g.GetSpatialReference() == null && tableMeta.SpatialReference != null)
                            {
                                throw new Exception($"导入{tableMeta.TableName}失败，未能获取输入源的坐标系信息！");
                            }
                            g = g.Project(tableMeta.SpatialReference);
                            wkt = g.AsText();
                        }
                        else
                        {
                            wkt = "GEOMETRYCOLLECTION EMPTY";// : g.AsText();
                        }
                        var str = $"\"{wkt}\"";
                        sb.Append(str);
                    }
                    else if (colum.DataType == typeof(bool))
                    {
                        var b = (bool)o;
                        if (b)
                        {
                            sb.Append(1);
                        }
                        else
                        {//在csv中对于bit(1)类型字段不填任何值表示false，否则都会置为true
                            sb.Append(string.Empty);
                        }
                        //sb.Append(((bool)o) ? 1 : 0);
                        //sb.Append(((bool)o) ? "true" :"false");
                    }
                    else if (colum.DataType == typeof(string))// && row[colum].ToString().Contains(","))
                    {
                        var str = o.ToString().Replace("\\", "\\\\");
                        if (str.Contains(","))
                        {
                            sb.Append("\"" + str.Replace("\"", "\\\"") + "\"");
                        }
                        else if (str.Contains("\r") || str.Contains("\n"))
                        {
                            sb.Append("\"" + str + "\"");
                        }
                        else
                        {
                            sb.Append(str);
                        }
                        //sb.Append("\"" +o.ToString().Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(o.ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// 连接串必须包含：AllowLoadLocalInfile=true;AllowUserVariables=true
        ///ConnectionString like: "server=192.168.1.117;uid=root;pwd=123456;Database=agriegov_cbd;Charset=utf8;AllowLoadLocalInfile=true;AllowUserVariables=true"
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="dt"></param>
        public override void SqlBulkCopyByDatatable(TableMeta tableMeta, DataTable dt)
        {
            var TableName = tableMeta.TableName;
            if (true)
            {
                if (dt.Rows.Count < 1)
                {
                    return;
                }

                var tmpPath = Path.GetTempFileName() + ".csv";

                var csv = DataTableToCsv(dt, tableMeta);
                try
                {
                    File.WriteAllText(tmpPath, csv);
                    var cls = dt.Columns.Cast<DataColumn>();
                    var insertCount = 0;

                    try
                    {
                        var bulk = new MySqlBulkLoader(_con)
                        {
                            Local = true,
                            FieldTerminator = ",",
                            FieldQuotationCharacter = '"',
                            //EscapeCharacter = '"',
                            LineTerminator = "\r\n",
                            FileName = tmpPath,
                            NumberOfLinesToSkip = 0,
                            TableName = dt.TableName,
                        };
                        bulk.Columns.AddRange(cls.Select(col => "@" + col.ColumnName));//根据标题列对应插入

                        bulk.Expressions.AddRange(cls.Select(col => $"{col.ColumnName}=@{col.ColumnName}"));
                        var iShapeField = cls.FindIndex(it => it.DataType == typeof(IGeometry));
                        if (iShapeField >= 0)
                        {
                            var srid = _dbVersion < 8 ? (tableMeta.SpatialReference?.SRID ?? 0) : 0;

                            var expr = $"{tableMeta.ShapeFieldName}=ST_GeomFromText(@{dt.Columns[iShapeField].ColumnName}, {srid}";
                            expr += ")";
                            bulk.Expressions[iShapeField] = expr;
                        }

                        insertCount = bulk.Load();
                    }
                    catch (MySqlException ex)
                    {
                        throw ex;
                    }

                    File.Delete(tmpPath);
                }
                catch (Exception ex)
                {
                    throw ex;
                    //OnLogError("批量插入收集库件级文书档案信息实体（批量）时异常。", ex);
                }


            }
            else
            {
                ConstructInsertSql(dt, TableName, sql =>
                {
                    try
                    {
                        this.ExecuteNonQuery(sql);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(sql);
                        throw ex;
                    }
                });
            }
        }

        private void ConstructInsertSql(DataTable dt, string tableName, Action<string> callback)
        {
            string sFields = null;
            var fields = dt.Columns;

            for (var i = 0; i < fields.Count; ++i)
            {
                var field = fields[i];
                var fieldName = field.ColumnName;
                if (sFields == null)
                {
                    sFields = fieldName;
                }
                else
                {
                    sFields += "," + fieldName;
                }
            }
            var n = 0;
            var sql = $"insert into {tableName}({sFields})values";
            var sb = new StringBuilder();
            foreach (var r in dt.Rows)
            {
                var row = r as DataRow;
                string sValues = null;
                for (var i = 0; i < fields.Count; ++i)
                {
                    var field = fields[i];
                    var fieldName = field.ColumnName;

                    var o = row[i];
                    if (o == null || o == DBNull.Value)
                    {
                        o = "null";
                    }
                    else if (o is IGeometry g)
                    {
                        var srid = g.GetSpatialReference()?.SRID ?? 0;
                        o = SqlFunc.GeomFromText(g);// $"ST_GeomFromText('{g.AsText()}',{srid})";
                    }
                    else if (o is string s)
                    {
                        o = $"'{s.Replace("'", "''")}'";
                    }
                    else if (o is DateTime t)
                    {
                        var str = $"'{t}'";
                        o = str;
                    }
                    if (sValues == null)
                    {
                        sValues = o.ToString();
                    }
                    else
                    {
                        sValues += "," + o;
                    }
                }
                if (sb.Length == 0)
                {
                    sb.Append(sql);
                }
                else
                {
                    sb.Append(",");
                }
                sb.Append($"({sValues})");
                if (++n >= 900)//sb.Length > 170000)
                {
                    n = 0;
                    callback(sb.ToString());
                    sb.Clear();
                }
            }
            if (sb.Length > 0)
            {
                callback(sb.ToString());
            }
        }
        internal void ValidShape(IGeometry g)
        {
            if (_dbVersion >= 8)
            {
                var srid = g.GetSpatialReference()?.SRID ?? 0;
                if (srid == 4326)
                {
                    g.Apply(swopXYFilter);
                    g.GeometryChanged();
                }
            }
        }

        #region Spatial parts
        public override int GetSRID(string sTableName)
        {
            sTableName = sTableName.ToLower();
            if (!_dicTableSrid.TryGetValue(sTableName, out int SRID))
            {
                var sql = $"select COLUMN_COMMENT from information_schema.columns where table_schema = '{DatabaseName}' and table_name = '{sTableName}' and DATA_TYPE in('point','linestring','polygon','geometry')";
                QueryCallback(sql, r =>
                {
                    if (!r.IsDBNull(0))
                    {
                        var s = r.GetString(0);
                        SRID = ParseShapeComment(s).Srid;
                        _dicTableSrid[sTableName] = SRID;
                    }
                });
            }
            return SRID;
        }


        public override eGeometryType QueryShapeType(string tableName, string shapeFieldName)
        {
            var geoType = eGeometryType.eGeometryNull;

            var sql = $"SELECT DATA_TYPE,COLUMN_COMMENT from information_schema.columns where table_schema = '{DatabaseName}' and table_name = '{tableName}' and COLUMN_NAME='{shapeFieldName}'";
            QueryCallback(sql, r =>
            {
                var n = r.GetString(0);
                switch (n)
                {
                    case "point":
                        geoType = eGeometryType.eGeometryPoint; break;
                    case "linestring":
                        geoType = eGeometryType.eGeometryPolyline; break;
                    case "polygon":
                        geoType = eGeometryType.eGeometryPolygon; break;
                    default:
                        {
                            if (!r.IsDBNull(1))
                            {
                                var s = r.GetString(1);
                                geoType = ParseShapeComment(s).GeoType;
                            }
                        }
                        break;
                }
                return false;
            });
            return geoType;
        }
        public override IGeometry GetShape(string tableName, string shapeFieldName, string oidFieldName, int oid)
        {
            IGeometry g = null;
            var srid = GetSRID(tableName);

            var sql = $"select {SqlFunc.AsBinary(shapeFieldName)} from {tableName} where {oidFieldName}={oid}";
            QueryCallback(sql, r =>
            {
                if (r.GetValue(0) is byte[] wkb)
                {
                    g = WKBHelper.FromBytes(wkb);
                    var sr = SpatialReferenceFactory.GetSpatialReference(srid);
                    g.SetSpatialReference(sr);
                    ValidShape(g);
                }
                return false;
            });
            return g;
        }
        public override OkEnvelope GetFullExtent(string sTableName, string shapeFieldName)
        {
            OkEnvelope geo = null;
            try
            {
                var srid = GetSRID(sTableName);
                #region 遍历所有数据
                var sql = $"select ST_AsBinary(st_envelope({shapeFieldName})) from {sTableName} where " + shapeFieldName + " is not null";
                OkEnvelope fe = null;
                QueryCallback(sql, r =>
                {
                    var wkb = r.GetValue(0) as byte[];
                    var g = WKBHelper.FromBytes(wkb);
                    var sr = SpatialReferenceFactory.GetSpatialReference(srid);
                    g.SetSpatialReference(sr);
                    ValidShape(g);
                    if (!g.IsEmpty)
                    {
                        var env = g.EnvelopeInternal;
                        if (fe == null)
                        {
                            fe = new OkEnvelope(env, g.GetSpatialReference());
                        }
                        else
                        {
                            fe.ExpandToInclude(env);
                        }
                    }
                });
                geo = fe;
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return geo;
        }
        public override void UpdateShape(string tableName, string shapeFieldName, string oidFieldName, int oid, IGeometry g)
        {
            var sShp = SqlFunc.GeomFromText(g);
            var sql = "update " + tableName + " set " + shapeFieldName + "=" + sShp + " where " + oidFieldName + "=" + oid;
            ExecuteNonQuery(sql);
        }
        #endregion

        internal static MySqlShapeFieldComment ParseShapeComment(string comment)
        {
            var r = new MySqlShapeFieldComment();
            var gt = eGeometryType.eGeometryNull;
            if (comment != null)
            {
                var sa = comment.Split(';');
                foreach (var str in sa)
                {
                    var sa1 = str.Split(':');
                    if (sa1.Length == 2)
                    {
                        var val = sa1[1].Trim();
                        switch (sa1[0].Trim())// == "srid")
                        {
                            case "srid": r.Srid = SafeConvertAux.ToInt32(val); break;
                            case "gt":
                                switch (val)
                                {
                                    case "polygon": gt = eGeometryType.eGeometryPolygon; break;
                                    case "linestring": gt = eGeometryType.eGeometryPolyline; break;
                                    case "point": gt = eGeometryType.eGeometryPoint; break;
                                }
                                break;
                        }
                    }
                }
            }
            r.GeoType = gt;
            return r;
        }
        /// <summary>
        /// 获取表的oid字段名（针对ArcSDE注册表）
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public override string QueryOidFieldName(string tableName)
        {
            string oidFieldName = null;
            var sql = $"select COLUMN_NAME from information_schema.columns where table_schema = '{DatabaseName}' and table_name = '{tableName}' and EXTRA='auto_increment'";
            QueryCallback(sql, r =>
            {
                oidFieldName = r.GetString(0);
            });
            return oidFieldName;
        }
        public override string QueryShapeFieldName(string tableName)
        {
            string shapeFieldName = null;
            var sql = $"select COLUMN_NAME from information_schema.columns where table_schema = '{DatabaseName}' and table_name = '{tableName}' and DATA_TYPE in('point','linestring','polygon','geometry')";
            QueryCallback(sql, r =>
            {
                shapeFieldName = r.GetString(0);
                return false;
            });
            return shapeFieldName;
        }


        #region yxm 2018-9-25
        public Dictionary<string, string> QueryFieldAliasName(string tableName)
        {
            var dic = new Dictionary<string, string>();
            var sql = $"select COLUMN_NAME,COLUMN_COMMENT from information_schema.columns where table_schema = '{this.DatabaseName}' and table_name = '{tableName}' ";
            try
            {
                this.QueryCallback(sql, r =>
                {
                    var fieldName = r.GetString(0).ToUpper();
                    var fieldAlias = r.IsDBNull(1) ? fieldName : r.GetString(1).Trim();
                    dic[fieldName] = fieldAlias;
                    return true;
                });
            }
            catch { }
            return dic;
        }

        #endregion

        /// <summary>
        /// 返回下一个ObjectID，为当前搜索中最大值的下一个
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>ID</returns> 
        /// <exception cref="Exception">表不存在</exception>
        public override int GetNextObjectID(string tableName, int num = 1)
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            int n = 0;
            lock (this)
            {
                using (var cmd = new MySqlCommand(sql, _con))
                {
                    cmd.CommandTimeout = this.CommandTimeout;
                    if (_sqlTransaction != null)
                    {
                        cmd.Transaction = _sqlTransaction;
                    }
                    if (lstParameters != null)
                    {
                        foreach (SQLParam kv in lstParameters)
                        {
                            cmd.Parameters.Add(new MySqlParameter(kv.ParamName, kv.ParamValue));
                        }
                    }
                    n = cmd.ExecuteNonQuery();
                }
            }
            return n;
        }


        public override object ExecuteScalar(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            using (var cmd = new MySqlCommand(sql, _con))
            {
                cmd.CommandTimeout = this.CommandTimeout;
                if (_sqlTransaction != null)
                {
                    cmd.Transaction = _sqlTransaction;
                }
                if (lstParameters != null)
                {
                    foreach (var kv in lstParameters)
                    {
                        cmd.Parameters.Add(new MySqlParameter(kv.ParamName, kv.ParamValue));
                    }
                }
                var o = cmd.ExecuteScalar();
                return o;
            }
        }

        public override bool IsTriggerExist(string tableName, string triggerName)
        {
            var sql = $"select count(*) from information_schema.`TRIGGERS` where TRIGGER_SCHEMA='{DatabaseName}' and trigger_name='{triggerName}' and EVENT_OBJECT_TABLE='{tableName}'";
            return QueryOneInt(sql) > 0;
        }
        public override Dictionary<string, Type> QueryFieldsType(string tableName)
        {
            var dic = new Dictionary<string, Type>();
            var fields = QueryFields2(tableName);
            foreach (var field in fields)
            {
                Type type = null;
                switch (field.FieldType)
                {
                    case eFieldType.eFieldTypeBlob: type = typeof(byte); break;
                    case eFieldType.eFieldTypeBool: type = typeof(bool); break;
                    case eFieldType.eFieldTypeByte: type = typeof(byte); break;
                    case eFieldType.eFieldTypeTime:
                    case eFieldType.eFieldTypeDate:
                    case eFieldType.eFieldTypeDateTime: type = typeof(DateTime); break;
                    case eFieldType.eFieldTypeDouble: type = typeof(double); break;
                    case eFieldType.eFieldTypeGeometry: type = typeof(IGeometry); break;
                    case eFieldType.eFieldTypeGUID: type = typeof(string); break;
                    case eFieldType.eFieldTypeOID:
                    case eFieldType.eFieldTypeInteger: type = typeof(int); break;
                    case eFieldType.eFieldTypeSingle: type = typeof(float); break;
                    case eFieldType.eFieldTypeSmallInteger: type = typeof(short); break;
                    case eFieldType.eFieldTypeString: type = typeof(string); break;
                }
                dic[field.FieldName] = type;
            }
            return dic;
        }

        public override void BeginTransaction()
        {
            if (_sqlTransaction != null)
            {
                throw new Exception("事务不能嵌套");
            }
            if (_con.State != ConnectionState.Open)
            {
                _con.Open();
            }
            _sqlTransaction = _con.BeginTransaction();
        }
        public override void Commit()
        {
            if (_sqlTransaction != null)
            {
                _sqlTransaction.Commit();
                _sqlTransaction = null;
            }
        }
        public override void Rollback()
        {
            if (_sqlTransaction != null)
            {
                _sqlTransaction.Rollback();
                _sqlTransaction = null;
            }
        }
        public override void Dispose()
        {
            Close();
        }
        public override IRow CreateRow(TableMeta table)
        {
            if (!string.IsNullOrEmpty(table.ShapeFieldName))
            {
                return new MySqlFeature(table.Fields);
            }
            return new Row(table.Fields);
        }
        public override int InsertRow(TableMeta table, IRow row, Predicate<IField> ignoreFields = null)
        {
            var lstParameters = new List<SQLParam>();
            var sql = ConstructInsertSql(table, row, lstParameters, ignoreFields);
            sql += $";SELECT LAST_INSERT_ID()";
            var o = ExecuteScalar(sql, lstParameters);
            row.Oid = SafeConvertAux.ToInt32(o);
            return 1;
        }
        public override int UpdateRow(TableMeta table, IRow row, string wh, Predicate<IField> ignoreFields = null)
        {
            var lstParameters = new List<SQLParam>();
            var sql = ConstructUpdateSql(table, RowUpdateData.GetAllValues(row), lstParameters, wh, ignoreFields);
            return ExecuteNonQuery(sql, lstParameters);
        }
        internal string ConstructUpdateSql(TableMeta table, RowUpdateData row, List<SQLParam> lstParameters, string where, Predicate<IField> ignoreFields = null)
        {
            var sql = $"update {table.TableName} set ";
            var fields = table.Fields;
            string s = null;
            foreach (var kv in row.UpdateValues)
            {
                var i = kv.Key;
                var field = fields.GetField(i);
                if (ignoreFields?.Invoke(field) == true)
                {
                    continue;
                }
                if (!field.Editable)
                {
                    continue;
                }
                var fieldName = field.FieldName;
                if (s == null)
                {
                    s = fieldName;
                }
                else
                {
                    s += "," + fieldName;
                }
                var o = kv.Value;
                if (o == null)
                {
                    s += "=null";
                }
                else if (o is IGeometry g)
                {
                    g.Project(table.SpatialReference);
                    s += $"={SqlFunc.GeomFromText(g)}";
                }
                else
                {
                    s += "=@" + fieldName;
                    var prm = new SQLParam() { ParamName = field.FieldName };
                    prm.ParamValue = o;
                    lstParameters.Add(prm);
                }
            }
            sql += s + " where " + where;
            return sql;
        }
        private string ConstructInsertSql(TableMeta table, IRow row, List<SQLParam> lstParameters, Predicate<IField> ignoreFields = null)
        {
            var strParamPrefix = SqlFunc.GetParamPrefix();
            var strFields = "";
            var strValues = "";
            var fields = table.Fields;
            for (var i = fields.FieldCount; --i >= 0;)
            {
                var field = fields.GetField(i);
                var val = row.GetValue(i);
                if (!field.Editable || val == null || val is DBNull || ignoreFields?.Invoke(field) == true)
                {
                    continue;
                }
                var fieldName = field.FieldName;

                if (strFields.Length > 0)
                {
                    strFields += ",";
                    strValues += ",";
                }

                strFields += fieldName;

                if (val is DateTime dt)
                {
                    strValues += SqlFunc.ToDate(dt);
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
                else if (val is IGeometry g)
                {
                    g.Project(table.SpatialReference);
                    strValues += $"={SqlFunc.GeomFromText(g)}";
                }
                else
                {
                    strValues += strParamPrefix + fieldName;
                    lstParameters.Add(new SQLParam()
                    {
                        ParamName = fieldName,
                        ParamValue = val
                    });
                }
            }

            System.Diagnostics.Debug.Assert(strValues != "");
            var sql = $"INSERT INTO {table.TableName} ({strFields}) VALUES ({strValues})";
            return sql;
        }

        private static string BuildCreateFeatureClassSql(string tableName, string aliasName, IFields Fields, eGeometryType geoType, int srid, TableIndex[] index)
        {
            var sql = $"create table {tableName}(";
            var fields = "";
            var shapeField = "";
            var fHasOidField = false;
            var primaryKey = "";
            for (var i = 0; i < Fields.FieldCount; ++i)
            {
                var fe = Fields.GetField(i);
                if (fields.Length > 0)
                {
                    fields += ",";
                }
                var fieldName = fe.FieldName;
                fields += fieldName + " " + ToFieldTypeString(fe);
                if (fe.DefaultValue != null)
                {
                    var defVal = fe.DefaultValue;
                    if (fe.FieldType == eFieldType.eFieldTypeString)
                    {
                        defVal = $"'{defVal.ToString().Replace("'", "''")}'";
                    }
                    else if (fe.FieldType == eFieldType.eFieldTypeDateTime)
                    {
                        if (defVal is string str && str == "getdate")
                        {
                            defVal = "CURRENT_TIMESTAMP";
                        }
                    }

                    fields += $" DEFAULT {defVal}";
                }
                if (fe.FieldType == eFieldType.eFieldTypeOID || fe.AutoIncrement)
                {
                    fields += " AUTO_INCREMENT";
                    fHasOidField = true;
                }
                else if (!string.IsNullOrEmpty(fe.AliasName))
                {
                    var alias = fe.AliasName;
                    if (fe.FieldType == eFieldType.eFieldTypeGeometry)
                    {
                        alias = MySqlShapeFieldComment.MakeComment(geoType, srid);
                    }
                    if (!StringUtil.isEqualIgnorCase(alias, fe.FieldName))
                    {
                        fields += $" COMMENT '{alias}'";
                    }
                }



                if (fe.PrimaryKey || fe.FieldType == eFieldType.eFieldTypeOID)
                {
                    if (primaryKey.Length > 0)
                    {
                        primaryKey += ",";
                    }
                    primaryKey += $"`{fieldName}`";
                }
            }
            sql += fields;
            if (!string.IsNullOrEmpty(shapeField))
            {
                sql += $",SPATIAL INDEX({shapeField})";
            }
            if (!string.IsNullOrEmpty(primaryKey))
            {
                sql += $",PRIMARY KEY ({primaryKey})";
            }
            if (index != null)
            {
                foreach (var idx in index)
                {
                    switch (idx.IndexType)
                    {
                        case TableIndexType.Unique:
                            {
                                sql += $",UNIQUE KEY `{idx.IndexName}` ({idx.IndexFields})";
                            }
                            break;
                    }
                }
            }
            sql += ") CHARSET=utf8";
            if (fHasOidField)
            {
                sql += " AUTO_INCREMENT = 1";
            }
            if (!string.IsNullOrEmpty(aliasName))
            {
                sql += $" COMMENT='{aliasName}'";
            }
            return sql;
        }

        private static string ToFieldTypeString(IField fe)
        {
            var strFieldType = "";
            if (fe.FieldType == eFieldType.eFieldTypeGeometry)
            {
                strFieldType = "geometry";
                //switch (fe.GeometryType)
                //{
                //	case eGeometryType.eGeometryPolygon: strFieldType = "polygon";break;
                //	case eGeometryType.eGeometryPoint: strFieldType = "point"; break;
                //	case eGeometryType.eGeometryPolyline: strFieldType = "linestring"; break;
                //}
            }
            else
            {
                var DEF_PRECISITION = 17;
                var DEF_SCALE = 8;
                var MAX_VARCHAR_LEN = 1000;

                switch (fe.FieldType)
                {
                    case eFieldType.eFieldTypeOID:
                    case eFieldType.eFieldTypeInteger:
                        strFieldType += "int";
                        break;
                    case eFieldType.eFieldTypeSmallInteger:
                        strFieldType += "smallint";
                        break;
                    case eFieldType.eFieldTypeDouble:
                    case eFieldType.eFieldTypeSingle:
                        var p = fe.Precision > 0 ? fe.Precision : DEF_PRECISITION;
                        var s = fe.Precision > 0 ? fe.Scale : DEF_SCALE;
                        strFieldType += "decimal(" + p + "," + s + ")";
                        break;
                    case eFieldType.eFieldTypeString:
                        var len = fe.Length > MAX_VARCHAR_LEN ? MAX_VARCHAR_LEN : fe.Length;
                        strFieldType += "nvarchar(" + len + ")";
                        break;
                    case eFieldType.eFieldTypeBlob:
                        strFieldType += "binary";
                        break;
                    case eFieldType.eFieldTypeDate:
                        strFieldType += "date";
                        break;
                    case eFieldType.eFieldTypeDateTime:
                        strFieldType += "datetime";
                        break;
                    case eFieldType.eFieldTypeTimeStamp:
                        strFieldType += "timestamp";
                        break;
                    case eFieldType.eFieldTypeBool:
                        strFieldType += "tinyint";
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false, "不支持的字段类型：" + fe.FieldType);
                        break;
                }
            }
            if (!fe.IsNullable)
            {
                strFieldType += " NOT NULL";
            }
            return strFieldType;
        }
    }

    internal class MySqlSqlFunction : ISqlFunction
    {
        internal int _dbVersion;
        public MySqlSqlFunction()
        {
        }
        #region ISqlFunction 默认针对Access的实现.
        /// <summary>
        /// IntDate年
        /// </summary> 
        /// <param name="intDateField">时间字符串</param>
        /// <returns>年</returns>
        public string IntDate_Year(string intDateField)
        {
            return " " + intDateField + "/10000 ";
        }
        /// <summary>
        /// IntDate月
        /// </summary> 
        /// <param name="intDateField">时间字符串</param>
        /// <returns>月</returns>
        public string IntDate_Month(string intDateField)
        {
            return " (" + intDateField + "%10000)/100 ";
        }
        /// <summary>
        /// 取用double表示日期的年的函数
        /// </summary>
        /// <param name="doubleDateField">doubleDateField字符串</param>
        /// <returns></returns>
        public string DoubleDate_Year(string doubleDateField)
        {
            return " " + doubleDateField + "/10000000000 ";
            // " Int(" + doubleDateField + "/10000000000)";
        }
        /// <summary>
        /// 取用double表示日期的月的函数
        /// </summary>
        /// <param name="doubleDateField">doubleDateField字符串</param>
        /// <returns>月</returns>
        public string DoubleDate_Month(string doubleDateField)
        {
            return " (" + doubleDateField + "%10000000000)/100000000 ";
            //" Int((" + doubleDateField + " mod 10000000000)/100000000)";
        }
        /// <summary>
        /// 转换为年度函数
        /// </summary> 
        /// <param name="dateTimeField">时间字符串</param>
        /// <returns>年</returns>
        public string Year(string dateTimeField)
        {
            return " year(" + dateTimeField + ")";
        }


        /// <summary>
        /// 转换为月份函数
        /// </summary> 
        /// <param name="dateTimeField">时间字符串</param>
        /// <returns>月</returns>
        public string Month(string dateTimeField)
        {
            return " month(" + dateTimeField + ") ";
        }
        /// <summary>
        /// 转换为日函数
        /// </summary> 
        /// <param name="dateTimeField">时间字符串</param>
        /// <returns>日</returns>
        public string Day(string dateTimeField)
        {
            return " day(" + dateTimeField + ")";
        }
        /// <summary>
        /// DateTime转换DateTime去掉‘-’
        /// </summary> 
        /// <param name="dateTime">时间</param>
        /// <returns>格式：YYYYMMDDHHMMSS</returns>
        public string ToDate(DateTime dateTime)
        {
            var str = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $" CONVERT('{str}',DATETIME)";
        }
        /// <summary>
        /// 返回与指定数值表达式对应的字符
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns>字符</returns>
        public string ToStr(string field)
        {
            return " str(" + field + ")";
        }
        public string ToDouble(string expr, int precisition = 38, int scale = 15)
        {
            return $" Cast ({expr} as decimal({precisition},{scale}))";
        }
        /// <summary>
        /// 返回文本字段中值的长度
        /// </summary>
        /// <param name="field">字段</param>
        /// <returns>文本字段中值的长度</returns>
        public string StrLen(string field)
        {
            return " len(" + field + ")";
        }
        /// <summary>
        /// 返回字符串在另一个字符串中的起始位置 ， subStr是要到field中寻找的字符中
        /// </summary>
        /// <param name="field">目标字段</param>
        /// <param name="subStr">处理字段</param>
        /// <returns>subStr在field中的位置</returns> 
        public string StrPos(string field, string subStr)
        {
            return " CHARINDEX(" + subStr + "," + field + ")";
        }
        /// <summary>
        /// 一个整数转换为字符串，使用LTrim函数来清除空白。
        /// </summary>
        /// <param name="strField">字符串</param>
        /// <returns>去掉空格的字符串</returns>
        public string Trim(string strField)
        {
            return " ltrim(rtrim(" + strField + "))";
        }
        /// <summary>
        /// sql语句拼接
        /// </summary>
        /// <returns>返回通配符“+”</returns>
        public string ConcactOperator()
        {
            return "+";
        }
        ///// <summary>
        ///// 截取字符串通配符
        ///// </summary>
        ///// <returns>返回截取字符串通配符SUBSTRING</returns>
        //public string SubStringCastOperator()
        //{
        //    return "SUBSTRING";
        //}

        public string SubString(string expr, int iStartPos, int len)
        {
            return "SUBSTRING(" + expr + "," + iStartPos + "," + len + ")";
        }

        /// <summary>
        /// 进行模糊查询
        /// </summary>
        /// <returns>模糊查询通配符</returns>
        public string LikeCastOperator()
        {
            return "%";
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
            return " case when (" + condition + ") then (" + trueSql + ") else (" + falseSql + ") end ";
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
            foreach (var kv in dicWhen)
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
        /// <returns>当前时间</returns>
        public string GetServerCurrentTime()
        {
            return "SELECT getdate()";
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

        public char GetParamPrefix()
        {
            return '@';
        }
        public string AsBinary(string shapeField)
        {
            return "ST_AsBinary(" + shapeField + ") as " + shapeField;
        }

        public string GeomFromText(IGeometry g)
        {
            var srid = _dbVersion < 8 ? (g.GetSpatialReference()?.SRID ?? 0) : 0;
            return $"ST_GeomFromText('{g.AsText()}',{srid})";
        }
        #endregion ISqlFunction
    }


    internal class MySqlShapeFieldComment
    {
        public int Srid = 0;
        public eGeometryType GeoType = eGeometryType.eGeometryNull;
        public static string MakeComment(eGeometryType geoType, int srid)
        {
            var sGeoType = "";
            switch (geoType)
            {
                case eGeometryType.eGeometryPolygon: sGeoType = "polygon"; break;
                case eGeometryType.eGeometryPoint: sGeoType = "point"; break;
                case eGeometryType.eGeometryPolyline: sGeoType = "linestring"; break;
            }
            return $"gt:{sGeoType};srid:{srid}";
        }
    }
}

