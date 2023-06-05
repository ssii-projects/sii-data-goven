package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.controls.tool.IEditTool
import ime.sii.gis.controls.tool.MeasureAreaTool
import ime.sii.gis.controls.tool.MeasureLengthTool

class ClearMeasureCommand (private val tbClearMeasure: ToolButton, val m:IMapHost):IMapCommand {
    init {
        tbClearMeasure.setOnClickListener {
            val ct = m.getMapControl().currentTool
            if (ct is MeasureAreaTool || ct is MeasureLengthTool) {
                (ct as IEditTool).cancel()
            }
        }
    }

    override fun updateUI() {
        val mapControl=m.getMapControl()
        var editTool:IEditTool?=null
        if(mapControl.currentTool is IEditTool){
            editTool=mapControl.currentTool as IEditTool
        }
        var fEnableClearBtn=false
        if(editTool is MeasureLengthTool||editTool is MeasureAreaTool){
            fEnableClearBtn=editTool!!.hasSketch()
        }
        tbClearMeasure.setEnable(fEnableClearBtn)
    }
}