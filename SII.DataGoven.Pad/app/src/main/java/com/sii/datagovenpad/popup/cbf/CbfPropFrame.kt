package com.sii.datagovenpad.popup.cbf

import android.view.View
import android.widget.FrameLayout
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.R
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.control.TabItem
import com.sii.datagovenpad.control.TabItemGroup
import com.sii.datagovenpad.dialog.CustomAlertDialog
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.entities.EnCbf
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import com.sii.view.stackview.StackViewContainer


interface ICbfTabItem{
    fun save():String?
    fun isDirty():Boolean
    fun tryLeave():Boolean
}
class CbfPropFrame(){
    val view: View = View.inflate(MyGlobal.mainActivity, R.layout.cbf_prop_frame, null)
    private val stackView: StackViewContainer = StackViewContainer(MyGlobal.mainActivity)
    private var cbfEn: EnCbf?=null
    private val mTabs: TabItemGroup = TabItemGroup()
    private val frameLayout: FrameLayout;
    private val cbfProp: CbfProp =CbfProp()
    private val cbfJtcyListView:CbfJtcyListView
    private val topBar:FormInfoTopBar
    init{
        stackView.push(view)
        cbfJtcyListView=CbfJtcyListView(stackView)

        frameLayout=view.findViewById(R.id.cbf_prop_from_fl)
        topBar=FormInfoTopBar(stackView, view, "承包方属性")
        topBar.showSaveButton(true)

        val tiCbfProp=view.findViewById<TabItem>(R.id.ti_cbf_prop)
        val tiCbfJtcy=view.findViewById<TabItem>(R.id.ti_cbf_jtcy)
        tiCbfProp.tag=cbfProp
        tiCbfJtcy.tag=cbfJtcyListView

        mTabs.onItemPreDeactive={
            it.cancel=true
            cbfJtcyListView.updateUI(cbfEn!!)
            stackView.push(this.cbfJtcyListView.view)
        }
        mTabs.addRange(arrayListOf(tiCbfProp, tiCbfJtcy))
        mTabs.currentItem = tiCbfProp
        setContentView(cbfProp.view)
    }
    fun setCbfEn(en: EnCbf){
        cbfEn=en
        cbfProp.updateUI(en)
    }
    fun isDirty():Boolean{
        return cbfProp.isDirty()||cbfJtcyListView.isDirty();
    }

    fun showDialog(onOK:()->Unit) {
        val dialog=CustomAlertDialog(stackView.containerView)
        topBar.btnSave.setOnClickListener {
            try {
                if(cbfProp.isDirty()) {
                    if (!doSave()) return@setOnClickListener
                        onOK()
                }
                dialog.dismiss(DialogResult.Ok,false)
            } catch (e: java.lang.Exception) {
                AlertUtil.alert(view.context, e.message)
            }
        }
        topBar.btnReturn.setOnClickListener{ //关闭对话框
            dialog.dismiss(DialogResult.Cancel)
        }
        dialog.onPreDismiss={ it ->
//            MyLog.i("stackView.viewCount:${stackView.viewCount}")
            it.cancel=true
            if(stackView.viewCount>1){
                stackView.pop()
            }else{
                if(cbfProp.isDirty()) {
                    AlertUtil.showConfirm(view.context, "数据已修改，是否保存?", MessageBoxButtons.YesNo) {
                        if (it == DialogResult.Yes) {
                            if (!doSave()) return@showConfirm
                        }
                        dialog.dismiss(it,false)
                    }
                }else{
                    dialog.dismiss(it.item,false)
                }
            }
        }
        dialog.show()
    }
    private fun doSave():Boolean{
        if(cbfProp.isDirty()) {
            var err = cbfProp.save()
            if (err != null) {
                AlertUtil.error(view.context, err)
                return false
            }
        }
        return true
    }
    private fun setContentView(v: View){
        var fExist=false;
        for(i in 0 until  frameLayout.childCount){
            val view=frameLayout.getChildAt(i)
            if(view==v){
                fExist=true
                if(v.visibility!=View.VISIBLE){
                    v.visibility=View.VISIBLE
                }
            }else{
                view.visibility=View.INVISIBLE
            }
        }
        if(!fExist){
            frameLayout.addView(v)
        }
    }
}


