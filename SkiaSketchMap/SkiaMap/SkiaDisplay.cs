using Agro.GIS;
using SkiaSharp;
using System.Drawing;

namespace SkiaMap
{
    public class SkiaDisplay : IDisplay, IDisposable
    {
        internal Action? OnDeviceFrameChanged;
        //private readonly object _host;
        private bool _fDirty = false;

        private SKBitmap? bmp;// = new(100, 100);
        private SKCanvas? canvas;

        public SkiaDisplay()
        {
            //canvas = new SKCanvas(bmp);
        }
        public SKCanvas GetCanvas() { return canvas!; }
        //public SKBitmap GetBitmap() { return bmp!; }
        #region IDisplay

        #region yxm 2018-9-30
        //protected RectangleF _clipRect;

        public void SaveDC()
        {
            canvas?.Save();
        }
        public void RestorDC()
        {
            canvas?.Restore();
        }
        /// <summary>
        /// 设置剪裁区（触发更新BackImage）；
        /// </summary>
        /// <param name="rc"></param>
        public void SetClipRect(RectangleF rc)
        {
            canvas?.ClipRect(new SKRect(rc.X, rc.Y, rc.Right, rc.Bottom));
            //_clipRect = rc;
            //if (BackImage != null)
            //{
            //    BackImage.Dispose();
            //    BackImage = null;
            //    Graphics.Dispose();
            //    Graphics = null;
            //}
            //int wi = (int)rc.Width;
            //int hi = (int)rc.Height;
            //if (wi > 0 && hi > 0)
            //{
            //    BackImage = new Bitmap(wi, hi);
            //    Graphics = ImageUtil.CreateGraphics(BackImage);
            //    if (rc != RectangleF.Empty)
            //    {
            //        Graphics.TranslateTransform(-rc.X, -rc.Y);
            //    }
            //}
        }
        public void Resize(int width, int height)
        {
            //if (BackImage != null)
            //{
            //    BackImage.Dispose();
            //    BackImage = null;
            //    Graphics.Dispose();
            //    Graphics = null;
            //}

            //int wi = DisplayTransformation.DeviceFrame.Width;
            //int hi = DisplayTransformation.DeviceFrame.Height;

            if (width > 0 && height > 0)
            {
                //BackImage = new Bitmap(wi, hi);
                //Graphics = ImageUtil.CreateGraphics(BackImage);
                //bmp.Resize(new SKSizeI(wi, hi), SKFilterQuality.None);
                bmp?.Dispose();
                bmp = new(width, height);
                canvas?.Dispose();
                canvas = new SKCanvas(bmp);
            }
        }
        //public RectangleF ClipRect
        //{
        //    get
        //    {
        //        return _clipRect;
        //    }
        //}

        /// <summary>
        /// BackImage.Width
        /// </summary>
        public int Width
        {
            get
            {
                return bmp?.Width??0;// DisplayTransformation.DeviceFrame.Width;
            }
            //get;set;
            //get
            //{
            //    return BackImage == null ? 0 : BackImage.Width;
            //}
        }

        /// <summary>
        /// the height of BackImage
        /// </summary>
        public int Height
        {
            get
            {
                return bmp?.Height??0;// DisplayTransformation.DeviceFrame.Height;
            }
            //get;set;
            //get
            //{
            //    return BackImage == null ? 0 : BackImage.Height;
            //}
        }

        //public ISymbolFactory SymbolFactory => SkiaSymbolFactory.Instance;

        #endregion

        //public IDisplayTransformation DisplayTransformation { get; set; } = new DisplayTransformation();

        //public void SetSymbol(ISymbol symbol)
        //{

        //}
        //public void DrawRectangle(Envelope env)
        //{

        //}
        //public void DrawBitmap(Bitmap bmp)
        //{

        //}
        //public void Progress(int VertexCount)
        //{

        //}
        #endregion
        public void SetCacheDirty(bool fDirty = true)
        {
            _fDirty = fDirty;
        }
        public bool IsCacheDirty()//OkScreenCache cacheIndex)
        {
            return _fDirty;
        }
        //public void StartRecording() { }
        //public void StopRecording() { }
        //public void DrawCache(Graphics g, OkScreenCache Index, OkEnvelope deviceRect = null, OkEnvelope cacheRect = null)
        //{
        //}
        //public void Invalidate(OkEnvelope rect, bool erase, OkScreenCache cacheIndex)
        //{
        //    SetCacheDirty(true);
        //}
        //public void SetDeviceFrame(Rectangle deviceFrame, bool fUpdateBackImage)
        //public void Resize(int width,int height)
        //{
        //    this.DisplayTransformation.SetDeviceFrame(deviceFrame);
        //    OnDeviceFrameChanged?.Invoke();
        //    if (fUpdateBackImage)
        //    {
        //        UpdateBackImage();
        //    }
        //}

        /// <summary>
        /// System.Drawing.Color.Transparent);
        /// </summary>
        /// <param name="clr"></param>
        //public void ClearBackground(Color clr)
        //{
        //    //Graphics?.Clear(clr);
        //}
        //private void Clear()
        //{
        //}
        public void Dispose()
        {
            if (canvas != null)
            {
                bmp?.Dispose();
                canvas.Dispose();
                bmp=null;
                canvas = null;
            }
            //if (BackImage != null)
            //{
            //    BackImage.Dispose();
            //    Graphics.Dispose();
            //}
            //Clear();
        }

        //public void FillRectangle(Color color, RectangleF rect)
        //{
        //    var fillPaint = new SKPaint
        //    {
        //        Style = SKPaintStyle.Fill,
        //        Color = new SKColor((uint)color.ToArgb())
        //    };
        //    canvas.DrawRect(new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom), fillPaint);
        //}

        public void SaveToFile(string file)
        {
            //保存成图片文件
            using SKImage img = SKImage.FromBitmap(bmp);
            using SKData p = img.Encode(SKEncodedImageFormat.Jpeg, 100);
            //return p.ToArray();
            //var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test1.jpg");//, name);
            //Console.WriteLine(path);
            using var stream = File.Create(file);
            stream.Write(p.ToArray(), 0, p.ToArray().Length);
        }
    }
}
