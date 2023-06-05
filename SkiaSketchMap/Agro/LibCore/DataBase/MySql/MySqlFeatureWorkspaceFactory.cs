using GeoAPI.Geometries;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
    /// <summary>
    /// yxm 2020-3-19
    /// </summary>
    public class MySqlFeatureWorkspaceFactory : FeatureWorkspaceFactoryBase, IFeatureWorkspaceFactory
    {
        private class ValueItem : DBMySql, IFeatureWorkspace
        {
            public int ReferenceCount = 0;

            internal Exception exception;

            public ValueItem(IFeatureWorkspaceFactory wf)
            {
                WorkspaceFactory = wf;
            }
            #region IFeatureWorkspace
            public IFeatureClass OpenFeatureClass(string tableName, string lpszAccess = "rb")
            {
                if (!base.IsTableExists(tableName))
                {
                    throw new Exception("表" + tableName + "不存在！");
                }
                var fc = new MySqlFeatureClass(this, (fc1) =>
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

        private MySqlFeatureWorkspaceFactory()
        {
        }
        public static readonly MySqlFeatureWorkspaceFactory Instance = new MySqlFeatureWorkspaceFactory();

        public override IFeatureWorkspace OpenWorkspace(string connectionString)
        {
            if (!_dic.TryGetValue(connectionString, out var vi))
            {
                try
                {
                    vi = new ValueItem(this);
                    vi.Connect(connectionString);
                    _dic[connectionString] = vi;
                }
                catch (Exception ex)
                {
                    if (vi != null)
                    {
                        vi.exception = ex;
                    }
                    throw ex;
                }
            }
            ++vi.ReferenceCount;
            return vi;
        }
        //public IFeatureClass CreateFeatureClass(string connectionString, string tableName, IFields fields)
        //{
        //    throw new NotImplementedException();
        //}
        //public void QueryObjectClasses(string connectionString, Action<FeatureClassItem> callback)
        //{
        //    using (var db = new DBMySql())
        //    {
        //        db.Connect(connectionString);
        //        db.QueryObjectClasses(callback);
        //    }
        //}
    }
    internal class MySqlFeature : Feature
    {
        public MySqlFeature(IFields fields = null)
            : base(fields)
        {

        }
    }
    public class MySqlFeatureClass : FeatureClassBase, IFeatureClass, IFeatureClassRender
    {

        private DBMySql _db
        {
            get
            {
                return Workspace as DBMySql;
            }
        }
        private OkEnvelope _fullExtent = null;
        private readonly MySqlFeature _recycleFeature = new MySqlFeature();

        public string ClassName
        {
            get
            {
                return this.GetType().Name;
            }
        }
        //public string TableName { get { return tableMeta.TableName; } }

        internal Action<IFeatureClass> OnDispose;
        public MySqlFeatureClass(DBMySql db, Action<IFeatureClass> onDispose)//string tableName)
            : base(db)
        {
            //_db = db;
            OnDispose = onDispose;
            //Fields = new Fields();
            //tableMeta.Fields = Fields;
            //ShapeFieldName = "SHAPE";
            //OIDFieldName = "objectid";
            _recycleFeature.Fields = new Fields();
        }

        //public string ConnectionString { get { return _db.ConnectionString; } }

        //public IWorkspace Workspace { get { return _db as IWorkspace; } }
        /// <summary>
        /// "Data Source=192.168.2.3;Initial Catalog=YuLinTuCQCJ77;User ID=sa;Password=123456;"
        /// </summary>
        /// <param name="connectionString"></param>
        //public void Open(string tableName)
        //{
        //          tableMeta=_db.QueryTableMeta(tableName);
        //          //tableMeta.TableName = tableName;
        //          //TableName = tableName;
        //          OIDFieldName = tableMeta.OIDFieldName;// _db.QueryOidFieldName(TableName);
        //          ShapeFieldName = tableMeta.ShapeFieldName;// _db.QueryShapeFieldName(TableName);
        //                                                    //var srid = _db.GetSRID(TableName);
        //                                                    //int nSRID = SafeConvertAux.ToInt32(srid);
        //          this.SpatialReference = tableMeta.SpatialReference;// SpatialReferenceFactory.CreateFromEpsgCode(nSRID);
        //          base.ShapeType = tableMeta.ShapeType;// _db.QueryShapeType(TableName, ShapeFieldName);
        //                                               //ResetFields(Fields);
        //          Fields = tableMeta.Fields;
        //	//tableMeta.SpatialReference = this.SpatialReference;
        //	base.FireOnOpened();
        //}

        //public bool IsOpen
        //{
        //	get
        //	{
        //		return _db.IsOpen();
        //	}
        //}
        public void CancelTask()
        {

        }
        //public IFields Fields { get; private set; }

        public OkEnvelope GetFullExtent()
        {
            if (_fullExtent == null)
            {
                var env = _db.GetFullExtent(TableName, ShapeFieldName);
                if (env != null)
                {
                    env.SpatialReference = this.SpatialReference;
                    _fullExtent = env;
                }
            }
            return _fullExtent;
        }

        //public IGeometry GetShape(int oid)
        //{
        //	var g = _db.GetShape(TableName, ShapeFieldName, OIDFieldName, oid);
        //	if (g != null)
        //	{
        //		g.SRID = SpatialReference == null ? 0 : SpatialReference.SRID;
        //	}
        //	return g;
        //}

        public override IFeature GetFeatue(int oid)
        {
            MySqlFeature feature = null;
            var subFields = FieldsUtil.GetFieldsString(this.Fields);
            subFields = MakeValidSubFields(subFields);
            var sql = "select " + subFields + " from " + this.TableName + " where " + this.OIDFieldName + "=" + oid;
            _db.QueryCallback(sql, r =>
            {
                feature = new MySqlFeature();
                ResetFeature(r, feature);
                return false;
            });
            return feature;
        }

        public bool SpatialQueryAsync(ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callbak)
        {
            bool fOK = true;
            using (var db = new DBMySql())
            {
                db.Connect(_db.ConnectionString);
                fOK = SpatialQuery(db, filter, cancel, callbak);
            }
            return fOK;
        }
        public override bool SpatialQuery(ISpatialFilter filter, Func<IFeature, bool> callbak, ICancelTracker cancel = null, bool fRecycle = true)
        {
            return SpatialQuery(_db, filter, cancel, callbak, fRecycle);
        }
        private bool SpatialQuery(DBMySql db, ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callbak, bool fRecycle = true)
        {
            var geo = filter.Geometry;
            geo.Project(this.SpatialReference);
            string geoRelation = null;
            switch (filter.SpatialRel)
            {
                case eSpatialRelEnum.eSpatialRelEnvelopeIntersects:
                    geoRelation = "MBRIntersects";//todo...
                    break;
                case eSpatialRelEnum.eSpatialRelIntersects:
                    geoRelation = "MBRIntersects";
                    break;
                case eSpatialRelEnum.eSpatialRelWithin:
                    geoRelation = "MBRWithin";
                    break;
                case eSpatialRelEnum.eSpatialRelContains:
                    geoRelation = "MBRContains";
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            var subFields = MakeValidSubFields(filter.SubFields);
            if (cancel == null)
            {
                cancel = new NotCancelTracker();
            }
            return DoSpatialQuery(db, geo, geoRelation, subFields, filter.WhereClause, filter.Oids, cancel, ft =>
            {
                var fok = true;
                switch (filter.SpatialRel)
                {
                    case eSpatialRelEnum.eSpatialRelIntersects:
                        fok = geo.Intersects(ft.Shape);
                        break;
                    case eSpatialRelEnum.eSpatialRelWithin:
                        fok = geo.Within(ft.Shape);
                        break;
                    case eSpatialRelEnum.eSpatialRelContains:
                        fok = geo.Contains(ft.Shape);
                        break;
                }
                return fok ? callbak(ft) : true;
            }, fRecycle);
        }

        public override void Search(IQueryFilter filter, Func<IRow, bool> callback)
        {
            Search(_db, filter, callback);
        }
        public void SearchAsync(IQueryFilter filter, Func<IRow, bool> callback)
        {
            using (var db = new DBMySql())
            {
                db.Connect(_db.ConnectionString);
                Search(db, filter, callback);
            }
        }
        public void Search(DBMySql db, IQueryFilter filter, Func<IRow, bool> callback)
        {
            var subFields = MakeValidSubFields(filter.SubFields);
            var sql = $"select {subFields} from {TableName}";

            bool fHasWhere = !string.IsNullOrEmpty(filter.WhereClause);
            if (fHasWhere)
            {
                sql += " where (" + filter.WhereClause + ")";
            }
            string sin = SqlUtil.ConstructIn(filter.Oids);
            if (sin != null)
            {
                if (fHasWhere)
                {
                    sql += " and " + OIDFieldName + " in(" + sin + ")";
                }
                else
                {
                    sql += " where " + OIDFieldName + " in(" + sin + ")";
                }
            }
            MySqlFeature row = null;
            int iOidField = -1;
            int iShapeField = -1;
            db.QueryCallback(sql, r =>
            {
                if (row == null)
                {
                    row = new MySqlFeature()
                    {
                        Fields = GetSubFields(r)
                    };
                    //row.ResetValueList();
                    //ResetFeature(r, row);

                    for (int i = 0; i < r.FieldCount; ++i)
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
                for (int i = 0; i < r.FieldCount; ++i)
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
                        row.Oid = r.GetInt32(i);
                        o = row.Oid;
                    }
                    else if (iShapeField == i)
                    {
                        if (o is byte[] wkb)
                        {
                            var nSrid = SpatialReference == null ? 0 : SpatialReference.SRID;
                            var geo = WKBHelper.FromBytes(wkb, nSrid);
                            _db.ValidShape(geo);
                            row.Shape = geo;
                            o = geo;
                        }
                    }
                    row.SetValue(i, o);
                }
                var fContinue = callback(row);
                return fContinue;
            });
        }

        //public int Count(string where)
        //{
        //	var sql = "select count(*) from " + this.TableName;
        //	if (!string.IsNullOrEmpty(where))
        //	{
        //		sql += " where " + where;
        //	}
        //	var n = _db.QueryOneInt(sql);
        //	return n;
        //}

        //public int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
        //{
        //	return _db.ExecuteNonQuery(sql, lstParameters);
        //}
        public IFeature CreateFeature()
        {
            var ft = new MySqlFeature();
            ResetFields(ft.Fields);
            ft.ResetValueList();
            return ft;
        }

        #region Edit
        //public override int Delete(IRow row)
        //{
        //	var sql = "delete from " + TableName + " where " + OIDFieldName + "=" + row.Oid;
        //	var n = _db.ExecuteNonQuery(sql);
        //	return n;
        //}
        public override int Append(IRow row)
        {
            var lstParameters = new List<SQLParam>();
            var sql = ConstructInsertSql(row, lstParameters);

            sql += ";select last_insert_id()";
            var o1 = _db.ExecuteScalar(sql, lstParameters);
            if (o1 != null)
            {
                row.Oid = SafeConvertAux.ToInt32(o1);
                row.GetOriginalValues().Clear();
                return 1;
            }
            else
            {
                return 0;
            }
        }
        //public override int Append(IEnumerable<IRow> adds, bool fUseTransaction = true)
        //{
        //	int nRes = 0;
        //	if (adds != null)
        //	{
        //		try
        //		{
        //                  if (fUseTransaction)
        //                  {
        //                      this._db.BeginTransaction();
        //                  }
        //			foreach (var r in adds)
        //			{
        //				nRes += Append(r);
        //			}
        //                  if (fUseTransaction)
        //                  {
        //                      _db.Commit();
        //                  }
        //		}
        //		catch (Exception ex)
        //		{
        //                  if (fUseTransaction)
        //                  {
        //                      _db.Rollback();
        //                  }
        //			throw ex;
        //		}
        //	}
        //	return nRes;
        //}

        //public override int Update(IRow row, bool fClearChangedValues = true)
        public override int Update(RowUpdateData row)
        {
            var lstParameters = new List<SQLParam>();
            var sql = ConstructUpdateSql(row, lstParameters);
            var n = _db.ExecuteNonQuery(sql, lstParameters);
            return n;
        }
        //public override int Edit(IEnumerable<IRow> adds, IEnumerable<IRow> updates, IEnumerable<IRow> deletes)
        //{
        //	this._db.BeginTransaction();
        //	int nRes = 0;
        //	string sql = null;
        //	try
        //	{
        //		#region update
        //		foreach (var r in updates)
        //		{
        //			nRes += Update(r);
        //		}
        //		#endregion

        //		#region delete rows
        //		int n = 0;
        //		string sin = null;
        //		foreach (var r in deletes)
        //		{
        //			if (sin == null)
        //			{
        //				sin = r.Oid.ToString();
        //			}
        //			else
        //			{
        //				sin += "," + r.Oid;
        //			}
        //			if (++n > 100)
        //			{
        //				sql = "delete from " + TableName + " where " + OIDFieldName + " in(" + sin + ")";
        //				nRes += _db.ExecuteNonQuery(sql);
        //				sin = null;
        //				n = 0;
        //			}
        //		}
        //		if (sin != null)
        //		{
        //			sql = "delete from " + TableName + " where " + OIDFieldName + " in(" + sin + ")";
        //			nRes += _db.ExecuteNonQuery(sql);
        //		}
        //		#endregion

        //		#region insert
        //		foreach (var r in adds)
        //		{
        //			nRes += Append(r);
        //		}
        //		#endregion

        //		_db.Commit();
        //	}
        //	catch (Exception ex)
        //	{
        //		try
        //		{
        //			_db.Rollback();
        //		}
        //		catch { }
        //		throw ex;
        //	}

        //	return nRes;
        //}

        public override void SqlBulkCopy(DataTable dt)
        {
            _db.SqlBulkCopyByDatatable(TableMeta, dt);
        }
        public int GetNextObjectID(int num = 1)
        {
            return _db.GetNextObjectID(TableName, num);
        }
        #endregion
        ///// <summary>
        /////按照SqlBulkCopy的方式插入数据（ 要求Objectid为自增长字段）
        ///// </summary>
        ///// <param name="adds"></param>
        ///// <returns></returns>
        //public int SqlBulkCopy(IEnumerable<IRow> adds)
        //{
        //	var dt = new DataTable(this.TableName);
        //	int iOidField = -1;
        //	int iShpField = -1;
        //	//int oid = GetNextObjectID();
        //	int cnt = adds.Count();
        //	//if (cnt > 1)
        //	//{
        //	//    GetNextObjectID(cnt - 1);
        //	//}
        //	#region init dt
        //	foreach (var r in adds)
        //	{
        //		var fields = r.Fields;
        //		for (int i = 0; i < fields.FieldCount; ++i)
        //		{
        //			var field = fields.GetField(i);
        //			Type type = null;
        //			switch (field.FieldType)
        //			{
        //				case eFieldType.eFieldTypeOID:
        //					iOidField = i;
        //					type = typeof(int);
        //					continue;
        //				case eFieldType.eFieldTypeGeometry:
        //					iShpField = i;
        //					type = typeof(SqlGeometry);
        //					break;
        //				case eFieldType.eFieldTypeInteger:
        //					type = typeof(int); break;
        //				case eFieldType.eFieldTypeSmallInteger:
        //					type = typeof(short); break;
        //				case eFieldType.eFieldTypeString:
        //					type = typeof(string); break;
        //				case eFieldType.eFieldTypeSingle:
        //				case eFieldType.eFieldTypeDouble:
        //					type = typeof(decimal);
        //					break;
        //				case eFieldType.eFieldTypeGUID:
        //					type = typeof(Guid);
        //					break;
        //				case eFieldType.eFieldTypeDate:
        //				case eFieldType.eFieldTypeDateTime:
        //					type = typeof(DateTime);
        //					break;
        //				case eFieldType.eFieldTypeBlob:
        //					type = typeof(byte[]);
        //					break;
        //			}
        //			dt.Columns.Add(field.FieldName, type);
        //		}
        //		break;
        //	}
        //	#endregion
        //	foreach (var r in adds)
        //	{
        //		var dr = dt.NewRow();
        //		var fields = r.Fields;
        //		for (int i = 0; i < fields.FieldCount; ++i)
        //		{
        //			if (i == iOidField)
        //			{
        //				//dr[i] = oid;
        //				continue;
        //			}
        //			else if (i == iShpField)
        //			{
        //				var o = r.GetValue(i);
        //				if (o is IGeometry g)
        //				{
        //					var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
        //					var sg = SqlGeometry.STGeomFromWKB(bc, SpatialReference == null ? 0 : this.SpatialReference.AuthorityCode);
        //					dr[i] = sg;
        //				}
        //				else
        //				{
        //					dr[i] = null;
        //				}
        //			}
        //			else
        //			{
        //				var o = r.GetValue(i);
        //				dr[i] = o;
        //			}
        //		}
        //		//++oid;
        //	}
        //	SqlBulkCopy(dt);
        //	return adds.Count();
        //}

        public void Dispose()
        {
            OnDispose(this);
        }

        private void ResetFeature(LibCore.IDataReader r, Feature feature)
        {
            if (feature.Fields.FieldCount == 0)
            {
                GetSubFields(r, feature.Fields);
                feature.ResetValueList();
            }
            else
            {
                feature.ClearValues();
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
                        feature.Oid = SafeConvertAux.ToInt32(o.ToString());
                        o = feature.Oid;
                    }
                    else if (i == iShapeField)
                    {
                        IGeometry g = null;
                        if (o is byte[] wkb)
                        {
                            //var bts = new byte[wkb.Length - 4];
                            //Array.Copy(wkb, 4, bts, 0, bts.Length);
                            //wkb = bts;
                            g = WKBHelper.FromBytes(wkb);
                            g.SetSpatialReference(this.SpatialReference);
                            _db.ValidShape(g);
                            g.SRID = SpatialReference == null ? 0 : SpatialReference.SRID;
                        }
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
        private void ResetFields(IFields fields)
        {
            fields.Clear();
            //var dicFieldAlias = QueryFieldAliasName();
            var lst = _db.QueryFields2(TableName);
            foreach (var field in lst)
            {
                if (field.FieldType == eFieldType.eFieldTypeGeometry)
                {
                    ShapeFieldName = field.FieldName;
                    field.GeometryType = this.ShapeType;
                    field.AliasName = "几何对象";
                }
                else if (field.FieldType == eFieldType.eFieldTypeInteger || field.FieldType == eFieldType.eFieldTypeBigInt)
                {
                    if (field.FieldName.Equals(OIDFieldName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        field.FieldType = eFieldType.eFieldTypeOID;
                        field.Editable = false;
                    }
                }
                fields.AddField(field);
            }
        }

        //private Dictionary<string, string> QueryFieldAliasName()
        //{
        //	return _db.QueryFieldAliasName(this.TableName);
        //}

        private IFields GetSubFields(LibCore.IDataReader r0, IFields fields = null)
        {
            var r = r0.GetRawDataReader() as MySqlDataReader;
            if (fields == null)
            {
                fields = new Fields();
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
                    var field = new Field
                    {
                        FieldName = fieldName,
                        AliasName = fieldName
                    };
                    if (StringUtil.isEqualIgnorCase(fieldName, OIDFieldName))
                    {
                        field.FieldType = eFieldType.eFieldTypeOID;
                    }
                    else if (StringUtil.isEqualIgnorCase(fieldName, ShapeFieldName))
                    {
                        field.FieldType = eFieldType.eFieldTypeGeometry;
                    }
                    else
                    {

                        var ft = r.GetFieldType(i);
                        if (ft == typeof(string))
                        {
                            field.FieldType = eFieldType.eFieldTypeString;
                        }
                        else if (ft == typeof(int))
                        {
                            field.FieldType = eFieldType.eFieldTypeInteger;
                        }
                        else if (ft == typeof(short))
                        {
                            field.FieldType = eFieldType.eFieldTypeSmallInteger;
                        }
                        else if (ft == typeof(double))
                        {
                            field.FieldType = eFieldType.eFieldTypeDouble;
                        }
                        else if (ft == typeof(float))
                        {
                            field.FieldType = eFieldType.eFieldTypeSingle;
                        }
                        else if (ft == typeof(Guid))
                        {
                            field.FieldType = eFieldType.eFieldTypeGUID;
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

            if (fieldName.Equals(ShapeFieldName, StringComparison.CurrentCultureIgnoreCase))
            {
                return _db.SqlFunc.AsBinary(ShapeFieldName);
            }
            return fieldName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="spatialRelation">Intersects,Within,...</param>
        /// <param name="subFields"></param>
        /// <param name="where"></param>
        /// <param name="cancel"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private bool DoSpatialQuery(DBMySql db, IGeometry geo, string spatialRelation, string subFields, string where
            , IObjectIDSet oids, ICancelTracker cancel, Func<IFeature, bool> callback, bool fRecycle)
        {
            //var srid = SpatialReference == null ? 0 : SpatialReference.AuthorityCode;
            var sql = "select " + subFields + " from " + TableName;
            var s = _db.SqlFunc.GeomFromText(geo);// $"ST_GeomFromText('{ geo.AsText()}',{srid})";
            sql += $" where {spatialRelation}({s},{this.ShapeFieldName})";
            if (!string.IsNullOrEmpty(where))
            {
                sql += " and (" + where + ")";
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
            MySqlFeature feature = _recycleFeature, preFeature = null;
            if (fRecycle)
            {
                _recycleFeature.Fields.Clear();
                //_recycleFeature.ClearValues();
            }

            db.QueryCallback(sql, r =>
            {
                if (cancel.Cancel())
                {
                    return false;
                }

                if (!fRecycle)
                {
                    feature = new MySqlFeature(preFeature?.Fields);
                    preFeature = feature;
                }

                ResetFeature(r, feature);

                if (oids != null && !oids.Contains(feature.Oid))
                {
                    return !cancel.Cancel();
                }
                var fContinue = callback(feature);
                return fContinue;
            }, cancel);
            return !cancel.Cancel();
        }

        private string ConstructInsertSql(IRow row, List<SQLParam> lstParameters)
        {
            string sFields = null;
            string sValues = "";
            var fields = this.Fields;
            for (int i = 0; i < fields.FieldCount; ++i)
            {
                var field = fields.GetField(i);
                if (!field.Editable)
                {
                    continue;
                }
                if (field.FieldType == eFieldType.eFieldTypeOID)
                {
                    continue;
                }
                var o = row.GetValue(i);
                if (o == null)
                {
                    continue;
                }

                bool fParamValue = false;

                bool fShapeField = o is IGeometry;
                if (fShapeField)
                {
                    var g = o as IGeometry;
                    g.Project(this.SpatialReference);
                }
                string shpValue = null;
                if (fShapeField)
                {
                    var g = o as IGeometry;
                    //var srid = 0;
                    //if (this.SpatialReference != null)
                    //{
                    //	srid = this.SpatialReference.AuthorityCode;
                    //}
                    shpValue = _db.SqlFunc.GeomFromText(g);// $"ST_GeomFromText('{g.AsText()}',{srid})";
                }
                if (sFields == null)
                {
                    sFields = field.FieldName;
                }
                else
                {
                    sFields += "," + field.FieldName;
                    sValues += ",";
                }
                if (fShapeField)
                {
                    sValues += shpValue;// ",geometry::STGeomFromText('" + g.AsText() + "'," + srid + ")";
                }
                else
                {
                    sValues += "@" + field.FieldName;
                    fParamValue = true;
                }
                #region commit

                #endregion
                if (fParamValue)
                {
                    var prm = new SQLParam() { ParamName = field.FieldName };
                    prm.ParamValue = o;
                    lstParameters.Add(prm);
                }
            }
            var sql = "insert into " + this.TableName + "(" + sFields + ")values(" + sValues + ")";
            return sql;
        }
        private string ConstructUpdateSql(RowUpdateData row, List<SQLParam> lstParameters)
        {
            return _db.ConstructUpdateSql(TableMeta, row, lstParameters, $"{OIDFieldName}={row.Oid}");
        }
    }
}

