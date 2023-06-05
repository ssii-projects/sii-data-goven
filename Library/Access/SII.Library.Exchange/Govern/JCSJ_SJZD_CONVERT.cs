// (C) 2015 凯普顿公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Captain.NetCore;
//
using Captain.Library.Model;
//
//

namespace Captain.Library.Exchange
{
    /// <summary>
    /// 数据字典
    /// </summary>
    [Serializable]
    public static class JCSJ_SJZD_CONVERT
    {
        /// <summary>
        /// 转换到界面实体
        /// </summary>
        /// <param name="sjzd">行政区域</param>
        /// <returns></returns>
        public static DataDictionary ToView(this SJZD sjzd)
        {
            if (sjzd == null)
            {
                return null;
            }
            DataDictionary dic = new DataDictionary();
            dic.Name = sjzd.MC;
            dic.Code = sjzd.BM;
            dic.AliseName = sjzd.ZDYMC;
            dic.GroupName = sjzd.FZMC;
            dic.GroupCode = sjzd.FZM;
            dic.Comment = sjzd.BZ;
            return dic;
        }

        /// <summary>
        /// 转换到界面实体集合
        /// </summary>
        /// <param name="sjzds">行政区域集合</param>
        /// <returns></returns>
        public static List<DataDictionary> ToViewArray(this List<SJZD> sjzds)
        {
            if (sjzds == null || sjzds.Count == 0)
            {
                return new List<DataDictionary>();
            }
            List<DataDictionary> dics = new List<DataDictionary>();
            sjzds.ForEach(ze => dics.Add(ze.ToView()));
            return dics;
        }

        /// <summary>
        /// 转换到底层实体
        /// </summary>
        /// <param name="zone">行政区域</param>
        /// <returns></returns>
        public static SJZD ToModel(this DataDictionary dic)
        {
            if (dic == null)
            {
                return null;
            }
            SJZD sjzd = null;
            IDictionaryWorkStation station = InitalizeDictionaryWorkStation();
            if (station == null)
            {
                return sjzd;
            }
            sjzd = station.Get(dic.GroupCode, dic.Code);
            if (sjzd == null)
            {
                return sjzd;
            }
            dic.ToModelProperty(sjzd);
            return sjzd;
        }

        /// <summary>
        /// 转换到底层实体属性
        /// </summary>
        /// <param name="zone">行政区域</param>
        /// <returns></returns>
        public static void ToModelProperty(this DataDictionary dic, SJZD sjzd)
        {
            if (dic == null || sjzd == null)
            {
                return;
            }
            sjzd.MC = dic.Name;
            sjzd.BM = dic.Code;
            sjzd.ZDYMC = dic.AliseName;
            sjzd.FZMC = dic.GroupName;
            sjzd.FZM = dic.GroupCode;
            sjzd.BZ = dic.Comment;
        }

        /// <summary>
        /// 转换到底层实体集合
        /// </summary>
        /// <param name="dics">行政区域集合</param>
        /// <returns></returns>
        public static List<SJZD> ToModelArray(this List<DataDictionary> dics)
        {
            if (dics == null || dics.Count == 0)
            {
                return new List<SJZD>();
            }
            List<SJZD> sjzds = new List<SJZD>();
            IDictionaryWorkStation station = InitalizeDictionaryWorkStation();
            if (station == null)
            {
                return sjzds;
            }
            foreach (var dic in dics)
            {
                var xzdy = station.Get(dic.GroupCode, dic.Code);
                if (xzdy == null)
                {
                    continue;
                }
                dic.ToModelProperty(xzdy);
                sjzds.Add(xzdy);
            }
            return sjzds;
        }

        /// <summary>
        /// 初始化地域访问对象
        /// </summary>
        /// <returns></returns>
        public static IDictionaryWorkStation InitalizeDictionaryWorkStation()
        {
            var db = DatabaseInstance.GetDataBaseSource();
            if (db == null)
            {
                return null;
            }
            ContainerFactory factroy = new ContainerFactory(db);
            IDictionaryWorkStation station = factroy.CreateWorkstation<IDictionaryWorkStation, IDictionaryRepository>();
            return station;
        }
    }
}
