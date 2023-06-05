using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Repository;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Agro.Module.DataExchange
{
	/// <summary>
	///导出汇交格式数据 属性页
	/// </summary>
	public partial class ExportHjsjPropertyPage : TaskPropertyPage
	{
		public enum OptionType
		{
			///// <summary>
			///// 导出未确权地块
			///// </summary>
			//ExportWQQDK,
			/// <summary>
			/// 导出注销数据
			/// </summary>
			ExportZX,
			/// <summary>
			/// 导出补证数据
			/// </summary>
			ExportBZ,
			/// <summary>
			/// 导出换证数据
			/// </summary>
			ExportHZ,
		}
		public class OptionItem: NotificationObject
		{
			private bool _fSelected;
			public bool IsSelected
			{
				get { return _fSelected; }
				set
				{
					_fSelected = value;
					base.RaisePropertyChanged(nameof(IsSelected));
				}
			}
			public string Name { get; set; }
			public readonly OptionType OptionType;
			public OptionItem(OptionType ot)
			{
				OptionType = ot;
				switch (ot)
				{
					//case OptionType.ExportWQQDK:Name = "是否导出未确权地块"; break;
					case OptionType.ExportZX:Name = "是否导出注销数据"; break;
					case OptionType.ExportBZ: Name = "是否导出补证数据"; break;
					case OptionType.ExportHZ: Name = "是否导出换证数据"; break;
				}
			}
		}

		public readonly ObservableCollection<OptionItem> OptionItems;

		public Action OnAppled;
		public ExportHjsjPropertyPage()
		{
			InitializeComponent();
			DialogHeight = 450;
			DialogWidth = 780;
			base._onApply += () =>
			  {
				  if (string.IsNullOrEmpty(RootPath))
				  {
					  return "未输入汇交格式数据路径！";
				  }

				  var err = CheckPath(RootPath);
				  if (err == null)
				  {
					  Persist();
					  OnAppled?.Invoke();
				  }
				  return err;
			  };
			tbFilePath.OnButtonClick += () =>
			{
				var dlg = new System.Windows.Forms.FolderBrowserDialog()
				{
					Description = "汇交格式数据路径"
				};
				var path = tbFilePath.Text.Trim();
				if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
				{
					dlg.SelectedPath = path;
				}
				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.tbFilePath.Text = dlg.SelectedPath;
					RootPath = dlg.SelectedPath;
					if (!RootPath.EndsWith("/") || !RootPath.EndsWith("\\"))
					{
						RootPath += "\\";
					}
				}

			};

			OptionItems = new ObservableCollection<OptionItem>()
			{
				//new OptionItem(OptionType.ExportWQQDK),
				new OptionItem(OptionType.ExportZX),
				new OptionItem(OptionType.ExportBZ),
				new OptionItem(OptionType.ExportHZ)
			};
			lstBox.ItemsSource = OptionItems;

			Persist(false);
		}
		/// <summary>
		/// 以'\'结尾
		/// </summary>
		public string OutPath { get;private set; }
		//public string HjZootPath { get;private set;}
		private string RootPath;
		private string CheckPath(string rootPath)
		{
			if (!Directory.Exists(rootPath))
			{
				return $"目录\"{rootPath}\"不存在";
			}

			var zone=DlxxXzdyRepository.Instance.QueryZone(t => t.JB == Library.Model.eZoneLevel.County);
			if (zone == null)
			{
				return "数据库内部格式异常，DLXX_XZDY中不存在县级行政区编码";
			}

			var HjZootPath = $"{zone.Code}{zone.Name}";
			var path =Path.Combine(rootPath, HjZootPath);
			if (Directory.Exists(path))
			{
				var err= $"所选路径下已存在 \"{HjZootPath}\" 目录";
				if (MessageBoxResult.Yes != MessageBox.Show($"{err},是否清空？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question))
				{
					return $"输出路径无效，{err}";
				}
				FileUtil.EnumFiles2(path, fi =>
				 {
					 try
					 {
						 fi.Delete();
					 }
					 catch (Exception ex)
					 {
						 err = ex.Message;
						 return false;
					 }
					 return true;
				 });
			}
			else
			{
				Directory.CreateDirectory(path);
			}
			if (!path.EndsWith("/") || !path.EndsWith("\\"))
			{
				path += "\\";
			}
			OutPath = path;
			/*
			var sa = new string[] { "矢量数据", "权属数据" };
			foreach (var s in sa)
			{
				var path = rootPath + s;
				if (Directory.Exists(path))
				{
					return $"目录 \"{path}\" 存在！";
				}
				//Directory.CreateDirectory(path);
			}*/
			return null;
		}

		private void Persist(bool fSave = true)
		{
			var key = "A9ED4940-AF87-4515-A10B-89803786ACC5";
			if (fSave)
			{
				if (!string.IsNullOrEmpty(RootPath))
				{
					MyGlobal.Persist.SaveSettingInfo(key, RootPath);
				}
			}
			else
			{
				var s = MyGlobal.Persist.LoadSettingInfo(key) as string;
				if (!string.IsNullOrEmpty(s))
				{
					tbFilePath.Text = s;
					RootPath = s;
				}
			}
		}

		private void Btn_Click(object sender, RoutedEventArgs e)
		{
			foreach (var cl in OptionItems)
			{
				if (sender == btnSelectAll)
				{
					cl.IsSelected = true;
				}
				else if (sender == btnNotSelectAll)
				{
					cl.IsSelected = false;
				}
				else if (sender == btnXorSelect)
				{
					cl.IsSelected = !cl.IsSelected;
				}
			}
		}
	}
}
