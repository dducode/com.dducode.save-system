using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SaveSystemPackage {

    [XmlRoot("map")]
    [Serializable]
    public class Map<Tkey, TValue> : Dictionary<Tkey, TValue>, IXmlSerializable {

        public Map () { }
        public Map (Dictionary<string, string> dictionary) : base((IDictionary<Tkey, TValue>)dictionary) { }


        public XmlSchema GetSchema () {
            return null;
        }


        public void ReadXml (XmlReader reader) {
            if (reader.IsEmptyElement)
                return;

            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement) {
                reader.ReadStartElement("keyValuePair");

                reader.ReadStartElement("key");
                object key = reader.ReadContentAs(typeof(Tkey), null);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                object value = reader.ReadContentAs(typeof(TValue), null);
                reader.ReadEndElement();

                reader.ReadEndElement();

                Add((Tkey)key ?? throw new InvalidOperationException(), (TValue)value);
                reader.Read();
            }
        }


        public void WriteXml (XmlWriter writer) {
            foreach ((Tkey key, TValue value) in this) {
                writer.WriteStartElement("keyValuePair");

                writer.WriteStartElement("key");
                writer.WriteValue(key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                writer.WriteValue(value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

    }

}