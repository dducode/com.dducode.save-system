using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaveSystem {

    /// <summary>
    /// Adapter to class <see cref="BinaryReader"/> for simplify reading data
    /// </summary>
    public class UnityReader : IDisposable {

        private readonly BinaryReader m_reader;


        public UnityReader (BinaryReader reader) {
            m_reader = reader;
        }


        public Vector2 ReadVector2 () {
            return new Vector2 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle()
            };
        }


        public Vector3 ReadVector3 () {
            return new Vector3 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle()
            };
        }


        public Vector4 ReadVector4 () {
            return new Vector4 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle(),
                w = m_reader.ReadSingle()
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


        public Color32 ReadColor32 () {
            return new Color32 {
                r = m_reader.ReadByte(),
                g = m_reader.ReadByte(),
                b = m_reader.ReadByte(),
                a = m_reader.ReadByte()
            };
        }


        public Matrix4x4 ReadMatrix () {
            var matrix = new Matrix4x4();
            for (var i = 0; i < 16; i++)
                matrix[i] = m_reader.ReadSingle();
            return matrix;
        }


        public T ReadObject<T> () {
            return JsonUtility.FromJson<T>(m_reader.ReadString());
        }


        public T ReadMonoBehaviour<T> () where T : MonoBehaviour {
            var localPath = m_reader.ReadString();
            var mono = Object.Instantiate(Resources.Load<T>(localPath));
            mono.name = m_reader.ReadString();
            mono.enabled = m_reader.ReadBoolean();
            mono.transform.SetSiblingIndex(m_reader.ReadInt32());
            mono.transform.position = ReadVector3();
            mono.transform.rotation = ReadRotation();

            return mono;
        }


        public List<T> ReadListObjects<T> () {
            var count = m_reader.ReadInt32();
            var listObjects = new List<T>(count);

            for (var i = 0; i < count; i++)
                listObjects.Add(ReadObject<T>());

            return listObjects;
        }


        public List<T> ReadListMonoBehaviours<T> () where T : MonoBehaviour {
            var count = m_reader.ReadInt32();
            var listMonoBehaviours = new List<T>(count);
            
            for (var i = 0; i < count; i++)
                listMonoBehaviours.Add(ReadMonoBehaviour<T>());

            return listMonoBehaviours;
        }


        public T[] ReadArrayObjects<T> () {
            var length = m_reader.ReadInt32();
            var arrayObjects = new T[length];

            for (var i = 0; i < length; i++)
                arrayObjects[i] = ReadObject<T>();

            return arrayObjects;
        }


        public T[] ReadArrayMonoBehaviours<T> () where T : MonoBehaviour {
            var length = m_reader.ReadInt32();
            var arrayMonoBehaviours = new T[length];

            for (var i = 0; i < length; i++)
                arrayMonoBehaviours[i] = ReadMonoBehaviour<T>();

            return arrayMonoBehaviours;
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


        public void Dispose () {
            m_reader?.Dispose();
        }

    }

}