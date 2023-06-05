// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Agro.LibCore;
using Agro.LibCore.Database;

namespace DotNetSharp.Library.Entity
{
    /// <summary>
    /// 承包权证
    /// </summary>
    [Serializable]
    public class CBQZ_VIEW : NotifyCDObject
    {
        #region Fields

        private string sendOrganization;
        private DateTime sendDate;
        private string regeditBookGetted;
        private DateTime regeditBookGettedDate;
        private string getterName;
        private string getterCardType;
        private string getterCardNumber;

        #endregion

        #region Properties

        /// <summary>
        ///承包经营权证编码
        /// </summary>
        public string CBJYQZBM
        {
            get; set;
        }

        /// <summary>
        ///发证机关
        /// </summary>
        public string FZJG
        {
            get { return sendOrganization; }
            set
            {
                sendOrganization = value.TrimSafe();
                NotifyPropertyChanged("FZJG");
            }
        }

        /// <summary>
        ///发证日期
        /// </summary>
        public DateTime FZRQ
        {
            get { return sendDate; }
            set
            {
                sendDate = value;
                NotifyPropertyChanged("FZRQ");
            }
        }

        /// <summary>
        /// 权证是否领取
        /// </summary>
        [DataColumn("QZSFLQ")]
        public string QZSFLQ
        {
            get { return regeditBookGetted; }
            set
            {
                regeditBookGetted = value.TrimSafe();
                NotifyPropertyChanged("QZSFLQ");
            }
        }

        /// <summary>
        /// 权证领取日期
        /// </summary>
        [DataColumn("QZLQRQ")]
        public DateTime QZLQRQ
        {
            get { return regeditBookGettedDate; }
            set
            {
                regeditBookGettedDate = value;
                NotifyPropertyChanged("QZLQRQ");
            }
        }

        /// <summary>
        /// 权证领取人姓名
        /// </summary>
        [DataColumn("QZLQRXM")]
        public string QZLQRXM
        {
            get { return getterName; }
            set
            {
                getterName = value.TrimSafe();
                NotifyPropertyChanged("QZLQRXM");
            }
        }

        /// <summary>
        /// 权证领取人证件类型
        /// </summary>
        [DataColumn("QZLQRZJLX")]
        public string QZLQRZJLX
        {
            get { return getterCardType; }
            set
            {
                getterCardType = value.TrimSafe();
                NotifyPropertyChanged("QZLQRZJLX");
            }
        }

        /// <summary>
        /// 权证领取人证件号码
        /// </summary>
        [DataColumn("QZLQRZJHM")]
        public string QZLQRZJHM
        {
            get { return getterCardNumber; }
            set
            {
                getterCardNumber = value.TrimSafe();
                NotifyPropertyChanged("QZLQRZJHM");
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造函数
        /// </summary>
        public CBQZ_VIEW()
        {
        }

        #endregion
    }
}