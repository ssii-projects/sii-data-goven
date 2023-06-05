using System.Reflection.PortableExecutable;
using System.Xml;
using System.Xml.Linq;
using Agro.LibCore;
using GeoAPI.Geometries;

namespace Agro.GIS
{
	/// <summary>
	/// 2019-1-2
	/// </summary>
	public class TextElement : ElementBase, ITextElement
	{
		public ITextSymbol Symbol { get; set; } = GisGlobal.SymbolFactory.CreateTextSymbol();// new TextSymbol();
		public string Text { get; set; }
		public bool ScaleText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


		public TextElement()//:base(new PolygonTracker(true))
		{
			
		}

		public override IPolygon? QueryOutline()//IDisplay pIDisplay,IDisplayTransformation trans)
		{
			if (Symbol == null || Geometry == null)
				return null;
			var pIDisplay = m_pIDisplay!;
            //var pIDT = trans;// pIDisplay.DisplayTransformation;
            Symbol.Text = Text;

			var pIOutline = Symbol.QueryBoundary(pIDisplay,_trans, Geometry);
			if (pIOutline!=null&&Symbol is IFormattedTextSymbol fts)
			{
				var pITB = fts.Background;
				if (pITB != null)
				{
					pITB.TextBoundary = pIOutline;
					pIOutline = pITB.QueryBoundary(pIDisplay);//.Graphics, pIDT);
				}
			}
			return pIOutline;
		}
		public override void Draw(/*IDisplay dc,IDisplayTransformation trans, */ICancelTracker cancel, OutputMode mode)
		{
			if (m_pIDisplay == null)
				return ;
			if (Geometry == null)
				return ;
			if (Symbol != null)
			{
				Symbol.SetupDC(m_pIDisplay!, _trans);//.Graphics, dc.DisplayTransformation);
				Symbol.Text = Text;
				Symbol.Draw(Geometry);
				Symbol.ResetDC();
			}
		}

		//public override void WriteXml(XmlWriter writer, bool fWriteDataSource = true)
		//{
		//	writer.WriteStartElement(this.GetType().Name);
		//	base.WriteInnerXml(writer);
		//	SerializeUtil.WriteStringElement(writer, "Text", Text);

		//	SerializeUtil.WriteSymbol(writer, "Symbol", Symbol);
		//	writer.WriteEndElement();
		//}

		protected override void ReadXmlElement(XmlElement x, SerializeSession session)
		{
			switch (x.Name)
			{
                case "Text":
                    this.Text = x.InnerText;
					break;
				case "Symbol":
					if (SerializeUtil.ReadSymbol(x) is ITextSymbol ts)
					{
						Symbol = ts;
					}
					break;
                default:
                    System.Diagnostics.Debug.Assert(false, $"not process Property ${x.Name}");
                    break;
			}
		}

		//public override void ReadXml(XmlReader reader, SerializeSession session)
		//{
		//	SerializeUtil.ReadNodeCallback(reader, this.GetType().Name, name =>
		//	{
		//		switch (name)
		//		{
		//			case "Text":
		//				this.Text = reader.ReadString();
		//				break;
		//			case "Symbol":
		//				Symbol = SerializeUtil.ReadSymbol(reader, name) as ITextSymbol;
		//				break;
		//			default:
		//				base.ReadXmlElement(reader, name);
		//				break;
		//		}
		//	});
		//}

		//protected override IElementPropertyPage GetPropertyPage()
		//{
		//	return new TextElementPropertyPage();
		//}

		protected override void RefreshTracker()
		{
			//if (base.m_pIDisplay == null)
			//	return;
			//var pIPolygon=this.QueryOutline(m_pIDisplay);

			////var pIFS = Symbol as IFormattedTextSymbol;
			//ITextBackground pITB = null;

			//if (Symbol is IFormattedTextSymbol fts)
			//{
			//	pITB = fts.Background;
			//}
			//if (pITB is ICallout pICallout)
			//{
			//	//var pIPoint=pICallout.AnchorPoint;
			//	//CComQIPtr<IGeometry> pIGeo1 = pIPoint, pIGeo2 = pIPolygon;
			//	//CComPtr<IGeometryCollection> pIGeometryBag;
			//	//pIGeometryBag.CoCreateInstance(CLSID_OkGeometryBag);
			//	//pIGeometryBag.AddGeometry(pIGeo1, NULL, NULL);
			//	//pIGeometryBag.AddGeometry(pIGeo2, NULL, NULL);
			//	//CComQIPtr<IGeometry> pIGeo = pIGeometryBag;
			//	//m_pISelectionTracker.put_Geometry(pIGeo);
			//	throw new NotImplementedException();
			//}
			//else
			//	m_pISelectionTracker.Geometry=pIPolygon;
		}


	}
}
