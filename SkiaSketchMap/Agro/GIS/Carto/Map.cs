using Agro.LibCore;
using GeoAPI.Geometries;
using System.Drawing;
using System.Xml;

namespace Agro.GIS
{
    //public interface IMap
    //{
    //    string ID { get; set; }
    //    string Name { get; set; }
    //    /// <summary>
    //    /// he scale of the map as a representative fraction.
    //    /// Scale is the relationship between the dimensions of features on a map and the geographic objects they represent on the earth, commonly expressed as a fraction or a ratio.  A map scale of 1/100,000 or 1:100,000 means that one unit of measure on the map equals 100,000 of the same units on the earth.
    //    /// The MapScale property is really a shortcut to IDisplayTransformation::ScaleRatio.
    //    /// </summary>
    //    double MapScale { get; set; }
    //    SpatialReference SpatialReference { get; }
    //    void SetSpatialReference(SpatialReference value, bool fRefresh = true);
    //    IDisplay ScreenDisplay
    //    {
    //        get;
    //    }

    //    void WriteXml(System.Xml.XmlWriter writer, string documentPath = "");
    //}
    public class Map : ActiveView, IDisposable//,IMap
    {
        public class LayerRenderer : IDisposable
        {
            public interface IInnerLayer : ILayer
            {
            }
            //public class SelectionLayer : LayerBase, IInnerLayer
            //{
            //    internal SelectionLayer(Map mc) : base(mc)
            //    {
            //    }

            //    public new bool Render(LayerRenderMeta meta, ICancelTracker cancel, Action drawCallback)
            //    {
            //        bool fCancel = false;
            //        var renderLayers = new List<IFeatureLayer>();
            //        MapUtil.EnumRenderLayer(Map, layer =>
            //        {
            //            fCancel = cancel.Cancel();
            //            if (fCancel)
            //            {
            //                return false;
            //            }
            //            if (layer is IFeatureLayer fl && fl.SelectionSet.Count > 0 && fl.FeatureClass != null)
            //            {
            //                renderLayers.Add(fl);
            //            }
            //            return true;
            //        });
            //        if (fCancel)
            //        {
            //            return false;
            //        }
            //        var g = meta.DC;//.Graphics;
            //        //g.Clear(Color.Transparent);

            //        var qf = new SpatialFilter();
            //        var env = GeometryUtil.MakePolygon(meta.Transform.FittedBounds);
            //        foreach (var fl in renderLayers)
            //        {
            //            fCancel = cancel.Cancel();
            //            if (fCancel)
            //            {
            //                break;
            //            }
            //            var symbol = fl.SelectionSymbol;
            //            if (symbol == null)
            //            {
            //                continue;
            //            }
            //            symbol.SetupDC(g);//, meta.Transform);
            //            try
            //            {
            //                qf.Geometry = env;
            //                qf.Oids = fl.SelectionSet;
            //                qf.GeometryField = fl.FeatureClass.ShapeFieldName;
            //                qf.SubFields = fl.FeatureClass.OIDFieldName;
            //                if (!string.IsNullOrEmpty(fl.FeatureClass.ShapeFieldName))
            //                {
            //                    qf.SubFields += "," + fl.FeatureClass.ShapeFieldName;
            //                }
            //                qf.SpatialRel = eSpatialRelEnum.eSpatialRelIntersects;
            //                (fl.FeatureClass as IFeatureClassRender).SpatialQueryAsync(qf, cancel, feature =>
            //                {
            //                    drawCallback();
            //                    var geo = feature.Shape;
            //                    geo.Project(this.Map.SpatialReference);
            //                    symbol.Draw(geo);
            //                    fCancel = cancel.Cancel();
            //                    return !fCancel;
            //                });
            //            }
            //            catch (Exception ex)
            //            {
            //                //Log.WriteLine(ex.Message);
            //            }
            //            finally
            //            {
            //                symbol.ResetDC();
            //            }
            //        }
            //        return !fCancel;
            //    }
            //}
            public class FeatureLabelLayer : LayerBase, IInnerLayer
            {
                internal FeatureLabelLayer(Map mc)
                    : base(mc)
                {
                }

