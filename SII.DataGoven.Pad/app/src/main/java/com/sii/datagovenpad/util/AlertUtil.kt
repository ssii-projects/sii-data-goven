package com.sii.datagovenpad.util

import android.app.AlertDialog
import android.content.Context
import com.sii.datagovenpad.dialog.DialogResult

enum class MessageBoxButtons{
    YesNo,
    YesNoCancel
}
object AlertUtil {
    fun alert(context: Context, msg: String?) {
        AlertDialog.Builder(context)
            .setTitle("警告")
            .setMessage(msg)
            .setNegativeButton("关闭"){_,_->}
//            .setPositiveButton("确定"){_,_->onOK()}
            .create().show()
//        AlertDialog.Builder(context)
//            .setTitle("警告")
//            .setMessage(msg)
//            .setNegativeButton(
//                "取消"
//            ) { dialog, which -> }
//            .setPositiveButton(
//                "确定"
//            ) { dialog, which -> }
//            .create().show()
    }

    /**
     * 非阻塞式，若需阻塞式请使用ConformDialog
     *
     * */
    fun showConfirm(context: Context,msg:String,btns:MessageBoxButtons,onOK:(DialogResult)->Unit){
        val builder=AlertDialog.Builder(context)

        if(btns!=MessageBoxButtons.YesNoCancel) {
            builder.setCancelable(false)
        }else{
            builder.setOnCancelListener{
                onOK(DialogResult.Cancel)
            }
        }

        builder.setTitle("确认")
        builder.setMessage(msg)
        if(btns==MessageBoxButtons.YesNoCancel) {
            builder.setNegativeButton("取消") { _, _ -> onOK(DialogResult.Cancel) }
        }
        builder.setNegativeButton("否"){_,_->onOK(DialogResult.No)}
        builder.setPositiveButton("是"){_,_->onOK(DialogResult.Yes)}
        builder.create().show()
    }
    fun info(context: Context, msg:String,onClose:(()->Unit)?=null){
        val builder=AlertDialog.Builder(context)
        builder.setOnCancelListener{
            onClose?.invoke()
        }
            builder.setTitle("提示")
            .setMessage(msg)
            .setNegativeButton("关闭"){_,_->onClose?.invoke()}
//            .setPositiveButton("确定"){_,_->onOK()}
            .create().show()
    }
    fun error(context: Context,msg:String){
        AlertDialog.Builder(context)
            .setTitle("错误")
            .setMessage(msg)
            .setNegativeButton("关闭"){_,_->}
//            .setPositiveButton("确定"){_,_->onOK()}
            .create().show()    }
}