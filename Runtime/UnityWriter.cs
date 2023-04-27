using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystem {

    /// <summary>
    /// Adapter to class <see cref="BinaryWriter"></see> for simplify writing data
    /// </summary>
    public sealed class UnityWriter : IDisposable, IAsyncDisposable {

        private readonly BinaryWriter m_writer;
        public readonly string localPath;


        internal UnityWriter (BinaryWriter writer, string localPath) {
            m_writer = writer;
            this.localPath = localPath;
        }


        public void Write (Version version) {
            m_writer.Write(version.Major);
            m_writer.Write(version.Minor);
            m_writer.Write(version.Build);
            m_writer.Write(version.Revision);
        }


        public void Write (Vector2 vector2) {
            m_writer.Write(vector2.x);
            m_writer.Write(vector2.y);
        }


        public void Write (Vector2[] vector2Array) {
            m_writer.Write(vector2Array.Length);
            foreach (var vector2 in vector2Array)
                Write(vector2);
        }


        public void Write (Vector3 vector3) {
            m_writer.Write(vector3.x);
            m_writer.Write(vector3.y);
            m_writer.Write(vector3.z);
        }


        public void Write (Vector3[] vector3Array) {
            m_writer.Write(vector3Array.Length);
            foreach (var vector3 in vector3Array)
                Write(vector3);
        }


        public void Write (Vector4 vector4) {
            m_writer.Write(vector4.x);
            m_writer.Write(vector4.y);
            m_writer.Write(vector4.z);
            m_writer.Write(vector4.w);
        }


        public void Write (Vector4[] vector4Array) {
            m_writer.Write(vector4Array.Length);
            foreach (var vector4 in vector4Array)
                Write(vector4);
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


        public void Write (Color[] colors) {
            m_writer.Write(colors.Length);
            foreach (var color in colors)
                Write(color);
        }


        public void Write (Color32 color32) {
            m_writer.Write(color32.r);
            m_writer.Write(color32.g);
            m_writer.Write(color32.b);
            m_writer.Write(color32.a);
        }


        public void Write (Color32[] colors32) {
            m_writer.Write(colors32.Length);
            foreach (var color32 in colors32)
                Write(color32);
        }


        public void Write (Matrix4x4 matrix) {
            for (var i = 0; i < 16; i++)
                m_writer.Write(matrix[i]);
        }


        public void Write (Matrix4x4[] matrices) {
            m_writer.Write(matrices.Length);
            foreach (var matrix in matrices)
                Write(matrix);
        }


        public void Write (MeshData mesh) {
            m_writer.Write(mesh.subMeshes.Length);

            for (var i = 0; i < mesh.subMeshes.Length; i++) {
                var subMesh = mesh.subMeshes[i];
                m_writer.Write(subMesh.baseVertex);
                Write(subMesh.bounds.center);
                Write(subMesh.bounds.extents);
                Write(subMesh.bounds.max);
                Write(subMesh.bounds.min);
                Write(subMesh.bounds.size);
                m_writer.Write(subMesh.firstVertex);
                m_writer.Write(subMesh.indexCount);
                m_writer.Write(subMesh.indexStart);
                m_writer.Write(subMesh.vertexCount);
                m_writer.Write((int) subMesh.topology);
                Write(mesh.subMeshIndices[i]);
            }

            m_writer.Write(mesh.name);
            Write(mesh.vertices);
            Write(mesh.uv);
            Write(mesh.uv2);
            Write(mesh.uv3);
            Write(mesh.uv4);
            Write(mesh.uv5);
            Write(mesh.uv6);
            Write(mesh.uv7);
            Write(mesh.uv8);
            Write(mesh.colors32);
            Write(mesh.normals);
            Write(mesh.tangents);
            Write(mesh.triangles);
            Write(mesh.bounds.center);
            Write(mesh.bounds.extents);
            Write(mesh.bounds.max);
            Write(mesh.bounds.min);
            Write(mesh.bounds.size);
            m_writer.Write((byte) mesh.indexBufferTarget);
            m_writer.Write((byte) mesh.indexFormat);
            m_writer.Write((byte) mesh.vertexBufferTarget);
        }


        public void Write<T> (T obj) {
            m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write<T> (T[] arrayObjects) {
            m_writer.Write(arrayObjects.Length);
            foreach (var obj in arrayObjects)
                Write(obj);
        }


        public void Write (byte byteValue) {
            m_writer.Write(byteValue);
        }


        public void Write (byte[] bytes) {
            m_writer.Write(bytes.Length);
            foreach (var byteValue in bytes)
                m_writer.Write(byteValue);
        }


        public void Write (short shortValue) {
            m_writer.Write(shortValue);
        }


        public void Write (short[] shorts) {
            m_writer.Write(shorts.Length);
            foreach (var shortValue in shorts)
                m_writer.Write(shortValue);
        }


        public void Write (int intValue) {
            m_writer.Write(intValue);
        }


        public void Write (int[] ints) {
            m_writer.Write(ints.Length);
            foreach (var intValue in ints)
                m_writer.Write(intValue);
        }


        public void Write (long longValue) {
            m_writer.Write(longValue);
        }


        public void Write (long[] longs) {
            m_writer.Write(longs.Length);
            foreach (var longValue in longs)
                m_writer.Write(longValue);
        }


        public void Write (char charValue) {
            m_writer.Write(charValue);
        }


        public void Write (char[] chars) {
            m_writer.Write(chars.Length);
            foreach (var charValue in chars)
                m_writer.Write(charValue);
        }


        public void Write (string stringValue) {
            m_writer.Write(stringValue);
        }


        public void Write (string[] strings) {
            m_writer.Write(strings.Length);
            foreach (var stringValue in strings)
                m_writer.Write(stringValue);
        }


        public void Write (float floatValue) {
            m_writer.Write(floatValue);
        }


        public void Write (float[] floats) {
            m_writer.Write(floats.Length);
            foreach (var floatValue in floats)
                m_writer.Write(floatValue);
        }


        public void Write (double doubleValue) {
            m_writer.Write(doubleValue);
        }


        public void Write (double[] doubles) {
            m_writer.Write(doubles.Length);
            foreach (var doubleValue in doubles)
                m_writer.Write(doubleValue);
        }


        public void Write (bool boolValue) {
            m_writer.Write(boolValue);
        }


        public void Dispose () {
            m_writer?.Dispose();
        }


        public ValueTask DisposeAsync () {
            return m_writer.DisposeAsync();
        }

    }

}