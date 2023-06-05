package com.sii.datagovenpad.popup.cbf

import androidx.core.widget.doOnTextChanged
import com.sii.datagovenpad.R
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.control.PropItemDate
import com.sii.datagovenpad.control.PropItemSpinner
import com.sii.datagovenpad.control.PropItemText
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.entities.EnCbfJtcy
import com.sii.datagovenpad.prop.EntityPropView
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import com.sii.util.StringUtil
import com.sii.view.stackview.StackViewContainer

class CbfJtcyProp(stackView:StackViewContainer)
    : EntityPropView<EnCbfJtcy>(R.layout.cbf_jtcy_prop,R.id.cbf_jtcy_prop_ll){
    val spCyxb:PropItemSpinner
    val tbCsrq:PropItemDate
    val tbZjhm:PropItemText
    var isCreateMode=true
    var onSaved:((EnCbfJtcy)->Unit)?=null
    init {
        FormInfoTopBar(stackView, view, "家庭成员属性") {
            if(doSave()){
                stackView.pop()
            }
        }
        stackView.onPrePop.addListener {
            if(stackView.top==view&&isDirty()){
                it.cancel=true
                AlertUtil.showConfirm(view.context,"数据已修改，是否保存？",MessageBoxButtons.YesNo){
                    if(it==DialogResult.Yes){
                        if(!doSave()) return@showConfirm
                    }
                    stackView.pop(false)
                }
            }
        }
//        stackView.onPrePop.addListener {
//            if(stackView.top==view){
//                if(isDirty()) {
//                    flush()
//                }
//                val err=logicCheck()
//                if(err!=null){
//                    AlertUtil.error(view.context,err)
//                   it.cancel=true
//                }
//            }
//        }

        spCyxb=view.findViewById(R.id.tb_cbf_jtcy_prop_cyxb)
        tbCsrq=view.findViewById(R.id.tb_cbf_jtcy_prop_csrq)
        tbZjhm=view.findViewById(R.id.tb_cbf_jtcy_prop_zjhm)

        tbZjhm._text.doOnTextChanged { text, start, before, count ->onPIDChanged()  }
    }


    fun logicCheck():String?{
        if(StringUtil.isNullOrEmpty(en.cyxm,true)){
            return "成员姓名必填"
        }
        if(StringUtil.isNullOrEmpty(en.cyxb,true)){
            return "成员性别必填"
        }
        return null
    }

    private fun doSave():Boolean{
        if(isDirty()) {
            flush()
            val err = logicCheck()
            if (err != null) {
                AlertUtil.error(view.context, err)
                return false
            } else {
                onSaved?.invoke(en)
            }
            return true
        }
        return false
    }

    /**
     * 身份证号解析
     */
    private fun onPIDChanged(){
        val zjhm=tbZjhm.getValue()
        if(zjhm.length==18||zjhm.length==15){
            val y=zjhm.substring(6,10)
            val m=zjhm.substring(10,12)
            val d=zjhm.substring(12,14)
            val rq="${y}-${m}-${d}"
            tbCsrq.setValue(rq)
            if(zjhm.length==18){
                val n=zjhm.substring(16,17).toInt()
                spCyxb.setSelection(if(n%2==0)"2" else "1")
            }
        }
    }
}