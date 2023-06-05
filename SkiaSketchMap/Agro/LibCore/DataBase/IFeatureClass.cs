using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Agro.LibCore
{
    public interface IObjectIDSet
    {
        int Count { get; }
        void Add(int oid, bool fFireChange = true);
		bool Remove(int oid, bool fFireChange = true);
        //void AddRange(IEnumerable<int> range);
        bool Contains(int oid);
        void Get(Func<int, bool> callback);
        void Clear();
        void FireChange();
    }

    public interface IRow
    {
        int Oid { get; set; }
        IFields Fields { get; }
        object GetValue(int index);
        void SetValue(int index, object Value);
        IRow Clone(bool fCloneFields = true);
        /// <summary>
        /// [iField,变化前的数据]
        /// </summary>
        /// <returns></returns>
        Dictionary<int, object> GetOriginalValues();
    }
    public interface IFeature : IRow
    {
        IGeometry? Shape { get; set; }
    }
	public class TableMeta
	{
		public string TableName { get; set; }
		public string PrimaryKey { get; set; }
		public IFields Fields { get; set; } = new Fields();
		public SpatialReference SpatialReference { get; set; }
        public string OIDFieldName
        {
            get; set;
        }
        public string ShapeFieldName
        {
            get; set;
        }
        public eGeometryType ShapeType
        {
            get; set;
        }

        public TableMeta(string tableName)
		{
			TableName = tableName;
		}
	}

    public class RowUpdateData
    {
        public int Oid;
        public readonly Dictionary<int, object> UpdateValues=new Dictionary<int, object>();

        public static RowUpdateData GetOriginalValues(IRow row)
        {
            var d = new RowUpdateData()
            {
                Oid=row.Oid
            };
            foreach (var kv in row.GetOriginalValues())
            {
                d.UpdateValues[kv.Key] = kv.Value;
            }
            return d;
        }
        public static RowUpdateData GetChangedValues(IRow row)
        {
            var d = new RowUpdateData()
            {
                Oid = row.Oid
            };
            foreach (var kv in row.GetOriginalValues())
            {
                var iField = kv.Key;
                d.UpdateValues[iField] = row.GetValue(iField);
            }
            return d;
        }

        public static RowUpdateData GetAllValues(IRow row)
        {
            var d = new RowUpdateData()
            {
                Oid = row.Oid
            };
            for(var i=row.Fields.FieldCount;--i>=0;)
            {
                var iField = i;
                d.UpdateValues[iField] = row.GetValue(iField);
            }
            return d;
        }
    }

    public interface ITable : IDisposable
    {
        eDatabaseType DatabaseType { get; }
        /// <summary>
        /// 实现类的名称
        /// </summary>
        string ClassName { get; }
        string ConnectionString { get; }

        IWorkspace Workspace { get; }
        //void Open(string connectionString, string lpszAccess = "rb");
        string TableName
        {
            get;
        }
        string AliasName
        {
            get;
            set;
        }

        string OIDFieldName { get; set; }

        IFields Fields { get; }
        bool IsOpen
        {
            get;
        }
        Action OnOpened { get; set; }
        void Search(IQueryFilter filter, Func<IRow, bool> callback);
		void Search(IQueryFilter filter, Action<IRow> callback);
		void SearchAsync(IQueryFilter filter, Func<IRow, bool> callback);
		int Count(string where);
        int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null);
        #region yxm 2017/5/10
        int Append(IRow row);
        int Append(IEnumerable<IRow> adds, bool fUseTransaction = true);
        int Delete(IRow row);
        int Delete(IEnumerable<IRow> deletes, bool fUseTransaction = true);
        /// <summary>
        /// 废弃
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fClearChangedValues"></param>
        /// <returns></returns>
        int Update(IRow row, bool fClearChangedValues = true);

        #region  yxm 2021-4-14 为撤销/重做提供支持
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateValues">[fieldIndex,fieldValue]</param>
        /// <returns></returns>
        int Update(RowUpdateData updateValues);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adds"></param>
        /// <param name="deletes"></param>
        /// <param name="updates"></param>
        /// <returns></returns>
        int Edit(IEnumerable<IRow> adds,IEnumerable<IRow> deletes, IEnumerable<RowUpdateData> updates);
        #endregion

        ///// <summary>
        ///// 废弃
        ///// </summary>
        ///// <param name="adds"></param>
        ///// <param name="updates"></param>
        ///// <param name="deletes"></param>
        ///// <returns></returns>
        //int Edit(IEnumerable<IRow> adds, IEnumerable<IRow> updates, IEnumerable<IRow> deletes);
        int ClearAll();
        #endregion
    }

    public enum eSpatialRelEnum
    {
        eSpatialRelUndefined = 0,
        eSpatialRelIntersects = 1,
        eSpatialRelEnvelopeIntersects = 2,
        eSpatialRelIndexIntersects = 3,
        eSpatialRelTouches = 4,
        eSpatialRelOverlaps = 5,
        eSpatialRelCrosses = 6,
        eSpatialRelWithin = 7,
        eSpatialRelContains = 8,
        eSpatialRelRelation = 9,
    }
    public enum eSearchOrder
    {
        eSearchOrderSpatial = 0,
        eSearchOrderAttribute = 1,
    }
    public interface IQueryFilter
    {
        string SubFields { get; set; }
        string WhereClause { get; set; }
        /// <summary>
        /// 如果设置了该属性则结果集的Objectid必须包含于Oids集合
        /// </summary>
        IObjectIDSet Oids { get; set; }
        //void AddField(string subField);
        //ISpatialReference get_OutputSpatialReference(string FieldName);
        //void set_OutputSpatialReference(string FieldName, ISpatialReference OutputSpatialReference);
    }
    public interface ISpatialFilter : IQueryFilter
    {
        /// <summary>
        /// IGeometry or Envelope
        /// </summary>
        IGeometry Geometry { get; set; }
        string GeometryField { get; set; }
        eSearchOrder SearchOrder { get; set; }
        eSpatialRelEnum SpatialRel { get; set; }
        string SpatialRelDescription { get; set; }
    }
    public interface IFeatureClassRender
    {
        /// <summary>
        /// 异步空间查询，只用于绘制
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancel"></param>
        /// <param name="callbak"></param>
        /// <returns></returns>
        bool SpatialQueryAsync(ISpatialFilter filter, ICancelTracker cancel, Func<IFeature, bool> callbak);

    }
    //public class GeometryOffset
    //{
    //    public double OffsetX;
    //    public double OffsetY;
    //    public SpatialReference SpatialReference;
    //}
    public interface IFeatureClass:ITable
    {
        eGeometryType ShapeType { get; set; }
        OkEnvelope GetFullExtent();
        string ShapeFieldName { get; set;}
        IGeometry GetShape(int oid);
        SpatialReference SpatialReference { get; set; }
        bool SpatialQuery(ISpatialFilter filter, Func<IFeature, bool> callbak, ICancelTracker cancel=null, bool fRecycle=true);
		bool SpatialQuery(ISpatialFilter filter, Action<IFeature> callbak, ICancelTracker cancel=null, bool fRecycle = true);
		void CancelTask();
        IFeature CreateFeature();
        IFeature GetFeatue(int oid);
        //OffsetFilter GeometryOffset { get; set; }
        void UpdateShape(int oid, IGeometry g);
        void RecalcFullExtent();
        Action OnStartBuildIndex { get; set; }
        Action OnEndBuildIndex { get; set; }
        void SqlBulkCopy(DataTable dt);
    }
    public class QueryFilterBase 
    {

        public string SubFields
        {
            get;
            set;
        }

        public string? WhereClause
        {
            get;
            set;
        }

        public IObjectIDSet? Oids
        {
            get;
            set;
        }
    }
    public class QueryFilter : QueryFilterBase, IQueryFilter
    {

    }
    public class SpatialFilter :QueryFilterBase, ISpatialFilter
    {

        public IGeometry Geometry
        {
            get;
            set;
        }

        public string GeometryField
        {
            get;
            set;
        }

        public eSearchOrder SearchOrder
        {
            get;
            set;
        }

        public eSpatialRelEnum SpatialRel
        {
            get;
            set;
        }

        public string SpatialRelDescription
        {
            get;
            set;
        }

        //public string SubFields
        //{
        //    get;
        //    set;
        //}

        //public string WhereClause
        //{
        //    get;
        //    set;
        //}
        //public IObjectSet Oids { get; set; }
    }
	//public class Field:IField
	//{
	//    private string _aliasName;
	//    public string FieldName
	//    {
	//        get;
	//        set;
	//    }
	//    public string AliasName
	//    {
	//        get
	//        {
	//            return _aliasName==null?FieldName:_aliasName;
	//        }
	//        set
	//        {
	//            _aliasName = value;
	//        }
	//    }
	//    public eFieldType FieldType
	//    {
	//        get;
	//        set;
	//    }
	//    public eGeometryType GeometryType { get; set; }
	//    public bool Editable { get; set; }
	//    public bool IsNullable { get; set; }
	//    public int Length { get; set; }
	//    public int Precision { get; set; }
	//    public int Scale { get; set; }
	//    public string DomainName
	//    {
	//        get;
	//        set;
	//    }
	//    public Field()
	//    {
	//        IsNullable = true;
	//        Editable = true;
	//    }
	//}
	//public class Fields : IFields
	//{
	//    private readonly List<IField> _fields = new List<IField>();
	//    public int FieldCount { get { return _fields.Count; } }

	//    public int FindField(string Name)
	//    {
	//        return _fields.FindIndex(fi =>
	//        {
	//            return StringUtil.isEqualIgnorCase(Name,fi.FieldName);
	//        });
	//    }
	//    public int FindFieldByAliasName(string Name)
	//    {
	//        return _fields.FindIndex(fi =>
	//        {
	//            return Name.Equals(fi.AliasName);
	//        });
	//    }
	//    public IField GetField(int Index)
	//    {
	//        if (Index < 0 || Index >= _fields.Count)
	//            return null;
	//        return _fields[Index];
	//    }
	//    public void AddField(IField field)
	//    {
	//        _fields.Add(field);
	//    }
	//    public void RemoveField(IField field)
	//    {
	//        _fields.Remove(field);
	//    }
	//    public void Clear()
	//    {
	//        _fields.Clear();
	//    }
	//}
	public class RowBase// : IRow
	{
		protected List<object> _fieldValues = new List<object>();
		protected IFields _fields;
		internal int _iShapeField = -1;
		internal int _iOidField = -1;
		protected readonly Dictionary<int, object> _dicOriginalValue = new Dictionary<int, object>();
		protected RowBase(IFields fields = null)
		{
			_fields = fields ?? new Fields();
			ResetValueList();
		}
		public int Oid
		{
			get
			{
				if (_iOidField >= 0)
				{
					var o = GetValue(_iOidField);
					return (int)o;
				}
				//System.Diagnostics.Debug.Assert(false);
				return -1;
			}
			set
			{
				if (_iOidField >= 0)
				{
					SetValue(_iOidField, value);
				}
			}
		}

		public IFields Fields
		{
			get
			{
				return _fields;
			}
			internal set
			{
				System.Diagnostics.Debug.Assert(value != null);
				_fields = value;
				ResetValueList();
			}
		}

		public object GetValue(int index)
		{
			if (index >= 0 && index < _fieldValues.Count)
			{
				return _fieldValues[index];
			}
			return null;
		}

		public virtual void SetValue(int index, object Value)
		{
			if (index >= 0 && index < _fieldValues.Count)
			{
				var field = Fields.GetField(index);
				if (field.Editable)
				{
                    if (!_dicOriginalValue.ContainsKey(index))
                    {
                        _dicOriginalValue[index] = _fieldValues[index];
                    }
                }
				_fieldValues[index] = Value;
			}
		}
		public Dictionary<int, object> GetOriginalValues()
		{
			return _dicOriginalValue;
		}

		internal virtual void ResetValueList()
		{
			_fieldValues.Clear();
			_dicOriginalValue.Clear();
			_iOidField = -1;
			_iShapeField = -1;
			if (Fields != null)
			{
				for (int i = 0; i < Fields.FieldCount; ++i)
				{
					var field = Fields.GetField(i);
					if (field.FieldType == eFieldType.eFieldTypeGeometry)
					{
						_iShapeField = i;
					}
					else if (field.FieldType == eFieldType.eFieldTypeOID)
					{
						_iOidField = i;
						_fieldValues.Add(0);
						continue;
					}
					_fieldValues.Add(null);
				}
			}
		}
		internal void ClearValues()
		{
			for (var i =_fieldValues.Count; --i>=0;)
			{
				_fieldValues[i] = null;
			}
			_dicOriginalValue.Clear();
		}
	}

	public class Row : RowBase, IRow
	{
		public Row(IFields fields = null)
			: base(fields)
		{
		}
		public IRow Clone(bool fCloneFields = true)
		{
			var ft = new Row(fCloneFields ? _fields.Clone() : _fields)
			{
				_iShapeField = _iShapeField,
				_iOidField = _iOidField
			};
			ft._fieldValues.Clear();
			foreach (var o in _fieldValues)
			{
				ft._fieldValues.Add(o);
			}
			return ft;
		}
	}

	public class Feature : RowBase, IFeature
    {
        //protected List<object> _fieldValues = new List<object>();
        //private IFields _fields;
        //internal int _iShapeField=-1;
        //internal int _iOidField = -1;
        //protected readonly Dictionary<int, object> _dicChangedValue = new Dictionary<int, object>();

        ////protected Action<int, IField> OnResetValue;
        //public int Oid
        //{
        //    get
        //    {
        //        if (_iOidField >= 0)
        //        {
        //            var o = GetValue(_iOidField);
        //            return (int)o;
        //        }
        //        System.Diagnostics.Debug.Assert(false);
        //        return -1;
        //    }
        //    set
        //    {
        //        if (_iOidField >= 0)
        //        {
        //            SetValue(_iOidField, value);
        //        }
        //    }
        //}
        public IGeometry Shape
        {
            get
            {
                if (_iShapeField >= 0)
                {
                    return GetValue(_iShapeField) as IGeometry;
                }
                return null;
            }
            set
            {
                if (_iShapeField >= 0)
                {
                    SetValue(_iShapeField, value);
                }
            }
        }

        //public IFields Fields
        //{
        //    get
        //    {
        //        return _fields;
        //    }
        //    internal set
        //    {
        //        System.Diagnostics.Debug.Assert(value != null);
        //        _fields = value;
        //        ResetValueList();
        //    }
        //}

        protected Feature(IFields fields=null)
			:base(fields)
        {
            //_fields = fields;
            //if (_fields == null)
            //{
            //    _fields = new Fields();
            //}
            //ResetValueList();
        }

        //public object GetValue(int index)
        //{
        //    if (index >= 0 && index < _fieldValues.Count)
        //    {
        //        return _fieldValues[index];
        //    }
        //    return null;
        //}

        //public virtual void SetValue(int index, object Value)
        //{
        //    if (index >= 0 && index < _fieldValues.Count)
        //    {
        //        var field=Fields.GetField(index);
        //        if (field.Editable)
        //        {
        //            //if (!_dicChangedValue.ContainsKey(index))
        //            //{
        //                _dicChangedValue[index] = _fieldValues[index];
        //            //}
        //        }
        //        _fieldValues[index]=Value;

        //    }
        //}
        protected virtual Feature CreateFeature(IFields fields)
        {
            return new Feature(_fields);
        }
		public virtual IRow Clone(bool fCloneFields = true)
		{
			var ft = CreateFeature(_fields);
			if (fCloneFields)
			{
				ft._fields = new Fields();
				for (int i = 0; i < _fields.FieldCount; ++i)
				{
					ft._fields.AddField(_fields.GetField(i));
				}
			}
			ft._iShapeField = _iShapeField;
			ft._iOidField = _iOidField;
			ft._fieldValues.Clear();
			foreach (var o in _fieldValues)
			{
				ft._fieldValues.Add(o);
			}
			return ft;
		}
		//public Dictionary<int, object> GetChangedValues()
		//{
		//    return _dicChangedValue;
		//}
		/// <summary>
		/// 根据Fields重置_fieldValues、_iOidField、_iShapeField等
		/// </summary>
    }
    public abstract class FeatureClassBase
    {
        public eDatabaseType DatabaseType
        {
            get {
                return Workspace.DatabaseType;
            }
        }
        public TableMeta TableMeta
        {
            get;protected set;
        }
        public string TableName
        {
            get {
                return TableMeta.TableName;
            }
        }
        public string AliasName
        {
            get;
            set;
        }

        public string OIDFieldName
        {
            get;
            set;
        }
        public string ShapeFieldName { get; set; }
        public eGeometryType ShapeType { get; set; }
        public Action OnOpened { get; set; }

        public Action OnStartBuildIndex { get; set; }
        public Action OnEndBuildIndex { get; set; }

        public SpatialReference SpatialReference
        {
            get;
            set;
            //get { return _spatialReference; }
            //set
            //{
            //    _spatialReference = value;
            //    if (OnSpatialReferenceChanged != null)
            //    {
            //        OnSpatialReferenceChanged(value);
            //    }
            //}
        }
        public virtual void SqlBulkCopy(DataTable dt)
        {
            throw new NotImplementedException();
        }
        public IWorkspace Workspace
        {
            get; private set;
        }
        public string ConnectionString
        {
            get {
                return Workspace.ConnectionString;
            }
        }
        public IFields Fields
        {
            get {
                return TableMeta.Fields;
            }
        }
        protected FeatureClassBase(IWorkspace db)
        {
            //DatabaseType = dt;
            Workspace = db;
            
        }
        public bool IsOpen
        {
            get {
                return Workspace.IsOpen();
            }
        }
        public virtual IGeometry GetShape(int oid)
        {
            var g = Workspace.GetShape(TableName, ShapeFieldName, OIDFieldName, oid);
            if (g != null)
            {
                g.SRID = SpatialReference == null ? 0 : SpatialReference.SRID;
            }
            return g;
        }
        public virtual IFeature GetFeatue(int oid)
        {
            return null;
        }

        //public OffsetFilter GeometryOffset { get; set; }

        public void Open(string tableName)
        {
            TableMeta = Workspace.QueryTableMeta(tableName);
            OIDFieldName = TableMeta.OIDFieldName;
            ShapeFieldName = TableMeta.ShapeFieldName;// _db.QueryShapeFieldName(TableName);
                                                      //var srid = _db.GetSRID(TableName);
                                                      //int nSRID = SafeConvertAux.ToInt32(srid);
            this.SpatialReference = TableMeta.SpatialReference;// SpatialReferenceFactory.CreateFromEpsgCode(nSRID);
            ShapeType = TableMeta.ShapeType;// _db.QueryShapeType(TableName, ShapeFieldName);
                                                 //ResetFields(Fields);
            //Fields = tableMeta.Fields;
            //tableMeta.SpatialReference = this.SpatialReference;
            FireOnOpened();
        }


        #region  yxm 2017/5/10
        public virtual int Append(IRow row)
        {
            return 0;
        }
        public virtual int Append(IEnumerable<IRow> adds, bool fUseTransaction = true)
        {
            var nRes = 0;
            if (adds != null)
            {
                var db = Workspace;
                try
                {
                    if (fUseTransaction)
                    {
                        db.BeginTransaction();
                    }
                    foreach (var r in adds)
                    {
                        nRes += Append(r);
                    }
                    if (fUseTransaction)
                    {
                        db.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (fUseTransaction)
                    {
                        db.Rollback();
                    }
                    throw ex;
                }
            }
            return nRes;
        }
        public virtual int Delete(IRow row)
        {
            var sql = $"delete from {TableName} where {OIDFieldName}={row.Oid}";
            var n = Workspace.ExecuteNonQuery(sql);
            row.GetOriginalValues().Clear();
            return n;
        }
        public virtual int Delete(IEnumerable<IRow> deletes, bool fUseTransaction = true)
        {
            var nRes = 0;
            if (deletes?.Count()>0)
            {
                var db = Workspace;
                try
                {
                    if (fUseTransaction)
                    {
                        db.BeginTransaction();
                    }
                    SqlUtil.ConstructIn(deletes, sin =>
                    {
                        var sql = $"delete from {TableName} where {OIDFieldName} in({sin})";
                        nRes += db.ExecuteNonQuery(sql);
                    }, it => it.Oid);
                    if (fUseTransaction)
                    {
                        db.Commit();
                    }
                }
                catch (Exception ex)
                {
                    if (fUseTransaction)
                    {
                        try
                        {
                            db.Rollback();
                        }
                        catch { }
                    }
                    throw ex;
                }
            }
            return nRes;
        }


        public int Update(IRow row, bool fClearChangedValues = true)
        {
            var nRes = 0;
            var dic=row.GetOriginalValues();
            if (dic.Count > 0)
            {
                var d=RowUpdateData.GetChangedValues(row);
                nRes=Update(d);
                if (fClearChangedValues)
                {
                    dic.Clear();
                }
            }
            return nRes;
        }

        //#region 拟废弃部分
        //public virtual int Edit(IEnumerable<IRow> adds, IEnumerable<IRow> updates, IEnumerable<IRow> deletes)
        //{
        //    var db = Workspace;
        //    db.BeginTransaction();
        //    var nRes = 0;
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
        //        nRes += Delete(deletes, false);
        //        #endregion

        //        #region insert
        //        nRes += Append(adds, false);
        //        #endregion

        //        db.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        try
        //        {
        //            db.Rollback();
        //        }
        //        catch { }
        //        throw ex;
        //    }

        //    return nRes;
        //}
        //#endregion

        #region  yxm 2021-4-14 为撤销/重做提供支持
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateValues">[fieldIndex,fieldValue]</param>
        /// <returns></returns>
        public virtual int Update(RowUpdateData updateValues)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adds"></param>
        /// <param name="deletes"></param>
        /// <param name="updates"></param>
        /// <returns></returns>
        public int Edit(IEnumerable<IRow> adds, IEnumerable<IRow> deletes, IEnumerable<RowUpdateData> updates)
        {
            var db = Workspace;
            db.BeginTransaction();
            var nRes = 0;
            try
            {
                #region update
                if (updates != null)
                {
                    foreach (var r in updates)
                    {
                        nRes += Update(r);
                    }
                }
                #endregion

                #region delete rows
                nRes += Delete(deletes, false);
                #endregion

                #region insert
                nRes += Append(adds, false);
                #endregion

                db.Commit();
            }
            catch (Exception ex)
            {
                try
                {
                    db.Rollback();
                }
                catch { }
                throw ex;
            }

            return nRes;
        }
        #endregion

        public virtual int ClearAll()
        {
            return 0;
        }
		#endregion

		public abstract void Search(IQueryFilter filter, Func<IRow,bool> callback);
		public void Search(IQueryFilter filter, Action<IRow> callback)
		{
			Search(filter, r => { callback(r); return true; });
		}
		public abstract bool SpatialQuery(ISpatialFilter filter, Func<IFeature, bool> callbak, ICancelTracker cancel=null, bool fRecycle = true);
		public bool SpatialQuery(ISpatialFilter filter, Action<IFeature> callbak, ICancelTracker cancel=null, bool fRecycle = true)
		{
			return SpatialQuery(filter, ft => { callbak(ft); return true; }, cancel, fRecycle);
		}

        public virtual int Count(string where)
        {
            var sql = "select count(*) from " + this.TableName;
            if (!string.IsNullOrEmpty(where))
            {
                sql += " where " + where;
            }
            var n = Workspace.QueryOneInt(sql);
            return n;
        }
        public virtual int ExecuteNonQuery(string sql, IEnumerable<SQLParam> lstParameters = null)
        {
            return Workspace.ExecuteNonQuery(sql, lstParameters);
        }

        public virtual void UpdateShape(int oid, IGeometry g)
        {

        }
        public virtual void RecalcFullExtent()
        {

        }
		protected void FireOnOpened()
		{
			OnOpened?.Invoke();
		}
	}

    public class ObjectIDSetBase
    {
        private readonly HashSet<int> _oids = new HashSet<int>();
        public Action OnDataChanged;
        public int Count
        {
            get
            {
                return _oids.Count;
            }
        }
        public void Add(int oid, bool fFireChange = true)
        {
            _oids.Add(oid);
            if (fFireChange)
            {
                FireDataChanged();
            }
        }
		public bool Remove(int oid, bool fFireChange = true)
		{
			if (_oids.Remove(oid))
			{
				if (fFireChange)
				{
					FireDataChanged();
				}
				return true;
			}
			return false;
		}
		public bool Contains(int oid)
        {
            return _oids.Contains(oid);
        }
        public void Get(Func<int, bool> callback)
        {
            foreach (var oid in _oids)
            {
                var fContinue = callback(oid);
                if (!fContinue)
                {
                    break;
                }
            }
        }
        public void Clear()
        {
            _oids.Clear();
            FireDataChanged();
        }
        public void FireChange()
        {
            FireDataChanged();
        }
        private void FireDataChanged()
        {
            OnDataChanged?.Invoke();
        }
    }
    public class ObjectIDSet : ObjectIDSetBase, IObjectIDSet { }
}
