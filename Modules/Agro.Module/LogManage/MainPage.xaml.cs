/*
 * (C) 2015  公司版权所有,保留所有权利 
 */
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Agro.Module.LogManage
{
    /// <summary>
    /// 日志管理主界面
    /// </summary>
    public partial class MainPage : UserControl
    {

        private readonly List<string[]> _data = new List<string[]>();
        #region Ctor

        /// <summary>
        /// 构造函数:初始化数据字典窗体
        /// </summary>
        public MainPage()
        {
            //SingleInstance = true;
            InitializeComponent();
            lbLogTypes.ItemsSource = new List<string>() { "系统日志", "异常日志", "所有日志" };
            var colLabels = new string[] { "日志类型","用户名","日志时间","日志信息"};
            grid.Cols = colLabels.Length;
            for(int i = 0; i < grid.Cols; ++i)
            {
                grid.SetColLabel(i, colLabels[i]);
                grid.SetColWidth(i, 150);
            }
            grid.SetColWidth(colLabels.Length - 1, 400);
            grid.OnGetCellText = (r, c) =>
            {
                if (r >= 0 && r < _data.Count&&c>=0&&c<grid.Cols)
                {
                    return _data[r][c];
                }
                return "";
            };
            grid.OnActiveRowChanged = (r) =>
            {
                r = grid.ActiveRow;
                if (r >= 0 && r < _data.Count)
                {
                    tbLogInfo.Text = _data[r][grid.Cols - 1];
                }
            };

            lbLogTypes.SelectionChanged += (s, e) =>RefreshGrid();
            btnQuery.Click += (s, e) =>RefreshGrid();

            this.Loaded += (s, e) =>
              {
                  RefreshGrid();
              };
        }

        #endregion

        private void RefreshGrid()
        {
            _data.Clear();
            var sql = "select LOGTYPE,USERNAME,LOGTIME,LOGINFO from CS_LOG";
            string wh = null;
            string logType = null;
            #region 日志类型匹配
            if (lbLogTypes.SelectedValue != null)
            {
                logType = lbLogTypes.SelectedValue.ToString();
                if (logType == "所有日志")
                {
                    logType = null;
                }
            }
            if (logType != null)
            {
                wh = " LOGTYPE='" + logType + "'";
            }
            #endregion

            var sKeywords = tbKeywords.Text.Trim();
            #region 关键字匹配
            if (!string.IsNullOrEmpty(sKeywords))
            {
                sKeywords = sKeywords.Replace("'", "''");
                var sLike = " like '%" + sKeywords + "%' ";
                var s = "(USERNAME " + sLike+ " or LOGINFO " + sLike + ")";
                if (wh == null)
                {
                    wh = s;
                }else
                {
                    wh += " and " + s;
                }
            }
            #endregion

            #region 日期匹配
            var dFrom = dpFromDate.SelectedDate;
            var dTo = dpToDate.SelectedDate;
            if (dFrom != null)
            {
                var s= "LOGTIME>=" + FromDateString((DateTime)dFrom);
                if (wh == null)
                {
                    wh = s;
                }else
                {
                    wh += " and " + s;
                }
            }
            if (dTo != null)
            {
                var s = "LOGTIME<=" + ToDateString((DateTime)dTo);
                if (wh == null)
                {
                    wh = s;
                }
                else
                {
                    wh += " and " + s;
                }
            }
            #endregion

            if (wh != null)
            {
                sql += " where " + wh;
            }
            sql+=" order by LOGTIME desc";
            var db = MyGlobal.Workspace;
           // using (var db = DataBaseSource.GetDatabase())
            {
                int cnt = 0;
                db.QueryCallback(sql, r =>
                {
                    if (++cnt > 10000)
                    {
                        return false;
                    }
                    var sa = new string[4];
                    sa[0] = r.GetString(0);
                    sa[1] = r.GetString(1);
                    sa[2] = r.GetValue(2) + "";
                    sa[3] = r.GetString(3);
                    _data.Add(sa);
                    return true;
                });
            }
            grid.Rows = _data.Count;
            grid.SetActiveRow(-1,false);
            grid.Refresh();
        }
        private static string FromDateString(DateTime dt)
        {
            var s = string.Format("to_timestamp('{0}-{1}-{2} 0:0:0.000000000','yyyy-mm-dd hh24:mi:ss.ff9')",dt.Year,dt.Month,dt.Day);
            return s;
        }
        private static string ToDateString(DateTime dt)
        {
            var s = string.Format("to_timestamp('{0}-{1}-{2} 23:59:59.000000000','yyyy-mm-dd hh24:mi:ss.ff9')", dt.Year, dt.Month, dt.Day);
            return s;
        }

    }
}
