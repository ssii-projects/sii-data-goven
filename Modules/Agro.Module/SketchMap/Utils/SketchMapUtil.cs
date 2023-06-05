using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.GIS;
using Agro.Library.Common;
using Agro.Library.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Shapes;


/*
yxm created at 2019/1/28 10:15:02
*/
namespace Agro.Module.SketchMap
{
	public class SketchMapUtil
	{
		/// <summary>
		/// 初始化地块示意图
		/// </summary>
		/// <param name="pl"></param>
		/// <param name="spatialReference"></param>
		/// <param name="land"></param>
		/// <param name="lands"></param>
		public static void InitalizeOwnerView(IPageLayout pl, SpatialReference spatialReference, VEC_CBDK land, List<VEC_CBDK> lands
			,string kptFileName=null,bool fUpdateSz=true)
		{
			if (land == null)
			{
				return;
			}
			var av = pl as IActiveView;

			string tmplFile = kptFileName;
			if (kptFileName == null)
			{
				tmplFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\地块示意图\地块四至示意图.kpd";
			}
			pl.OpenDocument(tmplFile, false);


			var map = av.FocusMap as GIS.Map;
			map.SetSpatialReference(spatialReference, false);
			var layer2 = map.Layers.GetLayer(0) as IFeatureLayer;
			var layer1 = map.Layers.GetLayer(1) as IFeatureLayer;
			var layer3 = map.Layers.GetLayer(2) as IFeatureLayer;
			if (layer3.Renderer is SimpleFeatureRenderer sfr)
			{
				sfr.SetSymbol(SymbolUtil.CreateSimpleMarkerSymbol(System.Drawing.Color.White, System.Drawing.Color.Red, 4
					, eSimpleMarkerStyle.Circle, 0.5));
			}
			var fc1 = MyMemorySourceUtil.CreateAreaFeatureClass("fc1", true);
			var fc2 = MyMemorySourceUtil.CreateFeatureClass("fc2", eGeometryType.eGeometryPolyline);
			var fc3 = MyMemorySourceUtil.CreateFeatureClass("fc3", eGeometryType.eGeometryPoint);
			layer1.FeatureClass = fc1;
			layer2.FeatureClass = fc2;
			layer3.FeatureClass = fc3;

			var ft = fc1.CreateFeature();
			ft.Shape = land.Shape.Geometry;
			IRowUtil.SetRowValue(ft, "SCMJM", land.SCMJM);
			var dkbm = land.DKBM ?? "";
			if (dkbm.Length > 5)
			{
				dkbm = dkbm.Substring(dkbm.Length - 5);
			}
			IRowUtil.SetRowValue(ft, "DKBM", dkbm);
			IRowUtil.SetRowValue(ft, "DKMC", land.DKMC);
			fc1.Append(ft);

			var env = ft.Shape.EnvelopeInternal;
			var fullEnv = new OkEnvelope(env);
			env = env.ScaleAt(1.15, env.Centre.X, env.Centre.Y);

			map.FullExtent = fullEnv;
			map.SetExtent(new OkEnvelope(env), false);

			#region 添加节点
			var lstVertex = InitalizeCircleView(land);
			foreach (var coord in lstVertex)
			{
				var ft1 = fc3.CreateFeature();
				ft1.Shape = GeometryUtil.MakePoint(coord);
				fc3.Append(ft1);
			}
			#endregion

			if (fUpdateSz)
			{
				#region 设置四至
				var gc = av.GraphicsContainer;
				gc.EnumElement(e =>
				{
					if (e is ITextElement te)
					{
						switch (te.Name)
						{
							case "DKDZ": te.Text = land.DKDZ; break;
							case "DKXZ": te.Text = land.DKXZ; break;
							case "DKNZ": te.Text = land.DKNZ; break;
							case "DKBZ": te.Text = land.DKBZ; break;
						}
					}
				});
				#endregion
			}

			#region 创建临宗-邻宗图形集合
			var landBuffer = InitalizeLandBuffer(land, 5);
			var neighborLands = lands.FindAll(ld => ld.DKBM != land.DKBM && ld.Shape.Geometry.Intersects(landBuffer));//查找邻宗地块
			foreach (var dk in neighborLands)
			{
				var sr = dk.Shape.GetSpatialReference();
				GeometryUtil.EnumEdge(dk.Shape.Geometry, (c0, c1) =>
				{
					var ls = GeometryUtil.MakeLineString(c0, c1, sr);
					if (landBuffer.Intersects(ls))
					{
						var ft2 = fc2.CreateFeature();
						ft2.Shape = ls;
						fc2.Append(ft2);
					}
				});
			}

			#endregion
		}

