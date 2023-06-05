using Agro.LibCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agro.GIS
{
    public class LayerRenderMeta
    {
        //public Graphics Graphics { get; set; }
        public IDisplay? DC { get; set; }
        public IDisplayTransformation Transform { get; set; }// { return DC?.DisplayTransformation; } }
        public RectangleF ClipBounds { get; set; }

        /// <summary>
        /// yxm 2018-11-23
        /// 获取可见区域（检查ClipBounds与Transform.DeviceFrame的交集并转化为地图坐标返回）
        /// 若ClipBounds为空则返回Transform.FittedBounds
        /// 返回null表示无交集
        /// </summary>
        /// <returns></returns>
        public OkEnvelope? GetVisibleBounds()
        {
            var cb = this.ClipBounds;//.Graphics.ClipBounds;
           
            if (Transform?.DeviceFrame != null)
            {
                var df = Transform!.DeviceFrame;
                if (!cb.IsEmpty && (cb.Left > df.Left || cb.Top > df.Top || cb.Right < df.Right || cb.Bottom < df.Bottom))
                {
                    var left = Math.Max(cb.Left, df.Left);
                    var top = Math.Max(cb.Top, df.Top);
                    var right = Math.Min(cb.Right, df.Right);
                    var bottom = Math.Min(cb.Bottom, df.Bottom);
                    if (left >= right || top >= bottom)
                    {
                        return null;
                    }
                    var env = new OkEnvelope(left, right, top, bottom);
                    return Transform!.ToMap(env);
                    //Console.WriteLine("FeatyreLayer Render User Clip Query:env="+env+" ext="+ext);
                }
            }
            return Transform?.FittedBounds;
        }
    }
    public class RenderMeta
    {
        public IDisplayTransformation Transform { get; set; }
        //public Graphics Graphics { get; internal set; }
        public IDisplay dc { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public enum eDocumentEventType
    {
        AfterNew,
        /// <summary>
        /// 文档图层已经准备好但还没有连接数据源
        /// </summary>
        AfterOpenNotConnected,
        /// <summary>
        /// 文档已经打开
        /// </summary>
        AfterOpen,
        AfterSave,
        AfterSaveAs,
    }
}
