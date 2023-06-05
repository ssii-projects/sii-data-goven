using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JzdxBuild
{
    public class ProgressHelper
    {
        public static void ReportProgress(Action<string, double> callback, string msg, int count, int i, ref double nOldProgress)
        {
            if (callback != null && count > 0)
            {
                double nProgress =Math.Round(i * 100.0 / count,1);
                if (nOldProgress != nProgress)
                {
                    callback(msg, nProgress);
                    nOldProgress = nProgress;
                }
            }
        }
        public static void ReportProgress(Action<string, int> callback, string msg, int count, int i, ref int nOldProgress)
        {
            if (callback != null && count > 0)
            {
                int nProgress = i * 100 / count;
                if (nOldProgress != nProgress)
                {
                    callback(msg, nProgress);
                    nOldProgress = nProgress;
                }
            }
        }
        public static void ReportProgress(Action<int> callback, int count, int i, ref int nOldProgress)
        {
            if (count > 0)
            {
                int nProgress =(int)( i * 100.0 / count);
                if (nOldProgress != nProgress)
                {
                    nOldProgress = nProgress;
                    callback(nProgress);
                }
            }
        }
    }
}
