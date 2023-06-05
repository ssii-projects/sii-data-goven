package com.sii.datagovenpad.prop

import android.view.View
import android.widget.LinearLayout
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.control.PropItemSpinner
import com.sii.datagovenpad.control.PropItemText
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.CodeUtil
import com.sii.orm.EntityBase
import com.sii.util.InputMethodUtil
import java.lang.Exception


open class EntityPropView<T :EntityBase<T>>(layoutID:Int,private val linearLayoutID:Int) {
    val view: View
    lateinit var en: T
    private val piTexts=ArrayList<PropItemText>()
    private val piSpinners=ArrayList<PropItemSpinner>()
    init {
        view= View.inflate(MyGlobal.mainActivity,layoutID,null)
    }
    fun save():String?{
        var err:String?=null
        val db=MyGlobal.mainDB!!
        try {
            flush()
            db.beginTransaction()
            preSave(en)
            db.SaveEntity(en)
            afterSave(en)
            db.setTransactionSuccessful()
            updateUI(en)
//            AlertUtil.showInfo(view.context,"保存成功！")
        }catch (e:Exception){
            err=if(e.message!=null) e.message else e.toString()
//            AlertUtil.alert(view.context, e.message)
        }
        db.endTransaction()
        return err
    }
    protected open fun preSave(en:T){

    }
    protected open fun afterSave(en:T){

    }
    fun updateUI(en: T) {
        this.en =en
        val ll=view.findViewById<LinearLayout>(linearLayoutID)
        for(i in 0..ll.childCount){
            val v=ll.getChildAt(i)
            if(v is PropItemText){
                setItemText(v)
            }else if(v is PropItemSpinner){
                setItemSpinner(v)
            }
        }
        ll.setOnClickListener { InputMethodUtil.hideSoftInputFromWindow(it)}
    }
    fun isDirty():Boolean{
        for(et in piTexts){
            if(!et.mReadOnly&&et.isDirty) return true
        }
        for(et in piSpinners){
            if(!et.mReadOnly&&et.isDirty) return true
        }
        return false
    }
    fun flush(){
        for(et in piTexts){
            if(!et.mReadOnly) {
                val fieldName = et.tag.toString()
                val text = et.getValue().trim()
                en.setFieldValue(fieldName, text)
            }
        }
        for(et in piSpinners){
            if(!et.mReadOnly) {
                val fieldName = et.tag.toString()
                val text = et.getValue()?.bm
                en.setFieldValue(fieldName, text)
            }
        }
    }

    private fun setItemText(et: PropItemText){
        val fieldName=et.tag.toString()
        val value=en?.getFieldValue(fieldName)
        et.setValue(value?.toString() ?: "")
        piTexts.add(et)
    }
    private fun setItemSpinner(spin: PropItemSpinner){
        piSpinners.add(spin)
        val fieldName=spin.tag.toString()

        val codeType=en.getFieldCodeType(fieldName)

        var lst = CodeUtil.queryCodeItems(codeType)

        spin.setItems(lst.toTypedArray())

        val value=en.getFieldValue(fieldName)
        spin.setSelection(value?.toString())
    }
}