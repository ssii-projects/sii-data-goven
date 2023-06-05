// (C) 2015 凯普顿公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Captain.NetCore;

using Captain.Library.Model;
using Captain.Library.Exchange;



namespace Captain.Library.Exchange
{
    /// <summary>
    /// 行政地域
    /// </summary>
    public static class XZQH_XZDY_CONVERT
    {
        /// <summary>
        /// 转换到界面实体
        /// </summary>
        /// <param name="xzdy">行政区域</param>
        /// <returns></returns>
        public static Zone ToView(this XZQH_XZDY xzdy)
        {
            if (xzdy == null)
            {
                return null;
            }
            Zone zone = new Zone();
            zone.ID = xzdy.ID;
            zone.Name = xzdy.MC;
            zone.FullCode = xzdy.BM;
            zone.Level = (eZoneLevel)xzdy.JB;
            int length = InitalizeZoneLevel(zone.Level);
            zone.Code = xzdy.BM.Substring(length);
            zone.Shape = xzdy.SHAPE as Geometry;
            zone.UpLevelCode = xzdy.BM.Substring(0, length);
            zone.Comment = xzdy.BZ;
            return zone;
        }

        /// <summary>
        /// 转换到界面实体集合
        /// </summary>
        /// <param name="xzdys">行政区域集合</param>
        /// <returns></returns>
        public static List<Zone> ToViewArray(this List<XZQH_XZDY> xzdys)
        {
            if (xzdys == null || xzdys.Count == 0)
            {
                return new List<Zone>();
            }
            List<Zone> zones = new List<Zone>();
            xzdys.ForEach(ze => zones.Add(ze.ToView()));
            return zones;
        }

        /// <summary>
        /// 转换到底层实体
        /// </summary>
        /// <param name="zone">行政区域</param>
        /// <returns></returns>
        public static XZQH_XZDY ToModel(this Zone zone)
        {
            if (zone == null)
            {
                return null;
            }
            XZQH_XZDY xzdy = null;
            IZoneWorkStation station = InitalizeZoneWorkStation();
            if (station == null)
            {
                return xzdy;
            }
            xzdy = station.Get(zone.FullCode);
            if (xzdy == null)
            {
                return xzdy;
            }
            zone.ToModelProperty(xzdy);
            return xzdy;
        }

        /// <summary>
        /// 转换到底层实体属性
        /// </summary>
        /// <param name="zone">行政区域</param>
        /// <returns></returns>
        public static void ToModelProperty(this Zone zone, XZQH_XZDY xzdy)
        {
            if (zone == null || xzdy == null)
            {
                return;
            }
            xzdy.MC = zone.Name;
            xzdy.BM = zone.FullCode;
            xzdy.JB = (int)zone.Level;
            xzdy.SHAPE = zone.Shape;
            xzdy.BZ = zone.Comment;
        }

        /// <summary>
        /// 转换到底层实体集合
        /// </summary>
        /// <param name="zones">行政区域集合</param>
        /// <returns></returns>
        public static List<XZQH_XZDY> ToModelArray(this List<Zone> zones)
        {
            if (zones == null || zones.Count == 0)
            {
                return new List<XZQH_XZDY>();
            }
            List<XZQH_XZDY> xzdys = new List<XZQH_XZDY>();
            IZoneWorkStation station = InitalizeZoneWorkStation();
            if (station == null)
            {
                return xzdys;
            }
            foreach(var zone in zones)
            {
                var xzdy = station.Get(zone.FullCode);
                if (xzdy == null)
                {
                    continue;
                }
                zone.ToModelProperty(xzdy);
                xzdys.Add(xzdy);
            }
            return xzdys;
        }

        /// <summary>
        /// 初始化行政区域级别长度
        /// </summary>
        /// <param name="level">行政区域级别</param>
        /// <returns></returns>
        public static int InitalizeZoneLevel(eZoneLevel level)
        {
            int length = 0;
            switch(level)
            {
                case eZoneLevel.State:
                    length = 0;
                    break;
                case eZoneLevel.Province:
                    length = Zone.ZONE_PROVICE_LENGTH;
                    break;
                case eZoneLevel.City:
                    length = Zone.ZONE_CITY_LENGTH;
                    break;
                case eZoneLevel.County:
                    length = Zone.ZONE_COUNTY_LENGTH;
                    break;
                case eZoneLevel.Town:
                    length = Zone.ZONE_TOWN_LENGTH;
                    break;
                case eZoneLevel.Village:
                    length = Zone.ZONE_VILLAGE_LENGTH;
                    break;
                case eZoneLevel.Group:
                    length = Zone.ZONE_GROUP_LENGTH;
                    break;
                default:
                    break;
            }
            return length;
        }

        /// <summary>
        /// 初始化地域访问对象
        /// </summary>
        /// <returns></returns>
        public static IZoneWorkStation InitalizeZoneWorkStation()
        {
            var db = DatabaseInstance.GetDataBaseSource();
            if (db == null)
            {
                return null;
            }
            ContainerFactory factroy = new ContainerFactory(db);
            IZoneWorkStation station = factroy.CreateWorkstation<IZoneWorkStation, IZoneRepository>();
            return station;
        }
    }
}
