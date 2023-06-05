package com.sii.datagovenpad.control

import android.view.View
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import com.sii.datagovenpad.R
import com.sii.view.stackview.StackViewContainer


class FormInfoTopBar(val stackView: StackViewContainer?,val view: View,title:String,onSave:(()->Unit)?=null) {
    val btnSave:Button
    val btnReturn:View
    private val tvTitle:TextView
    init{
        btnReturn=view.findViewById<View>(R.id.form_info_top_bar_btn_return)
        btnReturn.setOnClickListener{stackView?.pop()}
        tvTitle=view.findViewById(R.id.form_into_top_bar_title)
        tvTitle.text = title
        btnSave=view.findViewById(R.id.form_info_top_bar_btn_save)
        if(onSave!=null){
            showSaveButton(true)
            btnSave.setOnClickListener{onSave()}
        }
        if(stackView==null){
            showReturnButton(false)
        }

    }
    fun getTile():String{
        return tvTitle.text.toString()
    }
    fun setTitle(title:String){
        tvTitle.text=title
    }
    fun showSaveButton(fShow:Boolean){
        btnSave.visibility=if(fShow) View.VISIBLE else View.GONE
    }
    fun showReturnButton(fShow:Boolean){
        btnReturn.visibility=if(fShow) View.VISIBLE else View.GONE
        if(!fShow){
            val tvp=tvTitle.layoutParams
            if(tvp is LinearLayout.LayoutParams) {
                tvp.leftMargin=10
                tvTitle.layoutParams = tvp
            }
        }
    }
}