                private readonly List<IPolygon> _listTotalLabelExtends = new ();
                public override bool Render(LayerRenderMeta meta, ICancelTracker cancel, Action drawCallback)
                {
                    bool fCancel = false;
                    var renderLayers = new List<IFeatureLayer>();
                    MapUtil.EnumRenderLayer(Map!, layer =>
                    {
                        if (cancel.Cancel())
                        {
                            return false;
                        }
                        if (layer is IFeatureLayer fl && fl.FeatureClass != null
                            && fl.FeatureLabeler != null && fl.FeatureLabeler.EnableLabel)
                        {
                            renderLayers.Add(fl);
                        }
                        return true;
                    });
                    if (cancel.Cancel())
                    {
                        return false;
                    }
                    var g = meta.DC;//.Graphics;
                    var trans = meta.Transform;

                    _listTotalLabelExtends.Clear();
                    var env = GeometryUtil.MakePolygon(meta.Transform.FittedBounds);
                    foreach (var fl in renderLayers)
                    {
                        fCancel = cancel.Cancel();
                        if (fCancel)
                        {
                            break;
                        }
                        var label = fl.FeatureLabeler;
                        label.SetTotalLabelExtends(_listTotalLabelExtends);
                        label.SetupDC(g,trans);//, meta.Transform);
                        try
                        {
                            var qf = new SpatialFilter
                            {
                                Geometry = env,
                                Oids = null,
                                GeometryField = fl.FeatureClass.ShapeFieldName,
                                SubFields = fl.FeatureClass.OIDFieldName + "," + fl.FeatureClass.ShapeFieldName,
                                SpatialRel = eSpatialRelEnum.eSpatialRelIntersects,
                                WhereClause = fl.UseWhere()
                            };
                            label.PrepareFilter(qf);
                            ((IFeatureClassRender)fl.FeatureClass).SpatialQueryAsync(qf, cancel, feature =>
                            {
                                drawCallback();
                                var geo = feature.Shape;
                                geo.Project(this.Map.SpatialReference);
                            label.Draw(feature);//, listTotalLabelExtends);
                                fCancel = cancel.Cancel();
                                return !fCancel;
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());   
                            //Log.WriteLine(ex.Message);
                        }
                        finally
                        {
                            label.ResetDC();
                        }
                    }
                    return !fCancel;
                }
            }

            private readonly Map _p;
            //private readonly LayerBackImageCache _layerBackImageCache = new LayerBackImageCache();
            internal readonly List<IInnerLayer> _innerLayers = new List<IInnerLayer>();
            public LayerRenderer(Map p)
            {
                _p = p;
                //_innerLayers.Add(new SelectionLayer(p));
                _innerLayers.Add(new FeatureLabelLayer(p));
            }

            internal void InvalidateLayer(ILayer layer)
            {
                //_layerBackImageCache.SetDirty(layer);
            }
            internal void InvalidateSelectionLayer()
            {
                //foreach (var l in _innerLayers)
                //{
                //    if (l is SelectionLayer)
                //    {
                //        //_layerBackImageCache.SetDirty(l);
                //    }
                //}
            }
            internal void InvalidateLabelLayer()
            {
                foreach (var l in _innerLayers)
                {
                    if (l is FeatureLabelLayer)
                    {
                        //_layerBackImageCache.SetDirty(l);
                    }
                }
            }
            internal void Invalidate()
            {
                //_layerBackImageCache.SetDirty();
            }
            internal void OnLayerRemoved(ILayer layer)
            {
                if (layer is IFeatureLayer fl && fl.FeatureLabeler != null && fl.FeatureLabeler.EnableLabel)
                {
                    InvalidateLabelLayer();
                }

                //_layerBackImageCache.Remove(layer);
            }

