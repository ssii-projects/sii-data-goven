/*
 * (C) 2017 xx公司版权所有，保留所有权利
 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   StringUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2017/5/12 12:46:54
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
    /**
     * 字符串工具类
     * @author yanxm
     *
     */
    public class StringUtil
    {
        public static bool contains(String[] ss, String s)
        {
            for (int i = 0; i < ss.Length; ++i)
            {
                var t = ss[i];
                if (isEqual(t, s))
                {
                    return true;
                }
            }
            return false;
        }
        public static String toUpper(String s)
        {
            return s.ToUpper();//.toUpperCase(Locale.getDefault());
        }
        public static String toLower(String s)
        {
            return s.ToLower();// s.toLowerCase(Locale.getDefault());
        }
        /**
         * 判断字符串是否相等（区分大小写）
         * @param a
         * @param b
         * @return
         */
        public static bool isEqual(String a, String b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return a == b;
        }
        /**
         * 判断字符串是否相等（不区分大小写）
         * @param a
         * @param b
         * @return
         */
        public static bool isEqualIgnorCase(String a, String b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            return a.Equals(b, StringComparison.CurrentCultureIgnoreCase);
        }
        /**
         * 格式化字符串,scale表示保留小数点位数
         * @param result
         * @param scale
         * @return
         */
        public static String format(double result, int scale)
        {
            throw new NotImplementedException();
            //DecimalFormat r = new DecimalFormat();
            //String pat = "#0.";
            //for (int i = 0; i < scale; ++i)
            //    pat += "0";
            //r.applyPattern(pat);//保留小数位数，不足会补零 
            //return r.format(result);
        }
        public static int countChar(String str, char c)
        {
            int n = 0;
            for (int i = str.Length - 1; i >= 0; --i)
            {
                char cc = str[i];
                if (cc == c)
                {
                    ++n;
                }
            }
            return n;
        }
    }

}
