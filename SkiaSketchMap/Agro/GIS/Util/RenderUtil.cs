namespace Agro.GIS
{
    public class RenderUtil
    {
        //public static SKPath GeneratePolygonPath(IPolygon g)
        //{
        //    SKPath path = new();
        //    addPoints(path, g.Shell.Coordinates);
        //    foreach (var h in g.Holes)
        //    {
        //        addPoints(path, h.Coordinates);
        //    }
        //    return path;
        //}
        //public static SKPath GenerateMultiPolygonPath(IMultiPolygon mp)
        //{
        //    SKPath path = new()
        //    {
        //        FillType = SKPathFillType.EvenOdd
        //    };
        //    foreach (var o in mp.Geometries)
        //    {
        //        var g = o as IPolygon;
        //        addPoints(path, g.Shell.Coordinates);
        //        foreach (var h in g.Holes)
        //        {
        //            addPoints(path, h.Coordinates);
        //        }
        //    }
        //    return path;
        //}
        //private static void addPoints(SKPath path, Coordinate[] coords,bool fClose=true)
        //{
        //    var c = coords[0];
        //    path.MoveTo((float)c.X, (float)c.Y);
        //    for(var i = 1; i < coords.Length; i++)
        //    {
        //        path.LineTo((float)coords[i].X, (float)coords[i].Y);
        //    }
        //    if (fClose)
        //    {
        //        path.Close();
        //    }
        //    //var pts = RenderUtil.ToDrawingPoints(coords);//,coords.Length-1);// new System.Drawing.Point[coords.Length];
        //    //try
        //    //{
        //    //    //for (int i = 0; i < pts.Length; ++i)
        //    //    //{
        //    //    //    pts[i] = new System.Drawing.Point((int)(coords[i].X), (int)(coords[i].Y));
        //    //    //}

        //    //    path.AddPolygon(pts);
        //    //    //path.CloseFigure();
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    //Log.WriteLine(ex.Message);
        //    //}
        //}

        //public static void GenerateMarkerPath(SKPath path, eSimpleMarkerStyle Style, double x, double y, double markerSize)
        //{
        //    var Size = markerSize;
        //    var halfSize = markerSize * 0.5;
        //    switch (Style)
        //    {
        //        case eSimpleMarkerStyle.Circle:
        //            path.AddCircle(
        //                (float)(x - Size / 2),
        //                (float)(y - Size / 2),
        //                (float)Size);
        //            break;
        //        case eSimpleMarkerStyle.Square:
        //            path.AddRect(new SKRect((float)(x - halfSize),
        //                (float)(y - halfSize),
        //                (float)(x+ halfSize), (float)(y+ halfSize)));
        //            break;
        //        case eSimpleMarkerStyle.Cross:
        //            System.Diagnostics.Debug.Assert(false);
        //            //path.StartFigure();
        //            //path.AddLine(
        //            //    (float)(x - Size / 2),
        //            //    (float)(y - Size / 2),
        //            //    (float)(x + Size / 2),
        //            //    (float)(y + Size / 2));
        //            //path.StartFigure();
        //            //path.AddLine(
        //            //    (float)(x - Size / 2),
        //            //    (float)(y + Size / 2),
        //            //    (float)(x + Size / 2),
        //            //    (float)(y - Size / 2));
        //            break;
        //        case eSimpleMarkerStyle.Diamond:
        //            System.Diagnostics.Debug.Assert(false);
        //            //path.StartFigure();
        //            //path.AddLine(
        //            //    (float)(x - Size / 4),
        //            //    (float)(y - Size / 2),
        //            //    (float)(x + Size / 4),
        //            //    (float)(y - Size / 2));
        //            //path.AddLine(
        //            //    (float)(x + Size / 4),
        //            //    (float)(y - Size / 2),
        //            //    (float)(x + Size / 2),
        //            //    (float)(y - Size / 4));
        //            //path.AddLine(
        //            //    (float)(x + Size / 2),
        //            //    (float)(y - Size / 4),
        //            //    (float)(x + Size / 2),
        //            //    (float)(y + Size / 4));
        //            //path.AddLine(
        //            //    (float)(x + Size / 2),
        //            //    (float)(y + Size / 4),
        //            //    (float)(x + Size / 4),
        //            //    (float)(y + Size / 2));
        //            //path.AddLine(
        //            //    (float)(x + Size / 4),
        //            //    (float)(y + Size / 2),
        //            //    (float)(x - Size / 4),
        //            //    (float)(y + Size / 2));
        //            //path.AddLine(
        //            //    (float)(x - Size / 4),
        //            //    (float)(y + Size / 2),
        //            //    (float)(x - Size / 2),
        //            //    (float)(y + Size / 4));
        //            //path.AddLine(
        //            //    (float)(x - Size / 2),
        //            //    (float)(y + Size / 4),
        //            //    (float)(x - Size / 2),
        //            //    (float)(y - Size / 4));
        //            //path.AddLine(
        //            //    (float)(x - Size / 2),
        //            //    (float)(y - Size / 4),
        //            //    (float)(x - Size / 4),
        //            //    (float)(y - Size / 2));
        //            break;
        //        case eSimpleMarkerStyle.Triangle:
        //            System.Diagnostics.Debug.Assert(false,"todo...");
        //            //path.StartFigure();
        //            //path.AddLine(
        //            //    (float)(x),
        //            //    (float)(y - Size / 2),
        //            //    (float)(x + Size / 2),
        //            //    (float)(y + Size / 2));
        //            //path.AddLine(
        //            //    (float)(x + Size / 2),
        //            //    (float)(y + Size / 2),
        //            //    (float)(x - Size / 2),
        //            //    (float)(y + Size / 2));
        //            //path.AddLine(
        //            //    (float)(x - Size / 2),
        //            //    (float)(y + Size / 2),
        //            //    (float)(x),
        //            //    (float)(y - Size / 2));
        //            break;
        //        default:
        //            throw new NotSupportedException();
        //    }
        //}



        //public static void DrawLine(SKCanvas dc, IGeometry g, SKPaint pen)
        //{
        //    if (g is ILineString)
        //    {
        //        drawLineString(dc, g as ILineString, pen);
        //    }
        //    else if (g is IMultiLineString)
        //    {
        //        var mls = g as IMultiLineString;
        //        foreach (var l in mls.Geometries)
        //        {
        //            drawLineString(dc, l as ILineString, pen);
        //        }
        //    }
        //    else if (g is IPolygon pgn)
        //    {
        //        drawLineString(dc, pgn.ExteriorRing, pen);
        //        foreach (var h in pgn.Holes)
        //        {
        //            drawLineString(dc, h, pen);
        //        }
        //    }
        //    else if (g is IMultiPolygon mpgn)
        //    {
        //        foreach (var g1 in mpgn.Geometries)
        //        {
        //            DrawLine(dc, g1, pen);
        //        }
        //    }
        //}

        //public static void drawLineString(SKCanvas dc, ILineString ls, SKPaint pen)
        //{
        //    var pts = RenderUtil.ToDrawingPoints(ls.Coordinates);
        //    dc.DrawLines(pen, pts);
        //}
    }
}
