/*
yxm created at 2019/5/16 14:47:33
*/
using Agro.GIS;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System;

namespace DataSyncTool
{
	/// <summary>
	/// ExportNwDataPanel.xaml 的交互逻辑
	/// </summary>
	public partial class ExportNwDataPanel : TaskPropertyPage
	{
		public ExportNwDataPanel(string Filter = "内网交换包(*.ndb)|*.ndb")
		{
			InitializeComponent();
			tbSaveFileName.Filter =Filter;
			dpLastCjrq.SelectedDate = DateTime.Now;
			LoadSeting();
		}
		public IFeatureWorkspace db { get; set; } 
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
			LastRiqi = dpLastCjrq.SelectedDate;
			SaveSeting();
			return null;
		}

		private void LoadSeting()
		{
			var s = MyGlobal.Persist.LoadSettingInfo("D893A5D3-1B57-42BD-9DDA-C624B2D64B3F") as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbSaveFileName.Text = s;
			}
		}
		private void SaveSeting()
		{
			if (!string.IsNullOrEmpty(SaveFileName))
			{
				MyGlobal.Persist.SaveSettingInfo("D893A5D3-1B57-42BD-9DDA-C624B2D64B3F", SaveFileName);
			}
		}
	}
}
