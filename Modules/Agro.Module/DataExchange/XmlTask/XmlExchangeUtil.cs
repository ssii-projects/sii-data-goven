using Agro.GIS;
using Agro.Library.Common;
using Agro.Library.Model;
using System;
using System.Xml;

namespace Agro.Module.DataExchange
{
	class XmlExchangeUtil
	{
		/// <summary>
		/// 根据登记小类获取业务接入码（农村土地承包经营权信息应用平台接入技术规范 表1接入业务详表）
		/// </summary>
		/// <param name="nDJXL"></param>
		/// <returns></returns>
		public static int DjxlToSJLX(int nDJXL)
		{
			switch (nDJXL)
			{
				case (int)EDjxl.SCDJ: return 101;//首次登记 / 初始登记
				case (int)EDjxl.ZRDJ: return 102;//转让登记
				case (int)EDjxl.YBBGDJ: return 103;//一般登记 / 其它变更登记
				case (int)EDjxl.HUHDJ: return 104;//互换登记
				case (int)EDjxl.FHDJ: return 105;//分户登记
				case (int)EDjxl.HEHDJ: return 106;//合户登记
				case (int)EDjxl.ZXDJ: return 107;//注销登记
				case (int)EDjxl.QZBF: return 201;//权证补发
				case (int)EDjxl.QZHF: return 202; //权证换发
			}
			System.Diagnostics.Debug.Assert(false);
			return -1;
		}
		public static int SJLXToDjxl(int nSJLX)
		{
			switch (nSJLX)
			{
				case 101:return (int)EDjxl.SCDJ;//首次登记 / 初始登记
				case 102: return (int)EDjxl.ZRDJ;//转让登记
				case 103: return (int)EDjxl.YBBGDJ;//一般登记 / 其它变更登记
				case 104: return (int)EDjxl.HUHDJ;//互换登记
				case 105: return (int)EDjxl.FHDJ;//分户登记
				case 106: return (int)EDjxl.HEHDJ;//合户登记
				case 107: return (int)EDjxl.ZXDJ;//注销登记
				case 201: return (int)EDjxl.QZBF;//权证补发
				case 202: return (int)EDjxl.QZHF; //权证换发
			}
			System.Diagnostics.Debug.Assert(false);
			return -1;
		}
		/// <summary>
		/// 根据登记小类获取登记类型
		/// </summary>
		/// <param name="DJXL"></param>
		/// <returns></returns>
		public static int DJXLToDJLX(int DJXL)
		{
			if (0 == DJXL) return 0;//初始登记
			if (DJXL > 0 && DJXL < 6) return 1;//变更登记
			if (DJXL == 6) return 2;//注销登记
			if (DJXL == 7 || DJXL == 8) return 3;//补证换证
			if (DJXL == 13) return 8;//更正登记
			return -1;
		}
		public static void RemoveXmlChildByName(XmlNode parent, string childName)
		{
			foreach (var n in parent.ChildNodes)
			{
				if (n is XmlElement xe && xe.Name == childName)
				{
					parent.RemoveChild(xe);
					break;
				}
			}
		}
		public static void RemoveXmlChildByNames(XmlNode parent, string[] childNames)
		{
			foreach (var name in childNames)
			{
				RemoveXmlChildByName(parent, name);
			}
		}

		public static string GetXmlInnerText(XmlNode xn)
		{
			if (xn is XmlElement xe)
			{
				return xe.InnerText;
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="qxq">开始时间</param>
		/// <param name="qxz">结束时间</param>
		/// <returns></returns>
		public static string GetQX(DateTime qxq, DateTime qxz)
		{
			var qx = qxz.Subtract(qxq);
			var totalDays = qx.TotalDays;
			var totalYears = (int)(totalDays / 365);
			if (totalYears >= 1)
			{
				return $"{(int)totalYears}年";
			}
			else
			{
				return $"{(int)qx.TotalDays}天";
			}
		}
		///// <summary>
		///// 根据承包经营权证编码查询有效的登记簿ID
		///// </summary>
		///// <param name="CBJYQZBM"></param>
		///// <returns></returns>
		//public static string QueryValidDjbIDByCbjyqzbm(IFeatureWorkspace db, string CBJYQZBM)
		//{
		//	return 
		//}

		public static string Fbfbm2SZDY(string FBFBM)
		{
			if (FBFBM.Length == 14 && FBFBM.EndsWith("00"))
			{
				return FBFBM.Substring(0, 12);
			}
			return FBFBM;
		}
	}

	class DkItem
	{
		public string DKID;
		public string DKMC;
		public string YDKBM;
		public string SYQXZ;
		public string DKLB;
		public string TDLYLX;
		public string DLDJ;
		public string TDYT;
		public string SFJBNT;
		public double SCMJ;
		public double SCMJM;
		public double ELHTMJ;
		public double QQMJ;
		public double JDDMJ;
	}
}
