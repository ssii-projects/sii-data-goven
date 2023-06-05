package com.sii.datagovenpad.map.control

import com.sii.datagovenpad.FeatureItem
import ime.sii.gis.carto.IFeatureLayer
import ime.sii.gis.controls.MapControl
import ime.sii.gis.controls.tool.CreateFeatureTool
import ime.sii.gis.geodatabase.IFeature

interface IMapHost{
    val mSelectedFeatures:ArrayList<FeatureItem>
    val tgtLayers:ArrayList<IFeatureLayer>
    fun getMapControl():MapControl
//    fun updateToolVisibleState()
    fun refillSelectedFeatures(fUpdateToolVisibleState: Boolean = true)
    fun showProperty(feat: IFeature?, tgtLyr:IFeatureLayer?=null, mCreateFeatureTool: CreateFeatureTool?=null)
}