package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.controls.tool.MeasureAreaTool
import ime.sii.gis.controls.tool.MeasureLengthTool
import ime.sii.gis.systemui.ITool

//abstract class SimpleToolCommand(override val btn: ToolButton, val m:IMapHost) :IMapTool {
//    private var mTool: ITool?=null
//    override fun active() {
//        if(mTool==null) mTool=initTool()
//        m.getMapControl().currentTool=mTool
//    }
//    override fun getTool(): ITool?{
//        return mTool
//    }
//    override fun updateUI() {
//    }
//    protected abstract fun initTool():ITool
//}

class MeasureAreaCommand(override val btn: ToolButton, m: IMapHost) :ToolCommandBase(btn, m) {
    override fun initTool(): ITool {
        return MeasureAreaTool(m.getMapControl())
    }
}

class MeasureLengthCommand(override val btn: ToolButton, m:IMapHost) :ToolCommandBase(btn,m) {
    override fun initTool(): ITool {
        return MeasureLengthTool(m.getMapControl())
    }
}