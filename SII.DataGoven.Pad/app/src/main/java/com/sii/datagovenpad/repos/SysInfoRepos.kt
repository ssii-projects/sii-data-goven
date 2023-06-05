package com.sii.datagovenpad.repos

import com.sii.database.IWorkspace
import com.sii.datagovenpad.MyGlobal
import com.sii.util.SafeConvertAux

object SysInfoRepos {
    val KEY_DKMJSCALE = "AB85771F-00D7-4978-9A5D-F57104159DAC"
    fun db(): IWorkspace {return MyGlobal.mainDB!!}
    fun loadInt(key: String, defVal: Int = 2): Int {
        val s = load(key) ?: return defVal
        return SafeConvertAux.safeConvertToInt(s)
    }

    fun load(key: String): String? {
        var value: String? = null
        val sql = "select VALUE from CS_SYSINFO where ID='" + key + "'"
        db().queryCallback(sql) {
            it.Continue = false
            value = it.getString(0)
        }
        return value
    }
}