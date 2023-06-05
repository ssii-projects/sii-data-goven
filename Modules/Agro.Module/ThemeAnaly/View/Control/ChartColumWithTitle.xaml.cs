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

namespace Agro.Module.ThemeAnaly.View.Control
{
    /// <summary>
    /// ChartColumWithTitle.xaml 的交互逻辑
    /// </summary>
    public partial class ChartColumWithTitle 
    {
        public ChartColumWithTitle()
        {
            InitializeComponent();
            TitleBar.ExpottAction += ExportImage;
            TitleBar.PrintAction += PrintImage;
        }

        private void PrintImage()
        {
            chart.Print();
        }

        private void ExportImage()
        {
            chart.Export();
        }
    }
}
