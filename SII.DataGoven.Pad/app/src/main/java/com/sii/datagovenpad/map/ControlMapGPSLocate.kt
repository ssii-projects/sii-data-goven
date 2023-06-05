package com.sii.datagovenpad.map

import android.graphics.drawable.Drawable
import android.view.View
import android.widget.Toast
import com.sii.location.GpsManager
import com.sii.util.GPSUtil
import ime.sii.gis.controls.ControlLocationMarker
import ime.sii.gis.controls.MapControl

/**
 * 地图控件上摆放的导航到GPS当前位置的控件
 *
 * @author yxm
 */
class ControlMapGPSLocate(mapControl: MapControl, icon: View, locationMarker: Drawable) {
    init {
        if (!GpsManager.instance.isGpsProviderEnabled) {
            GPSUtil.openGpsService(mapControl.context)
        }
        icon.setOnClickListener {
            if (!GpsManager.instance.isGpsProviderEnabled) {
                GPSUtil.openGpsService(mapControl.context)
            } else {
                try {
                    GPSUtil.panToGPSPoint(mapControl)
                } catch (ex: java.lang.Exception) {
                    mapControl.map.showToast(ex.message, Toast.LENGTH_LONG)
                }

            }
        }
        mapControl.setLocationMarker(locationMarker)
        ControlLocationMarker(mapControl)
    }
}