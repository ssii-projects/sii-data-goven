using Agro.GIS;
using Agro.LibCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/4/18 18:26:52
*/
namespace TestTool
{

	public class Processor
	{
		public void Process(IFeatureWorkspace db, string esUrl = "http://192.168.0.3:9200")
		{
			var dkbm = SafeConvertAux.ToStr(db.QueryOne("select top(1) dkbm from DLXX_DK where dkbm is not null"));
			var qxdm = SafeConvertAux.ToInt32(dkbm.Substring(0, 6));

			var sa = new Agriegov_statsBase[]{
			new Agriegov_stats_lands(esUrl),
			new Agriegov_stats_biz_regcert(esUrl),
			new Agriegov_stats_biz_regcert_freq(esUrl),
			new Agriegov_stats_contractees(esUrl),
			new Agriegov_stats_clcerts(esUrl),
			new Agriegov_stats_contracts(esUrl),
			new Agriegov_stats_contractors(esUrl),
			new Agriegov_stats_qygk(esUrl)
			};
			foreach (var bz in sa)
			{
				bz.Build(db, qxdm);
			}
		}

		//private void ExecSQL(IFeatureWorkspace db, string sql)
		//{
		//	Console.WriteLine(sql);
		//	db.ExecuteNonQuery(sql);
		//}

	}


	abstract class Agriegov_statsBase
	{
		protected readonly string IndexName;
		protected readonly string _esUrl;
		protected IFeatureWorkspace _db;

		/// <summary>
		/// 区县代码
		/// </summary>
		public int Qxdm;
		protected Agriegov_statsBase(string indexName, string esUrl)
		{
			IndexName = indexName;
			_esUrl = esUrl;
			//_db = db;
			//Qxdm = nQxdm;
		}
		protected static string CreateESMapingField(string fieldName, string type)
		{
			var str = $"\"{fieldName}\":{{\"type\":\"{type}\"}},";
			return str;
		}
		protected static string MapingFieldInteger(string fieldName)
		{
			return CreateESMapingField(fieldName, "integer");
		}
		protected static string MapingFieldDouble(string fieldName)
		{
			return CreateESMapingField(fieldName, "double");
		}
		protected static string MapingFieldDate(string fieldName)
		{
			var str = $"\"{fieldName}\":{{\"type\":\"date\",\"format\": \"yyyy - MM - dd\"}},";
			return str;
		}

		protected static string CreateESFieldNum(string fieldName, double val)
		{
			return $"\"{fieldName}\":{val},";
		}
		protected static string CreateESFieldText(string fieldName, object val)
		{
			return $"\"{fieldName}\":\"{val}\",";
		}
		//protected string CreateIndexBody()
		//{
		//	string str = "{";
		//	str += "\"settings\" : {\"number_of_shards\" : 1,\"number_of_replicas\" : 0	},";
		//	str += "\"mappings\" : {\"doc\" : {\"properties\" : {";
		//	str += BuildElasticMaping();
		//	str += "}}}}";
		//	str = JsonUtil.ConvertJsonString(str);
		//	Console.WriteLine(str);
		//	return str;
		//}
		protected virtual void PutData()
		{
			var esUrl = _esUrl;

			//HttpUtil.ApiDelete(esUrl + "/" + IndexName + "/doc/" + Qxdm);

			//var sql = CreateIndexBody();
			//HttpUtil.PutResponse(esUrl + "/" + IndexName, sql, out string statusCode);
			//await ClearDataAsync();
			var json = BuildElasticValues();
			HttpUtil.PutResponse(esUrl + "/" + IndexName + "/doc/" + Qxdm, json, out string statusCode);
		}
		//abstract protected string BuildElasticMaping();
		abstract protected string BuildElasticValues();
		abstract protected void FillData();
		public void Build(IFeatureWorkspace db, int nQxdm)
		{
			try
			{
				_db = db;
				Qxdm = nQxdm;
				FillData();
				var res = ClearData();
				Console.WriteLine(res);
				PutData();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		class _shards
		{
			public int successful { get; set; }
			public int failed { get; set; }
		}
		class ESDelRet
		{
			public _shards _shards { get; set; }
		}
		protected virtual string ClearData()
		{
			bool fOK = true;
			var url = $"{_esUrl}/{IndexName}/doc/{Qxdm}";
			var ret = HttpUtil.ApiDelete(url);
			if (ret != null)
			{
				var dr = JsonConvert.DeserializeObject<ESDelRet>(ret);
				if (dr != null)
				{
					fOK = dr._shards.successful == 1 && dr._shards.failed == 0;
				}
			}
			if (!fOK)
			{
				throw new Exception($"ClearData {IndexName}/doc/{Qxdm} failed,ret is {ret}");
			}
			return ret;
		}
	}
	class Agriegov_stats_lands : Agriegov_statsBase
	{
		public readonly Dictionary<string, CountMjItem> dicDklb = new Dictionary<string, CountMjItem>();
		public readonly Dictionary<string, CountMjItem> dicDldj = new Dictionary<string, CountMjItem>();
		public readonly Dictionary<string, CountMjItem> dicSyqxz = new Dictionary<string, CountMjItem>();
		public readonly Dictionary<string, CountMjItem> dicTdlylx = new Dictionary<string, CountMjItem>();
		public readonly Dictionary<string, CountMjItem> dicTdyt = new Dictionary<string, CountMjItem>();
		public readonly CountMjItem jbnt = new CountMjItem();
		public double Scmj;
		/// <summary>
		/// 总地块数
		/// </summary>
		public int Zdks;

		public Agriegov_stats_lands(string esUrl) : base("agriegov_stats_lands", esUrl)
		{
		}

		protected override void FillData()
		{
			QueryCountMj("DKLB", "SCMJM", dicDklb);
			QueryCountMj("Dldj", "SCMJM", dicDldj);
			QueryCountMj("Syqxz", "SCMJM", dicSyqxz);
			QueryCountMj("Tdlylx", "SCMJM", dicTdlylx);
			QueryCountMj("Tdyt", "SCMJM", dicTdyt);

			_db.QueryCallback("SELECT COUNT(*),SUM(SCMJM) FROM DLXX_DK WHERE SFJBNT=2", r =>
			{
				jbnt.Count = SafeConvertAux.ToInt32(r.GetValue(0));
				jbnt.Mj = SafeConvertAux.ToDouble(r.GetValue(1));
				return true;
			});
			_db.QueryCallback("SELECT COUNT(*),SUM(SCMJM) FROM DLXX_DK", r =>
			{
				Zdks = SafeConvertAux.ToInt32(r.GetValue(0));
				Scmj = SafeConvertAux.ToDouble(r.GetValue(1));
				return false;
			});


			//base.PutData();
		}
		private void QueryCountMj(string fieldName, string mjFieldName, Dictionary<string, CountMjItem> dic)
		{
			var sql = $"select {fieldName},count(*), sum({mjFieldName}) from DLXX_DK  group by {fieldName}";
			_db.QueryCallback(sql, r =>
			{
				var key = SafeConvertAux.ToStr(r.GetValue(0));
				var it = new CountMjItem()
				{
					Count = SafeConvertAux.ToInt32(r.GetValue(1)),
					Mj = SafeConvertAux.ToDouble(r.GetValue(2)),
				};
				dic[key] = it;
				return true;
			});
		}

		protected override string BuildElasticValues()
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += MapFieldValue("dklb", dicDklb);
			str += MapFieldValue("dldj", dicDldj);
			str += MapFieldValue("syqxz", dicSyqxz);
			str += MapFieldValue("tdlylx", dicTdlylx);
			str += MapFieldValue("tdyt", dicTdyt);
			str += CreateESFieldNum("jbnt", jbnt.Count);
			str += CreateESFieldNum("jbnt_mj", jbnt.Mj);
			str += CreateESFieldNum("scmj", Scmj);
			str += CreateESFieldNum("zdks", Zdks);
			str = str.TrimEnd(',');
			str += "}";
			return str;
		}
		//protected override string BuildElasticMaping()
		//{
		//	string str= CreateESMapingField("qxdm","keyword");
		//	str += BuildElasticMapingString("dklb", dicDklb);
		//	str += BuildElasticMapingString("dldj", dicDldj);
		//	str += BuildElasticMapingString("syqxz", dicSyqxz);
		//	str += BuildElasticMapingString("tdlylx", dicTdlylx);
		//	str += BuildElasticMapingString("tdyt", dicTdyt);
		//	str += CreateESMapingField("jbnt", "integer");
		//	str += CreateESMapingField("jbnt_mj", "double");
		//	str += CreateESMapingField("scmj", "double");
		//	str += CreateESMapingField("zdks", "integer");
		//	str = str.TrimEnd(',');
		//	return str;
		//}

		private static string BuildElasticMapingString(string preFix, Dictionary<string, CountMjItem> dic)
		{
			string str = "";
			foreach (var kv in dic)
			{
				str += CreateESMapingField($"{preFix}_{kv.Key}", "integer");
				str += CreateESMapingField($"{preFix}_{kv.Key}_mj", "double");
			}
			return str;
		}

		private static string MapFieldValue(string preFix, Dictionary<string, CountMjItem> dic)
		{
			var str = "";
			foreach (var kv in dic)
			{
				str += CreateESFieldNum($"{preFix}_{kv.Key}", kv.Value.Count);
				str += CreateESFieldNum($"{preFix}_{kv.Key}_mj", kv.Value.Mj);
			}
			return str;
		}

	}

