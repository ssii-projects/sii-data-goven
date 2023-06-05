using Agro.LibCore;
//using DotNetSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.Module.ThemeAnaly.Common
{
    public static class Extend
    {

        //public static List<string> EnumToList<T>(this T t) where T:struct 
        //{
        //    var enumItems= t.GetType().GetFields().Select(f => f.GetValue(t)?.ToString()).ToList();
        //    return enumItems;
        //}


        /// <summary>
        /// 将bool转换成字符串是or否or“”
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string BoolToString(this bool? value)
        {
            return value == null ? "" : BoolToString(value.Value);
        }

        /// <summary>
        /// 获取当前枚举项的描述, 请在枚举前加EnumNameAttribute
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        public static string EnumToString<T>(this T sourceValue) where T : struct
        {
            try
            {
                foreach (var f in typeof(T).GetFields())
                {
                    if (f.Name != sourceValue.ToString()) continue;
                    if (f.GetAttribute<EnumNameAttribute>() != null)
                        return f.GetAttribute<EnumNameAttribute>().Description;
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                throw new Exception("枚举转换失败！" + e.Message + e.StackTrace);
            }
        }


        /// <summary>
        /// 通过枚举描述string获得枚举项,使用前请加上EnumNameAttribute特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value) where T : struct
        {
            foreach (var f in typeof(T).GetFields())
            {
                if (f.GetAttribute<EnumNameAttribute>() == null) continue;
                if (f.GetAttribute<EnumNameAttribute>().Description == value?.Trim())
                    return (T)f.GetValue(null);
            }
            return default(T);
        }







    }
}
