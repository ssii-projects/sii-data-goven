package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.util.IRowUtil
import com.sii.functional.Action
import ime.jts.geom.Polygon
import ime.sii.gis.controls.tool.CutFeatureTool
import ime.sii.gis.geodatabase.IFeature
import java.lang.Exception

class CutAngleCommand(override val btn: ToolButton, m:IMapHost) :EditCommandBase(btn,m) {
    override fun initTool(): CutFeatureTool {
        val tool= CutFeatureTool(m.getMapControl())
        tool.onPreSubmit= Action {
            it.cancel=true
            val features=ArrayList<IFeature>();
            if(!tool.tryGetCutFeatures(it.geometry,it.line,features)) return@Action
            val maxAreaFeature:IFeature=features.get(0)
            var maxArea=(maxAreaFeature.geometry as Polygon).area
            for(ft in features) {
                if(ft!=maxAreaFeature){
                    val g=ft.geometry;
                    if(g is Polygon && g.area>maxArea){
                        maxAreaFeature.geometry=g
                        maxArea=g.area
                    }
                }
            }
            val ft=maxAreaFeature
            val o = ft.getFieldValue("DKBM")
            if (o != null && o.toString().trim().isNotEmpty()) {
                val value=ft.getFieldValue("BGLX");
                if(value==null||value.toString().trim().isEmpty()) {
                    ft.setFieldValue("BGLX", "图形变更")
                }
            }
            IRowUtil.setNow(ft,"XGSJ")
            calcAndSetMjFields(ft)
            val fc=tool.targetLayer.featureClass
            val db=fc.workspace
            try {
                db.beginTransaction()
                fc.updateFeature(ft,true)
                m.getMapControl().refresh()
                db.setTransactionSuccessful()
                tool.cancel()
            }catch (e:Exception){

            }
            db.endTransaction()
        }
        return tool
    }
}