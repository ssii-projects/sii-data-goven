using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Agro.LibMapServer;

namespace DataGovenServer.Service.LandDotCoil
{
	public class DotCoilBuilder
	{
		class JzdRing : List<JzdItem>
		{

		}
		private readonly JzdEqualComparer _jzdEqualComp;// = new JzdEqualComparer(0.05);
		private readonly DotCoilBuilderParam _prm;
		private readonly List<DkItem> _lstTmpAroundDk = new List<DkItem>();
		private readonly HashSet<string> _tmpSetQlr = new HashSet<string>();
		private readonly HashSet<string> _tmpSetZjr = new HashSet<string>();

		public DotCoilBuilder(DotCoilBuilderParam prm)
		{
			_prm = prm;
			_jzdEqualComp = new JzdEqualComparer(prm.Tolerance);
		}
		public DotColBuilderResult Build(int srid)
		{
			var res = new DotColBuilderResult();
			QueryJzdRing(_prm.Dk.Shape, r =>
			{
				#region 根据已有界址点识别关键界址点
				foreach (var jzd in _prm.lstAroundJzd)
				{
					Process(r, jzd);
				}
				#endregion
				if (r.Count <= _prm.MinKeyJzdCount)
				{
					foreach (var jd in r)
					{
						jd.IsKey = true;
					}
				}
				else
				{
					//按角度判断关键界址点
					CheckKeyAngle(r);

					Ensure4KeyJzd(r, r.Count(ji =>
					{
						return ji.IsKey;
					}), _prm.MinKeyJzdCount);
				}

				#region 输出界址线
				OutputJzx(r,srid, jzx =>
				{
					res.lstJzx.Add(jzx);
				});
				#endregion

				#region 输出关键界址点
				foreach (var di in r)
				{
					if (di.IsKey)
					{
						res.lstJzd.Add(di);
					}
				}
				#endregion

			});
			return res;
		}
		private void Process(JzdRing r, JzdItem di)
		{
			foreach (var jd in r)
			{
				if (_jzdEqualComp.Equals(di.Shape, jd.Shape))
				{
					jd.Jzdh = di.Jzdh;
					jd.IsKey = true;
					return;
				}
			}
			var preC = r[r.Count - 1].Shape;
			for (int i = 0; i < r.Count; ++i)
			{
				var c = r[i].Shape;
				if (IsPointOnLineSegment(preC, c, di.Shape, _prm.Tolerance, _prm.Tolerance2))
				{
					r.Insert(i, di);
					return;
				}
				preC = c;
			}
		}
		private void QueryJzdRing(IGeometry dkShape, Action<JzdRing> callback)
		{
			GeometryUtil.EnumPolygon(dkShape, pgn =>
			{
				var r = ToJzdRing(pgn.Shell);
				callback(r);
				foreach (var h in pgn.Holes)
				{
					r = ToJzdRing(h);
					callback(r);
				}
			});
		}
		private JzdRing ToJzdRing(ILinearRing lr)
		{
			var r = new JzdRing();
			Coordinate pre = null;
			for (int i = 0; i < lr.Coordinates.Length - 1; ++i)
			{
				bool fSame = false;
				var c = lr.Coordinates[i];
				if (pre != null && _jzdEqualComp.Equals(c, pre))
				{
					fSame = true;
				}
				if (!fSame)
				{
					r.Add(new JzdItem(c));
				}
				pre = c;
			}
			return r;
		}
		private static bool IsPointOnLineSegment(Coordinate ptFrom, Coordinate ptTo, Coordinate p, double tolerance, double tolerance2)
		{
			if (p.X + tolerance < Math.Min(ptFrom.X, ptTo.X))
				return false;
			if (p.X - tolerance > Math.Max(ptFrom.X, ptTo.X))
				return false;
			if (p.Y + tolerance < Math.Min(ptFrom.Y, ptTo.Y))
				return false;
			if (p.Y - tolerance > Math.Max(ptFrom.Y, ptTo.Y))
				return false;
			return CglHelper.IsPointOnLine(ptFrom, ptTo, p, tolerance2);
		}

