
using Agro.Module.ThemeAnaly.Entity;
using Agro.Module.ThemeAnaly.View.Control;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class ChartPieViewModel : NotifyObject
    {

        private string _title;
        private List<CharDataColum> _rows=new List<CharDataColum>() ;
        private List<ChartDataPie> _datas = new List<ChartDataPie>();

        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public List<CharDataColum> Rows
        {
            get { return _rows; }
            set { _rows = value;
                OnPropertyChanged(nameof(Rows)); }
        }

        //public List<ChartDataPie> datas {
        //    get
        //    {
        //        return _datas;
        //    }
        //    set
        //    {
        //        _datas = value;
        //        OnPropertyChanged(nameof(datas));
        //    }
        //}

    }
}
