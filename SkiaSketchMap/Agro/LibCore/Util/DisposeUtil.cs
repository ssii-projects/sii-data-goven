using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/1/15 13:51:43
*/
namespace Agro.LibCore
{
	public static class DisposeUtil
	{
		public static void SafeDispose<T>(ref T? t) where T : IDisposable
		{
			if (t != null)
			{
				t.Dispose();
				t = default;
			}
		}
	}
}
