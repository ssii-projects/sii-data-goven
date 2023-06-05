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

namespace Agro.Module.DataQuery
{
    /// <summary>
    /// NavigationPanelTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationPanelTreeView : UserControl
    {
        private ContentPanel _contentPnl;
        public NavigationPanelTreeView()
        {
            InitializeComponent();
            lstBox.ItemsSource = new List<string>()
            {
               HzbTitleConstants.Hzb1,// "地块汇总表",
               HzbTitleConstants.Hzb2,// "承包地土地用途汇总表",
                HzbTitleConstants.Hzb3,//"非承包地地块类别汇总表",
                HzbTitleConstants.Hzb4,//"承包地是否基本农田汇总表",
                HzbTitleConstants.Hzb5,//"权证信息汇总表",
                HzbTitleConstants.Hzb6,//"承包方汇总表"
                HzbTitleConstants.Hzb7,//"登记簿汇总表"
            };
            lstBox.SelectionChanged += (s, e) =>
            {
                var title=lstBox.SelectedItem as string;
                if (title != null)
                {
                    _contentPnl.ShowHzb(title);
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
