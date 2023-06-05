package com.sii.datagovenpad.control

import android.content.Context
import android.graphics.Bitmap
//import android.support.v4.content.ContextCompat
import android.view.LayoutInflater
import android.widget.ImageView
import android.widget.LinearLayout
import android.widget.TextView
import com.sii.datagovenpad.R
import android.util.AttributeSet
import android.view.View
import androidx.core.content.ContextCompat

class ToolButton(context: Context, attrs: AttributeSet) : LinearLayout(context, attrs) {
    //private val _frame: LinearLayout
    private val _image: ImageView
    private val _text: TextView
    private val _imageNormal: Int
    private val _imageCheck: Int
    private val _imageDisable:Int
    private var _fCheck: Boolean = false
    private var _fDisable:Boolean=false
    private val _normalColor: Int
    private val _checkColor: Int


    init {
        LayoutInflater.from(context).inflate(R.layout.tool_button, this)
        _checkColor = ContextCompat.getColor(context,R.color.colorCheck)
        _normalColor = ContextCompat.getColor(context,R.color.white)
        //_frame = findViewById(R.id.ll_frame)
        _image = findViewById(R.id.iv_toolbutton_image)
        //view_space = findViewById(R.id.view_space)
        _text = findViewById(R.id.tv_toolbutton_text)

        val typedArray = context.obtainStyledAttributes(attrs, R.styleable.ToolButton)


        _imageNormal = typedArray.getResourceId(R.styleable.ToolButton_image_normal, R.mipmap.ic_launcher)
        _imageCheck = typedArray.getResourceId(R.styleable.ToolButton_image_check, R.mipmap.ic_launcher)
        _imageDisable=typedArray.getResourceId(R.styleable.ToolButton_image_disable, R.mipmap.ic_launcher)
        _image.setImageResource(_imageNormal)
        //_image.drawable

        val text = typedArray.getString(R.styleable.ToolButton_text)
        _text.text = text

        typedArray.recycle()


    }

    fun isCheck(): Boolean {
        return _fCheck
    }

    fun setCheck(fCheck: Boolean) {
        if (fCheck == _fCheck) {
            return
        }
        _image.setImageResource(if (fCheck) _imageCheck else _imageNormal)
        _text.setTextColor(if (fCheck) _checkColor else _normalColor)
        _fCheck = fCheck
    }
    fun isEnable():Boolean{
        return !_fDisable
    }
    fun setEnable(fEnable:Boolean){
        if(_fDisable==!fEnable){
            return
        }
        _fDisable=!fEnable
        if(_fDisable){
            _image.setImageResource(_imageDisable)
        }else{
            _image.setImageResource(if (_fCheck) _imageCheck else _imageNormal)
        }
    }

    fun setTextVisible(fVisible:Boolean){
        _text.visibility=if(fVisible) View.VISIBLE else View.GONE
    }
}