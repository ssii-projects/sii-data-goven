using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Agro.Module.ThemeAnaly.Entity;
using Agro.Module.ThemeAnaly.ViewModel.Control;
//using Agro.Library.Exchange;
using Agro.Library.Common;
using Agro.Module.ThemeAnaly.Common;
using System;
using Agro.LibCore;
using Agro.Library.Common.Util;

namespace Agro.Module.ThemeAnaly.ViewModel.Page
{
    public class DiffrencePageViewModel:ColumChartPageBaseViewModel
    {
        class Data
        {
            /// <summary>
            /// 统计项
            /// </summary>
            class AreaItem
            {
                /// <summary>
                /// 合同面积
                /// </summary>
                public double Htmj;
                /// <summary>
                /// 实测面积
                /// </summary>
                public double Scmj;
                /// <summary>
                /// 增加面积
                /// </summary>
                public double Zjmj;
                /// <summary>
                /// 减少面积
                /// </summary>
                public double Jsmj;
            }
            ///// <summary>
            ///// 统计项，包含【合同面积、实测面积、增加面积、减少面积】
            ///// </summary>
            //internal List<string> ListCountItems;
            /// <summary>
            /// 地域名称集合
            /// </summary>
            internal List<string> ListZoneNames;

            /// <summary>
            /// [地域名称，统计项]
            /// </summary>
            private Dictionary<string, AreaItem> _dicData;
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
                AreaItem ai;
                if (_dicData.TryGetValue(zoneName, out ai))
                {
                    switch (countItemName)
                    {
                        case "实测面积":
                            return ai.Scmj;
                        case "合同面积":
                            return ai.Htmj;
                        case "增加面积":
                            return ai.Zjmj;
                        case "减少面积":
                            return ai.Jsmj;
                    }
                }
                return value;
            }


            private Dictionary<string, AreaItem> QueryValues(IWorkspace db)
            {
                int nYear = Util.QuerLastYearFromDJB(db);
                var lst = new Dictionary<string, AreaItem>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT Z.MC,T.SCMJ,T.HTMJ FROM DLXX_XZDY Z
                        JOIN(
                            SELECT ZE.SJID, SUM(T.SCMJ) SCMJ,SUM(T.HTMJ) HTMJ FROM DLXX_XZDY_EXP ZE
                            JOIN(
                                SELECT ZE.SJID, ZE.ZJID, T.SCMJ,T.HTMJ FROM DLXX_XZDY_EXP ZE
                                JOIN(
                                    SELECT R.SZDY, ROUND(SUM(RL.SCMJ*0.0015),2) SCMJ,SUM(RL.HTMJM) HTMJ
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
                    var sqlFormat = @"SELECT R.MC,SUM(T.SCMJ),SUM(T.HTMJ) FROM (
					SELECT ZE.SJID, ZE.ZJID,ze.SJJB,ze.ZJJB, T.SZDY, T.SCMJ,T.HTMJ FROM 
				(
					SELECT R.SZDY, ROUND(SUM(RL.HTMJ*0.0015),2) HTMJ,SUM(RL.SCMJM) SCMJ
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

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var ai = new AreaItem();
                    ai.Scmj = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai.Htmj = SafeConvertAux.ToDouble(r.GetValue(2));
                    if (ai.Htmj > ai.Scmj)
                    {
                        ai.Jsmj =Math.Round(ai.Htmj - ai.Scmj,2);
                    }else
                    {
                        ai.Zjmj =Math.Round(ai.Scmj - ai.Htmj);
                    }
                    lst[name] = ai;
                    //dic[name] = mj;
                    return true;
                });
                return lst;
            }
        }

        private TablePanelViewModel _tablePanelViewModel = new TablePanelViewModel();
      


        public TablePanelViewModel TablePanelViewModel
        {
            get { return _tablePanelViewModel; }
            set { _tablePanelViewModel = value; OnPropertyChanged(nameof(TablePanelViewModel)); }
        }

       

        private ShortZone _zone;
        public void Init(ShortZone zone)
        {
            _zone = zone;
            InitData();
        }

        //public override void InitData()
        protected override void InitData()
        {
            if (_zone == null)
            {
                return;
            }
            ChartColumWithTitleViewModel.TitleBarViewModel.Title = "差异面积对比图";
            TitleBarViewModel.Title = "差异面积对比表";

            var rows = new ObservableCollection<string>();// { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            var rowItems = new ObservableCollection<string> { "合同面积", "实测面积", "增加面积", "减少面积" };// "2016年", "2017年", "2018年", "2019年", "2020年", "2021年" };

            var data = new Data(_zone);

            foreach(var zoneName in data.ListZoneNames)
            {
                rows.Add(zoneName);
            }

            var chartData = new List<CharDataColum>();
            foreach (var zoneName in rows)
            {
                foreach (var countItem in rowItems)
                {
                    double value = data.GetValue(zoneName, countItem);
                    var cdc = new CharDataColum() { Category = zoneName, Series = countItem, Value = value };
                    chartData.Add(cdc);
                }
            }



            ChartColumWithTitleViewModel.Rows = rows;// new ObservableCollection<string> { "锦江区", "武侯区", "双流区", "陈华区", "青羊区", "金牛区", "高新区" };
            ChartColumWithTitleViewModel.RowItems = rowItems;// new ObservableCollection<string> { "合同面积", "实测", "增加面积", "减少面积" };
            ChartColumWithTitleViewModel.ChartData = chartData;// Model.Model.GetCharDataColums(ChartColumWithTitleViewModel.Rows, ChartColumWithTitleViewModel.RowItems);

            TablePanelViewModel.TableViewModel.Colums = rows.ToList();// rowItems.ToList();
            TablePanelViewModel.TitleBarViewModel.Title = "差异面积对比表";
            var gridDatas = new List<GridDataDynamicObject>();
            foreach (var r in rowItems)
            {
                var gridData = new GridDataDynamicObject();
                gridData._properties["Header"] = r;
                foreach(var zoneName in rows)
                {
                    gridData._properties["Value" + zoneName] = data.GetValue(zoneName, r);
                }
                //dynamic gridData = new GridDataDynamicObject();
                //gridData.Header = r;
                //gridData.Value2016年 = 2000;
                //gridData.Value2017年 = 2000;
                //gridData.Value2018年 = 2000;
                //gridData.Value2019年 = 2000;
                //gridData.Value2020年 = 2000;
                //gridData.Value2021年 = 2000;
                gridDatas.Add(gridData);
            }
            TablePanelViewModel.TableViewModel.GridDataDynamicObjects = gridDatas;


        }
    }
}