	/// <summary>
	/// 登记小类
	/// </summary>
	public class DJxl
	{
		/// <summary>
		/// 初始登记
		/// </summary>
		public const int csdj = 0;
		/// <summary>
		/// 分户登记
		/// </summary>
		public const int fhdj = 1;

		/// <summary>
		/// 合户登记
		/// </summary>
		public const int hhdj = 2;
		/// <summary>
		/// 转让登记
		/// </summary>
		public const int zrdj = 3;
		/// <summary>
		/// 互换登记
		/// </summary>
		public const int huhdj = 4;
		/// <summary>
		/// 变更登记
		/// </summary>
		public const int bgdj = 5;
		/// <summary>
		/// 注销登记
		/// </summary>
		public const int zxdj = 6;
		/// <summary>
		/// 补证登记
		/// </summary>
		public const int bzdj = 7;
		/// <summary>
		/// 换证登记
		/// </summary>
		public const int hzdj = 8;
		/// <summary>
		/// 更正登记
		/// </summary>
		public const int gzdj = 13;
	}

	/// <summary>
	/// 各类登记数量
	/// </summary>
	public class DjxlItem
	{
		/// <summary>
		/// 变更登记
		/// </summary>
		public int bgdj;
		/// <summary>
		/// 补证登记
		/// </summary>
		public int bzdj;
		/// <summary>
		/// 初始登记
		/// </summary>
		public int csdj;
		/// <summary>
		/// 分户登记
		/// </summary>
		public int fhdj;
		/// <summary>
		/// 更正登记
		/// </summary>
		public int gzdj;
		/// <summary>
		/// 合户登记
		/// </summary>
		public int hhdj;
		/// <summary>
		/// 互换登记
		/// </summary>
		public int huhdj;
		/// <summary>
		/// 换证登记
		/// </summary>
		public int hzdj;
		/// <summary>
		/// 转让登记
		/// </summary>
		public int zrdj;
		/// <summary>
		/// 注销登记
		/// </summary>
		public int zxdj;

		/// <summary>
		/// 业务总数
		/// </summary>
		public int Ywzs
		{
			get
			{
				return bgdj + bzdj + csdj + fhdj + gzdj + hhdj + huhdj + hzdj + zrdj + zxdj;
			}
		}

		public void Clear()
		{
			csdj = 0;
			fhdj = 0;
			hhdj = 0;
			zrdj = 0;
			huhdj = 0;
			bgdj = 0;
			zxdj = 0;
			bzdj = 0;
			hzdj = 0;
			gzdj = 0;
		}
	}

	/*
	 {
  "mapping": {
    "doc": {
      "properties": {
        "dbyws_bgdj": {
          "type": "integer"
        },
        "dbyws_bzdj": {
          "type": "integer"
        },
        "dbyws_csdj": {
          "type": "integer"
        },
        "dbyws_fhdj": {
          "type": "integer"
        },
        "dbyws_gzdj": {
          "type": "integer"
        },
        "dbyws_hhdj": {
          "type": "integer"
        },
        "dbyws_huhdj": {
          "type": "integer"
        },
        "dbyws_hzdj": {
          "type": "integer"
        },
        "dbyws_zrdj": {
          "type": "integer"
        },
        "dbyws_zxdj": {
          "type": "integer"
        },
        "qxdm": {
          "type": "keyword"
        },
        "ybyws": {
          "type": "integer"
        },
        "ybyws_bgdj": {
          "type": "integer"
        },
        "ybyws_bzdj": {
          "type": "integer"
        },
        "ybyws_csdj": {
          "type": "integer"
        },
        "ybyws_fhdj": {
          "type": "integer"
        },
        "ybyws_gzdj": {
          "type": "integer"
        },
        "ybyws_hhdj": {
          "type": "integer"
        },
        "ybyws_huhdj": {
          "type": "integer"
        },
        "ybyws_hzdj": {
          "type": "integer"
        },
        "ybyws_zrdj": {
          "type": "integer"
        },
        "ybyws_zxdj": {
          "type": "integer"
        },
        "zdbyws": {
          "type": "integer"
        }
      }
    }
  }
}
	 * */
	class Agriegov_stats_biz_regcert : Agriegov_statsBase
	{
		/// <summary>
		/// 待办业务数量
		/// </summary>
		private readonly DjxlItem DbywItem = new DjxlItem();
		/// <summary>
		/// 已办业务数量
		/// </summary>
		private readonly DjxlItem YbywItem = new DjxlItem();

