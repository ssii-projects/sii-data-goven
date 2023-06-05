package com.sii.datagovenpad.util

import android.content.Context
import android.content.pm.ApplicationInfo
import android.util.Log
import java.util.*


/**
 * 一个日志的工具类 可以开启和关闭打印日志 最好不要用System打印，消耗内存。
 */
object LogUtils {
    /**
     * 获取是否DEBUG模式
     *
     * @return
     */
    var isDebugable = true // 默认开启
        private set
    private const val defaultTag = "tag" // log默认的 tag
    private const val TAG_CONTENT_PRINT = "%s.%s:%d"

    /**
     * 获得当前的 堆栈
     *
     * @return
     */
    private val currentStackTraceElement: StackTraceElement
        get() = Thread.currentThread().stackTrace[4]

    /**
     * 设置 debug是否启用 根据 判断 是否 为上线模式 android:debuggable 打包后变为false，没打包前为true
     *
     *
     * 要在application中 首先进行调用此方法 对 isLogEnabled 进行赋值
     *
     * @param context
     * @return
     */
    fun setDebugable(context: Context) {
        try {
            val info = context.applicationInfo
            isDebugable = info.flags and ApplicationInfo.FLAG_DEBUGGABLE != 0
        } catch (e: Exception) {
            // 友盟上报错误日志
            // MobclickAgent.reportError(context, e);
        }
    }

    /**
     * 打印的log信息 类名.方法名:行数--->msg
     *
     * @param trace
     * @return
     */
    private fun getContent(trace: StackTraceElement): String {
        return String.format(
            Locale.CHINA,
            TAG_CONTENT_PRINT,
            trace.className,
            trace.methodName,
            trace.lineNumber
        )
    }

    /**
     * debug
     *
     * @param tag
     * @param msg
     */
    fun d(tag: String?, msg: String, tr: Throwable?) {
        if (isDebugable) {
            Log.d(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg,
                tr
            )
        }
    }

    /**
     * debug
     *
     * @param tag
     * @param msg
     */
    fun d(tag: String?, msg: String) {
        if (isDebugable) {
            // getContent(getCurrentStackTraceElement())
            Log.d(tag, "--->$msg")
        }
    }

    /**
     * debug
     *
     * @param msg
     */
    fun d(msg: String) {
        if (isDebugable) {
            Log.d(
                defaultTag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * error
     *
     * @param tag
     * @param msg
     */
    fun e(tag: String?, msg: String, tr: Throwable?) {
        if (isDebugable) {
            Log.e(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg,
                tr
            )
        }
    }

    /**
     * error
     *
     * @param tag
     * @param msg
     */
    fun e(tag: String?, msg: String) {
        if (isDebugable) {
            Log.e(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * error
     *
     * @param msg
     */
    fun e(msg: String) {
        if (isDebugable) {
            Log.e(
                defaultTag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * info
     *
     * @param tag
     * @param msg
     */
    fun i(tag: String?, msg: String, tr: Throwable?) {
        if (isDebugable) {
            Log.i(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg,
                tr
            )
        }
    }

    /**
     * info
     *
     * @param tag
     * @param msg
     */
    fun i(tag: String?, msg: String) {
        if (isDebugable) {
            Log.i(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * info
     *
     * @param msg
     */
    fun i(msg: String) {
        if (isDebugable) {
            Log.i(
                defaultTag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * verbose
     *
     * @param tag
     * @param msg
     */
    fun v(tag: String?, msg: String, tr: Throwable?) {
        if (isDebugable) {
            Log.v(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg,
                tr
            )
        }
    }

    /**
     * verbose
     *
     * @param tag
     * @param msg
     */
    fun v(tag: String?, msg: String) {
        if (isDebugable) {
            Log.v(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * verbose
     *
     * @param msg
     */
    fun v(msg: String) {
        if (isDebugable) {
            Log.v(
                defaultTag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * warn
     *
     * @param tag
     * @param msg
     */
    fun w(tag: String?, msg: String, tr: Throwable?) {
        if (isDebugable) {
            Log.w(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg,
                tr
            )
        }
    }

    /**
     * warn
     *
     * @param tag
     * @param msg
     */
    fun w(tag: String?, msg: String) {
        if (isDebugable) {
            Log.w(
                tag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }

    /**
     * warn
     *
     * @param msg
     */
    fun w(msg: String) {
        if (isDebugable) {
            Log.w(
                defaultTag,
                getContent(currentStackTraceElement) + "--->" + msg
            )
        }
    }
}