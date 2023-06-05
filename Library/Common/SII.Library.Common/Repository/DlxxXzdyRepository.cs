using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Database;
using Agro.LibCore.GIS;
using Agro.Library.Model;
using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Agro.Library.Common.Repository
{
	public class DlxxXzdyRepository:CrudRepository<DlxxXzdyRepository,DLXX_XZDY>, IZoneTreeModel
    {

		private List<string> MakeSubFields()
		{
			var subFields = SubFields.Make<DLXX_XZDY>((c, t) => c(t.BM, t.MC, t.JB, t.ID)).Value.ToList();
			subFields.Add(Db is SQLiteWorkspace ? "rowid" : "bsm");
			return subFields;
		}
		public ShortZone FindRootZone()
		{
			var subFields = MakeSubFields();

			int jb = -1;
			int cnt = 0;
			Db.QueryCallback($"select JB,count(1) from {TableName} where JB>1 group by JB order by count(1)", r =>
			 {
				 var j = r.GetInt32(0);
				 var n = SafeConvertAux.ToInt32(r.GetValue(1));
				 if (jb == -1 || cnt > n||cnt==n&&jb>j)
				 {
					 jb = j;
					 cnt = n;
				 }
			 });
			if (jb != -1)
			{
				var en=Find(t=>t.JB==(eZoneLevel)jb, SubFields.Make(subFields));
				if (en != null)
				{
					return new ShortZone(en);
				}
			}
			return null;
			//var lst = FindAll(q => q.JB == eZoneLevel.City || q.JB == eZoneLevel.County, SubFields.Make(subFields));//(c,t)=>c(t.BM,t.MC,t.JB,t.ID));
			//if (lst?.Count(it => it.JB ==eZoneLevel.County) == 1)
			//{
			//	var it1 = lst.Find(it => it.JB ==eZoneLevel.County);
			//	return new ShortZone(it1);
			//}
			//else
			//{
			//	var en = lst?.Find(it => it.JB ==eZoneLevel.City);
			//	return en != null ? new ShortZone(en) : null;
			//}
		}
		public ShortZone FindRootZone(SEC_ID_USER  user)
		{
			if (user?.IsAdmin()!=false)
				return FindRootZone();
			ShortZone zone = null;
			FindNoBinary(q => q.BM.StartsWith(user.ZoneCode), it =>
			 {
				 var en = it.Item;
				 if (zone == null || (int)zone.Level <(int)en.JB)
				 {
					 zone = new ShortZone(en);
				 }
			 });
			return zone;
		}

		public List<ShortZone> QueryChildren(ShortZone zone, List<ShortZone> lst = null)
		{
			if (lst == null)
			{
				lst = new List<ShortZone>();
			}
			else
			{
				lst.Clear();
			}

			FindCallback1(q => q.SJID == zone.ID, it=>
			{
				var i = it.Item;
				lst.Add(new ShortZone(i.ID, i.BM, i.MC, i.JB, i.ObjectId));
			}, SubFields.Make(MakeSubFields()));
			return lst;
		}

		public OkEnvelope GetFullExtent(string where=null,IFeatureWorkspace db=null)
		{
			if (db == null) db = Db as IFeatureWorkspace;
			using (var fc = db.OpenFeatureClass(TableName))
			{
				if (where == null)
				{
					return fc.GetFullExtent();
				}
				else
				{
					Envelope fullEnv = null;
					fc.Search(SqlUtil.MakeQueryFilter(FieldName(nameof(DLXX_XZDY.SHAPE)), where), r =>
					  {
						  var env=(r as IFeature).Shape?.EnvelopeInternal;
						  if (fullEnv == null)
							  fullEnv = env;
						  else
							  fullEnv.ExpandToInclude(env);
					  });
					return fullEnv == null ? fc.GetFullExtent() :new OkEnvelope(fullEnv);
				}
			}
		}

		public IGeometry QueryShape(ShortZone zone)
		{
			return Instance.GetShape(zone.OID);
		}

		public bool HasChildren(ShortZone zone, bool fContainGroupNode = false)
		{
			if (!fContainGroupNode)
			{
				if (zone.Level == eZoneLevel.Group)
					return false;
			}
			var cnt = Count(q => q.SJID == zone.ID);
			return cnt > 0;
		}

		public void HasChildren(List<ShortZone> lst, Action<ShortZone, bool> callback, bool fContainGroupNode = false)
		{
			var set = new HashSet<string>();
			SqlUtil.ConstructIn(lst.Select(z => z.ID), sin =>
			   {
				   var sql = $"select SJID,count(*) from {TableName} where SJID in({sin}) group by SJID";
				   Db.QueryCallback(sql, r =>set.Add(r.GetString(0)));
			   });
			foreach (var zone in lst)
			{
				if (!fContainGroupNode)
				{
					if (zone.Level == eZoneLevel.Group)
					{
						callback(zone, false);
						continue;
					}
				}
				callback(zone, set.Contains(zone.ID));
			}
		}
		public ShortZone QueryZone(Expression<Func<DLXX_XZDY, bool>> wh)
		{
			//var fields = DLXX_XZDY.GetFieldsByProperties(new string[]{
			//	nameof(DLXX_XZDY.ID),nameof(DLXX_XZDY.BM),nameof(DLXX_XZDY.JB),nameof(DLXX_XZDY.BM)
			//});
			var en= FindNoBinary(wh);
			return en==null?null:new ShortZone(en);
		}


	}

	/// <summary>
	/// 行政地域全名称生成
	/// </summary>
	public class XzdyFullNameBuilder
	{

		/// <summary>
		/// [id,[sjid,mc]]
		/// </summary>
		private readonly Dictionary<string, Tuple<string, string>> _dicIDToName = new Dictionary<string, Tuple<string, string>>();

		public XzdyFullNameBuilder()
		{
			DlxxXzdyRepository.Instance.FindCallback(t => t.ID != null, it => _dicIDToName[it.Item.ID] = new Tuple<string, string>(it.Item.SJID, it.Item.MC), (c, t) => c(t.ID, t.MC, t.SJID));
		}

		/// <summary>
		/// 获取全名称
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		public string GetFullName(string id)
		{
			string str = "";// zone.Name;
							//var id = zone.ID;
			while (!string.IsNullOrEmpty(id))
			{
				if (_dicIDToName.TryGetValue(id, out var t))
				{
					str = t.Item2 + str;
					id = t.Item1;
				}
				else
				{
					break;
				}
			}
			return str;
		}

	}
}
