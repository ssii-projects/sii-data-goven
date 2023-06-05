using Agro.LibCore;
using System;
using System.IO;
using System.Text;

namespace Agro.Library.Common
{
	public class ProductInfo
	{
		public string Title { get; set; } = "农村土地承包经营权数据库管理系统";
		public string Version { get; set; } = "1.0.0.0102";
		public string Description { get; set; } = "提供数据分析入库、查询统计、主题分析、数据更新等相关功能";
	};

	public class Support
	{
		public string Email { get; set; } = "";
		public string PhoneNum { get; set; } = "";
		public string Copyright { get; set; } = "";
	}
	public class LoginForm {
		public string BottomText { get; set; } = "";
	}
	public class AppConfig
	{
		public ProductInfo ProductInfo { get; set; } = new ProductInfo();
		public Support Support { get; set; } = new Support();
		public LoginForm LoginForm { get; set; } = new LoginForm();
		public EExportFbfMode ExportFbfMode { get; set; } = EExportFbfMode.SingleExport;

		public static AppConfig Load()
		{
			var appJsonFile = AppDomain.CurrentDomain.BaseDirectory + @"App\App.json";
			if (File.Exists(appJsonFile))
			{
				var json = File.ReadAllText(appJsonFile,Encoding.UTF8);
				return JsonUtil.DeserializeObject<AppConfig>(json);
			}
			return new AppConfig();
		}
		public static void Save(AppConfig appConfig)
		{
			var appJsonFile = AppDomain.CurrentDomain.BaseDirectory + @"App\App.json";
			var json=JsonUtil.SerializeObject(appConfig);
			File.WriteAllText(appJsonFile, json);//,Encoding.UTF8);
		}
	}
}
