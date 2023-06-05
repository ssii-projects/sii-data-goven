package com.sii.datagovenpad.control

import android.content.Context
import android.util.AttributeSet
import android.view.LayoutInflater
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.TextView
import com.sii.datagovenpad.R

class ListItemKeyValue  (context: Context, attrs: AttributeSet) : LinearLayout(context, attrs) {
    private val _label: TextView
    private val _text: TextView
    init {
        LayoutInflater.from(context).inflate(R.layout.list_item_key_value, this)
        _label = findViewById(R.id.likv_label)
        _text=findViewById(R.id.likv_value)

        val typedArray = context.obtainStyledAttributes(attrs, R.styleable.PropItemText)

        val text = typedArray.getString(R.styleable.PropItemText_text)
        _label.text = text

//        val isReadonly=typedArray.getBoolean(R.styleable.PropItemText_readonly,false)
//        if(isReadonly){
//            _text.keyListener=null
//            _text.setTextIsSelectable(true)
//        }
//        val inputType=typedArray.getInt(R.styleable.PropItemText_inputType,1)
//        if(inputType==2) {
//            _text.inputType =8194//InputType.TYPE_CLASS_NUMBER //| InputType.TYPE_NUMBER_FLAG_DECIMAL
//        }
        typedArray.recycle()
    }
    fun setValue(txt:String){
        _text.setText(txt)
    }
    fun getValue():String{
        return _text.text.toString()
    }
    fun setLabel(label:String){
        _label.text=label
    }
}