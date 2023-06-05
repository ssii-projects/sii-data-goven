package com.sii.datagovenpad.prop

import android.view.View
import com.sii.datagovenpad.R
import com.sii.datagovenpad.entities.EnDk
import ime.sii.gis.carto.IFeatureLayer
import ime.sii.gis.controls.tool.CreateFeatureTool
import ime.sii.gis.geodatabase.IFeature

class PropViewDk(isCreateModel:Boolean)
    :FeaturePropView(R.layout.prop_dk,R.id.PROP_DK_LL,if(isCreateModel)"新建地块" else "地块属性",isCreateModel){

    override fun getCodeTypeByFieldName(fieldName:String):String?{
        return EnDk.instance.getFieldCodeType(fieldName)
    }

    override fun finalConstruct() {
//        if(isCreateModel) {
        view.findViewById<View>(R.id.DK_DKBM).visibility = View.GONE
        view.findViewById<View>(R.id.DK_CBFBM).visibility = View.GONE
//        }
    }
    override fun logicCheck():String?{
        val iFeature=mFeature!!

//        val saFields= arrayOf("DKMC","DKLB")
        var str=checkField(iFeature,"DKMC","地块名称")
        if(str!=null) return str
        str=checkField(iFeature,"DKLB","地块类别")
        if(str!=null) return str
        return null
    }
    private fun checkField(iFeature: IFeature,fieldName:String,aliasName:String):String?{
        var str=iFeature.getFieldValue(fieldName)?.toString()
        if(str==null||str.trim().isEmpty()){
            return "${aliasName}不能为空！"
        }
        return null
    }
}