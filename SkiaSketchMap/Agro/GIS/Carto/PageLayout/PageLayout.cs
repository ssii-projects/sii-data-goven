using Agro.LibCore;
using System.Drawing;
using System.Xml;

namespace Agro.GIS
{
    public class PageLayout:ActiveView
    {
        //public string? DocumentFileName { get; protected set; }
        private readonly Page m_pIPage = new();
        private IMapFrame? m_pIFocusMapFrame;

        #region IPageLayout
        public IPage Page { get { return m_pIPage; } }
        #endregion

        public PageLayout(IDisplay? dc):base(dc)
        {
            //m_pIPage = new(symbolFactory);
            //_host = p;
            //m_pIExtentStack = new ExtentStack(this);
            m_pIPage.OnPageSizeChanged += () =>
            {
                ReCalcFullExtent();
            };
            m_pIPage.OnPageUnitsChanged += () =>
            {
                this.Transformation.Units = m_pIPage.Units;
            };

            Transformation.Units = m_pIPage.Units;
            //(base.GraphicsContainer as GraphicsContainer).OnElementSelectChanged += () => OnElementSelectChanged?.Invoke(this);
            //this.ScreenDisplay?.SetDeviceFrame(new System.Drawing.Rectangle(0, 0, 512, 512),true);
        }

