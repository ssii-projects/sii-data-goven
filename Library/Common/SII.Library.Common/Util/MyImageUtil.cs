using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;

namespace Agro.Library.Common.Util
{
    public class CommonImageUtil
    {
        /// <summary>
        /// 用当前程序集Resources/Images/32下指定名称的图片构造BitmapImage实例
        /// </summary>
        /// <param name="picFileName"></param>
        /// <returns></returns>
        public static BitmapImage Image32(string picFileName)
        {
            return CreateImage(picFileName, 32);
        }
        public static BitmapImage Image24(string picFileName)
        {
            return CreateImage(picFileName, 24);
        }
        /// <summary>
        /// 用当前程序集Resources/Images/16下指定名称的图片构造BitmapImage实例
        /// </summary>
        /// <param name="picFileName"></param>
        /// <returns></returns>
        public static BitmapImage Image16(string picFileName)
        {
            return CreateImage(picFileName, 16);
        }
        private static BitmapImage CreateImage(string picFileName, int size)
        {
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
			var path = string.Format(@"pack://application:,,,/{0};component/Resources\Images\{1}\{2}", appName, size, picFileName);
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }
    }
}
