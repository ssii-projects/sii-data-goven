package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.util.IRowUtil
import com.sii.functional.Action
import ime.sii.gis.controls.tool.ModifyFeatureTool
import ime.sii.gis.geodatabase.IFeature

class ModifyFeatureCommand(override val btn: ToolButton, m:IMapHost) :EditCommandBase(btn,m){
    override fun initTool():ModifyFeatureTool{
        val tool=ModifyFeatureTool(m.getMapControl())
        tool.onPreSubmit=Action{
            setFeatureDefValues(it.feature)
        }
        return tool
    }
    private fun setFeatureDefValues(it: IFeature){
        it.setFieldValue("BGLX","图形变更")
        IRowUtil.setNow(it,"XGSJ")
        calcAndSetMjFields(it)
    }
}