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
    [DataTable("QSSJ_CBHT", AliasName = "承包合同")]
    public class ATT_CBHT : ATT_CBHTEXP
    {
        #region Propertys

        /// <summary>
        /// 承包确权(合同)总面积亩(M)
        /// </summary>
        [DataColumn("HTZMJM", AliasName = "承包确权(合同)总面积亩")]
        public virtual double? HTZMJM { get; set; }

        /// <summary>
        /// 原承包合同总面积(C)
        /// </summary>
        [DataColumn("YHTZMJ", AliasName = "原承包合同总面积")]
        public virtual double? YHTZMJ { get; set; }

        /// <summary>
        /// 原承包合同总面积亩(C)
        /// </summary>
        [DataColumn("YHTZMJM", AliasName = "原承包合同总面积亩")]
        public virtual double? YHTZMJM { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public ATT_CBHT()
        {
        }

        #endregion
    }
}
