using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	///承包地用途结构分析
	/// </summary>
	public partial class PurposePage : UserControl,IThemePage
    {
        public string Title { get { return "承包地用途结构分析"; } }
        public PurposePage()
        {
            InitializeComponent();
            DataContext = new PurposePageViewModel();
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as PurposePageViewModel;
            ap.Init(zone);
        }
    }
}
