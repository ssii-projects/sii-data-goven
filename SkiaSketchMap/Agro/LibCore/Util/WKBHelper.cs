/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.5
 * 文 件 名：   WKBHelper
 * 创 建 人：   颜学铭
 * 创建时间：   2016/5/19 16:21:03
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace Agro.LibCore
{
    public class WKBHelper
    {
        public static IGeometry FromBytes(byte[] bytes, int srid = 0)
        {
            try
            {
                var reader = new NetTopologySuite.IO.WKBReader(NetTopologySuite.NtsGeometryServices.Instance);
                var geo = reader.Read(bytes);
                geo.SRID = srid;
                return geo;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
		/// <summary>
		/// yxm 2019-1-22
		/// </summary>
		/// <param name="wkt"></param>
		/// <param name="srid"></param>
		/// <returns></returns>
		public static IGeometry FromWKT(string wkt, int srid = 0)
		{
			try
			{
				var reader = new NetTopologySuite.IO.WKTReader();// NetTopologySuite.NtsGeometryServices.Instance);
				var geo = reader.Read(wkt);
				geo.SRID = srid;
				return geo;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}
	}
}
