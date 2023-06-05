package com.sii.datagovenpad.util

import android.app.Activity
import android.os.Environment
import android.os.StatFs


/**
 * 操作SD卡
 * Created by Administrator on 2017/7/19 0019.
 */
object UtilsSDCard {
    fun requestPermission(activity: Activity?) {
//        if (ContextCompat.checkSelfPermission(activity, Manifest.permission.WRITE_EXTERNAL_STORAGE)
//                != PackageManager.PERMISSION_GRANTED || ContextCompat.checkSelfPermission(activity, Manifest.permission.READ_EXTERNAL_STORAGE)
//                != PackageManager.PERMISSION_GRANTED) {
//            AndPermission.with(activity)
//                    .requestCode(100)
//                    .permission(Manifest.permission.WRITE_EXTERNAL_STORAGE, Manifest.permission.READ_EXTERNAL_STORAGE)
//                    .send();
//        }
    }

    // 判断允许读写
    val isRWEnable: Boolean
        get() = Environment.getExternalStorageState() == Environment.MEDIA_MOUNTED

    // 获取SDCard文件路径
    fun root(): String? {
        var ok: String? = null
        if (isRWEnable) {
            ok = Environment.getExternalStorageDirectory().path
        }
        return ok
    }

    // 获取SDCard总容量大小(bytes)
    fun total(): Double {
        var ok = 0.0
        if (root()!!.isNotEmpty()) {
            val statfs = StatFs(root())
            ok = statfs.blockCount * statfs.blockSize.toDouble()
        }
        return ok
    }

    // 获取SDCard总容量大小(bytes)
    fun totalString(): String? {
        var ok: String? = null
        run {
            var free = total()
            if (free >= 1024.0 * 1024 * 1024) {
                free /= 1024.0 * 1024 * 1024
                ok = UtilsValueOf.doubleToString(free, 2).toString() + "GB"
            } else {
                free /= 1024.0 * 1024
                ok = free.toString() + "MB"
            }
        }
        return ok
    }

    // 获取SDCard可用容量大小(bytes)
    fun free(): Double {
        var ok = 0.0
        if (!root()!!.isEmpty()) {
            val statfs = StatFs(root())
            ok =
                statfs.availableBlocks.toDouble() * statfs.blockSize.toDouble()
        }
        return ok
    }

    // 获取SDCard可用容量大
    fun freeString(): String? {
        var ok: String? = null
        run {
            var free = free()
            if (free >= 1024.0 * 1024 * 1024) {
                free /= 1024.0 * 1024 * 1024
                ok = UtilsValueOf.doubleToString(free, 2).toString() + "GB"
            } else {
                free /= 1024.0 * 1024
                ok = free.toString() + "MB"
            }
        }
        return ok
    }
}
