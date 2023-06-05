using Agro.Library.Model;
using Agro.LibCore.Database;
using Agro.LibCore.GIS;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using Agro.Library.Common.Repository;
using System.Linq.Expressions;
using Agro.GIS;

namespace Agro.Library.Common.Util
{

	public class ZoneUtil
	{
		private static DlxxXzdyRepository repos = DlxxXzdyRepository.Instance;
		//public static void Test()
		//{
		//	var lst = new List<ShortZone>();

		//	var zone = new Zone
		//	{
		//		FullCode = "341502100204"
		//	};
			

		//	MyGlobal.Orm.LoadEntities<XZQH_XZDY>(q => q.KZBM == zone.FullCode || q.BM == zone.FullCode
		//	, i =>
		//	{
		//		var sz = new ShortZone(i.ID, i.BM, i.MC, (eZoneLevel)i.JB, i.ObjectId);
		//		lst.Add(sz);
		//		return true;
		//	}, EntityBase.GetFields<XZQH_XZDY>(false));
		//	lst = null;
		//}

		public static OkEnvelope GetFullExtent(string where = null, IFeatureWorkspace db = null)
		{
			return repos.GetFullExtent(where,db);
		}

		///// <summary>
		///// 查询zone下的子集（包含zone自身)
		///// </summary>
		///// <param name="zone"></param>
		///// <param name="lst"></param>
		///// <returns></returns>
		public static List<ShortZone> QueryChildren(ShortZone zone, List<ShortZone> lst = null)
		{
			return repos.QueryChildren(zone, lst);
		}

		public static IGeometry QueryShape(ShortZone zone)
		{
			return repos.QueryShape(zone);
		}


		public static ShortZone QueryZone(Expression<Func<DLXX_XZDY, bool>> wh)
		{
			return repos.QueryZone(wh);
		}
	}
}
