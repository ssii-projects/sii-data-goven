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
	public partial class ExportNwDataPanel : TaskPropertyPage
	{
		class Item
		{
			public string DKBM { get; set; }
			public double DKMJM { get; set; }
			public string DKDZ { get; set; }
			public string DKNZ { get; set; }
			public string DKXZ { get; set; }
			public string DKBZ { get; set; }
		}
		private readonly string PSKEY = "D893A5D3-1B57-42BD-9DDA-C624B2D64B3F";
		private readonly List<Item> _items = new List<Item>();
		public ExportNwDataPanel()
		{
			InitializeComponent();
			tbSaveFileName.Filter = "内网交换包(*.ndb)|*.ndb";// Filter;
			base.DialogWidth = 820;
			DialogHeight = 450;
			dpLastCjrq.SelectedDate = DateTime.Now.AddMonths(-1);
			listView1.ItemsSource = _items;
			LoadSeting();
			RefreshList();
			dpLastCjrq.SelectedDateChanged += (s, e) => RefreshList();
			ckbExportUpdatedData.Checked += (s, e) => RefreshList();
			ckbExportUpdatedData.Unchecked += (s, e) => RefreshList();
		}
		public string SaveFileName { get; private set; }
		/// <summary>
		/// 最后创建日期
		/// </summary>
		public DateTime? LastRiqi { get; private set; }
		public bool ExportUpdatedData { get; private set; }
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
			ExportUpdatedData = ckbExportUpdatedData.IsChecked == true;
			SaveSeting();
			return null;
		}

		static string GE(string fieldName, DateTime? rq)
		{
			return $"convert(date,{fieldName})>=convert(date,'{rq}')";
		}
		void RefreshList()
		{
			_items.Clear();
			var rq = dpLastCjrq.SelectedDate;
			var whZhxgsj = $"({GE("CJSJ", rq)} or {GE("DJSJ", rq)} or {GE("ZHXGSJ", rq)})";
			var sql = $"select DKBM,SCMJM,DKDZ,DKXZ,DKNZ,DKBZ from DLXX_DK where {whZhxgsj}";

			if (ckbExportUpdatedData.IsChecked == true)
			{
				sql+=  " and DKBM in(select DKBM from DJ_CBJYQ_DKXX where DJBID in (select ID from DJ_CBJYQ_DJB where DJYY='批量更新入库'))";				
			}


			MyGlobal.Workspace.QueryCallback(sql, r =>
			 {
				 var it = new Item()
				 {
					 DKBM=r.GetString(0),
					 DKMJM=SafeConvertAux.ToDouble(r.GetValue(1)),
					 DKDZ=SafeConvertAux.ToStr(r.GetValue(2)),
					 DKXZ = SafeConvertAux.ToStr(r.GetValue(3)),
					 DKNZ = SafeConvertAux.ToStr(r.GetValue(4)),
					 DKBZ = SafeConvertAux.ToStr(r.GetValue(5)),
				 };
				 _items.Add(it);
				 return true;
			 });
			listView1.ItemsSource = null;
			listView1.ItemsSource = _items;
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
