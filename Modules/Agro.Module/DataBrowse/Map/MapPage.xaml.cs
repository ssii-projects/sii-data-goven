using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.GIS;
using Agro.GIS.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using GeoAPI.Geometries;
using Agro.Module.DataBrowse;
using Agro.LibCore.GIS;

namespace DxExport
{
    /// <summary>
    /// MapPage.xaml 的交互逻辑
    /// </summary>
    public partial class MapPage : UserControl, IRequestClosing, IDisposable, IBusyWork
    {
        class Item
        {
            public IFeatureClass Item1;
            public int Item2;
        }
        class LayerPanelContextMenu
        {
            private readonly MapPage _p;
            private readonly ContextMenu _layerMenu;
            public LayerPanelContextMenu(MapPage p)
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
                    var fl = mi.Tag as ILayer;
                    if (fl != null)
                    {
                        p.map.FocusMap.Layers.RemoveLayer(fl);
                        p.Toc.Refresh();
                        p.map.Refresh();
                    }
                };
                p.miLayerProperty.Click += (s, e) =>
                {
                    var lyr = (s as MenuItem).Tag as ILayer;// as IFeatureLayer;
                    if (lyr != null)
                    {
                        var dlg = new LayerPropertyDialog();
                        dlg.ShowProperty(lyr, new LayerPropertyDialog.ShowPropertyPrms()
                        {
                            OnApplied = () => { p.Toc.Refresh(); p.map.Refresh(); }
                        });
                    }
                };
                p.miClearVisibleScale.Click += (s, e) =>
                {
					//var mi = s as MenuItem;
					// as IFeatureLayer;
					if ((s as MenuItem).Tag is ILayer lyr)
					{
						lyr.MaxVisibleScale = null;
						lyr.MinVisibleScale = null;
						p.map.RefreshLayer(lyr);
					}
				};
                p.miSetMaxVisibleScale.Click += (s, e) =>
                {
                    var lyr = (s as MenuItem).Tag as ILayer;
                    lyr.MaxVisibleScale = p.map.FocusMap.MapScale;//.Scale;
                    p.map.RefreshLayer(lyr);
                };
                p.miSetMinVisibleScale.Click += (s, e) =>
                {
                    var lyr = (s as MenuItem).Tag as ILayer;
                    lyr.MinVisibleScale = p.map.FocusMap.MapScale;//.Scale;
                    p.map.RefreshLayer(lyr);
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
                if (fl != null)
                {
					_p.mlTableView.ShowPropertyTable(fl);
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
                return map;
            }
        }
		internal Map Map
		{
			get { return map.FocusMap; }
		}

		private readonly Persist _persist;

        internal HJDataRootPath CurrentPath { get;private set; }

