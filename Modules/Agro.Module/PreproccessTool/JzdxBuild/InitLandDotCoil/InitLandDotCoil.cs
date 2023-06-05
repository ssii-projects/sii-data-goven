using Agro.GIS;
using Agro.LibCore;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace JzdxBuild
{
    /// <summary>
    /// 初始化界址点和界址线业务类
    /// </summary>
    public class InitLandDotCoil
    {
        /// <summary>
        /// 界址点表辅助类
        /// </summary>
        class TableBase
        {
            protected int _nCurrShapeID = 0;

            protected readonly InitLandDotCoil _p;

            protected TableBase(InitLandDotCoil p_)
            {
                _p = p_;
            }

            public int ExportedCount
            {
                get
                {
                    return _nCurrShapeID;
                }
            }
            public void saveDeletedRowids()//DBSpatialite db)
            {
            }
            public void Clear()
            {
                _nCurrShapeID = 0;
            }
            public void testLogout(string tableAlias)
            {
                //Console.WriteLine("修改" + _tableName + "：" + _nTestUpdateCount + "条");
                //Console.WriteLine("插入" + _tableName + "：" + _nTestInsertCount + "条");
                //Console.WriteLine("删除" + _tableName + "：" + _delRowids.Count + "条");
            }


        }
        /// <summary>
        /// 界址点表辅助类
        /// </summary>
        class JzdTable : TableBase
        {
            private int _nStartBSM;
            private readonly JzxTable _jzxTable;
            private ShapeFile _dkShp;
            private ShapeFile _jzdShp;
            private int _nKJZBField;
            private IPoint _tmpPoint = GeometryUtil.MakePoint(0, 0);
            private readonly int[] _jzdDbfFieldIndex = new int[8];
            public JzdTable(InitLandDotCoil p_)
                : base(p_)
            {
                _jzxTable = new JzxTable(p_);
            }
            public void init(ShapeFile dkShp, ShapeFile jzdShp, ShapeFile jzxShp)
            {
                _dkShp = dkShp;
                _jzdShp = jzdShp;
                _nKJZBField = dkShp.FindField("KJZB");
                int i = 0;
                _jzdDbfFieldIndex[0] = _jzdShp.FindField(JzdFields.BSM);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.YSDM);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.JZDH);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.JZDLX);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.JBLX);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.DKBM);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.XZBZ);
                _jzdDbfFieldIndex[++i] = _jzdShp.FindField(JzdFields.YZBZ);
                _jzxTable.init(jzxShp);
            }
            /// <summary>
            /// 获取生成的界址线的总数
            /// </summary>
            /// <returns></returns>
            public int GetExportedJzxCount()
            {
                return _jzxTable.ExportedCount;
            }
            public new void Clear()
            {
                base.Clear();
                _jzxTable.Clear();
                _nStartBSM = _p._param.nJzdBSMStartVal;
            }
            public void testLogout()
            {
                base.testLogout("界址点");
                _jzxTable.testLogout();
            }

            /// <summary>
            /// 保存界址点
            /// </summary>
            /// <param name="db"></param>
            /// <param name="initParam"></param>
            /// <param name="lst"></param>
            public void Save(ShortZd_cbd cbd)
            {
                if (!cbd.fSelected)
                    return;

                foreach (var en in cbd.lstJzdEntity)
                {
                    save(cbd, en);
                }

                string sKJZB = null;
                foreach (var en in cbd.lstJzdEntity)
                {
                    string s = null;

                    if (_p._jzdCache.TryGetValue(en.shape, out var val))
                    {
                        s = val.Jzdh;
                    }

                    if (s != null)
                    {
                        if (sKJZB == null)
                            sKJZB = s;
                        else if (sKJZB.Length + s.Length < 254)
                            sKJZB += "/" + s;
                    }
                }
                if (_nKJZBField >= 0)
                {
                    System.Diagnostics.Debug.Assert(sKJZB.Length < 254);
                    _dkShp.WriteFieldString(cbd.rowid, _nKJZBField, sKJZB);
                }

                _jzxTable.Save(cbd);
            }

            private void save(ShortZd_cbd cbd, JzdEntity jzdEn)
            {
                if (_p._param.fOnlyExportKeyJzd && jzdEn.SFKY == false)
                {
                    return;
                }
                if (_p._jzdCache.HasExported(jzdEn.shape))
                    return;

                if (_p._jzdCache.TryGetValue(jzdEn.shape, out var val))
                {
                    if (val.fHasExported)
                        return;

                    #region yxm 2021-4-15
                    foreach (var it in val)
                    {
                        jzdEn.Dks.Add(it.dk);
                    }
                    //if (jzdEn.Dks.Count > 1)
                    //{
                    //    Console.WriteLine($"jzdEn.Dks.Count={jzdEn.Dks.Count}");
                    //}
                    #endregion

                    val.fHasExported = true;
                }
                else
                {//程序没有写错的话不会走到这里来
                    System.Diagnostics.Debug.Assert(false);
                    //_p._jzdCache.AddPoint(jzdEn.shape);
                    return;
                }

                var shp = _jzdShp;
                string jzdh = _p._param.AddressPointPrefix + (1 + _nCurrShapeID);
                val.Jzdh = jzdh;
                var wkb = toWKB(jzdEn.shape);
                var x = Math.Round(jzdEn.shape.X, 3);
                var y = Math.Round(jzdEn.shape.Y, 3);
                shp.WriteWKB(-1, wkb);
                int i = -1;
                shp.WriteFieldInt(_nCurrShapeID, _jzdDbfFieldIndex[++i], _nStartBSM++);
                shp.WriteFieldString(_nCurrShapeID, _jzdDbfFieldIndex[++i], _p._param.sJzdYSDMVal);
                shp.WriteFieldString(_nCurrShapeID, _jzdDbfFieldIndex[++i], jzdh);
                shp.WriteFieldString(_nCurrShapeID, _jzdDbfFieldIndex[++i], _p._param.sJZDLXVal);
                shp.WriteFieldString(_nCurrShapeID, _jzdDbfFieldIndex[++i], _p._param.sJBLXVal);
                shp.WriteFieldString(_nCurrShapeID, _jzdDbfFieldIndex[++i], jzdEn.GetDkbm());
                shp.WriteFieldDouble(_nCurrShapeID, _jzdDbfFieldIndex[++i], y);//X 坐标、对应为投影坐标中的纵坐标。
                shp.WriteFieldDouble(_nCurrShapeID, _jzdDbfFieldIndex[++i], x);//Y 坐标、对应为投影坐标中的横坐标。
                ++_nCurrShapeID;
            }
            private byte[] toWKB(Coordinate c)
            {
                _tmpPoint.X = c.X;
                _tmpPoint.Y = c.Y;
                return _tmpPoint.AsBinary();
            }
        }

        /// <summary>
        /// 界址线表辅助类
        /// </summary>
        class JzxTable : TableBase
        {
            /// <summary>
            /// 界址线位置常量
            /// </summary>
            class JzxwzType
            {
                /// <summary>
                /// 内
                /// </summary>
                public const string Left = "1";
                /// <summary>
                /// 中
                /// </summary>
                public const string Middle = "2";
                /// <summary>
                /// 外
                /// </summary>
                public const string Right = "3";
            }

            private readonly List<JzdEntity> _points = new List<JzdEntity>();
            private readonly JzxEntity _jzxEn = new JzxEntity();
            private readonly HashSet<ShortZd_cbd> _qlrSet = new HashSet<ShortZd_cbd>();
            private readonly Dictionary<ShortZd_cbd, IGeometry> _polygonCache = new Dictionary<ShortZd_cbd, IGeometry>();

            /// <summary>
            /// 非正常释放的地块数
            /// </summary>
            private int _nTestReleasedDk = 0;
            private ShapeFile _jzxShp;
            private int[] _jzxDbfFieldIndex = new int[12];
            private int _nStartBSM = 0;
            public JzxTable(InitLandDotCoil p_)
                : base(p_)
            {
            }
            public void init(ShapeFile jzxShp)
            {
                _jzxShp = jzxShp;
                int i = 0;
                _jzxDbfFieldIndex[0] = _jzxShp.FindField(JzxFields.BSM);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.YSDM);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.JXXZ);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.JZXLB);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.JZXWZ);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.JZXSM);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.PLDWQLR);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.PLDWZJR);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.JZXH);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.DKBM);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.QJZDH);
                _jzxDbfFieldIndex[++i] = _jzxShp.FindField(JzxFields.ZJZDH);
            }
            public new void Clear()
            {
                base.Clear();
                _points.Clear();
                _qlrSet.Clear();
                _polygonCache.Clear();
                _nTestReleasedDk = 0;
                //_nCurrShapeID = 0;
                _nStartBSM = _p._param.nJzxBSMStartVal;
            }
            /// <summary>
            /// 保存界址线
            /// </summary>
            /// <param name="db"></param>
            /// <param name="initParam"></param>
            /// <param name="lst"></param>
            public void Save(ShortZd_cbd cbd)
            {
                var initParam = _p._param;
                int i = 0;
                queryPoints(cbd, points =>
                {
                    var jzxEn = _jzxEn;
                    jzxEn.Clear();
                    var jzdEnd = points[points.Count - 1];
                    var JZXCD = Math.Round(calcLength(cbd, points, out var shape, out var pld, out var fTwin), 2);
                    if (pld != null)
                    {
                        jzxEn.PLDWQLR = pld.qlrMc;// calcPldwQlr(cbd, points);
                        jzxEn.PLDWZJR = pld.zjrMc != null ? pld.zjrMc : pld.qlrMc;
                    }

                    if (string.IsNullOrEmpty(jzxEn.PLDWZJR))
                    {
                        jzxEn.PLDWZJR = initParam.PLDWZJR;
                    }
                    if (string.IsNullOrEmpty(jzxEn.PLDWQLR))
                    {
                        jzxEn.PLDWQLR = initParam.PLDWZJR;
                    }

                    if (fTwin == false)
                    {
                        jzxEn.JZXWZ = JzxwzType.Right;
                    }
                    else
                    {
                        if (pld != null)
                        {
                            if (cbd.elevation > 9000 || pld.elevation > 9000
                                || cbd.elevation == pld.elevation)
                            {
                                jzxEn.JZXWZ = JzxwzType.Middle;
                            }
                            else if (pld.elevation > cbd.elevation)
                            {
                                jzxEn.JZXWZ = JzxwzType.Left;
                            }
                            else
                            {
                                jzxEn.JZXWZ = JzxwzType.Right;
                            }
                        }
                        else
                        {
                            jzxEn.JZXWZ = JzxwzType.Middle;
                        }
                    }
                    if (string.IsNullOrEmpty(jzxEn.JZXWZ))
                    {
                        jzxEn.JZXWZ = initParam.JZXWZ;
                    }

                    jzxEn.Shape = shape;
                    if (_p._param.IsLineDescription)
                    {
                        jzxEn.JZXSM = JZXCD.ToString();// jzxEn.JZXCD.ToString();
                    }
                    //jzxEn.JXXZ = initParam.JXXZ;
                    //jzxEn.JZXLB = initParam.JZXLB;

                    save(jzxEn);
                    ++i;
                });
            }

            public void testLogout()
            {
                base.testLogout("界址线");
                Console.WriteLine("非正常释放的地块数：" + _nTestReleasedDk);
            }
            private void save(JzxEntity en)
            {
                if (_p._jzdCache.TryGetValue(en.Shape[0], out var val))
                {
                    if (val.lstJzxEntities == null)
                    {
                        val.lstJzxEntities = new List<Coordinate[]>
            {
              en.Shape
            };
                    }
                    else
                    {
                        bool fFind = false;
                        foreach (var je in val.lstJzxEntities)
                        {
                            if (isSame(en.Shape, je))
                            {
                                fFind = true;
                                break;
                            }
                        }
                        if (fFind)
                        {
                            return;
                        }
                        val.lstJzxEntities.Add(en.Shape);
                    }
                }
                else
                {//程序没有写错的话不会走到这里来
                    System.Diagnostics.Debug.Assert(false);
                }

                var shp = _jzxShp;
                var jzxh = (1 + _nCurrShapeID).ToString();

                string qjzdh = _p._jzdCache.QueryJzdh(en.Shape[0]);
                string zjzdh = _p._jzdCache.QueryJzdh(en.Shape[en.Shape.Length - 1]);
                var dkbm = _p._jzdCache.QueryDkbm(en);


                var wkb = toWKB(en.Shape);
                shp.WriteWKB(-1, wkb);
                int i = -1;
                shp.WriteFieldInt(_nCurrShapeID, _jzxDbfFieldIndex[++i], _nStartBSM++);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], _p._param.sJzxYSDMVal);// .sJzdYSDMVal);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], _p._param.JXXZ);// "J" + jzdh);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], _p._param.JZXLB);// jzdEn.JZDLX);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], en.JZXWZ);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], en.JZXSM);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], en.PLDWQLR);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], en.PLDWZJR);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], jzxh);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], dkbm);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], qjzdh);
                shp.WriteFieldString(_nCurrShapeID, _jzxDbfFieldIndex[++i], zjzdh);
                ++_nCurrShapeID;
            }
            private byte[] toWKB(Coordinate[] coords)
            {
                return GeometryUtil.MakeLineString(coords).AsBinary();
            }
            private bool isSame(Coordinate[] l1, Coordinate[] l2)
            {
                if (l1.Length != l2.Length)
                    return false;
                for (int i = 0; i < l1.Length; ++i)
                {
                    var c1 = l1[i];
                    var c2 = l2[i];
                    if (!CglUtil.IsSame2(c1, c2, _p._param.Tolerance, _p._param.Tolerance2))
                        return false;
                }
                return true;
            }

            private void queryPoints(ShortZd_cbd cbd, Action<List<JzdEntity>> callback)
            {
                _points.Clear();
                JzdEntity cBegin = null;
                for (int i = 0; i < cbd.lstJzdEntity.Count;)
                {
                    var jzdEn = cbd.lstJzdEntity[i];
                    if (cBegin == null)
                    {
                        cBegin = jzdEn;
                    }
                    _points.Add(jzdEn);
                    if (_points.Count == 1)
                    {
                        ++i;
                        continue;
                    }
                    if (jzdEn.SFKY && jzdEn.fRingLastPoint)
                    {
                        callback(_points);
                        _points.Clear();
                        _points.Add(jzdEn);
                        _points.Add(cBegin);
                        cBegin = null;
                        callback(_points);
                        _points.Clear();
                        ++i;
                        continue;
                    }
                    if (jzdEn.SFKY)
                    {
                        callback(_points);
                        _points.Clear();
                        continue;
                    }
                    if (jzdEn.fRingLastPoint)
                    {
                        _points.Add(cBegin);
                        cBegin = null;
                        callback(_points);
                        _points.Clear();
                    }
                    ++i;
                }
            }

            private double calcLength(ShortZd_cbd cbd, List<JzdEntity> lst, out Coordinate[] lineWkb, out ShortZd_cbd qlr, out bool fTwin)
            {
                _qlrSet.Clear();
                qlr = null;
                fTwin = false;

                var coords = new Coordinate[lst.Count];
                double len = 0;
                var pre = lst[0];
                coords[0] = pre.shape;
                for (int i = 1; i < lst.Count; ++i)
                {
                    #region 获取权利人
                    var jzx = findOutEdge(cbd, pre.shape);
                    if (jzx != null && jzx.lstQlr != null)
                    {
                        if (jzx.fQlrFind)
                        {
                            fTwin = true;
                        }
                        foreach (var q in jzx.lstQlr)
                        {
                            _qlrSet.Add(q);
                        }
                    }
                    #endregion

                    len += Math.Sqrt(CglUtil.GetDistance2(pre.shape, lst[i].shape));
                    coords[i] = lst[i].shape;
                    pre = lst[i];
                }
                var shape = GeometryUtil.MakeLineString(coords);

                if (coords[0].X > coords[coords.Length - 1].X
                    || coords[0].X == coords[coords.Length - 1].X && coords[0].Y > coords[coords.Length - 1].Y)
                {
                    GeometryUtil.Reverse(coords);
                }
                lineWkb = coords;

                if (_qlrSet.Count == 1)
                {
                    qlr = _qlrSet.First();//.qlrMc;
                }
                else if (_qlrSet.Count > 1)
                {
                    var s = onlyOneQlr(_qlrSet);
                    if (s != null)
                    {
                        qlr = s;
                    }
                    else
                    {
                        double maxLen = 0;
                        ShortZd_cbd maxLenDk = null;
                        var bp = new NetTopologySuite.Operation.Buffer.BufferParameters
                        {
                            EndCapStyle = GeoAPI.Operation.Buffer.EndCapStyle.Flat,
                            JoinStyle = GeoAPI.Operation.Buffer.JoinStyle.Bevel
                        };
                        var offLine = shape.Buffer(_p._param.AddressLinedbiDistance, bp);//,new GeoAPI.Operation.Buffer.IBufferParameters().JoinStyle. offsetLeft(lst);
                        foreach (var q in _qlrSet)
                        {
                            var g = getCachePolygon(q);//.MakePolygon();
                            if (g != null)
                            {
                                var oi = Topology.Intersect(g.AsBinary(), offLine.AsBinary());
                                if (oi != null)
                                {
                                    //var gi = WKBHelper.FromBytes(oi);

                                    if (WKBHelper.FromBytes(oi) is IGeometry gi)
                                    {
                                        var len1 = gi.Area;
                                        if (maxLenDk == null || maxLen < len1)
                                        {
                                            maxLenDk = q;
                                            maxLen = len1;
                                        }
                                    }
                                }
                            }
                        }

                        if (maxLenDk != null)
                        {
                            qlr = maxLenDk;//.qlrMc;
                        }
                    }
                }
                return len;
            }
            private Jzx findOutEdge(ShortZd_cbd cbd, Coordinate p)
            {
                if (_p._jzdCache.TryGetValue(p, out var edges))
                {
                    foreach (var je in edges)
                    {
                        if (je.dk == cbd)
                        {
                            return je.OutEdge;
                        }
                    }
                }
                return null;
            }
            private ShortZd_cbd onlyOneQlr(HashSet<ShortZd_cbd> lst)
            {
                var q0 = _qlrSet.First();
                foreach (var q in _qlrSet)
                {
                    if (q0 != q && q0.qlrMc != q.qlrMc)
                    {
                        return null;
                    }
                }
                return q0;//.qlrMc;
            }

            private IGeometry getCachePolygon(ShortZd_cbd cbd)
            {
                if (_polygonCache.TryGetValue(cbd, out var g))
                {
                    return g;
                }
                g = cbd.MakePolygon();
                if (_polygonCache.Count > 5000)
                {
                    _polygonCache.Clear();
                }
                if (g != null)
                {
                    _polygonCache[cbd] = g;
                }
                return g;
            }

        }

        /// <summary>
        /// 保存界址点和界址线的缓存
        /// </summary>
        class SaveCacheHelper
        {
            private readonly InitLandDotCoil p;
            private readonly List<ShortZd_cbd> _cacheList = new List<ShortZd_cbd>();
            private readonly List<ShortZd_cbd> _saveList = new List<ShortZd_cbd>();
            private readonly List<ShortZd_cbd> _releaseList = new List<ShortZd_cbd>();

            #region 确保至少4个界址点相关部分
            class JzdOutEdge
            {
                public JzdEntity jzd;
                /// <summary>
                /// 该界址点出度长度的平方
                /// </summary>
                public double len2;
                public JzdOutEdge(JzdEntity je, double len2_)
                {
                    jzd = je;
                    len2 = len2_;
                }
            }
            class MyList
            {
                //[JzdEntity,出度长的平方]
                public readonly List<JzdOutEdge> points = new List<JzdOutEdge>();
                public int nSplitIndex = 0;
                public double minLen2 = 0;
                /// <summary>
                /// 总长度
                /// </summary>
                public double sumLen2 = 0;
                public void Clear()
                {
                    nSplitIndex = 0;
                    minLen2 = 0;
                    sumLen2 = 0;
                }
            }

            /// <summary>
            /// 确保至少_param.MinKeyJzdCount个界址点
            /// </summary>
            /// <param name="lstJzd"></param>
            /// <param name="nKeyJzdCount"></param>
            /// <param name="MinKeyJzdCount"></param>
            public void Ensure4KeyJzd(List<JzdEntity> lstJzd, int nKeyJzdCount, int MinKeyJzdCount)
            {
                if (lstJzd.Count <= MinKeyJzdCount)
                {
                    foreach (var jzdEn in lstJzd)
                    {
                        if (!jzdEn.SFKY)
                        {
                            jzdEn.SFKY = true;
                            SetKeyJzd(jzdEn.shape);
                        }
                    }
                    return;
                }

                var ll = new List<MyList>();
                var lst = new MyList();

                for (int i = 0; i < lstJzd.Count; ++i)
                {
                    var jzd = lstJzd[i];
                    var nextJzd = lstJzd[i == lstJzd.Count - 1 ? 0 : (i + 1)];
                    var len2 = Math.Sqrt(CglUtil.GetDistance2(jzd.shape, nextJzd.shape));
                    var tpl = new JzdOutEdge(jzd, len2);
                    if (jzd.SFKY)
                    {
                        if (lst.points.Count > 0)
                        {
                            if (lst.points.Count > 1)
                            {
                                ll.Add(lst);
                                lst = new MyList();
                            }
                            else
                            {
                                lst.points.Clear();
                                lst.sumLen2 = 0;
                                lst.nSplitIndex = 0;
                                lst.minLen2 = 0;
                            }
                        }
                    }
                    lst.points.Add(tpl);
                    lst.sumLen2 += len2;
                }
                if (lst.points.Count > 0)
                {
                    ll.Add(lst);
                }
                while (true)
                {
                    MyList splitItem = null;
                    foreach (var ml in ll)
                    {
                        if (ml.nSplitIndex == 0)
                        {
                            double leftLen2 = 0;
                            for (int i = 0; i < ml.points.Count; ++i)
                            {
                                var p = ml.points[i];
                                leftLen2 += p.len2;
                                var rLen2 = ml.sumLen2 - leftLen2;
                                if (leftLen2 > rLen2)
                                {
                                    var pl2 = leftLen2 - p.len2;
                                    if (pl2 > rLen2)
                                    {
                                        ml.nSplitIndex = i;// i - 1;
                                        ml.minLen2 = pl2;
                                    }
                                    else
                                    {
                                        ml.nSplitIndex = Math.Min(i + 1, ml.points.Count - 1);
                                        ml.minLen2 = rLen2;
                                    }
                                    break;
                                }
                            }
                        }
                        if (splitItem == null || splitItem.minLen2 < ml.minLen2)
                        {
                            splitItem = ml;
                        }
                    }
                    if (splitItem == null)
                        break;
                    splitItem.points[splitItem.nSplitIndex].jzd.SFKY = true;
                    SetKeyJzd(splitItem.points[splitItem.nSplitIndex].jzd.shape);
                    ++nKeyJzdCount;
                    if (nKeyJzdCount >= MinKeyJzdCount)
                    {
                        break;
                    }
                    if (splitItem.points.Count - splitItem.nSplitIndex > 2)
                    {
                        var rml = new MyList();
                        for (int i = splitItem.nSplitIndex; i < splitItem.points.Count; ++i)
                        {
                            var spi = splitItem.points[i];
                            rml.points.Add(spi);
                            rml.sumLen2 += spi.len2;
                        }
                        ll.Add(rml);
                    }
                    for (int i = splitItem.points.Count - 1; i >= splitItem.nSplitIndex; --i)
                    {
                        var spi = splitItem.points[i];
                        splitItem.sumLen2 -= spi.len2;
                        splitItem.points.RemoveAt(i);
                    }
                    splitItem.nSplitIndex = 0;
                    splitItem.minLen2 = 0;
                    if (splitItem.points.Count < 3)
                    {
                        ll.Remove(splitItem);
                    }
                }
            }


            #endregion

            public SaveCacheHelper(InitLandDotCoil p_)
            {
                p = p_;
            }
            public void Add(ShortZd_cbd en)
            {
                System.Diagnostics.Debug.Assert(en.lstJzdEntity == null);
                System.Diagnostics.Debug.Assert(en.shell.Count > 0);

                #region 生成en.lstJzdEntity并将en加入到_cacheList
                en.lstJzdEntity = new List<JzdEntity>();
                var lstJzd = en.lstJzdEntity;
                toJzdList(en.shell, true, lstJzd);
                Assign(en, lstJzd, 0);
                if (en.holes != null)
                {
                    foreach (var h in en.holes)
                    {
                        int iBegin = lstJzd.Count;
                        toJzdList(h, false, lstJzd);
                        Assign(en, lstJzd, iBegin);
                    }
                }
                _cacheList.Add(en);
                #endregion

                if (_cacheList.Count >= 1000)
                {
                    for (int i = _cacheList.Count - 1; i >= 0; --i)
                    {
                        var c = _cacheList[i];
                        if (canSave(c))
                        {
                            //c.fTobeRemove = true;
                            _cacheList.RemoveAt(i);
                            _saveList.Add(c);
                        }
                    }
                }

                if (_saveList.Count >= 1000)
                {
                    Flush(false);
                }
            }

            public void Clear()
            {
                _cacheList.Clear();
                _saveList.Clear();
                _releaseList.Clear();
            }
            public void Flush(bool fLast)
            {
                if (fLast)
                {
                    foreach (var c in _cacheList)
                    {
                        _saveList.Add(c);
                    }
                    _cacheList.Clear();
                    Console.WriteLine("正在保存最后一个缓存：" + _saveList.Count + "个");
                }
                else
                {
                    Console.WriteLine("正在保存缓存：" + _saveList.Count + "个");
                }
                foreach (var c in _saveList)
                {
                    p._jzdTable.Save(c);//p._db, p._param, c.lstJzd, p._srid);
                    c.fSaved = true;
                    _releaseList.Add(c);
                }

                _saveList.Clear();

                if (_releaseList.Count >= 1000)
                {
                    tryRelease();
                }

                if (p._nTestMaxCacheJzdCount < p._jzdCache.Count)
                {
                    p._nTestMaxCacheJzdCount = p._jzdCache.Count;
                }
            }

            private void tryRelease()
            {
                for (int i = _releaseList.Count - 1; i >= 0; --i)
                {
                    var c = _releaseList[i];
                    bool fCanRelease = true;
                    if (c.lstNeibors != null)
                    {
                        foreach (var nb in c.lstNeibors)
                        {
                            if (!nb.fSaved)
                            {
                                fCanRelease = false;
                                break;
                            }
                        }
                    }
                    if (fCanRelease)
                    {
                        foreach (var jzdEn in c.lstJzdEntity)
                        {
                            p._jzdCache.Remove(jzdEn.shape);
                        }
                        c.Clear();
                        _releaseList.RemoveAt(i);
                    }
                }
            }

            /// <summary>
            /// 判断一个地块是否可以进行保存；
            /// </summary>
            /// <param name="cbd"></param>
            /// <returns></returns>
            private bool canSave(ShortZd_cbd cbd)
            {
                bool fRemove = true;
                if (cbd.lstNeibors != null)
                {
                    foreach (var nb in cbd.lstNeibors)
                    {
                        if (!nb.fRemovedFromCache)
                        {
                            fRemove = false;
                            break;
                        }
                    }
                }
                return fRemove;
            }

            /// <summary>
            /// 从iBegin开始为集合中的实体赋值
            /// </summary>
            /// <param name="lst"></param>
            /// <param name="iBegin"></param>
            /// <param name="iEnd"></param>
            private void Assign(ShortZd_cbd en, List<JzdEntity> lstJzd, int iBegin)
            {
                lstJzd[lstJzd.Count - 1].fRingLastPoint = true;
                var _param = p._param;
                int nKeyJzdCount = 0;//关键界址点的个数
                                     //short nJzdh = 0;//界址点点号
                                     //JzdEntity preJzd = lstJzd[lstJzd.Count - 1];
                for (int j = iBegin; j < lstJzd.Count; ++j)
                {
                    var jzdEn = lstJzd[j];

                    jzdEn.Dks.Add(en);//yxm 2021-4-15

                    if (jzdEn.SFKY != true)
                    {
                        #region 检查是否关键界址点
                        JzdEdges lst;
                        if (p._jzdCache.TryGetValue(jzdEn.shape, out lst))
                        {
                            if (lst.fKeyJzd == null)
                            {//判断是否关键界址点
                                if (lst.Count >= 3)
                                {
                                    lst.fKeyJzd = true;
                                }
                                else
                                {
                                    if (_param.MinAngleFileter != null)
                                    {
                                        if (lst.Count == 1)
                                        {
                                            var angle = CglUtil.CalcAngle(jzdEn.shape, lst[0].InEdge.qJzd, lst[0].OutEdge.zJzd);
                                            if (isKeyAngle(angle))
                                            {
                                                lst.fKeyJzd = true;
                                            }
                                        }
                                        else if (lst.Count == 2)
                                        {
                                            //Coordinate q0 = null, q1 = null;
                                            var p0 = lst[0].InEdge.qJzd;
                                            var p1 = lst[0].OutEdge.zJzd;
                                            //if (Less(p0, p1))
                                            //{
                                            //    p0 = p1;
                                            //    p1 = lst[0].InEdge.qJzd;
                                            //}
                                            var q0 = lst[1].InEdge.qJzd;
                                            var q1 = lst[1].OutEdge.zJzd;
                                            //if (Less(q0, q1))
                                            //{
                                            //    q0 = q1;
                                            //    q1 = lst[1].InEdge.qJzd;
                                            //}
                                            if (!p._jzdEqualComparer.Equals(p0, q1)
                                                || !p._jzdEqualComparer.Equals(p1, q0))
                                            {
                                                lst.fKeyJzd = true;
                                            }
                                            else
                                            {
                                                var angle = CglUtil.CalcAngle(jzdEn.shape, p0, p1);
                                                if (isKeyAngle(angle))
                                                {
                                                    lst.fKeyJzd = true;
                                                }
                                                else
                                                {
                                                    angle = CglUtil.CalcAngle(jzdEn.shape, q0, q1);
                                                    if (isKeyAngle(angle))
                                                    {
                                                        lst.fKeyJzd = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (lst.fKeyJzd == null)
                            {
                                lst.fKeyJzd = false;
                            }
                            jzdEn.SFKY = (bool)lst.fKeyJzd;
                        }
                        //preJzd = jzdEn;
                        #endregion
                    }
                    if (j == iBegin && jzdEn.SFKY == false)
                    {//总是将西北角开始的第一个点设置为关键界址点
                        jzdEn.SFKY = true;
                        SetKeyJzd(jzdEn.shape);
                    }
                    nKeyJzdCount += jzdEn.SFKY ? 1 : 0;
                }

                if (nKeyJzdCount < _param.MinKeyJzdCount)
                {
                    Ensure4KeyJzd(lstJzd, nKeyJzdCount, _param.MinKeyJzdCount);
                }
            }

            /// <summary>
            /// 判断角度是否可判断为关键界址点
            /// </summary>
            /// <param name="angle"></param>
            /// <returns></returns>
            private bool isKeyAngle(double angle)
            {
                return angle >= (double)p._param.MinAngleFileter
                                            && angle <= (double)p._param.MaxAngleFilter;
            }

            private void toJzdList(JzxRing r, bool fShell, List<JzdEntity> lst)
            {
                InitLandDotCoilUtil.SortCoordsByWNOrder(r, fShell, c =>
                {
                    var en = new JzdEntity();
                    lst.Add(en);
                    en.shape = c;
                });
            }

            /// <summary>
            /// 设置为关键界址点
            /// </summary>
            public void SetKeyJzd(Coordinate jzd)
            {
                if (!p._jzdCache.TryGetValue(jzd, out var lst))
                {
                    return;
                }
                if (lst.fKeyJzd != null && true == (bool)lst.fKeyJzd)
                {
                    return;
                }
                lst.fKeyJzd = true;
                foreach (var jzx in lst)
                {
                    if (jzx.dk.lstJzdEntity != null)
                    {
                        foreach (var jzdEn in jzx.dk.lstJzdEntity)
                        {
                            if (jzdEn.SFKY == false && p._jzdEqualComparer.Equals(jzdEn.shape, jzd))
                            {
                                jzdEn.SFKY = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 缓存界址点对应的出度的集合
        /// </summary>
        class JzdCache
        {
            private readonly InitLandDotCoil p;
            private readonly Dictionary<Coordinate, JzdEdges> _dicJzdOutEdges;

            public JzdCache(InitLandDotCoil p_)
            {
                p = p_;
                _dicJzdOutEdges = new Dictionary<Coordinate, JzdEdges>(p._jzdEqualComparer);
            }

            /// <summary>
            /// 从界址线中获取界址点
            /// </summary>
            /// <param name="r"></param>
            /// <param name="dicJzd"></param>
            private void AcquireJzd(JzxRing r)
            {
                var dicJzd = _dicJzdOutEdges;
                var preJzx = r[r.Count - 1];
                for (int i = 0; i < r.Count; ++i)
                {
                    var jzx = r[i];
                    var jzd = jzx.qJzd;
                    if (!dicJzd.TryGetValue(jzd, out var lst))
                    {
                        lst = new JzdEdges();
                        dicJzd[jzd] = lst;
                    }
                    lst.Add(new JzdEdge() { OutEdge = jzx, InEdge = preJzx });
                    preJzx = jzx;
                }
            }

            public void AcquireJzd(ShortZd_cbd c)
            {
                AcquireJzd(c.shell);
                if (c.holes != null)
                {
                    foreach (var h in c.holes)
                    {
                        AcquireJzd(h);
                    }
                }
            }
            public void ReAcquireJzd(ShortZd_cbd en)
            {
                if (en.lstJzdEntity != null)
                {
                    foreach (var c in en.lstJzdEntity)
                    {
                        if (_dicJzdOutEdges.TryGetValue(c.shape, out var jes))
                        {
                            for (int i = jes.Count - 1; i <= 0; --i)
                            {
                                var je = jes[i];
                                if (je.dk == en)
                                {
                                    jes.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
                AcquireJzd(en);
            }

            public void Remove(Coordinate jzd)
            {
                _dicJzdOutEdges.Remove(jzd);
            }
            public int Count
            {
                get { return _dicJzdOutEdges.Count; }
            }
            public bool TryGetValue(Coordinate jzd, out JzdEdges val)
            {
                return _dicJzdOutEdges.TryGetValue(jzd, out val);
            }
            public bool HasExported(Coordinate pt)
            {
                if (_dicJzdOutEdges.TryGetValue(pt, out var val))
                {
                    return val.fHasExported;
                }
                return false;
            }
            public Jzx findTwin(Jzx jzx)
            {
                for (int i = 0; i < 2; ++i)
                {
                    if (_dicJzdOutEdges.TryGetValue(i == 0 ? jzx.qJzd : jzx.zJzd, out var lst))
                    {
                        foreach (var j in lst)
                        {
                            if (jzx.dk != j.dk)
                            {
                                if (p._jzdEqualComparer.Equals(jzx.minYPoint, j.OutEdge.minYPoint)
                                    && p._jzdEqualComparer.Equals(jzx.maxYPoint, j.OutEdge.maxYPoint))
                                {
                                    return j.OutEdge;
                                }
                            }
                        }
                    }
                }
                return null;
            }

            public string QueryJzdh(Coordinate pt)
            {
                if (_dicJzdOutEdges.TryGetValue(pt, out var val))
                {
                    return val.Jzdh;
                }
                return null;
            }

            public string QueryDkbm(JzxEntity jzx)
            {
                var dks = new HashSet<string>();
                var pt = jzx.Shape[0];
                if (_dicJzdOutEdges.TryGetValue(pt, out var val))
                {
                    foreach (var it in val)
                    {
                        dks.Add(it.dk.DKBM);
                    }
                }
                string dkbm = null;
                pt = jzx.Shape[jzx.Shape.Length - 1];
                if (_dicJzdOutEdges.TryGetValue(pt, out val))
                {
                    foreach (var it in val)
                    {
                        if (dks.Contains(it.dk.DKBM))
                        {
                            if (dkbm == null)
                            {
                                dkbm = it.dk.DKBM;
                            }
                            else
                            {
                                dkbm += "/" + it.dk.DKBM;
                            }
                        }
                    }
                }

                return dkbm;
            }

            public void Clear()
            {
                _dicJzdOutEdges.Clear();
            }
        }

        private readonly InitLandDotCoilParam _param;

        private int _curProgress, _oldProgress;//进度相关
        private int _progressCount;// { get { return _rowids.Count; } }

        /// <summary>
        /// 打断线的点数
        /// </summary>
        private int _nInsertedJzdCount = 0;
        /// <summary>
        /// 集合_dicJzd中存有界址点的最大数量
        /// </summary>
        private int _nTestMaxCacheJzdCount = 0;
        /// <summary>
        /// 包含一个以上毗邻地权利人的界址线的个数
        /// </summary>
        private int _nGreat1QlrJzxCount = 0;


        internal readonly JzdEqualComparer _jzdEqualComparer;


        private readonly JzdTable _jzdTable;

        /// <summary>
        /// 承包地缓存
        /// </summary>
        private readonly ShortZd_cbdCache _cbdCache = new ShortZd_cbdCache();
        private readonly JzdCache _jzdCache;

        private readonly SaveCacheHelper _saveCache;
        private readonly XzdyUtil _xzdyUtil = new XzdyUtil();


        public Action<string, int> ReportProgress;
        public Action<string> ReportInfomation;
        /// <summary>
        /// 获取承包地的权利人名称
        /// </summary>
        public Func<ShortZd_cbd, string> OnQueryCbdQlr;
        public InitLandDotCoil(InitLandDotCoilParam param)
        {
            _param = param;
            _jzdEqualComparer = new JzdEqualComparer(_param.Tolerance);
            _saveCache = new SaveCacheHelper(this);
            _jzdCache = new JzdCache(this);
            _jzdTable = new JzdTable(this);
            _xzdyUtil.Init();
        }
        public void DoInit(string dkShapeFile, string jzdShapeFile, string jzxShapeFile)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (!File.Exists(jzdShapeFile))
            {
                throw new Exception("文件" + jzdShapeFile + "不存在！");
            }
            if (!File.Exists(jzxShapeFile))
            {
                throw new Exception("文件" + jzxShapeFile + "不存在！");
            }
            ShapeFileHelper.ClearShapeFile(jzdShapeFile);
            ShapeFileHelper.ClearShapeFile(jzxShapeFile);
            using (var dkShp = new ShapeFile())
            using (var jzdShp = new ShapeFile())
            using (var jzxShp = new ShapeFile())
            {
                dkShp.Open(dkShapeFile, "rb+");
                jzdShp.Open(jzdShapeFile, "rb+");
                jzxShp.Open(jzxShapeFile, "rb+");
                DoInit(dkShp, jzdShp, jzxShp);
            }
            sw.Stop();
            ReportInfomation("总耗时：" + sw.Elapsed);

        }
        /// <summary>
        /// 开始初始化界址点和界址线
        /// </summary>
        /// <param name="wh"></param>
        private void DoInit(ShapeFile dkShp, ShapeFile jzdShp, ShapeFile jzxShp)//string wh)
        {
            //Console.WriteLine("开始时间：" + DateTime.Now);
            ReportInfomation("开始时间：" + DateTime.Now);
            //_rowids.Clear();
            _curProgress = 0;
            _oldProgress = 0;
            _nInsertedJzdCount = 0;
            _nGreat1QlrJzxCount = 0;
            _nTestMaxCacheJzdCount = 0;
            _jzdCache.Clear();
            //_lstSaveCache.Clear();
            _jzdTable.Clear();
            _cbdCache.Clear();
            _saveCache.Clear();
            _jzdTable.init(dkShp, jzdShp, jzxShp);

            int nDkShpCount = dkShp.GetRecordCount();

            _progressCount = nDkShpCount;
            var lstXBounds = new List<XBounds>();

            ReportInfomation("已选择地块个数共计：" + nDkShpCount + "个");

            var env = dkShp.GetFullExtent();
            var icc = new IntCoordConter();
            icc.Init(env.MinX);
            for (int i = 0; i < nDkShpCount; ++i)
            {
                var x = new XBounds
                {
                    rowid = i
                };
                var wkb = dkShp.GetWKB(i, false);
                if (wkb == null) continue;
                var e = WKBHelper.FromBytes(wkb).EnvelopeInternal;
                x.minx = icc.toInt(e.MinX);
                x.maxx = icc.toInt(e.MaxX);
                lstXBounds.Add(x);
            }
            lstXBounds.Sort((a, b) =>
            {
                if (a.minx < b.minx)
                    return -1;
                if (a.minx > b.minx)
                    return 1;
                if (a.maxx == b.maxx)
                    return 0;
                return a.maxx < b.maxx ? -1 : 1;
            });

            int nTestMaxCacheSize = 0;

            var cacheRowids = new List<int>();
            var dicCbd = new Dictionary<int, ShortZd_cbd>();
            ShortZd_cbd preCbd = null;
            for (int i = 0; i < lstXBounds.Count;)
            {
                int j = i + 100;
                if (j >= lstXBounds.Count)
                {
                    j = lstXBounds.Count - 1;
                }
                for (int k = i; k <= j; ++k)
                {
                    cacheRowids.Add(lstXBounds[k].rowid);
                }
                InitLandDotCoilUtil.QueryShortZd_cbd(dkShp, cacheRowids, dicCbd, en =>
                {
                    en.qlrMc = OnQueryCbdQlr(en);
                    _jzdCache.AcquireJzd(en);
                });
                for (int k = i; k <= j; ++k)
                {
                    var xb = lstXBounds[k];
                    if (dicCbd.TryGetValue(xb.rowid, out var cbd))
                    {
                        var fProcessed = processLeft(cbd, preCbd);
                        if (fProcessed)
                        {
                            preCbd = cbd;
                        }
                        if (_cbdCache.Count > nTestMaxCacheSize)
                        {
                            nTestMaxCacheSize = _cbdCache.Count;
                        }
                    }
                }
                dicCbd.Clear();
                cacheRowids.Clear();
                i = j + 1;
            }
            processLeft(null, null);

            ReportProgress("保存缓存数据", 100);
            _saveCache.Flush(true);



            _jzdTable.testLogout();


            Console.WriteLine("共发现插入点" + _nInsertedJzdCount + "个");
            Console.WriteLine("包含一个以上毗邻地权利人的界址线个数是：" + _nGreat1QlrJzxCount);
            Console.WriteLine("nTestMaxCacheSize=" + nTestMaxCacheSize);
            Console.WriteLine("缓存界址点的最大数量为：" + Math.Max(_jzdCache.Count, _nTestMaxCacheJzdCount));


            ReportInfomation("生成界址点共计：" + _jzdTable.ExportedCount + "个");
            ReportInfomation("生成界址线共计：" + _jzdTable.GetExportedJzxCount() + "条");
            ReportInfomation("结束时间：" + DateTime.Now);
        }
        private bool processLeft(ShortZd_cbd current, ShortZd_cbd preCbd)
        {
            var left = _cbdCache;
            if (current != null)
            {
                if (left.Count == 0)
                {
                    left.Add(current);
                    left.x1 = current.xmax;
                    return false;
                }
                left.Add(current);
                if (left.x1 > current.xmax)
                {
                    left.x1 = current.xmax;
                }
                if (left.x1 + _param.AddressLinedbiDistance > current.xmin)
                {
                    return false;
                }
            }
            var tolerance = _param.Tolerance;
            var tolerance2 = tolerance * tolerance;

            var lstSortedJzx = new List<Jzx>();
            buildSortedJzxList(left, lstSortedJzx);

            #region 打断界址线
            var lstSortedJzd = new SortedSet<Coordinate>(new JzdComparer(tolerance));
            foreach (var cbd in left)
            {
                cbd.QueryPoint(c =>
                {
                    lstSortedJzd.Add(c);
                });
            }

            foreach (var pt in lstSortedJzd)
            {
                if (InitLandDotCoilUtil.testIsEqual(pt, 37451621.64925243, 3379330.0265524415))
                {
                    Console.WriteLine(pt.ToString());
                }

                for (int i = lstSortedJzx.Count; --i >= 0;)
                {
                    var jzx = lstSortedJzx[i];

                    var zJzd = jzx.maxYPoint;//.zJzd;

                    //if (InitLandDotCoilUtil.testIsEqual(zJzd, 37451623.866125114,3379352.4266991792)
                    //        && InitLandDotCoilUtil.testIsEqual(pt, 37451621.64925243, 3379330.0265524415))
                    //{
                    //    Console.WriteLine(pt.ToString());
                    //}

                    if (pt.Y > zJzd.Y + tolerance)
                    {
                        lstSortedJzx.RemoveAt(i);
                        continue;
                    }
                    var qJzd = jzx.minYPoint;
                    if (pt.Y < qJzd.Y - tolerance)
                    {
                        break;
                    }

                    var minX = qJzd.X;
                    var maxX = zJzd.X;
                    if (minX > maxX)
                    {
                        minX = zJzd.X;
                        maxX = qJzd.X;
                    }
                    if (pt.X >= minX && pt.X <= maxX && pt.Y >= qJzd.Y && pt.Y <= zJzd.Y)
                    {
                        if (CglUtil.IsSame2(pt, qJzd, tolerance2))
                        {
                            continue;
                        }
                        if (CglUtil.IsSame2(pt, zJzd, tolerance2))
                        {
                            continue;
                        }
                        if (CglUtil.IsPointOnLine(qJzd, zJzd, pt, tolerance2))
                        {
                            var p1 = CglUtil.GetProjectionPoint(qJzd, zJzd, pt);

                            //if (InitLandDotCoilUtil.testIsEqual(p1, 37451621.64925243,3379330.0265524415))
                            //{
                            //	Console.WriteLine(pt.ToString());
                            //}

                            if (!Overlaps(jzx.lstInsertJzd, p1))
                            {
                                jzx.lstInsertJzd.Add(p1);
                                ++_nInsertedJzdCount;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < left.Count; ++i)
            {
                var cbd = left[i];
                bool fRebuild = cbd.shell.RebuildRing();
                int nRebuildCount = fRebuild ? 1 : 0;
                if (cbd.holes != null)
                {
                    foreach (var r in cbd.holes)
                    {
                        fRebuild = r.RebuildRing();
                        nRebuildCount += fRebuild ? 1 : 0;
                    }
                }
                if (nRebuildCount > 0)
                {//若该地块的边上有插入点则保存地块的shape数据
                    _jzdCache.ReAcquireJzd(cbd);
                }
            }
            #endregion


            #region 查找毗邻地权利人
            buildSortedJzxList(left, lstSortedJzx);
            int nTestJzxCount = lstSortedJzx.Count;
            for (int i = lstSortedJzx.Count - 1; i >= 0; --i)
            {
                var jzx = lstSortedJzx[i];

                #region 查找相邻的地块（外接矩形在1.5米范围内相交的地块）
                for (int j = lstSortedJzx.Count - 1; j >= 0; --j)
                {
                    if (j == i)
                        continue;
                    var jzxJ = lstSortedJzx[j];
                    if (jzxJ.maxYPoint.Y + _param.AddressLinedbiDistance < jzx.minYPoint.Y)
                    {
                        lstSortedJzx.RemoveAt(j);
                        continue;
                    }
                    if (jzx.maxYPoint.Y + _param.AddressLinedbiDistance < jzxJ.minYPoint.Y)
                    {
                        break;
                    }
                    double minx = jzxJ.qJzd.X, maxx = jzxJ.zJzd.X;
                    if (jzxJ.qJzd.X > jzxJ.zJzd.X)
                    {
                        minx = jzxJ.zJzd.X;
                        maxx = jzxJ.qJzd.X;
                    }
                    double minxi = jzx.qJzd.X, maxxi = jzx.zJzd.X;
                    if (minxi > maxxi)
                    {
                        minxi = maxxi;
                        maxxi = jzx.qJzd.X;
                    }
                    if (!(maxxi + _param.AddressLinedbiDistance < minx
                        || maxx + _param.AddressLinedbiDistance < minxi))
                    {
                        if (jzx.dk.lstNeibors == null)
                        {
                            jzx.dk.lstNeibors = new HashSet<ShortZd_cbd>();
                        }
                        jzx.dk.lstNeibors.Add(jzxJ.dk);
                    }
                }
                #endregion
                //if(InitLandDotCoilUtil.testIsEqual(jzx.qJzd,452091.232727, 3484712.010925)
                //    && InitLandDotCoilUtil.testIsEqual(jzx.qJzd, 452091.232727, 3484712.010925))
                //{
                //    Console.WriteLine("ok");
                //}

                if (jzx.fQlrFind)
                {
                    continue;
                }

                var twin = _jzdCache.findTwin(jzx);
                if (twin != null)
                {
                    if (jzx.lstQlr == null)
                    {
                        jzx.lstQlr = new List<ShortZd_cbd>();
                    }
                    jzx.lstQlr.Clear();
                    jzx.lstQlr.Add(twin.dk);

                    //System.Diagnostics.Debug.Assert(jzx != twin);
                    jzx.fQlrFind = true;
                    if (twin.lstQlr == null)
                    {
                        twin.lstQlr = new List<ShortZd_cbd>();
                    }
                    twin.lstQlr.Clear();
                    twin.lstQlr.Add(jzx.dk);
                    twin.fQlrFind = true;
                    continue;
                }
                var fSelected = jzx.dk.fSelected;
                var buffer = InitLandDotCoilUtil.BufferLeft(jzx.qJzd, jzx.zJzd, _param.AddressLinedbiDistance);
                var env = buffer.EnvelopeInternal;
                for (int j = lstSortedJzx.Count - 1; j >= 0; --j)
                {
                    if (j == i)
                        continue;
                    var jzxJ = lstSortedJzx[j];
                    if (jzxJ.maxYPoint.Y + _param.AddressLinedbiDistance < jzx.minYPoint.Y)
                    {
                        System.Diagnostics.Debug.Assert(false);//在查找相邻的地块处已经处理了，应该不会走到这里
                        lstSortedJzx.RemoveAt(j);
                        continue;
                    }
                    if (jzx.maxYPoint.Y + _param.AddressLinedbiDistance < jzxJ.minYPoint.Y)
                    {
                        break;
                    }
                    if (jzx.dk == jzxJ.dk)
                        continue;
                    if (jzx.lstQlr != null && jzx.lstQlr.Contains(jzxJ.dk))
                    {
                        continue;
                    }
                    double minx = jzxJ.qJzd.X, maxx = jzxJ.zJzd.X;
                    if (jzxJ.qJzd.X > jzxJ.zJzd.X)
                    {
                        minx = jzxJ.zJzd.X;
                        maxx = jzxJ.qJzd.X;
                    }
                    if (maxx < env.MinX || minx > env.MaxX)
                        continue;
                    if (buffer.Intersects(GeometryUtil.MakeLineString(jzxJ.qJzd, jzxJ.zJzd)))
                    {
                        if (jzx.lstQlr == null)
                        {
                            jzx.lstQlr = new List<ShortZd_cbd>();
                        }
                        jzx.lstQlr.Add(jzxJ.dk);

                        if (jzxJ.fQlrFind == false)
                        {
                            if (jzxJ.lstQlr == null)
                            {
                                jzxJ.lstQlr = new List<ShortZd_cbd>
                {
                  jzx.dk
                };
                            }
                            else if (!jzxJ.lstQlr.Contains(jzx.dk))
                            {
                                jzxJ.lstQlr.Add(jzx.dk);
                            }
                        }
                    }
                }
                if (jzx.lstQlr != null && jzx.lstQlr.Count > 1)
                {
                    ++_nGreat1QlrJzxCount;
                }
            }
            #endregion

            #region 将_cbdCache中所有在当前地块最左边1.5米前完全出现的地块加入到保存缓存中并从_cbdCache中移除
            double? x1 = null;
            for (int i = left.Count - 1; i >= 0; --i)
            {
                var cbd = left[i];
                if (current == null || cbd.xmax + _param.AddressLinedbiDistance < current.xmin)
                {
                    if (cbd.fSelected)
                    {
                        ++_curProgress;
                        reportProgress();
                    }

                    left.RemoveAt(i);
                    cbd.fRemovedFromCache = true;
                    _saveCache.Add(cbd);
                }
                else
                {
                    if (x1 == null || (double)x1 < cbd.xmax)
                    {
                        x1 = cbd.xmax;
                    }
                }
            }
            if (x1 != null)
            {
                left.x1 = (double)x1;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// 用cache中的地块数据构建一个按y方向降序排列的界址线集合，
        /// 该方法会首先清空lstSortedJzx中的数据；
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="lstSortedJzx"></param>
        private void buildSortedJzxList(ShortZd_cbdCache cache, List<Jzx> lstSortedJzx)
        {
            lstSortedJzx.Clear();
            foreach (var cbd in cache)
            {
                if (cbd.shell.Count == 0)
                {
                    System.Diagnostics.Debug.Assert(cbd.shell.Count > 0);
                    throw new Exception("内部错误");
                }
                foreach (var jzx in cbd.shell)
                {
                    lstSortedJzx.Add(jzx);
                }
                if (cbd.holes != null)
                {
                    foreach (var h in cbd.holes)
                    {
                        foreach (var jzx in h)
                        {
                            lstSortedJzx.Add(jzx);
                        }
                    }
                }
            }
            lstSortedJzx.Sort((a, b) =>
            {//按Y轴优先排序
                if (a.minYPoint.Y > b.minYPoint.Y)
                    return -1;
                if (a.minYPoint.Y < b.minYPoint.Y)
                    return 1;
                return a.minYPoint.X < b.minYPoint.X ? -1 : 1;
            });
        }
        /// <summary>
        /// 判断集合lst中的点是否存在于c重叠的点；
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool Overlaps(List<Coordinate> lst, Coordinate c)
        {
            var cmp = new JzdComparer(_param.Tolerance);
            foreach (var c0 in lst)
            {
                if (cmp.Compare(c0, c) == 0)
                    return true;
            }
            return false;
        }

        private void reportProgress(string msg = "初始化界址点线")
        {
            if (ReportProgress != null)
            {
                ProgressHelper.ReportProgress(ReportProgress, msg, _progressCount, _curProgress, ref _oldProgress);
            }
        }
    }
    /// <summary>
    /// 初始化界址点界址线的参数类
    /// </summary>
    public class InitLandDotCoilParam
    {
        /// <summary>
        /// 领宗地距离，单位：米
        /// </summary>
        public double AddressLinedbiDistance = 1.5;
        internal readonly double Tolerance = 0.05;//视为同一个点的距离容差，单位：米
        internal readonly double Tolerance2;
        /// <summary>
        /// 界址标识
        /// </summary>
        public string AddressPointPrefix = "";// "J";
        /// <summary>
        /// 最小过滤角度值，单位：度
        /// </summary>
        public double? MinAngleFileter = 5;

        /// <summary>
        /// 最大过滤角度值，单位：度
        /// </summary>
        public double? MaxAngleFilter = 120;

        /// <summary>
        /// 一个地块包含的最少关键界址点个数
        /// </summary>
        public short MinKeyJzdCount = 4;

        ///// <summary>
        ///// 界址点类型
        ///// </summary>
        //public short AddressDotType=1;

        ///// <summary>
        ///// 界标类型
        ///// </summary>
        //public short AddressDotMarkType=3;

        /// <summary>
        /// 界线性质
        /// </summary>
        public string JXXZ = "600001";

        /// <summary>
        /// 界址线类型
        /// </summary>
        public string JZXLB = "01";

        /// <summary>
        /// 界址线位置
        /// </summary>
        public string AddressLinePosition = "1";

        /// <summary>
        /// 界址线说明填写长度
        /// </summary>
        public bool IsLineDescription = true;
        ///// <summary>
        ///// 创建时间
        ///// </summary>
        //public DateTime cjsj;

        /// <summary>
        /// 界址点标识码初始值
        /// </summary>
        public int nJzdBSMStartVal = 50000000;

        /// <summary>
        /// 界址线标识码初始值
        /// </summary>
        public int nJzxBSMStartVal = 20000000;

        /// <summary>
        /// 界址点要素代码
        /// </summary>
        public string sJzdYSDMVal = "211021";

        /// <summary>
        /// 界址线要素代码
        /// </summary>
        public string sJzxYSDMVal = "211031";

        /// <summary>
        /// 界标类型
        /// </summary>
        public string sJBLXVal = "3";
        /// <summary>
        /// 界址点类型
        /// </summary>
        public string sJZDLXVal = "1";

        /// <summary>
        /// 毗邻地物指界人
        /// </summary>
        public string PLDWZJR;
        /// <summary>
        /// 界址线位置
        /// </summary>
        public string JZXWZ;

        /// <summary>
        /// 是否只导出关键界址点
        /// </summary>
        public bool fOnlyExportKeyJzd = true;

        public InitLandDotCoilParam(double tolerance = 0.05)
        {
            Tolerance = tolerance;
            Tolerance2 = tolerance * tolerance;
        }
    }
    public class JzdEdge
    {
        /// <summary>
        /// 出度
        /// </summary>
        public Jzx OutEdge;
        /// <summary>
        /// 入度
        /// </summary>
        public Jzx InEdge;
        public ShortZd_cbd dk { get { return OutEdge.dk; } }
    }
    public class JzdEdges : List<JzdEdge>
    {
        /// <summary>
        /// 界址点号
        /// </summary>
        public string Jzdh;
        /// <summary>
        /// 是否关键界址点 
        /// </summary>
        public bool? fKeyJzd = null;
        public bool fHasExported = false;
        public List<Coordinate[]> lstJzxEntities = null;// new List<JzxEntity>();
    }

    /// <summary>
    /// 用整数表示坐标值
    /// </summary>
    public class IntCoordConter
    {
        private double _minValue;
        public void Init(double minValue)
        {
            _minValue = minValue;
        }
        public int toInt(double value)
        {
            return (int)((value - _minValue) * 10000);
        }
        public double toDouble(int value)
        {
            return value / 10000.0 + _minValue;
        }
    }

    /// <summary>
    /// 记录每个地块的外切横坐标
    /// </summary>
    public struct XBounds
    {
        public int rowid;
        public int minx;
        public int maxx;
    }

    /// <summary>
    /// 记录简要信息的承包地类
    /// </summary>
    public class ShortZd_cbd
    {
        private short _bit = 0;
        public bool fRemovedFromCache
        {
            get
            {
                return (_bit & 1) == 1;
            }
            set
            {
                if (value)
                {
                    _bit |= 1;
                }
                else
                {
                    _bit &= 0xFE;
                }
            }
        }
        /// <summary>
        /// 是否被用户选中要生成界址点和界址线的地块
        /// </summary>
        public bool fSelected
        {
            get
            {
                return true;
            }
        }

        public bool fSaved
        {
            get
            {
                return (_bit & (1 << 2)) != 0;
            }
            set
            {
                if (value)
                {
                    _bit |= 1 << 2;
                }
                else
                {
                    _bit &= ~(1 << 2);
                }
            }
        }


        public int rowid;
        public double xmin;
        public double xmax;
        /// <summary>
        /// 权利人名称
        /// </summary>
        internal string qlrMc;
        /// <summary>
        /// 指界人名称
        /// </summary>
        public string zjrMc;
        /// <summary>
        /// 地块编码
        /// </summary>
        public string DKBM;
        /// <summary>
        /// 地块名称
        /// </summary>
        public string DKMC;

        /// <summary>
        /// 海拔
        /// </summary>
        internal double elevation = 10000;

        public JzxRing shell = new JzxRing();
        /// <summary>
        /// may by null
        /// </summary>
        public List<JzxRing> holes = null;

        /// <summary>
        /// 界址点实体
        /// </summary>
        public List<JzdEntity> lstJzdEntity = null;

        /// <summary>
        /// 与该地块相邻的所有地块（近似判断）；
        /// </summary>
        public HashSet<ShortZd_cbd> lstNeibors = null;
        public void QueryPoint(Action<Coordinate> callback)
        {
            foreach (var j in shell)
            {
                callback(j.qJzd);
            }
            if (holes != null)
            {
                foreach (var h in holes)
                {
                    foreach (var j in h)
                    {
                        callback(j.qJzd);
                    }
                }
            }
        }

        /// <summary>
        /// 遍历环
        /// </summary>
        /// <param name="callback">【环，是否shell】</param>
        public void QueryRing(Action<JzxRing, bool> callback)
        {
            callback(shell, true);
            if (holes != null)
            {
                foreach (var h in holes)
                {
                    callback(h, false);
                }
            }
        }
        public IPolygon MakePolygon()//int srid=-1)
        {
            if (shell.Count == 0)
                return null;
            ILinearRing[] rs = null;
            if (holes != null && holes.Count > 0)
            {
                rs = new ILinearRing[holes.Count];
                for (int i = 0; i < holes.Count; ++i)
                {
                    rs[i] = holes[i].MakeLinearRing();
                }
            }
            return GeometryUtil.MakePolygon(shell.MakeLinearRing(), rs);//,srid);
        }

        /// <summary>
        /// 清理，避免循环引用
        /// </summary>
        public void Clear()
        {
            if (fRemovedFromCache)
            {
                System.Diagnostics.Debug.Assert(shell.Count > 0);
                System.Diagnostics.Debug.Assert(fSaved == true);
                shell.clear();
                if (holes != null)
                {
                    foreach (var h in holes)
                    {
                        h.Clear();
                    }
                    holes.Clear();
                    holes = null;
                }
                if (lstJzdEntity != null)
                {
                    lstJzdEntity.Clear();
                    lstJzdEntity = null;
                }
                if (lstNeibors != null)
                {
                    lstNeibors.Clear();
                    lstNeibors = null;
                }
            }
        }
    }

    /// <summary>
    /// 如果是shell则按顺时针方向排列否则按逆时针方向排列
    /// </summary>
    public class JzxRing : List<Jzx>
    {
        /// <summary>
        /// 重塑环：根据每一条边上的插入点打断界址线；
        /// 如果没有插入点则返回false;
        /// </summary>
        public bool RebuildRing()
        {
            if (!hasIntsertedJzd())
                return false;
            var lst = new List<Jzx>();
            var t1 = new List<Jzx>();
            foreach (var jzx in this)
            {
                if (jzx.lstInsertJzd.Count > 0)
                {
                    jzx.GetSplitJzxs(t1);
                    foreach (var j in t1)
                    {
                        lst.Add(j);
                    }
                }
                else
                {
                    lst.Add(jzx);
                }
            }
            Clear();
            foreach (var j in lst)
            {
                Add(j);
            }
            return true;
        }
        public void clear()
        {
            foreach (var jzx in this)
            {
                jzx.Clear();
            }
            base.Clear();
        }
        /// <summary>
        /// 构造为Ｎts类型的环
        /// </summary>
        /// <returns></returns>
        public ILinearRing MakeLinearRing()
        {
            var coords = new Coordinate[Count + 1];
            for (int i = 0; i < Count; ++i)
            {
                coords[i] = this[i].qJzd;
            }
            coords[Count] = coords[0];
            return GeometryUtil.MakeLinearRing(coords);
        }
        private bool hasIntsertedJzd()
        {
            foreach (var jzx in this)
            {
                if (jzx.lstInsertJzd.Count > 0)
                    return true;
            }
            return false;
        }

    }

    /// <summary>
    /// 承包地缓存
    /// </summary>
    public class ShortZd_cbdCache : List<ShortZd_cbd>
    {
        public double x1;
    }



    /// <summary>
    /// //按Y轴优先排序
    /// </summary>
    public class JzdComparer : Comparer<Coordinate>
    {
        private double _tolerace;
        public JzdComparer(double tolerace)
        {
            _tolerace = tolerace;
        }
        public override int Compare(Coordinate a, Coordinate b)
        {
            if (a.Y + _tolerace < b.Y)
                return -1;
            if (b.Y + _tolerace < a.Y)
                return 1;
            if (a.X + _tolerace < b.X)
                return -1;
            if (b.X + _tolerace < a.X)
                return 1;
            return 0;
        }
    }

    /// <summary>
    /// 点相等的比较
    /// </summary>
    public class JzdEqualComparer : IEqualityComparer<Coordinate>
    {
        private double _tolerace, _tolerace2;
        private Coordinate _tmpC = new Coordinate();
        public JzdEqualComparer(double tolerance)
        {
            _tolerace = tolerance;
            _tolerace2 = tolerance * tolerance;
        }
        public bool Equals(Coordinate a, Coordinate b)
        {
            return CglUtil.IsSame2(a, b, _tolerace2);
        }

        public int GetHashCode(Coordinate obj)
        {
            _tmpC.X = func(obj.X);
            _tmpC.Y = func(obj.Y);
            return _tmpC.GetHashCode();
        }
        private static double func(double x)
        {
            long n = (long)(x * 100);
            return n / 100.0;
        }
    }
    public class Jzx
    {
        /// <summary>
        /// 是否起界址点的y值小于止界址点的y值
        /// </summary>
        private readonly bool _isQjzdYLess;
        /// <summary>
        /// 所属地块
        /// </summary>
        public ShortZd_cbd dk;
        public readonly bool isShell;
        /// <summary>
        /// 起界址点ID（对应Jzd.id）
        /// </summary>
        public readonly Coordinate qJzd;
        /// <summary>
        /// 止界址点ID（对应Jzd.id）
        /// </summary>
        public readonly Coordinate zJzd;
        public Coordinate minYPoint
        {
            get
            {
                return _isQjzdYLess ? qJzd : zJzd;
            }
        }
        public Coordinate maxYPoint
        {
            get
            {
                return _isQjzdYLess ? zJzd : qJzd;
            }
        }
        /// <summary>
        /// 毗邻地权利人
        /// </summary>
        public List<ShortZd_cbd> lstQlr = null;
        public bool fQlrFind = false;
        public Jzx(Coordinate p0, Coordinate p1, ShortZd_cbd dk_, bool fShell)
        {
            qJzd = p0;
            zJzd = p1;
            dk = dk_;
            isShell = fShell;
            _isQjzdYLess = p0.Y < p1.Y;
            if (p0.Y == p1.Y)
            {
                _isQjzdYLess = p0.X < p1.X;
            }
        }
        public string GetQlr()
        {
            string s = "";
            if (lstQlr != null)
            {
                foreach (var q in lstQlr)
                {
                    if (!string.IsNullOrEmpty(s))
                        s += ";";
                    s += q.qlrMc;
                }
            }
            return s;
        }
        /// <summary>
        /// 保存从要打断的界址点ID
        /// </summary>
        public readonly List<Coordinate> lstInsertJzd = new List<Coordinate>();
        /// <summary>
        /// 根据 lstInsertJzd中的数据获取打断后的界址线数据；
        /// <param name="lst">会首先清空里面的数据</param>
        /// </summary>
        public List<Jzx> GetSplitJzxs(List<Jzx> lst, bool fClearLstInsertJzd = true)
        {
            lst.Clear();
            if (lstInsertJzd.Count == 0)
            {
                lst.Add(this);
                return lst;
            }
            if (lstInsertJzd.Count > 1)
            {
                if (qJzd.X < zJzd.X)
                {
                    lstInsertJzd.Sort((a, b) =>
                    {
                        return a.X < b.X ? -1 : 1;
                    });
                }
                else if (qJzd.X > zJzd.X)
                {
                    lstInsertJzd.Sort((a, b) =>
                    {
                        return a.X > b.X ? -1 : 1;
                    });
                }
                else if (qJzd.Y < zJzd.Y)
                {
                    lstInsertJzd.Sort((a, b) =>
                    {
                        return a.Y < b.Y ? -1 : 1;
                    });
                }
                else
                {
                    lstInsertJzd.Sort((a, b) =>
                    {
                        return a.Y > b.Y ? -1 : 1;
                    });
                }
            }
            lstInsertJzd.Add(zJzd);
            var preJzd = qJzd;
            for (int i = 0; i < lstInsertJzd.Count; ++i)
            {
                var p1 = lstInsertJzd[i];
                lst.Add(new Jzx(preJzd, p1, dk, isShell));
                preJzd = p1;
            }
            if (fClearLstInsertJzd)
            {
                lstInsertJzd.Clear();
            }
            else
            {
                lstInsertJzd.RemoveAt(lstInsertJzd.Count - 1);
            }
            return lst;
        }

        /// <summary>
        /// 清理，避免循环引用
        /// </summary>
        public void Clear()
        {
            dk = null;
            if (lstQlr != null)
            {
                lstQlr.Clear();
                lstQlr = null;
            }
            lstInsertJzd.Clear();
        }
    }
}
