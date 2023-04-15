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


        public void Write (Vector2 vector2) {
            m_writer.Write(vector2.x);
            m_writer.Write(vector2.y);
        }


        public void Write (Vector3 vector3) {
            m_writer.Write(vector3.x);
            m_writer.Write(vector3.y);
            m_writer.Write(vector3.z);
        }


        public void Write (Vector4 vector4) {
            m_writer.Write(vector4.x);
            m_writer.Write(vector4.y);
            m_writer.Write(vector4.z);
            m_writer.Write(vector4.w);
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


        public void Write (Color32 color32) {
            m_writer.Write(color32.r);
            m_writer.Write(color32.g);
            m_writer.Write(color32.b);
            m_writer.Write(color32.a);
        }


        public void Write (Matrix4x4 matrix) {
            for (var i = 0; i < 16; i++)
                m_writer.Write(matrix[i]);
        }


        public void Write<T> (T obj) {
            m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write<T> (string prefabPath, T monoBehaviour) where T : MonoBehaviour {
            if (monoBehaviour.transform.parent is not null) {
                const string message = "MonoBehaviour for writing must be root in hierarchy";
                throw new NotRootObjectException(message);
            }

            m_writer.Write(prefabPath);
            m_writer.Write(monoBehaviour.name);
            m_writer.Write(monoBehaviour.enabled);
            m_writer.Write(monoBehaviour.transform.GetSiblingIndex());
            Write(monoBehaviour.transform.position);
            Write(monoBehaviour.transform.rotation);
        }


        public void Write<T> (List<T> listObjects) {
            m_writer.Write(listObjects.Count);
            foreach (var obj in listObjects)
                Write(obj);
        }


        public void Write<T> (string prefabPath, List<T> listMonoBehaviours) where T : MonoBehaviour {
            m_writer.Write(listMonoBehaviours.Count);
            foreach (var monoBehaviour in listMonoBehaviours)
                Write(prefabPath, monoBehaviour);
        }


        public void Write<T> (T[] arrayObjects) {
            m_writer.Write(arrayObjects.Length);
            foreach (var obj in arrayObjects)
                Write(obj);
        }


        public void Write<T> (string prefabPath, T[] arrayMonoBehaviours) where T : MonoBehaviour {
            m_writer.Write(arrayMonoBehaviours.Length);
            foreach (var monoBehaviour in arrayMonoBehaviours)
                Write(prefabPath, monoBehaviour);
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