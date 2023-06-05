
//using Microsoft.SqlServer.Types;
using Agro.GIS;
using Agro.Library.Common;
using Agro.LibCore;
using NetTopologySuite.Geometries;
/*
* (C) 2017 xx公司版权所有，保留所有权利
*
* CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
* 文 件 名：   ShapeFileToSQLServer
* 创 建 人：   颜学铭
* 创建时间：   2017/5/26 10:36:15
* 版    本：   1.0.0
* 备注描述：
* 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GeoAPI.Geometries;

namespace Agro.Library.Handle.ImportShapeAndMdb
{
    public class ShapeFileToSQLServer:ShapeFileImportBase
    {
        /// <summary>
        /// 将ShapeFile导入到SqlServer数据库
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tableName"></param>
        /// <param name="shp"></param>
        /// <param name="lstFieldMap">字段映射</param>
        /// <param name="onGetValue"></param>
        /// <param name="reportProgress"></param>
        /// <param name="fClearAllOldData"></param>
        /// <returns></returns>
        public LogInfo DoImport(IWorkspace db, string tableName, ShapeFile shp, 
            List<ImportFieldMap> lstFieldMap,
            Action<DataTable> onDataTableCreated,
            Action<DataRow,int> onDataRowFilled,//  Func<GetValueParam,int,DataRow, object> onGetValue,
            ReportInfo reportInfo
            ,string shapeFieldName="SHAPE")
        {
            var logInfo = new LogInfo();
            var startTime = DateTime.Now.ToString();
            
            var srid = db.GetSRID(tableName);
			var spatialReference = SpatialReferenceFactory.CreateFromEpsgCode(srid);


			int cnt = shp.GetRecordCount();
			var progress = new ProgressReporter(reportInfo.reportProgress, cnt);

			if (cnt > 0)
			{
				logInfo.RecordCount = cnt;
				Console.WriteLine(tableName + ":记录条数：" + cnt);
				// double oldProgress = 0;
				var tableMeta = db.QueryTableMeta(tableName);

				var prm = new GetValueParam
				{
					shp = shp
				};
				using (var dt = new DataTable(tableName))
				{
					//dt.Columns.Add(oidFieldName, typeof(int));
					dt.Columns.Add(shapeFieldName, typeof(IGeometry));
					foreach (var l in lstFieldMap)
					{
						var t = TaskImportUtil.ToType(l.fieldType);
						dt.Columns.Add(l.FieldName, t);
					}
					onDataTableCreated?.Invoke(dt);

					//var lstNotNullableFieldIdx = FindNotNullableFieldMap(dt, tableMeta);

					//int oid = db.GetNextObjectID(tableName);
					for (int i = 0; i < cnt; ++i)
					{
						progress.Step();
						if (shp.IsFeatureDeleted(i))
							continue;

						var dr = dt.NewRow();

						//dr[0] = oid++;
						var g = shp.GetGeometry(i);
						if (g != null)
						{
							g = g.Project(spatialReference);
						}
						bool fShapeValid = g != null;
						if (g is LineString && g.Length == 0)
						{
							fShapeValid = false;
						}
						else if ((g is Polygon || g is MultiPolygon) && g.Area == 0)
						{
							fShapeValid = false;
						}
						if (fShapeValid)
						{
							//var bc = new System.Data.SqlTypes.SqlBytes(g.AsBinary());
							//var sg = SqlGeometry.STGeomFromWKB(bc, srid);
							//dr[0] = sg;
							dr[0] = g;
						}
						else
						{
							dr[0] = null;
						}
						prm.row = i;

						int c = 0;
						foreach (var it in lstFieldMap)
						{
							prm.fieldMap = it;
							prm.Handled = false;
							object o = null;
							if (it.iShpField >= 0)
							{
								o = shp.GetFieldValue(i, it.iShpField);
							}
							//var o = onGetValue(prm,c,dr);
							//if (!prm.Handled)
							//{
							//    o = shp.GetFieldValue(i, it.iShpField);
							//}
							dr[++c] = o ?? DBNull.Value;//:o;
						}
						onDataRowFilled?.Invoke(dr, i);

						//foreach (var iClm in lstNotNullableFieldIdx)
						//{
						//	if (dr[iClm] == null)
						//	{
						//		throw new Exception($"字段:{dt.Columns[i].ColumnName}不能为空");
						//	}
						//}
						dt.Rows.Add(dr);
						if (dt.Rows.Count >= 50000)
						{
							db.SqlBulkCopyByDatatable(tableMeta, dt);
							db.GetNextObjectID(tableName, dt.Rows.Count - 1);
							dt.Rows.Clear();
						}

					}
					if (dt.Rows.Count > 0)
					{
						db.SqlBulkCopyByDatatable(tableMeta, dt);
						//db.GetNextObjectID(tableName, dt.Rows.Count - 1);
						dt.Rows.Clear();
					}
					//ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);

				}
				//sw.Stop();
				//logInfo.Times = sw.Elapsed;
				//Console.WriteLine("导入" + tableName + "完成，耗时：" + logInfo.Times.ToString()
				//    +",开始时间："+startTime);
			}

			progress.ForceFinish();
			return logInfo;
        }


		private static List<int> FindNotNullableFieldMap(DataTable dt, TableMeta tm)
		{
			var lst = new List<int>();
			var fields=tm.Fields;
			for (var i = fields.FieldCount; --i >= 0;)
			{
				var field=fields.GetField(i);
				if (!field.IsNullable&&!field.AutoIncrement)
				{
					var n=DataTableUtil.FindField(dt, field.FieldName);
					if (n < 0) throw new Exception($"DataTable中必须包含必填字段：{field.FieldName}");
					lst.Add(i);
				}
			}
			return lst;
		}
    }
    
}
