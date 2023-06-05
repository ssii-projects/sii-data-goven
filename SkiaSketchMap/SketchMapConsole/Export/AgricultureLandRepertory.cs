using Agro.LibCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SketchMap
{
    /// <summary>
    /// 交换数据
    /// </summary>
    public class AgricultureLandRepertory
    {
        /// <summary>
        /// 表索引
        /// </summary>
        public int TableIndex { get; set; }

        /// <summary>
        /// 行索引
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 列索引
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        /// 地块编码
        /// </summary>
        public string LandNumber { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 图像文件
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 地图路径
        /// </summary>
        public string EaglePath { get; set; }

        /// <summary>
        /// 承包方
        /// </summary>
        public ContractConcord Contractor { get; set; }

        /// <summary>
        /// 承包地块
        /// </summary>
        public VEC_CBDK AgriLand { get; set; }

        /// <summary>
        /// 其它地块集合
        /// </summary>
        public List<VEC_CBDK> LandCollection { get; set; }

        /// <summary>
        /// 示意图属性
        /// </summary>
        public SkecthMapProperty MapProperty { get; set; }


        #region Ctor

        public AgricultureLandRepertory()
        {

        }

        #endregion

        //#region Static

        ///// <summary>
        ///// 系列化数据
        ///// </summary>
        ///// <param name="version"></param>
        //public static void SerializeXml(List<AgricultureLandRepertory> lands, string fileName)
        //{
        //    if (lands == null || string.IsNullOrEmpty(fileName))
        //    {
        //        return;
        //    }
        //    fileName += ".xml";
        //    Serialization.SerializeXml(fileName, lands);
        //}

        ///// <summary>
        ///// 反系列化图层数据
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public static List<AgricultureLandRepertory> DeserializeXml(string fileName)
        //{
        //    if (!System.IO.File.Exists(fileName))
        //    {
        //        return new List<AgricultureLandRepertory>();
        //    }
        //    try
        //    {
        //        List<AgricultureLandRepertory> lands = Serialization.DeserializeXml(fileName, typeof(List<AgricultureLandRepertory>)) as List<AgricultureLandRepertory>;
        //        return lands;
        //    }
        //    catch (SystemException ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //    }
        //    return new List<AgricultureLandRepertory>();
        //}

        //#endregion
    }
}
