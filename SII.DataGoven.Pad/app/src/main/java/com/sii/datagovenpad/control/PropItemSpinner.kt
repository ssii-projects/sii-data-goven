package com.sii.datagovenpad.control

import android.content.Context
import android.util.AttributeSet
import android.view.LayoutInflater
import android.view.View
import android.widget.*
import com.sii.datagovenpad.R
import com.sii.datagovenpad.util.CodeItem


class PropItemSpinner (context: Context, attrs: AttributeSet) : LinearLayout(context, attrs) {
    val label: TextView
    val tv:EditText
//    private val _text: EditText
    private val _spinner:Spinner
    private val _context:Context
    val mReadOnly:Boolean
    var originValue:String?=null
    init {
        _context=context
        LayoutInflater.from(context).inflate(R.layout.prop_item_spinner, this)
        label = findViewById(R.id.tv_prop_item_spin)
        _spinner=findViewById(R.id.sp_prop_item_spin)
        tv=findViewById(R.id.sp_prop_item_tv)
        val typedArray = context.obtainStyledAttributes(attrs, R.styleable.PropItemSpinner)
        mReadOnly=typedArray.getBoolean(R.styleable.PropItemSpinner_readonly,false)
        if(mReadOnly){
            _spinner.visibility= View.GONE
            tv.visibility= View.VISIBLE
            tv.keyListener=null
            tv.setTextIsSelectable(true)
        }
        val text = typedArray.getString(R.styleable.PropItemSpinner_text)
        label.text = text
        typedArray.recycle()
    }
    override fun isDirty():Boolean{
        val it=getValue()
        return it?.bm!=originValue
    }
    fun setItems(items:Array<CodeItem>){
        val adapter =ArrayAdapter<CodeItem>(_context, android.R.layout.simple_spinner_item, items)
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item)
        _spinner.adapter=adapter
    }
    fun setSelection(bm:String?){
        if(_spinner.adapter==null) return
        originValue=bm
        val apsAdapter = _spinner.adapter //得到SpinnerAdapter对象
        val k = apsAdapter.count
        for (i in 0 until k) {
            val it=apsAdapter.getItem(i) as CodeItem
            if (it.bm==bm) {
                _spinner.setSelection(i, true)// 默认选中项
                if(mReadOnly){
                    tv.setText(it.toString())
                }
                break
            }
        }
    }
    fun getValue():CodeItem?{
        val si=_spinner.selectedItem
        return if(si==null)null else si as CodeItem
    }
}
