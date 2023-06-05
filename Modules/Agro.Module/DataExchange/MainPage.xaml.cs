using Agro.Library.Common;
using Agro.Library.Common.Repository;
using System;
using System.Collections.Generic;
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

namespace Agro.Module.DataExchange
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            taskPage.Title = "数据处理";
            taskPage.ShowRootNode = false;

            //taskPage.AddTask(new ImportShape() { Icon = MyImageSourceUtil.Image24("ImportData.png") });
            taskPage.AddTask(new ImportTasks());
            taskPage.AddTask(new ImportGroupTasks(taskPage));
            taskPage.AddTask(new ImportDcdkData());
            taskPage.AddTask(new ExportDkData());
			taskPage.AddTask(new ExportHjsj());
			//taskPage.AddTask(new ImportSDEDataTask(taskPage));
			taskPage.AddTask(new DataUpdateTask());

            if (DlxxXzdyRepository.Instance.FindRootZone() is ShortZone zone && zone.Code?.StartsWith("50") == true)
            {
                taskPage.AddTask(new UpdateQQMJ2010Task());
            }
            //var prm = new InputParam
            //{
            //	mdbFileName = @"D:\tmp\510681广汉市\5106812017.mdb",
            //	Workspace = Library.Common.MyGlobal.Workspace
            //};
            ////taskPage.AddTask(new Library.Handle.ImportShapeAndMdb.TaskImportUtil.TaskImportData(prm, "导入QSSJCBF", new Library.Handle.ImportShapeAndMdb.ImportCbf(false)));
            //taskPage.AddTask(new Library.Handle.ImportShapeAndMdb.TaskImportUtil.TaskImportData(prm, "导入QSSJ_CBDKXX", new Library.Handle.ImportShapeAndMdb.ImportCBDKXX(false)));
        }
    }
}
