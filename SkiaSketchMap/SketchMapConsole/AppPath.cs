using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SketchMapConsole
{
    internal static class AppPath
    {
        public static string DataPath =>$"{AppDomain.CurrentDomain.BaseDirectory}Data/";
        public static string TemplatePath => $"{DataPath}Template/";
        public static string FontPath => $"{DataPath}Fonts/";

    }
}
