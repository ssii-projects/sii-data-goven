/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Agro.Library.Model
{
    /// <summary>
    /// 发包方
    /// </summary>
    [Serializable]
    [DataTable("QSSJ_FBF", AliasName = "发包方")]
    public class QSSJ_FBF : Entity<QSSJ_FBF >
	{
		#region Property

		/// <summary>
		/// 发包方代码(M)
		/// </summary>
		[DataColumn("FBFBM", AliasName = "发包方代码")]
        public virtual string FBFBM { get; set; }

        /// <summary>
        /// 发包方名称(M)
        /// </summary>
        [DataColumn("FBFMC", AliasName = "发包方名称")]
        public virtual string FBFMC { get; set; }

        /// <summary>
        /// 发包方负责人姓名(M)
        /// </summary>
        [DataColumn("FBFFZRXM", AliasName = "发包方负责人姓名")]
        public virtual string FBFFZRXM { get; set; }

        /// <summary>
        /// 负责人证件类型(M)(eZJLX)
        /// </summary>
        [DataColumn("FZRZJLX", AliasName = "负责人证件类型")]
        public virtual string FZRZJLX { get; set; }

        /// <summary>
        /// 负责人证件号(M)
        /// </summary>
        [DataColumn("FZRZJHM", AliasName = "负责人证件号")]
        public virtual string FZRZJHM { get; set; }

        /// <summary>
        /// 联系电话(O)
        /// </summary>
        [DataColumn("LXDH", AliasName = "联系电话")]
        public virtual string LXDH { get; set; }

        /// <summary>
        /// 发包方地址(M)
        /// </summary>
        [DataColumn("FBFDZ", AliasName = "发包方地址")]
        public virtual string FBFDZ { get; set; }

        /// <summary>
        /// 邮政编码(M)
        /// </summary>
        [DataColumn("YZBM", AliasName = "邮政编码")]
        public virtual string YZBM { get; set; }

        /// <summary>
        /// 发包方调查员(M)
        /// </summary>
        [DataColumn("FBFDCY", AliasName = "发包方调查员")]
        public virtual string FBFDCY { get; set; }

        /// <summary>
        /// 发包方调查日期(M)
        /// </summary>
        [DataColumn("FBFDCRQ", AliasName = "发包方调查日期")]
        public virtual DateTime? FBFDCRQ { get; set; }

        /// <summary>
        /// 发包方调查记事(C)
        /// </summary>
        [DataColumn("FBFDCJS", AliasName = "发包方调查记事")]
        public virtual string FBFDCJS { get; set; }
               
        #endregion
    }
}