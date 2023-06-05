using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.GIS;
using Agro.GIS.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.LibCore.GIS;
using Agro.Library.Model;

namespace Agro.Module.Map
{
	/// <summary>
	/// MapPage.xaml 的交互逻辑
	/// </summary>
	public partial class MapPage : UserControl, IRequestClosing, IDisposable//, IBusyWork
	{
		class LayerPanelContextMenu
		{
			private readonly MapPage _p;
			private readonly ContextMenu _layerMenu;
			public LayerPanelContextMenu(MapPage p)
			{
				_p = p;
				_layerMenu = p.sidePage.ContextMenu;				
				InitContextMenu();
				p.miExportShapeFile.Click += (s, e) =>
				{
					var mi = (MenuItem)s;
					ExportToShapefileDialog.ShowDialog(Window.GetWindow(p), (FeatureLayer)mi.Tag);
				};
				p.miExportLayerFile.Click += (s, e) =>
				{
					var mi = (MenuItem)s;
					var fl = (FeatureLayer)mi.Tag;
					fl.SaveLayerAs(null);
				};
				p.miPropertyTable.Click += (s, e) =>
				{
					var mi = (MenuItem)s;
					var fl = (FeatureLayer)mi.Tag;
					ShowPropertyTable(fl);
				};
				p.miRemoveLayer.Click += (s, e) =>
				{
					var mi = (MenuItem)s;
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
					if (((MenuItem)s).Tag is ILayer lyr)
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
					if (((MenuItem)s).Tag is ILayer lyr)
					{
						lyr.MaxVisibleScale = null;
						lyr.MinVisibleScale = null;
						p.Map.RefreshLayer(lyr);
					}
				};
				p.miSetMaxVisibleScale.Click += (s, e) =>
				{
					var lyr = (ILayer)((MenuItem)s).Tag;
					lyr.MaxVisibleScale = p.Map.MapScale;
					p.Map.RefreshLayer(lyr);
				};
				p.miSetMinVisibleScale.Click += (s, e) =>
				{
					var lyr = (ILayer)((MenuItem)s).Tag;
					lyr.MinVisibleScale = p.Map.MapScale;
					p.Map.RefreshLayer(lyr);
				};
			}
			private void InitContextMenu()
			{
				var p = _p;
				p.sidePage.ContextMenu = null;
				p.layersControl.TocControl.MouseRightButtonDown += (s, e) =>
				{
					var hi = p.layersControl.TocControl.HitTest(e.GetPosition(p.layersControl.TocControl));
					if (hi == null)
					{
						_layerMenu.PlacementTarget = null;
						_layerMenu.IsOpen = false;
						return;
					}
					var fFeatureLayer = hi.Layer is FeatureLayer;
					p.miPropertyTable.IsEnabled = fFeatureLayer == true;
					p.miExportShapeFile.IsEnabled = fFeatureLayer == true;
					p.miExportLayerFile.IsEnabled = fFeatureLayer == true;

					p.miClearVisibleScale.IsEnabled = hi.Layer.MinVisibleScale != null || hi.Layer.MaxVisibleScale != null;

					foreach (var it in _layerMenu.Items)
					{
						AttachTag(it as MenuItem, hi.Layer);
					}
					_layerMenu.PlacementTarget = p.layersControl.TocControl;
					_layerMenu.IsOpen = true;
				};
			}
			private void AttachTag(MenuItem? mi, object tag)
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
			private void ShowPropertyTable(FeatureLayer fl)
			{
				if (fl != null)
				{
					_p.mlTableView.ShowPropertyTable(fl);
				}
			}
		}

