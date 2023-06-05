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
    ///  影像数据拼接
    /// </summary>
    class RasterMosaicTask : Task
    {
        /// <summary>
        /// 数据拼接
        /// </summary>
        public RasterMosaicTask()
        {
            base.Name = "影像服务数据拼接";
            base.PropertyPage = new RasterMosaicPropertyPage();
        }
        protected override void DoGo(ICancelTracker cancel)
        {
            var d = PropertyPage as RasterMosaicPropertyPage;
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
                DoProcess(d.InputShpFiles, d.OutputFileFullName, ri.reportProgress,ri.reportWarning,ri.reportError);
            }
            catch (Exception ex)
            {
                ri.reportError(ex.Message);
            }
        }

        private void DoProcess(List<string> inputShpFiles, string outFile, Action<double> reportProgress,Action<string> onWarning,Action<string> onError)
        {
            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            MergePic.Merge(inputShpFiles, outFile, reportProgress,onWarning,onError);// i =>
        }
    }
}
