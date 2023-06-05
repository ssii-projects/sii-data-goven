using Agro.GIS.UI;
using Agro.GIS;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Module.DataExchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataOperatorTool
{
    /// <summary>
    /// 导出发包方地块
    /// </summary>
    class DownloadFbfDkTask
    {
        public DownloadFbfDkTask()
        {
        }
        public static void ShowDialog(Window owner)//MapControl mapControl, TocControl Toc)
        {
            var    ti = new ExportDkData();
            var dlg = new KuiDialog(owner, "下载地块数据")
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
                                //AddShapeCommand.AddShapeFile(mapControl, Toc, shpFile, dpp.DatabaseType, false);
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
