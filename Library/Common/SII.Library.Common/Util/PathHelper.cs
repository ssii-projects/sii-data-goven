/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.5
 * 文 件 名：   PathHelper
 * 创 建 人：   颜学铭
 * 创建时间：   2016/5/20 11:32:33
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Captain.Library.Common
{
    public class PathHelper
    {
        static PathHelper()
        {
            if (!System.IO.Directory.Exists(PersistDirectory))
            {
                System.IO.Directory.CreateDirectory(PersistDirectory);
            }
            if (!System.IO.Directory.Exists(TempDirectory))
            {
                System.IO.Directory.CreateDirectory(TempDirectory);
            }
        }
        /// <summary>
        /// 执行文件的目录，以“/”结合
        /// </summary>
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 序列化目录
        /// </summary>
        public static readonly string PersistDirectory = BaseDirectory + @"Persist\";
        public static readonly string TempDirectory = BaseDirectory + @"Temp\";
    }
}
