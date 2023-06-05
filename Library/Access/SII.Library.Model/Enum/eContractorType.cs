/*
 * (C) 2012-2016 公司版权所有，保留所有权利
*/
using System;

using Agro.LibCore;

namespace Agro.Library.Model
{
    /// <summary>
    /// 承包方类型
    /// </summary>
    [Serializable]
    public enum eContractorType
    {
        /// <summary>
        /// 农户
        /// </summary>
        [EnumNameAttribute("农户")]
        Farmer = 1,

        /// <summary>
        /// 个人
        /// </summary>
        [EnumNameAttribute("个人")]
        Personal = 2,

        /// <summary>
        /// 单位
        /// </summary>
        [EnumNameAttribute("单位")]
        Unit = 3
    }
}