using Agro.GIS;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Agro.Module.PreproccessTool
{
    /// <summary>
    /// 坐标系定义
    /// </summary>
    class CoordinateDefineTask : Task
    {
        /// <summary>
        /// 添加代号
        /// </summary>
        public CoordinateDefineTask()
        {
            base.Name = "坐标系定义";
            base.PropertyPage = new CrsPropertyPage(() => OnApply(), true, false);
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as CrsPropertyPage;
            var ri = new ReportInfo();
            ri.reportProgress += i =>this.ReportProgress((int)i);
            ri.reportError += msg =>this.ReportError(msg);
            ri.reportInfo += msg =>this.ReportInfomation(msg);
			ri.reportWarning += msg => this.ReportWarning(msg);
            try
            {
                ShapeFileUtil.ChangePrjFile(d.InputFileFullName, d.SelectedSpatialReference.ToEsriString());
                ri.reportProgress(100);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private string OnApply()
        {
            var d = PropertyPage as CrsPropertyPage;
            var s = d.tbShpFilePath.Text.Trim();
            if (s.Length == 0)
            {
                return "未输入文件路径！";
            }
            if (!File.Exists(s))
            {
                return "文件：" + s + "不存在！";
            }
            if (!s.ToLower().EndsWith(".shp"))
            {
                return "输入文件必须以.shp结尾！";
            }
            if (d.crsPnl.SelectedSpatialReference == null)
            {
                return "未选择新的坐标系！";
            }
            return null;
        }
    }
}
