using Agro.GIS;
using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace JzdxBuild
{
	/// <summary>
	/// 初始化界址点界址线辅助类
	/// </summary>
	class InitLandDotCoilUtil
    {
        ///// <summary>
        ///// 从界址线中获取界址点
        ///// </summary>
        ///// <param name="r"></param>
        ///// <param name="dicJzd"></param>
        //public static void AcquireJzd(JzxRing r, Dictionary<Coordinate, JzdEdges> dicJzd)
        //{
        //    var preJzx = r[r.Count - 1];
        //    for (int i = 0; i < r.Count; ++i)
        //    {
        //        var jzx = r[i];
        //        var jzd = jzx.qJzd;
        //        JzdEdges lst;
        //        if (!dicJzd.TryGetValue(jzd, out lst))
        //        {
        //            lst = new JzdEdges();
        //            dicJzd[jzd] = lst;
        //        }
        //        lst.Add(new JzdEdge(){OutEdge=jzx,InEdge=preJzx});
        //    }
        //    /*
        //    foreach (var jzx in r)
        //    {
        //        var jzd = jzx.qJzd;

        //        //if(CglUtil.equal(jzd.X,448724.333679,0.001)
        //        //    && CglUtil.equal(jzd.Y, 3483816.152527, 0.001))
        //        //{
        //        //    Console.WriteLine("(448724.333679, 3483816.152527)'s dkid=" + jzx.dk.dkID);
        //        //}

        //        JzdEdges lst;
        //        if (!dicJzd.TryGetValue(jzd, out lst))
        //        {
        //            lst = new JzdEdges();
        //            dicJzd[jzd] = lst;
        //        }
        //        lst.Add(jzx);
        //    }
        //    */
        //}
        /// <summary>
        /// 根据一组rowid集合查询对应承包地简要信息；
        /// </summary>
        /// <param name="db"></param>
        /// <param name="rowids"></param>
        /// <param name="dic"></param>
        public static void QueryShortZd_cbd(ShapeFile db, List<int> rowids, Dictionary<int, ShortZd_cbd> dic
            , Action<ShortZd_cbd> callback)
        {
            foreach(var shpID in rowids)
            {
                var en = new ShortZd_cbd();
                en.rowid =shpID;// r.GetInt32(0);
                var wkb=db.GetWKB(shpID);
                var g = WKBHelper.FromBytes(wkb);
                en.xmin = g.EnvelopeInternal.MinX;
                en.xmax = g.EnvelopeInternal.MaxX;

                en.zjrMc=db.GetFieldString(shpID,db.FindField(Zd_cbdFields.ZJRXM));
                //en.qlrMc = SqlUtil.GetString(r, 2);
                //en.dkID = SqlUtil.GetString(r, 3);
                //en.zlDM = SqlUtil.GetString(r, 4);
                en.DKBM =db.GetFieldString(shpID,db.FindField(Zd_cbdFields.DKBM));
                if (g is Polygon)
                {
                    parsePoints(g as Polygon, en);
                }
                else if (g is MultiPolygon)
                {
                    var mg = g as MultiPolygon;
                    foreach (var g1 in mg.Geometries)
                    {
                        parsePoints(g1 as Polygon, en);
                    }
                }
                dic[en.rowid] = en;//.use();
                callback(en);
            }
        }

        //public static void QueryShapeDkEntities(DBSpatialite db,string where, List<ExportJzdx.ShapeDkEntity> lst
        //    ,Action<ExportJzdx.ShapeDkEntity> callback)
        //{
        //    var xml = new XmlDocument();

        //    //string sin = SqlUtil.ConstructInClause(rowids);
        //    var subFields =Zd_cbdFields.DKBM+","+Zd_cbdFields.DKMC
        //    +","+Zd_cbdFields.DKLB+","+Zd_cbdFields.TDLYLX
        //    +","+Zd_cbdFields.DLDJ+","+Zd_cbdFields.TDYT
        //    +","+Zd_cbdFields.SFJBNT+","+Zd_cbdFields.SCMJ
        //    +","+Zd_cbdFields.DKDZ+","+Zd_cbdFields.DKXZ
        //    +","+Zd_cbdFields.DKNZ+","+Zd_cbdFields.DKBZ
        //    + "," + Zd_cbdFields.DKBZXX + "," + Zd_cbdFields.DKKZXX 
        //    + ",AsBinary(" + Zd_cbdFields.Shape + ")";
        //    var sql = "select " + subFields + " from " + Zd_cbdFields.TABLE_NAME
        //        + " where "+where;
        //    db.QueryCallback(sql, r =>
        //    {
        //        var en = new ExportJzdx.ShapeDkEntity();
        //        int i = 0;
        //        en.DKBM = SqlUtil.GetString(r, 0);
        //        en.DKMC = SqlUtil.GetString(r, ++i);
        //        en.DKLB = SqlUtil.GetString(r, ++i);
        //        en.TDLYLX = SqlUtil.GetString(r, ++i);
        //        en.DLDJ = SqlUtil.GetString(r, ++i);
        //        en.TDYT = SqlUtil.GetString(r, ++i);
        //        en.SFJBNT = SqlUtil.GetBoolean(r, ++i)==true?"1":"0";
        //        en.SCMJM = r.GetDouble(++i);
        //        //en.SCMJ = en.SCMJM * 10000 / 15.0;
        //        en.DKDZ = SqlUtil.GetString(r, ++i);
        //        en.DKXZ = SqlUtil.GetString(r, ++i);
        //        en.DKNZ = SqlUtil.GetString(r, ++i);
        //        en.DKBZ = SqlUtil.GetString(r, ++i);
        //        en.DKBZXX = SqlUtil.GetString(r, ++i);
        //        var xmlKzxx = SqlUtil.GetString(r, ++i);
        //        en.wkb = r.GetValue(++i) as byte[];
        //        if (en.wkb != null)
        //        {
        //            var g = WKBHelper.fromWKB(en.wkb);
        //            en.env = g.EnvelopeInternal;
        //            en.coords = g.Coordinates;
        //            //en.xmin = g.EnvelopeInternal.MinX;
        //            //en.xmax = g.EnvelopeInternal.MaxX;
        //            //en.qlrMc = SqlUtil.GetString(r, 2);
        //            //en.dkID = SqlUtil.GetString(r, 3);
        //            //en.zlDM = SqlUtil.GetString(r, 4);
        //            //en.DKBM = SqlUtil.GetString(r, 5);
        //            //var xmlKzxx = SqlUtil.GetString(r, 6);
        //            if (xmlKzxx != null)
        //            {
        //                //xmlKzxx="<?xml version=\"1.0\" encoding=\"utf-16\"?>";
        //                //xmlKzxx+="<AgricultureLandExpand>";
        //                //xmlKzxx += "<Elevation>100</Elevation>";
        //                //xmlKzxx+="</AgricultureLandExpand>";
        //                xml.LoadXml(xmlKzxx);
        //                //var n = xml.SelectSingleNode("/AgricultureLandExpand/Elevation");
        //                //if (n != null && !string.IsNullOrEmpty(n.InnerText))
        //                //{
        //                //    en.elevation = SafeConvertAux.SafeConvertToDouble(n.InnerText);
        //                //}
        //                var n = xml.SelectSingleNode("/AgricultureLandExpand/ReferPerson");
        //                if (n != null && !string.IsNullOrEmpty(n.InnerText))
        //                {
        //                    en.ZJRXM = n.InnerText;
        //                }
        //            }
        //            lst.Add(en);
        //            //parsePoints(g as Polygon, en);
        //            //dic[en.rowid] = en;//.use();
        //            callback(en);
        //        }
        //        return true;
        //    });
        //}

        ///// <summary>
        ///// 按条件删除界址点
        ///// </summary>
        ///// <param name="db"></param>
        ///// <param name="wh"></param>
        //public static void DeleteJzd(DBSpatialite db, string wh)
        //{
        //    var sql = "delete from " + JzdFields.TABLE_NAME + " where " + wh;
        //    db.ExecuteNonQuery(sql);
        //}

        //public static void DeleteByRowids(DBSpatialite db,string tableName, List<int> rowids)
        //{
        //    for (int i = 0; i < rowids.Count; )
        //    {
        //        int j = i + 100;
        //        if (j > rowids.Count)
        //        {
        //            j = rowids.Count;
        //        }
        //        string sin = null;
        //        for (int k = i; k < j; ++k)
        //        {
        //            if (sin != null)
        //                sin += ",";
        //            sin += rowids[k].ToString();
        //        }
        //        if (sin != null)
        //        {
        //            db.Delete(tableName, "rowid in(" + sin + ")");
        //        }
        //        i = j;
        //    }
        //}

        /// <summary>
        /// 按西北角顺时针排序
        /// </summary>
        /// <param name="coords">in,out</param>
        /// <param name="fCW">输入集合coords是否按顺时针排序</param>
        public static void SortCoordsByWNOrder(JzxRing r, bool fCW, Action<Coordinate> callback)
        {
            //System.Diagnostics.Debug.Assert(coords.Length > 0);
            //var orderCdts = new Coordinate[coords.Length];
            int len = r.Count;
            //double area = 0.0f;
            var p0 = r[0].qJzd;// coords[0];
            double leftX = p0.X;
            double topY = p0.Y;
            int ip = len - 1;
            for (int i = 1; i < len; ip = i++)
            {
                var p = r[ip].qJzd;
                var q = r[i].qJzd;
                //double a1 = p.X * q.Y;
                //double a2 = q.X * p.Y;
                ////area += a1 - a2;
                //if (i > 0)
                //{
                if (leftX > q.X)
                    leftX = q.X;
                if (topY < q.Y)
                    topY = q.Y;
                //}
            }
            //fCCW = area > 0;
            double d2 = 0;
            int n = 0;
            for (int i = 0; i < len; ++i)
            {
                var p = r[i].qJzd;
                double dx = p.X - leftX;
                double dy = p.Y - topY;
                double d = dx * dx + dy * dy;
                if (d < d2 || i == 0)
                {
                    d2 = d;
                    n = i;
                }
            }
            Coordinate c0 = null;
            if (fCW)
            {
                for (int i = n; i < len; ++i)
                {
                    if (c0 == null)
                        c0 = r[i].qJzd;
                    callback(r[i].qJzd);
                    //orderCdts[j] = coords[i];
                }
                for (int i = 0; i < n; ++i)
                {
                    callback(r[i].qJzd);
                }
            }
            else
            {
                for (int i = n; i >= 0; --i)
                {
                    if (c0 == null)
                        c0 = r[i].qJzd;
                    callback(r[i].qJzd);
                    //orderCdts[j] = coords[i];
                }
                for (int i = len-1; i > n; --i)
                {
                    callback(r[i].qJzd);
                }
            }
        }

        public static string CreateNewID()
        {
            return Guid.NewGuid().ToString().Trim(new char[] { '{', '}' }); 
        }
        public static bool testIsEqual(Coordinate c, double x, double y,double tolerance=0.001)
        {
            return CglUtil.Equal(c.X, x, tolerance) && CglUtil.Equal(c.Y, y, tolerance);
        }
        /// <summary>
        /// 向左边缓冲distance距离和得到的Polygon对象
        /// </summary>
        /// <param name="ptFrom"></param>
        /// <param name="ptTo"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static IPolygon BufferLeft(Coordinate ptFrom, Coordinate ptTo, double distance)
        {
            var p1 = CglUtil.DeflectionDistance(ptFrom, ptTo, 90, 0.1);
            var p2 = CglUtil.DeflectionDistance(ptTo, ptFrom, -90, 0.1);
            var c1 = CglUtil.DeflectionDistance(ptFrom, ptTo, 90, distance);
            var c2 = CglUtil.DeflectionDistance(ptTo, ptFrom, -90, distance);



            var coords = new Coordinate[5];
            coords[0] = p1;// ptTo;// deflection_distance(ptFrom, ptTo, 90, distance);
            coords[1] = c1;
            coords[2] = c2;
            coords[3] = p2;// ptFrom;// deflection_distance(ptTo, ptFrom, -90, distance);
            coords[coords.Length - 1] = coords[0];
            return new Polygon(new LinearRing(coords));
        }

        //public static void SaveJzd(DBSpatialite db,InitLandDotCoilParam initParam, List<JzdEntity> lst)
        //{
        //    var cjsj = DateTime.Now.ToString();
        //    var fields=new string[]{
        //        JzdFields.ID,
        //        JzdFields.BSM,
        //        JzdFields.TBJZDH,
        //        JzdFields.JZDH,

        //    };
        //    var updateParams = new SQLiteParam[fields.Length+1];
        //    var insertParams = new SQLiteParam[fields.Length];
        //    for (int i = 0; i < fields.Length; ++i)
        //    {
        //        updateParams[i] = new SQLiteParam()
        //        {
        //            ParamName = fields[i]
        //        };
        //        insertParams[i] = new SQLiteParam()
        //        {
        //            ParamName = fields[i]
        //        };
        //    }
        //    updateParams[fields.Length]=new SQLiteParam(){ParamName="rowid"};

        //    var updateSql =constructUpdateSql(JzdFields.TABLE_NAME,fields,"rowid=@rowid");
        //    var insertSql = constructInsertSql(JzdFields.TABLE_NAME, fields);
        //    foreach (var en in lst)
        //    {
        //        var fUpdate = en.rowID > 0;
        //        var prms = fUpdate ? updateParams : insertParams;
        //        int i = 0;
        //        prms[i].ParamValue = en.ID;
        //        prms[++i].ParamValue = en.BSM;
        //        prms[++i].ParamValue = initParam.AddressPointPrefix + en.TBJZDH;
        //        prms[++i].ParamValue = initParam.AddressPointPrefix + en.JZDH;
        //        if (fUpdate)
        //        {
        //            prms[++i].ParamValue = en.rowID;
        //            db.ExecuteNonQuery(updateSql, prms);
        //        }
        //        else
        //        {
        //            db.ExecuteNonQuery(insertSql, prms);
        //        }
        //    }
        //}
        //public static void QueryJzdRowIDs(DBSpatialite db, Dictionary<string, List<int>> dic)
        //{

        //}

        private static void parsePoints(Polygon g,ShortZd_cbd cbd)
        {
            parsePoints(resort(g.Shell.Coordinates,true),cbd,true);
            foreach (var h in g.Holes)
            {
                parsePoints(resort(h.Coordinates,false), cbd, false);
            }
        }

        private static void parsePoints(Coordinate[] coords, ShortZd_cbd cbd,bool fShell)
        {
            if (coords.Length < 4)
                return;
            var lstJzx=fShell?cbd.shell:new JzxRing();
            if (!fShell)
            {
                if (cbd.holes == null)
                {
                    cbd.holes = new List<JzxRing>();
                }
                cbd.holes.Add(lstJzx);
            }

            Coordinate preJzd = null;
            for (int i = 0; i < coords.Length; ++i)
            {
                var jzd = coords[i];
                if (i == coords.Length - 1)
                {
                    if (!CglUtil.IsSamePoint(preJzd, jzd))
                    {
                        var jzx = new Jzx(preJzd, jzd, cbd, fShell);
                        lstJzx.Add(jzx);
                    }
                    else
                    {
                        Console.WriteLine("preJzd=" + preJzd + ",jzd=" + jzd);
                    }
                    return;
                }
                //lstJzd.Add(jzd);
                if (i == 0)
                {
                    preJzd = jzd;
                    //iJzd0Index = lstJzd.Count - 1;
                }
                else
                {
                    if (!CglUtil.IsSamePoint(preJzd, jzd))
                    {
                        var jzx = new Jzx(preJzd, jzd, cbd, fShell);
                        preJzd = jzd;
                        lstJzx.Add(jzx);
                    }
                    else
                    {
                        Console.WriteLine("preJzd=" + preJzd + ",jzd=" + jzd);
                    }
                }
            }
        }

        /// <summary>
        /// 降coords按顺时针方向排序
        /// </summary>
        /// <param name="coords"></param>
        private static Coordinate[] resort(Coordinate[] coords,bool fShell)
        {
            var fCCW=GeometryUtil.IsCCW(coords);
            if (fCCW&&fShell||!fCCW&&!fShell)
            {
                ArrayUtil.Reverse(coords);
            }
            return coords;
        }

        public static string constructUpdateSql(string tableName, string[] fields,string where,string geometry)
        {
            string updateSql = String.Format("update {0} set ", tableName);
            for (int i = 0; i < fields.Length; ++i)
            {
                var fieldName = fields[i];
                if (i > 0)
                    updateSql += ",";
                if (StringUtil.isEqualIgnorCase(fieldName, "shape"))
                {
                    updateSql += fieldName + "=" + geometry;
                }
                else
                {
                    updateSql += fieldName + "=@" + fieldName;
                }
            }
            updateSql += " where " + where;
            return updateSql;
        }
        public static string constructInsertSql(string tableName, string[] fields,string geomText)
        {
            var sql = String.Format("insert into {0}(", tableName);
            for (int i = 0; i < fields.Length; ++i)
            {
                var fieldName = fields[i];
                if (i > 0)
                    sql += ",";
                sql += fieldName;
            }
            sql += ") values(";
            for (int i = 0; i < fields.Length; ++i)
            {
                var fieldName = fields[i];
                if (i > 0)
                    sql += ",";
                if (StringUtil.isEqualIgnorCase(fieldName, "shape"))
                {
                    sql += geomText;
                }
                else
                {
                    sql += "@" + fieldName;
                }
            }
            sql += ")";
            return sql;
        }

        public static string findSpatialReference(string sSpatialReferencesPath, int srid)
        {
            string str = null;
            FileUtil.EnumFiles(sSpatialReferencesPath, fi =>
            {
                
                return true;
            });
            return str;
        }
        private static void aaa(string folderFullName, Func<System.IO.FileInfo, bool> callback)
        {
            var TheFolder = new System.IO.DirectoryInfo(folderFullName);
            //遍历文件
            foreach (var NextFile in TheFolder.GetFiles())
            {
                var fContinue= callback(NextFile);
                if (!fContinue)
                    return;
            }
            //遍历文件夹
            foreach (var NextFolder in TheFolder.GetDirectories())
            {
                aaa(NextFolder.FullName, callback);
            }
        }
    }

    /// <summary>
    /// 行政地域辅助类
    /// </summary>
    class XzdyUtil
    {
        private readonly Dictionary<string, string> _dicXiang = new Dictionary<string, string>();

        /// <summary>
        /// [村的全编码，<村名称，乡全编码>]
        /// </summary>
        private readonly Dictionary<string, Tuple<string, string>> _dicCun = new Dictionary<string, Tuple<string, string>>();
        /// <summary>
        /// 获取村的短全名称（乡镇名+村名），如：安云乡二龙村
        /// 返回映射[地域全编码，短全名称]
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public void Init()//DBSpatialite db)
        {
            //var sql = "select DYJB,DYQBM,DYMC,SJQBM from JCSJ_XZQY where DYJB in(2,3)";
            //db.QueryCallback(sql, r =>
            //{
            //    var jb = r.GetInt32(0);
            //    var dm = SqlUtil.GetString(r, 1);
            //    var name = SqlUtil.GetString(r, 2);
            //    if (jb == 3)
            //    {
            //        _dicXiang[dm] = name;
            //    }
            //    else
            //    {
            //        var sjQbm = SqlUtil.GetString(r, 3);
            //        _dicCun[dm] =new Tuple<string,string>(name,sjQbm);
            //    }
            //    return true;
            //});
        }

        /// <summary>
        /// 根据村的全编码获取村的短全名称（乡镇名+村名），如：安云乡二龙村
        /// </summary>
        /// <param name="cunQbm"></param>
        /// <returns></returns>
        public string GetShortQmc(string cunQbm)
        {
            if (cunQbm.Length > 12)
            {//村的全编码是12位
                cunQbm = cunQbm.Substring(0, 12);
            }
            Tuple<string, string> cun;
            if (_dicCun.TryGetValue(cunQbm, out cun))
            {
                string xiangMc;
                var cunMc = cun.Item1;
                var xiangQbm = cun.Item2;
                if (_dicXiang.TryGetValue(xiangQbm, out xiangMc))
                {
                    return xiangMc + cunMc;
                }
                else
                {
                    return cunMc;
                }
            }
            return null;
        }
    }
}
