using Agro.GIS;
using Agro.LibCore;
using GeoAPI.Geometries;
using SkiaSharp;
using System.Drawing;
using System.Xml;


/*
yxm created at 2019/1/15 11:37:46
*/
namespace SkiaMap.Symbol
{
    public class TextSymbol : Symbol<TextSymbol>, ITextSymbol
	{
        protected readonly SKPaint _paint = new() { IsAntialias = true, Style = SKPaintStyle.StrokeAndFill };

        /// <summary>
        /// yxm 2019-1-14
        /// 实现竖排文字
        /// </summary>
        class VerticalTextOut
		{
			class Word
			{
				//public RectangleF rc;
				public SizeF Size=new();
				public string word="";
			}
			private readonly List<Word> _lst = new List<Word>();
			public float Width;
			public float Height;

			/// <summary>
			/// 行间距，单位：Points
			/// </summary>
			public const double HGAP =1.1;
			//private GraphicsPath _path = new GraphicsPath();
			private readonly TextSymbol _p;
			public VerticalTextOut(TextSymbol p)
			{
				_p = p;
			}

			//public void ConstructPath(PointF pt, GraphicsPath path)
			//{
			//	Init(_p.Text);
			//	var iGap = AdjustGap();
			//	var origin = new PointF(0, 0);
			//	var y = pt.Y - Height / 2;
			//	var x = pt.X;// +(Width-w.rc.Width)/2;
			//	for (int i = 0; i < _lst.Count; ++i)
			//	{
			//		var w = _lst[i];
			//		var text = w.word;
			//		//y += w.rc.Height / 2;
			//		origin.X = (float)x;
			//		origin.Y = (float)(y + w.rc.Height / 2);
			//		path.AddString(text, _p._font.FontFamily, (int)_p._font.Style, (float)_p._font.Size, origin, _p._format);
			//		y += w.rc.Height+iGap;
			//	}
			//}

			public RectangleF GetBounds(PointF origin)
			{
				var x = origin.X - Width * 0.5;
				var y = origin.Y - Height * 0.5;
				var rc=new RectangleF((float)x, (float)y, Width, Height);
				return rc;
				//throw new NotImplementedException();
				//using (var path = new GraphicsPath())
				//{
				//	ConstructPath(origin, path);
				//	return path.GetBounds();
				//}
			}
			private void Init(string text)
			{
				_lst.Clear();
				//var origin = new PointF(0, 0);
				float width = 0;
				float height = 0;

				double iGap = AdjustGap();// HGAP;
				var dpi = _p._trans!.Resolution;
				if (dpi != 0)
				{
					iGap = DpiUtil.POINTS2PIXELS2(iGap, dpi);
				}
				var paint = _p._paint;
				var sz = new SKRect();
				foreach (var ch in text)
				{
					var str = ch.ToString();
					paint.MeasureText(str, ref sz);
					//var rc = paint.QueryTextBound(str, origin);
					//rc.Height += (float)iGap;// HGAP;
					var word=new Word()
					{
						word = str,
					};
					word.Size.Width = sz.Width;
					word.Size.Height = sz.Height;	

                    _lst.Add(word);
					if (width < sz.Width)
						width = sz.Width;
					height += sz.Height;
				}
				Width = width;
				Height = height+(float)((_lst.Count-1)*iGap);
			}
			private float AdjustGap()
			{
				double iGap = HGAP;
				var dpi = _p._trans!.Resolution;
				if (dpi != 0)
				{
					iGap = DpiUtil.POINTS2PIXELS2(iGap, dpi);
					//iGap = _p._trans.ToMapSizeX(iGap);
				}
				return (float)iGap;
			}

			internal void Draw(IPoint pt)
			{
                Init(_p.Text);
                //_vto.ConstructPath(origin, _path);
                //	Init(_p.Text);
				var paint= _p._paint;
				var canvas = ((SkiaDisplay)_p._dc).GetCanvas();
                var iGap = AdjustGap();
                var origin = new PointF(0, 0);
                var y = pt.Y - Height / 2;
                var x = pt.X;// +(Width-w.rc.Width)/2;
                var lst = _lst;
                for (int i = 0; i < lst.Count; ++i)
                {
                    var w = lst[i];
                    var text = w.word;
                    //y += w.rc.Height / 2;
                    origin.X = (float)x;
                    origin.Y = (float)(y + w.Size.Height / 2);

                    var sz = new SKRect();
                    paint.MeasureText(text, ref sz);
                    canvas.DrawText(text, (float)(origin.X - sz.Width * 0.5), (float)(origin.Y + sz.Height * 0.5), paint);


                    //path.AddString(text, _p._font.FontFamily, (int)_p._font.Style, (float)_p._font.Size, origin, _p._format);
                    y += w.Size.Height + iGap;
                }
            }
		}

