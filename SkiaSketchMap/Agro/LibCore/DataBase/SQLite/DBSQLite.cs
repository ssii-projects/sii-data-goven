using Agro.GIS;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Data;
using System.Data.SQLite;
using JTSGeometry = GeoAPI.Geometries.IGeometry;
using WKBPolygon = NetTopologySuite.Geometries.Polygon;
using WKBCoordinate = GeoAPI.Geometries.Coordinate;
using IWKBLinearRing = GeoAPI.Geometries.ILinearRing;

namespace Agro.LibCore
{
    /// <summary>
    /// Sqlite数据库访问辅助类
    /// </summary>
    ///<remark>
    /// 编号        作者        日期                    操作
    /// 001         颜学铭     2010-01-05 16:27   编写源码
    ///</remark>
    public class DBSQLite : IDisposable
    {
        private class SqlFunction : ISqlFunction
        {
            public string GeomFromText(IGeometry g)
            {
                throw new NotImplementedException();
            }
            public string AsBinary(string shapeField)
            {
                throw new NotImplementedException();
            }

            public string CaseWhen(string caseSql, Dictionary<string, string> dicWhen, string elseSql)
            {
                throw new NotImplementedException();
            }

            public string ConcactOperator()
            {
                throw new NotImplementedException();
            }

            public string DoubleDate_Month(string doubleDateField)
            {
                throw new NotImplementedException();
            }

            public string DoubleDate_Year(string doubleDateField)
            {
                throw new NotImplementedException();
            }

            public char GetParamPrefix()
            {
                return '@';
            }

            public string GetServerCurrentTime()
            {
                throw new NotImplementedException();
            }

            public string if_else(string condition, string trueSql, string falseSql)
            {
                throw new NotImplementedException();
            }

            public string IntDate_Month(string intDateField)
            {
                throw new NotImplementedException();
            }

            public string IntDate_Year(string intDateField)
            {
                throw new NotImplementedException();
            }

            public string LikeCastOperator()
            {
                throw new NotImplementedException();
            }

            public string Round(string fieldExpr, int n)
            {
                throw new NotImplementedException();
            }

            public string StrLen(string field)
            {
                throw new NotImplementedException();
            }

            public string StrPos(string field, string subStr)
            {
                throw new NotImplementedException();
            }

            public string SubString(string expr, int iStartPos, int len)
            {
                throw new NotImplementedException();
            }

            public string ToDate(DateTime dateTime)
            {
                var str = dateTime.ToString("yyyy-MM-dd");
                if (dateTime.Hour != 0 || dateTime.Minute != 0 || dateTime.Second != 0)
                {
                    str += dateTime.ToString(" HH:mm:ss");
                }
                return $"'{str}'";
            }

            public string ToDouble(string expr, int precisition = 38, int scale = 15)
            {
                throw new NotImplementedException();
            }

            public string ToStr(string field)
            {
                throw new NotImplementedException();
            }

            public string Trim(string strField)
            {
                throw new NotImplementedException();
            }

            public string Year(string expr)
            {
                throw new NotImplementedException();
            }
        }
        public ISqlFunction SqlFunc { get; } = new SqlFunction();

        public const int DEF_PRECISITION = 17;
        public const int DEF_SCALE = 8;
        public const int MAX_VARCHAR_LEN = 1000;

        #region Private Fields
        protected SQLiteConnection _con;

        #endregion

        #region Public Properties And Methods
        /// <summary>
        /// 创建一个新的SQLite数据库
        /// </summary>
        /// <param name="SQLiteName">传入的数据库名称，如：c:/test.db</param>
        public static void CreatNewSQLite(string SQLiteName)
        {
            SQLiteConnection.CreateFile(SQLiteName);
        }
        public string ConnectionString
        {
            get;
            private set;
        } = "";
        /// <summary>
        /// 建立数据库连接
        /// 连接串格式示例：
        ///     strFileName:@"e:\test.db3"
        /// </summary>
        /// <param name="strFileName"></param>
        public virtual void Open(string strFileName)
        {
            Close();
            ConnectionString = strFileName;
            _con = new SQLiteConnection("Data Source=" + strFileName);//"Data Source=e:\\test.db3"
            _con.Open();
        }

        public void QueryObjectClasses(Action<FeatureClassItem> callback)
        {
            var oi = new FeatureClassItem();
            if (IsTableExists("ime_ObjectClasses"))
            {
                var sql = "select Name,AliasName,ShapeFieldName,GeometryType from ime_ObjectClasses where Name is not null";
                QueryCallback(sql, r =>
                {
                    var tableName = r.GetString(0);
                    var alias = SqlUtil.SafeGetString(r, 1);
                    var shapeFieldName = SqlUtil.SafeGetString(r, 2);
                    var n = SqlUtil.SafeGetShort(r, 3);
                    var geoType = eGeometryType.eGeometryNull;
                    if (n != null)
                    {
                        geoType = (eGeometryType)n;
                    }
                    oi.TableName = tableName;
                    oi.AliasName = string.IsNullOrEmpty(alias) ? tableName : alias;
                    oi.GeometryType = geoType;
                    oi.ShapeFieldName = shapeFieldName;
                    callback(oi);
                    return true;
                });
            }
        }

