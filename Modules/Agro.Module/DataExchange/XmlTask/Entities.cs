using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.Module.DataExchange.XmlTask
{
	class DJ_YW_SLSQ
	{
		public readonly string ID=Guid.NewGuid().ToString();
		public string YWH;

		/// <summary>
		/// 权利类型 0：承包经营权
		/// </summary>
		public int QLLX=0;

		///// <summary>
		///// 
		///// </summary>
		//public readonly int? TXQLLX=null;

		public int DJLX
		{
			get
			{
				return XmlExchangeUtil.DJXLToDJLX(DJXL);
			}
		}
		public int DJXL;
		//public int DJSX;
		//public string YWMC;

		/// <summary>
		/// 受理人员
		/// </summary>
		public string SLRY="未知";
		/// <summary>
		/// 受理时间
		/// </summary>
		public DateTime SLSJ { get { return ZHXGSJ; } }
		//public string ZL;
		//public string TZRXM;
		//public string TZFS;
		//public string TZRDH;
		//public string TZRYDDH;
		//public string TZRDZYJ;
		//public bool SFWTAJ;
		//public DateTime? JSSJ;

		/// <summary>
		/// 登记原因
		/// 针对注销登记：表示注销原因
		/// </summary>
		public string DJYY;

		/// <summary>
		/// 案件状态：
		/// -1：临时，0：在办，1：已办，2：停办，3：不予登记，4：用户撤回
		/// </summary>
		public readonly int AJZT=1;
		//public int LCJDSL;
		//public int DQJDXH;
		//public string YWZT;
		/// <summary>
		/// 所在地域
		/// </summary>
		public string SZDY;
		public DateTime CJSJ { get; set; } = DateTime.Now;
		//public string CJYH;
		/// <summary>
		/// 最后修改时间（业务办理时间）
		/// </summary>
		public DateTime ZHXGSJ;
		//public string ZHXGYH;
		//public string BZ;
	}
	class DJ_CBJYQ_YDJB
	{
		public string ID = Guid.NewGuid().ToString();
		public string DJBID;
		public string YDJBID;
		public string SLSQID;
		public int DJLX
		{
			get
			{
				return XmlExchangeUtil.DJXLToDJLX(DJXL);
			}
		}
		public int DJXL;
		/// <summary>
		/// 变更类型（0：一般变更，1：分户，2：合户，3：转让，4：互换，5：更正）
		/// </summary>
		public int BGLX
		{
			get
			{
				if (DJXL == 13) return 5;
				if (DJXL == 5) return 0; 
				return DJXL;
			}
		}
		public string BGCS;

		internal string YCBHTBM;
		internal string YCBFBM;
	}
	class DJ_CBJYQ_LZHT
	{
		public string ID = Guid.NewGuid().ToString();
		public string DJBID;
		public string YDJBID;
		public string CBHTBM;
		public string LZHTBM;
		public string CBFBM;
		public string SRFBM;
		public int LZFS;
		public string LZQX;
		public DateTime LZQXKSRQ;
		public DateTime LZQXJSRQ;
		public double LZMJ;
		public double LZMJM;
		public int LZDKS;
		public string LZQTDYT;
		public string LZHTDYT;
		public string LZJGSM;
		public DateTime HTQDRQ;
		public string SZDY;
		public string CJYH;
		public DateTime CJSJ=DateTime.Now;
		public string ZHXGYH;
		public DateTime ZHXGSJ { get { return CJSJ; } }
	}
	class DJ_CBJYQ_LZDK
	{
		public string ID = Guid.NewGuid().ToString();
		public string DJBID;
		public string YDJBID;
		public string DJBBM;
		public string HTID;
		public string HTBM;
		public string DKID;
		public string DKBM;
		public double DKMJ;
		public double DKMJM;
	}
	class DJ_CBJYQ_QZBF
	{
		public readonly string ID = Guid.NewGuid().ToString();
		public string QZID;
		public string SQID;
		public string YWH;
		public string DJBID;
		public readonly QZBF qZBF;
		public DJ_CBJYQ_QZBF(QZBF q)
		{
			qZBF = q;
		}
	}
	class DJ_CBJYQ_QZHF
	{
		public readonly string ID = Guid.NewGuid().ToString();
		public string QZID;
		public string SQID;
		public string YWH;
		public string DJBID;
		public readonly QZHF qZHF;
		public DJ_CBJYQ_QZHF(QZHF q)
		{
			qZHF = q;
		}
	}

	class DJ_CBJYQ_DJB
	{
		public readonly Cbjyqzdjb djb;
		public DJ_CBJYQ_DJB(Cbjyqzdjb d)
		{
			djb = d;
		}
	}
	//class Entities
	//{
	//	public readonly DJ_YW_SLSQ DJ_YW_SLSQ = new DJ_YW_SLSQ();
	//	public readonly List<DJ_CBJYQ_YDJB> lstYDJB = new List<DJ_CBJYQ_YDJB>();
	//	public readonly List<DJ_CBJYQ_LZHT> lstLZHT = new List<DJ_CBJYQ_LZHT>();
	//	public readonly List<DJ_CBJYQ_LZDK> lstLZDK = new List<DJ_CBJYQ_LZDK>();
	//	public DJ_CBJYQ_QZBF QZBF ;
	//	public DJ_CBJYQ_QZHF QZHF;
	//}
}
