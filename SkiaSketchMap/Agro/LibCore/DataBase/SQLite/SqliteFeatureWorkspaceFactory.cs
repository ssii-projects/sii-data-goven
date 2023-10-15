using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
//using System.Data.SQLite;

using Microsoft.Data.Sqlite;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using SQLiteTransaction = Microsoft.Data.Sqlite.SqliteTransaction;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;

namespace Agro.LibCore
{
    public class SqliteFeatureWorkspaceFactory : FeatureWorkspaceFactoryBase, IFeatureWorkspaceFactory
    {
        public class ValueItem : SQLiteWorkspace, IFeatureWorkspace
        {
            public int ReferenceCount;
            public ValueItem(IFeatureWorkspaceFactory wf)
            {
                WorkspaceFactory = wf;
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
                        Instance._dic.Remove(base.ConnectionString);
                    }
                }
            }
        }
        private readonly Dictionary<string, ValueItem> _dic = new Dictionary<string, ValueItem>();
        private SqliteFeatureWorkspaceFactory()
        {

        }
        public static readonly SqliteFeatureWorkspaceFactory Instance = new SqliteFeatureWorkspaceFactory();

        public override IFeatureWorkspace OpenWorkspace(string connectionString)
        {
            if (!_dic.TryGetValue(connectionString, out var vi))
            {
                vi = new ValueItem(this);
                _dic[connectionString] = vi;
                vi.Open(connectionString);
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
            GeoDBSqlite.CreateDatabase(connectionString);
            return OpenWorkspace(connectionString);
        }

        //public void QueryObjectClasses(string fileName, Action<FeatureClassItem> callback)
        //{
        //    using var db = new DBSQLite();
        //    db.Open(fileName);
        //    db.QueryObjectClasses(callback);
        //}
    }

    public class SqliteFeatureClass : FeatureClassBase, IFeatureClass, IFeatureClassRender
    {
        private class SqliteFeature : Feature
        {
            //private int _iMinxField = -1;
            //private int _iMinyField = -1;
            //private int _iMaxxField = -1;
            //private int _iMaxyField = -1;
            //private int _iShapeLengthField = -1;
            //private int _iShapeAreaField = -1;
            public SqliteFeature(IFields fields = null) : base(fields)
            {
                //ResetShapeIndexFields(fields);
            }
            protected override Feature CreateFeature(IFields fields)
            {
                return new SqliteFeature(fields);
            }
            //public override IRow Clone(bool fCloneFields = true)
            //{
            //    var ft= base.Clone(fCloneFields) as SqliteFeature;
            //    //_iMaxxField = ft._iMaxxField;
            //    //_iMaxyField = ft._iMaxyField;
            //    //_iMinxField = ft._iMinxField;
            //    //_iMinyField = ft._iMinyField;
            //    //_iShapeLengthField = ft._iShapeLengthField;
            //    //_iShapeAreaField = ft._iShapeAreaField;
            //    return ft;
            //}
            /// <summary>
            /// 根据Fields重置_fieldValues、_iOidField、_iShapeField等
            /// </summary>
            //internal override void ResetValueList()
            //{
            //    base.ResetValueList();
            //    ResetShapeIndexFields(Fields);
            //}
            //private void ResetShapeIndexFields(IFields fields)
            //{
            //    //if (fields != null)
            //    //{
            //    //    _iMinxField = fields.FindField("mingx");
            //    //    _iMinyField = fields.FindField("mingy");
            //    //    _iMaxxField = fields.FindField("maxgx");
            //    //    _iMaxyField = fields.FindField("maxgy");
            //    //}
            //}
            //public override void SetValue(int index, object Value)
            //{
            //    base.SetValue(index, Value);
            //    if (index == _iShapeField)
            //    {
            //        if (_iMaxxField >= 0 || _iMaxyField >= 0 || _iMinxField >= 0 || _iMinyField >= 0)// || _iShapeAreaField >= 0 || _iShapeLengthField >= 0)
            //        {
            //            double minx = 0, miny = 0, maxx = 0, maxy = 0;//, shpArea = 0, shpLen = 0;
            //            var g = Value as IGeometry;
            //            if (g != null)
            //            {
            //                var e = g.EnvelopeInternal;
            //                minx = e.MinX;
            //                miny = e.MinY;
            //                maxx = e.MaxX;
            //                maxy = e.MaxY;
            //                //shpArea = g.Area;
            //                //shpLen = g.Length;
            //            }
            //            if (_iMaxxField >= 0)
            //            {
            //                _fieldValues[_iMaxxField] = maxx;
            //            }
            //            if (_iMaxyField >= 0)
            //            {
            //                _fieldValues[_iMaxyField] = maxy;
            //            }
            //            if (_iMinxField >= 0)
            //            {
            //                _fieldValues[_iMinxField] = minx ;
            //            }
            //            if (_iMinyField >= 0)
            //            {
            //                _fieldValues[_iMinyField] = miny;
            //            }
            //            //if (_iShapeAreaField >= 0)
            //            //{
            //            //    _fieldValues[_iShapeAreaField] = shpArea;
            //            //}
            //            //if (_iShapeLengthField >= 0)
            //            //{
            //            //    _fieldValues[_iShapeLengthField] = shpLen;
            //            //}
            //        }
            //    }
            //}
        }

        private SQLiteWorkspace _db
        {
            get {
                return Workspace as SQLiteWorkspace;
            }
        }
        private OkEnvelope _fullExtent = null;
        private readonly Feature _recycleFeature;
        internal Action<IFeatureClass> OnDispose;
        private bool _fRendering = false;

        internal SqliteFeatureClass(SQLiteWorkspace db, Action<IFeatureClass> onDispose)
            : base(db)
        {
            OnDispose = onDispose;
            _recycleFeature = new SqliteFeature(new Fields());
        }

        public string ClassName
        {
            get {
                return this.GetType().Name;
            }
        }

        public void CancelTask()
        {

        }

        public OkEnvelope GetFullExtent()
        {
            if (_fullExtent == null)
            {
                var fe = _db.GetFullExtent(TableName, ShapeFieldName);
                if (fe != null)
                {
                    fe.SpatialReference = this.SpatialReference;
                    _fullExtent = fe;
                }
            }
            return _fullExtent;
        }
        public override IFeature GetFeatue(int oid)
        {
            SqliteFeature feature = null;
            var subFields = FieldsUtil.GetFieldsString(this.Fields);
            var sql =$"select {subFields} from {TableName} where {OIDFieldName}={oid}";
            _db.QueryCallback(sql, r =>
            {
                feature = new SqliteFeature();
                ResetFeature(r, feature);
                return false;
            });
            return feature;
        }

        private void ResetFeature(IDataReader r, Feature feature)
        {
            if (feature.Fields.FieldCount == 0)
            {
                getSubFields(r, feature.Fields);
                feature.ResetValueList();
            }
            var iOidField = feature._iOidField;
            var iShapeField = feature._iShapeField;
            for (var i = r.FieldCount; --i >= 0;)
            {
                if (r.IsDBNull(i))
                {
                    feature.SetValue(i, null);
                }
                else
                {
                    var o = r.GetValue(i);
                    if (i == iOidField)
                    {
                        //feature.Oid = (int)(r.GetRawDataReader() as System.Data.SQLite.SQLiteDataReader).GetInt64(i);
                        feature.Oid = (int)(r.GetRawDataReader() as Microsoft.Data.Sqlite.SqliteDataReader).GetInt64(i);

                        o = feature.Oid;
                    }
                    else if (i == iShapeField)
                    {
                        var nSrid = SpatialReference == null ? 0 : SpatialReference.SRID;
                        var g = WKBHelper.FromBytes(o as byte[], nSrid);
                        //if (base.GeometryOffset != null)
                        //{
                        //    var sr = g.GetSpatialReference();
                        //    g.Project(base.GeometryOffset.SpatialReference);
                        //    GeometryOffset.Apply(g);
                        //    g.Project(sr);
                        //}
                        feature.Shape = g;
                        o = g;
                    }
                    feature.SetValue(i, o);
                }
            }
            feature.GetOriginalValues().Clear();
        }

        public bool SpatialQueryAsync(ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callbak)
        {
            _fRendering = true;
            try
            {
                return DoSpatialQuery(filter, cancel, callbak);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
            }
            finally
            {
                _fRendering = false;
            }
            return false;
        }
        public override bool SpatialQuery(ISpatialFilter filter, Func<IFeature, bool> callbak, ICancelTracker cancel = null, bool fRecycle = true)
        {
            if (_fRendering)
            {
                return false;
            }
            return DoSpatialQuery(filter, cancel, callbak, fRecycle);
        }
        private bool DoSpatialQuery(ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callback, bool fRecycle = true)
        {
            var geo = filter.Geometry.Clone() as IGeometry;
            geo.Project(this.SpatialReference);

            //if (base.GeometryOffset != null)
            //{
            //    try
            //    {
            //        geo.Project(base.GeometryOffset.SpatialReference);
            //        base.GeometryOffset.Revert();
            //        GeometryOffset.Apply(geo);
            //        //geo.Apply(GeometryOffset);
            //        //geo.GeometryChanged();
            //        geo.Project(this.SpatialReference);
            //    }
            //    finally
            //    {
            //        base.GeometryOffset.Revert();
            //    }
            //}

            if (cancel == null)
            {
                cancel = new NotCancelTracker();
            }
            var subFields = filter.SubFields;
            if (string.IsNullOrEmpty(subFields))
            {
                for (int i = 0; i < Fields.FieldCount; ++i)
                {
                    var field = Fields.GetField(i);
                    if (string.IsNullOrEmpty(subFields))
                    {
                        subFields = field.FieldName;
                    }
                    else
                    {
                        subFields += "," + field.FieldName;
                    }
                }
            }
            var where = filter.WhereClause;
            var oids = filter.Oids;
            var env = geo.EnvelopeInternal;


            var sql = "select " + subFields + " from " + TableName;
            if (this.ShapeType == eGeometryType.eGeometryPoint)
            {
                sql += $" where (mingx>{env.MinX} and mingy>{env.MinY} and mingx<{env.MaxX} and mingy<{env.MaxY})";
            }
            else
            {
                sql += $" where (maxgx>{env.MinX} and maxgy>{env.MinY} and mingx<{env.MaxX} and mingy<{env.MaxY})";
            }
            if (!string.IsNullOrEmpty(where))
            {
                sql += " and(" + where + ")";
            }

            if (oids != null && oids.Count < 100)
            {
                var sin = SqlUtil.ConstructIn(oids);
                if (sin != null)
                {
                    sql += " and " + OIDFieldName + " in(" + sin + ")";
                }
                oids = null;
            }
            Feature feature = _recycleFeature, preFeature = null;
            if (fRecycle)
            {
                feature.Fields.Clear();
            }
            _db.QueryCallback(sql, r =>
            {
                try
                {
                    if (cancel.Cancel())
                    {
                        return false;
                    }
                    if (!fRecycle)
                    {
                        feature = new SqliteFeature(preFeature == null ? null : preFeature.Fields);
                        preFeature = feature;
                    }

                    ResetFeature(r, feature);

                    if (oids != null && !oids.Contains(feature.Oid))
                    {
                        return !cancel.Cancel();
                    }

                    geo = filter.Geometry;
                    geo.Project(this.SpatialReference);
                    bool fCheckOk = false;
                    if (feature.Shape != null && !feature.Shape.IsEmpty)
                    {
                        try
                        {
                            switch (filter.SpatialRel)
                            {
                                case eSpatialRelEnum.eSpatialRelEnvelopeIntersects:
                                    {
                                        fCheckOk = true;
                                    }
                                    break;
                                case eSpatialRelEnum.eSpatialRelIntersects:
                                    {
                                        fCheckOk = geo.Intersects(feature.Shape);
                                    }
                                    break;
                                case eSpatialRelEnum.eSpatialRelWithin:
                                    {
                                        fCheckOk = geo.Within(feature.Shape);
                                    }
                                    break;
                                case eSpatialRelEnum.eSpatialRelContains:
                                    {
                                        fCheckOk = geo.Contains(feature.Shape);
                                    }
                                    break;
                                default:
                                    System.Diagnostics.Debug.Assert(false);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    if (fCheckOk)
                    {
                        var fContinue = callback(feature);
                        return fContinue;
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return !cancel.Cancel();
            }, cancel);
            return true;
        }

        public override void Search(IQueryFilter filter, Func<IRow, bool> callback)
        {
            var subFields = MakeValidSubFields(filter.SubFields);
            var sql = $"select {subFields} from {TableName}";

            var fHasWhere = !string.IsNullOrEmpty(filter.WhereClause);
            if (fHasWhere)
            {
                sql += " where (" + filter.WhereClause + ")";
            }
            var sin = SqlUtil.ConstructIn(filter.Oids);
            if (sin != null)
            {
                if (fHasWhere)
                {
                    sql += $" and {OIDFieldName} in({sin})";
                }
                else
                {
                    sql += $" where {OIDFieldName} in({sin})";
                }
            }
            sql += " order by " + OIDFieldName;
            Feature row = null;// new Feature();
            var iOidField = -1;
            var iShapeField = -1;
            _db.QueryCallback(sql, dr =>
            {
                var r = dr.GetRawDataReader() as SQLiteDataReader;
                if (row == null)
                {
                    row = new SqliteFeature(getSubFields(dr));

                    for (var i = 0; i < r.FieldCount; ++i)
                    {
                        var fieldName = r.GetName(i);
                        if (fieldName.Equals(OIDFieldName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            iOidField = i;
                        }
                        else if (fieldName.Equals(ShapeFieldName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            iShapeField = i;
                        }
                        if (iOidField >= 0 && iShapeField >= 0)
                        {
                            break;
                        }
                    }
                }
                for (var i = 0; i < r.FieldCount; ++i)
                {
                    if (r.IsDBNull(i))
                    {
                        row.SetValue(i, null);
                        continue;
                    }
                    var fieldName = r.GetName(i);
                    var o = r.GetValue(i);
                    if (iOidField == i)
                    {
                        row.Oid = (int)r.GetInt64(i);
                        o = row.Oid;
                    }
                    else if (iShapeField == i)
                    {
                        try
                        {
                            var g = GeoDBSqlite.MyWKBHelper.Bytes2Geometry(o as byte[]);//, nSrid);
                            g.SetSpatialReference(SpatialReference);
                            o = g;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    row.SetValue(i, o);
                }
                row.GetOriginalValues().Clear();
                var fContinue = callback(row);
                return fContinue;
            });
        }
        public void SearchAsync(IQueryFilter filter, Func<IRow, bool> callback)
        {
            Search(filter, callback);
        }
        //public int Count(string where)
        //{
        //    var sql = "select count(*) from " + this.TableName;
        //    if (!string.IsNullOrEmpty(where))
        //    {
        //        sql += " where " + where;
        //    }
        //    var n = _db.QueryOneInt(sql);
        //    return n;// SafeConvertAux.ToInt32(o);
        //}

        //public int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
        //{
        //    return _db.ExecuteNonQuery(sql, lstParameters);
        //}
        #region  yxm 2017/5/10
        //public override int Delete(IRow row)
        //{
        //    var sql = "delete from " + TableName + " where rowid=" + row.Oid;
        //    var n = _db.ExecuteNonQuery(sql);
        //    row.GetChangedValues().Clear();
        //    return n;
        //}
        public override int Append(IRow row)
        {
            //resetParameters(row);
            var _lstParameters = new List<SQLParam>();
            //_lstParameters.Clear();
            var fields = row.Fields;
            string sFields = null;
            string sValues = null;
            IGeometry g = null;
            for (int i = 0; i < fields.FieldCount; ++i)
            {
                var field = fields.GetField(i);
                if (!field.Editable)
                {
                    continue;
                }
                var fieldName = field.FieldName;
                if (fieldName == "mingx" || fieldName == "mingy"
                    || fieldName == "maxgx" || fieldName == "maxgy")
                {
                    continue;
                }
                var o = row.GetValue(i);
                if (o is IGeometry)
                {
                    g = o as IGeometry;
                    g.Project(SpatialReference);
                    var bts = g.AsBinary();
                    o = bts;
                    //_lstParameters[n++].ParamValue = bts;
                }

                if (sFields == null)
                {
                    sFields = fieldName;
                    sValues = "@" + fieldName;
                }
                else
                {
                    sFields += "," + fieldName;
                    sValues += ",@" + fieldName;
                }
                _lstParameters.Add(new SQLParam() { ParamName = fieldName, ParamValue = o });
            }
            double minx = 0, miny = 0, maxx = 0, maxy = 0;
            //double shpLen = 0, shpArea = 0;
            if (g != null)
            {
                var env = g.EnvelopeInternal;
                minx = env.MinX;
                miny = env.MinY;
                maxx = env.MaxX;
                maxy = env.MaxY;
                //shpLen = g.Length;
                //shpArea = g.Area;
            }

            if (ShapeType != eGeometryType.eGeometryNull)
            {
                sFields += ",mingx,mingy";
                sValues += ",@mingx,@mingy";
                _lstParameters.Add(new SQLParam() { ParamName = "mingx", ParamValue = minx });
                _lstParameters.Add(new SQLParam() { ParamName = "mingy", ParamValue = miny });
                if (ShapeType != eGeometryType.eGeometryPoint)
                {
                    sFields += ",maxgx,maxgy";
                    sValues += ",@maxgx,@maxgy";
                    _lstParameters.Add(new SQLParam() { ParamName = "maxgx", ParamValue = maxx });
                    _lstParameters.Add(new SQLParam() { ParamName = "maxgy", ParamValue = maxy });
                }
            }
            var sql = $"insert into {TableName}({sFields})values({sValues})";


            int n = _db.ExecuteNonQuery(sql, _lstParameters);
            var oid = _db.GetRawDB().GetLastInsertRowID(TableName);
            row.Oid = oid;
            row.GetOriginalValues().Clear();
            return n;
        }
        //public override int Append(IEnumerable<IRow> adds,bool fUseTransaction=true)
        //{
        //    int nRes = 0;
        //    if (adds != null)
        //    {
        //        SQLiteTransaction trans = null;
        //        if (fUseTransaction)
        //        {
        //            trans = _db.GetRawDB().BeginTransaction();
        //        }
        //        foreach (var r in adds)
        //        {
        //            nRes += Append(r);
        //        }
        //        trans?.Commit();
        //    }
        //    return nRes;
        //}
        //public override int Update(IRow row, bool fClearChangedValues = true)
        //{
        //    #region yxm 2017/5/16
        //    var dic = row.GetOriginalValues();
        //    if (!(dic?.Count > 0))
        //    {
        //        return 0;
        //    }
        //    var lstParameters = new List<SQLParam>();
        //    var n = 0;
        //    var sql = $"update { TableName} set ";
        //    var iShapeField = -1;
        //    IGeometry g = null;
        //    foreach (var kv in dic)
        //    {
        //        var iField = kv.Key;
        //        var field = row.Fields.GetField(iField);
        //        if (!field.Editable)
        //        {
        //            continue;
        //        }
        //        var o = row.GetValue(iField);

        //        if (field.FieldType == eFieldType.eFieldTypeGeometry)
        //        {
        //            iShapeField = iField;
        //            g = o as IGeometry;
        //            if (g != null)
        //            {
        //                g.Project(SpatialReference);
        //                o = g.AsBinary();
        //            }
        //        }
        //        var fieldName = field.FieldName;
        //        if (n == 0)
        //        {
        //            sql += fieldName;
        //        }
        //        else
        //        {
        //            sql += "," + fieldName;
        //        }

        //        sql += "=@" + fieldName;
        //        lstParameters.Add(new SQLParam() { ParamName = fieldName, ParamValue = o });
        //        ++n;
        //    }
        //    if (iShapeField >= 0)
        //    {
        //        var field = Fields.GetField(iShapeField);
        //        var ShapeType = field.GeometryType;
        //        double minx = 0, miny = 0, maxx = 0, maxy = 0;
        //        if (g != null)
        //        {
        //            var e = g.EnvelopeInternal;
        //            minx = e.MinX;
        //            miny = e.MinY;
        //            maxx = e.MaxX;
        //            maxy = e.MaxY;
        //        }
        //        if (ShapeType != eGeometryType.eGeometryNull)
        //        {
        //            sql += ",mingx=@mingx,mingy=@mingy";
        //            lstParameters.Add(new SQLParam() { ParamName = "mingx", ParamValue = minx });
        //            lstParameters.Add(new SQLParam() { ParamName = "mingy", ParamValue = miny });
        //            if (ShapeType != eGeometryType.eGeometryPoint)
        //            {
        //                sql += ",maxgx=@maxgx,maxgy=@maxgy";
        //                lstParameters.Add(new SQLParam() { ParamName = "maxgx", ParamValue = maxx });
        //                lstParameters.Add(new SQLParam() { ParamName = "maxgy", ParamValue = maxy });
        //            }
        //        }
        //    }

        //    sql += " where rowid=" + row.Oid;
        //    #endregion
        //    n = _db.ExecuteNonQuery(sql, lstParameters);
        //    if (fClearChangedValues)
        //    {
        //        dic.Clear();
        //    }
        //    return n;
        //}

        public override int Update(RowUpdateData updateValues)
        {
            var lstParameters = new List<SQLParam>();
            var n = 0;
            var sql = $"update { TableName} set ";
            var iShapeField = -1;
            IGeometry g = null;
            foreach (var kv in updateValues.UpdateValues)
            {
                var iField = kv.Key;
                var field = Fields.GetField(iField);
                if (!field.Editable)
                {
                    continue;
                }
                var o = kv.Value;// row.GetValue(iField);

                if (field.FieldType == eFieldType.eFieldTypeGeometry)
                {
                    iShapeField = iField;
                    g = o as IGeometry;
                    if (g != null)
                    {
                        g.Project(SpatialReference);
                        o = g.AsBinary();
                    }
                }
                var fieldName = field.FieldName;
                if (n == 0)
                {
                    sql += fieldName;
                }
                else
                {
                    sql += "," + fieldName;
                }

                sql += "=@" + fieldName;
                lstParameters.Add(new SQLParam() { ParamName = fieldName, ParamValue = o });
                ++n;
            }
            if (iShapeField >= 0)
            {
                var field = Fields.GetField(iShapeField);
                var ShapeType = field.GeometryType;
                double minx = 0, miny = 0, maxx = 0, maxy = 0;
                if (g != null)
                {
                    var e = g.EnvelopeInternal;
                    minx = e.MinX;
                    miny = e.MinY;
                    maxx = e.MaxX;
                    maxy = e.MaxY;
                }
                if (ShapeType != eGeometryType.eGeometryNull)
                {
                    sql += ",mingx=@mingx,mingy=@mingy";
                    lstParameters.Add(new SQLParam() { ParamName = "mingx", ParamValue = minx });
                    lstParameters.Add(new SQLParam() { ParamName = "mingy", ParamValue = miny });
                    if (ShapeType != eGeometryType.eGeometryPoint)
                    {
                        sql += ",maxgx=@maxgx,maxgy=@maxgy";
                        lstParameters.Add(new SQLParam() { ParamName = "maxgx", ParamValue = maxx });
                        lstParameters.Add(new SQLParam() { ParamName = "maxgy", ParamValue = maxy });
                    }
                }
            }

            sql += " where rowid=" + updateValues.Oid;

            n = _db.ExecuteNonQuery(sql, lstParameters);
            return n;
        }

        //public override int Edit(IEnumerable<IRow> adds, IEnumerable<IRow> updates, IEnumerable<IRow> deletes)
        //{
        //    var trans = _db.GetRawDB().BeginTransaction();
        //    int nRes = 0;
        //    try
        //    {
        //        #region update
        //        if (updates != null)
        //        {
        //            foreach (var r in updates)
        //            {
        //                nRes += Update(r, false);
        //            }
        //        }
        //        #endregion

        //        #region delete rows
        //        if (deletes != null)
        //        {
        //            int n = 0;
        //            string sin = null;
        //            string sql;
        //            foreach (var r in deletes)
        //            {
        //                if (sin == null)
        //                {
        //                    sin = r.Oid.ToString();
        //                }
        //                else
        //                {
        //                    sin += "," + r.Oid;
        //                }
        //                if (++n > 100)
        //                {
        //                    sql = "delete from " + TableName + " where rowid in(" + sin + ")";
        //                    nRes += _db.ExecuteNonQuery(sql);
        //                    sin = null;
        //                    n = 0;
        //                }
        //            }
        //            if (sin != null)
        //            {
        //                sql = "delete from " + TableName + " where rowid in(" + sin + ")";
        //                nRes += _db.ExecuteNonQuery(sql);
        //            }
        //        }
        //        #endregion

        //        #region insert
        //        nRes += Append(adds,false);
        //        #endregion

        //        trans.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            trans.Rollback();
        //        }
        //        catch { }
        //        throw ex;
        //    }

        //    return nRes;
        //}
        #endregion
        public IFeature CreateFeature()
        {
            var ft = new SqliteFeature(TableMeta.Fields);
            //resetFields(ft.Fields);

            ft.ResetValueList();
            return ft;
        }

        private IFields getSubFields(IDataReader dr, IFields fields = null)
        {
            var r = dr.GetRawDataReader() as Microsoft.Data.Sqlite.SqliteDataReader;// System.Data.SQLite.SQLiteDataReader;
            if (fields == null)
            {
                fields = new Fields();
            }
            else
            {
                fields.Clear();
            }
            for (int i = 0; i < r.FieldCount; ++i)
            {
                string fieldName = r.GetName(i);
                int it = Fields.FindField(fieldName);
                if (it >= 0)
                {
                    fields.AddField(Fields.GetField(it));
                }
                else
                {
                    var field = new Field();
                    field.FieldName = fieldName;
                    field.AliasName = fieldName;
                    if (field.FieldName == this.ShapeFieldName)
                    {
                        field.FieldType = eFieldType.eFieldTypeGeometry;
                    }
                    else if (field.FieldName == this.OIDFieldName)
                    {
                        field.FieldType = eFieldType.eFieldTypeOID;
                    }
                    else
                    {
                        var ft = r.GetFieldType(i);
                        if (ft == typeof(string))
                        {
                            field.FieldType = eFieldType.eFieldTypeString;
                        }
                        else if (ft == typeof(Byte[]))
                        {
                            field.FieldType = eFieldType.eFieldTypeBlob;
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(false, "todo...");
                        }
                    }
                    fields.AddField(field);
                }
            }
            return fields;
        }
        private string MakeValidSubFields(string subFields)
        {
            string str = null;
            if (string.IsNullOrEmpty(subFields))
            {
                for (int i = 0; i < Fields.FieldCount; ++i)
                {
                    string fieldName = Fields.GetField(i).FieldName;
                    string s = MakeValidQueryFieldName(fieldName);
                    if (str == null)
                    {
                        str = s;
                    }
                    else
                    {
                        str += "," + s;
                    }
                }
            }
            else
            {
                var sa = subFields.Split(',');
                foreach (var fieldName in sa)
                {
                    string s = MakeValidQueryFieldName(fieldName);
                    if (str == null)
                    {
                        str = s;
                    }
                    else
                    {
                        str += "," + s;
                    }
                }
            }
            return str;
        }
        private string MakeValidQueryFieldName(string fieldName)
        {
            //if (fieldName.Equals(ShapeFieldName, StringComparison.CurrentCultureIgnoreCase))
            //{
            //    return "AsBinary(" + fieldName + ") as " + fieldName;
            //}
            //else
            //{
            return fieldName;
            //}
        }

        public void Dispose()
        {
            OnDispose(this);
        }
    }
}
