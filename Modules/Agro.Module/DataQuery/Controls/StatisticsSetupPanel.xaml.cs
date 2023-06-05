using Agro.Library.Common.Util;
using Agro.Library.Model;
using Agro.LibCore;
using Agro.LibCore.UI;
using System.Collections.Generic;
using System.Windows.Controls;
using Agro.Library.Common;

namespace Agro.Module.DataQuery
{
	/// <summary>
	/// 承包方类型
	/// </summary>
	enum ECBFLX
    {
        None=0,
        /// <summary>
        /// 农户
        /// </summary>
        NF=1,
        /// <summary>
        /// 个人
        /// </summary>
        GR=2,
        /// <summary>
        /// 单位
        /// </summary>
        DW=3,
    }
    /// <summary>
    /// 自定义统计设置界面
    /// </summary>
    public partial class StatisticsSetupPanel : UserControl
    {
        public class StaticsItem
        {
            private readonly Dictionary<string, string> _dic = new Dictionary<string, string>();
            /// <summary>
            /// 统计类别
            /// </summary>
            public string ItemType
            {
                get
                {
                    return _ckb.Content.ToString();
                }
            }
            ///// <summary>
            ///// 表名（在哪张表中进行统计）
            ///// </summary>
            //public string TableName;
            ///// <summary>
            ///// 在哪个字段中进行统计
            ///// </summary>
            //public string FieldName;
            ///// <summary>
            ///// 统计条件
            ///// </summary>
            //public string Where;
            internal readonly CheckBox _ckb;
            private readonly StatisticsSetupPanel _p;
            public StaticsItem(StatisticsSetupPanel p, CheckBox ckb)
            {
                _p = p;
                _ckb = ckb;
            }
            public string GetText(string zoneCode)
            {
                string s;
                if(_dic.TryGetValue(zoneCode,out s))
                {
                    return s;
                }
                return "";
            }
            public string GetSum()
            {
                double d = 0;
                foreach(var v in _dic.Values)
                {
                    d += SafeConvertAux.ToDouble(v);
                }
                if (d == 0)
                {
                    return "";
                }
                var s = d.ToString();
                if (s.IndexOf('.') >= 0)
                {
                    s = s.TrimEnd('0');
                }
                return s;
            }
            public void OnZoneChanged(IWorkspace db,ShortZone zone)
            {
                string zoneCode = zone.Code;
                if (zone.Level == eZoneLevel.State)
                {
                    zoneCode = "";
                }
                int len = GetSubstringLen(zone.Level);

                //var tCBJYQZ = new ATT_CBJYQZ();
                //var tFBF = new ATT_FBF();
                //var tCBF = new ATT_CBF();
                //var tCBDKXX = new QSSJ_CBDKXX ();
                //var tDK = new VEC_CBDK();
                //var tCBHT = new ATT_CBHT();

                string sql = null;
                if (_ckb == _p.ckbFbfzs)
                {
                    var subStr = db.SqlFunc.SubString(nameof(QSSJ_FBF .FBFBM), 1, len);
                    sql = string.Format("select a.ZoneCode,count(*) from (select {0} as ZoneCode from "+ QSSJ_FBF .GetTableName()+"  where fbfbm is not null and fbfbm like '{1}%') a group by a.ZoneCode"
                        , subStr, zoneCode);
                }
                else if (_ckb == _p.ckbCbfzs)
                {
                    sql = getCfbSql(db, zone, ECBFLX.None);
                }
                else if (_ckb == _p.ckbCbnhzs)
                {
                    sql = getCfbSql(db, zone, ECBFLX.NF);
                }
                else if (_ckb == _p.ckbCbgrzs)
                {
                    sql = getCfbSql(db, zone, ECBFLX.GR);
                }
                else if (_ckb == _p.ckbCbdwzs)
                {
                    sql = getCfbSql(db, zone, ECBFLX.DW);
                }else if (_ckb == _p.ckbJtcyzs)
                {
                    #region 家庭成员数量
                    var en = new QSSJ_CBF ();
                    var subStr = db.SqlFunc.SubString(nameof(en.CBFBM), 1, len);
                    var sql0 = "select " + subStr + " as ZoneCode," + nameof(en.CBFCYSL) + " as cnt from " + QSSJ_CBF .GetTableName()
                        + " where " + nameof(en.CBFBM) + " is not null and " + nameof(en.CBFBM) + " like '" + zoneCode + "%'";
                    sql = "select a.ZoneCode,sum(a.cnt) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if (_ckb == _p.ckbJtcb||_ckb==_p.ckbQtcbfs)
                {//承包方式，参考：农村土地承包经营权汇交成果汇总表格统计方法
                    #region 承包方式（家庭承包，其它方式承包）
                    var en = new QSSJ_CBDKXX ();
                    string wh = nameof(en.CBJYQQDFS) + "='110'";
                    if(_ckb == _p.ckbQtcbfs)
                    {//其它方式承包
                        wh = nameof(en.CBJYQQDFS) + " in('120','121','122','123','129')";
                    }
                    //var t = new Entity<ATT_CBDKXX>();
                    var subStr = db.SqlFunc.SubString(nameof(en.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + QSSJ_CBDKXX.GetTableName() + "  where " + nameof(en.DKBM) + " is not null and "
                        + nameof(en.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if (_ckb == _p.ckbZzy || _ckb == _p.ckbLy || _ckb == _p.ckbXmy || _ckb == _p.ckbYy || _ckb == _p.ckbFnyt)
                {
                    #region 土地用途
                    var en = new DLXX_DK();
                    var sTdyt = eTdytCode.Zzy;
                    if (_ckb == _p.ckbLy)
                    {//其它方式承包
                        sTdyt = eTdytCode.Ly;
                    }else if(_ckb == _p.ckbXmy)
                    {
                        sTdyt = eTdytCode.Xmy;
                    }
                    else if (_ckb == _p.ckbYy)
                    {
                        sTdyt = eTdytCode.Ye;
                    }
                    else if (_ckb == _p.ckbFnyt)
                    {
                        sTdyt = eTdytCode.Fnyt;
                    }
                    string wh = nameof(en.TDYT) + "='" +sTdyt + "'";

                    //var t = new Entity<VEC_DK>();
                    var subStr = db.SqlFunc.SubString(nameof(en.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(en.DKBM) + " is not null and "
                        + nameof(en.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if(_ckb==_p.ckbSt||_ckb == _p.ckbHd|| _ckb == _p.ckbSjd|| _ckb == _p.ckbQtdl)
                {
                    #region 土地利用类型
                    var en = new DLXX_DK();
                    string sTdlylx = null;
                    if (_ckb == _p.ckbSt)
                    {
                        sTdlylx = eTdlylxCode.St;
                    }
                    else if (_ckb == _p.ckbHd)
                    {
                        sTdlylx = eTdlylxCode.Hd;
                    }
                    else if (_ckb == _p.ckbSjd)
                    {
                        sTdlylx = eTdlylxCode.Sjd;
                    }
                    string wh = nameof(en.TDLYLX) + " not in ('" + eTdlylxCode.St + "','"+eTdlylxCode.Hd+"','"+eTdlylxCode.Sjd+"')";
                    if (sTdlylx != null)
                    {
                        wh = nameof(en.TDLYLX) + "='" + sTdlylx + "'";
                    }
                    //var t = new Entity<VEC_DK>();
                    var subStr = db.SqlFunc.SubString(nameof(en.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(en.DKBM) + " is not null and "
                        + nameof(en.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if (_ckb == _p.ckbGytdsyq || _ckb == _p.ckbJttdsyq || _ckb == _p.ckbCmxz || _ckb == _p.ckbCjjtjjzz
                   || _ckb == _p.ckbXjjtjjzz || _ckb == _p.ckbQtnmjtjjzz)
                {
                    #region 所有权性质
                    var en = new DLXX_DK();
                    var eSyqxz = eLandPropertyType.Stated;
                    if (_ckb == _p.ckbJttdsyq)
                    {
                        eSyqxz = eLandPropertyType.Collectived;
                    }
                    else if (_ckb == _p.ckbCmxz)
                    {
                        eSyqxz = eLandPropertyType.GroupOfPeople;
                    }
                    else if (_ckb == _p.ckbCjjtjjzz)
                    {
                        eSyqxz = eLandPropertyType.VillageCollective;
                    }
                    else if (_ckb == _p.ckbCjjtjjzz)
                    {
                        eSyqxz = eLandPropertyType.TownCollective;
                    }
                    else if (_ckb == _p.ckbQtnmjtjjzz)
                    {
                        eSyqxz = eLandPropertyType.TownCollective;
                    }
                    var wh = nameof(en.SYQXZ) + "='" + (int)eSyqxz + "'";
                    //var t = new Entity<VEC_DK>();
                    var subStr = db.SqlFunc.SubString(nameof(en.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(en.DKBM) + " is not null and "
                        + nameof(en.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if (_ckb == _p.ckbDkzl)
                {
                    #region 地块总数量
                    var subStr = db.SqlFunc.SubString(nameof(DLXX_DK.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(DLXX_DK.DKBM) + " is not null and "
                        + nameof(DLXX_DK.DKBM) + " like '" + zoneCode + "%'";
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }else if (_ckb == _p.ckbElhtmj || _ckb == _p.ckbQqmj)
                {
                    //var en = new QSSJ_CBDKXX ();
                    var sSumField = nameof(QSSJ_CBDKXX.HTMJ);//确权面积
                    if (_ckb == _p.ckbElhtmj)
                    {//二轮合同面积
                        sSumField = nameof(QSSJ_CBDKXX.YHTMJ);
                    }
                    //var t = new Entity<ATT_CBDKXX>();
                    var sql0 = "select "+db.SqlFunc.SubString(nameof(QSSJ_CBDKXX.DKBM),1,len)+" ZoneCode,"+sSumField
                        +" cnt from "+ QSSJ_CBDKXX.GetTableName() + "  where "+nameof(QSSJ_CBDKXX.DKBM)+" is not null and "+nameof(QSSJ_CBDKXX.DKBM)+" like '"+zoneCode+"%'";
                    sql = "select a.ZoneCode,sum(a.cnt) from (" + sql0 + ") a group by a.ZoneCode";
                }else if (_ckb == _p.ckbScmj)
                {
                    #region 实测面积
                    var sql0 = "select " + db.SqlFunc.SubString(nameof(DLXX_DK.DKBM), 1, len) + " ZoneCode," + nameof(DLXX_DK.SCMJ)
                        + " cnt from " + DLXX_DK.GetTableName() + "  where " + nameof(DLXX_DK.DKBM) + " is not null and " + nameof(DLXX_DK.DKBM) + " like '" + zoneCode + "%'";
                    sql = "select a.ZoneCode,sum(a.cnt) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if (_ckb == _p.ckbCbdk || _ckb == _p.ckbZld || _ckb == _p.ckbJdd || _ckb == _p.ckbKhd||_ckb==_p.ckbQtjttd)
                {
                    #region 地块类别
                    //var en = new VEC_CBDK();
                    string sTdlb = null;
                    if (_ckb == _p.ckbCbdk)
                    {
                        sTdlb = eDklbCode.Cbdk;
                    }
                    else if (_ckb == _p.ckbZld)
                    {
                        sTdlb = eDklbCode.Zld;
                    }
                    else if (_ckb == _p.ckbJdd)
                    {
                        sTdlb = eDklbCode.Jdd;
                    }
                    else if (_ckb == _p.ckbKhd)
                    {
                        sTdlb = eDklbCode.Khd;
                    }
                    else if (_ckb == _p.ckbQtjttd)
                    {
                        sTdlb = eDklbCode.Qtjttd;
                    }
                    var    wh = nameof(DLXX_DK.DKLB) + "='" + sTdlb + "'";
                    //var t = new Entity<VEC_DK>();
                    var subStr = db.SqlFunc.SubString(nameof(DLXX_DK.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(DLXX_DK.DKBM) + " is not null and "
                        + nameof(DLXX_DK.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }
                else if(_ckb==_p.ckbDldj1|| _ckb == _p.ckbDldj2 || _ckb == _p.ckbDldj3 || _ckb == _p.ckbDldj4 || _ckb == _p.ckbDldj5
                    ||_ckb == _p.ckbDldj6 || _ckb == _p.ckbDldj7 || _ckb == _p.ckbDldj8 || _ckb == _p.ckbDldj9 || _ckb == _p.ckbDldj10)
                {
                    #region 地力等级
                    //var en = new VEC_CBDK();
                    string sDldj = "01";
                    if (_ckb == _p.ckbDldj2)
                    {
                        sDldj = "02";
                    }
                    else if (_ckb == _p.ckbDldj3)
                    {
                        sDldj = "03";
                    }
                    else if (_ckb == _p.ckbDldj4)
                    {
                        sDldj = "04";
                    }
                    else if (_ckb == _p.ckbDldj5)
                    {
                        sDldj = "05";
                    }
                    else if (_ckb == _p.ckbDldj6)
                    {
                        sDldj = "06";
                    }
                    else if (_ckb == _p.ckbDldj7)
                    {
                        sDldj = "07";
                    }
                    else if (_ckb == _p.ckbDldj8)
                    {
                        sDldj = "08";
                    }
                    else if (_ckb == _p.ckbDldj9)
                    {
                        sDldj = "09";
                    }
                    else if (_ckb == _p.ckbDldj10)
                    {
                        sDldj = "10";
                    }
                    var wh = nameof(DLXX_DK.DLDJ) + "='" + sDldj + "'";
                    //var t = new Entity<VEC_DK>();
                    var subStr = db.SqlFunc.SubString(nameof(DLXX_DK.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(DLXX_DK.DKBM) + " is not null and "
                        + nameof(DLXX_DK.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }else if (_ckb == _p.ckbJbnt || _ckb == _p.ckbFjbnt)
                {
                    #region 是否基本农田
                    //var en = new VEC_CBDK();
                    string sSfjbnt = eSfjbntCode.Fou;
                    if (_ckb == _p.ckbJbnt)
                    {
                        sSfjbnt = eSfjbntCode.Shi;
                    }
                     var wh = nameof(DLXX_DK.SFJBNT) + "='" + sSfjbnt + "'";
                    //var t = new Entity<VEC_DK>();
                    var subStr = db.SqlFunc.SubString(nameof(DLXX_DK.DKBM), 1, len);
                    var sql0 = "select " + subStr + "  ZoneCode from " + DLXX_DK.GetTableName() + "  where " + nameof(DLXX_DK.DKBM) + " is not null and "
                        + nameof(DLXX_DK.DKBM) + " like '" + zoneCode + "%' and " + wh;
                    sql = "select a.ZoneCode,count(*) from (" + sql0 + ") a group by a.ZoneCode";
                    #endregion
                }else if (_ckb == _p.ckbGytdsyq || _ckb == _p.ckbJttdsyq || _ckb == _p.ckbCmxz || _ckb == _p.ckbCjjtjjzz || _ckb == _p.ckbXjjtjjzz || _ckb == _p.ckbQtnmjtjjzz)
                {

                }else if (_ckb == _p.ckbJtcbqzsl || _ckb == _p.ckbFjtcbqzsl|| _ckb == _p.ckbHtzs)
                {
                    string wh = null;
                    if (_ckb == _p.ckbFjtcbqzsl)
                    {
                        wh = "d.CBFS<>'110'";
                    }else if (_ckb == _p.ckbJtcbqzsl)
                    {
                        wh="d.CBFS='110'"; 
                    }
                    var subStr = db.SqlFunc.SubString("CBJYQZBM", 1, len);
                    var sql0 = "select " + subStr + " ZoneCode, d.CBFS  from "+ QSSJ_CBJYQZ.GetTableName() + " c left join "+ QSSJ_CBHT.GetTableName() + " as d on c.CBJYQZBM = d.CBHTBM ";
                    if (wh != null)
                    {
                        sql0 +=" where "+ wh;
                    }
                    sql = "select a.ZoneCode,count(*) from(" + sql0 + ")  a group by a.ZoneCode";
                }else if (_ckb == _p.ckbHtzmj)
                {//承包合同总面积
                    //var en = new ATT_CBHT();
                    //var t = new Entity<ATT_CBHT>();
                    var sql0 = "select "+db.SqlFunc.SubString(nameof(QSSJ_CBHT.CBHTBM), 1,len)+" ZoneCode,"+nameof(QSSJ_CBHT.HTZMJ)+" cnt from "+ QSSJ_CBHT.GetTableName()
						+ " where "+nameof(QSSJ_CBHT.CBHTBM)+" is not null and "+nameof(QSSJ_CBHT.CBHTBM)+" like '"+zoneCode+"%'";
                    sql = "select a.ZoneCode,sum(a.cnt) from(" + sql0 + ") a group by a.ZoneCode";
                }else if (_ckb == _p.ckbBfqzsl)
                {//颁发权证总数
                    //var en = new ATT_CBJYQZ();
                    var sql0 = "select "+db.SqlFunc.SubString(nameof(QSSJ_CBJYQZ.CBJYQZBM), 1, len)+"  ZoneCode from "+ QSSJ_CBJYQZ.GetTableName()
						+ "  where "+nameof(QSSJ_CBJYQZ.CBJYQZBM)+" is not null and "+nameof(QSSJ_CBJYQZ.CBJYQZBM)+" like '"+zoneCode+"%'";
                    sql = "select a.ZoneCode,count(*) from(" + sql0 + ") a group by a.ZoneCode";
                }else if (_ckb == _p.ckbBfqzmj)
                {//颁发权证面积
                    var sql0 = "select " + db.SqlFunc.SubString("a.CBJYQZBM", 1, len) + " ZoneCode,b.HTZMJ cnt from "+ QSSJ_CBJYQZ.GetTableName() + " as a left join "+ QSSJ_CBHT.GetTableName() + " as b on a.CBJYQZBM = b.CBHTBM";
                    sql = "select c.ZoneCode,sum(c.cnt) from (" + sql0 + ") c group by c.ZoneCode";
                }

                doQuery(db, sql);
            }
            private void doQuery(IWorkspace db,string sql)
            {
                if (sql == null)
                {
                    return;
                }
                _dic.Clear();
                db.QueryCallback(sql, r =>
                {
                    var bm = r.GetValue(0);
                    var cnt = SafeConvertAux.ToStr(r.GetValue(1));
                    if (cnt.IndexOf('.') >= 0)
                    {
                        cnt = cnt.TrimEnd('0');
                    }
                    _dic[bm.ToString()] = cnt;
                    return true;
                });
            }

            /// <summary>
            /// 承包方相关
            /// </summary>
            /// <param name="db"></param>
            /// <param name="zone"></param>
            /// <param name="cfblx"></param>
            /// <returns></returns>
            private string getCfbSql(IWorkspace db,ShortZone zone, ECBFLX cfblx)
            {
                int len = GetSubstringLen(zone.Level);
                var cbfbm = nameof(QSSJ_CBF .CBFBM);
                var subStr = db.SqlFunc.SubString(cbfbm, 1, len);
                var sql0 = string.Format("select {0} as ZoneCode from {1} where {2} is not null and {3} like '{4}%'"
                    , subStr, QSSJ_CBF .GetTableName(), cbfbm, cbfbm,zone.Code);
                if (cfblx != ECBFLX.None)
                {
                    sql0 += " and " + nameof(QSSJ_CBF .CBFLX) + "=" + (int)cfblx;
                }
               var sql = string.Format("select a.ZoneCode,count(*) from ({0}) a group by a.ZoneCode" , sql0);

                return sql;
            }
            private static int GetSubstringLen(eZoneLevel level)
            {
                switch (level)
                {
                    case eZoneLevel.State:
                        return 2;
                    case eZoneLevel.Province:
                        return 4;
                    case eZoneLevel.City:
                        return 6;
                    case eZoneLevel.County:
                        return 9;
                    case eZoneLevel.Town:
                        return 12;
                    case eZoneLevel.Village:
                        return 14;
                }
                return 14;
            }
        }
        private readonly List<StaticsItem> _lstTjlb = new List<StaticsItem>();
        public StatisticsSetupPanel()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 获取统计类别
        /// </summary>
        /// <returns></returns>
        public List<StaticsItem> GetTjlbs()
        {
            _lstTjlb.Clear();
            ScanControl.Scan(sp1, c =>
             {
				 if (c is CheckBox ckb && ckb.IsChecked == true)
				 {
					 var i = new StaticsItem(this, ckb);
					 _lstTjlb.Add(i);
				 }
				 return true;
             });
            _lstTjlb.Sort((a, b) =>
            {
                var c0 = SafeConvertAux.ToInt32(a._ckb.Tag.ToString());
                var c1 = SafeConvertAux.ToInt32(b._ckb.Tag.ToString());
                return c0 < c1?-1:1;
            });
            return _lstTjlb;
        }
    }
}
