using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

/*
yxm created at 2019/5/5 14:44:09
*/
namespace Agro.Module.DataExchange
{
	/// <summary>
	/// DataUpdatePanel.xaml 的交互逻辑
	/// </summary>
	public partial class DataUpdatePanel : TaskPropertyPage
	{
		public DataUpdatePanel()
		{
			InitializeComponent();
			base.DialogHeight = 320;
			LoadSeting();
			
		}

		/// <summary>
		/// 已/结尾
		/// </summary>
		public string RootPath
		{
			get;private set;
		}

		/// <summary>
		/// 权属资料保存路径
		/// 已/结尾
		/// </summary>
		public string QszlBcPath
		{
			get;private set;
		}
		public readonly Dictionary<string, string> dicShp = new Dictionary<string, string>();
		/// <summary>
		/// Mdb文件名（如：权属数据\2205812016.mdb）
		/// </summary>
		public string mdbFileName;

		public override string Apply()
		{
			if (string.IsNullOrEmpty(tbFilePath.Text))
			{
				return "未选择批量更新数据路径！";
			}
			if (!Directory.Exists(tbFilePath.Text))
			{
				return $"路径{tbFilePath.Text}不存在！";
			}
			if (string.IsNullOrEmpty(tbFilePath1.Text))
			{
				return "未选择权属资料保存路径！";
			}
			if (!Directory.Exists(tbFilePath1.Text))
			{
				return $"路径{tbFilePath1.Text}不存在！";
			}

			var sQszlPath = tbFilePath1.Text.Trim();
			if (!(sQszlPath.EndsWith("/") || sQszlPath.EndsWith("\\")))
			{
				sQszlPath += "\\";
			}
			QszlBcPath = sQszlPath;

			var err= DoInit(tbFilePath.Text);
			if (err == null)
			{
				SaveSeting();
			}
			return err;
		}

		/// <summary>
		/// 通过传入的质检数据根目录遍历所需要的文件路径，路径正确则返回null
		/// 否则返回错误信息
		/// </summary>
		/// <param name="sRootPath"></param>
		/// <returns></returns>
		private string DoInit(string sRootPath)//, string ConnectionString)
		{
			//this.RootPath = null;
			//StartTime = Stopwatch.StartNew();
			//this.sDBConnectionString = ConnectionString;
			if (!(sRootPath.EndsWith("/") || sRootPath.EndsWith("\\")))
			{
				sRootPath += "\\";
			}
			//for (int i = sRootPath.Length - 2; i >= 0; --i)
			//{
			//	var ch = sRootPath[i];
			//	if (ch == '/' || ch == '\\')
			//	{
			//		bool fOK = false;
			//		var str = sRootPath.Substring(i + 1);
			//		if (str.Length > 6)
			//		{
			//			sXxzqhdm = str.Substring(0, 6);
			//			if (int.TryParse(sXxzqhdm, out int n))
			//			{
			//				if (n.ToString() == sXxzqhdm)
			//				{
			//					this.sXzqmc = str.Substring(6, str.Length - 7);

			//					fOK = true;
			//				}
			//			}
			//		}
			//		if (!fOK)
			//		{
			//			sXxzqhdm = null;
			//		}
			//		break;
			//	}
			//}
			//if (sXxzqhdm == null)
			//{
			//	return "汇交资料文件根目录必须以6位县级区划代码开头！";
			//}
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
			err = CheckPath(sRootPath, "权属资料");
			if (err != null)
			{
				return err;
			}
			//err = checkPath(sRootPath, "汇总表格");
			//if (err != null)
			//{
			//	return err;
			//}
			//this.SumTablePath = sRootPath + "汇总表格";


			var sa = new string[] { "DK", "JZD", "JZX", };
			//dicShp.Clear();
			//foreach (var s in sa1)
			//{
			//	dicShp1[s] = new List<string>();
			//}

			//var sa = new string[] { "JBNTBHQ", "KZD", "QYJX", "XJXZQ", "XJQY", "CJQY", "ZJQY", "MZDW", "XZDW", "DZDW", "ZJ" };
			////var dicShp = new Dictionary<string, bool>();
			//dicShp.Clear();
			foreach (var s in sa)
			{
				dicShp[s] = null;
			}

			//var dic = new Dictionary<string, string>();
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
							//dic[kv.Key] = fi.FullName;
							break;
						}
					}
				}
				//foreach (var k in sa1)
				//{
				//	var fileName = fi.Name.ToUpper();
				//	if (fileName.StartsWith(k) && fileName.EndsWith(".SHP"))
				//	{
				//		dicShp1[k].Add(fi.FullName);
				//		if (Year == 0)
				//		{
				//			var str = fileName.Substring(fileName.Length - 8, 4);
				//			Year = SafeConvertAux.ToInt32(str);
				//		}
				//		break;
				//	}
				//}
				return true;
			});
			//foreach (var kv in dic)
			//{
			//    dicShp[kv.Key] = kv.Value;
			//}
			foreach (var kv in dicShp)
			{
				if (kv.Value == null)
				{
					return "在" + sRootPath + "矢量数据目录下未找到以" + kv.Key + " 开头的shp文件！";
				}
			}
			//foreach (var kv in dicShp1)
			//{
			//	if (kv.Value.Count == 0)
			//	{
			//		if (!(kv.Key == "ZJ"))
			//		{
			//			return "在" + sRootPath + "矢量数据目录下未找到以" + kv.Key + " 开头的shp文件！";
			//		}
			//	}
			//}


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

			//FileUtil.EnumFiles(sRootPath + "权属数据", fi =>
			//{
			//	if (fi.Name.EndsWith("权属单位代码表.xls"))
			//	{
			//		this.qsXlsFileName = fi.FullName;
			//		return false;
			//	}
			//	return true;
			//});
			//if (!File.Exists(this.qsXlsFileName))
			//{
			//	return "在" + sRootPath + "权属数据目录下未找到权属单位代码表.xls！";
			//}

			//LogoutUtil.SetLogoutFile(RootPath + "数据导入日志.txt");
			//LogoutUtil.WriteLog("", false);
			//LogoutUtil.WriteLog("开始时间：" + DateTime.Now);
			return null;
		}
		private string CheckPath(string sRootPath, string sPath)
		{
			if (!Directory.Exists(sRootPath + sPath))
			{
				return "在" + sRootPath + "目录下未找到" + sPath + "目录！";
			}
			return null;
		}

		private void LoadSeting()
		{
			var s = MyGlobal.Persist.LoadSettingInfo("F53D6DC2A-EB7D-4969-96D6-1E9AD2F1B429") as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbFilePath.Text = s;
			}

			s = MyGlobal.Persist.LoadSettingInfo("CE5194DC-8722-4DA4-93B6-6BDE41DEDEEB") as string;
			if (!string.IsNullOrEmpty(s))
			{
				tbFilePath1.Text = s;
			}
		}
		private void SaveSeting()
		{
			if (!string.IsNullOrEmpty(RootPath))
			{
				MyGlobal.Persist.SaveSettingInfo("F53D6DC2A-EB7D-4969-96D6-1E9AD2F1B429", RootPath);
			}
			if (!string.IsNullOrEmpty(QszlBcPath))
			{
				MyGlobal.Persist.SaveSettingInfo("CE5194DC-8722-4DA4-93B6-6BDE41DEDEEB", QszlBcPath);
			}
		}
	}
}
