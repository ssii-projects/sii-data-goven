using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Agro.Module.DataView
{
    /// <summary>
    /// NavigationPanelTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationPanelTreeView : UserControl
    {
		class Item: NotificationObject
		{
			public string Title { get; set; }
			public string TableName;
			public string IDFieldName;
			public string DkbmFieldName;
			public HashSet<string> ExcludeFieldNames = new HashSet<string>();
			public Item(string title, string tableName, string idFieldName,string dkbmFieldName,string excludeFieldNames)
			{
				Title = title;
				TableName = tableName;
				IDFieldName = idFieldName;
				DkbmFieldName = dkbmFieldName;
				if (excludeFieldNames != null)
				{
					var sa = excludeFieldNames.Split(',');
					foreach (var s in sa)
					{
						ExcludeFieldNames.Add(s);
					}
				}
			}
			public override string ToString()
			{
				return Title;
			}
		}
        private ContentPanel _contentPnl;
        public NavigationPanelTreeView()
        {
            InitializeComponent();
			lstBox.ItemsSource = new List<Item>()
			{
			   new Item("承包地块信息","DJ_CBJYQ_DKXX","ID","DKBM","ID,DKID,DJBID"),// "地块汇总表",
			   new Item("承包方","DJ_CBJYQ_CBF","ID","CBFBM","ID,DJBID"),
			   new Item("家庭成员","DJ_CBJYQ_CBF_JTCY","ID","CBFBM","ID,CBFID"),
			   new Item("发包方","QSSJ_FBF","ID","FBFBM","ID,SZDY"),
			   new Item("承包合同","QSSJ_CBHT","ID","FBFBM","ID"),
			   new Item("承包经营权登记簿","QSSJ_CBJYQZDJB","ID","FBFBM","ID"),
			   new Item("承包经营权证","QSSJ_CBJYQZ","ID","CBJYQZBM","ID"),
               //HzbTitleConstants.Hzb2,// "承包地土地用途汇总表",
               // HzbTitleConstants.Hzb3,//"非承包地地块类别汇总表",
               // HzbTitleConstants.Hzb4,//"承包地是否基本农田汇总表",
               // HzbTitleConstants.Hzb5,//"权证信息汇总表",
               // HzbTitleConstants.Hzb6,//"承包方汇总表"
               // HzbTitleConstants.Hzb7,//"登记簿汇总表"
            };
            lstBox.SelectionChanged += (s, e) =>
            {
                //var title=lstBox.SelectedItem as string;
                //if (title != null)
				if(lstBox.SelectedItem is Item it)
                {
                    _contentPnl.ShowHzb(it.Title,it.TableName,it.IDFieldName,it.DkbmFieldName, it.ExcludeFieldNames);
                }
            };

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

        }
        public void Init(ContentPanel cp)
        {
            _contentPnl = cp;
            navigator.OnZoneSelected += _contentPnl.OnZoneChanged;
            lstBox.SelectedIndex = 0;
        }
    }
}
