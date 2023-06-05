package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import ime.sii.gis.systemui.ITool

abstract class ToolCommandBase (override val btn: ToolButton, val m:IMapHost) :IMapTool{
    protected var mTool: ITool?=null

    override fun active() {
        if(mTool==null) mTool=initTool()
        m.getMapControl().currentTool=mTool
        mTool!!.activated()
    }
    override fun updateUI() {

    }

    override fun getTool(): ITool? {
        return mTool
    }
    protected abstract fun initTool(): ITool
}