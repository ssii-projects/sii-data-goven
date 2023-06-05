package com.sii.datagovenpad.popup.cbf

import com.sii.datagovenpad.R
import com.sii.datagovenpad.dialog.ConformDialog
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.entities.EnCbf
import com.sii.datagovenpad.prop.EntityPropView
import com.sii.datagovenpad.repos.CbfRepos
import com.sii.datagovenpad.util.AlertUtil
import com.sii.util.DateTimeUtil


class CbfProp(): EntityPropView<EnCbf>(R.layout.cbf_prop,R.id.cbf_prop_ll),ICbfTabItem{
    override fun preSave(en:EnCbf) {
        en.zhxgsj=DateTimeUtil.getCurrentTimeString()
    }
    override fun tryLeave():Boolean{
        if(isDirty()){
            val dlg= ConformDialog()
            val res=dlg.showDialog("承包方数据已变更，是否保存？")
            when(res){
                DialogResult.No->cancel()
                DialogResult.Yes-> {
                    val err = save()
                    if (err != null) {
                        AlertUtil.error(view.context, err)
                        return false
                    }
                }
            }
        }
        return true
    }
    private fun cancel(){
        val oldEn=CbfRepos.instance().find("rowid=${en.rowid}")
        if(oldEn!=null){
            oldEn.writeTo(en)
            updateUI(en)
        }
    }
}