		public Agriegov_stats_biz_regcert(string esUrl) : base("agriegov_stats_biz_regcert", esUrl)
		{
		}
		protected override void FillData()
		{
			DbywItem.Clear();
			YbywItem.Clear();
			var sql = "select ajzt,djxl,count(*) from DJ_YW_SLSQ where ajzt>=0 and djxl<14 and QLLX=0 and TXQLLX is null  group by ajzt, DJXL order by ajzt";
			_db.QueryCallback(sql, r =>
			{
				var ajzt = SafeConvertAux.ToInt32(r.GetValue(0));
				var djxl = SafeConvertAux.ToInt32(r.GetValue(1));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(2));
				var it = ajzt == 0 ? DbywItem : YbywItem;
				switch (djxl)
				{
					case DJxl.csdj: it.csdj += cnt; break;
					case DJxl.fhdj: it.fhdj += cnt; break;
					case DJxl.hhdj: it.hhdj += cnt; break;
					case DJxl.zrdj: it.zrdj += cnt; break;
					case DJxl.huhdj: it.huhdj += cnt; break;
					case DJxl.bgdj: it.bgdj += cnt; break;
					case DJxl.zxdj: it.zxdj += cnt; break;
					case DJxl.bzdj: it.bzdj += cnt; break;
					case DJxl.hzdj: it.hzdj += cnt; break;
					case DJxl.gzdj: it.gzdj += cnt; break;
				}
				return true;
			});
		}
		//protected override string BuildElasticMaping()
		//{
		//	string str = CreateESMapingField("qxdm", "keyword");
		//	str += MapingFieldInteger("ybyws");
		//	str += MapingFieldInteger("zdbyws");
		//	str += MapingFields("dbyws");
		//	str += MapingFields("ybyws");
		//	str = str.TrimEnd(',');
		//	return str;
		//}
		protected override string BuildElasticValues()
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += MapFieldValues(DbywItem);
			str += MapFieldValues(YbywItem);
			str += CreateESFieldNum("ybyws", YbywItem.Ywzs);
			str += CreateESFieldNum("zdbyws", DbywItem.Ywzs);
			str = str.TrimEnd(',');
			str += "}";
			return str;
		}

		//private void FillData(Item it)
		//{
		//	var ajlx = it == DbywItem ? "=0" : ">0";
		//	var saDjxl = new int[] {
		//		DJxl.csdj,DJxl.fhdj,DJxl.hhdj,DJxl.zrdj,DJxl.huhdj,DJxl.bgdj,DJxl.zxdj,DJxl.bzdj,DJxl.hzdj,DJxl.gzdj
		//	};
		//	int i = 0;
		//	foreach (var nDjxl in saDjxl)
		//	{
		//		string sql = Count(nDjxl, ajlx);
		//		var cnt=SafeConvertAux.ToInt32(_db.QueryOne(sql));
		//		switch (i)
		//		{
		//			case DJxl.csdj: it.csdj = cnt; break;
		//			case DJxl.fhdj: it.fhdj = cnt; break;
		//			case DJxl.hhdj: it.hhdj = cnt; break;
		//			case DJxl.zrdj: it.zrdj = cnt; break;
		//			case DJxl.huhdj: it.huhdj = cnt; break;
		//			case DJxl.bgdj: it.bgdj = cnt; break;
		//			case DJxl.zxdj: it.zxdj = cnt; break;
		//			case DJxl.bzdj: it.bzdj = cnt; break;
		//			case DJxl.hzdj: it.hzdj = cnt; break;
		//			default:
		//				it.gzdj = cnt; break;
		//		}
		//		++i;
		//	}
		//}
		private string MapingFields(string preFix)
		{
			string str = "";
			str += MapingFieldInteger($"{preFix}_bgdj");
			str += MapingFieldInteger($"{preFix}_bzdj");
			str += MapingFieldInteger($"{preFix}_csdj");
			str += MapingFieldInteger($"{preFix}_fhdj");
			str += MapingFieldInteger($"{preFix}_gzdj");
			str += MapingFieldInteger($"{preFix}_hhdj");
			str += MapingFieldInteger($"{preFix}_huhdj");
			str += MapingFieldInteger($"{preFix}_hzdj");
			str += MapingFieldInteger($"{preFix}_zrdj");
			str += MapingFieldInteger($"{preFix}_zxdj");
			return str;
		}


