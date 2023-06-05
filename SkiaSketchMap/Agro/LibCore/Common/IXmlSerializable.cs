using System.Xml;

namespace Agro.LibCore.Xml
{
    public interface IXmlSerializable
    {
        void ReadXml(XmlElement reader);
        void WriteXml(XmlWriter writer);
    }
}
