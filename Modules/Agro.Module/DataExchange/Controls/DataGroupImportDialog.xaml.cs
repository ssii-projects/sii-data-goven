using Agro.Library.Common;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Agro.Module.DataExchange
{
    /// <summary>
    ///批量导入汇交数据对话框
    /// </summary>
    public partial class DataGroupImportDialog : TaskPropertyPage// UserControl
    {
        public Func<List<DataImportProperty>,HashSet<string>, string> OnApply;
        private readonly ObservableCollection<DataImportProperty> DataSource = new ObservableCollection<DataImportProperty>();
        public DataGroupImportDialog(Func<List<DataImportProperty>,HashSet<string>, string> onApply)//string filter="所有文件（*.*）}*.*",bool fShowClearOldDataControl=false,bool fClearOldData=true)
        {
            OnApply = onApply;
            InitializeComponent();
            view.ItemsSource = DataSource;
			//base.Width = 820;
			//AddDataImportControl();
			//btnAdd.Click += (s,e) => {
			//    AddDataImportControl(); 
			//};
			base._onPreShow += dlg => dlg.Width = 820;
            LoadSeting();

        }
        //public override void OnPreShow(KuiDialog dlg)
        //{
        //    dlg.Width = 820;
        //}
        //private void AddDataImportControl() {
        //    //DataImportControl cui = new DataImportControl();
        //    //cui.OnDelete += (c) =>
        //    //{
        //    //    spDataIpmortCtr.Children.Remove(c);
        //    //    return null;
        //    //};
        //    //cui.IsDeleteVisible = true;
        //    //spDataIpmortCtr.Children.Add(cui);
        //}
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new KuiDialog(Window.GetWindow(this), "添加连接")
            {
                Content = new DataImportControl(),
                Height=260,
            };
            dlg.BtnOK.Click += (s, e1) =>
            {
                var c = dlg.Content as DataImportControl;
                var sPath = c.tbPath.Text.Trim();
                var sConStr = c.tbConnectionStr.Text.Trim();
                if (string.IsNullOrEmpty(sPath))
                {
                    MessageBox.Show("未设置汇交成果路径！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var err = new InputParam().Init(sPath, MyGlobal.Workspace.ConnectionString, "", new HashSet<string>());
                if (err != null)
                {
                    MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(sConStr))
                {
                    MessageBox.Show("未设置数据库！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DataSource.Add(new DataImportProperty()
                {
                    Path=sPath,
                    ConnectionString=sConStr,
                    ProviderName=c.ProviderName
                });
                btnDelete.IsEnabled = true;
                dlg.Close();
            };
            dlg.ShowDialog();
        }
        private void BtnDelete_Click(object sender,RoutedEventArgs e)
        {
            if (view.SelectedItem is DataImportProperty si)
            {
                DataSource.Remove(si);
                btnDelete.IsEnabled = DataSource.Count > 0;
            }
        }
        private void LoadSeting()
        {
            //var s=MyGlobal.Persist.LoadSettingInfo("k7C97FD4B3A6443A0A10D4102095BFFED") as string;
            //if (!string.IsNullOrEmpty(s))
            //{
            //    tbPath.Text = s;
            //}
        }
        internal void SaveSeting()
        {
            //if (!string.IsNullOrEmpty(FilePath))
            //{
            //    MyGlobal.Persist.SaveSettingInfo("k7C97FD4B3A6443A0A10D4102095BFFED", FilePath);
            //}
        }

        public string FilePath
        {
            get
            {
                return "";
            }
        }
        public bool ClearOldData
        {
            get
            {
                return true;
            }
        }

        public override string Apply()
        {
            if (DataSource.Count == 0)
            {
                return "未添加数据源！";
            }
            var listprop =  new List<DataImportProperty>();
            listprop.AddRange(DataSource);
            //foreach (DataImportControl control in this.spDataIpmortCtr.Children.OfType<DataImportControl>())
            //{

            //    if (string.IsNullOrEmpty(control.tbPath.Text))
            //    {
            //        control.tbPath.Focus();
            //        control.tbPath.SelectAll();
            //        return "未设置文件路径！";
            //    }
            //    if (!Directory.Exists(control.tbPath.Text))
            //    {
            //        control.tbPath.Focus();
            //        control.tbPath.SelectAll();
            //        return "文件夹不存在！";
            //    }
            //    if (string.IsNullOrEmpty(control.tbConnectionStr.Text))
            //    {
            //        control.tbConnectionStr.Focus();
            //        control.tbConnectionStr.SelectAll();
            //        return "未设置数据库连接！";
            //    }
            //    DataImportProperty prop = new DataImportProperty();
            //    prop.ConnectionString = control.tbConnectionStr.Text;
            //    prop.Paht = control.tbPath.Text;
            //    prop.ProviderName = control.ProviderName;
            //    listprop.Add(prop);
            //}
            string err = null;
            if (OnApply != null)
            {
                var notImportShapeFilePrefix = new HashSet<string>();
                //todo...
                err = OnApply(listprop, notImportShapeFilePrefix);
            }

            return err;
        }
    }
    public class DataImportProperty : NotificationObject
    {
        private string _path;
        public string Path { get
            {
                return _path;
            }
            set
            {
                _path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }
        private string _cs;
        public string ConnectionString { get
            {
                return _cs;
            }
            set
            {
                _cs = value;
                RaisePropertyChanged(nameof(ConnectionString));
            }
        }
        public string ProviderName { get; set; }
    }
}
