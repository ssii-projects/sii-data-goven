using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore
{
    /// <summary>
    /// 安全转换（无异常抛出）辅助类
    /// </summary>
    public class SafeConvertAux
    {
        public static double ToDouble(object? o)
        {
            try
            {
                var n = Convert.ToDouble(o);
                return n;
            }
            catch
            {
            }
            return 0;
        }
        public static float ToFloat(object? o)
        {
            try
            {
                var n = Convert.ToSingle(o);
                return n;
            }
            catch
            {
            }
            return 0;
        }
        /// <summary>
        /// 将对象转换为int类型，若不能转换也返回0
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static int ToInt32(object? o)
        {
            try
            {
                var n = Convert.ToInt32(o);
                return n;
            }
            catch
            {
            }
            return 0;
        }
        public static short ToShort(object? o)
        {
            try
            {
                var n = Convert.ToInt16(o);
                return n;
            }
            catch
            {
            }
            return 0;
        }
        public static string ToStr(object? o)
        {
            if (o == null)
                return "";
            return o.ToString()??string.Empty;
        }
    }
}
