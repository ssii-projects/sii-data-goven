using GeoAPI.Geometries;
using System;

namespace Agro.LibCore
{
    public class DBUtil
    {
		///// <summary>
		///// 显示SQLServer连接对话框
		///// 注意：中文界面需要在bin目录下面建一个zh-CHS的目录并将Microsoft.Data.ConnectionUI.Dialog.resources.dll拷入到该目录下
		///// </summary>
		///// <param name="initCS">初始化连接串</param>
		///// <returns></returns>
		//public static string ShowSQLServerConnectionDialog(string initCS=null)
		//{
		//	using (var dlg = new DataConnectionDialog())
		//	{
		//		dlg.DataSources.Clear();
		//		dlg.DataSources.Add(DataSource.SqlDataSource);
		//		dlg.SetSelectedDataProvider(DataSource.SqlDataSource, DataProvider.SqlDataProvider);

		//		if (initCS != null)
		//		{
		//			try
		//			{
		//				var wsf = SQLServerFeatureWorkspaceFactory.Instance;
		//				using (var ws = wsf.OpenWorkspace(initCS))
		//				{
		//					dlg.ConnectionString = initCS;
		//				}
		//			}
		//			catch
		//			{
		//			}
		//		}
		//		if (System.Windows.Forms.DialogResult.OK == DataConnectionDialog.Show(dlg))
		//		{
		//			return dlg.ConnectionString;
		//		}
		//		return null;
		//	}
		//}

