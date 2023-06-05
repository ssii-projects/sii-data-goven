// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Agro.LibCore;
using Agro.Library.Model;

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 承包合同
    /// </summary>
    [Serializable]
    public class CBHT_VIEW : NotifyCDObject
    {
        #region Fields

        private string concordNumber;
        private string sourceNubmer;//原承包合同编码
        private string senderName;
        private string arableLandType;
        private DateTime? arableLandStartTime;
        private DateTime? arableLandEndTime;
        private double htzmj;
        private double yhtzmj;
        private DateTime? qdsj;//签订时间
        private int cbdks;//承包地块数
        private string contracterName;//承包方名称

        #endregion

        #region Properties

        /// <summary>
        ///承包合同编号
        /// </summary>
        public string CBHTBM
        {
            get { return concordNumber; }
            set
            {
                concordNumber = value;
                NotifyPropertyChanged("CBHTBM");
                if (string.IsNullOrEmpty(concordNumber))
                {
                    return;
                }
                concordNumber = concordNumber.Trim();

            }
        }

        /// <summary>
        ///原承包合同编号
        /// </summary>
        public string YCBHTBM
        {
            get { return sourceNubmer; }
            set
            {
                sourceNubmer = value;
                sourceNubmer = sourceNubmer.TrimSafe();
                NotifyPropertyChanged("YCBHTBM");
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
            get { return arableLandType; }
            set { arableLandType = value.TrimSafe(); NotifyPropertyChanged("CBFS"); }
        }

        /// <summary>
        ///承包起始时间
        /// </summary>
        public DateTime? CBQXQ
        {
            get { return arableLandStartTime; }
            set { arableLandStartTime = value; NotifyPropertyChanged("CBQXQ"); }
        }

        /// <summary>
        ///承包结束时间
        /// </summary>
        public DateTime? CBQXZ
        {
            get { return arableLandEndTime; }
            set { arableLandEndTime = value; NotifyPropertyChanged("CBQXZ"); }
        }
     
        /// <summary>
        ///合同总面积(亩)
        /// </summary>
        public double HTZMJ
        {
            get { return htzmj; }
            set { htzmj = value; NotifyPropertyChanged("HTZMJ"); }
        }

        /// <summary>
        ///原合同总面积(亩)
        /// </summary>
        public double YHTZMJ
        {
            get { return yhtzmj; }
            set { yhtzmj = value; NotifyPropertyChanged("YHTZMJ"); }
        }

        /// <summary>
        ///承包期限
        /// </summary>
        public int CBDKZS
        {
            get { return cbdks; }
            set { cbdks = value; NotifyPropertyChanged("CBDKZS"); }
        }

        /// <summary>
        ///签订时间
        /// </summary>
        public DateTime? QDSJ
        {
            get { return qdsj; }
            set
            {
                qdsj = value; NotifyPropertyChanged("QDSJ");
            }
        }

        #endregion

        #region Ctor

        public CBHT_VIEW()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// 转换成标准模型
        /// </summary>
        /// <returns></returns>
        public ATT_CBHT ToStandardModel()
        {
            object obj = this.ConvertTo(typeof(ATT_CBHT));
            ATT_CBHT cbht = obj as ATT_CBHT;
            return cbht;
        }

        /// <summary>
        /// 转换成界面模型
        /// </summary>
        /// <param name="fbf"></param>
        public static CBHT_VIEW ToViewModel(ATT_CBHT cbht)
        {
            object obj = cbht.ConvertTo(typeof(CBHT_VIEW));
            CBHT_VIEW cbhtView = obj as CBHT_VIEW;
            return cbhtView;
        }

        #endregion
    }
}