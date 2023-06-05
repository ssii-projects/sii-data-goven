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
using Agro.LibCore.UI;

namespace Agro.Module.DataExchange
{
    /// <summary>
    /// 导入汇交成果数据
    /// </summary>
    public class ImportGroupTasks: Agro.LibCore.Task.GroupTask
    {
        private readonly List<InputParam> _prms = new List<InputParam>();

        public ImportGroupTasks(TaskPage page )
        {
            base.Name = "批量导入汇交数据";
            base.Description = "导入符合农业部要求格式数据";
            base.IsExpanded = true;
            //base.ShowRoot = false;
           
            base.PropertyPage = new DataGroupImportDialog((rootPath, notImportShapeFilePrefix )=>
            {
                _prms.Clear();
                string err = null;
                foreach (DataImportProperty prop in rootPath) {
                    InputParam prm = new InputParam();
                    err = prm.Init(prop.Path, prop.ConnectionString, prop.ProviderName,notImportShapeFilePrefix);
                    //prm.sDBConnectionString = prop.ConnectionString;
                    //prm.sDBProviderName = prop.ProviderName;
                    if (!string.IsNullOrEmpty(err)) {
                        return err;
                    }
                    _prms.Add(prm);
                    
                }
                this.ClearTasks();
                foreach (InputParam prm in _prms) {
                    this.AddTask(new ImportNodeTasks(prm));
                }
                return null;
            });
        }
    }    
}
