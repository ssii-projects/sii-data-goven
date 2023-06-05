package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import com.sii.functional.Action
import ime.sii.gis.controls.tool.SelectTool

open class SelectToolCommand (override val btn: ToolButton, m:IMapHost) :ToolCommandBase(btn,m) {
//    private val mTool:SelectTool by lazy { initTool() }
    override fun active() {
        super.active()
        (mTool as SelectTool).selectedLayers = m.tgtLayers
    }

    override fun initTool():SelectTool{
        val tool=SelectTool(m.getMapControl())
        tool.selectModel = SelectTool.ESelectionMode.MULTI
        tool.onFeatureSelected = Action { m.refillSelectedFeatures() }
        return tool
    }
}