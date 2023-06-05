using Agro.GIS;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using Agro.LibCore.UI;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Agro.Library.Common.Repository;
using Agro.Library.Common;

namespace Agro.Module.DataBrowse
{
    /// <summary>
    /// yxm 2018-3-15 行政地域树控件
    /// </summary>
    public partial class NaviTree : UserControl
    {
        private static TreeItemData _dummyNode = null;
        public class TreeItemData : TreeItemDataBase<TreeItemData>
        {
            static TreeItemData()
            {
                //_errImg = MyImageUtil.Image24("exclamation.png");
            }

            private static ImageSource icon = CommonImageUtil.Image16("Map.png");
            //private string _title;
            private bool _fBusy = false;

            /// <summary>
            /// 图层名称
            /// </summary>
            public string Title
            {
                get
                {
                    return Zone==null?null:Zone.Name;
                }
                //get { return _title; }
                //set { _title = value; NotifyPropertyChanged("Title"); }
            }
            private ImageSource _icon;
            /// <summary>
            /// 图层样式
            /// </summary>
            public ImageSource Icon
            {
                get { return _icon; }
                set { _icon = value; RaisePropertyChanged("Icon"); }
            }

            public bool IsBusy
            {
                get
                {
                    return _fBusy;
                }
                set
                {
                    _fBusy = value;
                    RaisePropertyChanged("IsBusy");
                    RaisePropertyChanged("IsReady");
                }
            }


            private bool _isExpanded;

            public ShortZone Zone { get; private set; }

            public bool IsExpanded
            {
                get { return _isExpanded; }
                set
                {
                    _isExpanded = value;
                    if (value)
                    {
                        if (Children.Count == 1 && Children[0] == _dummyNode)
                        {
                            FillChildren();
                        }
                    }
                    RaisePropertyChanged("IsExpanded");
                }
            }

            private readonly NaviTree _p;
            public TreeItemData(NaviTree p, ShortZone zone, TreeItemData parent=null)//, bool fExpanded=true)
                : base(parent)
            {
                _p = p;
                Zone = zone;
                _icon = icon;
            }

            internal IGeometry QueryShape()
            {
                IFeatureClass fc = null;
                if (Zone.Level == eZoneLevel.County)
                {
                    fc = _p._layerXian.FeatureClass;
                }
                else if (Zone.Level == eZoneLevel.Town)
                {
                    fc = _p._layerXiang.FeatureClass;
                }else if (Zone.Level == eZoneLevel.Village)
                {
                    fc = _p._layerCun.FeatureClass;
                }
                if (fc != null)
                {
                    return fc.GetShape(Zone.OID);
                }
                    return null;
            }