            internal void RenderBackground(RenderMeta meta, ICancelTracker cancel, OutputMode mode)
            {
                //if (cancel.Cancel())
                //{
                //    Log.WriteLine("线程取消.");
                //    return;
                //}

                //if (cancel.Cancel())
                //{
                //    Log.WriteLine("thread canceled.");
                //    return;
                //}
                ////Log.WriteLine("thread RenderBackground start." + iTestThreadID);
                var _renderLayers = ResetRenderLayers();
                //_layerBackImageCache.Remove(ri =>
                //{
                //    if (!_renderLayers.Contains(ri) && ri.fDirty)
                //    {
                //        return true;
                //    }
                //    return false;
                //});

                //bool fCancel = false;
                //var preTime = System.DateTime.Now;

                //var pSD = _p.ScreenDisplay;

                //try
                //{
                //    var layerRenderMeta = new LayerRenderMeta()
                //    {
                //        Transform = meta.Transform,
                //        ClipBounds = pSD.ClipRect,
                //    };

                //    for (int i = 0; i < _renderLayers.Count; ++i)
                //    {
                //        var ri = _renderLayers[i];
                //        fCancel = cancel.Cancel();
                //        if (fCancel)
                //        {
                //            break;
                //        }
                //        if (!ri.fDirty)
                //        {
                //            continue;
                //        }

                //        bool fFlush = true;
                //        ri.BackImage.Create(meta.Width, meta.Height);
                //        var layerImage = ri.BackImage.Bitmap;

                //        if (layerImage == null)
                //            continue;
                //        using (var g = ImageUtil.CreateGraphics(layerImage))
                //        {
                //            try
                //            {
                //                var cr = _p.m_pIScreenDisplay.ClipRect;
                //                if (!cr.IsEmpty)
                //                {
                //                    g.TranslateTransform(-cr.X, -cr.Y);
                //                }
                //            }
                //            catch { }
                //            layerRenderMeta.Graphics = g;
                //            ri.fTransparent = true;
                //            var fOK = ri.Layer.Render(layerRenderMeta, cancel, () =>
                //            {
                //                ri.fTransparent = false;
                //                if (mode == OutputMode.ToScreen)
                //                {
                //                    var now = DateTime.Now;
                //                    fFlush = now.Subtract(preTime).TotalMilliseconds > 800;
                //                    if (fFlush)
                //                    {
                //                        preTime = now;
                //                        Flush(meta, cancel, i, _renderLayers, mode);
                //                    }
                //                }
                //            });
                //            fCancel = cancel.Cancel();
                //            if (fOK && !fCancel && !fFlush)
                //            {
                //                ri.fDirty = false;
                //            }
                //        }
                //        if (cancel.Cancel())
                //        {
                //            break;
                //        }
                //    }
                //    fCancel = cancel.Cancel();
                //    if (!fCancel)
                //    {
                //        Flush(meta, cancel, _renderLayers.Count - 1, _renderLayers, mode, true);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Log.WriteLine(ex.ToString());
                //}
                //finally
                //{
                //    //Log.WriteLine("thread RenderBackground End." + iTestThreadID + ",fCancel=" + fCancel);
                //    //iTestThreadID++;
                //}
            }
            //private void Flush(RenderMeta meta1, ICancelTracker cancel, int iRenderLayer, List<RenderLayerItem> renderLayers
            //     , OutputMode mode, bool fLast = false)
            //{
            //    Invoke(() => {
            //        var pSD = _p.ScreenDisplay;
            //        //var g = pSD.DC;// Graphics;

