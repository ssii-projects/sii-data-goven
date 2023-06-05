package com.sii.datagovenpad.popup.cbf

import android.content.Context
import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.SearchView
import com.sii.core.MyLog
import com.sii.datagovenpad.R
import com.sii.datagovenpad.adaptor.abslistview.CommonAdapter
import com.sii.datagovenpad.adaptor.abslistview.ViewHolder
import com.sii.datagovenpad.control.CustomImageButton
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.control.ListItemKeyValue
import com.sii.datagovenpad.control.SingleSelectListView
import com.sii.datagovenpad.entities.EZt
import com.sii.datagovenpad.entities.EnCbf
import com.sii.datagovenpad.repos.CbfRepos
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.CodeUtil
import com.sii.functional.Cancel
import com.sii.util.InputMethodUtil
import com.sii.util.StringUtil
import com.sii.view.stackview.StackViewContainer

class CbfListView(context:Context): SingleSelectListView<EnCbf>(){

    val view: View

    private val propViewCbf: CbfPropFrame by lazy {
        CbfPropFrame()
    }
    init {
        view=View.inflate(context, R.layout.cbf_list, null)


        val btnInfo=view.findViewById<CustomImageButton>(R.id.btn_cbf_lst_info)

        val btnEdit=view.findViewById<Button>(R.id.btn_edit_cbf)
        super.lstView=view.findViewById(R.id.cbf_list_lv)
        val searchView =view.findViewById<SearchView>(R.id.cbf_lst_search_view)
        lstView.isNestedScrollingEnabled = false
        val cbfSearch= CbfSearch(){doSearch(it)}
        searchView.setOnQueryTextListener(cbfSearch)
//        stackView.onAfterPop.addListener{
//            if(stackView.top==view){
//                refresh()
//            }
//        }

        onDblClick.addListener {
            showItemProperty(it)
//            propViewCbf.setCbfEn(it)
//            propViewCbf.resetTab()
//            stackView.push(propViewCbf.view)
        }
        btnInfo.setOnClickListener{
//            propViewCbf.setCbfEn(getSelectedItem()!!)
//            stackView.push(propViewCbf.view)
            showItemProperty(getSelectedItem()!!)
        }
        super.itemSelectChanged={
            btnEdit.isEnabled=it>=0
//            btnDel.isEnabled=it>=0
            btnInfo.isEnabled=it>=0
            InputMethodUtil.hideSoftInputFromWindow(view)
        }

        lstView.adapter = object : CommonAdapter<EnCbf>(searchView.context, R.layout.cbf_list_item, ArrayList<EnCbf>()) {
            override fun convert(holder: ViewHolder, pi: EnCbf, position: Int) {
                if(holder.convertView is LinearLayout){
                    for(i in 0..holder.convertView.childCount){
                        val v=holder.convertView.getChildAt(i)
                        if(v is ListItemKeyValue){
                            val fieldName=v.tag.toString()
                            var value=pi.getFieldValue(fieldName)

                            if(value is String) {
                                val codeType=EnCbf.instance.getFieldCodeType(fieldName)
                                if (!StringUtil.isNullOrEmpty(codeType,false)){//fieldName == "cbflx") {
                                    value = CodeUtil.code2Name(codeType,value)// pi.cbfLx)
                                }
                            }
                            v.setValue(value?.toString() ?: "")
                        }
                    }
                }
                adapterConvert(holder)
            }
        }

//        doSearch()

    }
//    fun isDirty():Boolean{
//        return  propViewCbf.isDirty()
//    }
//    fun save(){
//        if(propViewCbf.isDirty()){
//            propViewCbf.save()
//        }
//    }
//    fun tryLeave(cancel: Cancel){
//        propViewCbf.tryLeave(cancel)
//    }
    fun clear() {
        selectedIndex = -1
        val adapter = adapter()
        val lst = adapter.datas as ArrayList<EnCbf>
        lst.clear()
        adapter.notifyDataSetChanged()
    }
    fun count():Number {
        val adapter = adapter()
        val lst = adapter.datas as ArrayList<EnCbf>
        return lst.size
    }
    fun doSearch(key:String?=null){
        selectedIndex=-1
        val adapter=adapter()
        val lst=adapter.datas as ArrayList<EnCbf>
        lst.clear()

        var wh="ZT<>${EZt.lishi.ordinal}"
        if(!StringUtil.isNullOrEmpty(key,true)){
            wh+=" and CBFMC like '%${key}%'"
        }
//        MyLog.i("wh:${wh}")
        val cbfRepos= CbfRepos.instance();
        cbfRepos.findAll(wh,lst)
        adapter.notifyDataSetChanged()
    }

    private fun showItemProperty(cbfEn:EnCbf){
        propViewCbf.setCbfEn(cbfEn)
        propViewCbf.showDialog(){
            refresh()
        }
    }
}

class CbfSearch(private val onSearch:(String?)->Unit):SearchView.OnQueryTextListener{
    override fun onQueryTextSubmit(p0: String?): Boolean {
        onSearch.invoke(p0)
        return false
    }

    override fun onQueryTextChange(p0: String?): Boolean {
        if(p0==null||p0==""){
            onSearch.invoke(null)
        }
        return false
    }
}