using System.Collections.Generic;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.GIS;

namespace Agro.Module.DataExchange
{
  /// <summary>
  /// 导入汇交成果数据
  /// </summary>
  public class ImportTasks : GroupTask
    {
        private readonly InputParam _prms = new InputParam();
        public ImportTasks()
        {
            base.Name = "导入汇交数据";
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
                        Library.Handle.ImportShapeAndMdb.TaskImportUtil.AddToTaskGroup(this, _prms);
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

    public class InputParam:HJDataRootPath
    {
        /// <summary>
        /// 数据库连接串
        /// </summary>
        public string? DBConnectionString { get; set; }
        public string? DBProviderName { get; set; }
        public eDatabaseType DatabaseType {
            get {
                if (Workspace != null) {
                    return Workspace.DatabaseType;
                }
                else {
                    if ("DataSource.Oracle" == DBProviderName)
                    {
                        return eDatabaseType.Oracle;
                    }
                    else if ("DataSource.SqlServer" == DBProviderName)
                    {
                       return eDatabaseType.SqlServer;
                    }
                    else if ("DataSource.MySql" == DBProviderName)
                    {
                        return eDatabaseType.MySql;
                    }
                }
                return eDatabaseType.Memory;
            }

        } 
        public IFeatureWorkspace Workspace{get;set;}
        /// <summary>
        /// 指定哪些前缀开头的ShapeFile不导入数据库
        /// </summary>
        public HashSet<string> NotImportShapeFilePrefix;
        public string Init(string sRootPath, string ConnectionString,string providerName,HashSet<string> notImportShapeFilePrefix)
        {
            DBConnectionString = ConnectionString;
            DBProviderName = providerName;
            NotImportShapeFilePrefix = notImportShapeFilePrefix;
            return base.Init(sRootPath);
        }
    }
    
}
