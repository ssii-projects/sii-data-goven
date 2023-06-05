using Agro.LibCore;
using GeoAPI.Geometries;
/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   SerializeUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/10/26 9:36:17
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Agro.GIS
{
    /// <summary>
    /// 序列化辅助类
    /// </summary>
    public class SerializeUtil
    {
        private const string DefNameSpace = "Agro.GIS";
        ///// <summary>
        ///// 用来实现序列化顺序无关的属性读取辅助方法
        ///// </summary>
        ///// <param name="reader"></param>
        ///// <param name="endElementName"></param>
        ///// <param name="callback"></param>
        //public static void ReadNodeCallback(XmlReader reader, string endElementName, Action<string> callback)
        //{
        //    bool fOK = reader.Read();
        //    while (fOK)
        //    {
        //        if (reader.NodeType == XmlNodeType.EndElement)
        //        {
        //            if (reader.Name == endElementName)
        //            {
        //                break;
        //            }
        //        }
        //        else if (reader.NodeType == XmlNodeType.Element)
        //        {
        //            //var tx = reader as XmlTextReader;
        //            if (reader is XmlTextReader tx)
        //            {
        //                var lineNum = tx.LineNumber;
        //                var linePos = tx.LinePosition;
        //                var name = reader.Name;
        //                callback(name);

        //                if (tx.LineNumber != lineNum || tx.LinePosition != linePos)// (!reader.IsStartElement(name))
        //                {
        //                    continue;
        //                }
        //            }
        //        }
        //        fOK=reader.Read();
        //    }
        //}

        public static IFeatureRenderer? CreateFeatureRenderer(string typeName)
        {
            return CreateInstance<IFeatureRenderer>(DefNameSpace, typeName);
        }
        public static IElement? CreateElement(string typeName)
        {
            return CreateInstance<IElement>(DefNameSpace, typeName);
        }
        public static T? CreateInstance<T>(string typeName) where T : class
        {
            return CreateInstance<T>(DefNameSpace, typeName);
        }
        public static T? CreateInstance<T>(string nameSpace,string typeName)where T: class
        {
            //var name = Assembly.GetExecutingAssembly().GetName();
            var type = System.Type.GetType(/*"Agro.GIS."*/nameSpace+"." + typeName);
            if (type == null)
                return null;// default(T);
            try
            {
                var t = Activator.CreateInstance(type);//name.FullName,"DxComm.GIS."+ typeName) ;
                return t as T;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Assert(false, ex.Message);
                throw ex;
            }
        }
        //public static ISymbol? CreateSymbol(string typeName)
        //{
        //    return CreateInstance<ISymbol>(typeName);
        //}
        public static ISymbol? ReadSymbol(XmlElement reader, ISymbolFactory? symbolFactory=null)//, string name)
        {
            symbolFactory ??= GisGlobal.SymbolFactory;
            //reader.MoveToAttribute("type");
            var type = reader.GetAttribute("type");
            var symbol = symbolFactory.CreateSymbol(type);
            if (symbol == null)
            {
                System.Diagnostics.Debug.Assert(false,$"not find class {type}");
                return null;
            }
                    symbol.ReadXml(reader);
            //}
            return symbol;
        }
        //public static ISymbol? ReadSymbol(XmlReader reader, string name)
        //{
        //    ISymbol? symbol = null;
        //    if (reader.IsStartElement(name))
        //    {
        //        reader.MoveToAttribute("type");
        //        var type = reader.Value;
        //        symbol = CreateSymbol(type);
        //        if (symbol == null)
        //            return null;
        //        if (symbol is IXmlSerializable xml)
        //        {
        //            xml.ReadXml(reader);
        //        }
        //    }
        //    return symbol;
        //}
        public static System.Drawing.Color? ReadColor(XmlReader reader, string name)
        {
            if (reader.IsStartElement(name))
            {
                return ColorUtil.ConvertFromString(reader.ReadElementContentAsString());
            }
            return null;
        }
        public static int ReadInt(XmlReader reader, string name,int defaultVal=0)
        {
            if (reader.IsStartElement(name))
                return reader.ReadElementContentAsInt();
            return defaultVal;
        }
        public static double ReadDouble(XmlReader reader, string name,double defaultVal=0)
        {
            if (reader.IsStartElement(name))
                return reader.ReadElementContentAsDouble();
            return defaultVal;
        }
        public static string? ReadStringElement(XmlReader reader, string name)
        {
            if (reader.IsStartElement(name))
                return reader.ReadElementContentAsString();
            return null;
        }

        //public static GlowEffect? ReadGlowEffect(XmlElement reader,  GlowEffect? ge = null)
        //{
        //    ge ??= new GlowEffect();
        //    ge.ReadXml(reader);
        //    return ge;
        //}

  //      public static GlowEffect? ReadGlowEffect(XmlReader reader, string name,GlowEffect? ge=null)
		//{
		//	if (ge == null)
		//	{
		//		ge = new GlowEffect();
		//	}
		//	ge.ReadXml(reader);
		//	return ge;
		//}

		//public static FontDisp? ReadFontDisp(XmlReader reader, string name,FontDisp? font=null)
		//{
  //          throw new NotImplementedException();
		//	//if (reader.IsStartElement(name))
		//	//{
		//	//	font ??= new FontDisp();
		//	//	font.ReadXml(reader, name);
		//	//}
		//	//return font;
		//}
		//public static System.Drawing.Font ReadFont(XmlReader reader, string name)
  //      {
  //          if (reader.IsStartElement(name))
  //          {
  //              reader.Read();
  //              var FontFamily = reader.ReadElementContentAsString();
  //              var FontSize = reader.ReadElementContentAsDouble();
  //              var FontStyle = reader.ReadElementContentAsString();
  //              reader.ReadEndElement();
  //              return new System.Drawing.Font(FontFamily, (float)FontSize);
  //          }
  //          return null;
  //      }

        public static void WriteIntElement(XmlWriter writer, string name, int val)
        {
            WriteStringElement(writer, name, val.ToString());
        }
		public static void WriteBoolElement(XmlWriter writer, string name, bool val)
		{
			WriteStringElement(writer, name, val.ToString());
		}
		public static void WriteDoubleElement(XmlWriter writer, string name, double val)
        {
            WriteStringElement(writer, name, val.ToString());
        }
        //public static void WriteColorElement(XmlWriter writer, string name, System.Windows.Media.Color? val)
        //{
        //    if (val != null)
        //    {
        //        WriteStringElement(writer, name, val.ToString());
        //    }
        //}
        public static void WriteColorElement(XmlWriter writer, string name, System.Drawing.Color? val)
        {
            if (val != null)
            {
                var c = (System.Drawing.Color)val;
                var str = $"#{c.A:x2}{c.R:x2}{c.G:x2}{c.B:x2}".ToUpper();
                //var clr = ColorUtil.ToMediaColor((System.Drawing.Color)val);
                WriteStringElement(writer, name, str);
            }
        }
        public static void WriteStringElement(XmlWriter writer, string name, string val)
        {
            if (val == null)
            {
                return;
            }
            writer.WriteStartElement(name);
            writer.WriteString(val);
            writer.WriteEndElement();
        }

        public static IBorder? ReadBorder(XmlElement reader,IBorder? border = null)
        {
            foreach(var o in reader.ChildNodes)
            {
                if (o is XmlElement n) {
                    switch (n.Name)
                    {
                        case "Symbol":
                            {
                                var symbol = ReadSymbol(n) as ILineSymbol;
                                if (border == null)
                                {
                                    border = new SymbolBorder(symbol);
                                }
                                else
                                {
                                    border.Symbol = symbol;
                                }
                            }
                            break;
                    }
                }
            }
            return border;
        }
        //      public static IBorder ReadBorder(XmlReader reader, string name, IBorder border = null)
        //{
        //	SerializeUtil.ReadNodeCallback(reader, name, n =>
        //	  {
        //		  switch (n)//=="Symbol")
        //		  {
        //			  case "Symbol":
        //				  {
        //					  var symbol = SerializeUtil.ReadSymbol(reader, "Symbol") as ILineSymbol;
        //					  if (border == null)
        //					  {
        //						  border = new SymbolBorder(symbol);
        //					  }
        //					  else
        //					  {
        //						  border.Symbol = symbol;
        //					  }
        //				  }break;
        //		  }
        //	  });
        //	return border;
        //}
        //public static void WriteBorder(XmlWriter writer,string name, IBorder border)
        //{
        //	if (border != null && border.Symbol != null)
        //	{
        //		writer.WriteStartElement(name);
        //		SerializeUtil.WriteSymbol(writer, "Symbol", border.Symbol);
        //		writer.WriteEndElement();
        //	}
        //}
        public static void WriteSymbol(XmlWriter writer, string name, ISymbol? symbol)
        {
            if (symbol is IXmlSerializable xml)
            {
                writer.WriteStartElement(name);
                writer.WriteStartAttribute("type");
                writer.WriteString(symbol.GetType().Name);
                writer.WriteEndAttribute();
                xml.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        //public static void WriteFont(XmlWriter writer, string name, System.Drawing.Font font)
        //{
        //	writer.WriteStartElement(name);
        //	WriteStringElement(writer, "FontFamily", font.FontFamily.Name);
        //	WriteDoubleElement(writer, "FontSize", font.Size);
        //	WriteStringElement(writer, "FontStyle", font.Style.ToString());
        //	writer.WriteEndElement();
        //}
        public static void WriteFont(XmlWriter writer, string name, FontDisp? font)
        {
            if (font != null)
            {
                writer.WriteStartElement(name);
                WriteStringElement(writer, "FontFamily", font.FontFamily);
                WriteDoubleElement(writer, "FontSize", font.FontSize);
                WriteStringElement(writer, "FontStyle", font.FontStyle.ToString());
                writer.WriteEndElement();
            }
        }
        //public static void WriteExtent(XmlWriter writer, Envelope env, string name = "Extent")
        //{
        //	if (env == null)
        //	{
        //		return;
        //	}
        //	//var env = pgn.EnvelopeInternal;
        //	writer.WriteStartElement(name);
        //	WriteDoubleElement(writer, "MinX", env.MinX);
        //	WriteDoubleElement(writer, "MaxX", env.MaxX);
        //	WriteDoubleElement(writer, "MinY", env.MinY);
        //	WriteDoubleElement(writer, "MaxY", env.MaxY);
        //	//WriteSpatialReference(writer, env.SpatialReference);
        //	writer.WriteEndElement();
        //}
        //public static void WriteExtent(XmlWriter writer, OkEnvelope env,string name="Extent")
        //      {
        //          if (env == null)
        //          {
        //              return;
        //          }
        //          //var env = pgn.EnvelopeInternal;
        //          writer.WriteStartElement(name);
        //          WriteDoubleElement(writer, "MinX", env.MinX);
        //          WriteDoubleElement(writer, "MaxX", env.MaxX);
        //          WriteDoubleElement(writer, "MinY", env.MinY);
        //          WriteDoubleElement(writer, "MaxY", env.MaxY);
        //          WriteSpatialReference(writer,env.SpatialReference);
        //          writer.WriteEndElement();
        //      }

        public static OkEnvelope? ReadExtent(XmlElement reader)//, string name = "Extent")
        {
            double? MinX = null, MinY = null, MaxX = null, MaxY = null;
            SpatialReference? sr = null;
            foreach(XmlNode n in reader.ChildNodes)
            {
                if(n is XmlElement e)
                {
                    switch (n.Name)
                    {
                        case "MinX":MinX = SafeConvertAux.ToDouble(n.InnerText);break;
                        case "MinY": MinY = SafeConvertAux.ToDouble(n.InnerText); break;
                        case "MaxX": MaxX = SafeConvertAux.ToDouble(n.InnerText); break;
                        case "MaxY": MaxY = SafeConvertAux.ToDouble(n.InnerText); break;
                    }
                }
            }
            //ReadNodeCallback(reader, name, n =>
            //{
            //    switch (n)
            //    {
            //        case "MinX":
            //            MinX = ReadDouble(reader, n);
            //            break;
            //        case "MinY":
            //            MinY = ReadDouble(reader, n);
            //            break;
            //        case "MaxX":
            //            MaxX = ReadDouble(reader, n);
            //            break;
            //        case "MaxY":
            //            MaxY = ReadDouble(reader, n);
            //            break;
            //        case "SpatialReference":
            //            {
            //                sr = SerializeUtil.ReadSpatialReference(reader);
            //            }
            //            break;
            //    }
            //});
            if (MinX != null && MinY != null && MaxX != null && MaxY != null)
            {
                var g = new OkEnvelope((double)MinX, (double)MaxX, (double)MinY, (double)MaxY);
                //var g=GeometryUtil.MakePolygon((double)MinX, (double)MinY, (double)MaxX, (double)MaxY);
                if (sr != null)
                {
                    g.SpatialReference = sr;
                }
                return g;
            }
            return null;
        }

        //public static OkEnvelope? ReadExtent(XmlReader reader,string name = "Extent")
        //{
        //    double? MinX = null, MinY = null, MaxX = null, MaxY = null;
        //    SpatialReference? sr = null;
        //    ReadNodeCallback(reader, name, n =>
        //    {
        //        switch (n)
        //        {
        //            case "MinX":
        //                MinX=ReadDouble(reader, n);
        //                break;
        //            case "MinY":
        //                MinY = ReadDouble(reader, n);
        //                break;
        //            case "MaxX":
        //                MaxX = ReadDouble(reader, n);
        //                break;
        //            case "MaxY":
        //                MaxY = ReadDouble(reader, n);
        //                break;
        //            case "SpatialReference":
        //                {
        //                    sr = SerializeUtil.ReadSpatialReference(reader);
        //                }
        //                break;
        //        }
        //    });
        //    if (MinX != null && MinY != null && MaxX != null && MaxY != null)
        //    {
        //        var g = new OkEnvelope((double)MinX, (double)MaxX, (double)MinY,(double)MaxY);
        //        //var g=GeometryUtil.MakePolygon((double)MinX, (double)MinY, (double)MaxX, (double)MaxY);
        //        if (sr != null)
        //        {
        //            g.SpatialReference=sr;
        //        }
        //        return g;
        //    }
        //    return null;
        //}
        //public static void WriteSpatialReference(XmlWriter writer, SpatialReference? sr)
        //{
        //    if (sr == null)
        //    {
        //        return;
        //    }
        //    writer.WriteStartElement("SpatialReference");
        //    WriteStringElement(writer, "Authority", sr.Authority??"");
        //    writer.WriteEndElement();
        //}
        public static SpatialReference? ReadSpatialReference(XmlElement reader)
        {
            SpatialReference? sr = null;
           foreach(var n in reader.ChildNodes)
            {
                if(n is XmlElement e && e.Name== "Authority")
                {
                    var authority = e.InnerText;
                    if (authority != null)
                    {
                        sr = SpatialReferenceFactory.CreateFromAuthority(authority);
                    }
                    break;
                }
            }
            //var authority = ReadStringElement(reader, "Authority");
            //if (authority != null)
            //{
            //    sr = SpatialReferenceFactory.CreateFromAuthority(authority);
            //}
            //reader.ReadEndElement();
            return sr;
        }
        public static SpatialReference? ReadSpatialReference(XmlReader reader)
        {
            SpatialReference? sr = null;
            reader.ReadStartElement();
            var authority = ReadStringElement(reader, "Authority");
            if (authority != null)
            {
                sr = SpatialReferenceFactory.CreateFromAuthority(authority);
            }
            reader.ReadEndElement();
            return sr;
        }
        public static void WriteAttribute(XmlWriter writer, string name, string val)
        {
            writer.WriteStartAttribute(name);
            writer.WriteString(val);
            writer.WriteEndAttribute();
        }
        public static string? ReadAttribute(XmlReader reader, string name)
        {
            if (reader.MoveToAttribute(name))
                return reader.Value;
            return null;
        }

        public static void ReadLayers(XmlElement reader, Map? map, Action<ILayer> callback , SerializeSession session)
        {
            foreach(var n in reader.ChildNodes)
            //SerializeUtil.ReadNodeCallback(reader, endElementName, name =>
            {
                if (n is XmlElement e) {
                    var layer = SerializeUtil.CreateInstance<ILayer>(e.Name);
                    if (layer != null)
                    {
                        layer.Map = map;
                        layer.ReadXml(e, session);
                        callback(layer);
                    }
                }
            }
        }

     //   public static void ReadLayers(XmlReader reader,Map map,string endElementName, Action<ILayer> callback
     //       , SerializeSession session)
     //   {
     //       SerializeUtil.ReadNodeCallback(reader, endElementName, name =>
     //       {
     //           var layer = SerializeUtil.CreateInstance<ILayer>(name);
     //           if (layer != null)
     //           {
     //               layer.Map = map;
					//layer.ReadXml(reader, session);
     //               callback(layer);
     //           }
     //       });
     //   }
    }
}