            internal void FillChildren()
            {
                Children.Clear();
                if (Zone == null)
                {
                    return;
                }
                if (_isExpanded)
                {
                    IFeatureLayer fl = null;
                    string subFields = null;
                    eZoneLevel level = eZoneLevel.City;
                    string where = null;
                    if (Zone.Level == eZoneLevel.County)
                    {
                        fl = _p._layerXiang;
                        subFields = "XJQYDM,XJQYMC";
                        level = eZoneLevel.Town;
                    }
                    else if (Zone.Level == eZoneLevel.Town)
                    {
                        fl = _p._layerCun;
                        subFields = "CJQYDM,CJQYMC";
                        level = eZoneLevel.Village;
                        where = "[CJQYDM] like '"+Zone.Code+"%'";
                    }
                    if (fl != null)
                    {
                        var qf = new QueryFilter();
                        subFields += "," + fl.FeatureClass.OIDFieldName;
                        qf.SubFields =subFields;
                        qf.WhereClause = where;
                        fl.FeatureClass.Search(qf, r =>
                        {
                            var code = r.GetValue(0).ToString();
                            var name = r.GetValue(1).ToString();
                            int oid = r.Oid;
                            var zone = new ShortZone(null, code, name, level, oid);
                            var n1 = new TreeItemData(_p,zone,this);
                            n1.FillChildren();
                            Children.Add(n1);
                            return true;
                        });

                    }
                    //var lst=ZoneUtil.QueryChildren(Zone);
                    //if (lst != null)
                    //{
                    //    foreach(var n in lst)
                    //    {
                    //        var n1 = new TreeItemData(n, this);
                    //        n1.FillChildren();
                    //        Children.Add(n1);
                    //    }
                    //}
                }
                else
                {
                    if (HasChildren(_p,Zone))
                    {
                        Children.Add(_dummyNode);
                    }
                }
            }
            private static bool HasChildren(NaviTree _p, ShortZone zone)
            {
                bool fHasChildren = false;
                IFeatureLayer fl = null;
                string subFields = null;
                //eZoneLevel level = eZoneLevel.City;
                string where = null;
                if (zone.Level == eZoneLevel.County)
                {
                    fl = _p._layerXiang;
                    subFields = "XJQYDM,XJQYMC";
                    //level = eZoneLevel.Town;
                }
                else if (zone.Level == eZoneLevel.Town)
                {
                    fl = _p._layerCun;
                    subFields = "CJQYDM,CJQYMC";
                    //level = eZoneLevel.Village;
                    where = "[CJQYDM] like '" + zone.Code + "%'";
                }
                if (fl != null)
                {
                    var qf = new QueryFilter();
                    qf.SubFields = subFields;
                    qf.WhereClause = where;
                    fl.FeatureClass.Search(qf, r =>
                    {
                        fHasChildren = true;
                        return false;
                        //var code = r.GetValue(0).ToString();
                        //var name = r.GetValue(1).ToString();
                        //var zone = new ShortZone(null, code, name, level, 0);
                        //var n1 = new TreeItemData(_p, zone, this);
                        //n1.FillChildren();
                        //Children.Add(n1);
                        //return true;
                    });

                }
                return fHasChildren;
            }
        }

        private readonly ObservableCollection<TreeItemData> _dataSoure = new ObservableCollection<TreeItemData>();

        private MapControl _map;
        private IFeatureLayer _layerXian;
        private IFeatureLayer _layerXiang;
        private IFeatureLayer _layerCun;

        public Action<TreeItemData> OnZoneSelected;

        public NaviTree()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            treeView.SelectedItemChanged += (s, e) =>
            {
                if (OnZoneSelected != null)
                {
                    var d=treeView.SelectedItem as TreeItemData;
                    OnZoneSelected(d == null ? null : d);//.Zone);// (s as TreeItemData).Zone);
                }
            };


            treeView.ItemsSource = _dataSoure;

        }

        public void Init(MapControl map, IFeatureLayer layerXian, IFeatureLayer layerXiang, IFeatureLayer layerCun)
        {
            _map = map;
            _layerXian = layerXian;
            _layerXiang = layerXiang;
            _layerCun = layerCun;

            var qf = new QueryFilter();
            qf.SubFields = "XZQDM,XZQMC,"+_layerXian.FeatureClass.OIDFieldName;
            _layerXian.FeatureClass.Search(qf, r =>
             {
                 var code = r.GetValue(0).ToString();
                 var name = r.GetValue(1).ToString();
                 var zone = new ShortZone(null, code, name, eZoneLevel.County, r.Oid);
				 RootNode = new TreeItemData(this, zone)
				 {
					 IsExpanded = true
				 };
				 RootNode.FillChildren();
                 _dataSoure.Add(RootNode);
                 TreeItemDataBase<TreeItemData>.SetSelectedItem(treeView, RootNode);
                 return false;
             });
        }

        public TreeItemData RootNode { get; set;}

        public ShortZone CurrentZone
        {
            get
            {
                var td=treeView.SelectedItem as TreeItemData;
                return td == null ? null : td.Zone;
            }
        }
    }
}
