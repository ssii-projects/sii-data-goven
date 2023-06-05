using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Task;
using Agro.Library.Common;
using Agro.Library.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;



/*
yxm created at 2019/6/19 10:11:16
*/
namespace Agro.Module.DataExchange.XmlTask
{
	/// <summary>
	/// 导出xml数据（农村土地承包经营权信息应用平台接入技术规范）
	/// 参考：../../doc/农村土地承包经营权数据交换规范.doc
	/// </summary>
	class ExportXmlTask : GroupTask
	{
		class MyTask : Task
		{
			readonly ExportXmlPanel.Item it;

			private readonly ExportXmlTask _p;
			private readonly int srid;
			public MyTask(ExportXmlTask p, ExportXmlPanel.Item i,int srid)
			{
				Name = "导出xml数据";
				Description = $"导出 DJYW{i.Ywh}.xml";

				it = i;
				_p = p;
				this.srid = srid;
			}
			protected override void DoGo(ICancelTracker cancel)
			{
				var prm = _p.PropertyPage as ExportXmlPanel;

				var data = new XmlData(this, MyGlobal.Workspace, new Head()
				{
					SJLX=it.SJLX,FSF=prm.Jrm,SQID=it.ID,SZDY=it.SZDY
				});
				var bzd = data.BusinessData;
				if (bzd.XZQDM == null)
				{
					bzd.XZQDM = it.Ywh.Substring(0, 6);
					var zone = ZoneUtil.QueryZone(u=>u.BM==bzd.XZQDM);// $"BM='{bzd.XZQDM}'");
					if (zone != null)
					{
						bzd.XZDYMC = zone.Name;
					}
				}
				bzd.YWBLSJ = it.Blsj;
				data.Query(it);

				var path = $"{prm.SavePath}{bzd.XZQDM}/{it.Ywh}/BW";
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				var xmlFile = $"{path}/DJYW{it.Ywh}.xml";
				if (File.Exists(xmlFile))
				{
					File.Delete(xmlFile);
				}

				var xml = CreateXmlFile(xmlFile);
				data.WriteXml(xml, srid);
				xml.Save(xmlFile);

				ReportProgress(100);

			}
		}
		public ExportXmlTask()
		{
			Name = "导出xml数据";
			Description = "导出符合农村土地承包经营权信息应用平台接入技术规范的xml数据";
			PropertyPage = new ExportXmlPanel(()=>
			{
				ClearTasks();
				var srid = MyGlobal.Workspace.GetSRID("DLXX_DK");
				var prm = PropertyPage as ExportXmlPanel;
				var lst = prm.SelectedItems;
				foreach (var i in lst)
				{
					AddTask(new MyTask(this, i, srid));
				}
			});
			//IsExpanded = true;
		}
		//protected override void DoGo(ICancelTracker cancel)
		//{
		//	var prm = PropertyPage as ExportXmlPanel;
		//	var db = MyGlobal.Workspace;
		//	var lst = prm.SelectedItems;

		//	var srid=db.GetSRID("DLXX_DK");

		//	var cnt = lst.Count;
		//	double nOldProgress = 0;
		//	int i = 0;
		//	foreach (var it in lst)
		//	{
		//		if (++i != cnt)
		//		{
		//			ProgressUtil.ReportProgress(base.ReportProgress, cnt, i, ref nOldProgress);
		//		}
		//		var data = new XmlData(this,db, it.SJLX, prm.Jrm);// BusinessData.Create(this,it.SJLX,prm.Jrm);
		//		var bzd = data.BusinessData;
		//		//var bzd = data.BusinessData;
		//		if (bzd.XZQDM == null)
		//		{
		//			bzd.XZQDM = it.Ywh.Substring(0, 6);
		//			var zone=ZoneUtil.QueryZone($"BM='{bzd.XZQDM}'");
		//			if (zone != null)
		//			{
		//				bzd.XZDYMC = zone.Name;
		//			}
		//		}
		//		bzd.YWBLSJ = it.Blsj;
		//		data.Query(it);

		//		var path = $"{prm.SavePath}{bzd.XZQDM}/{it.Ywh}/BW";
		//		if (!Directory.Exists(path))
		//		{
		//			Directory.CreateDirectory(path);
		//		}
		//		var xmlFile = $"{path}/DJYW{it.Ywh}.xml";
		//		if (File.Exists(xmlFile))
		//		{
		//			File.Delete(xmlFile);
		//		}

		//		var xml = CreateXmlFile(xmlFile);
		//		data.WriteXml(xml,srid);
		//		xml.Save(xmlFile);

		//		ProgressUtil.ReportProgress(base.ReportProgress, cnt, cnt, ref nOldProgress);
		//	}
		//}

		internal static XmlDocument CreateXmlFile(string fileName)
		{
			System.Diagnostics.Debug.Assert(!File.Exists(fileName));
			using (var fst = new FileStream(fileName, FileMode.Create))
			{
				using (var swt = new StreamWriter(fst, Encoding.GetEncoding("utf-8")))
				{
					swt.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
					swt.WriteLine("<SUBMIT>");
					swt.WriteLine("	<HEAD/>");
					swt.WriteLine("	<BUSINESS_DATA/>");
					swt.WriteLine("	<ORIGINAL_DATA/>");
					swt.WriteLine("	<CHANGE_DATA/>");
					swt.WriteLine(" </SUBMIT>");
				}
			}
			var xml = new XmlDocument();
			xml.Load(fileName);
			
			return xml;
		}
	}





}
