using Agro.Module.DataExchange;
using System;
using System.Collections.Generic;
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

namespace Agro.Module.DataSync
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            taskPage.Title = "内网数据同步";
            taskPage.ShowRootNode = false;

			//         taskPage.AddTask(new ExportNwData());
			//taskPage.AddTask(new ImportWwData());
			taskPage.AddTask(new ExportDkJsonTask());
			taskPage.AddTask(new ImportWwDjbJsonTask());
		}
    }
}
