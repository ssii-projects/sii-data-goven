using System.Collections.Generic;
using System.Collections.ObjectModel;
using Agro.Module.ThemeAnaly.Entity;
using Agro.Module.ThemeAnaly.View.Control;

namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public  class ChartColumWithTitleViewModel:NotifyObject
    {

        private TitleBarViewModel _titleBarViewModel = new TitleBarViewModel();

        private ObservableCollection<string> _rows;

        private ObservableCollection<string> _rowItems;
        private List<CharDataColum> _chartData;


        public RelayCommand ExportCommand { get; set; }

        public RelayCommand PrintCommand { get; set; }

        public TitleBarViewModel TitleBarViewModel
        {
            get { return _titleBarViewModel; }
            set { _titleBarViewModel = value; OnPropertyChanged(nameof(TitleBarViewModel)); }
        }

        public ObservableCollection<string> Rows
        {
            get { return _rows; }
            set
            {
                _rows = value;
                OnPropertyChanged(nameof(Rows));
            }
        }

        public ObservableCollection<string> RowItems
        {
            get { return _rowItems; }
            set
            {
                _rowItems = value;
                OnPropertyChanged(nameof(RowItems));
            }
        }

        public List<CharDataColum> ChartData
        {
            get { return _chartData; }
            set { _chartData = value; OnPropertyChanged(nameof(ChartData));}
        }

        public ChartColumWithTitleViewModel()
        {
            ExportCommand=new RelayCommand(Export);
            PrintCommand=new RelayCommand(Print);
          

        }



        private void Export()
        {
        }

        private void Print()
        {
        }


      
    }
}
