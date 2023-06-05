// (C) 2015 公司版权所有，保留所有权利
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Agro.LibCore;

using System.Xml.Serialization;
using Agro.Library.Model;

namespace Agro.Library.Exchange
{
    /// <summary>
    /// 承包方家庭成员
    /// </summary>
    [Serializable]
    public class JTCY_VIEW : NotifyInfoCDObject
    {
        #region Fields

        private string name;
        private string cardType;//证件类型
        private string icn;
        private string relationship;
        private bool isSharePerson;//是否共有人
        private string cybz;//成员备注
        private string cybzsm;//成员备注说明

        #endregion

        #region Properties

        /// <summary>
        /// 家庭成员姓名
        /// </summary>
        public string CYXM
        {
            get { return name; }
            set { name = value.TrimSafe(); NotifyPropertyChanged("CYXM"); }
        }

        /// <summary>
        /// 成员性别
        /// </summary>
        public string CYXB { get; set; }

        /// <summary>
        /// 证件类型
        /// </summary>
        public string CYZJLX
        {
            get { return cardType; }
            set { cardType = value; NotifyPropertyChanged("CYZJLX"); }
        }

        /// <summary>
        /// 成员证件号码
        /// </summary>
        public string CYZJHM
        {
            get { return icn; }
            set { icn = value; if (icn != null) icn = icn.Trim(); NotifyPropertyChanged("CYZJHM"); }
        }

        /// <summary>
        /// 家庭关系
        /// </summary>
        public string YHZGX
        {
            get { return relationship; }
            set
            {
                relationship = value;
                NotifyPropertyChanged("YHZGX");
            }
        }

        /// <summary>
        /// 是否共有人
        /// </summary>
        public bool SFGYR
        {
            get { return isSharePerson; }
            set
            {
                isSharePerson = value;
                NotifyPropertyChanged("SFGYR");
            }
        }

        /// <summary>
        /// 成员备注
        /// </summary>
        public string CYBZ
        {
            get { return cybz; }
            set
            {
                cybz = value;
                NotifyPropertyChanged("CYBZ");
            }
        }

        /// <summary>
        /// 成员备注说明
        /// </summary>
        public string CYBZSM
        {
            get { return cybzsm; }
            set
            {
                cybzsm = value;
                NotifyPropertyChanged("CYBZSM");
            }
        }

        #endregion

        #region Ctor

        public JTCY_VIEW()
        {
            CYZJLX = "1";
            SFGYR = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 获取年龄
        /// </summary>
        /// <returns></returns>
        public int GetAge()
        {
            if (CYZJLX != "1")
            {
                return -1;
            }
            DateTime birthday = GetBirthday(icn);
            if (birthday.Year == DateTime.MinValue.Year)
            {
                return -1;
            }
            DateTime now = DateTime.Now;

            int year = birthday.Year;
            int nowYear = now.Year;

            int month = birthday.Month;
            int nowMonth = now.Month;

            int day = birthday.Day;
            int nowDay = now.Day;

            int age = nowYear - year;
            if (age < 1)
            {
                return -1;
            }
            return nowMonth > month || (nowMonth == month && nowDay >= day) ? age : --age;
        }

        /// <summary>
        /// 提取身份证号码出生日期
        /// </summary>
        private DateTime GetBirthday(string icn)
        {
            DateTime val = DateTime.MinValue;
            int year = 0, month = 0, day = 0;
            bool flag = false;
            if (icn.Length == 15)
            {
                flag = int.TryParse("19" + icn.Substring(6, 2), out year)
                    && int.TryParse(icn.Substring(8, 2), out month)
                    && int.TryParse(icn.Substring(10, 2), out day);
            }
            else if (icn.Length == 18)
            {
                flag = int.TryParse(icn.Substring(6, 4), out year)
                    && int.TryParse(icn.Substring(10, 2), out month)
                    && int.TryParse(icn.Substring(12, 2), out day);
            }
            //2月30号依然异常
            //flag = flag && year > 0 && year < 10000 && month > 0 && month < 13 && day > 0 && day < 32;
            if (flag)
                try
                {
                    val = new DateTime(year, month, day);
                }
                catch { }
            return val;
        }

        /// <summary>
        /// 转换成标准模型
        /// </summary>
        /// <returns></returns>
        public ATT_CBF_JTCY ToStandardModel()
        {
            object obj = this.ConvertTo(typeof(ATT_CBF_JTCY));
            ATT_CBF_JTCY jtcy = obj as ATT_CBF_JTCY;
            return jtcy;
        }

        /// <summary>
        /// 转换成界面模型
        /// </summary>
        /// <param name="jtcy"></param>
        public static JTCY_VIEW ToViewModel(ATT_CBF_JTCY jtcy)
        {
            object obj = jtcy.ConvertTo(typeof(JTCY_VIEW));
            JTCY_VIEW jtcyView = obj as JTCY_VIEW;
            return jtcyView;
        }

        #endregion
    }
}