using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace SaveSystem {

    public class UnityAsyncReader : IDisposable {

        private BinaryReader m_reader;


        public UnityAsyncReader (BinaryReader reader) {
            m_reader = reader;
        }


        public async Task<Vector2> ReadVector2 () {
            return await new ValueTask<Vector2>(new Vector2 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle()
            });
        }


        public async Task<Vector2[]> ReadVector2Array () {
            var length = m_reader.ReadInt32();
            var vector2Array = new Vector2[length];

            for (var i = 0; i < length; i++)
                vector2Array[i] = await ReadVector2();

            return await new ValueTask<Vector2[]>(vector2Array);
        }


        public async Task<Vector3> ReadVector3 () {
            return await new ValueTask<Vector3>(new Vector3 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle()
            });
        }


        public async Task<Vector3[]> ReadVector3Array () {
            var length = m_reader.ReadInt32();
            var vector3Array = new Vector3[length];

            for (var i = 0; i < length; i++)
                vector3Array[i] = await ReadVector3();

            return await new ValueTask<Vector3[]>(vector3Array);
        }


        public async Task<Vector4> ReadVector4 () {
            return await new ValueTask<Vector4>(new Vector4 {
                x = m_reader.ReadSingle(),
                y = m_reader.ReadSingle(),
                z = m_reader.ReadSingle(),
                w = m_reader.ReadSingle()
            });
        }


        public async Task<Vector4[]> ReadVector4Array () {
            var length = m_reader.ReadInt32();
            var vector4Array = new Vector4[length];

            for (var i = 0; i < length; i++)
                vector4Array[i] = await ReadVector4();

            return await new ValueTask<Vector4[]>(vector4Array);
        }


        public async Task<Color32> ReadColor32 () {
            return await new ValueTask<Color32>(new Color32 {
                r = m_reader.ReadByte(),
                g = m_reader.ReadByte(),
                b = m_reader.ReadByte(),
                a = m_reader.ReadByte()
            });
        }


        public async Task<Color32[]> ReadColors32 () {
            var length = m_reader.ReadInt32();
            var colors32 = new Color32[length];

            for (var i = 0; i < length; i++)
                colors32[i] = await ReadColor32();

            return await new ValueTask<Color32[]>(colors32);
        }


        public async Task<Mesh> ReadMesh () {
            return await new ValueTask<Mesh>(new Mesh {
                name = m_reader.ReadString(),
                vertices = await ReadVector3Array(),
                uv = await ReadVector2Array(),
                uv2 = await ReadVector2Array(),
                uv3 = await ReadVector2Array(),
                uv4 = await ReadVector2Array(),
                uv5 = await ReadVector2Array(),
                uv6 = await ReadVector2Array(),
                uv7 = await ReadVector2Array(),
                uv8 = await ReadVector2Array(),
                bounds = new Bounds {
                    center = await ReadVector3(),
                    extents = await ReadVector3(),
                    max = await ReadVector3(),
                    min = await ReadVector3(),
                    size = await ReadVector3(),
                },
                colors32 = await ReadColors32(),
                normals = await ReadVector3Array(),
                tangents = await ReadVector4Array(),
                triangles = await ReadIntArray()
            });
        }


        public async Task<int[]> ReadIntArray () {
            var length = m_reader.ReadInt32();
            var intArray = new int[length];

            for (var i = 0; i < length; i++)
                intArray[i] = m_reader.ReadInt32();

            return await new ValueTask<int[]>(intArray);
        }


        public void Dispose () {
            m_reader?.Dispose();
        }

    }

}