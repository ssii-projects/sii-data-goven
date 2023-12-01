using Agro.LibCore;
using Agro.Library.Model;
using NPOI.SS.Formula.Functions;
using RTools_NTS.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DataOperatorTool.OData
{
    internal class ODataService
    {
        public string Token { get; set; } = string.Empty;
        public void Login(string userName, string password)
        {
            var url = AppPref.WebServiceUrl + "/user/login";// "http://localhost:5000/user/login";
            var datas = $"Username={userName}&Password={password}";
            var result = PostXForm(url, datas);
            var m = JsonUtil.DeserializeObject<LoginModel>(result);
            Token = m.Token;
            //Test();
        }
        protected static string PostJson(string url, string data,string token)
        {
            var accept = "application/json";
            var contentType = "application/json";// ; charset=UTF-8";
            return Post(url, data, accept, contentType,token);
        }
        private static string PostXForm(string url, string data)
        {
            var accept = "text/html, application/xhtml+xml, */*";
            var contentType = "application/x-www-form-urlencoded";
            return Post(url, data, accept, contentType);

            //string paramData = data;
            //var request = (HttpWebRequest)WebRequest.Create(url);
            //request.Method = "POST";
            //byte[] bytes = Encoding.UTF8.GetBytes(paramData);
            //request.Accept = "text/html, application/xhtml+xml, */*";
            //request.ContentType = "application/x-www-form-urlencoded";
            ////设置C# HttpWebRequest post请求头消息
            ////request.Headers.Add("version", "1.0.0");
            ////request.Headers.Add("ak", "45b83b3b780d46028afa1da41dcdd0a4");
            ////request.Headers.Add("sk", "1+JX1Gsg3hEu4Ui+VdQvWlaz1gY=");
            //request.ContentLength = bytes.Length;
            ////try
            ////{
            //using var myResponseStream = request.GetRequestStream();
            //myResponseStream.Write(bytes, 0, bytes.Length);
            //using var response = (HttpWebResponse)request.GetResponse();
            //using var myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            //var retString = myStreamReader.ReadToEnd();
            //request.Abort();
            ////var statusCode = (int)response.StatusCode;
            ////if (statusCode == 200)
            ////{
            ////    // Console.WriteLine(statusCode);
            ////    myStreamReader.Close();
            ////    myResponseStream.Close();

            ////    response?.Close();
            ////    request?.Abort();
            ////}
            ////}
            ////catch (Exception ex)
            ////{
            ////    //抛出异常返回具体错误消息
            ////    retString = ex.Message;
            ////}
            //return retString;

        }

        private static string Post(string url, string data,string accept,string contentType,string? token=null)
        {
            var paramData = data;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("ContentType", contentType);// "application/json");
            client.DefaultRequestHeaders.Add("Accept", accept);// "*/*");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            }

            HttpContent content = new StringContent(data);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);// "application/json");
            var result = client.PostAsync(url, content);
            result.Wait();
            var Result=result.Result;
            if (Result.IsSuccessStatusCode)//.StatusCode == System.Net.HttpStatusCode.OK
            {
                var responseJson = Result.Content.ReadAsStringAsync();
                responseJson.Wait();
                return responseJson.Result;
            }
            else
            {
                var responseJson = Result.Content.ReadAsStringAsync();
                responseJson.Wait();
                var err = responseJson.Result;
                throw new Exception(err);
            }
        }
    }
    /// <summary>
    /// 承包地块数据下载服务
    /// </summary>
    internal class DownLoadService: ODataService
    {
        private DownLoadService() { }
        public static DownLoadService Instance = new();


        public async Task<ZonesModel> GetCounties()
        {
            var url = AppPref.WebServiceUrl + $"/odata/zones?filter=jb eq {(int)eZoneLevel.County}";// "http://localhost:5000/user/login";
            var result = await httpGet(url, Token);
            return JsonUtil.DeserializeObject<ZonesModel>(result);
        }
        public async Task<ZonesModel> GetSubZones(string sjid)
        {
            var url = AppPref.WebServiceUrl + $"/odata/zones?filter=SJID eq '{sjid}'";// "http://localhost:5000/user/login";
            var result = await httpGet(url, Token);
            return JsonUtil.DeserializeObject<ZonesModel>(result);
        }

        public async Task<int> CountDk(string zoneCode)
        {
            var url = AppPref.WebServiceUrl + $"/odata/lands/$count?filter=startswith(DKBM,'{zoneCode}') and zt eq 1";
            var result = await httpGet(url, Token);
            var cnt = SafeConvertAux.ToInt32(result);
            return cnt;
        }
        public async Task<int> GetSRID()
        {
            var url = AppPref.WebServiceUrl + $"/odata/lands/?select=shape & top=1";
            var result = await httpGet(url, Token);
            var cnt = 0;
            var t=JsonUtil.DeserializeObject<ValueModel<LandModel>>(result);
            if (t.Value?.Length > 0)
            {
                var sa=t.Value[0].Shape?.Split(';');
                if(sa?.Length> 0)
                {
                    sa=sa[0].Split('=');
                    if (sa?.Length > 0)
                    {
                        cnt = SafeConvertAux.ToInt32(sa[1]);
                    }
                }
            }
            //Console.WriteLine(t);
            return cnt;
        }
        public async Task GetLands(string zoneCode,Action<LandsModel> callback)
        {
            var url = AppPref.WebServiceUrl + $"/odata/lands?filter=startswith(DKBM,'{zoneCode}') and zt eq 1";
            var result = await httpGet(url, Token);
            var t=JsonUtil.DeserializeObject<LandsModel>(result);
            if (t == null) return;
            callback(t);
            while(t.NextLink!= null)
            {
                url = t!.NextLink;
                result = await httpGet(url, Token);
                t = JsonUtil.DeserializeObject<LandsModel>(result);
                if (t == null) return;
                callback(t);
            }
        }

        public async Task GetDkbm2Cbfbm(string zoneCode,Action<ValueModel<DkbmAndCbfBm>> callback)
        {
            var url = AppPref.WebServiceUrl + $"/odata/ContractLands?filter=startswith(DKBM,'{zoneCode}') & select=DKBM,CBFBM";
            var result = await httpGet(url, Token);
            var t = JsonUtil.DeserializeObject<ValueModel<DkbmAndCbfBm>>(result);
            if (t == null) return;
            callback(t);
            while (t.NextLink != null)
            {
                url = t!.NextLink;
                result = await httpGet(url, Token);
                t = JsonUtil.DeserializeObject<ValueModel<DkbmAndCbfBm>>(result);
                if (t == null) return;
                callback(t);
            }
        }

        public async void Test()
        {
            var url = AppPref.WebServiceUrl + "/odata/zones";// "http://localhost:5000/user/login";
            var result = await httpGet(url, Token);
            var m = JsonUtil.DeserializeObject<ZonesModel>(result);
            Console.WriteLine(m);
        }

        public static async Task<string> httpGet(string url, string token)
        {
            using var http = new System.Net.Http.HttpClient();
            //http.DefaultRequestHeaders.Add("User - Agent", @"Mozilla / 5.0(compatible; Baiduspider / 2.0;)");
            //http.DefaultRequestHeaders.Add("Accept", @"text / html, application / xhtml + xml, application / xml; q = 0.9,image / webp,/; q = 0.8");
            http.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var t = await http.GetAsync(url);
            var r = t.Content.ReadAsStringAsync();//.Result;
                                                  //var myStreamReader = new System.IO.StreamReader(myResponseStream, System.Text.Encoding.GetEncoding("utf - 8"));
                                                  //var result = myStreamReader.ReadToEnd();
                                                  //myStreamReader.Close();
                                                  //myResponseStream.Close();
                                                  //Console.WriteLine(myResponseStream);
                                                  //Console.WriteLine(t);
                                                  //return myResponseStream;
            return r.Result;
        }

    }
}
