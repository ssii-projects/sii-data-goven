using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.UI;
using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Web.Script.Serialization;
using Agro.Library.Common.Repository;
using Agro.Library.Model;

namespace Agro.Library.Common
{
	/// <summary>
	/// 登录对话框
	/// yxm 2018-1-23
	/// </summary>
	public partial class LoginWindow : Window
	{
		private Persist _persist
		{
			get
			{
				return MyGlobal.Persist;
			}
		}


		private IFeatureWorkspace _db;

		private SEC_ID_USER _user;
		private bool _fShowInnerConnectionDialog;
		private LoginWindow(ConnectionStringTypeMetadata cs = null)
		{
			_fShowInnerConnectionDialog = cs == null;
			InitializeComponent();
			new NoneStyleWindowHelper(this, dragPart);
			LoadUI();
			LoadAppConfig();

			var lst = new List<ConnectionStringTypeMetadata>();
			var def = DataSourceConfig.Instance.Load(it => lst.Add(it));
			if (lst.Count > 0)
			{
				cbDataSource.ItemsSource = lst;
				var sit = def;
				if (cs != null)
				{
					sit = lst.Find(it => cs.Name == it.Name);
				}
				cbDataSource.SelectedItem = sit;
			}
			dpDataSource.Visibility = lst.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		/// 显示账号验证对话框
		/// 副作用：
		///		1.验证成功：修改MyGlobal.Workspace
		///		2.验证失败：释放db
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		public static bool ShowDialog(ConnectionStringTypeMetadata cs,AppType appType, string mainTitle = null)
		{
			var d = new LoginWindow(cs);
			if (mainTitle != null)
			{
				d.tbMainTitle.Text = mainTitle;
			}
			var fok = d.ShowDialog() == true;
			var db = d._db;
			if (!fok)
			{
				db?.Dispose();
				return false;
			}

			try
			{
				MyGlobal.Connected(db,appType, d._user);

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return false;
			}
			return true;
		}

		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (!(cbDataSource.SelectedItem is ConnectionStringTypeMetadata cs))
				{
					MessageBox.Show("未配置数据源", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
				Try.Catch(() => _db = cs.Open(), false);
				if (_db == null)
				{
					if (_fShowInnerConnectionDialog)
					{
						var res = DataSourceConfig.Instance.OpenDefault(this, true, eDatabaseType.SqlServer);
						if (res.Workspace == null)
						{
							if (!res.IsInnerConnectionDialogUsed)
							{
								MessageBox.Show("数据库连接失败！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
							}
							return;
						}
						_db = res.Workspace;
						cs.ConnectionString = _db.ConnectionString;
					}
				}
				DataSourceConfig.Instance.SetDefault(cs);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var err = LoginService.Verify(_db, txtUsername.Text, txtPwd.Password, out _user);
			if (err == null)
			{
				DialogResult = true;
				SaveUI();
				Close();
			}
			else
			{
				MessageBox.Show(err);
			}
		}
		private void BtnExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		private void OnCommand(object sender, EventArgs e)
		{

		}
		private void FilterButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{

		}

		private void LoadUI()
		{
			string userName = "", pwd = "";

			var o = _persist.LoadSettingInfo("login_chkSavePwd");
			if (o != null && o.ToString() == "1")
			{
				chkSavePwd.IsChecked = true;
				o = _persist.LoadSettingInfo("login_user");
				userName = o != null ? o.ToString() : "";
				o = _persist.LoadSettingInfo("login_pwd");
				pwd = o == null ? "" : o.ToString();
			}
			txtUsername.Text = userName;
			txtPwd.Password = pwd;
		}
		private void SaveUI()
		{
			bool fSavePwd = chkSavePwd.IsChecked == true;
			_persist.SaveSettingInfo("login_chkSavePwd", fSavePwd ? "1" : "0");
			_persist.SaveSettingInfo("login_user", fSavePwd ? txtUsername.Text.Trim() : "");
			_persist.SaveSettingInfo("login_pwd", fSavePwd ? txtPwd.Password : "", true);
		}
		void LoadAppConfig()
		{
			tbBottom.Visibility = Visibility.Collapsed;

			var c = AppConfig.Load();
			MyGlobal.AppConfig = c;
			if (!string.IsNullOrEmpty(c.LoginForm?.BottomText))
			{
				tbBottom.Text = c.LoginForm.BottomText;
				tbBottom.Visibility = Visibility.Visible;
			}
		}
	}

	public static class LoginService
	{
		public static string Verify(IFeatureWorkspace db, string sUserName, string pwd, out SEC_ID_USER user)
		{
			user = null;
			var userDao = new UserRepository(db);
			try
			{
				var err = MyDBUtil.IsValidDB(db);
				if (err != null)
				{
					return err;
				}

				user = userDao.FindByUserName(sUserName);
				if (user == null)
				{
					return "用户名不存在！";
				}
				if (!userDao.IsPasswordOK(user, pwd))
				{
					return "密码错误！";
				}
				if (!user.IsAdmin())
				{
					if (string.IsNullOrEmpty(user.ZoneCode))
					{
						return "该用户未设置所属地域信息，请与管理员联系！";
					}
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
			return null;
		}
	}
}
