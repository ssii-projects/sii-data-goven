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
    /// 数据字典
    /// </summary>
    [Serializable]
    public class DataDictionary : NotifyCDObject
    {
        #region Const

        /// <summary>界线类型数据字典分组码。</summary>
        public const string DICKEY_JXLX = "00001";

        /// <summary>界线性质数据字典分组码。</summary>
        public const string DICKEY_JXXZ = "00002";

        /// <summary>界址线位置数据字典分组码。</summary>
        public const string DICKEY_JZXWZ = "00003";

        /// <summary>界标类型数据字典分组码。</summary>
        public const string DICKEY_JBLX = "00004";

        /// <summary>界址点类型数据字典分组码。</summary>
        public const string DICKEY_JZDLX = "00005";

        /// <summary>面积单位数据字典分组码。</summary>
        public const string DICKEY_MJDW = "00007";

        /// <summary>权利类型数据字典分组码。</summary>
        public const string DICKEY_QLLX = "00008";

        /// <summary>权利性质数据字典分组码。</summary>
        public const string DICKEY_QLXZ = "00009";

        /// <summary>权力设立情况数据字典分组码。</summary>
        public const string DICKEY_QLSLQK = "00010";

        /// <summary>海域海岛用海类型A数据字典分组码。</summary>
        public const string DICKEY_HYHD_YHLXA = "00012";

        /// <summary>海域海岛用海类型B数据字典分组码。</summary>
        public const string DICKEY_HYHD_YHLXB = "00013";

        /// <summary>海域海岛等别数据字典分组码。</summary>
        public const string DICKEY_HYHD_DB = "00014";

        /// <summary>户型数据字典分组码。</summary>
        public const string DICKEY_HX = "00015";

        /// <summary>房屋类型数据字典分组码。</summary>
        public const string DICKEY_FWLX = "00018";

        /// <summary>户型结构数据字典分组码。</summary>
        public const string DICKEY_HXJG = "00016";

        /// <summary>规划用途数据字典分组码。</summary>
        public const string DICKEY_GHYT = "00017";

        /// <summary>房屋性质数据字典分组码。</summary>
        public const string DICKEY_FWXZ = "00019";

        /// <summary>登记类型数据字典分组码。</summary>
        public const string DICKEY_DJLX = "00021";

        /// <summary>林种数据字典标识码。</summary>
        public const string DICKEY_LZ = "00026";

        /// <summary>抵押不动产类型数据字典分组码。</summary>
        public const string DICKEY_DYBDCLX = "00027";

        /// <summary>抵押方式数据字典分组码。</summary>
        public const string DICKEY_DYFS = "00028";

        /// <summary>证件种类数据字典分组码。</summary>
        public const string DICKEY_ZJZL = "00030";

        /// <summary>共有方式数据字典分组码。</summary>
        public const string DICKEY_GYFS = "00034";

        /// <summary>预告登记种类数据字典分组码。</summary>
        public const string DICKEY_YGDJZL = "00029";

        /// <summary>国家/地区数据字典分组码。</summary>
        public const string DICKEY_GJ = "00035";

        /// <summary>权利人类型数据字典分组码。</summary>
        public const string DICKEY_QLRLX = "00036";

        /// <summary>户籍所在省市数据字典分组码。</summary>
        public const string DICKEY_HJSZSS = "00038";

        /// <summary>收件类型分组码。</summary>
        public const string DICKEY_SJLX = "00040";

        /// <summary>所属行业数据字典分组码。</summary>
        public const string DICKEY_SSHY = "00041";

        /// <summary>通知方式数据字典分组码。</summary>
        public const string DICKEY_TZFS = "00042";

        /// <summary>性别数据字典分组码。</summary>
        public const string DICKEY_XB = "00043";

        /// <summary>房屋结构数据字典分组码。</summary>
        public const string DICKEY_FWJG = "00046";

        /// <summary>建筑物状态数据字典分组码。</summary>
        public const string DICKEY_JZWZT = "00047";

        /// <summary>界址线类别数据字典标识码。</summary>
        public const string DICKEY_JZXLB = "00048";

        /// <summary>海域海岛项目性质数据字典标识码。</summary>
        public const string DICKEY_HYHD_XMXZ = "00049";

        /// <summary>土地等级数据字典标识码。</summary>
        public const string DICKEY_TDDJ = "00050";

        /// <summary>用海方式数据字典标识码。</summary>
        public const string DICKEY_YHFS = "00051";

        /// <summary>起源数据字典标识码。</summary>
        public const string DICKEY_QY = "00052";

        /// <summary>无居民海岛用途数据字典标识码。</summary>
        public const string DICKEY_WJMHDYT = "00053";

        /// <summary>农用地地块类别数据字典标识码。</summary>
        public const string DICKEY_NYDDKLB = "61012";

        /// <summary>农用地承包方式数据字典标识码。</summary>
        public const string DICKEY_NYDCBFS = "61011";

        /// <summary>土地用途数据字典标识码。</summary>
        public const string DICKEY_TDYT = "61018";

        /// <summary>构筑物类型数据字典标识码。</summary>
        public const string DICKEY_GZWLX = "00025";

        /// <summary>  家庭关系 </summary>
        public const string DICKEY_JTGX2 = "61024";

        #endregion

        #region Fields

        /// <summary>
        ///  私有变量—分组名称
        /// </summary>
        private string groupName;

        /// <summary>
        ///  私有变量—分组码
        /// </summary>
        private string groupCode;

        /// <summary>
        ///  私有变量—名称
        /// </summary>
        private string name;

        /// <summary>
        ///  私有变量—编码
        /// </summary>
        private string code;

        /// <summary>
        ///  私有变量—别名
        /// </summary>
        private string aliseName;

        /// <summary>
        ///  私有变量—说明
        /// </summary>
        private string comment;

        #endregion

        #region Properties

        /// <summary>
        /// 分组码
        /// </summary>
        [DisplayLanguage("分组码")]
        public string GroupCode
        {
            get { return groupCode; }
            set
            {
                groupCode = value;
                NotifyPropertyChanged("GroupCode");
            }
        }

        /// <summary>
        /// 分组名称
        /// </summary>
        [DisplayLanguage("分组名称")]
        public string GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
                NotifyPropertyChanged("GroupName");
            }
        }

        /// <summary>
        /// 具体类型代码
        /// </summary>
        [DisplayLanguage("代码")]
        public string Code
        {
            get { return code; }
            set
            {
                code = value;
                NotifyPropertyChanged("Code");
            }
        }

        /// <summary>
        /// 具体类型名称
        /// </summary>
        [DisplayLanguage("名称")]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// 具体类型别名
        /// </summary>
        [DisplayLanguage("别名")]
        public string AliseName
        {
            get { return aliseName; }
            set
            {
                aliseName = value;
                NotifyPropertyChanged("AliseName");
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        [DisplayLanguage("说明")]
        public string Comment
        {
            get { return comment; }
            set
            {
                comment = value;
                NotifyPropertyChanged("Comment");
            }
        }

        #endregion

        #region Ctor

        /// <summary>
        /// 初始化数据字典ID编号
        /// </summary>
        public DataDictionary()
        {
        }

        #endregion
    }
}
