package com.sii.datagovenpad.util

import java.text.SimpleDateFormat
import java.util.*


object UtilsDateTime {
    val year: String
        get() = SimpleDateFormat("yyyy")
            .format(Date(System.currentTimeMillis()))

    val dateForID: String
        get() {
            return SimpleDateFormat("yyyyMMdd")
                .format(Date(System.currentTimeMillis()))
        }

    val fileName: String
        get() {
            return SimpleDateFormat("yyyyMMddHHmmSS")
                .format(Date(System.currentTimeMillis()))
        }
}
