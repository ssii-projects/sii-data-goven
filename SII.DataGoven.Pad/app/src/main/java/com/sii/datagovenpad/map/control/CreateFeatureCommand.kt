package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.prop.PropViewDk
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.IRowUtil
import com.sii.functional.Action
import ime.sii.gis.controls.tool.CreateFeatureTool
import ime.sii.gis.geodatabase.IFeature
import ime.sii.gis.systemui.ITool

class CreateFeatureCommand(override val btn: ToolButton, m:IMapHost) :EditCommandBase(btn,m){
    private fun setFeatureDefValues(it:IFeature){
        it.setFieldValue("BGLX","新增")
        IRowUtil.setNow(it,"XGSJ")
        it.setFieldValue("FBFDM",findFbfBM())
        it.setFieldValue("DJZT",0)
        calcAndSetMjFields(it)
    }
    private fun findFbfBM(): String? {
        var fbfbm:String?=null
        var n=0.0
        val db=MyGlobal.mainDB!!
        val sql="select FBFDM,count(*) from VEC_SURVEY_DK  where FBFDM is not null  group by FBFDM"
        db.queryCallback(sql){
            val s=it.getString(0)
            val cnt=it.getDouble(1)
            if(fbfbm==null||cnt>n){
                fbfbm=s
                n=cnt
            }
        }
        return fbfbm
    }
    override  fun initTool():ITool{
        val mapControl=m.getMapControl()
        val tool = CreateFeatureTool(mapControl)
        tool.mAutoSave=false
        tool.onPreCreateFeature.addListener(Action {
            try {
                setFeatureDefValues(it)
                m.showProperty(it,null,tool)
            }catch (e:Exception) {
                AlertUtil.alert(mapControl.context,e.message)
            }
        })
        return tool
    }
}