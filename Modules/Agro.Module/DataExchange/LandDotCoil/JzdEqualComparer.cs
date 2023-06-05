using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/*
yxm created at 2019/4/24 14:09:50
*/
namespace Agro.Module.DataExchange.LandDotCoil
{
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
			return CglHelper.IsSame2(a, b, _tolerace2);
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
}
