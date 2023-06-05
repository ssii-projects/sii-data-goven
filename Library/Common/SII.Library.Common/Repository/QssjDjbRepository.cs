using Agro.Library.Model;
using System;

namespace Agro.Library.Common.Repository
{
	public class QssjDjbRepository : CrudRepository<QssjDjbRepository,QSSJ_CBJYQZDJB>
	{
		/// <summary>
		///CBJYQZLSH like '吉(2016)梅河口市农村土地承包经营权第000001号'
		///解析出其中的： 吉 2016 000001
		///成功返回it,失败返回null
		/// </summary>
		/// <param name="CBJYQZLSH"></param>
		/// <param name="it"></param>
		/// <returns></returns>
		public static LshItem ParseLsh(string CBJYQZLSH, LshItem it=null)
		{
			try
			{
				if (it == null)
				{
					it = new LshItem();
				}
				else
				{
					it.Reset();
				}
				if (!string.IsNullOrEmpty(CBJYQZLSH))
				{
					var str = CBJYQZLSH;
					var n = str.IndexOf('(');
					if (n < 0)
					{
						n = str.IndexOf('（');
					}
					if (n > 0)
					{
						it.SQSJC = str.Substring(0, n);
						var n1 = str.IndexOf(')', n);
						if (n1 < n)
						{
							n1 = str.IndexOf('）', n);
						}
						if (n1 > n)
						{
							var s = str.Substring(n + 1, n1 - n - 1);
							if (int.TryParse(s, out int nYear))
							{
								it.BZNF = nYear;
							}
						}
					}

					n = str.Length - 7; //str.LastIndexOf('第');
					var n2 = str.Length - 1;// str.LastIndexOf('号');
					var s1 = str.Substring(n, n2 - n);
					if (int.TryParse(s1, out int nNdsxh))
					{
						it.NDSXH = nNdsxh;
						//return it;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return it;
		}
	}

	public class LshItem
	{
		/// <summary>
		/// 省区市的简称
		/// </summary>
		public string SQSJC;
		/// <summary>
		/// 颁证年份
		/// </summary>
		public int? BZNF;
		/// <summary>
		/// 年度顺序号
		/// </summary>
		public int? NDSXH;
		public void Reset()
		{
			SQSJC = null;
			BZNF = null;
			NDSXH = null;
		}
	}
}
