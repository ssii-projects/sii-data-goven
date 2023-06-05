using Agro.GIS;
using GeoAPI.Geometries;
using SkiaSharp;

namespace SkiaMap
{
    public static class SKPathExtension
    {
        public static void AddMarker(this SKPath path, eSimpleMarkerStyle Style, double x, double y, double markerSize)
        {
            var Size = markerSize;
            var halfSize = markerSize * 0.5;
            switch (Style)
            {
                case eSimpleMarkerStyle.Circle:
                    path.AddCircle(
                        (float)x,
                        (float)y,
                        (float)halfSize);
                    break;
                case eSimpleMarkerStyle.Square:
                    path.AddRect(new SKRect((float)(x - halfSize),
                        (float)(y - halfSize),
                        (float)(x + halfSize), (float)(y + halfSize)));
                    break;
                case eSimpleMarkerStyle.Cross:
                    System.Diagnostics.Debug.Assert(false);
                    //path.StartFigure();
                    //path.AddLine(
                    //    (float)(x - Size / 2),
                    //    (float)(y - Size / 2),
                    //    (float)(x + Size / 2),
                    //    (float)(y + Size / 2));
                    //path.StartFigure();
                    //path.AddLine(
                    //    (float)(x - Size / 2),
                    //    (float)(y + Size / 2),
                    //    (float)(x + Size / 2),
                    //    (float)(y - Size / 2));
                    break;
                case eSimpleMarkerStyle.Diamond:
                    System.Diagnostics.Debug.Assert(false);
                    //path.StartFigure();
                    //path.AddLine(
                    //    (float)(x - Size / 4),
                    //    (float)(y - Size / 2),
                    //    (float)(x + Size / 4),
                    //    (float)(y - Size / 2));
                    //path.AddLine(
                    //    (float)(x + Size / 4),
                    //    (float)(y - Size / 2),
                    //    (float)(x + Size / 2),
                    //    (float)(y - Size / 4));
                    //path.AddLine(
                    //    (float)(x + Size / 2),
                    //    (float)(y - Size / 4),
                    //    (float)(x + Size / 2),
                    //    (float)(y + Size / 4));
                    //path.AddLine(
                    //    (float)(x + Size / 2),
                    //    (float)(y + Size / 4),
                    //    (float)(x + Size / 4),
                    //    (float)(y + Size / 2));
                    //path.AddLine(
                    //    (float)(x + Size / 4),
                    //    (float)(y + Size / 2),
                    //    (float)(x - Size / 4),
                    //    (float)(y + Size / 2));
                    //path.AddLine(
                    //    (float)(x - Size / 4),
                    //    (float)(y + Size / 2),
                    //    (float)(x - Size / 2),
                    //    (float)(y + Size / 4));
                    //path.AddLine(
                    //    (float)(x - Size / 2),
                    //    (float)(y + Size / 4),
                    //    (float)(x - Size / 2),
                    //    (float)(y - Size / 4));
                    //path.AddLine(
                    //    (float)(x - Size / 2),
                    //    (float)(y - Size / 4),
                    //    (float)(x - Size / 4),
                    //    (float)(y - Size / 2));
                    break;
                case eSimpleMarkerStyle.Triangle:
                    System.Diagnostics.Debug.Assert(false, "todo...");
                    //path.StartFigure();
                    //path.AddLine(
                    //    (float)(x),
                    //    (float)(y - Size / 2),
                    //    (float)(x + Size / 2),
                    //    (float)(y + Size / 2));
                    //path.AddLine(
                    //    (float)(x + Size / 2),
                    //    (float)(y + Size / 2),
                    //    (float)(x - Size / 2),
                    //    (float)(y + Size / 2));
                    //path.AddLine(
                    //    (float)(x - Size / 2),
                    //    (float)(y + Size / 2),
                    //    (float)(x),
                    //    (float)(y - Size / 2));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public static void AddGeometry(this SKPath path, IGeometry geo)
        {
            if (geo is IPolygon pgn)
            {
                addPoints(path, pgn.Shell.Coordinates);
                foreach (var h in pgn.Holes)
                {
                    addPoints(path, h.Coordinates);
                }
            }
            else if (geo is IMultiPolygon mpgn)
            {
                foreach (var o in mpgn.Geometries.Cast<IPolygon>())
                {
                    path.AddGeometry(o);
                }
            }
            else if (geo is ILineString)
            {
                addPoints(path, geo.Coordinates, false);
            }
            else if (geo is IMultiLineString mls)
            {
                foreach (var o in mls.Geometries.Cast<ILineString>())
                {
                    path.AddGeometry(o);
                }
            }
        }

        private static void addPoints(SKPath path, Coordinate[] coords, bool fClose = true)
        {
            var c = coords[0];
            path.MoveTo((float)c.X, (float)c.Y);
            for (var i = 1; i < coords.Length; i++)
            {
                path.LineTo((float)coords[i].X, (float)coords[i].Y);
            }
            if (fClose)
            {
                path.Close();
            }
        }
    }
}
