using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// 农户人口空间分布情况分析
	/// </summary>
	public partial class PopulationPage : UserControl,IThemePage
    {
        public string Title { get { return "农户人口空间分布情况分析"; } }
        public PopulationPage()
        {
            InitializeComponent();
            DataContext = new PopulationPageViewModel();
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as PopulationPageViewModel;
            ap.Init(zone);
        }
    }
}
