using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Agro.LibCore;

namespace Agro.Library.Model
{
    /// <summary>
    /// 证件类型
    /// </summary>
    public enum eCredentialsType
    {
        /// <summary>
        /// 居民身份证
        /// </summary>
        [EnumNameAttribute("居民身份证")]
        IdentifyCard = 1,

        /// <summary>
        /// 军官证
        /// </summary>
        [EnumNameAttribute("军官证")]
        OfficerCard = 2,

        /// <summary>
        /// 行政、企事业单位机构代码证或法人代码证
        /// </summary>
        [EnumNameAttribute("行政、企事业单位机构代码证或法人代码证")]
        AgentCard = 3,

        /// <summary>
        /// 户口簿
        /// </summary>
        [EnumNameAttribute("户口簿")]
        ResidenceBooklet = 4,

        /// <summary>
        /// 护照
        /// </summary>
        [EnumNameAttribute("护照")]
        Passport = 5,

        /// <summary>
        /// 其他证件
        /// </summary>
        [EnumNameAttribute("其他证件")]
        Other = 9,
    }
}
