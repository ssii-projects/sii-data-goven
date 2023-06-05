/*
 * (C) 2016 公司版权所有，保留所有权利
*/
using Agro.LibCore.Database;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Agro.Module.SketchMap
{
    /// <summary>
    /// 地块
    /// </summary>
    [Serializable]
    [DataTable("DLXX_DK", AliasName = "地块")]
    public class DCDK
    {
        #region Property

        /// <summary>
        /// 地块代码(M)
        /// </summary>
        [DataColumn("DKBM", AliasName = "地块代码")]
        public virtual string DKBM { get; set; }

        /// <summary>
        /// 要素代码(M)(eFeatureType)
        /// </summary>
        [DataColumn("CBFMC", AliasName = "承包方名称")]
        public virtual string CBFMC { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// 构造方法
        /// </summary>
        public DCDK()
        {

        }

        #endregion

    };
}