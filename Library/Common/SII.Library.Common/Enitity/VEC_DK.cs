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
    /// 地块
    /// </summary>
    [Serializable]
    [DataTable("DLXX_DK", AliasName = "地块")]
    public class VEC_DK : VEC_CBDK
    {
        #region Property

        /// <summary>
        /// 是否基本农田(M)(eWhether)
        /// </summary>
        [DataColumn("SFJBNT", AliasName = "是否基本农田")]
        public virtual string SFJBNT { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public VEC_DK()
        {
            YSDM = "211011";
        }

        #endregion

    };

    /// <summary>
    /// 统计地块
    /// </summary>
    public class DKSUM : VEC_DK
    {
        /// <summary>
        /// 合同面积
        /// </summary>
        public double HTMJ { get; set; }
    }
}