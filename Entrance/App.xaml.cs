using Agro.FrameWork;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using Agro.LibCore;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Agro.Library.Common.Repository;
using Agro.GIS;
using Agro.LibCore.Database;
using Agro.Library.Model;

namespace Agro.FrameApp
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
		private readonly List<string> _enableFuncNames = new List<string>();
		protected override void OnStartup(StartupEventArgs e)
		{
			try
			{
				SetPathEnv();

				ApplicationData.SmallIcon = MyImageUtil.Image16("全图16.png");

				CreateDefaultAppConfig();

				AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

				this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

				WinFontUtil.InstallFont("esri_40.ttf", "ESRI North");

				int skin = 10;
				AppJsonUtil.LoadAppJson(jo =>
				{
					if (jo.Skin != null)
					{
						skin = (int)jo.Skin;
					}
				});
				SkinUtil.ApplySkin(skin, false);

				if (!LoginWindow.ShowDialog(null,AppType.DataGoven))
				{
					Current.Shutdown();
					return;
				}

				//Test();

                #region 获取当前用户授权的模块
                if (!MyGlobal.LoginUser.IsAdmin())
				{
					var db = MyGlobal.Workspace;
					if (db.IsTableExists("CS_USER_PERMISSION"))
					{
						//var lst = new List<string>();
						var un = MyGlobal.LoginUser.Name.Replace("'", "''");
						var sql = "select distinct FUNCTION_ID from CS_USER_PERMISSION where USERNAME='" + un + "'";
						db.QueryCallback(sql, r =>
						{
							_enableFuncNames.Add(r.GetString(0));
							return true;
						});
					}
				}
				#endregion


				var main = new MainWindow()
				{
					CaptionIcon = MyImageUtil.Image16("全图16.png"),
				};

				#region yxm 2018-11-30 监听数据源变化事件
				main.Events.OnOptionCreated += pnl =>
					{
						if (pnl is DataSourceSelectPanel sp)
						{
							sp.OnFilter += it => it.dbType == eDatabaseType.SqlServer||it.dbType==eDatabaseType.MySql;
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
					};
				#endregion

				main.Events.OnMainMenuCreated += OnMainMenuCreated;
				MyGlobal.MainWindow = main;

				var startPage = main.LoadModules();
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
					bool fOK = MyGlobal.LoginUser.IsAdmin();
					#region  yxm 2019-11-12 若管理员登录且数据库还未初始化则默认启动数据处理页面
					if (fOK)
					{
						if (!DlxxXzdyRepository.Instance.Exists(t => t.JB == Library.Model.eZoneLevel.County))
						{
							var m = main.Modules.Find(t => t.Page?.ClassName == "Agro.Module.DataExchange.MainPage");
							if (m != null)
							{
								startPage = m.Page;
							}
						}
					}
					#endregion
					if (!fOK)
					{
						fOK = _enableFuncNames.Contains(startPage.Title);
					}
					if (fOK)
					{
						if (main.CopyRightInfo.IsAuthorityCodeOK)
						{
							main.OpenPage(startPage);
						}
					}
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
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
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
					if (mi.Tag is FrameWork.Module md)
					{
						if (md.Page?.ApplyAuthority == false)
							continue;
					}
					else
					{
						continue;
					}
					var title = mi.Header.ToString();
					if (!_enableFuncNames.Contains(title))
					{
						mi.Visibility = Visibility.Collapsed;
					}
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

		///// <summary>
		///// yxm 2018-11-30
		///// 比较指定数据源与当前数据源是否不同
		///// </summary>
		///// <param name="nit"></param>
		///// <returns></returns>
		//private bool IsDatasourceChange(ConnectionStringTypeMetadata nit)
		//{
		//	bool fChanged = false;
		//	var ods = new DataSourceSelectPanel().GetDefaultDataSource();
		//	if (!( ods== null && nit == null))
		//	{
		//		if (ods == null || nit == null)
		//		{
		//			fChanged = true;
		//		}
		//		else
		//		{
		//			if (!(ods.dbType == nit.dbType && ods.ConnectionString == nit.ConnectionString))
		//			{
		//				fChanged = true;
		//			}
		//		}
		//	}
		//	return fChanged;
		//}

		private void CreateDefaultAppConfig()
		{
			Try.Catch(() =>
			{
				var appJsonFile = @AppDomain.CurrentDomain.BaseDirectory + @"App\App.json";
				if (!File.Exists(appJsonFile))
				{
					var o = FrameApp.Properties.Resources.AppJson;
					string str = System.Text.Encoding.UTF8.GetString(o);
					File.WriteAllText(appJsonFile, str);
				}
			});

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
					var o = FrameApp.Properties.Resources.ConnectionStrings;
					File.WriteAllText(file, o);
				}
			}, false);
		}

		private void SetPathEnv()
		{
			var envPath = Environment.GetEnvironmentVariable("Path");
			Console.WriteLine(envPath);
			var sa = envPath.Split(';');
			var binPath = AppDomain.CurrentDomain.BaseDirectory;
			if (sa.FindIndex(it => StringUtil.isEqualIgnorCase(it, binPath)) < 0)
			{
				Environment.SetEnvironmentVariable("Path", envPath + ";" + binPath, EnvironmentVariableTarget.User);
			}

		}

		private void Test()
		{
			var db = MyGlobal.Workspace;
			var srid=db.GetSRID(EntityUtil.GetTableName<DLXX_XZDY>());
			var file = "d:/tmp/xlqnjq.db";
			if (File.Exists(file))
			{
				File.Delete(file);
			}

            using (var tgtDb = SqliteFeatureWorkspaceFactory.Instance.CreateWorkspace(file))
			{
                CopyTable<CS_SYSINFO>(tgtDb);
                CopyTable<DJ_CBJYQ_DJB>(tgtDb);
                CopyTable<DJ_CBJYQ_DKXX>(tgtDb);
                CopyTable<QSSJ_CBJYQZDJB>(tgtDb);
                CopyFeatureClass<DLXX_XZDY>(tgtDb,srid);
                CopyFeatureClass<DLXX_DK>(tgtDb, srid);

            }
		}
        private void CopyFeatureClass<T>(IFeatureWorkspace tgtDb, int srid)
        {
            var db = MyGlobal.Workspace;
            var tbName = EntityUtil.GetTableName<T>();
            using (var fc = db.OpenFeatureClass(tbName))
            {
			
                tgtDb.CreateFeatureClass(tbName, fc.Fields, srid, fc.AliasName);
                using (var toFc = tgtDb.OpenFeatureClass(tbName))
                {
                    tgtDb.BeginTransaction();
                    var qf = new QueryFilter()
                    {

                    };
                    var ft = toFc.CreateFeature();
                    fc.Search(qf, r =>
                    {
                        IRowUtil.CopyValues(r, ft);
                        toFc.Append(ft);
                    });
                    tgtDb.Commit();
                }
            }
        }
		private void CopyTable<T>(IFeatureWorkspace tgtDb)
		{
			var db = MyGlobal.Workspace;
			var tbName = EntityUtil.GetTableName<T>();
            using (var fc = db.OpenFeatureClass(tbName))
            {
				tgtDb.CreateTable(tbName,fc.Fields, fc.AliasName);
                using (var toFc = tgtDb.OpenFeatureClass(tbName))
                {
                    tgtDb.BeginTransaction();
                    var qf = new QueryFilter()
                    {

                    };
                    var ft = toFc.CreateFeature();
                    fc.Search(qf, r =>
                    {
                        IRowUtil.CopyValues(r, ft);
                        toFc.Append(ft);
                    });
                    tgtDb.Commit();
                }
            }
        }

		//private void Test()
		//{
		//	var mdbFileName = @"D:\tmp\新建文件夹\5106812017.mdb";
		//	using (var mdb = DBAccess.Open(mdbFileName))
		//	{
		//		#region using-block

		//		try
		//		{
		//			#region try-block
		//			var sTableName = "CBJYQZDJB";
		//			//double oldProgress = 0;
		//			//int n = 0;
		//			var cnt = mdb.QueryOneInt($"select count(*) from {sTableName}");
		//			//base.RecordCount = cnt;
		//			Console.WriteLine($"共：{cnt}条数据");
		//			{
		//				var sql = $"select * from {sTableName}";
		//				//int oid = 0;//
		//				using (var dr = mdb.QueryReader(sql))
		//				{
		//					while (dr.Read())
		//					{
		//						string str = "";
		//						for (int i = 0; i < dr.FieldCount; ++i)
		//						{
		//							var o = dr.IsDBNull(i) ? "" : dr.GetValue(i);
		//							str += o.ToString() + ",";
		//							Console.WriteLine(str);
		//							//row[++c] = o ?? DBNull.Value;
		//							//var fi = lstFields[c];
		//							//if (fi.FieldType == eFieldType.eFieldTypeString && fi.Length > 0 && o != null)
		//							//{
		//							//	var s = o.ToString();
		//							//	if (s.Length > fi.Length)
		//							//	{
		//							//		throw new Exception("字符串\"" + s + "\"的长度" + s.Length + "超过字段[" + fi.FieldName + "]允许的最大长度：" + fi.Length);
		//							//	}
		//							//}
		//						}

		//					}
		//				}
		//			}
		//			#endregion
		//		}
		//		catch
		//		{
		//		}
		//		#endregion
		//	}
		//}
	}
}
