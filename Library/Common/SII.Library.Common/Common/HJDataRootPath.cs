using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Library.Common
{
    /// <summary>
    /// 汇交数据根目录
    /// </summary>
    public class HJDataRootPath
    {
        /// <summary>
        /// 地块shp文件名（如：矢量数据\DK2205812016.shp）
        /// </summary>
        //public string dkShpFileName;
        //public string jzdShpFileName;
        //public string jzxShpFileName;
        //public string jbntbhqShpFileName;
        //public string kzdShpFileName;
        //public string qyjxShpFileName;

        public string RootPath;

        /// <summary>
        /// 【前缀，shapeFile全路径】
        /// 如：[DK,..\DK2205812016.shp]
        /// </summary>
        public readonly Dictionary<string, string> dicShp = new Dictionary<string, string>();
        /// <summary>
        /// 针对界址点，界址线，地块有可能被分割为多个shape文件的
        /// </summary>
        public readonly Dictionary<string, List<string>> dicShp1 = new Dictionary<string, List<string>>();
        /// <summary>
        /// Mdb文件名（如：权属数据\2205812016.mdb）
        /// </summary>
        public string mdbFileName;
        /// <summary>
        /// 权属单位代码表文件名（如：权属数据\2205812016权属单位代码表.xls）
        /// </summary>
        public string qsXlsFileName;
        /// <summary>
        /// 6位县行政区划代码
        /// </summary>
        public string sXxzqhdm;
        /// <summary>
        /// 行政区名称
        /// </summary>
        public string sXzqmc;
        /// <summary>
        /// 数据库连接串
        /// </summary>
       // public string sDBConnectionString { get; set; }

        /// <summary>
        /// 汇总表格路径
        /// </summary>
        public string SumTablePath;

        public Stopwatch StartTime;

		/// <summary>
		/// 年度
		/// </summary>
		public int Year;

        public string ErrorInfo
        {
            get;private set;
        }
        public HJDataRootPath()
        {
            this.ErrorInfo = "未设置根目录";
        }
        public string Init(string sRootPath)//, string ConnectionString)
        {
            ErrorInfo = DoInit(sRootPath);
            return ErrorInfo;
        }
        /// <summary>
        /// 通过传入的质检数据根目录遍历所需要的文件路径，路径正确则返回null
        /// 否则返回错误信息
        /// </summary>
        /// <param name="sRootPath"></param>
        /// <returns></returns>
        private string DoInit(string sRootPath)//, string ConnectionString)
        {
            this.RootPath = null;
            StartTime = Stopwatch.StartNew();
            //this.sDBConnectionString = ConnectionString;
            if (!(sRootPath.EndsWith("/") || sRootPath.EndsWith("\\")))
            {
                sRootPath += "\\";
            }
            for (int i = sRootPath.Length - 2; i >= 0; --i)
            {
                var ch = sRootPath[i];
                if (ch == '/' || ch == '\\')
                {
                    bool fOK = false;
                    var str = sRootPath.Substring(i + 1);
                    if (str.Length > 6)
                    {
                        sXxzqhdm = str.Substring(0, 6);
                        if (int.TryParse(sXxzqhdm, out int n))
                        {
                            if (n.ToString() == sXxzqhdm)
                            {
                                this.sXzqmc = str.Substring(6, str.Length - 7);

                                fOK = true;
                            }
                        }
                    }
                    if (!fOK)
                    {
                        sXxzqhdm = null;
                    }
                    break;
                }
            }
            if (sXxzqhdm == null)
            {
                return "汇交资料文件根目录必须以6位县级区划代码开头！";
            }
            RootPath = sRootPath;
            var err = checkPath(sRootPath, "矢量数据");
            if (err != null)
            {
                return err;
            }
            err = checkPath(sRootPath, "权属数据");
            if (err != null)
            {
                return err;
            }

            err = checkPath(sRootPath, "汇总表格");
            if (err != null)
            {
                return err;
            }
            this.SumTablePath = sRootPath + "汇总表格";


            var sa1 = new string[] { "DK", "JZD", "JZX", };
            dicShp1.Clear();
            foreach (var s in sa1)
            {
                dicShp1[s] = new List<string>();
            }

            var sa = new string[] { "JBNTBHQ", "KZD", "QYJX", "XJXZQ", "XJQY", "CJQY", "ZJQY", "MZDW", "XZDW", "DZDW","ZJ" };
            //var dicShp = new Dictionary<string, bool>();
            dicShp.Clear();
            foreach (var s in sa)
            {
                dicShp[s] = null;
            }

            //var dic = new Dictionary<string, string>();
            FileUtil.EnumFiles(sRootPath + "矢量数据", fi =>
            {
                foreach (var kv in sa)
                {
                    var fileName = fi.Name.ToUpper();
                    if (fileName.StartsWith(kv) && fileName.EndsWith(".SHP"))
                    {
                        var ch= fileName.Substring(kv.Length)[0];
                        if (ch >= '0' && ch <= '9')
                        {
                            dicShp[kv] = fi.FullName;
                            //dic[kv.Key] = fi.FullName;
                            break;
                        }
                    }
                }
                foreach (var k in sa1)
                {
                    var fileName = fi.Name.ToUpper();
                    if (fileName.StartsWith(k) && fileName.EndsWith(".SHP"))
                    {
                        dicShp1[k].Add(fi.FullName);
						if (Year == 0)
						{
							var str = fileName.Substring(k.Length+6, 4);
							//var str=fileName.Substring(fileName.Length - 8, 4);
							Year = SafeConvertAux.ToInt32(str);
						}
                        break;
                    }
                }
                return true;
            });
            //foreach (var kv in dic)
            //{
            //    dicShp[kv.Key] = kv.Value;
            //}
            //foreach (var kv in dicShp)
            //{
            //    if (kv.Value == null)
            //    {
            //        return "在" + sRootPath + "矢量数据目录下未找到以" + kv.Key + " 开头的shp文件！";
            //    }
            //}
            foreach (var kv in dicShp1)
            {
                if (kv.Value.Count == 0)
                {
					if (!(kv.Key == "ZJ"))
					{
						return "在" + sRootPath + "矢量数据目录下未找到以" + kv.Key + " 开头的shp文件！";
					}
                }
            }


            FileUtil.EnumFiles(sRootPath + "权属数据", fi =>
            {
                var fileName = fi.Name.ToLower();
                if (fileName.EndsWith(".mdb"))
                {
                    this.mdbFileName = fi.FullName;
                    return false;
                }
                return true;
            });
            if (!File.Exists(this.mdbFileName))
            {
                return "在" + sRootPath + "权属数据目录下未找到权属数据库(.mdb文件）！";
            }

            FileUtil.EnumFiles(sRootPath + "权属数据", fi =>
            {
                if (fi.Name.EndsWith("权属单位代码表.xls"))
                {
                    this.qsXlsFileName = fi.FullName;
                    return false;
                }
                return true;
            });
            if (!File.Exists(this.qsXlsFileName))
            {
                return "在" + sRootPath + "权属数据目录下未找到权属单位代码表.xls！";
            }

            LogoutUtil.SetLogoutFile(RootPath + "数据导入日志.txt");
            LogoutUtil.WriteLog("", false);
            LogoutUtil.WriteLog("开始时间：" + DateTime.Now);
            return null;
        }
        private string checkPath(string sRootPath, string sPath)
        {
            if (!Directory.Exists(sRootPath + sPath))
            {
                return "在" + sRootPath + "目录下未找到" + sPath + "目录！";
            }
            return null;
        }
    }
}
