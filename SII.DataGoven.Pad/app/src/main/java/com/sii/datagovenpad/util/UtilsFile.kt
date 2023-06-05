package com.sii.datagovenpad.util

import android.content.Context
import java.io.File
import java.io.FileInputStream
import java.io.FileOutputStream
import java.io.IOException
import java.util.*


object UtilsFile {
    // 判断文件/文件夹是否已经存在
    fun isExist(path: String?): Boolean {
        return File(path).exists()
    }

    // 判断文件扩展名
    fun isExt(file: String, ext: String): Boolean {
        return file.toLowerCase().endsWith(ext.toLowerCase())
    }

    // 判断文件是否是AVI文件
    fun isAviFile(file: String): Boolean {
        return isExt(file, ".avi")
    }

    // 判断文件是否是MP4文件
    fun isMp4File(file: String): Boolean {
        return isExt(file, ".mp4")
    }

    // 判断文件是否是BMP文件
    fun isBmpFile(file: String): Boolean {
        return isExt(file, ".bmp")
    }

    // 判断文件是否是PNG文件
    fun isPngFile(file: String): Boolean {
        return isExt(file, ".png")
    }

    // 判断文件是否是JPG文件
    fun isJpgFile(file: String): Boolean {
        return isExt(file, ".jpg") || isExt(file, ".jpeg")
    }

    // 判断文件是否是TIF文件
    fun isTifFile(file: String): Boolean {
        return isExt(file, ".tif") || isExt(file, ".tiff")
    }

    // 判断文件是否是DICOM文件
    fun isDcmFile(file: String): Boolean {
        return isExt(file, ".dcm")
    }

    // 返回一个与时间有在的文件名
    fun nameWithTime(): String {
        return UtilsDateTime.fileName
    }

    // 返回一个与时间有在的文件名(扩展名必须以"."开头)
    fun nameWithTime(ext: String): String {
        return UtilsDateTime.fileName.toString() + ext
    }

    // 返回一个与时间有在的文件名(扩展名必须以"."开头)
    fun nameWithTime(path: String, ext: String): String {
        return if (path.endsWith("/") || path.endsWith("\\")) path + UtilsDateTime.fileName
            .toString() + ext else path + "/" + UtilsDateTime.fileName + ext
    }

    // 创建目录
    fun createFolder(folder: String?): Boolean {
        return File(folder).mkdir()
    }

    // 删除目录
    fun deleteFolder(folder: String?): Boolean {
        val file = File(folder)
        return deleteFolder(file)
    }

    // 删除文件
    fun deleteFile(file: String): Boolean {
        var ok = false
        if (file.isNotEmpty()) {
            val hFile = File(file)
            if (hFile.exists() && !hFile.isDirectory) {
                hFile.delete()
                ok = true
            }
        }
        return ok
    }

    // 重命名文件夹
    fun renameFolder(srcFolder: String?, dstFolder: String?): Boolean {
        val src = File(srcFolder)
        val dst = File(dstFolder)
        return src.renameTo(dst)
    }

    // 重命名文件
    fun renameFile(srcFile: String?, dstFile: String?): Boolean {
        val src = File(srcFile)
        val dst = File(dstFile)
        return src.renameTo(dst)
    }

    // 复制资源文件夹
    fun copyResource(
        context: Context,
        srcFolder: String,
        dstFolder: String
    ): Boolean {
        var ok = false
        try {
            val fileNames = context.assets.list(srcFolder)
            if (fileNames.isNotEmpty()) {
                val file = File(dstFolder)
                file.mkdirs()
                for (fileName in fileNames) {
                    copyResource(
                        context,
                        "$srcFolder/$fileName",
                        "$dstFolder/$fileName"
                    )
                }
            } else {
                val file = File(dstFolder)
                if (!file.exists()) {
                    val `is` = context.assets.open(srcFolder)
                    val fos =
                        FileOutputStream(File(dstFolder))
                    val buffer = ByteArray(1024)
                    var byteCount = 0
                    while (`is`.read(buffer).also { byteCount = it } != -1) {
                        fos.write(buffer, 0, byteCount)
                    }
                    fos.flush()
                    `is`.close()
                    fos.close()
                }
            }
            ok = true
        } catch (e: Exception) {
            e.printStackTrace()
        }
        return ok
    }

    // 复制文件夹
    @Throws(IOException::class)
    fun copyFolder(srcFolder: String?, dstFolder: String?): Boolean {
        val src = File(srcFolder)
        val dst = File(dstFolder)
        return copyFolder(src, dst)
    }

    // 复制文件
    @Throws(IOException::class)
    fun copyFile(srcFile: String?, dstFile: String?): Boolean {
        val src = File(srcFile)
        val dst = File(dstFile)
        return copyFile(src, dst)
    }

    // 移动文件夹
    @Throws(IOException::class)
    fun moveFolder(srcFolder: String?, dstFolder: String?): Boolean {
        val src = File(srcFolder)
        val dst = File(dstFolder)
        return moveFolder(src, dst)
    }

    // 移动文件
    @Throws(IOException::class)
    fun moveFile(srcFile: String?, dstFile: String?): Boolean {
        val src = File(srcFile)
        val dst = File(dstFile)
        return moveFile(src, dst)
    }

