using Agro.GIS;
using Agro.LibCore;
using Agro.LibCore.Xml;
using GeoAPI.Geometries;
using SkiaSharp;
using System.Drawing;


/*
yxm created at 2019/1/15 11:48:08
*/
namespace SkiaMap.Symbol
{
    static class PenPropertyExtentsion
	{
		public static void SetupPaint(this PenProperty t, SKPaint paint, IDisplayTransformation? dts)
		{
			if (t.StrokeThickness > 0 && t.StrokeColor is Color clr)
			{
				var strokeWidth=t.StrokeThickness;
				if (dts != null)
				{
					strokeWidth= DpiUtil.POINTS2PIXELS2(strokeWidth, dts.Resolution);
                }
				paint.Style = SKPaintStyle.Stroke;
				paint.Color = clr.ToSkColor();
				paint.StrokeWidth =(float)strokeWidth;
			}
		}
	}

    public abstract class SKLineSymbol<T> : LineSymbol<T> where T:IXmlSerializable,new()
    {
        protected readonly SKPaint _paint = new() { IsAntialias = true,Style=SKPaintStyle.Stroke };

        private readonly SKPath path = new();
		#region ISymbol
		public override void SetupDC(IDisplay dc, IDisplayTransformation trans)
		{
			base.SetupDC(dc, trans);

			this.SetupPaint(_paint, trans);
			//if(StrokeThickness>0&& StrokeColor is Color clr)
			//{
			//	_paint.Color = clr.ToSkColor();
			//	_paint.StrokeWidth=DpiUtil.POINTS2PIXELS2(StrokeThickness,_trans.Resolution);
			//}
		}
		public override void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
		{
			var g = geometry;
			if (g == null || g.IsEmpty)
				return;

			if (!fDeviceCoords)
			{
				g = _trans.ToDevice(g);
			}
			if (!g!.Intersects(_trans.GetViewPolygon()))
			{
				return;
			}
			path.Reset();
			path.AddGeometry(g);
			if (_dc is SkiaDisplay dc)
			{
				dc.GetCanvas().DrawPath(path, _paint);
			}
		}
		#endregion
	}
	public class SimpleLineSymbol : SKLineSymbol<SimpleLineSymbol>, ILineSymbol
	{
	}

	public abstract class SKFillSymbol<T> : FillSymbol<T> where T : IXmlSerializable, new()
    {
        protected readonly SKPaint _paint = new() { IsAntialias = true, Style = SKPaintStyle.Fill };
        private readonly SKPath path = new();
        protected SKFillSymbol():base(SkiaSymbolFactory.Instance)
		{
            Outline = new SimpleLineSymbol();
        }

		#region ISymbol
		public void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
		{
			if (Outline == null&&FillColor==null)
			{
				return;
			}
			var trans = _trans;
			var dc = _dc;
			var g = geometry;
			if (dc==null||g == null || g.IsEmpty)
				return;
			if (trans != null)
			{
				if (!fDeviceCoords)
				{
					g = trans.ToDevice(g);
				}
				if (!g.Intersects(trans.GetViewPolygon()))
				{
					return;
				}
			}

			if (g is IGeometryCollection igc)
			{
				foreach (var g0 in igc.Geometries)
				{
					DrawPrimitive(dc, g0);
				}
			}
			else
			{
				DrawPrimitive(dc, g);
			}
		}

		public override void SetupDC(IDisplay dc, IDisplayTransformation trans)
		{
			base.SetupDC(dc, trans);
			Outline?.SetupDC(dc, trans);
			//ReleaseFillBrush();
			_paint.Color = FillColor?.ToSkColor()??SKColor.Empty;
			//_fillBrush = new SolidBrush((Color)FillColor);
		}
		public new void ResetDC()
		{
			base.ResetDC();
			Outline?.ResetDC();
		}
		#endregion

		private void DrawPrimitive(IDisplay dc, IGeometry g)
		{
			if (g is IPolygon pgn)
			{
				DrawPolygon(dc, pgn);
			}
			else if (g is IMultiPolygon mpgn)
			{
				DrawMultiPolygon(dc, mpgn);
			}
		}
		private void DrawMultiPolygon(IDisplay dc, IMultiPolygon g)
		{
			foreach (var o in g.Geometries)
			{
				if (o is IPolygon pgn) DrawPolygon(dc, pgn);
			}
		}
		private void DrawPolygon(IDisplay dc, IPolygon g)
		{
			path.Reset();
			path.AddGeometry(g);
			if (FillColor != null)
			{
				((SkiaDisplay)dc).GetCanvas().DrawPath(path, _paint);
			}
			Outline?.Draw(g, path, true);
		}
	}
	public class SimpleFillSymbol : SKFillSymbol<SimpleFillSymbol>, IFillSymbol
	{
	}

