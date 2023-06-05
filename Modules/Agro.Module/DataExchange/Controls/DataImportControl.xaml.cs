using Agro.Library.Common;
using Agro.LibCore.UI;
//using Microsoft.Data.ConnectionUI;
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

namespace Agro.Module.DataExchange
{
    /// <summary>
    ///数据导入对话框
    /// </summary>
    public partial class DataImportControl : UserControl
    {

        //public Func<DataImportControl, string> OnDelete;
        public DataImportControl()//string filter="所有文件（*.*）}*.*",bool fShowClearOldDataControl=false,bool fClearOldData=true)
        {
            InitializeComponent();
            tbPath.OnButtonClick += () =>
            {
                var dlg = new System.Windows.Forms.FolderBrowserDialog
                {
                    //dlg.RootFolder = Environment.SpecialFolder.Recent;
                    Description = "汇交成果数据路径"
                };
                var path = tbPath.Text.Trim();
                if (!string.IsNullOrEmpty(path) && System.IO.Directory.Exists(path))
                {
                    dlg.SelectedPath = path;
                }
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.tbPath.Text = dlg.SelectedPath;
                }

            };
            tbConnectionStr.OnButtonClick += () => {
                var str = this.ShowSQLServerConnectionDialog(this.tbConnectionStr.Text);
                this.tbConnectionStr.Text = str;
            };
            //tbDel.Click += (s, e) =>
            //{
            //    if (OnDelete != null)
            //    {
            //        OnDelete(this);
            //    }
            //};
        }

        private string ShowSQLServerConnectionDialog(string cs)
        {
            string retcs = cs;
            //#region 显示SQLServer连接对话框
            //using (var dlg = new DataConnectionDialog())
            //{
            //    dlg.DataSources.Clear();
            //    dlg.DataSources.Add(DataSource.SqlDataSource);
            //    dlg.DataSources.Add(DataSource.OracleDataSource);
            //    dlg.SelectedDataSource = DataSource.SqlDataSource;
            //    dlg.SelectedDataProvider = DataProvider.SqlDataProvider;
            //    if (!string.IsNullOrEmpty(cs))
            //    {
            //        if (ProviderName == "DataSource.SqlServer")
            //        {
            //            dlg.SelectedDataSource = DataSource.SqlDataSource;
            //            dlg.SelectedDataProvider = DataProvider.SqlDataProvider;
            //            dlg.ConnectionString = cs;
            //        }
            //        if (ProviderName == "DataSource.Oracle")
            //        {
            //            dlg.SelectedDataSource = DataSource.OracleDataSource;
            //            dlg.SelectedDataProvider = DataProvider.OracleDataProvider;
            //            dlg.ConnectionString = cs;
            //        }
                   
            //    }
            //    if (System.Windows.Forms.DialogResult.OK == DataConnectionDialog.Show(dlg))
            //    {
            //        retcs = dlg.ConnectionString;

            //        if (dlg.SelectedDataSource == DataSource.SqlDataSource)
            //        {
            //            ProviderName = "DataSource.SqlServer";
            //        }
            //        if (dlg.SelectedDataSource == DataSource.OracleDataSource)
            //        {
            //            ProviderName = "DataSource.Oracle";
            //        }
            //    }
            //}
            //#endregion
            return retcs;
        }
        public bool IsDeleteVisible
        {
            set {
                //if (!value)
                //{
                //    tbDel.Visibility = Visibility.Collapsed;
                //}
                //else {

                //    tbDel.Visibility = Visibility.Visible;
                //}
            }
        }
        public String ProviderName { get; set; }
    }
}
