using Agro.Library.Common;
using Agro.LibCore;
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

namespace Agro.Module.Authority
{
    /// <summary>
    /// AddUserPanel.xaml 的交互逻辑
    /// </summary>
    public partial class AddUserPanel : UserControl
    {
        public AddUserPanel(bool fShowUserBox=true)
        {
            InitializeComponent();
            if (!fShowUserBox)
            {
                dpUserName.Visibility = Visibility.Collapsed;
            }
        }
        public bool OnAddUserOK()
        {
            try
            {
                var sUserName = txtUsername.Text.Trim();
                if ( sUserName.Length== 0)
                {
                    ShowWarning("未输入用户名！");
                    return false;
                }
                if (sUserName.Contains('\''))
                {
                    ShowWarning("用户名不能包含单引号（'）字符！");
                    return false;
                }
                var sPwd = txtPwd.Password;
                if (sPwd.Length < 6)
                {
                    ShowWarning("密码长度至少6位！");
                    return false;
                }
                if (sPwd != txtPwd1.Password)
                {
                    ShowWarning("两次密码输入不一致！");
                    return false;
                }
                var db = MyGlobal.Workspace;
                //using (var db = DataBaseSource.GetDatabase())
                {
                    bool fExist = false;
                    var sql = "select count(*) from CS_USER where USERNAME='"+sUserName+"'";
                    db.QueryCallback(sql, r =>
                    {
                        fExist = SafeConvertAux.ToInt32(r.GetValue(0))>0;
                        return true;
                    });
                    if (fExist)
                    {
                        ShowWarning("该用户名已存在！");
                        return false;
                    }
                    DoAddUser(db, sUserName, sPwd);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public bool OnModifyPwdOK(string sUserName)
        {
            try
            {
                var sPwd = txtPwd.Password;
                if (sPwd.Length < 6)
                {
                    ShowWarning("密码长度至少6位！");
                    return false;
                }
                if (sPwd != txtPwd1.Password)
                {
                    ShowWarning("两次密码输入不一致！");
                    return false;
                }
                var db = MyGlobal.Workspace;
                //using (var db = DataBaseSource.GetDatabase())
                {
                    sPwd = sPwd.Replace("'", "''");
                    var sql = "update CS_USER set PWD='"+sPwd+"' where USERNAME='" + sUserName + "'";
                    db.ExecuteNonQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        private void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void DoAddUser(IWorkspace db,string sUserName,string sPwd)
        {
            sPwd = sPwd.Replace("'", "''");
            var sql = "insert into CS_USER(USERNAME,PWD) values('" + sUserName + "','" + sPwd + "')";
            db.ExecuteNonQuery(sql);
        }
    }
}
