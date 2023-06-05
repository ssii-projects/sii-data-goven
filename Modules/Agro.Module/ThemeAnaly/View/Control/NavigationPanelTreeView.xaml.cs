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
using Agro.Module.ThemeAnaly.ViewModel.Control;

namespace Agro.Module.ThemeAnaly.View.Control
{
    /// <summary>
    /// NavigationPanelTreeView.xaml 的交互逻辑
    /// </summary>
    public partial class NavigationPanelTreeView : UserControl
    {
        #region Properties

        public LinkButtonsPanelViewModel LinkButtonsPanelViewModel { get; set; } = new LinkButtonsPanelViewModel();

        #endregion

        #region Ctor

        public NavigationPanelTreeView()
        {
            InitializeComponent();
            DataContext = this;
        }

        #endregion

        #region Methods

        #region Methods - Events

        #endregion

        #endregion
    }
}