        private readonly NaviTree _zoneTree = new NaviTree();
        public MapPage()
        {
            _persist = null;// persist;
            InitializeComponent();

            new LayerPanelContextMenu(this);
            _featureVertextView=new FeatureVertexViewWrap(this.sidePage,MapControl);
			mlTableView.FinalConstruct(MapControl, sidePage, _featureVertextView);
			this.btnFeatureVertex.OnFeatureSelected +=it => { _featureVertextView.Show(it.FeatureLayer, it.Feature); };

			//new ToggleViewButton(this, btnLayer,sidePage.LeftPanel);// _leftPnlContainer.Child);
			new ToggleSideView(this.sidePage, btnLayer, null, (s, ov, nv) => {
				if (nv)
				{
					var sl = layersControl.TocControl.SelectedLayer;
					layersControl.TocControl.SelectedLayer = sl;
				}
			}).Panel = sidePage.LeftPanel;
			layersControl.OnAddDataButtonClick += MyMapUtil.OnAddDataButtonClick;
			//new ToggleViewButton(this, btnTopoCheck, new TopoCheckSelectPanel(map));


			_zoneTree.OnZoneSelected += zone =>
            {
                try
                {
                    var g = zone.QueryShape();// ZoneUtil.QueryShape(zone);
                    if (g != null && !g.IsEmpty && g is IPolygon)
                    {
                        map.FocusMap.SetExtent(new OkEnvelope(g));// g as IPolygon);
                    }
                }
                catch { }
            };
			//new ToggleViewButton(this, btnZone, _zoneTree);
			new ToggleSideView(sidePage, btnZone, () => {return _zoneTree;});

			//var zoneTree = new ZoneTree();
			//zoneTree.OnZoneSelected += zone =>
			//{
			//    try
			//    {
			//        var g = ZoneUtil.QueryShape(zone);
			//        if (g != null && !g.IsEmpty && g is IPolygon)
			//        {
			//            map.SetExtent(g as IPolygon);
			//        }
			//    }
			//    catch { }
			//};
			//new ToggleViewButton(this, btnZone, zoneTree);

			//var lsp = new LayerSelectionPanel(map.FocusMap);
			//new ToggleViewButton(this, btnLayerSelection, lsp,(ov,nv)=> {if(nv) lsp.Refresh(); });
			new ToggleSideView(this.sidePage, btnLayerSelection, () => { return new LayerSelectionPanel(Map); }, (s, ov, nv) => { if (nv) (s.Panel as LayerSelectionPanel).Refresh(); });


			//var efp = new EditFeaturePanel(map.FocusMap);
			//         new ToggleViewButton(this, btnEdit,efp,(ov,nv)=> { efp.OnVisible(nv); } );
			new ToggleSideView(sidePage, btnEdit, () => {
				var p = new EditFeaturePanel(map.FocusMap);
				p.OnFilter += fl =>
				{
					IFeatureClass f = fl.FeatureClass;
					if (f == null || f.ShapeType == eGeometryType.eGeometryPolygon)
					{
						return false;
					}
					return true;
				};
				return p;
			}, (s, ov, nv) => { (s.Panel as EditFeaturePanel).OnVisible(nv); });


			DependencyObjectUtilEx.FindCommandButton(toolbar, cb =>BindMapBuddy(cb));


            #region yxm 2018-3-8
            BindMapBuddy(btnClear);
            map.TempElements.OnAddElement += me => { btnClear.Visibility = Visibility.Visible; };
            map.TempElements.OnClear += () => btnClear.Visibility = Visibility.Collapsed;
            #endregion

            map.CurrentTool = MapControl.CreatePanTool();// new MapPanTool();

            Toc.MapHost = map;
            //toc.SetMap(map);

            msrSS.Map = map.FocusMap;
            coordSS.Map = map.FocusMap;
            msSS.Map = map.FocusMap;
            sidePage.HidePanel(AnchorSide.Bottom);

            //new MapDocContextMenu(this);
           
        }
        public void Init(HJDataRootPath path)
        {
            CurrentPath = path;
            //LoadData();
        }
        public void LoadData()
        {
            try
            {
                var pWS = ShapeFileFeatureWorkspaceFactory.Instance.OpenWorkspace(CurrentPath.RootPath+"矢量数据");// MyGlobal.Workspace;// Factory.OpenWorkspace("DATA SOURCE =ORCL33;USER ID=REGCERT;PASSWORD = 123456");
                Invoke(() =>
                {
					//map.SuprresEvents = true;
					try
					{
						IFeatureLayer layerXian, layerXiang, layerCun;
						IPolygon fullExtent;
						var layers = DefaultLayers.CreateLayers(pWS, this.MapControl, CurrentPath, out fullExtent, out layerXian, out layerXiang, out layerCun);

						//IFeatureLayer fullExtentLayer;
						//var layers = DefaultLayers.CreateLayers(pWS, this.MapControl, CurrentPath, out fullExtentLayer);
						foreach (var layer in layers)
						{
							this.map.FocusMap.Layers.AddLayer(layer);
						}
						//this.map.SaveDocumentAs("d:/temp/库管地图.kmd");
						if (fullExtent != null)
						{
							map.FocusMap.FullExtent = new Agro.LibCore.GIS.OkEnvelope(fullExtent);// fullExtentLayer.FullExtent;
							map.FocusMap.SetExtent(map.FocusMap.FullExtent);
						}

						_zoneTree.Init(map, layerXian, layerXiang, layerCun);

					}
					catch { }
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    MessageBox.Show(ex.ToString(), "异常", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
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
            buddyControl.MapHost = map;
            if(buddyControl is IDisposable)
            {
                map.OnDispose += () =>  (buddyControl as IDisposable).Dispose();
            }
        }
        #region IRequestClosing
        public bool RequestClosing()
        {
            if (map.FocusMap.IsDocumentDirty)
            {
                var mr = MessageBox.Show("文档已经发生改变，是否保存", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (mr == MessageBoxResult.Cancel)
                {
                    return false;
                }
                else if (mr == MessageBoxResult.Yes)
                {
                    map.SaveDocument();
                }
            }
            return true;
        }
        #endregion

        public void Dispose()
        {
            map.Dispose();
        }
    }


    class DefaultLayers
    {
        /// <summary>
        /// 创建本地图层表映射数据
        /// </summary>
        public static List<ILayer> CreateLayers(IFeatureWorkspace wfc, MapControl map, HJDataRootPath prms, out IPolygon fullExtent
            , out IFeatureLayer lyrXian, out IFeatureLayer lyrXiang, out IFeatureLayer lyrCun)
        {
            fullExtent = null;
            List<ILayer> layers = new List<ILayer>();
            //ILayer layer = InitalizeLayer("栅格数据", MapLayerName.RasterGroup_Layer, "", eGeometryType.Unknown, false, false, false, false);//栅格数据
            //layers.Add(layer);
            //layer = InitalizeLayer("其他栅格数据", MapLayerName.OtherRaster_Layer, "", eGeometryType.Unknown, false, false, false, false, 0, 0, MapLayerName.RasterGroup_Layer);//其他栅格数据
            //layers.Add(layer);
            //layer = InitalizeLayer("数字栅格地图", MapLayerName.DigitalRaster_Layer, "", eGeometryType.Unknown, false, false, false, false, 0, 0, MapLayerName.RasterGroup_Layer);//数字栅格地图
            //layers.Add(layer);
            //layer = InitalizeLayer("数字正射影像图", MapLayerName.OrthoRaster_Layer, "", eGeometryType.Unknown, false, false, false, false, 0, 0, MapLayerName.RasterGroup_Layer);//数字正射影像图
            //layers.Add(layer);
            #region 定位基础
            {
                var tableName = GetShapeTableName(prms, "KZD");
                if (tableName != null)
                {
                    var groupLayer = new GroupLayer(map.FocusMap, "定位基础");
                    var kzdLayer = CreatePointFeatureLayer(wfc, map, tableName, "控制点"
                        , "#FFCCE1A0", "#FF728944", 6);
                    groupLayer.AddLayer(kzdLayer);
                    layers.Add(groupLayer);
                }
            }
            #endregion
            #region 管辖区域
            {
                var groupLayer = new GroupLayer(map.FocusMap, "管辖区域");
                layers.Add(groupLayer);

                var tableName = GetShapeTableName(prms, "XJXZQ");
                var layer = CreateFillFeatureLayer(wfc, map, tableName, "县级区域",
                    "#FFEDE8E6", "#FFC4BFBD", 1
                    , "JB = 4", "[XZQMC]", 100000, 1000000);
                groupLayer.AddLayer(layer);

                lyrXian = layer;
                fullExtent = GetFullExtent(layer);

                tableName = GetShapeTableName(prms, "XJQY");
                layer = CreateFillFeatureLayer(wfc, map, tableName, "乡级区域",
                    "#FFEFEBDB", "#FFC4BFBD", 1
                    , "JB = 3", "[XJQYMC]");//, 30000, 150000);
                groupLayer.AddLayer(layer);
                lyrXiang = layer;
                if (fullExtent == null)
                {
                    fullExtent = GetFullExtent(layer);
                }

                tableName = GetShapeTableName(prms, "CJQY");
                layer = CreateFillFeatureLayer(wfc, map, tableName, "村级区域",
                    "#FFEAE0BD", "#FF9E9470", 1
                    , "JB = 2", "[CJQYMC]");//, 30000, 150000);
                    groupLayer.AddLayer(layer);

                lyrCun = layer;
                if (fullExtent == null)
                {
                    fullExtent = GetFullExtent(layer);
                }
            }
            #endregion

            #region 区域界线
            {
                var groupLayer = new GroupLayer(map.FocusMap, "区域界线");
                layers.Add(groupLayer);
                var tableName = GetShapeTableName(prms, "QYJX");
                var layer = CreateLineFeatureLayer(wfc, map, tableName, "区域界线"
                    , "#FF9E9470", 1, null, null, 3000, 100000);
                groupLayer.AddLayer(layer);
            }
            #endregion

            #region 基本农田
            {
                var groupLayer = new GroupLayer(map.FocusMap, "基本农田");
                layers.Add(groupLayer);

                var tableName = GetShapeTableName(prms, "JBNTBHQ");
                var layer = CreateFillFeatureLayer(wfc, map, tableName, "基本农田保护区",
                    "#FFEDE8E6", "#FFC4BFBD", 1
                    , "", "", null, 10000);//, 30000, 150000);
                groupLayer.AddLayer(layer);
            }
            #endregion

            #region 其他地物
            {
                var groupLayer = new GroupLayer(map.FocusMap, "其他地物");
                layers.Add(groupLayer);

                var tableName = GetShapeTableName(prms, "MZDW");
                var layer = CreateFillFeatureLayer(wfc, map, tableName, "面状地物",
                    "#FFEFEBDB", "#FFC4BFBD", 1
                    , "", "DWMC", null, 8000);
                groupLayer.AddLayer(layer);

                tableName = GetShapeTableName(prms, "XZDW");
                layer = CreateLineFeatureLayer(wfc, map, tableName, "线状地物"
                    , "#FF9E9470", 1, null, "[DWMC]", null, 8000);
                groupLayer.AddLayer(layer);

                tableName = GetShapeTableName(prms, "DZDW");
                layer = CreatePointFeatureLayer(wfc, map, tableName, "点状地物"
                    , "#FFCCE1A0", "#FF728944", 6, null, "[DWMC]");
                layer.MaxVisibleScale = 8000;
                groupLayer.AddLayer(layer);
            }
            #endregion

            #region 地块类别
            {
                var groupLayer = new GroupLayer(map.FocusMap, "地块类别");
                groupLayer.IsExpanded = true;
                layers.Add(groupLayer);

                var tableName = GetShapeTableName(prms, "DK");
                if (tableName != null)
                {
                    var layer = CreateFillFeatureLayer(wfc, map, tableName, "承包地",
                        "#FFCCE1A0", "#FF728944", 1
                        , "[DKLB] = '10'", "[DKMC]", null, 8000);//, 30000, 150000);
                    groupLayer.AddLayer(layer);

                    layer = CreateFillFeatureLayer(wfc, map, tableName, "自留地",
                        "#D3FFBEFF", "#FF728944", 1
                        , "[DKLB] = '21'", "[DKMC]", null, 8000);//, 30000, 150000);
                    groupLayer.AddLayer(layer);

                    layer = CreateFillFeatureLayer(wfc, map, tableName, "其他集体土地",
                                      "#BED2FFFF", "#FF728944", 1
                                      , "[DKLB] = '99'", "[DKMC]", null, 8000);//, 30000, 150000);
                    groupLayer.AddLayer(layer);
                }
            }
            #endregion

            #region 界址数据
            {
                var groupLayer = new GroupLayer(map.FocusMap, "界址数据");
                layers.Add(groupLayer);

                var tableName = GetShapeTableName(prms, "JZX");
                if (tableName != null)
                {
                    var layer = CreateLineFeatureLayer(wfc, map, tableName, "界址线"
                        , "#FF9E9470", 1, null, null, null, 8000);
                    groupLayer.AddLayer(layer);
                }
                tableName = GetShapeTableName(prms, "JZD");
                if (tableName != null)
                {
                    var layer = CreatePointFeatureLayer(wfc, map,tableName, "界址点"
                     , "#FFCCE1A0", "#FF728944", 6);
                    layer.MaxVisibleScale = 8000;
                    groupLayer.AddLayer(layer);
                }
            }
            #endregion
            return layers;
        }


        private static IPolygon GetFullExtent(IFeatureLayer layer)
        {
            if (layer != null && layer.FeatureClass != null)
            {
                var env= layer.FeatureClass.GetFullExtent();
                return env==null?null: GeometryUtil.MakePolygon(env);
            }
            return null;
        }
        private static string GetShapeTableName(HJDataRootPath prm,string sPrefix)
        {
            string shpFile=null;
            if (sPrefix == "DK" || sPrefix == "JZD" || sPrefix == "JZX")
            {
                List<string> lst;
                if (prm.dicShp1.TryGetValue(sPrefix, out lst))
                {
                    shpFile = lst[0];
                }
            }
            else
            {
                if(!prm.dicShp.TryGetValue(sPrefix, out shpFile))
                {
                    return null;
                }
            }

            if(shpFile!=null)//prm.dicShp.TryGetValue(sPrefix,out shpFile))
            {
                int n = shpFile.LastIndexOf('/');
                int m = shpFile.LastIndexOf('\\');
                var tableName = shpFile.Substring(Math.Max(m, n) + 1);
                n = tableName.LastIndexOf('.');
                tableName = tableName.Substring(0, n);
                return tableName;
            }
            return null;
        }
        private static IFeatureLayer CreateLineFeatureLayer(IFeatureWorkspace wfc, MapControl map
                  , string tableName, string layerName, string lineColor, double lineWidth = 1, string where = null, string sLabelExpr = null
                  , double? MinVisibleScale = null, double? MaxVisibleScale = null
                  )
        {
            var symbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth) as ILineSymbol;

            var fl = new FeatureLayer(map.FocusMap);
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

        private static IFeatureLayer CreateFillFeatureLayer(IFeatureWorkspace wfc, MapControl map
            , string tableName, string layerName, string fillColor, string lineColor, double lineWidth = 1, string where = null, string sLabelExpr = null
            , double? MinVisibleScale = null, double? MaxVisibleScale = null
            )
        {
            var lineSymbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth) as ILineSymbol;
            var symbol = SymbolUtil.CreateSimpleFillSymbol(ColorUtil.ConvertFromString(fillColor), lineSymbol);

            var fl = new FeatureLayer(map.FocusMap);
            fl.FeatureClass = wfc.OpenFeatureClass(tableName);
            fl.Name = layerName;
            fl.Where = where;

            fl.MinVisibleScale = MinVisibleScale;
            fl.MaxVisibleScale = MaxVisibleScale;

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
        private static IFeatureLayer CreatePointFeatureLayer(IFeatureWorkspace wfc, MapControl map, string tableName, string layerName
            , string fillColor, string lineColor, double markerSize = 6
            , string where = null, string sLabelExpr = null)
        {
            var symbol = SymbolUtil.CreateSimpleMarkerSymbol(ColorUtil.ConvertFromString(fillColor),
                ColorUtil.ConvertFromString(lineColor), markerSize);
            var fl = new FeatureLayer(map.FocusMap);
            fl.FeatureClass = fl.FeatureClass = wfc.OpenFeatureClass(tableName);
            fl.Name = layerName;
            fl.Where = where;
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

        ///// <summary>
        ///// 初始化图层
        ///// </summary>
        ///// <returns></returns>
        //private static ILayer InitalizeLayer(string aliseName, string name, string tableName = "",
        //    eGeometryType geometryType = eGeometryType.eGeometryNull, 
        //    bool selectable = true, bool drawable = true, bool editable = true, 
        //    bool visible = true, int minScaler = 0, int maxScaler = 0,
        //    string groupName = "", string whereClause = "", string labelName = "")
        //{
        //    var layer = new FeatureLayer();
        //    layer.Name = name;
        //    layer.AliseName = aliseName;
        //    layer.TableName = tableName;
        //    layer.Selectable = selectable;
        //    layer.Drawable = drawable;
        //    layer.Editable = editable;
        //    layer.Visible = visible;
        //    layer.MinScaler = minScaler;
        //    layer.MaxScaler = maxScaler;
        //    layer.GeometryType = geometryType;
        //    layer.GroupName = groupName;
        //    layer.WhereClause = whereClause;
        //    layer.LabelName = labelName;
        //    return layer;
        //}

    }
}
