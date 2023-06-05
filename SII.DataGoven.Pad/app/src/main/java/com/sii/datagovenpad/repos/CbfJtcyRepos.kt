package com.sii.datagovenpad.repos

import com.sii.datagovenpad.entities.EnCbfJtcy

class CbfJtcyRepos :MyReposBase<EnCbfJtcy>(){
    companion object{
        private val _i:CbfJtcyRepos=CbfJtcyRepos()

        fun instance():CbfJtcyRepos{
            return _i
        }
    }
}