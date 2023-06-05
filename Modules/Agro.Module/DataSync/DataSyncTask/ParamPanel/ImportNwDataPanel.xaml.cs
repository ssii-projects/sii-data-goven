/*
yxm created at 2019/5/16 14:47:33
*/
using Agro.GIS;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;

namespace Agro.Module.DataSync
{
	/// <summary>
	/// ExportNwDataPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ImportNwDataPanel : TaskPropertyPage
	{
		private readonly string PSKEY;
		public ImportNwDataPanel(string Filter = "内网交换包(*.ndb)|*.ndb",string psKey= "A9F8A8909-DE45-4A29-B1CC-D7CBF5389F79")
		{
			PSKEY = psKey;
			InitializeComponent();
			tbFileName.Filter =Filter;
			base.DialogWidth = 600;
			DialogHeight = 300;
			//dpLastCjrq.SelectedDate = DateTime.Now;
			LoadSeting();
		}
		//public IFeatureWorkspace db { get; set; } 
		public string FileName { get; private set; }
		///// <summary>
		///// 最后创建日期
		///// </summary>
		//public DateTime? LastRiqi { get; private set; }
		public override string Apply()
		{
			FileName = tbFileName.Text.Trim();
			if (string.IsNullOrEmpty(FileName))
			{
				return "未设置数据文件路径";
			}
			//LastRiqi = dpLastCjrq.SelectedDate;
			SaveSeting();
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
			if (!string.IsNullOrEmpty(FileName))
			{
				MyGlobal.Persist.SaveSettingInfo(PSKEY, FileName);
			}
		}
	}
}
