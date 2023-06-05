package com.sii.datagovenpad.util

import com.sii.datagovenpad.MyGlobal
import com.sii.datagovenpad.repos.DkRepos
import com.sii.util.StringUtil

class CodeItem(val bm:String,val mc:String){
    override fun toString(): String {
        return mc
    }
}
object CodeUtil {
    private val dicCodeItems=HashMap<String,ArrayList<CodeItem>>()
    fun clear(){
        dicCodeItems.clear()
    }
    fun code2Name(codeType: String,code:String?):String?{
        if(StringUtil.isNullOrEmpty(code)) return null
        if(!dicCodeItems.containsKey(codeType)){
            queryCodeItems(codeType)
        }
        val lst=dicCodeItems.get(codeType)!!
        val ci=lst.find {it.bm==code}
        var name=ci?.mc ?: code
        if(codeType=="承包方类型"){
            name+="承包"
        }
        return name
    }
    fun queryCodeItems(codeType: String): ArrayList<CodeItem> {
        if(dicCodeItems.containsKey(codeType)){
            return dicCodeItems.get(codeType)!!
        }
        var lst=ArrayList<CodeItem>()
        when(codeType){
            "是否基本农田"->{
                lst.add(CodeItem("1", "基本农田"))
                lst.add(CodeItem("2","非基本农田"))
            }
            "变更类型"->{
                lst.add(CodeItem("", ""))
                lst.add(CodeItem("新增","新增"))
                lst.add(CodeItem("分割","分割"))
                lst.add(CodeItem("图形变更","图形变更"))
            }
            "土地利用类型"->queryTdlylxItems(lst)
            "发包方编码"->{
                val fbfbms= DkRepos.instance().queryFbfbms()
                for(fbfbm in fbfbms){
                    lst.add(CodeItem(fbfbm,fbfbm))
                }
            }
            else->{
                val sql =
                    "select BM,MC from xtpz_sjzd where SJID in(select id from xtpz_sjzd where mc='${codeType}' and id is not null) and ID is not null and MC is not null and BM is not null order by BM"
                queryCodes(sql,lst)
            }
        }
//        if(codeType=="是否基本农田"){
////            lst=ArrayList<CodeItem>()
//            lst.add(CodeItem("1", "基本农田"))
//            lst.add(CodeItem("2","非基本农田"))
//        }else if(codeType=="变更类型"){
////            lst=ArrayList<CodeItem>()
//            lst.add(CodeItem("", ""))
//            lst.add(CodeItem("新增","新增"))
//            lst.add(CodeItem("分割","分割"))
//            lst.add(CodeItem("图形变更","图形变更"))
//        }else  if (codeType == "土地利用类型") {
//            queryTdlylxItems(lst)
//        }else {
//            val sql =
//                "select BM,MC from xtpz_sjzd where SJID in(select id from xtpz_sjzd where mc='${codeType}' and id is not null) and ID is not null and MC is not null and BM is not null order by BM"
//            queryCodes(sql,lst)
//        }
        dicCodeItems.put(codeType,lst)
        return lst
    }

    fun queryTdlylxItems(lst:ArrayList<CodeItem>): ArrayList<CodeItem> {
        val sql =
            "SELECT BM,MC FROM XTPZ_TDLYXZFL where SDL = '农用地' and SJBM is not null and BM is not null and MC is not null order by BM"
        return queryCodes(sql,lst)
    }

    private fun queryCodes(sql: String,lst:ArrayList<CodeItem>): ArrayList<CodeItem> {
        lst.add(CodeItem("",""))
        val db = MyGlobal.mainDB!!
        val cursor = db.query(sql)
        while (cursor.next()) {
            val bm = cursor.getString(0)
            val mc = cursor.getString(1)
            lst.add(CodeItem(bm, mc))
        }
        return lst;
    }
}