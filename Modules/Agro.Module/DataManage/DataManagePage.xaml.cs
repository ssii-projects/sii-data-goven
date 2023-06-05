/*
 * (C) 2015  公司版权所有,保留所有权利 
 */
using Agro.LibCore;
using Agro.LibCore.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;
using Agro.Library.Common.Util;
using System.Diagnostics;

namespace Agro.Module.DataManage
{
	/// <summary>
	/// 数据管理主界面
	/// </summary>
	public partial class DataManagePage : UserControl
	{
		/// <summary>
		/// 汇交数据根目录
		/// </summary>
		public class HJDataRootPath
		{

			public string RootPath;

			/// <summary>
			/// 【前缀，shapeFile全路径】
			/// 如：[DK,..\DK2205812016.shp]
			/// </summary>
			public readonly Dictionary<string, string> dicShp = new Dictionary<string, string>();
			/// <summary>
			/// 针对界址点，界址线，地块有可能被分割为多个shape文件的
			/// </summary>
			public readonly Dictionary<string, List<string>> dicShp1 = new Dictionary<string, List<string>>();
			/// <summary>
			/// Mdb文件名（如：权属数据\2205812016.mdb）
			/// </summary>
			public string mdbFileName;
			/// <summary>
			/// 权属单位代码表文件名（如：权属数据\2205812016权属单位代码表.xls）
			/// </summary>
			public string qsXlsFileName;
			/// <summary>
			/// 6位县行政区划代码
			/// </summary>
			public string sXxzqhdm;
			/// <summary>
			/// 行政区名称
			/// </summary>
			public string sXzqmc;

			/// <summary>
			/// 年份
			/// </summary>
			public short Year;

			/// <summary>
			/// 汇总表格路径
			/// </summary>
			public string SumTablePath;

			public Stopwatch StartTime;


			public string ErrorInfo
			{
				get; private set;
			}
			public HJDataRootPath()
			{
				this.ErrorInfo = "未设置根目录";
			}
			public string Init(string sRootPath)//, string ConnectionString)
			{
				ErrorInfo = DoInit(sRootPath);
				return ErrorInfo;
			}
			/// <summary>
			/// 通过传入的质检数据根目录遍历所需要的文件路径，路径正确则返回null
			/// 否则返回错误信息
			/// </summary>
			/// <param name="sRootPath"></param>
			/// <returns></returns>
			private string DoInit(string sRootPath)
			{
				this.RootPath = null;
				StartTime = Stopwatch.StartNew();
				if (!(sRootPath.EndsWith("/") || sRootPath.EndsWith("\\")))
				{
					sRootPath += "\\";
				}
				for (int i = sRootPath.Length - 2; i >= 0; --i)
				{
					var ch = sRootPath[i];
					if (ch == '/' || ch == '\\')
					{
						bool fOK = false;
						var str = sRootPath.Substring(i + 1);
						if (str.Length > 6)
						{
							sXxzqhdm = str.Substring(0, 6);
							if (int.TryParse(sXxzqhdm, out int n))
							{
								if (n.ToString() == sXxzqhdm)
								{
									this.sXzqmc = str.Substring(6, str.Length - 7);

									fOK = true;
								}
							}
						}
						if (!fOK)
						{
							sXxzqhdm = null;
						}
						break;
					}
				}
				if (sXxzqhdm == null)
				{
					return "汇交资料文件根目录必须以6位县级区划代码开头！";
				}
				RootPath = sRootPath;
				var err = CheckPath(sRootPath, "矢量数据");
				if (err != null)
				{
					return err;
				}
				err = CheckPath(sRootPath, "权属数据");
				if (err != null)
				{
					return err;
				}

				err = CheckPath(sRootPath, "汇总表格");
				if (err != null)
				{
					return err;
				}
				this.SumTablePath = sRootPath + "汇总表格";


				var sa1 = new string[] { "DK", "JZD", "JZX", };
				dicShp1.Clear();
				foreach (var s in sa1)
				{
					dicShp1[s] = new List<string>();
				}

				var sa = new string[] { "JBNTBHQ", "KZD", "QYJX", "XJXZQ", "XJQY", "CJQY", "ZJQY", "MZDW", "XZDW", "DZDW", "ZJ" };
				dicShp.Clear();
				foreach (var s in sa)
				{
					dicShp[s] = null;
				}
				FileUtil.EnumFiles(sRootPath + "矢量数据", fi =>
				{
					foreach (var kv in sa)
					{
						var fileName = fi.Name.ToUpper();
						if (fileName.StartsWith(kv) && fileName.EndsWith(".SHP"))
						{
							var ch = fileName.Substring(kv.Length)[0];
							if (ch >= '0' && ch <= '9')
							{
								dicShp[kv] = fi.FullName;
								break;
							}
						}
					}
					foreach (var k in sa1)
					{
						var fileName = fi.Name.ToUpper();
						if (fileName.StartsWith(k) && fileName.EndsWith(".SHP"))
						{
							dicShp1[k].Add(fi.FullName);
							break;
						}
					}
					return true;
				});

				foreach (var kv in dicShp1)
				{
					if (kv.Value.Count == 0)
					{
						if (!(kv.Key == "ZJ"))
						{
							return "在" + sRootPath + "矢量数据目录下未找到以" + kv.Key + " 开头的shp文件！";
						}
					}
				}


				FileUtil.EnumFiles(sRootPath + "权属数据", fi =>
				{
					var fileName = fi.Name.ToLower();
					if (fileName.EndsWith(".mdb"))
					{
						this.mdbFileName = fi.FullName;
						return false;
					}
					return true;
				});
				if (!File.Exists(this.mdbFileName))
				{
					return "在" + sRootPath + "权属数据目录下未找到权属数据库(.mdb文件）！";
				}

				FileUtil.EnumFiles(sRootPath + "权属数据", fi =>
				{
					if (fi.Name.EndsWith("权属单位代码表.xls"))
					{
						this.qsXlsFileName = fi.FullName;
						var s = fi.Name.Substring(6, 4);
						if (short.TryParse(s, out short n))
						{
							Year = n;
						}
						return false;
					}
					return true;
				});
				if (!File.Exists(this.qsXlsFileName))
				{
					return "在" + sRootPath + "权属数据目录下未找到权属单位代码表.xls！";
				}

				LogoutUtil.SetLogoutFile(RootPath + "数据导入日志.txt");
				LogoutUtil.WriteLog("", false);
				LogoutUtil.WriteLog("开始时间：" + DateTime.Now);
				return err;
			}
			private string CheckPath(string sRootPath, string sPath)
			{
				if (!Directory.Exists(sRootPath + sPath))
				{
					return "在" + sRootPath + "目录下未找到" + sPath + "目录！";
				}
				return null;
			}
		}

