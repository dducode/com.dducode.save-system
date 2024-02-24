using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.Rendering;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskAlias = Cysharp.Threading.Tasks.UniTask;

#else
using TaskAlias = System.Threading.Tasks.Task;
#endif

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SaveSystem.BinaryHandlers {

    public class BinaryWriter : IDisposable {

        private readonly Stream m_input;


        public BinaryWriter (Stream input) {
            m_input = input;
        }


        public void Write<T> (T value) where T : unmanaged {
            ReadOnlySpan<T> tSpan = MemoryMarshal.CreateReadOnlySpan(ref value, 1);
            m_input.Write(MemoryMarshal.AsBytes(tSpan));
        }


        public void Write<T> ([NotNull] T[] array) where T : unmanaged {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Length > 0)
                Write(MemoryMarshal.CreateReadOnlySpan(ref array[0], array.Length));
            else
                Write(0);
        }


        public void Write<T> (ReadOnlySpan<T> span) where T : unmanaged {
            Write(span.Length);
            m_input.Write(MemoryMarshal.AsBytes(span));
        }


        public void Write (Version version) {
            Write(version.Major);
            Write(version.Minor);
            Write(version.Build);
            Write(version.Revision);
        }


        public void Write (MeshData mesh) {
            Write(mesh.subMeshes.Length);

            for (var i = 0; i < mesh.subMeshes.Length; i++) {
                SubMeshDescriptor subMesh = mesh.subMeshes[i];
                Write(subMesh.baseVertex);
                Write(subMesh.bounds.center);
                Write(subMesh.bounds.extents);
                Write(subMesh.bounds.max);
                Write(subMesh.bounds.min);
                Write(subMesh.bounds.size);
                Write(subMesh.firstVertex);
                Write(subMesh.indexCount);
                Write(subMesh.indexStart);
                Write(subMesh.vertexCount);
                Write(subMesh.topology);
                Write(mesh.subMeshIndices[i]);
            }

            Write(mesh.name);
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
            Write(mesh.indexBufferTarget);
            Write(mesh.indexFormat);
            Write(mesh.vertexBufferTarget);
        }


        public void Write ([NotNull] MeshData[] array) {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Write(array.Length);
            foreach (MeshData meshData in array)
                Write(meshData);
        }


        public void Write (ReadOnlySpan<MeshData> span) {
            Write(span.Length);
            foreach (MeshData meshData in span)
                Write(meshData);
        }


        public void Write (string value) {
            Write((ReadOnlySpan<char>)value);
        }


        public void Write ([NotNull] string[] array) {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            Write(array.Length);
            foreach (string value in array)
                Write(value);
        }


        public void Write (DataBuffer dataBuffer) {
            dataBuffer.WriteData(this);
        }


        public void Dispose () {
            m_input.SetLength(m_input.Position);
            m_input.Dispose();
        }


        internal async TaskAlias WriteDataToFileAsync (string path, CancellationToken token) {
            await File.WriteAllBytesAsync(path, ((MemoryStream)m_input).ToArray(), token);
        }

    }

}