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
		public string ID { get; set; } = Guid.NewGuid().ToString();
		/// <summary>
		/// 地块代码(M)
		/// </summary>
		[DataColumn("DKBM", AliasName = "地块代码")]
        public  string DKBM { get; set; }

        /// <summary>
        /// 发包方代码(M)
        /// </summary>
        [DataColumn("FBFBM", AliasName = "发包方代码")]
        public  string FBFBM { get; set; }

        /// <summary>
        /// 承包方代码(M)
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方代码")]
        public  string CBFBM { get; set; }

        /// <summary>
        /// 承包经营权取得方式(M)（eCBJYQQDFS）
        /// </summary>
        [DataColumn("CBJYQQDFS", AliasName = "承包经营权取得方式")]
        public  string CBJYQQDFS { get; set; }

        /// <summary>
        /// 承包合同代码(M)
        /// </summary>
        [DataColumn("CBHTBM", AliasName = "承包合同代码")]
        public  string CBHTBM { get; set; }

        /// <summary>
        /// 确权(合同)面积(M)
        /// </summary>
        [DataColumn("HTMJ", AliasName = "确权(合同)面积")]
        public  decimal HTMJ { get; set; }

        /// <summary>
        /// 确权(合同)面积(M)
        /// </summary>
        [DataColumn("HTMJM", AliasName = "确权(合同)面积亩")]
        public  decimal? HTMJM { get; set; }

        /// <summary>
        /// 原合同面积(M)
        /// </summary>
        [DataColumn("YHTMJ", AliasName = "原合同面积")]
        public  decimal? YHTMJ { get; set; }

        /// <summary>
        /// 原合同面积(M)
        /// </summary>
        [DataColumn("YHTMJM", AliasName = "原合同面积(亩)")]
        public  decimal? YHTMJM { get; set; }

        /// <summary>
        /// 承包经营权证(登记簿)代码(M)
        /// </summary>
        [DataColumn("CBJYQZBM", AliasName = "承包经营权证代码")]
        public  string CBJYQZBM { get; set; }

        /// <summary>
        /// 流转合同代码(O)
        /// </summary>
        [DataColumn("LZHTBM", AliasName = "流转合同代码")]
        public  string LZHTBM { get; set; }

        /// <summary>
        /// 是否确权确股(O)
        /// </summary>
        [DataColumn("SFQQQG", AliasName = "是否确权确股")]
        public  string SFQQQG { get; set; }
    }
}