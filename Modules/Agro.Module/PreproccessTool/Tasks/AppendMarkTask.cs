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
    /// 添加代号
    /// </summary>
    class AppendMarkTask:Task
    {
        /// <summary>
        /// 添加代号
        /// </summary>
        public AppendMarkTask()
        {
            base.Name = "添加代号";
            base.PropertyPage = new AddBandPropertyPage();
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            //for (int i = 0; i < 100; ++i)
            //{
            //    if (cancel.Cancel())
            //    {
            //        break;
            //    }
            //    Thread.Sleep(50);
            //    base.ReportProgress(i + 1);
            //}

            var d =PropertyPage as AddBandPropertyPage;
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
                DoProcess(d.OutputFileFullName, d.Band, ri);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private void DoProcess(string shpFile, int nBand, ReportInfo reportInfo)
        {
            //double oldProgress = 0;
            var offset = new OffsetFilter(new Coordinate(nBand * 1000000, 0),null);
            using (var shp = new ShapeFile())
            {
                shp.Open(shpFile, "rb+");
                var cnt = shp.GetRecordCount();
				var progress = new ProgressReporter(reportInfo.reportProgress, cnt);
				for (int i = 0; i < cnt; ++i)
                {
					progress.Step();
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
                    var n = (int)p.X;
                    if (n.ToString().Length > 6)
                    {
                        continue;
                    }
                    g.Apply(offset);
                    g.GeometryChanged();
                    shp.WriteWKB(i, g.AsBinary());
                    //ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, i, ref oldProgress);
                }
                //var env=shp.GetFullExtent();
                //ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);
            }
        }
    }
}
