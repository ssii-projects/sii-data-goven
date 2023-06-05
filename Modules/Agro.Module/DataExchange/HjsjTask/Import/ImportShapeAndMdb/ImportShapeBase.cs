using Agro.GIS;
using Agro.Library.Common;
using Agro.Module.DataExchange;
using Agro.LibCore;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data;
using static Agro.Library.Handle.ImportShapeAndMdb.ShapeFileImportBase;
using System.Diagnostics;
using Agro.Library.Common.Repository;
using GeoAPI.Geometries;
//using Microsoft.SqlServer.Types;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
	public class  ImportShapeBase :ImportTableBase, IImportTable
    {
        //protected readonly string _tableName;
        protected string _oidFieldName = "BSM";
        protected string _shapeFileName = "SHAPE";
        protected string _shpFilePrefix;
        protected Action<HJDataRootPath, List<ShapeFileImportBase.ImportFieldMap>> OnPreImport;
        //protected Func<ShapeFileImportBase.GetValueParam, int,DataRow, object> onGetValue;
        protected Action<DataTable> OnDataTableCreated;
        protected Action<DataRow,int> OnRowDataFilled;
        protected HJDataRootPath _params;
        //public string TableName { get { return _tableName;}}// return TableNameConstants.KZD; } }
        protected readonly bool _fCheckDataExist;
        public ImportShapeBase(string tableName,string shpFilePrefix, bool fCheckDataExist = true,string oidFieldName="BSM")
            :base(tableName)
        {
            //_tableName = tableName;
            _shpFilePrefix = shpFilePrefix;
            _oidFieldName = oidFieldName;
            _fCheckDataExist = fCheckDataExist;
        }
        //var str = "Data Source=192.168.10.146;Initial Catalog=ARCSUMDATA;User ID=sa;Password=123456;";
        // @"C:\Users\Public\Nwt\cache\recv\李昌松\511702通川区\矢量数据\JZD5117022016.shp";
        public virtual void DoImport(InputParam prm, ReportInfo reportInfo, ICancelTracker cancel)
        {
            _params = prm;
            var dkShpFile = prm.dicShp[_shpFilePrefix];


            var db = prm.Workspace;
            using (var shp = new ShapeFile())
            {
                if (_fCheckDataExist && !PreImportCheck(db, reportInfo))
                {
                    return;
                }
				try
                {
                    if (string.IsNullOrEmpty(dkShpFile))
                    {
						if (_shpFilePrefix == "ZJ")
						{
							reportInfo.reportProgress(100);
							reportInfo.reportWarning("矢量数据目录下未找到以" + _shpFilePrefix + " 开头的shp文件！");
							return;
						}
                        throw new Exception("矢量数据目录下未找到以" + _shpFilePrefix + " 开头的shp文件！");
                    }

                    shp.Open(dkShpFile);
                    var sTableName = TableName;
                    var imp = new ShapeFileToSQLServer();
                    var fm = CreateFieldMap(db, shp, sTableName);
                    fm.RemoveAll(fmi =>StringUtil.isEqualIgnorCase(fmi.FieldName, _oidFieldName));
                    OnPreImport?.Invoke(prm, fm);
					DBUtil.UseTransaction(db, () =>imp.DoImport(db, sTableName, shp, fm, OnDataTableCreated, OnRowDataFilled, reportInfo,_shapeFileName));
                }
                catch (Exception ex)
                {
					reportInfo.reportError("错误：" + ex.Message);
                }
            }
        }
        public Type ToType(eFieldType fieldType)
        {
            switch (fieldType)
            {
                case eFieldType.eFieldTypeInteger:
                case eFieldType.eFieldTypeOID:
                    return typeof(int);
                case eFieldType.eFieldTypeSmallInteger:
                    return typeof(short);
                case eFieldType.eFieldTypeGeometry:
                    return typeof(IGeometry);
                case eFieldType.eFieldTypeString:
                    return typeof(string);
                case eFieldType.eFieldTypeDouble:
                    // return typeof(double);
                    return typeof(decimal);
                case eFieldType.eFieldTypeGUID:
                    return typeof(Guid);
				case eFieldType.eFieldTypeDateTime:
					return typeof(DateTime);
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            return null;
        }
    }

    public class ImportJbntbhq : ImportShapeBase
    {
        public ImportJbntbhq(bool fCheckDataExist = true) : base(TableNameConstants.JBNTBHQ, "JBNTBHQ",fCheckDataExist)
        {
            base.OnPreImport += (ip, lst) =>
            {
                var fmi = new ShapeFileImportBase.ImportFieldMap("QXDM", -1, eFieldType.eFieldTypeString);
                lst.Add(fmi);
            };
            
            int iQxdmField = -1;

            base.OnDataTableCreated += dt =>
            {
                DataTable d = dt;
                for(int c = 0; c < d.Columns.Count; ++c)
                {
                    if (d.Columns[c].ColumnName == "QXDM")
                    {
                        iQxdmField = c;
                        break;
                    }
                };
            };

            base.OnRowDataFilled += (dr, iShpOID) =>
            {
                dr[iQxdmField]= _params.sXxzqhdm;
            };

            //base.onGetValue += (v, c, dr) =>
            //{
            //    if (iQxdmField == -1)
            //    {
            //        if (v.fieldMap.FieldName == "QXDM")
            //        {
            //            iQxdmField = c;
            //        }
            //    }
            //    if (iQxdmField == c)
            //    {
            //        v.Handled = true;
            //        return _params.sXxzqhdm;
            //    }
            //    return null;
            //};
        }
    }
    public class ImportQyjx : ImportShapeBase
    {
        public ImportQyjx(bool fCheckDataExist = true) : base(TableNameConstants.QYJX, "QYJX",fCheckDataExist)
        {
            base.OnPreImport += (ip, lst) =>
            {
                var fmi = new ShapeFileImportBase.ImportFieldMap("SZDY", -1, eFieldType.eFieldTypeString);
                lst.Add(fmi);
            };
            int iSzdy = -1;
            base.OnDataTableCreated += dt =>
            {
                DataTable d = dt;
                for (int c = 0; c < d.Columns.Count; ++c)
                {
                    if (d.Columns[c].ColumnName == "SZDY")
                    {
                        iSzdy = c;
                        break;
                    }
                };
            };

            base.OnRowDataFilled += (dr, iShpOID) =>
            {
                dr[iSzdy] = _params.sXxzqhdm;
            };
            //base.onGetValue += (v, c, dr) =>
            //{
            //    if (iSzdy == -1)
            //    {
            //        if (v.fieldMap.FieldName == "SZDY")
            //        {
            //            iSzdy = c;
            //        }
            //    }
            //    if (iSzdy == c)
            //    {
            //        v.Handled = true;
            //        return _params.sXxzqhdm;
            //    }
            //    return null;
            //};
        }
    }

    public class ImportKzd: ImportShapeBase
    {
        public ImportKzd(bool fCheckDataExist = true) : base(TableNameConstants.KZD, "KZD",fCheckDataExist)
        {
            base.OnPreImport += (ip, lst) =>
            {
                var fmi = new ShapeFileImportBase.ImportFieldMap("QXDM", -1, eFieldType.eFieldTypeString);
                lst.Add(fmi);
            };

            int iSzdy = -1;
            base.OnDataTableCreated += dt =>
            {
                DataTable d = dt;
                for (int c = 0; c < d.Columns.Count; ++c)
                {
                    if (d.Columns[c].ColumnName == "QXDM")
                    {
                        iSzdy = c;
                        break;
                    }
                };
            };

            base.OnRowDataFilled += (dr, iShpOID) =>
            {
                dr[iSzdy] = _params.sXxzqhdm;
            };
            //base.onGetValue += (v, c, dr) =>
            //{
            //    if (iSzdy == -1)
            //    {
            //        if (v.fieldMap.FieldName == "QXDM")
            //        {
            //            iSzdy = c;
            //        }
            //    }
            //    if (iSzdy == c)
            //    {
            //        v.Handled = true;
            //        return _params.sXxzqhdm;
            //    }
            //    return null;
            //};
        }
    }

	public class ImportMzdw : ImportShapeBase
	{
		public ImportMzdw(bool fCheckDataExist = true) : base(TableNameConstants.MZDW, "MZDW", fCheckDataExist)
		{
			base.OnPreImport += (ip, lst) => lst.Add(new ImportFieldMap("QXDM", -1, eFieldType.eFieldTypeString));

			int iSzdy = -1;
			base.OnDataTableCreated += dt =>
			{
				DataTable d = dt;
				for (int c = 0; c < d.Columns.Count; ++c)
				{
					if (d.Columns[c].ColumnName == "QXDM")
					{
						iSzdy = c;
						break;
					}
				};
			};

			base.OnRowDataFilled += (dr, iShpOID) =>
			{
				dr[iSzdy] = _params.sXxzqhdm;
			};
		}
	}
}