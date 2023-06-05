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
    /// 流转合同
    /// </summary>
    [Serializable]
    [DataTable("LZHT", AliasName = "流转合同")]
    public class ATT_LZHT
    {
        #region Property

        /// <summary>
        /// 承包合同代码（M）
        /// </summary>
        [DataColumn("CBHTBM", AliasName = "承包合同代码")]
        public virtual string CBHTBM { get; set; }

        /// <summary>
        /// 流转合同代码(M)
        /// </summary>
        [DataColumn("LZHTBM", AliasName = "流转合同代码")]
        public virtual string LZHTBM { get; set; }

        /// <summary>
        /// 承包方代码(M)
        /// </summary>
        [DataColumn("CBFBM", AliasName = "承包方代码")]
        public virtual string CBFBM { get; set; }

        /// <summary>
        /// 受让方代码(M)
        /// </summary>
        [DataColumn("SRFBM", AliasName = "受让方代码")]
        public virtual string SRFBM { get; set; }

        /// <summary>
        /// 流转方式(M)(eCBJYQLZFS)
        /// </summary>
        [DataColumn("LZFS", AliasName = "流转方式")]
        public virtual string LZFS { get; set; }

        /// <summary>
        /// 流转期限(M)
        /// </summary>
        [DataColumn("LZQX", AliasName = "流转期限")]
        public virtual string LZQX { get; set; }

        /// <summary>
        /// 流转期限开始日(M)
        /// </summary>
        [DataColumn("LZQXKSRQ", AliasName = "流转期限开始日")]
        public virtual DateTime? LZQXKSRQ { get; set; }

        /// <summary>
        /// 流转期限结束日(M)
        /// </summary>
        [DataColumn("LZQXJSRQ", AliasName = "流转期限结束日")]
        public virtual DateTime? LZQXJSRQ { get; set; }

        /// <summary>
        /// 流转面积(M)
        /// </summary>
        [DataColumn("LZMJ", AliasName = "流转面积")]
        public virtual double LZMJ { get; set; }

        /// <summary>
        /// 流转面积(o)
        /// </summary>
        [DataColumn("LZMJM", AliasName = "流转面积(亩)")]
        public virtual double? LZMJM { get; set; }

        /// <summary>
        /// 流转地块数(M)
        /// </summary>
        [DataColumn("LZDKS", AliasName = "流转地块数")]
        public virtual int LZDKS { get; set; }

        /// <summary>
        /// 流转前土地用途(O)(eTDYT)
        /// </summary>
        [DataColumn("LZQTDYT", AliasName = "流转前土地用途")]
        public virtual string LZQTDYT { get; set; }

        /// <summary>
        /// 流转后土地用途(O)(eTDYT)
        /// </summary>
        [DataColumn("LZHTDYT", AliasName = "流转后土地用途")]
        public virtual string LZHTDYT { get; set; }

        /// <summary>
        /// 流转费用说明(M)
        /// </summary>
        [DataColumn("LZJGSM", AliasName = "流转费用说明")]
        public virtual string LZJGSM { get; set; }

        /// <summary>
        /// 合同签订日期(M)
        /// </summary>
        [DataColumn("HTQDRQ", AliasName = "合同签订日期")]
        public virtual DateTime? HTQDRQ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_LZHT()
        {
        }

        #endregion
    }
}