		/// <summary>
		/// 获取邻宗地
		/// </summary>
		/// <param name="cbfBm"></param>
		/// <param name="cancel"></param>
		/// <returns></returns>
		public static List<VEC_CBDK> QueryDKByCbfbm(string cbfBm, ContractLand[] cLands, ICancelTracker cancel,bool fCheckZT)
		{
			var lands = new List<VEC_CBDK>();
			//var db = MyGlobal.Workspace;
			//var fields = "DKBM,SCMJ,SCMJM,Shape,DKDZ,DKXZ,DKNZ,DKBZ,DKMC";
			var ztWhere = fCheckZT ? $"and ZT={(int)EDKZT.Youxiao}" : $"and ZT<2";
			//var sql1 = "";
			//var sql2 = "";

            using var fc = MyGlobal.Workspace.OpenFeatureClass("DLXX_DK");
			var qf = new QueryFilter()
            {
                SubFields = "DKBM,SCMJ,SCMJM,Shape,DKDZ,DKXZ,DKNZ,DKBZ,DKMC",
            };

            if (cbfBm != null)
			{
				if (cbfBm.Length > 14)
				{
					cbfBm = cbfBm.Substring(0, 14);
				}
				//sql1 = $"select {fields} from DLXX_DK where Shape is not null {ztWhere} and DKBM like '{cbfBm}%'";
				//db.QueryCallback(sql1, r =>
				//{
				//	var en = Convert(r);
				//	lands.Add(en);
				//}, cancel);
				qf.WhereClause = $"Shape is not null {ztWhere} and DKBM like '{cbfBm}%'";
                fc.Search(qf, r =>
                {
                    var en = Convert(r);
                    //ReplaceDXNBZ(en, cl);
					lands.Add(en);
                    return !cancel.Cancel();
                });
            }

			SqlUtil.ConstructIn(cLands.Select(i => i.DKBM), sin =>
			   {
				   //sql2 = $"select {fields} from DLXX_DK where Shape is not null {ztWhere} and DKBM in({sin})";
				   //db.QueryCallback(sql2, r =>
				   //{
					  // var en = Convert(r);
					  // if (lands.Find(it => it.DKBM == en.DKBM) == null)
					  // {
						 //  lands.Add(en);
					  // }
				   //}, cancel);

				   qf.WhereClause = $"Shape is not null {ztWhere} and DKBM in({sin})";
				   fc.Search(qf, r =>
				   {
					   var en = Convert(r);
                       if (lands.Find(it => it.DKBM == en.DKBM) == null)
                       {
                           lands.Add(en);
                       }
					   return !cancel.Cancel();
                   });


               });
			//if (lands.Count == 0)
			//{
				
			//}
			return lands;
		}

		public static ContractConcord ToContractConcord(TaskSketchMapArgumet.CbfItem it)
		{
			var cc = new ContractConcord()
			{
				CBFBM = it.CBFBM,
				CBFMC = it.CbfMc
			};
			var lst = new List<ContractLand>();
			var sql = $" select a.DKBM,HTMJ,HTMJM,a.SFQQQG,DKDZ,DKNZ,DKXZ,DKBZ from QSSJ_CBDKXX a left join DLXX_DK b on a.DKBM=b.DKBM where CBFBM='{it.CBFBM}' and b.DJZT={(int)EDjzt.Ydj}";
			//var sql = $" select a.DKBM,HTMJ,HTMJM,a.SFQQQG,DKDZ,DKNZ,DKXZ,DKBZ from QSSJ_CBDKXX a left join DLXX_DK b on a.DKBM=b.DKBM where CBFBM='{it.CBFBM}'";
			MyGlobal.Workspace.QueryCallback(sql, r =>
			{
				int i = -1;
				var c = new ContractLand()
				{
					DKBM = r.GetString(++i),
					HTMJ = SafeConvertAux.ToDouble(r.GetValue(++i)),
					HTMJM = SafeConvertAux.ToDouble(r.GetValue(++i)),
					IsShared = r.IsDBNull(++i) ? "" : r.GetString(i),
					DKDZ = GetStr(r, ++i),
					DKNZ = GetStr(r, ++i),
					DKXZ = GetStr(r, ++i),
					DKBZ = GetStr(r, ++i)
				};
				lst.Add(c);
				return true;
			});
			cc.Lands = lst.ToArray();
			return cc;
		}
		static string GetStr(IDataReader r, int c)
		{
			return r.IsDBNull(c) ? "" : r.GetString(c);
		}
        static string? GetStr(IRow r, int c)
        {
            return r.GetValue(c)?.ToString();
        }

