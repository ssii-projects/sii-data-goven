using Captain.GIS;
using Captain.NetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Captain.FrameApp
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            LoadData();
        }

        public void LoadData()
        {
            var pWS = OracleWorkspaceFactory.Instance.OpenWorkspace("DATA SOURCE =ORCL33;USER ID=REGCERT;PASSWORD = 123456");
           // Invoke(() =>
            {
                var mc=map.MapControl;
                var layers = DefaultLayers.CreateLayers(pWS, mc);
                foreach (var layer in layers)
                {
                    mc.Layers.AddLayer(layer);
                }
            };
        }
    }

    class DefaultLayers
    {
        /// <summary>
        /// 创建本地图层表映射数据
        /// </summary>
        public static List<ILayer> CreateLayers(IFeatureWorkspace wfc, MapControl map)
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
            #region 定位基础
            {
                var groupLayer = new GroupLayer(map, "定位基础");
                var kzdLayer = CreatePointFeatureLayer(wfc, map, "DLXX_KZD", "控制点"
                    , "#FFCCE1A0", "#FF728944", 6);
                groupLayer.AddLayer(kzdLayer);
                layers.Add(groupLayer);
            }
            #endregion
            #region 管辖区域
            {
                var groupLayer = new GroupLayer(map, "管辖区域");
                layers.Add(groupLayer);

                var layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "乡级区域",
                    "#FFEFEBDB", "#FFC4BFBD", 1
                    , "JB = 3", "[MC]");//, 30000, 150000);
                groupLayer.AddLayer(layer);

                layer = CreateFillFeatureLayer(wfc, map, "DLXX_XZDY", "村级区域",
                    "#FFEAE0BD", "#FF9E9470", 1
                    , "JB = 2", "[MC]");//, 30000, 150000);
                groupLayer.AddLayer(layer);

            }
            #endregion

            #region 地块类别
            {
                var groupLayer = new GroupLayer(map, "地块类别");
                layers.Add(groupLayer);

                var layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "承包地",
                    "#FFCCE1A0", "#FF728944", 1
                    , "DKLB = '10'");//, 30000, 150000);
                groupLayer.AddLayer(layer);

                layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "自留地",
                    "#D3FFBEFF", "#FF728944", 1
                    , "DKLB = '21'", "[CBFMC]");//, 30000, 150000);
                groupLayer.AddLayer(layer);

                layer = CreateFillFeatureLayer(wfc, map, "DLXX_DK", "其他集体土地",
                                  "#BED2FFFF", "#FF728944", 1
                                  , "DKLB = '99'", "[CBFMC]");//, 30000, 150000);
                groupLayer.AddLayer(layer);
            }
            #endregion

            #region 界址数据
            {
                var groupLayer = new GroupLayer(map, "界址数据");
                layers.Add(groupLayer);
                var layer = CreateLineFeatureLayer(wfc, map, "DLXX_DK_JZX", "界址线"
                    , "#FF9E9470");
                groupLayer.AddLayer(layer);

                layer = CreatePointFeatureLayer(wfc, map, "DLXX_DK_JZD", "界址点"
                    , "#FFCCE1A0", "#FF728944", 6);
                groupLayer.AddLayer(layer);
            }
            #endregion

            //layer = InitalizeLayer("管辖区域", MapLayerName.ZoneGroup_Layer);//管辖区域
            //layers.Add(layer);
            //layer = InitalizeLayer("省级行政区", MapLayerName.Zone_Province_Layer, MapLayerName.Zone_LayerName, eGeometryType.Polygon, false, false, false, true, 1200000, 5000000, MapLayerName.ZoneGroup_Layer, "JB = 6", "MC");//省级行政区
            //layers.Add(layer);
            //layer = InitalizeLayer("市级行政区", MapLayerName.Zone_City_Layer, MapLayerName.Zone_LayerName, eGeometryType.Polygon, false, false, false, true, 1000000, 2000000, MapLayerName.ZoneGroup_Layer, "JB = 5", "MC");//市级行政区
            //layers.Add(layer);
            //layer = InitalizeLayer("县级行政区", MapLayerName.Zone_County_Layer, MapLayerName.Zone_LayerName, eGeometryType.Polygon, false, false, false, true, 100000, 1000000, MapLayerName.ZoneGroup_Layer, "JB = 4", "MC");//县级行政区
            //layers.Add(layer);
            //layer = InitalizeLayer("乡级区域", MapLayerName.Zone_Town_Layer, MapLayerName.Zone_LayerName, eGeometryType.Polygon, false, false, false, true, 30000, 150000, MapLayerName.ZoneGroup_Layer, "JB = 3", "MC");//乡级区域
            //layers.Add(layer);
            //layer = InitalizeLayer("村级区域", MapLayerName.Zone_Village_Layer, MapLayerName.Zone_LayerName, eGeometryType.Polygon, false, false, false, true, 10000, 60000, MapLayerName.ZoneGroup_Layer, "JB = 2", "MC");//村级区域
            //layers.Add(layer);
            //layer = InitalizeLayer("组级区域", MapLayerName.Zone_Group_Layer, MapLayerName.Zone_LayerName, eGeometryType.Polygon, false, false, false, true, 3000, 20000, MapLayerName.ZoneGroup_Layer, "JB = 1", "MC");//组级区域
            //layers.Add(layer);
            //layer = InitalizeLayer("区域界线", MapLayerName.RegionLineGroup_Layer);//区域界线
            //layers.Add(layer);
            //layer = InitalizeLayer("区域界线", MapLayerName.RegionBoundaryLine_Layer, MapLayerName.Region_LayerName, eGeometryType.Polyline, true, false, false, true, 3000, 100000, MapLayerName.RegionLineGroup_Layer);//区域界线
            //layers.Add(layer);
            //layer = InitalizeLayer("基本农田", MapLayerName.FarmerLandGroup_Layer);//基本农田
            //layers.Add(layer);
            //layer = InitalizeLayer("基本农田保护区", MapLayerName.FarmerLand_Layer, MapLayerName.FarmerLand_LayerName, eGeometryType.Polygon, true, false, false, true, 0, 10000, MapLayerName.FarmerLandGroup_Layer);//基本农田保护区
            //layers.Add(layer);
            //layer = InitalizeLayer("其他地物", MapLayerName.OtherGroundGroup_Layer);//其他地物
            //layers.Add(layer);
            //layer = InitalizeLayer("面状地物", MapLayerName.PlaneGroundGroup_Layer, MapLayerName.PlaneGround_LayerName, eGeometryType.Polygon, true, true, true, true, 0, 8000, MapLayerName.OtherGroundGroup_Layer, "", "DWMC");//面状地物
            //layers.Add(layer);
            //layer = InitalizeLayer("线状地物", MapLayerName.LineGroundGroup_Layer, MapLayerName.LineGround_LayerName, eGeometryType.Polyline, true, true, true, true, 0, 8000, MapLayerName.OtherGroundGroup_Layer, "", "DWMC");//线状地物
            //layers.Add(layer);
            //layer = InitalizeLayer("点状地物", MapLayerName.DotGroundGroup_Layer, MapLayerName.DotGround_LayerName, eGeometryType.Point, true, true, true, true, 0, 8000, MapLayerName.OtherGroundGroup_Layer, "", "DWMC");//点状地物
            //layers.Add(layer);
            //layer = InitalizeLayer("地块类别", MapLayerName.LandCatalogGroup_Layer);//地块类别
            //layers.Add(layer);
            //layer = InitalizeLayer("承包地", MapLayerName.ContractLand_Layer, MapLayerName.LandCatalog_LayerName, eGeometryType.Polygon, true, true, true, true, 0, 8000, MapLayerName.LandCatalogGroup_Layer, "DKLB = \"10\"", "(CBFMC!=null&&CBFMC.IndexOf('/')>=0)?\"共有地块\":CBFMC");//承包地
            //layers.Add(layer);
            //layer = InitalizeLayer("自留地", MapLayerName.PrivateLand_Layer, MapLayerName.LandCatalog_LayerName, eGeometryType.Polygon, true, true, true, true, 0, 8000, MapLayerName.LandCatalogGroup_Layer, "DKLB = \"21\"", "CBFMC");//自留地
            //layers.Add(layer);
            //layer = InitalizeLayer("机动地", MapLayerName.ManeuverLand_Layer, MapLayerName.LandCatalog_LayerName, eGeometryType.Polygon, true, true, true, true, 0, 8000, MapLayerName.LandCatalogGroup_Layer, "DKLB = \"22\"", "CBFMC");//机动地
            //layers.Add(layer);
            //layer = InitalizeLayer("开荒地", MapLayerName.WasteLand_Layer, MapLayerName.LandCatalog_LayerName, eGeometryType.Polygon, true, true, true, true, 0, 8000, MapLayerName.LandCatalogGroup_Layer, "DKLB = \"23\"", "CBFMC");//开荒地
            //layers.Add(layer);
            //layer = InitalizeLayer("其他集体土地", MapLayerName.OtherLand_Layer, MapLayerName.LandCatalog_LayerName, eGeometryType.Polygon, true, true, true, true, 0, 8000, MapLayerName.LandCatalogGroup_Layer, "DKLB = \"99\"", "CBFMC");//其他集体土地
            //layers.Add(layer);
            //layer = InitalizeLayer("界址数据", MapLayerName.BoundaryGroup_Layer, "", eGeometryType.Unknown, false, false, false, false);//界址数据
            //layers.Add(layer);
            //layer = InitalizeLayer("界址线", MapLayerName.BoundaryLine_Layer, MapLayerName.BoundaryLine_LayerName, eGeometryType.Polyline, true, false, false, true, 0, 8000, MapLayerName.BoundaryGroup_Layer);//界址线
            //layers.Add(layer);
            //layer = InitalizeLayer("界址点", MapLayerName.BoundaryPoint_Layer, MapLayerName.BoundaryPoint_LayerName, eGeometryType.Point, true, false, false, true, 0, 8000, MapLayerName.BoundaryGroup_Layer);//界址点
            //layers.Add(layer);
            return layers;
        }

        private static IFeatureLayer CreateLineFeatureLayer(IFeatureWorkspace wfc, MapControl map
                  , string tableName, string layerName, string lineColor, double lineWidth = 1, string where = null, string sLabelExpr = null
                  , double? MinVisibleScale = null, double? MaxVisibleScale = null
                  )
        {
            var symbol = SymbolUtil.CreateSimpleLineSymbol(ColorUtil.ConvertFromString(lineColor), lineWidth) as ILineSymbol;

            var fl = new FeatureLayer(map);
            fl.FeatureClass = wfc.OpenFeatureClass(tableName);
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

            var fl = new FeatureLayer(map);
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
            var fl = new FeatureLayer(map);
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


    }
}
