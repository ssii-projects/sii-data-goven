using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using Agro.Module.DataOperator;
using DataOperatorTool;
using System;
using System.Configuration;
using System.Windows;

namespace TestTool
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			try
			{
				Gdal.LoadNative();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			var loginDlg =new DataOperatorTool.LoginWindow();
			if (loginDlg.ShowDialog()!=true)
			{
				Current.Shutdown();
				return;
			}
            DataOperatorUtil.Operator = loginDlg.UserName;

			try
			{
				var sAppType = ConfigurationManager.AppSettings["AppType"];
                AppPref.AppType =sAppType=="WebService"?AppType.DataOperator_WebService:(sAppType == "SQLiteWnd" ? AppType.DataOperator_SQLite : AppType.DataOperator_ShapeFile);

                //AppPref.UseDownFbfDk = ConfigurationManager.AppSettings["DownFbfDk"] == "true";
				AppPref.WebServiceUrl = ConfigurationManager.AppSettings["WebServiceUrl"];
                //AppPref.UpLoadUrl = ConfigurationManager.AppSettings["UpLoadUrl"];

                ApplicationData.SmallIcon = MyImageUtil.Image16("全图16.png");
				var file = AppDomain.CurrentDomain.BaseDirectory + "data.db";
				var db = SqliteFeatureWorkspaceFactory.Instance.OpenWorkspace(file);
				MyGlobal.Connected(db, AppPref.AppType);
				var wnd = new MainWindow().mainWnd;
				wnd.Closed += (s, e1) =>
				  {
					  MyGlobal.ShutDown();
					  Current.Shutdown();
				  };
				wnd.ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
