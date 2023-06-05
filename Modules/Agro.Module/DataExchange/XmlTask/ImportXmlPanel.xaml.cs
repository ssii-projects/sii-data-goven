/*
yxm created at 2019/5/16 14:47:33
*/
using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Xml;

namespace Agro.Module.DataExchange.XmlTask
{
	/// <summary>
	/// ExportNwDataPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ImportXmlPanel : TaskPropertyPage
	{
		public class Item: NotificationObject
		{
			private bool _isChecked;

			public bool IsChecked
			{
				get
				{
					return _isChecked;
				}
				set
				{
					_isChecked = value;
					RaisePropertyChanged(nameof(IsChecked));
				}
			}
			public string XmlFileName { get {return FileInfo.FullName; } }
			internal readonly System.IO.FileInfo FileInfo;
			public Item(System.IO.FileInfo fi)
			{
				FileInfo = fi;
			}
		}
		private readonly string PSKEY= "BF997562-0606-4657-8DC0-706DE7DBEC80";

		internal readonly ObservableCollection<Item> Items = new ObservableCollection<Item>();

		private readonly Action onApply;

		public ImportXmlPanel(Action apply)//string Filter = "Xml数据包(*.xml)|*.xml")
		{
			onApply = apply;
			InitializeComponent();
			//tbFileName.Filter =Filter;
			base.DialogWidth =840;
			DialogHeight = 550;
			listView.ItemsSource = Items;
			tbFileName.TextChanged += (s, e) => RefreshList();
			_onPreShow += d => RefreshList();
			LoadSeting();
		}
		//public XmlDocument Xml { get; private set; }
		public string FilePath { get; private set; }
		public override string Apply()
		{
			FilePath = tbFileName.Text.Trim();
			if (string.IsNullOrEmpty(FilePath))
			{
				return "未设置数据文件路径";
			}

			int cnt = 0;
			foreach (var i in Items)
			{
				if (i.IsChecked) ++cnt;
			}
			if (cnt == 0)
			{
				return "未选择导入文件";
			}
			try
			{
				/*				
				 Xml = new XmlDocument();
				Xml.Load(FilePath);
				if (null == Xml.SelectSingleNode("/SUBMIT"))
				{
					return $"{FilePath}不是有效的xml文件（根节点名称必须为SUBMIT）";
				}
				var sa = new string[] { "/SUBMIT/HEAD", "/SUBMIT/BUSINESS_DATA", "/SUBMIT/ORIGINAL_DATA"
					, "/SUBMIT/CHANGE_DATA","/SUBMIT/BUSINESS_DATA/YWLSH","/SUBMIT/BUSINESS_DATA/XZQDM"
				,"/SUBMIT/HEAD/SJLX","/SUBMIT/HEAD/FSF"};
				foreach (var s in sa)
				{
					if (null == Xml.SelectSingleNode(s))
					{
						return $"{FilePath}不是有效的xml文件（未找到{s}节点）";
					}
				}
				var sjlx =SafeConvertAux.ToInt32(XmlExchangeUtil.GetXmlInnerText(Xml.SelectSingleNode("/SUBMIT/HEAD/SJLX")));
				if (!BusinessDataFactory.IsValidSjlx(sjlx))
				{
					return $"Xml中的数据类型 {sjlx} 无效！【有效值为 101-107 或 201或202】";
				}
				var xzqdm=XmlExchangeUtil.GetXmlInnerText(Xml.SelectSingleNode("/SUBMIT/BUSINESS_DATA/XZQDM"));
				if (string.IsNullOrEmpty(xzqdm)||xzqdm.Length!=6)
				{
					return "/SUBMIT/BUSINESS_DATA/XZQDM节点内容无效，必须为6位行政区代码";
				}
				var lst = new List<string>();
				MyGlobal.Workspace.QueryCallback("select BM from DLXX_XZDY where JB=4 and BM is not null", r =>
				 {
					 lst.Add(r.GetString(0));
					 return true;
				 });
				if (!lst.Contains(xzqdm))
				{
					if (lst.Count == 1)
					{
						return $"Xml文件中的行政区代码 {xzqdm} 与数据库中的行政区代码 {lst[0]} 不匹配！";
					}
					return $"Xml文件中的行政区代码 {xzqdm} 与数据库中的行政区代码不匹配！";
				}
				*/
				SaveSeting();
				onApply();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return null;
		}

		private void LoadSeting()
		{
			var s = MyGlobal.Persist.LoadSettingInfo(PSKEY) as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbFileName.Text = s;
			}
		}
		private void SaveSeting()
		{
			if (!string.IsNullOrEmpty(FilePath))
			{
				MyGlobal.Persist.SaveSettingInfo(PSKEY, FilePath);
			}
		}

		void RefreshList()
		{
			FilePath = tbFileName.Text;
			var lst = new List<System.IO.FileInfo>();
			FileUtil.EnumFiles2(FilePath, fi =>
			{
				if (fi.Name.EndsWith(".xml") && fi.Name.StartsWith("DJYW"))
				{
					lst.Add(fi);
				}
				return true;
			});
			lst.Sort((a, b) =>
			{
				return a.Name.CompareTo(b.Name);
			});
			foreach (var fi in lst)
			{
				Items.Add(new Item(fi));
			}
		}
		private void Btn_Click(object sender, RoutedEventArgs e)
		{
			var _items = Items;
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
