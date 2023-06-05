using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// 实测面积与合同面积差异对比分析
	/// </summary>
	public partial class DiffrencePage : UserControl,IThemePage
    {
        public string Title { get { return "实测面积与合同面积差异对比分析"; } }
        public DiffrencePage()
        {
            InitializeComponent();
            DataContext = new DiffrencePageViewModel();
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as DiffrencePageViewModel;
            ap.Init(zone);
        }
    }
}