        //internal static List<VEC_CBDK> ToVecCBDK(ContractLand[] cl, ICancelTracker cancel)
        //{
        //	var lands = new List<VEC_CBDK>();
        //	var lst = new List<string>();
        //	foreach (var c in cl) lst.Add(c.DKBM);
        //	SqlHelper.ConstructIn(lst, sin =>
        //	{
        //		MyGlobal.Workspace.QueryCallback($"select DKBM,SCMJ,SCMJM,Shape,DKDZ,DKXZ,DKNZ,DKBZ from DLXX_DK where Shape is not null and DKBM in ({sin})", r =>
        //		{
        //			var en = Convert(r);

        //			#region 如果dat文件中包含地块东西南北至则以dat文件为准
        //			foreach (var c in cl)
        //			{
        //				if (c.DKBM == en.DKBM)
        //				{
        //					ReplaceDXNBZ(en, c);
        //					break;
        //				}
        //			}
        //			#endregion

        //			lands.Add(en);
        //			return true;
        //		}, cancel);
        //	});
        //	return lands;
        //}

        public static VEC_CBDK? QueryDKByDkbm(ContractLand cl)
		{
			//var db = MyGlobal.Workspace;
			//var shape = "shape";
			//if (db.DatabaseType == eDatabaseType.MySql)
			//{
			//	shape = db.SqlFunc.AsBinary(shape);
			//}
			VEC_CBDK? en = null;
			//var sql = $"select DKBM,SCMJ,SCMJM,{shape},DKDZ,DKXZ,DKNZ,DKBZ,DKMC from DLXX_DK where Shape is not null and DKBM = '" + cl.DKBM + "'";
   //         db.QueryCallback(sql, r =>
			//{
			//	en = Convert(r);
			//	ReplaceDXNBZ(en, cl);
			//	return false;
			//});

            using var fc = MyGlobal.Workspace.OpenFeatureClass("DLXX_DK");
            var qf = new QueryFilter()
            {
                WhereClause = $"Shape is not null and DKBM = '{cl.DKBM}'",
                SubFields = "DKBM,SCMJ,SCMJM,shape,DKDZ,DKXZ,DKNZ,DKBZ,DKMC",
            };
            fc.Search(qf, r =>
            {
                en = Convert(r);
                ReplaceDXNBZ(en, cl);
            });

            return en;
		}

		/// <summary>
		/// 用cl替换en中的东西南北至
		/// </summary>
		/// <param name="en"></param>
		/// <param name="cl"></param>
		internal static void ReplaceDXNBZ(VEC_CBDK en, ContractLand cl)
		{
			if (!string.IsNullOrEmpty(cl.DKDZ)) en.DKDZ = cl.DKDZ;
			if (!string.IsNullOrEmpty(cl.DKNZ)) en.DKNZ = cl.DKNZ;
			if (!string.IsNullOrEmpty(cl.DKXZ)) en.DKXZ = cl.DKXZ;
			if (!string.IsNullOrEmpty(cl.DKBZ)) en.DKBZ = cl.DKBZ;
		}
		/// <summary>
		/// 查找en附近的地块（不包含en)
		/// en.Shape有效
		/// </summary>
		/// <param name="en"></param>
		/// <param name="lst"></param>
		/// <param name="bufferSize"></param>
		/// <returns></returns>
		public static List<VEC_CBDK> QueryNeareastDK(VEC_CBDK en, List<VEC_CBDK>? lst=null, double bufferSize=5.0)
		{
			if (lst == null)
			{
				lst = new List<VEC_CBDK>();
			}
			var buffer = en.Shape.Geometry;
			if (bufferSize > 0)
			{
				buffer = buffer.Buffer(bufferSize);
			}

			using (var fc = MyGlobal.Workspace.OpenFeatureClass("DLXX_DK"))
			{
				var qf = new SpatialFilter()
				{
					Geometry = buffer,
					SpatialRel = eSpatialRelEnum.eSpatialRelIntersects,
					WhereClause = "DKBM<>'" + en.DKBM + "'",
					SubFields =fc.ShapeFieldName,
				};
				fc.SpatialQuery(qf, ft =>lst.Add(new VEC_CBDK(){Shape = new SerialShape(ft.Shape)}));
			}
			return lst;
		}

