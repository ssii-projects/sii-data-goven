package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.systemui.ITool

class PanCommand(override val btn: ToolButton, val m:IMapHost) :IMapTool {
    private val mTool:ITool by lazy {  m.getMapControl().currentTool}
    override fun getTool(): ITool? {
        return mTool
    }

    override fun active() {
        m.getMapControl().currentTool = mTool
    }

    override fun updateUI() {

    }
}