        //private GraphicsPath _path;
        //private readonly StringFormat _format = StringFormat.GenericDefault;// new System.Drawing.StringFormat();
        //private Font _font;
        //private SolidBrush _GlowEffectSolidBrush;//((Color)ge.GlowEffect;
        //private Pen _GlowEffectPen;
        //private Pen _pen;
        //private SolidBrush _brush;



        private readonly VerticalTextOut _vto;

		public TextSymbol()//double fontSize = 10)
		{

			this.Font = new FontDisp();
			this.Color = Color.Black;
			Font.FontSize = 10;// fontSize;
			Font.FontStyle = eFontStyle.Regular;
			Font.FontFamily = "微软雅黑";

			GlowEffect = new GlowEffect(Color.White, 1.5);// 3);

			//_format.Alignment = StringAlignment.Center;
			//_format.LineAlignment = StringAlignment.Center;
			this.HorizontalAlignment = eTextHorizontalAlignment.eTHACenter;
			this.VerticalAlignment = eTextVerticalAlignment.eTVACenter;

			_vto = new VerticalTextOut(this);

		}
		public FontDisp Font
		{
			get;
			set;
		}

		public Color Color
		{
			get;
			set;
		}

		public double OffsetX
		{
			get;
			set;
		}

		public double OffsetY
		{
			get;
			set;
		}

		public eTextHorizontalAlignment HorizontalAlignment
		{
			get;
			set;
		}

		public eTextVerticalAlignment VerticalAlignment
		{
			get;
			set;
		}

		public GlowEffect GlowEffect
		{
			get;
			set;
		}

		/// <summary>
		/// 是否竖排文字（从上到下显示）
		/// </summary>
		public bool IsVerticalTextDirection { get; set; }

		public string Text
		{
			get;
			set;
		} = "";

		public bool AllowGlowEffect => GlowEffect != null && GlowEffect.GlowEffectColor != null && GlowEffect.Enable;

		//public void GetTextSize(Graphics g, Transform trans, string Text, out double xSize, out double ySize)
		//{
		//	xSize = 0;
		//	ySize = 0;
		//}

		//public ITextBackground Background
		//{
		//    get;
		//    set;
		//}

		public override void SetupDC(IDisplay dc,IDisplayTransformation trans)
		{
			base.SetupDC(dc, trans);
			Dispose();


            if (Font.IsValid)
            {
                var fontSize = Font.FontSize;
                var dpi = _trans == null ? 0 : _trans.Resolution;
                if (dpi != 0)
                {
                    fontSize = DpiUtil.POINTS2PIXELS2(Font.FontSize, dpi);
                }

                _paint.Color = this.Color.ToSkColor();
                _paint.TextSize = (float)fontSize;
                _paint.Typeface = SKFontManager.Default.MatchFamily(Font.FontFamily);
				if (_paint.Typeface == null)// && Font.FontFamily == "微软雅黑")
				{
					_paint.Typeface = SKFontManager.Default.MatchFamily("Microsoft YaHei");
				}
				if(_paint.Typeface == null)
				{
					Console.WriteLine("微软雅黑 not find");
				}

                //var font = this.Font;



                //_font = new Font(font.FontFamily, (float)fontSize, (FontStyle)font.FontStyle);
            }

            //var font = this.Font;
            //_font = new Font(font.FontFamily, (float)fontSize, (FontStyle)font.FontStyle);
            //_brush = new SolidBrush(this.Color);
            //_pen = new Pen(this.Color);

            //_path = new GraphicsPath();

            //if (AllowGlowEffect)
            //{
            //	var dGlowEffectSize = DpiUtil.POINTS2PIXELS2(GlowEffect.GlowEffectSize, dpi);

            //	_GlowEffectSolidBrush = new SolidBrush((Color)GlowEffect.GlowEffectColor);
            //	_GlowEffectPen = new Pen(_GlowEffectSolidBrush, (float)dGlowEffectSize);
            //}



            //switch (HorizontalAlignment)
            //{
            //	case eTextHorizontalAlignment.eTHACenter:
            //		_format.Alignment = StringAlignment.Center;
            //		break;
            //	case eTextHorizontalAlignment.eTHALeft:
            //		_format.Alignment = StringAlignment.Near;
            //		break;
            //	case eTextHorizontalAlignment.eTHARight:
            //		_format.Alignment = StringAlignment.Far;
            //		break;
            //}
            //switch (VerticalAlignment)
            //{
            //	case eTextVerticalAlignment.eTVACenter:
            //		_format.LineAlignment = StringAlignment.Center;
            //		break;
            //	case eTextVerticalAlignment.eTVATop:
            //		_format.LineAlignment = StringAlignment.Near;
            //		break;
            //	case eTextVerticalAlignment.eTVABottom:
            //		_format.LineAlignment = StringAlignment.Far;
            //		break;
            //}
        }
		public new void ResetDC()
		{
			Dispose();
			base.ResetDC();
		}

