using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/4/18 18:29:58
*/
namespace TestTool
{
	class IndexManager
	{
		#region 创建索引部分
		class Index
		{
			public string index { get; set; }
		}
		public void CreateIndex(string esUrl)
		{
			var url = esUrl + "/_cat/indices";
			var lstIndices = HttpUtil.GetResponse<List<Index>>(url);
			var setIndex = new HashSet<string>();
			foreach (var li in lstIndices)
			{
				setIndex.Add(li.index.ToLower());
			}

			{//agriegov_stats_zones
				var s = @"      'properties' : {
        'dm': { 'type' : 'keyword' },
        'mc': { 'type': 'keyword' },
        'jb': { 'type': 'byte' },
        'sjdm': { 'type': 'keyword' },
        'sjmc': { 'type': 'keyword' },
        'qc': { 'type': 'text' },
        'shape': { 'type': 'geo_shape' },
        'jrsl': { 'type': 'short' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_zones", str, setIndex);
			}

			{//agriegov_stats_contractees
				var s = @"      'properties' : {
        'qxdm': { 'type' : 'keyword' },
        'zfbs': { 'type': 'integer' },
        'zfbmj': { 'type': 'double' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_contractees", str, setIndex);
			}

			{//agriegov_stats_contractors
				var str = "";
				#region agriegov_stats_contractors
				var s = @"      'properties' : {
        'qxdm': { 'type' : 'keyword' },
        'zcbfs': { 'type': 'integer' },
        'cbflx_1': { 'type': 'integer' },
        'cbflx_2': { 'type': 'integer' },
        'cbflx_3': { 'type': 'integer' },
        'zcys': { 'type': 'integer' },
        'gyrs': { 'type': 'integer' },
        'cyxb_1': { 'type': 'integer'},
        'cyxb_2': { 'type': 'integer'},
        'cynl_10': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_20': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_30': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_40': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_60': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_80': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_100': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_100u': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        },
        'cynl_un': {
          'properties': {
            'xb': { 'type': 'keyword' },
            'sl': { 'type': 'integer' }
          }
        }
      }";
				#endregion

				str += s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_contractors", str, setIndex);
			}

			{//agriegov_stats_lands
				var str = "";
				#region agriegov_stats_lands
				var s = @"      'properties' : {
        'qxdm': { 'type' : 'keyword' },
        'zdks': { 'type': 'integer' },
        'scmj': { 'type': 'double' },
        'syqxz_10': { 'type': 'integer' },
        'syqxz_10_mj': { 'type': 'double' },
        'syqxz_30': { 'type': 'integer' },
        'syqxz_30_mj': { 'type': 'double' },
        'syqxz_31': { 'type': 'integer' },
        'syqxz_31_mj': { 'type': 'double' },
        'syqxz_32': { 'type': 'integer' },
        'syqxz_32_mj': { 'type': 'double' },
        'syqxz_33': { 'type': 'integer' },
        'syqxz_33_mj': { 'type': 'double' },
        'syqxz_34': { 'type': 'integer' },
        'syqxz_34_mj': { 'type': 'double' },
        'syqxz_un': { 'type': 'integer' },
        'syqxz_un_mj': { 'type': 'double' },
        'dklb_10': { 'type': 'integer' },
        'dklb_10_mj': { 'type': 'double' },
        'dklb_21': { 'type': 'integer' },
        'dklb_21_mj': { 'type': 'double' },
        'dklb_22': { 'type': 'integer' },
        'dklb_22_mj': { 'type': 'double' },
        'dklb_23': { 'type': 'integer' },
        'dklb_23_mj': { 'type': 'double' },
        'dklb_99': { 'type': 'integer' },
        'dklb_99_mj': { 'type': 'double' },
        'tdlylx_011': { 'type': 'integer' },
        'tdlylx_011_mj': { 'type': 'double' },
        'tdlylx_012': { 'type': 'integer' },
        'tdlylx_012_mj': { 'type': 'double' },
        'tdlylx_013': { 'type': 'integer' },
        'tdlylx_013_mj': { 'type': 'double' },
        'tdyt_1': { 'type': 'integer' },
        'tdyt_1_mj': { 'type': 'double' },
        'tdyt_2': { 'type': 'integer' },
        'tdyt_2_mj': { 'type': 'double' },
        'tdyt_3': { 'type': 'integer' },
        'tdyt_3_mj': { 'type': 'double' },
        'tdyt_4': { 'type': 'integer' },
        'tdyt_4_mj': { 'type': 'double' },
        'dldj_01': { 'type': 'integer' },
        'dldj_01_mj': { 'type': 'double' },
        'dldj_02': { 'type': 'integer' },
        'dldj_02_mj': { 'type': 'double' },
        'dldj_03': { 'type': 'integer' },
        'dldj_03_mj': { 'type': 'double' },
        'dldj_04': { 'type': 'integer' },
        'dldj_04_mj': { 'type': 'double' },
        'dldj_05': { 'type': 'integer' },
        'dldj_05_mj': { 'type': 'double' },
        'dldj_06': { 'type': 'integer' },
        'dldj_06_mj': { 'type': 'double' },
        'dldj_07': { 'type': 'integer' },
        'dldj_07_mj': { 'type': 'double' },
        'dldj_08': { 'type': 'integer' },
        'dldj_08_mj': { 'type': 'double' },
        'dldj_09': { 'type': 'integer' },
        'dldj_09_mj': { 'type': 'double' },
        'dldj_10': { 'type': 'integer' },
        'dldj_10_mj': { 'type': 'double' },
        'jbnt': { 'type': 'integer' },
        'jbnt_mj': { 'type': 'double' }
      }";
				#endregion

				str += s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_lands", str, setIndex);
			}

			{//agriegov_stats_contracts
				var s = @"      'properties' : {
        'qxdm': { 'type' : 'keyword' },
        'zqqs': { 'type': 'integer' },
        'zqqmj': { 'type': 'double' },
        'cbfs_100': { 'type': 'integer' },
        'cbfs_100_mj': { 'type': 'double' },
        'cbfs_110': { 'type': 'integer' },
        'cbfs_110_mj': { 'type': 'double' },
        'cbfs_120': { 'type': 'integer' },
        'cbfs_120_mj': { 'type': 'double' },
        'cbfs_121': { 'type': 'integer' },
        'cbfs_121mj': { 'type': 'double' },
        'cbfs_122': { 'type': 'integer' },
        'cbfs_122_mj': { 'type': 'double' },
        'cbfs_123': { 'type': 'integer' },
        'cbfs_123_mj': { 'type': 'double' },
        'cbfs_129': { 'type': 'integer' },
        'cbfs_129_mj': { 'type': 'double' },
        'cbfs_200': { 'type': 'integer' },
        'cbfs_200_mj': { 'type': 'double' },
        'cbfs_300': { 'type': 'integer' },
        'cbfs_300_mj': { 'type': 'double' },
        'cbfs_900': { 'type': 'integer' },
        'cbfs_900_mj': { 'type': 'double' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_contracts", str, setIndex);
			}

			{//agriegov_stats_clcerts
				var s = @"      'properties': {
        'qxdm': { 'type': 'keyword' },
        'bzs': { 'type': 'integer' },
        'bzmj': { 'type': 'double' },
        'cbfs_100': { 'type': 'integer' },
        'cbfs_100_mj': { 'type': 'double' },
        'cbfs_110': { 'type': 'integer' },
        'cbfs_110_mj': { 'type': 'double' },
        'cbfs_120': { 'type': 'integer' },
        'cbfs_120_mj': { 'type': 'double' },
        'cbfs_121': { 'type': 'integer' },
        'cbfs_121mj': { 'type': 'double' },
        'cbfs_122': { 'type': 'integer' },
        'cbfs_122_mj': { 'type': 'double' },
        'cbfs_123': { 'type': 'integer' },
        'cbfs_123_mj': { 'type': 'double' },
        'cbfs_129': { 'type': 'integer' },
        'cbfs_129_mj': { 'type': 'double' },
        'cbfs_200': { 'type': 'integer' },
        'cbfs_200_mj': { 'type': 'double' },
        'cbfs_300': { 'type': 'integer' },
        'cbfs_300_mj': { 'type': 'double' },
        'cbfs_900': { 'type': 'integer' },
        'cbfs_900_mj': { 'type': 'double' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_clcerts", str, setIndex);
			}

			{//agriegov_stats_biz_regcert
				var s = @"      'properties' : {
        'qxdm' : { 'type' : 'keyword' },
        'zdbyws': { 'type': 'integer' },
        'ybyws': { 'type': 'integer' },
        'dbyws_csdj': { 'type': 'integer' },
        'ybyws_csdj': { 'type': 'integer' },
        'dbyws_bgdj': { 'type': 'integer' },
        'ybyws_bgdj': { 'type': 'integer' },
        'dbyws_gzdj': { 'type': 'integer' },
        'ybyws_gzdj': { 'type': 'integer' },
        'dbyws_fhdj': { 'type': 'integer' },
        'ybyws_fhdj': { 'type': 'integer' },
        'dbyws_hhdj': { 'type': 'integer' },
        'ybyws_hhdj': { 'type': 'integer' },
        'dbyws_zrdj': { 'type': 'integer' },
        'ybyws_zrdj': { 'type': 'integer' },
        'dbyws_huhdj': { 'type': 'integer' },
        'ybyws_huhdj': { 'type': 'integer' },
        'dbyws_bzdj': { 'type': 'integer' },
        'ybyws_bzdj': { 'type': 'integer' },
        'dbyws_hzdj': { 'type': 'integer' },
        'ybyws_hzdj': { 'type': 'integer' },
        'dbyws_zxdj': { 'type': 'integer' },
        'ybyws_zxdj': { 'type': 'integer' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_biz_regcert", str, setIndex);
			}

			{//agriegov_stats_biz_regcert_freq
				var s = @"      'properties' : {
        'qxdm' : { 'type' : 'keyword' },
        'tjrq': {
          'type': 'date',
          'format': 'yyyy-MM-dd'
        },
        'csdj': { 'type': 'integer' },
        'bgdj': { 'type': 'integer' },
        'gzdj': { 'type': 'integer' },
        'fhdj': { 'type': 'integer' },
        'hhdj': { 'type': 'integer' },
        'zrdj': { 'type': 'integer' },
        'huhdj': { 'type': 'integer' },
        'bzdj': { 'type': 'integer' },
        'hzdj': { 'type': 'integer' },
        'zxdj': { 'type': 'integer' },
        'sjcbf': { 'type': 'integer' },
        'sjjtcy': { 'type': 'integer' },
        'sjdks': { 'type': 'integer' },
        'sjdkmj': { 'type': 'double' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_biz_regcert_freq", str, setIndex);
			}

			{//agriegov_stats_qygk
				var s = @"      'properties' : {
        'qxdm' : { 'type' : 'keyword' },
        'qymc':{ 'type' : 'keyword' },
        'zxzsl': { 'type': 'integer' },
        'zhs': { 'type': 'integer' },
        'zrs': { 'type': 'integer' },
        'zdkmj': { 'type': 'double' },
        'zdks': { 'type': 'integer' },
        'djbsl': { 'type': 'integer' },
        'yzxsl': { 'type': 'integer' },
        'byzxsl': { 'type': 'integer' }
      }";
				var str = s.Replace('\'', '"');
				CreateIndex(esUrl, "agriegov_stats_qygk", str, setIndex);
			}
		}
		private void CreateIndex(string esUrl, string indexName, string fields, HashSet<string> allIndex)
		{

			if (allIndex.Contains(indexName.ToLower()))
			{
				Console.WriteLine($"index {indexName} 已存在！");
				return;
			}
			var url = $"{esUrl}/{indexName}";

			var json = "{\"settings\" : {\"number_of_shards\" : 1,\"number_of_replicas\": 0},\"mappings\" : {\"doc\": {";//\"properties\" : {";
			json += fields;
			json += "}}}";

			Console.WriteLine($"put {url}");
			Console.WriteLine(JsonUtil.ConvertJsonString(json));

			var ret = HttpUtil.PutResponse(url, json, out string statusCode);
			Console.WriteLine(ret);
		}
		#endregion
	}
}