		private void CheckKeyAngle(JzdRing r)
		{
			for (int i = 0; i < r.Count; ++i)
			{
				var pre = i == 0 ? r[r.Count - 1].Shape : r[i - 1].Shape;
				var c = r[i].Shape;
				var c1 = i == r.Count - 1 ? r[0].Shape : r[i + 1].Shape;
				var angle = CglHelper.CalcAngle(c, pre, c1);
				if (IsKeyAngle(angle))
				{
					bool fExist = false;
					foreach (var jd in r)
					{
						if (jd.IsKey && _jzdEqualComp.Equals(jd.Shape, c))
						{
							fExist = true;
							break;
						}
					}
					if (!fExist)
					{
						r[i].IsKey = true;
					}
				}
			}

		}


		#region 确保至少4个界址点相关部分
		class JzdOutEdge
		{
			public JzdItem jzd;
			/// <summary>
			/// 该界址点出度长度的平方
			/// </summary>
			public double len2;
			public JzdOutEdge(JzdItem je, double len2_)
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
		private void Ensure4KeyJzd(JzdRing lstJzd, int nKeyJzdCount, int MinKeyJzdCount)
		{
			if (nKeyJzdCount >= MinKeyJzdCount)
			{
				return;
			}
			if (lstJzd.Count <= MinKeyJzdCount)
			{
				foreach (var jzdEn in lstJzd)
				{
					jzdEn.IsKey = true;
				}
				return;
			}

			var ll = new List<MyList>();
			var lst = new MyList();

			for (int i = 0; i < lstJzd.Count; ++i)
			{
				var jzd = lstJzd[i];
				var nextJzd = lstJzd[i == lstJzd.Count - 1 ? 0 : (i + 1)];
				var len2 = Math.Sqrt(CglHelper.GetDistance2(jzd.Shape, nextJzd.Shape));
				var tpl = new JzdOutEdge(jzd, len2);
				if (jzd.IsKey)
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
				splitItem.points[splitItem.nSplitIndex].jzd.IsKey = true;
				//SetKeyJzd(splitItem.points[splitItem.nSplitIndex].jzd.Shape);
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


		/// <summary>
		/// 根据角度判断是否关键界址点
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		private bool IsKeyAngle(double angle)
		{
			return angle >= (double)_prm.MinAngleFileter
										&& angle <= (double)_prm.MaxAngleFilter;
		}

		private void OutputJzx(JzdRing r,int srid, Action<JzxItem> callback)
		{
			var lst = new List<JzdItem>();
			var lstC = new List<Coordinate>();
			int iFirstPos = r.FindIndex(di => { return di.IsKey; });
			int iLastPos = r.FindLastIndex(di => { return di.IsKey; });
			for (int i = iFirstPos; i <= iLastPos; ++i)
			{
				var ri = r[i];
				lst.Add(ri);
				if (ri.IsKey && lst.Count > 1)
				{
					var jzx = MakeJzx(lst, lstC,srid);
					callback(jzx);
					lst.Add(ri);
				}
			}
			for (int i = iLastPos; i < r.Count; ++i)
			{
				lst.Add(r[i]);
			}
			for (int i = 0; i <= iFirstPos; ++i)
			{
				lst.Add(r[i]);
			}
			if (lst.Count > 0)
			{
				var jzx = MakeJzx(lst, lstC,srid);
				callback(jzx);
			}
		}
		private JzxItem MakeJzx(List<JzdItem> lst, List<Coordinate> lstC,int srid)
		{
			lstC.Clear();
			var jzx = new JzxItem()
			{
				Qjzd = lst[0],
				Zjzd = lst[lst.Count - 1],
			};
			foreach (var c in lst)
			{
				lstC.Add(c.Shape);
			}
			jzx.Shape = GeometryUtil.MakeLineString(lstC.ToArray(),srid);
			lst.Clear();
			if (FindJzxFromInputPrm(jzx.Shape, out JzxItem jx))
			{
				jzx.Jzxh = jx.Jzxh;
				jzx.sQjzd = jx.sQjzd;
				jzx.sZjzd = jx.sZjzd;
				//jzx.PldwZjr = jx.PldwZjr;
				//jzx.PldwQlr = jx.PldwQlr;
			}

			#region 查找界址线的指界人和毗邻地物权利人
			FindAroundDk(jzx.Shape, _lstTmpAroundDk);
			_lstTmpAroundDk.Add(_prm.Dk);
			_tmpSetQlr.Clear();
			_tmpSetZjr.Clear();
			foreach (var dk in _lstTmpAroundDk)
			{
				if (!string.IsNullOrEmpty(dk.ZjrXm))
				{
					_tmpSetZjr.Add(dk.ZjrXm);
				}
				if (!string.IsNullOrEmpty(dk.CbfMc))
				{
					_tmpSetQlr.Add(dk.CbfMc);
				}
			}
			jzx.PldwQlr = MakeString(_tmpSetQlr);
			jzx.PldwZjr = MakeString(_tmpSetZjr);
			#endregion

			return jzx;
		}
		private bool FindJzxFromInputPrm(ILineString ls, out JzxItem jzx)
		{
			foreach (var l in _prm.lstAroundJzx)
			{
				if (IsSame(l.Shape, ls))
				{
					jzx = l;
					return true;
				}
			}
			jzx = null;
			return false;
		}
		private bool IsSame(ILineString ls1, ILineString ls2)
		{
			#region 外框判断
			var e1 = ls1.EnvelopeInternal;
			var e2 = ls2.EnvelopeInternal;
			var dx = Math.Abs(e1.MinX - e2.MinX);
			if (dx > _prm.Tolerance)
				return false;
			dx = Math.Abs(e1.MaxX - e2.MaxX);
			if (dx > _prm.Tolerance)
				return false;
			var dy = Math.Abs(e1.MinY - e2.MinY);
			if (dy > _prm.Tolerance)
				return false;
			dy = Math.Abs(e1.MaxY - e2.MaxY);
			if (dy > _prm.Tolerance)
				return false;
			#endregion

			foreach (var c in ls1.Coordinates)
			{
				if (!IsPointOnLine(ls2, c))
					return false;
			}
			foreach (var c in ls2.Coordinates)
			{
				if (!IsPointOnLine(ls1, c))
					return false;
			}
			return true;
		}
		private bool IsPointOnLine(ILineString ls, Coordinate c)
		{
			var sa = ls.Coordinates;
			foreach (var ci in sa)
			{
				if (CglHelper.isSamePoint(ci, c, _prm.Tolerance))
				{
					return true;
				}
			}
			for (int i = 1; i < sa.Length; ++i)
			{
				var p0 = sa[i - 1];
				var p1 = sa[i];
				if (IsPointOnLineSegment(p0, p1, c, _prm.Tolerance, _prm.Tolerance2))
				{
					return true;
				}
			}
			return false;
		}
		private bool FindAroundDk(ILineString ls, List<DkItem> lstAroundDk)
		{
			lstAroundDk.Clear();
			var dLsLen = ls.Length;
			var eLs = ls.EnvelopeInternal;
			foreach (var d in _prm.lstAroundDk)
			{
				var e = d.Shape.EnvelopeInternal;
				if (!eLs.Intersects(e))
				{
					continue;
				}
				try
				{
					var lsInter = ls.Intersection(d.Shape);
					if (lsInter != null)
					{
						var iPercent = lsInter.Length / dLsLen;
						if (iPercent > 0.4)
						{
							lstAroundDk.Add(d);
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			return lstAroundDk.Count > 0;
		}

		private string MakeString(HashSet<string> set)
		{
			string str = null;
			foreach (var s in set)
			{
				if (str == null)
				{
					str = s;
				}
				else
				{
					str += "/" + s;
				}
			}
			return str;
		}
	}
	public class DkItem
	{
		/// <summary>
		/// 原地块编码
		/// </summary>
		public string OriDkbm;

		/// <summary>
		/// 地块编码
		/// </summary>
		public string Dkbm;
		/// <summary>
		/// 指界人姓名
		/// </summary>
		public string ZjrXm;
		/// <summary>
		/// 承包方名称
		/// </summary>
		public string CbfMc;
		public IGeometry Shape;
	}
	public class JzdItem
	{
		public string Jzdh;
		public readonly Coordinate Shape;
		/// <summary>
		/// 是否关键界址点
		/// </summary>
		public bool IsKey = false;
		public JzdItem(Coordinate c)
		{
			Shape = c;
		}
	}
	public class JzxItem
	{
		/// <summary>
		/// 界址线号
		/// </summary>
		public string Jzxh;
		/// <summary>
		/// 毗邻地物指界人
		/// </summary>
		public string PldwZjr;
		/// <summary>
		/// 毗邻地物权利人
		/// </summary>
		public string PldwQlr;

		/// <summary>
		/// 起界址点
		/// </summary>
		public JzdItem Qjzd;
		/// <summary>
		/// 止界址点
		/// </summary>
		public JzdItem Zjzd;
		public ILineString Shape;

		/// <summary>
		/// 原起界址点
		/// </summary>
		public string sQjzd;
		/// <summary>
		/// 原止界址点
		/// </summary>
		public string sZjzd;
	}
	public class DotCoilBuilderParam
	{
		public readonly double Tolerance = 0.05;
		public readonly double Tolerance2;

		/// <summary>
		/// 界址标识
		/// </summary>
		public string AddressPointPrefix = "J";
		/// <summary>
		/// 最小过滤角度值，单位：度
		/// </summary>
		public double? MinAngleFileter = 5;

		/// <summary>
		/// 最大过滤角度值，单位：度
		/// </summary>
		public double? MaxAngleFilter = 130;

		/// <summary>
		/// 一个地块包含的最少关键界址点个数
		/// </summary>
		public short MinKeyJzdCount = 4;

		public readonly DkItem Dk = new DkItem();

		/// <summary>
		/// 输入地块周边已有的界址点
		/// </summary>
		internal readonly List<JzdItem> lstAroundJzd = new List<JzdItem>();
		/// <summary>
		/// 输入地块周边已有界址线
		/// </summary>
		internal readonly List<JzxItem> lstAroundJzx = new List<JzxItem>();
		/// <summary>
		/// 输入地块周边的地块
		/// </summary>
		internal readonly List<DkItem> lstAroundDk = new List<DkItem>();

		private SortedSet<Coordinate> lstSortedJzd;
		public DotCoilBuilderParam(double tolerance = 0.05)
		{
			lstSortedJzd = new SortedSet<Coordinate>(new JzdComparer(tolerance));
			Tolerance2 = tolerance * tolerance;
		}
		public void AddJzd(Coordinate c, string jzdh)
		{
			if (!lstSortedJzd.Contains(c))
			{
				lstAroundJzd.Add(new JzdItem(c) { Jzdh = jzdh, IsKey = true });
			}
		}
		public void AddJzx(ILineString ls, string jzxh, string pldwZjr, string pldwQlr, string qjzdh, string zjzdh)
		{
			if (lstAroundJzx.Find(ji => { return ji.Jzxh == jzxh; }) == null)
			{
				lstAroundJzx.Add(new JzxItem()
				{
					Jzxh = jzxh,
					Shape = ls,
					PldwZjr = pldwZjr,
					PldwQlr = pldwQlr,
					sQjzd = qjzdh,
					sZjzd = zjzdh
				});
			}
		}
		public void AddAroundDk(IGeometry shape, string zjrXm, string cbfMc)
		{
			var dk = new DkItem()
			{
				Shape = shape,
				CbfMc = cbfMc,
				ZjrXm = zjrXm
			};
			try
			{
				dk.Shape = dk.Shape.Buffer(Tolerance);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			lstAroundDk.Add(dk);
		}
	}
	public class DotColBuilderResult
	{
		public readonly List<JzdItem> lstJzd = new List<JzdItem>();
		public readonly List<JzxItem> lstJzx = new List<JzxItem>();
	}
}
