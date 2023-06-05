//using SkiaSharp;
using System.Drawing;

namespace Agro.GIS
{
    public interface IDisplay
    {
        //SKCanvas GetCanvas();
        int Width { get; }
        int Height { get; }
        void Resize(int width, int height);
        void SetClipRect(RectangleF rc);
        void SaveDC();
        void RestorDC();
        //IDisplayTransformation DisplayTransformation { get; set; }
        //void SetSymbol(ISymbol symbol);
        //void DrawRectangle(Envelope env);
        //void FillRectangle(Color color, RectangleF rect);
        //void SetDeviceFrame(Rectangle deviceFrame, bool fUpdateBackImage);
        void SaveToFile(string file);

        //ISymbolFactory SymbolFactory { get; }

    }
}
