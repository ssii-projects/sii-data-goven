package com.sii.datagovenpad

import android.app.Activity
import android.content.Context
import android.os.Environment
import com.sii.core.MyLog
import com.sii.datagovenpad.entities.EnCbf
import com.sii.datagovenpad.entities.EnCbfJtcy
import com.sii.datagovenpad.entities.EnDk
import com.sii.datagovenpad.util.CodeUtil
import com.sii.datagovenpad.util.LogUtils
import com.sii.datagovenpad.util.UtilsFile
import com.sii.datagovenpad.util.UtilsSDCard
import com.sii.util.AuthenticateUtil
import com.sii.util.FileUtil
import ime.sii.gis.geodatabase.IFeatureWorkspace
import ime.sii.gis.geodatabase.SqliteWorkspaceFactory.SqliteWorkspaceFactory
import jsqlite.Constants
import java.io.File

object MyGlobal {
    var mainDB: IFeatureWorkspace?=null
        private set
    lateinit var mainActivity: MainActivity
        private set
    /**
     *当前打开的项目
     */
    lateinit var curProject:ProjectInfo
        private set
    /**
     * 根路径下的所有项目
     */
    val allProjects = ArrayList<ProjectInfo>()

    lateinit var onCurProjectChanged:()->Unit
        private set

    fun startup(ma:MainActivity,cpc:()->Unit) {
        onCurProjectChanged=cpc
        mainActivity = ma
        AppDataPath.startup(allProjects)
        isAppAuthenticated(ma)
        curProject = allProjects[0]
        mainDB = curProject.open()
        cpc()
    }
    fun shutdown(){
        allProjects.forEach(){
            try {
                it.close()
            }catch (e:java.lang.Exception){
                MyLog.e(e.toString())
            }
        }
        allProjects.clear()
    }

    fun setCurProject(pi:ProjectInfo){
        if(curProject!=pi) {
            var newDb=pi.open()
            curProject.close()
            curProject = pi
            mainDB = newDb
            CodeUtil.clear()
            mainActivity.onCurProjectChanged()
            onCurProjectChanged()
        }
    }

    /**
     * 检查App是否已经授权
     * @return null表示已授权，否则返回申请码
     */
    private fun isAppAuthenticated(ctx: Context){
        val APPID="F03BC2ED-003C-4E39-8561-0B276270C9BD"
        var mAuthCode: String? = null

        var cid:String?=null

        val authFileName="${AppDataPath.rootPath}/Authorize.key"
        val file = File(authFileName)
        if (file.exists()) {
            mAuthCode = UtilsFile.getFileMsg(file)
            val mAuthorityInfo = AuthenticateUtil.IsKeyAuthenticated(ctx, mAuthCode, APPID)
            if (!mAuthorityInfo.isOK) {
                mAuthCode = null
                cid=mAuthorityInfo.ComputerID
            }
        }/*else{
            throw Exception("授权文件：${authFileName}不存在！")
        }*/
        if (mAuthCode == null) {
            if(cid==null){
                cid= AuthenticateUtil.getComputerID(ctx)
            }
            LogUtils.e("tag", "cpuid--------------------$cid")
            throw Exception("软件还未授权，申请码为：            $cid")
        }
//        if(err!=null) throw Exception(err)
    }
}



/**
 * @param dbName:数据库名称（含扩展名）
 */
class ProjectInfo (private val dbName:String){
    private var _db:IFeatureWorkspace?=null
    /**
     * 获取数据库完整路径
     */
    private inline fun dbPath():String=AppDataPath.rootPath+"/"+dbName
    fun projectName():String{
        return dbName.substring(0,dbName.length-".dk".length)
    }
    /**
     * 打开项目数据库
     */
    fun open():IFeatureWorkspace{
        if(_db!=null) return _db!!
        val wf = SqliteWorkspaceFactory.Instance() as SqliteWorkspaceFactory
        var dbFileName=dbPath()
        val db= wf.Open(dbFileName, Constants.SQLITE_OPEN_READWRITE) as IFeatureWorkspace
        arrayListOf<String>("DLXX_XZDY",EnDk.instance.GetTableName()
            ,EnCbf.instance.GetTableName()
            ,EnCbfJtcy.instance.GetTableName()).forEach(){
            if(!db.tableExists(it)){
                db.close()
                throw java.lang.Exception("文件[${projectName()}]格式异常，表名：${it}不存在！")
            }
        }
        _db=db
        return  db
    }
    fun close(){
        if(_db!=null){
            _db!!.close()
            _db=null
        }
    }
    override fun toString(): String {
        return projectName()
    }
}
/**
 * 管理项目路径
 */
object AppDataPath {
    /**
     * 根路径，比如ssii/CBJYQ（不以‘/’结尾）
     */
    lateinit var rootPath: String
    lateinit var rdbPath: String

    fun startup(allProjects: ArrayList<ProjectInfo>) {
        allProjects.clear()
        var error: String? = null
        val rootPathName = "CBJYQ";
        val sPath = UtilsSDCard.root() + "/sii/${rootPathName}"
        if (FileUtil.fileExists(sPath)) {
            val paths = File(sPath)
            // 判断SD卡是否存在，并且是否具有读写权限
            if (Environment.getExternalStorageState() == Environment.MEDIA_MOUNTED) {
                val files = paths.listFiles()// 读取文件夹下文件
                if (files != null) {
                    for (file in files!!) {
                        if (file.isDirectory) {
                            if (file.name == "rdb") {
                                rdbPath = file.absolutePath
                            }
                        } else {
                            val str = file.name.toLowerCase()
                            if (str.endsWith(".dk")) {
                                val dbName = file.name
                                allProjects.add(ProjectInfo(dbName))
                            }
                        }
                    }
                    //Log.e("tag", ss.toString())
                }
            }

            if (allProjects.size > 0) {
                rootPath = sPath
            } else {
                error = "sii/${rootPathName}目录/下未找到.dk文件"
            }
        } else {
            error = "未找到sii/${rootPathName}目录"
        }
        if (error != null) throw Exception(error)
    }
}