        public bool IsOpen()
        {
            return _con != null;
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {
            //_con.
            Dispose();
        }
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <returns></returns>
        public SQLiteTransaction BeginTransaction()
        {
            return _con.BeginTransaction();
        }

        public void CreateTable(string tableName, IFields fields, string aliasName = null)
        {
            if (IsTableExists(tableName))
            {
                throw new Exception("表：" + tableName + "已经存在！");
            }

            var sql = BuildCreateTableSql(tableName, fields);

            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// insert示例：
        ///     sql:insert into Book values(@ID,@BookName,@Price);
        ///     lstParameters:{["ID",1]、["BookName","语文"]、["Price",35]}
        /// update示例：
        ///     sql:update Book set BookName=@BookName,Price=@Price where ID=@ID;"
        ///     lstParameters:{["ID",1]、["BookName","语文"]、["Price",35]}
        /// delete示例：
        ///     sql:delete from Book where ID=@ID;
        ///     lstParameters:{["ID",1]}
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="lstParameters"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            lock (this)
            {
                var cmd = _con.CreateCommand();

                cmd.CommandText = sql;
                if (lstParameters != null)
                {
                    foreach (var kv in lstParameters)
                    {
                        cmd.Parameters.Add(new SQLiteParameter(kv.ParamName, kv.ParamValue));
                    }
                }
                int i = cmd.ExecuteNonQuery();
                return i;
            }
        }
        public object ExecuteScalar(string sql, IEnumerable<SQLParam> lstParameters)
        {
            lock (this)
            {
                var cmd = _con.CreateCommand();
                cmd.CommandText = sql;// "insert into Book values(@ID,@BookName,@Price);";
                if (lstParameters != null)
                {
                    foreach (var kv in lstParameters)
                    {
                        cmd.Parameters.Add(new SQLiteParameter(kv.ParamName, kv.ParamValue));
                    }
                }
                object o = cmd.ExecuteScalar();
                return o;
            }
        }
        public SQLiteCommand CreateCommand(string sql)
        {
            var cmd = _con.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = _con;
            return cmd;
        }
        /// <summary>
        /// select示例：
        ///     sql:"select * from Book where ID=@ID;";
        ///     lstParameters:{["ID",1]}
        /// 结果集读取示例：
        ///       while(dr.Read())
        ///       {
        ///           Book book = new Book();
        ///           book.ID = dr.GetInt32(0);
        ///           book.BookName = dr.GetString(1);
        ///           book.Price = dr.GetDecimal(2);
        ///       }
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="lstParameters"></param>
        /// <returns></returns>
        public SQLiteDataReader Query(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            //_con.Open();

            SQLiteCommand cmd = _con.CreateCommand();

            cmd.CommandText = sql;// "select * from Book where ID=@ID;";
            if (lstParameters != null)
            {
                foreach (SQLParam kv in lstParameters)
                {
                    cmd.Parameters.Add(new SQLiteParameter(kv.ParamName, kv.ParamValue));//cmd.Parameters.Add(new SQLiteParameter("ID", ID));
                }
            }
            SQLiteDataReader dr = cmd.ExecuteReader();
            return dr;
        }

        public void QueryCallback(string sql, Func<SQLiteDataReader, bool> callback, ICancelTracker cancel = null)
        {
            lock (this)
            {
                using (var r = Query(sql))
                {
                    while (r.Read())
                    {
                        if (cancel != null && cancel.Cancel())
                        {
                            break;
                        }
                        var fContinue = callback(r);
                        if (!fContinue)
                            break;
                    }
                    r.Close();
                }
            }
        }
        public object QueryOne(string sql)
        {
            object o = null;
            QueryCallback(sql, r =>
            {
                o = r.IsDBNull(0) ? null : r.GetValue(0);
                return false;
            });
            return o;
        }
        public int QueryOneInt(string sql)
        {
            var o = QueryOne(sql);
            var n = SafeConvertAux.ToInt32(o);
            return n;
        }

        public bool IsTableExists(string tableName)
        {
            int n = QueryOneInt("select count(*) from sqlite_master where type='table' and lower(tbl_name)='" + tableName.ToLower() + "'");
            return n == 1;
        }
        public bool IsFieldExist(string tableName, string fieldName)
        {
            if (IsTableExists(tableName))
            {
                try
                {
                    var sql = "select " + fieldName + " from " + tableName + " where 1<0";
                    Query(sql);
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }
        public string QueryPrimaryKey(string tableName)
        {
            string priKey = null;
            var sql = $"select * from {tableName} where 1<0";
            using (var r = Query(sql))
            {
                var t = r.GetSchemaTable();
                foreach (DataRow dr in t.Rows)
                {
                    var fieldName = dr["ColumnName"].ToString();
                    var isKey = dr["IsKey"].ToString();
                    if (isKey == "True")
                    {
                        if (priKey == null)
                        {
                            priKey = fieldName;
                        }
                        else
                        {
                            priKey += $",{fieldName}";
                        }
                    }
                }
            }
            return priKey;
        }
        public virtual List<Field> QueryFields2(string tableName, List<Field> fields = null)
        {
            if (fields == null)
            {
                fields = new List<Field>();
            }
            var sql = $"select * from {tableName} where 1<0";
            using (var r = Query(sql))
            {
                var t = r.GetSchemaTable();
                foreach (DataRow dr in t.Rows)
                {
                    var field = new Field()
                    {
                        FieldName = dr["ColumnName"].ToString(),
                    };
                    var fieldType = eFieldType.eFieldTypeNull;
                    var dataTypeName = dr["DataTypeName"].ToString().ToUpper();
                    var nNumericPrecision = SafeConvertAux.ToInt32(dr["NumericPrecision"]);
                    var nNumericScale = SafeConvertAux.ToInt32(dr["NumericScale"]);
                    switch (dataTypeName)
                    {
                        case "VARCHAR":
                        case "NVARCHAR":
                        case "CHAR":
                        case "NCHAR":
                        case "TEXT":
                        case "NTEXT":
                            fieldType = eFieldType.eFieldTypeString;
                            field.Length = SafeConvertAux.ToInt32(dr["ColumnSize"]);
                            break;
                        case "BOOL":
                            fieldType = eFieldType.eFieldTypeBool;
                            break;
                        case "INT":
                        case "INTERGER":
                        case "INTEGER":
                            fieldType = eFieldType.eFieldTypeInteger;
                            break;
                        case "REAL":
                        case "DOUBLE":
                        case "DECIMAL":
                            fieldType = eFieldType.eFieldTypeDouble;
                            field.Length = nNumericPrecision;
                            field.Scale = nNumericScale;
                            break;
                        case "DATE":
                            fieldType = eFieldType.eFieldTypeDate;
                            break;
                        case "DATETIME":
                            fieldType = eFieldType.eFieldTypeDateTime;
                            break;
                        case "BLOB":
                            fieldType = eFieldType.eFieldTypeBlob;
                            break;
                        default:
                            System.Diagnostics.Debug.Assert(false, $"Not imple for {dataTypeName}");
                            throw new Exception($"Not imple for {dataTypeName}");
                    }
                    field.FieldType = fieldType;
                    fields.Add(field);
                }
            }

            {//yxm 2017/6/18
                sql = $"select rowid from {tableName} where 1<0";
                using (var r = Query(sql))
                {
                    var oidFieldName = r.GetName(0);
                    var oidField = fields.Find(it => StringUtil.isEqualIgnorCase(it.FieldName, oidFieldName));
                    if (oidField == null)
                    {
                        oidField = FieldsUtil.CreateOIDField(oidFieldName) as Field;
                        fields.Insert(0, oidField);
                    }
                    else
                    {
                        oidField.FieldType = eFieldType.eFieldTypeOID;
                    }
                }
            }

            return fields;
        }
        ///// <summary>
        ///// 获取表的字段名
        ///// </summary>
        ///// <param name="tableName"></param>
        ///// <returns></returns>
        //public List<string> QueryFields(string tableName)
        //{
        //    return QueryFields2(tableName).Select(it => it.FieldName).ToList();
        //}

        public int Delete(string tableName, string where = null)
        {
            var sql = "delete from " + tableName;
            if (!string.IsNullOrEmpty(where))
            {
                sql += " where " + where;
            }
            return ExecuteNonQuery(sql);
        }

        public int GetLastInsertRowID(string tableName)
        {
            var sql = "select last_insert_rowid() from " + tableName;
            return QueryOneInt(sql);
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int GetMaxRowID(string tableName)
        {
            var sql = "select max(rowid)+1 from " + tableName;// "select last_insert_rowid() from " + tableName;
            int n = QueryOneInt(sql);
            //sql = "select last_insert_rowid()+1 from " + tableName;
            //int n1 = QueryOneInt(sql);
            //return Math.Max(n,n1);
            return n;
            //SQLiteDataReader r = ws.query("select last_insert_rowid() from ime_featureDataset");
            //if (r.Read())
            //{
            //    return r.GetInt32(0);
            //}
            //return 0;
        }



        #region IDisposable
        public void Dispose()
        {
            if (_con != null && !string.IsNullOrEmpty(_con.ConnectionString))
            {
                _con.Dispose();
                _con = null;
            }
        }
        #endregion
        #endregion

        private string BuildCreateTableSql(string tableName, IFields Fields)//, string aliasName=null)
        {
            var sql = "create table " + tableName + "(";
            var fields = "";

            for (var i = 0; i < Fields.FieldCount; ++i)
            {
                var fe = Fields.GetField(i);
                if (!fe.Editable)
                {
                    continue;
                }

                if (fe.FieldType == eFieldType.eFieldTypeOID
                    || fe.FieldType == eFieldType.eFieldTypeGeometry
                    || IsSpatialFieldName(fe.FieldName))
                {
                    continue;
                }
                if (fields.Length > 0)
                {
                    fields += ",";
                }
                var fieldName = fe.FieldName;
                fields += fieldName + " " + ToFieldTypeString(fe);
            }
            sql += fields;
            sql += ")";
            return sql;
        }
        protected string ToFieldTypeString(IField fe)
        {
            var strFieldType = "";
            switch (fe.FieldType)
            {
                case eFieldType.eFieldTypeInteger:
                    strFieldType += "interger";
                    break;
                case eFieldType.eFieldTypeSmallInteger:
                    strFieldType += "smallint";
                    break;
                case eFieldType.eFieldTypeDouble:
                case eFieldType.eFieldTypeSingle:
                    int p = fe.Precision > 0 ? fe.Precision : DEF_PRECISITION;
                    int s = fe.Precision > 0 ? fe.Scale : DEF_SCALE;
                    strFieldType += "decimal(" + p + "," + s + ")";
                    break;
                case eFieldType.eFieldTypeString:
                    int len = fe.Length > MAX_VARCHAR_LEN ? MAX_VARCHAR_LEN : fe.Length;
                    strFieldType += "nvarchar(" + len + ")";
                    break;
                case eFieldType.eFieldTypeBlob:
                    strFieldType += "BLOB";
                    break;
                case eFieldType.eFieldTypeDate:
                    strFieldType += "Date";
                    break;
                case eFieldType.eFieldTypeDateTime:
                    strFieldType += "DateTime";
                    break;
                case eFieldType.eFieldTypeBool:
                    strFieldType += "BOOL";
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "不支持的字段类型：" + fe.FieldType);
                    break;
            }
            if (!fe.IsNullable)
            {
                strFieldType += " NOT NULL";
            }
            if (fe.PrimaryKey)
            {
                strFieldType += " PRIMARY KEY ASC";
            }
            return strFieldType;
        }
        protected virtual bool IsSpatialFieldName(string fieldName)
        {
            if (StringUtil.isEqualIgnorCase(fieldName, "rowid"))
            {
                return true;
            }
            return false;
        }
    }

    public class GeoDBSqlite : DBSQLite
    {
        public class MyWKBHelper
        {
            public struct MyIntPoint
            {
                public int x;
                public int y;
            }
            private static object _missing = Type.Missing;
            public class WKBConstants
            {
                public const int wkbXDR = 0;
                public const int wkbNDR = 1;

                public const int wkbPoint = 1;
                public const int wkbLineString = 2;
                public const int wkbPolygon = 3;
                public const int wkbMultiPoint = 4;
                public const int wkbMultiLineString = 5;
                public const int wkbMultiPolygon = 6;
                public const int wkbGeometryCollection = 7;
            }
            public class MyWKBReader
            {
                private static readonly String INVALID_GEOM_TYPE_MSG = "Invalid geometry type encountered in ";
                private static WKBReader _jtsReader = new WKBReader();
                //private ByteOrderDataInStream dis = new ByteOrderDataInStream();
                private static GeometryFactory factory = new GeometryFactory();
                public static JTSGeometry read(byte[] bytes)
                {
                    int b = bytes[0] & 0xff;
                    if (b == WKBConstants.wkbXDR// ByteOrderValues.LITTLE_ENDIAN
                            || b == WKBConstants.wkbNDR)//ByteOrderValues.BIG_ENDIAN)
                        return _jtsReader.Read(bytes);
                    else if ((b & (7 << 5)) == 0x80)
                    {
                        try
                        {
                            return read(new BinaryReader(new MemoryStream(bytes)));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Unexpected IOException caught: " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unexpected IOException");
                        //throw new Exception("Unexpected IOException");
                    }
                    return null;
                }
                private static JTSGeometry read(BinaryReader reader)//throws IOException, ParseException
                {
                    //dis.setInStream(stream);
                    //int byteOrder = ByteOrderValues.LITTLE_ENDIAN;
                    //dis.setOrder(byteOrder);
                    //BinaryReader reader = new BinaryReader(stream);
                    Geometry g = readGeometry(reader);
                    return g;
                }
                private static Geometry readGeometry(BinaryReader dis)
                {
                    Geometry geom = null;
                    int b = dis.ReadByte() & 0xff;
                    int geometryType = 31 & b;
                    switch (geometryType)
                    {
                        case WKBConstants.wkbPolygon:
                            geom = readPolygon(dis);
                            break;

                        case WKBConstants.wkbMultiPolygon:
                            geom = readMultiPolygon(dis);
                            break;
                        default:
                            throw new Exception("暂不支持类型为：" + geometryType + "的几何类型的读取！");
                    }
                    return geom;
                }
                private static WKBPolygon readPolygon(BinaryReader dis)
                {
                    int numRings = readUShort(dis);
                    LinearRing[] holes = null;
                    if (numRings > 1)
                        holes = new LinearRing[numRings - 1];

                    LinearRing shell = readLinearRing(dis);
                    for (int i = 0; i < numRings - 1; i++)
                    {
                        holes[i] = readLinearRing(dis);
                    }
                    return (WKBPolygon)factory.CreatePolygon(shell, holes);
                }
                private static MultiPolygon readMultiPolygon(BinaryReader dis)
                {
                    int numGeom = readUShort(dis);// dis.readInt();
                    WKBPolygon[] geoms = new WKBPolygon[numGeom];
                    for (int i = 0; i < numGeom; i++)
                    {
                        Geometry g = readGeometry(dis);
                        if (!(g is WKBPolygon))
                            throw new GeoAPI.IO.ParseException(INVALID_GEOM_TYPE_MSG + "MultiPolygon");
                        geoms[i] = (WKBPolygon)g;
                    }
                    return (MultiPolygon)factory.CreateMultiPolygon(geoms);
                }
                private static LinearRing readLinearRing(BinaryReader dis)
                {
                    int size = dis.ReadInt32();
                    int minX = dis.ReadInt32();
                    int minY = dis.ReadInt32();
                    int xnb = (int)(((minX & 0xC0000000) >> 30) + 1);
                    int ynb = (int)(((minY & 0xC0000000) >> 30) + 1);
                    minX = minX & 0x3FFFFFFF;
                    minY = minY & 0x3FFFFFFF;
                    //	    if(size>10000){
                    //	    	throw new Exception("error size");
                    //	    }
                    WKBCoordinate[] coords = new WKBCoordinate[size + 1];
                    for (int i = 0; i < size; ++i)
                    {
                        coords[i] = new WKBCoordinate();
                        if (xnb == 1)
                        {
                            int b = dis.ReadByte() & 0xff;
                            double x = minX + b;
                            coords[i].X = x / 1E6;
                        }
                        else if (xnb == 2)
                        {
                            double x = minX + readUShort(dis);
                            coords[i].X = x / 1E6;
                        }
                        else if (xnb == 3)
                        {
                            double x = minX + read3byte(dis);
                            coords[i].X = x / 1E6;
                        }
                        else if (xnb == 4)
                        {
                            double x = dis.ReadInt32();
                            coords[i].X = x / 1E6;
                        }
                        else
                        {
                            throw new Exception("压缩错误！");
                        }
                    }
                    for (int i = 0; i < size; ++i)
                    {
                        if (ynb == 1)
                        {
                            int b = dis.ReadByte() & 0xff;
                            double y = minY + b;
                            coords[i].Y = y / 1E6;
                        }
                        else if (ynb == 2)
                        {
                            double y = minY + readUShort(dis);
                            coords[i].Y = y / 1E6;
                        }
                        else if (ynb == 3)
                        {
                            double y = minY + read3byte(dis);
                            coords[i].Y = y / 1E6;
                        }
                        else if (ynb == 4)
                        {
                            double y = dis.ReadInt32();
                            coords[i].Y = y / 1E6;
                        }
                        else
                        {
                            throw new Exception("压缩错误！");
                        }
                    }
                    coords[size] = new WKBCoordinate(coords[0]);
                    return (LinearRing)factory.CreateLinearRing(coords);
                    //	    CoordinateSequence pts = readCoordinateSequenceRing(size);
                    //	    return factory.createLinearRing(pts);
                    //return null;
                }
                private static int readUShort(BinaryReader dis)
                {
                    int b1 = dis.ReadByte() & 0xff;
                    int b2 = dis.ReadByte() & 0xff;
                    int n = (b2 << 8) | b1;
                    return n;
                }
                private static int read3byte(BinaryReader dis)
                {
                    int b1 = dis.ReadByte() & 0xff;
                    int b2 = dis.ReadByte() & 0xff;
                    int b3 = dis.ReadByte() & 0xff;
                    int n = (b3 << 16) | (b2 << 8) | b1;
                    return (short)n;
                }
            }

            #region Public Properties And Methods
            public static int Byte1Count = 0;
            public static int Byte2Count = 0;
            public static int Byte3Count = 0;
            public static int Byte4Count = 0;
            public static int MultiPolygonCount = 0;
            //public static byte[] Compress(IPolygon pgn)
            //{
            //    byte[] bts = null;
            //    IPolygon4 polygon = pgn as IPolygon4;
            //    //IPolygon4.ExteriorRingBag should be used instead of IPolygon.QueryExteriorRings,
            //    //which does not work in .NET because of C-Style Arrays
            //    IGeometryBag exteriorRings = polygon.ExteriorRingBag;
            //    IEnumGeometry exteriorRingsEnum = exteriorRings as IEnumGeometry;
            //    exteriorRingsEnum.Reset();
            //    int nBytes = 0;
            //    List<List<byte[]>> lstBytes = new List<List<byte[]>>();
            //    for (IRing currentExteriorRing = exteriorRingsEnum.Next() as IRing; currentExteriorRing != null; currentExteriorRing = exteriorRingsEnum.Next() as IRing)
            //    {
            //        List<byte[]> lstRing = new List<byte[]>();
            //        byte[] bytes = WriteCompress(currentExteriorRing);
            //        nBytes += bytes.Length;
            //        lstRing.Add(bytes);
            //        IGeometryBag interiorRings = polygon.get_InteriorRingBag(currentExteriorRing);
            //        IEnumGeometry interiorRingsEnum = interiorRings as IEnumGeometry;
            //        interiorRingsEnum.Reset();
            //        for (IRing currentinteriorRing = interiorRingsEnum.Next() as IRing; currentinteriorRing != null; currentinteriorRing = interiorRingsEnum.Next() as IRing)
            //        {
            //            bytes = WriteCompress(currentinteriorRing);
            //            lstRing.Add(bytes);
            //            nBytes += bytes.Length;
            //        }
            //        lstBytes.Add(lstRing);
            //    }
            //    if (lstBytes.Count > 1)
            //    {
            //        uint u = (uint)WKBGeometryTypes.WKBMultiPolygon & 0xff;
            //        byte b = (byte)((1 << 8) | u);
            //        nBytes += 3 + 3 * lstBytes.Count;
            //        byte[] bytes = new byte[nBytes];
            //        BinaryWriter writer = new BinaryWriter(new MemoryStream(bytes));
            //        writer.Write(b);
            //        writer.Write((short)lstBytes.Count);
            //        foreach (List<byte[]> rings in lstBytes)
            //        {
            //            u = (uint)WKBGeometryTypes.WKBPolygon & 0xff;
            //            b = (byte)((1 << 8) | u);
            //            writer.Write(b);
            //            writer.Write((short)rings.Count);
            //            foreach (byte[] ringBuffer in rings)
            //            {
            //                writer.Write(ringBuffer);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        nBytes += 3;
            //        byte[] bytes = new byte[nBytes];
            //        BinaryWriter writer = new BinaryWriter(new MemoryStream(bytes));
            //        List<byte[]> rings = lstBytes[0];
            //        uint u = (uint)WKBGeometryTypes.WKBPolygon & 0xff;
            //        byte b = (byte)((1 << 8) | u);
            //        writer.Write(b);
            //        writer.Write((short)rings.Count);
            //        foreach (byte[] ringBuffer in rings)
            //        {
            //            writer.Write(ringBuffer);
            //        }
            //    }
            //    return bts;
            //}
            //public static Geometry Convert2WKBGeometry(IGeometry geo)
            //{
            //    if (geo is IPolygon)
            //    {
            //        return conv_polygon(geo as IPolygon);
            //    }
            //    else if (geo is IPolyline)
            //    {
            //        return conv_polyline(geo as IPolyline);
            //    }
            //    else if (geo is IPoint)
            //    {
            //        IPoint p = geo as IPoint;
            //        var g = new NetTopologySuite.Geometries.Point(p.X, p.Y);
            //        return g;// new WKBCoordinate(p.X, p.Y);
            //    }
            //    System.Diagnostics.Debug.Assert(false);
            //    return null;
            //}
            public static byte[] Geometry2Bytes(Geometry geo)
            {
                //if (true)
                //{
                //    WKBWriter writer = new WKBWriter();
                //    byte[] bytes = writer.Write(geo);
                //    return bytes;
                //}
                if (geo is WKBPolygon)
                {
                    WKBPolygon[] pgns = new WKBPolygon[1];
                    pgns[0] = geo as WKBPolygon;
                    return Compress(pgns);
                }
                else if (geo is MultiPolygon)
                {
                    ++MultiPolygonCount;
                    MultiPolygon mp = geo as MultiPolygon;
                    WKBPolygon[] pgns = new WKBPolygon[mp.NumGeometries];
                    for (int i = 0; i < pgns.Length; ++i)
                        pgns[i] = mp.GetGeometryN(i) as WKBPolygon;
                    return Compress(pgns);
                }
                else
                {
                    WKBWriter writer = new WKBWriter();
                    byte[] bytes = writer.Write(geo);
                    return bytes;
                }
            }

            public static JTSGeometry Bytes2Geometry(byte[] bytes)
            {
                return MyWKBReader.read(bytes);
            }

            //public static IGeometry Convert2Geometry(JTSGeometry geo)
            //{
            //    if (geo is WKBPolygon)
            //    {
            //        WKBPolygon wp = geo as WKBPolygon;
            //        IPointCollection pgn = new ESRI.ArcGIS.Geometry.Polygon() as IPointCollection;
            //        foreach (object o in wp.Coordinates)
            //        {
            //            var p = o as WKBCoordinate;
            //            IPoint pt = new ESRI.ArcGIS.Geometry.Point();
            //            pt.PutCoords(p.X, p.Y);
            //            pgn.AddPoint(pt);
            //            //(pgn as IPolygon).SpatialReference = p.SpatialReference;
            //        }
            //        //(pgn as ITopologicalOperator).Simplify();
            //        ITopologicalOperator topologicalOperator = pgn as ITopologicalOperator;
            //        topologicalOperator.Simplify();
            //        return pgn as IPolygon;
            //    }
            //    else if (geo is MultiPolygon)
            //    {
            //        MultiPolygon mp = geo as MultiPolygon;
            //        IGeometryCollection geometryCollection = new ESRI.ArcGIS.Geometry.Polygon() as IGeometryCollection;
            //        foreach(var g in mp.Geometries){
            //            WKBPolygon wkbPolygon = g as WKBPolygon;
            //            IPointCollection outerPointCollection = new Ring();
            //            foreach (var wkbPoint in wkbPolygon.Coordinates)
            //            {
            //                IPoint pt=new ESRI.ArcGIS.Geometry.Point();
            //                pt.PutCoords(wkbPoint.X,wkbPoint.Y);
            //                outerPointCollection.AddPoint(pt);
            //            }
            //            geometryCollection.AddGeometry(outerPointCollection as IGeometry, ref _missing, ref _missing);
            //        }
            //        ITopologicalOperator topologicalOperator =geometryCollection as ITopologicalOperator;
            //        topologicalOperator.Simplify();
            //        return geometryCollection as IGeometry;
            //    }
            //    else if (geo is NetTopologySuite.Geometries.LineString)
            //    {
            //        var wkbLine = geo as NetTopologySuite.Geometries.LineString;
            //        IGeometryCollection geometryCollection = new Polyline() as IGeometryCollection;
            //        IPointCollection pointCollection = new ESRI.ArcGIS.Geometry.Path() as IPointCollection;
            //        foreach (object o in wkbLine.Coordinates)
            //        {
            //            WKBCoordinate p = o as WKBCoordinate;
            //            IPoint pt = new ESRI.ArcGIS.Geometry.Point();
            //            pt.PutCoords(p.X, p.Y);
            //            pointCollection.AddPoint(pt, ref _missing, ref _missing);
            //        }
            //        geometryCollection.AddGeometry(pointCollection as IGeometry, ref _missing, ref _missing);
            //        IPolyline pl = geometryCollection as IPolyline;
            //        //pl.SpatialReference = sr;
            //        return pl;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.Assert(false,"未处理的几何类型");
            //    }
            //    return null;
            //}
            //public Geometry read(byte[] bytes)  {
            //    int b=bytes[0]&0xff;
            //    if(b==WKBConstants.wkbXDR// ByteOrderValues.LITTLE_ENDIAN
            //            ||b==WKBConstants.wkbNDR)//ByteOrderValues.BIG_ENDIAN)
            //    //if((b&(1<<7))==0)
            //        return _jtsReader.read(bytes);
            //    else if((b&(7<<5))==0x80){
            //        try {
            //              return read(new ByteArrayInStream(bytes));
            //        }catch (IOException ex) {
            //          throw new RuntimeException("Unexpected IOException caught: " + ex.getMessage());
            //        }			
            //    }else{
            //        throw new RuntimeException("Unexpected IOException"); 
            //    }
            //    //return null;
            //}

            #endregion

            #region Private Properties And Methods
            //private byte[] Convert2WKB(IGeometry geo)
            //{
            //    if (geo is IPolygon)
            //    {
            //        return conv_polygon(geo as IPolygon);
            //    }
            //    return null;
            //}

            //private static byte[] WriteCompress(IRing r)
            //{

            //    WKSPoint[] wksPoints = convert2WKSPoints(r);
            //    IntPoint[] pts=new IntPoint[wksPoints.Length];
            //    for (int i = 0; i < wksPoints.Length; ++i)
            //    {
            //        pts[i].x = (int)(wksPoints[i].X * 1E6);
            //        pts[i].y = (int)(wksPoints[i].Y * 1E6);
            //    }
            //    int minX =pts[0].x;
            //    int minY =pts[0].y;
            //    int maxX = pts[0].x;
            //    int maxY = pts[0].y;
            //    for (int i = 1; i < pts.Length; ++i)
            //    {
            //        if (pts[i].x < minX)
            //            minX = pts[i].x;
            //        else if (pts[i].x > maxX)
            //            maxX = pts[i].x;
            //        if (pts[i].y < minY)
            //            minY = pts[i].y;
            //        else if (pts[i].y > maxY)
            //            maxY = pts[i].y;
            //    }
            //    int dx = maxX - minX;
            //    int dy = maxY - minY;
            //    int xnb = dx < 256 ? 1 : (dx < 65536 ? 2 : (dx < 16777216?3:4));
            //    int ynb = dy < 256 ? 1 : (dy < 65536 ? 2 : (dy < 16777216 ? 3 : 4));
            //    int nByteLen = 3 * sizeof(int)+pts.Length*xnb+pts.Length+ynb;
            //    byte[] bytes = new byte[nByteLen];
            //    BinaryWriter writer=new BinaryWriter(new MemoryStream(bytes));
            //    writer.Write(pts.Length);
            //    int n = minX;
            //    if (xnb == 2)
            //        n |= 1 << 31;
            //    else if (xnb == 3)
            //        n |= 1 << 32;
            //    else if (xnb == 4)
            //        n |= (1 << 31) | (1 << 32);
            //    writer.Write(n);
            //    n = minY;
            //    if (ynb == 2)
            //        n |= 1 << 31;
            //    else if (ynb == 3)
            //        n |= 1 << 32;
            //    else if (ynb == 4)
            //        n |= (1 << 31) | (1 << 32);
            //    writer.Write(n);
            //    for (int i = 0; i < pts.Length; ++i)
            //    {
            //        if (xnb == 1)
            //        {
            //            byte b = (byte)(pts[i].x - minX);
            //            writer.Write(b);
            //        }
            //        else if (xnb == 2)
            //        {
            //            short b = (short)(pts[i].x - minX);
            //            writer.Write(b);
            //        }
            //        else if (xnb == 4)
            //        {
            //            int b =pts[i].x - minX;
            //            writer.Write(b);
            //        }
            //        else if (xnb == 3)
            //        {
            //            int m = pts[i].x - minX;
            //            writer.Write((byte)m);
            //            m >>= 8;
            //            writer.Write((byte)m);
            //            m >>= 8;
            //            writer.Write((byte)m);
            //        }
            //    }
            //    for (int i = 0; i < pts.Length; ++i)
            //    {
            //        if (ynb == 1)
            //        {
            //            byte b = (byte)(pts[i].y - minY);
            //            writer.Write(b);
            //        }
            //        else if (ynb == 2)
            //        {
            //            short b = (short)(pts[i].y - minY);
            //            writer.Write(b);
            //        }
            //        else if (ynb == 4)
            //        {
            //            int b = pts[i].y - minY;
            //            writer.Write(b);
            //        }
            //        else if (ynb == 3)
            //        {
            //            int m = pts[i].y - minY;
            //            writer.Write((byte)m);
            //            m >>= 8;
            //            writer.Write((byte)m);
            //            m >>= 8;
            //            writer.Write((byte)m);
            //        }
            //    }
            //    return bytes;
            //}
            //private static WKSPoint[] convert2WKSPoints(IRing r)
            //{
            //    IPointCollection pIPC = r as IPointCollection;
            //    int lPointCount = pIPC.PointCount;
            //    WKSPoint[] buffer = new WKSPoint[lPointCount];
            //    pIPC.QueryWKSPoints(0, lPointCount, out buffer[0]);
            //    return buffer;
            //}
            //private static WKBCoordinate[] AEPointCollection2WKBPoints(IPointCollection pc, bool fClose = false)
            //{
            //    int lPointCount = pc.PointCount;
            //    System.Diagnostics.Debug.Assert(lPointCount > 0);
            //    WKSPoint[] buffer = new WKSPoint[fClose ? (lPointCount + 1) : lPointCount];
            //    pc.QueryWKSPoints(0, lPointCount, out buffer[0]);
            //    if (fClose)
            //    {
            //        buffer[buffer.Length - 1] = buffer[0];
            //    }
            //    return Convert2WKBPoints(buffer);
            //}
            private static WKBPolygon ListRing2WKBPolygon(List<LinearRing> rings)
            {
                LinearRing shell = rings[0];
                IWKBLinearRing[] holes = null;
                if (rings.Count > 1)
                {
                    holes = new IWKBLinearRing[rings.Count - 1];
                    for (int i = 0; i < holes.Length; ++i)
                    {
                        holes[i] = rings[i + 1];
                    }
                }
                WKBPolygon wkbPolygon = new WKBPolygon(shell, holes);
                return wkbPolygon;
            }
            //private static Geometry conv_polygon(IPolygon pgn)
            //{
            //    IPolygon4 polygon = pgn as IPolygon4;
            //    //IPolygon4.ExteriorRingBag should be used instead of IPolygon.QueryExteriorRings,
            //    //which does not work in .NET because of C-Style Arrays
            //    IGeometryBag exteriorRings = polygon.ExteriorRingBag;
            //    IEnumGeometry exteriorRingsEnum = exteriorRings as IEnumGeometry;
            //    exteriorRingsEnum.Reset();
            //    List<List<LinearRing>> lstRings = new List<List<LinearRing>>();
            //    for (IRing currentExteriorRing = exteriorRingsEnum.Next() as IRing; currentExteriorRing != null; currentExteriorRing = exteriorRingsEnum.Next() as IRing)
            //    {
            //        List<LinearRing> lstRing = new List<LinearRing>();
            //        lstRing.Add(new LinearRing(AEPointCollection2WKBPoints(currentExteriorRing as IPointCollection)));
            //        IGeometryBag interiorRings = polygon.get_InteriorRingBag(currentExteriorRing);
            //        IEnumGeometry interiorRingsEnum = interiorRings as IEnumGeometry;
            //        interiorRingsEnum.Reset();
            //        for (IRing currentinteriorRing = interiorRingsEnum.Next() as IRing; currentinteriorRing != null; currentinteriorRing = interiorRingsEnum.Next() as IRing)
            //        {
            //            lstRing.Add(new LinearRing(AEPointCollection2WKBPoints(currentinteriorRing as IPointCollection)));
            //        }
            //        lstRings.Add(lstRing);
            //    }
            //    if (lstRings.Count == 1)
            //    {
            //        return ListRing2WKBPolygon(lstRings[0]);
            //    }
            //    else if (lstRings.Count > 1)
            //    {
            //        GeoAPI.Geometries.IPolygon[] pgns = new GeoAPI.Geometries.IPolygon[lstRings.Count];
            //        for (int i = 0; i < pgns.Length; ++i)
            //            pgns[i] = ListRing2WKBPolygon(lstRings[i]);
            //        MultiPolygon mp = new MultiPolygon(pgns);
            //        return mp;
            //    }
            //    return null;
            //    //IGeometryCollection pIGC = polygon as IGeometryCollection;
            //    //int lGeometryCount = pIGC.GeometryCount;

            //    //WGStoChina_lb c = new WGStoChina_lb();

            //    //List<WKSPoint[]> rings = new List<WKSPoint[]>();
            //    //for (int i = 0; i < lGeometryCount; i++)
            //    //{
            //    //    IPointCollection pIPC = pIGC.get_Geometry(i) as IPointCollection;
            //    //    int lPointCount = pIPC.PointCount;
            //    //    WKSPoint[] buffer = new WKSPoint[lPointCount+1];
            //    //    pIPC.QueryWKSPoints(0, lPointCount, out buffer[0]);

            //    //    #region 转换为火星坐标
            //    //    for (int j = 0; j < lPointCount; ++j)
            //    //    {
            //    //        WGStoChina_lb.Point cp = c.getEncryPoint(buffer[j].X,buffer[j].Y);
            //    //        buffer[j].X = cp.getX();
            //    //        buffer[j].Y = cp.getY();
            //    //    }
            //    //    buffer[buffer.Length-1] = buffer[0];
            //    //    #endregion 转换为火星坐标

            //    //    rings.Add(buffer);
            //    //}
            //    //if (rings.Count == 0)
            //    //    return null;
            //    //LinearRing shell = new LinearRing(Convert2WKBPoints(rings[0]));
            //    //IWKBLinearRing[] holes = null;
            //    //if (rings.Count > 1)
            //    //{
            //    //    holes = new IWKBLinearRing[rings.Count - 1];
            //    //    for (int i = 0; i < holes.Length; ++i)
            //    //    {
            //    //        holes[i] = new LinearRing(Convert2WKBPoints(rings[i + 1]));
            //    //    }
            //    //}
            //    ////new IWKBLinearRing
            //    //WKBPolygon wkbPolygon = new WKBPolygon(shell, holes);
            //    //return wkbPolygon;


            //}
            //private static Geometry conv_polyline(IPolyline line)
            //{
            //    IGeometryCollection gcc = line as IGeometryCollection;
            //    if (gcc.GeometryCount == 1)
            //    {
            //        IPointCollection pcc = gcc.get_Geometry(0) as IPointCollection;
            //        GeoAPI.Geometries.Coordinate[] points = new GeoAPI.Geometries.Coordinate[pcc.PointCount];
            //        for (int i = 0; i < pcc.PointCount; ++i)
            //        {
            //            var p = pcc.get_Point(i);
            //            points[i] = new WKBCoordinate(p.X, p.Y);
            //        }
            //        LineString ls = new LineString(points);
            //        return ls;
            //    }
            //    else if (gcc.GeometryCount > 1)
            //    {
            //        var lineStrings = new GeoAPI.Geometries.ILineString[gcc.GeometryCount];
            //        for (int j = 0; j < gcc.GeometryCount; ++j)
            //        {
            //            IPointCollection pcc = gcc.get_Geometry(j) as IPointCollection;
            //            GeoAPI.Geometries.Coordinate[] points = new GeoAPI.Geometries.Coordinate[pcc.PointCount];
            //            for (int i = 0; i < pcc.PointCount; ++i)
            //            {
            //                var p = pcc.get_Point(i);
            //                points[i] = new WKBCoordinate(p.X, p.Y);
            //                //points[i].X = p.X;
            //                //points[i].Y = p.Y;
            //            }
            //            lineStrings[j] = new LineString(points);
            //        }
            //        MultiLineString mp = new MultiLineString(lineStrings);
            //        return mp;
            //    }
            //    return null;
            //}
            //private static WKBCoordinate Convert2WKBPoint(WKSPoint point)
            //{
            //    WKBCoordinate wc = new WKBCoordinate(point.X, point.Y);
            //    return wc;
            //}
            //private static WKBCoordinate[] Convert2WKBPoints(WKSPoint[] pts)
            //{
            //    WKBCoordinate[] arr = new WKBCoordinate[pts.Length];
            //    for (int i = 0; i < pts.Length; ++i)
            //    {
            //        arr[i] = Convert2WKBPoint(pts[i]);
            //    }
            //    return arr;
            //}
            private static byte[] Compress(WKBPolygon[] pgns)
            {
                byte[] bts = null;
                int nBytes = 0;
                List<List<byte[]>> lstRings = new List<List<byte[]>>();
                foreach (WKBPolygon pgn in pgns)
                {
                    List<byte[]> lstRingBytes = new List<byte[]>();
                    byte[] bytes = WriteCompress(pgn.ExteriorRing);
                    nBytes += bytes.Length;
                    lstRingBytes.Add(bytes);
                    foreach (IWKBLinearRing r in pgn.Holes)
                    {
                        byte[] bytes1 = WriteCompress(r);
                        lstRingBytes.Add(bytes1);
                        nBytes += bytes1.Length;
                    }
                    lstRings.Add(lstRingBytes);
                }
                if (pgns.Length > 1)
                {//write MultiPolygon
                    uint u = (uint)WKBGeometryTypes.WKBMultiPolygon & 0xff;
                    byte b = (byte)((1 << 7) | u);
                    nBytes += 3 + 3 * pgns.Length;
                    bts = new byte[nBytes];
                    BinaryWriter writer = new BinaryWriter(new MemoryStream(bts));
                    writer.Write(b);
                    ushort nPolygonCount = (ushort)pgns.Length;
                    writer.Write(nPolygonCount);
                    System.Diagnostics.Debug.Assert(nPolygonCount == lstRings.Count);
                    foreach (List<byte[]> rings in lstRings)
                    {
                        writePolygon(writer, rings);
                    }
                }
                else
                {//Write Polygon
                    nBytes += 3;
                    bts = new byte[nBytes];
                    BinaryWriter writer = new BinaryWriter(new MemoryStream(bts));
                    writePolygon(writer, lstRings[0]);
                    //List<byte[]> rings = lstRings[0];
                    //uint u = (uint)WKBGeometryTypes.WKBPolygon & 0xff;
                    //byte b = (byte)((1 << 7) | u);
                    //writer.Write(b);
                    //writer.Write((ushort)rings.Count);
                    //foreach (byte[] ringBuffer in rings)
                    //{
                    //    writer.Write(ringBuffer);
                    //}
                }
                return bts;
            }
            private static void writePolygon(BinaryWriter writer, List<byte[]> rings)
            {
                //List<byte[]> rings = lstRings[0];
                uint u = (uint)WKBGeometryTypes.WKBPolygon & 0xff;
                byte b = (byte)((1 << 7) | u);
                writer.Write(b);
                writer.Write((ushort)rings.Count);
                foreach (byte[] ringBuffer in rings)
                {
                    writer.Write(ringBuffer);
                }
            }
            private static byte[] WriteCompress(GeoAPI.Geometries.ILineString r)
            {
                MyIntPoint[] pts = new MyIntPoint[r.NumPoints];
                //System.Diagnostics.Debug.Assert(r.NumPoints < 10000);
                for (int i = 0; i < pts.Length; ++i)
                {
                    WKBCoordinate c = r.GetCoordinateN(i);
                    pts[i].x = (int)(c.X * 1E6);
                    pts[i].y = (int)(c.Y * 1E6);
                }
                int minX = pts[0].x;
                int minY = pts[0].y;
                int maxX = pts[0].x;
                int maxY = pts[0].y;
                for (int i = 1; i < pts.Length; ++i)
                {
                    if (pts[i].x < minX)
                        minX = pts[i].x;
                    else if (pts[i].x > maxX)
                        maxX = pts[i].x;
                    if (pts[i].y < minY)
                        minY = pts[i].y;
                    else if (pts[i].y > maxY)
                        maxY = pts[i].y;
                }
                int dx = maxX - minX;
                int dy = maxY - minY;
                int xnb = dx < 256 ? 1 : (dx < 65536 ? 2 : (dx < 16777216 ? 3 : 4));
                int ynb = dy < 256 ? 1 : (dy < 65536 ? 2 : (dy < 16777216 ? 3 : 4));
                int nByteLen = 3 * sizeof(int) + pts.Length * xnb + pts.Length * ynb;
                byte[] bytes = new byte[nByteLen];
                BinaryWriter writer = new BinaryWriter(new MemoryStream(bytes));
                writer.Write(pts.Length);
                int n = minX;
                if (xnb == 2)
                {
                    n |= 1 << 30;
                    ++Byte2Count;
                }
                else if (xnb == 3)
                {
                    n |= 1 << 31;
                    ++Byte3Count;
                }
                else if (xnb == 4)
                {
                    n |= 3 << 30;
                    ++Byte4Count;
                }
                else
                    ++Byte1Count;
                writer.Write(n);
                n = minY;
                if (ynb == 2)
                {
                    n |= 1 << 30;
                    ++Byte2Count;
                }
                else if (ynb == 3)
                {
                    n |= 1 << 31;
                    ++Byte3Count;
                }
                else if (ynb == 4)
                {
                    n |= 3 << 30;
                    ++Byte4Count;
                }
                else
                {
                    ++Byte1Count;
                }
                writer.Write(n);
                for (int i = 0; i < pts.Length; ++i)
                {
                    if (xnb == 1)
                    {
                        byte b = (byte)(pts[i].x - minX);
                        writer.Write(b);
                    }
                    else if (xnb == 2)
                    {
                        ushort b = (ushort)(pts[i].x - minX);
                        writer.Write(b);
                    }
                    else if (xnb == 4)
                    {
                        //int b = pts[i].x - minX;
                        writer.Write(pts[i].x);
                    }
                    else if (xnb == 3)
                    {
                        int m = pts[i].x - minX;
                        byte b = (byte)m;
                        writer.Write(b);
                        m >>= 8;
                        b = (byte)m;
                        writer.Write(b);
                        m >>= 8;
                        b = (byte)m;
                        writer.Write(b);
                    }
                }
                for (int i = 0; i < pts.Length; ++i)
                {
                    if (ynb == 1)
                    {
                        byte b = (byte)(pts[i].y - minY);
                        writer.Write(b);
                    }
                    else if (ynb == 2)
                    {
                        ushort b = (ushort)(pts[i].y - minY);
                        writer.Write(b);
                    }
                    else if (ynb == 4)
                    {
                        //int b = pts[i].y - minY;
                        writer.Write(pts[i].y);
                    }
                    else if (ynb == 3)
                    {
                        int m = pts[i].y - minY;
                        writer.Write((byte)m);
                        m >>= 8;
                        writer.Write((byte)m);
                        m >>= 8;
                        writer.Write((byte)m);
                    }
                }
                return bytes;
            }

            #endregion
        }
        public class ConstructConditionHelper
        {
            //public static String constructSpatialFilterCondition(IFeatureClass fc, IQueryFilter filter, List<String> args
            //        , String mingxFieldName, String mingyFieldName, String maxgxFieldName, String maxgyFieldName)
            //{

            //    String wh = filter.WhereClause;
            //    if (!(filter is ISpatialFilter))
            //        return wh;
            //    //CoordinateSequenceFilter csFilter=null;
            //    //CoordinateSequenceFilter csReverseFilter=null;

            //    ISpatialFilter spatialFilter = (ISpatialFilter)filter;
            //    var pIGeo = spatialFilter.Geometry;
            //    var spatialRel = spatialFilter.SpatialRel;
            //    String spatialQueryWh = null;
            //    if (pIGeo != null)
            //    {
            //        IGeometry geom = null;
            //        //if(fc!=null){
            //        //    if(pIGeo is IGeometry){
            //        //        csFilter=fc.getGeometryFilter();
            //        //        csReverseFilter=fc.getGeometryReverseFilter();
            //        //        geom=(IGeometry)pIGeo;
            //        //        if(csReverseFilter!=null)
            //        //            geom.apply(csReverseFilter);
            //        //    }
            //        //}				
            //        if (pIGeo is Envelope)
            //        {
            //            if (spatialRel == eSpatialRelEnum.eSpatialRelEnvelopeIntersects)
            //                spatialQueryWh = constructEnvelopeIntersectsCondition((Envelope)pIGeo, args, mingxFieldName, mingyFieldName, maxgxFieldName, maxgyFieldName);
            //            else if (spatialRel == eSpatialRelEnum.eSpatialRelContains)
            //            {
            //                spatialQueryWh = constructContainsCondition((Envelope)pIGeo, mingxFieldName, mingyFieldName, maxgxFieldName, maxgyFieldName);
            //            }
            //        }
            //        else if (pIGeo is IPoint)
            //        {
            //            if (spatialRel == eSpatialRelEnum.eSpatialRelWithin)
            //            {
            //                spatialQueryWh = constructWithinCondition((IPoint)pIGeo, mingxFieldName, mingyFieldName, maxgxFieldName, maxgyFieldName);
            //            }
            //        }
            //        else
            //        {
            //            //@TODO...
            //        }

            //        //if(csFilter!=null){
            //        //    geom.apply(csFilter);
            //        //}			
            //    }

            //    if (wh == null || wh.Length == 0)
            //        wh = spatialQueryWh;
            //    else if (spatialQueryWh != null)
            //        wh = "(" + wh + ") and (" + spatialQueryWh + ")";



            //    return wh;
            //}
            //public static String constructSpatialFilterCondition(IFeatureClass fc, IQueryFilter filter, List<String> args)
            //{
            //    return constructSpatialFilterCondition(fc, filter, args, "mingx", "mingy", "maxgx", "maxgy");
            //}
            //public static String constructSpatialFilterCondition(IFeatureClass fc, IQueryFilter filter)
            //{
            //    //List<String> args=new List<String>();
            //    return constructSpatialFilterCondition(fc, filter, null, "mingx", "mingy", "maxgx", "maxgy");
            //}
            //public static String constructSpatialFilterCondition(IFeatureClass fc, IQueryFilter filter
            //        , String mingxFieldName, String mingyFieldName, String maxgxFieldName, String maxgyFieldName)
            //{
            //    return constructSpatialFilterCondition(fc, filter, null, mingxFieldName, mingyFieldName, maxgxFieldName, maxgyFieldName);
            //}
            public static String constructEnvelopeIntersectsCondition(Envelope env, List<String> args
                    , String mingxFieldName, String mingyFieldName, String maxgxFieldName, String maxgyFieldName)
            {
                double minx = env.MinX;
                double miny = env.MinY;
                double maxx = env.MaxX;
                double maxy = env.MaxY;
                String s = mingxFieldName + " is not null and " + mingyFieldName + " is not null";
                if (maxgxFieldName != null && !StringUtil.isEqual(mingxFieldName, maxgxFieldName))
                    s += " and " + maxgxFieldName + " is not null";
                if (maxgyFieldName != null && !StringUtil.isEqual(mingyFieldName, maxgyFieldName))
                    s += " and " + maxgyFieldName + " is not null";
                String maxXFieldName = maxgxFieldName == null ? mingxFieldName : maxgxFieldName;
                String maxYFieldName = maxgyFieldName == null ? mingyFieldName : maxgyFieldName;
                if (args == null)
                {
                    String wh = s + " and " + mingxFieldName + "<" + maxx + " and " + maxXFieldName + ">" + minx + " and " + mingyFieldName + "<" + maxy
                            + " and " + maxYFieldName + ">" + miny;
                    return wh;
                }
                else
                {
                    String wh = s + " and " + mingxFieldName + "<? and " + maxXFieldName + ">? and " + mingyFieldName + "<? and " + maxYFieldName + ">?";
                    args.Add(maxx.ToString());
                    args.Add(minx.ToString());
                    args.Add(maxy.ToString());
                    args.Add(miny.ToString());
                    return wh;
                }
            }
            public static String constructEnvelopeIntersectsCondition(Envelope env, List<String> args)
            {
                return constructEnvelopeIntersectsCondition(env, args, "mingx", "mingy", "maxgx", "maxgy");
                //		double minx=env.getMinX();
                //		double miny=env.getMinY();
                //		double maxx=env.getMaxX();
                //		double maxy=env.getMaxY();
                //		String s="mingx is not null and mingy is not null and maxgx is not null and maxgy is not null";
                //		if(args==null){
                //			String wh=s+" and mingx<"+maxx+" and maxgx>"+minx+" and mingy<"+maxy+" and maxgy>"+miny;
                //			return wh;
                //		}else{
                //			String wh=s+" and mingx<? and maxgx>? and mingy<? and maxgy>?";
                //			args.add(Double.toString(maxx));
                //			args.add(Double.toString(minx));
                //			args.add(Double.toString(maxy));
                //			args.add(Double.toString(miny));
                //			return wh;
                //		}
            }
            private static String constructContainsCondition(Envelope env, String mingxFieldName, String mingyFieldName, String maxgxFieldName, String maxgyFieldName)
            {
                double minx = env.MinX;
                double miny = env.MinY;
                double maxx = env.MaxX;
                double maxy = env.MaxY;
                String s = mingxFieldName + " is not null and " + mingyFieldName + " is not null";// and maxgx is not null and maxgy is not null";
                if (maxgxFieldName != null && !StringUtil.isEqual(mingxFieldName, maxgxFieldName))
                    s += " and " + maxgxFieldName + " is not null";
                if (maxgyFieldName != null && !StringUtil.isEqual(mingyFieldName, maxgyFieldName))
                    s += " and " + maxgyFieldName + " is not null";
                String maxXFieldName = maxgxFieldName == null ? mingxFieldName : maxgxFieldName;
                String maxYFieldName = maxgyFieldName == null ? mingyFieldName : maxgyFieldName;

                String wh = s + " and " + mingxFieldName + ">" + minx + " and " + maxXFieldName + "<" + maxx + " and " + mingyFieldName + ">" + miny
                        + " and " + maxYFieldName + "<" + maxy;
                return wh;
            }
            public static String constructContainsCondition(Envelope env)
            {
                return constructContainsCondition(env, "mingx", "mingy", "maxgx", "maxgy");
            }
            private static String constructWithinCondition(IPoint pt, String mingxFieldName, String mingyFieldName, String maxgxFieldName, String maxgyFieldName)//,List<String> args)
            {
                double x = pt.X;
                double y = pt.Y;
                //String s="mingx is not null and mingy is not null and maxgx is not null and maxgy is not null";
                String s = mingxFieldName + " is not null and " + mingyFieldName + " is not null";// and maxgx is not null and maxgy is not null";
                if (maxgxFieldName != null && !StringUtil.isEqual(mingxFieldName, maxgxFieldName))
                    s += " and " + maxgxFieldName + " is not null";
                if (maxgyFieldName != null && !StringUtil.isEqual(mingyFieldName, maxgyFieldName))
                    s += " and " + maxgyFieldName + " is not null";
                String maxXFieldName = maxgxFieldName == null ? mingxFieldName : maxgxFieldName;
                String maxYFieldName = maxgyFieldName == null ? mingyFieldName : maxgyFieldName;

                String wh = s + " and " + mingxFieldName + "<" + x + " and " + maxXFieldName + ">" + x + " and " + mingyFieldName + "<" + y
                        + " and " + maxYFieldName + ">" + y;
                return wh;
            }
            public static String constructWithinCondition(IPoint pt)
            {
                return constructWithinCondition(pt, "mingx", "mingy", "maxgx", "maxgy");
            }

            /// <summary>
            /// 构造in查询条件集合
            /// 说明：将传入的数据集合inList按groupSize大小分组并形成sql的in查询条件
            /// 返回格式:a,b,c,d,...或'a','b','c','d',...
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="inList"></param>
            /// <param name="groupSize">单个in语句包含的最多元素个数</param>
            /// <returns></returns>
            public static void ConstructInContitions(List<int> inList, int groupSize/*=20*/, List<String> lstIn)
            {
                //List<String> lstIn = new List<String>();
                //bool fTIsStr = typeof(T) == typeof(String) || typeof(T) == typeof(String);
                int GROUP_COUNT = groupSize;
                int i = 0;
                while (true)
                {
                    int n = inList.Count - i;
                    if (n > GROUP_COUNT)
                    {
                        n = GROUP_COUNT;
                    }
                    if (n <= 0)
                        break;
                    String sin = null;
                    //for (; i < n; ++i)
                    for (; n > 0; --n, ++i)
                    {

                        String t = null;
                        //                if (fTIsStr)
                        //                {
                        //                    String str = inList[i].ToString();
                        //                    str = str.Replace("'", "''");
                        //                    t = "'" + str + "'";
                        //                }
                        //                else
                        t = inList[i].ToString();
                        if (sin == null)
                            sin = t;
                        else
                        {
                            sin += "," + t;
                        }
                    }
                    lstIn.Add(sin);
                }
                //return lstIn;
            }
        }

        internal class CreateSysTables
        {
            public static void Create(DBSQLite db)
            {
                var tran = db.BeginTransaction();
                try
                {
                    android_metadata(db);
                    GDB_FeatureDataset(db);
                    GDB_ObjectClasses(db);
                    GDB_FieldInfo(db);
                    GDB_Domain(db);
                    //GDB_CodedDomains(db);
                    //ime_CodeType(db);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            public static void Repair(DBSQLite db)
            {
                var tran = db.BeginTransaction();
                try
                {
                    if (!db.IsTableExists("android_metadata"))
                        android_metadata(db);
                    if (!db.IsTableExists("ime_FeatureDataset"))
                        GDB_FeatureDataset(db);
                    if (!db.IsTableExists("ime_ObjectClasses"))
                        GDB_ObjectClasses(db);
                    if (!db.IsTableExists("ime_FieldInfo"))
                        GDB_FieldInfo(db);
                    if (!db.IsTableExists("ime_Domain"))
                        GDB_Domain(db);
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            private static void android_metadata(DBSQLite db)
            {
                var sql = "CREATE TABLE [android_metadata] ([locale] TEXT)";
                if (!db.IsTableExists("android_metadata"))
                {
                    db.ExecuteNonQuery(sql);
                }
                sql = "insert into [android_metadata] values('zh_CN');";
                db.ExecuteNonQuery(sql);
            }
            private static void GDB_FeatureDataset(DBSQLite db)
            {
                /*
                String sql = "create table ime_FeatureDataset(";
                sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT,";
                sql += "Name TEXT NOT NULL,";
                sql += "SRTEXT TEXT,";
                sql += "FalseX double,";
                sql += "FalseY double,";
                sql += "XYUnits double,";
                sql += "FalseZ double,";
                sql += "ZUnits double,";
                sql += "FalseM double,";
                sql += "MUnits double,";
                sql += "IsHighPrecision smallint,";
                sql += "XYTolerance double,";
                sql += "ZTolerance double,";
                sql += "MTolerance double";
                sql += ")";*/
                var sql = "create table ime_FeatureDataset(";
                sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT,";
                sql += "Name TEXT NOT NULL,";
                sql += "SRID int,";
                sql += "SRTEXT TEXT";
                sql += ")";
                db.ExecuteNonQuery(sql);
            }
            private static void GDB_Domain(DBSQLite db)
            {
                var sql = "CREATE TABLE [ime_Domain](";
                sql += "[Name] TEXT NOT NULL, ";
                sql += "[Code] TEXT NOT NULL, ";
                sql += "[Value] TEXT NOT NULL, ";
                sql += "PRIMARY KEY([Name], [Code]));";
                db.ExecuteNonQuery(sql);
            }
            private static void GDB_ObjectClasses(DBSQLite db)
            {
                var sql = "create table ime_ObjectClasses(";
                sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT,";
                sql += "Name TEXT NOT NULL,";
                sql += "AliasName TEXT,";
                sql += "DatasetID INTEGER,";
                sql += "ShapeFieldName TEXT,";
                sql += "GeometryType smallint";
                sql += ")";
                db.ExecuteNonQuery(sql);
            }
            private static void GDB_FieldInfo(DBSQLite db)
            {
                var sql = "create table ime_FieldInfo(";
                sql += "ClassID INTEGER NOT NULL,";
                sql += "TableName TEXT NOT NULL,";
                sql += "FieldName TEXT NOT NULL,";
                sql += "FieldType smallint NOT NULL,";
                sql += "AliasName TEXT,";
                sql += "ModelName TEXT,";
                //sql += "ParentDomainName TEXT,";
                sql += "DefaultDomainName TEXT,";
                sql += "DefaultValueString TEXT,";
                sql += "DefaultValueNumber double,";
                sql += "IsRequired smallint,";
                sql += "IsSubtypeFixed smallint,";
                sql += "IsEditable smallint,";
                sql += "precision_ smallint,";
                sql += "scale smallint,";
                sql += "constraint ime_FieldInfo_pk primary key (ClassID,FieldName)";
                sql += ")";
                db.ExecuteNonQuery(sql);
            }
            //private static void GDB_CodedDomains(DBSQLite db)
            //{
            //    String sql = "create table ime_CodeTable(";
            //    sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT,";
            //    sql += "DomainName TEXT NOT NULL,";
            //    sql += "Code INTEGER NOT NULL,";
            //    sql += "ParentCode INTEGER,";
            //    sql += "Islocal smallint";
            //    sql += ")";
            //    db.ExecuteNonQuery(sql);
            //}
            //private static void ime_CodeType(DBSQLite db)
            //{
            //    String sql = "create table ime_CodeType(";
            //    sql += "ID INTEGER PRIMARY KEY AUTOINCREMENT,";
            //    sql += "CodeType TEXT NOT NULL,";
            //    sql += "codeTablename TEXT NOT NULL,";
            //    sql += "codeField TEXT NOT NULL,";
            //    sql += "name_field TEXT NOT NULL";
            //    sql += ")";
            //    db.ExecuteNonQuery(sql);
            //}
        }


        #region yxm 20171021
        /// <summary>
        /// 特殊字段的常量（全部是小写）
        /// </summary>
        public class SpecialFieldNameConstants
        {
            public const string objectid = "objectid";
            public const string shape = "shape";
            public const string mingx = "mingx";
            public const string mingy = "mingy";
            public const string maxgx = "maxgx";
            public const string maxgy = "maxgy";
        }

        public void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null)
        {
            var datasetID = srid;

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
            var sql = BuildCreateFeatureClassSql(tableName, tableAliasName, fields, geoType);

            ExecuteNonQuery(sql);
            CreateFeatureClassSpatialIndex(tableName, geoType);
            var fid = InsertObjectClasses(tableName, tableAliasName, geoType, datasetID, shpField.FieldName);

            InsertFieldInfo(fid, fields, tableName);
        }


        private string BuildCreateFeatureClassSql(string tableName, string aliasName, IFields Fields, eGeometryType geoType)
        {
            var sql = "create table " + tableName + "(";
            var fields = "";
            if (geoType != eGeometryType.eGeometryNull)
            {
                fields = "shape BLOB,mingx double,mingy double";
            }
            if (geoType != eGeometryType.eGeometryPoint)
            {
                fields += ",maxgx double , maxgy double";
            }

            for (var i = 0; i < Fields.FieldCount; ++i)
            {
                var fe = Fields.GetField(i);
                if (!fe.Editable || fe.FieldType == eFieldType.eFieldTypeOID || fe.FieldType == eFieldType.eFieldTypeGeometry
                    || IsSpatialFieldName(fe.FieldName, true))
                {
                    continue;
                }

                if (fields.Length > 0)
                {
                    fields += ",";
                }
                var fieldName = fe.FieldName;
                fields += fieldName + " " + ToFieldTypeString(fe);
            }
            sql += fields;
            sql += ")";
            return sql;
        }

        /// <summary>
        /// 创建空间索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="geoType"></param>
        private void CreateFeatureClassSpatialIndex(string tableName, eGeometryType geoType)
        {
            if (geoType == eGeometryType.eGeometryNull)
                return;
            var sql = $"CREATE INDEX [idx_{tableName}_extent] ON [{tableName}] ([mingx], [mingy]";
            if (geoType != eGeometryType.eGeometryPoint)
            {
                sql += ", [maxgx], [maxgy]";
            }
            sql += ")";
            ExecuteNonQuery(sql);
        }
        private int InsertObjectClasses(string tableName, string aliasName, eGeometryType geoType, int datasetID, string shapeFieldName)
        {
            string sqlObjectClass = "insert into ime_ObjectClasses(Name,AliasName,DatasetID,GeometryType,ShapeFieldName)";
            sqlObjectClass += "values(@Name,@AliasName,@DatasetID,@GeometryType,@ShapeFieldName)";
            var lstParm = new List<LibCore.SQLParam>()
                {
                    //new SQLiteParam(){ParamName="ID",ParamValue=featureClassID},
                    new SQLParam(){ParamName="Name",ParamValue=tableName},
                    new SQLParam(){ParamName="AliasName",ParamValue=aliasName},
                    new SQLParam(){ParamName="DatasetID",ParamValue=datasetID},
                    new SQLParam(){ParamName="ShapeFieldName",ParamValue=shapeFieldName},
                };
            if (geoType == eGeometryType.eGeometryNull)
                lstParm.Add(new LibCore.SQLParam() { ParamName = "GeometryType", ParamValue = null });//geoType==esriGeometryType.esriGeometryNull?null:(short)geoType},
            else
                lstParm.Add(new LibCore.SQLParam() { ParamName = "GeometryType", ParamValue = (short)geoType });
            sqlObjectClass += ";select last_insert_rowid() from ime_ObjectClasses";
            //ws.ExecuteNonQuery(sqlObjectClass, lstParm);
            var o = ExecuteScalar(sqlObjectClass, lstParm);
            return SafeConvertAux.ToInt32(o);
        }
        private void InsertFieldInfo(int featureOID, IFields Fields, string tableName)
        {
            var lstParm = new List<SQLParam>
            {
                new SQLParam() { ParamName = "ClassID", ParamValue = featureOID },
                new SQLParam() { ParamName = "FieldName" },//, ParamValue = field.Name });
				new SQLParam() { ParamName = "FieldType" },//, ParamValue = field.Name });
				new SQLParam() { ParamName = "AliasName" },//, ParamValue = field.AliasName });
				new SQLParam() { ParamName = "DefaultDomainName" },//, ParamValue = field.Domain.Name });
				new SQLParam() { ParamName = "IsEditable" },//, ParamValue = field.Editable?1:0 });
				new SQLParam() { ParamName = "precision_" },//, ParamValue =null });
				new SQLParam() { ParamName = "scale" },//, ParamValue = null });
				new SQLParam() { ParamName = "TableName" }//, ParamValue = null });
			};
            for (var i = 0; i < Fields.FieldCount; ++i)
            {
                var fe = Fields.GetField(i);
                InsertIntoFieldInfo(fe, featureOID, tableName, lstParm);
            }
        }
        /// <summary>
        /// 向ime_FieldInfo表插入信息
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="fe"></param>
        /// <param name="featureOID"></param>
        /// <param name="lstParm"></param>
        private void InsertIntoFieldInfo(IField fe, int featureOID, string tableName, List<LibCore.SQLParam> lstParm = null)
        {
            if (!fe.Editable)
                return;
            if (fe.FieldType == LibCore.eFieldType.eFieldTypeOID)
                return;
            if (lstParm == null)
            {
                lstParm = new List<SQLParam>
                {
                    new SQLParam() { ParamName = "ClassID", ParamValue = featureOID },
                    new SQLParam() { ParamName = "FieldName" },//, ParamValue = field.Name });
					new SQLParam() { ParamName = "FieldType" },//, ParamValue = field.Name });
					new SQLParam() { ParamName = "AliasName" },//, ParamValue = field.AliasName });
					new SQLParam() { ParamName = "DefaultDomainName" },//, ParamValue = field.Domain.Name });
					new SQLParam() { ParamName = "IsEditable" },//, ParamValue = field.Editable?1:0 });
					new SQLParam() { ParamName = "precision_" },//, ParamValue =null });
					new SQLParam() { ParamName = "scale" },//, ParamValue = null });
					new SQLParam() { ParamName = "TableName" }//, ParamValue = null });
				};
            }
            lstParm[1].ParamValue = fe.FieldName.ToLower().Trim();
            lstParm[2].ParamValue = (short)fe.FieldType;
            lstParm[3].ParamValue = fe.AliasName == null ? fe.FieldName : fe.AliasName.Trim();
            lstParm[4].ParamValue = fe.DomainName;
            lstParm[5].ParamValue = fe.Editable ? 1 : 0;
            lstParm[6].ParamValue = null;
            lstParm[7].ParamValue = null;
            lstParm[8].ParamValue = tableName;
            if (fe.FieldType == LibCore.eFieldType.eFieldTypeString)
            {
                lstParm[6].ParamValue = fe.Length > MAX_VARCHAR_LEN ? MAX_VARCHAR_LEN : fe.Length;
            }
            else if (fe.FieldType == LibCore.eFieldType.eFieldTypeSingle || fe.FieldType == LibCore.eFieldType.eFieldTypeDouble)
            {
                if (fe.Precision > 0)
                {
                    lstParm[6].ParamValue = fe.Precision;
                    lstParm[7].ParamValue = fe.Scale;
                }
                else
                {
                    lstParm[6].ParamValue = DEF_PRECISITION;
                    lstParm[7].ParamValue = DEF_SCALE;
                }
            }
            var sql = "delete from ime_FieldInfo where ClassID=" + featureOID + " and FieldName='" + fe.FieldName + "'";
            ExecuteNonQuery(sql);

            sql = "insert into ime_FieldInfo(ClassID,FieldName,FieldType,AliasName,DefaultDomainName,";
            sql += "IsEditable,precision_,scale,TableName)";
            sql += " values(@ClassID,@FieldName,@FieldType,@AliasName,@DefaultDomainName,@IsEditable,@precision_,@scale,@TableName)";
            ExecuteNonQuery(sql, lstParm);
        }
        /// <summary>
        /// 是否特殊字段名（系统保留的字段名）
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private bool IsSpatialFieldName(string fieldName, bool isFeatureClass)
        {
            if (isFeatureClass)
            {
                switch (fieldName.ToLower())
                {
                    case SpecialFieldNameConstants.objectid:
                    case SpecialFieldNameConstants.shape:
                    case SpecialFieldNameConstants.mingx:
                    case SpecialFieldNameConstants.mingy:
                    case SpecialFieldNameConstants.maxgx:
                    case SpecialFieldNameConstants.maxgy:
                        //case SpecialFieldNameConstants.shape_length:
                        //case SpecialFieldNameConstants.shape_area:
                        return true;
                }
            }
            else
            {
                return fieldName.ToLower() == SpecialFieldNameConstants.objectid;
            }
            return false;
        }

        #endregion

        #region hepler method

        /// <summary>
        /// 新建Sqlite数据库
        /// 连接串格式示例：
        ///     dbFileName:@"e:\test.db3"
        /// </summary>           
        public static void CreateDatabase(String dbFileName)
        {
            DBSQLite.CreatNewSQLite(dbFileName);
            using var db = new DBSQLite();
            db.Open(dbFileName);
            CreateSysTables.Create(db);
        }

        public Envelope GetFullExtent(string tableName, string shapeFieldName)
        {
            Envelope env = null;

            //var sql = string.Format("SELECT Min(MbrMinX(\"{0}\")) AS MinX, Min(MbrMinY(\"{0}\")) AS MinY, Max(MbrMaxX(\"{0}\")) AS MaxX, Max(MbrMaxY(\"{0}\")) AS MaxY FROM \"{1}\"", shapeFieldName, tableName);
            var sql = "SELECT min(mingx),min(mingy),max(maxgx),max(maxgy) FROM " + tableName
                + " where " + shapeFieldName + " is not null";
            base.QueryCallback(sql, r =>
            {
                if (r.IsDBNull(0) || r.IsDBNull(1) || r.IsDBNull(2) || r.IsDBNull(3))
                {
                    return false;
                }
                var xmin = r.GetDouble(0);
                var ymin = r.GetDouble(1);
                var xmax = r.GetDouble(2);
                var ymax = r.GetDouble(3);
                env = new Envelope(xmin, xmax, ymin, ymax);
                Console.WriteLine("env=" + env);
                return false;
            });
            return env;
        }

        public override List<Field> QueryFields2(string tableName, List<Field> fields = null)
        {
            var lst = base.QueryFields2(tableName, fields);

            var sa = "mingx,mingy,maxgx,maxgy".Split(',');
            lst.RemoveAll(it => sa.Contains(it.FieldName));
            if (IsTableExists("ime_FieldInfo"))
            {
                var shapeFieldName = QueryShapeFieldName(tableName);
                if (!string.IsNullOrEmpty(shapeFieldName))
                {
                    var xt = lst.Find(it => StringUtil.isEqualIgnorCase(it.FieldName, shapeFieldName));
                    if (xt != null)
                    {
                        xt.FieldType = eFieldType.eFieldTypeGeometry;
                        xt.GeometryType = QueryShapeType(tableName, shapeFieldName);
                    }
                }

                var sql = $"select FieldName,AliasName from ime_FieldInfo where TableName='{tableName}' and FieldName is not null";
                QueryCallback(sql, r =>
                 {
                     var fieldName = r.GetString(0);
                     var alias = r.GetString(1);
                     var xt = lst.Find(it => StringUtil.isEqualIgnorCase(fieldName, it.FieldName));
                     if (xt != null)
                     {
                         xt.AliasName = alias;
                     }
                     return true;
                 });
            }
            return lst;
        }

        /// <summary>
        /// 查询包含指定条件的数据集的最小外接矩形；
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="shapeFieldName"></param>
        /// <param name="where"></param>
        /// <param name="subFields">附加字段</param>
        /// <param name="callback">回调</param>
        /// <returns></returns>
        public Envelope QueryEnvelope(string tableName, string shapeFieldName, string where = null
            , List<string> subFields = null, Action<SQLiteDataReader> callback = null)
        {
            System.Diagnostics.Debug.Assert(IsOpen());
            GeoAPI.Geometries.Envelope env = null;
            var fields = shapeFieldName;// "AsBinary(" + shapeFieldName + ")";
            if (subFields != null)
            {
                foreach (var fieldName in subFields)
                {
                    fields += "," + fieldName;
                }
            }
            var sql = "select " + fields + " from " + tableName + " where " + shapeFieldName + " is not null";
            if (!string.IsNullOrEmpty(where))
            {
                sql += " and (" + where + ")";
            }
            base.QueryCallback(sql, r =>
            {
                var t = r.GetValue(0);
                var g = WKBHelper.FromBytes(t as byte[]);
                if (env == null)
                {
                    env = g.EnvelopeInternal;
                }
                else
                {
                    env.ExpandToInclude(g.EnvelopeInternal);
                }
                if (callback != null)
                {
                    callback(r);
                }
                return true;
            });
            return env;
        }

        /// <summary>
        /// 相交查询
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="shapeFieldName"></param>
        /// <param name="env"></param>
        /// <param name="callback"></param>
        /// <param name="subFields"></param>
        /// <param name="where"></param>
        //public void QueryIntersectsCallback(string tableName, string shapeFieldName, Envelope env, Func<SQLiteDataReader, bool> callback
        //    , List<string> subFields = null, string where = null)
        //{
        //    if (subFields == null)
        //    {
        //        subFields = base.QueryFields(tableName);
        //    }
        //    var sql = "select ";
        //    for (var i = 0; i < subFields.Count; ++i)
        //    {
        //        if (i > 0)
        //        {
        //            sql += ",";
        //        }
        //        var fieldName = subFields[i];
        //        sql += fieldName;
        //    }
        //    sql += " from " + tableName;
        //    var wh = ConstructConditionHelper.constructEnvelopeIntersectsCondition(env, null);
        //    sql += " where " + wh;
        //    if (!string.IsNullOrEmpty(where))
        //    {
        //        sql += " and (" + where + ")";
        //    }
        //    base.QueryCallback(sql, callback);
        //}

        public IGeometry GetShape(string tableName, string shapeFieldName, int rowid)
        {
            IGeometry g = null;
            var sql = string.Format("select {0} from {1} where rowid={2} and {0} is not null", shapeFieldName, tableName, rowid);
            QueryCallback(sql, r =>
            {
                g = MyWKBHelper.Bytes2Geometry(r.GetValue(0) as byte[]);
                return false;
            });
            return g;
        }

        public void DropTable(string tableName)
        {
            this.ExecuteNonQuery("drop table " + tableName);
            ExecuteNonQuery("delete from ime_FieldInfo where TableName='" + tableName + "'");
            ExecuteNonQuery("delete from ime_ObjectClasses where Name='" + tableName + "'");
        }

        /// <summary>
        /// 创建控件索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="shapeFieldName"></param>
        public void CreateSpatialIndex(string tableName, string shapeFieldName)
        {
            //var sql = "SELECT CreateSpatialIndex('" + tableName + "', '" + shapeFieldName + "')";
            //base.ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 删除空间索引
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="shapeFieldName"></param>
        public void DropSpatialIndex(string tableName, string shapeFieldName)
        {
            //bool fHasIndex = IsSpatialIndexEnabled(tableName);
            ////QueryCallback(string.Format("select spatial_index_enabled from geometry_columns where lower(f_table_name)='{0}'", tableName.ToLower()), r =>
            ////{
            ////    fHasIndex = 1 == r.GetInt32(0);
            ////    return false;
            ////});
            //if (fHasIndex)
            //{
            //    var sql = string.Format(" SELECT DisableSpatialIndex('{0}', '{1}')", tableName, shapeFieldName);
            //    ExecuteNonQuery(sql);
            //    sql = string.Format("DROP TABLE idx_{0}_{1}", tableName, shapeFieldName);
            //    ExecuteNonQuery(sql);
            //}
        }
        public bool IsSpatialIndexEnabled(string tableName)
        {
            return false;
            //bool fHasIndex = false;
            //QueryCallback(string.Format("select spatial_index_enabled from geometry_columns where lower(f_table_name)='{0}'", tableName.ToLower()), r =>
            //{
            //    fHasIndex = 1 == r.GetInt32(0);
            //    return false;
            //});
            //return fHasIndex;
        }
        /// <summary>
        /// 压缩数据库
        /// </summary>
        public void Vaccum()
        {
            ExecuteNonQuery("VACUUM");
        }
        /// <summary>
        /// 更新shape字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="shapeFieldName"></param>
        /// <param name="wh"></param>
        /// <param name="g"></param>
        public void UpdateShape(string tableName, string shapeFieldName, string wh, IGeometry g, int srid = -1)
        {
            var sql = "update " + tableName + " set " + shapeFieldName + "=@" + shapeFieldName;
            sql += ",mingx=@mingx,mingy=@mingy,maxgx=@maxgx,maxgy=@maxgy";
            if (g is IPolygon || g is IMultiPolygon)
            {
                sql += ",shape_length=@shape_length,shape_area=@shape_area";
            }
            else if (g is ILineString || g is IMultiLineString)
            {
                sql += ",shape_length=@shape_length";
            }
            sql += " where " + wh;
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 查询空间表的SRID，-1表示没有找到
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public int QuerySRID(string tableName)
        {
            #region 直接将SRID存放到DatasetID字段的情况 yxm 2018-5-21
            int srid = QueryOneInt("select DatasetID from ime_ObjectClasses where lower(Name)='" + tableName.ToLower() + "'");
            if (srid > 1000)
            {
                return srid;
            }
            #endregion

            var sql = string.Format("select srid from ime_FeatureDataset where id=(select DatasetID from ime_ObjectClasses where Name='{0}')", tableName.ToLower());
            QueryCallback(sql, r =>
            {
                if (!r.IsDBNull(0))
                {
                    srid = r.GetInt32(0);
                }
                return false;
            });
            return srid;
        }

        public eGeometryType QueryShapeType(string tableName, string shapeFieldName)
        {
            eGeometryType geoType = eGeometryType.eGeometryNull;
            //var sql = string.Format("select GeometryType({0}) from {1} where {0} is not null limit 1", shapeFieldName, tableName);
            var sql = string.Format("select GeometryType from ime_ObjectClasses where lower(Name)='{0}'", tableName.ToLower());
            QueryCallback(sql, r =>
            {
                var n = r.GetInt16(0);
                geoType = (eGeometryType)n;
                //var sgt = r.GetString(0);
                //switch (sgt)
                //{
                //    case "POLYGON":
                //    case "MULTIPOLYGON":
                //        geoType = eGeometryType.eGeometryPolygon;
                //        break;
                //    case "LINESTRING":
                //    case "MULTILINESTRING":
                //        geoType = eGeometryType.eGeometryPolyline;
                //        break;
                //    case "POINT":
                //    case "MULTIPOINT":
                //        geoType = eGeometryType.eGeometryPoint;
                //        break;
                //    default:
                //        System.Diagnostics.Debug.Assert(false, "todo...");
                //        break;
                //}
                return false;
            });
            return geoType;
        }
        public string QueryShapeFieldName(string tableName)
        {
            string shapeFieldName = null;
            if (IsTableExists("ime_ObjectClasses"))
            {
                //var sql = string.Format("select f_geometry_column from geometry_columns where lower(f_table_name)='{0}'", tableName.ToLower());
                var sql = $"select ShapeFieldName from ime_ObjectClasses where lower(Name)='{tableName.ToLower()}'";
                this.QueryCallback(sql, r =>
                {
                    shapeFieldName = r.GetString(0);
                    return false;
                });
            }
            return shapeFieldName;
        }

        //public static String constructSpatialFilterCondition(IFeatureClass fc, IQueryFilter filter, List<String> args)
        //{
        //    return ConstructConditionHelper.constructSpatialFilterCondition(fc, filter, args, "mingx", "mingy", "maxgx", "maxgy");
        //}
        #endregion
    }

    public class SQLiteWorkspace : WorkspaceBase, IWorkspace
    {
        protected readonly GeoDBSqlite _db = new();

        private SQLiteTransaction? _trans = null;

        public SQLiteWorkspace()
        {
            SqlFunc = _db.SqlFunc;
        }

        public void CreateDatabase(string dbFileName)
        {
            DBSQLite.CreatNewSQLite(dbFileName);
            Open(dbFileName);
        }

        public void Open(string strFileName)
        {
            _db.Open(strFileName);
        }
        public bool IsOpen()
        {
            return _db.IsOpen();
        }
        public GeoDBSqlite GetRawDB()
        {
            return _db;
        }
        public override int GetSRID(string sTableName)
        {
            return _db.QuerySRID(sTableName);
        }

        public override void QueryObjectClasses(Action<FeatureClassItem> callback)
        {
            _db.QueryObjectClasses(callback);
        }

        #region IDatabase
        public eDatabaseType DatabaseType
        {
            get
            {
                return eDatabaseType.SQLite;
            }
        }
        public string ConnectionString
        {
            get
            {
                return _db.ConnectionString;
            }
        }
        public override void CreateTable(string tableName, IFields fields, string aliasName = null, TableIndex[] index = null)
        {
            _db.CreateTable(tableName, fields);
        }
        public override void CreateFeatureClass(string tableName, IFields fields, int srid, string tableAliasName = null, TableIndex[] index = null)
        {
            _db.CreateFeatureClass(tableName, fields, srid, tableAliasName);
        }
        public override void QueryTables(Action<TableItem> callback)
        {
            var sql = $"select name from sqlite_master where type='table'";
            QueryCallback(sql, r =>
            {
                var tableName = r.GetString(0);
                var alias = tableName;// r.IsDBNull(1) ? "" : r.GetString(1);
                var oi = new TableItem
                {
                    TableName = tableName,
                    AliasName = alias,
                };
                callback(oi);
            });
        }

        public override void QueryCallback(string sql, Func<IDataReader, bool> callback, ICancelTracker cancel = null)
        {
            var ir = new DBSQLiteDataReader();
            _db.QueryCallback(sql, r =>
            {
                ir.Attach(r);
                var fContinue = callback(ir);
                return fContinue;
            });
        }

        public override int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            return _db.ExecuteNonQuery(sql, lstParameters);
        }

        public override void DropTable(string tableName)
        {
            _db.DropTable(tableName);
        }
        //#region yxm 2018-3-13

        //public override List<string> QueryFields(string tableName)
        //{
        //    return _db.QueryFields(tableName);
        //}

        //#endregion

        public override eGeometryType QueryShapeType(string tableName, string shapeFieldName)
        {
            eGeometryType geoType = eGeometryType.eGeometryNull;
            var sql = "select GeometryType from ime_objectclasses where lower(name)='" + tableName.ToLower() + "'";
            QueryCallback(sql, r =>
            {
                if (!r.IsDBNull(0))
                {
                    var sgt = r.GetInt16(0);// r.GetString(0);
                    geoType = (eGeometryType)sgt;
                }
                return false;
            });
            return geoType;
        }
        public override IGeometry GetShape(string tableName, string shapeFieldName, string oidFieldName, int oid)
        {
            IGeometry g = null;
            //var sql = string.Format("select {0} from {1} where rowid={2} and {0} is not null", shapeFieldName, tableName, oid);
            var sql = $"select {shapeFieldName} from {tableName} where rowid={oid} and {shapeFieldName} is not null";
            QueryCallback(sql, r =>
            {
                g = WKBHelper.FromBytes(r.GetValue(0) as byte[]);
                return false;
            });
            return g;
        }
        public override OkEnvelope GetFullExtent(string sTableName, string shapeFieldName)
        {
            OkEnvelope env = null;

            var sql = $"SELECT min(mingx),min(mingy),max(maxgx),max(maxgy) FROM {sTableName} where {shapeFieldName} is not null";
            _db.QueryCallback(sql, r =>
            {
                if (r.IsDBNull(0) || r.IsDBNull(1) || r.IsDBNull(2) || r.IsDBNull(3))
                {
                    return false;
                }
                var xmin = r.GetDouble(0);
                var ymin = r.GetDouble(1);
                var xmax = r.GetDouble(2);
                var ymax = r.GetDouble(3);
                env = new OkEnvelope(xmin, xmax, ymin, ymax);
                //Console.WriteLine("env=" + env);
                return false;
            });
            return env;
        }
        public override void UpdateShape(string tableName, string shapeFieldName, string oidFieldName, int oid, IGeometry g)
        {
            var srid = g.SRID;
            var sql = string.Format("update {0} set {1}={2} where {3}", tableName, shapeFieldName
                , "GeomFromText('" + g.AsText() + "'," + srid + ")", "rowid=" + oid);
            ExecuteNonQuery(sql);
        }

        public override string QueryShapeFieldName(string tableName)
        {
            return _db.QueryShapeFieldName(tableName);
        }

        #region 事务相关
        public override void BeginTransaction()
        {
            _trans = _db.BeginTransaction();
        }
        public override void Commit()
        {
            if (_trans != null)
            {
                _trans.Commit();
                _trans = null;
            }
        }
        public override void Rollback()
        {
            if (_trans != null)
            {
                _trans.Rollback();
                _trans = null;
            }
        }
        #endregion
        #endregion

        public override object ExecuteScalar(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            return _db.ExecuteScalar(sql, lstParameters);
        }
        public override List<Field> QueryFields2(string tableName, List<Field> fields = null)
        {
            return _db.QueryFields2(tableName, fields);
            //var lst = _db.QueryFields2(tableName, fields);

            ////var srid = rdb.QuerySRID(tableName);
            ////int nSRID = SafeConvertAux.ToInt32(srid);
            ////this.SpatialReference = SpatialReferenceFactory.CreateFromEpsgCode(nSRID);
            //var shapeFieldName = _db.QueryShapeFieldName(tableName);
            //if (!string.IsNullOrEmpty(shapeFieldName))
            //{
            //    var xt = lst.Find(it => StringUtil.isEqualIgnorCase(it.FieldName, shapeFieldName));
            //    if (xt != null)
            //    {
            //        xt.FieldType = eFieldType.eFieldTypeGeometry;
            //        xt.GeometryType = _db.QueryShapeType(tableName, shapeFieldName);
            //    }
            //}


            //return lst;
        }
        public override string QueryPrimaryKey(string tableName)
        {
            return _db.QueryPrimaryKey(tableName);
        }

        //public override int Insert<T>(T entity, SubFields subFields = null)
        //{
        //    var se = Orm.GetInsertSqlByEntity(entity, subFields);
        //    se.Sql += ";SELECT last_insert_rowid()";
        //    var o1 =_db.ExecuteScalar(se.Sql, se.Params);
        //    if (o1 != null)
        //    {
        //       var Oid = SafeConvertAux.ToInt32(o1);
        //        return Oid;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}

        public override void Dispose()
        {
            _db.Dispose();
        }
    }
}
