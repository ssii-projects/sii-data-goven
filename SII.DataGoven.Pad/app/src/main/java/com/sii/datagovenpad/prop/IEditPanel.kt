package com.sii.datagovenpad.prop

import android.view.View

//interface IPanel{
//    fun getPanel(): View
//}
interface IEditPanel {//}:IPanel{
    fun getTitle():String
    fun isDirty():Boolean
    fun save()
}