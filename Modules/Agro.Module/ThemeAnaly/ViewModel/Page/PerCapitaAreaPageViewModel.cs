using Agro.Library.Common;
using Agro.Module.ThemeAnaly.Common;
using Agro.Module.ThemeAnaly.Entity;
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class PerCapitaAreaPageViewModel :ColumChartPageBaseViewModel
    {
        class Data
        {
            /// <summary>
            /// 地域名称集合
            /// </summary>
            internal List<string> ListZoneNames;

            /// <summary>
            /// [地域名称，人均面积]
            /// </summary>
            private Dictionary<string, double> _dicData;
            private ShortZone _zone;
            /// <summary>
            /// 要统计的地域级别
            /// </summary>
            private int _nDyJb = 3;
            internal Data(ShortZone zone)
            {
                _zone = zone;
                _nDyJb = ((int)zone.Level) - 1;
                var db = MyGlobal.Workspace;
                //using (var db = DataBaseSource.GetDatabase())
                {
                    ListZoneNames = Util.QueryChildZoneNames(db, _zone);
                    _dicData = QueryValues(db);
                }
            }
            internal double GetValue(string zoneName)
            {
                double value = 0;
                if (!_dicData.TryGetValue(zoneName, out value))
                {
                    return 0;
                }
                return value;
            }

            private Dictionary<string, double> QueryValues(IWorkspace db)
            {
                int nYear = Util.QuerLastYearFromDJB(db);
                var lst = new Dictionary<string,double>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT D.MC,T.CYSL,T.MJ FROM 
	  (SELECT DE.SJID, SUM(T.CBFCYSL) CYSL,SUM(T.SCMJM) MJ FROM 
		(SELECT DE.ZJID, T.CBFCYSL,T.SCMJM FROM  
			(SELECT R.SZDY,T.CBFCYSL,T.SCMJM FROM 
				(SELECT L.DJBID DJBID, L.CBFBM,R.CBFCYSL, ROUND(L.SCMJ*0.0015,2) SCMJM FROM DJ_CBJYQ_DKXX L LEFT JOIN DJ_CBJYQ_CBF R ON L.CBFBM=R.CBFBM) T
				LEFT JOIN DJ_CBJYQ_DJB R 
					ON R.ID=T.DJBID WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}) T
			LEFT JOIN DLXX_XZDY_EXP DE ON DE.ZJID=T.SZDY WHERE DE.SJID='{1}') T
		LEFT JOIN DLXX_XZDY_EXP DE ON DE.ZJID=T.ZJID WHERE DE.SJJB={2} GROUP BY DE.SJID) T
	LEFT JOIN DLXX_XZDY D ON T.SJID=D.ID";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT D.MC,T.CBFCYSL,T.SCMJM 
FROM(
    SELECT DE.ZJID, T.CBFCYSL,T.SCMJM 
	FROM(
		SELECT R.SZDY,T.CBFCYSL,T.SCMJM 
		FROM(
			SELECT L.DJBID DJBID, L.CBFBM,R.CBFCYSL, ROUND(L.SCMJ*0.0015,2) SCMJM 
			FROM DJ_CBJYQ_DKXX L LEFT JOIN DJ_CBJYQ_CBF R ON L.CBFBM=R.CBFBM
		)T LEFT JOIN DJ_CBJYQ_DJB R ON R.ID=T.DJBID 
		WHERE (R.QSZT=1 OR (R.QSZT=2 AND " + db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
	)T LEFT JOIN DLXX_XZDY_EXP DE ON DE.ZJID=T.SZDY 
	WHERE DE.SJID='{1}'
)T LEFT JOIN DLXX_XZDY D ON T.ZJID=D.ID";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var nCysl = SafeConvertAux.ToDouble(r.GetValue(1));
                    var nMj = SafeConvertAux.ToDouble(r.GetValue(2));
                    double d = 0;
                    if (nCysl > 0)
                    {
                        d = Math.Round(nMj / nCysl, 2);
                    }
                    lst[name] = d;
                    return true;
                });
                return lst;
            }
        }
        private ShortZone _zone;
        public void Init(ShortZone zone)
        {
            _zone = zone;
            InitData();
        }
        protected override void InitData()
        {
            if (_zone == null)
            {
                return;
            }
            var rows = new ObservableCollection<string>();// { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            var rowItems = new ObservableCollection<string> { "人均面积" };// { "一等地", "二等地", "三等地", "四等地", "五等地", "六等地", "七等地", "八等地", "九等地", "十等地" };

            var data = new Data(_zone);
            foreach (var zoneName in data.ListZoneNames)
            {
                rows.Add(zoneName);// new ChartValue(zoneName,1000));
            }
            var chartData = new List<CharDataColum>();
            foreach (var name in rows)
            {
                    double value = data.GetValue(name);
                    var cdc = new CharDataColum() { Category = name, Series =rowItems[0], Value = value };
                    chartData.Add(cdc);
            }

            ChartColumWithTitleViewModel.TitleBarViewModel.Title = "人均承包面积分析图";
            ChartColumWithTitleViewModel.Rows = rows;// new ObservableCollection<string> { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            ChartColumWithTitleViewModel.RowItems = rowItems;// new ObservableCollection<string> { "人均面积" };
            ChartColumWithTitleViewModel.ChartData = chartData;// Model.Model.GetCharDataColums(ChartColumWithTitleViewModel.Rows, ChartColumWithTitleViewModel.RowItems);
        }
    }
}
