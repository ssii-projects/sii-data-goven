using Agro.GIS;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.LibCore.Task;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Agro.Module.PreproccessTool
{
    /// <summary>
    /// 更改坐标系
    /// </summary>
    class CoordinateChangeTask : Task
    {
        /// <summary>
        /// 添加代号
        /// </summary>
        public CoordinateChangeTask()
        {
            base.Name = "更改坐标系";
            base.PropertyPage = new CrsPropertyPage(()=>OnApply(),true,false);
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as CrsPropertyPage;
            var ri = new ReportInfo();
            ri.reportProgress += i =>
            {
                this.ReportProgress((int)i);

            };
            ri.reportError += msg =>
            {
                this.ReportError(msg);
            };
            ri.reportInfo += msg =>
            {
                this.ReportInfomation(msg);
            };
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
            //d.SelectedSpatialReference = d.crsPnl.SelectedSpatialReference;
            //if (InputShpFiles.Count == 0)
            //{
            //    return "未输入文件路径！";
            //}
            //var s = tbOutShpFilePath.Text.Trim();
            //if (s.Length == 0)
            //{
            //    return "未选择输出文件路径！";
            //}
            //if (!s.ToLower().EndsWith(".shp"))
            //{
            //    return "输出文件必须以.shp结尾！";
            //}
            //var shpFile = InputShpFiles[0];
            //for(int i = 1; i < InputShpFiles.Count; ++i)
            //{
            //    var shpFile2 = InputShpFiles[i];
            //    var err = ShapeFileUtil.IsSameStruct(shpFile,shpFile2);
            //    if (err!=null)
            //    {
            //        return shpFile + "与" + shpFile2 + err;
            //    }
            //}
            return null;
        }
    }
}
