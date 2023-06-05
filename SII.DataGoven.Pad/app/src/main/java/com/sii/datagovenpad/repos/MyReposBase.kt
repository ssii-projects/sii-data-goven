package com.sii.datagovenpad.repos

import com.sii.datagovenpad.MyGlobal
import com.sii.orm.EntityBase
import com.sii.repos.ReposBase
import ime.sii.gis.geodatabase.IFeatureWorkspace

abstract class  MyReposBase<T : EntityBase<T>?> : ReposBase<T>(){
    override fun db(): IFeatureWorkspace {
        return MyGlobal.mainDB!!
    }
}