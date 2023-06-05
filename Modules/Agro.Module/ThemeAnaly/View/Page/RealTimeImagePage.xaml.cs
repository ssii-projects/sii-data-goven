using System;
using System.Collections.Generic;
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
using Agro.Library.Common;
using Agro.Library.Common.Util;

namespace Agro.Module.ThemeAnaly.View.Page
{
    /// <summary>
    /// 实时数据对比分析图
    /// </summary>
    public partial class RealTimeImagePage : UserControl,IThemePage
    {
        public string Title { get { return "实时数据对比分析图"; } }
        public RealTimeImagePage()
        {
            InitializeComponent();
        }

        public void Init(ShortZone zone)
        {
            //throw new NotImplementedException();
        }
    }
}
