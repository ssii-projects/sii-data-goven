using Agro.FrameWork;
using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Common;
using Agro.Library.Model;
using Agro.Module.DataExchange.Repository;
using Agro.Module.DataOperator;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using TestTool;
using DataOperatorTool.OData;
using System.Runtime.InteropServices;

namespace DataOperatorTool
{
    class MainWindow
    {
        class DownLoadImpl
        {
            private readonly MainWindow p;
            private readonly DownLoadService service=DownLoadService.Instance;
            public DownLoadImpl(MainWindow p)
            {
                this.p = p;
            }
            public void InitDownFileMenu()
            {
                p._miDownFile.Icon = new Image { Source = p.GetImage32("download") };
                p._miDownFile.Click += (s, e) =>
                {
                    var dlg = new DownloadLoginWindow
                    {
                        Owner = p.mainWnd
                    };
                    dlg.Login = (user, name) =>
                    {
                        service.Login(user, name);
                        DownloadFbfDkTask.ShowDialog(dlg, shpFile =>
                        {
                            p.OpenFile(shpFile);
                        });/// p.mainWnd);
                        //MessageBox.Show(service.Token,"token");
                        return null;
                    };
                    dlg.ShowDialog();

                    /*

                    var url = AppPref.DownLoadUrl + "/user/login";// "http://localhost:5000/user/login";
                    var datas = "Username=admin&Password=admin@123456";
                    var result = DownLoadService.httpPost(url, datas);
                    var m = JsonUtil.DeserializeObject<LoginModel>(result);
                    url = "http://localhost:5000/odata/lands?top=1&select=DKBM,Shape";
                    //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJuYW1laWQiOiJVX0NTQURNSU4iLCJzdWIiOiJhZG1pbiIsIm5hbWUiOiIiLCJwaG9uZV9udW1iZXIiOiIiLCJlbWFpbCI6IiIsIlpvbmUiOiIiLCJpYXQiOiIxNjc2MTIyNTg2IiwiZXhwIjoxNjc2MjA4OTg2LCJuYmYiOjE2NzYxMjI1ODZ9.2TvVv0EUCJcN7ltHgSabXRqVmSEDjlh74y9GDQjCxdM";
                    result = await DownLoadService.httpGet(url, m.Token);

                    Console.WriteLine(result);
                    MessageBox.Show("ok");
                    */
                };
            }
        }
        internal readonly TabMainWindow mainWnd;
        private readonly MenuItem _miOpenFile = new MenuItem()
        {
            Header = "打开",
            MinWidth = 200
        };
        private readonly MenuItem _miDownFile = new MenuItem() { Header = "下载发包方地块" };
        private readonly DownLoadImpl downLoadImpl;
        public MainWindow()
        {
            downLoadImpl = new DownLoadImpl(this);
            mainWnd = new TabMainWindow()
            {
                CaptionVisible = Visibility.Collapsed
            };

            if (AppPref.UseDownFbfDk)
            {
                InitOpenFileMenu();
                downLoadImpl.InitDownFileMenu();

                mainWnd.Events.OnMainMenuCreated += mi =>
                {
                    mi.Items.Insert(0, _miDownFile);
                    mi.Items.Insert(0, _miOpenFile);
                };
            }
            else
            {
                mainWnd.Loaded += (s, e1) =>
                {
                    mainWnd.StartButton.Content = "文件";
                };
                mainWnd.Events.OnPreShowMainMenu += it =>
                {
                    it.Cancel = true;
                    var suffix = AppPref.AppType == AppType.DataOperator_SQLite ? "dk" : "shp";
                    var dlg = new OpenFileDialog
                    {
                        Filter = $"{suffix}文件(*.{suffix})|*.{suffix}",
                        RestoreDirectory = true,
                        //FilterIndex = 1,
                        //Multiselect = true
                    };
                    if (dlg.ShowDialog() != true)
                    {
                        return;
                    }
                    OpenFile(dlg.FileName);
                };
            }
            mainWnd.Events.OnPageClosed += it =>
              {
                  try
                  {
                      if (it.Tag is IDisposable dis)
                      {
                          dis.Dispose();
                      }
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine(ex.Message);
#if DEBUG
                      MessageBox.Show(ex.ToString());
#endif
                  }
              };
        }
        void OpenFile(string fileName)
        {
            var n = fileName.LastIndexOfAny(new char[] { '/', '\\' });
            var title = fileName.Substring(n + 1);
            var icon = MyImageUtil.Image16("全图16.png");
            try
            {
                if (AppPref.AppType == AppType.DataOperator_SQLite)
                {
                    var db = SqliteFeatureWorkspaceFactory.Instance.OpenWorkspace(fileName);
                    if (!db.IsTableExists(DC_DLXX_DK.GetTableName()))
                    {
                        throw new Exception($"{title} 内部格式异常！");
                    }
                    var pnl = new MapPageSQLiteSource(db);
                    mainWnd.OpenPage(pnl, title, icon);
                }
                else
                {
                    var fc = OuterDcdkRepository.OpenSrcFeatureClass(eDatabaseType.ShapeFile, fileName, true);
                    var pnl = new MapPageShapeFileSource(fc,AppPref.AppType);
                    if(AppPref.AppType==AppType.DataOperator_WebService)
                    {
                        pnl.OnUploadData += shpFc =>
                        {
                            UploadService.Instance.UploadDkxxs(this, shpFc);
                        };
                    }
                    mainWnd.OpenPage(pnl, title, icon);
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowExceptionMessage(ex);
            }
        }

        private ImageSource GetImage32(string fileName)
        {
            ImageSource exitImg = null;
            try
            {
                var appName = Application.ResourceAssembly.GetName().Name;
                var path = $"pack://application:,,,/{appName};component/Resources/Images/32/{fileName}.png";
                exitImg = new BitmapImage(new Uri(path, UriKind.Absolute));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return exitImg;
        }
        private void InitOpenFileMenu()
        {
            _miOpenFile.Icon = new Image { Source = GetImage32("open") };

            _miOpenFile.Click += (s, e) =>
            {
                var suffix = AppPref.AppType == AppType.DataOperator_SQLite ? "dk" : "shp";
                var dlg = new OpenFileDialog
                {
                    Filter = $"{suffix}文件(*.{suffix})|*.{suffix}",
                    RestoreDirectory = true,
                    //FilterIndex = 1,
                    //Multiselect = true
                };
                if (dlg.ShowDialog() != true)
                {
                    return;
                }
                OpenFile(dlg.FileName);
            };
        }
        //private void InitDownFileMenu()
        //{
        //    _miDownFile.Icon = new System.Windows.Controls.Image { Source = GetImage32("download") };
        //    _miDownFile.Click += async (s, e) =>
        //    {
        //        var dlg = new DownloadLoginWindow
        //        {
        //            Owner = mainWnd
        //        };
        //        dlg.ShowDialog();


        //        var url = AppPref.DownLoadUrl+"/user/login";// "http://localhost:5000/user/login";
        //        var datas = "Username=admin&Password=admin@123456";
        //        var result = httpPost(url, datas);
        //        var m = JsonUtil.DeserializeObject<LoginModel>(result);
        //        url = "http://localhost:5000/odata/lands?top=1&select=DKBM,Shape";
        //        //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJuYW1laWQiOiJVX0NTQURNSU4iLCJzdWIiOiJhZG1pbiIsIm5hbWUiOiIiLCJwaG9uZV9udW1iZXIiOiIiLCJlbWFpbCI6IiIsIlpvbmUiOiIiLCJpYXQiOiIxNjc2MTIyNTg2IiwiZXhwIjoxNjc2MjA4OTg2LCJuYmYiOjE2NzYxMjI1ODZ9.2TvVv0EUCJcN7ltHgSabXRqVmSEDjlh74y9GDQjCxdM";
        //        result = await httpGet(url, m.Token);

        //        Console.WriteLine(result);
        //        MessageBox.Show("ok");
        //    };
        //}



    }
}
