using Agro.Library.Common.Util;
//using Agro.Library.Exchange;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agro.Library.Common;

namespace Agro.Module.ThemeAnaly.Common
{
    class Util
    {
        /// <summary>
        /// 从登记簿表中查询最近5年的年份
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<int> QueryDistinctYearsFromDJB(IWorkspace db)
        {
            var lst = new List<int>();
            var sql = "SELECT DISTINCT "+db.SqlFunc.Year("DJSJ")+" FROM DJ_CBJYQ_DJB";
            db.QueryCallback(sql, r =>
            {
                if (!r.IsDBNull(0))
                {
                    var sYear = r.GetValue(0).ToString();
                    lst.Add(SafeConvertAux.ToInt32(sYear));
                }
                return true;
            });
            lst.Sort();
            while (lst.Count > 5)
            {
                lst.RemoveAt(0);
            }
            return lst;
        }
        /// <summary>
        /// 从登记簿表中查询最近一条数据的年份
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static int QuerLastYearFromDJB(IWorkspace db)
        {
            int nYear = DateTime.Now.Year;
            var sql = "SELECT  DISTINCT "+db.SqlFunc.Year("DJSJ")+" t FROM DJ_CBJYQ_DJB order by t desc";
            db.QueryCallback(sql, r =>
            {
                nYear = SafeConvertAux.ToInt32(r.GetValue(0));
                return false;
            });
            return nYear;
        }

        /// <summary>
        /// 查询zone所有下级地域的名称（不包含zone自身）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public static List<string> QueryChildZoneNames(IWorkspace db,ShortZone zone)
        {
            var lst = new List<string>();

            var ls = ZoneUtil.QueryChildren(zone);// selZone, _zoneCol);
            foreach (var l in ls)
            {
                if (l.Level != zone.Level)
                {
                    lst.Add(l.Name);
                }
            }
            return lst;
        }
    }
}
