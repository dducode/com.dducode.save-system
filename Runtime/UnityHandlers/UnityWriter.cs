using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem.UnityHandlers {

    /// <summary>
    /// Adapter to class <see cref="BinaryWriter"></see> for simplify writing data
    /// </summary>
    public sealed class UnityWriter : IDisposable, IAsyncDisposable {

        private readonly BinaryWriter m_writer;
        public readonly string localPath;


        internal UnityWriter (BinaryWriter writer, string localPath = "") {
            m_writer = writer;
            this.localPath = localPath;
        }


        public void Write (Version version) {
            m_writer.Write(version.Major);
            m_writer.Write(version.Minor);
            m_writer.Write(version.Build);
            m_writer.Write(version.Revision);
            m_writer.Close();
        }


        public void Write (Vector2 vector2) {
            m_writer.Write(vector2.x);
            m_writer.Write(vector2.y);
        }


        public void Write (Vector2[] vector2Array) {
            m_writer.Write(vector2Array.Length);
            foreach (Vector2 vector2 in vector2Array)
                Write(vector2);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<Vector2> vector2Collection) {
            Write(vector2Collection.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<Vector2> vector2Collection) {
            await UniTask.RunOnThreadPool(() => Write(vector2Collection.ToArray()));
        }


        public void Write (Vector3 vector3) {
            m_writer.Write(vector3.x);
            m_writer.Write(vector3.y);
            m_writer.Write(vector3.z);
        }


        public void Write (Vector3[] vector3Array) {
            m_writer.Write(vector3Array.Length);
            foreach (Vector3 vector3 in vector3Array)
                Write(vector3);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<Vector3> vector3Collection) {
            Write(vector3Collection.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<Vector3> vector3Collection) {
            await UniTask.RunOnThreadPool(() => Write(vector3Collection.ToArray()));
        }


        public void Write (Vector4 vector4) {
            m_writer.Write(vector4.x);
            m_writer.Write(vector4.y);
            m_writer.Write(vector4.z);
            m_writer.Write(vector4.w);
        }


        public void Write (Vector4[] vector4Array) {
            m_writer.Write(vector4Array.Length);
            foreach (Vector4 vector4 in vector4Array)
                Write(vector4);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<Vector4> vector4Collection) {
            Write(vector4Collection.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<Vector4> vector4Collection) {
            await UniTask.RunOnThreadPool(() => Write(vector4Collection.ToArray()));
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
            foreach (Color color in colors)
                Write(color);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<Color> colors) {
            Write(colors.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<Color> colors) {
            await UniTask.RunOnThreadPool(() => Write(colors.ToArray()));
        }


        public void Write (Color32 color32) {
            m_writer.Write(color32.r);
            m_writer.Write(color32.g);
            m_writer.Write(color32.b);
            m_writer.Write(color32.a);
        }


        public void Write (Color32[] colors32) {
            m_writer.Write(colors32.Length);
            foreach (Color32 color32 in colors32)
                Write(color32);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<Color32> colors32) {
            Write(colors32.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<Color32> colors32) {
            await UniTask.RunOnThreadPool(() => Write(colors32.ToArray()));
        }


        public void Write (Matrix4x4 matrix) {
            for (var i = 0; i < 16; i++)
                m_writer.Write(matrix[i]);
        }


        public void Write (Matrix4x4[] matrices) {
            m_writer.Write(matrices.Length);
            foreach (Matrix4x4 matrix in matrices)
                Write(matrix);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<Matrix4x4> matrices) {
            Write(matrices.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<Matrix4x4> matrices) {
            await UniTask.RunOnThreadPool(() => Write(matrices.ToArray()));
        }


        public void Write (MeshData mesh) {
            m_writer.Write(mesh.subMeshes.Length);

            for (var i = 0; i < mesh.subMeshes.Length; i++) {
                SubMeshDescriptor subMesh = mesh.subMeshes[i];
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
                m_writer.Write((int)subMesh.topology);
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
            m_writer.Write((byte)mesh.indexBufferTarget);
            m_writer.Write((byte)mesh.indexFormat);
            m_writer.Write((byte)mesh.vertexBufferTarget);
        }


        public async UniTask WriteAsync (MeshData mesh) {
            await UniTask.RunOnThreadPool(() => Write(mesh));
        }


        public void WriteObject<T> (T obj) {
            m_writer.Write(JsonUtility.ToJson(obj));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteObjects<T> (IEnumerable<T> objects) {
            WriteObjectsArray(objects.ToArray());
        }


        public async UniTask WriteObjectsAsync<T> (IEnumerable<T> objects) {
            await UniTask.RunOnThreadPool(() => WriteObjectsArray(objects.ToArray()));
        }


        public void WriteObjectsArray<T> (T[] arrayObjects) {
            m_writer.Write(arrayObjects.Length);
            foreach (T obj in arrayObjects)
                WriteObject(obj);
        }


        public void Write (byte byteValue) {
            m_writer.Write(byteValue);
        }


        public void Write (byte[] bytes) {
            m_writer.Write(bytes.Length);
            foreach (byte byteValue in bytes)
                m_writer.Write(byteValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<byte> bytes) {
            Write(bytes.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<byte> bytes) {
            await UniTask.RunOnThreadPool(() => Write(bytes.ToArray()));
        }


        public void Write (short shortValue) {
            m_writer.Write(shortValue);
        }


        public void Write (short[] shorts) {
            m_writer.Write(shorts.Length);
            foreach (short shortValue in shorts)
                m_writer.Write(shortValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<short> shorts) {
            Write(shorts.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<short> shorts) {
            await UniTask.RunOnThreadPool(() => Write(shorts.ToArray()));
        }


        public void Write (int intValue) {
            m_writer.Write(intValue);
        }


        public void Write (int[] ints) {
            m_writer.Write(ints.Length);
            foreach (int intValue in ints)
                m_writer.Write(intValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<int> ints) {
            Write(ints.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<int> ints) {
            await UniTask.RunOnThreadPool(() => Write(ints.ToArray()));
        }


        public void Write (long longValue) {
            m_writer.Write(longValue);
        }


        public void Write (long[] longs) {
            m_writer.Write(longs.Length);
            foreach (long longValue in longs)
                m_writer.Write(longValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<long> longs) {
            Write(longs.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<long> longs) {
            await UniTask.RunOnThreadPool(() => Write(longs.ToArray()));
        }


        public void Write (char charValue) {
            m_writer.Write(charValue);
        }


        public void Write (char[] chars) {
            m_writer.Write(chars.Length);
            foreach (char charValue in chars)
                m_writer.Write(charValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<char> chars) {
            Write(chars.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<char> chars) {
            await UniTask.RunOnThreadPool(() => Write(chars.ToArray()));
        }


        public void Write (string stringValue) {
            m_writer.Write(stringValue);
        }


        public void Write (string[] strings) {
            m_writer.Write(strings.Length);
            foreach (string stringValue in strings)
                m_writer.Write(stringValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<string> strings) {
            Write(strings.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<string> strings) {
            await UniTask.RunOnThreadPool(() => Write(strings.ToArray()));
        }


        public void Write (float floatValue) {
            m_writer.Write(floatValue);
        }


        public void Write (float[] floats) {
            m_writer.Write(floats.Length);
            foreach (float floatValue in floats)
                m_writer.Write(floatValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<float> floats) {
            Write(floats.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<float> floats) {
            await UniTask.RunOnThreadPool(() => Write(floats.ToArray()));
        }


        public void Write (double doubleValue) {
            m_writer.Write(doubleValue);
        }


        public void Write (double[] doubles) {
            m_writer.Write(doubles.Length);
            foreach (double doubleValue in doubles)
                m_writer.Write(doubleValue);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write (IEnumerable<double> doubles) {
            Write(doubles.ToArray());
        }


        public async UniTask WriteAsync (IEnumerable<double> doubles) {
            await UniTask.RunOnThreadPool(() => Write(doubles.ToArray()));
        }


        public void Write (bool boolValue) {
            m_writer.Write(boolValue);
        }


        internal byte[] GetMemoryData () {
            return ((MemoryStream)m_writer.BaseStream).GetBuffer();
        }


        internal void WriteBufferToFile () {
            File.WriteAllBytes(localPath, ((MemoryStream)m_writer.BaseStream).GetBuffer());
        }


        internal async UniTask WriteBufferToFileAsync () {
            await File.WriteAllBytesAsync(localPath, ((MemoryStream)m_writer.BaseStream).GetBuffer());
        }


        public void Dispose () {
            m_writer?.Dispose();
        }


        public ValueTask DisposeAsync () {
            return m_writer.DisposeAsync();
        }

    }

}