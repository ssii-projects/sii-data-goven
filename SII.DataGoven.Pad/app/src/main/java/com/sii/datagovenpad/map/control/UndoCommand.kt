package com.sii.datagovenpad.map.control

import android.view.View
import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.controls.tool.IEditTool
import ime.sii.gis.geometry.GeometryType

class UndoCommand (private val tbUndo: ToolButton, val m:IMapHost) :IMapCommand{
    init {
        tbUndo.setOnClickListener{
            val mapControl=m.getMapControl()
            val ct=mapControl.currentTool
            if (ct is IEditTool) {
                if (ct.undo()) {
                    m.refillSelectedFeatures()
                    mapControl.firUpdateCommandUI()
                }
            }
        }
    }
    override fun updateUI(){
        val mapControl=m.getMapControl()
        val tgtLayers=m.tgtLayers
        var editTool:IEditTool?=null
        if(mapControl.currentTool is IEditTool){
            editTool=mapControl.currentTool as IEditTool
        }
        val fEditTool = editTool!=null
        if(tgtLayers.size>0) {
            val fl = tgtLayers[0]
            when (fl.featureClass.shapeType) {
                GeometryType.esriGeometryPoint -> {
                    tbUndo.visibility = View.GONE
                }
                GeometryType.esriGeometryPolyline -> {
                    tbUndo.visibility = if (fEditTool) View.VISIBLE else View.GONE
                }
                GeometryType.esriGeometryPolygon -> {
                    tbUndo.visibility = if (fEditTool) View.VISIBLE else View.GONE
                }
            }
        }
        if(fEditTool) {
            tbUndo.setEnable(editTool!!.canUndo())
        }
    }
}