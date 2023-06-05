// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Agro.Library.Model;
using Agro.LibCore;

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 发包方
    /// </summary>
    [Serializable]
    public class FBF_VIEW : NotifyCDObject
    {
        #region Fields

        private string name;
        private string lawyerName;
        private string lawyerTelephone;
        private string lawyerAddress;
        private string lawyerPosterNumber;
        private string lawyerCredentType;
        private string lawyerCartNumber;
        private string code;
        private string surveyPerson;
        private DateTime? surveyDate;
        private string surveyChronicle;

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public FBF_VIEW()
        {
            FZRZJLX = "1";
        }

        #endregion

        #region Properties

        /// <summary>
        ///发包方编码
        /// </summary>
        public string FBFBM
        {
            get { return code; }
            set
            {
                code = value.TrimSafe(); NotifyPropertyChanged("FBFBM");
            }
        }

        /// <summary>
        ///发包方名称
        /// </summary>
        public string FBFMC
        {
            get { return name; }
            set
            {
                name = value.TrimSafe(); NotifyPropertyChanged("FBFMC");
            }
        }

        /// <summary>
        ///发包方负责人姓名
        /// </summary>
        public string FBFFZRXM
        {
            get { return lawyerName; }
            set
            {
                lawyerName = value.TrimSafe(); NotifyPropertyChanged("FBFFZRXM");
            }
        }

        /// <summary>
        /// 负责人证件类型
        /// </summary>
        public string FZRZJLX
        {
            get { return lawyerCredentType; }
            set
            {
                lawyerCredentType = value; NotifyPropertyChanged("FZRZJLX");
            }
        }

        /// <summary>
        ///负责人证件号码
        /// </summary>
        public string FZRZJHM
        {
            get { return lawyerCartNumber; }
            set
            {
                lawyerCartNumber = value.TrimSafe(); NotifyPropertyChanged("FZRZJHM");
            }
        }

        /// <summary>
        ///联系电话
        /// </summary>
        public string LXDH
        {
            get { return lawyerTelephone; }
            set
            {
                lawyerTelephone = value.TrimSafe();
                NotifyPropertyChanged("LXDH");
            }
        }

        /// <summary>
        ///发包方地址
        /// </summary>
        public string FBFDZ
        {
            get { return lawyerAddress; }
            set
            {
                lawyerAddress = value.TrimSafe(); NotifyPropertyChanged("FBFDZ");
            }
        }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string YZBM
        {
            get { return lawyerPosterNumber; }
            set
            {
                lawyerPosterNumber = value.TrimSafe(); NotifyPropertyChanged("YZBM");
            }
        }

        /// <summary>
        /// 发包方调查员
        /// </summary>
        public string FBFDCY
        {
            get { return surveyPerson; }
            set
            {
                surveyPerson = value.TrimSafe(); NotifyPropertyChanged("FBFDCY");
            }
        }

        /// <summary>
        /// 发包方调查日期
        /// </summary>
        public DateTime? FBFDCRQ
        {
            get { return surveyDate; }
            set
            {
                surveyDate = value; NotifyPropertyChanged("FBFDCRQ");
            }
        }

        /// <summary>
        /// 发包方调查记事
        /// </summary>
        public string FBFDCJS
        {
            get { return surveyChronicle; }
            set
            {
                surveyChronicle = value.TrimSafe(); NotifyPropertyChanged("FBFDCJS");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 转换成标准模型
        /// </summary>
        /// <returns></returns>
        public ATT_FBF ToStandardModel()
        {
            object obj = this.ConvertTo(typeof(ATT_FBF));
            ATT_FBF fbf = obj as ATT_FBF;
            return fbf;
        }

        /// <summary>
        /// 转换成界面模型
        /// </summary>
        /// <param name="fbf"></param>
        public static FBF_VIEW ToViewModel(ATT_FBF fbf)
        {
            object obj = fbf.ConvertTo(typeof(FBF_VIEW));
            FBF_VIEW fbfView = obj as FBF_VIEW;
            return fbfView;
        }

        #endregion
    }
}