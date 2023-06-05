using Agro.LibCore.UI;
using Agro.Module.DataExchange;
using System.Windows;

namespace Agro.Module.DataOperator
{
	/// <summary>
	/// 导出地块JSON交换包
	/// </summary>
	class ExportJsonTask
	{
		public static void ShowDialog(Window owner)
		{
			var ti = new ExportDkJsonTask();
			var panel = ti.PropertyPage as ExportJsonPanel;
			var dlg = new KuiDialog(owner, "导出地块更新数据包")
			{
				Width =panel.DialogWidth,// 800,
				Height =panel.DialogHeight,// 480,
				Content=panel
			};

			#region 显示任务页并自动启动任务
			//dlg.HideBottom();

			var taskContainer = new TaskPage();

			dlg.Closing += (s2, e2) => {
				if (taskContainer.IsRuning())
				{
					e2.Cancel = true;
					MessageBox.Show("任务正在运行", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			};
			dlg.BtnOK.Click += (s, e) =>
			  {
				  if (dlg.Content == panel)
				  {
					  var err=panel.Apply();
					  if (err != null)
					  {
						  MessageBox.Show(err, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
						  return;
					  }
					  taskContainer.HideTaskSelectPanel();
					  taskContainer.AddTaskToPool(ti);
					  dlg.Content = taskContainer;
					  taskContainer.Start();
					  taskContainer.AutoAjustColumnWidth();
					  dlg.BtnOK.IsEnabled = false;
				  }
			  };
			#endregion
			dlg.ShowDialog();
			dlg.Content = null;
		}
	}
}
