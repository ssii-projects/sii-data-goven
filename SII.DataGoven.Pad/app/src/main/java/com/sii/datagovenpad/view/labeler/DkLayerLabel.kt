package com.sii.datagovenpad.view.labeler

import android.graphics.Color
import com.sii.datagovenpad.entities.DKFields
import ime.jts.geoapi.IEnvelope
import ime.jts.geoapi.IPoint
import ime.jts.geom.Geometry
import ime.sii.gis.carto.SimpleFeatureLabel
import ime.sii.gis.display.IDisplay
import ime.sii.gis.display.ISymbol
import ime.sii.gis.display.ITextSymbol
import ime.sii.gis.display.symbol.TextSymbol
import ime.sii.gis.geodatabase.IFeature
import ime.sii.gis.geodatabase.IQueryFilter
import ime.sii.gis.system.esriDrawPhase
import java.util.ArrayList


/**
 * 地块标注
 *地块上默认标注显示承包方名称；
 * @author yanxm
 */
class DkLayerLabel : SimpleFeatureLabel() {
    override fun prepareFilter(drawPhase: esriDrawPhase, qf: IQueryFilter) {
        //if (drawPhase == esriDrawPhase.esriDPSelection) {
        qf.AddField(_labelField)
        //}
    }
//
//    fun draw(
//        drawPhase: esriDrawPhase?, pIDisplay: IDisplay?,
//        pIFeature: IFeature
//    ) {
//        val o=pIFeature.getFieldValue(_labelField)
//        if(o==null) return
//        val cunming: String =o.toString()
////        val cunming: String = pIFeature.getString(
////            pIFeature.fields.findField(_labelField)
////        )
//        val ptLabel: IPoint = (pIFeature.geometry as Geometry)
//            .centroid
//        val ts = _labelSymbol as ITextSymbol
//        ts.text = cunming
//        _labelSymbol.Draw(pIDisplay, ptLabel)
//    }

    init
    {
        // super("shape_area",symbol);
        _labelField = DKFields.cbfmc
        val ts: ITextSymbol = TextSymbol()
        _labelSymbol = ts as ISymbol
        ts.size = 32f
        ts.color = Color.WHITE
        (ts as ISymbol).paint.setShadowLayer(5f, 3f, 3f, -0x1000000)
    }
}
