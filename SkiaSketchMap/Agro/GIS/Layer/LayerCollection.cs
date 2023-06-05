using Agro.LibCore;
using GeoAPI.Geometries;
/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   LayerCollection
 * 创 建 人：   颜学铭
 * 创建时间：   2016/10/8 10:45:32
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.GIS
{
    public class LayerCollection: LayerBase,IDisposable
	{
        protected readonly List<ILayer> _layers = new();

        public ILayer GetLayer(int i)
        {
            //if (i < 0 || i >= _layers.Count)
            //    return null;
            return _layers[i];
        }

        public int LayerCount
        {
            get
            {
                return _layers.Count;
            }
        }
        /// <summary>
        /// yxm 2018-2-27
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public int IndexOf(ILayer layer)
        {
            return _layers.IndexOf(layer);
        }

        /// <summary>
        /// 显示到Toc控件上是是否展开下级
        /// </summary>
        public bool IsExpanded
        {
            get;
            set;
        }
        public LayerCollection(Map map)
        {
            Map = map;
        }
        public ILayer AddLayer(ILayer layer)
        {
			if (!_layers.Contains(layer))
			{
				SetLayerParent(layer);
				_layers.Add(layer);
				FireLayerChanged(ELayerCollectionChangeType.Add, layer);
			}
			return layer;
        }

		/// <summary>
		/// yxm 2018-11-23
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public ILayer? FindLayer(Predicate<ILayer> predicate)
		{
			foreach (var layer in _layers)
			{
				if (predicate(layer))
				{
					return layer;
				}
			}
			return null;
		}

		/// <summary>
		/// yxm 2018-2-27
		/// 将图层layer放到指定的索引处
		/// </summary>
		/// <param name="index"></param>
		/// <param name="layer"></param>
		public void InsertLayer(int index, ILayer layer)
		{
			if (index > _layers.Count - 1)
			{
				_layers.Add(layer);
			}
			else if (index < 0)
			{
				_layers.Insert(0, layer);
			}
			else
			{
				_layers.Insert(index, layer);
			}
			SetLayerParent(layer);
		}
		public void ClearLayers()
		{
			Map?.CancelRender();
			while (_layers.Count > 0)
			{
				RemoveLayer(_layers[_layers.Count - 1]);
			}
			_layers.Clear();
			FireLayerChanged(ELayerCollectionChangeType.Clear);
		}
		public void RemoveLayer(ILayer layer)
		{
			var lc = MapUtil.FindParent(this, layer);
			if (lc == null)
				return;
			Map?.CancelRender();
			layer.CancelTask();
			RemoveParent(layer);
			if (layer is GroupLayer gl)
			{
				gl.ClearLayers();
			}
			lc._layers.Remove(layer);
			FireLayerChanged(ELayerCollectionChangeType.Remove, layer);
			if (layer is IDisposable d)
			{
				d.Dispose();
			}
		}

        public override bool Render(LayerRenderMeta meta, ICancelTracker cancel, Action drawCallback)// Func<IFeature, bool> drawFeatureCallback)
        {
            foreach (var layer in _layers)
			{
				if (!cancel.Cancel()&&layer.Visible)
				{
					if(Map!=null&& !layer.IsScaleVisible(Map.MapScale))
					{
						continue;
					}
					layer.Render(meta, cancel, drawCallback);
				}
			}
            return false;
        }

        /// <summary>
        /// yxm 2018-2-27 
        /// 内部移除，不释放不抛出事件
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        internal bool InnerRemoveLayer(ILayer layer)
		{
			RemoveParent(layer);
			return _layers.Remove(layer);
		}

        public override OkEnvelope? FullExtent
        {
            get
            {
                OkEnvelope? env = null;
                if (_layers.Count > 0)
                {
                    foreach (var layer in _layers)
                    {
                        var e = layer.FullExtent;
                        if (e == null)
                            continue;
                        if (env == null)
                        {
                            env = new OkEnvelope(e);
                        }
                        else
                        {
                            env.ExpandToInclude(e);
                        }
                    }
                }
				return env;
            }
        }

		private void SetLayerParent(ILayer layer)
		{
			if (layer is LayerBase lb&&lb.Parent!=this)
			{
				lb.Parent = this;
			}
		}
		private void RemoveParent(ILayer layer)
		{
			if (layer is LayerBase lb)
			{
				lb.Parent = null;
			}
		}

		private void FireLayerChanged(ELayerCollectionChangeType type, object? args = null)
		{
			//Map?.FireLayerChanged(type, args);
		}
        public virtual void Dispose()
        {
            foreach (var fl in _layers)
            {
				if (fl is IDisposable dis)
				{
					dis.Dispose();
				}
			}
            _layers.Clear();
        }
    }
    public enum ELayerCollectionChangeType
    {
        Add,
        Remove,
        Clear,
        VisibleChanged,
        //OpenDocument,
    }
}
