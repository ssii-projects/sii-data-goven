/*
 * (C)2016 公司版权所有,保留所有权利
*/
using System;
using System.Collections.Generic;

namespace Agriculture.Classify
{
    /// <summary>
    /// 文件信息
    /// </summary>
    public class FileCondition : ICloneable
    {
        #region Property

        /// <summary>
        /// 简称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string AliesName { get; set; }

        /// <summary>
        /// 是否必须(确定数据量)
        /// </summary>
        public bool IsNecessary { get; set; }

        /// <summary>
        /// 全称(带扩展名)
        /// </summary>
        public string ExtFileName { get; set; }

        /// <summary>
        /// 全称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件是否完整
        /// </summary>
        public bool IsFileComplate { get; set; }

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool IsExist { get; set; }

        /// <summary>
        /// 缺失其他信息
        /// </summary>
        public List<string> LackInfo { get; set; }

        /// <summary>
        /// 是否可以访问
        /// </summary>
        public bool CanRead { get; set; }

        /// <summary>
        /// (矢量文件)是否包含ZM值
        /// </summary>
        public bool HasZM { get; set; }

        /// <summary>
        /// 数据量
        /// </summary>
        public int DataCount { get; set; }

        /// <summary>
        /// 字段列表
        /// </summary>
        public List<FieldInformation> FieldList { get; set; }

        /// <summary>
        /// 多余字段
        /// </summary>
        public List<FieldInformation> ExtentList { get; set; }

        #endregion

        #region Methods

        public FileCondition()
        {
            IsExist = false;
            IsFileComplate = true;
            FieldList = new List<FieldInformation>();
            ExtentList = new List<FieldInformation>();
            LackInfo = new List<string>();
        }

        public override string ToString()
        {
            string str = "";
            var prolist = typeof(FileCondition).GetProperties();
            for (int i = 0; i < prolist.Length; i++)
            {
                str += prolist[i].Name + ":" + prolist[i].GetValue(this, null) + " ";
            }
            return str;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