		public void Draw(IGeometry g, object? extension = null, bool fDeviceCoords = false)
		{
			if (string.IsNullOrEmpty(Text) || Font == null || g == null || g.IsEmpty)
			{
				return;
			}
			var trans = _trans;
			var dc = _dc;


			if (trans != null)
			{
				if (!fDeviceCoords)
				{
					g = trans!.ToDevice(g!)!;
				}
			}

			var pt = g.Centroid;

			DrawText(pt);

#if DEBUG
			if (true)//test
			{
				var markerSymbol = new SimpleMarkerSymbol();
				markerSymbol.SetupDC(_dc!,trans);
				markerSymbol.Draw(pt, null, true);
				markerSymbol.ResetDC();
			}
#endif
		}

		public new IPolygon? QueryBoundary(IDisplay dc,IDisplayTransformation trans, IGeometry Geometry)
		{
			if (Text == null)
				return null;

			IPolygon? polygon = null;
			if (Geometry is IPoint pt)
			{
				bool fSetupDC = dc != _dc;
				if (fSetupDC)
				{
					SetupDC(dc, trans);
				}
				//var trans = dc.DisplayTransformation;

				pt.Project(trans.SpatialReference);
				pt = (IPoint)trans.ToDevice(pt)!;

				Envelope env;

                var origin = AdjustTextOrigin(new PointF((float)pt.X, (float)pt.Y));

				//var rc = IsVerticalTextDirection ? _vto.GetBounds(origin) : GDIPlusUtil.QueryTextBound(Text, _font, _format, origin);

				//var env = new Envelope(rc.Left, rc.Right, rc.Top, rc.Bottom);

				if (IsVerticalTextDirection)
				{
					var rc = _vto.GetBounds(origin);
                    env = new Envelope(rc.Left, rc.Right, rc.Top, rc.Bottom);
				}
				else
				{
                    var rc = _paint.QueryTextBound(Text,origin);
                    env = new Envelope(rc.Left, rc.Right, rc.Top, rc.Bottom);
                }

				polygon = GeometryUtil.MakePolygon(env);
				polygon =(IPolygon)trans.ToMap(polygon)!;
				polygon!.SetSpatialReference(Geometry.GetSpatialReference());

				if (fSetupDC)
				{
					ResetDC();
				}
			}
			return polygon;
		}


		private void DrawText(IPoint pt)
		{

			//var origin = new PointF((float)pt.X, (float)pt.Y);// (float)env.MinX,(float)env.MinY);
			//origin = AdjustTextOrigin(origin);
			var canvas = ((SkiaDisplay)_dc).GetCanvas()!;
			if (IsVerticalTextDirection)
			{
				_vto.Draw(pt);
			}
            else
			{

                var origin = new PointF((float)pt.X, (float)pt.Y);
                AdjustTextOrigin(origin);

                var sz = new SKRect();
                _paint.MeasureText(Text, ref sz);

				var left = (float)(origin.X - sz.Width * 0.5);

#if DEBUG
				canvas.DrawRect(left,(float)(origin.Y-sz.Height*0.5), sz.Width,sz.Height,new SKPaint()
					{
						Style=SKPaintStyle.Stroke,
						Color=SKColor.Parse("#FF0000")
					});
#endif

				canvas.DrawText(Text,left, (float)(origin.Y + sz.Height * 0.5), _paint);


                //_path.AddString(Text, _font.FontFamily, (int)_font.Style, (float)_font.Size, origin, _format);
            }
            ////_dc.FillPath(_GlowEffectSolidBrush, _path);
            //if (AllowGlowEffect)
            //	_dc.DrawPath(_GlowEffectPen, _path);
            //_dc.FillPath(_brush, _path);
            ////_dc.DrawPath(_pen, _path);


        }

