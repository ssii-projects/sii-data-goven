package com.sii.datagovenpad.popup

import android.view.View
import android.widget.ListView
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.ProjectInfo
import com.sii.datagovenpad.R
import com.sii.datagovenpad.adaptor.abslistview.CommonAdapter
import com.sii.datagovenpad.adaptor.abslistview.ViewHolder
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.view.MapView

class QuyuPopup : PopupBase() {
    fun showPopup(anchor: View, mv: MapView) {
        val mc = mv.mapControl
        val context = mc.context
        val with = defWidth(context)
        val high = mc.height
        val sjyView = View.inflate(context, R.layout.quyu_pop, null)
        FormInfoTopBar(null,sjyView,"区域选择")
        val wnd = createPopWindow(context, sjyView, with, high, anchor)

        val sjy = sjyView.findViewById<ListView>(R.id.quyu_listview)// as ListView
        sjy.isNestedScrollingEnabled = false

        var sss =MyGlobal.allProjects


        sjy.adapter = object : CommonAdapter<ProjectInfo>(context, R.layout.quyu_items, sss) {
            override fun convert(holder: ViewHolder, pi: ProjectInfo, position: Int) {
//                holder.setVisible(R.id.iv_cur_village, if(pi.oid == App.instance.currentVillage?.oid) View.VISIBLE else View.INVISIBLE)
                holder.setText(R.id.tv_quyu,pi.projectName())
            }
        }

        sjy.setOnItemClickListener { _, _, i, _ ->
            val qi = sss[i]
//            val map =mv.mapControl.map
            try {
                MyGlobal.setCurProject(qi)
                mv.refreshDataSource()
                mv.zoomToFullExtent()
                wnd.dismiss()
            }catch (e:Exception){
                AlertUtil.error(MyGlobal.mainActivity,e.toString())
            }
        }
    }
}