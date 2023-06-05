using SII.GIS;
using SII.Library.Common;
using SII.Library.Common.Util;
using SII.LibCore;
using SII.LibCore.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SII.FrameApp
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
            //_persist =  new Persist(System.IO.Path.Combine(PathHelper.PersistDirectory, "persist.xml"));
            new NoneStyleWindowHelper(this,dragPart);
            LoadUI();
        }
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //if (false)
            //{//yxm 2018-3-30 SQLServer测试
            //    var cs = ConfigurationManager.ConnectionStrings[0];
            //    //bool fOracle = cs.ProviderName == "DataSource.Oracle";
            //    //if (!fOracle)
            //    //{
            //    //    throw new Exception("当前只支持Oracle数据库！");
            //    //}
            //    var sUserName = txtUsername.Text;
            //    var db1 = DatabaseFactory.CreateSqlServerDatabase(cs.ConnectionString);
            //    MyGlobal.Connected(db1, sUserName);

            //    DialogResult = true;
            //    SaveUI();
            //    Close();
            //}
            IFeatureWorkspace db = null;
            try
            {
                var sUserName = txtUsername.Text;
                //var cs =ConfigurationManager.ConnectionStrings[0];
                var cs = new DataSourceSelectPanel().GetDefaultDataSource();
                bool fOracle = cs.ProviderName== "DataSource.Oracle";
                if (fOracle)
                {
                    db = OracleFeatureWorkspaceFactory.Instance.OpenWorkspace(cs.ConnectionString);
                }
                else
                {
                    db = SQLServerFeatureWorkspaceFactory.Instance.OpenWorkspace(cs.ConnectionString);
                }

                if (!db.IsTableExists("CS_USER"))
                {
                    if(!(UserUtil.IsAdminUser(sUserName)&& txtPwd.Password == "123456"))
                    {
                        MessageBox.Show("初始用户名admin，密码123456", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    InitUserTables(db);
                }
                else
                {
                    if (!IsUserOK(db, sUserName, txtPwd.Password))
                    {
                        MessageBox.Show("用户名或密码错误！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (!db.IsTableExists("CS_LOG"))
                {
                    InitLogTable(db);
                }
                LogUtil.WriteLoginLog(db, sUserName);

                RepairDBUtil.Repair(db);

                //MyGlobal.LoginUserName = sUserName;
                MyGlobal.Connected(db, sUserName);
                DialogResult = true;
                SaveUI();
                Close();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            //object o = _persist.LoadSettingInfo("db_type");
            //if (o != "")
            //    _dbType = (DBType)Convert.ToInt32(o);
            //object server = _persist.LoadSettingInfo("mdb_file");
            //if (null != server && !string.IsNullOrEmpty(server.ToString()))
            //    miAccess.Header = server;
            //server = _persist.LoadSettingInfo("oracle_server");
            //if (null != server && !string.IsNullOrEmpty(server.ToString()))
            //    miOracle.Header = server;
            //server = _persist.LoadSettingInfo("sqlserver_server");
            //if (null != server && !string.IsNullOrEmpty(server.ToString()))
            //    miSqlServer.Header = server;

            //if (_dbType == DBType.Access)
            //{
            //    _strMdbFile = miAccess.Header.ToString();
            //    if (!File.Exists(_strMdbFile))
            //    {
            //        #region 设置默认的Access数据库为安装路径下DataBase目录下的mdb文件
            //        try
            //        {
            //            string mdb = AppDomain.CurrentDomain.BaseDirectory + @"..\DataBase\";
            //            DirectoryInfo TheFolder = new DirectoryInfo(mdb);
            //            //遍历文件
            //            foreach (FileInfo NextFile in TheFolder.GetFiles())
            //            {
            //                if (NextFile.FullName.ToLower().EndsWith(".mdb"))
            //                {
            //                    _strMdbFile = NextFile.FullName;
            //                    break;
            //                }
            //            }
            //        }
            //        catch
            //        {
            //        }
            //        #endregion
            //    }
            //    txtOracle.Text = _strMdbFile;
            //}
            //else if (_dbType == DBType.Oracle)
            //{
            //    txtOracle.Text = miOracle.Header.ToString();
            //}
            //else
            //{
            //    txtOracle.Text = miSqlServer.Header.ToString();
            //}
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
            //if (_dbType == DBType.Access)
            //{
            //    _strMdbFile = txtOracle.Text.Trim();
            //    _persist.SaveSettingInfo("mdb_file", _strMdbFile);
            //}
            //else if (_dbType == DBType.Oracle)
            //{
            //    _persist.SaveSettingInfo("oracle_server", txtOracle.Text.Trim());
            //}
            //else if (_dbType == DBType.SQLServer)
            //{
            //    _persist.SaveSettingInfo("sqlserver_server", txtOracle.Text.Trim());
            //}
            //_persist.SaveSettingInfo("db_type", ((int)_dbType).ToString());
            bool fSavePwd = chkSavePwd.IsChecked == true;
            _persist.SaveSettingInfo("login_chkSavePwd", fSavePwd ? "1" : "0");
            _persist.SaveSettingInfo("login_user", fSavePwd ? txtUsername.Text.Trim() : "");
            _persist.SaveSettingInfo("login_pwd", fSavePwd ? txtPwd.Password : "", true);
        }

        /// <summary>
        /// 创建日志表
        /// </summary>
        /// <param name="db"></param>
        private void InitLogTable(IWorkspace db)
        {
            //var sql = "create table CS_LOG(ID VARCHAR2(38) NOT NULL,LOGTYPE VARCHAR2(30) NOT NULL,USERNAME VARCHAR2(20) NOT NULL,LOGTIME  TIMESTAMP default systimestamp,LOGINFO CLOB)";
            //db.ExecuteNonQuery(sql);
            if (db.DatabaseType == eDatabaseType.Oracle)
            {
                var sql = "create table CS_LOG(ID VARCHAR2(38) NOT NULL,LOGTYPE VARCHAR2(30) NOT NULL,USERNAME VARCHAR2(20) NOT NULL,LOGTIME  TIMESTAMP default systimestamp,LOGINFO CLOB)";
                db.ExecuteNonQuery(sql);
            }
            else if (db.DatabaseType == eDatabaseType.SqlServer)
            {
                var sql = "create table CS_LOG(ID VARCHAR(38) NOT NULL,LOGTYPE VARCHAR(30) NOT NULL,USERNAME VARCHAR(20) NOT NULL,LOGTIME  DATETIME default getdate(),LOGINFO TEXT)";
                db.ExecuteNonQuery(sql);
            }
        }
        //private void WriteLoginLog(IWorkspace db,string sUserName)
        //{
        //    var sql = "insert into CS_LOG(ID,LOGTYPE,USERNAME,LOGINFO values('" + Guid.NewGuid().ToString() + "','系统日志','" + sUserName + "','登录系统')";
        //    db.ExecuteNonQuery(sql);
        //}
        /// <summary>
        /// 初始化用户表
        /// </summary>
        /// <param name="db"></param>
        private void InitUserTables(IWorkspace db)
        {
            //var sql = "create table CS_USER(USERNAME VARCHAR2(20) NOT NULL,PWD VARCHAR2(30) NOT NULL)";
            //db.ExecuteNonQuery(sql);
            //sql = "create table CS_USER_PERMISSION(USERNAME VARCHAR2(20) NOT NULL,FUNCTION_ID VARCHAR2(50) NOT NULL,FLAG int NOT NULL)";
            //db.ExecuteNonQuery(sql);
            //sql = "insert into CS_USER(USERNAME,PWD) values('admin','123456')";
            //db.ExecuteNonQuery(sql);
            if (db.DatabaseType == eDatabaseType.Oracle)
            {
                var sql = "create table CS_USER(USERNAME VARCHAR2(20) NOT NULL,PWD VARCHAR2(30) NOT NULL)";
                db.ExecuteNonQuery(sql);
                sql = "create table CS_USER_PERMISSION(USERNAME VARCHAR2(20) NOT NULL,FUNCTION_ID VARCHAR2(50) NOT NULL,FLAG int NOT NULL)";
                db.ExecuteNonQuery(sql);
                sql = "insert into CS_USER(USERNAME,PWD) values('admin','123456')";
                db.ExecuteNonQuery(sql);
            }
            else if (db.DatabaseType == eDatabaseType.SqlServer)
            {
                var sql = "create table CS_USER(USERNAME VARCHAR(20) NOT NULL,PWD VARCHAR(30) NOT NULL)";
                db.ExecuteNonQuery(sql);
                sql = "create table CS_USER_PERMISSION(USERNAME VARCHAR(20) NOT NULL,FUNCTION_ID VARCHAR(50) NOT NULL,FLAG int NOT NULL)";
                db.ExecuteNonQuery(sql);
                sql = "insert into CS_USER(USERNAME,PWD) values('admin','123456')";
                db.ExecuteNonQuery(sql);

            }
        }

        private bool IsUserOK(IWorkspace db,string userName,string pwd)
        {
            bool fOK = false;
            userName = userName.Replace("'", "''");
            pwd = pwd.Replace("'", "''");
            var sql = "select count(*) from CS_USER where USERNAME='" + userName + "' and PWD='" + pwd + "'";
            db.QueryCallback(sql, r =>
            {
                var n=SafeConvertAux.ToInt32(r.GetValue(0));
                fOK = n == 1;
                return false;
            });
            return fOK;
        }
    }
}
