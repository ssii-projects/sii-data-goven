using Agro.Library.Common;
using Agro.Module.DataExchange;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Agro.Library.Model;
using Agro.LibCore.Database;
using Agro.GIS;
using Agro.Library.Common.Repository;
using Agro.Library.Common.Service;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	public class ImportMdbTableBase : ImportTableBase, IImportTable
	{
		protected readonly string _srcTableName;
		protected readonly Dictionary<string, string> _dicFieldNameSrc2Dst = new Dictionary<string, string>();
		protected readonly bool _fAddOidField = false;
		protected Action<DataTable> OnPreImport;
		protected Action<DataRow> OnPreAddRow;
		protected Func<IWorkspace, string> OnConnected;
		protected Func<string, Type> OnPreGetFieldType;
		protected Action<DataTable> OnFlush;
		protected Action<bool> OnImportFinish;
		protected Action<IWorkspace> OnPreCommit;

		protected readonly bool _fCheckDataExist;
		protected IWorkspace _mdb;
		protected InputParam _inputParam;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tableName">SqlServer中新命名的表名</param>
		/// <param name="srcTableName">权属数据库mdb中的表名</param>
		public ImportMdbTableBase(string tableName, string srcTableName, bool fCheckDataExist = true)
			: base(tableName)
		{
			_srcTableName = srcTableName;
			//_fAddOidField = fAddOidField;
			this._fCheckDataExist = fCheckDataExist;
		}
		//var str = "Data Source=192.168.10.146;Initial Catalog=ARCSUMDATA;User ID=sa;Password=123456;";
		// @"C:\Users\Public\Nwt\cache\recv\李昌松\511702通川区\矢量数据\JZD5117022016.shp";
		public virtual void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
		{
			_inputParam = prm;
			//DataTable? dt = null;
			var lstFields = new List<Field>();
			var db = prm.Workspace;
			using var mdb = DBAccess.Open(prm.mdbFileName);
			_mdb = mdb;
			if (_fCheckDataExist)
			{
				#region 判断表中是否存在数据，若存在数据则不执行导入
				var cnt = SqlUtil.QueryOneInt(db, "select count(*) from " + TableName);
				if (cnt > 0)
				{
					reportInfo.reportWarning("表" + TableName + "中已存在数据，不允许执行此操作！");
					reportInfo.reportProgress(100);
					return;
				}
				#endregion
			}
			bool fOK = true;
			try
			{
				var err = OnConnected?.Invoke(db);
				if (err != null)
				{
					reportInfo.reportError(err);
					return;
				}
				var sTableName = TableName;
				var tableMeta = db.QueryTableMeta(TableName);

				var tableFields = db.QueryFields2(sTableName);

				bool fContainIDField = false;
				using DataTable dt = QueryTableStruct(mdb, _srcTableName, db);
				var sFields = "";
				for (int i = _fAddOidField ? 1 : 0; i < dt.Columns.Count; ++i)
				{
					var fieldName = dt.Columns[i].ColumnName;
					foreach (var kv in _dicFieldNameSrc2Dst)
					{
						if (fieldName == kv.Value)
						{
							fieldName = kv.Key;
							break;
						}
					}
					var field = tableFields.Find(fi => { return StringUtil.isEqualIgnorCase(fieldName, fi.FieldName); });
					lstFields.Add(field);
					if (sFields == "")
					{
						sFields = fieldName;
					}
					else
					{
						sFields += "," + fieldName;
					}
					if (fContainIDField == false && StringUtil.isEqual(fieldName, "ID"))
					{
						fContainIDField = true;
					}
				}
				if (!fContainIDField)
				{
					dt.Columns.Add("ID", typeof(string));
				}
				OnPreImport?.Invoke(dt);
				base.RecordCount = mdb.QueryOneInt("select count(*) from " + _srcTableName); ;

				var progress = new ProgressReporter(reportInfo.reportProgress, RecordCount);

				db.BeginTransaction();
				{
					var sql = "select " + sFields + " from " + _srcTableName;
					int oid = 0;//
					if (_fAddOidField)
					{
						oid = db.GetNextObjectID(TableName);
					}
					mdb.QueryCallback(sql, dr =>
					 {
						 var row = dt.NewRow();
						 int c = -1;
						 if (_fAddOidField)
						 {
							 row[++c] = oid++;
						 }
						 for (int i = 0; i < dr.FieldCount; ++i)
						 {
							 var o = dr.IsDBNull(i) ? null : dr.GetValue(i);
							 row[++c] = o ?? DBNull.Value;
							 var fi = lstFields[c];
							 if (fi.FieldType == eFieldType.eFieldTypeString && fi.Length > 0 && o != null)
							 {
								 var s = o.ToString();
								 if (s.Length > fi.Length)
								 {
									 throw new Exception("字符串\"" + s + "\"的长度" + s.Length + "超过字段[" + fi.FieldName + "]允许的最大长度：" + fi.Length);
								 }
							 }
						 }
						 if (!fContainIDField)
						 {
							 row[++c] = Guid.NewGuid().ToString();
						 }
						 OnPreAddRow?.Invoke(row);
						 dt.Rows.Add(row);
						 if (dt.Rows.Count >= 50000)
						 {
							 db.SqlBulkCopyByDatatable(tableMeta, dt);
							 if (_fAddOidField)
							 {
								 db.GetNextObjectID(TableName, dt.Rows.Count - 1);
							 }
							 OnFlush?.Invoke(dt);
							 dt.Rows.Clear();
						 }

						 progress.Step();
					 }, cancel);
					if (dt.Rows.Count > 0)
					{
						db.SqlBulkCopyByDatatable(tableMeta, dt);
						if (_fAddOidField)
						{
							db.GetNextObjectID(TableName, dt.Rows.Count - 1);
						}
						OnFlush?.Invoke(dt);
						dt.Rows.Clear();
					}
					OnPreCommit?.Invoke(db);
					db.Commit();
					progress.ForceFinish();
				}
			}
			catch (Exception ex)
			{
				db.Rollback();
				Console.WriteLine(ex.Message);
				reportInfo.reportError("错误：" + ex.Message);
				fOK = false;
			}
			finally
			{
				OnImportFinish?.Invoke(fOK);
			}
		}
		protected DataTable QueryTableStruct(IWorkspace mdb, string tableName, IWorkspace db)//,bool fAddOidField=true)
		{
			var dt = new DataTable(TableName);
			if (_fAddOidField)
			{
				dt.Columns.Add("OBJECTID", typeof(int));
			}
			var lstFields = db.QueryFields(TableName);
			var dicMdbFieldType = mdb.QueryFieldsType(tableName);
			foreach (var kv in dicMdbFieldType)
			{
				var fieldName = kv.Key;// dr.GetName(i);
				if (_dicFieldNameSrc2Dst.ContainsKey(fieldName))
				{
					fieldName = _dicFieldNameSrc2Dst[fieldName];
				}
				if(lstFields.Find(it=>it.Equals(fieldName,StringComparison.CurrentCultureIgnoreCase))!=null)
				//if (lstFields.Contains(fieldName))
				{
					var t = OnPreGetFieldType?.Invoke(fieldName) ?? kv.Value;
					dt.Columns.Add(fieldName, t);
				}
			}
			return dt;
		}
	}
	class ImportDJTableBase
	{
		internal static void Copy(DataRow from, DataRow to,Func<string,string> mapFieldName=null)
		{
			var toClms = to.Table.Columns;
			for (int i = toClms.Count; --i >= 0;)
			{
				var toFieldName = toClms[i].ColumnName;
				if (mapFieldName != null)
				{
					toFieldName = mapFieldName(toFieldName);
				}
				var j = FindIndex(from,toFieldName);
				if (j >= 0)
				{
					to[i] = from[j];
				}
			}
		}
		internal static int FindIndex(DataRow dr, string fieldName)
		{
			var clms = dr.Table.Columns;
			for (int i = clms.Count; --i >= 0;)
			{
				if (StringUtil.isEqualIgnorCase(fieldName, clms[i].ColumnName))
				{
					return i;
				}
			}
			return -1;
		}

		internal static DataTable ToDataTable<T>(Predicate<EntityProperty> predicate = null)
		{
			return EntityUtil.ToDataTable<T>(predicate);
			/*
			var dt = new DataTable();
			var lst =EntityUtil.GetAttributes<T>();
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
					else if (type == typeof(decimal?))
					{
						type = typeof(decimal);
					}
					dt.Columns.Add(it.FieldName,type);
				}
			}
			return dt;
			*/
		}

		internal static void SetDataRowValue(DataRow r, string fieldName, object value)
		{
			var i = FindIndex(r, fieldName);
			if (i >= 0)
			{
				var type = r.Table.Columns[i].DataType;
				if (type == typeof(bool))
				{
					if (value is int n)
					{
						value = n != 0;
					}
				}
				r[i] = value;
			}
		}
		internal static object GetDataRowValue(DataRow r, string fieldName)
		{
			var i = FindIndex(r, fieldName);
			if (i >= 0)
			{
				return r[i];
			}
			return null;
		}

		internal static void WriteFeatureToDataRow(IFeature ft, DataRow dr)
		{
			for (int i = ft.Fields.FieldCount; --i >= 0;)
			{
				var field = ft.Fields.GetField(i);
				var iField = FindIndex(dr, field.FieldName);
				if (iField >= 0)
				{
					var o = ft.GetValue(i);
					if (o != null)
					{
						dr[iField] = o;
					}
				}
			}
		}

		internal static object DbVal(object o)
		{
			return o ?? DBNull.Value;
		}
	}
    public class ImportCbf : ImportMdbTableBase
    {
		class ImportDJ_CBJYQ_CBF : ImportDJTableBase
		{
			private readonly DataTable dt;
			private readonly Dictionary<string, string> dicCbfbm2Qzbm = new Dictionary<string, string>();
			public ImportDJ_CBJYQ_CBF()
			{
				dt = ToDataTable<DJ_CBJYQ_CBF>();
			}
			public void PreImport(IWorkspace mdb)
			{
				mdb.QueryCallback("select CBFBM,CBJYQZBM from CBJYQZDJB", r =>
				 {
					 var cbfBm = r.GetString(0);
					 var qzbm = r.GetString(1);
					 dicCbfbm2Qzbm[cbfBm] = qzbm;
				 });
			}
			public void Flush(IWorkspace db, DataTable dtQssj,int iDjztField)
			{
				var tableMeta = db.QueryTableMeta(DJ_CBJYQ_CBF.GetTableName());
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var djzt = (EDjzt)fromR[iDjztField];
					if (djzt == EDjzt.Ydj)
					{
						var cbfBm = GetDataRowValue(fromR, "CBFBM")?.ToString();
						if (dicCbfbm2Qzbm.TryGetValue(cbfBm, out string djbid))
						{
							var r = dt.NewRow();
							dt.Rows.Add(r);
							Copy(fromR, r);
							//SetDataRowValue(r, "ID", Guid.NewGuid().ToString());
							SetDataRowValue(r, "DJBID", djbid);
						}
					}
				}
				db.SqlBulkCopyByDatatable(tableMeta, dt);
				dt.Clear();
			}
			public bool IsYdj(string cbfbm)
			{
				return dicCbfbm2Qzbm.ContainsKey(cbfbm);
			}
			public void Dispose()
			{
				dt.Dispose();
				dicCbfbm2Qzbm.Clear();
			}
		}
		class Index
		{
			public int iCbfBmField = -1;
			public int iFbfbmField = -1;
			public int iCjsjField = -1;
			public int iDjztField = -1;
			public int iZtField = -1;
			public int ID;
			public int CBFLX;
			internal void Reset(DataTable dt)
			{
				for (int i = dt.Columns.Count; --i >=0 ;)
				{
					switch (dt.Columns[i].ColumnName)
					{
						case "CBFBM":iCbfBmField = i;break;
						case "FBFBM": iFbfbmField = i; break;
						case "CJSJ": iCjsjField = i; break;
						case "DJZT": iDjztField = i; break;
						case "ZT": iZtField = i; break;
						case "ID":ID = i;break;
						case "CBFLX": CBFLX = i;break;
					}
				}
			}
		}

		//class CbfLsh
		//{
		//	private readonly Dictionary<string, int> dic = new Dictionary<string, int>();
		//	public void Add(string fbfBm, string cbfLx, string cbfBm)
		//	{
		//		int bh;
		//		string lx;
		//		switch (cbfLx.Trim())
		//		{
		//			case "1"://CONTRACTOR_FAMILY
		//				lx = "CONTRACTOR_FAMILY";
		//				bh = SafeConvertAux.ToInt32(cbfBm.Right(3)) + 1;
		//				break;
		//			case "2"://CONTRACTOR_PERSON
		//				lx = "CONTRACTOR_PERSON";
		//				bh =SafeConvertAux.ToInt32(cbfBm.Right(3))+8000+1;
		//				break;
		//			case "3"://CONTRACTOR_ORG
		//				lx = "CONTRACTOR_ORG";
		//				bh = SafeConvertAux.ToInt32(cbfBm.Right(3)) + 9000 + 1;
		//				break;
		//			default:
		//				throw new Exception($"CBFLX值只能为（1,2,3）中的一种，当前值为：{cbfLx}");
		//		}
		//		var key = fbfBm + "~" + lx;
		//		if (dic.TryGetValue(key, out int n))
		//		{
		//			if (bh > n)
		//			{
		//				dic[key] = bh;
		//			}
		//		}
		//		else
		//		{
		//			dic[key] = bh;
		//		}
		//	}

		//	public void UpdateLsh(IWorkspace db)
		//	{
		//		var repos = new XtpzLshRepository(db);
		//		foreach (var kv in dic)
		//		{
		//			var sa=kv.Key.Split('~');
		//			repos.UpdateLsh(sa[1], sa[0], kv.Value);
		//		}
		//		dic.Clear();
		//	}
		//}
        public ImportCbf(bool fCheckDataExist = true) : base(TableNameConstants.CBF, "CBF",fCheckDataExist)
        {
			var djCbf = new ImportDJ_CBJYQ_CBF();
			var idx = new Index();
			var lshService = new XtpzLshService();
			
            base.OnPreImport += dt =>
            {
                dt.Columns.Add("ZT", typeof(int));
                dt.Columns.Add("DJZT", typeof(int));
                dt.Columns.Add("CJSJ",typeof(DateTime));
                dt.Columns.Add("FBFBM", typeof(string));
				idx.Reset(dt);
				djCbf.PreImport(_mdb);
            };
            var vCjsj = DateTime.Now;//.ToString("yyyy-MM-dd HH:mm:ss");// "to_timestamp("+ DateTime.Now.ToString()+")";
            base.OnPreAddRow += r =>
            {
                var cbfbm = r[idx.iCbfBmField].ToString();
                var sFbfbm = cbfbm.Substring(0, 14);
				r[idx.ID] = cbfbm;
                r[idx.iFbfbmField] = sFbfbm;
                r[idx.iZtField] = 1;
                r[idx.iDjztField] =(int)( djCbf.IsYdj(cbfbm)?EDjzt.Ydj:EDjzt.Wdj);
                r[idx.iCjsjField] = vCjsj;
				if (r[idx.CBFLX] is string cbfLx)
				{
					lshService.AddCbf(sFbfbm, cbfLx, cbfbm);
				}
			};

			base.OnFlush += dt => djCbf.Flush(_inputParam.Workspace, dt, idx.iDjztField);
			base.OnPreCommit+=db=>lshService.UpdateLsh(db);
			base.OnImportFinish += t =>djCbf.Dispose();
        }
    }

	public class ImportCBJYQZ_QZBF : ImportMdbTableBase
	{
		class ImportDJ_CBJYQ_QZBF : ImportDJTableBase
		{
			private readonly DataTable dt;
			public ImportDJ_CBJYQ_QZBF()
			{
				dt = ToDataTable<DJ_CBJYQ_QZBF>();
			}
			public void Flush(IWorkspace db, DataTable dtQssj)
			{
				var tableMeta = db.QueryTableMeta(DJ_CBJYQ_QZBF.GetTableName());
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var r = dt.NewRow();
					dt.Rows.Add(r);
					Copy(fromR, r);
				}
				db.SqlBulkCopyByDatatable(tableMeta, dt);
				dt.Clear();
			}
			public void Dispose()
			{
				dt.Dispose();
			}
		}
		public ImportCBJYQZ_QZBF(bool fCheckDataExist = true) : base(TableNameConstants.CBJYQZ_QZBF, "CBJYQZ_QZBF", fCheckDataExist)
		{
			var djQzbf = new ImportDJ_CBJYQ_QZBF();
			base.OnFlush += dt => djQzbf.Flush(_inputParam.Workspace, dt);
			base.OnImportFinish += t => djQzbf.Dispose();
		}
	}

	public class ImportCBJYQ_QZHF : ImportMdbTableBase
	{
		class ImportDJ_CBJYQ_QZHF : ImportDJTableBase
		{
			private readonly DataTable dt;
			public ImportDJ_CBJYQ_QZHF()
			{
				dt = ToDataTable<DJ_CBJYQ_QZHF>();
			}
			public void Flush(IWorkspace db, DataTable dtQssj)
			{
				var tableMeta = db.QueryTableMeta(DJ_CBJYQ_QZHF.GetTableName());
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var r = dt.NewRow();
					dt.Rows.Add(r);
					Copy(fromR, r);
				}
				db.SqlBulkCopyByDatatable(tableMeta, dt);
				dt.Clear();
			}
			public void Dispose()
			{
				dt.Dispose();
			}
		}
		public ImportCBJYQ_QZHF(bool fCheckDataExist = true) : base(TableNameConstants.CBJYQZ_QZHF, "CBJYQZ_QZHF", fCheckDataExist)
		{
			var djQzhf = new ImportDJ_CBJYQ_QZHF();
			base.OnFlush += dt => djQzhf.Flush(_inputParam.Workspace, dt);
			base.OnImportFinish += t => djQzhf.Dispose();
		}
	}
	public class ImportCBDKXX : ImportMdbTableBase
    {
		//class ContractLandLsh
		//{
		//	private readonly Dictionary<string, int> dic = new Dictionary<string, int>();
		//	public void Add(string fbfBm, string dkBm)
		//	{
		//		int bh = SafeConvertAux.ToInt32(dkBm.Right(5)) + 1;
		//		var key = fbfBm;
		//		if (dic.TryGetValue(key, out int n))
		//		{
		//			if (bh > n)
		//			{
		//				dic[key] = bh;
		//			}
		//		}
		//		else
		//		{
		//			dic[key] = bh;
		//		}
		//	}

		//	public void UpdateLsh(IWorkspace db)
		//	{
		//		var repos = new XtpzLshRepository(db);
		//		foreach (var kv in dic)
		//		{
		//			repos.UpdateLsh("CONTRACTLAND",kv.Key, kv.Value);
		//		}
		//		dic.Clear();
		//	}
		//}
		/// <summary>
		/// 导入DJ_CBJYQ_DJB
		/// </summary>
		class ImportDJ_CBJYQ_DKXX : ImportDJTableBase
		{
			private readonly DataTable dt;
			private readonly DkRepos dkRepos;
			private int iSFJBNTField = -1;
			public ImportDJ_CBJYQ_DKXX(InputParam prm)
			{
				dkRepos = new DkRepos(prm);
				dt = ToDataTable<DJ_CBJYQ_DKXX>();
				for (int i = dt.Columns.Count; --i >= 0;)
				{
					if (StringUtil.isEqualIgnorCase(dt.Columns[i].ColumnName , "SFJBNT"))
					{
						iSFJBNTField = i;
						break;
					}
				}
			}
			public void Flush(IWorkspace db, DataTable dtQssj)
			{
				var tableMeta = db.QueryTableMeta(DJ_CBJYQ_DKXX.GetTableName());
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var r = dt.NewRow();
					dt.Rows.Add(r);
					Copy(fromR, r,fn=>
					{
						switch (fn.ToUpper())
						{
							case "ELHTMJ":return "YHTMJM";
							case "QQMJ":return "HTMJM";
						};
						return fn;
					});
					var cbhtBm = GetDataRowValue(r, "CBHTBM");
					SetDataRowValue(r, "DJBID", cbhtBm);
					var dkbm = GetDataRowValue(r, "DKBM")?.ToString();// r[index.DKBM]?.ToString();
					if (dkbm == null)
						throw new Exception("DKBM不能为null");
					SetDataRowValue(r, "DKID", dkbm);
					SetDataRowValue(r, "DYZT", 0);
					SetDataRowValue(r, "YYZT", 0);
					SetDataRowValue(r, "LZZT", 0);
					
					var ft = dkRepos.GetFeatureByDkbm(dkbm);
					if (ft == null)
					{
						throw new Exception($"地块编码{dkbm}在矢量地块中不存在！");
					}
					WriteFeatureToDataRow(ft, r);
					if (iSFJBNTField >= 0&&(r[iSFJBNTField]==null||r[iSFJBNTField]==DBNull.Value))
					{
						r[iSFJBNTField] = "";
					}
				}
				db.SqlBulkCopyByDatatable(tableMeta, dt);
				dt.Clear();
			}

			public void Dispose()
			{
				dkRepos.Dispose();
				dt.Dispose();
			}
		}
		public ImportCBDKXX(bool fCheckDataExist = true) :base(TableNameConstants.CBDKXX, "CBDKXX",fCheckDataExist)
        {
			ImportDJ_CBJYQ_DKXX djDk = null;

			//var cbfLsh = new ContractLandLsh();
			int iHTMJField = -1;
            int iHTMJMField = -1;
			int iDKBMField = -1;
			int iFbfBmField = -1;
			//int SFJBNT = -1;
			//int iIDField = -1;
            base.OnPreImport += dt =>
            {
				djDk = new ImportDJ_CBJYQ_DKXX(_inputParam);

				for (int i = 0; i < dt.Columns.Count; ++i)
                {
                    switch (dt.Columns[i].ColumnName)
                    {
                        case "HTMJ":iHTMJField = i;break;
                        case "HTMJM":iHTMJMField = i;break;
						case "DKBM":iDKBMField = i;break;
						case "FBFBM":iFbfBmField = i;break;
						//case "SFJBNT":SFJBNT = i;break;
                    }
                }
			};
            base.OnPreAddRow += r =>
            {
                var o=r[iHTMJMField];
                var rMJ = r[iHTMJField];
                if (SafeConvertAux.ToDouble(o)<=0&&  rMJ!=null&&rMJ!=DBNull.Value)
                {
                    var d = Math.Round(SafeConvertAux.ToDouble(rMJ) * 0.0015, 2);
                    r[iHTMJMField] = d;
                }
				//if (r[SFJBNT] == null||r[SFJBNT]==DBNull.Value)
				//{
				//	r[SFJBNT] = "";
				//}
				//var fbfBm = r[iFbfBmField]?.ToString();
				//var dkbm = r[iDKBMField].ToString();
				//cbfLsh.Add(fbfBm, dkbm);
			};

			base.OnFlush += dt =>djDk.Flush(_inputParam.Workspace, dt);
			//base.OnPreCommit += db => cbfLsh.UpdateLsh(db);
			base.OnImportFinish += t => djDk.Dispose();
        }
    }
	
	public class ImportQSSJ_CBJYQZDJB : ImportMdbTableBase
	{
		/// <summary>
		/// 导入DJ_CBJYQ_DJB
		/// </summary>
		class ImportDJ_CBJYQ_DJB : ImportDJTableBase
		{
			class Index
			{
				public int QLLX;
				public int DJYY;
				public int QSZT;
				public int DYZT;
				public int YYZT;
				public int SFYZX;
				public int QXDM;
				public int FBFBM;
				public int SZDY;
				public int LSBB;
				public int CBFBM;
				public int CBFMC;
				public int DJLX;
				public int FBFMC;
				public int FBFFZRXM;
				public void Reset(DataTable dt)
				{
					for (int i = dt.Columns.Count; --i >= 0;)
					{
						switch (dt.Columns[i].ColumnName.ToUpper())
						{
							case "QLLX":QLLX = i;break;
							case "DJYY": DJYY = i; break;
							case "QSZT": QSZT = i; break;
							case "DYZT": DYZT = i; break;
							case "YYZT":YYZT = i;break;
							case "SFYZX": SFYZX = i;break;
							case "QXDM": QXDM = i;break;
							case "FBFBM": FBFBM = i;break;
							case "SZDY": SZDY = i;break;
							case "LSBB": LSBB = i;break;
							case "CBFBM": CBFBM = i;break;
							case "CBFMC": CBFMC = i;break;
							case "DJLX": DJLX = i;break;
							case "FBFMC": FBFMC = i;break;
							case "FBFFZRXM": FBFFZRXM = i; break;
						}
					}
				}
			}
			class FbfItem
			{
				public string FBFMC;
				public string FBFFZRXM;
			}
			private readonly DataTable dt;
			private readonly Index index = new Index();
			private readonly Dictionary<string, string> dicCbfbm2Cbfmc = new Dictionary<string, string>();
			private readonly Dictionary<string, FbfItem> dicFbfbm2Item = new Dictionary<string, FbfItem>();
			public ImportDJ_CBJYQ_DJB()
			{
				dt = ToDataTable<DJ_CBJYQ_DJB>();
				index.Reset(dt);
			}



			public void PreImport(IWorkspace mdb)
			{
				mdb.QueryCallback("select CBFBM,CBFMC from CBF where CBFBM is not null and CBFMC is not null", r =>
				 {
					 dicCbfbm2Cbfmc[r.GetString(0)] = r.GetString(1);
				 });
				mdb.QueryCallback("select FBFBM,FBFMC,FBFFZRXM from FBF where FBFBM is not null", r =>
				 {
					 dicFbfbm2Item[r.GetString(0)] = new FbfItem()
					 {
						 FBFMC=SafeConvertAux.ToStr(r.GetValue(1)),
						 FBFFZRXM=SafeConvertAux.ToStr(r.GetValue(2))
					 };
				 });
			}
			public void Flush(IWorkspace db, DataTable dtQssj)
			{
				var tableMeta = db.QueryTableMeta(DJ_CBJYQ_DJB.GetTableName());
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var r=dt.NewRow();
					dt.Rows.Add(r);
					Copy(fromR, r,fn=>
					{
						if (StringUtil.isEqual(fn, "DJBFJ"))
							return "FJ";
						return fn;
					});
					r[index.DJYY] = "初始化总登记入库";
					r[index.QLLX] = 0;
					r[index.QSZT] = 1;
					r[index.DYZT] = 0;
					r[index.YYZT] = 0;
					r[index.SFYZX] = 0;
					r[index.QXDM] = r[index.FBFBM]?.ToString()?.Substring(0, 6);
					var fbfbm= r[index.FBFBM].ToString();
					var szdy = fbfbm;
					if (szdy.EndsWith("00000")){
						szdy = szdy.Substring(0, szdy.Length -5);
					}
					else if (szdy.EndsWith("00"))
					{
						szdy = szdy.Substring(0, szdy.Length - 2);
					}
					r[index.SZDY] = szdy;
					r[index.LSBB] = 0;
					r[index.DJLX] = 0;
					var cbfBm = r[index.CBFBM]?.ToString();
					if (cbfBm != null && dicCbfbm2Cbfmc.TryGetValue(cbfBm, out string cbfMc))
					{
						r[index.CBFMC] = cbfMc;
					}
					if (dicFbfbm2Item.TryGetValue(fbfbm, out FbfItem it1))
					{
						r[index.FBFMC] = it1.FBFMC;
						r[index.FBFFZRXM] = it1.FBFFZRXM;
					}
				}
				db.SqlBulkCopyByDatatable(tableMeta, dt);
				dt.Clear();
			}

			public void Dispose()
			{
				dt.Dispose();
			}
		}
		class Index
		{
			public int CBJYQZBM = -1;
			public int ID;
			internal void Reset(DataTable dt)
			{
				for (int i = 0; i < dt.Columns.Count; ++i)
				{
					switch (dt.Columns[i].ColumnName)
					{
						case "ID": ID = i; break;
						case "CBJYQZBM": CBJYQZBM = i; break;
					}
				}
			}
		}
		public ImportQSSJ_CBJYQZDJB(bool fCheckDataExist = true) : base(QSSJ_CBJYQZDJB.GetTableName(), "CBJYQZDJB", fCheckDataExist)
		{
			var djDjb = new ImportDJ_CBJYQ_DJB();
			var idx = new Index();
			base.OnPreImport += dt =>
			{
				idx.Reset(dt);
				djDjb.PreImport(_mdb);
			};
			base.OnPreAddRow += r =>
			{
				var CBJYQZBM = r[idx.CBJYQZBM]?.ToString();
				r[idx.ID] = CBJYQZBM ?? throw new Exception("CBJYQZBM不能为null");
			};
			base.OnFlush += dt => djDjb.Flush(_inputParam.Workspace, dt);
			base.OnImportFinish += fok =>djDjb.Dispose();
		}
	}

	public class ImportQSSJ_CBJYQZ : ImportMdbTableBase
	{
		class ImportDJ_CBJYQ_QZ : ImportDJTableBase
		{

			private readonly DataTable dt;
			private readonly Dictionary<string, string> dicQzbm2Lsh = new Dictionary<string, string>();
			private readonly XtpzLshService _lsh;
			public ImportDJ_CBJYQ_QZ(XtpzLshService lsh)
			{
				this._lsh = lsh;
				dt = ToDataTable<DJ_CBJYQ_QZ>();
			}

			public void PreImport(IWorkspace mdb)
			{
				mdb.QueryCallback("select CBJYQZBM,CBJYQZLSH from CBJYQZDJB where CBJYQZLSH is not null", r =>
				 {
					 var qzbm = r.GetString(0);
					 var lsh = r.GetString(1);
					 dicQzbm2Lsh[qzbm] = lsh;
				 });
			}
			public void Flush(IWorkspace db, DataTable dtQssj,string fzjgMc)
			{
				dt.Rows.Clear();
				var lsh = new LshItem();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var r = dt.NewRow();
					dt.Rows.Add(r);
					Copy(fromR, r);
					var cbjyqzBm = GetDataRowValue(fromR, "CBJYQZBM").ToString();
					SetDataRowValue(r, "DJBID", cbjyqzBm);
					SetDataRowValue(r, "ZSBS", 0);
					SetDataRowValue(r, "DYCS", 0);
					SetDataRowValue(r, "SFYZX", 0);
					SetDataRowValue(r, "FZJGSZDMC", fzjgMc);
					if (dicQzbm2Lsh.TryGetValue(cbjyqzBm, out string strLsh))
					{
						QssjDjbRepository.ParseLsh(strLsh, lsh);
						SetDataRowValue(r, "CBJYQZLSH", strLsh);
						if (lsh.BZNF != null && lsh.NDSXH != null)
						{
							_lsh.AddCertificate(cbjyqzBm.Substring(0, 6), (int)lsh.BZNF, ((int)lsh.NDSXH) + 1);
						}
					}
				
					SetDataRowValue(r, "SQSJC", DbVal(lsh?.SQSJC));
					int fzNd = 0;
					var o = GetDataRowValue(fromR, "FZRQ");
					if (o is DateTime d)
					{
						fzNd = d.Year;
					}
					SetDataRowValue(r, "BZNF", DbVal(lsh.BZNF ?? fzNd));
					SetDataRowValue(r, "NDSXH", DbVal(lsh?.NDSXH));
				}


				db.SqlBulkCopyByDatatable(db.QueryTableMeta(DJ_CBJYQ_QZ.GetTableName()), dt);
				dt.Clear();
			}

			public void Dispose()
			{
				dicQzbm2Lsh.Clear();
				dt.Dispose();
			}
		}

		private readonly XtpzLshService certificateLsh = new XtpzLshService();
		public ImportQSSJ_CBJYQZ(bool fCheckDataExist = true) : base(TableNameConstants.CBJYQZ, "CBJYQZ", fCheckDataExist)
		{
			var djQz = new ImportDJ_CBJYQ_QZ(certificateLsh);
			base.OnPreImport += dt => djQz.PreImport(_mdb);
			base.OnPreAddRow += r =>
			{
				var CBJYQZBM = ImportDJTableBase.GetDataRowValue(r,"CBJYQZBM")?.ToString();
				if(CBJYQZBM==null) throw new Exception("CBJYQZBM不能为null");
				ImportDJTableBase.SetDataRowValue(r, "ID", CBJYQZBM);
			};
			base.OnFlush += dt => djQz.Flush(_inputParam.Workspace, dt,_inputParam.sXzqmc);
			base.OnPreCommit += db => certificateLsh.UpdateLsh(db);
			base.OnImportFinish += f => djQz.Dispose();
		}
	}

	public class ImportCbht : ImportMdbTableBase
    {
		/// <summary>
		/// 导入DJ_CBJYQ_CBHT
		/// </summary>
		class ImportDJ_CBJYQ_CBHT : ImportDJTableBase
		{
			class Index
			{
				public int CBHTBM;
				public int DJBID;
				public void Reset(DataTable dt)
				{
					for (int i = dt.Columns.Count; --i >= 0;)
					{
						switch (dt.Columns[i].ColumnName.ToUpper())
						{
							case "CBHTBM": CBHTBM = i; break;
							case "DJBID": DJBID = i; break;
						}
					}
				}
			}
			private readonly DataTable dt;
			private readonly Index index = new Index();
			private readonly HashSet<string> setDjQZBM = new HashSet<string>();
			public ImportDJ_CBJYQ_CBHT()
			{
				dt = ToDataTable<DJ_CBJYQ_CBHT>();
				index.Reset(dt);
			}
			public void Flush(IWorkspace db, DataTable dtQssj,int iDjztField)
			{
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;
					var djZt=(EDjzt)fromR[iDjztField];
					if (djZt == EDjzt.Ydj)
					{
						var r = dt.NewRow();
						dt.Rows.Add(r);
						Copy(fromR, r);
						r[index.DJBID] = r[index.CBHTBM];
					}
				}
				if (dt.Rows.Count > 0)
				{
					db.SqlBulkCopyByDatatable(db.QueryTableMeta(DJ_CBJYQ_CBHT.GetTableName()), dt);
					dt.Clear();
				}
			}

			public void Dispose()
			{
				dt.Dispose();
			}
		}
		class Index
		{
			public int iCjsjField = -1;
			public int iDjztField = -1;
			public int iZtField = -1;
			public int iHTZMJField = -1;
			public int iHTZMJMField = -1;
			public int CBHTBM = -1;
			public int ID;
			internal void Reset(DataTable dt)
			{
				for (int i = 0; i < dt.Columns.Count; ++i)
				{
					switch (dt.Columns[i].ColumnName)
					{
						case "ID": ID = i; break;
						case "HTZMJ": iHTZMJField = i; break;
						case "HTZMJM": iHTZMJMField = i; break;
						case "CJSJ": iCjsjField = i; break;
						case "DJZT": iDjztField = i; break;
						case "ZT": iZtField = i; break;
						case "CBHTBM": CBHTBM = i; break;
					}
				}
			}
		}
        public ImportCbht(bool fCheckDataExist = true) : base(TableNameConstants.CBHT, "CBHT",fCheckDataExist)
        {
			var djCbht = new ImportDJ_CBJYQ_CBHT();
			var idx = new Index();
			var setDjCbhtbm = new HashSet<string>();

			base.OnPreImport += dt =>
            {
                dt.Columns.Add("ZT", typeof(int));
                dt.Columns.Add("DJZT", typeof(int));
                dt.Columns.Add("CJSJ", typeof(DateTime));
				idx.Reset(dt);

				_mdb.QueryCallback("select CBJYQZBM from CBJYQZDJB where CBJYQZBM is not null", r =>setDjCbhtbm.Add(r.GetString(0)));
            };
            var vCjsj = DateTime.Now;//.ToString("yyyy-MM-dd HH:mm:ss");// "to_timestamp("+ DateTime.Now.ToString()+")";
            base.OnPreAddRow += r =>
            {
				var cbhtbm = r[idx.CBHTBM]?.ToString();
				r[idx.ID] = cbhtbm ?? throw new Exception("CBHTBM不能为null");
                r[idx.iZtField] = 1;
                r[idx.iDjztField] =(int)(setDjCbhtbm.Contains(cbhtbm)? EDjzt.Ydj:EDjzt.Wdj);
                r[idx.iCjsjField] = vCjsj;

                var o = r[idx.iHTZMJMField];
                var rMJ = r[idx.iHTZMJField];
                if (SafeConvertAux.ToDouble(o)<=0 && rMJ != null&&rMJ!=DBNull.Value)
                {
                    var d = Math.Round(SafeConvertAux.ToDouble(rMJ) * 0.0015, 2);
                    r[idx.iHTZMJMField] = d;
                }
            };

			base.OnFlush += dt => djCbht.Flush(_inputParam.Workspace, dt,idx.iDjztField);
			base.OnImportFinish += fok => djCbht.Dispose();

		}
    }

    public class ImportFbf : ImportMdbTableBase
    {
		class Index
		{
			public int iSzdyField = -1;
			public int iFbfbmField = -1;
			public int ID;
			internal void Reset(DataTable dt)
			{
				for (int i = dt.Columns.Count; --i>=0 ; )
				{
					switch (dt.Columns[i].ColumnName.ToUpper())
					{
						case "ID":ID = i;break;
						case "FBFBM":iFbfbmField = i;break;
						case "SZDY": iSzdyField = i; break;
					}
				}
			}
		}
        public ImportFbf(bool fCheckDataExist = true) :base(TableNameConstants.FBF, "FBF",fCheckDataExist)
        {
			var idx = new Index();
            //var dicBm2ID = new Dictionary<string, string>();
            //base.OnConnected += db =>
            //{
            //    db.QueryCallback("select BM,ID from DLXX_XZDY where JB in(1,2)", r =>
            //    {
            //        var bm = r.GetString(0).PadRight(14, '0');
            //        var id = r.GetString(1);
            //        dicBm2ID[bm] = id;
            //        return true;
            //    });
            //    if (dicBm2ID.Count == 0)
            //    {
            //        return "必须先导入行政地域数据后才能导入发包方数据";
            //    }
            //    return null;
            //};

            base.OnPreImport += dt =>
            {
                dt.Columns.Add("SZDY", typeof(string));
				idx.Reset(dt);
            };
            base.OnPreAddRow += r =>
            {
                var s = r[idx.iFbfbmField].ToString();
                var sFbfbm = s.Substring(0, 14);
				//if(!dicBm2ID.TryGetValue(sFbfbm,out string id))
				//{
				//    id = sFbfbm;
				//}
				if (sFbfbm.EndsWith("00000"))
				{
					sFbfbm = sFbfbm.Substring(0, sFbfbm.Length - 5);
				}
				else if (sFbfbm.EndsWith("00"))
				{
					sFbfbm = sFbfbm.Substring(0, sFbfbm.Length - 2);
				}
                r[idx.iSzdyField] = sFbfbm;
				r[idx.ID] = sFbfbm;
            };
        }
    }
    //public class ImportMdbCbjyqz : ImportMdbTableBase
    //{
    //    public ImportMdbCbjyqz():base(TableNameConstants.CBJYQZ, "CBJYQZ")
    //    {

    //    }
    //    public override void DoImport(string connectionString, InputParams prm, ReportInfo reportInfo, bool fClearAllOldData = false)
    //    {
    //        try
    //        {
    //            using (var mdb = new DBAccess(prm.mdbFileName))
    //            //using (var db = new SqlServer())
    //            using (var db = DataBaseSource.GetDatabase())
    //            {
    //                #region 判断表中是否存在数据，若存在数据则不执行导入
    //                var cnt = SafeConvertAux.ToInt32(SqlHelper.QueryOne(db, "select count(*) from " + TableName));
    //                //if (false)
    //                //{
    //                if (cnt > 0)
    //                {
    //                    reportInfo.reportInfo("表" + TableName + "中已存在数据，不允许执行此操作！");
    //                    reportInfo.reportProgress(100);
    //                    return;
    //                }
    //                //}
    //                #endregion
    //                //db.Connect(connectionString);
    //                if (fClearAllOldData)
    //                {
    //                    db.ExecuteNonQuery("TRUNCATE TABLE " + TableName);
    //                }
    //                var sTableName = TableName;
    //                // var imp = new ShapeFileToSQLServer();
    //                //var dt = queryTableStruct(mdb, _srcTableName, db);
    //                var dt = new DataTable(TableName);
    //                dt.Columns.Add("CBJYQZBM", typeof(string));
    //                dt.Columns.Add("FZJG", typeof(string));
    //                dt.Columns.Add("FZRQ", typeof(DateTime));

    //                var sFields = "";
    //                for (int i = 1; i < dt.Columns.Count; ++i)
    //                {
    //                    var fieldName = dt.Columns[i].ColumnName;
    //                    foreach (var kv in _dicFieldNameSrc2Dst)
    //                    {
    //                        if (fieldName == kv.Value)
    //                        {
    //                            fieldName = kv.Key;
    //                            break;
    //                        }
    //                    }
    //                    if (sFields == "")
    //                    {
    //                        sFields = fieldName;
    //                    }
    //                    else
    //                    {
    //                        sFields += "," + fieldName;
    //                    }
    //                }
    //                dt.Columns.Add("ID", typeof(string));
    //                double oldProgress = 0;
    //                int n = 0;
    //                cnt = mdb.QueryOneInt("select count(*) from " + _srcTableName);
    //                //if (cnt == 0)
    //                //{
    //                //    ProgressUtil.reportProgress(reportInfo.reportProgress, 1, 1, ref oldProgress);
    //                //}
    //                //else
    //                {
    //                    var sql = "select " + sFields + " from " + _srcTableName;
    //                    int oid = db.GetNextObjectID(TableName);
    //                    using (var dr = mdb.QueryReader(sql))
    //                    {
    //                        while (dr.Read())
    //                        {
    //                            //var oid = ++n;
    //                            var row = dt.NewRow();
    //                            int c = 0;
    //                            row[c] = oid++;

    //                            for (int i = 0; i < dr.FieldCount; ++i)
    //                            {
    //                                row[++c] = dr.GetValue(i);
    //                            }
    //                            row[++c] = Guid.NewGuid().ToString();
    //                            dt.Rows.Add(row);
    //                            if (dt.Rows.Count >= 50000)
    //                            {
    //                                db.SqlBulkCopyByDatatable(TableName, dt);
    //                                db.GetNextObjectID(TableName, dt.Rows.Count - 1);
    //                                dt.Rows.Clear();
    //                            }

    //                            ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, ++n, ref oldProgress);
    //                        }
    //                    }
    //                    if (dt.Rows.Count > 0)
    //                    {
    //                        db.SqlBulkCopyByDatatable(TableName, dt);
    //                        db.GetNextObjectID(TableName, dt.Rows.Count - 1);
    //                        dt.Rows.Clear();
    //                    }
    //                    ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);
    //                }
    //            }

    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //            reportInfo.reportError("错误：" + ex.Message);
    //        }
    //    }
    //}

    public class ImportQSSJ_CBF_JTCY : ImportMdbTableBase
    {

		class ImportDJ_CBJYQ_CBF_JTCY : ImportDJTableBase
		{
			private readonly DataTable dt;
			private readonly Dictionary<string, string> dicCbfbm2Qzbm = new Dictionary<string, string>();
			public ImportDJ_CBJYQ_CBF_JTCY()
			{
				dt = ToDataTable<DJ_CBJYQ_CBF_JTCY>();
			}
			public void PreInport(IWorkspace mdb)
			{
				mdb.QueryCallback("select CBFBM,CBJYQZBM from CBJYQZDJB", r =>
				 {
					 var cbfBm = r.GetString(0);
					 var qzbm = r.GetString(1);
					 dicCbfbm2Qzbm[cbfBm] = qzbm;
				 });
			}
			public void Flush(IWorkspace db, DataTable dtQssj)
			{
				dt.Rows.Clear();
				foreach (var it in dtQssj.Rows)
				{
					var fromR = it as DataRow;

					var cbfbm = GetDataRowValue(fromR, "CBFBM").ToString();
					if (dicCbfbm2Qzbm.TryGetValue(cbfbm, out string djbID))
					{
						var r = dt.NewRow();
						dt.Rows.Add(r);
						Copy(fromR, r);
						SetDataRowValue(r, "DJBID", djbID);
						//var djCbfID = GetDataRowValue(r, "ID");
						SetDataRowValue(r, "CBFID", cbfbm);// djCbfID);
					}
				}
				db.SqlBulkCopyByDatatable(db.QueryTableMeta(DJ_CBJYQ_CBF_JTCY.GetTableName()), dt);
				dt.Clear();
			}
			public void Dispose()
			{
				dt.Dispose();
				dicCbfbm2Qzbm.Clear();
			}
		}

		public ImportQSSJ_CBF_JTCY(bool fCheckDataExist = true) : base(TableNameConstants.CBF_JTCY, "CBF_JTCY",fCheckDataExist)
        {
			var djJtcy = new ImportDJ_CBJYQ_CBF_JTCY();

			base.OnPreGetFieldType += fieldName =>
            {
                if (fieldName == "SFGYR")
                {
                    return typeof(decimal);
                }
                return null;
            };
            base.OnPreImport += dt =>
            {
                dt.Columns.Add("CSRQ", typeof(DateTime));
				djJtcy.PreInport(_mdb);
            };
            base.OnPreAddRow += dr =>
            {
                var sZjham=SafeConvertAux.ToStr(dr["CYZJHM"]);
                var sCYZJLX =SafeConvertAux.ToStr(dr["CYZJLX"]);
                if (sCYZJLX == "1")
                {
                    if (sZjham.Length == 18)
                    {
                        var sYear =sZjham.Substring(6, 4);
                        var sMonth = sZjham.Substring(10, 2);
                        var sDay = sZjham.Substring(12, 2);
                        if(DateTime.TryParse(String.Format("{0}-{1}-{2}", sYear, sMonth, sDay), out DateTime csrq))
                        {
                            dr["CSRQ"] = csrq;
                        }
                    }
                }
            };
			base.OnFlush += dt => djJtcy.Flush(_inputParam.Workspace, dt);
			base.OnImportFinish += t => djJtcy.Dispose();
        }
    }
}