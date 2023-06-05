using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	///承包经营权取得方式结构分析
	/// </summary>
	public partial class GetWayPage : UserControl,IThemePage
    {
        public string Title { get { return "承包经营权取得方式结构分析"; } }
        public GetWayPage()
        {
            InitializeComponent();
            DataContext = new GetWayPageViewModel();
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as GetWayPageViewModel;
            ap.Init(zone);
        }
    }
}
