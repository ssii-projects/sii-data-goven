namespace Agro.Module.ThemeAnaly.ViewModel.Control
{
    public class TitleBarViewModel :NotifyObject
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value;OnPropertyChanged(nameof(Title)); }
        }

        public RelayCommand ExportCommand { get; set; }

        public RelayCommand PrintCommand { get; set; }



        
    }
}
