using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Agro.LibCore;

namespace Agro.Library.Model
{
    /// <summary>
    /// 指示地域的级别。
    /// </summary>
    public enum eZoneLevel
    {
        Unknown=0,
        /// <summary>
        /// 组级
        /// </summary>
        [EnumNameAttribute("组级")]
        Group = 1,

        /// <summary>
        /// 村级
        /// </summary>
        [EnumNameAttribute("村级")]
        Village = 2,

        /// <summary>
        /// 乡镇级
        /// </summary>
        [EnumNameAttribute("镇级")]
        Town = 3,

        /// <summary>
        /// 区县级
        /// </summary>
        [EnumNameAttribute("县级")]
        County = 4,

        /// <summary>
        /// 市地级
        /// </summary>
        [EnumNameAttribute("市级")]
        City = 5,

        /// <summary>
        /// 省级
        /// </summary>
        [EnumNameAttribute("省级")]
        Province = 6,

        /// <summary>
        /// 国家级
        /// </summary>
        [EnumNameAttribute("国家级")]
        State = 7,
    }
}