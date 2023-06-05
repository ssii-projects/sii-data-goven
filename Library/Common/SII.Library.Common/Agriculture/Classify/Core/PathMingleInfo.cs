/*
 * (C)2016 公司版权所有,保留所有权利
*/
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Agriculture.Classify
{
    /// <summary>
    /// 文件路径基本信息
    /// </summary>
    public class PathMingleInfo
    {
        #region Fields

        #endregion

        #region Property

        /// <summary>
        /// 权属数据
        /// </summary>
        public BaseFileMingle ThroneFilePath { get; set; }

        /// <summary>
        /// 矢量数据
        /// </summary>
        public FileMingle VictorFilePath { get; set; }

        /// <summary>
        /// 栅格数据
        /// </summary>
        public RuleFileMingle RuleFilePath { get; set; }

        /// <summary>
        /// 扫描资料
        /// </summary>
        public ScanFileMingle ScanFilePath { get; set; }

        /// <summary>
        /// 图件
        /// </summary>
        public FileMingle ImagePath { get; set; }

        /// <summary>
        /// 文字报告 
        /// </summary>
        public FileMingle WordReportPath { get; set; }

        /// <summary>
        /// 其他资料
        /// </summary>
        public FileMingle OtherFilePath { get; set; }

        /// <summary>
        /// 汇总表格
        /// </summary>
        public ExcelFileMingle SumTablePath { get; set; }

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// 成果目录名称
        /// </summary>
        public string PathName { get; set; }

        /// <summary>
        /// 选择的成果目录
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 矢量文件信息
        /// </summary>
        public ShapeFileCondition ShapeFile { get; set; }

        /// <summary>
        /// 数据库文件
        /// </summary>
        public DataBaseCondition DatabaseFile { get; set; }

        /// <summary>
        /// 原始有效的矢量文件集合
        /// </summary>
        public List<FileCondition> ShapeList
        {
            get
            {
                if (ShapeFile == null)
                    return new List<FileCondition>();
                else
                    return ShapeFile.FileConditionList();
            }
        }

        #endregion

        #region Ctor

        public PathMingleInfo()
        {
            ShapeFile = new ShapeFileCondition();
            DatabaseFile = new DataBaseCondition();
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            string str = "";
            var prolist = typeof(PathMingleInfo).GetProperties();
            for (int i = 0; i < prolist.Length; i++)
            {
                str += prolist[i].Name + ":" + prolist[i].GetValue(this, null) + " ";
            }
            return str;
        }

        #endregion
    }

    /// <summary>
    /// 文件夹基类
    /// </summary>
    public class FileMingle
    {
        #region Fileds

        /// <summary>
        /// 区域代码
        /// </summary>
        public string zoneCode;

        /// <summary>
        /// 数据年份
        /// </summary>
        public string yearCode;

        /// <summary>
        /// 行政区域代码
        /// </summary>
        public string uintName;

        #endregion

        #region Properties

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exist { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; set; }

        #endregion

        #region Ctor

        public FileMingle()
        {
        }

        public FileMingle(string zoneCode, string yearCode, string uintName)
        {
            this.zoneCode = zoneCode;
            this.yearCode = yearCode;
            this.uintName = uintName;
        }

        #endregion
    }

    /// <summary>
    /// 矢量文件目录
    /// </summary>
    public class ShapeFileMingle : FileMingle
    {
        #region Ctor

        public ShapeFileMingle()
        { }

        public ShapeFileMingle(string zoneCode, string yearCode, string unitName) :
            base(zoneCode, yearCode, unitName)
        {
            ShapeFile = new ShapeFileCondition();
        }

        #endregion

        #region Property

        /// <summary>
        /// 矢量文件信息
        /// </summary>
        public ShapeFileCondition ShapeFile { get; set; }

        #endregion
    }

    /// <summary>
    /// 栅格文件目录
    /// </summary>
    public class RuleFileMingle : FileMingle
    {
        #region Ctor
        public RuleFileMingle()
        { }

        public RuleFileMingle(string zoneCode, string yearCode, string unitName) :
            base(zoneCode, yearCode, unitName)
        {
        }

        #endregion

        #region Property

        /// <summary>
        /// 数字正射影像
        /// </summary>
        public FileMingle JustFile { get; set; }

        /// <summary>
        /// 数字栅格地图
        /// </summary>
        public FileMingle DigitalFile { get; set; }

        /// <summary>
        /// 其他栅格
        /// </summary>
        public FileMingle OtherFile { get; set; }

        #endregion
    }

    /// <summary>
    /// 扫描资料文件目录
    /// </summary>
    public class ScanFileMingle : FileMingle
    {
        #region Ctor
        public ScanFileMingle()
        { }
        public ScanFileMingle(string zoneCode, string yearCode, string unitName) :
            base(zoneCode, yearCode, unitName)
        {
        }

        #endregion

        #region Property

        /// <summary>
        /// 权属来源资料
        /// </summary>
        public FileMingle FJFile { get; set; }

        /// <summary>
        /// 控制点点之记
        /// </summary>
        public FileMingle DZJFile { get; set; }

        #endregion
    }

    /// <summary>
    /// 汇总Excel文件目录
    /// </summary>
    public class ExcelFileMingle : FileMingle
    {
        #region Ctor
        public ExcelFileMingle()
        { }
        public ExcelFileMingle(string zoneCode, string yearCode, string unitName) :
            base(zoneCode, yearCode, unitName)
        {
        }

        #endregion

        #region Property

        /// <summary>
        /// Excel文件目录
        /// </summary>
        //public FileMingle ExcelFoleder { get; set; }

        /// <summary>
        /// 按地块汇总表
        /// </summary>
        public FileCondition LandFile { get; set; }

        /// <summary>
        ///  按承包地土地用途汇总表
        /// </summary>
        public FileCondition LandUseFile { get; set; }

        ///// <summary>
        ///// 按承包地所有权性质汇总表
        ///// </summary>
        //public FileCondition LandOwnFile { get; set; }

        /// <summary>
        ///  按非承包地地块类别汇总表
        /// </summary>
        public FileCondition LandTypeFile { get; set; }

        /// <summary>
        /// 按承包地是否基本农田汇总表
        /// </summary>
        public FileCondition LandBaseFile { get; set; }

        /// <summary>
        ///  按权证信息汇总表
        /// </summary>
        public FileCondition RegisterFile { get; set; }

        /// <summary>
        ///  按承包方汇总表
        /// </summary>
        public FileCondition ContractorFile { get; set; }

        #endregion
    }

    /// <summary>
    /// 权属文件目录
    /// </summary>
    public class BaseFileMingle : FileMingle
    {
        #region Ctor

        public BaseFileMingle()
        {
        }

        public BaseFileMingle(string zoneCode, string yearCode, string unitName) :
            base(zoneCode, yearCode, unitName)
        {
        }

        #endregion

        #region Property

        /// <summary>
        /// 数据库文件
        /// </summary>
        public FileCondition TableFile { get; set; }

        /// <summary>
        ///  权属单位代码表
        /// </summary>
        public FileCondition ExcelFile { get; set; }

        #endregion
    }
}
