using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class ChartPiesWithTitleBarViewModel :NotifyObject
    {
        private ChartPiesViewModel _chartPiesViewModel=new ChartPiesViewModel();
        private TitleBarViewModel _titleBarViewModel=new TitleBarViewModel() ;

        public ChartPiesViewModel ChartPiesViewModel
        {
            get { return _chartPiesViewModel; }
            set { _chartPiesViewModel = value;OnPropertyChanged(nameof(ChartPiesViewModel)); }
        }


        public TitleBarViewModel TitleBarViewModel
        {
            get { return _titleBarViewModel; }
            set { _titleBarViewModel = value; OnPropertyChanged(nameof(TitleBarViewModel));}
        }
    }
}
