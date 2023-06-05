using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Agro.LibCore;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;


/*
yxm created at 2019/1/21 9:37:49
*/
namespace Agro.GIS
{
	public class ScaleText : ElementBase, IScaleText, IDisposable
	{
		private Map _map;
		public ScaleText() //: base(new EnvelopeTracker(false, OkEnvelopeConstraints.okEnvelopeConstraintsAspect))
		{
			(NumberFormat as INumericFormat).RoundingValue = 0;
		}
		#region IScaleText
		public string Text { get; private set; }

		public ITextSymbol Symbol { get; set; } = GisGlobal.SymbolFactory.CreateTextSymbol();// new TextSymbol();
		public OkScaleTextStyleEnum Style { get; set; } = OkScaleTextStyleEnum.okScaleTextAbsolute;
		public string Format { get; set; }
		public OkUnits PageUnits { get; set; }
		public OkUnits MapUnits { get; set; }
		public string PageUnitLabel { get; set; }
		public string MapUnitLabel { get; set; }
		public INumberFormat NumberFormat { get; set; } = new NumericFormat();
		public Map Map { get { return _map; }
			set
			{
				if (_map is Map m0)
				{
					m0.OnTransformChanged -= OnTransformChanged;
				}

					_map = value;
				if (value is Map m)
				{
					m.OnTransformChanged += OnTransformChanged;
				}
			}
		}

		//public string MapID { get; private set; }
		#endregion
		#region IElement
		public override IPolygon? QueryOutline()//IDisplay pIDisplay,IDisplayTransformation trans)
		{
			if (Symbol == null || Geometry == null && m_pIDisplay != null)
				return null;
			var pIDisplay = m_pIDisplay!;

            //var pIDT = _trans;// pIDisplay.DisplayTransformation;

			CreateScaleTextElement(pIDisplay);
			Symbol.Text = Text;

			var pIOutline = Symbol.QueryBoundary(pIDisplay,_trans, Geometry);
			if (Symbol is IFormattedTextSymbol fts)
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
		public override void Draw(/*IDisplay dc,IDisplayTransformation trans,*/ ICancelTracker cancel, OutputMode mode)
		{
			//if (m_pIDisplay == null) return;
			var dc = m_pIDisplay!;
			CreateScaleTextElement(dc);
			Symbol.SetupDC(dc,_trans);//.Graphics, dc.DisplayTransformation);
			Symbol.Text = this.Text;
			Symbol.Draw(Geometry);
			Symbol.ResetDC();
		}
		#endregion

		//protected override IElementPropertyPage GetPropertyPage()
		//{
		//	return new ScaleTextPropertyPage();
		//}

		protected override void ReadXmlElement(XmlElement e, SerializeSession session)
		{
			var name = e.Name;
			var s = e.InnerText;
            switch (name)
			{
				case "MapID":
                    {
						var mapID = s;
                        session._mapID[this] = mapID;
                    }
					break;
				case "Symbol":
					if (SerializeUtil.ReadSymbol(e) is ITextSymbol t)
					{
						Symbol = t;
					}
					break;
                case "Style":
					Style = (OkScaleTextStyleEnum)Enum.Parse(typeof(OkScaleTextStyleEnum), s);
                    break;
                case "PageUnits":
                    PageUnits = (OkUnits)Enum.Parse(typeof(OkUnits),s);
                    break;
                case "MapUnits":
                    MapUnits = (OkUnits)Enum.Parse(typeof(OkUnits), s);
                    break;
                case "PageUnitLabel":
                    PageUnitLabel = s;
                    break;
                case "MapUnitLabel":
                    PageUnitLabel = s;
                    break;
                case "NumericFormat":
                    var nf = NumberFormat as NumericFormat;
                    nf ??= new NumericFormat();
					nf.ReadXml(e);//, session, name);
					NumberFormat = nf;
					break;
                default:
                    System.Diagnostics.Debug.Assert(false, $"not process Property ${name}");
                    break;
			}
		}

		//public override void ReadXml(XmlReader reader, SerializeSession session)
		//{
		//	SerializeUtil.ReadNodeCallback(reader, this.GetType().Name, name =>
		//	{
		//		switch (name)
		//		{
		//			case "MapID":
		//				{
		//					var mapID = reader.ReadString();
		//					session._mapID[this] = mapID;
		//				}
		//				break;
		//			case "Symbol":
		//				Symbol = SerializeUtil.ReadSymbol(reader, name) as ITextSymbol;
		//				break;
		//			case "Style":
		//				Style = (OkScaleTextStyleEnum)Enum.Parse(typeof(OkScaleTextStyleEnum), reader.ReadString());
		//				break;
		//			case "PageUnits":
		//				PageUnits = (OkUnits)Enum.Parse(typeof(OkUnits), reader.ReadString());
		//				break;
		//			case "MapUnits":
		//				MapUnits = (OkUnits)Enum.Parse(typeof(OkUnits), reader.ReadString());
		//				break;
		//			case "PageUnitLabel":
		//				PageUnitLabel = reader.ReadString();
		//				break;
		//			case "MapUnitLabel":
		//				PageUnitLabel = reader.ReadString();
		//				break;
		//			case "NumericFormat":
		//				var nf = NumberFormat as NumericFormat;
		//				if (nf == null)
		//				{
		//					nf = new NumericFormat();
		//				}
		//				nf.ReadXml(reader, session, name);
		//				NumberFormat = nf;
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
		//	SerializeUtil.WriteSymbol(writer, "Symbol", Symbol);
		//	SerializeUtil.WriteStringElement(writer, "Style", Style.ToString());

		//	if (Style != OkScaleTextStyleEnum.okScaleTextAbsolute)
		//	{
		//		SerializeUtil.WriteStringElement(writer, "PageUnits", PageUnits.ToString());
		//		SerializeUtil.WriteStringElement(writer, "MapUnits", MapUnits.ToString());
		//		if (PageUnitLabel != null)
		//		{
		//			SerializeUtil.WriteStringElement(writer, "PageUnitLabel", PageUnitLabel.ToString());
		//		}
		//		if (MapUnitLabel != null)
		//		{
		//			SerializeUtil.WriteStringElement(writer, "MapUnitLabel", MapUnitLabel.ToString());
		//		}
		//	}
		//	if (NumberFormat is NumericFormat mf)
		//	{
		//		mf.WriteXml(writer);
		//	}

		//	writer.WriteEndElement();
		//}


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
				var fontSize = Symbol.Font.FontSize;
				Symbol.Font.FontSize =(float)Math.Round( fontSize * newEnv.Width / env.Width,2);
			}
			return true;
		}

