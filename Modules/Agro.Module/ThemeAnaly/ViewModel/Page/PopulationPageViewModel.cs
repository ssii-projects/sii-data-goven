using System.Collections.ObjectModel;
using Agro.Module.ThemeAnaly.ViewModel.Control;
using System.Collections.Generic;
using Agro.Library.Common;
using Agro.Module.ThemeAnaly.Common;
using Agro.Module.ThemeAnaly.Entity;
using Agro.LibCore;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class PopulationPageViewModel :ColumChartPageBaseViewModel
    {
        class Data
        {
            internal List<string> ListZoneNames;

            /// <summary>
            /// [地域名称，[成员备注，统计项]
            /// </summary>
            private Dictionary<string, Dictionary<string, double>> _dicData;
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
            internal double GetValue(string zoneName, string countItemName)
            {
                double value = 0;
                Dictionary<string, double> ai;
                if (_dicData.TryGetValue(zoneName, out ai))
                {
                    if (!ai.TryGetValue(countItemName, out value))
                    {
                        value = 0;
                    }
                }
                return value;
            }
            internal List<string> GetDistinctCybz()
            {
                var lst = new List<string>();
                foreach (var kv in _dicData.Values)
                {
                    foreach (var s in kv.Keys)
                    {
                        if (!lst.Contains(s))
                        {
                            lst.Add(s);
                        }
                    }
                }
                return lst;
            }
            private Dictionary<string, Dictionary<string, double>> QueryValues(IWorkspace db)
            {
                int nYear = Util.QuerLastYearFromDJB(db);
                var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT D.MC,T.CYBZ,T.CNT
FROM DLXX_XZDY D
JOIN (
	SELECT DE.SJID,T.CYBZ,COUNT(*) CNT 
	FROM (
		SELECT DE.SJID,DE.SJJB,DE.ZJID,DE.ZJJB,T.CYBZ
		FROM (
			SELECT R.SZDY,J.CYBZ
			FROM  DJ_CBJYQ_CBF_JTCY J
			JOIN  DJ_CBJYQ_CBF T ON T.ID=J.CBFID
			JOIN DJ_CBJYQ_DJB R ON R.ID=T.DJBID
			WHERE J.CYBZ IS NOT NULL AND (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
		) T
		LEFT JOIN DLXX_XZDY_EXP DE ON DE.ZJID=T.SZDY
		WHERE DE.SJID='{1}'
	) T
	JOIN DLXX_XZDY_EXP DE ON DE.ZJID=T.ZJID 
	WHERE DE.SJJB={2} AND T.CYBZ IS NOT NULL
	GROUP BY DE.SJID,T.CYBZ
) T ON D.ID=T.SJID";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT D.MC,T.CYBZ,T.CNT
FROM DLXX_XZDY D
JOIN (
	SELECT T.SZDY,T.CYBZ,COUNT(*) CNT 
	FROM (
			SELECT R.SZDY,J.CYBZ
			FROM  DJ_CBJYQ_CBF_JTCY J
			JOIN  DJ_CBJYQ_CBF T ON T.ID=J.CBFID
			JOIN DJ_CBJYQ_DJB R ON R.ID=T.DJBID
			JOIN DLXX_XZDY_EXP DE ON R.SZDY=DE.ZJID
			WHERE J.CYBZ IS NOT NULL AND (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0} AND DE.SJID='{1}'
		) T
	GROUP BY T.SZDY,T.CYBZ
)T ON D.ID=T.SZDY";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        ai["总人口"] = 0;
                        lst[name] = ai;
                    }
                    //ai.Scmj = SafeConvertAux.ToDouble(r.GetValue(1));
                    //ai.Htmj = SafeConvertAux.ToDouble(r.GetValue(2));
                    var n = SafeConvertAux.ToStr(r.GetValue(1));
                    string s = "";
                    switch (n)
                    {
                        case "1": s = "外嫁女"; break;
                        case "2": s = "入赘男"; break;
                        case "3": s = "在校大学生"; break;
                        case "4": s = "国家公职人员"; break;
                        case "5": s = "军人（军官、士兵）"; break;
                        case "6":s = "新生儿"; break;
                        case "7": s = "去世"; break;
                        //case "9": s = "其他备注"; break;
                        default:s = "其他";break;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(2));
                    ai[s] = v;
                    ai["总人口"] = ai["总人口"] + v;
                    //lst[name] = ai;
                    //dic[name] = mj;
                    return true;
                });
                return lst;
            }
        }

        private ChartPiesWithTitleBarViewModel _chartPiesWithTitleBarViewModel = new ChartPiesWithTitleBarViewModel();
        public ChartPiesWithTitleBarViewModel ChartPiesWithTitleBarViewModel
        {
            get { return _chartPiesWithTitleBarViewModel; }
            set { _chartPiesWithTitleBarViewModel = value; OnPropertyChanged(nameof(ChartPiesWithTitleBarViewModel)); }
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
            ChartColumWithTitleViewModel.TitleBarViewModel.Title = "农业人口空间分布情况分析图";
            TitleBarViewModel.Title = "农业人口空间分布情况百分比分析图";

            var rows = new ObservableCollection<string>();// { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            var rowItems = new ObservableCollection<string>();// { "总人口数", "外嫁女", "入赘男", "在校大学生", "国家公职人员", "军人（军官、士兵）", "新生儿", "去世" };

            var data = new Data(_zone);
            foreach (var zoneName in data.ListZoneNames)
            {
                rows.Add(zoneName);// new ChartValue(zoneName,1000));
            }

            var chartData = new List<CharDataColum>();
            var lstTdyt = data.GetDistinctCybz();
            foreach(var s in lstTdyt)
            {
                rowItems.Add(s);
            }

            foreach (var zoneName in rows)
            {
                foreach (var sTdyt in lstTdyt)
                {
                    double value = data.GetValue(zoneName, sTdyt);
                    var cdc = new CharDataColum() { Category = zoneName, Series = sTdyt, Value = value };
                    chartData.Add(cdc);
                }
            }


            ChartColumWithTitleViewModel.Rows = rows;
            ChartColumWithTitleViewModel.RowItems = rowItems;
            ChartColumWithTitleViewModel.ChartData =chartData;// Model.Model.GetCharDataColums(ChartColumWithTitleViewModel.Rows, ChartColumWithTitleViewModel.RowItems);

            ChartPiesWithTitleBarViewModel.TitleBarViewModel.Title = "农业人口空间分布情况百分比分析图";

            var chartData1 = new List<CharDataColum>();
            foreach(var c in chartData)
            {
                if (c.Series != "总人口")
                {
                    chartData1.Add(c);
                }
            }
            foreach (var r in rows)
            {
                ChartPiesWithTitleBarViewModel.ChartPiesViewModel.ChartPieViewModels.Add(new ChartPieViewModel() { Title = r, Rows = chartData1 });
            }

        }

    }
}