		private string MapFieldValues(DjxlItem it)
		{
			var preFix = it == DbywItem ? "dbyws" : "ybyws";
			string str = "";
			str += CreateESFieldNum($"{preFix}_bgdj", it.bgdj);
			str += CreateESFieldNum($"{preFix}_bzdj", it.bzdj);
			str += CreateESFieldNum($"{preFix}_csdj", it.csdj);
			str += CreateESFieldNum($"{preFix}_fhdj", it.fhdj);
			str += CreateESFieldNum($"{preFix}_gzdj", it.gzdj);
			str += CreateESFieldNum($"{preFix}_hhdj", it.hhdj);
			str += CreateESFieldNum($"{preFix}_huhdj", it.huhdj);
			str += CreateESFieldNum($"{preFix}_hzdj", it.hzdj);
			str += CreateESFieldNum($"{preFix}_zrdj", it.zrdj);
			str += CreateESFieldNum($"{preFix}_zxdj", it.zxdj);
			return str;
		}
	}
	/*
	 {
  "mapping": {
    "doc": {
      "properties": {
        "bgdj": {
          "type": "integer"
        },
        "bzdj": {
          "type": "integer"
        },
        "csdj": {
          "type": "integer"
        },
        "fhdj": {
          "type": "integer"
        },
        "gzdj": {
          "type": "integer"
        },
        "hhdj": {
          "type": "integer"
        },
        "huhdj": {
          "type": "integer"
        },
        "hzdj": {
          "type": "integer"
        },
        "qxdm": {
          "type": "keyword"
        },
        "sjcbf": {
          "type": "integer"
        },
        "sjdkmj": {
          "type": "integer"
        },
        "sjdks": {
          "type": "integer"
        },
        "sjjtcy": {
          "type": "integer"
        },
        "tjrq": {
          "type": "date",
          "format": "yyyy-MM-dd"
        },
        "zrdj": {
          "type": "integer"
        },
        "zxdj": {
          "type": "integer"
        }
      }
    }
  }
}
	 */
	class Agriegov_stats_biz_regcert_freq : Agriegov_statsBase
	{
		class Item : DjxlItem
		{
			/// <summary>
			/// 涉及承包方
			/// </summary>
			public int sjcbf;
			/// <summary>
			/// 涉及地块面积
			/// </summary>
			public double sjdkmj;
			/// <summary>
			/// 涉及地块数
			/// </summary>
			public int sjdks;
			/// <summary>
			/// 涉及家庭成员
			/// </summary>
			public int sjjtcy;
			///// <summary>
			///// 统计日期
			///// </summary>
			//public DateTime tjrq;
		}

		/// <summary>
		/// [统计日期,Item]
		/// </summary>
		private readonly Dictionary<string, Item> dicItem = new Dictionary<string, Item>();

		public Agriegov_stats_biz_regcert_freq(string esUrl) : base("agriegov_stats_biz_regcert_freq", esUrl)
		{
		}

		//protected override string BuildElasticMaping()
		//{
		//	string str = CreateESMapingField("qxdm", "keyword");
		//	str += MapingFieldInteger("bgdj");
		//	str += MapingFieldInteger("bzdj");
		//	str += MapingFieldInteger("csdj");
		//	str += MapingFieldInteger("fhdj");
		//	str += MapingFieldInteger("gzdj");
		//	str += MapingFieldInteger("hhdj");
		//	str += MapingFieldInteger("huhdj");
		//	str += MapingFieldInteger("hzdj");
		//	str += MapingFieldInteger("zrdj");
		//	str += MapingFieldInteger("zxdj");
		//	str += MapingFieldInteger("sjcbf");
		//	str += MapingFieldInteger("sjdks");
		//	str += MapingFieldInteger("sjjtcy");
		//	str += MapingFieldDouble("sjdkmj");
		//	str += MapingFieldDate("tjrq");
		//	str = str.TrimEnd(',');
		//	return str;
		//}
		protected override string BuildElasticValues()
		{
			throw new NotImplementedException();
		}

		private static string GetRq(object o)
		{
			if (o is DateTime d)
			{
				var str = d.ToString("d");
				str = str.Replace('/', '-');
				return str;
			}
			return o.ToString();
		}
		/// <summary>
		/// DJ_YW_SLSQ:登记_业务_受理申请
		/// </summary>
		protected override void FillData()
		{
			var wh = "ajzt>=0 and djxl<14 and QLLX=0 and TXQLLX is null";
			var dt = "convert(date,CJSJ)";
			var sql = $"select djxl,{dt},count(*)  from DJ_YW_SLSQ where {wh}  group by djxl,{dt}";
			_db.QueryCallback(sql, r =>
			{
				var djxl = SafeConvertAux.ToInt32(r.GetValue(0));
				var rq = GetRq(r.GetValue(1));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(2));
				var it = GetItem(rq);
				switch (djxl)
				{
					case DJxl.csdj: it.csdj += cnt; break;
					case DJxl.fhdj: it.fhdj += cnt; break;
					case DJxl.hhdj: it.hhdj += cnt; break;
					case DJxl.zrdj: it.zrdj += cnt; break;
					case DJxl.huhdj: it.huhdj += cnt; break;
					case DJxl.bgdj: it.bgdj += cnt; break;
					case DJxl.zxdj: it.zxdj += cnt; break;
					case DJxl.bzdj: it.bzdj += cnt; break;
					case DJxl.hzdj: it.hzdj += cnt; break;
					case DJxl.gzdj: it.gzdj += cnt; break;
				}
				return true;
			});

			sql = $"select {dt},count(distinct t.CBFBM) from ";
			sql += "(select a.*, b.CBFBM from DJ_YW_SLSQ a left join DJ_CBJYQ_DJB b on a.YWH = b.YWH ) t";
			sql += $" where {wh} group by {dt}";
			_db.QueryCallback(sql, r =>
			{
				var rq = GetRq(r.GetValue(0));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(1));
				var it = GetItem(rq);
				it.sjcbf += cnt;
				return true;
			});

			sql = $"select {dt},count(distinct t.jtcyID)  from ";
			sql += "(select a.*, c.ID jtcyID from DJ_YW_SLSQ a left join DJ_CBJYQ_DJB b on a.YWH = b.YWH";
			sql += " left join DJ_CBJYQ_CBF c on b.id = c.DJBID left  join DJ_CBJYQ_CBF_JTCY d on d.CBFID = c.ID) t ";
			sql += $" where {wh} group by {dt}";
			_db.QueryCallback(sql, r =>
			{
				var rq = GetRq(r.GetValue(0));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(1));
				var it = GetItem(rq);
				it.sjjtcy += cnt;
				return true;
			});

			sql = $"select {dt} ,count(distinct t.dkid),sum(t.scmjm) from ";
			sql += "(select a.*, b.CBFBM, c.DKID, c.SCMJM from DJ_YW_SLSQ a left join DJ_CBJYQ_DJB b on a.YWH = b.YWH left join DJ_CBJYQ_DKXX c on b.id = c.DJBID) t";
			sql += $" where {wh} group by {dt}";
			_db.QueryCallback(sql, r =>
			{
				var rq = GetRq(r.GetValue(0));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(1));
				var mj = SafeConvertAux.ToDouble(r.GetValue(2));
				var it = GetItem(rq);
				it.sjdks += cnt;
				it.sjdkmj += mj;
				return true;
			});

		}


		protected override void PutData()
		{
			var esUrl = _esUrl;

			//ClearData();

			int k = 0;
			var lstJson = new List<string>();
			foreach (var kv in dicItem)
			{
				var json = BuildElasticValues(kv.Key, kv.Value);
				lstJson.Add(json);
				if (lstJson.Count > 500)
				{
					k = Bulk(lstJson, k);
					lstJson.Clear();
				}
			}
			if (lstJson.Count > 0)
			{
				Bulk(lstJson, k);
			}
		}

		//class JsonRet
		//{
		//	public int total { get; set; }
		//	public int deleted { get; set; }
		//}
		protected override string ClearData()
		{
			var json = "{\"query\": {\"match\": {\"qxdm\": \"" + Qxdm + "\"}}}";
			var url = $"{_esUrl}/{IndexName}/_delete_by_query";
			var str = HttpUtil.HttpPost(url, json);
			bool fOK = false;
			if (str != null)
			{
				var jr = JsonConvert.DeserializeObject<JsonRet>(str);
				fOK = jr != null && jr.total == jr.deleted;
				Console.WriteLine($"ok:total={jr.total},deleted={jr.deleted}");
			}
			if (!fOK)
			{
				throw new Exception($"ClearData {IndexName}/{Qxdm} failed,ret is {str}");
			}
			return str;
		}

		protected int Bulk(List<string> lst, int k)
		{
			var tableName = IndexName;
			var url = _esUrl + "/_bulk";
			string str = "";
			foreach (var json in lst)
			{
				string jsonList = CreateBulkIndex(tableName, ++k) + "\n" + json + "\n";
				str += jsonList;
			}
			var res = HttpUtil.PutResponse(url, str, out string statusCode);//, "application/x-ndjson");
			lst.Clear();
			return k;
		}
		protected string CreateBulkIndex(string tableName, int k)
		{
			string str = "{ \"create\" : { \"_index\" : \"" + tableName + "\", \"_type\" : \"doc\", \"_id\" : \"" + Qxdm + "_" + k + "\" } }";
			//string str = "{ \"create\" : { \"_index\" : \"" + tableName + "\", \"_type\" : \"doc\"} }";
			return str;
		}


		private string BuildElasticValues(string tjRq, Item it)
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += CreateESFieldText("tjrq", tjRq);

			str += CreateESFieldNum("bgdj", it.bgdj);
			str += CreateESFieldNum("bzdj", it.bzdj);
			str += CreateESFieldNum("csdj", it.csdj);
			str += CreateESFieldNum("fhdj", it.fhdj);
			str += CreateESFieldNum("gzdj", it.gzdj);
			str += CreateESFieldNum("hhdj", it.hhdj);
			str += CreateESFieldNum("huhdj", it.huhdj);
			str += CreateESFieldNum("hzdj", it.hzdj);
			str += CreateESFieldNum("zrdj", it.zrdj);
			str += CreateESFieldNum("zxdj", it.zxdj);

			str += CreateESFieldNum("sjcbf", it.sjcbf);
			str += CreateESFieldNum("sjdks", it.sjdks);
			str += CreateESFieldNum("sjdkmj", it.sjdkmj);
			str += CreateESFieldNum("sjjtcy", it.sjjtcy);
			str = str.TrimEnd(',');
			str += "}";
			return str;
		}

		private Item GetItem(string rq)
		{
			if (!dicItem.TryGetValue(rq, out Item it))
			{
				it = new Item();
				dicItem[rq] = it;
			}
			return it;
		}

	}

	/*
	  "mapping": {
		"doc": {
		  "properties": {
			"qxdm": {
			  "type": "keyword"
			},
			"zfbmj": {
			  "type": "double"
			},
			"zfbs": {
			  "type": "integer"
			}
		  }
		}
	  }
	}
	*/
	class Agriegov_stats_contractees : Agriegov_statsBase
	{
		/// <summary>
		/// 总发包方数量
		/// </summary>
		private int zfbs;
		/// <summary>
		/// 总发包方面积
		/// </summary>
		private double zfbmj;
		public Agriegov_stats_contractees(string esUrl) : base("agriegov_stats_contractees", esUrl)
		{
		}

		//protected override string BuildElasticMaping()
		//{
		//	throw new NotImplementedException();
		//}

		protected override string BuildElasticValues()
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += CreateESFieldNum("zfbs", zfbs);
			str += CreateESFieldNum("zfbmj", zfbmj);
			str = str.TrimEnd(',');
			str += "}";
			return str;
		}

		protected override void FillData()
		{
			var sql = "select count(1) from QSSJ_FBF";
			zfbs = SafeConvertAux.ToInt32(_db.QueryOne(sql));
			zfbmj = SafeConvertAux.ToDouble(_db.QueryOne("select sum(scmjm) from DLXX_DK where zt=1"));
		}
	}

	class Agriegov_stats_clcerts : Agriegov_statsBase
	{
		class Data
		{
			public readonly Dictionary<string, CountMjItem> dicCbfs = new Dictionary<string, CountMjItem>();
			/// <summary>
			/// 办证数
			/// </summary>
			public int bzs
			{
				get
				{
					var cnt = dicCbfs.Sum(kv =>
					{
						return kv.Value.Count;
					});
					return cnt;
				}
			}

			/// <summary>
			/// 办证面积
			/// </summary>
			public double bzmj
			{
				get
				{
					var cnt = dicCbfs.Sum(kv =>
					{
						return kv.Value.Mj;
					});
					return cnt;
				}
			}
		}
		private readonly Data _data = new Data();
		public Agriegov_stats_clcerts(string esUrl) : base("agriegov_stats_clcerts", esUrl)
		{
		}

		protected override string BuildElasticValues()
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += CreateESFieldNum("bzs", _data.bzs);
			str += CreateESFieldNum("bzmj", _data.bzmj);
			foreach (var kv in _data.dicCbfs)
			{
				str += CreateESFieldNum($"cbfs_{kv.Key}", kv.Value.Count);
				str += CreateESFieldNum($"cbfs_{kv.Key}_mj", kv.Value.Mj);
			}

			str = str.TrimEnd(',');
			str += "}";
			return str;
		}

		protected override void FillData()
		{
			var dic = _data.dicCbfs;
			dic.Clear();
			var sql = @"select t.CBFS, sum(scmjm) mj from
(select a.SCMJM, b.id, b.CBFS, b.QSZT, b.SFYZX from DJ_CBJYQ_DKXX a
	left
											   join DJ_CBJYQ_DJB b on a.DJBID = b.ID
where QSZT = 1 and SFYZX = 0 and b.id in (select distinct djbid from dj_cbjyq_qz where SFYZX = 0)
) t
group by CBFS";
			_db.QueryCallback(sql, r =>
			{
				var cbfs = SafeConvertAux.ToStr(r.GetValue(0));
				var cnt = SafeConvertAux.ToDouble(r.GetValue(1));
				if (!string.IsNullOrEmpty(cbfs))
				{
					if (dic.TryGetValue(cbfs, out CountMjItem ti))
					{
						ti.Mj = cnt;
					}
					else
					{
						dic[cbfs] = new CountMjItem()
						{
							Mj = cnt
						};
					}
				}

				return true;
			});

			sql = @"
  select R.CBFS, COUNT(DISTINCT Z.ID) QZS from DJ_CBJYQ_QZ Z
JOIN DJ_CBJYQ_DJB R ON R.ID=Z.DJBID
WHERE R.QSZT=1 AND Z.SFYZX=0
GROUP BY CBFS";
			_db.QueryCallback(sql, r =>
			{
				var cbfs = SafeConvertAux.ToStr(r.GetValue(0));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(1));
				if (!string.IsNullOrEmpty(cbfs))
				{
					if (dic.TryGetValue(cbfs, out CountMjItem ti))
					{
						ti.Count = cnt;
					}
					else
					{
						dic[cbfs] = new CountMjItem()
						{
							Count = cnt
						};
					}
				}

				return true;
			});

		}
	}


	/*
	 * 
SELECT * FROM QSSJ_CBHT
WHERE DJZT=2
	 */
	class Agriegov_stats_contracts : Agriegov_statsBase
	{
		class Data
		{
			public readonly Dictionary<string, CountMjItem> dicCbfs = new Dictionary<string, CountMjItem>();
			/// <summary>
			/// 总确权数
			/// </summary>
			public int zqqs
			{
				get
				{
					var cnt = dicCbfs.Sum(kv =>
					{
						return kv.Value.Count;
					});
					return cnt;
				}
			}

			/// <summary>
			/// 总确权面积
			/// </summary>
			public double zqqmj
			{
				get
				{
					var cnt = dicCbfs.Sum(kv =>
					{
						return kv.Value.Mj;
					});
					return cnt;
				}
			}
		}
		private readonly Data _data = new Data();
		public Agriegov_stats_contracts(string esUrl) : base("agriegov_stats_contracts", esUrl)
		{
		}

		protected override string BuildElasticValues()
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += CreateESFieldNum("zqqs", _data.zqqs);
			str += CreateESFieldNum("zqqmj", _data.zqqmj);
			foreach (var kv in _data.dicCbfs)
			{
				str += CreateESFieldNum($"cbfs_{kv.Key}", kv.Value.Count);
				str += CreateESFieldNum($"cbfs_{kv.Key}_mj", kv.Value.Mj);
			}

			str = str.TrimEnd(',');
			str += "}";
			return str;
		}

		protected override void FillData()
		{
			_data.dicCbfs.Clear();
			var sql = "select CBFS,count(*) ,sum(HTZMJM) from QSSJ_CBHT where cbfs is not null and DJZT=2 group by CBFS";
			_db.QueryCallback(sql, r =>
			{
				var cbfs = r.GetString(0);
				_data.dicCbfs[cbfs] = new CountMjItem()
				{
					Count = SafeConvertAux.ToInt32(r.GetValue(1)),
					Mj = SafeConvertAux.ToDouble(r.GetValue(2)),
				};
				return true;
			});
		}
	}

	/*
	 
SELECT * FROM QSSJ_CBF [QSSJ_CBF_JTCY]
WHERE DJZT=2
	 * */
	class Agriegov_stats_contractors : Agriegov_statsBase
	{
		//class NlItem
		//{
		//	/// <summary>
		//	/// 性别
		//	/// </summary>
		//	public string Xb;
		//	/// <summary>
		//	/// 数量
		//	/// </summary>
		//	public int Sl;
		//}

		class Data
		{
			/// <summary>
			/// 总承包方数
			/// </summary>
			public int zcbfs
			{
				get
				{
					return dicCbflxSl.Sum(kv =>
					{
						return kv.Value;
					});
				}
			}

			/// <summary>
			/// 总成员数
			/// </summary>
			public int zcys
			{
				get
				{
					int n = dicNl.Sum(kv =>
					{
						return kv.Value.Sum(kv1 => { return kv1.Value; });
					});
					return n;
				}
			}

			/// <summary>
			/// 家庭成员共有人数量
			/// </summary>
			public int gyrs;
			/// <summary>
			/// [承包方类型(1,2,3),数量]
			/// </summary>
			public readonly Dictionary<string, int> dicCbflxSl = new Dictionary<string, int>();
			/// <summary>
			/// [年龄段,[性别,数量]]
			/// </summary>
			public readonly Dictionary<string, Dictionary<string, int>> dicNl = new Dictionary<string, Dictionary<string, int>>();
			public void Clear()
			{
				dicNl.Clear();
				dicCbflxSl.Clear();
			}
		}
		private readonly Data _data = new Data();
		public Agriegov_stats_contractors(string esUrl) : base("agriegov_stats_contractors", esUrl)
		{
		}

		protected override string BuildElasticValues()
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += CreateESFieldNum("zcbfs", _data.zcbfs);
			str += CreateESFieldNum("zcys", _data.zcys);
			str += CreateESFieldNum("gyrs", _data.gyrs);
			foreach (var kv in _data.dicCbflxSl)
			{
				str += CreateESFieldNum($"cbflx_{kv.Key}", kv.Value);
			}

			var dic1 = new Dictionary<string, int>
			{
				["1"] = 0,
				["2"] = 0
			};
			foreach (var kv in _data.dicNl)
			{
				str += $"\"cynl_{kv.Key}\":";
				str += "{\"properties\": [";
				foreach (var kv1 in kv.Value)
				{
					dic1[kv1.Key] = kv1.Value;
				}
				foreach (var kv1 in dic1)
				{
					var str1 = "{";
					str1 += CreateESFieldText("xb", kv1.Key);
					str1 += CreateESFieldNum("sl", kv1.Value);
					str1 = str1.TrimEnd(',');
					str1 += "},";
					str += str1;
				}
				str = str.TrimEnd(',');
				str += "]},";
			}

			str = str.TrimEnd(',');
			str += "}";
			return str;
		}

		protected override void FillData()
		{
			_data.Clear();

			_db.QueryCallback("select CBFLX,count(1) from QSSJ_CBF where CBFLX in(1,2,3) and DJZT=2 group by CBFLX", r =>
			{
				var nCbflx = SafeConvertAux.ToStr(r.GetValue(0));
				var cnt = SafeConvertAux.ToInt32(r.GetValue(1));
				_data.dicCbflxSl[nCbflx] = cnt;
				return true;
			});

			var wh1 = "cbfbm in (select distinct cbfbm from QSSJ_CBF where DJZT=2)";
			var sql = $"select 'un',CYXB,count(1) from QSSJ_CBF_JTCY where CSRQ is null and {wh1} group by CYXB";
			sql += $" union select '10',CYXB,count(1) from QSSJ_CBF_JTCY where (year(getdate()) - year(CSRQ)) <= 10 and {wh1} group by CYXB";
			for (int n = 20; n <= 100; n += 10)
			{
				sql += SubSql(n);
			}
			sql += $" union select '100u',CYXB,count(1) from QSSJ_CBF_JTCY where (year(getdate())-year(CSRQ))>100  and {wh1} group by CYXB";
			_db.QueryCallback(sql, r =>
			{
				if (!r.IsDBNull(1))
				{
					var nl = r.GetString(0);
					var xb = SafeConvertAux.ToStr(r.GetValue(1));
					var cnt = SafeConvertAux.ToInt32(r.GetValue(2));
					var dic = _data.dicNl;
					if (!dic.TryGetValue(nl, out Dictionary<string, int> dic1))
					{
						dic1 = new Dictionary<string, int>();
						dic[nl] = dic1;
					}
					dic1[xb] = cnt;
				}
				return true;
			});

			sql = $"select count(1) from QSSJ_CBF_JTCY where SFGYR=1 and {wh1}";
			_data.gyrs = SafeConvertAux.ToInt32(_db.QueryOne(sql));
		}

		private static string SubSql(int n)
		{
			var sql = $" union select '{n}',CYXB,count(1) from QSSJ_CBF_JTCY where (year(getdate())-year(CSRQ))>{n - 10} and (year(getdate())-year(CSRQ))<={n} and cbfbm in (select distinct cbfbm from QSSJ_CBF where DJZT=2) group by CYXB";
			return sql;
		}
	}

	/// <summary>
	/// 企业概况
	/// yxm 2019-7-30
	/// </summary>
	class Agriegov_stats_qygk: Agriegov_statsBase
	{
		class Item
		{
			/// <summary>
			/// 总乡镇数量
			/// </summary>
			public int ZXZSL;
			/// <summary>
			/// 总户数
			/// </summary>
			public int ZHS;
			/// <summary>
			/// 总人数
			/// </summary>
			public int ZRS;
			/// <summary>
			/// 总地块面积
			/// </summary>
			public double ZDKMJ;
			/// <summary>
			/// 总地块数
			/// </summary>
			public int ZDKS;
			/// <summary>
			/// 登记簿数量（权证数量）
			/// </summary>
			public int DJBSL;
			/// <summary>
			/// 一致性数量
			/// </summary>
			public int YZXSL;
			/// <summary>
			/// 不一致性数量
			/// </summary>
			public int BYZXSL;
		}

		/// <summary>
		/// [企业名称,Item]
		/// </summary>
		private readonly Dictionary<string, Item> dicItem = new Dictionary<string, Item>();
		/// <summary>
		/// 全县的数据
		/// </summary>
		private Item _xianItem;
		public Agriegov_stats_qygk(string esUrl) : base("agriegov_stats_qygk", esUrl)
		{
		}


		protected override string ClearData()
		{
			var json = "{\"query\": {\"match\": {\"qxdm\": \"" + Qxdm + "\"}}}";
			var url = $"{_esUrl}/{IndexName}/_delete_by_query";
			var str = HttpUtil.HttpPost(url, json);
			bool fOK = false;
			if (str != null)
			{
				var jr = JsonConvert.DeserializeObject<JsonRet>(str);
				fOK = jr != null && jr.total == jr.deleted;
				Console.WriteLine($"ok:total={jr.total},deleted={jr.deleted}");
			}
			if (!fOK)
			{
				throw new Exception($"ClearData {IndexName}/{Qxdm} failed,ret is {str}");
			}
			return str;
		}
		protected override string BuildElasticValues()
		{
			throw new NotImplementedException();
		}

		protected override void PutData()
		{
			int k = 0;
			var lstJson = new List<string>();
			foreach (var kv in dicItem)
			{
				var json = BuildElasticValues(kv.Key, kv.Value);
				lstJson.Add(json);
				if (lstJson.Count > 500)
				{
					k = Bulk(lstJson, k);
					lstJson.Clear();
				}
			}
			if (lstJson.Count > 0)
			{
				Bulk(lstJson, k);
			}
		}
		private string BuildElasticValues(string qymc, Item it)
		{
			string str = "{";
			str += CreateESFieldText("qxdm", Qxdm);
			str += CreateESFieldText("qymc", qymc);

			str += CreateESFieldNum("zxzsl", it.ZXZSL);
			str += CreateESFieldNum("zhs", it.ZHS);
			str += CreateESFieldNum("zrs", it.ZRS);
			str += CreateESFieldNum("zdkmj", it.ZDKMJ);
			str += CreateESFieldNum("zdks", it.ZDKS);
			str += CreateESFieldNum("djbsl", it.DJBSL);
			str += CreateESFieldNum("yzxsl", it.YZXSL);
			str += CreateESFieldNum("byzxsl", it.BYZXSL);
			str = str.TrimEnd(',');
			str += "}";
			return str;
		}
		protected int Bulk(List<string> lst, int k)
		{
			var tableName = IndexName;
			var url = _esUrl + "/_bulk";
			string str = "";
			foreach (var json in lst)
			{
				string jsonList = CreateBulkIndex(tableName, ++k) + "\n" + json + "\n";
				str += jsonList;
			}
			var res = HttpUtil.PutResponse(url, str, out string statusCode);//, "application/x-ndjson");
			lst.Clear();
			return k;
		}
		protected string CreateBulkIndex(string tableName, int k)
		{
			string str = "{ \"create\" : { \"_index\" : \"" + tableName + "\", \"_type\" : \"doc\", \"_id\" : \"" + Qxdm + "_" + k + "\" } }";
			return str;
		}

		#region FillData Part
		protected override void FillData()
		{
			//dic:[企业名称,乡编码集合]
			var dic = new Dictionary<string, HashSet<string>>();
			var sql = "select c.MC QYMC,b.BM XZQBM from QYXX_XZDY a left join DLXX_XZDY b on a.XZDYID=b.ID left join QYXX c on a.QYID=c.ID where c.MC is not null and b.BM is not null";
			_db.QueryCallback(sql, r =>
			{
				var qymc = r.GetString(0);
				var bm = r.GetString(1);
				if (bm.Length > 9)
				{
					bm = bm.Substring(0, 9);
				}
				if (!dic.TryGetValue(qymc, out HashSet<string> v))
				{
					v = new HashSet<string>();
					dic[qymc] = v;
				}
				v.Add(bm);
				return true;
			});

			foreach (var kv in dic)
			{
				var qymc = kv.Key;
				var setXiangBM = kv.Value;
				bool fXian = false;
				foreach (var v in kv.Value)
				{
					if (v.Length == 6)
					{
						fXian = true;
						break;
					}
				}
				if (fXian)
				{
					if (_xianItem == null)
					{
						_xianItem = GetXianItem();
					}
					dicItem[qymc] = _xianItem;
				}
				else
				{
					QueryZhsAndZcysl(out int zhs, out int zcys, setXiangBM);
					QueryDkxx(out int zdks, out double zdkmj, setXiangBM);
					QueryYzx(out int yzxSl, out int byzxSl,setXiangBM);
					var it = GetItem(qymc);
					it.ZXZSL = setXiangBM.Count;
					it.ZHS = zhs;
					it.ZRS = zcys;
					it.ZDKMJ = zdkmj;
					it.ZDKS = zdks;
					it.DJBSL = QueryQzs(setXiangBM);
					it.YZXSL = yzxSl;
					it.BYZXSL = byzxSl;
				}
			}
		}


		Item GetXianItem()
		{
			QueryZhsAndZcysl(out int zhs, out int zcys);
			QueryDkxx(out int zdks, out double zdkmj);
			QueryYzx(out int yzxSl, out int byzxSl);
			var it = new Item()
			{
				ZXZSL = SafeConvertAux.ToInt32(_db.QueryOne("select count(*) from DLXX_XZDY where JB=3")),
				ZHS=zhs,
				ZRS=zcys,
				ZDKS=zdks,
				ZDKMJ=zdkmj,
				DJBSL= QueryQzs(),
				YZXSL=yzxSl,
				BYZXSL=byzxSl
			};

			
			return it;
		}
		/// <summary>
		/// 查询一致性和不一致性数量
		/// </summary>
		/// <param name="setXiangBM"></param>
		/// <returns></returns>
		void QueryYzx(out int YzxSl, out int ByzxSl, HashSet<string> setXiangBM = null)
		{
			int nYzxSl = 0;
			int nByzxSl = 0;
			var sql = "select SFYWT,count(1) from DJ_CBJYQ_WTFK";
			if (setXiangBM != null && setXiangBM.Count > 0)
			{
				SqlUtil.ConstructIn(setXiangBM, sin =>
				{
					var sql1 = sql + $" where FBFBM is not null and LEN(FBFBM)>8 and SUBSTRING(FBFBM,1,9) in({sin})  group by SFYWT";
					_db.QueryCallback(sql1, r =>
					{
						var bSFYWT = SafeConvertAux.ToInt32(r.GetValue(0));
						int cnt = SafeConvertAux.ToInt32(r.GetValue(1));
						if (bSFYWT == 1)
						{
							nByzxSl += cnt;
						}
						else
						{
							nYzxSl += cnt;
						}
						return true;
					});
				});
			}
			else
			{
				_db.QueryCallback(sql+ " group by SFYWT", r =>
				{
					var bSFYWT = SafeConvertAux.ToInt32(r.GetValue(0));
					int cnt = SafeConvertAux.ToInt32(r.GetValue(1));
					if (bSFYWT == 1)
					{
						nByzxSl += cnt;
					}
					else
					{
						nYzxSl += cnt;
					}
					return true;
				});
			}
			YzxSl = nYzxSl;
			ByzxSl = nByzxSl;
		}

		/// <summary>
		/// 查询总户数和总成员数量
		/// </summary>
		/// <param name="setXiangBM"></param>
		/// <returns></returns>
		void QueryZhsAndZcysl(out int Zhs,out int Zcys, HashSet<string> setXiangBM=null)
		{
			int nZhs = 0;
			int nZcys = 0;
			var sql = "select count(1),sum(CBFCYSL) from QSSJ_CBF where CBFLX='1' and zt=1";
			if (setXiangBM != null && setXiangBM.Count > 0)
			{
				SqlUtil.ConstructIn(setXiangBM, sin =>
				 {
					 var sql1 = sql + $" and SUBSTRING(FBFBM,1,9) in({sin})";
					 _db.QueryCallback(sql1, r =>
					 {
						 nZhs += SafeConvertAux.ToInt32(r.GetValue(0));
						 nZcys += SafeConvertAux.ToInt32(r.GetValue(1));
						 return false;
					 });
				 });
			}
			else
			{
				_db.QueryCallback(sql, r =>
				 {
					 nZhs = SafeConvertAux.ToInt32(r.GetValue(0));
					 nZcys = SafeConvertAux.ToInt32(r.GetValue(1));
					 return false;
				 });
			}
			Zhs = nZhs;
			Zcys = nZcys;
		}
		void QueryDkxx(out int Zdks, out double Zdkmj, HashSet<string> setXiangBM = null)
		{
			int nZdks = 0;
			double nZdkmj = 0;
			var sql = "select count(1),sum(SCMJM) from DLXX_DK where zt=1";
			if (setXiangBM != null && setXiangBM.Count > 0)
			{
				SqlUtil.ConstructIn(setXiangBM, sin =>
				{
					var sql1 = sql + $" and SUBSTRING(FBFBM,1,9) in({sin})";
					_db.QueryCallback(sql1, r =>
					{
						nZdks += SafeConvertAux.ToInt32(r.GetValue(0));
						nZdkmj += SafeConvertAux.ToDouble(r.GetValue(1));
						return false;
					});
				});
			}
			else
			{
				_db.QueryCallback(sql, r =>
				{
					nZdks += SafeConvertAux.ToInt32(r.GetValue(0));
					nZdkmj += SafeConvertAux.ToDouble(r.GetValue(1));
					return false;
				});
			}
			Zdks = nZdks;
			Zdkmj = nZdkmj;
		}

		/// <summary>
		/// 查询登记簿数量
		/// </summary>
		/// <param name="setXiangBM"></param>
		/// <returns></returns>
		int QueryQzs(HashSet<string> setXiangBM = null)
		{
			var sql = "select count(1) from QSSJ_CBJYQZ";
			if (setXiangBM != null && setXiangBM.Count > 0)
			{
				int n = 0;
				SqlUtil.ConstructIn(setXiangBM, sin =>
				{
					var sql1 = sql + $" where SUBSTRING(CBJYQZBM,1,9) in({sin})";
					n += SafeConvertAux.ToInt32(_db.QueryOne(sql1));
				});
				return n;
			}
			return SafeConvertAux.ToInt32(_db.QueryOne(sql));
		}
		#endregion

		/// <summary>
		/// 根据企业名称获取对应数据
		/// </summary>
		/// <param name="qymc"></param>
		/// <returns></returns>
		private Item GetItem(string qymc)
		{
			if (!dicItem.TryGetValue(qymc, out Item it))
			{
				it = new Item();
				dicItem[qymc] = it;
			}
			return it;
		}
	}
	class CountMjItem
	{
		/// <summary>
		/// 数量
		/// </summary>
		public int Count;
		/// <summary>
		/// 面积
		/// </summary>
		public double Mj;
	}


	class JsonRet
	{
		public int total { get; set; }
		public int deleted { get; set; }
	}
}
