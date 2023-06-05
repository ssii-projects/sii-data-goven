package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.util.IRowUtil
import com.sii.functional.Action
import ime.sii.gis.controls.tool.CutFeatureTool

class CutCommand(override val btn: ToolButton, m:IMapHost) :EditCommandBase(btn,m) {
    override fun initTool():CutFeatureTool{
        val tool=CutFeatureTool(m.getMapControl())
        tool.onPreSave=Action {
            for(ft in it.features) {
                val o = ft.getFieldValue("DKBM")
                if (o != null && o.toString().trim().isNotEmpty()) {
                    ft.setFieldValue("BGLX", "分割")
                }
                IRowUtil.setNow(ft,"XGSJ")
                calcAndSetMjFields(ft)
            }
        }
        return tool
    }
}