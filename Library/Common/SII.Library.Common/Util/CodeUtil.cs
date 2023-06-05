using Agro.LibCore;
using Agro.Library.Model;
using System;
using System.Collections.Generic;


namespace Agro.Library.Common
{

	public class CodeUtil
	{
		private static readonly Dictionary<string, List<CodeItem>> dicCodeItems = new Dictionary<string, List<CodeItem>>();

		public static string Code2Name(string codeType, string code, IWorkspace db = null)
		{
			if (string.IsNullOrEmpty(code)) return null;
			if (!dicCodeItems.TryGetValue(codeType, out List<CodeItem> lst))
			{
				lst = new List<CodeItem>();
				QueryCodeItems(codeType, (bm, mc) =>
				  {
					  lst.Add(new CodeItem()
					  {
						  Bm = bm,
						  Mc = mc
					  });
				  },db);
				dicCodeItems[codeType] = lst;
			}
			var ci = lst.Find(it => it.Bm == code);
			return ci?.Mc;
		}

		/// <summary>
		/// 根据土地用途代码获取土地用途名称
		/// </summary>
		/// <param name="sTdytCode"></param>
		/// <returns></returns>
		public static string Tdyt(object sTdytCode)
		{
			string s = "";
			switch (sTdytCode?.ToString())
			{
				case eTdytCode.Zzy: s = "种植业"; break;
				case eTdytCode.Ly: s = "林业"; break;
				case eTdytCode.Xmy: s = "畜牧业"; break;
				case eTdytCode.Ye: s = "渔业"; break;
				case eTdytCode.Fnyt: s = "非农用途"; break;
			}
			return s;
		}

		/// <summary>
		/// 根据基本农田代码获取基本农田名称
		/// </summary>
		/// <param name="sJbntCode"></param>
		/// <returns></returns>
		public static string Jbnt(object sJbntCode)
		{
			switch (sJbntCode?.ToString())
			{
				case eSfjbntCode.Shi:
					return "基本农田";
			}
			return "非基本农田";
		}

		/// <summary>
		/// 根据地力等级代码获取地力等级名称
		/// </summary>
		/// <param name="sDldjCode"></param>
		/// <returns></returns>
		public static string Dldj(object sDldjCode)
		{
			var n = SafeConvertAux.ToInt32(sDldjCode);
			switch (n)
			{
				case 1: return "一等地";
				case 2: return "二等地";
				case 3: return "三等地";
				case 4: return "四等地";
				case 5: return "五等地";
				case 6: return "六等地";
				case 7: return "七等地";
				case 8: return "八等地";
				case 9: return "九等地";
				case 10: return "十等地";
			}
			return sDldjCode?.ToString();
		}
		/// <summary>
		/// 家庭关系代码->家庭关系名称
		/// </summary>
		/// <param name="sJtgxCode"></param>
		/// <returns></returns>
		public static string Jtgx(object sJtgxCode)
		{
			return Code2Name(CodeType.JTGX, sJtgxCode?.ToString()) ?? "";
			//var n = SafeConvertAux.ToInt32(sJtgxCode);
			//switch (n)
			//{
			//	case 1: return "本人";
			//	case 2: return "户主";
			//	case 10: return "配偶	";
			//	case 11: return "夫";
			//	case 12: return "妻";
			//	case 20: return "子";
			//	case 21: return "独生子	";
			//	case 22: return "长子	";
			//	case 23: return "次子	";
			//	case 24: return "三子	";
			//	case 25: return "四子	";
			//	case 26: return "五子	";
			//	case 27: return "养子或继子";
			//	case 28: return "女婿";
			//	case 29: return "其他儿子";
			//	case 30: return "女";
			//	case 31: return "独生女";
			//	case 32: return "长女";
			//	case 33: return "次女";
			//	case 34: return "三女";
			//	case 35: return "四女";
			//	case 36: return "五女";
			//	case 37: return "养女或继女";
			//	case 38: return "儿媳";
			//	case 39: return "其他女儿";
			//	case 40: return "孙子、孙女或外孙子、外孙女";
			//	case 41: return "孙子";
			//	case 42: return "孙女";
			//	case 43: return "外孙子";
			//	case 44: return "外孙女";
			//	case 45: return "孙媳妇或外孙媳妇";
			//	case 46: return "女婿或外孙女婿";
			//	case 47: return "曾孙子或外曾孙子";
			//	case 48: return "曾孙女或外曾孙女";
			//	case 49: return "其他孙子、孙女或外孙子、外孙女";
			//	case 50: return "父母";
			//	case 51: return "父亲";
			//	case 52: return "母亲";
			//	case 53: return "公公";
			//	case 54: return "婆婆";
			//	case 55: return "岳父";
			//	case 56: return "岳母";
			//	case 57: return "继父或养父";
			//	case 58: return "继母或养母";
			//	case 59: return "其他父母关系";
			//	case 60: return "祖父母或外祖父母";
			//	case 61: return "祖父";
			//	case 62: return "祖母";
			//	case 63: return "外祖父";
			//	case 64: return "外祖母";
			//	case 65: return "配偶的祖父母或外祖父母";
			//	case 66: return "曾祖父";
			//	case 67: return "曾祖母";
			//	case 68: return "配偶的曾祖父母或外曾祖父母";
			//	case 69: return "其他祖父母或外祖父母关系";
			//	case 70: return "兄弟姐妹";
			//	case 71: return "兄";
			//	case 72: return "嫂";
			//	case 73: return "弟";
			//	case 74: return "弟媳";
			//	case 75: return "姐姐";
			//	case 76: return "姐夫";
			//	case 77: return "妹妹";
			//	case 78: return "妹夫";
			//	case 79: return "其他兄弟姐妹";
			//	case 80: return "其他";
			//	case 81: return "伯父";
			//	case 82: return "伯母";
			//	case 83: return "叔父";
			//	case 84: return "婶母";
			//	case 85: return "舅父";
			//	case 86: return "舅母";
			//	case 87: return "姨父";
			//	case 88: return "姨母";
			//	case 89: return "姑父";
			//	case 90: return "姑母";
			//	case 91: return "堂兄弟、堂姐妹";
			//	case 92: return "表兄弟、表姐妹";
			//	case 93: return "侄子";
			//	case 94: return "侄女";
			//	case 95: return "外甥";
			//	case 96: return "外甥女";
			//	case 97: return "其他亲属";
			//	case 99: return "非亲属";
			//}
			//return "";
		}
		/// <summary>
		/// 是否共有人代码->是否共有人名称
		/// </summary>
		/// <param name="sSfgyrCode"></param>
		/// <returns></returns>
		public static string Sfgyr(object sSfgyrCode)
		{
			var n = SafeConvertAux.ToInt32(sSfgyrCode);
			return n == 1?"是":"否";
		}

