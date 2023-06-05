using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;


/*
yxm created at 2019/1/16 16:54:41
*/
namespace Agro.GIS
{
	public class MarkerNorthArrow :ElementBase, IMarkerNorthArrow
	{
		public MarkerNorthArrow()//:base(new EnvelopeTracker(false,OkEnvelopeConstraints.okEnvelopeConstraintsAspect))
		{
			var ms = GisGlobal.SymbolFactory.CreateCharacterMarkerSymbol();// new CharacterMarkerSymbol();
			ms.Font.FontFamily = "ESRI North";
			ms.CharacterIndex = 177;// 180;
			ms.Size = 62;
			MarkerSymbol = ms;
			Color = Color.Black;
		}

		#region IMapSurround
		public Map Map { get; set; }
		//public string MapID { get; private set; }
		#endregion

		#region IMarkerNorthArrow
		public IMarkerSymbol MarkerSymbol { get; set; }
		public Color Color { get { return (Color)MarkerSymbol.FillColor; } set
			{
				MarkerSymbol.FillColor = value;
			}
		}
		public double Size
		{
			get
			{
				return MarkerSymbol.Size;
			}
			set { MarkerSymbol.Size = value; }
		}

		public double Angle
		{
			get
			{
				return 0.0;
			}
		}

		public double CalibrationAngle { get; set; }
		public IPoint ReferenceLocation { get; set; }
		#endregion

		#region IElement
		public override IPolygon? QueryOutline()//IDisplay pIDisplay,IDisplayTransformation trans)
		{
			var Symbol = MarkerSymbol;
			if (Symbol == null || Geometry == null && m_pIDisplay != null)
				return null;
			var pIOutline = Symbol.QueryBoundary(m_pIDisplay!,_trans, Geometry!);
			return pIOutline;
		}
		public override void Draw(/*IDisplay dc,IDisplayTransformation trans, */ICancelTracker cancel, OutputMode mode)
		{
			if (Geometry != null&&m_pIDisplay!=null)
			{
				var pISymbol = MarkerSymbol;
				pISymbol.SetupDC(m_pIDisplay!, _trans);
				pISymbol.Draw(Geometry);
				pISymbol.ResetDC();
			}
		}
		#endregion

		

		public override void Transform(AffineTransformation trans)
		{
			base.Transform(trans);
			FitToBounds(trans);
		}

		private bool FitToBounds(AffineTransformation trans)
		{
			var env = this.QueryBounds();// this.m_pIDisplay);
			if (env.Width != 0)
			{
				var newEnv = trans.Transform(env).EnvelopeInternal;
				this.Size = Math.Round(this.Size * newEnv.Width / env.Width, 2);
			}
			return true;
		}

		protected override void ReadXmlElement(XmlElement e, SerializeSession session)
		{
			var name = e.Name;
			switch (name)
			{
				case "MapID":
					{
						var mapID = e.InnerText;
						session._mapID[this] = mapID;
					}
					break;
				case "Symbol":
					MarkerSymbol = SerializeUtil.ReadSymbol(e) as IMarkerSymbol;
					break;
				default:
					System.Diagnostics.Debug.Assert(false, $"not process Property ${name}");
					break;
			}
		}

  //      public override void ReadXml(XmlReader reader,SerializeSession session)
		//{
		//	SerializeUtil.ReadNodeCallback(reader, this.GetType().Name, name =>
		//	{
		//	switch (name)
		//	{
		//		case "MapID":
		//			{
		//				var mapID = reader.ReadString();
		//				session._mapID[this] = mapID;
		//			}
		//			break;
		//		case "Symbol":
		//				MarkerSymbol=SerializeUtil.ReadSymbol(reader, name) as IMarkerSymbol;
		//				break;
		//			default:
		//				base.ReadXmlElement(reader, name);
		//				break;
		//		}
		//	});
		//}
		//public override void WriteXml(System.Xml.XmlWriter writer, bool fWriteDataSource = true)
		//{
		//	writer.WriteStartElement(this.GetType().Name);
		//	base.WriteInnerXml(writer);
		//	if (Map != null)
		//	{
		//		SerializeUtil.WriteStringElement(writer, "MapID", (Map as Map).ID);
		//	}
		//	SerializeUtil.WriteSymbol(writer, "Symbol", MarkerSymbol);

		//	writer.WriteEndElement();
		//}

		//protected override IElementPropertyPage GetPropertyPage()
		//{
		//	return new MarkerNorthArrowPropertyPage();
		//}
	}
}
