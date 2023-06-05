package com.sii.datagovenpad.popup

import android.util.Pair
import android.view.View
import android.widget.AdapterView
import android.widget.ListView
import com.sii.datagovenpad.R
import com.sii.datagovenpad.adaptor.abslistview.CommonAdapter
import com.sii.datagovenpad.adaptor.abslistview.ViewHolder
import com.sii.datagovenpad.control.FormInfoTopBar
import ime.sii.gis.carto.IFeatureLayer
import ime.sii.gis.carto.ILayer
import ime.sii.gis.controls.MapControl
import ime.sii.gis.util.ImageUtil
import java.util.*

class LayerPopup : PopupBase() {
    /**
     * 图层选择POP
     */
    fun showPopup(anchor: View, mc: MapControl) {
        val context = mc.context
        val with = defWidth(context)
        val high = mc.height

        val view = View.inflate(context, R.layout.tucengpop, null)
        FormInfoTopBar(null,view,"图层管理")
        val wnd = createPopWindow(context, view, with, high, anchor)
        val tcList = view.findViewById(R.id.tuceng_listview) as ListView
        val layers = ArrayList<Pair<String, ILayer>>()
        val maps = mc.map
        layers.clear()
        for (i in 0 until maps.layerCount) {
            val layer = maps.getLayer(i)
            val layerName = layer.name
            layers.add(Pair(layerName, layer))
            //            LogUtils.e("tag",_layers.toString());
        }

        tcList.adapter = object : CommonAdapter<Pair<String, ILayer>>(context, R.layout.tucengitem, layers) {
            override fun convert(holder: ViewHolder, o: Pair<String, ILayer>, pos: Int) {
                holder.setText(R.id.item_tv, o.first)
                val iLayer = o.second
                holder.setChecked(R.id.item_cb, iLayer.visible)
                if (iLayer is IFeatureLayer) {
                    holder.setImageBitmap(R.id.item_im, ImageUtil.layerToImage(iLayer, 100, 100))
                } else {
                    holder.setImageResource(R.id.item_im, R.mipmap.biakuang)
                }
            }
        }

        tcList.onItemClickListener = AdapterView.OnItemClickListener { _, view, i, _ ->
            val holder = view.tag as ViewHolder
            val layer = layers[i].second
            val fVisible =!layer.visible
            holder.setChecked(R.id.item_cb,fVisible)
            layer.visible = fVisible
            mc.refresh()
        }

        wnd.setTouchInterceptor { _, _ -> false }
        // tuCengPopupWindow.setOnDismissListener { view_img.setVisibility(View.GONE) }
    }


}

