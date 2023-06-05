using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Agro.LibCore;
using Agro.GIS;

/*
yxm created at 2019/1/22 16:38:31
*/
namespace SketchMap
{
	public class SerialShape// : IXmlSerializable
	{
		public IGeometry? Geometry;
		public SerialShape()
		{
		}
		public SerialShape(IGeometry? g)
		{
			Geometry = g;
		}
		public SpatialReference? GetSpatialReference()
		{
			return Geometry?.GetSpatialReference();
		}
		public SerialShape Project(SpatialReference sr)
		{
			var g=Geometry?.Project(sr);
			return new SerialShape(g ?? Geometry);
		}

		//public XmlSchema GetSchema()
		//{
		//	return null;
		//}

		//public void ReadXml(XmlReader reader)
		//{
		//	string wkt = null;
		//	int srid = 0;
		//	SerializeUtil.ReadNodeCallback(reader, "Shape", name =>
		//	  {
		//		  switch (name) {
		//			  case "GeometryText":
		//				  {
		//					  wkt = reader.ReadString();							  
		//				  }break;
		//			  case "SRID":
		//				  srid = SafeConvertAux.ToInt32(reader.ReadString());
		//				  break;						 
		//		  }
		//	  });
		//	if (wkt != null)
		//	{
		//		this.Geometry= WKBHelper.FromWKT(wkt,srid);
		//	}
		//}

		//public void WriteXml(XmlWriter writer)
		//{
		//	if (Geometry == null)
		//		return;
		//	writer.WriteStartElement("Shape");

		//	writer.WriteElementString("GeometryText", Geometry.AsText());
		//	var sr = Geometry.GetSpatialReference();
		//	if (sr != null)
		//	{
		//		writer.WriteElementString("SRID", sr.AuthorityCode.ToString());
		//	}
		//	writer.WriteEndElement();
		//}
	}
}
