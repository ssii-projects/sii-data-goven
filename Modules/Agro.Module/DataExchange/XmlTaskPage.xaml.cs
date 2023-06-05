using System.Windows.Controls;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// UserControl1.xaml 的交互逻辑
	/// </summary>
	public partial class XmlTaskPage : UserControl
    {
        public XmlTaskPage()
        {
            InitializeComponent();
            taskPage.Title = "数据交换";
            taskPage.ShowRootNode = false;
			taskPage.AddTask(new XmlTask.ExportXmlTask());
			taskPage.AddTask(new XmlTask.ImportXmlTask());
		}
    }
}
