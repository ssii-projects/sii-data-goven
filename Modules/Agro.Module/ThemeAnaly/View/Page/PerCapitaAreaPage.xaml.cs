using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// 农户人均承包地面积水平分析
	/// </summary>
	public partial class PerCapitaAreaPage : UserControl,IThemePage
    {
        public string Title { get { return "农户人均承包地面积水平分析"; } }
        public PerCapitaAreaPage()
        {
            InitializeComponent();
            DataContext = new PerCapitaAreaPageViewModel();
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as PerCapitaAreaPageViewModel;
            ap.Init(zone);
        }
    }
}
