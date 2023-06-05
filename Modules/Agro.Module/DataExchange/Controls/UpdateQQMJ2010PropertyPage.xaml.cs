using Agro.LibCore;
using Agro.LibCore.UI;
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

namespace Agro.Module.DataExchange
{
	/// <summary>
	/// UpdateQQMJ2010PropertyPage.xaml 的交互逻辑
	/// </summary>
	public partial class UpdateQQMJ2010PropertyPage : TaskPropertyPage
	{
		public UpdateQQMJ2010PropertyPage()
		{
			InitializeComponent();
			DialogHeight = 320;
			tbFilePath.Filter = "Access数据库文件 (*.mdb)|*.mdb";
			LoadSeting();
		}
		public override string Apply()
		{
			FileName = tbFilePath.Text;
			if (string.IsNullOrEmpty(FileName))
			{
				return "未输入权属数据路径！";
			}
			try
			{
				using (var mdb = DBAccess.Open(FileName))
				{
					if (!mdb.IsTableExists("DKDC"))
					{
						return "地块调查表（DKDC）不存在！";
					}
					foreach (var fieldName in "DKBM,QQMJM2010".Split(','))
					{
						if (!mdb.IsFieldExists("DKDC",fieldName))
						{
							return $"地块调查表（DKDC）中缺少{fieldName}字段！";
						}
					}
				}
				SaveSeting();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return null;
		}


		//public eDatabaseType DatabaseType { get; set; }

		private string _file;
		public string FileName { get { return _file; } set { _file = value; tbFilePath.Text = value; } }

		private const string _persistKey = "DD100048-A840-4E49-8948-39D108F9499C";
		private void LoadSeting()
		{
			var s = MyGlobal.Persist.LoadSettingInfo(_persistKey) as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbFilePath.Text = s;
			}
		}
		private void SaveSeting()
		{
			if (!string.IsNullOrEmpty(FileName))
			{
				MyGlobal.Persist.SaveSettingInfo(_persistKey, FileName);
			}
		}
	}
}
