using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.LibCore.UI;
using System.Windows;
using System.Windows.Controls;

namespace Agro.Module.DataUpdate
{
	/// <summary>
	/// UserControl1.xaml 的交互逻辑
	/// </summary>
	public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            taskPage.Title = "数据更新";
            taskPage.ShowRootNode = false;
			taskPage.AddTask(new DataDeleteTask());

			taskPage.AddTask(new ImportTasks());
        }
    }

	public class DataDeleteTask : Task
	{
		public DataDeleteTask()
		{
			Name = "数据删除";
		}
		public override void OnPreAddTaskToPool(CancelItem<ITask> cancel)
		{
			cancel.Cancel = true;
			var pnl = new DataDeleteProperyPage();
			var dlg = new KuiDialog(Application.Current.MainWindow, "数据删除", false, false)
			{
				Content = pnl,
				Width = 740,
				Height = 440
			};
			dlg.Closing += pnl.OnDlgClosing;
			dlg.ShowDialog();
		}
		protected override void DoGo(ICancelTracker cancel)
		{
			//throw new NotImplementedException();
		}
	}
}
