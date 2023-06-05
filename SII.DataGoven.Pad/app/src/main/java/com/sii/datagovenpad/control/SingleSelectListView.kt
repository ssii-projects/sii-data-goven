package com.sii.datagovenpad.control

import android.widget.BaseAdapter
import android.widget.ListView
import com.sii.core.LiteEvent
import com.sii.datagovenpad.R
import com.sii.datagovenpad.adaptor.abslistview.CommonAdapter
import com.sii.datagovenpad.adaptor.abslistview.ViewHolder
import com.sii.datagovenpad.entities.EnCbfJtcy

class ListItemDblClicker{
    private var lastClickTime:Long=0
    private var preClickIdx:Int=-1
    //防止重复点击 事件间隔，在这里我定义的是1000毫秒
    fun isFastDoubleClick(clickPos:Int): Boolean {
        val time = System.currentTimeMillis()
        val timeD = time - lastClickTime
        return if (timeD >= 0 && timeD <= 500&&clickPos==preClickIdx) {
            true
        } else {
            preClickIdx=clickPos
            lastClickTime = time
            false
        }
    }
}
open class SingleSelectListView<T>{
    lateinit var lstView: ListView
    private val mDblClicker=ListItemDblClicker()
    val onDblClick=LiteEvent<T>()
    var itemSelectChanged: ((Int) -> Unit)? =null
    var selectedIndex:Int=-1
        set(value) {
            field=value
            itemSelectChanged?.invoke(value)
        }
    protected fun adapterConvert(holder: ViewHolder) {
        val bkgndID=if(holder.itemPosition==selectedIndex) R.drawable.bg_shadow else R.drawable.bg_shadow1
        holder.convertView.setBackgroundResource(bkgndID)
        holder.convertView.setOnClickListener{
            if(holder.itemPosition!=selectedIndex) {
                selectedIndex = holder.itemPosition
                (lstView.adapter as BaseAdapter).notifyDataSetChanged()
            }
            if(mDblClicker.isFastDoubleClick(selectedIndex)){
                onDblClick.fire(getSelectedItem()!!)
            }
        }
    }

    fun datas():ArrayList<T>{
        return   adapter().datas as ArrayList<T>
    }

    fun getSelectedItem(): T?{
        return if(selectedIndex>=0) adapter().getItem(selectedIndex) else null
    }
    fun adapter(): CommonAdapter<T> {
        return  lstView.adapter as CommonAdapter<T>
    }
    fun refresh(){
        adapter().notifyDataSetChanged()
    }
}