package com.sii.datagovenpad.map.control

import android.view.View
import com.sii.datagovenpad.control.ToolButton
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import kotlinx.android.synthetic.main.activity_main.*

class DeleteCommand (private val tbDelete: ToolButton, val m:IMapHost):IMapCommand {
    init {
        tbDelete.setOnClickListener {
            var mapControl=m.getMapControl()
            AlertUtil.showConfirm(mapControl.context, "请确认是否删除选中要素？",MessageBoxButtons.YesNo) {
                if(it==DialogResult.Yes) {
                    for (i in m.mSelectedFeatures) {
                        val fc = i.featureLayer.featureClass
                        fc.deleteByOid(i.oid)
                    }
                    mapControl.map.clearSelection()
                    m.mSelectedFeatures.clear()
                    mapControl.map.refresh()
                    m.refillSelectedFeatures()
                }
            }
        }
    }

    override fun updateUI() {
        tbDelete.visibility = if (m.mSelectedFeatures.size > 0) View.VISIBLE else View.GONE
    }
}