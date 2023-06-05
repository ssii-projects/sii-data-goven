using Agro.LibCore.UI;
using Agro.Library.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Agro.Library.Common
{
	/// <summary>
	/// AppOptionPanel.xaml 的交互逻辑
	/// </summary>
	public partial class AppOptionPanel : UserControl, IPropertyPage
	{
		
		public AppOptionPanel()
		{
			InitializeComponent();
			tbDkmjScale.Value = MyGlobal.AppOption.DkmjScale;
			cbExportFbfMode.SelectedIndex= MyGlobal.AppOption.ExportFbfMode == EExportFbfMode.SingleExport ? 0 : 1;
			//tbDksytOutpath.Text = MyGlobal.AppOption.LoadDksytOutpath();
		}

		public string Title { get; set; }

		public string Apply()
		{
			var scale= (int)tbDkmjScale.Value;
			CsSysInfoRepository.Instance.Save(CsSysInfoRepository.KEY_DKMJSCALE, scale);
			MyGlobal.AppOption.DkmjScale = scale;
			//MyGlobal.AppOption.ExportFbfMode = cbExportFbfMode.SelectedIndex == 0 ? EExportFbfMode.SingleExport : EExportFbfMode.BatchExport;
			MyGlobal.AppConfig.ExportFbfMode = cbExportFbfMode.SelectedIndex == 0 ? EExportFbfMode.SingleExport : EExportFbfMode.BatchExport;
			AppConfig.Save(MyGlobal.AppConfig);
			//MyGlobal.AppOption.SaveDksytOutpath(tbDksytOutpath.Text);
			return null;
		}
	}
}
