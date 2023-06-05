package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.control.ToolButton
import com.sii.functional.Action
import ime.sii.gis.controls.tool.SelectTool

class IdentifyCommand(override val btn: ToolButton, m:IMapHost) :SelectToolCommand(btn,m){
    override fun initTool():SelectTool{
        val tool = SelectTool(m.getMapControl())
        tool.selectModel = SelectTool.ESelectionMode.SINGLE
        tool.onFeatureSelected= Action {
            m.refillSelectedFeatures()
            if(m.mSelectedFeatures.size>0) {
                val fi=m.mSelectedFeatures[0]
                val feat=fi.featureLayer.featureClass.getFeatureByOid(fi.oid)
                m.showProperty(feat,fi.featureLayer)
            }else{
                MyGlobal.mainActivity.showProperty(null,null,null)// tryHideProperty()
            }
        }
        return tool
    }
}