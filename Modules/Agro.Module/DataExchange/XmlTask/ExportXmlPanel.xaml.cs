using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// ExportXmlPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ExportXmlPanel : TaskPropertyPage
	{
		public class Item: NotificationObject
		{
			private bool _isChecked;

			public bool IsChecked { get
				{
					return _isChecked;
				}
				set
				{
					_isChecked = value;
					RaisePropertyChanged(nameof(IsChecked));
				}
			}
			public string Ywh { get; set; }
			public string Ywmc { get; set; }
			public DateTime? Blsj { get; set; }
			/// <summary>
			/// 登记小类（登记小类 业务接入码  业务类型）
			/// 0 	 101	    首次登记/初始登记
			/// 3 	 102  	转让登记
			/// 5 	 103  	一般登记/其它变更登记
			/// 4 	 104  	互换登记
			/// 1 	 105  	分户登记 
			/// 2 	 106  	合户登记
			/// 6 	 107  	注销登记
			/// 7 	 201  	权证补发
			/// 8 	 202  	权证换发
			/// </summary>
			public int DJXL;
			/// <summary>
			/// 所在地域
			/// </summary>
			public string SZDY;
			/// <summary>
			/// DJ_YW_SLSQ.ID
			/// </summary>
			internal string ID;
			/// <summary>
			/// 登记原因
			/// </summary>
			public string DJYY;
			public int SJLX
			{
				get
				{
					return XmlExchangeUtil.DjxlToSJLX(DJXL);
				}
			}
		}

		private readonly ObservableCollection<Item> _items = new ObservableCollection<Item>();

		private readonly Action onApply;
		public ExportXmlPanel(Action apply)
		{
			onApply = apply;
			InitializeComponent();
			DialogWidth = 800;
			DialogHeight = 550;


			dpLastCjrq.SelectedDate = DateTime.Now.AddMonths(-3);
			listView.ItemsSource = _items;

			LoadState();
			dpLastCjrq.SelectedDateChanged += (s, e) => RefreshList();
			_onPreShow += d => RefreshList();
		}

		/// <summary>
		/// 以/结尾
		/// </summary>
		public string SavePath { get; private set; }
		/// <summary>
		/// 接入码（上一级平台向前置机分配的唯一标识密钥。）
		/// </summary>
		public string Jrm { get; private set; }
		public List<Item> SelectedItems
		{
			get{
				var lst = new List<Item>();
				foreach (var it in _items)
				{
					if (it.IsChecked) lst.Add(it);
				}
				return lst;
			}
		}
		public override string Apply()
		{
			if (string.IsNullOrEmpty(tbPath.Text))
				return "未选择保存路径";
			if (!Directory.Exists(tbPath.Text))
			{
				return $"目录{tbPath.Text}不存在！";
			}
			if (SelectedItems.Count == 0)
			{
				return "未选择导出对象！";
			}
			SavePath = tbPath.Text.Trim();
			if (!SavePath.EndsWith("/") && !SavePath.EndsWith("\\"))
			{
				SavePath += "/";
			}
			Jrm = tbJrm.Text.Trim();
			SaveSate();
			onApply();
			return null;
		}

		void RefreshList()
		{
			_items.Clear();
			var whZhxgsj = $"convert(date,ZHXGSJ)>=convert(date,'{dpLastCjrq.SelectedDate}')";
			var wh = $"AJZT=1 and DJXL >=0 and DJXL<9 and {whZhxgsj}";

			#region test
#if DEBUG
			wh = $"YWH='54030220190000001353'";//首次登记
			wh = $"YWH='54030220190000001391'";//一般变更登记
			wh = $"YWH='54030220180000001001'";//转让
			wh = $"YWH='54030220190000001474'";//互换
			wh = $"YWH='54030220190000001246'";//分户
			wh = $"YWH='54030220190000001250'";//合户
			//wh = $"YWH='54030220180000000048'";//注销登记
			wh = $"YWH='54030220190000001292'";//权证补发
			wh = $"YWH='54030220190000001287'";//权证换发
#endif
			#endregion

			var sql = $"select YWH,YWMC,ZHXGSJ,DJXL,ID,DJYY,SZDY from DJ_YW_SLSQ where {wh} order by ZHXGSJ desc";
			MyGlobal.Workspace.QueryCallback(sql, r =>
			 {
				 var it = new Item()
				 {
					 Ywh=r.GetString(0),
					 Ywmc=r.GetString(1),
					 //Blsj=r.IsDBNull(2)?null:DateTime.Parse(r.GetValue(2).ToString()),
					 DJXL=SafeConvertAux.ToInt32(r.GetValue(3)),
					 ID=r.GetString(4),
					 DJYY=r.IsDBNull(5)?null:r.GetString(5),
					 SZDY=r.GetString(6),
				 };
				 if (!r.IsDBNull(2))
				 {
					 it.Blsj = DateTime.Parse(r.GetValue(2).ToString());
				 }
				 //if (DateTime.TryParse(it.Blsj, out DateTime dt))
				 //{
					// it.Blsj = dt.ToString("yyyy-MM-dd HH:mm:ss");
				 //}
				 _items.Add(it);
				 return true;
			 });
		}
		void SaveSate()
		{
			MyGlobal.Persist.SaveSettingInfo("F86CE562-6FF1-4CB6-91D2-977066BFD4AB", tbPath.Text);
			MyGlobal.Persist.SaveSettingInfo("F17E14D4-2271-4087-B65A-040002DD3665", tbJrm.Text);
		}
		void LoadState()
		{
			tbPath.Text = MyGlobal.Persist.LoadSettingInfo("F86CE562-6FF1-4CB6-91D2-977066BFD4AB").ToString();
			tbJrm.Text= MyGlobal.Persist.LoadSettingInfo("F17E14D4-2271-4087-B65A-040002DD3665").ToString();
		}
		private void Btn_Click(object sender, RoutedEventArgs e)
		{
			if (sender == btnSelectAll)
			{
				foreach (var it in _items) it.IsChecked = true;
			}
			else if (sender == btnNotSelectAll)
			{
				foreach (var it in _items) it.IsChecked = false;
			}
			else if (sender == btnXorSelect)
			{
				foreach (var it in _items) it.IsChecked = !it.IsChecked;
			}
		}
	}
}
