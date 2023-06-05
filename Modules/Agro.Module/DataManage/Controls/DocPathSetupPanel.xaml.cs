using Agro.Library.Common;
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

namespace Agro.Module.DataManage
{
	/// <summary>
	/// DocPathSetupPanel.xaml 的交互逻辑
	/// </summary>
	public partial class DocPathSetupPanel : UserControl
	{
		public DocPathSetupPanel()
		{
			InitializeComponent();
			//LoadSeting();
			var s=LoadRootPath();
			if (!string.IsNullOrEmpty(s))
			{
				tbDocSavePath.Text = s;
			}
		}
		/// <summary>
		/// 不以'/'或'\\'结尾
		/// </summary>
		public string SaveDocRootPath { get {
				var s = tbDocSavePath.Text.Trim();
				if (s.EndsWith("/") || s.EndsWith("\\"))
				{
					//s += "\\";
					s = s.Substring(0, s.Length - 1);
				}
				return s;// tbDocSavePath.Text.Trim();
			} }
		//private void LoadSeting()
		//{
		//	var s = MyGlobal.Persist.LoadSettingInfo("ADF5701C-6F5F-4D52-A843-65FE140DD836") as string;
		//	if (!string.IsNullOrEmpty(s))
		//	{
		//		tbDocSavePath.Text = s;
		//	}
		//}
		//internal void SaveSeting()
		//{
		//	if (!string.IsNullOrEmpty(SaveDocRootPath))
		//	{
		//		MyGlobal.Persist.SaveSettingInfo("ADF5701C-6F5F-4D52-A843-65FE140DD836", SaveDocRootPath);
		//	}
		//}
		/// <summary>
		/// 返回两区文档根路径
		/// return null or '\\'结尾的路径
		/// </summary>
		/// <returns></returns>
		public static string LoadRootPath()
		{
			var s = MyGlobal.Persist.LoadSettingInfo("ADF5701C-6F5F-4D52-A843-65FE140DD836") as string;
			//if (!string.IsNullOrEmpty(s))
			//{
			//	if (s.EndsWith("/") || s.EndsWith("\\"))
			//	{
			//		//s += "\\";
			//		s = s.Substring(0, s.Length - 1);
			//	}
			//}
			return s;
		}
		public static void SaveRootPath(string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				MyGlobal.Persist.SaveSettingInfo("ADF5701C-6F5F-4D52-A843-65FE140DD836", path);
			}
		}
	}
}
