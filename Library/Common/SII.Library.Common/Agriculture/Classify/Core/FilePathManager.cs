/*
 * (C)2016 公司版权所有,保留所有权利
*/
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetSharp;
using Captain.GIS;

namespace Agriculture.Classify
{
    /// <summary>
    /// 文件目录
    /// </summary>
    public class FilePathManager
    {
        #region Property

        /// <summary>
        /// 文件目录
        /// </summary>
        public PathMingleInfo CurrentPath { get; set; }

        /// <summary>
        /// 矢量数据目录名称
        /// </summary>
        public string VictorName
        {
            get { return "矢量数据"; }
        }

        /// <summary>
        /// 权属目录名称
        /// </summary>
        public string CategoryName
        {
            get { return "权属数据"; }
        }

        /// <summary>
        /// 栅格数据目录名称
        /// </summary>
        public string RuleName
        {
            get { return "栅格数据"; }
        }

        /// <summary>
        /// 扫描资料目录名称
        /// </summary>
        public string ScanName
        {
            get { return "扫描资料"; }
        }

        /// <summary>
        /// 图件目录名称
        /// </summary>
        public string ImageName
        {
            get { return "图件"; }
        }

        /// <summary>
        /// 汇总目录名称
        /// </summary>
        public string SumName
        {
            get { return "汇总表格"; }
        }

        /// <summary>
        /// 文档目录名称
        /// </summary>
        public string WordName
        {
            get { return "文字报告"; }
        }

        /// <summary>
        /// 其他资料目录名称
        /// </summary>
        public string OtherName
        {
            get { return "其他资料"; }
        }

        /// <summary>
        /// 正射影像图名称
        /// </summary>
        public string RuleJustName
        {
            get { return "数字正射影像图"; }
        }

        /// <summary>
        /// 数字栅格目录名称
        /// </summary>
        public string RuleMapName
        {
            get { return "数字栅格地图"; }
        }

        /// <summary>
        /// 其他栅格目录名称
        /// </summary>
        public string RuleOtherName
        {
            get { return "其他栅格数据"; }
        }

        /// <summary>
        /// 点之记目录名称
        /// </summary>
        public string PointName
        {
            get { return "点之记"; }
        }

        /// <summary>
        /// 权属来源证明资料附件目录名称
        /// </summary>
        public string FJName
        {
            get
            {
                return "权属来源附件";
            }
        }

        /// <summary>
        /// Excel目录名称
        /// </summary>
        public string ExcelName
        {
            get { return "权属单位代码表"; }
        }

        /// <summary>
        /// 矢量元数据名称
        /// </summary>
        public string MetaFileName
        {
            get { return "SL"; }
        }

        /// <summary>
        /// 权属单位代码表
        /// </summary>
        public string CategoryZoneName
        {
            get { return "权属单位代码表"; }
        }

        /// <summary>
        /// 按地块汇总表
        /// </summary>
        public string SummaryByLand
        {
            get { return "按地块汇总表"; }
        }

        /// <summary>
        /// 按承包地土地用途汇总表
        /// </summary>
        public string SummaryLandUserFor
        {
            get { return "按承包地土地用途汇总表"; }
        }

        /// <summary>
        /// 按承包地所有权性质汇总表
        /// </summary>
        public string SummaryLandOwn
        {
            get { return "按承包地所有权性质汇总表"; }
        }

        /// <summary>
        /// 按非承包地地块类别汇总表
        /// </summary>
        public string SummaryLandType
        {
            get { return "按非承包地地块类别汇总表"; }
        }

        /// <summary>
        /// 按承包地是否基本农田汇总表
        /// </summary>
        public string SummaryLandIsBase
        {
            get { return "按承包地是否基本农田汇总表"; }
        }

        /// <summary>
        /// 按权证信息汇总表
        /// </summary>
        public string SummaryRigsterBook
        {
            get { return "按权证信息汇总表"; }
        }

        /// <summary>
        /// 按承包方汇总表
        /// </summary>
        public string SummaryContractor
        {
            get { return "按承包方汇总表"; }
        }

