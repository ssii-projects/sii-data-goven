/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   EnumUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/10/26 21:44:14
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore
{
    public class EnumUtil
    {
        public static bool StrToEnum<TEnum>(string source, ref TEnum val)where TEnum:struct
        {
            if (!string.IsNullOrEmpty(source))
            {
                Type target = typeof(TEnum);
                System.Diagnostics.Debug.Assert(target.IsEnum);
                TEnum t;
                if (Enum.TryParse(source, out t))
                {
                    val = t;
                    return true;
                }
            }
            return false;
        }
    }
}
