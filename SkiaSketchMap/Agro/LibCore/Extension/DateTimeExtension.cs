using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
    public static class DateTimeExtension
    {
        public static string CalcateTerm(this DateTime startTime, DateTime endTime)
        {
            int num = endTime.Year - startTime.Year;
            int num2 = endTime.Month - startTime.Month;
            int num3 = endTime.Day - startTime.Day;
            if (num3 == 30 && num2 == 11)
            {
                num++;
            }

            return num.ToString();
        }

        public static string LongDateString(this DateTime date)
        {
            return date.Year + "年" + date.Month + "月" + date.Day + "日";
        }

        public static string FullDateString(this DateTime date)
        {
            return $"{date:yyyy-MM-dd}";
        }

        public static string ShortDateString(this DateTime date)
        {
            return $"{date:yyyy-MM-dd}";
        }

        public static string DateYear(this DateTime date)
        {
            return date.Year + "年";
        }

        public static string DateMonth(this DateTime date)
        {
            return date.Day + "月";
        }

        public static string DateDay(this DateTime date)
        {
            return date.Day + "日";
        }

        public static string DateWithYearAndMonth(this DateTime date)
        {
            return date.Year + "." + date.Month;
        }
    }
}
