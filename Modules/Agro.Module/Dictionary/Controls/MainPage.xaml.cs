/*
 * (C) 2015  公司版权所有,保留所有权利 
 */
using Agro.LibCore;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using Agro.Library.Common;
using System.Collections.Generic;

namespace Agro.Module.Dictionary
{
    /// <summary>
    /// 数据字典主界面
    /// </summary>
    public partial class MainPage : UserControl, IBusyWork
    {
        class SjzdItem
        {
            public string ID;
            public string Mc { get; set; }
            public string Bm { get; set; }
            public string Bz { get; set; }
            public override string ToString()
            {
                return Mc;
            }
        }
        
        /// <summary>
        /// 构造函数:初始化数据字典窗体
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            lbTypes.SelectionChanged += (s, e) =>
            {
                var it = lbTypes.SelectedItem as SjzdItem;
                if (it != null)
                {
                    var lst = QueryItems(it.ID);
                    lstView.ItemsSource = null;
                    lstView.ItemsSource = lst;
                }
            };

        }

        #region IBusyWork
        public void LoadData()
        {
            var lst = QueryItems();
            InvokeUtil.Invoke(this, () => lbTypes.ItemsSource = lst);
        }
        #endregion
        private List<SjzdItem> QueryItems(string sjid = null)
        {
            var lst = new List<SjzdItem>();
            var sql = "select ID,MC,BM,BZ from xtpz_sjzd where ID is not null and MC is not null and BM is not null";
            if (!string.IsNullOrEmpty(sjid))
            {
                sql += " and sjid='" + sjid + "'";
            }
            else
            {
                sql += " and sjid is null";
            }
            MyGlobal.Workspace.QueryCallback(sql, r =>
            {
                var it = new SjzdItem();
                it.ID = r.GetString(0);
                it.Mc = r.GetString(1);
                it.Bm = r.GetString(2);
                it.Bz = r.IsDBNull(3) ? null : r.GetString(3);
                lst.Add(it);
                return true;
            });
            return lst;
        }
    }
}
