package com.sii.datagovenpad.control

import android.view.View
import android.widget.ViewFlipper
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.prop.IEditPanel
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import com.sii.view.stackview.StackViewContainer
import java.lang.Exception

open class  SideBar(vf: ViewFlipper){
//    private val fadeInAnimation: Animation
    protected val viewContainer: StackViewContainer = StackViewContainer(vf)
    var onVisibleChanged:(()->Unit)?=null

    init{
        //        fadeInAnimation=AnimationUtils.loadAnimation(contaner.context, R.anim.right_enter)
        hide()
        vf.setOnClickListener{

        }
    }
    fun isVisible():Boolean{
        return viewContainer.containerView.visibility==View.VISIBLE
    }
    fun toggleVisible(){
        if(isVisible()) hide() else show()
    }
    fun show(){
        if(!this.isVisible()) {
            viewContainer.containerView.visibility = View.VISIBLE
            onVisibleChanged?.invoke()
        }
    }

    fun showContent(contentView: View?,onBeforeShow:(()->Unit)?=null){
        val ep=top()?.tag
        if(ep is IEditPanel && ep.isDirty()) {
            AlertUtil.showConfirm(viewContainer.containerView.context, "${ep.getTitle()}已改变，是否保存",MessageBoxButtons.YesNo) {
                try {
                    if (it==DialogResult.Yes) {
                        ep.save()
                    }
                    onBeforeShow?.invoke()
                    doShow(contentView)
                } catch (e: Exception) {
                    AlertUtil.error(viewContainer.containerView.context, e.toString())
                }
            }
            return
        }
        onBeforeShow?.invoke()
        doShow(contentView)
    }
    private fun doShow(contentView:View?){

        while (viewContainer.viewCount>1){
            viewContainer.pop()
        }
        if(contentView!=null){
            viewContainer.push(contentView)
        }
        if(!this.isVisible()&&contentView!=null) {
            viewContainer.containerView.visibility = View.VISIBLE
            onVisibleChanged?.invoke()
        }
    }
    fun hide(){
        val ep=top()?.tag
        if(ep is IEditPanel && ep.isDirty()) {
            AlertUtil.showConfirm(viewContainer.containerView.context, "${ep.getTitle()}已改变，是否保存",MessageBoxButtons.YesNo) {
                try {
                    if (it==DialogResult.Yes) {
                        ep.save()
                    }
                    doHide()
                } catch (e: Exception) {
                    AlertUtil.error(viewContainer.containerView.context, e.toString())
                }
            }
            return
        }
        doHide()
    }
    private fun doHide(){
        viewContainer.containerView.visibility=View.GONE
        onVisibleChanged?.invoke()
    }
    fun top():View?{
        return viewContainer.top
    }
//    fun pop(){
//        viewContainer.pop()
//        if(viewContainer.viewCount==0){
//            hide()
//        }
//    }
}