    // 查询文件
    fun queryOneFile(path: String, filter: String): String {
        var ok = ""
        if (!path.isEmpty()) {
            val exts =
                Arrays.asList(*filter.split(";".toRegex()).toTypedArray())
            val query = File(path)
            if (query.isDirectory) {
                for (i in query.listFiles().indices) {
                    val fileName = query.listFiles()[i].absolutePath
                    for (n in exts.indices) {
                        if (fileName.endsWith(exts[n])) {
                            ok = fileName
                            break
                        }
                    }
                    if (!ok.isEmpty()) break
                }
            }
        }
        return ok
    }

    /**
     * 查询文件
     *
     * @param path
     * @param filter =".bmp;.png;.jpg;.tif;.dcm;.avi";
     * @return
     */
    fun queryFiles(
        callBack: refreshCallBack?,
        path: String,
        filter: String,
        list: MutableList<String?>
    ) {
        Thread(Runnable {
            if (!path.isEmpty()) {
                val exts =
                    Arrays.asList(
                        *filter.split(";".toRegex()).toTypedArray()
                    )
                val query = File(path)
                if (query.isDirectory) {
                    for (i in query.listFiles().indices) {
                        val fileName = query.listFiles()[i].absolutePath
                        for (n in exts.indices) {
                            if (fileName.endsWith(exts[n])) {
                                list.add(fileName)
                                callBack?.refreshData()
                                try {
                                    Thread.sleep(10)
                                } catch (e: InterruptedException) {
                                    e.printStackTrace()
                                }
                                break
                            }
                        }
                    }
                }
            }
        }).start()
    }

    // 删除目录
    private fun deleteFolder(hFile: File?): Boolean {
        var ok = false
        if (hFile != null && hFile.exists() && !hFile.isFile) {
            for (file in hFile.listFiles()) {
                if (file.isFile) {
                    file.delete()
                } else if (file.isDirectory) {
                    deleteFolder(file) // 递归
                }
            }
            hFile.delete()
            ok = true
        }
        return ok
    }

    // 删除文件
    private fun deleteFile(hFile: File): Boolean {
        return !hFile.isDirectory && hFile.delete()
    }

    // 复制文件夹
    @Throws(IOException::class)
    private fun copyFolder(hSrc: File, hDst: File): Boolean {
        var ok = false
        if (hSrc.isDirectory && hDst.isDirectory) {
            hDst.mkdir()
            if (hDst.exists()) {
                val files = hSrc.listFiles()
                for (i in files.indices) {
                    if (files[i].isFile) {
                        copyFile(
                            files[i],
                            File(hDst.path + "//" + files[i].name)
                        )
                    } else if (files[i].isDirectory) {
                        copyFolder(
                            files[i],
                            File(hDst.path + "//" + files[i].name)
                        )
                    }
                }
                ok = true
            }
        }
        return ok
    }

    // 复制文件
    @Throws(IOException::class)
    private fun copyFile(hSrc: File, hDst: File): Boolean {
        var ok = false
        if (!hSrc.isDirectory && !hDst.isDirectory) {
            val srcStream = FileInputStream(hSrc)
            val dstStream = FileOutputStream(hDst)
            var readLen = 0
            val buf = ByteArray(1024)
            while (srcStream.read(buf).also { readLen = it } != -1) {
                dstStream.write(buf, 0, readLen)
            }
            dstStream.flush()
            dstStream.close()
            srcStream.close()
            ok = true
        }
        return ok
    }

    // 移动文件夹
    @Throws(IOException::class)
    private fun moveFolder(hSrc: File, hDst: File): Boolean {
        var ok = false
        if (hSrc.isDirectory && hDst.isDirectory) {
            val files = hSrc.listFiles()
            for (i in files.indices) {
                if (files[i].isFile) {
                    moveFile(
                        files[i],
                        File(hDst.path + "//" + files[i].name)
                    )
                    deleteFile(files[i])
                } else if (files[i].isDirectory) {
                    moveFolder(
                        files[i],
                        File(hDst.path + "//" + files[i].name)
                    )
                    deleteFolder(files[i])
                }
            }
            ok = true
        }
        return ok
    }

    // 移动文件
    @Throws(IOException::class)
    private fun moveFile(hSrc: File, hDst: File): Boolean {
        var ok = false
        if (copyFile(hSrc, hDst)) {
            ok = deleteFile(hSrc)
        }
        return ok
    }

    fun getFileMsg(path: File?): String? {
        var s: String? = null
        try {
            val inputStream = FileInputStream(path)
            val b = ByteArray(inputStream.available())
            inputStream.read(b)
            s = String(b)
//            LogUtils.e("tag", s)
        } catch (e: Exception) {
            e.printStackTrace()
        }
        return s
    }

    //读取指定目录下的所有TXT文件的文件名
    fun getFileName(
        files: Array<File>?,
        st: String?
    ): List<String> {
        val str: MutableList<String> =
            ArrayList()
        if (files != null) {    // 先判断目录是否为空，否则会报空指针
            for (file in files) {
                if (file.isDirectory) { //检查此路径名的文件是否是一个目录(文件夹)
                    getFileName(file.listFiles(), st)
                } else {
                    val fileName = file.name
                    if (fileName.endsWith(st!!)) {
                        val s =
                            fileName.substring(0, fileName.lastIndexOf("."))
                        str.add(s)
                    }
                }
            }
        }
        return str
    }

    interface refreshCallBack {
        fun refreshData()
    }
}
