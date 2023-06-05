package com.sii.datagovenpad.view


import android.graphics.Color
import com.sii.database.DataSourceException
import com.sii.datagovenpad.AppDataPath
import com.sii.datagovenpad.entities.DKFields
import com.sii.datagovenpad.entities.XzdyFields
import com.sii.util.FileUtil
import ime.jts.geom.LineString
import ime.sii.gis.carto.*

import ime.sii.gis.controls.MapControl
import ime.sii.gis.geodatabase.Feature
import ime.sii.gis.geodatabase.IFeatureClass
import ime.sii.gis.geodatabase.IFeatureWorkspace
import ime.sii.gis.util.CreateLayerUtil
import ime.sii.gis.util.FeatureLayerItem
import ime.sii.gis.util.FeatureLayerUtil
import ime.sii.gis.util.SymbolUtil

object MapDocument{

    lateinit var mRasterLayer: GroupLayer
    /**
     *原地块图层
     */
    lateinit var mLayerDk: IFeatureLayer
//    lateinit var mLayerZu:IFeatureLayer


    /**
     * 初始化地图文档
     * 1.设置地图最小和最大的显示比例；
     * 2.添加图层；
     * 3.设置地图初始显示范围；
     */
    fun init(mainDB: IFeatureWorkspace,mapControl: MapControl)
    {
        val map = mapControl.map

        mLayerDk = MyLayers.createDkLayer(mainDB)
        var env = mLayerDk.extent
        map.extent = env
        mRasterLayer = initRasterLayer(map)

        map.addLayer(mRasterLayer)

        val mLayerZu=MyLayers.createZuLayer(mainDB)
        map.addLayer(mLayerZu)
        map.addLayer(mLayerDk)

        val dcdk=MyLayers.createDcDkLayer(mainDB)
        map.addLayer(dcdk)
    }

    private fun initRasterLayer(map: IMap): GroupLayer {
        val gl = GroupLayer(map, "影像图")
        val rdbPath=AppDataPath.rdbPath;
        if (rdbPath != null) {
            FileUtil.folderScan(rdbPath, {
                if(it.name.endsWith((".rdb"))){
                    val rl = RasterLayer();
                    rl.CreateFromRdb(it.path)
                    gl.Add(rl)
                }
            },false)
        }
        return gl
    }
}

object MyLayers {
    /**
     * 地块图层
     *
     * @param db
     * @return
     */
    fun createDkLayer(db: IFeatureWorkspace): IFeatureLayer {
        val fli = FeatureLayerItem(null,"VEC_SURVEY_DK", Color.BLACK, 3, -1.0)
        fli.fillColor = Color.parseColor("#1A00FFFF")
        fli.alpha = 80
        val layer = CreateLayerUtil.CreateAreaFeatureLayer(db, fli,"调查地块")

        val labelSymbol= SymbolUtil.makeTextSymbol(Color.WHITE,32f)
        SymbolUtil.setSymbolShadow(labelSymbol,-0x1000000,5f,3f,3f)
        FeatureLayerUtil.setLabel(layer,labelSymbol, DKFields.cbfmc)
        return layer
    }

    /**
     * 地块图层
     *
     * @param db
     * @return
     */
    fun createDcDkLayer(db: IFeatureWorkspace): IFeatureLayer {
        val fli = FeatureLayerItem(null,
            "VEC_SURVEY_DK", Color.BLACK, // Color.GREEN,
            3, -1.0)
        fli.fillColor = Color.parseColor("#1A000059")
        fli.alpha = 80
        val layer = CreateLayerUtil.CreateAreaFeatureLayer(db, fli,"变更标记")
        layer.definitionExpression = "XGSJ is not null"
        return layer
    }

    /**
     * 创建组图层
     * @param db
     * @return
     */
    fun createZuLayer(db: IFeatureWorkspace): IFeatureLayer {
        val fli = FeatureLayerItem(null, XzdyFields.TB_NAME, Color.YELLOW,3, -1.0)
        fli.fillColor = null// Color.MAGENTA;
        val layer = CreateLayerUtil.CreateAreaFeatureLayer(db, fli,"村界")
        val lblSbl= SymbolUtil.makeTextSymbol(Color.BLACK,48f);
        SymbolUtil.setSymbolShadow(lblSbl, Color.WHITE,5f,3f,3f)
        FeatureLayerUtil.setLabel(layer,lblSbl, XzdyFields.MC)
        layer.definitionExpression="JB=1"
        return layer
    }

    /**
     * 插入一条GPS轨迹
     *
     * @param fc
     * @param ls
     * @param startTime
     * @param stopTime
     */
    fun createGpsLine(fc: IFeatureClass, ls: LineString?, startTime: Double, stopTime: Double) {
        if (ls == null) {
            return
        }
        try {
            val feature = Feature(fc.fields, 0, ls)
            feature.setFieldValue("KSSJ", startTime.toInt())
            feature.setFieldValue("JSSJ", stopTime.toInt())
            fc.addFeature(feature)
        } catch (e: DataSourceException) {
            System.err.print(e)
        }

    }

}