        /// <summary>
        /// 其他扫描资料
        /// </summary>
        public string OtherScanFile
        {
            get { return "其他扫描资料"; }
        }

        /// <summary>
        /// Excel文件目录下汇总表扩展名称
        /// </summary>
        public string ExcelFileSummaryFile
        {
            get { return "农村承包土地确权登记汇总表"; }
        }
        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public FilePathManager()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// 获取文件目录路径
        /// </summary>
        public static FilePathInfo GetCurrentPath(string filePath, string extentName = "")
        {
            FilePathInfo currentPath = new FilePathInfo();
            if (string.IsNullOrEmpty(filePath))
            {
                return currentPath;
            }
            FilePathManager fpm = new FilePathManager();
            currentPath.Path = filePath;
            currentPath.WordReportPath = Path.Combine(filePath, fpm.WordName);
            currentPath.VictorFilePath = Path.Combine(filePath, fpm.VictorName);
            currentPath.ThroneFilePath = Path.Combine(filePath, fpm.CategoryName);
            currentPath.SumTablePath = Path.Combine(filePath, fpm.SumName);
            currentPath.ScanFilePath = Path.Combine(filePath, fpm.ScanName);
            currentPath.RuleFilePath = Path.Combine(filePath, fpm.RuleName);
            currentPath.OtherFilePath = Path.Combine(filePath, fpm.OtherName);
            currentPath.ImagePath = Path.Combine(filePath, fpm.ImageName);
            currentPath.RuleJustPath = Path.Combine(filePath, fpm.RuleName, fpm.RuleJustName);
            currentPath.RuleDigitalPath = Path.Combine(filePath, fpm.RuleName, fpm.RuleMapName);
            currentPath.RuleOtherPath = Path.Combine(filePath, fpm.RuleName, fpm.RuleOtherName);
            currentPath.FJFilePath = Path.Combine(filePath, fpm.ScanName, fpm.FJName);
            currentPath.ControlPointPath = Path.Combine(filePath, fpm.ScanName, fpm.PointName);
            currentPath.ExcelTablePath = Path.Combine(filePath, fpm.SumName, fpm.ExcelName);
            string rootPath = filePath.Substring(0, filePath.LastIndexOf("\\"));
            currentPath.RootPath = rootPath;
            if (currentPath.ShapeFileList == null && Directory.Exists(currentPath.VictorFilePath))
            {
                string[] files = Directory.GetFiles(currentPath.VictorFilePath);
                if (files.Length > 0)
                {
                    currentPath.ShapeFileList = CreateNamePair(files, extentName);
                }
            }
            fpm = null;
            return currentPath;
        }