        public override Map? FocusMap
        {
            get
            {
                if (m_pIFocusMapFrame == null)
                    FocusNextMapFrame();
                if (m_pIFocusMapFrame != null)
                    return m_pIFocusMapFrame.Map;
                return null;
            }
        }
        public bool FocusNextMapFrame()
        {
            var g = _graphicsContainer;
            if (g._elements.Count == 0)
            {
                return false;
            }
            if (m_pIFocusMapFrame == null)
            {
                //for (long i = 0; i < (long)m_arrIElement.size(); i++)
                foreach (var vi in g._elements)
                {
                    // m_arrIElement[i].m_T;
                    if (vi.Element is IMapFrame pIMapFrame)
                    {
                        SetFocusMapFrame(pIMapFrame);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                int iFocusMapFrame = 0;
                for (int i = 0; i < g._elements.Count; i++)
                {
                    //if (m_arrIElement[i].m_T.IsEqualObject(m_pIFocusMapFrame))
                    if (g._elements[i] == m_pIFocusMapFrame)
                    {
                        iFocusMapFrame = i;
                        break;
                    }
                }

                for (int i = iFocusMapFrame + 1; i < g._elements.Count; i++)
                {
                    if (g._elements[i].Element is IMapFrame pIMapFrame)
                    {
                        SetFocusMapFrame(pIMapFrame);
                        return true;
                    }
                }
                for (int i = 0; i < iFocusMapFrame; i++)
                {
                    if (g._elements[i].Element is IMapFrame pIMapFrame)
                    {
                        SetFocusMapFrame(pIMapFrame);
                        return true;
                    }
                }
                return false;
            }
        }



        private bool m_bDrawingInProgress = false;
        public override void Draw(ICancelTracker? trackCancel, OutputMode mode)
        {
            if (m_bDrawingInProgress||Display==null)
                return;
            m_bDrawingInProgress = true;
            //m_pIScreenDisplay.StartDrawing(dc);

            //var bIsCacheDirty = m_pIScreenDisplay.IsCacheDirty();// OkScreenCache.okScreenRecording);//, &bIsCacheDirty);
            //if (bIsCacheDirty)
            //{
            //    m_pIScreenDisplay.StartRecording();
                DrawPageLayout(/*dc,*/ trackCancel, mode);
            //    m_pIScreenDisplay.StopRecording();
            //}
            //else
            //{
            //    m_pIScreenDisplay.DrawCache(m_pIScreenDisplay.Graphics, OkScreenCache.okScreenRecording); //draw from offscreen bitmap.
            //}
            m_bDrawingInProgress = false;
        }

        public bool OpenDocument(string fileName, bool fConnectSource = true)
        {
            //try
            //{
            var doc = new XmlDocument();
            doc.Load(fileName);
            if (doc.SelectSingleNode("/PageLayout") is XmlElement xe)
            {
                new PageLayoutXmlDocument(this).ReadXml(xe, FileUtil.GetFilePath(fileName));
                DocumentFileName = fileName;
            }
            //if (false)
            //{
            //    using (var reader = new StreamReader(fileName))
            //    using (var x = new System.Xml.XmlTextReader(reader))
            //    {
            //        new PageLayoutXmlDocument(this).ReadXml(x, FileUtil.GetFilePath(fileName));
            //        DocumentFileName = fileName;
            //        //FireDocumentEvents(eDocumentEventType.AfterOpenNotConnected, fileName);
            //        //if (fConnectSource)
            //        //{
            //        //    #region yxm 2018-3-9 异步连接数据源
            //        //    var gc = this.GraphicsContainer as GraphicsContainer;
            //        //    var task = new MyTask();
            //        //    task.Go(token =>
            //        //    {
            //        //        this.OnDispose += () =>
            //        //        {
            //        //            task.Cancel();
            //        //        };
            //        //        this.OnDocumentEvents += (t, o) =>
            //        //        {
            //        //            if (t == eDocumentEventType.AfterNew)
            //        //            {
            //        //                task.Cancel();
            //        //            }
            //        //        };

            //        //        gc.EnumElement(e =>
            //        //        {
            //        //            if (e is IMapFrame mf)
            //        //            {
            //        //                var map = mf.Map as Map;
            //        //                MapUtil.EnumFeatureLayer(map, fl =>
            //        //                {
            //        //                    try
            //        //                    {
            //        //                        if (token.IsCancellationRequested)
            //        //                        {
            //        //                            return false;
            //        //                        }
            //        //                        OnLayerPreConnected?.Invoke(map, fl);
            //        //                        var fc = fl.DataSourceMeta.Connect();
            //        //                        fl.FeatureClass = fc;
            //        //                    }
            //        //                    catch (Exception ex)
            //        //                    {
            //        //                        (fl as FeatureLayer).DatasourceException = ex;
            //        //                    }
            //        //                    return true;
            //        //                });
            //        //            }
            //        //        });
            //        //        Invoke(() =>
            //        //        {
            //        //            this.UpdateCommandUI();
            //        //            this.IsDocumentDirty = false;
            //        //            FireDocumentEvents(eDocumentEventType.AfterOpen, fileName);
            //        //            FireFocusMapChanged(null, FocusMap);
            //        //            this.SuprresEvents = false;
            //        //            this.Refresh();
            //        //        });
            //        //    });
            //        //    #endregion
            //        //}
            //    }
            //}
            return true;
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //finally
            //{
            //    this.SuprresEvents = false;
            //}
        }

        public void ClearDocument()
        {
            try
            {
                CancelRender();
                this.SuprresEvents = true;
                Clear();
                //this.ExtentStack.Reset();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void Clear()
        {
            var gc = GraphicsContainer;
            gc.UnselectAllElements();
            gc.DeleteAllElements();
            //FireFocusMapChanged(null, null);
        }

        public override void SaveToImage(string fileName, float bmpDpi, Action<double> reportProgress, ICancelTracker? cancel = null, eImageFormat format = eImageFormat.JPG)
        {
            if (FullExtent != null)
            {
                var dc = Display!;
                Transformation.Resolution = bmpDpi;
                var wi = (int)(UnitsUtil.ConvertUnits(this.Page.Units, OkUnits.okInches, this.Page.Width) * bmpDpi);
                var hi = (int)(UnitsUtil.ConvertUnits(this.Page.Units, OkUnits.okInches, this.Page.Height) * bmpDpi);
                //Console.WriteLine($"bmp size is ({wi}*{hi}");
                dc.Resize(wi, hi);
                Transformation.SetDeviceFrame(new Rectangle(0, 0, (int)wi, (int)hi));
                //ZoomToPercent(100, false);
                SetExtent(this.FullExtent, false);
                RefreshMaps();
                Draw(NotCancelTracker.Instance, OutputMode.ToPicture);
                dc.SaveToFile(fileName);
            }
        }

        public override void SetExtent(OkEnvelope value, bool fRefresh = true)
        {
            if (SuprresEvents)
            {
                fRefresh = false;
            }
            var oldScale = Transformation?.ScaleRatio;
            OkEnvelope? oldExtent = null;
            if (Extent != null)
            {
                oldExtent = new OkEnvelope(Extent);
            }

            //System.Diagnostics.Debug.Assert(m_pIScreenDisplay != null);
            var pIDisplayTransformation = Transformation;// m_pIScreenDisplay.DisplayTransformation;
            pIDisplayTransformation.VisibleBounds = value;
            //if (fRefresh)
            //{
            //    if (this.ExtentStack != null)
            //        this.ExtentStack.Do(value);
            //    Refresh();
            //}

            //#region yxm 2018-7-1
            if (fRefresh)
            {
                RefreshMaps();
            }
            //#endregion
            //if (fRefresh)
            //{
            //    UpdateCommandUI();
            //}

            #region 2019-1-2
            if (fRefresh)
            {
                if (OnTransformChanged != null)
                {
                    //_transformChangedEventArgs.MapRefreshed = fRefresh;
                    _transformChangedEventArgs.ScaleChanged = oldScale != Transformation.ScaleRatio;
                    _transformChangedEventArgs.MapExtentChanged = oldExtent == null || oldExtent.Equals(this.Extent);
                    OnTransformChanged(_transformChangedEventArgs);
                }
            }
            #endregion
        }

        public void ZoomToPercent(double percent, bool fRefresh = true)
        {
            if (ISZERO(percent))
                return;
            var pIDT = Transformation;// m_pIScreenDisplay.DisplayTransformation;
            pIDT.ZoomResolution = false;
            pIDT.ScaleRatio = 1 / ((double)percent / 100);
            pIDT.ZoomResolution = true;

            //pIDT.SetDeviceFrameF(pIDT.DeviceFrameF);
            SetExtent(pIDT.VisibleBounds, fRefresh);
            //pIDT.FittedBounds.ScaleAt(

            //RefreshMaps();

            if (fRefresh)
            {
                Refresh();
            }
        }
        public void Refresh(bool fClearBackImage = true)
        {
            throw new NotImplementedException();
            //if (this.Extent == null)
            //{
            //    return;
            //}
            //this.CancelRender();

            //_delayRefresh.DelayDo(() =>
            //{
            //    if (!_currentTask.IsCompleted)
            //    {
            //        _currentTask.Cancel();
            //        _currentTask.Wait();
            //    }
            //    m_pIScreenDisplay.Invalidate(null, true, OkScreenCache.okAllScreenCaches);
            //    if (fClearBackImage)
            //    {
            //        this.ScreenDisplay.Graphics?.Clear(Color.Transparent);
            //    }
            //    var t = this;//as T;

            //    _currentTask.Go(token => Draw(new TokenCancelTracker(token), OutputMode.ToScreen), token =>
            //    {
            //        Invoke(() =>
            //        {
            //            if (!SuprresEvents)
            //            {
            //                Fire_ViewRefreshed(t, OkViewDrawPhase.okViewBackground);
            //                FireContenChanged(t, this.ScreenDisplay.BackImage, (m_pIScreenDisplay as ScreenDisplay).ClipRect, ContentChangeType.Refresh);
            //            }
            //        });
            //    });
            //});
        }
        internal void SetFocusMapFrame(IMapFrame mf)
        {
            //var oldFocusMap = m_pIFocusMapFrame?.Map;
            //var newFocusMap = mf?.Map;
            m_pIFocusMapFrame = mf;
            //FireFocusMapChanged(oldFocusMap, newFocusMap);
        }

        private void ReCalcFullExtent()
        {
            var dtf = this.Transformation;
            m_pIPage.QuerySize(out double width, out double height);
            var bounds = new OkEnvelope(0, width, 0, height);
            dtf.Bounds = bounds;
        }

        private void RefreshMaps()
        {
            var lst = _graphicsContainer._elements;
            foreach (var ei in lst)
            {
                if (ei.Element is MapFrame mf)
                {
                    mf.UpdateLayout();
                }
            }
        }

        private void DrawPageLayout(ICancelTracker? cancel, OutputMode mode)//Action<Bitmap, RectangleF, bool> flushCallback, bool fFlushOnce)
        {
            var trackCancel = cancel ?? NotCancelTracker.Instance;
            var trans = this.Transformation;// m_pIScreenDisplay.DisplayTransformation;
            var rect = trans.DeviceFrame;
            int nWidth = rect.Width;
            int nHeight = rect.Height;
            if (nWidth == 0 || nHeight == 0)
                return;
            var dc = Display!;
            try
            {
                m_pIPage.DrawPaper(dc, trans, mode,Color.White);

                var elements = _graphicsContainer._elements;
                foreach (var e in elements)
                {
                    if (trackCancel.Cancel())
                    {
                        break;
                    }
                    var rc1 = e.Element.QueryBounds();// dc, pIDT);
                    if (rc1 != null)
                    {
                        rc1 = trans.ToDevice(rc1);
                        if (!(rc1.MaxX < 0 || rc1.MaxY < 0 || rc1.MinX > dc.Width || rc1.MinY > dc.Height))
                        {
                            //e.Element.Activate(m_pIScreenDisplay);
                            e.Element.Draw(/*m_pIScreenDisplay, pIDT,*/ trackCancel, mode);
                        }
                    }
                }
                //if (mode == OutputMode.ToScreen)
                //{
                //    var gc = GraphicsContainer;
                //    foreach (var e in gc.SelectedElements)
                //    {
                //        var pIST = e.GetSelectionTracker();
                //        if (pIST != null)
                //        {
                //            var trackerStyle = TrackerStyle.okTrackerNormal;
                //            if (gc.DominantElement == e)
                //                trackerStyle = TrackerStyle.okTrackerDominant;
                //            pIST.Draw(dc, dc.Graphics, trackerStyle);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("PageLayout.DrawPageLayout ex=" + ex.Message);
            }
        }

    }
}
