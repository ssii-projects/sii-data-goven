using Agro.LibMapServer;
using Agro.Library.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGovenServer.Repository
{
	public class XtpzLshRepository : CrudRepositoryBase<XtpzLshRepository,XTPZ_LSH>
	{
		//public XtpzLshRepository() : base(XTPZ_LSH.GetTableName()) { }


		/// <summary>
		/// 获取指定发包方编码的下一个流水号
		/// return -1表示未找到
		/// </summary>
		/// <param name="fbfBM"></param>
		/// <returns></returns>
		public XTPZ_LSH FindByFbfbm(IDbConnection con, string fbfBM)
		{
			return Find(con,t => t.FZM == fbfBM && t.LX == "CONTRACTLAND");
		}

		/// <summary>
		/// 根据发包方编码获取新的地块编码（该方法会更新XTPZ_LSH表的BH字段）；
		/// yxm 2018-5-7
		/// </summary>
		/// <param name="db"></param>
		/// <param name="sFBFBM"></param>
		/// <returns></returns>
		public string QueryNewDKBM(IDbConnection con, string sFBFBM)
		{
			var en = FindByFbfbm(con,sFBFBM);
			if (en == null)
			{
				return null;
			}
			string s = (100000 + en.BH).ToString().Substring(1);
			var sNewDKBM = sFBFBM + s;
			en.BH += 1;
			Update(con,en, t => t.ID == en.ID, (c, t) => c(t.BH));
			return sNewDKBM;
		}

		public void UpdateLsh(IDbConnection con, string lx, string fzm, int bh)
		{
			var en = Find(con,t => t.LX == lx && t.FZM == fzm);
			if (en != null)
			{
				if (en.BH < bh)
				{
					en.BH = bh;
					Update(con,en, t => t.ID == en.ID);
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
				Insert(con,en);
			}
		}

		#region JZD
		public int GetMaxJzdh(IDbConnection con, string xiangBM)
		{
			var db = _db;
			var sql = $"select BH from XTPZ_LSH where LX='JZDH' and FZM='{xiangBM}'";
			var o = db.QueryOne(con,sql);
			if (o == null)
			{
				int nMax = 0;
				InsertMaxJzdh(con,xiangBM, nMax);
				return nMax;
			}
			return SafeConvertAux.ToInt32(o);
		}
		public void UpdateMaxJzdh(IDbConnection con, string xiangBM, int nJzdh)
		{
			var sql = $"update XTPZ_LSH set BH={nJzdh} where LX='JZDH' and FZM='{xiangBM}'";
			_db.ExecuteNonQuery(con,sql);
		}
		public void InsertMaxJzdh(IDbConnection con, string xiangBM, int nJzdh)
		{
			var sql = $"insert into XTPZ_LSH(ID,LX,FZM,BH) values('{Guid.NewGuid().ToString()}','JZDH','{xiangBM}',{nJzdh})";
			_db.ExecuteNonQuery(con,sql);
		}
		#endregion JZD

		#region JZX
		public int GetMaxJzxh(IDbConnection con, string xiangBM)
		{
			var sql = $"select BH from XTPZ_LSH where LX='JZXH' and FZM='{xiangBM}'";
			var o = _db.QueryOne(con,sql);
			if (o == null)
			{
				int nMax = 0;
				InsertMaxJzxh(con,xiangBM, nMax);
				return nMax;
			}
			return SafeConvertAux.ToInt32(o);
		}
		public void UpdateMaxJzxh(IDbConnection con, string xiangBM, int nJzxh)
		{
			var sql = $"update XTPZ_LSH set BH={nJzxh} where LX='JZXH' and FZM='{xiangBM}'";
			_db.ExecuteNonQuery(con,sql);
		}
		public void InsertMaxJzxh(IDbConnection con, string xiangBM, int nJzxh)
		{
			var sql = $"insert into XTPZ_LSH(ID,LX,FZM,BH) values('{Guid.NewGuid().ToString()}','JZXH','{xiangBM}',{nJzxh})";
			_db.ExecuteNonQuery(con,sql);
		}
		#endregion JZX
	}
}
