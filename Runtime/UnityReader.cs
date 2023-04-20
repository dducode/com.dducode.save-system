using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem {

    /// <summary>
    /// Adapter to class <see cref="BinaryReader"/> for simplify reading data
    /// </summary>
    public sealed class UnityReader : IDisposable {

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


        public Vector2[] ReadVector2Array () {
            var length = m_reader.ReadInt32();
            var vector2Array = new Vector2[length];

            for (var i = 0; i < length; i++)
                vector2Array[i] = ReadVector2();

            return vector2Array;
        }


        public async UniTask<Vector2[]> ReadVector2ArrayAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadVector2Array);

            await UniTask.NextFrame();
            return ReadVector2Array();
        }


        public Vector3 ReadVector3 () {
            return new Vector3 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle()
            };
        }


        public Vector3[] ReadVector3Array () {
            var length = m_reader.ReadInt32();
            var vector3Array = new Vector3[length];

            for (var i = 0; i < length; i++)
                vector3Array[i] = ReadVector3();

            return vector3Array;
        }


        public async UniTask<Vector3[]> ReadVector3ArrayAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadVector3Array);

            await UniTask.NextFrame();
            return ReadVector3Array();
        }


        public Vector4 ReadVector4 () {
            return new Vector4 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle(),
                w = m_reader.ReadSingle()
            };
        }


        public Vector4[] ReadVector4Array () {
            var length = m_reader.ReadInt32();
            var vector4Array = new Vector4[length];

            for (var i = 0; i < length; i++)
                vector4Array[i] = ReadVector4();

            return vector4Array;
        }


        public async UniTask<Vector4[]> ReadVector4ArrayAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadVector4Array);

            await UniTask.NextFrame();
            return ReadVector4Array();
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


        public Color[] ReadColors () {
            var length = m_reader.ReadInt32();
            var colors = new Color[length];

            for (var i = 0; i < length; i++)
                colors[i] = ReadColor();

            return colors;
        }


        public async UniTask<Color[]> ReadColorsAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadColors);

            await UniTask.NextFrame();
            return ReadColors();
        }


        public Color32 ReadColor32 () {
            return new Color32 {
                r = m_reader.ReadByte(),
                g = m_reader.ReadByte(),
                b = m_reader.ReadByte(),
                a = m_reader.ReadByte()
            };
        }


        public Color32[] ReadColors32 () {
            var length = m_reader.ReadInt32();
            var colors32 = new Color32[length];

            for (var i = 0; i < length; i++)
                colors32[i] = ReadColor32();

            return colors32;
        }


        public async UniTask<Color32[]> ReadColors32Async (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadColors32);

            await UniTask.NextFrame();
            return ReadColors32();
        }


        public Matrix4x4 ReadMatrix () {
            var matrix = new Matrix4x4();
            for (var i = 0; i < 16; i++)
                matrix[i] = m_reader.ReadSingle();

            return matrix;
        }


        public Matrix4x4[] ReadMatrices () {
            var length = m_reader.ReadInt32();
            var matrices = new Matrix4x4[length];

            for (var i = 0; i < length; i++)
                matrices[i] = ReadMatrix();

            return matrices;
        }


        public async UniTask<Matrix4x4[]> ReadMatricesAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadMatrices);

            await UniTask.NextFrame();
            return ReadMatrices();
        }



        #region ReadingMeshes

        public Mesh ReadMesh () {
            var subMeshesCount = m_reader.ReadInt32();
            var subMeshes = new SubMeshDescriptor[subMeshesCount];

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
                    topology = (MeshTopology) m_reader.ReadInt32(),
                    vertexCount = m_reader.ReadInt32()
                };
            }

            var mesh = new Mesh {
                name = m_reader.ReadString(),
                vertices = ReadVector3Array(),
                uv = ReadVector2Array(),
                uv2 = ReadVector2Array(),
                uv3 = ReadVector2Array(),
                uv4 = ReadVector2Array(),
                uv5 = ReadVector2Array(),
                uv6 = ReadVector2Array(),
                uv7 = ReadVector2Array(),
                uv8 = ReadVector2Array(),
                colors32 = ReadColors32(),
                normals = ReadVector3Array(),
                tangents = ReadVector4Array(),
                triangles = ReadIntArray(),
                bounds = new Bounds {
                    center = ReadVector3(),
                    extents = ReadVector3(),
                    max = ReadVector3(),
                    min = ReadVector3(),
                    size = ReadVector3()
                },
                indexBufferTarget = (GraphicsBuffer.Target) m_reader.ReadInt32(),
                indexFormat = (IndexFormat) m_reader.ReadInt32(),
                vertexBufferTarget = (GraphicsBuffer.Target) m_reader.ReadInt32()
            };
            mesh.SetSubMeshes(subMeshes);

            return mesh;
        }


        public async UniTask<Mesh> ReadMeshAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            var subMeshesCount = m_reader.ReadInt32();
            var subMeshes = new SubMeshDescriptor[subMeshesCount];

            if (asyncMode == AsyncMode.OnThreadPool)
                await UniTask.RunOnThreadPool(ReadSubMeshes);
            else {
                await UniTask.NextFrame();
                ReadSubMeshes();
            }

            if (asyncMode == AsyncMode.OnThreadPool)
                await UniTask.SwitchToThreadPool();
            var meshData = new MeshData {
                vertices = await ReadVector3ArrayAsync(asyncMode),
                uv = await ReadVector2ArrayAsync(asyncMode),
                uv2 = await ReadVector2ArrayAsync(asyncMode),
                uv3 = await ReadVector2ArrayAsync(asyncMode),
                uv4 = await ReadVector2ArrayAsync(asyncMode),
                uv5 = await ReadVector2ArrayAsync(asyncMode),
                uv6 = await ReadVector2ArrayAsync(asyncMode),
                uv7 = await ReadVector2ArrayAsync(asyncMode),
                uv8 = await ReadVector2ArrayAsync(asyncMode),
                colors32 = await ReadColors32Async(asyncMode),
                normals = await ReadVector3ArrayAsync(asyncMode),
                tangents = await ReadVector4ArrayAsync(asyncMode),
                triangles = await ReadIntArrayAsync(asyncMode)
            };
            if (asyncMode == AsyncMode.OnThreadPool)
                await UniTask.SwitchToMainThread();
            var mesh = new Mesh {
                name = m_reader.ReadString(),
                vertices = meshData.vertices,
                uv = meshData.uv,
                uv2 = meshData.uv2,
                uv3 = meshData.uv3,
                uv4 = meshData.uv4,
                uv5 = meshData.uv5,
                uv6 = meshData.uv6,
                uv7 = meshData.uv7,
                uv8 = meshData.uv8,
                colors32 = meshData.colors32,
                normals = meshData.normals,
                tangents = meshData.tangents,
                triangles = meshData.triangles,
                bounds = new Bounds {
                    center = ReadVector3(),
                    extents = ReadVector3(),
                    max = ReadVector3(),
                    min = ReadVector3(),
                    size = ReadVector3(),
                },
                indexBufferTarget = (GraphicsBuffer.Target) m_reader.ReadInt32(),
                indexFormat = (IndexFormat) m_reader.ReadInt32(),
                vertexBufferTarget = (GraphicsBuffer.Target) m_reader.ReadInt32()
            };
            mesh.SetSubMeshes(subMeshes);

            return mesh;

            void ReadSubMeshes () {
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
                        topology = (MeshTopology) m_reader.ReadInt32(),
                        vertexCount = m_reader.ReadInt32()
                    };
                }
            }
        }


        public Mesh[] ReadMeshes () {
            var length = m_reader.ReadInt32();
            var meshes = new Mesh[length];

            for (var i = 0; i < length; i++)
                meshes[i] = ReadMesh();

            return meshes;
        }


        public async UniTask<Mesh[]> ReadMeshesAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            var length = m_reader.ReadInt32();
            var meshes = new Mesh[length];

            for (var i = 0; i < length; i++)
                meshes[i] = await ReadMeshAsync(asyncMode);

            return meshes;
        }

        #endregion



        #region ReadingObjects

        public T ReadObject<T> () {
            return JsonUtility.FromJson<T>(m_reader.ReadString());
        }


        public List<T> ReadObjectsList<T> () {
            var count = m_reader.ReadInt32();
            var listObjects = new List<T>(count);

            for (var i = 0; i < count; i++)
                listObjects.Add(ReadObject<T>());

            return listObjects;
        }


        public async UniTask<List<T>> ReadObjectsListAsync<T> (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadObjectsList<T>);

            await UniTask.NextFrame();
            return ReadObjectsList<T>();
        }


        public T[] ReadObjectsArray<T> () {
            var length = m_reader.ReadInt32();
            var arrayObjects = new T[length];

            for (var i = 0; i < length; i++)
                arrayObjects[i] = ReadObject<T>();

            return arrayObjects;
        }


        public async UniTask<T[]> ReadObjectsArrayAsync<T> (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadObjectsArray<T>);

            await UniTask.NextFrame();
            return ReadObjectsArray<T>();
        }

        #endregion



        public byte ReadByte () {
            return m_reader.ReadByte();
        }


        public byte[] ReadBytes () {
            var length = m_reader.ReadInt32();
            var bytes = new byte[length];

            for (var i = 0; i < length; i++)
                bytes[i] = m_reader.ReadByte();

            return bytes;
        }


        public async UniTask<byte[]> ReadBytesAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadBytes);

            await UniTask.NextFrame();
            return ReadBytes();
        }


        public short ReadShort () {
            return m_reader.ReadInt16();
        }


        public short[] ReadShorts () {
            var length = m_reader.ReadInt32();
            var shorts = new short[length];

            for (var i = 0; i < length; i++)
                shorts[i] = m_reader.ReadInt16();

            return shorts;
        }


        public int ReadInt () {
            return m_reader.ReadInt32();
        }


        public int[] ReadIntArray () {
            var length = m_reader.ReadInt32();
            var intArray = new int[length];

            for (var i = 0; i < length; i++)
                intArray[i] = m_reader.ReadInt32();

            return intArray;
        }


        public async UniTask<int[]> ReadIntArrayAsync (AsyncMode asyncMode = AsyncMode.OnThreadPool) {
            if (asyncMode == AsyncMode.OnThreadPool)
                return await UniTask.RunOnThreadPool(ReadIntArray);

            await UniTask.NextFrame();
            return ReadIntArray();
        }


        public long ReadLong () {
            return m_reader.ReadInt64();
        }


        public long[] ReadLongArray () {
            var length = m_reader.ReadInt32();
            var longArray = new long[length];

            for (var i = 0; i < length; i++)
                longArray[i] = m_reader.ReadInt64();

            return longArray;
        }


        public char ReadChar () {
            return m_reader.ReadChar();
        }


        public char[] ReadChars () {
            var length = m_reader.ReadInt32();
            var chars = new char[length];

            for (var i = 0; i < length; i++)
                chars[i] = m_reader.ReadChar();

            return chars;
        }


        public string ReadString () {
            return m_reader.ReadString();
        }


        public string[] ReadStringArray () {
            var length = m_reader.ReadInt32();
            var stringArray = new string[length];

            for (var i = 0; i < length; i++)
                stringArray[i] = m_reader.ReadString();

            return stringArray;
        }


        public float ReadFloat () {
            return m_reader.ReadSingle();
        }


        public float[] ReadFloats () {
            var length = m_reader.ReadInt32();
            var floats = new float[length];

            for (var i = 0; i < length; i++)
                floats[i] = m_reader.ReadSingle();

            return floats;
        }


        public double ReadDouble () {
            return m_reader.ReadDouble();
        }


        public double[] ReadDoubles () {
            var length = m_reader.ReadInt32();
            var doubles = new double[length];

            for (var i = 0; i < length; i++)
                doubles[i] = m_reader.ReadInt64();

            return doubles;
        }


        public bool ReadBool () {
            return m_reader.ReadBoolean();
        }


        public void Dispose () {
            m_reader?.Dispose();
        }

    }

}