		/// <summary>
		/// 修正文本绘制原点
		/// </summary>
		/// <param name="screenX"></param>
		/// <param name="screenY"></param>
		/// <returns></returns>
		private PointF AdjustTextOrigin(PointF origin)//double screenX, double screenY)
		{
			double dx,dy;
			if (IsVerticalTextDirection)
			{
				var rc = _vto.GetBounds(origin); //: GDIPlusUtil.QueryTextBound(Text, _font, _format, origin);
				dx = (rc.Left + rc.Right) / 2.0 - origin.X;
				dy = (rc.Top + rc.Bottom) / 2.0 - origin.Y;
			}
			else {
				var rc =_paint.QueryTextBound(Text,  origin);
				dx = (rc.Left + rc.Right) / 2.0 - origin.X;
				dy = (rc.Top + rc.Bottom) / 2.0 - origin.Y;
			}
			double x = origin.X - dx;
			double y = origin.Y - dy;
			var dpi = _trans.Resolution;
			if (OffsetX != 0)
			{
				var offsetX = OffsetX;
				if (dpi != 0)
				{
					offsetX = DpiUtil.POINTS2PIXELS2(offsetX, dpi);
				}
				x += offsetX;
			}
			if (OffsetY != 0)
			{
				var offsetY = OffsetY;
				if (dpi != 0)
				{
					offsetY = DpiUtil.POINTS2PIXELS2(offsetY, dpi);
				}
				y += offsetY;
			}
			origin.X = (float)x;
			origin.Y = (float)y;
			return origin;
		}
		private Envelope CalcBounds(double screenX, double screenY)
		{
			throw new NotImplementedException();
			/*
			var sz = TextUtil.MeasureString(Text, _font, _format, 0);// MeasureString(_dc, Text, _font);

			//var x = pt.X;
			//var y = pt.Y;
			switch (HorizontalAlignment)
			{
				case eTextHorizontalAlignment.eTHACenter:
					screenX = screenX - sz.Width / 2;
					break;
				case eTextHorizontalAlignment.eTHALeft:
					screenX = screenX - sz.Width;
					break;
			}
			switch (VerticalAlignment)
			{
				case eTextVerticalAlignment.eTVACenter:
					screenY = screenY - sz.Height / 2;
					break;
				case eTextVerticalAlignment.eTVATop:
					screenY = screenY - sz.Height;
					break;
			}
			var dpi = _trans.Resolution;
			if (OffsetX != 0)
			{
				var offsetX = OffsetX;
				if (dpi != 0)
				{
					offsetX = DpiUtil.POINTS2PIXELS2(offsetX, dpi);
				}
				screenX += offsetX;
			}
			if (OffsetY != 0)
			{
				var offsetY = OffsetY;
				if (dpi != 0)
				{
					offsetY = DpiUtil.POINTS2PIXELS2(offsetY, dpi);
				}
				screenY += offsetY;
			}


			double xExpand = 0;
			var env = new Envelope(screenX, screenX + sz.Width + 2 * xExpand, screenY, screenY + sz.Height);
			return env;*/
		}

		//private SizeF MeasureString(Graphics g, string text, Font font)
		//{
		//	if (true)
		//	{
		//		//var sz = TextUtil.MeasureString(text, font, _format, 0);// _trans.Resolution);//, StringFormat.GenericTypographic);

		//		//return sz;
		//		// Declare a proposed size with dimensions set to the maximum integer value.
		//		Size proposedSize = new Size(int.MaxValue, int.MaxValue);
		//		// Set TextFormatFlags to no padding so strings are drawn together.
		//		var flags = System.Windows.Forms.TextFormatFlags.NoPadding;