	public class SimpleMarkerSymbol : MarkerSymbol<SimpleMarkerSymbol>, IMarkerSymbol//, IXmlSerializable
	{
        protected readonly SKPaint _paint = new() { IsAntialias = true };

        public void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
		{
			var g = geometry;
            var dc = _dc;
            if (g == null||dc==null)
			{
				return;
			}
			var trans = _trans;
			if (trans != null)
			{
				if (!fDeviceCoords)
				{
					g = trans.ToDevice(g);
				}
			}
			if (g is IPoint)
			{
				DrawPoint(dc, g.Coordinate);
			}
			else if (g is IMultiPoint mpt)
			{
				foreach (var pt in mpt.Coordinates)
				{
					DrawPoint(dc, pt);
				}
			}
			else
			{
				System.Diagnostics.Debug.Assert(false);
			}
		}
		protected void DrawPoint(IDisplay dc, Coordinate c)
		{
			var size = Size;
			//float strokeThickness = 0;
			//Pen pen = _pen;
			if (_trans != null)
			{
				//pen = new Pen(_pen.Brush, (float)DpiUtil.POINTS2PIXELS2(_pen.Width, _trans.Resolution));
				size = DpiUtil.POINTS2PIXELS2(size, _trans.Resolution);
				//strokeThickness = DpiUtil.POINTS2PIXELS2(Strock.StrokeThickness, _trans.Resolution);
			}

			using var path = new SKPath();
			path.AddMarker(Style, c.X, c.Y, size);

			var canvas = ((SkiaDisplay)dc).GetCanvas();
			if (FillColor is Color fillColor)
			{
				_paint.Style = SKPaintStyle.Fill;
				_paint.Color = fillColor.ToSkColor();
				canvas.DrawPath(path, _paint);
			}
			if (Strock.IsValid)
			{
				//_paint.Style = SKPaintStyle.Stroke;
				Strock.SetupPaint(_paint, _trans);
				//            _paint.Color = ColorUtil.ToSkColor((Color)Strock.StrokeColor!);
				//_paint.StrokeWidth= strokeThickness;
				canvas.DrawPath(path, _paint);
			}

			//using (GraphicsPath path = new GraphicsPath())
			//{
			//	RenderUtil.GenerateMarkerPath(path, Style, c.X, c.Y, size);

			//	graphics.FillPath(_fillBrush, path);
			//	graphics.DrawPath(pen, path);
			//}
		}

		//public override object Clone()
		//{
		//	return CloneUtil.Clone<SimpleMarkerSymbol>(this);
		//}
	}

	public class CharacterMarkerSymbol : CharacterMarkerSymbolBase<CharacterMarkerSymbol>
	{
		private readonly SKPaint _paint = new() { IsAntialias = true };

		public override void SetupDC(IDisplay dc, IDisplayTransformation trans)
		{
			//ReleaseDC();
			base.SetupDC(dc, trans);
			if (Font.IsValid)
			{
				var fontSize = Font.FontSize;
				var dpi = _trans == null ? 0 : _trans.Resolution;
				if (dpi != 0)
				{
					fontSize = DpiUtil.POINTS2PIXELS2(Font.FontSize, dpi);
				}

				_paint.Color = this.Strock.StrokeColor?.ToSkColor() ?? SKColor.Empty;
				_paint.TextSize = (float)fontSize;

				if (SkiaSymbolFactory.dicTypeFace.TryGetValue(Font.FontFamily, out var typeface))
				{
					_paint.Typeface = typeface;
                }
				else
				{
					_paint.Typeface = SKFontManager.Default.MatchFamily(Font.FontFamily);
				}

				//var font = this.Font;



				//_font = new Font(font.FontFamily, (float)fontSize, (FontStyle)font.FontStyle);
			}
		}

		public override void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");


			if (_trans != null)
			{
				geometry = _trans.ToDevice(geometry)!;
			}

