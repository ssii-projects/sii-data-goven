using System.Collections.Generic;
using Agro.Module.ThemeAnaly.Entity;

namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class TableViewModel:NotifyObject
    {
        private List<string> _colums;
        private List<GridDataDynamicObject> _gridDataDynamicObjects;

        public List<string> Colums
        {
            get { return _colums; }
            set { _colums = value; OnPropertyChanged(nameof(Colums));}
        }


        public List<GridDataDynamicObject> GridDataDynamicObjects
        {
            get { return _gridDataDynamicObjects; }
            set { _gridDataDynamicObjects = value;OnPropertyChanged(nameof(GridDataDynamicObjects)); }
        }
    }
}
