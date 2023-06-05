using System.Collections.ObjectModel;
using Agro.Module.ThemeAnaly.ViewModel.Control;
using System.Collections.Generic;
using Agro.Module.ThemeAnaly.Common;
using Agro.Library.Common;
//using Agro.Library.Exchange;
using Agro.Module.ThemeAnaly.Entity;
using Agro.LibCore;
using Agro.Library.Common.Util;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class LandLevelPageBaseViewModel:ColumChartPageBaseViewModel
    {
        class Data
        {
            /// <summary>
            /// 地域名称集合
            /// </summary>
            internal List<string> ListZoneNames;

            /// <summary>
            /// [地域名称，[承包经营权取得方式，统计项]
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
            /// <summary>
            /// 获取所有地力等级
            /// </summary>
            /// <returns></returns>
            internal List<string> GetDistinctDldj()
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
                    var sqlFormat = @"SELECT Z.MC,T.DLDJ,SUM(T.SCMJ) MJ FROM DLXX_XZDY Z
                        JOIN(
                            SELECT ZE.SJID,T.DLDJ, SUM(T.SCMJ) SCMJ FROM DLXX_XZDY_EXP ZE
                            JOIN(
                                SELECT ZE.SJID, ZE.ZJID,T.DLDJ, T.SCMJ FROM DLXX_XZDY_EXP ZE
                                JOIN(
                                    SELECT R.SZDY,RL.DLDJ DLDJ,ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ
                                    FROM DJ_CBJYQ_DKXX RL
                                    JOIN DJ_CBJYQ_DJB R ON RL.DJBID = R.ID
                                    WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
                                    GROUP BY R.SZDY,RL.DLDJ
                                ) T ON T.SZDY = ZE.ZJID
                                WHERE ZE.SJID = '{1}'
                            ) T ON T.ZJID = ZE.ZJID
                            WHERE ZE.SJJB = {2}
                            GROUP BY ZE.SJID,T.DLDJ
                        ) T ON Z.ID = T.SJID
						GROUP BY Z.MC,T.DLDJ";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT Z.MC,T.DLDJ,SUM(T.SCMJ) MJ FROM DLXX_XZDY Z
                        JOIN(
                                SELECT ZE.SJID, ZE.ZJID,ZE.SJJB,ZE.ZJJB, T.DLDJ, T.SCMJ FROM DLXX_XZDY_EXP ZE
                                JOIN(
                                    SELECT R.SZDY,RL.DLDJ DLDJ,ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ
                                    FROM DJ_CBJYQ_DKXX RL
                                    JOIN DJ_CBJYQ_DJB R ON RL.DJBID = R.ID
                                    WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
                                    GROUP BY R.SZDY,RL.DLDJ
                                ) T ON T.SZDY = ZE.ZJID
                                WHERE ZE.SJID = '{1}'
                        ) T ON Z.ID = T.ZJID
						GROUP BY Z.MC,T.DLDJ";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var n = SafeConvertAux.ToStr(r.GetValue(1));
                    string s="";
                    switch (n)
                    {
                        case "01":s = "一等地";break;
                        case "02": s = "二等地"; break;
                        case "03": s = "三等地"; break;
                        case "04": s = "四等地"; break;
                        case "05": s = "五等地"; break;
                        case "06": s = "六等地"; break;
                        case "07": s = "七等地"; break;
                        case "08": s = "八等地"; break;
                        case "09": s = "九等地"; break;
                        case "10": s = "十等地"; break;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(2));
                    double ov;
                    if (ai.TryGetValue(s, out ov))
                    {
                        v += ov;
                    }
                    ai[s] = v;
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
            ChartColumWithTitleViewModel.TitleBarViewModel.Title = "承包地地力等级结构分析面积对比图";
            var rows = new ObservableCollection<string>();// { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            var rowItems = new ObservableCollection<string>();// { "一等地", "二等地", "三等地", "四等地", "五等地", "六等地", "七等地", "八等地", "九等地", "十等地" };

            var data = new Data(_zone);
            foreach (var zoneName in data.ListZoneNames)
            {
                rows.Add(zoneName);// new ChartValue(zoneName,1000));
            }


            var chartData = new List<CharDataColum>();
            var lstTdyt = data.GetDistinctDldj();
            foreach (var zoneName in rows)
            {
                foreach (var sTdyt in lstTdyt)
                {
                    double value = data.GetValue(zoneName, sTdyt);
                    var cdc = new CharDataColum() { Category = zoneName, Series = sTdyt, Value = value };
                    chartData.Add(cdc);
                }
            }
            foreach(var s in lstTdyt)
            {
                rowItems.Add(s);
            }

            ChartColumWithTitleViewModel.Rows = rows;
            ChartColumWithTitleViewModel.RowItems = rowItems;
            ChartColumWithTitleViewModel.ChartData = chartData;// Model.Model.GetCharDataColums(ChartColumWithTitleViewModel.Rows, ChartColumWithTitleViewModel.RowItems);

            ChartPiesWithTitleBarViewModel.TitleBarViewModel.Title = "承包地地力等级结构分析百分比图";
            foreach (var r in rows)
            {
                ChartPiesWithTitleBarViewModel.ChartPiesViewModel.ChartPieViewModels.Add(new ChartPieViewModel() { Title = r, Rows = chartData });
            }
        }
    }
}
