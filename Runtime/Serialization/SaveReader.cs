using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SaveSystemPackage.Security;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage.Serialization {

    public class SaveReader : IDisposable, IAsyncDisposable {

        public readonly Stream output;
        private long m_streamPosition;


        public SaveReader (Stream output) {
            this.output = output;
        }


        public bool IsEndOfStream () {
            return m_streamPosition >= output.Length;
        }


        public TValue Read<TValue> () where TValue : unmanaged {
            var value = default(TValue);
            Span<TValue> span = MemoryMarshal.CreateSpan(ref value, 1);
            m_streamPosition += output.Read(MemoryMarshal.AsBytes(span));
            return value;
        }


        public TValue[] ReadArray<TValue> () where TValue : unmanaged {
            var array = new TValue[Read<int>()];
            m_streamPosition += output.Read(MemoryMarshal.AsBytes((Span<TValue>)array));
            return array;
        }


        public Version ReadVersion () {
            var major = Read<int>();
            var minor = Read<int>();
            var build = Read<int>();
            var revision = Read<int>();

            if (build == -1)
                return new Version(major, minor);
            else if (revision == -1)
                return new Version(major, minor, build);
            else
                return new Version(major, minor, build, revision);
        }


        public MeshData ReadMeshData () {
            var subMeshesCount = Read<int>();
            var subMeshes = new SubMeshDescriptor[subMeshesCount];
            var subMeshIndices = new int[subMeshesCount][];

            for (var i = 0; i < subMeshesCount; i++) {
                subMeshes[i] = new SubMeshDescriptor {
                    baseVertex = Read<int>(),
                    bounds = new Bounds {
                        center = Read<Vector3>(),
                        extents = Read<Vector3>(),
                        max = Read<Vector3>(),
                        min = Read<Vector3>(),
                        size = Read<Vector3>()
                    },
                    firstVertex = Read<int>(),
                    indexCount = Read<int>(),
                    indexStart = Read<int>(),
                    vertexCount = Read<int>(),
                    topology = Read<MeshTopology>()
                };
                subMeshIndices[i] = ReadArray<int>();
            }

            return new MeshData {
                subMeshes = subMeshes,
                subMeshIndices = subMeshIndices,
                name = ReadString(),
                vertices = ReadArray<Vector3>(),
                uv = ReadArray<Vector2>(),
                uv2 = ReadArray<Vector2>(),
                uv3 = ReadArray<Vector2>(),
                uv4 = ReadArray<Vector2>(),
                uv5 = ReadArray<Vector2>(),
                uv6 = ReadArray<Vector2>(),
                uv7 = ReadArray<Vector2>(),
                uv8 = ReadArray<Vector2>(),
                colors32 = ReadArray<Color32>(),
                normals = ReadArray<Vector3>(),
                tangents = ReadArray<Vector4>(),
                triangles = ReadArray<int>(),
                bounds = new Bounds {
                    center = Read<Vector3>(),
                    extents = Read<Vector3>(),
                    max = Read<Vector3>(),
                    min = Read<Vector3>(),
                    size = Read<Vector3>()
                },
                indexBufferTarget = Read<GraphicsBuffer.Target>(),
                indexFormat = Read<IndexFormat>(),
                vertexBufferTarget = Read<GraphicsBuffer.Target>()
            };
        }


        public MeshData[] ReadMeshDataArray () {
            var length = Read<int>();
            var array = new MeshData[length];

            for (var i = 0; i < length; i++)
                array[i] = ReadMeshData();

            return array;
        }


        public string ReadString () {
            char[] array = ReadArray<char>();
            return string.Create(array.Length, array, (span, chars) => {
                for (var i = 0; i < span.Length; i++)
                    span[i] = chars[i];
            });
        }


        public string[] ReadStringArray () {
            var length = Read<int>();
            var array = new string[length];

            for (var i = 0; i < length; i++)
                array[i] = ReadString();

            return array;
        }


        public DataBuffer ReadDataBuffer () {
            return new DataBuffer(this);
        }


        public SecureDataBuffer ReadSecureDataBuffer () {
            return new SecureDataBuffer(this);
        }


        public void Dispose () {
            output?.Dispose();
        }


        public async ValueTask DisposeAsync () {
            if (output != null)
                await output.DisposeAsync();
        }


        internal object ReadObject (Type type) {
            var size = Read<int>();
            var bytes = new byte[size];
            m_streamPosition += output.Read(bytes);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            return Marshal.PtrToStructure(ptr, type);
        }

    }

}