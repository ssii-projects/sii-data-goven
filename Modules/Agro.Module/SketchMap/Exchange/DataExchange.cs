using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Agro.LibCore;
namespace Agro.Module.SketchMap
{
    /// <summary>
    /// 交换数据
    /// </summary>
    public class DataExchange
    {
        #region Static

        /// <summary>
        /// 从文件中读取数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<ContractConcord> Deserialize(string fileName)
        {
            try
            {
                string text = "";
                using (TextReader reader = new StreamReader(fileName, Encoding.UTF8))
                {
                    text = reader.ReadToEnd();
                }
                ContractConcord[] concords = JsonConvert.DeserializeObject<ContractConcord[]>(text);
                if (concords == null || concords.Length == 0)
                {
                    return new List<ContractConcord>();
                }
                var datas = concords.TryToList();
                datas.ForEach(da => da.Lands = da.Lands.TryToList().OrderBy(ld => ld.DKBM).ToArray());
                return datas;
            }
            catch (SystemException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return new List<ContractConcord>();
        }

        /// <summary>
        /// 初始化承包方编码
        /// </summary>
        /// <param name="concord"></param>
        /// <returns></returns>
        public static string InitalizeSenderCode(ContractConcord concord)
        {
            if (concord == null || string.IsNullOrEmpty(concord.CBFBM))
            {
                return "";
            }
            string senderCode = concord.CBFBM.Length >= 14 ? concord.CBFBM.Substring(0, 14) : concord.CBFBM;
            return senderCode;
        }

        /// <summary>
        /// 从文件中读取数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<AttornConcord> ReaderData(string fileName)
        {
            try
            {
                string text = "";
                using (TextReader reader = new StreamReader(fileName, Encoding.UTF8))
                {
                    text = reader.ReadToEnd();
                }
                AttornConcord[] concords = JsonConvert.DeserializeObject<AttornConcord[]>(text);
                if (concords == null || concords.Length == 0)
                {
                    return new List<AttornConcord>();
                }
                var datas = concords.TryToList();
                datas.ForEach(da => da.Lands = da.Lands.TryToList().OrderBy(ld => ld.DKBM).ToArray());
                datas.ForEach(da => da.Lands.TryToList().ForEach(ld =>
                {
                    if (ld.DKMJM == 0.0)
                    {
                        ld.DKMJM = Math.Round((ld.DKMJ * 0.0015), 2);
                    }
                }));
                return datas;
            }
            catch (SystemException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return new List<AttornConcord>();
        }

        #endregion
    }

    /// <summary>
    /// 承包合同
    /// </summary>
    public class ContractConcord
    {
        [JsonProperty("CBFBM")]
        public string CBFBM { get; set; }
        [JsonProperty("CBFMC")]
        public string CBFMC { get; set; }
        [JsonProperty("CBDKXX")]
        public ContractLand[] Lands { get; set; }
    }

    /// <summary>
    /// 承包地块
    /// </summary>
    public class ContractLand
    {
        [JsonProperty("DKBM")]
        public string DKBM { get; set; }
		[JsonProperty("DKMC")]
		public string DKMC { get; set; }

        [JsonProperty("HTMJ")]
        public double HTMJ { get; set; }
        [JsonProperty("HTMJM")]
        public double HTMJM { get; set; }
        [JsonProperty("SFQQQG")]
        public string IsShared { get; set; }
		[JsonProperty("dkdz")]
		public string DKDZ { get; set; }
		[JsonProperty("dknz")]
		public string DKNZ { get; set; }
		[JsonProperty("dkxz")]
		public string DKXZ { get; set; }
		[JsonProperty("dkbz")]
		public string DKBZ { get; set; }
	}

    /// <summary>
    /// 流转合同
    /// </summary>
    public class AttornConcord
    {
        [JsonProperty("JZSBM")]
        public string JZSBM { get; set; }
        [JsonProperty("SRF")]
        public string SRF { get; set; }
        [JsonProperty("FBFBM")]
        public string[] FBFBMS { get; set; }
        [JsonProperty("LZDK")]
        public AttornLand[] Lands { get; set; }
    }

    /// <summary>
    /// 流转地块
    /// </summary>
    public class AttornLand
    {
        [JsonProperty("DKBM")]
        public string DKBM { get; set; }
        [JsonProperty("DKMJ")]
        public double DKMJ { get; set; }
        [JsonProperty("DKMJM")]
        public double DKMJM { get; set; }
    }
}