        private static VEC_CBDK Convert(IRow r)
        {
            var en = new VEC_CBDK()
            {
                DKBM =r.GetValue(0)?.ToString()??"",// r.GetString(0),
                SCMJ = SafeConvertAux.ToDouble(r.GetValue(1)),
                SCMJM = SafeConvertAux.ToDouble(r.GetValue(2)),
                Shape = new SerialShape((r as IFeature)?.Shape),// r.GetValue(3) as IGeometry),
                DKDZ = GetStr(r, 4),// r.IsDBNull(4) ? "" : r.GetString(4),
                DKXZ = GetStr(r, 5),//.IsDBNull(5) ? "" : r.GetString(5),
                DKNZ = GetStr(r, 6),//.IsDBNull(6) ? "" : r.GetString(6),
                DKBZ = GetStr(r, 7),//.IsDBNull(7) ? "" : r.GetString(7),
                DKMC = GetStr(r, 8),
            };
            //#if DEBUG
            //			var g = en.Shape?.Geometry;
            //			Console.WriteLine($"DKBM={en.DKBM},g={g?.AsText()}");
            //#endif
            return en;
        }

//        private static VEC_CBDK Convert(IDataReader r)
//		{
//			var en = new VEC_CBDK()
//			{
//				DKBM = r.GetString(0),
//				SCMJ = SafeConvertAux.ToDouble(r.GetValue(1)),
//				SCMJM = SafeConvertAux.ToDouble(r.GetValue(2)),
//				Shape = new SerialShape(r.IsDBNull(3) ? null :r.GetShape(3)),// r.GetValue(3) as IGeometry),
//				DKDZ = GetStr(r, 4),// r.IsDBNull(4) ? "" : r.GetString(4),
//				DKXZ =GetStr( r, 5),//.IsDBNull(5) ? "" : r.GetString(5),
//				DKNZ =GetStr( r, 6),//.IsDBNull(6) ? "" : r.GetString(6),
//				DKBZ =GetStr( r, 7),//.IsDBNull(7) ? "" : r.GetString(7),
//				DKMC=GetStr(r,8),
//			};
////#if DEBUG
////			var g = en.Shape?.Geometry;
////			Console.WriteLine($"DKBM={en.DKBM},g={g?.AsText()}");
////#endif
//			return en;
//		}
		/// <summary>
		/// 初始化地块缓冲图形
		/// </summary>
		/// <returns></returns>
		private static IGeometry InitalizeLandBuffer(VEC_CBDK land, double distance = 5.0)
		{
			if (land.Shape == null)
			{
				return null;
			}
			var landBuffer = land.Shape.Geometry;
			try
			{
				if (landBuffer != null && landBuffer.IsValid)
				{
					landBuffer = landBuffer.Buffer(distance);
				}
			}
			catch (SystemException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
			return landBuffer;
		}
		/// <summary>
		/// 添加节点标注
		/// </summary>
		private static List<Coordinate> InitalizeCircleView(VEC_CBDK localLand)
		{
			var points = localLand.Shape.Geometry.Coordinates;
			var cds = points.ToList();
			cds = InitalizeLabelCircle(cds);
			cds = VacuteDotRing(cds);
			return cds;
		}

		/// <summary>
		/// 根据设置，没有界址信息的情况下，抽稀全部的点
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		private static List<Coordinate> VacuteDotRing(List<Coordinate> points)
		{
			var filterGeos = new List<Coordinate>();
			double vacuteDotDistence = 3.0;
			filterGeos.Add(points[0]);
			for (int i = 1; i < points.Count; i++)
			{
				if (i != points.Count - 1)
				{
					var ii1distance = points[i].Distance(points[i - 1]);
					if (ii1distance < vacuteDotDistence)
					{
						points.Remove(points[i]);
						i = i - 1;
					}
					else if (ii1distance > vacuteDotDistence)
					{
						filterGeos.Add(points[i]);
					}
				}
				else
				{
					var ii1distance = points[i].Distance(points[0]);
					if (ii1distance < vacuteDotDistence)
					{
						points.Remove(points[i]);
					}
					else if (ii1distance > vacuteDotDistence)
					{
						if (filterGeos.Contains(points[i]) == false)
						{
							filterGeos.Add(points[i]);
						}
					}
				}
			}
			return filterGeos;
		}

		/// <summary>
		/// 获取标注点坐标集合
		/// </summary>
		/// <param name="polygon">多段线</param>
		/// <param name="points">点集合</param>
		/// <returns></returns>
		private static List<Coordinate> InitalizeLabelCircle(List<Coordinate> points)
		{
			var markPoints = new List<Coordinate>();
			Coordinate basePoint;
			for (int i = 0; i < points.Count - 2; i++)
			{
				basePoint = InitalizeMarkPoint(points[i], points[i + 1], points[i + 2]);//获取标注点
				if (basePoint == null)
				{
					continue;
				}
				markPoints.Add(basePoint);// CSharpFramework.Spatial.Geometry.CreatePoint(basePoint, Reference.WKID));
			}
			basePoint = InitalizeMarkPoint(points[points.Count - 2], points[points.Count - 1], points[0]);//获取标注点
			if (basePoint != null)
			{
				markPoints.Add(basePoint);// CSharpFramework.Spatial.Geometry.CreatePoint(basePoint, Reference.WKID));
			}
			basePoint = InitalizeMarkPoint(points[points.Count - 1], points[0], points[1]);//获取标注点
			if (basePoint != null)
			{
				markPoints.Add(basePoint);// CSharpFramework.Spatial.Geometry.CreatePoint(basePoint, Reference.WKID));
			}
			return markPoints;
		}
		/// <summary>
		/// 获取标注点
		/// </summary>
		/// <param name="polygon">多段线</param>
		/// <param name="firstPoint">第一点</param>
		/// <param name="secondPoint">第二点</param>
		/// <param name="thirdPoint">第三点</param>
		/// <param name="dist">间距</param>
		/// <returns></returns>
		private static Coordinate InitalizeMarkPoint(Coordinate firstPoint, Coordinate secondPoint, Coordinate thirdPoint)
		{
			double startAng = InitalizeAngle(secondPoint, firstPoint);
			double endAng = InitalizeAngle(secondPoint, thirdPoint);
			double angle = Math.Abs(endAng - startAng);
			if (angle > 180)
			{
				angle = angle - 180;
			}
			if (angle <= 150)
			{
				return secondPoint;
			}
			return null;
		}
		/// <summary>
		/// 计算两点角度
		/// </summary>
		/// <param name="firPoint"></param>
		/// <param name="secPoint"></param>
		/// <returns></returns>
		private static double InitalizeAngle(Coordinate firPoint, Coordinate secPoint)
		{
			double y = secPoint.Y - firPoint.Y;
			double x = secPoint.X - firPoint.X;
			double angle = Math.Atan2(y, x) * 180 / Math.PI;
			if (angle < 0)
			{
				angle = 360 + angle;
			}
			return angle;
		}
	}

