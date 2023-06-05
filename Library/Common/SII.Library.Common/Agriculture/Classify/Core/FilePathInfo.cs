/*
 * (C) 2016 -2015 公司版权所有,保留所有权利
*/
using System;
using System.Collections.Generic;
using NetTopologySuite.IO;

namespace Agriculture.Classify
{
    /// <summary>
    /// 文件路径
    /// </summary>
    public class FilePathInfo
    {
        #region Property

        /// <summary>
        /// 矢量数据
        /// </summary>
        public string VictorFilePath { get; set; }

        /// <summary>
        /// 权属数据
        /// </summary>
        public string ThroneFilePath { get; set; }

        /// <summary>
        /// 栅格数据
        /// </summary>
        public string RuleFilePath { get; set; }

        /// <summary>
        /// 扫描资料
        /// </summary>
        public string ScanFilePath { get; set; }

        /// <summary>
        /// 数字正射影像图
        /// </summary>
        public string RuleJustPath { get; set; }

        /// <summary>
        /// 数据栅格地图
        /// </summary>
        public string RuleDigitalPath { get; set; }

        /// <summary>
        /// 其他栅格数据
        /// </summary>
        public string RuleOtherPath { get; set; }

        /// <summary>
        /// 权属来源证明资料附件
        /// </summary>
        public string FJFilePath { get; set; }

        /// <summary>
        /// 控制点之记
        /// </summary>
        public string ControlPointPath { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 汇总表格
        /// </summary>
        public string SumTablePath { get; set; }

        /// <summary>
        /// 汇总下Excel表格
        /// </summary>
        public string ExcelTablePath { get; set; }

        /// <summary>
        /// 文字报告 
        /// </summary>
        public string WordReportPath { get; set; }

        /// <summary>
        /// 其他资料
        /// </summary>
        public string OtherFilePath { get; set; }

        /// <summary>
        /// 数据库全路径
        /// </summary>
        public string DataBasePath { get; set; }

        /// <summary>
        /// 根目录
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// 选择的成果目录
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 矢量图层
        /// </summary>
        public List<FileCondition> ShapeFileList { get; set; }

        /// <summary>
        /// 地域编码
        /// </summary>
        public string ZoneCode { get; set; }

        /// <summary>
        /// 年份编码
        /// </summary>
        public string YearCode { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        #endregion
    }
}
