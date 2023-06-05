/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;


namespace Agro.Library.Model
{
	/// <summary>
	/// 承包方
	/// </summary>
	[Serializable]
    [DataTable("QSSJ_CBF", AliasName = "承包方")]
    public class QSSJ_CBF :Entity<QSSJ_CBF >
    {
		public string ID { get; set; } = Guid.NewGuid().ToString();
		public string FBFBM { get; set; }
		/// <summary>
		/// 承包方代码(M)
		/// </summary>
		[DataColumn("CBFBM", AliasName = "承包方代码")]
        public  string CBFBM { get; set; }

        /// <summary>
        /// 承包方类型(M)(eCBFLX)
        /// </summary>
        [DataColumn("CBFLX", AliasName = "承包方类型")]
        public  string CBFLX { get; set; }

        /// <summary>
        /// 承包方(代表)名称(M)
        /// </summary>
        [DataColumn("CBFMC", AliasName = "承包方(代表)名称")]
        public  string CBFMC { get; set; }

        /// <summary>
        /// 承包方(代表)证件类型(eZJLX)(M)
        /// </summary>
        [DataColumn("CBFZJLX", AliasName = "承包方(代表)证件类型")]
        public  string CBFZJLX { get; set; }

        /// <summary>
        /// 承包方(代表)证件号码(M)
        /// </summary>
        [DataColumn("CBFZJHM", AliasName = "承包方(代表)证件号码")]
        public  string CBFZJHM { get; set; }

        /// <summary>
        /// 承包方地址(M)
        /// </summary>
        [DataColumn("CBFDZ", AliasName = "承包方地址")]
        public  string CBFDZ { get; set; }

        /// <summary>
        /// 邮政编码(M)
        /// </summary>
        [DataColumn("YZBM", AliasName = "邮政编码")]
        public  string YZBM { get; set; }

        /// <summary>
        /// 联系电话(O)
        /// </summary>
        [DataColumn("LXDH", AliasName = "联系电话")]
        public  string LXDH { get; set; }

        /// <summary>
        /// 承包方成员数量(M)
        /// </summary>
        [DataColumn("CBFCYSL", AliasName = "承包方成员数量")]
        public  int CBFCYSL { get; set; }

        /// <summary>
        /// 承包方调查日期(M)
        /// </summary>
        [DataColumn("CBFDCRQ", AliasName = "承包方调查日期")]
        public  DateTime? CBFDCRQ { get; set; }

        /// <summary>
        /// 承包方调查员(M)
        /// </summary>
        [DataColumn("CBFDCY", AliasName = "承包方调查员")]
        public  string CBFDCY { get; set; }

        /// <summary>
        /// 承包方调查记事(C)
        /// </summary>
        [DataColumn("CBFDCJS", AliasName = "承包方调查记事")]
        public  string CBFDCJS { get; set; }

        /// <summary>
        /// 公示记事(C)
        /// </summary>
        [DataColumn("GSJS", AliasName = "公示记事")]
        public  string GSJS { get; set; }

        /// <summary>
        /// 公示记事人(M)
        /// </summary>
        [DataColumn("GSJSR", AliasName = "公示记事人")]
        public  string GSJSR { get; set; }

        /// <summary>
        /// 公示审核日期(M)
        /// </summary>
        [DataColumn("GSSHRQ", AliasName = "公示审核日期")]
        public  DateTime? GSSHRQ { get; set; }

        /// <summary>
        /// 公示审核人(M)
        /// </summary>
        [DataColumn("GSSHR", AliasName = "公示审核人")]
        public  string GSSHR { get; set; }

		public int ZT { get; set; }
       public EDjzt DJZT { get; set; }
       public DateTime? CJSJ { get; set; }
	}
}