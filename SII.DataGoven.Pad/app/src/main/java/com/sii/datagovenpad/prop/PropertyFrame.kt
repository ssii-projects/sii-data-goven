package com.sii.datagovenpad.prop

import android.content.Context
import android.view.View
import android.widget.FrameLayout
import android.widget.PopupWindow
import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.R
import com.sii.datagovenpad.control.FormInfoTopBar
import com.sii.datagovenpad.popup.PopupBase
import com.sii.datagovenpad.view.MapView
import com.sii.functional.Action
import com.sii.functional.Cancel
import com.sii.widget.HookPopupWindow

/**
 * 放置所有属性页的框架
 */
class PropertyFrame : PopupBase() {

    fun showPopup(anchor: View, mv: MapView, contentView: View,title:String, onSave:()->Unit){//}: PopupWindow {
        val mc = mv.mapControl
        val context = mc.context
        val with = defWidth(context)
        val high = mc.height

        val view = View.inflate(mc.context, R.layout.property_frame, null)

        val fl = view.findViewById<FrameLayout>(R.id.fl_prop_frame)
        fl.addView(contentView)

//        val wnd= createPopWindow(context, view, with, high, anchor)
        FormInfoTopBar(null,view,title){
            onSave()
        }

        MyGlobal.mainActivity.sideBarRight.showContent(view)
//        topBar.showReturnButton(false)
//        return wnd
    }

    override fun createPopWindow(context: Context, view: View, with: Int, high: Int, anchor: View,onPreDismiss:((cancel: Cancel)->Unit)?): PopupWindow {
        val wnd = HookPopupWindow(view, with, high)
        wnd.onPreDismiss= Action{
            onPreDismiss?.invoke(it)
        }
        wnd.isOutsideTouchable = true
        //wnd.setBackgroundDrawable(getDrawable(context))
        wnd.update()
        wnd.isTouchable = true
        wnd.isFocusable = true
        //wnd.animationStyle = R.style.PopupAnimation
        wnd.showAsDropDown(anchor)
        return wnd
    }
}
