package com.sii.datagovenpad.map.control

import android.view.View
import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.controls.tool.IEditTool
import ime.sii.gis.geometry.GeometryType

class SaveCommand(private val tbCommit:ToolButton, val m:IMapHost) :IMapCommand{
    init {
        tbCommit.setOnClickListener {
            val mc=m.getMapControl()
            val ct=mc.currentTool
            if (ct is IEditTool) {
                if (ct.submit()) {
                    m.refillSelectedFeatures()
                }
            }
            mc.firUpdateCommandUI()
        }
    }
    override fun updateUI(){//_tgtLayers:ArrayList<IFeatureLayer>){
        val mapControl=m.getMapControl()
        var editTool:IEditTool?=null//
        if(mapControl.currentTool is IEditTool){
            editTool=mapControl.currentTool as IEditTool
        }
        val fEditTool = editTool!=null
        var fHasSketch=editTool!=null&&editTool.hasSketch()
        if(m.tgtLayers.size>0) {
            val fl = m.tgtLayers[0]
            when (fl.featureClass.shapeType) {
                GeometryType.esriGeometryPoint -> {
                    tbCommit.visibility = View.GONE
                }
                GeometryType.esriGeometryPolyline -> {
                    tbCommit.visibility = if (fEditTool) View.VISIBLE else View.GONE
                }
                GeometryType.esriGeometryPolygon -> {
                    tbCommit.visibility = if (fEditTool) View.VISIBLE else View.GONE
                }
            }
        }
        tbCommit.setEnable(fHasSketch)
    }
}