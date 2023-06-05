using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Agro.Module.DataExchange.XmlTask
{
	class ImportXmlTask : GroupTask
	{
		class MyTask : Task
		{
			private readonly string _xmlFile;
			public MyTask(System.IO.FileInfo fi)
			{
				_xmlFile = fi.FullName;
				Name = "导入Xml数据";

				Description = $"导入 {fi.Name}";
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				var db = MyGlobal.Workspace;
				var xml = new XmlDocument();
				xml.Load(_xmlFile);
				var err = CheckXml(xml, _xmlFile);
				if (err != null)
				{
					ReportError(err);
					return;
				}

				var ywh = XmlExchangeUtil.GetXmlInnerText(xml.SelectSingleNode("/SUBMIT/BUSINESS_DATA/YWLSH"));
				var sql = $"select count(1) from DJ_YW_SLSQ where YWH='{ywh}'";
				if (SafeConvertAux.ToInt32(db.QueryOne(sql)) > 0)
				{
					ReportWarning($"业务流水号 {ywh} 在数据库中已存在");
					ReportProgress(100);
					return;
				}
				
				var sFSF = XmlExchangeUtil.GetXmlInnerText(xml.SelectSingleNode("/SUBMIT/HEAD/FSF"));
				var sSJLX = SafeConvertAux.ToInt32(XmlExchangeUtil.GetXmlInnerText(xml.SelectSingleNode("/SUBMIT/HEAD/SJLX")));
				var QYWLSH = XmlExchangeUtil.GetXmlInnerText(xml.SelectSingleNode("/SUBMIT/BUSINESS_DATA/QYWLSH"));
				if (!string.IsNullOrEmpty(QYWLSH))
				{
					sql = $"select count(1) from DJ_YW_SLSQ where YWH='{QYWLSH}'";
					if (SafeConvertAux.ToInt32(db.QueryOne(sql)) == 0)
					{
						ReportWarning($"前业务流水号 {QYWLSH} 在数据库中不存在，请先导入前业务Xml数据");
						ReportProgress(100);
						return;
					}
				}

				var data = new XmlData(this, db,new Head(){
					SJLX=sSJLX,FSF= sFSF });
				data.ReadXml(xml);

				data.Import();

				ReportInfomation($"成功导入文件{_xmlFile}");
				ReportProgress(100);
			}

			string CheckXml(XmlDocument Xml,string FilePath)
			{
				Xml.Load(FilePath);
				if (null == Xml.SelectSingleNode("/SUBMIT"))
				{
					return $"{FilePath}不是有效的xml文件（根节点名称必须为SUBMIT）";
				}
				var sa = new string[] { "/SUBMIT/HEAD", "/SUBMIT/BUSINESS_DATA", "/SUBMIT/ORIGINAL_DATA"
					, "/SUBMIT/CHANGE_DATA","/SUBMIT/BUSINESS_DATA/YWLSH","/SUBMIT/BUSINESS_DATA/XZQDM"
				,"/SUBMIT/HEAD/SJLX","/SUBMIT/HEAD/FSF"};
				foreach (var s in sa)
				{
					if (null == Xml.SelectSingleNode(s))
					{
						return $"{FilePath}不是有效的xml文件（未找到{s}节点）";
					}
				}
				var sjlx = SafeConvertAux.ToInt32(XmlExchangeUtil.GetXmlInnerText(Xml.SelectSingleNode("/SUBMIT/HEAD/SJLX")));
				if (!BusinessDataFactory.IsValidSjlx(sjlx))
				{
					return $"Xml中的数据类型 {sjlx} 无效！【有效值为 101-107 或 201或202】";
				}
				var xzqdm = XmlExchangeUtil.GetXmlInnerText(Xml.SelectSingleNode("/SUBMIT/BUSINESS_DATA/XZQDM"));
				if (string.IsNullOrEmpty(xzqdm) || xzqdm.Length != 6)
				{
					return "/SUBMIT/BUSINESS_DATA/XZQDM节点内容无效，必须为6位行政区代码";
				}
				var lst = new List<string>();
				MyGlobal.Workspace.QueryCallback("select BM from DLXX_XZDY where JB=4 and BM is not null", r =>
				{
					lst.Add(r.GetString(0));
					return true;
				});
				if (!lst.Contains(xzqdm))
				{
					if (lst.Count == 1)
					{
						return $"Xml文件中的行政区代码 {xzqdm} 与数据库中的行政区代码 {lst[0]} 不匹配！";
					}
					return $"Xml文件中的行政区代码 {xzqdm} 与数据库中的行政区代码不匹配！";
				}
				return null;
			}
		}
		public ImportXmlTask()
		{
			Name = "导入xml数据";
			Description = "导入符合农村土地承包经营权信息应用平台接入技术规范的xml数据";
			PropertyPage = new ImportXmlPanel(()=>
			{
				
				base.ClearTasks();
				var prm = PropertyPage as ImportXmlPanel;
				foreach (var i in prm.Items)
				{
					if (i.IsChecked)
					{
						AddTask(new MyTask(i.FileInfo));
					}
				}
			});
			base.IsExpanded = true;
		}
	}
}
