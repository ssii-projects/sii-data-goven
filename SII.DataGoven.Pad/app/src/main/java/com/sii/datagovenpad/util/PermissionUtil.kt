package com.sii.datagovenpad.util

import android.Manifest
import android.app.Activity
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat

object PermissionUtil {
    fun requestPermissions(activity: Activity):Boolean{
        return isPermissionGranted(activity, arrayOf(
            Manifest.permission.READ_EXTERNAL_STORAGE,
            Manifest.permission.WRITE_EXTERNAL_STORAGE,
            Manifest.permission.ACCESS_FINE_LOCATION
        ))
    }
    private fun isPermissionGranted(activity: Activity,permissions:Array<String>): Boolean {
        val context = activity
        if (Build.VERSION.SDK_INT >= 23) {
            val lst = ArrayList<String>()
            for (permission in permissions) {
                val readPermissionCheck = ContextCompat.checkSelfPermission(context, permission)
                if (readPermissionCheck != PackageManager.PERMISSION_GRANTED) {
                    lst.add(permission);
                }
            }
            if (lst.isEmpty()) return true

            ActivityCompat.requestPermissions(activity, lst.toTypedArray(), 1)
        }
        return true
    }
}