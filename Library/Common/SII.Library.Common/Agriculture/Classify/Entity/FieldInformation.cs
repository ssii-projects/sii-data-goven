/*
 * (C)2016 公司版权所有,保留所有权利
*/
using System;
using System.Collections.Generic;
using NetTopologySuite.IO;

namespace Agriculture.Classify
{
    /// <summary>
    /// 数据字段信息类
    /// </summary>
    [Serializable]
    public class FieldInformation
    {
        #region Properties

        /// <summary>
        /// 中文名称
        /// </summary>
        public string AliseName { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string CharType { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int FieldLength { get; set; }

        /// <summary>
        /// 字段精度
        /// </summary>
        public int FieldPrecision { get; set; }

        /// <summary>
        /// 检查长度
        /// </summary>
        public bool CheckLength { get; set; }

        /// <summary>
        /// 是否必须
        /// </summary>
        public bool IsNecessary { get; set; }

        /// <summary>
        /// 字段是否存在
        /// </summary>
        public bool FieldExist { get; set; }

        /// <summary>
        /// 字段必填填写
        /// </summary>
        public bool Nullable { get; set; }

        #region Reality

        /// <summary>
        /// 字段类型
        /// </summary>
        public string RealFieldType { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int RealFieldLength { get; set; }

        /// <summary>
        /// 字段必填填写
        /// </summary>
        public bool RealNullable { get; set; }

        /// <summary>
        /// 字段精度
        /// </summary>
        public int RealPrecision { get; set; }

        #endregion

        public bool ReportError { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public FieldInformation()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="AliseName">别名</param>
        /// <param name="name">字段名</param>
        /// <param name="type">类型</param>
        /// <param name="length">长度</param>
        /// <param name="precision">精度</param>
        /// <param name="request">是否必需(true为不可为空,false为可空)</param>
        /// <param name="checkLength">检查长度</param>
        /// <param name="isNecessary">是否必须(O/C/M)</param>
        /// <returns></returns>
        public FieldInformation CreateEntity(string AliseName, string name, string type, int length,
            int precision = 0, bool request = true, bool checkLength = true, bool isNecessary = true, string charType = "C")
        {
            return new FieldInformation()
            {
                AliseName = AliseName,
                FieldName = name,
                FieldType = type,
                Nullable = request,
                FieldLength = length,
                FieldPrecision = precision,
                CheckLength = checkLength,
                FieldExist = false,
                IsNecessary = isNecessary,
                CharType = charType
            };
        }
        #endregion
    }
}
