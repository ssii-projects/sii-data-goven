package com.sii.datagovenpad.control

import android.content.Context
import android.util.AttributeSet
import android.view.LayoutInflater
import android.view.View
import android.widget.RelativeLayout
import android.widget.TextView
import com.sii.datagovenpad.R
import com.sii.functional.CancelItem

class TabItem(context: Context, attrs: AttributeSet) : RelativeLayout(context, attrs) {
    private val _frame: RelativeLayout
    private val _text: TextView
    private val mIndicator: View

    var isChecked: Boolean=true
        get() = mIndicator.visibility == View.VISIBLE
        set(value){
            field=value
            mIndicator.visibility = if (value) View.VISIBLE else View.GONE
        }
    init {
        LayoutInflater.from(context).inflate(R.layout.tab_item, this)
        _frame = findViewById(R.id.ll_frame)
        _text = findViewById(R.id.tv_tab_item_text)
        mIndicator = findViewById(R.id.view_indicator)
        val typedArray = context.obtainStyledAttributes(attrs, R.styleable.TabItem)

        val text = typedArray.getString(R.styleable.TabItem_text)
        _text.text = text
        typedArray.recycle()
    }

}

class TabItemGroup {
    private val mTabItems = ArrayList<TabItem>()
    var onItemPreDeactive:((ti:CancelItem<TabItem>)->Unit)?=null
    var onItemActived:((ti:TabItem)->Unit)?=null
    var currentItem:TabItem? = null
        set(value) {
            if(onItemPreDeactive!=null&&field!=null){
                val it=CancelItem<TabItem>()
                it.item=field
                onItemPreDeactive!!(it)
                if(it.cancel) return
            }
            for (t in mTabItems) {
                if (t != value) {
                    t.isChecked=false
                }
            }
            value?.isChecked=true
            if (value != null) {
                onItemActived?.invoke(value)
            }
            field=value
        }

    fun getTabItem(i:Int):TabItem?{
        return if(i>=0&&i<mTabItems.size) mTabItems[i] else null
    }

    fun addRange(r: ArrayList<TabItem>) {
        r.forEach { t -> add(t) }
    }

    fun add(t: TabItem) {
        mTabItems.add(t)
        t.setOnClickListener { currentItem = t }
    }
}