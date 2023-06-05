// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Agro.LibCore;
using Agro.Library.Model;
using Agro.LibCore;
using NetTopologySuite.Geometries;
//

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 行政地域
    /// </summary>
    [Serializable]
    public class Zone : NotifyCDObject
    {
        #region Const

        /// <summary>
        /// 国家级代码86
        /// </summary>
        public const string ZONE_STATE_CODE = "86";

        /// <summary>
        /// 国家级地域长度2
        /// </summary>
        public const int ZONE_STATE_LENGTH = 2;

        /// <summary>
        /// 省级地域长度3
        /// </summary>
        public const int ZONE_PROVICE_LENGTH = 2;

        /// <summary>
        /// 市级地域长度4
        /// </summary>
        public const int ZONE_CITY_LENGTH = 4;

        /// <summary>
        /// 区县级地域长度6
        /// </summary>
        public const int ZONE_COUNTY_LENGTH = 6;

        /// <summary>
        /// 乡镇级地域长度9
        /// </summary>
        public const int ZONE_TOWN_LENGTH = 9;

        /// <summary>
        /// 村级地域长度12
        /// </summary>
        public const int ZONE_VILLAGE_LENGTH = 12;

        /// <summary>
        /// 组级地域长度14
        /// </summary>
        public const int ZONE_GROUP_LENGTH = 14;

        #endregion

        #region Fields

        private string name;
        private string code;
        private string fullCode;
        private string upLevelCode;
        private eZoneLevel level;
        private Geometry shape;
        private string comment;

        #endregion

        #region Properties
        public string ID { get; set; }
        /// <summary>
        /// 地域名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value.TrimSafe();
                NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// 地域编码
        /// </summary>
        public string Code
        {
            get { return code; }
            set
            {
                code = value.TrimSafe();
                NotifyPropertyChanged("Code");
            }
        }

        /// <summary>
        /// 地域全编码
        /// </summary>
        public string FullCode
        {
            get { return fullCode; }
            set
            {
                fullCode = value.TrimSafe();
                NotifyPropertyChanged("FullCode");
            }
        }

        /// <summary>
        /// 上级地域代码
        /// </summary>
        public string UpLevelCode
        {
            get { return upLevelCode; }
            set
            {
                upLevelCode = value.TrimSafe();
                NotifyPropertyChanged("UpLevelCode");
            }
        }

        /// <summary>
        /// 地域级别
        /// </summary>
        public eZoneLevel Level
        {
            get { return level; }
            set
            {
                level = value;
                NotifyPropertyChanged("Level");
            }
        }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value.TrimSafe();
                NotifyPropertyChanged("Comment");
            }
        }

        /// <summary>
        /// 空间字段
        /// </summary>
        public Geometry Shape
        {
            get { return shape; }
            set
            {
                shape = value;
                NotifyPropertyChanged("Geometry");
            }
        }

        #endregion

        #region Ctor

        public Zone()
        {
            Level = eZoneLevel.Group;
        }

        #endregion

    }
}