        /// <summary>
        /// 获取数据库目录
        /// </summary>
        /// <param name="filePath">选择路径</param>
        /// <param name="exportScan">是否导出扫描资料</param>
        public static Dictionary<string, string> GetDataBasePath(string filePath, bool exportScan = false)
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                Directory.CreateDirectory(filePath);
                for (int i = 1; i <= 8; i++)
                {
                    string fileIndex = "file70021" + i.ToString();
                    string name = LanguageAttribute.GetLanguage(fileIndex);
                    if (name.Contains('?'))
                    {
                        continue;
                    }
                    if (!exportScan && name.Equals("扫描资料"))
                    {
                        continue;
                    }
                    dic.Add(name, Path.Combine(filePath, name));
                    for (int j = 0; j < 5; j++)
                    {
                        string cname = LanguageAttribute.GetLanguage(fileIndex + j.ToString());
                        if (cname.Contains('?'))
                        {
                            continue;
                        }
                        dic.Add(cname, Path.Combine(dic[name], cname));
                    }
                }
                foreach (var item in dic)
                {
                    Directory.CreateDirectory(item.Value);
                }
                return dic;
            }
            catch (Exception ex)
            {
                string errorMsg = "创建导出目录出错:" + ex.Message;
                LogWrite.WriteErrorLog(errorMsg + ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 获Shap文件信息
        /// </summary>
        /// <param name="directory">文件目录数组</param>
        /// <param name="extentionName">扩展名称</param>
        /// <param name="fileList">文件名集合</param>
        public static List<FileCondition> CreateNamePair(string[] directory, string extentionName = "", List<FileCondition> extentList = null, List<string> fileList = null)
        {
            List<FileCondition> namePairList = new List<FileCondition>();
            if (directory.Length == 0)
            {
                return namePairList;
            }
            List<string> otherTypeFile = new List<string>();
            for (int i = 0; i < directory.Length; i++)
            {
                string extentName = Path.GetExtension(directory[i]);
                if (extentName.Equals(".dbf") || extentName.Equals(".prj") ||
                       extentName.Equals(".shx") || extentName.Equals(".dbf")
                     || extentName.Equals(".sbx") || extentName.Equals(".sbn")
                    )
                {
                    otherTypeFile.Add(directory[i]);
                    continue;
                }
                var gexspare = new Regex("[a-zA-Z]{2,3}\\d{10}-\\d+");
                FileCondition fnp = new FileCondition();
                fnp.IsExist = true;
                fnp.FilePath = directory[i];
                fnp.ExtFileName = Path.GetFileName(fnp.FilePath);
                fnp.FullName = Path.GetFileNameWithoutExtension(fnp.FilePath);
                if (fileList != null)
                {
                    fileList.Add(fnp.FullName);
                }
                if (extentName.ToLower().Equals(".shp"))
                {
                    string dbffileName = Path.ChangeExtension(fnp.FilePath, "dbf");
                    string shxfileName = Path.ChangeExtension(fnp.FilePath, "shx");
                    string prjfileName = Path.ChangeExtension(fnp.FilePath, "prj");
                    if (!File.Exists(shxfileName))
                    {
                        fnp.LackInfo.Add("shx");
                        fnp.IsFileComplate = false;
                    }
                    if (!File.Exists(dbffileName))
                    {
                        fnp.LackInfo.Add("dbf");
                        fnp.IsFileComplate = false;
                    }
                    if (!File.Exists(prjfileName))
                    {
                        fnp.LackInfo.Add("prj");
                        fnp.IsFileComplate = false;
                    }
                    if (!string.IsNullOrEmpty(extentionName))
                    {
                        if (gexspare.IsMatch(fnp.FullName))
                        {
                            var gex = new Regex("\\d{10}-\\d+");
                            fnp.Name = gex.Replace(fnp.FullName, "");
                            //fnp.Name = fnp.FullName.Replace(extentionName + "-", "");
                        }
                        else
                            fnp.Name = fnp.FullName.Replace(extentionName, "");
                    }
                    else
                    {
                        if (gexspare.IsMatch(fnp.FullName))
                        {
                            var gex = new Regex("\\d{10}-\\d+");
                            fnp.Name = gex.Replace(fnp.FullName, "");
                        }
                        else
                        {
                            var gex = new Regex("\\d+");
                            fnp.Name = gex.Replace(fnp.FullName, "");
                        }
                    }
                    try
                    {
                        using (var sf = new ShapeFile())
                        {
                            var err = sf.Open(fnp.FilePath);
                            if (!string.IsNullOrEmpty(err))
                            {
                                fnp.CanRead = false;
                            }
                            else
                            {
                                fnp.CanRead = true;
                                fnp.DataCount = sf.GetRecordCount();
                            }
                        }

                        using (var reader = new ShapefileDataReader(fnp.FilePath, GeometryFactory.Default))
                        {
                            if (!fnp.CanRead)
                            {
                                reader.Read();
                                fnp.DataCount = reader.RecordCount;
                                fnp.CanRead = true;
                            }
                            if (reader.ShapeHeader.ShapeType > ShapeGeometryType.MultiPoint)
                            {
                                fnp.HasZM = true;
                                fnp.CanRead = false;
                            }
                        }
                    }
                    catch
                    {
                        fnp.CanRead = false;
                    }
                    namePairList.Add(fnp);
                }
                else
                {
                    if (extentList == null)
                    {
                        continue;
                    }
                    extentList.Add(fnp);
                }
            }
            foreach (var item in namePairList)
            {
                otherTypeFile.RemoveAll(t => Path.GetFileNameWithoutExtension(t).Equals(item.FullName));
            }
            if (extentList != null)
            {
                otherTypeFile.ForEach(t => extentList.Add(new FileCondition()
                {
                    FilePath = t,
                    FullName = Path.GetFileNameWithoutExtension(t),
                    ExtFileName = Path.GetFileName(t)
                }));
            }
            return namePairList;
        }

        /// <summary>
        /// 获取文件中的目录名称信息
        /// </summary>
        public static List<FileCondition> GetFolderNamePair(string victorFilePath)
        {
            if (string.IsNullOrEmpty(victorFilePath) || !Directory.Exists(victorFilePath))
            {
                return null;
            }
            string[] files = Directory.GetFiles(victorFilePath);
            if (files.Length > 0)
            {
                return CreateNamePair(files);
            }
            return null;
        }

        /// <summary>
        /// 获文件信息
        /// </summary>
        /// <param name="directory">文件目录数组</param>
        /// <param name="extentionName">扩展名称</param>
        public static List<FileCondition> CreatNamePairNoExtent(string[] directory, string extentionName, List<FileCondition> extList, string ext)
        {
            List<FileCondition> namePairList = new List<FileCondition>();
            if (directory.Length == 0)
            {
                return namePairList;
            }
            for (int i = 0; i < directory.Length; i++)
            {
                string path = directory[i];
                string fullName = Path.GetFileName(path);
                string fullNameWithExtent = string.Empty;
                string name = string.Empty;
                string fileExt = Path.GetExtension(path);
                fullNameWithExtent = Path.GetFileName(fullName);
                fullName = Path.GetFileNameWithoutExtension(fullNameWithExtent);
                if (!string.IsNullOrEmpty(extentionName))
                {
                    name = fullName.Replace(extentionName, "");
                }
                else
                {
                    Regex gex = new Regex("\\d+");
                    name = gex.Replace(fullName, "");
                }
                FileCondition fnp = new FileCondition()
                {
                    FilePath = path,
                    FullName = fullName,
                    ExtFileName = fullNameWithExtent,
                    Name = name
                };
                if (extList != null && fileExt.ToLower().Equals(ext))
                {
                    extList.Add(fnp);
                }
                namePairList.Add(fnp);
            }
            return namePairList;
        }

        ///// <summary>
        ///// 查找重复图层
        ///// </summary>
        //public static List<TypeDescrip> SearchRepeat(string[] directory)
        //{
        //    List<TypeDescrip> errorMessage = new List<TypeDescrip>();
        //    List<string> layerNameList = new List<string>();
        //    for (int i = 0; i < directory.Length; i++)
        //    {
        //        string path = directory[i];
        //        string fullName = Path.GetFileName(path);
        //        string extentName = Path.GetExtension(fullName);
        //        if (!extentName.ToLower().Equals(".shp"))
        //        {
        //            continue;
        //        }
        //        string name = string.Empty;
        //        fullName = Path.GetFileNameWithoutExtension(fullName);
        //        if (fullName.Length <= 10)
        //        {
        //            errorMessage.Add(new TypeDescrip() { Type = 0, Message = "矢量图层" + fullName + "名称不符合命名规范" });
        //            continue;
        //        }
        //        Regex gex = new Regex("\\d+");
        //        name = gex.Replace(fullName, "");
        //        if (layerNameList.Any(n => n.Equals(name)))
        //        {
        //            errorMessage.Add(new TypeDescrip() { Type = 0, Message = "矢量图层中存在" + name + "的图层" });
        //            continue;
        //        }
        //        layerNameList.Add(name);
        //    }
        //    return errorMessage;
        //}

        /// <summary>
        /// 获取Exception
        /// </summary>
        public static Exception GetEndOfStreamException(Exception ex)
        {
            Exception e = ex;
            while (e != null)
            {
                if (e is EndOfStreamException)
                {
                    return e;
                }
                e = e.InnerException;
            }
            return null;
        }

        #endregion
    }
}
