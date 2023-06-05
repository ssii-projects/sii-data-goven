using Agro.GIS;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.LibCore
{
	public static class TransformExtension
	{
		/// <summary>
		/// yxm 2018-11-22
		/// 不影响输入参数env
		/// </summary>
		/// <param name="source"></param>
		/// <param name="env"></param>
		/// <returns></returns>
		public static IPolygon Transform(this AffineTransformation source, Envelope env)
		{
			if (source == null || env == null)
			{
				return null;
			}
			//if (true)//yxm 2019-11-16
			//{
			var pgn = GeometryUtil.MakePolygon(env);
			return source.Transform(pgn) as IPolygon;
			//}

			//var c = new Coordinate(env.MinX, env.MinY);
			//source.Transform(c, c);
			//var mx = c.X;
			//var my = c.Y;
			//c.X = env.MaxX;c.Y = env.MaxY;
			//source.Transform(c, c);
			//var e = new Envelope(mx, c.X, my, c.Y);
			//return e;
		}
	}
}
