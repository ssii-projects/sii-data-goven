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
    /// 数据拼接
    /// </summary>
    class DataMosaicTask : Task
    {
        /// <summary>
        /// 数据拼接
        /// </summary>
        public DataMosaicTask()
        {
            base.Name = "矢量数据拼接";
            base.PropertyPage = new AddBandPropertyPage(false, true, () =>CheckPropertyPageValue());
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as AddBandPropertyPage;
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
                DoProcess(d.InputShpFiles, d.OutputFileFullName, ri.reportProgress);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private string CheckPropertyPageValue()
        {
            var d = PropertyPage as AddBandPropertyPage;
            var InputShpFiles = d.InputShpFiles;
            var shpFile = InputShpFiles[0];
            for (int i = 1; i < InputShpFiles.Count; ++i)
            {
                var shpFile2 = InputShpFiles[i];
                var err = ShapeFileUtil.IsSameStruct(shpFile, shpFile2);
                if (err != null)
                {
                    return shpFile + "与" + shpFile2 + err;
                }
            }
            return null;
        }

        private void DoProcess(List<string> inputShpFiles, string outFile, Action<double> reportProgress)//  ReportInfo reportInfo)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }
            double oldProgress = 0;
            int iProgress = 0;
            int nProgressCount = 100 * (inputShpFiles.Count - 1);
            ShapeFileUtil.CopyShapeFile(inputShpFiles[0], outFile);
            for (int i = 1; i < inputShpFiles.Count; ++i)
            {
                using (var su = new ShapeUnion())
                {
                    var shpFile = inputShpFiles[i];
                    su.Union(outFile, shpFile, n =>
                    {
                        ProgressUtil.ReportProgress(reportProgress, nProgressCount, ++iProgress, ref oldProgress);
                    });
                }
            }
            reportProgress(100);
            //ProgressUtil.ReportProgress(reportProgress, nProgressCount, ++iProgress, ref oldProgress);
        }
    }
}
