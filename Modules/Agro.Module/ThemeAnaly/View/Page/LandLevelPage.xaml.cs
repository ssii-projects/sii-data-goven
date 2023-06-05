using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// 承包地地力等级结构分析
	/// </summary>
	public partial class LandLevelPage : UserControl,IThemePage
    {
        public string Title { get { return "承包地地力等级结构分析"; } }
        public LandLevelPage()
        {
            InitializeComponent();
            DataContext = new LandLevelPageBaseViewModel();
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as LandLevelPageBaseViewModel;
            ap.Init(zone);
        }
    }
}
