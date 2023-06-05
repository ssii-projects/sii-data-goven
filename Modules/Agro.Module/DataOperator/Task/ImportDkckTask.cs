using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Module.DataExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


/*
yxm created at 2019/4/10 17:34:14
*/
namespace Agro.Module.DataOperator
{
	/// <summary>
	/// 导入调查地块
	/// </summary>
	class ImportDkckTask
	{
		public static bool? ShowDialog(Window owner,string shpFile, eDatabaseType dbType)
		{
			var delay = new DelayDoImpl();
			var ti = new ImportDcdkData();
			var dlg = new KuiDialog(owner, "导入地块数据")
			{
				Width = 760,
				Height = 380,
			};
			var taskPropertyPage = (ImportDcdkDataPropertyPage)ti.PropertyPage;
			taskPropertyPage.FileName = shpFile;
			taskPropertyPage.DatabaseType = dbType;


			#region 显示任务页并自动启动任务
			dlg.HideBottom();

			var taskContainer = new TaskPage();

			dlg.Closing += (s2, e2) => {
				if (taskContainer.IsRuning())
				{
					e2.Cancel = true;
					MessageBox.Show("任务正在运行", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			};
			taskContainer.HideTaskSelectPanel();
			taskContainer.AddTaskToPool(ti);
			dlg.Content = taskContainer;
			taskContainer.Start();
			taskContainer.AutoAjustColumnWidth();
			ti.OnFinish+=(t,s)=>
			{
				if (ti.ErrorCount() == 0 && !ti.HasException)
				{
					dlg.Dispatcher.Invoke(() =>
					{
						MessageBox.Show("上传成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
						//delay.DelayDo(() => dlg.Close());
					});
					//()=>dlg.Close());

				}
			};
			#endregion
			var fok=dlg.ShowDialog();
			dlg.Content = null;
			return fok;
		}
	}
}