		private void CreateScaleTextElement(IDisplay display)
		{
			double dScaleRatio;
			if (Style ==OkScaleTextStyleEnum.okScaleTextAbsolute)
				dScaleRatio = 1000000;
			else
				dScaleRatio = 352778348;
			if (Map is ActiveView pIAV)
			{
				dScaleRatio=pIAV.Transformation.ScaleRatio;
			}

			string bstrText;
			if (Style ==OkScaleTextStyleEnum.okScaleTextAbsolute)
			{
				var bstrValue=NumberFormat.ValueToString(dScaleRatio);
				bstrText = "1:";
				bstrText+=bstrValue;
			}
			else //okScaleTextRelative 
			{
				bstrText = "1 ";
				bstrText+=PageUnitLabel;
				bstrText+=" 等于 ";
				dScaleRatio=UnitsUtil.ConvertUnits(PageUnits,MapUnits, dScaleRatio);
				var bstrValue=NumberFormat.ValueToString(dScaleRatio);
				bstrText+=bstrValue;
				bstrText+=" ";
				bstrText+=MapUnitLabel;
			}
			this.Text = bstrText;
			//Symbol.Text = bstrText;
		}

		private void OnTransformChanged(TransformChangeEventArgs t)
		{
			//if (Map is IActiveView m)
			//{
			//	if (t.ScaleChanged)
			//	{
			//		m.Host.ActiveView.RefreshCache();
			//	}
			//}
		}

