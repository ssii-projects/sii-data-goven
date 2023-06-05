using System.Windows;
using System.Windows.Controls;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// MainFramePage.xaml 的交互逻辑
	/// </summary>
	public partial class ThemeAnalyFrame :UserControl
    {
        //private System.Windows.Controls.UserControl _contentUserControl;
        //private NavigationPanelTreeView _navigationPanelTreeView;

        public ShortZone CurrentZone
        {
            get
            {
                return navTree.navigator.CurrentZone;
            }
        }

        public ThemeAnalyFrame()
        {
            InitializeComponent();
            //NavigatorType = eNavigatorType.None;
            //SingleInstance = true;
            navTree.btnDiffrenceCommand.Click += (s, e) => ShowDiffrencePage();
            navTree.btnRealTimeTableCommand.Click += (s, e) => ShowRealTimeTablePage();
            navTree.btnAreaCommand.Click += (s, e) => ShowAreaPage();
            navTree.btnPurposeCommand.Click += (s, e) => ShowPurposePage();
            navTree.btnGetWayCommand.Click += (s, e) => ShowGetWayPage();
            navTree.btnLandLevelCommand.Click += (s, e) => ShowLandLevelPage();
            navTree.btnPerCapitaAreaCommand.Click += (s, e) => ShowPerCapitaAreaPage();
            navTree.btnPopulationCommand.Click += (s, e) => ShowPopulationPage();
        }

        private void ShowPopulationPage()
        {
            //var ue = new PopulationPage();
            //ue.Init(CurrentZone);
            showPage(new PopulationPage());// ue, ue.Title);
            //ShowPage<PopulationPage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }

        private void ShowPerCapitaAreaPage()
        {
            showPage(new PerCapitaAreaPage());
            //ShowPage<PerCapitaAreaPage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }

        private void ShowLandLevelPage()
        {
            showPage(new LandLevelPage());
            //ShowPage<LandLevelPage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }

        private void ShowGetWayPage()
        {
            showPage(new GetWayPage());
            //ShowPage<GetWayPage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }

        private void ShowPurposePage()
        {
            showPage(new PurposePage());
            //ShowPage<PurposePage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }

        private void ShowDiffrencePage()
        {
            showPage(new DiffrencePage());
            //var ue = new DiffrencePage();
            //ue.Init(CurrentZone);
            //showPage(ue, DiffrencePage.Title);
            //ShowPage<DiffrencePage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }
        private void ShowRealTimeTablePage()
        {
            showPage(new RealTimeTablePage());

            //ShowPage<RealTimeTablePage>(p =>
            //{
            //    p.Init(CurrentZone);
            //});
        }
        private void ShowAreaPage()
        {
            showPage(new AreaPage());
            //ShowPage<AreaPage>(p=>
            //{
            //    p.Init(CurrentZone);
            //});
        }
        private void showPage(IThemePage page)//UserControl ue,string title)
        {
            if (CurrentZone == null)
            {
                MessageBox.Show("请先选择行政地域！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            page.Init(CurrentZone);
            MyGlobal.MainWindow.OpenPage(page as UserControl,page.Title,null);
        }

        private void ShowRealTimeImagePage()
        {
            showPage(new RealTimeImagePage());
            //ShowPage<RealTimeImagePage>();
        }


    }

    public interface IThemePage
    {
        string Title { get; }
        void Init(ShortZone zone);
    }
}
