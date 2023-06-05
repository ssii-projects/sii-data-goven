using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using static Agro.Module.DataExchange.SelectFbfPanel;

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// ExportDkDataPropertyPage1.xaml 的交互逻辑
	/// </summary>
	public partial class ExportDkDataPropertyPage1 : ExportDkDataPropertyPageBase
	{
		public readonly ObservableCollection<FbfItem> _lstFbf = new ObservableCollection<FbfItem>();

		private readonly Action<IEnumerable<FbfItem>> onApply;
		public ExportDkDataPropertyPage1(Action<IEnumerable<FbfItem>> onApply)
		{
			this.onApply = onApply;
			InitializeComponent();
			this.DialogHeight = 320;
			tbFbfBM.OnButtonClick += SelectFbf;
			//tbFbfBM.TextChanged += (s, e) => Fbfbm = tbFbfBM.Text.Trim();
			listView1.ItemsSource = _lstFbf;
			Persist(false);
		}
		///// <summary>
		///// 发包方编码
		///// </summary>
		//public string Fbfbm { get; set; }
		/// <summary>
		/// 导出文件路径
		/// </summary>
		public string ExportFilePath { get; set; }
		public eDatabaseType DatabaseType { get; private set; } = eDatabaseType.SQLite;
		public override string Apply()
		{
			//Fbfbm = tbFbfBM.Text.Trim();
			ExportFilePath = tbFolder.Text;
			if (_lstFbf.Count==0)//string.IsNullOrEmpty(Fbfbm))
			{
				return "未输入发包方编码！";
			}
			if (string.IsNullOrEmpty(ExportFilePath))
			{
				return "未输入导出文件！";
			}
			onApply(_lstFbf);
			Persist();
			//DatabaseType = ExportFilePath.EndsWith(".dk") ? eDatabaseType.SQLite : eDatabaseType.ShapeFile;
			return null;
		}

		private void SelectFbf()
		{
			var pnl = new SelectFbfPanel()
			{
				ContainTownNode=true
			};
			var dlg = new KuiDialog(Window.GetWindow(this), "选择发包方")
			{
				Width = 700,
				Content = pnl
			};
			dlg.BtnOK.Click += (s, e) =>
			{
				var err = pnl.OnApply();
				if (err != null)
				{
					UIHelper.ShowError(dlg, err);
					return;
				}
				tbFbfBM.Tag = pnl.SelectedFbf;//.FbfMC;
				tbFbfBM.Text = pnl.SelectedFbf.FbfBM;
				listView1.Visibility = Visibility.Visible;
				DockPanel.SetDock(dp1, Dock.Bottom);
				_lstFbf.Clear();
				_lstFbf.AddRange(pnl._lstFbf);
				dlg.Close();
			};
			dlg.ShowDialog();
		}

		private void Persist(bool fSave = true)
		{
			var key = "F4217A03-7FFD-4891-89C4-FB61F78A4310";
			if (fSave)
			{
				if (!string.IsNullOrEmpty(ExportFilePath))
				{
					MyGlobal.Persist.SaveSettingInfo(key, ExportFilePath);
				}
			}
			else
			{
				var s = MyGlobal.Persist.LoadSettingInfo(key) as string;
				if (!string.IsNullOrEmpty(s))
				{
					tbFolder.Text = s;
				}
			}
		}
	}
}
