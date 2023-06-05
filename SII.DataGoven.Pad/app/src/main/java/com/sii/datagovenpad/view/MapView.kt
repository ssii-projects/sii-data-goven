package com.sii.datagovenpad.view

import com.sii.datagovenpad.MyGlobal
import ime.sii.gis.controls.MapControl
import ime.sii.gis.geodatabase.IFeatureWorkspace
import ime.sii.gis.util.MapUtil


class MapView(val mapControl: MapControl){
    fun init(db:IFeatureWorkspace) {
        MapDocument.init(db,mapControl)
//        updateLayerDefinition()
    }

//    /**
//     * 修改图层的过滤条件
//     * @param bm:xzqdm(行政区代码)
//     */
//    fun updateLayerDefinition(bm:String?=null){
//    }

    /**
     * 更新数据源
     */
    fun refreshDataSource() {
        //更新数据
        val map = mapControl.map//获取图层
        map.clearSelection()
        var iFeatureLayers= MapUtil.enumFeatureLayers(map,false)
        val db= MyGlobal.mainDB!!
        for (fl in iFeatureLayers) {
            val ofc = fl.featureClass//获取老的layer
            val nfc = db.OpenFeatureClass(ofc.tableName, ofc.shapeType)//打开新的数据库
            fl.featureClass = nfc//设置新的layer
        }
    }
    fun zoomToFullExtent(){
        val map=mapControl.map
        if(map.layerCount>0) {
            map.extent = MapDocument.mLayerDk.extent
            map.refresh()
        }
    }
}
