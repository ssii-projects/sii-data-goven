/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Agro.Library.Model
{
    /// <summary>
    /// 承包合同
    /// </summary>
    [Serializable]
    [DataTable("CBHT", AliasName = "承包合同")]
    public class ATT_CBHTEXP
    {
        #region Propertys

        /// <summary>
        /// 承包合同代码(M)
        /// </summary>
        [DataColumn("CBHTBM", AliasName = "承包合同代码")]
        public virtual string CBHTBM { get; set; }

        /// <summary>
        /// 原承包合同代码(C)
        /// </summary>
        [DataColumn("YCBHTBM", AliasName = "原承包合同代码")]
        public virtual string YCBHTBM { get; set; }

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
        /// 承包方式(M)(eCBJYQQDFS)
        /// </summary>
        [DataColumn("CBFS", AliasName = "承包方式")]
        public virtual string CBFS { get; set; }

        /// <summary>
        /// 承包期限起(M)
        /// </summary>
        [DataColumn("CBQXQ", AliasName = "承包期限起")]
        public virtual DateTime? CBQXQ { get; set; }

        /// <summary>
        /// 承包期限止(M)
        /// </summary>
        [DataColumn("CBQXZ", AliasName = "承包期限止")]
        public virtual DateTime? CBQXZ { get; set; }

        /// <summary>
        /// 承包确权(合同)总面积(M)
        /// </summary>
        [DataColumn("HTZMJ", AliasName = "承包确权(合同)总面积")]
        public virtual double HTZMJ { get; set; }

        /// <summary>
        /// 承包地块总数 (M)
        /// </summary>
        [DataColumn("CBDKZS", AliasName = "承包地块总数")]
        public virtual int CBDKZS { get; set; }

        /// <summary>
        /// 签订时间（M）
        /// </summary>
        [DataColumn("QDSJ", AliasName = "签订时间")]
        public virtual DateTime? QDSJ { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBHTEXP()
        {
        }

        #endregion
    }
}
