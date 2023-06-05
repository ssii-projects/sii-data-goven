package com.sii.datagovenpad.map.control

import android.view.View
import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.controls.tool.IEditTool
import ime.sii.gis.geometry.GeometryType

class RedoCommand (private val tbRedo: ToolButton, val m:IMapHost) :IMapCommand{
    init {
        tbRedo.setOnClickListener{
            val mapControl=m.getMapControl()
            val ct=mapControl.currentTool
            if (ct is IEditTool) {
                if (ct.redo()) {
                    m.refillSelectedFeatures()
                    mapControl.firUpdateCommandUI()
                }
            }
        }
    }
    override fun updateUI(){
        val mapControl=m.getMapControl()
        val tgtLayers=m.tgtLayers
        var editTool:IEditTool?=null//
        if(mapControl.currentTool is IEditTool){
            editTool=mapControl.currentTool as IEditTool
        }
        val fEditTool = editTool!=null
        if(tgtLayers.size>0) {
            val fl = tgtLayers[0]
            when (fl.featureClass.shapeType) {
                GeometryType.esriGeometryPoint -> {
                    tbRedo.visibility = View.GONE
                }
                GeometryType.esriGeometryPolyline -> {
                    tbRedo.visibility = if (fEditTool) View.VISIBLE else View.GONE
                }
                GeometryType.esriGeometryPolygon -> {
                    tbRedo.visibility = if (fEditTool) View.VISIBLE else View.GONE
                }
            }
        }
        if(fEditTool) {
            tbRedo.setEnable(editTool!!.canRedo())
        }
    }
}