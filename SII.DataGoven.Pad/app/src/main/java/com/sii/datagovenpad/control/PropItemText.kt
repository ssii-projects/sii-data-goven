package com.sii.datagovenpad.control

import android.app.AlertDialog
import android.content.Context
import android.content.DialogInterface
import android.util.AttributeSet
import android.view.LayoutInflater
import android.view.View
import android.widget.DatePicker
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.TextView
import androidx.core.content.ContextCompat
import com.sii.datagovenpad.R
import com.sii.util.StringUtil
import java.text.SimpleDateFormat
import java.util.*

open class PropItemText (context: Context, attrs: AttributeSet) : LinearLayout(context, attrs) {
    private val _label: TextView
    val _text: EditText
    var originValue:String?=null
    val mReadOnly:Boolean
    init {
        LayoutInflater.from(context).inflate(R.layout.prop_item_text, this)
        _label = findViewById(R.id.tv_prop_item_text)
        _text=findViewById(R.id.et_prop_item_text)



        val typedArray = context.obtainStyledAttributes(attrs, R.styleable.PropItemText)

        val text = typedArray.getString(R.styleable.PropItemText_text)
        _label.text = text

        mReadOnly=typedArray.getBoolean(R.styleable.PropItemText_readonly,false)
        if(mReadOnly){
            _text.keyListener=null
            _text.setTextIsSelectable(true)
            _text.setTextColor(ContextCompat.getColor(context,R.color.colorTextReadOnly))
//            _text.isEnabled=false
        }
        val inputType=typedArray.getInt(R.styleable.PropItemText_inputType,1)
        if(inputType==2) {
            _text.inputType =8194//InputType.TYPE_CLASS_NUMBER //| InputType.TYPE_NUMBER_FLAG_DECIMAL
        }
        typedArray.recycle()
    }

    override fun isDirty():Boolean{
        return getValue()!=originValue
    }
    open fun setValue(txt:String){
        originValue=txt
        _text.setText(txt)
    }
    fun getValue():String{
        return _text.text.toString()
    }
    fun setLabel(label:String){
        _label.text=label
    }
}

class  PropItemDate (context: Context, attrs: AttributeSet) : PropItemText(context, attrs)
{

    init {
        _text.keyListener=null
        _text.setTextIsSelectable(true)
        _text.setOnFocusChangeListener { view, b ->  if(b) showDatePicker()}
        _text.setOnClickListener{showDatePicker()}
    }

    override fun setValue(txt:String){
        val str=makeValidDateString(txt)
        super.setValue(str)
    }
    private fun makeValidDateString(txt: String):String{
        var str=txt;
        if(txt.indexOf("-")>0) {
            val dateFormat = SimpleDateFormat("yyyy-MM-dd");
            val d=dateFormat.parse(txt)
            str=dateFormat.format(d)
        }
        return str
    }
    private fun showDatePicker(){
        val ctx =context
        val builder: AlertDialog.Builder = android.app.AlertDialog.Builder(ctx)
        val picker = View.inflate(ctx, R.layout.date_picker_spinner,null) as DatePicker

        val str=getValue()
        if(!StringUtil.isNullOrEmpty(str,false)) {
            val dateFormat = SimpleDateFormat("yyyy-MM-dd");
            val date = dateFormat.parse(str)
            val calendar = Calendar.getInstance()
            calendar.time = date
            val year = calendar[Calendar.YEAR]
            val month = calendar[Calendar.MONTH]
            val day = calendar[Calendar.DAY_OF_MONTH]
            picker.updateDate(year, month, day)
        }

        builder.setTitle("选择日期")
        builder.setView(picker)
        builder.setNegativeButton("取消", null)
        builder.setPositiveButton("确定"){ dialogInterface: DialogInterface, i: Int ->
            var str="${picker.year}-${picker.month+1}-${picker.dayOfMonth}"
            str=makeValidDateString(str)
            _text.setText(str)
        }
        builder.show()
    }
}