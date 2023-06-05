using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class TablePanelViewModel:NotifyObject
    {
        private TitleBarViewModel _titleBarViewModel=new TitleBarViewModel();
        private TableViewModel _tableViewModel=new TableViewModel();

        public TitleBarViewModel TitleBarViewModel
        {
            get { return _titleBarViewModel; }
            set { _titleBarViewModel = value; OnPropertyChanged(nameof(TitleBarViewModel));}
        }

        public TableViewModel TableViewModel
        {
            get { return _tableViewModel; }
            set { _tableViewModel = value; OnPropertyChanged(nameof(TableViewModel));}
        }
    }
}
