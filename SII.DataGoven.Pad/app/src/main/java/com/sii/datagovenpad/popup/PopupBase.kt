package com.sii.datagovenpad.popup

import android.content.Context
import android.graphics.drawable.Drawable
import android.graphics.drawable.ShapeDrawable
import android.graphics.drawable.shapes.OvalShape
import android.view.View
import android.view.WindowManager
import android.widget.PopupWindow
import androidx.core.content.ContextCompat
import com.sii.datagovenpad.R
import com.sii.functional.Action
import com.sii.functional.Cancel
import com.sii.util.DensityUtil
import com.sii.widget.HookPopupWindow

open class PopupBase {
    /**
     * 生成一个 透明的背景图片
     *
     * @return
     */
    protected fun getDrawable(context: Context): Drawable {
        val bgdrawable = ShapeDrawable(OvalShape())
        bgdrawable.paint.color = ContextCompat.getColor(context, android.R.color.transparent)// this@MainActivity.getResources().getColor(android.R.color.transparent)
        return bgdrawable
    }

    protected fun defWidth(context:Context):Int{
        return  DensityUtil.dip2px(context, 340f)
    }

    protected open fun createPopWindow(context: Context, view:View,with:Int,high:Int,anchor:View,onPreDismiss:((cancel:Cancel)->Unit)?=null): PopupWindow{
        val wnd = HookPopupWindow(view, with, high)
//        wnd.canDismiss=false
        wnd.onPreDismiss= Action {
            onPreDismiss?.invoke(it)
        }
        wnd.isOutsideTouchable=true
        wnd.isTouchable = true
        wnd.isFocusable = true
//        wnd.isOutsideTouchable = true
        wnd.setBackgroundDrawable(getDrawable(context))
        wnd.update()
        wnd.inputMethodMode=WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE or WindowManager.LayoutParams.SOFT_INPUT_STATE_VISIBLE


        wnd.animationStyle = R.style.RightEnterAnimation
        wnd.showAsDropDown(anchor)
        return wnd
    }
}