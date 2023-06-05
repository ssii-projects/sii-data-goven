using Agro.Library.Common;
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
using Agro.LibCore.Database;
using Agro.Library.Model;

namespace Agro.Module.DataView
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
                    return EntityUtil.GetTableName(entity);
                }
            }
            public string DisplayName;
            public string ZoneCodeFieldName;
            public string OidFieldName;
            public QueryTypeItem(string dn, string oidFieldName, object en,Func<string> zoneCodeFielddName)//,string zoneCodeFieldName )
            {
                entity = en;
                DisplayName = dn;
                ZoneCodeFieldName = zoneCodeFielddName();
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

		class ComboItemData : NotificationObject
		{
			//public string FieldName;
			//public eFieldType FieldType;
			public Field _field;
			public ComboItemData(Field field)//string fieldName, eFieldType fieldType)
			{
				_field = field;
				//FieldName = fieldName;
				//FieldType = fieldType;
			}
			public override string ToString()
			{
				return _field.AliasName;// FieldName;
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
				return Convert(EntityUtil.GetAttributes(qti.entity,false));
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
			class CodeCache
			{
				private readonly Dictionary<string, Dictionary<string, string>> _dic = new Dictionary<string, Dictionary<string, string>>();
				public string GetName(string codeType, string code)
				{
					if (!_dic.TryGetValue(codeType, out Dictionary<string, string> d))
					{
						d = new Dictionary<string, string>();
						if (codeType == "土地利用类型")
						{
							CodeUtil.QueryTdlylxItems((c, n) =>
							{
								d[c] = n;
							});
						} else {
							CodeUtil.QueryCodeItems(codeType, (c, n) =>
							  {
								  d[c] = n;
							  });
						}
						_dic[codeType] = d;
					}
					if (d != null)
					{
						if (d.TryGetValue(code, out string n))
						{
							return n;
						}
					}
					return code;
				}
			}
            internal class TableData
            {
				//public readonly List<EntityProperty> Columns = new List<EntityProperty>();
				public readonly List<Field> Columns = new List<Field>();
				//public readonly List<RowData> Values = new List<RowData>();
				private readonly Dictionary<string, RowData> _dicIDRow = new Dictionary<string, RowData>();
				private readonly List<string> _lstIDs = new List<string>();
				private readonly HzbSheet _p;

				public TableData(HzbSheet p)
				{
					_p = p;
				}
				public void ReQuery(string wh = null)
				{
					_lstIDs.Clear();
					_dicIDRow.Clear();
					var sql = "select " + _p._idFieldName + " from " + _p._tableName+" where "+_p._idFieldName+" is not null";
					if (!string.IsNullOrEmpty(wh)) {
						sql += " and (" + wh + ")";
					}
					_p._db.QueryCallback(sql, r =>
					 {
						 _lstIDs.Add(r.GetString(0));
						 return true;
					 });

				}
                public object GetValue(int row, int col)
                {
                    if (row >= 0 && row < _lstIDs.Count)
                    {
						var id = _lstIDs[row];
						if (!_dicIDRow.TryGetValue(id, out RowData rd))
						{
							RefreshCache(row);
						}
						if (_dicIDRow.TryGetValue(id, out rd))
						{
							return rd[col];
						}
					}
                    return null;
                }
                public void ClearValues()
                {

                    //Values.Clear();
                }
				public int RowCount { get { return _lstIDs.Count; } }

				private void RefreshCache(int row)
				{
					if (row < 0 || row >= RowCount)
						return;
					var setIDs = new HashSet<string>();
					for (int r = row; r < RowCount; ++r)
					{
						var oid = _lstIDs[r];
						if (!_dicIDRow.ContainsKey(oid))
						{
							setIDs.Add(oid);
						}
						if (setIDs.Count > 100)
						{
							break;
						}
					}
					for (int r = row; r >= 0; --r)
					{
						var oid = _lstIDs[r];
						if (!_dicIDRow.ContainsKey(oid))
						{
							setIDs.Add(oid);
						}
						if (setIDs.Count > 100)
						{
							break;
						}
					}
					var subFields = GetSubFields();

					SqlUtil.ConstructIn(setIDs, sin =>
					 {
						 var sql = $"select {subFields},{ _p._idFieldName} from {_p._tableName} where {_p._idFieldName} in({sin})";
						 _p._db.QueryCallback(sql, r =>
						  {
							  var rd = new RowData();
							  var id = r.GetString(Columns.Count);
							  for (int c =0; c < Columns.Count; ++c)
							  {
								  rd.Add(r.GetValue(c));
							  }
							  _dicIDRow[id] = rd;
						  });
					 });
				}

				private string GetSubFields()
				{
					string str = null;
					for (int i = 0; i < Columns.Count; ++i)
					{
						var fieldName = Columns[i].FieldName;
						if (str == null)
						{
							str = fieldName;
						}
						else
						{
							str += "," + fieldName;
						}
					}
					return str;
				}
			}
            internal readonly TableData _data;
            private readonly Agro.LibCore.UI.GridView grid;
            private ContentPanel _p;
            private IWorkspace _db { get { return _p._db; } }
            public string _title;

			internal string _tableName;
			internal string _idFieldName;
			internal string _dkbmFieldName;
			internal string _zoneCode;
			internal HashSet<string> _excludeFieldNames;
			private readonly CodeCache _cc = new CodeCache();
			public HzbSheet(ContentPanel p)
            {
                _p = p;
                grid = p.grid;
				_data = new TableData(this);
				//grid.OnGetCellText += (r, c) =>
				//{
				//    var o = _data.GetValue(r, c);
				//    return o == null ? "" : o.ToString();
				//};
			}

            /// <summary>
            /// 按名称显示6个汇总表中的一个
            /// </summary>
            /// <param name="title"></param>
            public void ShowHzb(string title,string tableName,string idFieldName,string dkbmFieldName,string zoneCode, HashSet<string> excludeFieldNames,string where=null)//ShortZone zone)
            {
                _title = title;
				_tableName = tableName;
				_idFieldName = idFieldName;
				_dkbmFieldName = dkbmFieldName;
				_zoneCode = zoneCode;
				_excludeFieldNames = excludeFieldNames;
				_data.Columns.Clear();
				_db.QueryFields2(tableName, _data.Columns);
				if (excludeFieldNames != null)
				{
					foreach (var fieldName in excludeFieldNames)
					{
						_data.Columns.RemoveAll(a => { return a.FieldName == fieldName; });
					}
				}

				string wh = null;
				if (zoneCode != null && !string.IsNullOrEmpty(dkbmFieldName))
				{
					wh = dkbmFieldName + " like '" + zoneCode + "%'";
				}
				if (wh == null)
				{
					wh = where;
				}
				else if (where != null)
				{
					wh += " and (" + where + ")";
				}

				_data.ReQuery(wh);


				var clms = _data.Columns;
				//_data.ClearValues();
				//var _values = _data.Values;

				//var lstColWidth = new double[clms.Count];
				grid.Cols = clms.Count;
				//string subFields = null;
				for (int i = 0; i < grid.Cols; ++i)
				{
					var cl = clms[i];
					var colLabel = cl.AliasName;
					grid.SetColLabel(i, colLabel);
				}
					//	//lstColWidth[i] = grid.CalcTextWidth(colLabel) + 10;

					//	if (subFields == null)
					//	{
					//		subFields = cl.ColumnName;
					//	}
					//	else
					//	{
					//		subFields += "," + cl.ColumnName;
					//	}
					//}
					//var sql = "select " + subFields + " from " + new Entity<ATT_SUMMERY>().TableName;
					//if (!string.IsNullOrEmpty(wh))
					//{
					//	sql += " where " + wh;
					//}
					//_db.QueryCallback(sql, r =>
					//{
					//	var rd = new RowData();
					//	_values.Add(rd);
					//	for (int c = 0; c < clms.Count; ++c)
					//	{
					//		var v = r.GetValue(c);
					//		rd.Add(v);
					//			//if (v != null)
					//			//{
					//			//    var s = v.ToString();
					//			//    var colWidth = grid.CalcTextWidth(s) + 10;
					//			//    if (lstColWidth[c] < colWidth)
					//			//    {
					//			//        lstColWidth[c] = colWidth;
					//			//    }
					//			//}
					//		}
					//	return true;
					//});
					grid.Rows = _data.RowCount;// _values.Count;
				//for (int c = 0; c < lstColWidth.Length; ++c)
				//{
				//    grid.SetColWidth(c, (int)lstColWidth[c]);
				//}
				_p.UpdateColumnWidth();
				grid.UpdateLayout();
				grid.Refresh();

				/*
                string wh = null;
                if (_p._zone != null)
                {
                    var en = new ATT_SUMMERY();
                    wh = nameof(en.BM) + " like '" + _p._zone.Code + "%'";
                }
                if (title == null)
                {
                    ShowAll(wh);
                    return;
                }
                var att = new Entity<ATT_SUMMERY>();
                switch (title)
                {
                    case HzbTitleConstants.Hzb1:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(att.Attribute(nameof(att.en.QSDWDM)));
                            clms.Add(att.Attribute(nameof(att.en.QSDWMC)));
                            clms.Add(att.Attribute(nameof(att.en.FBFZS)));
                            clms.Add(att.Attribute(nameof(att.en.CBDKZS)));
                            clms.Add(att.Attribute(nameof(att.en.CBDKZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.FCBDKZS)));
                            clms.Add(att.Attribute(nameof(att.en.FCBDZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.BFQZSL)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb2:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(att.Attribute(nameof(att.en.QSDWDM)));
                            clms.Add(att.Attribute(nameof(att.en.QSDWMC)));
                            clms.Add(att.Attribute(nameof(att.en.NYYTZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.ZZYZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.LYZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.XMYZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.YYZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.FNYYTZMJ)));
                            clms.Add(att.Attribute(nameof(att.en.NYYFNYYTZMJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb3:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(att.Attribute(nameof(att.en.QSDWDM)));
                            clms.Add(att.Attribute(nameof(att.en.QSDWMC)));
                            clms.Add(att.Attribute(nameof(att.en.ZLDMJ)));
                            clms.Add(att.Attribute(nameof(att.en.JDDMJ)));
                            clms.Add(att.Attribute(nameof(att.en.KHDMJ)));
                            clms.Add(att.Attribute(nameof(att.en.QTJTTDMJ)));
                            clms.Add(att.Attribute(nameof(att.en.FCBDZMJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb4:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(att.Attribute(nameof(att.en.QSDWDM)));
                            clms.Add(att.Attribute(nameof(att.en.QSDWMC)));
                            clms.Add(att.Attribute(nameof(att.en.JBNTMJ)));
                            clms.Add(att.Attribute(nameof(att.en.FJBNTMJ)));
                            clms.Add(att.Attribute(nameof(att.en.JBNTMJHJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb5:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(att.Attribute(nameof(att.en.QSDWDM)));
                            clms.Add(att.Attribute(nameof(att.en.QSDWMC)));
                            clms.Add(att.Attribute(nameof(att.en.BFQZSL)));
                            clms.Add(att.Attribute(nameof(att.en.JTCBQZS)));
                            clms.Add(att.Attribute(nameof(att.en.QTFSCBHJ)));
                            clms.Add(att.Attribute(nameof(att.en.BFQZMJ)));
                            UpdateTable(wh);
                        }
                        break;
                    case HzbTitleConstants.Hzb6:
                        {
                            var clms = _data.Columns;
                            clms.Clear();
                            clms.Add(att.Attribute(nameof(att.en.QSDWDM)));
                            clms.Add(att.Attribute(nameof(att.en.QSDWMC)));
                            clms.Add(att.Attribute(nameof(att.en.CBFZS)));
                            clms.Add(att.Attribute(nameof(att.en.CBNHS)));
                            clms.Add(att.Attribute(nameof(att.en.CBNHCYS)));
                            clms.Add(att.Attribute(nameof(att.en.QTFSCBHJ)));
                            clms.Add(att.Attribute(nameof(att.en.DWCBZS)));
                            clms.Add(att.Attribute(nameof(att.en.GRCBZS)));
                            UpdateTable(wh);
                        }
                        break;
                }
				*/
			}
			public int RowCount
            {
                get { return _data.RowCount; }//.Values.Count; }
            }
            public bool IsNumericField(int col)
            {
				var ft = _data.Columns[col].FieldType;

				return ft==eFieldType.eFieldTypeDouble||ft==eFieldType.eFieldTypeInteger||ft==eFieldType.eFieldTypeSingle||ft==eFieldType.eFieldTypeSmallInteger;// _data.Columns[col].IsNumericField;
            }

            //private void ShowAll(string wh = null)
            //{
            //    var att = new Entity<ATT_SUMMERY>();
            //    att.GetProperties(_data.Columns);
            //    UpdateTable(wh);
            //}
            //private void UpdateTable(string wh = null)
            //{
            //    var clms = _data.Columns;
            //    _data.ClearValues();
            //    var _values = _data.Values;

            //    //var lstColWidth = new double[clms.Count];
            //    grid.Cols = clms.Count;
            //    string subFields = null;
            //    for (int i = 0; i < grid.Cols; ++i)
            //    {
            //        var cl = clms[i];
            //        var colLabel = cl.AliasName;
            //        grid.SetColLabel(i, colLabel);
            //        //lstColWidth[i] = grid.CalcTextWidth(colLabel) + 10;

            //        if (subFields == null)
            //        {
            //            subFields = cl.ColumnName;
            //        }
            //        else
            //        {
            //            subFields += "," + cl.ColumnName;
            //        }
            //    }
            //    var sql = "select " + subFields + " from " + new Entity<ATT_SUMMERY>().TableName;
            //    if (!string.IsNullOrEmpty(wh))
            //    {
            //        sql += " where " + wh;
            //    }
            //    _db.QueryCallback(sql, r =>
            //    {
            //        var rd = new RowData();
            //        _values.Add(rd);
            //        for (int c = 0; c < clms.Count; ++c)
            //        {
            //            var v = r.GetValue(c);
            //            rd.Add(v);
            //            //if (v != null)
            //            //{
            //            //    var s = v.ToString();
            //            //    var colWidth = grid.CalcTextWidth(s) + 10;
            //            //    if (lstColWidth[c] < colWidth)
            //            //    {
            //            //        lstColWidth[c] = colWidth;
            //            //    }
            //            //}
            //        }
            //        return true;
            //    });
            //    grid.Rows = _values.Count;
            //    //for (int c = 0; c < lstColWidth.Length; ++c)
            //    //{
            //    //    grid.SetColWidth(c, (int)lstColWidth[c]);
            //    //}
            //    _p.updateColumnWidth();
            //    grid.UpdateLayout();
            //    grid.Refresh();
            //}
            public string OnGetCellText(int r,int c)
            {
				var o = _data.GetValue(r, c);
				if (o is DateTime dt)
				{
					o = dt.ToString("d");
				}
				else
				{
					if (o != null)
					{
						switch (_tableName)// == "DJ_CBJYQ_DKXX")
						{
							case "DJ_CBJYQ_DKXX":
								{
									var clmName = _data.Columns[c].FieldName;
									switch (clmName)
									{
										case "CBJYQQDFS":
											o = _cc.GetName(CodeType.CBFS, o.ToString());
											break;
										case "SYQXZ":
											o = _cc.GetName(CodeType.SYQXZ, o.ToString());
											break;
										case "DKLB":
											o = _cc.GetName("地块类别", o.ToString());
											break;
										case "TDLYLX":
											o = _cc.GetName("土地利用类型", o.ToString());
											break;
										case "DLDJ":
											o = _cc.GetName("地力等级", o.ToString());
											break;
										case "TDYT":
											o = _cc.GetName("土地用途", o.ToString());
											break;
										case "SFJBNT":
											o = _cc.GetName("是否", o.ToString());
											break;
									}
								}
								break;
							case "DJ_CBJYQ_CBF":
								{
									var clmName = _data.Columns[c].FieldName;
									switch (clmName)
									{
										case "CBFLX":
											o = _cc.GetName("承包方类型", o.ToString());
											break;
										case "CBFZJLX":
											o = _cc.GetName("证件类型", o.ToString());
											break;
									}
								}
								break;
							case "DJ_CBJYQ_CBF_JTCY":
								{
									var clmName = _data.Columns[c].FieldName;
									switch (clmName)
									{
										case "CYXB":
											o = _cc.GetName("性别", o.ToString());
											break;
										case "CYZJLX":
											o = _cc.GetName("证件类型", o.ToString());
											break;
										case "YHZGX":
											o = _cc.GetName("家庭关系", o.ToString());
											break;
										case "SFGYR":
											o = _cc.GetName("是否", o.ToString());
											break;
									}
								}
								break;
							case "QSSJ_FBF":
								{
									var clmName = _data.Columns[c].FieldName;
									switch (clmName)
									{
										case "FZRZJLX":
											o = _cc.GetName("证件类型", o.ToString());
											break;
									}
								}
								break;
							case "QSSJ_CBHT":
								{
									var clmName = _data.Columns[c].FieldName;
									switch (clmName)
									{
										case "CBFS":
											o = _cc.GetName(CodeType.CBFS, o.ToString());
											break;
									}
								}
								break;
							case "QSSJ_CBJYQZDJB":
								{
									var clmName = _data.Columns[c].FieldName;
									switch (clmName)
									{
										case "CBFS":
											o = _cc.GetName(CodeType.CBFS, o.ToString());
											break;
									}
								}
								break;
						}
					}
				}
                return o == null ? "" : o.ToString();
            }
        }


   //     /// <summary>
   //     /// 登记簿汇总表表单
   //     /// yxm 2018-6-11
   //     /// </summary>
   //     class DjbHzbSheet : ICurrentSheet
   //     {
   //         class TableData
   //         {
   //             public readonly List<string> Columns;
   //             public readonly List<RowData> Values = new List<RowData>();
   //             public TableData()
   //             {
   //                 Columns = new List<string>()
   //                 {
   //                     "登记簿代码","承包方名称","承包方证件号码","承包方式","承包期限","登记时间"
   //                 };
   //             }
   //             public object GetValue(int row, int col)
   //             {
   //                 if (row >= 0 && row < Values.Count)
   //                 {
   //                     var r = Values[row];
   //                     var o = r[col];
   //                     return o;// == null ? "" : o.ToString();
   //                 }
   //                 return null;
   //             }
   //             public void ClearValues()
   //             {
   //                 Values.Clear();
   //             }
   //         }
   //         private readonly TableData _data = new TableData();
   //         private readonly Agro.LibCore.UI.GridView grid;
   //         private ContentPanel _p;
   //         private IWorkspace _db { get { return _p._db; } }
   //         //public string _name;
   //         public DjbHzbSheet(ContentPanel p)
   //         {
   //             _p = p;
   //             grid = p.grid;
   //         }

   //         /// <summary>
   //         /// 按名称显示6个汇总表中的一个
   //         /// </summary>
   //         /// <param name="name"></param>
   //         public void ShowHzb()//string name)
   //         {
   //             //_name = name;
   //             string wh = null;
   //             if (_p._zone != null)
   //             {
   //                 //var en = new ATT_SUMMERY();
   //                 wh = "FBFBM like '" + _p._zone.Code + "%'";
   //             }
   //             UpdateTable(wh);
   //         }
   //         public int RowCount
   //         {
   //             get { return _data.Values.Count; }
   //         }
   //         public bool IsNumericField(int col)
   //         {
   //             return false;// _data.Columns[col].IsNumericField;
   //         }

   //         //private void ShowAll(string wh = null)
   //         //{
   //         //    var att = new Entity<ATT_SUMMERY>();
   //         //    att.GetProperties(_data.Columns);
   //         //    UpdateTable(wh);
   //         //}
   //         private void UpdateTable(string wh = null)
   //         {
   //             var clms = _data.Columns;
   //             _data.ClearValues();
   //             var _values = _data.Values;

   //             grid.Cols = clms.Count;
   //             for(int c = 0; c < clms.Count; ++c)
   //             {
   //                 grid.SetColLabel(c, clms[c]);
   //             }
   
   //             var sql = @"select CBJYQZBM as 登记簿代码,b.CBFMC as 承包方名称,b.CBFZJHM as 承包方证件号码,CBFS as 承包方式,CBQX as 承包期限,DJSJ as 等级时间 from DJ_CBJYQ_DJB a
   //left join DJ_CBJYQ_CBF b on a.CBFBM = b.CBFBM";
   //             if (!string.IsNullOrEmpty(wh))
   //             {
   //                 sql += " where " + wh;
   //             }
   //             _db.QueryCallback(sql, r =>
   //             {
   //                 var rd = new RowData();
   //                 _values.Add(rd);
   //                 for (int c = 0; c < clms.Count; ++c)
   //                 {
   //                     var v = r.GetValue(c);
   //                     rd.Add(v);
   //                 }
   //                 return true;
   //             });
   //             grid.Rows = _values.Count;
   //             _p.UpdateColumnWidth();
   //             grid.UpdateLayout();
   //             grid.Refresh();
   //         }
   //         public string OnGetCellText(int r, int c)
   //         {
   //             var o = _data.GetValue(r, c);
   //             return o == null ? "" : o.ToString();
   //         }
   //     }

        ///// <summary>
        ///// 以表格方式显示指定表的数据
        ///// 指定表必须包含objectid字段
        ///// </summary>
        //class TableSheet: ICurrentSheet
        //{
        //    interface ITableData
        //    {
        //        string OidFieldName { get; set; }
        //        void BuildIndex(string wh);
        //        //RowData GetRowData(int row);
        //        string OnGetCellText(int row, int col);
        //        int RowCount { get; }
        //    }
        //    class OidTableData : ITableData
        //    {
        //        private readonly List<int> _oids = new List<int>();
        //        private readonly Dictionary<int, RowData> _dicOidRow = new Dictionary<int, RowData>();
        //        private readonly TableSheet _p;

        //        public OidTableData(TableSheet p,string oidFieldName="BSM")
        //        {
        //            _p = p;
        //            OidFieldName = oidFieldName;
        //        }
        //        #region ITableData
        //        public string OidFieldName { get; set; }
        //        public void BuildIndex(string wh)
        //        {
        //            _oids.Clear();
        //            var sql = "select " + OidFieldName + " from " + _p._entity.TableName;// _tableName;
        //            if (!string.IsNullOrEmpty(wh))
        //            {
        //                sql += " where " + wh;
        //            }
        //            _p._p._db.QueryCallback(sql, r =>
        //            {
        //                var o = r.GetValue(0);
        //                if (o != null)
        //                {
        //                    var oid = SafeConvertAux.ToInt32(o);
        //                    _oids.Add(oid);
        //                }
        //                return true;
        //            });
        //            _dicOidRow.Clear();
        //            refreshCache(0);
        //        }

        //        public int RowCount { get { return _oids.Count; } }
        //        public string OnGetCellText(int row, int col)
        //        {
        //            object o = null;
        //            if (row >= 0 && row < _oids.Count)
        //            {
        //                //var oid = _oids[row];
        //                var v = GetRowData(row);// getRowDataByOid(oid);
        //                if (v != null)
        //                {
        //                    o = v[col];
        //                }
        //                else
        //                {
        //                    if (_dicOidRow.Count > 1000)
        //                    {
        //                        _dicOidRow.Clear();
        //                    }
        //                    refreshCache(row);
        //                    v = GetRowData(row);// getRowDataByOid(oid);
        //                    if (v != null)
        //                    {
        //                        o = v[col];
        //                    }
        //                }
        //            }
        //            return o == null ? "" : o.ToString();
        //            //var o = _data.GetValue(r, c);
        //            //return o == null ? "" : o.ToString();
        //        }
        //        #endregion
        //        private RowData GetRowData(int row)
        //        {
        //            int oid = _oids[row];
        //            //return _dicOidRow[oid];
        //            RowData val;
        //            if (_dicOidRow.TryGetValue(oid, out val))
        //            {
        //                return val;
        //            }

        //            return null;
        //        }
        //        private void refreshCache(int row)
        //        {
        //            if (row < 0 || row >= _oids.Count)
        //                return;

        //            var lstOid = new List<int>();
        //            for (int r = row; r < _oids.Count; ++r)
        //            {
        //                var oid = _oids[r];
        //                if (!_dicOidRow.ContainsKey(oid))
        //                {
        //                    lstOid.Add(oid);
        //                }
        //                if (lstOid.Count > 50)
        //                {
        //                    break;
        //                }
        //            }
        //            for (int r = row; r >= 0; --r)
        //            {
        //                var oid = _oids[r];
        //                if (!_dicOidRow.ContainsKey(oid))
        //                {
        //                    lstOid.Add(oid);
        //                }
        //                if (lstOid.Count > 50)
        //                {
        //                    break;
        //                }
        //            }
        //            string subFields = OidFieldName;// "objectid";
        //            for (int i = 0; i < _p.Columns.Count; ++i)
        //            {
        //                subFields += "," + _p.Columns[i].ColumnName;
        //            }

        //            constructIn(lstOid, sin =>
        //            {
        //                var tableName = _p._entity.TableName;
        //                var sql = "select " + subFields + " from " + tableName + " where " + OidFieldName + " in(" + sin + ")";
        //                _p._p._db.QueryCallback(sql, r =>
        //                {
        //                    var dr = new RowData();
        //                    var oid = SafeConvertAux.ToInt32(r.GetValue(0));
        //                    for (int i = 0; i < _p.Columns.Count; ++i)
        //                    {
        //                        var o = r.GetValue(i + 1);
        //                        dr.Add(o);
        //                    }
        //                    _dicOidRow[oid] = dr;
        //                    return true;
        //                });
        //            });
        //        }
        //        private void constructIn(List<int> oids, Action<string> callback)
        //        {
        //            int i = 0;
        //            while (i < oids.Count)
        //            {
        //                int j = i + 50;
        //                if (j > oids.Count)
        //                {
        //                    j = oids.Count;
        //                }
        //                string sin = null;
        //                for (int k = i; k < j; ++k)
        //                {
        //                    var oid = oids[i];
        //                    if (sin == null)
        //                    {
        //                        sin = oid + "";
        //                    }
        //                    else
        //                    {
        //                        sin += "," + oid;
        //                    }
        //                }
        //                if (sin != null)
        //                {
        //                    callback(sin);
        //                }
        //                i = j;
        //            }
        //        }
        //    }
        //    class IDTableData : ITableData
        //    {
        //        private readonly List<string> _oids = new List<string>();
        //        private readonly Dictionary<string, RowData> _dicOidRow = new Dictionary<string, RowData>();
        //        private readonly TableSheet _p;
        //        //private string OidFieldName;
        //        public IDTableData(TableSheet p, string oidFieldName = "ID")
        //        {
        //            _p = p;
        //            OidFieldName = oidFieldName;
        //        }
        //        #region ITableData
        //        public string OidFieldName { get; set; }
        //        public void BuildIndex(string wh)
        //        {
        //            _oids.Clear();
        //            var sql = "select " + OidFieldName + " from " + _p._entity.TableName;// _tableName;
        //            if (!string.IsNullOrEmpty(wh))
        //            {
        //                sql += " where " + wh;
        //            }
        //            _p._p._db.QueryCallback(sql, r =>
        //            {
        //                var o = r.GetValue(0);
        //                if (o != null)
        //                {
        //                    var oid = o.ToString();// SafeConvertAux.ToInt32(o);
        //                    _oids.Add(oid);
        //                }
        //                return true;
        //            });
        //            _dicOidRow.Clear();
        //            refreshCache(0);
        //        }

        //        public int RowCount { get { return _oids.Count; } }
        //        public string OnGetCellText(int row, int col)
        //        {
        //            object o = null;
        //            if (row >= 0 && row < _oids.Count)
        //            {
        //                //var oid = _oids[row];
        //                var v = GetRowData(row);// getRowDataByOid(oid);
        //                if (v != null)
        //                {
        //                    o = v[col];
        //                }
        //                else
        //                {
        //                    if (_dicOidRow.Count > 1000)
        //                    {
        //                        _dicOidRow.Clear();
        //                    }
        //                    refreshCache(row);
        //                    v = GetRowData(row);// getRowDataByOid(oid);
        //                    if (v != null)
        //                    {
        //                        o = v[col];
        //                    }
        //                }
        //            }
        //            return o == null ? "" : o.ToString();
        //            //var o = _data.GetValue(r, c);
        //            //return o == null ? "" : o.ToString();
        //        }
        //        #endregion
        //        private RowData GetRowData(int row)
        //        {
        //            var oid = _oids[row];
        //            //return _dicOidRow[oid];
        //            RowData val;
        //            if (_dicOidRow.TryGetValue(oid, out val))
        //            {
        //                return val;
        //            }

        //            return null;
        //        }
        //        private void refreshCache(int row)
        //        {
        //            if (row < 0 || row >= _oids.Count)
        //                return;

        //            var lstOid = new List<string>();
        //            for (int r = row; r < _oids.Count; ++r)
        //            {
        //                var oid = _oids[r];
        //                if (!_dicOidRow.ContainsKey(oid))
        //                {
        //                    lstOid.Add(oid);
        //                }
        //                if (lstOid.Count > 50)
        //                {
        //                    break;
        //                }
        //            }
        //            for (int r = row; r >= 0; --r)
        //            {
        //                var oid = _oids[r];
        //                if (!_dicOidRow.ContainsKey(oid))
        //                {
        //                    lstOid.Add(oid);
        //                }
        //                if (lstOid.Count > 50)
        //                {
        //                    break;
        //                }
        //            }
        //            string subFields = OidFieldName;// "objectid";
        //            for (int i = 0; i < _p.Columns.Count; ++i)
        //            {
        //                subFields += "," + _p.Columns[i].ColumnName;
        //            }

        //            constructIn(lstOid, sin =>
        //            {
        //                var tableName = _p._entity.TableName;
        //                var sql = "select " + subFields + " from " + tableName + " where " + OidFieldName + " in(" + sin + ")";
        //                _p._p._db.QueryCallback(sql, r =>
        //                {
        //                    var dr = new RowData();
        //                    var oid = SafeConvertAux.ToStr(r.GetValue(0));// SafeConvertAux.ToInt32(r.GetValue(0));
        //                    for (int i = 0; i < _p.Columns.Count; ++i)
        //                    {
        //                        var o = r.GetValue(i + 1);
        //                        dr.Add(o);
        //                    }
        //                    _dicOidRow[oid] = dr;
        //                    return true;
        //                });
        //            });
        //        }
        //        private void constructIn(List<string> oids, Action<string> callback)
        //        {
        //            int i = 0;
        //            while (i < oids.Count)
        //            {
        //                int j = i + 50;
        //                if (j > oids.Count)
        //                {
        //                    j = oids.Count;
        //                }
        //                string sin = null;
        //                for (int k = i; k < j; ++k)
        //                {
        //                    var oid ="'"+ oids[i]+"'";
        //                    if (sin == null)
        //                    {
        //                        sin = oid;
        //                    }
        //                    else
        //                    {
        //                        sin += "," + oid;
        //                    }
        //                }
        //                if (sin != null)
        //                {
        //                    callback(sin);
        //                }
        //                i = j;
        //            }
        //        }
        //    }
        //    private readonly List<EntityProperty> Columns = new List<EntityProperty>();
        //    public IEntity _entity;
        //    private List<double> _lstColWidth = new List<double>();
        //    private string _where;
        //    private ContentPanel _p;
        //    private QueryTypeItem _qti;
        //    private ITableData _data;
        //    private OidTableData _oidTableData;
        //    private IDTableData _idTableData;
        //    public TableSheet(ContentPanel p)
        //    {
        //        _p = p;
        //        //OidFieldName = oidFieldName;
        //        _oidTableData = new OidTableData(this);
        //        _idTableData = new IDTableData(this);
        //        _data = _oidTableData;
        //    }
        //    public void Update(QueryTypeItem qti,string wh)
        //    {
        //        _qti = qti;
        //        _entity = qti.entity;
        //        if (qti.OidFieldName == "ID")
        //        {
        //            _data = _idTableData;
        //        }else
        //        {
        //            _data = _oidTableData;
        //        }
        //        _data.OidFieldName = qti.OidFieldName;
        //        if (string.IsNullOrEmpty(wh))
        //        {
        //            _where = GetWhere();
        //        }else
        //        {
        //            string nw = wh;
        //            if (_p._zone != null)
        //            {
        //                wh = _qti.ZoneCodeFieldName + " like '" + _p._zone.Code + "%'";
        //            }
        //            if (wh != null && nw != null)
        //            {
        //                _where = wh + " and (" + nw + ")";
        //            }
        //        }
        //        Update(_where);
        //    }
        //    public void Update(string wh)
        //    {
        //        var subFields = updateColumns();
        //        _data.BuildIndex(wh);
        //        //buildOidIndex(wh);
        //        _p.grid.Rows = _data.RowCount;// _oids.Count;
        //        //_dicOidRow.Clear();
        //        //refreshCache(0);

        //        _p.UpdateColumnWidth();
        //        _p.grid.Refresh();
        //    }
        //    public int RowCount
        //    {
        //        get { return _data.RowCount; }// _oids.Count; }
        //    }
        //    public bool IsNumericField(int col)
        //    {
        //        return Columns[col].IsNumericField;
        //    }

        //    private string updateColumns()
        //    {
        //        _lstColWidth.Clear();
        //        _entity.GetProperties(Columns,false);
        //        var clms = Columns;
        //        var grid = _p.grid;
        //        grid.Cols = clms.Count;
        //        string subFields = null;
        //        for (int i = 0; i < grid.Cols; ++i)
        //        {
        //            var cl = clms[i];
        //            var colLabel = cl.AliasName;
        //            grid.SetColLabel(i, colLabel);
        //            var colWidth =Math.Min(200, grid.CalcTextWidth(colLabel) + 10);
        //            _lstColWidth.Add(colWidth);
        //            grid.SetColWidth(i, (int)colWidth);

        //            if (subFields == null)
        //            {
        //                subFields = cl.ColumnName;
        //            }
        //            else
        //            {
        //                subFields += "," + cl.ColumnName;
        //            }
        //        }
        //        return subFields;
        //    }

        //    private string GetWhere()
        //    {
        //        string wh = null;
        //        if (_p._zone != null)
        //        {
        //            wh = _qti.ZoneCodeFieldName + " like '" + _p._zone.Code + "%'";
        //        }
        //        if (wh == null)
        //        {
        //            wh = GetSimpleWhere();
        //        }else
        //        {
        //            var swh = GetSimpleWhere();
        //            if (swh != null)
        //            {
        //                wh = wh + " and " + swh;
        //            }
        //        }
        //        return wh;
        //    }
        //    /// <summary>
        //    /// 获取上边面板设置的简单条件
        //    /// </summary>
        //    /// <returns></returns>
        //    private string GetSimpleWhere()
        //    {
        //        string wh = null;
        //        if (_p.cbCxlb.SelectedItem != null
        //            &&_p.cbCxtj.SelectedItem!=null
        //            &&_p.cbCxzd.SelectedItem!=null
        //            &&!string.IsNullOrEmpty(_p.tbKeywords.Text.Trim()))
        //        {
        //            var field = _p.cbCxzd.SelectedItem as FieldColumn;
        //            var fStringField = field.ColumnAttribute.FieldType == typeof(string);
        //            //cbCxtj "等于", "不等于", "大于", "大于等于", "小于", "小于等于", "开头是", "开头不是", "包含", "不包含", "结尾是", "结尾不是" };
        //            var cc = _p.cbCxtj.SelectedItem as CompareCondition;
        //            string sCondition =cc.Name;
        //            string keyWords = _p.tbKeywords.Text.Trim();
        //            wh = field.ColumnAttribute.ColumnName + sCondition;
        //            switch (cc.DisplayName)
        //            {
        //                case "开头是":
        //                case "开头不是":
        //                    wh += "'" + keyWords + "%'";
        //                    break;
        //                case "包含":
        //                case "不包含":
        //                    wh += " '%" + keyWords + "%'";
        //                    break;
        //                case "结尾是":
        //                case "结尾不是":
        //                    wh += "'%" + keyWords + "'";
        //                    break;
        //                default:
        //                    if (fStringField)
        //                    {
        //                        wh += "'" + keyWords + "'";
        //                    }else
        //                    {
        //                        wh += keyWords;
        //                    }
        //                    break;
        //            }
        //        }
        //        return wh;
        //    }

        //    public string OnGetCellText(int row, int col)
        //    {
        //        return _data.OnGetCellText(row, col);
        //        /*
        //        object o = null;
        //        if (row >= 0 && row < _oids.Count)
        //        {
        //            var oid = _oids[row];
        //            var v = getRowDataByOid(oid);
        //            if (v != null)
        //            {
        //                o= v[col];
        //            }
        //            else
        //            {
        //                if (_dicOidRow.Count > 1000)
        //                {
        //                    _dicOidRow.Clear();
        //                }
        //                refreshCache(row);
        //                v = getRowDataByOid(oid);
        //                if (v != null)
        //                {
        //                    o= v[col];
        //                }
        //            }
        //        }
        //        return o==null?"":o.ToString();
        //        //var o = _data.GetValue(r, c);
        //        //return o == null ? "" : o.ToString();
        //        */
        //    }
        //}




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

                var dlg = new KuiDialog(wnd, title);// "自定义查询");

                //var qti = cbCxlb.SelectedItem as QueryTypeItem;
                //var pnl = new TableFilterDialog(qti, _fieldCombo.GetColumns(qti), _db, _strCustomWhere);
                dlg.Content = pnl;
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
        //private readonly DjbHzbSheet _DjbHzbSheet;
        //private readonly TableSheet _tableSheet;
        //private readonly StatisticsSheet _statisticsSheet;
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
            //_DjbHzbSheet = new DjbHzbSheet(this);
            //_tableSheet = new TableSheet(this);
            //_statisticsSheet = new StatisticsSheet(this);
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

			cbFields.ItemsSource = new List<ComboItemData>();
			txtSearch.OnButtonClick += () =>
			{
				var wh = GetQueryWhere();
				//if (!string.IsNullOrEmpty(wh))
				//{
					ShowHzb(_hzbSheet._title, _hzbSheet._tableName, _hzbSheet._idFieldName, _hzbSheet._dkbmFieldName, _hzbSheet._excludeFieldNames,wh);
				//}
			};

			Init();

            //ShowHzb(null);
            //cbCxlb.SelectedIndex = 0;
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
        public void OnZoneChanged(ShortZone zone)//Captain.Library.Exchange.Zone zone)
        {
            _zone = zone;
            //if (_currentSheet == _tableSheet)
            //{
            //    DoQuery();
            //} else if (_currentSheet == _hzbSheet)
            //{
				
                ShowHzb(_hzbSheet._title,_hzbSheet._tableName,_hzbSheet._idFieldName,_hzbSheet._dkbmFieldName, _hzbSheet._excludeFieldNames);
            //} else if (_currentSheet== _DjbHzbSheet) {
            //    _DjbHzbSheet.ShowHzb();
            //} else
            //{
            //   // _statisticsSheet.Update();
            //}
			UpdateComboSource();
		}
        /// <summary>
        /// 显示汇总表（6个固定格式的汇总表）
        /// tableAlias参见：HzbTitleConstants；
        /// 例外：tableAlias传入 null则显示所有字段；
        /// </summary>
        /// <param name="title"></param>
        /// <param name="wh"></param>
        public void ShowHzb(string title,string tableName,string idFieldName,string dkbmFieldName, HashSet<string> excludeFieldNames,string wh=null)
        {
            //if (title != HzbTitleConstants.Hzb7)
            //{
                _currentSheet = _hzbSheet;

                _hzbSheet.ShowHzb(title,tableName,idFieldName,dkbmFieldName,_zone?.Code, excludeFieldNames,wh);
            //}else
            //{
            //    _currentSheet = _DjbHzbSheet;
            //    _DjbHzbSheet.ShowHzb();
            //}
            grid.EnsureRowVisible(0);
            grid.EnsureColVisible(0);
			UpdateComboSource();

		}
        private void Init()
        {
            //cbCxlb.ItemsSource = new QueryTypeItem[] {
            //   new QueryTypeItem("发包方","ID",new Entity<ATT_FBF>(),()=> {var en=new ATT_FBF(); return nameof(en.FBFBM); }),
            //    new QueryTypeItem("承包方","ID",new Entity<ATT_CBF>(),()=> {var en=new ATT_CBF(); return nameof(en.CBFBM); }),
            //    new QueryTypeItem("家庭成员","ID",new Entity<ATT_CBF_JTCY>(),()=> {var en=new ATT_CBF_JTCY(); return nameof(en.CBFBM); }),
            //    new QueryTypeItem("地块","BSM",new Entity<VEC_CBDK>(),()=> {var en=new VEC_CBDK(); return nameof(en.DKBM); }),
            //    new QueryTypeItem("承包地信息","ID",new Entity<ATT_CBDKXX>(),()=> {var en=new ATT_CBDKXX(); return nameof(en.DKBM); }),
            //    new QueryTypeItem("承包合同","ID",new Entity<ATT_CBHT>(),()=> {var en=new ATT_CBHT(); return nameof(en.CBHTBM); }),
            //    new QueryTypeItem("登记簿","ID",new Entity<ATT_CBJYQDJBEXP>(),()=> {var en=new ATT_CBJYQDJBEXP(); return nameof(en.CBJYQZBM); }),
            //     new QueryTypeItem("权证","ID",new Entity<ATT_CBJYQZ>(),()=> {var en=new ATT_CBJYQZ(); return nameof(en.CBJYQZBM); })
            //};
            //cbCxtj.ItemsSource = new CompareCondition[] {
            //    new CompareCondition("等于","="),
            //    new CompareCondition("不等于","<>"),
            //    new CompareCondition("大于",">"),
            //    new CompareCondition("大于等于",">="),
            //    new CompareCondition("小于","<"),
            //    new CompareCondition("小于等于","<="),
            //    new CompareCondition("开头是"," like "),
            //    new CompareCondition("开头不是"," not like "),
            //    new CompareCondition("包含"," like "),
            //    new CompareCondition("不包含"," not like "),
            //    new CompareCondition("结尾是"," like "),
            //    new CompareCondition("结尾不是"," not like ") };
            ////cbCxtj.SelectedIndex = 0;
            //cbCxlb.SelectionChanged += (s, e) =>
            //{
            //    var tableAlias = cbCxlb.SelectedItem as QueryTypeItem;
            //    if (tableAlias != null)
            //    {
            //        var lst =_fieldCombo.GetColumns(tableAlias);
            //        cbCxzd.ItemsSource = lst;
            //    }
            //};
            grid.OnGetCellText += (r, c) =>
            {
                if (_currentSheet != null)
                {
                    return _currentSheet.OnGetCellText(r, c);
                }
                return "";
            };
            //btnQuery.Click += (s, e) =>
            //{
            //    DoQuery();
            //};

            //#region 自定义查询
            //btnCustomQuery.Click += (s, e) =>
            //{
            //    //var dlg = new TableFilterDialog();
            //    //_ctx.Workpage.Page.ShowDialog(dlg);
            //    var qti = cbCxlb.SelectedItem as QueryTypeItem;
            //    var pnl = new TableFilterDialog(qti, _fieldCombo.GetColumns(qti), _db, _strCustomWhere);


            //    var dlg = new ShowDialogUtil(this);//.bdrMask);
            //    dlg.ShowDialog("自定义查询", pnl, () =>
            //       {
            //           try
            //           {
            //               var wh = pnl.GetWhere();
            //               if (string.IsNullOrEmpty(wh))
            //               {
            //                   MessageBox.Show("未输入查询表达式！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            //                   return false;
            //               }
            //               var err = pnl.CheckWhere();
            //               if (err != null)
            //               {
            //                   MessageBox.Show(err, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            //                   return false;
            //               }
            //               _strCustomWhere = wh;
            //               DoQuery(wh);
            //               return true;
            //           }
            //           catch (Exception ex)
            //           {
            //                   MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            //           }
            //           return false;
            //       });
            // };
            //#endregion

            //#region 自定义统计
            //btnCustomStatistics.Click += (s, e) =>
            //{
            //    if (_zone == null)
            //    {
            //        MessageBox.Show("请先选择当前地域！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            //        return;
            //    }
            //    var dlg = new ShowDialogUtil(this);//.bdrMask);
            //    var pnl = new StatisticsSetupPanel();
            //    dlg.ShowDialog("自定义统计", pnl, () =>
            //    {
            //        try
            //        {
            //            var lst = pnl.GetTjlbs();
            //            if (lst.Count == 0)
            //            {
            //                MessageBox.Show("未选择统计类别！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            //                return false;
            //            }
            //            _currentSheet = _statisticsSheet;
            //            _statisticsSheet.Update(lst);
            //            _statisticsSheet.OnZoneChanged(_zone);
            //            updateColumnWidth();
            //            return true;
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            //        }
            //        return false;
            //    });
            //};
            //#endregion

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

            //tbKeywords.KeyDown += (s, e) =>
            //{
            //    if (e.Key == Key.Enter)
            //    {
            //        DoQuery();
            //    }
            //};
        }

		private void UpdateComboSource()
		{
			var lst = cbFields.ItemsSource as List<ComboItemData>;
			lst.Clear();
			//cbFields.ItemsSource = null;

			foreach (var field in _hzbSheet._data.Columns)
			{
				lst.Add(new ComboItemData(field));//.FieldName, field.FieldType));
			}

			//if (mdbView.Visibility == Visibility.Visible)
			//{
			//	lst.AddRange(_mdbTableData.Columns);
			//	lst.RemoveAll(i => { return i.FieldType != eFieldType.eFieldTypeString; });
			//}
			//else
			//{
			//	IFeatureClass fc = dbfView.Tag as IFeatureClass;
			//	if (fc != null)
			//	{
			//		var fields = fc.Fields;
			//		for (int i = 0; i < fields.FieldCount; ++i)
			//		{
			//			var field = fields.GetField(i);
			//			if (field.FieldType == eFieldType.eFieldTypeString)//!(field.FieldType == eFieldType.eFieldTypeOID || field.FieldType == eFieldType.eFieldTypeGeometry))
			//			{
			//				lst.Add(new ComboItemData(field.FieldName, field.FieldType));// field.FieldName);
			//			}
			//		}
			//	}
			//}
			//cbFields.ItemsSource = lst;
		}

		private string GetQueryWhere()
		{
			string where = null;
			var filterText = txtSearch.Text.Trim();
			if (!string.IsNullOrEmpty(filterText))
			{
				if (cbFields.SelectedItem is ComboItemData cid)
				{
					var fieldName = cid._field.FieldName;
					//if (fShpView)
					//{
					//	fieldName = "[" + fieldName + "]";
					//}
					if (checkBoxFuzzy.IsChecked == true)
					{
						where = fieldName + " = '" + filterText.Replace("'", "''") + "'";
					}
					else
					{
						where = fieldName + " like '%" + filterText.Replace("'", "''") + "%'";
					}
				}
			}
			return where;
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

        private void UpdateColumnWidth()
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

        //private void DoQuery(string wh=null)
        //{
        //    //_currentSheet = _tableSheet;
        //    var ti= cbCxlb.SelectedItem as QueryTypeItem;
        //    if (ti != null)
        //    {
        //        _tableSheet.Update(ti,wh);
        //        grid.EnsureRowVisible(0);
        //        grid.EnsureColVisible(0);
        //    }
        //}
    }
}
