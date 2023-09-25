using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SaveSystem.UnityHandlers {

    /// <summary>
    /// Adapter to class <see cref="BinaryReader"></see> for simplify reading data
    /// </summary>
    public sealed class UnityReader : IDisposable {

        private readonly BinaryReader m_reader;
        private readonly string m_fullPath;


        public UnityReader (BinaryReader reader, string fullPath = "") {
            m_reader = reader;
            m_fullPath = fullPath;
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


        public Vector2 ReadVector2 () {
            return new Vector2 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle()
            };
        }


        public Vector2[] ReadVector2Array () {
            int length = m_reader.ReadInt32();
            var vector2Array = new Vector2[length];

            for (var i = 0; i < length; i++)
                vector2Array[i] = ReadVector2();

            return vector2Array;
        }


        public async UniTask<Vector2[]> ReadVector2ArrayAsync () {
            Vector2[] vector2Array = Array.Empty<Vector2>();
            await UniTask.RunOnThreadPool(() => vector2Array = ReadVector2Array());
            return vector2Array;
        }


        public Vector3 ReadVector3 () {
            return new Vector3 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle()
            };
        }


        public Vector3[] ReadVector3Array () {
            int length = m_reader.ReadInt32();
            var vector3Array = new Vector3[length];

            for (var i = 0; i < length; i++)
                vector3Array[i] = ReadVector3();

            return vector3Array;
        }


        public async UniTask<Vector3[]> ReadVector3ArrayAsync () {
            Vector3[] vector3Array = Array.Empty<Vector3>();
            await UniTask.RunOnThreadPool(() => vector3Array = ReadVector3Array());
            return vector3Array;
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
            int length = m_reader.ReadInt32();
            var vector4Array = new Vector4[length];

            for (var i = 0; i < length; i++)
                vector4Array[i] = ReadVector4();

            return vector4Array;
        }


        public async UniTask<Vector4[]> ReadVector4ArrayAsync () {
            Vector4[] vector4Array = Array.Empty<Vector4>();
            await UniTask.RunOnThreadPool(() => vector4Array = ReadVector4Array());
            return vector4Array;
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
            int length = m_reader.ReadInt32();
            var colors = new Color[length];

            for (var i = 0; i < length; i++)
                colors[i] = ReadColor();

            return colors;
        }


        public async UniTask<Color[]> ReadColorsAsync () {
            Color[] colors = Array.Empty<Color>();
            await UniTask.RunOnThreadPool(() => colors = ReadColors());
            return colors;
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
            int length = m_reader.ReadInt32();
            var colors32 = new Color32[length];

            for (var i = 0; i < length; i++)
                colors32[i] = ReadColor32();

            return colors32;
        }


        public async UniTask<Color32[]> ReadColors32Async () {
            Color32[] colors32 = Array.Empty<Color32>();
            await UniTask.RunOnThreadPool(() => colors32 = ReadColors32());
            return colors32;
        }


        public Matrix4x4 ReadMatrix () {
            var matrix = new Matrix4x4();
            for (var i = 0; i < 16; i++)
                matrix[i] = m_reader.ReadSingle();

            return matrix;
        }


        public Matrix4x4[] ReadMatrices () {
            int length = m_reader.ReadInt32();
            var matrices = new Matrix4x4[length];

            for (var i = 0; i < length; i++)
                matrices[i] = ReadMatrix();

            return matrices;
        }


        public async UniTask<Matrix4x4[]> ReadMatricesAsync () {
            Matrix4x4[] matrices = Array.Empty<Matrix4x4>();
            await UniTask.RunOnThreadPool(() => matrices = ReadMatrices());
            return matrices;
        }


        public MeshData ReadMesh () {
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
                    topology = (MeshTopology)m_reader.ReadInt32()
                };
                subMeshIndices[i] = ReadIntArray();
            }

            return new MeshData {
                subMeshes = subMeshes,
                subMeshIndices = subMeshIndices,
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
                indexBufferTarget = (GraphicsBuffer.Target)m_reader.ReadByte(),
                indexFormat = (IndexFormat)m_reader.ReadByte(),
                vertexBufferTarget = (GraphicsBuffer.Target)m_reader.ReadByte()
            };
        }


        public async UniTask<MeshData> ReadMeshAsync () {
            MeshData mesh = default;
            await UniTask.RunOnThreadPool(() => mesh = ReadMesh());
            return mesh;
        }


        public T ReadObject<T> () {
            return JsonUtility.FromJson<T>(m_reader.ReadString());
        }


        public T[] ReadObjectsArray<T> () {
            int length = m_reader.ReadInt32();
            var arrayObjects = new T[length];

            for (var i = 0; i < length; i++)
                arrayObjects[i] = ReadObject<T>();

            return arrayObjects;
        }


        public async UniTask<T[]> ReadObjectsArrayAsync<T> () {
            T[] objectsArray = Array.Empty<T>();
            await UniTask.RunOnThreadPool(() => objectsArray = ReadObjectsArray<T>());
            return objectsArray;
        }


        public byte ReadByte () {
            return m_reader.ReadByte();
        }


        public byte[] ReadBytes () {
            int length = m_reader.ReadInt32();
            var bytes = new byte[length];

            for (var i = 0; i < length; i++)
                bytes[i] = m_reader.ReadByte();

            return bytes;
        }


        public async UniTask<byte[]> ReadBytesAsync () {
            byte[] bytes = Array.Empty<byte>();
            await UniTask.RunOnThreadPool(() => bytes = ReadBytes());
            return bytes;
        }


        public short ReadShort () {
            return m_reader.ReadInt16();
        }


        public short[] ReadShorts () {
            int length = m_reader.ReadInt32();
            var shorts = new short[length];

            for (var i = 0; i < length; i++)
                shorts[i] = m_reader.ReadInt16();

            return shorts;
        }


        public async UniTask<short[]> ReadShortsAsync () {
            short[] shorts = Array.Empty<short>();
            await UniTask.RunOnThreadPool(() => shorts = ReadShorts());
            return shorts;
        }


        public int ReadInt () {
            return m_reader.ReadInt32();
        }


        public int[] ReadIntArray () {
            int length = m_reader.ReadInt32();
            var intArray = new int[length];

            for (var i = 0; i < length; i++)
                intArray[i] = m_reader.ReadInt32();

            return intArray;
        }


        public async UniTask<int[]> ReadIntArrayAsync () {
            int[] intArray = Array.Empty<int>();
            await UniTask.RunOnThreadPool(() => intArray = ReadIntArray());
            return intArray;
        }


        public long ReadLong () {
            return m_reader.ReadInt64();
        }


        public long[] ReadLongArray () {
            int length = m_reader.ReadInt32();
            var longArray = new long[length];

            for (var i = 0; i < length; i++)
                longArray[i] = m_reader.ReadInt64();

            return longArray;
        }


        public async UniTask<long[]> ReadLongArrayAsync () {
            long[] longArray = Array.Empty<long>();
            await UniTask.RunOnThreadPool(() => longArray = ReadLongArray());
            return longArray;
        }


        public char ReadChar () {
            return m_reader.ReadChar();
        }


        public char[] ReadChars () {
            int length = m_reader.ReadInt32();
            var chars = new char[length];

            for (var i = 0; i < length; i++)
                chars[i] = m_reader.ReadChar();

            return chars;
        }


        public async UniTask<char[]> ReadCharsAsync () {
            char[] chars = Array.Empty<char>();
            await UniTask.RunOnThreadPool(() => chars = ReadChars());
            return chars;
        }


        public string ReadString () {
            return m_reader.ReadString();
        }


        public string[] ReadStringArray () {
            int length = m_reader.ReadInt32();
            var stringArray = new string[length];

            for (var i = 0; i < length; i++)
                stringArray[i] = m_reader.ReadString();

            return stringArray;
        }


        public async UniTask<string[]> ReadStringArrayAsync () {
            string[] stringArray = Array.Empty<string>();
            await UniTask.RunOnThreadPool(() => stringArray = ReadStringArray());
            return stringArray;
        }


        public float ReadFloat () {
            return m_reader.ReadSingle();
        }


        public float[] ReadFloats () {
            int length = m_reader.ReadInt32();
            var floats = new float[length];

            for (var i = 0; i < length; i++)
                floats[i] = m_reader.ReadSingle();

            return floats;
        }


        public async UniTask<float[]> ReadFloatsAsync () {
            float[] floats = Array.Empty<float>();
            await UniTask.RunOnThreadPool(() => floats = ReadFloats());
            return floats;
        }


        public double ReadDouble () {
            return m_reader.ReadDouble();
        }


        public double[] ReadDoubles () {
            int length = m_reader.ReadInt32();
            var doubles = new double[length];

            for (var i = 0; i < length; i++)
                doubles[i] = m_reader.ReadInt64();

            return doubles;
        }


        public async UniTask<double[]> ReadDoublesAsync () {
            double[] doubles = Array.Empty<double>();
            await UniTask.RunOnThreadPool(() => doubles = ReadDoubles());
            return doubles;
        }


        public bool ReadBool () {
            return m_reader.ReadBoolean();
        }


        public void Dispose () {
            m_reader?.Dispose();
        }


        internal async UniTask WriteToBufferAsync (ReadOnlyMemory<byte> data) {
            await ((MemoryStream)m_reader.BaseStream).WriteAsync(data);
        }


        internal bool ReadFileDataToBuffer () {
            if (File.Exists(m_fullPath)) {
                var memoryStream = (MemoryStream)m_reader.BaseStream;
                memoryStream.Write(File.ReadAllBytes(m_fullPath));
                memoryStream.Position = 0;
                return true;
            }
            else {
                return false;
            }
        }


        internal async UniTask<bool> ReadFileDataToBufferAsync () {
            if (File.Exists(m_fullPath)) {
                var memoryStream = (MemoryStream)m_reader.BaseStream;
                await memoryStream.WriteAsync(await File.ReadAllBytesAsync(m_fullPath));
                memoryStream.Position = 0;
                return true;
            }
            else {
                return false;
            }
        }

    }

}