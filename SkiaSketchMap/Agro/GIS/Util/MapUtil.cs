/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   MapUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/10/25 16:06:27
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.GIS
{
    public class MapUtil
    {
        public static KeyValuePair<LegendClass,ISymbol> GetLayerFirstSymbol(ILayer layer)
        {
            ISymbol symbol = null;
            LegendClass lc = null;
            var legendInfo = layer.Renderer.LegendInfo;
            var lg = legendInfo.Get(0);
            if (legendInfo.Count == 1 && lg != null
                && lg.ClassCount == 1)
            {
                lc = lg.GetClass(0);//[0];
                //ti.BitmapImg=TocUtil.ToImageSource(lc.Symbol, 24, 24);
                symbol = lc.Symbol;
                //ti.LegendClass = lc;
                //a.Symbol.Draw()
            }
            else
            {
                //todo...
                //Log.WriteLine("enumLayer:todo...");
            }
            return new KeyValuePair<LegendClass,ISymbol>(lc,symbol);
        }
        public static void EnumLayer(Map map, Func<ILayer, bool> callback)
        {
            var layers = map.Layers;
            for (int i = 0; i < layers.LayerCount; ++i)
            {
                bool fContue=EnumLayer(layers.GetLayer(i), callback);
                if (!fContue)
                {
                    break;
                }
            }
        }
        public static bool  EnumLayer(ILayer layer, Func<ILayer,bool> callback)
        {
            var fContinue=callback(layer);
            if (!fContinue)
            {
                return fContinue;
            }

			if (layer is LayerCollection gl)
			{
				for (int i = 0; i < gl.LayerCount; ++i)
				{
					fContinue = EnumLayer(gl.GetLayer(i), callback);
					if (!fContinue)
					{
						break;
					}
				}
			}
			return fContinue;
        }

		public static void EnumFeatureLayer(Map map, Func<IFeatureLayer, bool> callback)
        {
            var layers = map.Layers;
            for (int i = 0; i < layers.LayerCount; ++i)
            {
                bool fContiune= EnumFeatureLayer(layers.GetLayer(i), callback);
                if (!fContiune)
                {
                    break;
                }
            }
        }        
        public static bool EnumFeatureLayer(ILayer layer, Func<IFeatureLayer,bool> callback)
        {
            bool fContinue = true;
            EnumLayer(layer, lyr =>
            {
                if (lyr is IFeatureLayer fl)
                {
                    fContinue=callback(fl);
                    return fContinue;
                }
                return true;
            });
            return fContinue;
        }

		public static bool ReverseEnumLayer(ILayer layer, Func<ILayer, bool> callback)
		{
			var fContinue = callback(layer);
			if (!fContinue)
			{
				return fContinue;
			}

			if (layer is LayerCollection gl)
			{
				for (int i = gl.LayerCount - 1; i >= 0; --i)
				{
					fContinue = ReverseEnumLayer(gl.GetLayer(i), callback);
					if (!fContinue)
					{
						break;
					}
				}
			}
			return fContinue;
		}
		public static void ReverseEnumFeatureLayer(Map map, Func<IFeatureLayer, bool> callback)
		{
			var layers = map.Layers;
			for (int i = layers.LayerCount - 1; i >= 0; --i)
			{
				bool fContiune = ReverseEnumFeatureLayer(layers.GetLayer(i), callback);
				if (!fContiune)
				{
					break;
				}
			}
		}
		public static bool ReverseEnumFeatureLayer(ILayer layer, Func<IFeatureLayer, bool> callback)
		{
			bool fContinue = true;
			ReverseEnumLayer(layer, lyr =>
			{
				if (lyr is IFeatureLayer fl)
				{
					fContinue = callback(fl);
					return fContinue;
				}
				return true;
			});
			return fContinue;
		}
		/// <summary>
		/// 遍历需要绘制的图层，需要满足以下条件：
		/// 1.图层Visible属性为true;
		/// 2.当前地图比例在图层的可见比例范围内；
		/// 3.该图层的所有上级图层都可见；
		/// </summary>
		/// <param name="map"></param>
		/// <param name="callback"></param>
		public static void EnumRenderLayer(Map map, Func<ILayer, bool> callback,bool fReverseEnum=false)
        {
            var layers = map.Layers;
            double mapScale = map.MapScale;
            if (fReverseEnum)
            {
                for (int i = layers.LayerCount - 1; i >= 0;--i)
                {
                    bool fContinue = true;
                    EnumRenderLayer(layers.GetLayer(i), mapScale, layer =>
                    {
                        fContinue = callback(layer);
                        return fContinue;
                    }, fReverseEnum);
                    if (!fContinue)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < layers.LayerCount; ++i)
                {
                    bool fContinue = true;
                    EnumRenderLayer(layers.GetLayer(i), mapScale, layer =>
                    {
                        fContinue = callback(layer);
                        return fContinue;
                    }, fReverseEnum);
                    if (!fContinue)
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 遍历需要绘制的图层，需要满足以下条件：
        /// 1.图层Visible属性为true;
        /// 2.当前地图比例在图层的可见比例范围内；
        /// 3.该图层的所有上级图层都可见；
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="mapScale"></param>
        /// <param name="callback"></param>
        public static void EnumRenderLayer(ILayer layer, double mapScale, Func<ILayer, bool> callback, bool fReverseEnum = false)
        {
            if (!layer.Visible||!layer.IsScaleVisible(mapScale))
                return;
            var fContinue=callback(layer);
            if (!fContinue)
            {
                return;
            }
            if (layer is LayerCollection gl)
            {
                if (fReverseEnum)
                {
                    for (int i = gl.LayerCount - 1; i >= 0;--i)
                    {
                        EnumRenderLayer(gl.GetLayer(i), mapScale, l =>
                        {
                            fContinue = callback(l);
                            return fContinue;
                        });
                        if (!fContinue)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < gl.LayerCount; ++i)
                    {
                        EnumRenderLayer(gl.GetLayer(i), mapScale, l =>
                        {
                            fContinue = callback(l);
                            return fContinue;
                        });
                        if (!fContinue)
                        {
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 判断ancestorLayer是否是layer的祖级
        /// yxm 2018-2-27
        /// </summary>
        /// <param name="map"></param>
        /// <param name="ancestorLayer"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool IsAncestor(Map map,ILayer ancestorLayer,ILayer layer)
        {
            if(!(ancestorLayer is LayerCollection))
            {
                return false;
            }
            if (ancestorLayer == layer)
            {
                return false;
            }
            var pl = FindParent(map, layer);
            while (pl != null)
            {
                if (pl == ancestorLayer)
                {
                    return true;
                }
                if (pl is ILayer)
                {
                    pl = FindParent(map, pl as ILayer);
                }else
                {
                    break;
                }
            }
            return false;
        }
        public static LayerCollection FindParent(Map map, ILayer layer)
        {
            return FindParent(map.Layers, layer);
        }
        public static LayerCollection FindParent(LayerCollection lc, ILayer layer)
        {
            for (int i = lc.LayerCount - 1; i >= 0; --i)
            {
                var l = lc.GetLayer(i);
                if (l == layer)
                {
                    return lc;
                }
                if (l is LayerCollection)
                {
                    var pl = FindParent(l as LayerCollection, layer);
                    if (pl != null)
                    {
                        return pl;
                    }
                }
            }           
            return null;
        }

        public static string GetPathString(Map map,ILayer layer)
        {
            var lst = new List<string>();
            var gl = FindParent(map,layer) as GroupLayer;
            while (gl != null)
            {
                lst.Add(gl.Name);
                gl = FindParent(map, gl) as GroupLayer;
            }
            string str = null;
            for(int i = lst.Count-1; i >= 0; --i)
            {

                if (str == null)
                {
                    str = lst[i];
                }
                else
                {
                    str += "/" + lst[i];
                }
            }
            if (str == null)
            {
                str = layer.Name;
            }
            else
            {
                str += "/" + layer.Name;
            }
            return str;
        }

        public static bool IsLayerExist(Map map, ILayer layer)
        {
            bool fExist = false;
            EnumLayer(map, l =>
            {
                if (l == layer)
                {
                    fExist = true;
                    return false;
                }
                return true;
            });
            return fExist;
        }
		public static ILayer FindLayer(Map map, Predicate<ILayer> predicate)
		{
			ILayer layer = null;
			EnumLayer(map, l =>
            {
                if (predicate(l))
                {
					layer = l;
                    return false;
                }
                return true;
            });
			return layer;
		}
		public static IFeatureLayer FindFeatureLayer(Map map, Predicate<IFeatureLayer> predicate)
		{
			IFeatureLayer layer = null;
			EnumFeatureLayer(map, l =>
			{
				if (predicate(l))
				{
					layer = l;
					return false;
				}
				return true;
			});
			return layer;
		}
		//public static bool IsParent(LayerCollection lc, ILayer layer)
		//{
		//    for (int i = lc.LayerCount - 1; i >= 0; --i)
		//    {
		//        var l = lc.GetLayer(i);
		//        if (l == layer)
		//        {
		//            return true;
		//        }
		//    }
		//    return false;
		//}
	}
}
