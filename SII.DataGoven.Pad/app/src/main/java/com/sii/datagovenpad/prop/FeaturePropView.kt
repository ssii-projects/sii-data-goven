package com.sii.datagovenpad.prop

import android.app.AlertDialog
import android.content.Intent
import android.view.View
import android.widget.*
import androidx.core.content.ContextCompat.startActivity
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.R
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.control.PropItemSpinner
import com.sii.datagovenpad.control.PropItemText
import com.sii.datagovenpad.dialog.ConformDialog
import com.sii.datagovenpad.dialog.CustomAlertDialog
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.CodeUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import com.sii.datagovenpad.view.MapView
import com.sii.util.InputMethodUtil
import com.sii.util.StringUtil
import ime.sii.gis.carto.IFeatureLayer
import ime.sii.gis.controls.tool.CreateFeatureTool
import ime.sii.gis.controls.tool.IEditTool
import ime.sii.gis.geodatabase.IFeature


open class FeaturePropView(layoutID: Int, idLinearLayout: Int,title: String,val isCreateModel:Boolean){//}:IEditPanel{
    val frame:View
    protected val view: View
    private val piTexts=ArrayList<PropItemText>()
    private val piSpinners=ArrayList<PropItemSpinner>()
    private val ll:LinearLayout
    private var mCreateFeatureTool: CreateFeatureTool?=null
    protected var mFeature: IFeature?=null
    private var tgtLyr: IFeatureLayer?=null
    private val topBar:FormInfoTopBar
    private val dicOldValues=HashMap<String,String?>()
    init {
        var context=MyGlobal.mainActivity
        view=View.inflate(context, layoutID, null)

        frame = View.inflate(context, R.layout.property_frame, null)
        frame.tag=this
        val fl = frame.findViewById<FrameLayout>(R.id.fl_prop_frame)
        fl.addView(view)

        topBar=FormInfoTopBar(null, frame, title){
            try {
                if(!isCreateModel) {
                    if (isDirty()) {
                        if(save()) {
                            AlertUtil.info(view.context, "保存成功！")
                        }
                    } else {
                        AlertUtil.info(view.context, "文档未改变！")
                    }
                }
            }catch (e:java.lang.Exception){
                AlertUtil.alert(context, e.message)
            }
        }

        ll=view.findViewById<LinearLayout>(idLinearLayout)
        ll.setOnClickListener {
            InputMethodUtil.hideSoftInputFromWindow(it)
        }
        finalConstruct()
    }
    fun updateUI(createFeatureTool: CreateFeatureTool?, iFeature: IFeature, tgtLyr_: IFeatureLayer?){
        mCreateFeatureTool=createFeatureTool
        mFeature=iFeature
        tgtLyr=tgtLyr_
        for(i in 0..ll.childCount){
            val v=ll.getChildAt(i)
            if(v is PropItemText){
                setItemText(v, iFeature)
            }else if(v is PropItemSpinner){
                setItemSpinner(v, iFeature)
            }
        }
        resetOldValues()
    }
    fun showCreate(feat:IFeature,mCreateFeatureTool:CreateFeatureTool?){
        updateUI(mCreateFeatureTool,feat,null)
        val dialog= CustomAlertDialog(frame)
        topBar.showReturnButton(true)
//        topBar.setTitle(title)
        topBar.btnSave.setOnClickListener(View.OnClickListener {
            if(save()) {
                dialog.dismiss(DialogResult.Ok)
            }
        })
        topBar.btnReturn.setOnClickListener(View.OnClickListener { //关闭对话框
            dialog.dismiss(DialogResult.Cancel)
        })
        dialog.onPreDismiss={
            if(it.item!=DialogResult.Ok){
                it.cancel=true
                AlertUtil.showConfirm(view.context,"确定不保存该地块吗？",MessageBoxButtons.YesNo){
                    if(it==DialogResult.Yes){
                        dialog.dismiss(DialogResult.Cancel,false)
                        mCreateFeatureTool!!.cancel()
                        return@showConfirm
                    }
                }
            }else {
                mCreateFeatureTool!!.cancel()
            }
        }
        dialog.show()
    }

    protected open fun logicCheck():String?{
        return null
    }
//    fun getTitle(): String {
//        return topBar.getTile()
//    }
    fun save():Boolean{
        val iFeature=mFeature!!
        val fCreate=iFeature.oId<1
        val db= MyGlobal.mainDB!!
        flush()
        val logicErr=logicCheck()
        if(logicErr!=null){
            AlertUtil.error(view.context,logicErr)
            return false
        }
        try {

            val db= MyGlobal.mainDB!!
            db.beginTransaction()
            if (fCreate) {
                mCreateFeatureTool!!.save(iFeature)
            }else{
                tgtLyr!!.featureClass.updateFeature(iFeature, false)
            }
            db.setTransactionSuccessful()
            resetOldValues()
        }catch (e: Exception){
//            return e.message
//            throw e
            AlertUtil.error(view.context,e.toString())
        }finally {
            db.endTransaction()
        }
    return true
//        return null
    }
    fun isDirty():Boolean{
        var dirty=false;
        scanText {
                fieldName,fieldValue->
            run {
                val fv = dicOldValues.get(fieldName)
                if (fv != fieldValue) {
                    dirty = true
                }
            }
        }
        return dirty
    }

    protected open fun finalConstruct(){

    }

    private fun flush(){
        val iFeature=mFeature!!
        scanText(){
            fieldName,fieldValue-> iFeature.setFieldValue(fieldName, fieldValue)
        }
    }
    private fun scanText(callback:(String,String?)->Unit){
        for(et in piTexts){
            if(!et.mReadOnly) {
                val fieldName = et.tag.toString()
                val text = et.getValue().trim()
                callback(fieldName,text)
            }
        }
        for(et in piSpinners){
            if(!et.mReadOnly) {
                val fieldName = et.tag.toString()
                val text = et.getValue()?.bm
                callback(fieldName,text)
            }
        }
    }
    private fun resetOldValues(){
        dicOldValues.clear()
        scanText{
                fieldName,fieldValue->  dicOldValues.put(fieldName,fieldValue)
        }
    }
    protected open fun getCodeTypeByFieldName(fieldName: String):String?{
        return null
    }
    private fun setItemText(et: PropItemText, feat: IFeature){
        val fieldName=et.tag.toString()
        val value=feat.getFieldValue(fieldName)
        et.setValue(value?.toString() ?: "")
        piTexts.add(et)
    }
    private fun setItemSpinner(spin: PropItemSpinner, feat: IFeature){
        piSpinners.add(spin)
        val fieldName=spin.tag.toString()
        var codeType:String?=getCodeTypeByFieldName(fieldName)//
        if(StringUtil.isNullOrEmpty(codeType)){
            codeType=spin.label.text.toString()
        }
        var lst= CodeUtil.queryCodeItems(codeType!!)
        spin.setItems(lst.toTypedArray())

        val value=feat.getFieldValue(fieldName)
        spin.setSelection(value?.toString())
    }
}