		/// <summary>
		/// yxm 2019-11-7
		/// </summary>
		/// <param name="db"></param>
		/// <param name="action"></param>
		public static void UseTransaction(IWorkspace db, Action action,bool fThrowEx=true)
		{
			try
			{
				db.BeginTransaction();
				action();
				db.Commit();
			}
			catch (Exception ex)
			{
				Try.Catch(()=>db.Rollback(),false);
                if (fThrowEx)
                {
                    throw ex;
                }
			}
		}
	}
    public class FieldsUtil
    {
        /// <summary>
        /// 返回以逗号分割字段的拼接字符串
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string GetFieldsString(IFields fields)
        {
            string subFields = null;
            for (int i = 0; i < fields.FieldCount; ++i)
            {
                var field = fields.GetField(i);
                if (string.IsNullOrEmpty(subFields))
                {
                    subFields = field.FieldName;
                }
                else
                {
                    subFields += "," + field.FieldName;
                }
            }
            return subFields;
        }
        public static Field AddOidField(IFields fields, string fieldName = "oid", string aliasName = "FID")
        {
            return fields.AddField(CreateOIDField(fieldName, aliasName)) as Field;
        }
        public static Field AddGeometryField(IFields fields, string fieldName,eGeometryType geoType, string aliasName=null)//, int len)
        {
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeGeometry,
				GeometryType = geoType,
			};
            return fields.AddField(field) as Field;
        }
		public static void AddShortField(IFields fields, string fieldName, string aliasName=null)//, int len)
		{
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeSmallInteger,
			};
            //field.Length = len;
            fields.AddField(field);
        }
        public static void AddTextField(IFields fields, string fieldName, int len, string aliasName=null)
        {
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeString,
				Length = len
			};
            fields.AddField(field);
        }
		public static void AddDoubleField(IFields fields, string fieldName,int precision,int scale, string aliasName=null)
		{
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeDouble,
				Length = precision,
				Scale = scale
			};
			//field.Length = len;
			fields.AddField(field);
		}
		public static Field AddIntField(IFields fields, string fieldName, string aliasName=null)//, int precision, int scale)
		{
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeInteger,
				//Precision = precision,
				//Scale = scale
			};
			//field.Length = len;
			fields.AddField(field);
            return field;
		}
		public static void AddDateField(IFields fields, string fieldName, string aliasName = null)
		{
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeDate,
				//Precision = precision,
				//Scale = scale
			};
			//field.Length = len;
			fields.AddField(field);
		}
		public static void AddDateTimeField(IFields fields, string fieldName, string aliasName = null)
		{
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName,
				FieldType = eFieldType.eFieldTypeDateTime,
				//Precision = precision,
				//Scale = scale
			};
			//field.Length = len;
			fields.AddField(field);
		}
		public static IField CreateOIDField(string OIDFieldName="Objectid",string aliasName="FID")
        {
			var oidField = new Field()
			{
				FieldName = OIDFieldName,
				AliasName = "FID",
				FieldType = eFieldType.eFieldTypeOID,
				Length = 64,
				IsNullable = false,
				Editable = false,
			};
            return oidField;
        }
        public static IField CreateShapeField(eGeometryType geoType,string fieldName="Shape",string fieldAliasName=null)
        {
			var shpField = new Field()
			{
				FieldName = fieldName,
				AliasName = fieldAliasName ?? fieldName,
				FieldType = eFieldType.eFieldTypeGeometry,
				GeometryType = geoType
			};
            return shpField;
        }
        public static IField CreateStringField(string fieldName, int width, string aliasName = null, bool fNullable = true)
        {
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName ?? fieldName,
				FieldType = eFieldType.eFieldTypeString,
				IsNullable = fNullable,
				Length = width
			};
            return field;
        }
		public static IField CreateDoubleField(string fieldName, int width, int nScale, string aliasName = null, bool fNullable = true)
		{
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName ?? fieldName,
				FieldType = eFieldType.eFieldTypeDouble,
				IsNullable = fNullable,
				Length = width,
				Scale = nScale,
			};
            return field;
        }
        public static IField CreateBoolField(string fieldName,string aliasName = null, bool fNullable = true)
        {
            return CreateField(fieldName, eFieldType.eFieldTypeBool);
        }
        public static IField CreateField(string fieldName, eFieldType fieldType, string aliasName = null, bool fNullable = true)
        {
			var field = new Field()
			{
				FieldName = fieldName,
				AliasName = aliasName ?? fieldName,
				FieldType = fieldType,
				IsNullable = fNullable
			};
            return field;
        }
        public static eGeometryType QueryShapeType(IFields fields)
        {
            for (int i = 0; i < fields.FieldCount; ++i)
            {
                var field = fields.GetField(i);
                if (field.FieldType == eFieldType.eFieldTypeGeometry)
                {
                    return field.GeometryType;
                }
            }
            return eGeometryType.eGeometryNull;
        }

        public static string ToFieldTypeString(eFieldType ft)
        {
            switch (ft)
            {
                case eFieldType.eFieldTypeBlob:
                    return "二进制";
                case eFieldType.eFieldTypeDate:
                    return "日期";
                case eFieldType.eFieldTypeDateTime:
                    return "时间/日期";
                case eFieldType.eFieldTypeTime:
                    return "日期";
                case eFieldType.eFieldTypeTimeStamp:
                    return "时间戳";
                case eFieldType.eFieldTypeDouble:
                    return "双精度";
                case eFieldType.eFieldTypeGeometry:
                    return "几何对象";
                case eFieldType.eFieldTypeGlobalID:
                    return "GlobalID";
                case eFieldType.eFieldTypeGUID:
                    return "GUID";
                case eFieldType.eFieldTypeInteger:
                    return "整数";
                case eFieldType.eFieldTypeOID:
                    return "OID";
                case eFieldType.eFieldTypeRaster:
                    return "Raster";
                case eFieldType.eFieldTypeSingle:
                    return "单精度";
                case eFieldType.eFieldTypeSmallInteger:
                    return "短整形";
                case eFieldType.eFieldTypeString:
                    return "文本";
                case eFieldType.eFieldTypeXML:
                    return "XML";
                case eFieldType.eFieldTypeBool:
                    return "布尔型";
            }
            return "";
        }

		/// <summary>
		/// yxm 2018-11-30
		/// </summary>
		/// <param name="ft"></param>
		/// <returns></returns>
		public static Type ToDataTableType(eFieldType ft)
		{
            //var t = typeof(string);
            Type t = null;
			switch (ft)
			{
				case eFieldType.eFieldTypeBlob: t = typeof(byte[]); break;
				case eFieldType.eFieldTypeBool: t = typeof(bool); break;
				case eFieldType.eFieldTypeByte: t = typeof(byte); break;
				case eFieldType.eFieldTypeDate: t = typeof(DateTime); break;
				case eFieldType.eFieldTypeDateTime: t = typeof(DateTime); break;
				case eFieldType.eFieldTypeDouble: t = typeof(decimal); break;
				case eFieldType.eFieldTypeGeometry: t = typeof(IGeometry); break;
				case eFieldType.eFieldTypeGUID: t = typeof(Guid); break;
				case eFieldType.eFieldTypeInteger: t = typeof(int); break;
				case eFieldType.eFieldTypeOID: t = typeof(int); break;
				case eFieldType.eFieldTypeSingle: t = typeof(decimal); break;
				case eFieldType.eFieldTypeSmallInteger: t = typeof(short); break;
				case eFieldType.eFieldTypeTime: t = typeof(DateTime); break;
				case eFieldType.eFieldTypeTimeStamp: t = typeof(DateTime); break;
                case eFieldType.eFieldTypeString:t = typeof(string);break;
				default:
					{
						Console.WriteLine("todo...");
						break;
					};
			}
			return t;
		}

		public static eFieldType ToFieldType(LibCore.Database.EntityProperty ep)
		{
            if (ep.AutoIncrement)
            {
                return eFieldType.eFieldTypeOID;
            }
            if (ep.FieldType != eFieldType.eFieldTypeNull)
			{
				return ep.FieldType;
			}
			var t = ep.PropertyType;
			if (t == typeof(string)) return eFieldType.eFieldTypeString;
			if (t == typeof(byte[])) return eFieldType.eFieldTypeBlob;
			if (t == typeof(bool) || t == typeof(bool?)) return eFieldType.eFieldTypeBool;
			if (t == typeof(byte)) return eFieldType.eFieldTypeByte;
			if (t == typeof(DateTime) || t == typeof(DateTime?))
			{
				return !ep.Insertable?eFieldType.eFieldTypeTimeStamp:eFieldType.eFieldTypeDate;
			}
            if (t == typeof(decimal) || t == typeof(decimal?) || t == typeof(double) || t == typeof(double?))
            {
                return eFieldType.eFieldTypeDouble;
            }
			if (t == typeof(IGeometry)) return eFieldType.eFieldTypeGeometry;
			if (t == typeof(Guid) || t == typeof(Guid?)) return eFieldType.eFieldTypeGUID;
			if (t == typeof(float) || t == typeof(float?)) return eFieldType.eFieldTypeSingle;
			if (t == typeof(short) || t == typeof(short?)) return eFieldType.eFieldTypeSmallInteger;
			if (t == typeof(int) || t == typeof(int?) || t == typeof(long) || t == typeof(long?))
			{
				return !ep.Insertable?eFieldType.eFieldTypeOID:eFieldType.eFieldTypeInteger;
			}

			System.Diagnostics.Debug.Assert(false);

			return eFieldType.eFieldTypeNull;
		}
    }
}