            //        if (pSD is IDisplay g)
            //        {
            //            if (fLast)
            //            {
            //                g.Clear(Color.Transparent);
            //            }
            //            Compose(g, renderLayers, iRenderLayer + 1);
            //            if (fLast)
            //            {
            //                var gc = _p.GraphicsContainer as GraphicsContainer;
            //                gc.Render(pSD, cancel, mode);
            //            }
            //            if (mode == OutputMode.ToScreen)
            //            {
            //                _p.FireContenChanged(this, pSD.BackImage, (pSD as ScreenDisplay).ClipRect, ContentChangeType.Else);
            //            }
            //        }
            //    });
            //}
            //private void Compose(Graphics g, List<RenderLayerItem> renderLayers, int count)
            //{
            //    for (int i = 0; i < count; ++i)
            //    {
            //        var ri = renderLayers[i];
            //        if (ri.fTransparent)
            //        {
            //            //Log.WriteLine(ri.Layer.Name + " is no data to draw");
            //            continue;
            //        }
            //        var bmp = ri.BackImage.Bitmap;
            //        if (bmp == null)
            //            return;
            //        if (ri.Layer.Alpha != null)
            //        {
            //            using (var abmp = ImageUtil.SetPictureAlpha(bmp, (int)ri.Layer.Alpha))
            //            {
            //                g.DrawImage(abmp, -g.Transform.OffsetX, -g.Transform.OffsetY);
            //            }
            //        }
            //        else
            //        {
            //            g.DrawImage(bmp, -g.Transform.OffsetX, -g.Transform.OffsetY);
            //        }
            //    }
            //}
            public List<ILayer> ResetRenderLayers()
            {
                var lst = new List<ILayer>();
                MapUtil.EnumRenderLayer(_p, layer =>
                {
                    if (!(layer is LayerCollection))
                    {
                        //var ri = _layerBackImageCache.GetLayerBackImage(layer);
                        //lst.Add(ri);
                        lst.Add(layer);
                    }
                    return true;
                });
                foreach (var il in _innerLayers)
                {
                    //var ri = _layerBackImageCache.GetLayerBackImage(il);
                    //lst.Add(ri);
                    lst.Add(il);
                }
                return lst;
            }

            //public void Invoke(Action action)
            //{
            //    this._p.Invoke(action);
            //}
            public void Dispose()
            {
                //_layerBackImageCache.Dispose();
            }
        }
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "地图";
        public double MapScale
        {
            get
            {
                return Transformation.ScaleRatio;
            }
            set
            {
                if (value != Transformation.ScaleRatio)
                {
                    Transformation.ScaleRatio = value;
                    //DoExtent(Transformation.FittedBounds);
                    //this.Refresh();
                }
            }
        }
        public LayerCollection Layers { get; private set; }

        public SpatialReference SpatialReference
        {
            get
            {
                return this.Transformation.SpatialReference;
            }
        }
        //public IDisplay ScreenDisplay => throw new NotImplementedException();

        public override Map FocusMap => this;

        //public void CancelRender() { }
        private readonly LayerRenderer _render;

        public Action<FieldLabelValue>? OnGetLabelValue;

        public Map(IDisplay? dc=null) : base(dc)
        {
            Layers = new(this);
            _render = new(this);
        }

