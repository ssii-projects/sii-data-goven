using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agro.LibCore;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.GIS;

namespace Agro.Module.DataExchange
{
    /// <summary>
    /// 导入汇交成果数据
    /// </summary>
    public class ImportNodeTasks: Agro.LibCore.Task.GroupTask
    {
        private readonly InputParam _prm;

        public ImportNodeTasks(InputParam prm)
        {
            _prm = prm;
            //base.Expanded = false;
            base.Name = "导入汇交数据("+ prm.sXzqmc+")";
            this.ClearTasks();
            Library.Handle.ImportShapeAndMdb.TaskImportUtil.AddToTaskGroup(this, _prm);
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            try
            {
                OpenWorkspace();
                var stardatetime = DateTime.Now;
                using (StreamWriter wt = new StreamWriter(_prm.RootPath + "\\数据入库报告.txt", true))
                {
                    var wtstr = string.Format("{0}  {1}导入开始-----", stardatetime.ToString("yyyy/mm/dd hh:MM:ss"), _prm.sXzqmc);
                    wt.WriteLine(wtstr);
                }
                base.DoGo(cancel);
                var timespec = DateTime.Now - stardatetime;
                using (StreamWriter wt = new StreamWriter(_prm.RootPath + "\\数据入库报告.txt", true))
                {
                    var wtstr = string.Format("{0}  {1}导入结束,耗时：{2}", DateTime.Now.ToString("yyyy/mm/dd hh:MM:ss"), _prm.sXzqmc, timespec.ToString());
                    wt.WriteLine(wtstr);
                }
            }
            finally
            {
                if (_prm != null && _prm.Workspace != null)
                {
                    _prm.Workspace.Dispose();
                }
            }
        }
        private void OpenWorkspace() {
            if ("DataSource.Oracle" == _prm.DBProviderName)
            {
                _prm.Workspace = OracleFeatureWorkspaceFactory.Instance.OpenWorkspace(_prm.DBConnectionString);
            }
            else if ("DataSource.SqlServer" == _prm.DBProviderName)
            {
                _prm.Workspace = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(_prm.DBConnectionString);
            }
        }
    }    
}