		public void Dispose()
		{
			if (_map is Map m0)
			{
				m0.OnTransformChanged -= OnTransformChanged;
			}
		}
	}

	public class NumericFormat : INumberFormat, INumericFormat
	{
		#region INumericFormat
		public OkRoundingOptionEnum RoundingOption { get; set; } = OkRoundingOptionEnum.OkRoundNumberOfDecimals;
		public int RoundingValue { get; set; } = 6;
		public OkNumericAlignmentEnum AlignmentOption { get; set; } = OkNumericAlignmentEnum.okNAlignLeft;
		public int AlignmentWidth { get; set; } = 16;
		public bool UseSeparator { get; set; } = false;
		public bool ZeroPad { get; set; } = false;
		public bool ShowPlusSign { get; set; } = false;
		#endregion

		#region INumberFormat
		public double StringToValue(string str)
		{
			//todo...
			return 0;
		}

		public string ValueToString(double Value)
		{
			if (RoundingValue < 0 || RoundingValue > 15)
			{
				System.Diagnostics.Debug.Assert(false);
			}
			string str=null;
			if (RoundingOption == OkRoundingOptionEnum.OkRoundNumberOfDecimals)
			{
				str=Math.Round(Value, RoundingValue).ToString();
				if (RoundingValue > 0)
				{
					if (ZeroPad)
					{
						int n = str.LastIndexOf('.');
						int cnt = n + 1 + RoundingValue;
						if (str.Length < cnt)
						{
							str = str.PadRight(cnt, '0');
						}
					}
					else
					{
						str = str.TrimEnd('0');
					}
				}

				if (AlignmentOption ==OkNumericAlignmentEnum.okNAlignRight && str.Length < AlignmentWidth)
				{
					int count = AlignmentWidth - str.Length;
					string str1="";
					for (int i = 0; i < count; i++)
					{
						str1 += " ";
					}
					str = str1 + str;
				}
			}
			else
			{
				System.Diagnostics.Debug.Assert(false, "todo...");
			}
			return str;
		}
        #endregion

        public void ReadXml(XmlElement reader)//, SerializeSession session, string elementName = "NumericFormat")
        {
            foreach(var o in reader.ChildNodes)
            {
				if (o is XmlElement e) {
					var name = e.Name;
					var s = e.InnerText;
					switch (name)
					{
						case "RoundingOption":
							RoundingOption = (OkRoundingOptionEnum)Enum.Parse(typeof(OkRoundingOptionEnum), s);
							break;
						case "AlignmentOption":
							AlignmentOption = (OkNumericAlignmentEnum)Enum.Parse(typeof(OkNumericAlignmentEnum),s);
							break;
						case "RoundingValue":
							RoundingValue = SafeConvertAux.ToInt32(s);
							break;
						case "AlignmentWidth":
							AlignmentWidth = SafeConvertAux.ToInt32(s);
							break;
						case "UseSeparator":
							UseSeparator = s == "True";
							break;
						case "ZeroPad":
							ZeroPad = s== "True";
							break;
						case "ShowPlusSign":
							ShowPlusSign =s== "True";
							break;
						default:
                            System.Diagnostics.Debug.Assert(false, $"not process Property ${name}");
                            break;
					}
				}
            }
        }

  //      public void ReadXml(XmlReader reader, SerializeSession session, string elementName = "NumericFormat")
		//{
		//	SerializeUtil.ReadNodeCallback(reader, elementName, name =>
		//	{
		//		switch (name)
		//		{
		//			case "RoundingOption":
		//				RoundingOption = (OkRoundingOptionEnum)Enum.Parse(typeof(OkRoundingOptionEnum), reader.ReadString());
		//				break;
		//			case "AlignmentOption":
		//				AlignmentOption = (OkNumericAlignmentEnum)Enum.Parse(typeof(OkNumericAlignmentEnum), reader.ReadString());
		//				break;
		//			case "RoundingValue":
		//				RoundingValue = SafeConvertAux.ToInt32(reader.ReadString());
		//				break;
		//			case "AlignmentWidth":
		//				AlignmentWidth = SafeConvertAux.ToInt32(reader.ReadString());
		//				break;
		//			case "UseSeparator":
		//				UseSeparator = reader.ReadString()=="True";
		//				break;
		//			case "ZeroPad":
		//				ZeroPad = reader.ReadString()=="True";
		//				break;
		//			case "ShowPlusSign":
		//				ShowPlusSign = reader.ReadString()=="True";
		//				break;
		//			//default:
		//			//	base.ReadXmlElement(reader, name);
		//			//	break;
		//		}
		//	});
		//}
		//public void WriteXml(XmlWriter writer,string elementName= "NumericFormat")
		//{
		//	writer.WriteStartElement(elementName);// this.GetType().Name);

		//	SerializeUtil.WriteStringElement(writer, "RoundingOption", RoundingOption.ToString());
		//	SerializeUtil.WriteStringElement(writer, "AlignmentOption", AlignmentOption.ToString());
		//	SerializeUtil.WriteIntElement(writer, "RoundingValue", RoundingValue);
		//	if (AlignmentOption == OkNumericAlignmentEnum.okNAlignRight)
		//	{
		//		SerializeUtil.WriteIntElement(writer, "AlignmentWidth", AlignmentWidth);
		//	}
		//	SerializeUtil.WriteBoolElement(writer, "UseSeparator", UseSeparator);
		//	SerializeUtil.WriteBoolElement(writer, "ZeroPad", ZeroPad);
		//	SerializeUtil.WriteBoolElement(writer, "ShowPlusSign", ShowPlusSign);

		//	writer.WriteEndElement();
		//}
	}
}
