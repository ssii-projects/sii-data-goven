using System;
using System.Windows;
using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Control;

namespace Agro.Module.ThemeAnaly.View.Control
{
    /// <summary>
    /// TitleBarUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class TitleBar
    {
        public TitleBar()
        {
            InitializeComponent();
            DataContext = new TitleBarViewModel();
        }


        public string Title
        {
            get { return Label.Content.ToString(); }
            set { Label.Content = value; }
        }

        public Action ExpottAction { get; set; }

        private void Btn_Export_Click(object sender, RoutedEventArgs e)
        {
            ExpottAction?.Invoke();
        }

        public Action PrintAction { get; set; }

        private void Btn_Print_Click(object sender, RoutedEventArgs e)
        {
            PrintAction?.Invoke();
        }



    }
}
