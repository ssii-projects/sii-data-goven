package com.sii.datagovenpad.repos

import com.sii.datagovenpad.entities.DKFields
import com.sii.datagovenpad.entities.EnDk

class DkRepos :MyReposBase<EnDk>(){
    companion object{
        private val _i:DkRepos=DkRepos()

        fun instance():DkRepos{
            return _i
        }
    }

    fun queryFbfbms():ArrayList<String>{
        val lst=ArrayList<String>()
        val sql = "select distinct ${DKFields.fbfbm} from ${DKFields.TB_NAME} where ${DKFields.fbfbm} is not null"
        db().queryCallback(sql) {
            lst.add(it.getString(0))
        }
        return lst
    }

//    override fun newEntity(): EnDk {
//       return EnDk()
//    }
}