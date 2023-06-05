using Agro.Library.Common.Util;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using Agro.Library.Common.Repository;
using Agro.LibCore;
using Agro.GIS;
using Agro.LibCore.Database;
using Agro.Library.Model;

namespace Agro.Library.Common
{
    public interface IZoneTreeModel
    {
        ShortZone FindRootZone();
        ShortZone FindRootZone(SEC_ID_USER user);
        List<ShortZone> QueryChildren(ShortZone zone, List<ShortZone> lst = null);
        bool HasChildren(ShortZone zone, bool fContainGroupNode = false);
        void HasChildren(List<ShortZone> lst, Action<ShortZone, bool> callback, bool fContainGroupNode = false);
    }
	/// <summary>
	/// yxm 2018-3-15 行政地域树控件
	/// </summary>
	public partial class ZoneTree : UserControl,IDisposable
    {
        private static TreeItemData _dummyNode = null;
        public class TreeItemData : TreeItemDataBase<TreeItemData>
        {
            static TreeItemData()
            {
                //_errImg = MyImageUtil.Image24("exclamation.png");
            }

            private static ImageSource icon = CommonImageUtil.Image16("region.png");
            //private string _title;
            private bool _fBusy = false;

            /// <summary>
            /// 图层名称
            /// </summary>
            public string Title
            {
                get
                {
                    return Zone?.Name;
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
                set { _icon = value; RaisePropertyChanged(nameof(Icon)); }
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
                    RaisePropertyChanged(nameof(IsBusy));
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
                    RaisePropertyChanged(nameof(IsExpanded));
                }
            }
			private readonly ZoneTree _p;

			public TreeItemData(ZoneTree p,ShortZone zone, TreeItemData parent=null)
                : base(parent)
            {
				_p = p;
                Zone = zone;
                _icon = icon;
            }
			public string GetPath()
			{
				var lst = new List<string>();
				var p = this;
				while (p != null)
				{
					lst.Insert(0,p.Zone.Name);
					p = p.Parent as TreeItemData;
				}
				return string.Join("/", lst);
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
                    var lst=_p._repository.QueryChildren(Zone);
                    if (lst != null)
                    {
						if (Zone.Level == Model.eZoneLevel.Town && !_p.ContainGroupNode)
						{
							lst.ForEach(n => Children.Add(new TreeItemData(_p, n, this)));
						}
						else
						{
							_p._repository.HasChildren(lst, (n, hasChildren) =>
							 {
								 var n1 = new TreeItemData(_p, n, this);
								 if (hasChildren)
								 {
									 n1.Children.Add(_dummyNode);
								 }
								 Children.Add(n1);
							 }, _p.ContainGroupNode);
						}
                    }
                }else
                {
                    if (_p._repository.HasChildren(Zone,_p.ContainGroupNode))
                    {
                        Children.Add(_dummyNode);
                    }
                }
            }	
        }


        //private DlxxXzdyRepository _repository { get { return DlxxXzdyRepository.Instance; } }
        public IZoneTreeModel _repository = DlxxXzdyRepository.Instance;

        private readonly ObservableCollection<TreeItemData> _dataSoure = new ObservableCollection<TreeItemData>();

        public Action<ShortZone> OnZoneSelected;
		public Action<ShortZone> OnItemDoubleClick;
		/// <summary>
		/// 是否包含组节点
		/// </summary>
		public bool ContainGroupNode { get; set; }
        public ZoneTree()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
			treeView.MouseDoubleClick += (s, e) =>
			  {
				  if (treeView.SelectedItem is TreeItemData td)
				  {
					  OnItemDoubleClick?.Invoke(td.Zone);
				  }
			  };
			treeView.SelectedItemChanged += (s, e) =>
			{
				OnZoneSelected?.Invoke((treeView.SelectedItem as TreeItemData)?.Zone);
			};
            treeView.ItemsSource = _dataSoure;
            MyGlobal.OnDatasourceChanged += OnDatasourceChanged;
			Loaded += (s, e) => { if (RootNode == null) Refresh(); };
		}
        public void FinalConstruct(IFeatureWorkspace db)
        {
            DlxxXzdyRepository.Instance.FinalConstruct(db);
        }
        public TreeItemData RootNode { get; set;}

        public ShortZone CurrentZone
        {
            get
            {
                return (treeView.SelectedItem as TreeItemData)?.Zone;
            }
        }
		public string GetCurrentPath()
		{
			if (treeView.SelectedItem is TreeItemData tid)
			{
				return tid.GetPath();
			}
			return "";
		}

		private void Refresh()
		{
			_dataSoure.Clear();
            var zone = _repository.FindRootZone(MyGlobal.LoginUser);
			RootNode = new TreeItemData(this,zone)
			{
				IsExpanded = true,
			};
			RootNode.FillChildren();

            _dataSoure.Add(RootNode);
            TreeItemDataBase<TreeItemData>.SetSelectedItem(treeView, RootNode);
		}

        private void OnDatasourceChanged()
        {
            Try.Catch(() => Refresh(), false);
        }
        public void Dispose()
        {
            MyGlobal.OnDatasourceChanged -= OnDatasourceChanged;
        }
    }
}
