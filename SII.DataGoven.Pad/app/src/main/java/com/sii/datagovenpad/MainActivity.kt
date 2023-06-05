package com.sii.datagovenpad

import android.os.Bundle
import android.view.*
import android.widget.EditText
import android.widget.ViewFlipper
import androidx.appcompat.app.AppCompatActivity
import com.sii.datagovenpad.control.SideBar
import com.sii.datagovenpad.dialog.DialogResult
import com.sii.datagovenpad.dialog.KeyBackProcessor
import com.sii.datagovenpad.map.ControlMapGPSLocate
import com.sii.datagovenpad.map.control.*
import com.sii.datagovenpad.popup.LayerPopup
import com.sii.datagovenpad.popup.QuyuPopup
import com.sii.datagovenpad.popup.cbf.CbfListView
import com.sii.datagovenpad.prop.PropViewDk
import com.sii.datagovenpad.util.AlertUtil
import com.sii.datagovenpad.util.MessageBoxButtons
import com.sii.datagovenpad.util.PermissionUtil
import com.sii.datagovenpad.view.MapDocument
import com.sii.datagovenpad.view.MapView
import com.sii.location.GpsManager
import com.sii.util.InputMethodUtil
import ime.sii.gis.carto.IFeatureLayer
import ime.sii.gis.carto.IFeatureSelection
import ime.sii.gis.controls.MapControl
import ime.sii.gis.controls.tool.CreateFeatureTool
import ime.sii.gis.geodatabase.IFeature
import ime.sii.gis.geodatabase.IFeatureWorkspace
import kotlinx.android.synthetic.main.activity_main.*

class FeatureItem(val featureLayer: IFeatureLayer, val oid: Int,val feature:IFeature?)
 open class MapHost:AppCompatActivity(),IMapHost {

     lateinit var sideBarRight: SideBarRight
     protected val mMapView by lazy { MapView(mapControl) }
     protected val mapCmds = ArrayList<IMapCommand>()
     override val tgtLayers = ArrayList<IFeatureLayer>()
     override val mSelectedFeatures = ArrayList<FeatureItem>()
     override fun getMapControl(): MapControl {
         return mapControl
     }

     private fun updateToolVisibleState() {
         for (it in mapCmds) {
             it.updateUI()
             if (it is IMapTool) {
                 it.btn.setCheck(it.getTool() == mapControl.currentTool)
             }
         }
         if (tgtLayers.size == 0) {
             showToolBar(false)
         }
     }

     override fun refillSelectedFeatures(fUpdateToolVisibleState: Boolean) {
         mSelectedFeatures.clear()
         for (fl in tgtLayers) {
             var fs = fl as IFeatureSelection
             for (oid in fs.getrefSelectionSet()) {
                 mSelectedFeatures.add(FeatureItem(fl, oid, null))
             }
         }
         if (fUpdateToolVisibleState) {
             mapControl.firUpdateCommandUI()
         }
     }

     protected fun initMap() {
         mapCmds.add(PanCommand(tbPan, this))
         mapCmds.add(SaveCommand(tbCommit, this))
         mapCmds.add(CancelCommand(tbCancel, this))
         mapCmds.add(ClearMeasureCommand(tbClearMeasure, this))
         mapCmds.add(ModifyFeatureCommand(tbModifyFeature, this))
         mapCmds.add(MeasureAreaCommand(tbMeasureArea, this))
         mapCmds.add(MeasureLengthCommand(tbMeasureLength, this))
         mapCmds.add(UndoCommand(tbUndo, this))
         mapCmds.add(RedoCommand(tbRedo, this))
         mapCmds.add(CutCommand(tbCut, this))
         mapCmds.add(CutAngleCommand(tbCutAngle, this))
         mapCmds.add(ExtendEdgeCommand(tbAddEdge, this))
         mapCmds.add(IdentifyCommand(tbProperty, this))
         mapCmds.add(CreateFeatureCommand(tbCreateFeature, this))
         mapCmds.add(SelectToolCommand(tbSingleSelect, this))
         mapCmds.add(DeleteCommand(tbDelete, this))

         for (cmd in mapCmds) {
             if (cmd is IMapTool) {
                 cmd.btn.setOnClickListener {
                     cmd.active()
                     mapControl.firUpdateCommandUI()
                 }
             }
         }

         mapControl.AddOnUpdateCommandUIListener {
             this.updateToolVisibleState()
         }
     }

     private fun showToolBar(fVisible: Boolean) {
         svToolBar.visibility = if (fVisible) View.VISIBLE else View.GONE
     }

//     fun tryHideProperty() {
//         val top = sideBarRight.top();
//         if (top != null && top.tag is FeaturePropView) {
//             sideBarRight.pop()
//         }
//     }

     override fun showProperty(
         feat: IFeature?,
         tgtLyr: IFeatureLayer?,
         mCreateFeatureTool: CreateFeatureTool?
     ) {
         try {
             if (tgtLyr == null&&feat!=null) {
                 sideBarRight.showFeatureCreateDialog(mMapView, feat, mCreateFeatureTool)
             } else {
                 sideBarRight.showFeatureProperty(feat, tgtLyr, mCreateFeatureTool)
             }
         } catch (ex: java.lang.Exception) {
             AlertUtil.alert(this, ex.message)
         }
     }
 }
