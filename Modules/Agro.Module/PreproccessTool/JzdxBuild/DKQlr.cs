/*
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   DKQlr
 * 创 建 人：   颜学铭
 * 创建时间：   2016/9/29 21:52:46
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.GIS;
using System;
using System.Collections.Generic;

namespace JzdxBuild
{
	/// <summary>
	/// 地块权利人
	/// </summary>
	class DKQlr
    {
        private readonly Dictionary<int, string> _dic = new Dictionary<int, string>();
        /// <summary>
        /// 根据地块objectid获取地块权利人
        /// </summary>
        /// <param name="dkOID"></param>
        /// <returns></returns>
        public string GetQlr(int dkOID)
        {
            string qlr;
            if (_dic.TryGetValue(dkOID, out qlr))
            {
                return qlr;
            }
            return null;
        }
        public bool Init(string mdbFolder, string dkShpFile)
        {
            //string txtFileOk = mdbFolder + "cbfmc.txt.ok";
            //if (!File.Exists(txtFileOk))
            //{
            //    return false;
            //}
            //string txtFile = mdbFolder + "cbfmc.txt";
            //if (!File.Exists(txtFileOk))
            //    throw new Exception("文件:" + txtFile + "不存在!");
            //var dicDkbm2Cbfmc = new Dictionary<string, string>();
            //using (var r = new StreamReader(txtFile, Encoding.Default))
            //{
            //    for (var line = r.ReadLine(); !string.IsNullOrEmpty(line); line = r.ReadLine())
            //    {
            //        int n = line.IndexOf(',');
            //        if (n >= 0)
            //        {
            //            var dkbm = line.Substring(0, n);
            //            var cbfmc = line.Substring(n + 1);
            //            dicDkbm2Cbfmc[dkbm] = cbfmc;
            //        }
            //    }
            //}
            var dicDkbm2Cbfmc = DkQlrUtil.LoadCbfmc(mdbFolder);

            using (var shp = new ShapeFile())
            {
                var err = shp.Open(dkShpFile);
                if (err != null)
                    throw new Exception("无法打开Shape文件:" + dkShpFile);
                int nDKBMField = shp.FindField("DKBM");
                int cnt = shp.GetRecordCount();
                for (int dkID = 0; dkID < cnt; ++dkID)
                {
                    var dkbm = shp.GetFieldString(dkID, nDKBMField);
                    if (dicDkbm2Cbfmc.TryGetValue(dkbm, out var cbfmc))
                    {
                        _dic[dkID] = cbfmc;
                    }
                }
            }

            //File.Delete(txtFileOk);
            //File.Delete(txtFile);
            return true;
        }
        //private static Dictionary<string, string> queryKeyValuePairs(IWorkspace db, string tableName, string keyFieldName, string valueFieldName, string wh = null)
        //{
        //    var dic = new Dictionary<string, string>();
        //    string sql = $"select {keyFieldName},{valueFieldName} from {tableName}";
        //    if (wh != null)
        //        sql += " where " + wh;
        //    db.QueryCallback(sql, r =>
        //     {
        //         var key = SafeConvertAux.ToStr(r.GetValue(0));
        //         var val = SafeConvertAux.ToStr(r.GetValue(1));
        //         dic[key] = val;
        //     });
        //    //var dt = db.QueryDataTable(sql);
        //    //foreach (DataRow r in dt.Rows)
        //    //{
        //    //    var key = SafeConvertAux.ToStr(r[0]);
        //    //    var val = SafeConvertAux.ToStr(r[1]);
        //    //    dic[key] = val;
        //    //}
        //    return dic;
        //}
    }


}
