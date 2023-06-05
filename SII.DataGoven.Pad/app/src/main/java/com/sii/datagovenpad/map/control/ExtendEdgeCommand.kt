package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.IRowUtil
import com.sii.functional.Action
import ime.jts.geoapi.IGeometry
import ime.jts.geom.Geometry
import ime.jts.geom.Polygon
import ime.sii.gis.controls.tool.CutFeatureTool
import ime.sii.gis.topo.AutoCompletePolygon
import ime.sii.gis.util.GeometryUtil


class ExtendEdgeCommand(override val btn: ToolButton, m:IMapHost) :EditCommandBase(btn,m) {
    override fun initTool(): CutFeatureTool {
        val tool= CutFeatureTool(m.getMapControl())
        tool.setShowCutArea(false)
        tool.onPreSubmit= Action {
            it.cancel=true
            val inputPolygons=ArrayList<IGeometry>()
            inputPolygons.add(it.geometry)
            val created = ArrayList<IGeometry>()
            val modified =HashMap<IGeometry, IGeometry>()
            AutoCompletePolygon.build(inputPolygons, it.line, created, modified)
            if (created.isNotEmpty()) {
                try {
                    var g = created[0] as Geometry
                    g = GeometryUtil.removeRepeatCoords(g,3)

                    for (i in 1 until created.size) {
                        g = g.union(created[i] as Geometry)
                    }
                    for (g1 in modified.values) {
                        var g2=g1 as Geometry;
                        g2 = GeometryUtil.removeRepeatCoords(g2,3)
                        g = g.union(g2)
                    }
                    if (g is Polygon) {
                        g = GeometryUtil.removeInvalidHoles(g, 5.0);
                    }
                    save(tool, g)
                }catch (e:java.lang.Exception){
                    AlertUtil.alert(m.getMapControl().context,e.message)
                }
            }
        }
        return tool
    }
    private fun save(tool:CutFeatureTool,g:Geometry){
        val fc=tool.targetLayer.featureClass
        val ft=fc.getFeatureByOid(tool.editFeature.oid)
        ft.geometry=g
        IRowUtil.setNow(ft,"XGSJ")
        calcAndSetMjFields(ft)
        ft.setFieldValue("BGLX","图形变更")
        val db=fc.workspace
        try {
            db.beginTransaction()
            fc.updateFeature(ft,true)
            m.getMapControl().refresh()
            db.setTransactionSuccessful()
            tool.cancel()
        }catch (e: Exception){

        }
        db.endTransaction()
    }
}