		private static TreeItemData _dummyNode = null;
		public class TreeItemData : TreeItemDataBase<TreeItemData>
		{
			static TreeItemData()
			{
				//_errImg = MyImageUtil.Image24("exclamation.png");
			}

			private static ImageSource icon = CommonImageUtil.Image16("folder.png");
			private bool _fBusy = false;

			/// <summary>
			/// 图层名称
			/// </summary>
			public string FolderName
			{
				get;
				set;
			}
			private ImageSource _icon;
			/// <summary>
			/// 图层样式
			/// </summary>
			public ImageSource Icon
			{
				get { return _icon; }
				set { _icon = value; RaisePropertyChanged(nameof(Icon)); }
			}

			public bool IsBusy
			{
				get
				{
					return _fBusy;
				}
				set
				{
					_fBusy = value;
					RaisePropertyChanged(nameof(IsBusy));
					RaisePropertyChanged("IsReady");
				}
			}

			private bool _isExpanded;
			internal string FullPath;

			public bool IsExpanded
			{
				get { return _isExpanded; }
				set
				{
					_isExpanded = value;
					if (value)
					{
						if (Children.Count == 1 && Children[0] == _dummyNode)
						{
							FillChildren();
						}
					}
					RaisePropertyChanged(nameof(IsExpanded));
				}
			}
			private readonly DataManagePage _p;

			public TreeItemData(DataManagePage p, string fullPath, string folderName, TreeItemData parent = null)//, bool fExpanded=true)
				: base(parent)
			{
				_p = p;
				FullPath = fullPath;
				FolderName = folderName;
				_icon = icon;
			}

			internal void FillChildren()
			{
				Children.Clear();
				if (FullPath == null)
				{
					return;
				}
				if (_isExpanded)
				{
					var di = new DirectoryInfo(FullPath);
					var lst = di.GetDirectories();
					foreach (var n in lst)
					{
						var n1 = new TreeItemData(_p, n.FullName, n.Name, this);
						if (HasChildren(n.FullName))
						{
							n1.Children.Add(_dummyNode);
						}
						Children.Add(n1);
					}
				}
				else
				{
					if (HasChildren(this.FullPath))
					{
						Children.Add(_dummyNode);
					}
				}
			}