class MainActivity : MapHost() {
    private  val keyBack= KeyBackProcessor()
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        try {
            if (!PermissionUtil.requestPermissions(this)) {
                throw Exception("请确认权限后重新打开应用！")
            }
            setContentView(R.layout.activity_main)
            sideBarRight= SideBarRight(side_bar_right_frame)
            MyGlobal.startup(this) {
                tvTitle.text = MyGlobal.curProject.projectName()
//                sideBarRight.cbfPopup.doSearch()
            }

            btnSideRight.setOnClickListener{
                sideBarRight.toggleVisible()
                btnSideRight.text=if(sideBarRight.isVisible()) "》" else "《"
            }
            initMap()
            tbFullExtent.setOnClickListener{mMapView.zoomToFullExtent()}
            tvTitle.setOnClickListener { QuyuPopup().showPopup(v_prop_anchor, mMapView) }
            tbUser.setOnClickListener {
                sideBarRight.showCbfProperty()
            }
            initView(MyGlobal.mainDB!!)
            tgtLayers.add(MapDocument.mLayerDk)
            mapControl.mapEditor.targetLayer=MapDocument.mLayerDk
//            MapUtil.findLayersByTableName(mapControl.map, "VEC_SURVEY_DK", tgtLayers)
            mapControl.firUpdateCommandUI()
        } catch (e: Exception) {
            val ev = EditText(this)
            ev.setText("错误：${e.message}")
            setContentView(ev)
        }
    }

    override fun dispatchTouchEvent(ev: MotionEvent?): Boolean {
        InputMethodUtil.hideSoftInputFromWindow(mapControl.rootView);
        return super.dispatchTouchEvent(ev)
    }
    fun updateCmdUI(){
        btnSideRight.text=if(sideBarRight.isVisible()) "》" else "《"
    }

    override fun onKeyDown(keyCode: Int, event: KeyEvent?): Boolean {
        keyBack.checkKeyBack(keyCode,event){
            AlertUtil.showConfirm(this,"确定退出吗？",MessageBoxButtons.YesNoCancel){
                if(it==DialogResult.Yes){
                    finish()
                }
            }
        }
        if(keyCode==KeyEvent.KEYCODE_BACK) return true
        return super.onKeyDown(keyCode, event)
    }
//    override fun onStop() {
//        mapControl?.dispose()
//        MyGlobal.shutdown()
//        super.onStop()
//    }
    override fun onDestroy() {
        mapControl?.dispose()
        MyGlobal.shutdown()
        super.onDestroy()
    }

    override fun onCreateOptionsMenu(menu: Menu): Boolean {
        // Inflate the menu; this adds items to the action bar if it is present.
        menuInflater.inflate(R.menu.menu_main, menu)
        return true
    }

    override fun onOptionsItemSelected(item: MenuItem): Boolean {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        return when (item.itemId) {
            R.id.action_settings -> true
            else -> super.onOptionsItemSelected(item)
        }
    }
    fun onCurProjectChanged(){
        sideBarRight.onCurProjectChanged()
    }
//    private fun zoomToFullExtent(){
//        val map=mMapView.mapControl.map
//        if(map.layerCount>0) {
//            map.extent = MapDocument.mLayerDk.extent
//            map.refresh()
//        }
//    }
    private fun initView(db: IFeatureWorkspace) {
//        try {
            mapControl.init(this)
            mMapView.init(db)

            tbLayer.setOnClickListener { LayerPopup().showPopup(v_prop_anchor, mapControl) }


            //添加GPS位置支持
            GpsManager.instance.startUp(this)
            GpsManager.instance.registerListener()
            //定位功能
            ControlMapGPSLocate(mapControl, tbLocate, this.getDrawable(R.mipmap.location)!!)
//        } catch (e: Exception) {
//            LogUtils.e("tag", e.toString())
//        }
    }
}

class SideBarRight(vf: ViewFlipper):SideBar(vf){
    private val propViewDk:PropViewDk by lazy { PropViewDk(false) }
    private val propViewDkCreate:PropViewDk by lazy { PropViewDk(true) }
    val cbfPopup:CbfListView by lazy { CbfListView(vf.context) }
    init {
        super.onVisibleChanged={
            MyGlobal.mainActivity.updateCmdUI()
        }
    }

    fun showFeatureCreateDialog(mv: MapView,feat:IFeature,mCreateFeatureTool:CreateFeatureTool?){
//        propViewDkCreate.updateUI(mCreateFeatureTool,feat,null)
        propViewDkCreate.showCreate(feat,mCreateFeatureTool)
    }
    fun showFeatureProperty(feat:IFeature?, tgtLyr:IFeatureLayer?, mCreateFeatureTool:CreateFeatureTool?) {
        if(feat==null){
            if(this.top()==propViewDk.frame) {
                showContent(null)
            }
        }else {
            showContent(propViewDk.frame){
                propViewDk.updateUI(mCreateFeatureTool, feat, tgtLyr)
            }
        }
    }
    fun showCbfProperty(){
        showContent(cbfPopup.view)
        if(cbfPopup.count()==0){
            cbfPopup.doSearch()
        }
    }
    fun onCurProjectChanged(){
        while (viewContainer.viewCount>1){
            viewContainer.pop(false)
        }
        hide()
        cbfPopup.clear()
    }
}

