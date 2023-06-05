using Agro.Library.Model;
using System;

namespace Agro.Library.Common.Repository
{
	public class XtpzLshRepository :CrudRepository<XtpzLshRepository,XTPZ_LSH>
	{

		/// <summary>
		/// 获取指定发包方编码的下一个流水号
		/// return null表示未找到
		/// </summary>
		/// <param name="fbfBM"></param>
		/// <returns></returns>
		public XTPZ_LSH FindByFbfbm(string fbfBM,string lx= "CONTRACTLAND")
		{
			return Find(t => t.FZM == fbfBM && t.LX == lx);
		}

		public static eContractorType CbfLx2Enum(string cbflx)
		{
			switch (cbflx)
			{
				case "农户": return eContractorType.Farmer;
				case "个人": return eContractorType.Personal;
				case "单位":return eContractorType.Unit;
			}
			return eContractorType.Unknown;
		}
		/// <summary>
		/// 获取新的承包方编码（该方法会更新XTPZ_LSH表的BH字段）；
		/// </summary>
		/// <param name="cbflx"></param>
		/// <param name="fbfBM"></param>
		/// <returns></returns>
		public string QueryNewCbfbm(eContractorType cbflx, string fbfBM)
		{
			var lx = "";
			switch (cbflx)
			{
				case eContractorType.Unit:lx = "CONTRACTOR_ORG"; break;
				case eContractorType.Farmer:lx = "CONTRACTOR_FAMILY";break;
				case eContractorType.Personal:lx = "CONTRACTOR_PERSON"; break;
				default:throw new Exception($"Invalid param for {cbflx}");
			}
			var en = FindByFbfbm(fbfBM, lx);
			if (en == null)
			{
				en = new XTPZ_LSH()
				{
					LX = lx,
					FZM = fbfBM,
					BH = 1
				};
				Insert(en);
				//return null;
			}
			string s = (10000 + en.BH).ToString().Substring(1);
			var sNewCBFBM = fbfBM + s;
			en.BH += 1;
			Update(en, t => t.ID == en.ID, (c, t) => c(t.BH));
			return sNewCBFBM;
		}

		/// <summary>
		/// 根据发包方编码获取新的地块编码（该方法会更新XTPZ_LSH表的BH字段）；
		/// yxm 2018-5-7
		/// </summary>
		/// <param name="db"></param>
		/// <param name="sFBFBM"></param>
		/// <returns></returns>
		public string QueryNewDKBM(string sFBFBM)
		{
			var en = FindByFbfbm(sFBFBM);
			if (en == null)
			{
				en = new XTPZ_LSH()
				{
					LX = "CONTRACTLAND",
					FZM = sFBFBM,
					BH = 1
				};
				Insert(en);
				//return null;
			}
			string s = (100000 + en.BH).ToString().Substring(1);
			var sNewDKBM = sFBFBM + s;
			en.BH += 1;
			Update(en, t => t.ID == en.ID,(c,t)=>c(t.BH));
			return sNewDKBM;
		}

		public void UpdateLsh(string lx, string fzm, int bh)
		{
			var en = Find(t => t.LX == lx && t.FZM == fzm);
			if (en != null)
			{
				if (en.BH < bh)
				{
					en.BH = bh;
					Update(en, t => t.ID == en.ID);
				}
			}
			else
			{
				en = new XTPZ_LSH()
				{
					LX = lx,
					FZM = fzm,
					BH = bh
				};
				Insert(en);
			}
		}
	}
}
