using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Module.ImportData
{
    class ProgressUtil
    {
        public static void reportProgress(Action<double> callback, int count, int i, ref double nOldProgress)
        {
            if (count > 0)
            {
                double nProgress = Math.Round((double)i * 100 / count, 1);
                if (nOldProgress != nProgress)
                {
                    callback(nProgress);
                    nOldProgress = nProgress;
                }
            }
            else
            {
                callback(100);
            }
        }
    }
}