		/// <summary>
		/// 查询通用代码列表
		/// </summary>
		/// <param name="db"></param>
		/// <param name="codeType">代码类型：如地块类别</param>
		/// <param name=""></param>
		/// <param name="callback"></param>
		public static void QueryCodeItems(string codeType, Action<string, string> callback, IWorkspace db=null)
		{
			switch (codeType)
			{
				case CodeType.TDLYLX: QueryTdlylxItems(callback, db);return;
				case CodeType.SFJBNT:
					callback("1", "基本农田");
					callback("2", "非基本农田");
					return;
				case CodeType.SCBZ:
					callback("1", "已上传");
					callback("0", "未上传");
					return;
				case CodeType.DJZT:
					callback("0", "未登记");
					callback("1", "登记中");
					callback("2", "已登记");
					return;
			}
			if (db == null)
			{
				db = MyGlobal.Workspace;
			}
			var lst = new List<Tuple<string, string>>();
			var sql = "select BM,MC from xtpz_sjzd where SJID in(select id from xtpz_sjzd where mc='"+codeType+"' and id is not null) and ID is not null and MC is not null and BM is not null order by BM";
			db.QueryCallback(sql, r =>
			{
				var bm = r.GetString(0);
				var mc = r.GetString(1);
				callback(bm, mc);
				return true;
			});
		}
		public static List<CodeItem> QueryCodeItems(string codeType,IWorkspace db=null)
		{
			var items = new List<CodeItem>();
			QueryCodeItems(codeType, (bm, mc) =>
			{
				items.Add(new CodeItem(bm, mc));
			}, db);
			return items;
		}
		//public static void QueryCodeItems(string codeType, Action<string, string> callback)
		//{
		//	QueryCodeItems(MyGlobal.Workspace, codeType, callback);
		//}
		/// <summary>
		/// 查询土地利用类型列表BM,MC]
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		public static void QueryTdlylxItems(Action<string,string> callback, IWorkspace db=null)
		{
			if (db == null)
			{
				db = MyGlobal.Workspace;
			}
			var lst = new List<Tuple<string, string>>();
			var sql = "SELECT MC,BM FROM XTPZ_TDLYXZFL where SDL = '农用地' and SJBM is not null and BM is not null and MC is not null order by BM";
			db.QueryCallback(sql, r =>
			 {
				 var mc = r.GetString(0);
				 var bm = r.GetString(1);
				 callback(bm, mc);
				 return true;
			 });
		}


	}
}
