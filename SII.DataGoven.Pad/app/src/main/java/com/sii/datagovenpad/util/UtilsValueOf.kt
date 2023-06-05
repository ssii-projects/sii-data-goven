package com.sii.datagovenpad.util

import java.text.DecimalFormat


object UtilsValueOf {
    fun stringToBoolean(str: String?): Boolean {
        var ok = false
        try {
            ok = java.lang.Boolean.valueOf(str)
        } catch (e: Exception) {
        }
        return ok
    }

    fun stringToByte(str: String?): Byte {
        var ok: Byte = 0
        try {
            ok = java.lang.Byte.valueOf(str!!)
        } catch (e: Exception) {
        }
        return ok
    }

    fun stringToInt(str: String?): Int {
        var ok = 0
        try {
            ok = Integer.valueOf(str!!)
        } catch (e: Exception) {
        }
        return ok
    }

    fun stringToLong(str: String?): Long {
        var ok: Long = 0
        try {
            ok = java.lang.Long.valueOf(str!!)
        } catch (e: Exception) {
        }
        return ok
    }

    fun stringToFloat(str: String?): Float {
        var ok = 0.0f
        try {
            ok = java.lang.Float.valueOf(str!!)
        } catch (e: Exception) {
        }
        return ok
    }

    fun stringToDouble(str: String?): Double {
        var ok = 0.0
        try {
            ok = java.lang.Double.valueOf(str!!)
        } catch (e: Exception) {
        }
        return ok
    }

    fun booleanToString(v: Boolean): String? {
        var ok: String? = null
        try {
            ok = v.toString()
        } catch (e: Exception) {
        }
        return ok
    }

    fun byteToString(v: Byte): String? {
        var ok: String? = null
        try {
            ok = v.toString()
        } catch (e: Exception) {
        }
        return ok
    }

    fun intToString(v: Int): String? {
        var ok: String? = null
        try {
            ok = v.toString()
        } catch (e: Exception) {
        }
        return ok
    }

    fun longToString(v: Long): String? {
        var ok: String? = null
        try {
            ok = v.toString()
        } catch (e: Exception) {
        }
        return ok
    }

    fun floatToString(v: Float, dec: Int): String? {
        var ok: String? = null
        try {
            ok = DecimalFormat("###.##").format(v.toDouble()).toString()
        } catch (e: Exception) {
        }
        return ok
    }

    fun doubleToString(v: Double, dec: Int): String? {
        var ok: String? = null
        try {
            ok = DecimalFormat("###.##").format(v).toString()
        } catch (e: Exception) {
        }
        return ok
    }
}