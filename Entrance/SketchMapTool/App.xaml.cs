using Agro.FrameWork;
using Agro.LibCore;
using Agro.LibCore.UI;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace TestTool
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		//private readonly List<string> _enableFuncNames = new List<string>();
		protected override void OnStartup(StartupEventArgs e)
		{
			try
			{
				CreateDefaultAppConfig();

				AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

				this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

				WinFontUtil.InstallFont("esri_40.ttf", "ESRI North");

				SkinUtil.ApplySkin(10, false);

				if (!LoginWindow.ShowDialog(null,AppType.DataGoven,"地块示意图生成工具"))
				{
					Current.Shutdown();
					return;
				}

				var main = new TabMainWindow()
				{
					CaptionIcon = MyImageUtil.Image16("全图16.png"),
					UseAuthority=false
				};

				#region yxm 2018-11-30 监听数据源变化事件
				main.Events.OnOptionCreated += pnl =>
				{
					if (pnl is DataSourceSelectPanel sp)
					{
						sp.OnFilter += it => it.dbType == eDatabaseType.SqlServer;
						sp.OnPreApply += it =>
						{
							if (it.IsSourceChanged)
							{
								if (!LoginWindow.ShowDialog(it.NewSource,AppType.DataGoven))
								{
									it.Cancel = true;
								}
							}
							return null;
						};
					}
					//else if (pnl is CopyRightPanel crp)
					//{
					//	crp.ShowAuthorityInfo(false);
					//}
				};
				#endregion

				main.Events.OnMainMenuCreated += OnMainMenuCreated;
				MyGlobal.MainWindow = main;

				try
				{
					Gdal.LoadNative();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				main.Loaded += (s, e1) =>
				{
					var pnl = new Agro.Module.SketchMap.MainPage();
					var icon = MyImageUtil.Image16("全图16.png");
					main.OpenPage(pnl, "地块示意图", icon,false);
				};
				main.Closed += (s, e1) =>
				{
					MyGlobal.ShutDown();
					Current.Shutdown();
				};
				main.ShowDialog();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
				Current.Shutdown();
			}
		}


		private void OnMainMenuCreated(ContextMenu menu)
		{
			if (MyGlobal.LoginUser.IsAdmin())
			{
				return;
			}
			CheckMenuItemAuthority(menu.Items);
		}
		void CheckMenuItemAuthority(ItemCollection items)
		{
			foreach (var ti in items)
			{
				if (!(ti is MenuItem mi))
				{
					continue;
				}
				if (mi.Items.Count > 0)
				{
					CheckMenuItemAuthority(mi.Items);
					bool fShow = false;
					foreach (var it in mi.Items)
					{
						if (it is MenuItem mi1 && mi1.Visibility == Visibility.Visible)
						{
							fShow = true;
							break;
						}
					}
					if (!fShow)
					{
						mi.Visibility = Visibility.Collapsed;
					}
				}
				else
				{
					if (mi.Tag is Module md)
					{
						if (md.Page?.ApplyAuthority == false)
							continue;
					}
					else
					{
						continue;
					}
					//var title = mi.Header.ToString();
					//if (!_enableFuncNames.Contains(title))
					//{
					//	mi.Visibility = Visibility.Collapsed;
					//}
				}
			}
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				LogUtil.WriteExceptionLog(MyGlobal.Workspace, e.ToString());
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}


		private void CreateDefaultAppConfig()
		{
			Try.Catch(() =>
			{
				var path = @AppDomain.CurrentDomain.BaseDirectory + "Config";
				var file = path + @"\ConnectionStrings.xml";
				if (!File.Exists(file))
				{
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					var o = SketchMapTool.Properties.Resources.ConnectionStrings;
					File.WriteAllText(file, o);
				}
			}, false);
		}
	}
}
