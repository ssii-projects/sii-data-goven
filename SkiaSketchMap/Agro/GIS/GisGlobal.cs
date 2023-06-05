using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.GIS
{
    public static class GisGlobal
    {
        private static ISymbolFactory? factory=null;
        public static ISymbolFactory SymbolFactory
        {
            get
            {
                System.Diagnostics.Debug.Assert(factory!=null);
                return factory!;
            }
            set
            {
                factory = value;
            }
        }
    }
}
