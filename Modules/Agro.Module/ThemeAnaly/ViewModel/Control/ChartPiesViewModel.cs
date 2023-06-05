//using DotNetSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class ChartPiesViewModel :NotifyObject
    {
        private List<ChartPieViewModel> _chartPieViewModels = new List<ChartPieViewModel>();// CDObjectList<ChartPieViewModel>() ;

        public List<ChartPieViewModel> ChartPieViewModels
        {
            get { return _chartPieViewModels; }
            set { _chartPieViewModels = value; OnPropertyChanged(nameof(ChartPieViewModels)); }
        }
    }
}
