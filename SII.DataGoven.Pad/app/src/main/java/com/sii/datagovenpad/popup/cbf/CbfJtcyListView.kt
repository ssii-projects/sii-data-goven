package com.sii.datagovenpad.popup.cbf

import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import com.sii.core.MyLog
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.R
import com.sii.datagovenpad.adaptor.abslistview.CommonAdapter
import com.sii.datagovenpad.adaptor.abslistview.ViewHolder
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.control.ListItemKeyValue
import com.sii.datagovenpad.control.SingleSelectListView
import com.sii.datagovenpad.dialog.ConformDialog
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.entities.EnCbf
import com.sii.datagovenpad.entities.EnCbfJtcy
import com.sii.datagovenpad.repos.CbfJtcyRepos
import com.sii.datagovenpad.repos.CbfRepos
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.CodeUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import com.sii.util.CollectionUtil
import com.sii.util.DateTimeUtil
import com.sii.util.StringUtil
import com.sii.view.stackview.StackViewContainer

class CbfJtcyListView(val stackView: StackViewContainer):SingleSelectListView<EnCbfJtcy>(){
    private val jtcyProp:CbfJtcyProp by lazy { CbfJtcyProp(stackView)}
    private lateinit var enCbf:EnCbf
    private val delItems=ArrayList<EnCbfJtcy>()
    private var _isDirty=false
    private val topBar: FormInfoTopBar
    val view: View = View.inflate(
        stackView.containerView.context,
        R.layout.cbf_jtcy_list,
        null
    )

    init {
        super.lstView=view.findViewById(R.id.cbf_jtcy_list_view)
        stackView.onAfterPop.addListener {
            if(it==jtcyProp.view){
                refresh()
                if(jtcyProp.isDirty()){
                    _isDirty=true
                }
            }
        }
        stackView.onPrePop.addListener {
            if(stackView.top==view&&isDirty()){
                it.cancel=true
                AlertUtil.showConfirm(view.context,"数据已修改，是否保存？", MessageBoxButtons.YesNo){
                    if(it==DialogResult.Yes){
                        if(!doSave()) return@showConfirm
                    }
                    stackView.pop(false)
                }
            }
        }

        jtcyProp.onSaved={
            if(jtcyProp.isCreateMode){
                val lst=datas()// this.adapter().datas as ArrayList<EnCbfJtcy>
                lst.add(it)
                this.adapter().notifyDataSetChanged()
                selectedIndex=lst.size-1
            }
        }

        topBar= FormInfoTopBar(stackView, view, "承包方家庭成员"){
            if(isDirty()) {
                if(!doSave()){
                    return@FormInfoTopBar
                }
            }
//            AlertUtil.info(view.context,"保存成功") {
                stackView.pop()
//            }
        }

        lstView.adapter = object : CommonAdapter<EnCbfJtcy>(view.context, R.layout.cbf_jtcy_list_item, ArrayList<EnCbfJtcy>()) {
            override fun convert(holder: ViewHolder, pi: EnCbfJtcy, position: Int) {
                if(holder.convertView is LinearLayout){
                    for(i in 0..holder.convertView.childCount){
                        val v=holder.convertView.getChildAt(i)
                        if(v is ListItemKeyValue){
                            val fieldName=v.tag.toString()
                            var value=pi.getFieldValue(fieldName)
                            if(value is String) {
                                val codeType=EnCbfJtcy.instance.getFieldCodeType(fieldName)
                                if (!StringUtil.isNullOrEmpty(codeType)){
                                    value = CodeUtil.code2Name(codeType,value)
                                }
                            }
                            v.setValue(value?.toString() ?: "")
                        }
                    }
                }
                adapterConvert(holder)
            }
//            private fun setValue(holder: ViewHolder, id:Int, value:String) {
//                holder.getView<ListItemKeyValue>(id).setValue(value)
//            }
        }

        val btnAdd=view.findViewById<Button>(R.id.btn_cbf_jtcy_add)
        val btnEdit=view.findViewById<Button>(R.id.btn_cbf_jtcy_edit)
        val btnDel=view.findViewById<Button>(R.id.btn_cbf_jtcy_del)
        btnAdd.setOnClickListener{
            val en=EnCbfJtcy()
            en.cbfbm=enCbf.cbfbm
            showProperty(en,true)
        }
        onDblClick.addListener {
            showProperty(it)
        }
        btnEdit.setOnClickListener{
            showProperty(this.getSelectedItem()!!,false)
        }
        btnDel.setOnClickListener{
            val en=getSelectedItem()!!
            if(en.rowid>0){
                delItems.add(en)
            }
            datas().remove(en)
            selectedIndex=-1
            refresh()

//            AlertUtil.showConfirm(view.context,"确定要删除选择项吗？"){
//                CbfJtcyRepos.instance().delete(en)
//                val newEnCbf=CbfRepos.instance().updateWhenJtcyChanged(en.cbfbm)
//                newEnCbf?.writeTo(enCbf)
//                updateUI(enCbf)
//            }
        }
        view.findViewById<View>(R.id.btn_cbf_jtcy_reset).setOnClickListener{
            updateUI(enCbf)
        }
        super.itemSelectChanged={
            btnEdit.isEnabled=it>=0
            btnDel.isEnabled=it>=0
        }
    }

