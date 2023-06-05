using Agro.GIS;
using Agro.Library.Common;
using Agro.LibCore;
using Agro.LibCore.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.PreproccessTool
{
    /// <summary>
    /// 去除代号
    /// </summary>
    class RemoveMarkTask : Task
    {
        /// <summary>
        /// 去除代号
        /// </summary>
        public RemoveMarkTask()
        {
            base.Name = "去除代号";
            base.PropertyPage = new AddBandPropertyPage(false);
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as AddBandPropertyPage;
            var ri = new ReportInfo();
            ri.reportProgress += i =>
            {
                // it.PbValue = i;
                this.ReportProgress((int)i);

            };
            ri.reportError += msg =>
            {
                this.ReportError(msg);
                //it.Info = msg;
            };
            ri.reportInfo += msg =>
            {
                this.ReportInfomation(msg);
                //it.Info = msg;
            };
            try
            {
                ShapeFileUtil.CopyShapeFile(d.ShapeFileFullName, d.OutputFileFullName);
                DoProcess(d.OutputFileFullName, ri);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private void DoProcess(string shpFile, ReportInfo reportInfo)
        {
            //double oldProgress = 0;

            var offsetXY = new GeoAPI.Geometries.Coordinate();
            OffsetFilter offset = new OffsetFilter(offsetXY,null);// new Coordinate(nBand * 1000000, 0));
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
                    var s = ((int)p.X).ToString();
                    if (s.Length != 8)
                    {
                        continue;
                    }
                    var nBand = SafeConvertAux.ToInt32(s.Substring(0, 2));
                    offsetXY.X = -nBand * 1000000;
                    g.Apply(offset);
                    g.GeometryChanged();
                    shp.WriteWKB(i, g.AsBinary());
                    //ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, i, ref oldProgress);
                }
                //ProgressUtil.ReportProgress(reportInfo.reportProgress, cnt, cnt, ref oldProgress);
            }
        }
    }
}
