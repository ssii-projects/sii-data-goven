using System.Windows.Controls;
using Agro.Module.ThemeAnaly.ViewModel.Page;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// 承包地面积对比分析
	/// </summary>
	public partial class AreaPage :UserControl,IThemePage//, IBusyWork
    {
        //private ShortZone _zone;
        public string Title { get { return "承包地面积对比分析"; } }
        public AreaPage()
        {
            InitializeComponent();
            DataContext = new AreaPageViewModel(); 
        }
        public void Init(ShortZone zone)
        {
            var ap = DataContext as AreaPageViewModel;
            ap.Init(zone);
        }
    }
}
