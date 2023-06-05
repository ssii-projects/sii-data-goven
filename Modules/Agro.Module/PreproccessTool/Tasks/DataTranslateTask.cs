using Agro.GIS;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.LibCore.Task;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.PreproccessTool
{
    /// <summary>
    /// 数据平移
    /// </summary>
    class DataTranslateTask : Task
    {
        /// <summary>
        /// 添加代号
        /// </summary>
        public DataTranslateTask()
        {
            base.Name = "数据平移";
            base.PropertyPage = new DataTranslatePropertyPage();
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as DataTranslatePropertyPage;
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
                ShapeFileUtil.CopyShapeFile(d.ShapeFileFullName, d.OutputFileFullName);
                DoProcess(d.OutputFileFullName, d.XOffset, d.YOffset, ri);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private void DoProcess(string shpFile, double xOffset, double yOffset, ReportInfo reportInfo)
        {
            double oldProgress = 0;
            var offset = new OffsetFilter(new Coordinate(xOffset, yOffset),null);
            using (var shp = new ShapeFile())
            {
                shp.Open(shpFile, "rb+");
                var cnt = shp.GetRecordCount();
                for (int i = 0; i < cnt; ++i)
                {
                    var g = shp.GetGeometry(i);
                    if (g == null || g.Coordinates == null || g.Coordinates.Length < 1)
                    {
                        continue;
                    }
                    var p = g.Coordinates[0];
                    if (p.X < 360)
                    {
                        continue;
                    }
                    //var n = (int)p.X;
                    //if (n.ToString().Length > 6)
                    //{
                    //    continue;
                    //}
                    g.Apply(offset);
                    g.GeometryChanged();
                    shp.WriteWKB(i, g.AsBinary());
                    ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, i, ref oldProgress);
                }
                ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);
            }
        }
    }
}