    fun isDirty():Boolean{
        if(_isDirty||delItems.isNotEmpty()) return true
        return datas().find { it.rowid<=0 }!=null
    }

    fun updateUI(en: EnCbf) {
        _isDirty=false
        delItems.clear()
        enCbf=en
        val adapter=adapter()
        val lst=adapter.datas as ArrayList<EnCbfJtcy>
        lst.clear()
        selectedIndex=-1

        CbfJtcyRepos.instance().findAll("cbfbm='${en.cbfbm}'",lst)
        CollectionUtil.removeIf(lst){
            val it1=it
            delItems.find {
                it.id==it1.id
            }!=null
        }
        adapter.notifyDataSetChanged()
    }
//    fun tryLeave():Boolean {
//        if(isDirty()) {
//            val err = logicCheck()
//            if (err != null) {
//                AlertUtil.error(view.context, err)
//                return false
//            } else {
//                val dlg= ConformDialog()
//                when(dlg.showDialog("家庭成员数据已变更，是否保存？")){
//                    DialogResult.No->cancel()
//                    DialogResult.Yes-> {
//                        val err = save()
//                        if (err != null) {
//                            AlertUtil.error(view.context, err)
//                            return false
//                        }
//                    }
//                }
//                return true
//            }
//        }
//        return true
//    }
    fun save():String?{
        var err:String?= logicCheck()
        if(err!=null) return err
        val db=MyGlobal.mainDB!!
        try{
            db.beginTransaction()
            val repos=CbfJtcyRepos.instance()
            for(en in delItems){
               repos.delete(en)
            }
            val lst=datas()
            var hzEn:EnCbfJtcy?=null
            for(en in lst){
               repos.save(en)
                if(isHuzu(en)){
                    hzEn=en
                }
            }
            enCbf.cbfCysl=lst.size
            enCbf.cbfmc=hzEn?.cyxm
            enCbf.cbfZjlx=hzEn?.cyZjlx
            enCbf.cbfZjhm=hzEn?.cyZjhm
            enCbf.zhxgsj=DateTimeUtil.getCurrentTimeString()
            CbfRepos.instance().save(enCbf)
            db.setTransactionSuccessful()
            delItems.clear()
            _isDirty=false
        }catch (e:Exception){
            MyLog.e(e.toString())
//            AlertUtil.error(view.context, e.toString())
            err=e.message
        }
        db.endTransaction()
        return err// _isDirty==false
    }

    private fun doSave():Boolean{
        if(isDirty()) {
            var err = save()
            if (err != null) {
                AlertUtil.error(view.context, err)
                return false
            }
        }
        return true
    }

    private fun cancel(){
        delItems.clear()
        updateUI(enCbf)
    }
    /**
     * 逻辑检查
     */
    private fun logicCheck():String?{
        var nHzCount=0
        for(it in datas()){
            if(isHuzu(it)) {
                ++nHzCount
                if(nHzCount>1){
                    return "与户主关系为'本人'或'户主'的只能有1个（户主只能有1个）"
                }
            }
            if(StringUtil.isNullOrEmpty(it.cyxm,true)){
                return "成员姓名必填"
            }
        }
        if(nHzCount==0){
            return "至少应该有一个户主（与户主关系为：'本人'或'户主'）"
        }
        return null
    }
    private fun isHuzu(en:EnCbfJtcy):Boolean{
        val name=CodeUtil.code2Name(en.getFieldCodeType("yhzgx"),en.yhzgx)
        return "本人"==name||"户主"==name
    }
    private fun showProperty(en:EnCbfJtcy,isCreateMode: Boolean =false){
        jtcyProp.isCreateMode=isCreateMode
        jtcyProp.updateUI(en)
        stackView.push(jtcyProp.view)
    }
}