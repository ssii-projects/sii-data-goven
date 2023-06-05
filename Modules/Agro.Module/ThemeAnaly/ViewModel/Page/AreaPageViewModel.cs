using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using Agro.Module.ThemeAnaly.Entity;
using Agro.Module.ThemeAnaly.Enum;
using Agro.Module.ThemeAnaly.ViewModel.Control;
using System.Data;
using Agro.Library.Common;
//using Agro.Library.Exchange;
using Agro.Library.Common.Util;
using Agro.Module.ThemeAnaly.Common;
using Agro.LibCore;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class AreaPageViewModel:ColumChartPageBaseViewModel
    {
        class Data
        {
            /// <summary>
            /// 统计年度集合
            /// </summary>
            internal List<int> ListYears;
            /// <summary>
            /// 地域名称集合
            /// </summary>
            internal List<string> ListZoneNames;
            /// <summary>
            /// [年度,[地域名称,面积]]
            /// </summary>
            private Dictionary<int, Dictionary<string, double>> _dicData;
            private ShortZone _zone;
            /// <summary>
            /// 要统计的地域级别
            /// </summary>
            private int _nDyJb=3;
            internal Data(ShortZone zone)
            {
                _zone = zone;
                _nDyJb = ((int)zone.Level) - 1;
                var db = MyGlobal.Workspace;
                //using (var db = DataBaseSource.GetDatabase())
                {
                    ListYears = Util.QueryDistinctYearsFromDJB(db);
                    ListZoneNames =Util.QueryChildZoneNames(db,_zone);
                    _dicData = QueryValues(db, ListYears);
                }
            }
            internal double GetValue(string name,int nYear)
            {
                double value = 0;
                Dictionary<string, double> dv;
                if (_dicData.TryGetValue(nYear, out dv))
                {
                    if (!dv.TryGetValue(name, out value))
                    {
                        value = 0;
                    }
                }
                return value;
            }


            private Dictionary<int, Dictionary<string, double>> QueryValues(IWorkspace db, List<int> lstYears)
            {
                var lst = new Dictionary<int, Dictionary<string, double>>();
                foreach (var nYear in lstYears)
                {

                    string sql = null;
                    if (_nDyJb > 1)
                    {
                        #region 按组以上地域级别的统计
                        var sqlFormat = @"SELECT Z.MC,T.SCMJ FROM DLXX_XZDY Z
                        JOIN(
                            SELECT ZE.SJID, SUM(T.SCMJ) SCMJ FROM DLXX_XZDY_EXP ZE
                            JOIN(
                                SELECT ZE.SJID, ZE.ZJID, T.SCMJ FROM DLXX_XZDY_EXP ZE
                                JOIN(
                                    SELECT R.SZDY, ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ
                                    FROM DJ_CBJYQ_DKXX RL
                                    JOIN DJ_CBJYQ_DJB R ON RL.DJBID = R.ID
                                    WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
                                    GROUP BY R.SZDY
                                ) T ON T.SZDY = ZE.ZJID
                                WHERE ZE.SJID = '{1}'
                            ) T ON T.ZJID = ZE.ZJID
                            WHERE ZE.SJJB = {2}
                            GROUP BY ZE.SJID
                        ) T ON Z.ID = T.SJID";
                        sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                        #endregion
                    }
                    else
                    {
                        #region 按组进行统计
                        var sqlFormat = @"SELECT R.MC,SUM(T.SCMJ) FROM (
					SELECT ZE.SJID, ZE.ZJID,ze.SJJB,ze.ZJJB, T.SZDY, T.SCMJ FROM 
				(
					SELECT R.SZDY,ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ
					FROM DJ_CBJYQ_DKXX RL
					JOIN DJ_CBJYQ_DJB R ON RL.DJBID = R.ID
					WHERE (R.QSZT=1 OR (R.QSZT=2 AND "+db.SqlFunc.Year("R.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("R.DJSJ")+@"<={0}
					GROUP BY R.SZDY
				) T 
				LEFT JOIN DLXX_XZDY_EXP ZE ON T.SZDY = ZE.ZJID where ze.SJID='{1}' 
			) T
			LEFT JOIN DLXX_XZDY R ON R.ID=T.ZJID
			GROUP BY R.MC";
                        sql = string.Format(sqlFormat, nYear, _zone.ID);
                        #endregion
                    }

                    var dic = new Dictionary<string, double>();
                    lst[nYear] = dic;
                    db.QueryCallback(sql, r =>
                    {
                        var name = r.GetString(0);
                        var mj = SafeConvertAux.ToDouble(r.GetValue(1));
                        dic[name] = mj;
                        return true;
                    });
                }
                return lst;
            }
        }
        private TablePanelViewModel _tablePanelViewModel=new TablePanelViewModel();

       
        public TablePanelViewModel TablePanelViewModel
        {
            get { return _tablePanelViewModel; }
            set { _tablePanelViewModel = value; OnPropertyChanged(nameof(TablePanelViewModel));}
        }

        public ObservableCollection<string> AreaAnalyseType =>
           new ObservableCollection<string>(System.Enum.GetNames(typeof(AreaAnalyseTypeEnum)));

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
            ChartColumWithTitleViewModel.TitleBarViewModel.Title = "面积对比图";
            TitleBarViewModel.Title = "面积对比表";

            var rows = new ObservableCollection<string>();// { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            var rowItems = new ObservableCollection<string>();// { "2016年", "2017年", "2018年", "2019年", "2020年", "2021年" };


            var data = new Data(_zone);
            foreach(var name in data.ListZoneNames)
            {
                rows.Add(name);
            }
            foreach(var nYear in data.ListYears)
            {
                rowItems.Add(nYear + "年");
            }

            var chartData = new List<CharDataColum>();
            foreach (var name in rows)
            {
                foreach(var nYear in data.ListYears)
                {
                    double value = data.GetValue(name,nYear);
                    var cdc = new CharDataColum() { Category = name, Series = nYear + "年" ,Value=value};
                    chartData.Add(cdc);
                }
            }

            ChartColumWithTitleViewModel.Rows = rows;
            ChartColumWithTitleViewModel.RowItems = rowItems;
            ChartColumWithTitleViewModel.ChartData = chartData;// Model.Model.GetCharDataColums(ChartColumWithTitleViewModel.Rows,ChartColumWithTitleViewModel.RowItems);

            TablePanelViewModel.TableViewModel.Colums = rowItems.ToList();
            TablePanelViewModel.TitleBarViewModel.Title = "面积对比表";
            var gridDatas = new List<GridDataDynamicObject>();
            foreach (var r in rows)
            {
                var gridData = new GridDataDynamicObject();
                gridData._properties["Header"] = r;
                foreach (var nYear in data.ListYears)
                {
                    gridData._properties["Value" + nYear + "年"] = data.GetValue(r, nYear);
                }

                /*
                dynamic gridData = new GridDataDynamicObject();
                gridData.Header = r;
                gridData.Value2016年 =1000;
                gridData.Value2017年 = 2000;
                gridData.Value2018年 = 2000;
                gridData.Value2019年 = 2000;
                gridData.Value2020年 = 2000; 
                gridData.Value2021年 = 2000;
                */
                gridDatas.Add(gridData);
            }
            TablePanelViewModel.TableViewModel.GridDataDynamicObjects = gridDatas;
        }
        //private DataTable QueryData()
        //{
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("地域名称");
        //    using(var db= DataBaseSource.GetDatabase()){
        //        var sql = "SELECT DISTINCT YEAR(DJSJ) FROM DJ_CBJYQ_DJB";
        //        db.QueryCallback(sql, r =>
        //        {
        //            if (!r.IsDBNull(0))
        //            {
        //                var sYear = r.GetValue(0).ToString();
        //                dt.Columns.Add(sYear);
        //            }
        //            return true;
        //        });
        //        for(int i = 1; i < dt.Columns.Count; ++i)
        //        {

        //        }
        //    }
        //    return dt;
        //}

        ///// <summary>
        ///// 查询最近5年的年份
        ///// </summary>
        ///// <param name="db"></param>
        ///// <returns></returns>
        //private List<int> QueryDistinctYears(IDatabase db)
        //{
        //    var lst = new List<int>();
        //    var sql = "SELECT DISTINCT YEAR(DJSJ) FROM DJ_CBJYQ_DJB";
        //    db.QueryCallback(sql, r =>
        //    {
        //        if (!r.IsDBNull(0))
        //        {
        //            var sYear = r.GetValue(0).ToString();
        //            lst.Add(SafeConvertAux.ToInt32(sYear));
        //        }
        //        return true;
        //    });
        //    lst.Sort();
        //    while(lst.Count > 5)
        //    {
        //        lst.RemoveAt(0);
        //    }
        //    return lst;
        //} 

        //private List<string> QueryDynames(IDatabase db)
        //{
        //    var lst = new List<string>();
        //    var sql = "SELECT DISTINCT MC FROM DLXX_XZDY WHERE JB=3 AND MC IS NOT NULL ORDER BY MC";
        //    db.QueryCallback(sql, r =>
        //    {
        //        if (!r.IsDBNull(0))
        //        {
        //            var sMC = r.GetValue(0).ToString();
        //            lst.Add(sMC);
        //        }
        //        return true;
        //    });
        //    return lst;
        //}

        //private Dictionary<int,Dictionary<string,double>> QueryMjs(IDatabase db,List<int> lstYears)
        //{
        //    var lst = new Dictionary<int, Dictionary<string, double>>();
        //    foreach(var nYear in lstYears)
        //    {
        //        var sql = @"SELECT N.MC,M.MJ FROM  (select ZE.SJID, SUM(T.ZMJ) MJ from (SELECT R.SZDY, SUM(RL.SCMJ) ZMJ FROM DJ_CBJYQ_DKXX RL";
        //        sql += " JOIN DJ_CBJYQ_DJB R ON RL.DJBID=R.ID";
        //        sql += " WHERE (R.QSZT=1 OR (R.QSZT=2 AND YEAR(R.ZHXGSJ)>" + nYear + ")) AND YEAR(R.DJSJ)<=" + nYear;
        //        sql += " GROUP BY R.SZDY) T left join DLXX_XZDY_EXP ZE ON ZE.ZJID=T.SZDY WHERE ZE.ZJID=T.SZDY AND ZE.SJJB=3 GROUP BY ZE.SJID) M";
        //        sql += " LEFT JOIN DLXX_XZDY N ON M.SJID=N.ID";
        //        var dic = new Dictionary<string, double>();
        //        lst[nYear]=dic;
        //        db.QueryCallback(sql, r =>
        //        {
        //            var name = r.GetString(0);
        //            var mj =SafeConvertAux.ToDouble(r.GetValue(1));
        //            dic[name] = mj;
        //            return true;
        //        });
        //    }
        //    return lst;
        //}
    }
}

