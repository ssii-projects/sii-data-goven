using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
    public static class FileUtil
    {
        /// <summary>
        /// 非递归遍历指定目录下的所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        public static void EnumFiles(string path, Func<System.IO.FileInfo, bool> callback)
        {
            try
            {
                if (System.IO.Directory.Exists(path))
                {
                    var di = new System.IO.DirectoryInfo(path);
                    var files = di.GetFiles();
                    foreach (var f in files)
                    {
                        var fContinue = callback(f);
                        if (!fContinue)
                            return;
                    }
                }
            }
            catch { }
        }
        /// <summary>
        /// 递归遍历指定目录下的所有文件
        /// </summary>
        /// <param name="folderFullName"></param>
        /// <param name="callback"></param>
        public static void EnumFiles2(string folderFullName, Func<System.IO.FileInfo, bool> callback)
        {
            try
            {
                var di = new System.IO.DirectoryInfo(folderFullName);
                //遍历文件
                foreach (var NextFile in di.GetFiles())
                {
                    var fContinue = callback(NextFile);
                    if (!fContinue)
                        return;
                }
                //遍历文件夹
                foreach (var NextFolder in di.GetDirectories())
                {
                    EnumFiles2(NextFolder.FullName, callback);
                }
            }
            catch { }
        }


        /// <summary>
        /// 递归遍历指定目录下的所有目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        public static void EnumFolder(string path, Func<System.IO.DirectoryInfo, bool> callback)
        {
            if (Directory.Exists(path))
            {
                enumFolder(path, callback);
            }
        }

        /// <summary>
        /// 递归遍历指定目录下的所有目录
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        private static bool enumFolder(string path, Func<System.IO.DirectoryInfo, bool> callback)
        {
            var di = new System.IO.DirectoryInfo(path);
            //遍历文件夹
            foreach (var NextFolder in di.GetDirectories())
            {
                var fContinue = callback(NextFolder);
                if (!fContinue)
                {
                    return false;
                }
                fContinue = enumFolder(NextFolder.FullName, callback);
                if (!fContinue)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 根据文件名的完整路径获取所在目录，结尾包含/
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePath(string fileName)
        {
            var s = fileName.Replace('\\', '/');
            int n = s.LastIndexOf('/');
            s = fileName.Substring(0, n + 1);
            return s;
        }

        /// <summary>
        /// 确保文件路径已/或\结尾
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MakeValidPath(string path)
        {
            if (!(path.EndsWith("/") || path.EndsWith("\\")))
            {
                path += "\\";
            }
            return path;
        }

        /// <summary>
        /// 根据文件名的完整路径获取文件名（不包含文件夹路径）
        /// </summary>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        public static string GetFileName(string fullFileName)
        {
            var s = fullFileName.Replace('\\', '/');
            int n = s.LastIndexOf('/');
            s = fullFileName.Substring(n + 1);
            return s;
        }
        /// <summary>
        /// 根据文件名获取扩展名（包括.），比如参数为c:/a.shp，则返回.shp
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileExtension(string fileName)
        {
            int n = fileName.LastIndexOf('.');
            return n < 0 ? "" : fileName.Substring(n);
        }

        /// <summary>
        /// 小于1M的采用KB显示
        /// 小于1G的采用M显示
        /// 小于1T的采用G显示
        /// 其余采用T显示
        /// </summary>
        /// <param name="len">单位：字节</param>
        /// <returns></returns>
        public static string GetFormattedFileLenth(long length)
        {
            double len = length / 1024;
            if (len < 1024)
            {
                return len + "KB";
            }
            len = len / 1024;
            if (len < 1024)
            {
                return Math.Round(len, 2) + "MB";
            }
            len = len / 1024;
            if (len < 1024)
            {
                return Math.Round(len, 2) + "GB";
            }
            return "";
        }
    }
}