        public override void Draw(ICancelTracker? cancel, OutputMode mode)
        {
            var dc = Display;
            if (dc == null) return;
            var trackCancel = cancel ?? NotCancelTracker.Instance;
            if (mode == OutputMode.ToPicture)
            {
                var restoreDC = false;
                try
                {
                    if (_clipRect != null)
                    {
                        restoreDC = true;
                        dc.SaveDC();
                        dc.SetClipRect((RectangleF)_clipRect!);
                    }

                    var layerRenderMeta = new LayerRenderMeta()
                    {
                        DC = dc,
                        Transform = this.Transformation
                    };
                    for (var i = 0; i < this.Layers.LayerCount; ++i)
                    {
                        Layers.GetLayer(i).Render(layerRenderMeta, trackCancel, () => { });
                    }
                    foreach (var lyr in _render._innerLayers)
                    {
                        lyr.Render(layerRenderMeta, trackCancel, () => { });
                    }
                }
                finally
                {
                    if (restoreDC)
                    {
                        dc.RestorDC();
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void SetExtent(OkEnvelope value, bool fRefresh = true)
        {
            if (value == null)
            {
                return;
            }
            value = new OkEnvelope(value.Project(SpatialReference));

            Transformation.VisibleBounds = value;

            var df = Transformation.DeviceFrame;
            ResetMatrix(df);
            if (fRefresh)
            {
                //DoExtent(Transformation.FittedBounds ?? value);
                //Refresh();
            }
        }

        public void SetSpatialReference(SpatialReference value, bool fRefresh = true)
        {
            if (SpatialReference == value)
            {
                return;
            }
            this.Transformation.SpatialReference = value;
            #region project
            if (FullExtent != null)
            {
                try
                {
                    FullExtent = new OkEnvelope(FullExtent.Project(value));
                }
                catch
                {
                    FullExtent.SpatialReference= value;
                }
            }
            if (Extent != null)
            {
                try
                {
                    var ext = new OkEnvelope(Extent.Project(value));
                    SetExtent(ext, fRefresh);
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            #endregion
            //OnSpatialReferenceChanged?.Invoke(value);
        }

        public void ReadXml(XmlElement reader, SerializeSession session)// string documentPath = "", string oldMapDocumentPath=null)
        {
            new MapXmlDocument(this).ReadXml(reader, session);
        }

        //public void ReadXml(XmlReader reader, SerializeSession session)// string documentPath = "", string oldMapDocumentPath=null)
        //{
        //    new MapXmlDocument(this).ReadXml(reader, session);
        //}
        //public void WriteXml(XmlWriter writer, string documentPath = "")
        //{
        //    throw new NotImplementedException();
        //}

        private void ResetMatrix(Rectangle deviceFrame)//, bool fRefresh = true)
        {
            if (Transformation != null && Extent != null)
            {
                var oldScale = Transformation.ScaleRatio;
                var oldExtent = new OkEnvelope(Extent);
                Transformation.SetDeviceFrame(deviceFrame);
                _render.Invalidate();
                if (OnTransformChanged != null)
                {
                    //_transformChangedEventArgs.MapRefreshed = fRefresh;
                    _transformChangedEventArgs.ScaleChanged = oldScale != Transformation.ScaleRatio;
                    _transformChangedEventArgs.MapExtentChanged = oldExtent.Equals(this.Extent);
                    OnTransformChanged(_transformChangedEventArgs);
                }
            }
        }

        public override void Dispose()
        {
            _render.Dispose();
            Layers.Dispose();
            //_snapEnviroment?.Dispose();
            base.Dispose();
        }
    }

    class MapXmlDocument
    {
        private readonly Map _p;
        public MapXmlDocument(Map p)
        {
            _p = p;
        }
        //public void WriteXml(System.Xml.XmlWriter writer, string documentPath)
        //{
        //    var map = _p;
        //    //writer.WriteStartElement("DocumentPath");
        //    //writer.WriteAttributeString("value", documentPath);
        //    if (!string.IsNullOrEmpty(documentPath))
        //    {
        //        SerializeUtil.WriteAttribute(writer, "DocumentPath", documentPath);
        //    }
        //    SerializeUtil.WriteStringElement(writer, "ID", map.ID);
        //    SerializeUtil.WriteSpatialReference(writer, map.SpatialReference);
        //    SerializeUtil.WriteExtent(writer, map.FullExtent, "FullExtent");
        //    SerializeUtil.WriteExtent(writer, map.Extent);
        //    WriteLayers(writer);
        //}

        public void ReadXml(XmlElement reader, SerializeSession session)// string documentPath, string oldMapDocumentPath=null)
        {
            var oldSuppress = _p.SuprresEvents;
            _p.SuprresEvents = true;
            try
            {
                var documentPath = session.DocumentPath;
                var map = _p;
                OkEnvelope? mapExtent = null;
                foreach (XmlNode n in reader.ChildNodes)
                {
                    if (n is XmlElement e)
                    {
                        var name = n.Name;
                        switch (name)
                        {
                            case "ID":
                                map.ID = e.InnerText;
                                break;
                            //case "Map":
                            //    var oriDocpath = SerializeUtil.ReadAttribute(reader, "DocumentPath");
                            //    if (!string.IsNullOrEmpty(oriDocpath))
                            //    {
                            //        session.OldDocumentPath = oriDocpath;
                            //    }
                            //    break;
                            case "SpatialReference":
                                {
                                    var sr = SerializeUtil.ReadSpatialReference(e);
                                    if (sr != null)
                                    {
                                        map.SetSpatialReference(sr, false);
                                    }
                                }
                                break;
                            case "FullExtent":
                                {
                                    var pgn = SerializeUtil.ReadExtent(e);
                                    if (pgn != null)
                                    {
                                        map.FullExtent = pgn;
                                    }
                                }
                                break;
                            case "Extent":
                                {
                                    mapExtent = SerializeUtil.ReadExtent(e);
                                }
                                break;
                            case "Layers":
                                {

                                    SerializeUtil.ReadLayers(e, map, layer =>
                                    {
                                        _p.Layers.AddLayer(layer);
                                    }, session);
                                }
                                break;
                        }
                    }
                }
                if (mapExtent != null)
                {
                    map.SetExtent(mapExtent, false);
                }
            }
            finally
            {
                _p.SuprresEvents = oldSuppress;
            }
        }

        //public void ReadXml(System.Xml.XmlReader reader, SerializeSession session)// string documentPath, string oldMapDocumentPath=null)
        //{
        //    var oldSuppress = _p.SuprresEvents;
        //    _p.SuprresEvents = true;
        //    try
        //    {
        //        var documentPath = session.DocumentPath;
        //        //var oldMapDocumentPath = session.OldDocumentPath;
        //        var map = _p;
        //        OkEnvelope mapExtent = null;
        //        //string oriDocpath = session.OldDocumentPath;
        //        SerializeUtil.ReadNodeCallback(reader, "Map", name =>
        //        {
        //            switch (name)
        //            {
        //                case "ID":
        //                    map.ID = reader.ReadString();
        //                    break;
        //                case "Map":
        //                    var oriDocpath = SerializeUtil.ReadAttribute(reader, "DocumentPath");
        //                    if (!string.IsNullOrEmpty(oriDocpath))
        //                    {
        //                        session.OldDocumentPath = oriDocpath;
        //                    }
        //                    break;
        //                case "SpatialReference":
        //                    {
        //                        var sr = SerializeUtil.ReadSpatialReference(reader);
        //                        if (sr != null)
        //                        {
        //                            map.SetSpatialReference(sr, false);
        //                        }
        //                    }
        //                    break;
        //                case "FullExtent":
        //                    {
        //                        var pgn = SerializeUtil.ReadExtent(reader, name);
        //                        if (pgn != null)
        //                        {
        //                            map.FullExtent = pgn;
        //                        }
        //                    }
        //                    break;
        //                case "Extent":
        //                    {
        //                        mapExtent = SerializeUtil.ReadExtent(reader, name);
        //                    }
        //                    break;
        //                case "Layers":
        //                    {

        //                        SerializeUtil.ReadLayers(reader, map, name, layer =>
        //                        {
        //                            _p.Layers.AddLayer(layer);
        //                        }, session);
        //                    }
        //                    break;
        //            }
        //        });
        //        if (mapExtent != null)
        //        {
        //            map.SetExtent(mapExtent, false);
        //        }
        //    }
        //    finally
        //    {
        //        _p.SuprresEvents = oldSuppress;
        //    }
        //}
        //private void WriteLayers(System.Xml.XmlWriter writer)
        //{
        //    var map = _p;
        //    if (map.Layers.LayerCount > 0)
        //    {
        //        writer.WriteStartElement("Layers");
        //        for (int i = 0; i < map.Layers.LayerCount; ++i)
        //        {
        //            var layer = map.Layers.GetLayer(i);
        //            //var x = layer as System.Xml.Serialization.IXmlSerializable;
        //            //if (x != null)
        //            //{
        //            layer.WriteXml(writer);
        //            //}
        //        }
        //        writer.WriteEndElement();
        //    }
        //}
    }
}
