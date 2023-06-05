using Agro.LibCore;
using System;
using System.Xml;

namespace Agro.GIS
{
	public abstract class LayerBase : IBusyChanged
	{
		private bool _fVisible;
		protected LayerBase(Map mc = null)
		{
			Map = mc;
		}

		public Action<object, bool> OnBusyChanged { get; set; }

		internal LayerBase? Parent { get;set; }
		#region ILayer
		public string Name
		{
			get;
			set;
		}
		public string Description { get; set; }
		public bool Visible
		{
			get
			{
				return _fVisible;
			}
			set
			{
				//var oldVisible = _fVisible;
				_fVisible = value;
				//if (Map?.OnLayerChanged != null)
				//{
				//	Map.FireLayerChanged(ELayerCollectionChangeType.VisibleChanged, oldVisible);
				//}
			}
		}
		public bool TocCheckBoxVisible { get; set; } = true;

		private bool _busy;
		public bool IsBusy
		{
			get { return _busy; }
			set
			{
				if (_busy != value)
				{
					_busy = value;
					OnBusyChanged?.Invoke((ILayer)this, value);
					if(Parent!=null)Parent.IsBusy = value;
				}
			}
		}

		public double? MinVisibleScale
		{
			get;
			set;
		}

		public double? MaxVisibleScale
		{
			get;
			set;
		}

		public int? Alpha
		{
			get;
			set;
		}
		public object? Tag { get; set; }
		public Map? Map
		{
			get;
			set;
		}

		public virtual OkEnvelope? FullExtent
		{
			get { return null; }
		}

		public IRenderer? Renderer
		{
			get { return null; }
		}
		public virtual SpatialReference? SpatialReference
		{
			get
			{
				return null;
			}
		}
        /// <summary>
        /// 图层比例是否可见
        /// </summary>
        /// <param name="mapScale"></param>
        /// <returns></returns>
        public bool IsScaleVisible(double mapScale)
		{
            if (MinVisibleScale != null)
            {
                if (mapScale < (double)MinVisibleScale)
                {
                    return false;
                }
            }
            if (MaxVisibleScale != null)
            {
                if (mapScale > (double)MaxVisibleScale)
                {
                    return false;
                }
            }
			return true;
        }
        public abstract bool Render(LayerRenderMeta meta, ICancelTracker cancel, Action drawCallback);// Func<IFeature, bool> drawFeatureCallback)
		//{
		//	return false;
		//}

		public virtual void CancelTask()
		{
		}
        #endregion


        public virtual void ReadXml(XmlElement reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
        {
            throw new NotImplementedException();
        }

        #region IXmlSerializable
        public System.Xml.Schema.XmlSchema? GetSchema()
		{
			return null;
		}

		public virtual void ReadXml(System.Xml.XmlReader reader, SerializeSession session)// string oldMapDocumentPath, string newMapDocumentPath)
		{
			throw new NotImplementedException();
		}

		public virtual void WriteXml(System.Xml.XmlWriter writer, bool fWriteDataSource = true)
		{
			throw new NotImplementedException();
		}
		#endregion

		protected static string MakeNewPath(string path, string oldMapDocumentPath, string newMapDocumentPath)
		{
			if (oldMapDocumentPath != newMapDocumentPath &&
				!string.IsNullOrEmpty(oldMapDocumentPath)
				&& !string.IsNullOrEmpty(newMapDocumentPath))
			{
				if (path.IndexOf(oldMapDocumentPath) == 0)
				{
					var newPath = path.Substring(oldMapDocumentPath.Length);
					newPath = newMapDocumentPath + newPath;
					if (System.IO.File.Exists(newPath))
					{
						return newPath;
					}
				}
			}
			return path;
		}
	}
}
