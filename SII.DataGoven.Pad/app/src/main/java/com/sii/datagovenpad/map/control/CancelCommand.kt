package com.sii.datagovenpad.map.control

import android.view.View
import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.carto.esriViewDrawPhase
import ime.sii.gis.controls.tool.IEditTool
import ime.sii.gis.controls.tool.SelectTool
import ime.sii.gis.geometry.GeometryType

class CancelCommand(private val tbCancel:ToolButton, val m:IMapHost):IMapCommand {
    init {
        tbCancel.setOnClickListener {
            val mapControl=m.getMapControl()
            val ct = mapControl.currentTool
            if (ct is IEditTool) {
                (ct as IEditTool).cancel()
                m.refillSelectedFeatures(false)
                if (m.mSelectedFeatures.size == 0) {
                    mapControl.firUpdateCommandUI()
                }
            } else if (ct is SelectTool) {
                mapControl.map.clearSelection()
                mapControl.map.partialRefresh(esriViewDrawPhase.esriViewGeoSelection.value(), null, null)
                m.refillSelectedFeatures()
            }
            mapControl.firUpdateCommandUI()
        }
    }

    override fun updateUI() {
        val mapControl=m.getMapControl()
        var editTool:IEditTool?=null
        if(mapControl.currentTool is IEditTool){
            editTool=mapControl.currentTool as IEditTool
        }
        val fEditTool = editTool!=null
        var fHasSketch=editTool!=null&&editTool.hasSketch()
        var fHasSelectedFeatures = false
        if (mapControl.currentTool is SelectTool) {
            if (m.mSelectedFeatures.size > 0) {
                fHasSelectedFeatures = true
            }
        }
        val tgtLayers=m.tgtLayers
        if(tgtLayers.size>0) {
            val fl = tgtLayers[0]
            when (fl.featureClass.shapeType) {
                GeometryType.esriGeometryPoint -> {
                    tbCancel.visibility = View.GONE
                }
                GeometryType.esriGeometryPolyline -> {
                    tbCancel.visibility = if (fEditTool || fHasSelectedFeatures) View.VISIBLE else View.GONE
                }
                GeometryType.esriGeometryPolygon -> {
                    tbCancel.visibility = if (fEditTool || fHasSelectedFeatures) View.VISIBLE else View.GONE
                }
            }
        }
        tbCancel.setEnable(fHasSketch||fHasSelectedFeatures)
    }
}