			if (geometry is IPoint pt)
			{
				DrawPoint(pt);
			}
			else if (geometry is IMultiPoint mpt)
			{
				foreach (var item in mpt.Geometries)
				{
					if (item is IPoint pt1) DrawPoint(pt1);
				}
			}
		}
		public override IPolygon? QueryBoundary(IDisplay dc, IDisplayTransformation trans, IGeometry Geometry)
		{
			var Text= ((char)CharacterIndex).ToString();

			IPolygon? polygon = null;
			if (Geometry is IPoint pt)
			{
				//var trans = displayTransform;
                bool fSetupDC = dc != _dc;
				if (fSetupDC)
				{
					SetupDC(dc,trans);
				}
				
				//if (trans != null)
				//{
					pt.Project(trans.SpatialReference);
					pt = (IPoint)trans.ToDevice(pt)!;
				//}

				var origin = AdjustTextOrigin(new PointF((float)pt.X, (float)pt.Y));
                //var rc = GDIPlusUtil.QueryTextBound(Text, _font, _format, origin);
				var rc=_paint.QueryTextBound(Text,origin);

                var size = new SKRect();
				_paint.MeasureText(Text, ref size);

				var p = origin;
				var env = new Envelope( rc.Left, rc.Right, rc.Top, rc.Bottom);
																							  //var env = new Envelope(p.X, p.X-size.Width, p.Y, p.Y + size.Height);// rc.Left, rc.Right, rc.Top, rc.Bottom);
				//Console.Write(env.Centre.X + "," + env.Centre.Y);
                polygon = GeometryUtil.MakePolygon(env);
				polygon = (IPolygon)trans.ToMap(polygon)!;
				polygon.SetSpatialReference(Geometry.GetSpatialReference());

				if (fSetupDC)
				{
					ResetDC();
				}
			}
			return polygon;
		}

