using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common
{
	/// <summary>
	/// yxm 2018-12-19
	/// </summary>
	public class MyMapUtil
	{
		class Item
		{
			public IFeatureClass Item1;
			public int Item2;
		}
		public static void OnAddDataButtonClick(GIS.Map map, TocControl TocControl)
		{
			try
			{
				var dlg = new OpenFileDialog
				{
					//openFileDialog.InitialDirectory="c:";//注意这里写路径时要用c:而不是c:　
					Filter = "Shape文件(*.shp)|*.shp|栅格文件(*.jpg;*.tiff;*.tif,*.img)|*.jpg;*.tiff;*.tif;*.img|rdb影像(*.rdb)|*.rdb",
					RestoreDirectory = true,
					FilterIndex = 1,
					Multiselect = true
				};
				if (dlg.ShowDialog() != true)
				{
					return;
				}

				var selLyr = TocControl.SelectedLayer;
				if (!(selLyr is LayerCollection gl))
				{
					if (selLyr == null)
					{
						gl = map.Layers;
					}
					else
					{
						gl = MapUtil.FindParent(map, selLyr);
					}
				}

				var wfc = ShapeFileFeatureWorkspaceFactory.Instance;
				var lstFeatureClass = new List<Item>();
				int i = 0;
				if (dlg.FilterIndex == 1)
				{
					foreach (string file in dlg.FileNames)
					{
						var fc = wfc.OpenFeatureClass2(file, "rb+");
						lstFeatureClass.Add(new Item
						{
							Item1 = fc,
							Item2 = i++
						});
					}
				}
				else if (dlg.FilterIndex == 2)
				{
					FileRasterLayer rl = null;
					foreach (string fileName in dlg.FileNames)
					{
						var tl = new FileRasterLayer(map);
						tl.Load(fileName);
						gl.AddLayer(tl);
						if (rl == null)
						{
							rl = tl;
						}
					}
					gl.IsExpanded = true;
					TocControl.Refresh();
					if (map.FullExtent == null && rl != null)
					{
						map.FullExtent = rl.FullExtent;
						map.SetExtent(map.FullExtent, false);
					}
					map.Refresh();
					return;
				}
				else if (3 == dlg.FilterIndex)
				{
					var fileName = dlg.FileNames.Length > 0 ? dlg.FileNames[0] : null;
					if (fileName != null)
					{
						var tl = new SqliteRdbTileLayer(map);
						tl.Load(fileName);
						map.Layers.AddLayer(tl);

						TocControl.Refresh();
						if (map.FullExtent == null)
						{
							map.FullExtent = tl.FullExtent;
							map.SetExtent(map.FullExtent, false);
						}
						map.Refresh();
					}
					return;
				}
				lstFeatureClass.Sort((t1, t2) =>
				{
					var a = t1.Item1;
					var b = t2.Item1;
					if (a.ShapeType == b.ShapeType)
					{
						return t1.Item2 < t2.Item2 ? -1 : 1;
					}
					if (a.ShapeType == eGeometryType.eGeometryPolygon)
						return -1;
					if (b.ShapeType == eGeometryType.eGeometryPolygon)
						return 1;
					if (a.ShapeType == eGeometryType.eGeometryPolyline)
						return -1;
					if (b.ShapeType == eGeometryType.eGeometryPolyline)
						return 1;
					return t1.Item2 < t2.Item2 ? -1 : 1;
				});

				ILayer selectedLayer = null;
				foreach (var fc in lstFeatureClass)
				{
					var fl = new FeatureLayer(map)
					{
						FeatureClass = fc.Item1
					};
					gl.AddLayer(fl);
					selectedLayer = fl;
				}
				gl.IsExpanded = true;

				TocControl.Refresh();
				TocControl.SelectedLayer = selectedLayer;

				var fMapRefreshed = false;
				if (map.FullExtent == null && lstFeatureClass.Count > 0)
				{
					map.FullExtent = lstFeatureClass[0].Item1.GetFullExtent();
					map.SetExtent(map.FullExtent, true);
					fMapRefreshed = true;
				}
				if (!fMapRefreshed)
				{
					map.Refresh();
				}
			}
			catch (Exception ex)
			{
				Log.WriteLine(ex.ToString());
				UIHelper.ShowExceptionMessage(ex);
			}
		}
	}
}
