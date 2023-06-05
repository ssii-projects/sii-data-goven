using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common
{
    internal class MyGlobal
    {
        private static IFeatureWorkspace? _ws;
        public static IFeatureWorkspace Workspace
        {
            get { return _ws!; }
            set { _ws = value; }
        }
    }
}
