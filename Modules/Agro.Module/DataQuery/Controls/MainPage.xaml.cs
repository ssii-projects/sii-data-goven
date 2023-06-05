/*
 * (C) 2015  xx公司版权所有,保留所有权利 
 */

using Agro.Library.Common;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Agro.Module.DataQuery
{
    /// <summary>
    /// 查询统计主界面
    /// </summary>
    public partial class MainPage :UserControl// NavigatableWorkpageFrame
    {
        //private NavigationPanelTreeView _navigationPanelTreeView;

        //public NavigationPanelTreeView NavigationPanelTreeView
        //{
        //    get { return _navigationPanelTreeView; }
        //    set
        //    {
        //        _navigationPanelTreeView = value;
        //    }
        //}

        //private Navigator _navigator = null;
        //private NavigatorTreeView _navigator;
        #region Ctor

        /// <summary>
        /// 构造函数:初始化数据字典窗体
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            //SingleInstance = true;
            navTree.Init(contentPnl);
        }

        //public void InitUI(DataQueryContext ctx)
        //{
        //    //FrameworkElement panel = null;
        //    var navigationPanal = new NavigationPanelTreeView();
        //    _navigator = new NavigatorTreeView(Workpage, navigationPanal);// as NavigationPanelTreeView);
        //    _navigator.SelectedItemChanged += _navigator_SelectedItemChanged;
        //    _navigator.Install();
        //    _navigationPanelTreeView = navigationPanal;
        //    //DbContext = DatabaseInstance.GetDataBaseSource();
        //    contentPnl.Init(DataBaseSource.GetDatabase(),ctx);
        //    contentPnl.bdrMask = bdrMask;
        //    _navigationPanelTreeView.Init(contentPnl);

        //    //_navigator.SelectedItem
        //}
        #endregion
    }
}
