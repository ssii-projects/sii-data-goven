using Agro.LibCore;
using System.Drawing;

namespace Agro.GIS
{
    public enum OutputMode
    {
        ToScreen,
        ToPicture,
    }
    public interface IActiveView
    {
    }

    public abstract class ActiveView : IActiveView, IDisposable
    {
        //protected readonly MyTask _currentTask = new MyTask();
        //protected IDisplay? m_pIScreenDisplay;
        protected readonly DisplayTransformation _trans = new();
        protected readonly GraphicsContainer _graphicsContainer = new();


        //#region events
        protected readonly TransformChangeEventArgs _transformChangedEventArgs = new TransformChangeEventArgs();

        //public Action<OkEnvelope> OnFullExtentChanged;
        public Action<TransformChangeEventArgs>? OnTransformChanged;
        //public Action<object, Bitmap, RectangleF, ContentChangeType> OnContentChanged;
        //public Action<eDocumentEventType, object> OnDocumentEvents;
        //public Action OnDispose;
        //#endregion

        public bool SuprresEvents { get; set; } = false;
        public bool IsDocumentDirty
        {
            get;
            set;
        }

        protected ActiveView(IDisplay? dc=null)
        {
            Display = dc;
            //m_pIScreenDisplay = dc;
            //Host = p;
            //var t = this;// as T;
            //m_pIScreenDisplay = new ScreenDisplay(t);
            //ExtentStack = new ExtentStack(t);
            _graphicsContainer.OnElementAdded += ei => ei.Element.Activate(Display, Transformation);
        }

        #region IActiveView
        //public IMapControl Host
        //{
        //    get;
        //    private set;
        //}

        public abstract Map? FocusMap { get; }
        public string? DocumentFileName { get; protected set; }

        //public Bitmap BackImage { get { return m_pIScreenDisplay.BackImage; } }
        protected RectangleF? _clipRect;
        public void SetClipRect(RectangleF? rc)
        {
            _clipRect = rc;
            //m_pIScreenDisplay.SetClipRect(rc);
        }
        public RectangleF? GetClipRect()
        {
            return _clipRect;
        }

        public IDisplay? Display { get; set; }// { get { return m_pIScreenDisplay; } }
        public IDisplayTransformation Transformation
        {
            get { return _trans; }// ScreenDisplay.DisplayTransformation; }
        }

        public OkEnvelope FullExtent
        {
            get
            {
                return Transformation.Bounds;
            }
            set
            {
                var oldVal = FullExtent;
                var pIDisplayTransformation = Transformation;
                if (pIDisplayTransformation != null&& value!=null)
                {
                    pIDisplayTransformation.Bounds = value;
                }

                if (FocusMap != null && FocusMap.SpatialReference == null)
                {
                    if (value?.SpatialReference != null)
                    {
                        FocusMap.SetSpatialReference(value.SpatialReference, false);
                    }
                }

                //OnFullExtentChanged?.Invoke(oldVal);
                //UpdateCommandUI();
            }
        }

        public OkEnvelope? Extent
        {
            get
            {
                return Transformation?.FittedBounds;
            }
        }
        //public IExtentStack ExtentStack { get; }

        public IGraphicsContainer GraphicsContainer { get { return _graphicsContainer; } }
        public virtual void CancelRender()
        {
            //_currentTask.Cancel();
        }

        //private readonly DelayDoImpl _delayRefresh = new DelayDoImpl(50);
        //public void Refresh(bool fClearBackImage = true)
        //{
        //    if (this.Extent == null)
        //    {
        //        return;
        //    }
        //    this.CancelRender();

        //    _delayRefresh.DelayDo(() =>
        //    {
        //        if (!_currentTask.IsCompleted)
        //        {
        //            _currentTask.Cancel();
        //            _currentTask.Wait();
        //        }
        //        m_pIScreenDisplay.Invalidate(null, true, OkScreenCache.okAllScreenCaches);
        //        if (fClearBackImage)
        //        {
        //            this.ScreenDisplay.Graphics?.Clear(Color.Transparent);
        //        }
        //        var t = this;//as T;

        //        _currentTask.Go(token => Draw(new TokenCancelTracker(token), OutputMode.ToScreen), token =>
        //        {
        //            Invoke(() =>
        //            {
        //                if (!SuprresEvents)
        //                {
        //                    Fire_ViewRefreshed(t, OkViewDrawPhase.okViewBackground);
        //                    FireContenChanged(t, this.ScreenDisplay.BackImage, (m_pIScreenDisplay as ScreenDisplay).ClipRect, ContentChangeType.Refresh);
        //                }
        //            });
        //        });
        //    });
        //}
        //public virtual void RefreshCache()
        //{
        //}
        //public void Invoke(Action action)
        //{
        //    Host.Container.Dispatcher.Invoke(action);
        //}
        public virtual void Invalidate()
        {
        }
        public abstract void SetExtent(OkEnvelope value, bool fRefresh = true);
        public abstract void Draw(ICancelTracker? trackCancel, OutputMode mode);
        //public void FireContenChanged(object sender, Bitmap bmp, RectangleF rc, ContentChangeType cct)
        //{
        //    OnContentChanged?.Invoke(sender, bmp, rc, cct);
        //}

        //public void FireDocumentEvents(eDocumentEventType type, object args = null)
        //{
        //    OnDocumentEvents?.Invoke(type, args);
        //}

        //public Cursor Cursor
        //{
        //    get { return Host.Container.Cursor; }
        //    set
        //    {
        //        Host.Container.Cursor = value;
        //    }
        //}

        public virtual void SaveToImage(string fileName, float bmpDpi, Action<double> reportProgress, ICancelTracker cancel, eImageFormat format = eImageFormat.JPG)
        {
        }
        #endregion


        protected static bool ISZERO(double x)
        {
            return ((x > 0 ? x : -x) < 9.9999999999999998E-13) ? true : false;
        }

        public virtual void Dispose()
        {
            this._graphicsContainer.Dispose();
            if(Display is IDisposable disposable)
            {
                disposable.Dispose();
            }
            //(this.ScreenDisplay as IDisposable)?.Dispose();
            //OnDispose?.Invoke();
        }
    }

    public class TransformChangeEventArgs
    {
        /// <summary>
        /// 比例是否发生了变化
        /// </summary>
        public bool ScaleChanged;
        /// <summary>
        /// 地图可视范围是否改变
        /// </summary>
        public bool MapExtentChanged;
        ///// <summary>
        ///// 本次变换地图是否被刷新了
        ///// </summary>
        //public bool MapRefreshed;
    }
}
