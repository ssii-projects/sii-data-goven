using System.Windows.Controls;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// UserControl1.xaml 的交互逻辑
	/// </summary>
	public partial class HekuPage : UserControl
    {
        public HekuPage()
        {
            InitializeComponent();
            taskPage.Title = "合库工具";
            taskPage.ShowRootNode = false;

            //taskPage.AddTask(new ImportShape() { Icon = MyImageSourceUtil.Image24("ImportData.png") });
   //         taskPage.AddTask(new ImportTasks());
   //         taskPage.AddTask(new ImportGroupTasks(taskPage));
   //         taskPage.AddTask(new ImportDcdkData());
   //         taskPage.AddTask(new ExportDkData());
			//taskPage.AddTask(new ExportHjsj());
			taskPage.AddTask(new ImportSDEDataTask(taskPage));
			//taskPage.AddTask(new DataUpdateTask());
		}
    }
}
