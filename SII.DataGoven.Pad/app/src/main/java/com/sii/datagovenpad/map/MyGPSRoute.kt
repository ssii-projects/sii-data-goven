package com.sii.datagovenpad.map

//import com.ssii.siidistrictpad.App
//import com.ssii.siidistrictpad.util.MyLayerUtil
//import com.sii.datagovenpad.App
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.view.MyLayers
import ime.sii.gis.controls.MapControl
import ime.sii.gis.geodatabase.IFeatureClass
import ime.sii.gis.geometry.GeometryType
import ime.sii.gis.gps.GPSRoute


/**
 * Created by Administrator on 2017/10/16.
 */

class MyGPSRoute(mc: MapControl) : GPSRoute(mc) {

    private var _fc: IFeatureClass? = null
    fun save() {
        val ls = super.getRoute(_map.getMap().getSpatialReference())
        val startTime = super.getStartTime()
        val stopTime = super.getStopTime()
        if (_fc == null) {
            val db= MyGlobal.mainDB!!
            _fc =db.OpenFeatureClass("GPSX", GeometryType.esriGeometryPolyline)
        }
        MyLayers.createGpsLine(_fc!!, ls, startTime, stopTime)
    }

}
