/*
 * (C) 2015  公司版权所有,保留所有权利 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Agro.LibCore;
using System.Xml;

using Agro.Library.Model;

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 承包方
    /// </summary>
    [Serializable]
    public class CBF_VIEW : NotifyInfoCDObject
    {
        #region Fields

        private string name;//承包方姓名
        private string address;//承包方地址
        private string cardType;//证件类型
        private string number;//证件号码
        private string telephone;//联系电话
        private string postalNumber;//邮政编码
        private string familyNumber;//承包方编码
        private int personCount;//家庭成员数
        private string contractorType;//承包方类型
        private string surveyPerson;//承包方调查员
        private DateTime? surveyDate;//承包方调查日期
        private string surveyChronicle;//承包方调查记事
        private string publicityChronicle;//公示记事
        private string publicityChroniclePerson;//公示审核人
        private DateTime? publicityDate;//公示审核日期
        private string publicityCheckPerson;//公示审核人

        #endregion

        #region Properties

        /// <summary>
        ///名称
        /// </summary>
        public string CBFMC
        {
            get { return name; }
            set { name = value.TrimSafe(); NotifyPropertyChanged("CBFMC"); }
        }

        /// <summary>
        /// 承包方编码
        /// </summary>
        public string CBFBM
        {
            get { return familyNumber; }
            set { familyNumber = value.TrimSafe(); NotifyPropertyChanged("CBFBM"); }
        }

        /// <summary>
        /// 承包方类型
        /// </summary>
        public string CBFLX
        {
            get { return contractorType; }
            set { contractorType = value; NotifyPropertyChanged("CBFLX"); }
        }

        /// <summary>
        ///承包方证件类型
        /// </summary>
        public string CBFZJLX
        {
            get { return cardType; }
            set { cardType = value; NotifyPropertyChanged("CBFZJLX"); }
        }

        /// <summary>
        ///承包方证件号码
        /// </summary>
        public string CBFZJHM
        {
            get { return number; }
            set
            {
                number = value;
                if (string.IsNullOrEmpty(number))
                {
                    return;
                }
                number = number.Trim();
                NotifyPropertyChanged("CBFZJHM");
            }
        }

        /// <summary>
        ///承包方地址
        /// </summary>
        public string CBFDZ
        {
            get { return address; }
            set { address = value.TrimSafe(); NotifyPropertyChanged("CBFDZ"); }
        }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string LXDH
        {
            get { return telephone; }
            set { telephone = value.TrimSafe(); NotifyPropertyChanged("LXDH"); }
        }

        /// <summary>
        /// 邮政编码
        /// </summary>
        public string YZBM
        {
            get { return postalNumber; }
            set { postalNumber = value.TrimSafe(); NotifyPropertyChanged("YZBM"); }
        }

        /// <summary>
        /// 承包方成员数量
        /// </summary>
        public int CBFCYSL
        {
            get { return personCount; }
            set
            {
                personCount = value;
                NotifyPropertyChanged("CBFCYSL");
            }
        }

        /// <summary>
        /// 调查员
        /// </summary>
        public string CBFDCY
        {
            get { return surveyPerson; }
            set { surveyPerson = value; NotifyPropertyChanged("CBFDCY"); }
        }

        /// <summary>
        /// 调查日期
        /// </summary>
        public DateTime? CBFDCRQ
        {
            get { return surveyDate; }
            set { surveyDate = value; NotifyPropertyChanged("CBFDCRQ"); }
        }

        /// <summary>
        /// 调查记事
        /// </summary>
        public string CBFDCJS
        {
            get { return surveyChronicle; }
            set { surveyChronicle = value; NotifyPropertyChanged("CBFDCJS"); }
        }

        /// <summary>
        /// 公示记事
        /// </summary>
        public string GSJS
        {
            get { return publicityChronicle; }
            set { publicityChronicle = value; NotifyPropertyChanged("GSJS"); }
        }

        /// <summary>
        /// 公示记事人
        /// </summary>
        public string GSJSR
        {
            get { return publicityChroniclePerson; }
            set { publicityChroniclePerson = value; NotifyPropertyChanged("GSJSR"); }
        }

        /// <summary>
        /// 公示日期
        /// </summary>
        public DateTime? GSSHRQ
        {
            get { return publicityDate; }
            set { publicityDate = value; NotifyPropertyChanged("GSSHRQ"); }
        }

        /// <summary>
        /// 公示审核人
        /// </summary>
        public string GSSHR
        {
            get { return publicityCheckPerson; }
            set { publicityCheckPerson = value; NotifyPropertyChanged("GSSHR"); }
        }

        #endregion

        #region Ctor

        public CBF_VIEW()
        {
            cardType = "1";
        }

        #endregion

        #region Methods

        /// <summary>
        /// 转换成标准模型
        /// </summary>
        /// <returns></returns>
        public ATT_CBF ToStandardModel()
        {
            object obj = this.ConvertTo(typeof(ATT_CBF));
            ATT_CBF cbf = obj as ATT_CBF;
            return cbf;
        }

        /// <summary>
        /// 转换成界面模型
        /// </summary>
        /// <param name="fbf"></param>
        public static CBF_VIEW ToViewModel(ATT_CBF cbf)
        {
            object obj = cbf.ConvertTo(typeof(CBF_VIEW));
            CBF_VIEW cbfView = obj as CBF_VIEW;
            return cbfView;
        }

        #endregion
    }
}
