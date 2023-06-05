using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/6/19 11:25:14
*/
namespace RouteConfigTool
{
	class ParseJsonTest
	{
		public void Test()
		{
			var file = @"D:\GitProjects\sii\Business\SII.DataGoven\WorkSpace\App\App.json";
			var json=File.ReadAllText(file);
			var jsonObj = JObject.Parse(json);
			Console.WriteLine(jsonObj);
		}
	}
}
