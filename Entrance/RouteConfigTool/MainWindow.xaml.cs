using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace RouteConfigTool
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		class NjqJson
		{
			public static void ParseJson(string json, Action<string, string> callback)
			{
				var jsonObj = JObject.Parse(json);
				ParseJObject(jsonObj, callback);
			}
			static void ParseJObject(JObject jo, Action<string, string> callback)
			{
				foreach (var kn in jo)
				{
					if (kn.Value is JArray ja)
					{
						foreach (var x in ja)
						{
							if (x is JObject jo1)
							{
								string bm = null;
								string url = null;
								foreach (var kv in jo1)
								{
									switch (kv.Key.ToUpper())
									{
										case "BM": bm = kv.Value.ToString(); break;
										case "URL": url = kv.Value.ToString(); break;
									}
									if (bm != null && url != null)
									{
										callback(bm, url);
										break;
									}
								}
							}
						}
					}
					else if (kn.Value is JObject jo1)
					{
						ParseJObject(jo1, callback);
					}
				}
			}

		}
		class ZonesJson
		{
			class Node
			{
				internal string BM;
				public string JsonString;
				public readonly List<Node> Children = new List<Node>();
				public Node(string bm)
				{
					BM = bm;
				}
				public override string ToString()
				{
					if (Children.Count == 0)
					{
						return JsonString;
					}
					else
					{
						var str = Key("subs")+":["+Children[0];
						for (int i = 1; i < Children.Count; ++i)
						{
							str += "," + Children[i];
						}
						str += "]";
						str = JsonString.Substring(0,JsonString.Length-1)+"," + str + "}";
						return str;
					}
				}
			}
			private static readonly List<Node> _nodes = new List<Node>();
			public static string Convert(string json)
			{
				_nodes.Clear();
				var jsonObj = JObject.Parse(json);
				Convert(jsonObj, _nodes);
				var err = Check(_nodes);
				if (err != null)
					throw new Exception(err);
				var str = "[";
				if (_nodes.Count > 0)
				{
					str += _nodes[0].ToString();
					for (int i = 1; i < _nodes.Count; ++i)
					{
						str += "," + _nodes[i];
					}
				}
				str += "]";
				str = JsonUtil.ConvertJsonString(str);
				return str;
			}
			static void Convert(JObject jo, List<Node> nodes)
			{
				foreach (var kn in jo)
				{
					var sa = kn.Key.Split('#');
					if (sa.Length != 2)
						throw new Exception($"{kn.Key}格式不正确，键命名规范： \"code#name\"格式");
					var node = new Node(sa[0])
					{
						JsonString= MakeJson(sa[0], sa[1])
					};
					nodes.Add(node);
					if (kn.Value is JArray ja)
					{
						foreach (var x in ja)
						{
							if (x is JObject jo1)
							{
								string urlKey = null;
								foreach (var kv in jo1)
								{
									if (string.Compare(kv.Key, "url", true) == 0)
									{
										urlKey = kv.Key;
										break;
									}
								}
								if (urlKey != null)
								{
									jo1.Remove(urlKey);
								}
								string bm = null;
								if (jo1.TryGetValue("bm", out JToken jt))
								{
									bm = jt.ToString();
								}
								node.Children.Add(new Node(bm)
								{
									JsonString = jo1.ToString()
								}); ;
							}
						}
					}
					else if (kn.Value is JObject jo1)
					{
						Convert(jo1, node.Children);
					}
				}
			}
			static string MakeJson(string bm, string mc)
			{
				var str ="{"+ $"{Key("bm")}:{Val(bm)},{Key("mc")}:{Val(mc)}"+"}";
				return str;
			}
			static string Key(string str)
			{
				return $"\"{str}\"";
			}
			static string Val(string str) {
				return Key(str);
			}
			static string Check(List<Node> nodes)
			{
				var set = new HashSet<string>();
				foreach (var n in nodes)
				{
					if (n.BM != null)
					{
						if(set.Contains(n.BM))
							return $"编码{n.BM}重复";
						set.Add(n.BM);
					}
				}
				return null;
			}
			
		}
		public MainWindow()
		{
			InitializeComponent();
			sidebarPage.LeftPanelWidth =new GridLength(500);
			FormapJson();
			teOutput.Text = LoadTemplate();
		}

		string LoadTemplate()
		{
			var njqConfFile = AppDomain.CurrentDomain.BaseDirectory + @"template\njq.conf";
			return File.ReadAllText(njqConfFile);
		}
		void FormapJson()
		{
			var txt = JsonUtil.ConvertJsonString(textEditor.Text);
			if (txt != textEditor.Text)
			{
				textEditor.Text = txt;
			}
		}

		void BtnHandle(Action action)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		static string DA()
		{
			return "\r\n";
		}
		string BuildLocation(string countyCode,string url)
		{
			var str = DoBuildLocation(countyCode, url);//, "cas");
			//var sa = new string[] { "regcert", "transtion", "mortgage", "dispuarbit" };
			//foreach (var appName in sa)
			//{
			//	str += DA()+ "  "+DA();// "\r\n\r\n";
			//	str += BuildLocation(countyCode, url, appName);
			//}
			return str;
		}
		string DoBuildLocation(string countyCode, string url, string appName=null)
		{
			string str = $"  location /{countyCode}/{appName} {{";
			if (string.IsNullOrEmpty(appName))
			{
				str= $"  location /{countyCode} {{";
			}
			str +=DA()+$"    proxy_pass {url};";
			str +=DA()+"    proxy_set_header Host $host;";
			str +=DA()+"    proxy_set_header X-Real-IP $remote_addr;";
			str +=DA()+"    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;";
			str +=DA()+"    proxy_set_header Upgrade $http_upgrade;";
			str +=DA()+"    proxy_set_header Connection \"upgrade\"";
			str +=DA()+"    proxy_connect_timeout 90;";
			str += DA()+ "  }";
			return str;
		}
		//void ParseJson(string json, Action<string,string> callback)
		//{
		//	var jsonObj = JObject.Parse(json);
		//	ParseJObject(jsonObj, callback);
		//}
		//void ParseJObject(JObject jo, Action<string, string> callback)
		//{
		//	foreach (var kn in jo)
		//	{
		//		if (kn.Value is JArray ja)
		//		{
		//			foreach (var x in ja)
		//			{
		//				if (x is JObject jo1)
		//				{
		//					string bm = null;
		//					string url = null;
		//					foreach (var kv in jo1)
		//					{
		//						switch (kv.Key.ToUpper())
		//						{
		//							case "BM":bm = kv.Value.ToString();break;
		//							case "URL":url = kv.Value.ToString();break;
		//						}
		//						if (bm != null && url != null)
		//						{
		//							callback(bm, url);
		//							break;
		//						}
		//					}
		//				}
		//			}
		//		}
		//		else if (kn.Value is JObject jo1)
		//		{
		//			ParseJObject(jo1, callback);
		//		}
		//	}
		//}

		void BtnFormatClick(object sender, RoutedEventArgs e)
		{
			BtnHandle(()=>FormapJson());
		}

		void BtnSaveClick(object sender, RoutedEventArgs e)
		{
			BtnHandle(() =>
			{
				var dlg = new SaveFileDialog
				{
					Filter = "Ngix配置文件(*.conf)|*.conf",
					OverwritePrompt=true,
					FileName="njq.conf",
				};
				if (true == dlg.ShowDialog())
				{
					File.WriteAllText(dlg.FileName, teOutput.Text.Trim());
					MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK,MessageBoxImage.Information);
				}
			});
		}
		void BtnSaveZonesClick(object sender, RoutedEventArgs e)
		{
			BtnHandle(() =>
			{
				var dlg = new SaveFileDialog
				{
					Filter = "Zones文件(*.json)|*.json",
					OverwritePrompt = true,
					FileName = "zones.json",
				};
				if (true == dlg.ShowDialog())
				{
					File.WriteAllText(dlg.FileName, teOutput.Text.Trim());
					MessageBox.Show("保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			});
		}
		void OpenFileClick(object sender, RoutedEventArgs e)
		{
			BtnHandle(() =>
			{
				var dlg = new OpenFileDialog()
				{
					Filter="路由映射文件(*.json)|*.json",
				};
				if (true == dlg.ShowDialog())
				{
					textEditor.Text = File.ReadAllText(dlg.FileName);
				}
			});
		}
		void SaveFileClick(object sender, RoutedEventArgs e)
		{
			BtnHandle(() =>
			{
				var dlg = new SaveFileDialog()
				{
					Filter = "路由映射文件(*.json)|*.json",
					OverwritePrompt=true
				};
				if (true == dlg.ShowDialog())
				{
					File.WriteAllText(dlg.FileName,textEditor.Text);
				}
			});
		}

		string BuildNjqJson()
		{
			var str = LoadTemplate();
			string str1 = "";
			var json = textEditor.Text;

			var lst = new List<Tuple<string, string>>();
			NjqJson.ParseJson(json, (countyCode, url) =>
			{
				if (lst.Find(a => { return string.Compare(a.Item1, countyCode, true) == 0; }) != null) {
					throw new Exception($"县级代码 {countyCode} 重复！");
				}
				lst.Add(new Tuple<string, string>(countyCode, url));
				//str1 += DA() + BuildLocation(countyCode, url) + DA();
			});
			foreach (var it in lst)
			{
				var countyCode = it.Item1;
				var url = it.Item2;
				str1 += DA() + BuildLocation(countyCode, url) + DA();
			}
			var n = str.LastIndexOf('}');
			str = str.Insert(n, str1);
			return str;
		}
		string BuildZonesJson()
		{
			var str = ZonesJson.Convert(textEditor.Text);
			return str;
		}
		private void BtnApplyClick(object sender, RoutedEventArgs e)
		{
			BtnHandle(() =>
			{
				FormapJson();
				teOutput.Text = BuildNjqJson();
				teOutputZones.Text = BuildZonesJson();
			});
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (sender == btnC1)
			{
				textEditor.Text = "{\"51#四川省\": {\"5101#成都市\": [{\"bm\": 510104,\"mc\": \"锦江区\",\"url\": \"http://localhost:6080\"}]}}";
				FormapJson();
			}
			else if (sender == btnC2)
			{
				textEditor.Text = "{\"5101#成都市\": [{\"bm\": 510104,\"mc\": \"锦江区\",\"url\": \"http://localhost:6080\"}]}";
				FormapJson();
			}
		}
	}
}
