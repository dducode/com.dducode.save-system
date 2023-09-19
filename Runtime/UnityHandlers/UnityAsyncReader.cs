using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Cysharp.Threading.Tasks;
using SaveSystem.InternalServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem.UnityHandlers {

    /// <summary>
    /// Adapter to class <see cref="BinaryReader"></see> for simplify reading data async
    /// </summary>
    public sealed class UnityAsyncReader : IUnityAsyncHandler {

        private readonly BinaryReader m_reader;


        private UnityAsyncReader (BinaryReader reader) {
            m_reader = reader;
        }


        [return: MaybeNull]
        internal static UnityAsyncReader GetLocal (string fileName) {
            BinaryReader binaryReader = BinaryHandlers.GetBinaryReader(fileName);
            return binaryReader != null ? new UnityAsyncReader(binaryReader) : null;
        }


        internal static async UniTask<UnityAsyncReader> GetRemote (string url) {
            BinaryReader binaryReader = await BinaryHandlers.GetBinaryReaderRemote(url);
            return binaryReader != null ? new UnityAsyncReader(binaryReader) : null;
        }


        public Version ReadVersion () {
            int major = m_reader.ReadInt32();
            int minor = m_reader.ReadInt32();
            int build = m_reader.ReadInt32();
            int revision = m_reader.ReadInt32();

            if (build == -1)
                return new Version(major, minor);
            else if (revision == -1)
                return new Version(major, minor, build);
            else
                return new Version(major, minor, build, revision);
        }


        public async UniTask<Vector2[]> ReadVector2Array () {
            int length = m_reader.ReadInt32();
            var vector2Array = new Vector2[length];

            for (var i = 0; i < length; i++)
                vector2Array[i] = ReadVector2();

            await UniTask.NextFrame();

            return vector2Array;
        }


        public async UniTask<Vector3[]> ReadVector3Array () {
            int length = m_reader.ReadInt32();
            var vector3Array = new Vector3[length];

            for (var i = 0; i < length; i++)
                vector3Array[i] = ReadVector3();

            await UniTask.NextFrame();

            return vector3Array;
        }


        public async UniTask<Vector4[]> ReadVector4Array () {
            int length = m_reader.ReadInt32();
            var vector4Array = new Vector4[length];

            for (var i = 0; i < length; i++)
                vector4Array[i] = ReadVector4();

            await UniTask.NextFrame();

            return vector4Array;
        }


        public async UniTask<Color[]> ReadColors () {
            int length = m_reader.ReadInt32();
            var colors = new Color[length];

            for (var i = 0; i < length; i++)
                colors[i] = ReadColor();

            await UniTask.NextFrame();

            return colors;
        }


        public async UniTask<Color32[]> ReadColors32 () {
            int length = m_reader.ReadInt32();
            var colors32 = new Color32[length];

            for (var i = 0; i < length; i++)
                colors32[i] = ReadColor32();

            await UniTask.NextFrame();

            return colors32;
        }


        public async UniTask<Matrix4x4[]> ReadMatrices () {
            int length = m_reader.ReadInt32();
            var matrices = new Matrix4x4[length];

            for (var i = 0; i < length; i++)
                matrices[i] = ReadMatrix();

            await UniTask.NextFrame();

            return matrices;
        }


        public async UniTask<MeshData> ReadMesh (int uvChannels = 1) {
            int subMeshesCount = m_reader.ReadInt32();
            var subMeshes = new SubMeshDescriptor[subMeshesCount];
            var subMeshIndices = new int[subMeshesCount][];

            for (var i = 0; i < subMeshesCount; i++) {
                subMeshes[i] = new SubMeshDescriptor {
                    baseVertex = m_reader.ReadInt32(),
                    bounds = new Bounds {
                        center = ReadVector3(),
                        extents = ReadVector3(),
                        max = ReadVector3(),
                        min = ReadVector3(),
                        size = ReadVector3()
                    },
                    firstVertex = m_reader.ReadInt32(),
                    indexCount = m_reader.ReadInt32(),
                    indexStart = m_reader.ReadInt32(),
                    vertexCount = m_reader.ReadInt32(),
                    topology = (MeshTopology)m_reader.ReadByte()
                };
                subMeshIndices[i] = await ReadIntArray();
            }

            var mesh = new MeshData {
                subMeshes = subMeshes,
                subMeshIndices = subMeshIndices,
                name = m_reader.ReadString(),
                vertices = await ReadVector3Array(),
                uv = await ReadVector2Array(),
                colors32 = await ReadColors32(),
                normals = await ReadVector3Array(),
                tangents = await ReadVector4Array(),
                triangles = await ReadIntArray(),
                bounds = new Bounds {
                    center = ReadVector3(),
                    extents = ReadVector3(),
                    max = ReadVector3(),
                    min = ReadVector3(),
                    size = ReadVector3()
                },
                indexBufferTarget = (GraphicsBuffer.Target)m_reader.ReadByte(),
                indexFormat = (IndexFormat)m_reader.ReadByte(),
                vertexBufferTarget = (GraphicsBuffer.Target)m_reader.ReadByte()
            };

            if (uvChannels >= 2)
                mesh.uv2 = await ReadVector2Array();
            if (uvChannels >= 3)
                mesh.uv3 = await ReadVector2Array();
            if (uvChannels >= 4)
                mesh.uv4 = await ReadVector2Array();
            if (uvChannels >= 5)
                mesh.uv5 = await ReadVector2Array();
            if (uvChannels >= 6)
                mesh.uv6 = await ReadVector2Array();
            if (uvChannels >= 7)
                mesh.uv7 = await ReadVector2Array();
            if (uvChannels == 8)
                mesh.uv8 = await ReadVector2Array();

            return mesh;
        }


        public async UniTask<T[]> ReadObjects<T> () {
            int length = m_reader.ReadInt32();
            var objects = new T[length];

            for (var i = 0; i < length; i++)
                objects[i] = JsonUtility.FromJson<T>(m_reader.ReadString());

            await UniTask.NextFrame();

            return objects;
        }


        public async UniTask<int[]> ReadIntArray () {
            int length = m_reader.ReadInt32();
            var intArray = new int[length];

            for (var i = 0; i < length; i++)
                intArray[i] = m_reader.ReadInt32();

            await UniTask.NextFrame();

            return intArray;
        }


        public async UniTask<byte[]> ReadBytes () {
            int length = m_reader.ReadInt32();
            var bytes = new byte[length];

            for (var i = 0; i < length; i++)
                bytes[i] = m_reader.ReadByte();

            await UniTask.NextFrame();

            return bytes;
        }


        public async UniTask<float[]> ReadFloatArray () {
            int length = m_reader.ReadInt32();
            var floatValues = new float[length];

            for (var i = 0; i < length; i++)
                floatValues[i] = m_reader.ReadSingle();

            await UniTask.NextFrame();

            return floatValues;
        }


        public void Dispose () {
            m_reader?.Dispose();
        }


        private Vector2 ReadVector2 () {
            return new Vector2 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle()
            };
        }


        private Vector3 ReadVector3 () {
            return new Vector3 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle()
            };
        }


        private Vector4 ReadVector4 () {
            return new Vector4 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle(),
                w = m_reader.ReadSingle()
            };
        }


        private Color ReadColor () {
            return new Color {
                r = m_reader.ReadSingle(),
                g = m_reader.ReadSingle(),
                b = m_reader.ReadSingle(),
                a = m_reader.ReadSingle()
            };
        }


        private Color32 ReadColor32 () {
            return new Color32 {
                r = m_reader.ReadByte(),
                g = m_reader.ReadByte(),
                b = m_reader.ReadByte(),
                a = m_reader.ReadByte()
            };
        }


        private Matrix4x4 ReadMatrix () {
            var matrix = new Matrix4x4();
            for (var i = 0; i < 16; i++)
                matrix[i] = m_reader.ReadSingle();

            return matrix;
        }

    }

}