package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.systemui.ITool

interface IMapCommand {
    fun updateUI()
}

interface IMapTool:IMapCommand{
    val btn:ToolButton
    fun active()
    fun getTool():ITool?
}

//abstract class MapToolCommandBase(override val btn: ToolButton, val m:IMapHost) :IMapTool {
////    protected lateinit var tool:ITool;
//    override fun updateUI() {
//        btn.setCheck(tool==m.getMapControl().currentTool)
//    }
//    protected
//}