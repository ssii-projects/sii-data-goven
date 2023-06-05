using Agro.LibCore;
using Agro.LibCore.Task;
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

namespace Agro.Module.SketchMap
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : UserControl,IDisposable
    {
        public MainPage()
        {
            InitializeComponent();
            taskPage.Title = "地块示意图生成工具";
			DependencyObjectUtil.FindControls<TaskCommandButton>(toolbar, btn => btn.Init(taskPage));
            taskPage.ShowRootNode = false;
			taskPage.HideTaskSelectPanel();
			//taskPage.AddTask(new ImportShape() { Icon = MyImageSourceUtil.Image24("ImportData.png") });
			//taskPage.AddTask(new ImportTasks());
			btnNew.Click += (s, e) =>taskPage.ShowPropertyPage(new ExportSketchMapTask(pageLayout),true,task=> RefreshNavigator(task));
			taskPage.OnTaskSelected += task => RefreshNavigator(task);
			taskPage.OnUpdateUI += UpdateToolbarUI;
			btnFolder.Click += BtnDirectory_Click;
			btnEdit.Click +=BtnEditor_Click;
			btnClear.Click += (s, e) => ClearData();
		}
		private void RefreshNavigator(ITask task)
		{
			if (task != null)
			{
				navigator.LoadData(((task as ExportSketchMapTask).PropertyPage as DataSelectedDialog).Argument.FileNames);
			}
			else
			{
				navigator.ClearData();
			}
		}
		private void UpdateToolbarUI() {
			btnFolder.IsEnabled = taskPage.SelectedTask != null;
			btnEdit.IsEnabled=taskPage.SelectedTask!=null;
			btnClear.IsEnabled = taskPage.SelectedTask != null;
		}
		private void ClearData()
		{
			try
			{
				if (taskPage.SelectedTask is ExportSketchMapTask task)
				{
					var rootPath = TaskOutputPath(task);
					foreach (var n in navigator.Concords)
					{
						var path = rootPath;
						if (!(path.EndsWith("/") || path.EndsWith("\\"))){
							path += "\\";
						}
						path += n.CBFMC;
						if (Directory.Exists(path))
						{
							FileUtil.EnumFiles2(path, fi =>
							 {
								 File.Delete(fi.FullName);
								 return true;
							 });
						}
					}
				}
			}
			catch (Exception ex)
			{
				UIHelper.ShowExceptionMessage(ex);
			}
		}
		/// <summary>
		/// 成果目录
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnDirectory_Click(object sender, RoutedEventArgs e)
		{
			var recorder = taskPage.SelectedTask;
			if (recorder == null)
			{
				return;
			}
			var path = TaskOutputPath(recorder);
			//var args =( recorder.PropertyPage as DataSelectedDialog).Argument;
			//if (args == null || string.IsNullOrEmpty(args.OutputPath) || !System.IO.Directory.Exists(args.OutputPath))
			//{
			//	return;
			//}
			if (path != null)
			{
				System.Diagnostics.Process.Start(path);
			}
		}
		internal static string TaskOutputPath(ITask task)
		{
			var args = (task.PropertyPage as DataSelectedDialog).Argument;
			if (args == null || string.IsNullOrEmpty(args.OutputPath) || !System.IO.Directory.Exists(args.OutputPath))
			{
				return null;
			}
			return args.OutputPath;
		}
		/// <summary>
		/// 编辑地块
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtnEditor_Click(object sender, RoutedEventArgs e)
		{
			if (navigator.Concord == null)
			{
				UIHelper.ShowWarning(Window.GetWindow(this), "未找到需要编辑数据", "编辑地块");
				return;
			}
			var task=taskPage.SelectedTask as ExportSketchMapTask;
			var path = ((DataSelectedDialog)task!.PropertyPage).Argument.OutputPath;

			string fileName = path + @"\" + navigator.Concord.CBFMC + @"\" + "DKSYT" + navigator.Concord.CBFBM + "J"; // InitalizeContractorPath();
			if (string.IsNullOrEmpty(fileName))
			{
				UIHelper.ShowWarning(Window.GetWindow(this), "未找到需要编辑数据", "编辑地块");
				return;
			}
			if (navigator.Concord == null || navigator.Concord.Lands == null || navigator.Concord.Lands.Count() == 0)
			{
				UIHelper.ShowWarning(Window.GetWindow(this), "未找到需要编辑数据", "编辑地块");
				return;
			}
            using var dialog = new EditSketchMapDialog(navigator, task);	
            dialog.ShowDialog(Window.GetWindow(this));
        }

		public void Dispose()
		{
			pageLayout.Dispose();
		}
	}
}
