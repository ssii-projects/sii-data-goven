using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/4/18 18:28:41
*/
namespace TestTool
{
	class HttpUtil
	{
		// Get请求
		public static string GetResponse(string url, out string statusCode)
		{
			string result = string.Empty;

			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				HttpResponseMessage response = httpClient.GetAsync(url).Result;
				statusCode = response.StatusCode.ToString();

				if (response.IsSuccessStatusCode)
				{
					result = response.Content.ReadAsStringAsync().Result;
				}
			}
			return result;
		}

		// 泛型：Get请求
		public static T GetResponse<T>(string url) where T : class, new()
		{
			T result = default(T);

			using (HttpClient httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
				HttpResponseMessage response = httpClient.GetAsync(url).Result;

				if (response.IsSuccessStatusCode)
				{
					var t = response.Content.ReadAsStringAsync();

					string s = t.Result;
					string json = JsonConvert.DeserializeObject(s).ToString();
					result = JsonConvert.DeserializeObject<T>(json);
				}
			}
			return result;
		}

		/// <summary>
		/// POST 同步
		/// </summary>
		/// <param name="url"></param>
		/// <param name="postStream"></param>
		/// <param name="encoding"></param>
		/// <param name="timeOut"></param>
		/// <returns></returns>
		public static string HttpPost(string url, string json)
		{
			HttpClient client = new HttpClient();
			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
			var t = client.PostAsync(url, content);
			t.Wait();
			var t2 = t.Result.Content.ReadAsStringAsync();
			return t2.Result;
			//HttpResponseMessage response = await client.PostAsync(url, content);
			//response.EnsureSuccessStatusCode();
			//string responseBody = await response.Content.ReadAsStringAsync();
			//return responseBody;
			/*
			HttpClientHandler handler = new HttpClientHandler();

			HttpClient client = new HttpClient(handler);
			MemoryStream ms = new MemoryStream();
			formData.FillFormDataStream(ms);//填充formData
			HttpContent hc = new StreamContent(ms);


			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("", 0.8));
			hc.Headers.Add("UserAgent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.57 Safari/537.36");
			hc.Headers.Add("Timeout", timeOut.ToString());
			hc.Headers.Add("KeepAlive", "true");

			var t = client.PostAsync(url, hc);
			t.Wait();
			var t2 = t.Result.Content.ReadAsByteArrayAsync();
			return encoding.GetString(t2.Result);
			*/
		}

		/// <summary>
		/// 使用post方法异步请求
		/// </summary>
		/// <param name="url">目标链接</param>
		/// <param name="json">发送的参数字符串，只能用json</param>
		/// <returns>返回的字符串</returns>
		public static async Task<string> PostAsyncJson(string url, string json)
		{
			HttpClient client = new HttpClient();
			HttpContent content = new StringContent(json);
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
			HttpResponseMessage response = await client.PostAsync(url, content);
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			return responseBody;
		}

		// Put请求
		public static string PutResponse(string url, string putData, out string statusCode, string contentType = "application/json")
		{
			string result = string.Empty;
			HttpContent httpContent = new StringContent(putData);
			httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType)
			{
				CharSet = "utf-8"
			};
			using (HttpClient httpClient = new HttpClient())
			{
				HttpResponseMessage response = httpClient.PutAsync(url, httpContent).Result;
				statusCode = response.StatusCode.ToString();
				//if (response.IsSuccessStatusCode)
				{
					result = response.Content.ReadAsStringAsync().Result;
				}
			}
			return result;
		}

		/// <summary>
		///  API发送DELETE请求，返回状态：200成功，201失败
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string ApiDelete(string url)
		{
			HttpClient client = new HttpClient();
			//client.BaseAddress = new Uri(System.Configuration.ConfigurationManager.AppSettings["ApiHttp"]);
			// 为JSON格式添加一个Accept报头
			client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			HttpResponseMessage response = client.DeleteAsync(url).Result;
			if (response.IsSuccessStatusCode)
			{
				return response.Content.ReadAsStringAsync().Result;
			}
			return "";
		}
	}
	class JsonUtil
	{
		public static string ConvertJsonString(string str)
		{
			//格式化json字符串
			JsonSerializer serializer = new JsonSerializer();
			TextReader tr = new StringReader(str);
			JsonTextReader jtr = new JsonTextReader(tr);
			object obj = serializer.Deserialize(jtr);
			if (obj != null)
			{
				StringWriter textWriter = new StringWriter();
				JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
				{
					Formatting = Formatting.Indented,
					Indentation = 4,
					IndentChar = ' '
				};
				serializer.Serialize(jsonWriter, obj);
				return textWriter.ToString();
			}
			else
			{
				return str;
			}
		}
	}
}
