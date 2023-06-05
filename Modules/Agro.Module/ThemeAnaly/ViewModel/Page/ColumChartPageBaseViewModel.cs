using Agro.Module.ThemeAnaly.ViewModel.Control;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class ColumChartPageBaseViewModel :NotifyObject
    {
        private ChartColumWithTitleViewModel _chartColumWithTitleViewModel=new ChartColumWithTitleViewModel();
        private TitleBarViewModel _titleBarViewModel=new TitleBarViewModel();

        public ChartColumWithTitleViewModel ChartColumWithTitleViewModel
        {
            get { return _chartColumWithTitleViewModel; }
            set { _chartColumWithTitleViewModel = value; OnPropertyChanged(nameof(ChartColumWithTitleViewModel)); }
        }

        public TitleBarViewModel TitleBarViewModel
        {
            get { return _titleBarViewModel; }
            set { _titleBarViewModel = value; OnPropertyChanged(nameof(TitleBarViewModel));}
        }

        public ColumChartPageBaseViewModel()
        {
            InitData();
            
        }

        protected virtual void InitData()
        {
        }

    }
}