		private void DrawPoint(IPoint pt)//,  RenderMetadata meta, RenderOptions options)
		{
            string s = ((char)CharacterIndex).ToString();
            var origin = new PointF((float)pt.X, (float)pt.Y);
            AdjustTextOrigin(origin);

            var sz = new SKRect();
            _paint.MeasureText(s, ref sz);

            ((SkiaDisplay)_dc)!.GetCanvas().DrawText(s, (float)(origin.X-sz.Width*0.5),(float)(origin.Y+sz.Height*0.5), _paint);
			/*
        Graphics graphics = _dc;
        var aagf = graphics.TextRenderingHint;
        var sm = graphics.SmoothingMode;
        var pom = graphics.PixelOffsetMode;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        try
        {
            var font = _font;
            {
                var pt = mpt;// TransformUtil.ToScreen(meta, mpt);

                string s = ((char)CharacterIndex).ToString();

                var origin = new PointF((float)pt.X,(float) pt.Y);
                AdjustTextOrigin(origin);

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddString(s, font.FontFamily, 0, font.Size, origin, _format);

                    if (GlowEffect != null && GlowEffect.Enable && GlowEffect.GlowEffectColor != null)
                    {
                        //var mx = graphics.Transform.Clone();
                        //graphics.Transform.Scale(1.2f, 1.2f);

                        //using (var brush = new SolidBrush((Color)GlowEffect.GlowEffectColor))
                        //{
                        //	graphics.FillPath(brush, path);
                        using (Pen p = new Pen((Color)GlowEffect.GlowEffectColor, (float)GlowEffect.GlowEffectSize))
                            {
                                graphics.DrawPath(p, path);
                            }
                        //}

                        //graphics.Transform = mx;
                    }

                    graphics.FillPath(base._fillBrush, path);
                }
            }
        }
        finally
        {
            graphics.TextRenderingHint = aagf;
            graphics.SmoothingMode = sm;
            graphics.PixelOffsetMode = pom;
        }
        */
        }
	}

	/// <summary>
	/// 线填充符号
	/// </summary>
	public class LineFillSymbol : FillSymbol<LineFillSymbol>, ILineFillSymbol
	{
		public LineFillSymbol() : base(SkiaSymbolFactory.Instance) { }
        #region ILineFillSymbol
        public double Offset { get; set; }
		public double Separation { get; set; }
		public double Angle { get; set; }
		public ILineSymbol? LineSymbol { get; set; }
		#endregion

		#region ISymbol
		public void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
		{
			//base.Draw(geometry, extension, fDeviceCoords);
			throw new NotImplementedException();
		}
		#endregion
	}


	public sealed class MultiLayerFillSymbol : MultiLayerFillSymbolBase<MultiLayerFillSymbol>
	{
		public MultiLayerFillSymbol() : base(SkiaSymbolFactory.Instance) { }
	}

	public sealed class MultiLayerMarkerSymbol : MultiLayerMarkerSymbolBase<MultiLayerMarkerSymbol>, IMarkerSymbol
	{
		public MultiLayerMarkerSymbol() : base(SkiaSymbolFactory.Instance) { }
	}

	public sealed class MultiLayerLineSymbol : MultiLayerLineSymbolBase<MultiLayerLineSymbol>, ILineSymbol
	{
		public MultiLayerLineSymbol() : base(SkiaSymbolFactory.Instance) { }
    }


	/// <summary>
	/// 制图线符号
	/// </summary>
	public class CartographicLineSymbol : CartographicLineSymbolBase<CartographicLineSymbol>, ICartographicLineSymbol//, IXmlSerializable
	{
        private readonly SKPaint _paint = new() { IsAntialias = true,Style=SKPaintStyle.Stroke };
		private readonly SKPath path = new();
        #region ISymbol
        public new void SetupDC(IDisplay dc, IDisplayTransformation trans)
        {
            base.SetupDC(dc, trans);

			this.SetupPaint(_paint,_trans);
        }
        public override void Draw(IGeometry geometry, object? extension = null, bool fDeviceCoords = false)
		{
			//暂时采用SimpleLineSymbol方式实现
			if (_dc is SkiaDisplay dc)
			{
				var canvas = dc.GetCanvas();
				if (extension is SKPath path1)
				{
					canvas.DrawPath(path1, _paint);
					return;
				}
				var g = geometry;
				if (g == null || g.IsEmpty)
					return;

				if (!fDeviceCoords)
				{
					g = _trans.ToDevice(g);
				}
				if (!g!.Intersects(_trans.GetViewPolygon()))
				{
					return;
				}
				path.Reset();
				path.AddGeometry(g);
                canvas.DrawPath(path, _paint);
			}
        }

		#endregion

		//public override void Render(Spatial.Geometry geometry, System.Drawing.Graphics graphics, Spatial.SpatialReference mapSpatialReference, RenderMetadata meta, RenderOptions options)
		//{
		//    var pen = CreatePen();
		//    if (pen != null)
		//    {
		//        graphics.TranslateTransform(0, (float)Offset);
		//        RenderUtil.RenderLine(geometry, graphics, mapSpatialReference, meta, pen);
		//        pen.Dispose();
		//        graphics.TranslateTransform(0, (float)-Offset);
		//    }
		//    //var dashPattern = Template.CreateDashPattern();
		//    //if (dashPattern != null)
		//    //{
		//    //    using (var pen = new System.Drawing.Pen(ColorUtil.ToDrawingColor(_lineSymbol.StrokeColor),(float)_lineSymbol.StrokeThickness))
		//    //    {
		//    //        pen.DashPattern = dashPattern;
		//    //        pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
		//    //        if (Offset != 0)
		//    //        {
		//    //            pen.TranslateTransform(0, (float)Offset);
		//    //        }
		//    //        RenderUtil.RenderLine(geometry, graphics, mapSpatialReference, meta, pen);
		//    //    }
		//    //}
		//    else
		//    {
		//        _lineSymbol.Render(geometry, graphics, mapSpatialReference, meta, options);
		//    }
		//}
		//public override void Render(Spatial.Geometry[] geometries, System.Drawing.Graphics graphics, Spatial.SpatialReference mapSpatialReference, RenderMetadata meta, RenderOptions options)
		//{
		//    var pen = CreatePen();
		//    if (pen != null)
		//    {
		//        RenderUtil.RenderLines(geometries, graphics, mapSpatialReference, meta, pen);
		//        pen.Dispose();
		//    }
		//    //var dashPattern = Template.CreateDashPattern();
		//    //if (dashPattern != null)
		//    //{
		//    //    using (var pen = new System.Drawing.Pen(ColorUtil.ToDrawingColor(_lineSymbol.StrokeColor), (float)_lineSymbol.StrokeThickness))
		//    //    {
		//    //        pen.DashPattern = dashPattern;
		//    //        pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
		//    //        RenderUtil.RenderLines(geometries, graphics, mapSpatialReference, meta, pen);
		//    //    }
		//    //}
		//    else
		//    {
		//        _lineSymbol.Render(geometries, graphics, mapSpatialReference, meta, options);
		//    }
		//}

		//private Pen CreateDashPen()
		//{
		//	var dashPattern = Template.CreateDashPattern();
		//	if (dashPattern != null && StrokeColor != null)
		//	{
		//		var pen = new Pen((Color)StrokeColor, (float)StrokeThickness);
		//		pen.DashPattern = dashPattern;
		//		pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
		//		if (Offset != 0)
		//		{
		//			//pen.Alignment
		//			//pen.TranslateTransform(0, (float)Offset);
		//		}
		//		return pen;
		//	}
		//	return null;
		//}
		//private void releaseDC()
		//{
		//	//if (_dashPen != null)
		//	//{
		//	//	_dashPen.Dispose();
		//	//	_dashPen = null;
		//	//}
		//}

	}

}
