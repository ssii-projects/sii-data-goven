using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataOperatorTool
{
    internal static class AppPref
    {
        public static AppType AppType{ get; set; }
        /// <summary>
        /// 是否包含下载发包方地块功能
        /// </summary>
        public static bool UseDownFbfDk
        {
            get
            {
                return AppType == AppType.DataOperator_WebService;
            }
        }
        public static string WebServiceUrl { get; set; }=string.Empty;
        //public static string UpLoadUrl { get;set; }=string.Empty;
    }
}
