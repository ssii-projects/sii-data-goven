/*
yxm created at 2019/5/16 14:47:33
*/
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// ExportNwDataPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ExportJsonPanel : TaskPropertyPage
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
		private readonly string PSKEY = "B4D83217-6F37-40F4-9861-52C24DFFB7B8";
		private readonly List<Item> _items = new List<Item>();
		public ExportJsonPanel()
		{
			InitializeComponent();
			tbSaveFileName.Filter = "BS交换包(*.ndt)|*.ndt";
			base.DialogWidth = 820;
			DialogHeight = 450;
			dpLastCjrq.SelectedDate = DateTime.Now;//.AddMonths(-1);
			listView1.ItemsSource = _items;
			LoadSeting();
			RefreshList();
			dpLastCjrq.SelectedDateChanged += (s, e) => RefreshList();
			//ckbExportUpdatedData.Checked += (s, e) => RefreshList();
			//ckbExportUpdatedData.Unchecked += (s, e) => RefreshList();
			tbRegion.OnButtonClick += () => ShowRegionDialog();
		}
		public string SaveFileName { get; private set; }
		/// <summary>
		/// 最后创建日期
		/// </summary>
		public DateTime? LastRiqi { get; private set; }
		public ShortZone CurrentZone { get; private set; }
		public IEnumerable<string> GetDKBMs()
		{
			return _items.Select(it => it.DKBM);
		}
		public int Count
		{
			get
			{
				return _items.Count;
			}
		}
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
			//ExportUpdatedData = ckbExportUpdatedData.IsChecked == true;
			SaveSeting();
			return null;
		}

		static string GE(string fieldName, DateTime? rq,eDatabaseType dbType)
		{
			if (dbType == eDatabaseType.MySql)
			{
				return $"{fieldName}>='{rq}'";
			}
			return $"convert(date,{fieldName})>=convert(date,'{rq}')";
		}
		void RefreshList()
		{
			_items.Clear();
			var db = MyGlobal.Workspace;
			var rq = dpLastCjrq.SelectedDate;
			//var whZhxgsj = $"({GE("CJSJ", rq)} or {GE("DJSJ", rq)} or {GE("ZHXGSJ", rq)})";
			var wh = $"{GE("CJSJ", rq,db.DatabaseType)}";
			if (CurrentZone != null)
			{
				wh += $" and FBFBM like '{CurrentZone.Code}%'";
			}
			var sql = $"select DKBM,SCMJM,DKDZ,DKXZ,DKNZ,DKBZ from DLXX_DK where {wh} and ZT<>{(int)EDKZT.Lishi} and DJZT={(int)EDjzt.Wdj} and  SJLY is not null";

            db.QueryCallback(sql, r =>
			 {
				 int i = 0;
				 var it = new Item()
				 {
					 DKBM=r.GetString(0),
					 DKMJM=SafeConvertAux.ToDouble(r.GetValue(++i)),
					 DKDZ=SafeConvertAux.ToStr(r.GetValue(++i)),
					 DKXZ = SafeConvertAux.ToStr(r.GetValue(++i)),
					 DKNZ = SafeConvertAux.ToStr(r.GetValue(++i)),
					 DKBZ = SafeConvertAux.ToStr(r.GetValue(++i)),
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

		private void ShowRegionDialog()
		{
			var pnl = new ZoneTree();
			var dlg = new KuiDialog(Window.GetWindow(this), "选择区域")
			{
				Content = pnl
			};
			dlg.BtnOK.Click += (s,e) =>
			  {
				  if (pnl.CurrentZone == null)
				  {
					  MessageBox.Show("请选择地域！","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
					  return;
				  }
				  tbRegion.Text = pnl.GetCurrentPath();
				  this.CurrentZone = pnl.CurrentZone;
				  RefreshList();
				  dlg.Close();
			  };
			dlg.ShowDialog();
		}
	}
}
