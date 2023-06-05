using Agro.LibCore.AST;
using Agro.LibCore;
using GeoAPI.Geometries;
using System.Xml;
using System.Drawing;

namespace Agro.GIS
{
    public class FormulaSymbol : Symbol<FormulaSymbol>, IFormulaSymbol
    {
        class LabelDrawingProperty// : IDisposable
        {
            //      internal GraphicsPath _path;
            //      internal readonly StringFormat _format = new StringFormat();
            //      //public Bitmap _bitmap = null;//=new Bitmap()
            //      public Font _font;
            //      //private SolidBrush _GlowEffectSolidBrush;//((Color)ge.GlowEffect;
            //      public Pen _GlowEffectPen;
            //      public Pen _pen;
            //      public SolidBrush _brush;
            //private Pen _lineShadowPen;
            private ILineSymbol lineSymbol = GisGlobal.SymbolFactory.CreateSimpleLineSymbol();
            internal ITextSymbol textSymbol= GisGlobal.SymbolFactory.CreateTextSymbol();
            //public LabelDrawingProperty()
            //{
            //    //_format.Alignment = StringAlignment.Center;
            //    //_format.LineAlignment = StringAlignment.Center;
            //}
            public void SetupDC(IDisplay dc, IDisplayTransformation trans, double penSize = 1)//, IDisplayTransformation trans = null)
            {
                lineSymbol.StrokeThickness = penSize;
                lineSymbol.StrokeColor = textSymbol.Color;

                textSymbol.SetupDC(dc, trans);
                lineSymbol.SetupDC(dc, trans);
            }
            public void ResetDC()
            {
                textSymbol.ResetDC();
                lineSymbol.ResetDC();
            }


            public void DrawText(string txt, MyRect rc, double dpi = 0)
            {
                textSymbol.Text = txt;
                var pt = GeometryUtil.MakePoint((rc.left + rc.right) * 0.5, (rc.top + rc.bottom) * 0.5);
                textSymbol.Draw(pt, null, true);
                /*
                //if (path == null)
                //{
                //    path = _path;
                //    //path.Reset();
                //}
                //int xExpand = 5;//OMG:绘制框应比字符串实际宽度大一点，否则最后一个字有可能画不出来；

                #region yxm 2018-6-22
                float fontSize = _font.Size;
                if (dpi != 0)
                {
                    fontSize = (float)DpiUtil.POINTS2PIXELS2(_font.Size, (double)dpi);
                    //xExpand = (int)DpiUtil.POINTS2PIXELS2(xExpand, (double)dpi);// * dpi / 96+0.5);//
                }
                #endregion

                var origin = new PointF((float)(rc.left + rc.right) / 2, (float)(rc.top + rc.bottom) / 2);
                //var origin = new PointF((float)(rc.left + rc.right)/2, (float)rc.bottom);

                #region yxm 2019-1-15 
                var rc1 = GDIPlusUtil.QueryTextBound(txt, _font, _format, origin);
                var dy = (rc1.Top + rc1.Bottom) / 2 - origin.Y;
                var dx = (rc1.Left + rc1.Right) / 2 - origin.X;
                origin.Y = origin.Y - dy;
                origin.X = origin.X - dx;
                #endregion

                //var r = new Rectangle((int)rc.left - xExpand, (int)rc.top, (int)rc.Width() + 2 * xExpand, (int)rc.Height());
                path.AddString(txt, _font.FontFamily, (int)_font.Style, fontSize, origin, _format);

                */
            }
            public void DrawLine(PointF from, PointF to)
            {
                var g = GeometryUtil.MakeLineString(from.X, from.Y, to.X, to.Y);
                lineSymbol.Draw(g, null, true);
                /*
                if (_lineShadowPen != null)
                {
                    var dx = (_lineShadowPen.Width-_pen.Width)/2;
                    g.DrawLine(_lineShadowPen, from.X - dx, from.Y, to.X + dx, to.Y);
                }
                g.DrawLine(_pen, from, to);
                */
            }
        }
        private ASTNode? _originAst = null;
        private readonly DrawNodeHelper _helper = new();
        private readonly LabelDrawingProperty _draw = new();//
        private string _expr = "";
        public FormulaSymbol()
        {
            //_drawProperty._format.Alignment = StringAlignment.Center;
            //_drawProperty._format.LineAlignment = StringAlignment.Center;
        }

        public Func<string, string> OnCallFieldValue
        {
            get { return _helper.CallFieldValue; }
            set
            {
                _helper.CallFieldValue = value;
            }
        }
        public Func<MyRect, bool>? ShouldDraw;

