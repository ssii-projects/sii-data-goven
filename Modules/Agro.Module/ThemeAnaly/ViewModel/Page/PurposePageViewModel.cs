using System.Collections.ObjectModel;
using Agro.Module.ThemeAnaly.ViewModel.Control;
using Agro.Module.ThemeAnaly.Common;
using System.Collections.Generic;
using Agro.Library.Common;
using Agro.Module.ThemeAnaly.View.Control;
using Agro.Module.ThemeAnaly.Entity;
using Agro.Library.Model;
using Agro.LibCore;
using Agro.Library.Common.Util;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class PurposePageViewModel:ColumChartPageBaseViewModel
    {
        class Data
        {
            ///// <summary>
            ///// 统计项
            ///// </summary>
            //class AreaItem
            //{
            //    /// <summary>
            //    /// 合同面积
            //    /// </summary>
            //    public double Htmj;
            //    /// <summary>
            //    /// 实测面积
            //    /// </summary>
            //    public double Scmj;
            //    /// <summary>
            //    /// 增加面积
            //    /// </summary>
            //    public double Zjmj;
            //    /// <summary>
            //    /// 减少面积
            //    /// </summary>
            //    public double Jsmj;
            //}
            ///// <summary>
            ///// 统计项，包含【合同面积、实测面积、增加面积、减少面积】
            ///// </summary>
            //internal List<string> ListCountItems;
            /// <summary>
            /// 地域名称集合
            /// </summary>
            internal List<string> ListZoneNames;

            /// <summary>
            /// [地域名称，[土地用途，统计项]
            /// </summary>
            private Dictionary<string, Dictionary<string,double>> _dicData;
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
                Dictionary<string,double> ai;
                if (_dicData.TryGetValue(zoneName, out ai))
                {
                    if(!ai.TryGetValue(countItemName,out value))
                    {
                        value = 0;
                    }
                    //switch (countItemName)
                    //{
                    //    case "实测面积":
                    //        return ai.Scmj;
                    //    case "合同面积":
                    //        return ai.Htmj;
                    //    case "增加面积":
                    //        return ai.Zjmj;
                    //    case "减少面积":
                    //        return ai.Jsmj;
                    //}
                }
                return value;
            }
            //internal Dictionary<string,double> GetValue(string zoneName)
            //{
            //    Dictionary<string, double> dic=null;
            //    if(_dicData.TryGetValue(zoneName,out dic))
            //    {
            //        return dic;
            //    }
            //    return null;
            //}
            internal List<string> GetDistinctTdyt()
            {
                var lst = new List<string>();
                foreach(var kv in _dicData.Values)
                {
                    foreach(var s in kv.Keys)
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
                    var sqlFormat = @"SELECT Z.MC,T.TDYT,SUM(T.SCMJ) MJ FROM DLXX_XZDY Z
                        JOIN(
                            SELECT ZE.SJID,T.TDYT, SUM(T.SCMJ) SCMJ FROM DLXX_XZDY_EXP ZE
                            JOIN(
                                SELECT ZE.SJID, ZE.ZJID,T.TDYT, T.SCMJ FROM DLXX_XZDY_EXP ZE
                                JOIN(
                                    SELECT R.SZDY,RL.TDYT TDYT, ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ
                                    FROM DJ_CBJYQ_DKXX RL
                                    JOIN DJ_CBJYQ_DJB R ON RL.DJBID = R.ID
                                    WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
                                    GROUP BY R.SZDY,RL.TDYT
                                ) T ON T.SZDY = ZE.ZJID
                                WHERE ZE.SJID = '{1}'
                            ) T ON T.ZJID = ZE.ZJID
                            WHERE ZE.SJJB = {2}
                            GROUP BY ZE.SJID,T.TDYT
                        ) T ON Z.ID = T.SJID
						GROUP BY Z.MC,T.TDYT";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT Z.MC,T.TDYT,SUM(T.SCMJ) MJ FROM DLXX_XZDY Z
                        JOIN(
                                SELECT ZE.SJID, ZE.ZJID,ZE.SJJB,ZE.ZJJB, T.TDYT, T.SCMJ FROM DLXX_XZDY_EXP ZE
                                JOIN(
                                    SELECT R.SZDY,RL.TDYT TDYT,ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ
                                    FROM DJ_CBJYQ_DKXX RL
                                    JOIN DJ_CBJYQ_DJB R ON RL.DJBID = R.ID
                                    WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
                                    GROUP BY R.SZDY,RL.TDYT
                                ) T ON T.SZDY = ZE.ZJID
                                WHERE ZE.SJID = '{1}'
                        ) T ON Z.ID = T.ZJID
						GROUP BY Z.MC,T.TDYT";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if(!lst.TryGetValue(name,out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    //ai.Scmj = SafeConvertAux.ToDouble(r.GetValue(1));
                    //ai.Htmj = SafeConvertAux.ToDouble(r.GetValue(2));
                    var n = SafeConvertAux.ToStr(r.GetValue(1));
                    string s = "";
                    switch (n)
                    {
                        case eTdytCode.Zzy: s = "种植业"; break;
                        case eTdytCode.Ly:s = "林业";break;
                        case eTdytCode.Xmy:s = "畜牧业";break;
                        case eTdytCode.Ye: s = "渔业"; break;
                        case eTdytCode.Fnyt: s = "非农用途"; break;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(2));
                    ai[s] = v;
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
        // public PurposePageViewModel()
        protected override void InitData()
        {
            if (_zone == null)
            {
                return;
            }
            var data = new Data(_zone);

            ChartColumWithTitleViewModel.TitleBarViewModel.Title = "承包地用途结构面积对比图";
            var rows = new ObservableCollection<string>();// { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            var rowItems= new ObservableCollection<string> { /*"农业用途",*/ "种植业", "林业", "畜牧业", "渔业", "非农业用途" };

            foreach(var zoneName in data.ListZoneNames)
            {
                rows.Add(zoneName);// new ChartValue(zoneName,1000));
            }

            var chartData = new List<CharDataColum>();
            var lstTdyt = data.GetDistinctTdyt();
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
            ChartColumWithTitleViewModel.ChartData = chartData;// Model.Model.GetCharDataColums(ChartColumWithTitleViewModel.Rows, ChartColumWithTitleViewModel.RowItems);

            ChartPiesWithTitleBarViewModel.TitleBarViewModel.Title = "承包地用途结构百分比对比图";


            foreach (var r in rows)
            {
                ChartPiesWithTitleBarViewModel.ChartPiesViewModel.ChartPieViewModels.Add(new ChartPieViewModel() { Title = r, Rows = chartData });// datas });
            }

        }

    }
}
