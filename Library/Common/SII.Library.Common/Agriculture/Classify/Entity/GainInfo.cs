/*
 * (C)2016 公司版权所有,保留所有权利
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Agriculture.Classify
{
    /// <summary>
    /// 成果信息
    /// </summary>
    public class GainInfo
    {
        #region Property

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 区域代码
        /// </summary>
        public string ZoneCode { get; set; }

        /// <summary>
        /// 数据年份
        /// </summary>
        public string Year { get; set; }

        public bool IsValide { get { return !string.IsNullOrEmpty(UnitName) && !string.IsNullOrEmpty(ZoneCode) && !string.IsNullOrEmpty(Year); } }

        #endregion

        #region Ctor

        public GainInfo(string filePath)
        {
            string folderName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
            if (folderName.Length < 6)
            {
                return;
            }
            try
            {
                ZoneCode = folderName.Substring(0, 6);
                UnitName = folderName.Substring(6);
                FilePathManager fpm = new FilePathManager();
                string categoryPath = Path.Combine(filePath, fpm.CategoryName);
                if (!Directory.Exists(categoryPath))
                {
                    return;
                }
                string[] files = Directory.GetFiles(categoryPath);
                if (files == null || files.Length == 0)
                {
                    return;
                }
                for (int i = 0; i < files.Length; i++)
                {
                    string path = files[i];
                    string name = Path.GetFileNameWithoutExtension(path);
                    string ext = Path.GetExtension(path);
                    if (ext.ToLower() != ".mdb")
                        continue;
                    if (name.StartsWith(ZoneCode) && name.Length == 10)
                    {
                        Year = name.Replace(ZoneCode, "");
                        break;
                    }
                }
            }
            catch
            {
                ZoneCode = "";
                Year = "";
                UnitName = "";
            }
        }

        #endregion
    }
}