		internal class DefaultLayers
		{
			/// <summary>
			/// 创建本地图层表映射数据
			/// </summary>
			public static List<ILayer> CreateLayers(IFeatureWorkspace wfc, GIS.Map map)
			{
				var layers = new List<ILayer>();

				#region 定位基础
				{
					var groupLayer = new GroupLayer(map, "定位基础")
					{
						Visible = false
					};
					var kzdLayer = CreatePointFeatureLayer(wfc, map, "DLXX_KZD", "控制点"
						, "#FFCCE1A0", "#FF728944", 6);
					groupLayer.AddLayer(kzdLayer);
					layers.Add(groupLayer);
				}
				#endregion
				#region 管辖区域
				{
					var groupLayer = new GroupLayer(map, "管辖区域")
					{
						IsExpanded = false
					};
					layers.Add(groupLayer);

					var layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "县级区域",
						"#FFEDE8E6", "#FFC4BFBD", 1
						, "JB = 4", "[MC]", 100000, 3000000);
					SetupLabeler(layer, 20, "#FF000000");
					groupLayer.AddLayer(layer);


					layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "乡级区域",
						"#FFEFEBDB", "#FFC4BFBD", 1
						, "JB = 3", "[MC]", 30000, 150000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnXzdyBeforeUseWhere;
					groupLayer.AddLayer(layer);


					layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "村级区域",
						"#FFEAE0BD", "#FF9E9470", 1
						, "JB = 2", "[MC]", 10000, 60000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnXzdyBeforeUseWhere;
					groupLayer.AddLayer(layer);

					layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "组级区域",
						"#FFEFEBDB", "#FFC4BFBD", 1
						, "JB = 1", "[MC]", 3000, 20000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnXzdyBeforeUseWhere;
					groupLayer.AddLayer(layer);
				}
				#endregion

				#region 区域界线
				{
					var groupLayer = new GroupLayer(map, "区域界线")
					{
						Visible = false
					};
					layers.Add(groupLayer);
					var layer = CreateLineFeatureLayer(wfc, map, "DLXX_QYJX", "区域界线"
						, "#FF9E9470", 1, null, null, 3000, 100000);
					groupLayer.AddLayer(layer);
				}
				#endregion



				#region 基本农田
				{
					var groupLayer = new GroupLayer(map, "基本农田")
					{
						Visible = false
					};
					layers.Add(groupLayer);

					var layer = CreateFillFeatureLayer(wfc, map, "DLXX_JBNTBHQ", "基本农田保护区",
						"#FFEDE8E6", "#FFC4BFBD", 1
						, "", "", null, 10000);//, 30000, 150000);
					groupLayer.AddLayer(layer);
				}
				#endregion

				#region 其他地物
				{
					var groupLayer = new GroupLayer(map, "其他地物")
					{
						Visible = false
					};
					layers.Add(groupLayer);
					var layer = CreateFillFeatureLayer(wfc, map, "DLXX_MZDW", "面状地物",
						"#FFEFEBDB", "#FFC4BFBD", 1
						, "", "DWMC", null, 8000);
					groupLayer.AddLayer(layer);

					layer = CreateLineFeatureLayer(wfc, map, "DLXX_XZDW", "线状地物"
						, "#FF9E9470", 1, null, "[DWMC]", null, 8000);
					groupLayer.AddLayer(layer);

					layer = CreatePointFeatureLayer(wfc, map, "DLXX_DZDW", "点状地物"
						, "#FFCCE1A0", "#FF728944", 6, null, "[DWMC]");
					layer.MaxVisibleScale = 8000;
					groupLayer.AddLayer(layer);
				}
				#endregion

				#region 地块类别
				{
					var groupLayer = new GroupLayer(map, "地块类别")
					{
						IsExpanded = true
					};
					layers.Add(groupLayer);

					var layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "承包地",
						"#FFCCE1A0", "#FF728944", 1
						, "DKLB = '10' AND ZT=1", "StrContains([CBFMC],'/')?\"共有地块\";[CBFMC]", null, 8000);//, 30000, 150000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					groupLayer.AddLayer(layer);

					layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "自留地",
						"#D3FFBEFF", "#FF728944", 1
						, "DKLB = '21' AND ZT=1", "[CBFMC]", null, 8000);//, 30000, 150000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					groupLayer.AddLayer(layer);

					layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "机动地",
						"#E39E00FF", "#FF728944", 1
						, "DKLB = '22' AND ZT=1", "[CBFMC]", null, 8000);//, 30000, 150000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					groupLayer.AddLayer(layer);

					layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "开荒地",
						"#F0B0CFFF", "#FF728944", 1
						, "DKLB = '23' AND ZT=1", "[CBFMC]", null, 8000);//, 30000, 150000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					groupLayer.AddLayer(layer);

					layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "其他集体土地",
										"#BED2FFFF", "#FF728944", 1
										, "DKLB = '99' AND ZT=1", "[CBFMC]", null, 8000);//, 30000, 150000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					groupLayer.AddLayer(layer);
				}
				#endregion

				#region test
				if (false)
				{
					var fl = new FeatureLayer(map)
					{
						FeatureClass = wfc.OpenFeatureClass("DLXX_DK"),
						Name = "地块类别",
						Where = "ZT=1",

						MinVisibleScale = null,
						MaxVisibleScale = 8000
					};

					var render = new UniqueValueRenderer();
					//render.SetSymbol(symbol);
					render.AddField("DKLB");
					render.AddValue("10", "承包地", CreateFillSymbol("#FF728944", "#FFCCE1A0", 1));
					render.AddValue("21", "自留地", CreateFillSymbol("#D3FFBEFF", "#FF728944", 1));
					render.AddValue("22", "机动地", CreateFillSymbol("#E39E00FF", "#FF728944", 1));
					render.AddValue("23", "开荒地", CreateFillSymbol("#F0B0CFFF", "#FF728944", 1));
					render.AddValue("99", "其他集体土地", CreateFillSymbol("#BED2FFFF", "#FF728944", 1));
					fl.FeatureRenderer = render;
					layers.Add(fl);

					var sLabelExpr = "StrContains([CBFMC],'/')?\"共有地块\";[CBFMC]";
					//if (sLabelExpr != null)
					//{
					var labeler = new ASTExpressionLabeler();
					//string sLabelExpr = @"([BSM]\[DKMC])&[__FID__]";
					//string sLabelExpr = @"[__FID__]";
					//string sLabelExpr = @"[DKMC]";
					labeler.SetLabelExpression(sLabelExpr);
					labeler.EnableLabel = true;
					fl.FeatureLabeler = labeler;
					//}
				}
				#endregion

				#region 界址数据
				{
					var groupLayer = new GroupLayer(map, "界址数据")
					{
						Visible = false
					};
					layers.Add(groupLayer);
					var layer = CreateLineFeatureLayer(wfc, map, "DLXX_DK_JZX", "界址线"
						, "#FF9E9470", 1, null, null, null, 8000);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					groupLayer.AddLayer(layer);

					layer = CreatePointFeatureLayer(wfc, map, "DLXX_DK_JZD", "界址点"
						, "#FFCCE1A0", "#FF728944", 6);
					(layer as FeatureLayer).OnBeforeUseWhere = LayerWhereUtil.OnDKBeforeUseWhere;
					layer.MaxVisibleScale = 8000;
					groupLayer.AddLayer(layer);
				}
				#endregion
				return layers;
			}


			public static GroupLayer CreateOverviewLayer(IFeatureWorkspace wfc, MapControl mc)
			{
				var map = mc.FocusMap;
				var groupLayer = new GroupLayer(map, "管辖区域");

				var layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "县级区域",
					"#FFEDE8E6", "#FFC4BFBD", 1
					, "JB = 4");//, "[MC]", 100000, 1000000);
											//SetupLabeler(layer, 20, "#FF000000");
				groupLayer.AddLayer(layer);


				layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "乡级区域",
					"#FFEFEBDB", "#FFC4BFBD", 1
					, "JB = 3");//, "[MC]", 30000, 150000);
				groupLayer.AddLayer(layer);

				return groupLayer;
			}

			private static IFeatureLayer CreateLineFeatureLayer(IFeatureWorkspace wfc, GIS.Map map
						, string tableName, string layerName, string lineColor, double lineWidth = 1, string where = null, string sLabelExpr = null
						, double? MinVisibleScale = null, double? MaxVisibleScale = null
						)
			{
				var symbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth);

				var fl = new FeatureLayer(map)
				{
					Name = layerName,
					Where = where,

					MinVisibleScale = MinVisibleScale,
					MaxVisibleScale = MaxVisibleScale
				};

				fl.DataSourceMeta.ConnectionString = wfc.ConnectionString;
				fl.DataSourceMeta.TableName = tableName;
				fl.DataSourceMeta.DataSourceType = DataSourceMetaData.DataSourceTypeString(wfc.DatabaseType);

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

			private static ISymbol CreateFillSymbol(string fillColor, string lineColor, double lineWidth = 1)
			{
				var lineSymbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth);
				var symbol = SymbolUtil.CreateSimpleFillSymbol(ColorUtil.ConvertFromString(fillColor), lineSymbol);
				return symbol;
			}

			private static IFeatureLayer CreateFillFeatureLayer(IFeatureWorkspace wfc, GIS.Map map
				, string tableName, string layerName, string fillColor, string lineColor, double lineWidth = 1, string? where = null, string? sLabelExpr = null
				, double? MinVisibleScale = null, double? MaxVisibleScale = null
				)
			{
				var lineSymbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth);
				var symbol = SymbolUtil.CreateSimpleFillSymbol(ColorUtil.ConvertFromString(fillColor), lineSymbol);

				var fl = new FeatureLayer(map)
				{
					Name = layerName,
					Where = where,

					MinVisibleScale = MinVisibleScale,
					MaxVisibleScale = MaxVisibleScale
				};
				fl.DataSourceMeta.ConnectionString = wfc.ConnectionString;
				fl.DataSourceMeta.TableName = tableName;
				fl.DataSourceMeta.DataSourceType = DataSourceMetaData.DataSourceTypeString(wfc.DatabaseType);

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
			private static IFeatureLayer CreatePointFeatureLayer(IFeatureWorkspace wfc, GIS.Map map, string tableName, string layerName
				, string fillColor, string lineColor, double markerSize = 6
				, string? where = null, string? sLabelExpr = null)
			{
				var symbol = SymbolUtil.CreateSimpleMarkerSymbol(ColorUtil.ConvertFromString(fillColor),
					ColorUtil.ConvertFromString(lineColor), markerSize);
				var fl = new FeatureLayer(map)
				{
					Name = layerName,
					Where = where
				};

				fl.DataSourceMeta.ConnectionString = wfc.ConnectionString;
				fl.DataSourceMeta.TableName = tableName;
				fl.DataSourceMeta.DataSourceType = DataSourceMetaData.DataSourceTypeString(wfc.DatabaseType);

				var render = new SimpleFeatureRenderer();
				render.SetSymbol(symbol);// new SimpleMarkerSymbol());
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


			private static void SetupLabeler(IFeatureLayer fl, double fontSize, string? fontColor)
			{
				if (fl.FeatureLabeler is ASTExpressionLabeler lbl)
				{
					var ts = lbl.TextSymbol;
					ts.Font.FontSize = fontSize;
					ts.Color = ColorUtil.ConvertFromString(fontColor);
				}
			}	
		}

		private TocControl Toc
		{
			get { return layersControl.TocControl; }
		}

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

		private Action? OnBeforeDisposed;
		public MapPage()
		{
			InitializeComponent();

			new LayerPanelContextMenu(this);
			_featureVertextView = new FeatureVertexViewWrap(this.sidePage, this.MapControl);
			mlTableView.FinalConstruct(MapControl, sidePage, _featureVertextView);
			this.btnFeatureVertex.OnFeatureSelected += it => { _featureVertextView.Show(it.FeatureLayer, it.Feature); };

			new ToggleSideView(this.sidePage, btnLayer, null, (s, ov, nv) =>
			{
				if (nv)
				{
					var sl = layersControl.TocControl.SelectedLayer;
					layersControl.TocControl.SelectedLayer = sl;
				}
			}).Panel = sidePage.LeftPanel;

			layersControl.OnAddDataButtonClick += MyMapUtil.OnAddDataButtonClick;

			new ToggleSideView(sidePage, btnZone, () =>
			{
				var zoneTree = new ZoneTree()
				{
					ContainGroupNode = true
				};
				zoneTree.OnItemDoubleClick += ZoomToZone;
				zoneTree.OnZoneSelected += ZoomToZone;
				return zoneTree;
			});

			new ToggleSideView(this.sidePage, btnLayerSelection, () => new LayerSelectionPanel(Map), (s, ov, nv) => { if (nv) (s.Panel as LayerSelectionPanel).Refresh(); });
			new ToggleSideView(this.sidePage, btnSearch, () => new MapSearchPanel(Map));
			new ToggleSideView(this.sidePage, btnDataQuery, () => new DataQueryControlPanel(mapControl));
			new ToggleSideView(this.sidePage, btnMapSheet, () => new MapsheetNavPanel(mapControl));
			DependencyObjectUtilEx.FindCommandButton(toolbar, cb => BindMapBuddy(cb));


			#region yxm 2018-3-8
			BindMapBuddy(btnClear);
			mapControl.TempElements.OnAddElement += me => { btnClear.Visibility = Visibility.Visible; };
			mapControl.TempElements.OnClear += () => btnClear.Visibility = Visibility.Collapsed;
			#endregion

			mapControl.CurrentTool = MapControl.CreatePanTool();
			Toc.MapHost = mapControl;

			msrSS.Map = Map;
			coordSS.Map = Map;
			msSS.Map = Map;

			sidePage.HidePanel(AnchorSide.Bottom);

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

			MyGlobal.OnDatasourceChanged += () => LoadData();

			btnHistory.Click += (s, e) =>
			{
				try
				{
					MyGlobal.MainWindow.OpenPage(new HistoryMapPage(), "历史回溯", CommonImageUtil.Image16("history.png"));
				}
				catch (Exception ex)
				{
					UIHelper.ShowExceptionMessage(ex);
				}
			};

			Loaded += (s, e) =>
			{
				if (Map.Layers.LayerCount == 0)
				{
					LoadData();
				}
			};
		}

		private void LoadData()
		{
			var pWS = MyGlobal.Workspace;
			var m = this.Map;

			Try.Catch(() =>
			{
				m.Layers.ClearLayers();
				Map.SuprresEvents = true;
				var layers = DefaultLayers.CreateLayers(pWS, Map);
				foreach (var layer in layers)
				{
					this.Map.Layers.AddLayer(layer);
				}
				//this.map.SaveDocumentAs("d:/temp/库管地图.kmd");
			}, false);
			Toc.Refresh();
			Map.SuprresEvents = false;

			var t = new MyTask();
			OnBeforeDisposed += () => t.Cancel();
			t.Go(token =>
			{
				Try.Catch(() =>
				{
					bool fFullExtentSet = false;
					Map.Connect(fl =>
					{
						if (fl.FeatureClass.TableName == DLXX_XZDY.GetTableName())
						{
							if (!fFullExtentSet)
							{
								var fullEnv = ZoneUtil.GetFullExtent();
								if (fullEnv != null)
								{
									Invoke(() =>
									{
										m.FullExtent = fullEnv;
										m.SetExtent(m.FullExtent);
									});
								}
								fFullExtentSet = true;
							}
						}
					});
				});
			});
		}
		private void Invoke(Action action)
		{
			this.Dispatcher.Invoke(new Action(() =>
			{
				action();
			}));
		}

		void ZoomToZone(ShortZone zone)
		{
			try
			{
				var g = ZoneUtil.QueryShape(zone);
				if (g != null && !g.IsEmpty && g.Area > 0)
				{
					Map.SetExtent(new OkEnvelope(g));
				}
			}
			catch { }
		}
		private void BindMapBuddy(IMapBuddy buddyControl)
		{
			buddyControl.MapHost = mapControl;
			if (buddyControl is IDisposable disposable)
			{
				Map.OnDispose += () => disposable.Dispose();
			}
		}

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

		public void Dispose()
		{
			OnBeforeDisposed?.Invoke();
			Map.Dispose();
			overview.Dispose();
		}
	}
}
