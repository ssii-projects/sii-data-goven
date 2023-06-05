using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.GIS;
using Agro.LibCore.Task;
using Agro.Library.Common;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;



/*
yxm created at 2019/1/16 11:13:39
*/
namespace Agro.Module.SketchMap
{
	public class ExportSketchMapTask : Task
	{

		private string FilePath;
		private readonly PageLayoutControl _plc;
		public readonly SpatialReference _spatialReference;

		private readonly SketchMapExporter _exporter;

		public ExportSketchMapTask(PageLayoutControl plc)
		{
			Name = "生成承包地块示意图";
			Description = "根据承包方下承包地块空间形状生成承包地块示意图";
			base.PropertyPage = new DataSelectedDialog();
			_plc = plc;
			var window = Window.GetWindow(plc);
			var srid = MyGlobal.Workspace.GetSRID("DLXX_DK");
			_spatialReference = SpatialReferenceFactory.CreateFromEpsgCode(srid);
			_exporter = new SketchMapExporter(window, _spatialReference, _plc);
		}
		protected override void DoGo(ICancelTracker cancel)
		{

			var args = (DataSelectedDialog)base.PropertyPage;
			_exporter.SetMapProperty(args.Argument.MapProperty);

			ArcDataProgress(args, cancel);
		}

		/// <summary>
		/// 数据处理
		/// </summary>
		private void ArcDataProgress(DataSelectedDialog dialog, ICancelTracker cancel)
		{
			var args = dialog.Argument;
			if (args == null)
			{
				return;
			}
			base.UpdateStateInfo("正在获取数据...");
			base.ReportProgress(1);
			//this.ReportProgress(1, "正在获取数据...");
			FilePath = args.OutputPath;
			var concords = new List<ContractConcord>();
			foreach (var o in args.FileNames)
			{
				try
				{
					switch (args.DataType) 
					{
						case TaskSketchMapArgumet.DType.DatFile:
							{
								var fileName = o.ToString();
								var datas = DataExchange.Deserialize(fileName);
								if(datas?.Count == 0)
								{
									base.ReportInfomation(Path.GetFileName(fileName) + "中数据无效,请确认数据是否是承包地块数据!");
									continue;
								}
								concords.AddRange(datas);
							}break;
						case TaskSketchMapArgumet.DType.CbfItem:
							concords.Add(SketchMapUtil.ToContractConcord(o as TaskSketchMapArgumet.CbfItem));
							break;
						default:
							System.Diagnostics.Debug.Assert(false);
							break;
					}
				}
				catch (SystemException ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
					this.ReportException(ex);
				}
			}
			var db = MyGlobal.Workspace;
			if (db == null)
			{
				this.ReportError("数据源无效!");
				return;
			}
			var progress = new ProgressReporter(ReportProgress, concords.Count);
			foreach (var concord in concords)
			{
				progress.Step();
				base.UpdateStateInfo(concord.CBFMC);
				if (concord.Lands== null||concord.Lands.Length==0)
				{
					ReportWarning(concord.CBFMC + "无地块数据!");
					continue;
				}
				var senderCode = DataExchange.InitalizeSenderCode(concord);
				var lands = SketchMapUtil.QueryDKByCbfbm(senderCode,concord.Lands, cancel,args.DataType==TaskSketchMapArgumet.DType.CbfItem);
				if (lands.Count == 0)
				{
					ReportWarning(concord.CBFMC + "无地块数据!");
					continue;
				}
				string filePath = FilePath + @"\" + concord.CBFMC;
				try
				{
					_exporter.ExportSketchMapByContractor(concord, lands, filePath);
				}
				catch (Exception ex)
				{
					ReportException(ex);
				}
				lands = null;
				GC.Collect();
				this.ReportInfomation(concord.CBFMC + "地块示意图成功导出!");
			}
			concords = null;
			GC.Collect();
		}
		/*
		/// <summary>
		/// 根据承包方导出地块示意图
		/// </summary>
		private void ExportSketchMapByContractor(ContractConcord concord, List<VEC_CBDK> lands)
		{
			string filePath = FilePath + @"\" + concord.CBFMC;
			if (!System.IO.Directory.Exists(filePath))
			{
				System.IO.Directory.CreateDirectory(filePath);
			}

			var docTmplFile=AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\农村土地承包经营权承包地块示意图.dot";
			string fileName = filePath + @"\" + "DKSYT" + concord.CBFBM + "J";
			//var entry = new SketchMapEntry();
			InitalizeAllView(concord, lands, filePath);
			InitalizeLocalView(concord, lands, filePath);
			var data = new SkecthMapExport
			{
				MapProperty = MapProperty,
				Contractor = concord,
				DKS = lands,
				FilePath = filePath + @"\Jpeg\"
			};
			if (!System.IO.Directory.Exists(data.FilePath))
			{
				System.IO.Directory.CreateDirectory(data.FilePath);
			}
			data.CanEditor = true;
			data.FileName = fileName;
			data.OpenTemplate(docTmplFile);
			if (MapProperty.SaveDocFormat && MapProperty.SavePdfFormat && MapProperty.SaveJpgFormat)
			{
				data.SaveAsDocAndPdfAndJpgFile(concord, fileName);
			}
			if (MapProperty.SaveDocFormat && !MapProperty.SavePdfFormat && !MapProperty.SaveJpgFormat)
			{
				data.SaveAs(concord, fileName);
			}
			if (!MapProperty.SaveDocFormat && MapProperty.SavePdfFormat && !MapProperty.SaveJpgFormat)
			{
				data.SaveAsPdf(concord, fileName);
			}
			if (!MapProperty.SaveDocFormat && !MapProperty.SavePdfFormat && MapProperty.SaveJpgFormat)
			{
				data.SaveAsJpeg(concord, fileName);
			}
			if (MapProperty.SaveDocFormat && MapProperty.SavePdfFormat && !MapProperty.SaveJpgFormat)
			{
				data.SaveAsDocAndPdfFile(concord, fileName);
			}
			if (MapProperty.SaveDocFormat && !MapProperty.SavePdfFormat && MapProperty.SaveJpgFormat)
			{
				data.SaveAsMultiFile(concord, fileName);
			}
			if (!MapProperty.SaveDocFormat && MapProperty.SavePdfFormat && MapProperty.SaveJpgFormat)
			{
				data.SaveAsJpgAndPdfFile(concord, fileName);
			}
			data.Destroyed();
			//entry.Destroyed();
		}

		/// <summary>
		/// 初始化所有视图
		/// </summary>
		/// <param name="concord"></param>
		/// <param name="lands"></param>
		/// <param name="reference"></param>
		/// <param name="filePath"></param>
		private void InitalizeAllView(ContractConcord concord, List<VEC_CBDK> lands, string filePath)
		{
			var sFilePath = filePath + @"\Jpeg\";
			if (!System.IO.Directory.Exists(sFilePath))
			{
				System.IO.Directory.CreateDirectory(sFilePath);
			}
			InvokeUtil.Invoke(window, () =>ExportOverView(concord, lands, sFilePath));
		}

		/// <summary>
		/// 获取邻宗地示意图
		/// </summary>
		private void InitalizeLocalView(ContractConcord concord,List<VEC_CBDK> lands, string filePath)
		{
			var sFilePath = filePath + @"\Jpeg\";
			try
			{
				foreach (var land in concord.Lands)//各个空间地块的邻宗地图导出
				{
					var localLand = lands.Find(ld => ld.DKBM == land.DKBM);
					if (localLand != null)
					{
						if (!string.IsNullOrEmpty(land.DKMC))
						{
							localLand.DKMC = land.DKMC;
						}
						localLand.SCMJ = land.HTMJ;
						localLand.SCMJM = land.HTMJM;
						SketchMapUtil.ReplaceDXNBZ(localLand, land);
						InvokeUtil.Invoke(window, () =>InitalizeOwnerView(localLand,lands, sFilePath));
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// 导出主地图（大图）
		/// </summary>
		/// <param name="concord"></param>
		/// <param name="lands"></param>
		/// <param name="reference"></param>
		/// <param name="filePath"></param>
		private void ExportOverView(ContractConcord concord, List<VEC_CBDK> lands, string filePath)
		{
			string fileName = filePath + concord.CBFBM + ".jpg";
			if (File.Exists(fileName))
			{
				return;
				//File.Delete(fileName);
			}
			var plc = _plc;// new PageLayoutControl();
			var pl=plc.ActiveView as IPageLayout;


			try
			{
				string tmplFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\地块示意图\地块鹰眼图.kpd";
				pl.OpenDocument(tmplFile, false);

				#region 连接数据源
				var map = plc.ActiveView.FocusMap as Map;
				map.SetSpatialReference(_spatialReference, false);
				var layer1 = map.Layers.GetLayer(0) as IFeatureLayer;
				var layer2 = map.Layers.GetLayer(1) as IFeatureLayer;
				layer2.Where = null;
				var fc1 = MyMemorySourceUtil.CreateAreaFeatureClass("fc1");
				var fc2 = MyMemorySourceUtil.CreateAreaFeatureClass("fc2");
				layer1.FeatureClass = fc1;
				layer2.FeatureClass = fc2;
				Envelope env = null;
				foreach (var en in lands)
				{
					var ft = fc1.CreateFeature();
					ft.Shape = en.Shape.Geometry;
					if (env == null)
					{
						env = ft.Shape.EnvelopeInternal;
					}
					else
					{
						env.ExpandToInclude(ft.Shape.EnvelopeInternal);
					}
					fc1.Append(ft);
				}
				var fullEnv = new OkEnvelope(env);
				map.FullExtent = fullEnv;
				map.SetExtent(fullEnv, false);

				foreach (var land in concord.Lands)
				{
					VEC_CBDK localLand = lands.Find(ld => ld.DKBM == land.DKBM);
					if (localLand == null)
					{
						continue;
					}
					var ft = fc2.CreateFeature();
					ft.Shape = localLand.Shape.Geometry;
					fc2.Append(ft);
				}

				#endregion


				plc.ActiveView.SaveToImage(fileName, 300, i => { }, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				pl.ClearDocument();
			}
		}
		private void InitalizeOwnerView(VEC_CBDK land, List<VEC_CBDK> lands, string filePath)
		{
			if (land == null)
			{
				return;
			}
			string fileName = filePath + land.DKBM + ".jpg";
			if (File.Exists(fileName))
			{
				return;
				//File.Delete(fileName);
			}
			var pageLayout = _plc.ActiveView as IPageLayout;
			try
			{
				SketchMapUtil.InitalizeOwnerView(pageLayout, _spatialReference, land, lands);
				pageLayout.SaveToImage(fileName, 300, i => { }, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			finally
			{
				pageLayout.ClearDocument();
			}
		}
		*/
		///// <summary>
		///// 添加节点标注
		///// </summary>
		//private List<Coordinate> InitalizeCircleView(VEC_CBDK localLand)
		//{
		//	var points = localLand.Shape.Geometry.Coordinates;
		//	var cds = points.ToList();
		//	cds = InitalizeLabelCircle(cds);
		//	cds = VacuteDotRing(cds);
		//	return cds;
		//}