			private static bool HasChildren(string path)
			{
				var di1 = new DirectoryInfo(path);
				var sa = di1.GetDirectories();
				return sa != null && sa.Length > 0;
			}
		}

		public class Item : NotificationObject
		{
			public ImageSource LocalThumbPath
			{
				get;
				set;
			}
			public string Name
			{
				get; private set;
			}
			public string FullName
			{
				get;
				private set;
			}
			public bool isFolder
			{
				get
				{
					return false;
				}
			}
			private readonly ImageSource _folderIcon = GetFolderIcon();
			public Item(string name, string fullName)
			{
				this.Name = name;
				this.FullName = fullName;
				LocalThumbPath = isFolder ? _folderIcon : FileIconUtil.GetFileIcon(Name);
			}
			private static ImageSource GetFolderIcon()
			{
				var appName = Assembly.GetCallingAssembly().GetName().Name;
				var path = string.Format(@"pack://application:,,,/{0};component/Resources\folder.png", appName);
				return new BitmapImage(new Uri(path, UriKind.Absolute));
			}
		}


		private string RootPath;



		private DecoratorWrap _lpc;
		private MetaDataPanel _metaDataPnl;

		private readonly ObservableCollection<TreeItemData> _dataSoure = new ObservableCollection<TreeItemData>();

		private readonly static HJDataRootPath _rootPath = new HJDataRootPath();

		/// <summary>
		/// 构造函数:初始化数据字典窗体
		/// </summary>
		public DataManagePage()
		{
			InitializeComponent();
			_lpc = new DecoratorWrap(bdrContent);
			treeView.ItemsSource = _dataSoure;
			itemListBox.ItemsSource = new ObservableCollection<Item>();
			treeView.SelectedItemChanged += (s, e) => TreeItemsSelectionChanged();
			btnDown.Click += (s, e) =>
			{
				if (itemListBox.SelectedItem is Item it)
				{
					DownLoad(it);
				}
			};
			btnViewFile.Click += (s, e) =>
			{
				if (itemListBox.SelectedItem is Item it)
				{
					ViewFile(it);
				}
			};
			btnSetup.Click += (s, e) =>
			{
				var fok = ShowSetupDialog();
				if (fok)
				{
					Refresh();
				}
			};
			Refresh();
		}
		private void Refresh()
		{
			this.RootPath = DocPathSetupPanel.LoadRootPath();
			if (string.IsNullOrEmpty(RootPath) || !Directory.Exists(RootPath))
			{
				ShowSetupDialog();
			}
			else
			{
				var err = IsRootPathOK(this.RootPath);
				if (err != null)
				{
					ShowError(err);
					return;
				}
			}

			try
			{
				var fok = Directory.Exists(RootPath);
				if (!fok)
				{
					ShowError("指定的目录：\"" + RootPath + "\"不存在！");
					return;
				}
				else
				{
					dpContent.Visibility = Visibility.Visible;
					tbError.Visibility = Visibility.Hidden;
				}
				_dataSoure.Clear();

				tbXzqmc.Text = "区县名称：" + _rootPath.sXzqmc;
				tbYear.Text = "年份：" + _rootPath.Year;

				var di = new DirectoryInfo(RootPath);
				var lst = new List<DirectoryInfo>(di.GetDirectories());
				lst.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
				bool fContainYSJ = false;
				foreach (var pi in lst)
				{
					var td = new TreeItemData(this, pi.FullName, pi.Name);
					td.FillChildren();
					_dataSoure.Add(td);
					if (pi.Name == "元数据")
					{
						fContainYSJ = true;
					}
				}
				if (_dataSoure.Count > 0)
				{
					if (!fContainYSJ)
					{
						_dataSoure.Add(new TreeItemData(this, null, "元数据"));
					}
					TreeItemDataBase<TreeItemData>.SetSelectedItem(treeView, _dataSoure[0]);
				}
			}
			catch (Exception ex)
			{
				ShowError(ex.Message);
			}
		}

		private bool ShowSetupDialog()
		{
			bool fOK = false;
			var pnl = new DocPathSetupPanel();
			var dlg = new KuiDialog(Window.GetWindow(this), "路径设置")
			{
				Content = pnl,
				Height = 300
			};
			dlg.BtnOK.Click += (s, e) =>
			{
				if (string.IsNullOrEmpty(pnl.SaveDocRootPath))
				{
					UIHelper.ShowWarning(dlg, "未设置文档路径！");
					return;
				}
				var err = IsRootPathOK(pnl.SaveDocRootPath);
				if (err != null)
				{
					UIHelper.ShowWarning(dlg, err);
					return;
				}
				DocPathSetupPanel.SaveRootPath(pnl.SaveDocRootPath);
				this.RootPath = pnl.SaveDocRootPath;
				dlg.Close();
				fOK = true;
			};
			dlg.ShowDialog();
			return fOK;
		}

