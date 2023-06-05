using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGovenServer.Service.LandDotCoil
{
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
}