		///// <summary>
		///// 根据设置，没有界址信息的情况下，抽稀全部的点
		///// </summary>
		///// <param name="points"></param>
		///// <returns></returns>
		//private List<Coordinate> VacuteDotRing(List<Coordinate> points)
		//{
		//	var filterGeos = new List<Coordinate>();
		//	double vacuteDotDistence = 3.0;
		//	filterGeos.Add(points[0]);
		//	for (int i = 1; i < points.Count; i++)
		//	{
		//		if (i != points.Count - 1)
		//		{
		//			var ii1distance = points[i].Distance(points[i - 1]);
		//			if (ii1distance < vacuteDotDistence)
		//			{
		//				points.Remove(points[i]);
		//				i = i - 1;
		//			}
		//			else if (ii1distance > vacuteDotDistence)
		//			{
		//				filterGeos.Add(points[i]);
		//			}
		//		}
		//		else
		//		{
		//			var ii1distance = points[i].Distance(points[0]);
		//			if (ii1distance < vacuteDotDistence)
		//			{
		//				points.Remove(points[i]);
		//			}
		//			else if (ii1distance > vacuteDotDistence)
		//			{
		//				if (filterGeos.Contains(points[i]) == false)
		//				{
		//					filterGeos.Add(points[i]);
		//				}
		//			}
		//		}
		//	}
		//	return filterGeos;
		//}
		///// <summary>
		///// 获取标注点坐标集合
		///// </summary>
		///// <param name="polygon">多段线</param>
		///// <param name="points">点集合</param>
		///// <returns></returns>
		//private List<Coordinate> InitalizeLabelCircle(List<Coordinate> points)
		//{
		//	var markPoints = new List<Coordinate>();
		//	Coordinate basePoint;
		//	for (int i = 0; i < points.Count - 2; i++)
		//	{
		//		basePoint = InitalizeMarkPoint(points[i], points[i + 1], points[i + 2]);//获取标注点
		//		if (basePoint == null)
		//		{
		//			continue;
		//		}
		//		markPoints.Add(basePoint);// CSharpFramework.Spatial.Geometry.CreatePoint(basePoint, Reference.WKID));
		//	}
		//	basePoint = InitalizeMarkPoint(points[points.Count - 2], points[points.Count - 1], points[0]);//获取标注点
		//	if (basePoint != null)
		//	{
		//		markPoints.Add(basePoint);// CSharpFramework.Spatial.Geometry.CreatePoint(basePoint, Reference.WKID));
		//	}
		//	basePoint = InitalizeMarkPoint(points[points.Count - 1], points[0], points[1]);//获取标注点
		//	if (basePoint != null)
		//	{
		//		markPoints.Add(basePoint);// CSharpFramework.Spatial.Geometry.CreatePoint(basePoint, Reference.WKID));
		//	}
		//	return markPoints;
		//}
		///// <summary>
		///// 获取标注点
		///// </summary>
		///// <param name="polygon">多段线</param>
		///// <param name="firstPoint">第一点</param>
		///// <param name="secondPoint">第二点</param>
		///// <param name="thirdPoint">第三点</param>
		///// <param name="dist">间距</param>
		///// <returns></returns>
		//private Coordinate InitalizeMarkPoint(Coordinate firstPoint, Coordinate secondPoint, Coordinate thirdPoint)
		//{
		//	double startAng = InitalizeAngle(secondPoint, firstPoint);
		//	double endAng = InitalizeAngle(secondPoint, thirdPoint);
		//	double angle = Math.Abs(endAng - startAng);
		//	if (angle > 180)
		//	{
		//		angle = angle - 180;
		//	}
		//	if (angle <= 150)
		//	{
		//		return secondPoint;
		//	}
		//	return null;
		//}
		///// <summary>
		///// 计算两点角度
		///// </summary>
		///// <param name="firPoint"></param>
		///// <param name="secPoint"></param>
		///// <returns></returns>
		//private double InitalizeAngle(Coordinate firPoint, Coordinate secPoint)
		//{
		//	double y = secPoint.Y - firPoint.Y;
		//	double x = secPoint.X - firPoint.X;
		//	double angle = Math.Atan2(y, x) * 180 / Math.PI;
		//	if (angle < 0)
		//	{
		//		angle = 360 + angle;
		//	}
		//	return angle;
		//}

