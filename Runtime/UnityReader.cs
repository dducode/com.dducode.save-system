using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveSystem {

    /// <summary>
    /// Адаптер к классу <see cref="BinaryReader"/> для упрощения чтения данных
    /// </summary>
    public class UnityReader {

        private readonly BinaryReader m_reader;


        public UnityReader (BinaryReader reader) {
            m_reader = reader;
        }


        public Vector3 ReadPosition () {
            return new Vector3 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle()
            };
        }


        public Quaternion ReadRotation () {
            return new Quaternion {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle(),
                w = m_reader.ReadSingle()
            };
        }


        public Color ReadColor () {
            return new Color {
                r = m_reader.ReadSingle(),
                g = m_reader.ReadSingle(),
                b = m_reader.ReadSingle(),
                a = m_reader.ReadSingle()
            };
        }


        public T ReadObject<T> () {
            return JsonUtility.FromJson<T>(m_reader.ReadString());
        }


        public List<T> ReadListObjects<T> () {
            var count = m_reader.ReadInt32();
            var list = new List<T>(count);

            for (var i = 0; i < count; i++)
                list.Add(JsonUtility.FromJson<T>(m_reader.ReadString()));

            return list;
        }


        public T[] ReadArrayObjects<T> () {
            var length = m_reader.ReadInt32();
            var array = new T[length];

            for (var i = 0; i < length; i++)
                array[i] = JsonUtility.FromJson<T>(m_reader.ReadString());

            return array;
        }


        public byte ReadByte () {
            return m_reader.ReadByte();
        }


        public short ReadShort () {
            return m_reader.ReadInt16();
        }


        public int ReadInt () {
            return m_reader.ReadInt32();
        }


        public long ReadLong () {
            return m_reader.ReadInt64();
        }


        public char ReadChar () {
            return m_reader.ReadChar();
        }


        public string ReadString () {
            return m_reader.ReadString();
        }


        public float ReadFloat () {
            return m_reader.ReadSingle();
        }


        public double ReadDouble () {
            return m_reader.ReadDouble();
        }


        public bool ReadBool () {
            return m_reader.ReadBoolean();
        }

    }

}