        #region IFormulaElement
        public string Expr
        {
            get
            {
                return _expr;
            }
            set
            {
                _expr = value;
                _originAst = new SimpleAstLabelExpress().BuildAST(value);
                if (_originAst != null)
                {
                    _originAst = SymplifyNodeHelper.Simplify(_originAst, null);
                }
            }
        }
        public ASTNode? ASTNode { get { return _originAst; } set { _originAst = value; } }
        public ITextSymbol TextSymbol
        {
            get { return _draw.textSymbol; }
            set
            {
                _draw.textSymbol = value;
            }
        }
        public double PenSize { get; set; } = 1.0;
        #endregion

        #region ISymbol
        public override void SetupDC(IDisplay dc, IDisplayTransformation trans)
        {
            base.SetupDC(dc, trans);
            //lineSymbol.SetupDC(dc, trans);
            _draw.SetupDC(dc, trans, PenSize);
            //lineSymbol.StrokeColor = TextSymbol.Color;
        }
        public new void ResetDC()
        {
            _draw.ResetDC();
            //base.ResetDC();
        }

        public new IPolygon QueryBoundary(IDisplay dc, IDisplayTransformation trans, IGeometry Geometry)
        {
            //var trans = _trans;// display.DisplayTransformation;

            bool fSetupDC = dc != _dc;
            if (fSetupDC)
            {
                SetupDC(dc, trans);
            }
            //_drawProperty.FromTextSymbol(TextSymbol);
            //var dc = _dc;// display.Graphics;
            //_drawProperty._path.Reset();
            _helper.VMargin = 4;
            _helper.HMargin = 3;
            _helper._penWidth = PenSize;
            var rc = _helper.CalcRect(_originAst, (s) =>
            {
                var sz = TextSymbol.MeasureText(s);// TextUtil.MeasureString(s, _drawProperty._font, _drawProperty._format);//, trans.Resolution);//, StringFormat.GenericTypographic);
                return new MyRect(0, 0, sz.Width, sz.Height);
            }, trans.Resolution);

            //var pt =Geometry.Centroid;
            var ptScreen = trans.ToDevice(Geometry.Centroid) as IPoint;

            rc.CenterAt(ptScreen.X, ptScreen.Y);
            var pgn = GeometryUtil.MakePolygon(rc.left, rc.top, rc.right, rc.bottom);
            pgn = trans.ToMap(pgn) as IPolygon;


            if (fSetupDC)
            {
                ResetDC();
            }

            return pgn;
        }
        public void Draw(IGeometry geo, object? extension = null, bool fDeviceCoords = false)
        {
            if (geo == null || _originAst == null) return;
            var trans = _trans;
            geo.Project(trans.SpatialReference);
            geo = trans.ToDevice(geo)!;
            var pt = geo.Centroid;
            LabelPoint(pt, _originAst);
        }
        #endregion

        public override void WriteXml(XmlWriter writer)
        {
            SerializeUtil.WriteStringElement(writer, "Expr", Expr);
            SerializeUtil.WriteDoubleElement(writer, "PenSize", PenSize);
            SerializeUtil.WriteSymbol(writer, "Symbol", TextSymbol);
        }
        public override void ReadXml(XmlElement reader)
        {
            //SerializeUtil.ReadNodeCallback(reader, "Symbol", name =>
            foreach (var o in reader.ChildNodes)
            {
                if (o is XmlElement e)
                {
                    switch (e.Name)
                    {
                        case "Expr":
                            Expr = e.InnerText;
                            break;
                        case "PenSize":
                            PenSize = SafeConvertAux.ToDouble(e.InnerText);
                            break;
                        case "Symbol":
                            TextSymbol = SerializeUtil.ReadSymbol(e) as ITextSymbol;
                            break;
                    }
                }
            }
        }

        private void LabelPoint(IPoint ptScreen, ASTNode labelAst)
        {
            _helper.VMargin = 4;
            _helper.HMargin = 3;
            _helper._penWidth = PenSize;
            var rc = _helper.CalcRect(labelAst, (s) =>
            {
                var sz = TextSymbol.MeasureText(s);
                return new MyRect(0, 0, sz.Width, sz.Height);
            }, _trans.Resolution);
            rc.CenterAt(ptScreen.X, ptScreen.Y);

            if (ShouldDraw?.Invoke(rc) != false)
            {
                _helper.Draw(labelAst, ptScreen.X, ptScreen.Y, true
                    , (s, rc1) => _draw.DrawText(s, rc1), (p0, p1) => _draw.DrawLine(p0, p1));
            }
        }

    }
}
