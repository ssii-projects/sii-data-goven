using Agro.Library.Common;
using Agro.Module.ThemeAnaly.Common;
using Agro.LibCore;
using Agro.LibCore.NPIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Agro.Module.ThemeAnaly.View.Page
{
	/// <summary>
	/// 实时数据对比分析表
	/// </summary>
	public partial class RealTimeTablePage : UserControl,IThemePage
    {
        class Data
        {
            /// <summary>
            /// 地域名称集合
            /// </summary>
            internal List<string> ListZoneNames;

            /// <summary>
            /// [地域名称，[统计项，统计值]
            /// </summary>
            private Dictionary<string, Dictionary<string, double>> _dicData=new Dictionary<string, Dictionary<string, double>>();
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
                    int nYear = Util.QuerLastYearFromDJB(db);
                    InitFbfValues(db);
                    InitCbfValues(db,nYear);
                    InitJtcyValues(db, nYear);
                    InitDkValues(db, nYear);
                    InitCbhtValues(db, nYear);
                    InitZmjValues(db, nYear);
                    InitDjbValues(db, nYear);
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
            private void InitFbfValues(IWorkspace db)
            {
                //int nYear = Util.QuerLastYearFromDJB(db);
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT T.MC,R.CNT 
FROM DLXX_XZDY T
JOIN (
	SELECT DE.SJID,COUNT(1) CNT
	FROM DLXX_XZDY_EXP DE
	JOIN(SELECT DE.ZJID
		FROM QSSJ_FBF F
		JOIN DLXX_XZDY_EXP DE ON DE.ZJID=F.SZDY
		WHERE DE.SJID='{0}'
		)T ON DE.ZJID=T.ZJID
	WHERE DE.SJJB={1}
	GROUP BY DE.SJID
	) R ON R.SJID=T.ID";
                    sql = string.Format(sqlFormat, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT T.MC,R.CNT
FROM DLXX_XZDY T
JOIN(
	SELECT DE.ZJID, COUNT(*) CNT
	FROM QSSJ_FBF F
	JOIN DLXX_XZDY_EXP DE ON DE.ZJID=F.SZDY
	WHERE DE.SJID='{0}'
	GROUP BY DE.ZJID
	)R ON R.ZJID=T.ID";
                    sql = string.Format(sqlFormat, _zone.ID);
                    #endregion
                }

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["发包方"] = v;
                    return true;
                });
            }

            private void InitCbfValues(IWorkspace db, int nYear)
            {
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT D.MC,T.CNT
FROM DLXX_XZDY D
JOIN(
	SELECT DE.SJID,COUNT(1) CNT
	FROM DLXX_XZDY_EXP DE
	JOIN(
		SELECT D.SZDY
		FROM DJ_CBJYQ_CBF C
		JOIN DJ_CBJYQ_DJB D ON C.DJBID=D.ID
		JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
		WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
		)T ON T.SZDY=DE.ZJID
	WHERE DE.SJJB={2}
	GROUP BY DE.SJID
	)T ON D.ID=T.SJID
";
                    sql = string.Format(sqlFormat,nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT D.MC,T.CNT
FROM DLXX_XZDY D
JOIN(
	SELECT D.SZDY,COUNT(1) CNT 
	FROM DJ_CBJYQ_CBF C
	JOIN DJ_CBJYQ_DJB D ON C.DJBID=D.ID
	JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
	WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND  DE.SJID='{1}'
	GROUP BY D.SZDY
	)T ON D.ID=T.SZDY
";
                    sql = string.Format(sqlFormat,nYear, _zone.ID);
                    #endregion
                }

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["承包方"] = v;
                    return true;
                });
            }

            private void InitJtcyValues(IWorkspace db, int nYear)
            {
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT D.MC,T.CNT
FROM DLXX_XZDY D
JOIN(
	SELECT DE.SJID,SUM(T.CNT) CNT
	FROM DLXX_XZDY_EXP DE
	JOIN(
		SELECT D.SZDY,SUM(C.CBFCYSL) CNT
		FROM DJ_CBJYQ_CBF C
		JOIN DJ_CBJYQ_DJB D ON C.DJBID=D.ID
		JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
		WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                   sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
		GROUP BY D.SZDY
		)T ON T.SZDY=DE.ZJID
	WHERE DE.SJJB={2}
	GROUP BY DE.SJID
	)T ON D.ID=T.SJID";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT D.MC,T.CNT
FROM DLXX_XZDY D
JOIN(
	SELECT D.SZDY,SUM(C.CBFCYSL) CNT
	FROM DJ_CBJYQ_CBF C
	JOIN DJ_CBJYQ_DJB D ON C.DJBID=D.ID
	JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
	WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                   sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND  DE.SJID='{1}'
	GROUP BY D.SZDY
	)T ON D.ID=T.SZDY";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["家庭成员"] = v;
                    return true;
                });
            }

            private void InitDkValues(IWorkspace db, int nYear)
            {
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT D.MC,COUNT(1) CNT
FROM DLXX_XZDY_EXP DE
JOIN(
	SELECT D.SZDY 
	FROM DJ_CBJYQ_DKXX K
	JOIN DJ_CBJYQ_DJB  D ON K.DJBID=D.ID
	JOIN DLXX_XZDY_EXP DE ON D.SZDY=DE.ZJID
	WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
	)T ON T.SZDY=DE.ZJID
JOIN DLXX_XZDY D ON D.ID=DE.SJID 
WHERE DE.SJJB={2}
GROUP BY D.MC";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT X.MC,COUNT(1) CNT
	FROM DJ_CBJYQ_DKXX K
	JOIN DJ_CBJYQ_DJB  D ON K.DJBID=D.ID
	JOIN DLXX_XZDY_EXP DE ON D.SZDY=DE.ZJID
	JOIN DLXX_XZDY X ON D.SZDY=X.ID
	WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
	GROUP BY X.MC";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                //var dic = new Dictionary<string, double>();
                //lst[nYear] = dic;
                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;// = new Dictionary<string, double>();
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["地块"] = v;
                    return true;
                });
            }

            private void InitCbhtValues(IWorkspace db, int nYear)
            {
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT X.MC,T.CNT
FROM DLXX_XZDY X
JOIN(
	SELECT DE.SJID,COUNT(1) CNT
	FROM DLXX_XZDY_EXP DE
	JOIN(
		SELECT D.SZDY
		FROM DJ_CBJYQ_CBHT H
		JOIN DJ_CBJYQ_DJB D ON H.DJBID=D.ID
		JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
		WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
		)T ON DE.ZJID=T.SZDY
	WHERE DE.SJJB={2}
	GROUP BY DE.SJID
	)T ON X.ID=T.SJID";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT X.MC,COUNT(1) CNT
FROM DJ_CBJYQ_CBHT H
JOIN DJ_CBJYQ_DJB D ON H.DJBID=D.ID
JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
JOIN DLXX_XZDY X ON X.ID=D.SZDY
WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0}  AND DE.SJID='{1}'
GROUP BY X.MC";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["承包合同"] = v;
                    return true;
                });
            }

            private void InitZmjValues(IWorkspace db, int nYear)
            {
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT D.MC,SUM(T.HTMJM) MJ
FROM DLXX_XZDY_EXP DE
JOIN(
	  SELECT D.SZDY,X.HTMJM
	  FROM DJ_CBJYQ_DKXX X
	  JOIN DJ_CBJYQ_DJB D ON X.DJBID=D.ID
	  JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
	  WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
	)T ON T.SZDY=DE.ZJID
JOIN DLXX_XZDY D ON D.ID=DE.SJID
WHERE DE.SJJB={2}
GROUP BY D.MC";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"SELECT XD.MC,SUM(X.HTMJM) MJ
FROM DJ_CBJYQ_DKXX X
JOIN DJ_CBJYQ_DJB D ON X.DJBID=D.ID
JOIN DLXX_XZDY_EXP DE ON DE.ZJID=D.SZDY
JOIN DLXX_XZDY XD ON XD.ID=D.SZDY
WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
GROUP BY XD.MC";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["总面积"] = v;
                    return true;
                });
            }

            private void InitDjbValues(IWorkspace db, int nYear)
            {
                //var lst = new Dictionary<string, Dictionary<string, double>>();
                string sql = null;
                if (_nDyJb > 1)
                {
                    #region 按组以上地域级别的统计
                    var sqlFormat = @"SELECT X.MC,COUNT(1) CNT
FROM DLXX_XZDY_EXP DE
JOIN(
	SELECT D.SZDY
	FROM DJ_CBJYQ_DJB D 
	JOIN DLXX_XZDY_EXP DE ON D.SZDY=DE.ZJID
	WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                    sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
	)T ON T.SZDY=DE.ZJID
JOIN DLXX_XZDY X ON DE.SJID=X.ID
WHERE DE.SJJB={2}
GROUP BY X.MC";
                    sql = string.Format(sqlFormat, nYear, _zone.ID, _nDyJb);
                    #endregion
                }
                else
                {
                    #region 按组进行统计
                    var sqlFormat = @"	SELECT X.MC,COUNT(1) CNT
	FROM DJ_CBJYQ_DJB D 
	JOIN DLXX_XZDY_EXP DE ON D.SZDY=DE.ZJID
	JOIN DLXX_XZDY X ON X.ID=D.SZDY
	WHERE (D.QSZT=1 OR (D.QSZT=2 AND ";
                   sqlFormat+=db.SqlFunc.Year("D.ZHXGSJ")+">{0})) AND "+db.SqlFunc.Year("D.DJSJ")+@"<={0} AND DE.SJID='{1}'
	GROUP BY X.MC
";
                    sql = string.Format(sqlFormat, nYear, _zone.ID);
                    #endregion
                }

                db.QueryCallback(sql, r =>
                {
                    var name = r.GetString(0);
                    var lst = _dicData;
                    Dictionary<string, double> ai;
                    if (!lst.TryGetValue(name, out ai))
                    {
                        ai = new Dictionary<string, double>();
                        lst[name] = ai;
                    }
                    var v = SafeConvertAux.ToDouble(r.GetValue(1));
                    ai["登记簿/权证"] = v;
                    return true;
                });
            }
        }

        public string Title { get { return "实时数据对比分析表"; } }
        public RealTimeTablePage()
        {
            InitializeComponent();
            btnExport.Click += (s, e) =>
            {
                //ExportExcel()
                var ofd = new SaveFileDialog();
                ofd.Filter = "Excel文件(*.xls)|*.xls";
                ofd.OverwritePrompt = true;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() != true)
                    return;
                try
                {
                    var file = ofd.FileName;
                    ExportExcel(file);
                    Win32.ShellExecute(IntPtr.Zero, "open", file, null, null, ShowCommands.SW_SHOWNORMAL);
                }
                catch (Exception ex)
                {
                    UIHelper.ShowExceptionMessage(ex);
                }
            };
        }
        public void Init(ShortZone zone)
        {
            var clms = new string[] {"", "发包方","承包方","家庭成员","地块","承包合同", "登记簿/权证","总面积" };
            grid.Cols = clms.Length;
            for(int c = 0; c < clms.Length; ++c)
            {
                grid.SetColLabel(c,clms[c]);
            }
            var data = new Data(zone);
            grid.Rows = data.ListZoneNames.Count;
            grid.OnGetCellText += (r, c) =>
            {
                switch (c)
                {
                    case 0:
                        return data.ListZoneNames[r];
                    //case 1:
                    //case 2:
                    //case 3:
                    //case 4:
                    //case 5:
                    default:
                        {
                            var name = grid.GetCellText(r, 0);
                            var label = grid.GetColLabel(c);
                            var val = data.GetValue(name, label);
                            return val.ToString();
                        }
                }
               // return "";
            };
            //for(int r = 0; int < grid.Rows; ++r)
            //{
            //}
        }

        private void ExportExcel(string fileName)
        {
            using (var sht = new NPIOSheet())   //打开myxls.xls文件
            {
                sht.Create();
                int cols = grid.Cols;
                for (int c = 0; c < cols; ++c)
                {
                    var colWidth = grid.GetColWidth(c);
                    sht.SetColumnWidth(c, colWidth);
                    var colLabel = grid.GetColLabel(c);
                    sht.SetCellText(0, c, colLabel);
                }

                for (int r = 0; r < grid.Rows; ++r)
                {
                    for (int c = 0; c < cols; ++c)
                    {
                        var o = grid.GetCellText(r, c);
                        if (string.IsNullOrEmpty(o))
                        {
                            sht.SetCellText(r + 1, c, o);
                        }
                        else
                        {
                            if (c>0)
                            {
                                double d = SafeConvertAux.ToDouble(o);
                                sht.SetCellDouble(r + 1, c, d);
                            }
                            else
                            {
                                sht.SetCellText(r + 1, c, o);
                            }
                        }
                    }
                }
                sht.ExportToExcel(fileName);
            }
        }
    }
}
