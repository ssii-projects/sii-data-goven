using Agro.LibCore;
using System;
using System.Collections.Generic;

namespace JzdxBuild
{
	public static class DkQlrUtil
	{
        /// <summary>
        /// 返回：[dkbm,cbfmc]
        /// </summary>
        /// <param name="mdbFolder"></param>
        /// <returns></returns>
        public static Dictionary<string, string> LoadCbfmc(string mdbFolder)//, string dkShpFile)
        {
            var dic = new Dictionary<string, string>();
            string mdbFile = null;
            FileUtil.EnumFiles(mdbFolder, fi =>
            {
                if (string.Equals(fi.Extension, ".mdb", StringComparison.CurrentCultureIgnoreCase))
                {
                    mdbFile = fi.Name;
                    return false;
                }
                return true;
            });
            if (mdbFile == null)
            {
                throw new Exception("权属数据下面不存在mdb文件");
            }
            using (var db = DBAccess.Open(mdbFolder + mdbFile))
            {

                var dicCbfbm2Cbfmc = QueryKeyValuePairs(db, "CBF", "CBFBM", "CBFMC", "CBFMC is not null");
                var dicDkbm2Cbfbm = QueryKeyValuePairs(db, "CBDKXX", "DKBM", "CBFBM");
                var dicDkbm2Cbfmc = new Dictionary<string, string>();
                foreach (var kv in dicDkbm2Cbfbm)
                {
                    if (dicCbfbm2Cbfmc.TryGetValue(kv.Value, out var cbfmc))
                    {
                        dic[kv.Key] = cbfmc;
                    }
                }
                dicCbfbm2Cbfmc.Clear();
                dicDkbm2Cbfbm.Clear();
                return dic;
            }
        }
        private static Dictionary<string, string> QueryKeyValuePairs(IWorkspace db, string tableName, string keyFieldName, string valueFieldName, string wh = null)
        {
            var dic = new Dictionary<string, string>();
            string sql = "select " + keyFieldName + "," + valueFieldName + " from " + tableName;
            if (wh != null)
                sql += " where " + wh;
            //var dt = db.QueryDataTable(sql);
            db.QueryCallback(sql, r =>
            //foreach (DataRow r in dt.Rows)
            {
                var key = SafeConvertAux.ToStr(r.GetValue(0));
                var val = SafeConvertAux.ToStr(r.GetValue(1));
                dic[key] = val;
            });
            return dic;
        }

    }
}
