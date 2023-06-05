using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Library.Common
{
    public class ReportInfo
    {
        public Action<double> reportProgress;
        public Action<string> reportError;
        public Action<string> reportInfo;
		public Action<string> reportWarning;
		public ReportInfo()
		{
		}
	}
}
