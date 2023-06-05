/*
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.5
 * 文 件 名：   ShapeFileHelper
 * 创 建 人：   颜学铭
 * 创建时间：   2016/6/1 11:17:05
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.GIS;
using System;
using System.IO;

namespace JzdxBuild
{
    public static class ShapeFileHelper
    {
        public static bool IsFileEmpty(string shpFile)
        {
            using (var shp = new ShapeFile())
            {
                var err=shp.Open(shpFile);
                if (err != null)
                    throw new Exception(err);
                return shp.GetRecordCount() == 0;
            }
        }
        public static void ClearShapeFile(string shpFile)
        {
            try
            {
                if (IsFileEmpty(shpFile))
                    return;
            }
            catch
            {

            }
            var s = shpFile.Replace('\\', '/');
            int n = s.LastIndexOf('/');
            var path = s.Substring(0, n + 1);
            var name = s.Substring(n + 1);
            n = name.LastIndexOf('.');
            name = name.Substring(0, n);


            var outName=System.Guid.NewGuid().ToString();
            var outPath=Path.GetTempPath();
            using (var srcShp = new ShapeFile())
            {
                srcShp.Open(shpFile);
                srcShp.CopyStruct(outPath + outName);
            }
            var sa = new string[]{
                ".shp",".dbf",".shx"
            };
            foreach (var ext in sa)
            {
                var fileName=path + name + ext;
                File.Delete(fileName);
                var tmpFileName=outPath + outName + ext;
                File.Copy(tmpFileName, fileName);
                File.Delete(tmpFileName);
            }
            s = outPath + outName + ".prj";
            if (File.Exists(s))
            {
                File.Delete(s);
            }
        }
    }
}
