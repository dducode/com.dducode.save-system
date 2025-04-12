using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace SaveSystemPackage {

    [XmlRoot("map")]
    [Serializable]
    public class Map<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable, ISerializationCallbackReceiver, ISaveData {

        [SerializeField]
        private SerializableKeyValuePair<TKey, TValue>[] map;

        public Map () { }
        public Map (Dictionary<TKey, TValue> dictionary) : base(dictionary) { }
        protected Map (SerializationInfo info, StreamingContext context) : base(info, context) { }


        public virtual XmlSchema GetSchema () {
            return null;
        }


        public void ReadXml (XmlReader reader) {
            if (reader.IsEmptyElement) {
                reader.Read();
                return;
            }

            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement) {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                object key = keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                object value = valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadEndElement();

                Add((TKey)key, (TValue)value);

                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }


        public void WriteXml (XmlWriter writer) {
            var keySerializer = new XmlSerializer(typeof(TKey));
            var valueSerializer = new XmlSerializer(typeof(TValue));

            foreach ((TKey key, TValue value) in this) {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }


        public void OnBeforeSerialize () {
            map = this
               .Select(item => (SerializableKeyValuePair<TKey, TValue>)item)
               .ToArray();
        }


        public void OnAfterDeserialize () {
            foreach ((TKey key, TValue value) in map)
                Add(key, value);
        }

    }

}