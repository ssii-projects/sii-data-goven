using Agro.LibCore;
using Agro.Module.SketchMap;
using System;
using System.IO;
using System.Windows;

namespace SketchMapConsole
{
  /// <summary>
  /// App.xaml 的交互逻辑
  /// </summary>
  public partial class App : Application
	{
		/// <summary>
		///命令行参数示例： "path:D:\tmp/新建文件夹 (2)/Test" "djbid:0001ebff-52c7-4eee-b728-2b4a9beeeff3,0001f7d8-efc8-4111-be57-3485d6df539b" "ztz:张三" "ztrq:2021/1/25" "shz:审核人" "shrq:2021/1/25" "ztdw:编制单位" "cons:Data Source=192.168.0.117;Initial Catalog=XWXSJK;User ID=sa;Password=ssii@123456"
		/// </summary>
		/// <param name="e"></param>
		protected override void OnStartup(StartupEventArgs e)
		{
			var wnd = new MainWindow
			{
				//wnd.WindowState = WindowState.Minimized;
				ShowInTaskbar = false
			};
			try
			{
				//if (true)
				//{
				//	this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
				//	var info = new ProcessStartInfo
				//	{
				//		FileName = @"D:\GitProjects\sii\Business\SII.DataGoven\Entrance\SketchMapConsole\bin\Debug\SketchMapConsole0.exe",
				//		Arguments = "",
				//		WindowStyle = ProcessWindowStyle.Hidden
				//	};
				//	Process pro = Process.Start(info);
				//	pro.WaitForExit();
				//	Current.Shutdown();
				//	return;
				//}

				var prm = new SkecthMapProperty
				{
					SavePdfFormat = true
				};
				//prm.OutputPath = MyGlobal.AppOption.LoadDksytOutpath();
				//if (string.IsNullOrEmpty(prm.OutputPath))
				//{
				//	throw new Exception("未设置地块示意图输出路径！");
				//}
				//if (!Directory.Exists(prm.OutputPath))
				//{
				//	throw new Exception($"路径：{prm.OutputPath} 不存在！");
				//}
				string dbType = "SqlServer";
				string cons = null;
				string[] djbids = null;
				foreach (var arg in e.Args)
				{
					var n = arg.IndexOf(':');
					//var sa = arg.Split(':');
					var key = arg.Substring(0, n);// sa[0].ToLower().Trim();
					var val = arg.Substring(n + 1);// sa[1];
					//Console.WriteLine($"key={key},value={val}");
					switch (key)
					{
						case "djbid":
							djbids = val.Split(',');
							break;
						case "path":prm.OutputPath = val;break;
						case "ztz": prm.DrawPerson = val; break;
						case "ztrq": prm.DrawDate = DateTime.Parse(val); break;
						case "shz": prm.CheckPerson = val; break;
						case "shrq": prm.CheckDate = DateTime.Parse(val); break;
						case "ztdw": prm.Company = val; break;
						case "cons":cons = val;break;
						case "dbType":dbType= val;break;
					}
				}
				if (string.IsNullOrEmpty(cons))
				{
					throw new Exception("命令行参数异常，未传入cons参数");
				}
				if (string.IsNullOrEmpty(prm.OutputPath))
				{
					throw new Exception("命令行参数异常，未传入path参数");
				}
				if (djbids==null|| djbids.Length==0)
				{
					throw new Exception("命令行参数异常，未传入djbid参数");
				}


				if (!Directory.Exists(prm.OutputPath))
				{
					Directory.CreateDirectory(prm.OutputPath);
				}

				this.ShutdownMode = ShutdownMode.OnMainWindowClose;// OnExplicitShutdown;



				WinFontUtil.InstallFont("esri_40.ttf", "ESRI North");

				Gdal.LoadNative();


				wnd.Loaded += (s, e1) =>
				  {
					  try
					  {
						  wnd.Hide();
						  wnd.ExportSketch(prm, djbids,cons,dbType);
						  Console.WriteLine("ExportSuccess");
					  }
					  catch (Exception ex)
					  {
						  wnd.ReportError(ex.Message);
						  //Console.Error.WriteLine(ex.Message);
					  }
					  wnd.Close();
				  };

				wnd.Show();
			}
			catch (Exception ex)
			{
				wnd.ReportError(ex.Message);
				//MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
				Current.Shutdown();
			}
		}



	}
}
