using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.GIS;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;


namespace Agro.Module.Map
{
	public partial class HistoryMapPage : UserControl, IRequestClosing, IDisposable, IBusyWork
	{
		class Item
		{
			public IFeatureClass Item1;
			public int Item2;
		}
		class LayerPanelContextMenu
		{
			private readonly HistoryMapPage _p;
			private readonly ContextMenu _layerMenu;
			public LayerPanelContextMenu(HistoryMapPage p)
			{
				_p = p;
				_layerMenu = p.sidePage.ContextMenu;
				initContextMenu();
				p.miExportShapeFile.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					ExportToShapefileDialog.ShowDialog(Window.GetWindow(p), fl);
					//var dlg=new KuiDialog(p,"导出为ShapeFile文件")
				};
				p.miExportLayerFile.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					fl.SaveLayerAs(null);
				};
				p.miPropertyTable.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					var fl = mi.Tag as FeatureLayer;
					showPropertyTable(fl);
				};
				p.miRemoveLayer.Click += (s, e) =>
				{
					var mi = s as MenuItem;
					if (mi.Tag is ILayer fl)
					{
						var m = p.Map;
						m.Layers.RemoveLayer(fl);
						p.Toc.Refresh();
						m.Refresh();
					}
				};
				p.miLayerProperty.Click += (s, e) =>
				{
					if ((s as MenuItem).Tag is ILayer lyr)
					{
						var dlg = new LayerPropertyDialog();
						dlg.ShowProperty(lyr, new LayerPropertyDialog.ShowPropertyPrms()
						{
							OnApplied = () => { p.Toc.Refresh(); p.Map.Refresh(); }
						});
					}
				};
				p.miClearVisibleScale.Click += (s, e) =>
				{
					if ((s as MenuItem).Tag is ILayer lyr)
					{
						lyr.MaxVisibleScale = null;
						lyr.MinVisibleScale = null;
						p.Map.RefreshLayer(lyr);
					}
				};
				p.miSetMaxVisibleScale.Click += (s, e) =>
				{
					var lyr = (s as MenuItem).Tag as ILayer;
					lyr.MaxVisibleScale = p.Map.MapScale;//.Scale;
					p.Map.RefreshLayer(lyr);
				};
				p.miSetMinVisibleScale.Click += (s, e) =>
				{
					var lyr = (s as MenuItem).Tag as ILayer;
					lyr.MinVisibleScale = p.Map.MapScale;//.Scale;
					p.Map.RefreshLayer(lyr);
				};
			}
			private void initContextMenu()
			{
				var p = _p;
				p.sidePage.ContextMenu = null;
				p.layersControl.TocControl.MouseRightButtonDown += (s, e) =>
				{
					var hi = p.layersControl.TocControl.HitTest(e.GetPosition(p.layersControl.TocControl));
					if (hi == null)
					{
						_layerMenu.PlacementTarget = null;// p.layersControl.TocControl;
						_layerMenu.IsOpen = false;
						return;
					}
					var fFeatureLayer = hi.Layer is FeatureLayer;
					p.miPropertyTable.IsEnabled = fFeatureLayer == true;
					p.miExportShapeFile.IsEnabled = fFeatureLayer == true;
					p.miExportLayerFile.IsEnabled = fFeatureLayer == true;

					p.miClearVisibleScale.IsEnabled = hi.Layer.MinVisibleScale != null || hi.Layer.MaxVisibleScale != null;
					//p.miSetMaxVisibleScale.is

					//p.miLayerProperty.IsEnabled = fFeatureLayer == true;
					foreach (var it in _layerMenu.Items)
					{
						AttachTag(it as MenuItem, hi.Layer);
					}
					_layerMenu.PlacementTarget = p.layersControl.TocControl;
					_layerMenu.IsOpen = true;
				};
			}
			private void AttachTag(MenuItem mi, object tag)
			{
				if (mi != null)
				{
					mi.Tag = tag;
					if (mi.HasItems)
					{
						foreach (var mi1 in mi.Items)
						{
							AttachTag(mi1 as MenuItem, tag);
						}
					}
				}
			}
			private void showPropertyTable(FeatureLayer fl)
			{
				if (fl == null)
				{
					return;
				}
				var tc = _p._propertyTables;
				tc.ShowPropertyTable(fl);
			}
		}

		/// <summary>
		/// 图层属性表管理类
		/// </summary>
		class PropertyTables
		{
			private readonly HistoryMapPage _p;
			private readonly Panel _container;
			private readonly DecoratorWrap _lpc;
			private readonly Dictionary<ILayer, Action> _dicLegendChangedEvents = new Dictionary<ILayer, Action>();
			private ListBox TabControl
			{
				get
				{
					return _p.tcTables;
				}
			}
			public PropertyTables(HistoryMapPage p)
			{
				_p = p;
				_container = p.propertyTables;
				_lpc = new DecoratorWrap(p.bdrFeatureLayer);
				p.Map.OnLayerChanged += (t, a) =>
				{
					if (t != ELayerCollectionChangeType.Remove)
					{
						return;
					}
					var layer = a as ILayer;
					var ti = findTabItem(layer);
					removeItem(ti);
				};
				TabControl.SelectionChanged += (s, e) =>
				{
                    if (TabControl.SelectedItem is not ImageTextContent ti)
                    {
                        _lpc.SetChild(null);
                    }
                    else
                    {
                        _lpc.SetChild(ti.Tag as UIElement);
                    }
                };
				_p.sidePage.OnBottomPanelChildChanged += (oldChild, newChild) =>
				{
					if (oldChild == _container && _p.sidePage.BottomPanel == null)
					{
						foreach (var ti in TabControl.Items)
						{
							var tv = (ti as ImageTextContent).Tag as FeatureLayerTableView;
							DetachEvents(tv.TableView.FeatureLayer);
						}
						TabControl.Items.Clear();
					}
				};

				UpdateUI();
			}
			public void ShowPropertyTable(FeatureLayer fl)
			{
				if (fl == null || fl.FeatureClass == null)
				{
					return;
				}
				var ti = findTabItem(fl);
				if (ti == null)
				{
					ti = new ImageTextContent
					{
						Margin = new Thickness(4, 2, 4, 2),
						ImagePosition = Dock.Left,
						HeaderMargin = new Thickness(4, 0, 0, 0),
						Header = fl.Name
					};
					var tv = new FeatureLayerTableView()
					{
						VertextView = _p._featureVertextView
					};
					tv.BtnClose.Click += (s, e) =>
					{
						removeItem(ti);
						// tabControl.Items.Remove(ti);
					};

					#region set image
					var legendInfo = fl.Renderer.LegendInfo;

					#region attach events
					Action onLegendInfoChanged = () =>
					{
						SetLayerImage(fl, ti);
					};
					_dicLegendChangedEvents[fl] = onLegendInfoChanged;
					legendInfo.OnLegendInfoChanged += onLegendInfoChanged;
					#endregion

					SetLayerImage(fl, ti);
					#endregion

					tv.Init(fl, _p.mapControl);
					_lpc.SetChild(tv);
					ti.Tag = tv;
					TabControl.Items.Add(ti);
				}
				TabControl.SelectedItem = ti;
				//ti.IsSelected = true;
				UpdateUI();
			}
			private ImageTextContent findTabItem(ILayer fl)
			{
				//var tabControl = _p.tcTables;
				foreach (var i in TabControl.Items)
				{
					var ti = i as ImageTextContent;//).Tag as FeatureLayerTableView;
					var tv = ti.Tag as FeatureLayerTableView;
					if (tv.TableView.FeatureLayer == fl)
						return ti;
				}
				return null;
			}
			private void removeItem(ImageTextContent ti)
			{
				if (ti != null)
				{
					var i = TabControl.Items.IndexOf(ti);
					if (i >= 0)
					{
						bool fSelected = TabControl.SelectedItem == ti;
						TabControl.Items.RemoveAt(i);
						if (fSelected)
						{
							if (i >= TabControl.Items.Count)
							{
								--i;
							}
							if (i >= 0)
							{
								TabControl.SelectedIndex = i;
							}
						}

						#region detach events
						var layer = (ti.Tag as FeatureLayerTableView).TableView.FeatureLayer;
						//if (tv.TableView.FeatureLayer == fl)

						DetachEvents(layer);
						#endregion

					}
					UpdateUI();
				}
			}
			private int TableCount
			{
				get
				{
					return TabControl.Items.Count;
				}
			}
			private void UpdateUI()
			{
				if (_p.sidePage.BottomPanel != _container
					&& TableCount > 0)
				{
					_p.sidePage.BottomPanel = _container;// ._bottomPnlContainer.SetChild(_container);
				}
				_p.bdrBottomMask.Visibility = TableCount == 0 ? Visibility.Visible : Visibility.Collapsed;
			}
			private void SetLayerImage(IFeatureLayer fl, ImageTextContent ti)
			{
				#region set image
				var legendInfo = fl.Renderer.LegendInfo;
				var lg = legendInfo.Get(0);
				if (/*legendInfo.Count == 1 && */lg != null
					&& lg.ClassCount > 0)
				{
					var lc = lg.GetClass(0);
					ti.Image = TocUtil.ToImageSource(lc.Symbol, 24, 24);
				}
				else
				{
					//todo...
				}
				#endregion
			}
			private void DetachEvents(ILayer layer)
			{
				if (_dicLegendChangedEvents.TryGetValue(layer, out Action onLegendInfoChanged))
				{
					var legendInfo = layer.Renderer.LegendInfo;
					legendInfo.OnLegendInfoChanged -= onLegendInfoChanged;
					_dicLegendChangedEvents.Remove(layer);
				}
			}
		}

		class DefaultLayers
		{
			/// <summary>
			/// 创建本地图层表映射数据
			/// </summary>
			public static List<ILayer> CreateLayers(IFeatureWorkspace wfc, GIS.Map map,out IFeatureLayer dkLayer)
			{
				List<ILayer> layers = new List<ILayer>();
				//ILayer layer = InitalizeLayer("栅格数据", MapLayerName.RasterGroup_Layer, "", eGeometryType.Unknown, false, false, false, false);//栅格数据
				//layers.Add(layer);
				//layer = InitalizeLayer("其他栅格数据", MapLayerName.OtherRaster_Layer, "", eGeometryType.Unknown, false, false, false, false, 0, 0, MapLayerName.RasterGroup_Layer);//其他栅格数据
				//layers.Add(layer);
				//layer = InitalizeLayer("数字栅格地图", MapLayerName.DigitalRaster_Layer, "", eGeometryType.Unknown, false, false, false, false, 0, 0, MapLayerName.RasterGroup_Layer);//数字栅格地图
				//layers.Add(layer);
				//layer = InitalizeLayer("数字正射影像图", MapLayerName.OrthoRaster_Layer, "", eGeometryType.Unknown, false, false, false, false, 0, 0, MapLayerName.RasterGroup_Layer);//数字正射影像图
				//layers.Add(layer);
				//#region 定位基础
				//{
				//	var groupLayer = new GroupLayer(map, "定位基础");
				//	var kzdLayer = CreatePointFeatureLayer(wfc, map, "DLXX_KZD", "控制点"
				//		, "#FFCCE1A0", "#FF728944", 6);
				//	groupLayer.AddLayer(kzdLayer);
				//	layers.Add(groupLayer);
				//}
				//#endregion
				if (true)
				{
					#region 管辖区域
					{
						//var groupLayer = new GroupLayer(map, "管辖区域");
						//layers.Add(groupLayer);

						var layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "县级区域",
							"#FFEDE8E6", "#FFC4BFBD", 1
							, "JB = 4", "[MC]", 100000, 1000000);
						SetupLabeler(layer, 20, "#FF000000");
						layer.Selectable = false;
						layers.Add(layer);


						layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "乡级区域",
							"#FFEFEBDB", "#FFC4BFBD", 1
							, "JB = 3", "[MC]");//, 30000, 150000);
						layer.Selectable = false;
						layers.Add(layer);


						layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "村级区域",
							"#00EAE0BD", "#FFFF0000", 1
							, "JB = 2", "[MC]", 100, 60000);
						layer.Selectable = false;
						layers.Add(layer);

						layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "组级区域",
							"#00EFEBDB", "#FFC4BFBD", 1
							, "JB = 1", "[MC]", 30, 20000);
						layer.Selectable = false;
						layers.Add(layer);
					}
					#endregion
				}
				//#region 区域界线
				//{
				//	var groupLayer = new GroupLayer(map, "区域界线");
				//	layers.Add(groupLayer);
				//	var layer = CreateLineFeatureLayer(wfc, map, "DLXX_QYJX", "区域界线"
				//		, "#FF9E9470", 1, null, null, 3000, 100000);
				//	groupLayer.AddLayer(layer);
				//}
				//#endregion



				//#region 基本农田
				//{
				//	var groupLayer = new GroupLayer(map, "基本农田");
				//	layers.Add(groupLayer);

				//	var layer = CreateFillFeatureLayer(wfc, map, "DLXX_JBNTBHQ", "基本农田保护区",
				//		"#FFEDE8E6", "#FFC4BFBD", 1
				//		, "", "", null, 10000);//, 30000, 150000);
				//	groupLayer.AddLayer(layer);
				//}
				//#endregion

				//#region 其他地物
				//{
				//	var groupLayer = new GroupLayer(map, "其他地物");
				//	layers.Add(groupLayer);
				//	var layer = CreateFillFeatureLayer(wfc, map, "DLXX_MZDW", "面状地物",
				//		"#FFEFEBDB", "#FFC4BFBD", 1
				//		, "", "DWMC", null, 8000);
				//	groupLayer.AddLayer(layer);

				//	layer = CreateLineFeatureLayer(wfc, map, "DLXX_XZDW", "线状地物"
				//		, "#FF9E9470", 1, null, "[DWMC]", null, 8000);
				//	groupLayer.AddLayer(layer);

				//	layer = CreatePointFeatureLayer(wfc, map, "DLXX_DZDW", "点状地物"
				//		, "#FFCCE1A0", "#FF728944", 6, null, "[DWMC]");
				//	layer.MaxVisibleScale = 8000;
				//	groupLayer.AddLayer(layer);
				//}
				//#endregion

				#region 地块类别
				{
					//var groupLayer = new GroupLayer(map, "地块类别");
					//groupLayer.IsExpanded = true;
					//layers.Add(groupLayer);

					var layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "地块",
						"#FFCCE1A0", "#FF728944", 1
						, "", /*"StrContains([CBFMC],'/')?\"共有地块\";[CBFMC]"*/null, null, 50000);//, 30000, 150000);
					layers.Add(layer);
					dkLayer = layer;
					//layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "自留地",
					//	"#D3FFBEFF", "#FF728944", 1
					//	, "DKLB = '21'", "[CBFMC]", null, 8000);//, 30000, 150000);
					//groupLayer.AddLayer(layer);

					//layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "机动地",
					//	"#E39E00FF", "#FF728944", 1
					//	, "DKLB = '22'", "[CBFMC]", null, 8000);//, 30000, 150000);
					//groupLayer.AddLayer(layer);

					//layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "开荒地",
					//	"#F0B0CFFF", "#FF728944", 1
					//	, "DKLB = '23'", "[CBFMC]", null, 8000);//, 30000, 150000);
					//groupLayer.AddLayer(layer);

					//layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "其他集体土地",
					//				  "#BED2FFFF", "#FF728944", 1
					//				  , "DKLB = '99'", "[CBFMC]", null, 8000);//, 30000, 150000);
					//groupLayer.AddLayer(layer);
				}
				#endregion

				//#region 界址数据
				//{
				//	var groupLayer = new GroupLayer(map, "界址数据");
				//	layers.Add(groupLayer);
				//	var layer = CreateLineFeatureLayer(wfc, map, "DLXX_DK_JZX", "界址线"
				//		, "#FF9E9470", 1, null, null, null, 8000);
				//	groupLayer.AddLayer(layer);

				//	layer = CreatePointFeatureLayer(wfc, map, "DLXX_DK_JZD", "界址点"
				//		, "#FFCCE1A0", "#FF728944", 6);
				//	layer.MaxVisibleScale = 8000;
				//	groupLayer.AddLayer(layer);
				//}
				//#endregion
				return layers;
			}

			public static GroupLayer CreateOverviewLayer(IFeatureWorkspace wfc, MapControl mc)
			{
				var map = mc.FocusMap;
				var groupLayer = new GroupLayer(map, "管辖区域");
				//layers.Add(groupLayer);

				var layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "县级区域",
					"#FFEDE8E6", "#FFC4BFBD", 1
					, "JB = 4");//, "[MC]", 100000, 1000000);
								//SetupLabeler(layer, 20, "#FF000000");
				groupLayer.AddLayer(layer);


				layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "乡级区域",
					"#FFEFEBDB", "#FFC4BFBD", 1
					, "JB = 3");//, "[MC]", 30000, 150000);
				groupLayer.AddLayer(layer);
				//#region 行政地域
				//var groupLayer = new GroupLayer(map.FocusMap, "行政地域");
				////layers.Add(groupLayer);
				//var tbName = EntityBase.TableName<XZQH_XZDY>();
				//var layer = CreateFillFeatureLayer(wfc, map, tbName, "县级区域",
				//	"#FFEDE8E6", "#FFFF0000", 1
				//	, "JB = 4");//, "[MC]");//, 30000, 1000000);
				//layer.Selectable = false;
				////SetupLabeler(layer, 20, "#FF000000");
				//groupLayer.AddLayer(layer);


				//layer = CreateFillFeatureLayer(wfc, map, tbName, "乡级区域",
				//	"#FFFFFFFF", "#FFC4BFBD", 1
				//	, "JB = 3");//, "[MC]");//, 10000, 300000);//, 30000, 150000);
				//				//SetupLabeler(layer, 15, "#FF000000");
				//layer.Selectable = false;
				//groupLayer.AddLayer(layer);


				////layer = CreateFillFeatureLayer(wfc, map, tbName, "村级区域",
				////	"#00EAE0BD", "#FFFF5500", 1
				////	, "JB = 2", "[MC]", 1000, 100000);//, 30000, 150000);
				////SetupLabeler(layer, 12, "#FF000000");
				////layer.Selectable = false;
				////groupLayer.AddLayer(layer);
				//#endregion
				return groupLayer;
			}

			private static IFeatureLayer CreateLineFeatureLayer(IFeatureWorkspace wfc, GIS.Map map
					  , string tableName, string layerName, string lineColor, double lineWidth = 1, string where = null, string sLabelExpr = null
					  , double? MinVisibleScale = null, double? MaxVisibleScale = null
					  )
			{
				var symbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth) as ILineSymbol;

				var fl = new FeatureLayer(map);
				try
				{
					fl.FeatureClass = wfc.OpenFeatureClass(tableName);
				}
				catch (Exception ex)
				{
					fl.DatasourceException = ex;
				}
				fl.Name = layerName;
				fl.Where = where;

				fl.MinVisibleScale = MinVisibleScale;
				fl.MaxVisibleScale = MaxVisibleScale;

				var render = new SimpleFeatureRenderer();
				render.SetSymbol(symbol);
				fl.FeatureRenderer = render;

				if (sLabelExpr != null)
				{
					var labeler = new ASTExpressionLabeler();
					//string sLabelExpr = @"([BSM]\[DKMC])&[__FID__]";
					//string sLabelExpr = @"[__FID__]";
					//string sLabelExpr = @"[DKMC]";
					labeler.SetLabelExpression(sLabelExpr);
					labeler.EnableLabel = true;
					fl.FeatureLabeler = labeler;
				}
				return fl;
			}

			internal static IFeatureLayer CreateFillFeatureLayer(IFeatureWorkspace wfc, GIS.Map map
				, string tableName, string layerName, string fillColor, string lineColor, double lineWidth = 1, string where = null, string sLabelExpr = null
				, double? MinVisibleScale = null, double? MaxVisibleScale = null
				)
			{
				var lineSymbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth) as ILineSymbol;
				var symbol = SymbolUtil.CreateSimpleFillSymbol(ColorUtil.ConvertFromString(fillColor), lineSymbol);

				var fl = new FeatureLayer(map)
				{
					FeatureClass = wfc.OpenFeatureClass(tableName),
					Name = layerName,
					Where = where,

					MinVisibleScale = MinVisibleScale,
					MaxVisibleScale = MaxVisibleScale,
					//Selectable=false
				};

				//fl.OrderByClause = "JB desc";
				var render = new SimpleFeatureRenderer();
				//var sfs = new SimpleFillSymbol();
				//var pen = new PenProperty(System.Drawing.Color.Blue, 4);
				//pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
				////pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;
				//sfs.Outline = new SimpleLineSymbol(pen);
				//sfs.Background = null;
				render.SetSymbol(symbol);
				fl.FeatureRenderer = render;

				if (sLabelExpr != null)
				{
					var labeler = new ASTExpressionLabeler();
					//string sLabelExpr = @"([BSM]\[DKMC])&[__FID__]";
					//string sLabelExpr = @"[__FID__]";
					//string sLabelExpr = @"[DKMC]";
					labeler.SetLabelExpression(sLabelExpr);
					labeler.EnableLabel = true;
					fl.FeatureLabeler = labeler;
				}
				return fl;
			}
			private static void SetupLabeler(IFeatureLayer fl, double fontSize, string fontColor)
			{
				var lbl = fl.FeatureLabeler as ASTExpressionLabeler;
				var ts = lbl.TextSymbol;// new TextSymbol(fontSize);
										//lbl.TextSymbol =ts;
				ts.Font.FontSize = fontSize;
				ts.Color = ColorUtil.ConvertFromString(fontColor);
			}
		}

		private TocControl Toc
		{
			get { return layersControl.TocControl; }
		}
		private readonly PropertyTables _propertyTables;


		private readonly FeatureVertexViewWrap _featureVertextView;

		public MapControl MapControl
		{
			get
			{
				return mapControl;
			}
		}
		internal GIS.Map Map
		{
			get { return mapControl.FocusMap; }
		}

		private HistoryListPanel _historyListPnl;
		private IFeatureLayer _dkLayer;
		//private readonly Persist _persist;
		public HistoryMapPage()//Persist persist)
		{
			//_persist = null;// persist;
			InitializeComponent();

			//var map = this.MapControl.FocusMap;

			new LayerPanelContextMenu(this);
			//_leftPnlContainer = new MyLeftPanelContainer(bdrLeftPnlContainer, grid1);
			//_rightPnlContainer = new MyRightPanelContainer(this);
			//_bottomPnlContainer = new MyBottomPanelContainer(this);
			_propertyTables = new PropertyTables(this);
			_featureVertextView = new FeatureVertexViewWrap(this.sidePage, this.MapControl);
			this.btnFeatureVertex.OnFeatureSelected += it => { _featureVertextView.Show(it.FeatureLayer, it.Feature); };

			//new ToggleViewButton(this, btnLayer,sidePage.LeftPanel);// _leftPnlContainer.Child);
			//new ToggleViewButton(this, btnTopoCheck, new TopoCheckSelectPanel(map));
			new ToggleSideView(this.sidePage, btnLayer, null, (s, ov, nv) => {
				if (nv)
				{
					var sl = layersControl.TocControl.SelectedLayer;
					layersControl.TocControl.SelectedLayer = sl;
				}
			}).Panel = sidePage.LeftPanel;
			layersControl.OnAddDataButtonClick += MyMapUtil.OnAddDataButtonClick;

			//new ToggleViewButton(this, btnZone, zoneTree);
			new ToggleSideView(sidePage, btnZone, () => {
				var zoneTree = new ZoneTree();
				zoneTree.OnZoneSelected += zone =>
				{
					try
					{
						var g = ZoneUtil.QueryShape(zone);
						if (g != null && !g.IsEmpty && g is IPolygon)
						{
							Map.SetExtent(new OkEnvelope(g));// as IPolygon);
						}
					}
					catch { }
				};
				return zoneTree;
			});

			//var lsp = new LayerSelectionPanel(Map);
			//new ToggleViewButton(this, btnLayerSelection, lsp,(ov,nv)=> {if(nv) lsp.Refresh(); });
			new ToggleSideView(this.sidePage, btnLayerSelection, () => new LayerSelectionPanel(Map), (s, ov, nv) => { if (nv) (s.Panel as LayerSelectionPanel).Refresh(); });
			new ToggleSideView(this.sidePage, btnSearch, () => new MapSearchPanel(Map));
			new ToggleSideView(this.sidePage, btnDataQuery, () => new DataQueryControlPanel(mapControl));
			new ToggleSideView(this.sidePage, btnMapSheet, () => new MapsheetNavPanel(mapControl));
			new ToggleSideView(this.sidePage, btnHistory, () =>
			{
				_historyListPnl=new HistoryListPanel(mapControl, tsc);
				return _historyListPnl;
			});
			btnHistory.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			//var efp = new EditFeaturePanel(map);
			//new ToggleViewButton(this, btnEdit,efp,(ov,nv)=> { efp.OnVisible(nv); } );
			DependencyObjectUtilEx.FindCommandButton(toolbar, cb =>BindMapBuddy(cb));


			#region yxm 2018-3-8
			BindMapBuddy(btnClear);
			mapControl.TempElements.OnAddElement += me => { btnClear.Visibility = Visibility.Visible; };
			mapControl.TempElements.OnClear += () => btnClear.Visibility = Visibility.Collapsed;
			#endregion

			mapControl.CurrentTool = MapControl.CreatePanTool();// new MapPanTool();

			// toc.SetMap(map);
			Toc.MapHost = mapControl;

			msrSS.Map = Map;
			coordSS.Map = Map;
			msSS.Map = Map;

			btnCloseBottom.Click += (s, e) => sidePage.BottomPanel = null;
			sidePage.HidePanel(AnchorSide.Bottom);

			//btnHawkeye.Click += (s, e) => { this.hawkeyeMap.Visibility = this.hawkeyeMap.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; };

			//btnAmplitude.Click += (s, e) => {

			//};

			#region yxm 2018-11-26 初始化鹰眼
			bdrOverview.Visibility = Visibility.Collapsed;
			btnOverview.Click += (s, e) =>
			{
				bdrOverview.Visibility = bdrOverview.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
				btnOverview.IsChecked = bdrOverview.Visibility == Visibility.Visible;
				if (btnOverview.IsChecked == true && overview.FocusMap.Layers.LayerCount == 0)
				{
					overview.Init(mapControl);
					var ovLayer = DefaultLayers.CreateOverviewLayer(MyGlobal.Workspace, overview);
					overview.FocusMap.Layers.AddLayer(ovLayer);
					overview.SetExtent(ovLayer.FullExtent);
				}
			};
			#endregion

			MyGlobal.OnDatasourceChanged += () =>
			{
				LoadData();
				_historyListPnl?.Refresh();
			};

			tsc.OnValueChanged += td =>
			{
				var sd = td.Year + "-" + td.Month + "-" + td.Day;
				var wh = "convert(date,CJSJ) <='" + sd + "' and (convert(date,MSSJ)>'" + sd + "' or MSSJ is null)";
				_dkLayer.Where = wh;
				_delayRefreshLayer.DelayDo(() =>
				{
					Map.Invoke(() => Map.RefreshLayer(_dkLayer));
				});
			};
			
		}

		public void LoadData()
		{
			//try
			//{
			var pWS = MyGlobal.Workspace;// Factory.OpenWorkspace("DATA SOURCE =ORCL33;USER ID=REGCERT;PASSWORD = 123456");
			Invoke(() =>
			{
				var m = this.Map;
				m.Layers.ClearLayers();

				var layers = DefaultLayers.CreateLayers(pWS, Map,out _dkLayer);
				foreach (var layer in layers)
				{
					this.Map.Layers.AddLayer(layer);
				}
				//this.map.SaveDocumentAs("d:/temp/库管地图.kmd");
				var fullEnv = ZoneUtil.GetFullExtent();
				if (fullEnv != null)
				{
					m.FullExtent = fullEnv;
					m.SetExtent(m.FullExtent);
				}
				Toc.Refresh();

				InitTimeSlider();
			});


			//Invoke(() =>
			//{
			//    var layers = DefaultLayers.CreateLayers(pWS, this.hawkeyeMap.oveloyMap.FocusMap);
			//    foreach (var layer in layers)
			//    {
			//        this.hawkeyeMap.oveloyMap.FocusMap.Layers.AddLayer(layer);
			//    }
			//    if (Map.FullExtent != null)
			//    {
			//        this.hawkeyeMap.oveloyMap.FocusMap.FullExtent = Map.FullExtent;
			//        this.hawkeyeMap.oveloyMap.FocusMap.SetExtent(Map.FullExtent);
			//    }
			//});
			//Map.OnTransformChanged += (e) =>
			//{
			//    if (e != null)
			//    {
			//        if (e.ScaleChanged || e.MapExtentChanged)
			//        {
			//            this.hawkeyeMap.SetPolygon(GeometryUtil.MakePolygon(Map.Extent));
			//        }
			//    }
			//};
			//this.hawkeyeMap.OnMovePolygon += (e) =>
			//{
			//    this.Map.SetExtent(new OkEnvelope( e));// as IPolygon);
			//};

		}
		internal void Invoke(Action action)
		{
			this.Dispatcher.Invoke(new Action(() =>
			{
				action();
			}));
		}
		private void BindMapBuddy(IMapBuddy buddyControl)
		{
			buddyControl.MapHost = mapControl;
			if (buddyControl is IDisposable)
			{
				Map.OnDispose += () => (buddyControl as IDisposable).Dispose();
			}
		}
		//private void QueryCoords(NetTopologySuite.Geometries.Polygon g, Action<GeoAPI.Geometries.Coordinate[]> callback)
		//{
		//    callback(g.Shell.Coordinates);
		//    foreach (var h in g.Holes)
		//    {
		//        callback(h.Coordinates);
		//    }
		//}

		//private void InitMap()
		//{
		//    var layers = map.Layers;
		//    layers.ClearLayers();
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = new SqlServerFeatureClass("DJ_CBJYQ_XZDY");
		//        fl.FeatureClass.OIDFieldName = "OBJECTID";
		//        fl.Where = "JB<=3";
		//        fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        render.SetSymbol(new SimpleFillSymbol());
		//        fl.FeatureRenderer = render;

		//        layers.AddLayer(fl);
		//    }
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = new SqlServerFeatureClass("DJ_CBJYQ_CBD");
		//        fl.FeatureClass.OIDFieldName = "OBJECTID";
		//        //fl.Alpha = 100;
		//        //fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        render.SetSymbol(new SimpleFillSymbol()
		//        {
		//            FillColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.YellowGreen),
		//        });
		//        fl.FeatureRenderer = render;

		//        layers.AddLayer(fl);
		//    }

		//    // map.Extent = new GeoAPI.Geometries.Envelope(609000, 645000, 4134000, 4179000);
		//    //map.Extent = new GeoAPI.Geometries.Envelope(624000, 627000, 4155000, 4158000);
		//    //_featureClass.TableName = "DJ_CBJYQ_CBD";

		//}
		//private void InitMap2()
		//{
		//    var wfc = ShapeFileWorkspaceFactory.Instance;
		//    var layers = map.Layers;
		//    layers.ClearLayers();
		//    var groupLayer = new GroupLayer(map, "组合图层");
		//    layers.AddLayer(groupLayer);
		//    layers = groupLayer;
		//    string path = @"C:\myprojects\鱼鳞图质检工具测试数据\513229马尔康县/矢量数据";
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = wfc.OpenFeatureClass(path, "DK5132292016.shp");// new ShapeFileFeatureClass();
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\temp\DK511425.shp");
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\temp\DK450125.shp");
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\myprojects\问题\地块数据_79624\DK3708832016.shp");
		//        // (fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\myprojects\鱼鳞图质检工具测试数据\513229马尔康县\矢量数据\DK5132292016.shp");

		//        //fl.Where = "JB<=3";
		//        //fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        var sfs = new SimpleFillSymbol();
		//        var pen = new PenProperty(System.Drawing.Color.Blue, 4);
		//        pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
		//        //pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;
		//        sfs.Outline = new SimpleLineSymbol(pen);
		//        //sfs.Background = null;
		//        render.SetSymbol(sfs);
		//        fl.FeatureRenderer = render;

		//        if (true)
		//        {
		//            var labeler = new ASTExpressionLabeler();
		//            string sLabelExpr = @"([BSM]\[DKMC])&[__FID__]";
		//            //string sLabelExpr = @"[__FID__]";
		//            //string sLabelExpr = @"[DKMC]";
		//            labeler.SetLabelExpression(sLabelExpr);
		//            labeler.EnableLabel = true;
		//            fl.FeatureLabeler = labeler;
		//        }

		//        layers.AddLayer(fl);
		//        //return;
		//    }
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = wfc.OpenFeatureClass(path, "JZX5132292016.shp");// new ShapeFileFeatureClass();
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\temp\DK511425.shp");
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\temp\JZX450125.shp");
		//        // (fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\myprojects\鱼鳞图质检工具测试数据\513229马尔康县\矢量数据\JZX5132292016.shp");

		//        //fl.Where = "JB<=3";
		//        //fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        //var lineSymbol = new SimpleLineSymbol();
		//        //lineSymbol.Border = new System.Drawing.Pen(System.Drawing.Color.Red, 1);
		//        var pen = new PenProperty(System.Drawing.Color.Red, 2);
		//        //pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Right;
		//        pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

		//        render.SetSymbol(new SimpleLineSymbol(pen));
		//        fl.FeatureRenderer = render;

		//        layers.AddLayer(fl);
		//    }
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = fl.FeatureClass = wfc.OpenFeatureClass(path, "JZD5132292016.shp");//  new ShapeFileFeatureClass();
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\temp\JZD511425.shp");
		//        // (fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\temp\JZD450125.shp");
		//        //(fl.FeatureClass as ShapeFileFeatureClass).Open(@"C:\myprojects\鱼鳞图质检工具测试数据\513229马尔康县\矢量数据\JZD5132292016.shp");

		//        if (false)
		//        {
		//            var labeler = new ASTExpressionLabeler();
		//            string sLabelExpr = @"[JZDH]\[JBLX]";
		//            labeler.SetLabelExpression(sLabelExpr);
		//            labeler.EnableLabel = true;
		//            fl.FeatureLabeler = labeler;
		//        }
		//        //fl.Where = "JB<=3";
		//        //fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        render.SetSymbol(new SimpleMarkerSymbol());
		//        fl.FeatureRenderer = render;

		//        layers.AddLayer(fl);
		//    }
		//    //if (true)
		//    //{
		//    //    var fl = new FeatureLayer(map);
		//    //    fl.FeatureClass = SpatialiteWorkspaceFactory.Instance.OpenFeatureClass(@"C:\myprojects\工单\20160816建库工具\资料\通川区安云乡.sqlite", "ZD_CBD");// new SpatialiteFeatureClass();
		//    //    //(fl.FeatureClass as SpatialiteFeatureClass).Open(@"C:\myprojects\工单\20160816建库工具\资料\通川区安云乡.sqlite","ZD_CBD");

		//    //    //fl.Where = "JB<=3";
		//    //    //fl.OrderByClause = "JB desc";
		//    //    var render = new SimpleFeatureRenderer();
		//    //    var sfs = new SimpleFillSymbol();
		//    //    var pen = new PenProperty(System.Drawing.Color.Blue, 4);
		//    //    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
		//    //    //pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;
		//    //    sfs.Outline = new SimpleLineSymbol(pen);
		//    //    //sfs.Background = null;
		//    //    render.SetSymbol(sfs);
		//    //    fl.FeatureRenderer = render;

		//    //    if (false)
		//    //    {
		//    //        var labeler = new ASTExpressionLabeler();
		//    //        string sLabelExpr = @"([BSM]\[DKMC])&[__FID__]";
		//    //        //string sLabelExpr = @"[__FID__]";
		//    //        //string sLabelExpr = @"[DKMC]";
		//    //        labeler.SetLabelExpression(sLabelExpr);
		//    //        labeler.EnableLabel = true;
		//    //        fl.FeatureLabeler = labeler;
		//    //    }

		//    //    layers.AddLayer(fl);
		//    //    //return;
		//    //}
		//}

		//private void btnAddSpatialite_Click(object sender, RoutedEventArgs e)
		//{
		//    var wfc = SpatialiteWorkspaceFactory.Instance;
		//    var layers = map.Layers;
		//    layers.ClearLayers();
		//    var groupLayer = new GroupLayer(map, "组合图层");
		//    layers.AddLayer(groupLayer);
		//    layers = groupLayer;
		//    string path = @"C:\myprojects\工单\20160816建库工具\资料\通川区安云乡.sqlite";
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = wfc.OpenFeatureClass(path, "ZD_CBD");
		//        //fl.Where = "JB<=3";
		//        //fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        var sfs = new SimpleFillSymbol();
		//        var pen = new PenProperty(System.Drawing.Color.Blue, 4);
		//        pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
		//        //pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;
		//        sfs.Outline = new SimpleLineSymbol(pen);
		//        //sfs.Background = null;
		//        render.SetSymbol(sfs);
		//        fl.FeatureRenderer = render;

		//        if (false)
		//        {
		//            var labeler = new ASTExpressionLabeler();
		//            string sLabelExpr = @"([BSM]\[DKMC])&[__FID__]";
		//            //string sLabelExpr = @"[__FID__]";
		//            //string sLabelExpr = @"[DKMC]";
		//            labeler.SetLabelExpression(sLabelExpr);
		//            labeler.EnableLabel = true;
		//            fl.FeatureLabeler = labeler;
		//        }

		//        layers.AddLayer(fl);
		//        //return;
		//    }
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = fl.FeatureClass = wfc.OpenFeatureClass(path, "JZX");

		//        var render = new SimpleFeatureRenderer();
		//        //var lineSymbol = new SimpleLineSymbol();
		//        //lineSymbol.Border = new System.Drawing.Pen(System.Drawing.Color.Red, 1);
		//        var pen = new PenProperty(System.Drawing.Color.Red, 2);
		//        //pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Right;
		//        pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

		//        render.SetSymbol(new SimpleLineSymbol(pen));
		//        fl.FeatureRenderer = render;

		//        layers.AddLayer(fl);
		//    }
		//    if (true)
		//    {
		//        var fl = new FeatureLayer(map);
		//        fl.FeatureClass = wfc.OpenFeatureClass(path, "JZD");

		//        if (false)
		//        {
		//            var labeler = new ASTExpressionLabeler();
		//            string sLabelExpr = @"[JZDH]\[JBLX]";
		//            labeler.SetLabelExpression(sLabelExpr);
		//            labeler.EnableLabel = true;
		//            fl.FeatureLabeler = labeler;
		//        }
		//        //fl.Where = "JB<=3";
		//        //fl.OrderByClause = "JB desc";
		//        var render = new SimpleFeatureRenderer();
		//        render.SetSymbol(new SimpleMarkerSymbol());
		//        fl.FeatureRenderer = render;

		//        layers.AddLayer(fl);
		//    }

		//    toc.Refresh();
		//    //var fc = (map.GetLayer(0) as FeatureLayer).FeatureClass;
		//    map.SetExtent(map.Layers.GetLayer(0).FullExtent);//  fc.GetFullExtent());// new GeoAPI.Geometries.Envelope(624000, 627000, 4155000, 4158000);
		//    map.FullExtent = map.Extent;
		//    map.Refresh();

		//}


		//private void btnConnect_Click(object sender, RoutedEventArgs e)
		//{
		//    var dlg = new ConnectDialog(_persist);
		//    dlg.Owner =Window.GetWindow(this);
		//    dlg.LoadState();
		//    dlg.ConectTestButton.Click += (s, e1) =>
		//    {
		//        try
		//        {
		//            if (_db == null)
		//            {
		//                _db = new SqlServer();
		//            }
		//            var conStr = dlg.GetSqlServerConnString();// "Data Source=192.168.2.3;Initial Catalog=YuLinTuCQCJ77;User ID=sa;Password=123456;";
		//            _db.Connect(conStr);
		//            dlg.SaveState();
		//            MessageBox.Show("连接成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
		//        }
		//        catch (Exception ex)
		//        {
		//            MessageBox.Show("err:" + ex.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
		//        }
		//    };
		//    dlg.OkButton.Click += (s, e1) =>
		//    {
		//        //try
		//        //{
		//        if (_db == null)
		//        {
		//            _db = new SqlServer();
		//        }
		//        var conStr = dlg.GetSqlServerConnString();// "Data Source=192.168.2.3;Initial Catalog=YuLinTuCQCJ77;User ID=sa;Password=123456;";
		//        _db.Connect(conStr);
		//        SQLServerDataSourceSelector.ShowDialog(map, conStr);
		//        dlg.SaveState();
		//        dlg.Close();
		//        /*
		//            InitMap();
		//            toc.Refresh();

		//            var layers = map.Layers;
		//            //IFeatureClass fc = null;
		//            ILayer layer0 = null;
		//           // int i = 0;
		//            //foreach (var l in map.Layers.Items)
		//            for (int i=0;i < layers.LayerCount; ++i)
		//            {

		//                //var fc1=(l as FeatureLayer).FeatureClass;
		//                //(fc1 as SqlServerFeatureClass).Connect(conStr);
		//                var fl = layers.GetLayer(i) as FeatureLayer;
		//                (fl.FeatureClass as SqlServerFeatureClass).Open(conStr);
		//                //if (fc == null)
		//                //    fc = fl.FeatureClass;
		//                if (layer0 == null)
		//                {
		//                    layer0 = fl;
		//                }
		//            }

		//            if (map.FullExtent == null)
		//            {
		//                map.SetExtent(layer0.FullExtent);// fc.GetFullExtent());// new GeoAPI.Geometries.Envelope(624000, 627000, 4155000, 4158000);
		//                map.FullExtent = map.Extent;
		//            }
		//            map.Refresh();
		//            //RefreshGrid();
		//            //map.InvalidateVisual();
		//            //btnFullExtent.IsEnabled = true;
		//            dlg.SaveState();
		//            dlg.Close();
		//        */
		//        //}
		//        //catch (Exception ex)
		//        //{
		//        //    MessageBox.Show("err:" + ex.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
		//        //}
		//    };
		//    dlg.ShowDialog();
		//}

		//private void btnAddShapeFile_Click(object sender, RoutedEventArgs e)
		//{
		//    try
		//    {
		//        bool fSetFullExtent = map.Layers.LayerCount == 0;
		//        var dlg = new OpenFileDialog();
		//        //openFileDialog.InitialDirectory="c:";//注意这里写路径时要用c:而不是c:　
		//        dlg.Filter = "Shape文件(*.shp)|*.shp";
		//        dlg.RestoreDirectory = true;
		//        dlg.FilterIndex = 1;
		//        dlg.Multiselect = true;
		//        if (dlg.ShowDialog() != true)
		//        {
		//            return;
		//        }
		//        var wfc = ShapeFileWorkspaceFactory.Instance;
		//        var lstFeatureClass = new List<Item>();
		//        int i = 0;
		//        foreach (string file in dlg.FileNames)
		//        {
		//            //var fc = new ShapeFileFeatureClass();
		//            //fc.Open(file);
		//            var path = FileUtil.GetFilePath(file);
		//            var tableName = FileUtil.GetFileName(file);
		//            var fc = wfc.OpenFeatureClass(path, tableName);// new ShapeFileFeatureClass();
		//            lstFeatureClass.Add(new Item
		//            {
		//                Item1 = fc,
		//                Item2 = i++
		//            });
		//        }
		//        lstFeatureClass.Sort((t1, t2) =>
		//        {
		//            var a = t1.Item1;
		//            var b = t2.Item1;
		//            if (a.ShapeType == b.ShapeType)
		//            {
		//                return t1.Item2 < t2.Item2 ? -1 : 1;
		//            }
		//            if (a.ShapeType == eGeometryType.eGeometryPolygon)
		//                return -1;
		//            if (b.ShapeType == eGeometryType.eGeometryPolygon)
		//                return 1;
		//            if (a.ShapeType == eGeometryType.eGeometryPolyline)
		//                return -1;
		//            if (b.ShapeType == eGeometryType.eGeometryPolyline)
		//                return 1;
		//            return t1.Item2 < t2.Item2 ? -1 : 1;
		//        });
		//        foreach (var fc in lstFeatureClass)
		//        {
		//            var fl = new FeatureLayer(map);
		//            fl.FeatureClass = fc.Item1;
		//            map.Layers.AddLayer(fl);
		//        }
		//        toc.Refresh();
		//        if (map.FullExtent == null)
		//        {
		//            if (lstFeatureClass.Count > 0)
		//            {
		//                map.FullExtent = lstFeatureClass[0].Item1.GetFullExtent();
		//            }
		//        }
		//        if (fSetFullExtent && map.FullExtent != null)
		//        {
		//            map.SetExtent(map.FullExtent, false);
		//        }
		//        map.Refresh();
		//        //bool fZoomToFullExtent = map.Layers.LayerCount == 0;
		//        //var fl=AddShapeFile(dlg.FileName);
		//        //toc.Refresh();
		//        //if (fZoomToFullExtent)
		//        //{
		//        //    map.FullExtent = fl.FullExtent;
		//        //    map.SetExtent(map.FullExtent);
		//        //}
		//        //else
		//        //{
		//        //    map.Refresh();
		//        //}
		//    }
		//    catch (Exception ex)
		//    {
		//        UIHelper.ShowExceptionMessage(ex);
		//    }
		//}
		//private void btnShapeFile_Click(object sender, RoutedEventArgs e)
		//{
		//    //System.Diagnostics.Debug.Assert(map.Layers.Items.Count() == 1);
		//    //IFeatureClass fc = null;
		//    //foreach (var l in map.Layers.Items)
		//    //{
		//    //    var fl = l as FeatureLayer;
		//    //    var fc1 = fl.FeatureClass;
		//    //    if (!(fc1 is ShapeFileFeatureClass))
		//    //    {
		//    //        var sff=new ShapeFileFeatureClass();
		//    //        fl.FeatureClass = sff;
		//    //        sff.Open(@"C:\temp\DK511425.shp");
		//    //        fc1 = sff;
		//    //    }
		//    //    //(fc1 as SqlServerFeatureClass).Connect(conStr);
		//    //    if (fc == null)
		//    //        fc = fc1;
		//    //}
		//    InitMap2();
		//    toc.Refresh();
		//    //var fc = (map.GetLayer(0) as FeatureLayer).FeatureClass;
		//    map.SetExtent(map.Layers.GetLayer(0).FullExtent);//  fc.GetFullExtent());// new GeoAPI.Geometries.Envelope(624000, 627000, 4155000, 4158000);
		//    map.FullExtent = map.Extent;
		//    map.Refresh();
		//    //RefreshGrid();
		//    //map.InvalidateVisual();
		//    //btnFullExtent.IsEnabled = true;
		//}

		//private void btnDocument_Click(object sender,RoutedEventArgs e)
		//{

		//}
		#region IRequestClosing
		public bool RequestClosing()
		{
			if (Map.IsDocumentDirty)
			{
				var mr = MessageBox.Show("文档已经发生改变，是否保存", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (mr == MessageBoxResult.Cancel)
				{
					return false;
				}
				else if (mr == MessageBoxResult.Yes)
				{
					Map.SaveDocument();
				}
			}
			return true;
		}
		#endregion

		private DelayDoImpl _delayRefreshLayer = new DelayDoImpl(200);
		private void InitTimeSlider()
		{
			var sql = "select min(DJSJ) from DLXX_DK";
			var o=MyGlobal.Workspace.QueryOne(sql);
			if (o is DateTime d)
			{
				tsc.Init(d);
			}
		}

		public void Dispose()
		{
			Map.Dispose();
			overview.Dispose();
			//hawkeyeMap.Dispose();
		}
	}
}
