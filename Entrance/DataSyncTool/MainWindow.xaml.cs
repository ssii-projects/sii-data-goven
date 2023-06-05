using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Module.DataSync;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace DataSyncTool
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private const string PSKEY = "A1C4D0122-5CB9-4494-A58F-62FE70E4443E";
		public MainWindow()
		{
			InitializeComponent();
			toolbar.Init(taskPage);

			//SkinUtil.ApplySkin(10, false);

			bool fConnected = false;
			try
			{
				var o = MyGlobal.Persist.LoadSettingInfo(PSKEY);
				if (o != null)
				{
					var ws = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(o.ToString());
					MyGlobal.Connected(ws,AppType.DataGoven, null);
					fConnected = true;
				}
			}
			catch
			{
			}
			if (!fConnected)
			{
				if (false == ShowConnectionDialog())
				{
					Application.Current.Shutdown();
				}
			}

			btnDataSource.Click += (s, e) =>Handle(() =>ShowConnectionDialog(MyGlobal.Workspace?.ConnectionString));

			Closed += (s, e) =>
			  {
				  Handle(()=>MyGlobal.ShutDown());
			  };

			taskPage.ShowRootNode = false;
			taskPage.Title = "外网数据同步";
			taskPage.AddTask(new ExportWwData());
			taskPage.AddTask(new ImportNwData());
		}
		bool ShowConnectionDialog(string cons=null)
		{
			var cs = DBUtil.ShowSQLServerConnectionDialog(cons);
			if (cs != null)
			{
				var ws = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(cs);
				MyGlobal.Connected(ws,AppType.DataGoven, null);
				MyGlobal.Persist.SaveSettingInfo(PSKEY, cs, true);
				return true;
			}
			return false;
		}
		private void Handle(Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				ShowException(ex);
				//MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void ShowException(Exception ex)
		{
				//UIHelper.ShowExceptionMessage(ex);
			MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
