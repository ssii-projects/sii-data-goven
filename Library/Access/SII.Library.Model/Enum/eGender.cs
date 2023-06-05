using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using Agro.LibCore;

namespace Agro.Library.Model
{
    /* 修改于2016/8/30 根据农业部规范进行修改 */
    public enum eGender
    {
        [EnumNameAttribute("男")]
        Male = 1,

        [EnumNameAttribute("女")]
        Female = 2,

        [EnumNameAttribute("")]
        Unknow = -1,
    }
}
