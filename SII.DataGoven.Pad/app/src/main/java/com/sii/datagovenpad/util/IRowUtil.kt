package com.sii.datagovenpad.util

import ime.sii.gis.geodatabase.IRow
import java.text.SimpleDateFormat
import java.util.*

object IRowUtil {
    fun setNow(ft: IRow, fieldName:String){
        val format = SimpleDateFormat("yyyy-MM-dd HH:mm:ss")
        val t: String = format.format(Date())
        ft.setFieldValue(fieldName,t)
    }
}