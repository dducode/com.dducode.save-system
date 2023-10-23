using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
using TaskBuffer = Cysharp.Threading.Tasks.UniTask<SaveSystem.DataBuffer>;
using TaskBool = Cysharp.Threading.Tasks.UniTask<bool>;
#else
using System.Threading.Tasks;
using TaskBuffer = System.Threading.Tasks.Task<SaveSystem.DataBuffer>;
using TaskBool = System.Threading.Tasks.Task<bool>;
#endif

namespace SaveSystem.UnityHandlers {

    /// <summary>
    /// Adapter to class <see cref="BinaryReader"></see> for simplify reading data
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<Vector2[]> ReadVector2ArrayAsync () {
            return await UniTask.RunOnThreadPool(ReadVector2Array);
        }
    #else
        public async Task<Vector2[]> ReadVector2ArrayAsync () {
            return await Task.Run(ReadVector2Array);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<Vector3[]> ReadVector3ArrayAsync () {
            return await UniTask.RunOnThreadPool(ReadVector3Array);
        }
    #else
        public async Task<Vector3[]> ReadVector3ArrayAsync () {
            return await Task.Run(ReadVector3Array);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<Vector4[]> ReadVector4ArrayAsync () {
            return await UniTask.RunOnThreadPool(ReadVector4Array);
        }
    #else
        public async Task<Vector4[]> ReadVector4ArrayAsync () {
            return await Task.Run(ReadVector4Array);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<Color[]> ReadColorsAsync () {
            return await UniTask.RunOnThreadPool(ReadColors);
        }
    #else
        public async Task<Color[]> ReadColorsAsync () {
            return await Task.Run(ReadColors);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<Color32[]> ReadColors32Async () {
            return await UniTask.RunOnThreadPool(ReadColors32);
        }
    #else
        public async Task<Color32[]> ReadColors32Async () {
            return await Task.Run(ReadColors32);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<Matrix4x4[]> ReadMatricesAsync () {
            return await UniTask.RunOnThreadPool(ReadMatrices);
        }
    #else
        public async Task<Matrix4x4[]> ReadMatricesAsync () {
            return await Task.Run(ReadMatrices);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<MeshData> ReadMeshAsync () {
            return await UniTask.RunOnThreadPool(ReadMesh);
        }
    #else
        public async Task<MeshData> ReadMeshAsync () {
            return await Task.Run(ReadMesh);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<T[]> ReadObjectsArrayAsync<T> () {
            return await UniTask.RunOnThreadPool(ReadObjectsArray<T>);
        }
    #else
        public async Task<T[]> ReadObjectsArrayAsync<T> () {
            return await Task.Run(ReadObjectsArray<T>);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<byte[]> ReadBytesAsync () {
            return await UniTask.RunOnThreadPool(ReadBytes);
        }
    #else
        public async Task<byte[]> ReadBytesAsync () {
            return await Task.Run(ReadBytes);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<short[]> ReadShortsAsync () {
            return await UniTask.RunOnThreadPool(ReadShorts);
        }
    #else
        public async Task<short[]> ReadShortsAsync () {
            return await Task.Run(ReadShorts);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<int[]> ReadIntArrayAsync () {
            return await UniTask.RunOnThreadPool(ReadIntArray);
        }
    #else
        public async Task<int[]> ReadIntArrayAsync () {
            return await Task.Run(ReadIntArray);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<long[]> ReadLongArrayAsync () {
            return await UniTask.RunOnThreadPool(ReadLongArray);
        }
    #else
        public async Task<long[]> ReadLongArrayAsync () {
            return await Task.Run(ReadLongArray);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<char[]> ReadCharsAsync () {
            return await UniTask.RunOnThreadPool(ReadChars);
        }
    #else
        public async Task<char[]> ReadCharsAsync () {
            return await Task.Run(ReadChars);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<string[]> ReadStringArrayAsync () {
            return await UniTask.RunOnThreadPool(ReadStringArray);
        }
    #else
        public async Task<string[]> ReadStringArrayAsync () {
            return await Task.Run(ReadStringArray);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<float[]> ReadFloatsAsync () {
            return await UniTask.RunOnThreadPool(ReadFloats);
        }
    #else
        public async Task<float[]> ReadFloatsAsync () {
            return await Task.Run(ReadFloats);
        }
    #endif


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


    #if SAVE_SYSTEM_UNITASK_SUPPORT
        public async UniTask<double[]> ReadDoublesAsync () {
            return await UniTask.RunOnThreadPool(ReadDoubles);
        }
    #else
        public async Task<double[]> ReadDoublesAsync () {
            return await Task.Run(ReadDoubles);
        }
    #endif


        public bool ReadBool () {
            return m_reader.ReadBoolean();
        }


        public void Dispose () {
            m_reader?.Dispose();
        }


        internal async TaskBuffer ReadDataBufferAsync () {
            var buffer = new DataBuffer {
                vector2 = ReadVector2(),
                vector3 = ReadVector3(),
                vector4 = ReadVector4(),
                quaternion = ReadRotation(),
                color = ReadColor(),
                color32 = ReadColor32(),
                matrix = ReadMatrix(),
                boolean = ReadBool()
            };

            if (!ReadBool()) // has any writable ReadOnlyMemory buffer?
                return buffer;

        #if SAVE_SYSTEM_UNITASK_SUPPORT
            return await UniTask.RunOnThreadPool(() => ReadDataBuffer(buffer));
        #else
            return await Task.Run(() => ReadDataBuffer(buffer));
        #endif

            DataBuffer ReadDataBuffer (DataBuffer dataBuffer) {
                dataBuffer.vector2Buffer = ReadVector2Array();
                dataBuffer.vector3Buffer = ReadVector3Array();
                dataBuffer.vector4Buffer = ReadVector4Array();
                dataBuffer.colors = ReadColors();
                dataBuffer.colors32 = ReadColors32();
                dataBuffer.matrices = ReadMatrices();
                dataBuffer.bytes = ReadBytes();
                dataBuffer.shorts = ReadShorts();
                dataBuffer.intBuffer = ReadIntArray();
                dataBuffer.longBuffer = ReadLongArray();
                dataBuffer.charBuffer = ReadChars();
                dataBuffer.stringBuffer = ReadStringArray();
                dataBuffer.floatBuffer = ReadFloats();
                dataBuffer.doubleBuffer = ReadDoubles();
                if (!ReadBool()) // has writable mesh data?
                    return dataBuffer;

                dataBuffer.meshData = ReadMesh();
                return dataBuffer;
            }
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


        internal async TaskBool ReadFileDataToBufferAsync () {
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