/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   MyBitmapResourceUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/11/26 20:39:05
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Agro.Module
{
  internal class MyImageSourceUtil
    {
        /// <summary>
        /// 用当前程序集Resources/Images/32下指定名称的图片构造BitmapImage实例
        /// </summary>
        /// <param name="picFileName"></param>
        /// <returns></returns>
        internal static BitmapImage Image32(string picFileName)
        {
            return CreateImage(picFileName, 32);
        }
        /// <summary>
        /// 用当前程序集Resources/Images/24下指定名称的图片构造BitmapImage实例
        /// </summary>
        /// <param name="picFileName"></param>
        /// <returns></returns>
        internal static BitmapImage Image24(string picFileName)
        {
            return CreateImage(picFileName, 24);
        }
        /// <summary>
        /// 用当前程序集Resources/Images/16下指定名称的图片构造BitmapImage实例
        /// </summary>
        /// <param name="picFileName"></param>
        /// <returns></returns>
        internal static BitmapImage Image16(string picFileName)
        {
            return CreateImage(picFileName, 16);
        }
		//internal static BitmapImage LibCoreImage16(string picFileName)
		//{
		//	var appName = "Agro.LibCore";// Assembly.GetCallingAssembly().GetName().Name;
		//	var path = string.Format(@"pack://application:,,,/{0};component/UI/Resources\Images\{1}\{2}", appName, 16, picFileName);
		//	//Console.WriteLine("path=" + path);
		//	return new BitmapImage(new Uri(path, UriKind.Absolute));
		//}

		private static BitmapImage CreateImage(string picFileName, int size)
        {
            var appName = Assembly.GetExecutingAssembly().GetName().Name;
            var path = string.Format(@"pack://application:,,,/{0};component/Resources\Images\{1}\{2}", appName, size, picFileName);
            return new BitmapImage(new Uri(path, UriKind.Absolute));
        }
    }
}
