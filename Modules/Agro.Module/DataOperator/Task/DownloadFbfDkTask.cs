using Agro.GIS;
using Agro.GIS.UI;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Module.DataExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


/*
yxm created at 2019/4/10 13:55:47
*/
namespace Agro.Module.DataOperator
{
	/// <summary>
	/// 导出发包方地块
	/// </summary>
	class DownloadFbfDkTask
	{
		//private static bool UseBatchExportMode = false;
		public DownloadFbfDkTask()
		{
			//base.Name = "导出地块数据";
			//base.Description = "导出符合农业部要求格式的调查地块数据";
			//base.PropertyPage = new ExportDkDataPropertyPage();
		}
		public static void ShowDialog(MapControl mapControl, TocControl Toc)
		{
			Task ti;
			if (MyGlobal.AppConfig.ExportFbfMode == EExportFbfMode.BatchExport)
			{
				ti = new BatchExportDkData();
			}
			else
			{
				ti = new ExportDkData();
			}
			var dlg = new KuiDialog(Window.GetWindow(mapControl), "下载地块数据")
			{
				Width = 740,
				Height = 380,
			};
			var taskPropertyPage = (ExportDkDataPropertyPageBase)ti.PropertyPage;
			dlg.Content = taskPropertyPage.Page;
			dlg.BtnOK.Click += (s1, e1) =>
			{
				if (dlg.Content == taskPropertyPage.Page)
				{
					var err = taskPropertyPage.Apply();
					if (err != null)
					{
						MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}
					#region 显示任务页并自动启动任务
					dlg.HideBottom();

					var taskContainer = new TaskPage();
					taskContainer.OnFinish += () =>
					{
						if (!ti.HasException && ti.ErrorCount() == 0)
						{
							if (ti.PropertyPage is ExportDkDataPropertyPage dpp)
							{
								var shpFile = dpp.ExportFilePath;
								AddShapeCommand.AddShapeFile(mapControl, Toc, shpFile, dpp.DatabaseType, false);
							}
							dlg.Close();
						}
					};
					dlg.Closing += (s2, e2) => {
						if (taskContainer.IsRuning())
						{
							e2.Cancel = true;
							MessageBox.Show("任务正在运行", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
						//else
						//{
						//	//tmp.Dispose();
						//}
					};
					taskContainer.HideTaskSelectPanel();
					taskContainer.AddTaskToPool(ti);
					dlg.Content = taskContainer;
					taskContainer.Start();
					taskContainer.AutoAjustColumnWidth();
					#endregion
				}
			};
			dlg.ShowDialog();
			dlg.Content = null;
		}
	}
}
