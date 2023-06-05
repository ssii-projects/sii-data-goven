package com.sii.datagovenpad.dialog

import android.app.AlertDialog
import android.view.KeyEvent
import android.view.View
import android.widget.FrameLayout
import com.sii.functional.Cancel
import com.sii.functional.CancelItem

class KeyBackProcessor{
    private var preTime: Long = 0
    fun checkKeyBack(keyCode: Int, event: KeyEvent?,onKeyBack:()->Unit){
        if(keyCode== KeyEvent.KEYCODE_BACK){
            val ct=System.currentTimeMillis()
            val delta=ct-preTime
            if (delta> 300) {
                onKeyBack()
            }
            preTime=System.currentTimeMillis()
        }
    }
}
class CustomAlertDialog(val contentView: View) {
    val dialog:AlertDialog
    var onPreDismiss:((CancelItem<DialogResult>)->Unit)?=null

    private  val keyBack=KeyBackProcessor()
    init{
        val builder = AlertDialog.Builder(contentView.context)
        builder.setView(contentView)
        dialog = builder.create()
        dialog.setCanceledOnTouchOutside(false)
        dialog.setOnDismissListener(){
            val parent=contentView.parent
            if(parent is FrameLayout) {
                parent.removeView(contentView)
            }
        }

        dialog.setOnKeyListener { _, keyCode, event ->
            processKeyBack(keyCode, event)
        }

    }
    fun show(){
        dialog.show()
    }
    fun dismiss(type:DialogResult, fireEvent:Boolean=true){
        if(fireEvent) {
            if (onPreDismiss != null) {
                val cancel = CancelItem<DialogResult>()
                cancel.item=type
                onPreDismiss?.invoke(cancel)
                if (cancel.cancel) return
            }
        }
        dialog.dismiss()
    }
    private fun processKeyBack(keyCode: Int, event: KeyEvent):Boolean{
        keyBack.checkKeyBack(keyCode,event){
            dismiss(DialogResult.Cancel)
        }
        return keyCode== KeyEvent.KEYCODE_BACK
    }
}