		//		var sz = System.Windows.Forms.TextRenderer.MeasureText(g, text, font, proposedSize, flags);
		//		//return sz;
		//		var sz1 = TextUtil.MeasureString(text, font, _format, 0);
		//		return sz1;
		//	}
		//	else
		//	{
		//		StringFormat sf = StringFormat.GenericTypographic;
		//		sf.Trimming = StringTrimming.Word;
		//		//sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

		//		SizeF s = g.MeasureString(text, font, 0, sf);
		//		return s;
		//	}
		//}

		//public override void WriteXml(XmlWriter writer)
		//{
		//	SerializeUtil.WriteColorElement(writer, "Color", Color);
		//	SerializeUtil.WriteFont(writer, "Font", Font);
		//	SerializeUtil.WriteStringElement(writer, "HorizontalAlignment", HorizontalAlignment.ToString());
		//	SerializeUtil.WriteStringElement(writer, "VerticalAlignment", VerticalAlignment.ToString());
		//	SerializeUtil.WriteBoolElement(writer, "IsVerticalTextDirection", IsVerticalTextDirection);
		//	if (AllowGlowEffect)
		//	{
		//		GlowEffect.WriteXml(writer);
		//	}
		//}
		//public override void ReadXml(XmlReader reader)
		//{
		//	SerializeUtil.ReadNodeCallback(reader, "Symbol", name =>
		//	{
		//		switch (name)
		//		{
		//			case "Color":
		//				Color = (Color)SerializeUtil.ReadColor(reader, name);
		//				break;
		//			case "Font":
		//				Font=SerializeUtil.ReadFontDisp(reader, name,Font);
		//				break;
		//			case "HorizontalAlignment":
		//				HorizontalAlignment = (eTextHorizontalAlignment)Enum.Parse(typeof(eTextHorizontalAlignment), reader.ReadString());
		//				break;
		//			case "VerticalAlignment":
		//				VerticalAlignment =(eTextVerticalAlignment)Enum.Parse(typeof(eTextVerticalAlignment), reader.ReadString());
		//				break;
		//			case "IsVerticalTextDirection":
		//				IsVerticalTextDirection = reader.ReadString()=="True";
		//				break;
		//			case "GlowEffect":
		//				GlowEffect = SerializeUtil.ReadGlowEffect(reader, name, GlowEffect);
		//				break;
		//		}
		//	});
		//}


        public override void ReadXml(XmlElement reader)
        {
            foreach(var o in reader.ChildNodes)
            {
				if (o is XmlElement e)
				{
					var s = e.InnerText;
					var name = e.Name;
					switch (name)
					{
						case "Color":
							Color = ColorUtil.ConvertFromString(s);// (Color)SerializeUtil.ReadColor(reader, name);
							break;
						case "Font":
							//Font = SerializeUtil.ReadFontDisp(reader, name, Font);
							{
                                Font ??= new FontDisp();
                                Font.ReadXml(e);
                            }
							break;
						case "HorizontalAlignment":
							HorizontalAlignment = (eTextHorizontalAlignment)Enum.Parse(typeof(eTextHorizontalAlignment), s);
							break;
						case "VerticalAlignment":
							VerticalAlignment = (eTextVerticalAlignment)Enum.Parse(typeof(eTextVerticalAlignment), s);
							break;
						case "IsVerticalTextDirection":
							IsVerticalTextDirection = s== "True";
							break;
						case "GlowEffect":
                            //GlowEffect = SerializeUtil.ReadGlowEffect(reader, name, GlowEffect);
                            GlowEffect ??= new GlowEffect();
                            GlowEffect.ReadXml(e);
                            break;
					}
				}
            }
        }

  //      public object Clone()
		//{
		//	throw new NotImplementedException();
		//}

		private void Dispose()
		{
			//DisposeUtil.SafeDispose(ref _font);
			//DisposeUtil.SafeDispose(ref _path);
			//DisposeUtil.SafeDispose(ref _brush);
			//DisposeUtil.SafeDispose(ref _pen);
			//DisposeUtil.SafeDispose(ref _GlowEffectPen);
			//DisposeUtil.SafeDispose(ref _GlowEffectSolidBrush);
		}

		public RectangleF MeasureText(string text)
		{
			var rc = new SKRect();
			_paint.MeasureText(text, ref rc);
			return new RectangleF(rc.Left,rc.Top,rc.Width,rc.Height);
		}
	}
}
