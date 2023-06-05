using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Agro.Module.DataExchange
{
	/// <summary>
	/// yxm 2019-10-23
	/// 导出DLXX_DK的JSON数据（包含相关的界址点、界址线，图形变更历史记录）
	/// </summary>
	public class ExportDkJsonTask : Task
	{
		public class KeyValue
		{
			public string Key;
			public object Value;
			public KeyValue(string k, object v)
			{
				Key = k;
				Value = v;
			}
		}
		public class KeyValues : List<KeyValue>
		{
			public void Add(string key, object v)
			{
				base.Add(new KeyValue(key, v));
			}
		}
		public class JsonObj
		{
			private readonly List<KeyValues> _lstDk = new List<KeyValues>();
			private readonly List<KeyValues> _lstJzd = new List<KeyValues>();
			private readonly List<KeyValues> _lstJzx = new List<KeyValues>();
			private readonly List<KeyValues> _lstTxbgjl = new List<KeyValues>();

			public void AddDk(KeyValues dk)
			{
				_lstDk.Add(dk);
			}
			public void AddJzd(KeyValues jzd)
			{
				_lstJzd.Add(jzd);
			}
			public void AddJzx(KeyValues jzx)
			{
				_lstJzx.Add(jzx);
			}
			public void AddTxbgjl(KeyValues en)
			{
				_lstTxbgjl.Add(en);
			}
			public void Clear()
			{
				_lstDk.Clear();
				_lstJzd.Clear();
				_lstJzx.Clear();
				_lstTxbgjl.Clear();
			}
			public override string ToString()
			{
				var str = "{";
				str += $"{JsonKey("DK")}:{ToJsonString(_lstDk)}";
				str += $",{JsonKey("JZD")}:{ToJsonString(_lstJzd)}";
				str += $",{JsonKey("JZX")}:{ToJsonString(_lstJzx)}";
				str += $",{JsonKey("TXBGJL")}:{ToJsonString(_lstTxbgjl)}";
				str += "}";
				return str;
			}

			private static string ToJsonString(List<KeyValues> lst)
			{
				var str = "[";
				foreach (var v in lst)
				{
					if (str.Length > 1)
						str += ",";
					str += ToJsonObjString(v);
				}
				str += "]";
				return str;
			}
			private static string ToJsonObjString(KeyValues lst)
			{
				var str = "{";
				foreach (var kv in lst)
				{
					if (str.Length > 1)
						str += ",";
					str += $"{JsonKey(kv.Key)}:{JsonValue(kv.Value)}";
				}
				str += "}";
				return str;
			}
			private static string JsonKey(string key)
			{
				return $"\"{key.ToLower()}\"";
			}
			private static string JsonValue(object o)
			{
				if (o == null)
				{
					return "null";
				}

				if (o is DateTime dt)
				{
					var s = dt.ToString("yyyy-MM-dd HH:mm:ss:ms");
					if (s.EndsWith(" 00:00:00:00"))
					{
						s = s.Substring(0, s.Length - " 00:00:00:00".Length);
					}
					o = s;
				}

				if (o is bool b)
				{
					return b ? "1" : "0";
				}
				else if (o is Enum)
				{
					return ((int)o).ToString();
				}

				var v = o.ToString();
				if (o is string str)
				{
					return $"\"{str}\"";
				}
				return v;
			}
		}

		private readonly JsonObj jsonObj = new JsonObj();

		public ExportDkJsonTask()
		{
			Name = "导出地块更新数据包";
			Description = "导出地块更新数据包";
			base.PropertyPage = new ExportJsonPanel();
			base.OnStart += t => ReportInfomation($"开始{Name}");
			base.OnFinish += (t, e) =>
			{
				if (0 == t.ErrorCount())
				{
					ReportInfomation($"结束{Name},耗时：{t.Elapsed}");
				}
			};
		}

		internal static KeyValues FromEntity(DLXX_DK_TXBGJL en)
		{
			var k = new KeyValues
			{
				{ "ID", en.ID },
				{ "DKID", en.DKID },
				{ "DKBM", en.DKBM },
				{ "YDKID", en.YDKID },
				{ "YDKBM", en.YDKBM },
				{ "BGFS", en.BGFS },
				{ "BGYY", en.BGYY }
			};
			return k;
		}

		internal void DoExport(string outJsonFileName,IEnumerable<string> lstDkbm, ICancelTracker cancel)
		{
			//var prm = base.PropertyPage as ExportJsonPanel;
			//var lstDkbm = prm.GetDKBMs();
			jsonObj.Clear();
			var db = MyGlobal.Workspace;
			try
			{
				var cnt = lstDkbm.Count() + QueryCount(db, "DLXX_DK_JZD", lstDkbm)
					+ QueryCount(db, "DLXX_DK_JZX", lstDkbm)
					+ QueryCount(db, "DLXX_DK_TXBGJL", lstDkbm);
				double oldProgress = 0;
				int i = 0;
				FillData(db, "DLXX_DK", lstDkbm, v =>
				{
					ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
					jsonObj.AddDk(v);
				});
				FillData(db, "DLXX_DK_JZD", lstDkbm, v =>
				{
					ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
					jsonObj.AddJzd(v);
				});
				FillData(db, "DLXX_DK_JZX", lstDkbm, v =>
				{
					ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
					jsonObj.AddJzx(v);
				});
				FillTxbgjlData(lstDkbm, v =>
				{
					ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
					jsonObj.AddTxbgjl(v);
				});

				File.WriteAllText(outJsonFileName, jsonObj.ToString());

				ReportProgress(100);
			}
			catch (Exception ex)
			{
				ReportException(ex);
			}
		}

		protected override void DoGo(ICancelTracker cancel)
		{
			var prm = base.PropertyPage as ExportJsonPanel;
			//var lstDkbm = prm.GetDKBMs();
			DoExport(prm.SaveFileName, prm.GetDKBMs(), cancel);
			//jsonObj.Clear();
			//var db = MyGlobal.Workspace;
			//try
			//{
			//	var cnt = lstDkbm.Count()+ QueryCount(db, "DLXX_DK_JZD", lstDkbm)
			//		+ QueryCount(db, "DLXX_DK_JZX", lstDkbm)
			//		+ QueryCount(db, "DLXX_DK_TXBGJL", lstDkbm);
			//	double oldProgress = 0;
			//	int i = 0;
			//	FillData(db, "DLXX_DK", lstDkbm, v =>
			//	{
			//		ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
			//		jsonObj.AddDk(v);
			//	});
			//	FillData(db, "DLXX_DK_JZD", lstDkbm, v =>
			//	{
			//		ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
			//		jsonObj.AddJzd(v);
			//	});
			//	FillData(db, "DLXX_DK_JZX", lstDkbm, v =>
			//	{
			//		ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
			//		jsonObj.AddJzx(v);
			//	});
			//	FillTxbgjlData(lstDkbm, v =>
			//	{
			//		ProgressUtil.ReportProgress(ReportProgress, cnt, ++i, ref oldProgress);
			//		jsonObj.AddTxbgjl(v);
			//	});

			//	File.WriteAllText(prm.SaveFileName, jsonObj.ToString());

			//	ReportProgress(100);
			//}
			//catch (Exception ex)
			//{
			//	ReportException(ex);
			//}
		}
		int QueryCount(IFeatureWorkspace db, string tableName, IEnumerable<string> lstDkbm)
		{
			int cnt = 0;
			SqlUtil.ConstructIn(lstDkbm, sin =>
			{
				cnt += SafeConvertAux.ToInt32(db.QueryOne($"select count(1) from {tableName} where DKBM in({sin})"));
			});
			return cnt;
		}
		void FillTxbgjlData(IEnumerable<string> lstDkbm, Action<KeyValues> action)
		{
			var repos = TxbgjlRepository.Instance;
			SqlUtil.Split(lstDkbm, dkbms =>
			{
				repos.FindCallback1(t => t.BGFS != ETXBGLX.Xinz && dkbms.Contains(t.DKBM), it =>action(FromEntity(it.Item)));
			});
		}
		void FillData(IFeatureWorkspace db, string tableName, IEnumerable<string> lstDkbm, Action<KeyValues> action)
		{
			using (var fc = db.OpenFeatureClass(tableName))
			{
				IQueryFilter qf = new QueryFilter();
				SqlUtil.ConstructIn(lstDkbm, sin =>
				{
					qf.WhereClause = $"DKBM in ({sin})";
					fc.Search(qf, ft =>
					{
						var kvs = new KeyValues();
						for (int i = 0; i < ft.Fields.FieldCount; ++i)
						{
							var field = ft.Fields.GetField(i);
							if (field.FieldType == eFieldType.eFieldTypeGeometry || field.FieldType == eFieldType.eFieldTypeBlob)
								continue;
							var fieldName = field.FieldName;
							if (fieldName == "BSM")
								continue;

							var o = ft.GetValue(i);
							if (o is DateTime dt)
							{
								o = dt.ToString("yyyy-MM-dd HH:mm:ss");
							}
							kvs.Add(fieldName, o);
						}
						action(kvs);
					});
				});
			}
		}
	}
}