	class MyMemorySourceUtil
	{
		public static IFeatureClass CreateAreaFeatureClass(string tableName, bool fHasLabel = false)
		{
			var fields = new Fields();
			fields.AddField(FieldsUtil.CreateOIDField("OID"));
			if (fHasLabel)
			{
				FieldsUtil.AddDoubleField(fields, "SCMJM", 15, 2);
				FieldsUtil.AddTextField(fields, "DKBM", 5);
				FieldsUtil.AddTextField(fields, "DKMC", 50);
			}
			fields.AddField(FieldsUtil.CreateShapeField(eGeometryType.eGeometryPolygon));
			using (var ws = MemoryWorkspaceFactory.Instance.OpenWorkspace("Temp"))
			{
				ws.CreateFeatureClass(tableName, fields, 0);
				return ws.OpenFeatureClass(tableName);
			}
		}
		public static IFeatureClass CreateFeatureClass(string tableName, eGeometryType geometryType)
		{
			var fields = new Fields();
			fields.AddField(FieldsUtil.CreateOIDField("OID"));
			fields.AddField(FieldsUtil.CreateShapeField(geometryType));// eGeometryType.eGeometryPoint));
			using (var ws = MemoryWorkspaceFactory.Instance.OpenWorkspace("Temp"))
			{
				ws.CreateFeatureClass(tableName, fields, 0);
				return ws.OpenFeatureClass(tableName);
			}
		}
	}
}
