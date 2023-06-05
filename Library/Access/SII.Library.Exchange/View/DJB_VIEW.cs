// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Agro.LibCore;

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 土地承包经营权登记薄
    /// </summary>
    [Serializable]
    public class DJB_VIEW : NotifyCDObject
    {
        #region Fields

        private string senderName;
        private string contracterName;//承包方名称
        private string cbfs;//承包方式
        private DateTime? startTime;
        private DateTime? endTime;
        private string cbqx;//承包期限
        private string dksyt;//地块示意图
        private string sourceNumber;//原承包经营权证编号
        private string serialNumber;
        private string regeditNumber;
        private string contractRegeditBookPerson;
        private DateTime? contractRegeditBookTime;
        private string contractRegeditBookExcursus;

        #endregion

        #region Properties

        /// <summary>
        ///承包经营权证编码
        /// </summary>
        public string CBJYQZBM
        {
            get { return regeditNumber; }
            set
            {
                regeditNumber = value.TrimSafe();
                NotifyPropertyChanged("CBJYQZBM");
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
        ///承包方式
        /// </summary>
        public string CBFS
        {
            get { return cbfs; }
            set { cbfs = value.TrimSafe(); NotifyPropertyChanged("CBFS"); }
        }

        /// <summary>
        ///承包起始时间
        /// </summary>
        public DateTime? CBQXQ
        {
            get { return startTime; }
            set { startTime = value; NotifyPropertyChanged("CBQXQ"); }
        }

        /// <summary>
        ///承包结束时间
        /// </summary>
        public DateTime? CBQXZ
        {
            get { return endTime; }
            set { endTime = value; NotifyPropertyChanged("CBQXZ"); }
        }

        /// <summary>
        ///承包期限
        /// </summary>
        public string CBQX
        {
            get { return cbqx; }
            set { cbqx = value.TrimSafe(); NotifyPropertyChanged("CBQX"); }
        }

        /// <summary>
        ///地块示意图
        /// </summary>
        public string DKSYT
        {
            get { return dksyt; }
            set { dksyt = value.TrimSafe(); NotifyPropertyChanged("DKSYT"); }
        }

        /// <summary>
        /// 原承包经营权证编码
        /// </summary>
        public string YCBJYQZBH
        {
            get { return sourceNumber; }
            set
            {
                sourceNumber = value.TrimSafe();
                NotifyPropertyChanged("YCBJYQZBH");
            }
        }

        /// <summary>
        /// 承包经营权证流水号
        /// </summary>
        public string CBJYQZLSH
        {
            get { return serialNumber; }
            set
            {
                serialNumber = value.TrimSafe();
                NotifyPropertyChanged("CBJYQZLSH");
            }
        }

        /// <summary>
        /// 登簿人
        /// </summary>
        public string DBR
        {
            get { return contractRegeditBookPerson; }
            set
            {
                contractRegeditBookPerson = value.TrimSafe();
                NotifyPropertyChanged("DBR");
            }
        }

        /// <summary>
        ///登记时间
        /// </summary>
        public DateTime? QZSJ
        {
            get { return contractRegeditBookTime; }
            set
            {
                contractRegeditBookTime = value;
                NotifyPropertyChanged("QZSJ");
            }
        }
        /// <summary>
        /// 登记簿附记
        /// </summary>
        public string DJBFJ
        {
            get { return contractRegeditBookExcursus; }
            set
            {
                contractRegeditBookExcursus = value.TrimSafe();
                NotifyPropertyChanged("DJBFJ");
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造函数
        /// </summary>
        public DJB_VIEW()
        {

        }

        #endregion
    }
}