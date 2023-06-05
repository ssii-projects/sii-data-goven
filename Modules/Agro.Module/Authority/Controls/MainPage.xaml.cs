/*
 * (C) 2015  公司版权所有,保留所有权利 
 */
using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Repository;

namespace Agro.Module.Authority
{
	public partial class MainPage : UserControl
    {
        public class FuncItem : INotifyPropertyChanged
        {
            public string Title { get; set; }
            public bool IsExpanded
            {
                get { return _isExpanded; }
                set
                {
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }

            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    if (_isChecked != value)
                    {
                        Set_Children_Check(value);
                        Set_Parent_Check(value);
                    }
                }
            }
            public ImageSource IconPath
            {
                get { return _bitmapImg; }
                set { _bitmapImg = value; NotifyPropertyChanged("IconPath"); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private FuncItem _parentNode;
            private ObservableCollection<FuncItem> _children = new ObservableCollection<FuncItem>();
            private ImageSource _bitmapImg;
            private bool _isExpanded;
            private bool _isChecked;

            public FuncItem ParentNode
            {
                get { return _parentNode; }
                private set { _parentNode = value; }
            }
            public ObservableCollection<FuncItem> Children
            {
                get { return _children; }
                set { _children = value; }
            }

            //public NewMetadata Data;
            public FuncItem(FuncItem parentNode)
            {
                ParentNode = parentNode;
            }

            public bool IsLeaf
            {
                get
                {
                    return Children.Count == 0;
                }
            }
            #region Private Properties And Methods
            private void Set_Children_Check(bool value)
            {
                this._isChecked = value;
                //if (Layer is IFeatureLayer)
                //{
                //    (Layer as IFeatureLayer).Selectable = value;
                //}
                NotifyPropertyChanged("IsChecked");
                foreach (FuncItem c in Children)
                    c.Set_Children_Check(value);
            }

            private void NotifyPropertyChanged(string info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
            private void Set_Parent_Check(bool value)
            {
                if (_parentNode != null)
                {
                    List<FuncItem> _lst_else = _parentNode.Children.Where(t => t.IsChecked == true).ToList();
                    int checkCount = _lst_else.Count();
                    if (checkCount == _parentNode.Children.Count && value == true)
                    {
                        _parentNode.IsChecked = value;
                        _parentNode.Set_Parent_Check(value);
                    }
                    else if (checkCount < _parentNode.Children.Count && value == false && checkCount > 0)
                    {
                        _parentNode.IsChecked = value;
                        _parentNode.Set_Parent_Check(value);
                        _lst_else.ForEach((t) => { t.IsChecked = true; });
                    }
                }
            }
            #endregion
        }


        private bool _fSuppressEvent = false;

		private readonly UserRepository userRepository = new UserRepository(MyGlobal.Workspace);
		/// <summary>
		/// 构造函数:初始化数据字典窗体
		/// </summary>
		public MainPage()
        {
            InitializeComponent();
            UpdateUsersBox();
            var lst = GetFuncItems();
            lbModules.ItemsSource = lst;

            btnAddUser.Click += (s, e) =>
            {
                var dlg = new KuiDialog(Window.GetWindow(this), "添加用户");
                //var dlg = new ShowDialogUtil(Window.GetWindow(this), bdrMask);
                var pnl = new AddUserPanel();
                dlg.Content = pnl;
                dlg.BtnOK.Click += (s1, e1) =>
                {
                    var fOK = pnl.OnAddUserOK();
                    if (fOK)
                    {
                        UpdateUsersBox();
                        dlg.Close();
                    }
                };
                dlg.ShowDialog();
                //dlg.ShowDialog("添加用户", pnl, () =>
                //  {
                //      var fOK= pnl.OnAddUserOK();
                //      if (fOK)
                //      {
                //          UpdateUsersBox();
                //      }
                //      return fOK;
                //  });
            };
            btnDelUser.Click += (s, e) =>
            {
                if (lbUsers.SelectedItem != null)
                {
					try
					{
						var selUserName = lbUsers.SelectedItem.ToString();
						if (selUserName == "admin")
						{
							MessageBox.Show("不能删除admin用户！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						var mr = MessageBox.Show("确定要删除用户：" + selUserName + "吗?", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
						if (mr != MessageBoxResult.OK)
						{
							return;
						}

						userRepository.Delete(t => t.Name == selUserName);
						MyGlobal.Workspace.ExecuteNonQuery($"delete from CS_USER_PERMISSION where USERNAME='{ selUserName }'");
						UpdateUsersBox();
					}
					catch { }
                }
            };
            btnModifyPwd.Click += (s, e) =>
            {
                if (lbUsers.SelectedItem != null)
                {
                    var dlg = new KuiDialog(Window.GetWindow(this), "设置密码");// new ShowDialogUtil(Window.GetWindow(this), bdrMask);
                    var pnl = new AddUserPanel(false);
                    dlg.Content = pnl;
                    dlg.BtnOK.Click += (s1, e1) =>
                    {
                        var fOK = pnl.OnModifyPwdOK(lbUsers.SelectedItem.ToString());
                        if (fOK)
                        {
                            UpdateUsersBox();
                            dlg.Close();
                        }
                    };
                    dlg.ShowDialog();
                    //dlg.ShowDialog("设置密码", pnl, () =>
                    //{
                    //    var fOK = pnl.OnModifyPwdOK(lbUsers.SelectedItem.ToString());
                    //    if (fOK)
                    //    {
                    //        UpdateUsersBox();
                    //    }
                    //    return fOK;
                    //});
                }
            };
            lbUsers.SelectionChanged += (s, e) => { UpdateToolButtonUI(); UpdateFuncListUI(); };
			MyGlobal.OnDatasourceChanged += UpdateUsersBox;
			Loaded += (s, e) =>
            {
                UpdateToolButtonUI();
                UpdateFuncListUI();
            };
        }

        private IEnumerable<string> QueryUsers()
        {
			return userRepository.FindDistinctBy(t=>t.Name, u => !u.Buildin).Select(o=>o.ToString());
        }

        private void UpdateUsersBox()
        {
            lbUsers.ItemsSource = null;
            lbUsers.ItemsSource = QueryUsers();

        }
        private void UpdateToolButtonUI()
        {
            btnDelUser.IsEnabled = lbUsers.SelectedItem != null;
            btnModifyPwd.IsEnabled= lbUsers.SelectedItem != null;
        }
        private void UpdateFuncListUI()
        {
            _fSuppressEvent = true;
            try
            {
                bool fEnable = lbUsers.SelectedItem != null && lbUsers.SelectedItem.ToString() != "admin";
                lbModules.IsEnabled = fEnable;
                if (fEnable)
                {
                    string sUserName = lbUsers.SelectedItem.ToString();
					try
					{
						if (lbModules.ItemsSource is ObservableCollection<FuncItem> lst)
						{
							foreach (var fi in lst)
							{
								fi.IsChecked = false;
								if (!fi.IsLeaf)
								{
									foreach (var fic in fi.Children)
									{
										fic.IsChecked = false;
									}
								}
							}

							var sql = $"select FUNCTION_ID from CS_USER_PERMISSION where USERNAME='{ sUserName.Replace("'", "''") }'";
							MyGlobal.Workspace.QueryCallback(sql, r =>
							 {
								 var fid = r.GetString(0);
								 var it = lst.ToList().Find(fi =>
								  {
									  if (!fi.IsLeaf)
									  {
										  foreach (var fic in fi.Children)
										  {
											  if (fic.Title == fid)
											  {
												  return true;
											  }
										  }
									  }
									  return fi.Title == fid;
								  });
								 if (it != null)
								 {
									 it.IsChecked = true;
								 }
								 return true;
							 });

							lbModules.ItemsSource = null;
							lbModules.ItemsSource = lst;
						}
					}
					catch { }
                }
                else
                {
					if (lbModules.ItemsSource is ObservableCollection<FuncItem> lst)
					{
						foreach (var fi in lst)
						{
							fi.IsChecked = true;
						}
						lbModules.ItemsSource = null;
						lbModules.ItemsSource = lst;
					}
				}
            }
            finally
            {
                _fSuppressEvent = false;
            }
        }
		private ObservableCollection<FuncItem> GetFuncItems(List<Agro.FrameWork.Module> modules, ObservableCollection<FuncItem> funcItems=null)
		{
			if (funcItems == null)
				funcItems = new ObservableCollection<FuncItem>();
			foreach (var m in modules)
			{
				FuncItem fi = null;
				if (!string.IsNullOrEmpty(m.Title))
				{
					fi = new FuncItem(null)
					{
						Title = m.Title,
						IconPath = m.Icon32,
						IsExpanded = true
					};
					funcItems.Add(fi);
					GetFuncItems(m.Children, fi.Children);
				}
			}
			return funcItems;
		}
        private ObservableCollection<FuncItem> GetFuncItems()
        {
			return GetFuncItems(MyGlobal.MainWindow.Modules);
		}


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_fSuppressEvent)
            {
                return;
            }
            var fi = (sender as CheckBox).Tag as FuncItem;
            if (fi.IsLeaf)
            {
                try
                {
                    var sUserName = lbUsers.SelectedItem.ToString();
                    var db = MyGlobal.Workspace;
                    //using (var db = DataBaseSource.GetDatabase())
                    {
                        var sql = "insert into CS_USER_PERMISSION(USERNAME,FUNCTION_ID,FLAG) values('" + sUserName + "','" + fi.Title + "',1)";
                        db.ExecuteNonQuery(sql);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            //CheckLayerSelectable((sender as CheckBox).Tag as IFeatureLayer, true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_fSuppressEvent)
            {
                return;
            }
            //CheckLayerSelectable((sender as CheckBox).Tag as IFeatureLayer, false);
            var fi = (sender as CheckBox).Tag as FuncItem;
            if (fi.IsLeaf)
            {
                try
                {
                    var sUserName = lbUsers.SelectedItem.ToString();
                    var db = MyGlobal.Workspace;
                    //using (var db = DataBaseSource.GetDatabase())
                    {
                        var sql = "delete from CS_USER_PERMISSION where USERNAME='" + sUserName + "' and FUNCTION_ID='" + fi.Title + "'";
                        db.ExecuteNonQuery(sql);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        //private void OnFuncItemChecked(object sender, RoutedEventArgs e)
        //{
        //    var btn = sender as CheckBox;
        //    var fi = btn.Tag as FuncItem;
        //    try
        //    {
        //        var sUserName = lbUsers.SelectedItem.ToString();
        //        using (var db = DataBaseSource.GetDatabase())
        //        {
        //            var sql = "insert into CS_USER_PERMISSION(USERNAME,FUNCTION_ID,FLAG) values('" + sUserName+"','"+fi.Title+"',1)";
        //            db.ExecuteNonQuery(sql);
        //        }
        //    }
        //    catch(Exception ex) {
        //        Console.WriteLine(ex);
        //    }
        //}

        //private void OnFuncItemUnChecked(object sender, RoutedEventArgs e)
        //{
        //    var btn = sender as CheckBox;
        //    var fi = btn.Tag as FuncItem;
        //    try
        //    {
        //        var sUserName = lbUsers.SelectedItem.ToString();
        //        using (var db = DataBaseSource.GetDatabase())
        //        {
        //            var sql = "delete from CS_USER_PERMISSION where USERNAME='" + sUserName + "' and FUNCTION_ID='" + fi.Title + "'";
        //            db.ExecuteNonQuery(sql);
        //        }
        //    }
        //    catch(Exception ex) {
        //        Console.WriteLine(ex);
        //    }

        //}
    }
}
