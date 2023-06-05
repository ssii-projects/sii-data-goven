/*
 * CLR 版本：   4.0.30319.34014            最低的 Framework 版本：4.0
 * 文 件 名：   CloneUtil
 * 创 建 人：   颜学铭
 * 创建时间：   2016/12/15 11:06:13
 * 版    本：   1.0.0
 * 备注描述：
 * 修订历史：
*/

using Agro.LibCore.Xml;
using System.Xml;

namespace Agro.LibCore
{
    public class CloneUtil
    {
        /// <summary>
        /// 通过xml序列化实现对象的克隆
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static object Clone<T>(IXmlSerializable xml) where T : IXmlSerializable, new()
        {
            var t = new T();

            var doc = new XmlDocument();
            //var x=doc.CreateElement(t.GetType().Name);

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            using var x =XmlWriter.Create(writer);
            x.WriteStartElement(t.GetType().Name);
            xml.WriteXml(x);
            x.WriteEndElement();
            x.Flush();


            var strXML = System.Text.Encoding.Default.GetString(ms.ToArray());
            //using var reader = new System.Xml.XmlTextReader(new StringReader(strXML));
            //t.ReadXml(x);

            doc.LoadXml(strXML);
            if (doc.LastChild is XmlElement e)
            {
                t.ReadXml(e);
            }
            return t;
        }
    }
}
