using Agro.LibCore;
using System.Reflection.PortableExecutable;
using System.Xml;
using System.Xml.Linq;

namespace Agro.GIS
{
    class PageLayoutXmlDocument
    {
        private readonly PageLayout _p;
        public PageLayoutXmlDocument(PageLayout p)
        {
            _p = p;
        }
        //public void WriteXml(System.Xml.XmlWriter writer, string documentPath)
        //{
        //    var av = _p;
        //    SerializeUtil.WriteAttribute(writer, "DocumentPath", documentPath);
        //    if (av.FocusMap != null)
        //    {
        //        SerializeUtil.WriteAttribute(writer, "FocusMap", av.FocusMap.ID);
        //    }
        //    SerializeUtil.WriteExtent(writer, av.FullExtent, "FullExtent");
        //    SerializeUtil.WriteExtent(writer, av.Extent);
        //    av.Page.WriteXml(writer);
        //    WriteElements(writer);
        //}

        public void ReadXml(XmlElement xml, string documentPath)
        {
            var av = _p;
            var oldSuppressEvent = av.SuprresEvents;
            try
            {
                av.SuprresEvents = true;
                OkEnvelope? mapExtent = null, fullExtent = null;
                var session = new SerializeSession(documentPath)
                {
                    OldDocumentPath = xml.GetAttribute("DocumentPath")
                };
                var sFocusMapID = xml.GetAttribute("FocusMap");
                foreach (XmlNode n in xml.ChildNodes)
                {
                    if (n is XmlElement xe)
                    {
                        switch (n.Name)
                        {
                            case "FullExtent": fullExtent = SerializeUtil.ReadExtent(xe); break;
                            case "Extent": mapExtent = SerializeUtil.ReadExtent(xe); break;
                            case "Page": av.Page.ReadXml(xe); break;
                            case "Elements": ReadElements(xe, session); break;
                            default:
                                System.Diagnostics.Debug.Assert(false, $"not process Property ${n.Name}");
                                break;
                        }
                    }
                }


                av.GraphicsContainer.EnumElement(e =>
                {
                    if (e is IMapSurround ms)
                    {
                        if (session._mapID.TryGetValue(ms, out var mapID))
                        {
                            av.GraphicsContainer.FindElement(el =>
                            {
                                if (el is IMapFrame mf && mf.Map != null && mf.Map.ID == mapID)
                                {
                                    ms.Map = mf.Map;
                                    return false;
                                }
                                return true;
                            });
                        }
                    }
                });

                if (sFocusMapID != null)
                {
                    av.GraphicsContainer.EnumElement(e =>
                    {
                        if (e is IMapFrame mf && mf.Map != null && mf.Map.ID == sFocusMapID)
                        {
                            av.SetFocusMapFrame(mf);
                        }
                    });
                }

                av.FullExtent = fullExtent;
                if (mapExtent != null)
                {
                    av.SetExtent(mapExtent);
                }

                session.Clear();
            }
            finally
            {
                av.SuprresEvents = oldSuppressEvent;
            }
        }
        //public void ReadXml(System.Xml.XmlReader reader, string documentPath)
        //{
        //    var av = _p;
        //    var session = new SerializeSession(documentPath);
        //    OkEnvelope? mapExtent = null, fullExtent = null;
        //    string? sFocusMapID = null;
        //    SerializeUtil.ReadNodeCallback(reader, "PageLayout", name =>
        //    {
        //        switch (name)
        //        {
        //            case "PageLayout":
        //                session.OldDocumentPath = SerializeUtil.ReadAttribute(reader, "DocumentPath");
        //                sFocusMapID = SerializeUtil.ReadAttribute(reader, "FocusMap");
        //                break;
        //            case "FullExtent":
        //                {
        //                    fullExtent = SerializeUtil.ReadExtent(reader, name);
        //                }
        //                break;
        //            case "Extent":
        //                {
        //                    mapExtent = SerializeUtil.ReadExtent(reader, name);
        //                }
        //                break;
        //            case "Page":
        //                av.Page.ReadXml(reader);
        //                break;
        //            case "Elements":
        //                {
        //                    ReadElements(reader, name, session, element =>
        //                    {
        //                        av.GraphicsContainer.AddElement(element);
        //                    });
        //                }
        //                break;
        //        }
        //    });

        //    av.GraphicsContainer.EnumElement(e =>
        //    {
        //        if (e is IMapSurround ms)
        //        {
        //            if (session._mapID.TryGetValue(ms, out var mapID))
        //            {
        //                av.GraphicsContainer.FindElement(el =>
        //                {
        //                    if (el is IMapFrame mf && mf.Map != null && mf.Map.ID == mapID)
        //                    {
        //                        ms.Map = mf.Map;
        //                        return false;
        //                    }
        //                    return true;
        //                });
        //            }
        //        }
        //    });

        //    if (sFocusMapID != null)
        //    {
        //        av.GraphicsContainer.EnumElement(e =>
        //        {
        //            if (e is IMapFrame mf && mf.Map != null && mf.Map.ID == sFocusMapID)
        //            {
        //                av.SetFocusMapFrame(mf);
        //            }
        //        });
        //    }

        //    av.FullExtent = fullExtent;
        //    if (mapExtent != null)
        //    {
        //        av.SetExtent(mapExtent);
        //    }

        //    session.Clear();
        //}
        //private void WriteElements(System.Xml.XmlWriter writer)
        //{
        //    var gc = _p.GraphicsContainer as GraphicsContainer;
        //    if (gc._elements.Count > 0)
        //    {
        //        writer.WriteStartElement("Elements");
        //        foreach (var e in gc._elements)
        //        {
        //            e.Element.WriteXml(writer);
        //        }
        //        writer.WriteEndElement();
        //    }
        //}

        private void ReadElements(XmlElement reader, SerializeSession session)
        {
            foreach(XmlNode n in reader.ChildNodes)
            {
                if(n is XmlElement e)
                {
                    //Console.WriteLine("element name is "+e.Name);
                    var element = SerializeUtil.CreateElement(n.Name);
                    //if (n.Name == "MapFrame")
                    //{
                    //    element = new MapFrame(this._p, new Map(_p.ScreenDisplay));
                    //}
                    //else
                    //{
                    //element = SerializeUtil.CreateElement(n.Name);
                    //}
                    if (element != null)
                    {
                        element.ReadXml(e, session);
                        //callback(element);
                        _p.GraphicsContainer.AddElement(element);
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false, $"not find class {n.Name}");
                    }
                }
            }
        }
        //private void ReadElements(System.Xml.XmlReader reader, string endElementName,
        //    SerializeSession session, Action<IElement> callback)
        //{
        //    SerializeUtil.ReadNodeCallback(reader, endElementName, name =>
        //    {
        //        IElement? element = null;
        //        if (name == "MapFrame")
        //        {
        //            element = new MapFrame(this._p, new Map());
        //        }
        //        else
        //        {
        //            element = SerializeUtil.CreateInstance<IElement>(name);
        //        }
        //        if (element != null)
        //        {
        //            element.ReadXml(reader, session);
        //            callback(element);
        //        }
        //    });
        //}
    }
}
