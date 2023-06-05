/*
yxm created at 2019/5/16 14:47:33
*/
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;

namespace Agro.Module.DataSync
{
	/// <summary>
	/// ExportNwDataPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ExportWwDataPanel : TaskPropertyPage
	{
		class Item
		{
			public string CBFMC { get; set; }
			public int ZCYS { get; set; }
			public int ZDKS { get; set; }
			public double DKZMJM { get; set; }
		}
		private readonly string PSKEY = "DB07E317-E3B3-4D2A-8D44-584DC0B268FA";
		private readonly List<Item> _items = new List<Item>();
		public ExportWwDataPanel()
		{
			InitializeComponent();
			tbSaveFileName.Filter = "外网交换包(*.wdb)|*.wdb";
			base.DialogWidth = 700;
			DialogHeight = 450;
			dpLastCjrq.SelectedDate = DateTime.Now.AddMonths(-1);
			listView1.ItemsSource = _items;
			LoadSeting();

			_onPreShow+=d=>RefreshList();
			dpLastCjrq.SelectedDateChanged += (s, e) => RefreshList();
		}
		public string SaveFileName { get; private set; }
		/// <summary>
		/// 最后创建日期
		/// </summary>
		public DateTime? LastRiqi { get; private set; }
		public override string Apply()
		{
			SaveFileName = tbSaveFileName.Text.Trim();
			if (string.IsNullOrEmpty(SaveFileName))
			{
				return "未设置数据保存路径";
			}
			if (_items.Count == 0)
			{
				return "无数据可供导出！";
			}
			LastRiqi = dpLastCjrq.SelectedDate;
			SaveSeting();
			return null;
		}

		void RefreshList()
		{
			Try.Catch(() =>
			{
				_items.Clear();
				if (MyGlobal.Workspace == null)
				{
					return;
				}
				var rq = dpLastCjrq.SelectedDate;
				var wh1 = $"DJBID in (select distinct ID from DJ_CBJYQ_DJB where QSZT<>0 and convert(date,ZHXGSJ)>=convert(date,'{rq}'))";
				var dic = new Dictionary<string, Tuple<int, double>>();
				var sql = $"select CBFBM,COUNT(*),sum(HTMJM) from DJ_CBJYQ_DKXX where {wh1} group by CBFBM";
				MyGlobal.Workspace.QueryCallback(sql, r =>
				 {
					 var cbfbm = SafeConvertAux.ToStr(r.GetValue(0));
					 int zdks = SafeConvertAux.ToInt32(r.GetValue(1));
					 double zmj = SafeConvertAux.ToDouble(r.GetValue(2));
					 dic[cbfbm] = new Tuple<int, double>(zdks, zmj);
					 return true;
				 });
				sql = $"select CBFBM,CBFMC,CBFCYSL from DJ_CBJYQ_CBF where {wh1}";
				MyGlobal.Workspace.QueryCallback(sql, r =>
				 {
					 var cbfbm = SafeConvertAux.ToStr(r.GetValue(0));
					 var it = new Item()
					 {
						 CBFMC = SafeConvertAux.ToStr(r.GetValue(1)),
						 ZCYS = SafeConvertAux.ToInt32(r.GetValue(2)),
					 };
					 if (dic.TryGetValue(cbfbm, out Tuple<int, double> t))
					 {
						 it.ZDKS = t.Item1;
						 it.DKZMJM = t.Item2;
					 }
					 _items.Add(it);
					 return true;
				 });

				listView1.ItemsSource = null;
				listView1.ItemsSource = _items;
				oc1.Header = $"简要信息（{_items.Count}）条";
			});
		}
		private void LoadSeting()
		{
			var s = MyGlobal.Persist.LoadSettingInfo(PSKEY) as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbSaveFileName.Text = s;
			}
		}
		private void SaveSeting()
		{
			if (!string.IsNullOrEmpty(SaveFileName))
			{
				MyGlobal.Persist.SaveSettingInfo(PSKEY, SaveFileName);
			}
		}
	}
}
