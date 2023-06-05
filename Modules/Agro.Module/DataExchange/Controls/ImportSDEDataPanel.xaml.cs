//using Microsoft.Data.ConnectionUI;
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Microsoft.Data.ConnectionUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	///合库任务设置界面
	///yxm 2018-11-29
	/// </summary>
	public partial class ImportSDEDataPanel : TaskPropertyPage
	{
		private readonly ObservableCollection<string> Datasource = new ObservableCollection<string>();
		public ImportSDEDataPanel(Action<IEnumerable<string>> onApply)
		{
			InitializeComponent();
			lstBox.ItemsSource = Datasource;
			base._onPreShow += d =>
			  {
				  d.Width =700;
				  d.Height = 390;
			  };
			base._onApply += () =>
			  {
				  if (Datasource.Count == 0)
				  {
					  return "未添加数据源！";
				  }
				  onApply(Datasource);
				  SaveSeting();
				  return null;
			  };
			LoadSeting();

			lstBox.SelectionChanged += (s, e) =>
			  {
				  btnDelete.IsEnabled = lstBox.SelectedItem != null;// Datasource.Count > 0;
			  };
		}
		private void BtnAdd_Click(object sender, RoutedEventArgs e)
		{
			var str = this.ShowSQLServerConnectionDialog();
			if (str != null)
			{
				var err = CheckSource(str);
				if (err != null)
				{
					UIHelper.ShowError(Window.GetWindow(this), err);
					return;
				}
				Datasource.Add(str);
				//btnDelete.IsEnabled = true;
			}
		}
		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if (lstBox.SelectedIndex>=0)
			{
				Datasource.RemoveAt(lstBox.SelectedIndex);
				//btnDelete.IsEnabled = Datasource.Count > 0;
			}
		}


		private string CheckSource(string conStr)
		{
			try
			{
				using (var db = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(conStr))
				{
					var err = MyDBUtil.IsValidDB(db);
					if (err != null)
					{
						return err;
					}

					var srid=db.GetSRID("DLXX_XZDY");
					var srid1=MyGlobal.Workspace.GetSRID("DLXX_XZDY");
					if (srid != srid1)
					{
						return "源数据库坐标系["+srid+"]与当前数据库坐标系["+srid1+"]不一致!";
					}
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return null;
		}

		private void LoadSeting()
		{
			MyGlobal.Persist.LoadList("k8789791B-0813-4896-932D-19D2B23B4130",s=>Datasource.Add(s));
		}
		private void SaveSeting()
		{
			MyGlobal.Persist.SaveList("k8789791B-0813-4896-932D-19D2B23B4130", Datasource);
		}

		private string? ShowSQLServerConnectionDialog(string? cs=null)
		{
			string? retcs = cs;
			#region 显示SQLServer连接对话框
			using (var dlg = new DataConnectionDialog())
			{
				dlg.DataSources.Clear();
				dlg.DataSources.Add(DataSource.SqlDataSource);
				//dlg.DataSources.Add(DataSource.OracleDataSource);
				dlg.SelectedDataSource = DataSource.SqlDataSource;
				dlg.SelectedDataProvider = DataProvider.SqlDataProvider;
				if (!string.IsNullOrEmpty(cs))
				{
					//if (ProviderName == "DataSource.SqlServer")
					//{
					dlg.SelectedDataSource = DataSource.SqlDataSource;
					dlg.SelectedDataProvider = DataProvider.SqlDataProvider;
					dlg.ConnectionString = cs;
					//}
					//if (ProviderName == "DataSource.Oracle")
					//{
					//	dlg.SelectedDataSource = DataSource.OracleDataSource;
					//	dlg.SelectedDataProvider = DataProvider.OracleDataProvider;
					//	dlg.ConnectionString = cs;
					//}

				}
				if (System.Windows.Forms.DialogResult.OK == DataConnectionDialog.Show(dlg))
				{
					retcs = dlg.ConnectionString;

					//if (dlg.SelectedDataSource == DataSource.SqlDataSource)
					//{
					//	ProviderName = "DataSource.SqlServer";
					//}
					//if (dlg.SelectedDataSource == DataSource.OracleDataSource)
					//{
					//	ProviderName = "DataSource.Oracle";
					//}
				}
			}
			#endregion
			return retcs;
		}
	}
}