		private void TreeItemsSelectionChanged()
		{
			if (treeView.SelectedItem is TreeItemData ti)
			{
				if (ti.FolderName == "元数据")// lst.Count - 2)
				{//元数据
					if (_lpc.Child != _metaDataPnl)
					{
						if (_metaDataPnl == null)
						{
							var metaFile = FindMetaFile(RootPath);
							_metaDataPnl = new MetaDataPanel(metaFile);
						}
						_lpc.SetChild(_metaDataPnl);
					}
				}
				else
				{
					if (_lpc.Child != itemListBox)
					{
						_lpc.SetChild(itemListBox);
					}
					RefreshContent(ti.FullPath);
				}
				spBtnPanel.Visibility = _lpc.Child == itemListBox ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void itemListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (itemListBox.SelectedItem is Item it)
			{
				if (it.isFolder)
				{
					//Refresh(it.fullName);
				}
				else
				{
					ViewFile(it);
				}
			}
		}

		private void RefreshContent(string fullPath)
		{
			var lst1 = itemListBox.ItemsSource as ObservableCollection<Item>;
			lst1.Clear();

			FileUtil.EnumFiles(fullPath, fi =>
			 {
				 var ld = new Item(fi.Name, fi.FullName)
				 {
					 LocalThumbPath = FileIconUtil.GetFileIcon(fi.Name)
				 };
				 lst1.Add(ld);
				 return true;
			 });
		}
		private void DownLoad(Item it)
		{
			var ext = FileUtil.GetFileExtension(it.Name);
			var savaFD = new SaveFileDialog
			{
				Title = "请选择文件名",
				Filter = "文件(*" + ext + ")|*" + ext,
				RestoreDirectory = true,
				OverwritePrompt = true,
				FileName = it.Name// "内外网数据交换文件" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
			};
			if (savaFD.ShowDialog() != true)
				return;
			if (File.Exists(savaFD.FileName))
				File.Delete(savaFD.FileName);
			File.Copy(it.FullName, savaFD.FileName);
			if (File.Exists(savaFD.FileName))
			{
        Process.Start("explorer.exe", FileUtil.GetFilePath(savaFD.FileName));
			}
		}
		private string GetTmpPath()
		{
			string temp = System.Environment.GetEnvironmentVariable("TEMP") + "\\DataGovern";
			if (!Directory.Exists(temp))
			{
				Directory.CreateDirectory(temp);
			}
			else
			{
				#region 清空临时文件
				var dir = new DirectoryInfo(temp);
				var files = dir.GetFiles();
				foreach (var item in files)
				{
					try
					{

						File.Delete(item.FullName);
					}
					catch (Exception)
					{
					}
				}
				#endregion
			}
			return temp;
		}
		private void ViewFile(Item it)
		{
			string temp = GetTmpPath();
			var tmpFile = temp + "\\" + it.Name;
			if (File.Exists(tmpFile))
			{
				File.Delete(tmpFile);
			}
			File.Copy(it.FullName, tmpFile);
			Win32.ShellExecute(IntPtr.Zero, "open", tmpFile, null, null, ShowCommands.SW_SHOWNORMAL);
		}
		private void ShowError(string err)
		{
			tbError.Text = err;
			dpContent.Visibility = Visibility.Hidden;
			tbError.Visibility = Visibility.Visible;
		}

		private static string FindMetaFile(string rootPath)
		{
			string fileName = null;
			var path = rootPath + "/矢量数据";
			if (Directory.Exists(path))
			{
				FileUtil.EnumFiles(path, fi =>
				 {
					 var name = fi.Name.ToUpper();
					 if (name.StartsWith("SL") && name.EndsWith(".XML"))
					 {
						 fileName = fi.FullName;
					 }
					 return true;
				 });
			}
			return fileName;
		}

		/// <summary>
		/// ok return null
		/// </summary>
		/// <param name="rootPath"></param>
		/// <returns></returns>
		private static string IsRootPathOK(string rootPath)
		{
			//var prm = new HJDataRootPath();
			var err = _rootPath.Init(rootPath);
			if (err == null)
			{
				if (FindMetaFile(rootPath) == null)
				{
					return "在矢量数据目录下未找到元数据文件（以SL开头的xml文件）！";
				}
			}
			return err;
		}
	}
}
