package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.repos.SysInfoRepos
import com.sii.util.MathUtil
import ime.jts.geom.Polygon
import ime.sii.gis.geodatabase.IFeature

abstract class EditCommandBase (override val btn: ToolButton,m:IMapHost) :ToolCommandBase(btn,m){
    protected fun calcAndSetMjFields(it:IFeature){
        val g=it.geometry
        if(g is Polygon) {
            val scale = SysInfoRepos.loadInt(SysInfoRepos.KEY_DKMJSCALE)
            val area = g.area
            it.setFieldValue("SCMJ", MathUtil.round(area,scale))
            it.setFieldValue("SCMJM", MathUtil.round(area * 0.0015, scale))
        }
    }
}