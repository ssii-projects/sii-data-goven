/*
 * (C) 2015  公司版权所有,保留所有权利 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Agro.LibCore;
using Agro.Library.Model;

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 承包地块
    /// </summary>
    [Serializable]
    public class CBDK_VIEW : NotifyCDObject
    {
        #region Fields

        /// <summary>
        /// 地块名称
        /// </summary>
        private string name;

        /// <summary>
        /// 发包方名称
        /// </summary>
        private string senderName;

        /// <summary>
        /// 承包方名称
        /// </summary>
        private string contracterName;

        /// <summary>
        /// 地块编码
        /// </summary>
        private string landNumber;

        /// <summary>
        /// 承包经营权取得方式
        /// </summary>
        private string qdfs;

        /// <summary>
        /// 土地利用类型名称
        /// </summary>
        private string landName;

        /// <summary>
        /// 所有权性质
        /// </summary>
        private string syqxz;

        /// <summary>
        /// 实测面积
        /// </summary>
        private double actualArea;

        /// <summary>
        /// 颁证面积
        /// </summary>
        private double awareArea;

        /// <summary>
        /// 地块备注信息
        /// </summary>
        private string comment;

        /// <summary>
        /// 原编码
        /// </summary>
        private string sourceNumber;

        /// <summary>
        /// 四至东
        /// </summary>
        private string neighborEast;

        /// <summary>
        /// 四至西
        /// </summary>
        private string neighborWest;

        /// <summary>
        /// 四至南
        /// </summary>
        private string neighborSouth;

        /// <summary>
        /// 四至北
        /// </summary>
        private string neighborNorth;

        /// <summary>
        /// 地块类别
        /// </summary>
        private string landcategory;

        /// <summary>
        /// 土地等级
        /// </summary>
        private string landLevel;

        /// <summary>
        /// 台账面积
        /// </summary>
        private double? tableArea;

        /// <summary>
        /// 是否基本农田
        /// </summary>
        private bool isFarmerLand;

        /// <summary>
        /// 土地用途
        /// </summary>
        private string purpose;

        /// <summary>
        /// 是否确权确股
        /// </summary>
        private string sfqqqg;

        #endregion

        #region Properties

        /// <summary>
        /// 地块编码
        /// </summary>
        public string DKBM
        {
            get { return landNumber; }
            set
            {
                landNumber = value;
                NotifyPropertyChanged("DKBM");
            }
        }

        /// <summary>
        /// 地块名称
        /// </summary>
        public string DKMC
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("DKMC");
            }
        }

        /// <summary>
        ///发包方名称
        /// </summary>
        public string FBFMC
        {
            get { return senderName; }
            set { senderName = value.TrimSafe(); NotifyPropertyChanged("FBFMC"); }
        }

        /// <summary>
        ///承包方姓名
        /// </summary>
        public string CBFMC
        {
            get { return contracterName; }
            set { contracterName = value.TrimSafe(); NotifyPropertyChanged("CBFMC"); }
        }

        /// <summary>
        ///承包经营权取得方式
        /// </summary>
        public string CBJYQQDFS
        {
            get { return qdfs; }
            set { qdfs = value.TrimSafe(); NotifyPropertyChanged("CBJYQQDFS"); }
        }

        /// <summary>
        ///所有权性质
        /// </summary>
        public string SYQXZ
        {
            get { return syqxz; }
            set { syqxz = value.TrimSafe(); NotifyPropertyChanged("SYQXZ"); }
        }

        /// <summary>
        ///地块类别
        /// </summary>
        public string DKLB
        {
            get { return landcategory; }
            set
            {
                landcategory = value.TrimSafe();
                NotifyPropertyChanged("DKLB");
            }
        }

        /// <summary>
        /// 土地利用类型
        /// </summary>
        public string TDLYLX
        {
            get { return landName; }
            set
            {
                landName = value;
                NotifyPropertyChanged("TDLYLX");
            }
        }

        /// <summary>
        ///地力等级
        /// </summary>
        public string DLDJ
        {
            get { return landLevel; }
            set
            {
                landLevel = value.TrimSafe();
                NotifyPropertyChanged("DLDJ");
            }
        }

        /// <summary>
        ///土地用途
        /// </summary>
        public string TDYT
        {
            get { return purpose; }
            set
            {
                purpose = value.TrimSafe();
                NotifyPropertyChanged("TDYT");
            }
        }

        /// <summary>
        ///是否基本农田
        /// </summary>
        public bool SFJBNT
        {
            get { return isFarmerLand; }
            set
            {
                isFarmerLand = value;
                NotifyPropertyChanged("SFJBNT");
            }
        }

        /// <summary>
        ///原合同面积(亩)
        /// </summary>
        public double? ELHTMJ
        {
            get { return tableArea; }
            set
            {
                tableArea = value;
                NotifyPropertyChanged("ELHTMJ");
            }
        }

        /// <summary>
        /// 实测面积(亩)
        /// </summary>
        public double SCMJ
        {
            get { return actualArea; }
            set
            {
                actualArea = value;
                NotifyPropertyChanged("SCMJ");
            }
        }

        /// <summary>
        /// 合同面积(亩)
        /// </summary>
        public double HTMJ
        {
            get { return awareArea; }
            set
            {
                awareArea = value;
                NotifyPropertyChanged("HTMJ");
            }
        }

        /// <summary>
        /// 四至东(右)
        /// </summary>
        public string DKDZ
        {
            get { return neighborEast; }
            set
            {
                neighborEast = value;
                NotifyPropertyChanged("DKDZ");
            }
        }

        /// <summary>
        /// 四至西(左)
        /// </summary>
        public string DKXZ
        {
            get { return neighborWest; }
            set
            {
                neighborWest = value;
                NotifyPropertyChanged("DKXZ");
            }
        }

        /// <summary>
        /// 四至南(下)
        /// </summary>
        public string DKNZ
        {
            get { return neighborSouth; }
            set
            {
                neighborSouth = value;
                NotifyPropertyChanged("DKNZ");
            }
        }

        /// <summary>
        /// 四至北(上)
        /// </summary>
        public string DKBZ
        {
            get { return neighborNorth; }
            set
            {
                neighborNorth = value;
                NotifyPropertyChanged("DKBZ");
            }
        }

        /// <summary>
        /// 是否确权确股
        /// </summary>
        public string SFQQQG
        {
            get { return sfqqqg; }
            set
            {
                sfqqqg = value.TrimSafe();
                NotifyPropertyChanged("SFQQQG");
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string DKBZXX
        {
            get { return comment; }
            set
            {
                comment = value;
                NotifyPropertyChanged("DKBZXX");
            }
        }

        #endregion

        #region Ctor

        public CBDK_VIEW()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// 转换成标准模型
        /// </summary>
        /// <returns></returns>
        public ATT_CBDKXX ToStandardModel()
        {
            object obj = this.ConvertTo(typeof(ATT_CBDKXX));
            ATT_CBDKXX dk = obj as ATT_CBDKXX;
            return dk;
        }

        /// <summary>
        /// 转换成界面模型
        /// </summary>
        /// <param name="fbf"></param>
        public static CBDK_VIEW ToViewModel(ATT_CBDKXX cbdk)
        {
            object obj = cbdk.ConvertTo(typeof(CBDK_VIEW));
            CBDK_VIEW dkView = obj as CBDK_VIEW;
            return dkView;
        }

        #endregion
    }
}
