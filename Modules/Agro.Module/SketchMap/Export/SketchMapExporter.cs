using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.GIS;
using Agro.Library.Common;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Agro.Module.SketchMap
{
	public class SketchMapExporter
	{
		private SkecthMapProperty MapProperty;
		private readonly PageLayoutControl _plc;
		public readonly SpatialReference _spatialReference;
		private readonly Window window;

		public bool DeleteJPGFolder = false;
		public SketchMapExporter(Window wnd=null, SpatialReference spatialReference=null, PageLayoutControl plc=null)
		{
			//
			_plc =plc??new PageLayoutControl();
			if (plc==null)
			{
				_plc.Width = 500;
				_plc.Height = 500;
			}
			window = wnd;// Window.GetWindow(plc);
			if (spatialReference == null)
			{
				var srid = MyGlobal.Workspace.GetSRID("DLXX_DK");
				spatialReference = SpatialReferenceFactory.CreateFromEpsgCode(srid);
			}
			_spatialReference = spatialReference;
		}
		public void SetMapProperty(SkecthMapProperty mp)
		{
			MapProperty = mp;
		}
		/// <summary>
		/// 根据承包方导出地块示意图
		/// </summary>
		public void ExportSketchMapByContractor(ContractConcord concord, List<VEC_CBDK> lands,string filePath,string tempPath=null,Action<Exception> onError=null)
		{
			//string filePath = FilePath + @"\" + concord.CBFMC;
			if (!Directory.Exists(filePath))
			{
				Directory.CreateDirectory(filePath);
			}
			if (tempPath == null)
			{
				tempPath =Path.Combine(filePath , "Jpeg/");
			}
			var fDeleteJPGFolder=DeleteJPGFolder;
			if (DeleteJPGFolder && Directory.Exists(tempPath))
			{
				fDeleteJPGFolder = false;
			}

			var docTmplFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\农村土地承包经营权承包地块示意图.dot";
			var fileName =Path.Combine(filePath , $"DKSYT{concord.CBFBM}J");
			InitalizeAllView(concord, lands, tempPath, onError);
			InitalizeLocalView(concord, lands, tempPath,onError);
			var data = new SkecthMapExport
			{
				MapProperty = MapProperty,
				Contractor = concord,
				DKS = lands,
				FilePath = tempPath// filePath + @"\Jpeg\"
			};
			if (!Directory.Exists(data.FilePath))
			{
				Directory.CreateDirectory(data.FilePath);
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
			data.Destroyed(fDeleteJPGFolder);
			//entry.Destroyed();
		}

		private void Invoke(Action action)
		{
			if (window != null)
			{
				InvokeUtil.Invoke(window, action);
			}
			else
			{
				action();
			}
		}

		/// <summary>
		/// 初始化所有视图
		/// </summary>
		/// <param name="concord"></param>
		/// <param name="lands"></param>
		/// <param name="reference"></param>
		/// <param name="filePath"></param>
		private void InitalizeAllView(ContractConcord concord, List<VEC_CBDK> lands, string filePath,Action<Exception> onFinish)
		{
			var sFilePath = filePath;// filePath + @"\Jpeg\";
			if (!Directory.Exists(sFilePath))
			{
				Directory.CreateDirectory(sFilePath);
			}
			Invoke(() =>
			{
				var err=ExportOverView(concord, lands, sFilePath);
				onFinish?.Invoke(err);
			});
		}

		/// <summary>
		/// 获取邻宗地示意图
		/// </summary>
		private void InitalizeLocalView(ContractConcord concord, List<VEC_CBDK> lands, string filePath,Action<Exception> onError)
		{
			var sFilePath = filePath;// + @"\Jpeg\";
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
						Invoke(()=>
						{
							var err = InitalizeOwnerView(localLand, lands, sFilePath);
							onError?.Invoke(err);
						});
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
		private Exception ExportOverView(ContractConcord concord, List<VEC_CBDK> lands, string filePath)
		{
			Exception retEx = null;
			string fileName = filePath + concord.CBFBM + ".jpg";
			if (File.Exists(fileName))
			{
				return retEx;
				//File.Delete(fileName);
			}
			var plc = _plc;// new PageLayoutControl();
			var pl = plc.ActiveView as IPageLayout;
			try
			{
				string tmplFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Template\地块示意图\地块鹰眼图.kpd";
				pl.OpenDocument(tmplFile, false);

				#region 连接数据源
				var map = plc.ActiveView.FocusMap as GIS.Map;
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
					var localLand = lands.Find(ld => ld.DKBM == land.DKBM);
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
				retEx = ex;
			}
			finally
			{
				pl.ClearDocument();
			}
			return retEx;
		}
		private Exception InitalizeOwnerView(VEC_CBDK land, List<VEC_CBDK> lands, string filePath)
		{
			Exception retEx = null;
			if (land == null)
			{
				return retEx;
			}
			string fileName = filePath + land.DKBM + ".jpg";
			if (File.Exists(fileName))
			{
				return retEx;
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
				retEx = ex;
			}
			finally
			{
				pageLayout.ClearDocument();
			}
			return retEx;
		}
	}
}
