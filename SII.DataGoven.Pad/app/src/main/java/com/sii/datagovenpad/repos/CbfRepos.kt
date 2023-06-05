package com.sii.datagovenpad.repos

import com.sii.datagovenpad.entities.EnCbf
import com.sii.util.DateTimeUtil

class CbfRepos :MyReposBase<EnCbf>(){
    companion object{
        private val _i:CbfRepos=CbfRepos()

        fun instance():CbfRepos{
            return _i
        }
    }

    fun updateWhenJtcyChanged(cbfbm:String):EnCbf?{
        val cbfEn=find("cbfbm='${cbfbm}'")
        if(cbfEn!=null){
            val cnt=CbfJtcyRepos.instance().count("cbfbm='${cbfbm}'")
            cbfEn.cbfCysl=cnt
            cbfEn.zhxgsj= DateTimeUtil.getCurrentTimeString()
            save(cbfEn, arrayListOf("zhxgsj","cbfCysl"))
        }
        return cbfEn
    }
}