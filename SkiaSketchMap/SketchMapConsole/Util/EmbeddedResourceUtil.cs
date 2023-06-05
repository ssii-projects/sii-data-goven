using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SketchMapConsole.Util
{
    internal static class EmbeddedResourceUtil
    {
        public static string GetFontPath(string name)
        {
            var exeName = Assembly.GetCallingAssembly().GetName().Name;
            return $"{exeName}.Data.Fonts.{name}";
        }
    }
}
