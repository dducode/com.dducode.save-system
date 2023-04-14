using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveSystem {

    /// <summary>
    /// Adapter to class <see cref="BinaryWriter"/> for simplify writing data
    /// </summary>
    public class UnityWriter : IDisposable {

        private readonly BinaryWriter m_writer;


        public UnityWriter (BinaryWriter writer) {
            m_writer = writer;
        }


        public void Write (Vector3 position) {
            m_writer.Write(position.x);
            m_writer.Write(position.y);
            m_writer.Write(position.z);
        }


        public void Write (Quaternion rotation) {
            m_writer.Write(rotation.x);
            m_writer.Write(rotation.y);
            m_writer.Write(rotation.z);
            m_writer.Write(rotation.w);
        }


        public void Write (Color color) {
            m_writer.Write(color.r);
            m_writer.Write(color.g);
            m_writer.Write(color.b);
            m_writer.Write(color.a);
        }


        public void Write<T> (T obj) {
            m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write<T> (List<T> list) {
            m_writer.Write(list.Count);
            foreach (var obj in list)
                m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write<T> (T[] array) {
            m_writer.Write(array.Length);
            foreach (var obj in array)
                m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write (byte value) {
            m_writer.Write(value);
        }


        public void Write (short value) {
            m_writer.Write(value);
        }


        public void Write (int value) {
            m_writer.Write(value);
        }


        public void Write (long value) {
            m_writer.Write(value);
        }


        public void Write (char value) {
            m_writer.Write(value);
        }


        public void Write (string value) {
            m_writer.Write(value);
        }


        public void Write (float value) {
            m_writer.Write(value);
        }


        public void Write (double value) {
            m_writer.Write(value);
        }


        public void Write (bool value) {
            m_writer.Write(value);
        }


        public void Dispose () {
            m_writer?.Dispose();
        }

    }

}