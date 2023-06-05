using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.Library.Model;
using Agro.LibCore;
using Agro.LibCore.NPIO;
using Agro.LibCore.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Agro.LibCore.Database;

namespace Agro.Module.DataQuery
{
	/// <summary>
	/// ContentPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ContentPanel : UserControl
    {
        public class QueryTypeItem
        {
            public object entity;
            public string TableName
            {
                get
                {
					return EntityUtil.GetTableName(entity);//.GetTableName();
                }
            }
            public string DisplayName;
            public string ZoneCodeFieldName;
            public string OidFieldName;
            public QueryTypeItem(string dn, string oidFieldName, object en,string zoneCodeFielddName)//,string zoneCodeFieldName )
            {
                entity = en;
                DisplayName = dn;
				ZoneCodeFieldName =EntityUtil.GetFieldName(en, zoneCodeFielddName);// en.GetFieldName(zoneCodeFielddName);
                OidFieldName = oidFieldName;
            }
            public override string ToString()
            {
                return DisplayName;
            }
        }
        public class FieldColumn
        {
            //public DotNetSharp.Data.DataColumnAttribute ColumnAttribute;
            public EntityProperty ColumnAttribute;
            public override string ToString()
            {
                return ColumnAttribute.AliasName;
            }
        }

        /// <summary>
        /// 比较条件
        /// 如："等于", "不等于", "大于", "大于等于", "小于", "小于等于", "开头是", "开头不是", "包含", "不包含", "结尾是", "结尾不是" 
        /// </summary>
        class CompareCondition
        {
            public string Name;
            public string DisplayName;
            public CompareCondition(string displayName,string name)
            {
                Name = name;
                DisplayName = displayName;
            }
            public override string ToString()
            {
                return DisplayName;
            }
        }
        /// <summary>
        /// 查询字段ComboBox相关
        /// </summary>
        class FieldCombo
        {
            private readonly ContentPanel _p;
            public FieldCombo(ContentPanel p)
            {
                _p = p;
            }
            public List<FieldColumn> GetColumns(QueryTypeItem qti)
            {
                var lst0 =EntityUtil.GetAttributes(qti.entity,false);
                return Convert(lst0);
            }
            private List<FieldColumn> Convert(List<EntityProperty> lst0)
            {
                var lst = new List<FieldColumn>();
                foreach (var l in lst0)
                {
                    lst.Add(new FieldColumn() { ColumnAttribute = l });
                }
                return lst;
            }
        }
        class RowData : List<Object>
        {

        }
        interface ICurrentSheet
        {
            string OnGetCellText(int row, int col);
            bool IsNumericField(int col);
            int RowCount { get; }
        }
        /// <summary>
        /// 汇总表表单
        /// </summary>
        class HzbSheet: ICurrentSheet
        {
            class TableData
            {
                public readonly List<EntityProperty> Columns = new List<EntityProperty>();
                public readonly List<RowData> Values = new List<RowData>();
                public object GetValue(int row, int col)
                {
                    if (row >= 0 && row < Values.Count)
                    {
                        var r = Values[row];
                        var o = r[col];
                        return o;// == null ? "" : o.ToString();
                    }
                    return null;
                }
                public void ClearValues()
                {
                    Values.Clear();
                }
            }
            private readonly TableData _data = new TableData();
            private readonly Agro.LibCore.UI.GridView grid;
            private ContentPanel _p;
            private IWorkspace _db { get { return _p._db; } }
            public string _name;
            public HzbSheet(ContentPanel p)
            {
                _p = p;
                grid = p.grid;
                //grid.OnGetCellText += (r, c) =>
                //{
                //    var o = _data.GetValue(r, c);
                //    return o == null ? "" : o.ToString();
                //};
            }

            /// <summary>
            /// 按名称显示6个汇总表中的一个
            /// </summary>
            /// <param name="name"></param>
            public void ShowHzb(string name)
            {
                _name = name;
                string wh = null;
                if (_p._zone != null)
                {
					wh=WhereExpression<JCSJ_SJHZ>.Where(t => t.BM.Contains(_p._zone.Code));
					//var en = new JCSJ_SJHZ ();
     //               wh = nameof(en.BM) + " like '" + _p._zone.Code + "%'";
                }
                if (name == null)
                {
                    ShowAll(wh);
                    return;
                }
                //var att = new JCSJ_SJHZ ();
                switch (name)
                {
                    case HzbTitleConstants.Hzb1:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWDM)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWMC)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.FBFZS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.CBDKZS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.CBDKZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.FCBDKZS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.FCBDZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.BFQZSL)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb2:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWDM)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWMC)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.NYYTZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.ZZYZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.LYZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.XMYZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.YYZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.FNYYTZMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.NYYFNYYTZMJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb3:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWDM)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWMC)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.ZLDMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.JDDMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.KHDMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QTJTTDMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.FCBDZMJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb4:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWDM)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWMC)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.JBNTMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.FJBNTMJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.JBNTMJHJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb5:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWDM)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWMC)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.BFQZSL)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.JTCBQZS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QTFSCBHJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.BFQZMJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb6:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWDM)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QSDWMC)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.CBFZS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.CBNHS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.CBNHCYS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.QTFSCBHJ)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.DWCBZS)));
                            clms.Add(JCSJ_SJHZ .GetAttribute(nameof(JCSJ_SJHZ.GRCBZS)));
                            UpdateTable(wh);
                        }
                        break;
                }
            }
            public int RowCount
            {
                get { return _data.Values.Count; }
            }
            public bool IsNumericField(int col)
            {
                return _data.Columns[col].IsNumericField;
            }

            private void ShowAll(string wh = null)
            {
				JCSJ_SJHZ .GetAttributes(true,_data.Columns);
                UpdateTable(wh);
            }
            private void UpdateTable(string wh = null)
            {
                var clms = _data.Columns;
                _data.ClearValues();
                var _values = _data.Values;

                //var lstColWidth = new double[clms.Count];
                grid.Cols = clms.Count;
                string subFields = null;
                for (int i = 0; i < grid.Cols; ++i)
                {
                    var cl = clms[i];
                    var colLabel = cl.AliasName;
                    grid.SetColLabel(i, colLabel);
                    //lstColWidth[i] = grid.CalcTextWidth(colLabel) + 10;

                    if (subFields == null)
                    {
                        subFields = cl.FieldName;
                    }
                    else
                    {
                        subFields += "," + cl.FieldName;
                    }
                }
                var sql = "select " + subFields + " from " + JCSJ_SJHZ .GetTableName();
                if (!string.IsNullOrEmpty(wh))
                {
                    sql += " where " + wh;
                }
                _db.QueryCallback(sql, r =>
                {
                    var rd = new RowData();
                    _values.Add(rd);
                    for (int c = 0; c < clms.Count; ++c)
                    {
                        var v = r.GetValue(c);
                        rd.Add(v);
                        //if (v != null)
                        //{
                        //    var s = v.ToString();
                        //    var colWidth = grid.CalcTextWidth(s) + 10;
                        //    if (lstColWidth[c] < colWidth)
                        //    {
                        //        lstColWidth[c] = colWidth;
                        //    }
                        //}
                    }
                    return true;
                });
                grid.Rows = _values.Count;
                //for (int c = 0; c < lstColWidth.Length; ++c)
                //{
                //    grid.SetColWidth(c, (int)lstColWidth[c]);
                //}
                _p.updateColumnWidth();
                grid.UpdateLayout();
                grid.Refresh();
            }
            public string OnGetCellText(int r,int c)
            {
                var o = _data.GetValue(r, c);
                return o == null ? "" : o.ToString();
            }
        }


        /// <summary>
        /// 登记簿汇总表表单
        /// yxm 2018-6-11
        /// </summary>
        class DjbHzbSheet : ICurrentSheet
        {
            class TableData
            {
                public readonly List<string> Columns;
                public readonly List<RowData> Values = new List<RowData>();
                public TableData()
                {
                    Columns = new List<string>()
                    {
                        "登记簿代码","承包方名称","承包方证件号码","承包方式","承包期限","登记时间"
                    };
                }
                public object GetValue(int row, int col)
                {
                    if (row >= 0 && row < Values.Count)
                    {
                        var r = Values[row];
                        var o = r[col];
                        return o;// == null ? "" : o.ToString();
                    }
                    return null;
                }
                public void ClearValues()
                {
                    Values.Clear();
                }
            }
            private readonly TableData _data = new TableData();
            private readonly Agro.LibCore.UI.GridView grid;
            private ContentPanel _p;
            private IWorkspace _db { get { return _p._db; } }
            //public string _name;
            public DjbHzbSheet(ContentPanel p)
            {
                _p = p;
                grid = p.grid;
            }

            /// <summary>
            /// 按名称显示6个汇总表中的一个
            /// </summary>
            /// <param name="name"></param>
            public void ShowHzb()//string name)
            {
                //_name = name;
                string wh = null;
                if (_p._zone != null)
                {
                    //var en = new ATT_SUMMERY();
                    wh = "FBFBM like '" + _p._zone.Code + "%'";
                }
                UpdateTable(wh);
            }
            public int RowCount
            {
                get { return _data.Values.Count; }
            }
            public bool IsNumericField(int col)
            {
                return false;// _data.Columns[col].IsNumericField;
            }

            //private void ShowAll(string wh = null)
            //{
            //    var att = new ATT_SUMMERY>();
            //    att.GetProperties(_data.Columns);
            //    UpdateTable(wh);
            //}
            private void UpdateTable(string wh = null)
            {
                var clms = _data.Columns;
                _data.ClearValues();
                var _values = _data.Values;

                grid.Cols = clms.Count;
                for(int c = 0; c < clms.Count; ++c)
                {
                    grid.SetColLabel(c, clms[c]);
                }
   
                var sql = @"select CBJYQZBM as 登记簿代码,b.CBFMC as 承包方名称,b.CBFZJHM as 承包方证件号码,CBFS as 承包方式,CBQX as 承包期限,DJSJ as 等级时间 from DJ_CBJYQ_DJB a
   left join DJ_CBJYQ_CBF b on a.CBFBM = b.CBFBM";
                if (!string.IsNullOrEmpty(wh))
                {
                    sql += " where " + wh;
                }
                _db.QueryCallback(sql, r =>
                {
                    var rd = new RowData();
                    _values.Add(rd);
                    for (int c = 0; c < clms.Count; ++c)
                    {
                        var v = r.GetValue(c);
                        rd.Add(v);
                    }
                    return true;
                });
                grid.Rows = _values.Count;
                _p.updateColumnWidth();
                grid.UpdateLayout();
                grid.Refresh();
            }
            public string OnGetCellText(int r, int c)
            {
                var o = _data.GetValue(r, c);
                return o == null ? "" : o.ToString();
            }
        }

        /// <summary>
        /// 以表格方式显示指定表的数据
        /// 指定表必须包含objectid字段
        /// </summary>
        class TableSheet: ICurrentSheet
        {
            interface ITableData
            {
                string OidFieldName { get; set; }
                void BuildIndex(string wh);
                //RowData GetRowData(int row);
                string OnGetCellText(int row, int col);
                int RowCount { get; }
            }
            class OidTableData : ITableData
            {
                private readonly List<int> _oids = new List<int>();
                private readonly Dictionary<int, RowData> _dicOidRow = new Dictionary<int, RowData>();
                private readonly TableSheet _p;

                public OidTableData(TableSheet p,string oidFieldName="BSM")
                {
                    _p = p;
                    OidFieldName = oidFieldName;
                }
                #region ITableData
                public string OidFieldName { get; set; }
                public void BuildIndex(string wh)
                {
                    _oids.Clear();
                    var sql = "select " + OidFieldName + " from " +EntityUtil.GetTableName(_p._entity);
                    if (!string.IsNullOrEmpty(wh))
                    {
                        sql += " where " + wh;
                    }
                    _p._p._db.QueryCallback(sql, r =>
                    {
                        var o = r.GetValue(0);
                        if (o != null)
                        {
                            var oid = SafeConvertAux.ToInt32(o);
                            _oids.Add(oid);
                        }
                        return true;
                    });
                    _dicOidRow.Clear();
                    refreshCache(0);
                }

                public int RowCount { get { return _oids.Count; } }
                public string OnGetCellText(int row, int col)
                {
                    object o = null;
                    if (row >= 0 && row < _oids.Count)
                    {
                        //var oid = _oids[row];
                        var v = GetRowData(row);// getRowDataByOid(oid);
                        if (v != null)
                        {
                            o = v[col];
                        }
                        else
                        {
                            if (_dicOidRow.Count > 1000)
                            {
                                _dicOidRow.Clear();
                            }
                            refreshCache(row);
                            v = GetRowData(row);// getRowDataByOid(oid);
                            if (v != null)
                            {
                                o = v[col];
                            }
                        }
                    }
                    return o == null ? "" : o.ToString();
                    //var o = _data.GetValue(r, c);
                    //return o == null ? "" : o.ToString();
                }
                #endregion
                private RowData GetRowData(int row)
                {
                    int oid = _oids[row];
                    //return _dicOidRow[oid];
                    RowData val;
                    if (_dicOidRow.TryGetValue(oid, out val))
                    {
                        return val;
                    }

                    return null;
                }
                private void refreshCache(int row)
                {
                    if (row < 0 || row >= _oids.Count)
                        return;

                    var lstOid = new List<int>();
                    for (int r = row; r < _oids.Count; ++r)
                    {
                        var oid = _oids[r];
                        if (!_dicOidRow.ContainsKey(oid))
                        {
                            lstOid.Add(oid);
                        }
                        if (lstOid.Count > 50)
                        {
                            break;
                        }
                    }
                    for (int r = row; r >= 0; --r)
                    {
                        var oid = _oids[r];
                        if (!_dicOidRow.ContainsKey(oid))
                        {
                            lstOid.Add(oid);
                        }
                        if (lstOid.Count > 50)
                        {
                            break;
                        }
                    }
                    string subFields = OidFieldName;// "objectid";
                    for (int i = 0; i < _p.Columns.Count; ++i)
                    {
                        subFields += "," + _p.Columns[i].FieldName;
                    }

                    constructIn(lstOid, sin =>
                    {
                        var tableName =EntityUtil.GetTableName(_p._entity);
                        var sql = "select " + subFields + " from " + tableName + " where " + OidFieldName + " in(" + sin + ")";
                        _p._p._db.QueryCallback(sql, r =>
                        {
                            var dr = new RowData();
                            var oid = SafeConvertAux.ToInt32(r.GetValue(0));
                            for (int i = 0; i < _p.Columns.Count; ++i)
                            {
                                var o = r.GetValue(i + 1);
                                dr.Add(o);
                            }
                            _dicOidRow[oid] = dr;
                            return true;
                        });
                    });
                }
                private void constructIn(List<int> oids, Action<string> callback)
                {
                    int i = 0;
                    while (i < oids.Count)
                    {
                        int j = i + 50;
                        if (j > oids.Count)
                        {
                            j = oids.Count;
                        }
                        string sin = null;
                        for (int k = i; k < j; ++k)
                        {
                            var oid = oids[i];
                            if (sin == null)
                            {
                                sin = oid + "";
                            }
                            else
                            {
                                sin += "," + oid;
                            }
                        }
                        if (sin != null)
                        {
                            callback(sin);
                        }
                        i = j;
                    }
                }
            }
            class IDTableData : ITableData
            {
                private readonly List<string> _oids = new List<string>();
                private readonly Dictionary<string, RowData> _dicOidRow = new Dictionary<string, RowData>();
                private readonly TableSheet _p;
                //private string OidFieldName;
                public IDTableData(TableSheet p, string oidFieldName = "ID")
                {
                    _p = p;
                    OidFieldName = oidFieldName;
                }
                #region ITableData
                public string OidFieldName { get; set; }
                public void BuildIndex(string wh)
                {
                    _oids.Clear();
                    var sql = "select " + OidFieldName + " from " +EntityUtil.GetTableName(_p._entity);
                    if (!string.IsNullOrEmpty(wh))
                    {
                        sql += " where " + wh;
                    }
                    _p._p._db.QueryCallback(sql, r =>
                    {
                        var o = r.GetValue(0);
                        if (o != null)
                        {
                            var oid = o.ToString();// SafeConvertAux.ToInt32(o);
                            _oids.Add(oid);
                        }
                        return true;
                    });
                    _dicOidRow.Clear();
                    refreshCache(0);
                }

                public int RowCount { get { return _oids.Count; } }
                public string OnGetCellText(int row, int col)
                {
                    object o = null;
                    if (row >= 0 && row < _oids.Count)
                    {
                        //var oid = _oids[row];
                        var v = GetRowData(row);// getRowDataByOid(oid);
                        if (v != null)
                        {
                            o = v[col];
                        }
                        else
                        {
                            if (_dicOidRow.Count > 1000)
                            {
                                _dicOidRow.Clear();
                            }
                            refreshCache(row);
                            v = GetRowData(row);// getRowDataByOid(oid);
                            if (v != null)
                            {
                                o = v[col];
                            }
                        }
                    }
                    return o == null ? "" : o.ToString();
                    //var o = _data.GetValue(r, c);
                    //return o == null ? "" : o.ToString();
                }
                #endregion
                private RowData GetRowData(int row)
                {
                    var oid = _oids[row];
                    //return _dicOidRow[oid];
                    RowData val;
                    if (_dicOidRow.TryGetValue(oid, out val))
                    {
                        return val;
                    }

                    return null;
                }
                private void refreshCache(int row)
                {
                    if (row < 0 || row >= _oids.Count)
                        return;

                    var lstOid = new List<string>();
                    for (int r = row; r < _oids.Count; ++r)
                    {
                        var oid = _oids[r];
                        if (!_dicOidRow.ContainsKey(oid))
                        {
                            lstOid.Add(oid);
                        }
                        if (lstOid.Count > 50)
                        {
                            break;
                        }
                    }
                    for (int r = row; r >= 0; --r)
                    {
                        var oid = _oids[r];
                        if (!_dicOidRow.ContainsKey(oid))
                        {
                            lstOid.Add(oid);
                        }
                        if (lstOid.Count > 50)
                        {
                            break;
                        }
                    }
                    string subFields = OidFieldName;// "objectid";
                    for (int i = 0; i < _p.Columns.Count; ++i)
                    {
                        subFields += "," + _p.Columns[i].FieldName;
                    }

                    constructIn(lstOid, sin =>
                    {
                        var tableName =EntityUtil.GetTableName(_p._entity);
                        var sql = "select " + subFields + " from " + tableName + " where " + OidFieldName + " in(" + sin + ")";
                        _p._p._db.QueryCallback(sql, r =>
                        {
                            var dr = new RowData();
                            var oid = SafeConvertAux.ToStr(r.GetValue(0));// SafeConvertAux.ToInt32(r.GetValue(0));
                            for (int i = 0; i < _p.Columns.Count; ++i)
                            {
                                var o = r.GetValue(i + 1);
                                dr.Add(o);
                            }
                            _dicOidRow[oid] = dr;
                            return true;
                        });
                    });
                }
                private void constructIn(List<string> oids, Action<string> callback)
                {
                    int i = 0;
                    while (i < oids.Count)
                    {
                        int j = i + 50;
                        if (j > oids.Count)
                        {
                            j = oids.Count;
                        }
                        string sin = null;
                        for (int k = i; k < j; ++k)
                        {
                            var oid ="'"+ oids[i]+"'";
                            if (sin == null)
                            {
                                sin = oid;
                            }
                            else
                            {
                                sin += "," + oid;
                            }
                        }
                        if (sin != null)
                        {
                            callback(sin);
                        }
                        i = j;
                    }
                }
            }
            private readonly List<EntityProperty> Columns = new List<EntityProperty>();
            public object _entity;
            private List<double> _lstColWidth = new List<double>();
            private string _where;
            private ContentPanel _p;
            private QueryTypeItem _qti;
            private ITableData _data;
            private OidTableData _oidTableData;
            private IDTableData _idTableData;
            public TableSheet(ContentPanel p)
            {
                _p = p;
                //OidFieldName = oidFieldName;
                _oidTableData = new OidTableData(this);
                _idTableData = new IDTableData(this);
                _data = _oidTableData;
            }
            public void Update(QueryTypeItem qti,string wh)
            {
                _qti = qti;
                _entity = qti.entity;
                if (qti.OidFieldName == "ID")
                {
                    _data = _idTableData;
                }else
                {
                    _data = _oidTableData;
                }
                _data.OidFieldName = qti.OidFieldName;
                if (string.IsNullOrEmpty(wh))
                {
                    _where = GetWhere();
                }else
                {
                    string nw = wh;
                    if (_p._zone != null)
                    {
                        wh = _qti.ZoneCodeFieldName + " like '" + _p._zone.Code + "%'";
                    }
                    if (wh != null && nw != null)
                    {
                        _where = wh + " and (" + nw + ")";
                    }
                }
                Update(_where);
            }
            public void Update(string wh)
            {
                var subFields = updateColumns();
                _data.BuildIndex(wh);
                //buildOidIndex(wh);
                _p.grid.Rows = _data.RowCount;// _oids.Count;
                //_dicOidRow.Clear();
                //refreshCache(0);

                _p.updateColumnWidth();
                _p.grid.Refresh();
            }
            public int RowCount
            {
                get { return _data.RowCount; }// _oids.Count; }
            }
            public bool IsNumericField(int col)
            {
                return Columns[col].IsNumericField;
            }

            private string updateColumns()
            {
                _lstColWidth.Clear();
                EntityUtil.GetAttributes(_entity,false,Columns);
                var clms = Columns;
                var grid = _p.grid;
                grid.Cols = clms.Count;
                string subFields = null;
                for (int i = 0; i < grid.Cols; ++i)
                {
                    var cl = clms[i];
                    var colLabel = cl.AliasName;
                    grid.SetColLabel(i, colLabel);
                    var colWidth =Math.Min(200, grid.CalcTextWidth(colLabel) + 10);
                    _lstColWidth.Add(colWidth);
                    grid.SetColWidth(i, (int)colWidth);

                    if (subFields == null)
                    {
                        subFields = cl.FieldName;
                    }
                    else
                    {
                        subFields += "," + cl.FieldName;
                    }
                }
                return subFields;
            }

            private string GetWhere()
            {
                string wh = null;
                if (_p._zone != null)
                {
                    wh = _qti.ZoneCodeFieldName + " like '" + _p._zone.Code + "%'";
                }
                if (wh == null)
                {
                    wh = GetSimpleWhere();
                }else
                {
                    var swh = GetSimpleWhere();
                    if (swh != null)
                    {
                        wh = wh + " and " + swh;
                    }
                }
                return wh;
            }
            /// <summary>
            /// 获取上边面板设置的简单条件
            /// </summary>
            /// <returns></returns>
            private string GetSimpleWhere()
            {
                string wh = null;
                if (_p.cbCxlb.SelectedItem != null
                    &&_p.cbCxtj.SelectedItem!=null
                    &&_p.cbCxzd.SelectedItem!=null
                    &&!string.IsNullOrEmpty(_p.tbKeywords.Text.Trim()))
                {
                    var field = _p.cbCxzd.SelectedItem as FieldColumn;
                    var fStringField = field.ColumnAttribute.PropertyType == typeof(string);
                    //cbCxtj "等于", "不等于", "大于", "大于等于", "小于", "小于等于", "开头是", "开头不是", "包含", "不包含", "结尾是", "结尾不是" };
                    var cc = _p.cbCxtj.SelectedItem as CompareCondition;
                    string sCondition =cc.Name;
                    string keyWords = _p.tbKeywords.Text.Trim();
                    wh = field.ColumnAttribute.FieldName + sCondition;
                    switch (cc.DisplayName)
                    {
                        case "开头是":
                        case "开头不是":
                            wh += "'" + keyWords + "%'";
                            break;
                        case "包含":
                        case "不包含":
                            wh += " '%" + keyWords + "%'";
                            break;
                        case "结尾是":
                        case "结尾不是":
                            wh += "'%" + keyWords + "'";
                            break;
                        default:
                            if (fStringField)
                            {
                                wh += "'" + keyWords + "'";
                            }else
                            {
                                wh += keyWords;
                            }
                            break;
                    }
                }
                return wh;
            }

            public string OnGetCellText(int row, int col)
            {
                return _data.OnGetCellText(row, col);
                /*
                object o = null;
                if (row >= 0 && row < _oids.Count)
                {
                    var oid = _oids[row];
                    var v = getRowDataByOid(oid);
                    if (v != null)
                    {
                        o= v[col];
                    }
                    else
                    {
                        if (_dicOidRow.Count > 1000)
                        {
                            _dicOidRow.Clear();
                        }
                        refreshCache(row);
                        v = getRowDataByOid(oid);
                        if (v != null)
                        {
                            o= v[col];
                        }
                    }
                }
                return o==null?"":o.ToString();
                //var o = _data.GetValue(r, c);
                //return o == null ? "" : o.ToString();
                */
            }
        }



        /// <summary>
        /// 自定义统计表单
        /// </summary>
        internal class StatisticsSheet : ICurrentSheet
        {
            interface ISheetData
            {
                /// <summary>
                /// 根据地域编码和统计类别获取单元格数据
                /// </summary>
                /// <param name="zoneCode"></param>
                /// <param name="sTjlb"></param>
                /// <returns></returns>
                string GetCellText(string zoneCode, string sTjlb);
            }
            class MySheetData : ISheetData
            {

                public string GetCellText(string zoneCode, string sTjlb)
                {
                    return "";
                }
            }
            ///// <summary>
            ///// 发包方表的统计数据
            ///// </summary>
            //class FBFStatistics
            //{
            //    /// <summary>
            //    /// [地域名称，发包方总数]
            //    /// </summary>
            //    private readonly Dictionary<string, int> _dic = new Dictionary<string, int>();
            //    public void UpdateData(IWorkspace db, Zone zone)
            //    {
            //        _dic.Clear();
            //        int len = GetSubstringLen(zone.Level);
            //        var sql =string.Format("select a.FBFBM,count(*) from (select SUBSTRING(fbfbm,0,{0}) as FBFBM from fbf  where fbfbm is not null and fbfbm like '{1}%') a group by a.FBFBM"
            //            , len,zone.FullCode);
            //        db.QueryCallback(sql, r =>
            //        {
            //            var bm=r.GetValue(0);
            //            var cnt = SafeConvertAux.ToInt32(r.GetValue(1));
            //            _dic[bm.ToString()] = cnt;
            //            return true;
            //        });
            //    }
            //    public string GetText(string zoneCode)
            //    {
            //        int n;
            //        if(_dic.TryGetValue(zoneCode,out n))
            //        {
            //            return n.ToString();
            //        }
            //        return "";
            //    }
            //}
            private readonly List<ShortZone> _zoneCol = new List<ShortZone>();
            private readonly ContentPanel _p;
            private List<StatisticsSetupPanel.StaticsItem> _lstTjlb;
            internal StatisticsSheet(ContentPanel p)
            {
                _p = p;
            }
            #region ICurrentSheet
            public int RowCount
            {
                get
                {
                    return _zoneCol.Count;
                }
            }
            public bool IsNumericField(int col)
            {
                return col > 0;
            }


            public string OnGetCellText(int row, int col)
            {
                if (col == 0)
                {
                    return _zoneCol[row].Name;
                }
                var clm = _lstTjlb[col - 1];
                if (row == 0)
                {
                    return clm.GetSum();
                }
                var zoneCode = _zoneCol[row].Code;
                var s=clm.GetText(zoneCode);
                return s;
            }
            #endregion
            /// <summary>
            /// 
            /// </summary>
            /// <param name="lstTjlb">统计类别</param>
            internal void Update(List<StatisticsSetupPanel.StaticsItem> lstTjlb=null)
            {
                if (lstTjlb != null)
                {
                    _lstTjlb = lstTjlb;
                }
                var selZone = _p._zone;
                ZoneUtil.QueryChildren(selZone, _zoneCol);
                updateColumns();
                var grid = _p.grid;
                grid.Rows = _zoneCol.Count;
                if(lstTjlb==null)
                {
                    OnZoneChanged(_p._zone);
                }
                grid.Refresh();
            }
            internal void OnZoneChanged(ShortZone zone)
            {
                foreach(var i in _lstTjlb)
                {
                    i.OnZoneChanged(_p._db, zone);
                }
                _p.updateColumnWidth();
            }
            private void updateColumns()
            {
                var grid = _p.grid;
                grid.Cols = _lstTjlb.Count + 1;
                //grid.Cols =3;
                grid.SetColLabel(0, "地域名称");
                for(int i = 0; i < _lstTjlb.Count; ++i)
                {
                    grid.SetColLabel(i + 1, _lstTjlb[i].ItemType);
                }
                //grid.SetColLabel(1, "地域编码");
                //grid.SetColLabel(2, "发包方总数");
            }
            private static int GetSubstringLen(eZoneLevel level)
            {
                switch (level)
                {
                    case eZoneLevel.State:
                        return 0;
                    case eZoneLevel.Province:
                        return 3;
                    case eZoneLevel.City:
                        return 7;
                    case eZoneLevel.County:
                        return 10;
                    case eZoneLevel.Town:
                        return 13;
                    case eZoneLevel.Village:
                        return 15;
                }
                return 15;
            }

        }

        class ShowDialogUtil
        {
            private Window _mainWindow;
            public readonly Border bdrMask;
            public ShowDialogUtil(ContentPanel p)//Border bdrMask_)
            {
                bdrMask = p.bdrMask;
                _mainWindow = Window.GetWindow(p);
            }
            public void ShowDialog(string title,UserControl pnl,Func<bool> onOKCallback)
            {
                var wnd = _mainWindow;// Window.GetWindow(this);
                wnd.Deactivated += OnMainWindowDeActivated;

				var dlg = new KuiDialog(wnd, title)
				{
					Content = pnl,
					Width=730,
				};
				dlg.Closed += (s1, e1) =>
                {
                    if (bdrMask != null)
                    {
                        double time = 0.25;
                        var sb = StoryboardsUtil.OpacityStoryboard(bdrMask, 1, 0, time);
                        sb.Completed += (s2, e2) =>
                        {
                            bdrMask.Visibility = System.Windows.Visibility.Hidden;
                        };
                        sb.Begin();
                    }
                };
                dlg.BtnOK.Click += (s1, e1) => {
                    try
                    {
                        var fOK = onOKCallback();
                        if (fOK)
                        {
                            //var wh = pnl.GetWhere();
                            //if (string.IsNullOrEmpty(wh))
                            //{
                            //    MessageBox.Show("未输入查询表达式！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            //    return;
                            //}
                            //var err = pnl.CheckWhere();
                            //if (err != null)
                            //{
                            //    MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                            //    return;
                            //}
                            //_strCustomWhere = wh;
                            //DoQuery(wh);
                            dlg.Close();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                };
                dlg.ShowDialog();
            }
            private void OnMainWindowDeActivated(object sender, EventArgs e)
            {
                if (bdrMask != null)
                {
                    double time = 0.25;
                    bdrMask.Visibility = System.Windows.Visibility.Visible;
                    StoryboardsUtil.OpacityStoryboard(bdrMask, 0, 1, time).Begin();
                    _mainWindow.Deactivated -= OnMainWindowDeActivated;
                }
            }
        }

        private readonly IWorkspace _db = MyGlobal.Workspace;
        private readonly HzbSheet _hzbSheet;
        private readonly DjbHzbSheet _DjbHzbSheet;
        private readonly TableSheet _tableSheet;
        private readonly StatisticsSheet _statisticsSheet;
        private ICurrentSheet _currentSheet;
        private readonly FieldCombo _fieldCombo;
        //private Zone _zone;
        private ShortZone _zone;
        //private DataQueryContext _ctx;
        public Border bdrMask;
        private string _strCustomWhere;
        public ContentPanel()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            _hzbSheet = new HzbSheet(this);
            _DjbHzbSheet = new DjbHzbSheet(this);
            _tableSheet = new TableSheet(this);
            _statisticsSheet = new StatisticsSheet(this);
            _fieldCombo = new FieldCombo(this);
            grid.ShowGrid = false;
            grid.RowHeight =grid.ColHeaderHeight= 28;
            grid.OnDrawCell += (dc, ci) =>
            {
                var r = ci.Row;
                if ((r %2)==0&&r!=grid.ActiveRow)
                {
                    ci.background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF1,0xF1,0xF1));// System.Windows.Media.Colors.LightGray);
                }
            };
            init();

            ShowHzb(null);
            cbCxlb.SelectedIndex = 0;
        }
        //public void Init(IWorkspace db)//, DataQueryContext ctx)
        //{
        //    if (db == null)
        //    {
        //        return;
        //    }
        //    _db = db;
        //    //_ctx = ctx;
        //    //_hzbSheet.ShowAll();
        //    ShowHzb(null);
        //    cbCxlb.SelectedIndex = 0;
        //}
        public void OnZoneChanged(ShortZone zone)
        {
            _zone = zone;
            if (_currentSheet == _tableSheet)
            {
                DoQuery();
            } else if (_currentSheet == _hzbSheet)
            {
                ShowHzb(_hzbSheet._name);
            } else if (_currentSheet== _DjbHzbSheet) {
                _DjbHzbSheet.ShowHzb();
            } else
            {
                _statisticsSheet.Update();
            }
        }
        /// <summary>
        /// 显示汇总表（6个固定格式的汇总表）
        /// tableAlias参见：HzbTitleConstants；
        /// 例外：tableAlias传入 null则显示所有字段；
        /// </summary>
        /// <param name="tableAlias"></param>
        /// <param name="wh"></param>
        public void ShowHzb(string tableAlias)//,string wh=null)
        {
            if (tableAlias != HzbTitleConstants.Hzb7)
            {
                _currentSheet = _hzbSheet;
                _hzbSheet.ShowHzb(tableAlias);
            }else
            {
                _currentSheet = _DjbHzbSheet;
                _DjbHzbSheet.ShowHzb();
            }
            grid.EnsureRowVisible(0);
            grid.EnsureColVisible(0);
        }
        private void init()
        {
            cbCxlb.ItemsSource = new QueryTypeItem[] {
			   new QueryTypeItem("发包方","ID",new QSSJ_FBF (),nameof(QSSJ_FBF .FBFBM)),
				new QueryTypeItem("承包方","ID",new QSSJ_CBF (),nameof(QSSJ_CBF .CBFBM)),
				new QueryTypeItem("家庭成员","ID",new QSSJ_CBF_JTCY(),nameof(QSSJ_CBF_JTCY.CBFBM)),
				new QueryTypeItem("地块","BSM",new DLXX_DK(),nameof(DLXX_DK.DKBM)),
				new QueryTypeItem("承包地信息","ID",new QSSJ_CBDKXX (),nameof(QSSJ_CBDKXX .DKBM)),
				new QueryTypeItem("承包合同","ID",new QSSJ_CBHT(),nameof(QSSJ_CBHT.CBHTBM)),
				new QueryTypeItem("登记簿","ID",new QSSJ_CBJYQZDJB (),nameof(QSSJ_CBJYQZDJB .CBJYQZBM)),
				 new QueryTypeItem("权证","ID",new QSSJ_CBJYQZ(),nameof(QSSJ_CBJYQZ.CBJYQZBM))
			};
            cbCxtj.ItemsSource = new CompareCondition[] {
                new CompareCondition("等于","="),
                new CompareCondition("不等于","<>"),
                new CompareCondition("大于",">"),
                new CompareCondition("大于等于",">="),
                new CompareCondition("小于","<"),
                new CompareCondition("小于等于","<="),
                new CompareCondition("开头是"," like "),
                new CompareCondition("开头不是"," not like "),
                new CompareCondition("包含"," like "),
                new CompareCondition("不包含"," not like "),
                new CompareCondition("结尾是"," like "),
                new CompareCondition("结尾不是"," not like ") };
            //cbCxtj.SelectedIndex = 0;
            cbCxlb.SelectionChanged += (s, e) =>
            {
                var tableAlias = cbCxlb.SelectedItem as QueryTypeItem;
                if (tableAlias != null)
                {
                    var lst =_fieldCombo.GetColumns(tableAlias);
                    cbCxzd.ItemsSource = lst;
                }
            };
            grid.OnGetCellText += (r, c) =>
            {
                if (_currentSheet != null)
                {
                    return _currentSheet.OnGetCellText(r, c);
                }
                return "";
            };
            btnQuery.Click += (s, e) =>
            {
                DoQuery();
            };

            #region 自定义查询
            btnCustomQuery.Click += (s, e) =>
            {
                //var dlg = new TableFilterDialog();
                //_ctx.Workpage.Page.ShowDialog(dlg);
                var qti = cbCxlb.SelectedItem as QueryTypeItem;
                var pnl = new TableFilterDialog(qti, _fieldCombo.GetColumns(qti), _db, _strCustomWhere);


                var dlg = new ShowDialogUtil(this);//.bdrMask);
                dlg.ShowDialog("自定义查询", pnl, () =>
                   {
                       try
                       {
                           var wh = pnl.GetWhere();
                           if (string.IsNullOrEmpty(wh))
                           {
                               MessageBox.Show("未输入查询表达式！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                               return false;
                           }
                           var err = pnl.CheckWhere();
                           if (err != null)
                           {
                               MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                               return false;
                           }
                           _strCustomWhere = wh;
                           DoQuery(wh);
                           return true;
                       }
                       catch (Exception ex)
                       {
                               MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                       }
                       return false;
                   });
             };
            #endregion

            #region 自定义统计
            btnCustomStatistics.Click += (s, e) =>
            {
                if (_zone == null)
                {
                    MessageBox.Show("请先选择当前地域！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var dlg = new ShowDialogUtil(this);//.bdrMask);
                var pnl = new StatisticsSetupPanel();
                dlg.ShowDialog("自定义统计", pnl, () =>
                {
                    try
                    {
                        var lst = pnl.GetTjlbs();
                        if (lst.Count == 0)
                        {
                            MessageBox.Show("未选择统计类别！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                        _currentSheet = _statisticsSheet;
                        _statisticsSheet.Update(lst);
                        _statisticsSheet.OnZoneChanged(_zone);
                        updateColumnWidth();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return false;
                });
            };
            #endregion

            #region 导出
            btnExport.Click += (s, e) =>
            {
                if (_currentSheet == null)
                {
                    return;
                }
                if (_currentSheet.RowCount >= 65535)
                {
                    MessageBox.Show("数据量太大，请补充适当的查询条件！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var ofd = new SaveFileDialog();
                ofd.Filter = "Excel文件(*.xls)|*.xls";
                ofd.OverwritePrompt = true;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() != true)
                    return;
                try
                {
                    var file = ofd.FileName;
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    if (_currentSheet !=null)
                    {
                        ExportExcel(file);
                        Win32.ShellExecute(IntPtr.Zero, "open", file, null, null, ShowCommands.SW_SHOWNORMAL);
                    }
                }catch(Exception ex)
                {
                    UIHelper.ShowExceptionMessage(ex);
                }
            };
            #endregion

            tbKeywords.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    DoQuery();
                }
            };
        }
        private void ExportExcel(string fileName)
        {
            if (_currentSheet == null)
            {
                return;
            }
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

                for(int r=0;r<grid.Rows;++r)
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
                            if (_currentSheet.IsNumericField(c))
                            {
                                double d = SafeConvertAux.ToDouble(o);
                                sht.SetCellDouble(r+1, c, d);
                            }
                            else
                            {
                                sht.SetCellText(r+1, c, o);
                            }
                        }
                    }
                }
                sht.ExportToExcel(fileName);
            }
        }

        private void updateColumnWidth()
        {
            GridViewUtil.UpdateColumnWidth(grid);
            //#region update ColWidth
            //var lstColWidth = new double[grid.Cols];
            //int rows = Math.Min(50, grid.Rows);
            //for (int c = 0; c < lstColWidth.Length; ++c)
            //{
            //    var colLabel = grid.GetColLabel(c);
            //    lstColWidth[c] = grid.CalcTextWidth(colLabel) + 10;
            //    for (int i = 0; i <rows; ++i)
            //    {
            //        var s =grid.GetCellText(i, c);
            //        var wi = Math.Min(250, grid.CalcTextWidth(s) + 10);
            //        if (lstColWidth[c] < wi)
            //        {
            //            lstColWidth[c] = wi;
            //        }
            //    }
            //}
            //for (int c = 0; c < lstColWidth.Length; ++c)
            //{
            //    grid.SetColWidth(c, (int)lstColWidth[c]);
            //}
            //#endregion

        }

        private void DoQuery(string wh=null)
        {
            _currentSheet = _tableSheet;
            var ti= cbCxlb.SelectedItem as QueryTypeItem;
            if (ti != null)
            {
                _tableSheet.Update(ti,wh);
                grid.EnsureRowVisible(0);
                grid.EnsureColVisible(0);
            }
        }
    }
}
