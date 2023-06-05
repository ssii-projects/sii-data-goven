using Agro.LibCore;
using Agro.LibCore.UI;
using System;
using System.Windows;
using Agro.Library.Common;

namespace DataOperatorTool
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

		public LoginWindow()
		{
			InitializeComponent();
            new NoneStyleWindowHelper(this,dragPart);
            LoadUI();
			LoadAppConfig();
		}
		public string UserName { get { return txtUsername.Text.Trim(); } }
		///// <summary>
		///// 显示账号验证对话框
		///// 副作用：
		/////		1.验证成功：修改MyGlobal.Workspace
		/////		2.验证失败：释放db
		///// </summary>
		///// <param name="db"></param>
		///// <returns></returns>
		//public static bool Show()
		//{
		//	var d = new LoginWindow();
		//	//if (mainTitle != null)
		//	//{
		//	//	d.tbMainTitle.Text = mainTitle;
		//	//}
		//	var fok = d.ShowDialog() == true;
		//	if (!fok)
		//	{
		//		//db?.Dispose();
		//		return false;
		//	}
		//	return true;
		//}

		private void BtnOK_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (txtUsername.Text.Trim().Length == 0)
				{
					MessageBox.Show("必须输入作业员名称", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}
				SaveUI();
				DialogResult = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
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
			var o = _persist.LoadSettingInfo("dataOperator");
			var userName = o != null ? o.ToString() : "";
			txtUsername.Text = userName;
		}
        private void SaveUI()
        {
			_persist.SaveSettingInfo("dataOperator", txtUsername.Text.Trim());
		}
		void LoadAppConfig()
		{
			tbBottom.Visibility = Visibility.Collapsed;

			var c=AppConfig.Load();
			MyGlobal.AppConfig = c;
			//MyGlobal.AppOption.ExportFbfMode = c.ExportFbfMode;
			if (!string.IsNullOrEmpty(c.LoginForm?.BottomText))
			{
				tbBottom.Text = c.LoginForm.BottomText;// ToString();
				tbBottom.Visibility = Visibility.Visible;
			}
		}
    }
}
