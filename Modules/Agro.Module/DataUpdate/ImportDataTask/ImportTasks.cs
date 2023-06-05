using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Module.DataExchange;
using Agro.Library.Handle.ImportShapeAndMdb;

namespace Agro.Module.DataUpdate
{
	/// <summary>
	/// 导入汇交成果数据
	/// </summary>
	public class ImportTasks : GroupTask
    {
        private readonly InputParam _prms = new InputParam();
        public ImportTasks()
        {
            base.Name = "追加导入汇交数据";
            base.Description = "导入符合农业部要求格式数据";

            base.PropertyPage = new DataImportDialog((rootPath, notImportShapeFilePrefix) =>
            {
                var os = _prms.NotImportShapeFilePrefix;
                var err = _prms.Init(rootPath, MyGlobal.Workspace.ConnectionString,"", notImportShapeFilePrefix);
                if (err == null)
                {
                    bool fChanged = false;
                    if (os==null||os.Count != notImportShapeFilePrefix.Count)
                    {
                        fChanged = true;
                    }
                    else
                    {
                        foreach(var k in os)
                        {
                            if (!notImportShapeFilePrefix.Contains(k))
                            {
                                fChanged = true;
                                break;
                            }
                        }
                    }
                    if (fChanged)
                    {
                        this.ClearTasks();
                        TaskImportUtil.AddToTaskGroup(this, _prms,false);
                    }
                }
                return err;
            });
            _prms.Workspace = MyGlobal.Workspace;
           // Library.Handle.ImportShapeAndMdb.TaskImportUtil.AddToTaskGroup(this, _prms);
        }
        public new void Go(ICancelTracker cancel)
        {
            base.Go(cancel);
        }
    }   
}
