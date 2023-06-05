package com.sii.datagovenpad.dialog

import android.widget.Button
import android.widget.TextView
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.R
import com.sii.dialog.BlockingDialog
enum class DialogResult(value:Int){
    Ok(0),
    Yes(1),
    No(2),
    Cancel(3)
}
/**
 * 阻塞式
 */
class ConformDialog {

    var title="提示"
    fun showDialog(msg:String):DialogResult{
        var res=DialogResult.No
        val dlg=BlockingDialog(MyGlobal.mainActivity, R.layout.conform_dialog)
        dlg.findViewById<Button>(R.id.btn_conform_dialog_no).setOnClickListener{
            res=DialogResult.No
            dlg.endDialog(res.ordinal)
        }
        dlg.findViewById<Button>(R.id.btn_conform_dialog_yes).setOnClickListener{
            res=DialogResult.Yes
            dlg.endDialog(res.ordinal)
        }
        dlg.findViewById<TextView>(R.id.conform_dialog_title).setText(title)
        dlg.findViewById<TextView>(R.id.conform_dialog_content).setText(msg)
        dlg.showDialog()
        return res
    }
}