		///// <summary>
		///// 初始化地块缓冲图形
		///// </summary>
		///// <returns></returns>
		//private IGeometry InitalizeLandBuffer(VEC_CBDK land, double distance = 4.0)
		//{
		//	if (land.Shape == null)
		//	{
		//		return null;
		//	}
		//	var landBuffer = land.Shape.Geometry;
		//	try
		//	{
		//		if (landBuffer != null && landBuffer.IsValid)
		//		{
		//			landBuffer = landBuffer.Buffer(distance);
		//		}
		//	}
		//	catch (SystemException ex)
		//	{
		//		System.Diagnostics.Debug.WriteLine(ex.Message);
		//	}
		//	return landBuffer;
		//}
	}


	public class TaskSketchMapArgumet// : TaskArgument
	{
		public class CbfItem
		{
			/// <summary>
			/// 承包方编码
			/// </summary>
			public string CBFBM;
			/// <summary>
			/// 承包方名称
			/// </summary>
			public string CbfMc { get; set; }
			public bool IsSelected { get; set; }

			public override string ToString()
			{
				return CbfMc;
			}
		}
		public enum DType
		{
			Null,
			DatFile,
			CbfItem
		}
		internal DType DataType = DType.Null;
		/// <summary>
		/// 文件集合
		/// [dat文件路径或SelectCbfPanel.CbfItem]
		/// </summary>
		public List<object> FileNames { get; private set; } = new List<object>();

		/// <summary>
		/// 输出路径
		/// </summary>
		public string OutputPath { get; set; }

		//[Enabled(false)]
		public SkecthMapProperty MapProperty { get; set; } = new SkecthMapProperty();


		public TaskSketchMapArgumet()
		{
		}
	}
}
