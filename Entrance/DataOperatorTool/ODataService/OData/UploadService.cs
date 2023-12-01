using Agro.GIS;
using Agro.LibCore;
using Agro.Library.Model;
using Agro.Module.DataExchange;
using Agro.Module.DataExchange.Repository;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Agro.Module.DataExchange.ExportDkJsonTask;

namespace DataOperatorTool.OData
{
    internal class UploadService : ODataService
    {
        private UploadService() { }
        public static UploadService Instance = new();
        public void UploadDkxxs(MainWindow window, ShapeFileFeatureClass srcFc)
        {
            using var _srcRepos = new OuterDcdkRepository(srcFc, false);
            //using var topCheck = new ImportDcdkData.TopChecker(_srcRepos);
            try
            {
                var cancel = NotCancelTracker.Instance;
                var lst = new List<VEC_SURVEY_DK>();
                _srcRepos.LoadChangeData(cancel, en => lst.Add(en),false,false);
                if (cancel?.Cancel() == true) return;
                //topCheck.Build(lst, cancel);

                //var lstCbf = new List<DC_QSSJ_CBF>();
                //var lstCbfJtcy = new Dictionary<string, List<DC_QSSJ_CBF_JTCY>>();

                var cnt = lst.Count;// + lstCbf.Count + lstCbfJtcy.Count;
                if (cnt == 0)
                {
                    MessageBox.Show("没有需要上传的数据","提示",MessageBoxButton.OK,MessageBoxImage.Warning);
                    return;
                }
                var dkxxs = new PostDkxxs
                {
                    Dkxx = new PostDkxx[lst.Count]
                };
                var srid = srcFc.SpatialReference?.AuthorityCode ?? 0;
                var wktWriter = new WKTWriter();
                for(int i = lst.Count; --i >= 0;)
                {
                    dkxxs.Dkxx[i] = new(lst[i], srid, wktWriter);
                }
                //var json=JsonUtil.SerializeObject(dkxxs);

                var json = JsonConvert.SerializeObject(dkxxs, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver=new CamelCasePropertyNamesContractResolver(),
                });

                new DownloadLoginWindow
                {
                    Owner = window.mainWnd,
                    Login = (user, name) =>
                    {
                        Login(user, name);
                        var url = AppPref.WebServiceUrl + "/lands/change/batch";// "http://localhost:5000/lands/change/batch";
                        Task.Run(() =>
                        {
                            try
                            {
                                var msg=PostJson(url, json, Token);
                                window.mainWnd.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show($"上传成功：{msg}", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                });
                            }
                            catch (Exception ex)
                            {
                                window.mainWnd.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(ex.Message,"错误",MessageBoxButton.OK, MessageBoxImage.Error);    
                                });
                            }
                        });
                        return null;
                    }
                }.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
