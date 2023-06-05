/*


 *
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   RasterRenderer
 * 创 建 人：   颜学铭
 * 创建时间：   2017/3/14 17:20:54
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Agro.GIS
{
    public class SimpleRasterRenderer : IRenderer, IXmlSerializable
    {
        private readonly LegendInfo _legendInfo = new();
        private LegendClass? _legendClass
        {
            get
            {
                var lg = _legendInfo.Get(0);
                return lg?.GetClass(0);
            }
        }
        public SimpleRasterRenderer()
        {
            var lg = new LegendGroup();
            lg.AddClass(new LegendClass());
            _legendInfo.Add(lg);

            var ls=SymbolUtil.CreateSimpleLineSymbol(System.Drawing.Color.Red);
            _legendClass.Symbol = SymbolUtil.CreateDefaultSelectionFillSymbol(ls as ILineSymbol);
        }
        #region IRenderer
        public LegendInfo LegendInfo
        {
            get {
                return _legendInfo;
            }
        }
        #endregion

        #region IXmlSerializable
        public System.Xml.Schema.XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
        }
        #endregion
    }
}
