/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;


namespace Agro.Library.Model
{
    /// <summary>
    /// 承包地信息
    /// </summary>
    [Serializable]
    [DataTable("QSSJ_CBDKXX", AliasName = "承包地块信息")]
    public class QSSJ_CBDKXX : Entity<QSSJ_CBDKXX>
	{
		#region Property

		/// <summary>
		/// 地块代码(M)
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块代码")]
        public virtual string DKBM { get; set; }

        /// <summary>
        /// 发包方代码(M)
        /// </summary>
        [DataColumn("FBFBM", AliasName = "发包方代码")]
        public virtual string FBFBM { get; set; }

        /// <summary>
        /// 承包方代码(M)
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方代码")]
        public virtual string CBFBM { get; set; }

        /// <summary>
        /// 承包经营权取得方式(M)（eCBJYQQDFS）
        /// </summary>
        [DataColumn("CBJYQQDFS", AliasName = "承包经营权取得方式")]
        public virtual string CBJYQQDFS { get; set; }

        /// <summary>
        /// 承包合同代码(M)
        /// </summary>
        [DataColumn("CBHTBM", AliasName = "承包合同代码")]
        public virtual string CBHTBM { get; set; }

        /// <summary>
        /// 确权(合同)面积(M)
        /// </summary>
        [DataColumn("HTMJ", AliasName = "确权(合同)面积")]
        public virtual double HTMJ { get; set; }

        /// <summary>
        /// 确权(合同)面积(M)
        /// </summary>
        [DataColumn("HTMJM", AliasName = "确权(合同)面积亩")]
        public virtual double? HTMJM { get; set; }

        /// <summary>
        /// 原合同面积(M)
        /// </summary>
        [DataColumn("YHTMJ", AliasName = "原合同面积")]
        public virtual double? YHTMJ { get; set; }

        /// <summary>
        /// 原合同面积(M)
        /// </summary>
        [DataColumn("YHTMJM", AliasName = "原合同面积(亩)")]
        public virtual double? YHTMJM { get; set; }

        /// <summary>
        /// 承包经营权证(登记簿)代码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证代码")]
        public virtual string CBJYQZBM { get; set; }

        /// <summary>
        /// 流转合同代码(O)
        /// </summary>
        [DataColumn("LZHTBM", AliasName = "流转合同代码")]
        public virtual string LZHTBM { get; set; }

        /// <summary>
        /// 是否确权确股(O)
        /// </summary>
        [DataColumn("SFQQQG", AliasName = "是否确权确股")]
        public virtual string SFQQQG { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public QSSJ_CBDKXX ()
        {
        }

        #endregion

    }
}