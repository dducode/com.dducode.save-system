using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem {

    /// <summary>
    /// Adapter to class <see cref="BinaryWriter"/> for simplify writing data
    /// </summary>
    public class UnityWriter : IDisposable {

        private readonly BinaryWriter m_writer;
        public readonly string localPath;


        public UnityWriter (BinaryWriter writer, string localPath) {
            m_writer = writer;
            this.localPath = localPath;
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


        public async Task WriteAsync (Vector2[] vector2Array) {
            await Task.Run(() => { Write(vector2Array); });
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


        public async Task WriteAsync (Vector3[] vector3Array) {
            await Task.Run(() => { Write(vector3Array); });
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


        public async Task WriteAsync (Vector4[] vector4Array) {
            await Task.Run(() => { Write(vector4Array); });
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


        public async Task WriteAsync (Color[] colors) {
            await Task.Run(() => { Write(colors); });
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


        public async Task WriteAsync (Color32[] colors32) {
            await Task.Run(() => { Write(colors32); });
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


        public async Task WriteAsync (Matrix4x4[] matrices) {
            await Task.Run(() => { Write(matrices); });
        }



        #region WritingMeshes

        public void Write (Mesh mesh) {
            var subMeshes = new SubMeshDescriptor[mesh.subMeshCount];
            for (var i = 0; i < subMeshes.Length; i++)
                subMeshes[i] = mesh.GetSubMesh(i);

            m_writer.Write(subMeshes.Length);

            foreach (var subMesh in subMeshes) {
                m_writer.Write(subMesh.baseVertex);
                Write(subMesh.bounds.center);
                Write(subMesh.bounds.extents);
                Write(subMesh.bounds.max);
                Write(subMesh.bounds.min);
                Write(subMesh.bounds.size);
                m_writer.Write(subMesh.firstVertex);
                m_writer.Write(subMesh.indexCount);
                m_writer.Write(subMesh.indexStart);
                m_writer.Write((int) subMesh.topology);
                m_writer.Write(subMesh.vertexCount);
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
            Write(mesh.bounds.center);
            Write(mesh.bounds.extents);
            Write(mesh.bounds.max);
            Write(mesh.bounds.min);
            Write(mesh.bounds.size);
            Write(mesh.colors32);
            m_writer.Write((int) mesh.indexBufferTarget);
            m_writer.Write((int) mesh.indexFormat);
            m_writer.Write((int) mesh.vertexBufferTarget);
            Write(mesh.normals);
            Write(mesh.tangents);
            Write(mesh.triangles);
        }


        public async Task WriteAsync (Mesh mesh) {
            var subMeshes = new SubMeshDescriptor[mesh.subMeshCount];
            for (var i = 0; i < subMeshes.Length; i++)
                subMeshes[i] = mesh.GetSubMesh(i);

            await Task.Run(() => {
                m_writer.Write(subMeshes.Length);

                foreach (var subMesh in subMeshes) {
                    m_writer.Write(subMesh.baseVertex);
                    Write(subMesh.bounds.center);
                    Write(subMesh.bounds.extents);
                    Write(subMesh.bounds.max);
                    Write(subMesh.bounds.min);
                    Write(subMesh.bounds.size);
                    m_writer.Write(subMesh.firstVertex);
                    m_writer.Write(subMesh.indexCount);
                    m_writer.Write(subMesh.indexStart);
                    m_writer.Write((int) subMesh.topology);
                    m_writer.Write(subMesh.vertexCount);
                }
            });

            m_writer.Write(mesh.name);
            await WriteAsync(mesh.vertices);
            await WriteAsync(mesh.uv);
            await WriteAsync(mesh.uv2);
            await WriteAsync(mesh.uv3);
            await WriteAsync(mesh.uv4);
            await WriteAsync(mesh.uv5);
            await WriteAsync(mesh.uv6);
            await WriteAsync(mesh.uv7);
            await WriteAsync(mesh.uv8);
            Write(mesh.bounds.center);
            Write(mesh.bounds.extents);
            Write(mesh.bounds.max);
            Write(mesh.bounds.min);
            Write(mesh.bounds.size);
            await WriteAsync(mesh.colors32);
            m_writer.Write((int) mesh.indexBufferTarget);
            m_writer.Write((int) mesh.indexFormat);
            m_writer.Write((int) mesh.vertexBufferTarget);
            await WriteAsync(mesh.normals);
            await WriteAsync(mesh.tangents);
            Write(mesh.triangles);
        }


        public void Write (Mesh[] meshes) {
            m_writer.Write(meshes.Length);
            foreach (var mesh in meshes)
                Write(mesh);
        }


        public async Task WriteAsync (Mesh[] meshes) {
            m_writer.Write(meshes.Length);
            foreach (var mesh in meshes)
                await WriteAsync(mesh);
        }

        #endregion



        #region WritingObjects

        public void Write<T> (T obj) {
            m_writer.Write(JsonUtility.ToJson(obj));
        }


        public void Write<T> (List<T> listObjects) {
            m_writer.Write(listObjects.Count);
            foreach (var obj in listObjects)
                Write(obj);
        }


        public async Task WriteAsync<T> (List<T> listObjects) {
            await Task.Run(() => { Write(listObjects); });
        }


        public void Write<T> (T[] arrayObjects) {
            m_writer.Write(arrayObjects.Length);
            foreach (var obj in arrayObjects)
                Write(obj);
        }


        public async Task WriteAsync<T> (T[] arrayObjects) {
            await Task.Run(() => { Write(arrayObjects); });
        }

        #endregion



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

    }

}