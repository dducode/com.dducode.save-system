using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem.UnityHandlers {

    /// <summary>
    /// Adapter to class <see cref="BinaryWriter"></see> for simplify writing data async
    /// </summary>
    public sealed class UnityAsyncWriter : IUnityAsyncHandler, IAsyncDisposable {

        private readonly BinaryWriter m_writer;
        public readonly string localPath;


        private UnityAsyncWriter (BinaryWriter writer, string localPath) {
            m_writer = writer;
            this.localPath = localPath;
        }


        internal static UnityAsyncWriter GetLocal (string fileName) {
            return new UnityAsyncWriter(
                BinaryHandlers.GetBinaryWriter(fileName, out string persistentPath), persistentPath
            );
        }


        internal static UnityAsyncWriter GetRemote () {
            return new UnityAsyncWriter(
                BinaryHandlers.GetBinaryWriterRemote(out string tempPath), tempPath
            );
        }


        public void Write (Version version) {
            m_writer.Write(version.Major);
            m_writer.Write(version.Minor);
            m_writer.Write(version.Build);
            m_writer.Write(version.Revision);
        }


        public async UniTask Write (Vector2[] vector2Array) {
            m_writer.Write(vector2Array.Length);

            foreach (Vector2 vector2 in vector2Array)
                Write(vector2);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<Vector2> vector2Collection) {
            await Write(vector2Collection.ToArray());
        }


        public async UniTask Write (Vector3[] vector3Array) {
            m_writer.Write(vector3Array.Length);

            foreach (Vector3 vector3 in vector3Array)
                Write(vector3);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<Vector3> vector3Collection) {
            await Write(vector3Collection.ToArray());
        }


        public async UniTask Write (Vector4[] vector4Array) {
            m_writer.Write(vector4Array.Length);

            foreach (Vector4 vector4 in vector4Array)
                Write(vector4);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<Vector4> vector4Collection) {
            await Write(vector4Collection.ToArray());
        }


        public async UniTask Write (Color[] colors) {
            m_writer.Write(colors.Length);

            foreach (Color color in colors)
                Write(color);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<Color> colors) {
            await Write(colors.ToArray());
        }


        public async UniTask Write (Color32[] colors32) {
            m_writer.Write(colors32.Length);

            foreach (Color32 color32 in colors32)
                Write(color32);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<Color32> colors32) {
            await Write(colors32.ToArray());
        }


        public async UniTask Write (Matrix4x4[] matrices) {
            m_writer.Write(matrices.Length);
            foreach (Matrix4x4 matrix in matrices)
                Write(matrix);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<Matrix4x4> matrices) {
            await Write(matrices.ToArray());
        }


        public async UniTask Write (MeshData meshData, int uvChannels = 1) {
            m_writer.Write(meshData.subMeshes.Length);

            for (var i = 0; i < meshData.subMeshes.Length; i++) {
                SubMeshDescriptor subMesh = meshData.subMeshes[i];
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
                m_writer.Write((byte)subMesh.topology);
                await Write(meshData.subMeshIndices[i]);
            }

            m_writer.Write(meshData.name);
            await Write(meshData.vertices);
            await Write(meshData.uv);
            await Write(meshData.colors32);
            await Write(meshData.normals);
            await Write(meshData.tangents);
            await Write(meshData.triangles);
            Write(meshData.bounds.center);
            Write(meshData.bounds.extents);
            Write(meshData.bounds.max);
            Write(meshData.bounds.min);
            Write(meshData.bounds.size);
            m_writer.Write((byte)meshData.indexBufferTarget);
            m_writer.Write((byte)meshData.indexFormat);
            m_writer.Write((byte)meshData.vertexBufferTarget);

            if (uvChannels >= 2)
                await Write(meshData.uv2);
            if (uvChannels >= 3)
                await Write(meshData.uv3);
            if (uvChannels >= 4)
                await Write(meshData.uv4);
            if (uvChannels >= 5)
                await Write(meshData.uv5);
            if (uvChannels >= 6)
                await Write(meshData.uv6);
            if (uvChannels >= 7)
                await Write(meshData.uv7);
            if (uvChannels == 8)
                await Write(meshData.uv8);
        }


        public async UniTask WriteObjectsArray<T> (T[] objects) {
            m_writer.Write(objects.Length);

            foreach (T obj in objects) {
                m_writer.Write(JsonUtility.ToJson(obj));
                await UniTask.NextFrame();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask WriteObjects<T> (IEnumerable<T> objects) {
            await WriteObjectsArray(objects.ToArray());
        }


        public async UniTask Write (int[] intValues) {
            m_writer.Write(intValues.Length);

            foreach (int intValue in intValues)
                m_writer.Write(intValue);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<int> ints) {
            await Write(ints.ToArray());
        }


        public async UniTask Write (byte[] bytes) {
            m_writer.Write(bytes.Length);

            foreach (byte byteValue in bytes)
                m_writer.Write(byteValue);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<byte> bytes) {
            await Write(bytes.ToArray());
        }


        public async UniTask Write (float[] floatValues) {
            m_writer.Write(floatValues.Length);

            foreach (float floatValue in floatValues)
                m_writer.Write(floatValue);

            await UniTask.NextFrame();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async UniTask Write (IEnumerable<float> floatValues) {
            await Write(floatValues.ToArray());
        }


        public void Dispose () {
            m_writer?.Dispose();
        }


        public ValueTask DisposeAsync () {
            return m_writer.DisposeAsync();
        }


        private void Write (Vector2 vector2) {
            m_writer.Write(vector2.x);
            m_writer.Write(vector2.y);
        }


        private void Write (Vector3 vector3) {
            m_writer.Write(vector3.x);
            m_writer.Write(vector3.y);
            m_writer.Write(vector3.z);
        }


        private void Write (Vector4 vector4) {
            m_writer.Write(vector4.x);
            m_writer.Write(vector4.y);
            m_writer.Write(vector4.z);
            m_writer.Write(vector4.w);
        }


        private void Write (Color color) {
            m_writer.Write(color.r);
            m_writer.Write(color.g);
            m_writer.Write(color.b);
            m_writer.Write(color.a);
        }


        private void Write (Color32 color32) {
            m_writer.Write(color32.r);
            m_writer.Write(color32.g);
            m_writer.Write(color32.b);
            m_writer.Write(color32.a);
        }


        private void Write (Matrix4x4 matrix) {
            for (var i = 0; i < 16; i++)
                m_writer.Write(matrix[i]);
        }

    }

}