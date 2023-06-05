using Agro.Library.Common;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace Agro.Module.DataExchange
{
	/// <summary>
	///数据导入对话框
	/// </summary>
	public partial class DataImportDialog : TaskPropertyPage// UserControl
    {
        public class CheckLayer : NotificationObject// INotifyPropertyChanged
        {
            private bool _fSelected;
            public string LayerName { get; private set; }
            public bool IsSelected
            {
                get { return _fSelected; }
                set
                {
                    _fSelected = value;
                    base.RaisePropertyChanged(nameof(IsSelected));
                }
            }
            public ImageSource Icon { get; set; }
            public string ShapeFilePrefix;
            public CheckLayer(string layerName,string shapeFilePrefix)
            {
                this.LayerName = layerName;
                this.ShapeFilePrefix = shapeFilePrefix;
                _fSelected = true;
            }
        }

        private readonly ObservableCollection<CheckLayer> DataSource;

        public Func<string, HashSet<string>, string> OnApply;
        public DataImportDialog(Func<string,HashSet<string>,string> onApply)
        {
            OnApply = onApply;
            InitializeComponent();
            DataSource = new ObservableCollection<CheckLayer>()
            {
                new CheckLayer("点状地物","DZDW"),
                new CheckLayer("基本农田保护区","JBNTBHQ"),
                new CheckLayer("界址点","JZD"),
                 new CheckLayer("界址线","JZX"),
                new CheckLayer("面状地物","MZDW"),
                new CheckLayer("区域界线","QYJX"),
                new CheckLayer("线状地物","XZDW"),
                new CheckLayer("注记","ZJ"),
            };
            lstBox.ItemsSource = DataSource;
            LoadSeting();
            tbPath.OnButtonClick += () =>
            {
				var dlg = new System.Windows.Forms.FolderBrowserDialog()
				{
					//dlg.RootFolder = Environment.SpecialFolder.Recent;
					Description = "汇交成果数据路径"
				};
                var path = tbPath.Text.Trim();
                if (!string.IsNullOrEmpty(path)&&System.IO.Directory.Exists(path))
                {
                    dlg.SelectedPath = path;
                }
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.tbPath.Text= dlg.SelectedPath;
                }
            };
        }
        private void LoadSeting()
        {
            var s=MyGlobal.Persist.LoadSettingInfo("k7C97FD4B3A6443A0A10D4102095BFFED") as string;
            if (!string.IsNullOrEmpty(s))
            {
                tbPath.Text = s;
            }
        }
        internal void SaveSeting()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                MyGlobal.Persist.SaveSettingInfo("k7C97FD4B3A6443A0A10D4102095BFFED", FilePath);
            }
        }
        public string FilePath
        {
            get
            {
                return tbPath.Text;
            }
        }
        //public bool ClearOldData
        //{
        //    get
        //    {
        //        return btnClearOldData.IsChecked == true;
        //    }
        //}

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cl in DataSource)
            {
                if (sender == btnSelectAll)
                {
                    cl.IsSelected = true;
                }
                else if (sender == btnNotSelectAll)
                {
                    cl.IsSelected = false;
                }
                else if (sender == btnXorSelect)
                {
                    cl.IsSelected = !cl.IsSelected;
                }
            }
        }

        public override string Apply()
        {
            var file = tbPath.Text;// _pnl.FilePath.Trim();
            if (string.IsNullOrEmpty(file))
            {
                return "未设置文件路径！";
            }
            if (!Directory.Exists(file))
            {
                return "文件夹不存在！";
            }
            string err = null;
            if (OnApply != null)
            {
                var notImportShapeFilePrefix = new HashSet<string>();
                foreach(var l in DataSource)
                {
                    if (!l.IsSelected)
                    {
                        notImportShapeFilePrefix.Add(l.ShapeFilePrefix);
                    }
                }
                err = OnApply(file,notImportShapeFilePrefix);
            }
			if (err == null)
			{
				SaveSeting();
			}
            return err;
        }
	}
}
