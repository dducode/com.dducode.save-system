using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;

// ReSharper disable UnusedMember.Global
namespace SaveSystem {

    /// <summary>
    /// The buffer for writing and reading data
    /// </summary>
    public record DataBuffer {

        private const string DefaultSemantic = "default_semantic";

        private readonly Dictionary<string, Vector2> m_vector2Buffer;
        private readonly Dictionary<string, Vector2Int> m_vector2IntBuffer;
        private readonly Dictionary<string, Vector3> m_vector3Buffer;
        private readonly Dictionary<string, Vector3Int> m_vector3IntBuffer;
        private readonly Dictionary<string, Vector4> m_vector4Buffer;
        private readonly Dictionary<string, Quaternion> m_quaternionBuffer;
        private readonly Dictionary<string, Color> m_colors;
        private readonly Dictionary<string, Color32> m_colors32;
        private readonly Dictionary<string, Matrix4x4> m_matrices;
        private readonly Dictionary<string, MeshData> m_meshDataBuffer;

        private readonly Dictionary<string, byte> m_bytes;
        private readonly Dictionary<string, sbyte> m_sBytes;
        private readonly Dictionary<string, short> m_int16Buffer;
        private readonly Dictionary<string, ushort> m_uint16Buffer;
        private readonly Dictionary<string, int> m_int32Buffer;
        private readonly Dictionary<string, uint> m_uint32Buffer;
        private readonly Dictionary<string, long> m_int64Buffer;
        private readonly Dictionary<string, ulong> m_uint64Buffer;
        private readonly Dictionary<string, char> m_charBuffer;
        private readonly Dictionary<string, string> m_stringBuffer;
        private readonly Dictionary<string, float> m_singleBuffer;
        private readonly Dictionary<string, double> m_doubleBuffer;
        private readonly Dictionary<string, bool> m_boolBuffer;
        private readonly Dictionary<string, decimal> m_decimalBuffer;


        public DataBuffer () {
            m_vector2Buffer = new Dictionary<string, Vector2>();
            m_vector2IntBuffer = new Dictionary<string, Vector2Int>();
            m_vector3Buffer = new Dictionary<string, Vector3>();
            m_vector3IntBuffer = new Dictionary<string, Vector3Int>();
            m_vector4Buffer = new Dictionary<string, Vector4>();
            m_quaternionBuffer = new Dictionary<string, Quaternion>();
            m_colors = new Dictionary<string, Color>();
            m_colors32 = new Dictionary<string, Color32>();
            m_matrices = new Dictionary<string, Matrix4x4>();
            m_meshDataBuffer = new Dictionary<string, MeshData>();

            m_bytes = new Dictionary<string, byte>();
            m_sBytes = new Dictionary<string, sbyte>();
            m_int16Buffer = new Dictionary<string, short>();
            m_uint16Buffer = new Dictionary<string, ushort>();
            m_int32Buffer = new Dictionary<string, int>();
            m_uint32Buffer = new Dictionary<string, uint>();
            m_int64Buffer = new Dictionary<string, long>();
            m_uint64Buffer = new Dictionary<string, ulong>();
            m_charBuffer = new Dictionary<string, char>();
            m_stringBuffer = new Dictionary<string, string>();
            m_singleBuffer = new Dictionary<string, float>();
            m_doubleBuffer = new Dictionary<string, double>();
            m_boolBuffer = new Dictionary<string, bool>();
            m_decimalBuffer = new Dictionary<string, decimal>();
        }


        internal DataBuffer (BinaryReader reader) {
            m_vector2Buffer = ReadBuffer<Vector2>(reader);
            m_vector2IntBuffer = ReadBuffer<Vector2Int>(reader);
            m_vector3Buffer = ReadBuffer<Vector3>(reader);
            m_vector3IntBuffer = ReadBuffer<Vector3Int>(reader);
            m_vector4Buffer = ReadBuffer<Vector4>(reader);
            m_quaternionBuffer = ReadBuffer<Quaternion>(reader);
            m_colors = ReadBuffer<Color>(reader);
            m_colors32 = ReadBuffer<Color32>(reader);
            m_matrices = ReadBuffer<Matrix4x4>(reader);
            m_meshDataBuffer = ReadMeshDataBuffer(reader);

            m_bytes = ReadBuffer<byte>(reader);
            m_sBytes = ReadBuffer<sbyte>(reader);
            m_int16Buffer = ReadBuffer<short>(reader);
            m_uint16Buffer = ReadBuffer<ushort>(reader);
            m_int32Buffer = ReadBuffer<int>(reader);
            m_uint32Buffer = ReadBuffer<uint>(reader);
            m_int64Buffer = ReadBuffer<long>(reader);
            m_uint64Buffer = ReadBuffer<ulong>(reader);
            m_charBuffer = ReadBuffer<char>(reader);
            m_stringBuffer = ReadStringBuffer(reader);
            m_singleBuffer = ReadBuffer<float>(reader);
            m_doubleBuffer = ReadBuffer<double>(reader);
            m_boolBuffer = ReadBuffer<bool>(reader);
            m_decimalBuffer = ReadBuffer<decimal>(reader);
        }


        public void Add (Vector2 value, string semantic = null) {
            m_vector2Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Vector2Int value, string semantic = null) {
            m_vector2IntBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Vector3 value, string semantic = null) {
            m_vector3Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Vector3Int value, string semantic = null) {
            m_vector3IntBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Vector4 value, string semantic = null) {
            m_vector4Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Quaternion value, string semantic = null) {
            m_quaternionBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Color value, string semantic = null) {
            m_colors.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Color32 value, string semantic = null) {
            m_colors32.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (Matrix4x4 value, string semantic = null) {
            m_matrices.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (MeshData value, string semantic = null) {
            m_meshDataBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (byte value, string semantic = null) {
            m_bytes.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (sbyte value, string semantic = null) {
            m_sBytes.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (short value, string semantic = null) {
            m_int16Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (ushort value, string semantic = null) {
            m_uint16Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (int value, string semantic = null) {
            m_int32Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (uint value, string semantic = null) {
            m_uint32Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (long value, string semantic = null) {
            m_int64Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (ulong value, string semantic = null) {
            m_uint64Buffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (char value, string semantic = null) {
            m_charBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (string value, string semantic = null) {
            m_stringBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (float value, string semantic = null) {
            m_singleBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (double value, string semantic = null) {
            m_doubleBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (bool value, string semantic = null) {
            m_boolBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public void Add (decimal value, string semantic = null) {
            m_decimalBuffer.Add(string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic, value);
        }


        public Vector2 GetVector2 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_vector2Buffer.ContainsKey(key) ? m_vector2Buffer[key] : default;
        }


        public Vector2Int GetVector2Int (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_vector2IntBuffer.ContainsKey(key) ? m_vector2IntBuffer[key] : default;
        }


        public Vector3 GetVector3 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_vector3Buffer.ContainsKey(key) ? m_vector3Buffer[key] : default;
        }


        public Vector3Int GetVector3Int (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_vector3IntBuffer.ContainsKey(key) ? m_vector3IntBuffer[key] : default;
        }


        public Vector4 GetVector4 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_vector4Buffer.ContainsKey(key) ? m_vector4Buffer[key] : default;
        }


        public Quaternion GetQuaternion (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_quaternionBuffer.ContainsKey(key) ? m_quaternionBuffer[key] : default;
        }


        public Color GetColor (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_colors.ContainsKey(key) ? m_colors[key] : default;
        }


        public Color32 GetColor32 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_colors32.ContainsKey(key) ? m_colors32[key] : default;
        }


        public Matrix4x4 GetMatrix (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_matrices.ContainsKey(key) ? m_matrices[key] : default;
        }


        public MeshData GetMeshData (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_meshDataBuffer.ContainsKey(key) ? m_meshDataBuffer[key] : default;
        }


        public byte GetByte (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_bytes.ContainsKey(key) ? m_bytes[key] : default;
        }


        public sbyte GetSByte (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_sBytes.ContainsKey(key) ? m_sBytes[key] : default;
        }


        public short GetInt16 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_int16Buffer.ContainsKey(key) ? m_int16Buffer[key] : default;
        }


        public ushort GetUInt16 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_uint16Buffer.ContainsKey(key) ? m_uint16Buffer[key] : default;
        }


        public int GetInt32 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_int32Buffer.ContainsKey(key) ? m_int32Buffer[key] : default;
        }


        public uint GetUInt32 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_uint32Buffer.ContainsKey(key) ? m_uint32Buffer[key] : default;
        }


        public long GetInt64 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_int64Buffer.ContainsKey(key) ? m_int64Buffer[key] : default;
        }


        public ulong GetUInt64 (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_uint64Buffer.ContainsKey(key) ? m_uint64Buffer[key] : default;
        }


        public char GetChar (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_charBuffer.ContainsKey(key) ? m_charBuffer[key] : default;
        }


        public string GetString (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_stringBuffer.ContainsKey(key) ? m_stringBuffer[key] : default;
        }


        public float GetSingle (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_singleBuffer.ContainsKey(key) ? m_singleBuffer[key] : default;
        }


        public double GetDouble (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_doubleBuffer.ContainsKey(key) ? m_doubleBuffer[key] : default;
        }


        public bool GetBoolean (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_boolBuffer.ContainsKey(key) ? m_boolBuffer[key] : default;
        }


        public decimal GetDecimal (string semantic = null) {
            string key = string.IsNullOrEmpty(semantic) ? DefaultSemantic : semantic;
            return m_decimalBuffer.ContainsKey(key) ? m_decimalBuffer[key] : default;
        }


        internal void WriteData (BinaryWriter writer) {
            WriteBuffer(m_vector2Buffer, writer);
            WriteBuffer(m_vector2IntBuffer, writer);
            WriteBuffer(m_vector3Buffer, writer);
            WriteBuffer(m_vector3IntBuffer, writer);
            WriteBuffer(m_vector4Buffer, writer);
            WriteBuffer(m_quaternionBuffer, writer);
            WriteBuffer(m_colors, writer);
            WriteBuffer(m_colors32, writer);
            WriteBuffer(m_matrices, writer);
            WriteBuffer(m_meshDataBuffer, writer);

            WriteBuffer(m_bytes, writer);
            WriteBuffer(m_sBytes, writer);
            WriteBuffer(m_int16Buffer, writer);
            WriteBuffer(m_uint16Buffer, writer);
            WriteBuffer(m_int32Buffer, writer);
            WriteBuffer(m_uint32Buffer, writer);
            WriteBuffer(m_int64Buffer, writer);
            WriteBuffer(m_uint64Buffer, writer);
            WriteBuffer(m_charBuffer, writer);
            WriteBuffer(m_stringBuffer, writer);
            WriteBuffer(m_singleBuffer, writer);
            WriteBuffer(m_doubleBuffer, writer);
            WriteBuffer(m_boolBuffer, writer);
            WriteBuffer(m_decimalBuffer, writer);
        }


        private void WriteBuffer<TValue> (Dictionary<string, TValue> buffer, BinaryWriter writer)
            where TValue : unmanaged {
            writer.Write(buffer.Count);
            writer.Write(buffer.Keys.ToArray());
            writer.Write(buffer.Values.ToArray());
        }


        private static Dictionary<string, TValue> ReadBuffer<TValue> (BinaryReader reader) where TValue : unmanaged {
            var count = reader.Read<int>();
            ReadOnlySpan<string> keys = reader.ReadStringArray();
            ReadOnlySpan<TValue> values = reader.ReadArray<TValue>();

            var buffer = new Dictionary<string, TValue>();
            for (var i = 0; i < count; i++)
                buffer.Add(keys[i], values[i]);
            return buffer;
        }


        private void WriteBuffer (Dictionary<string, MeshData> buffer, BinaryWriter writer) {
            writer.Write(buffer.Count);
            writer.Write(buffer.Keys.ToArray());
            writer.Write(buffer.Values.ToArray());
        }


        private static Dictionary<string, MeshData> ReadMeshDataBuffer (BinaryReader reader) {
            var count = reader.Read<int>();
            ReadOnlySpan<string> keys = reader.ReadStringArray();
            ReadOnlySpan<MeshData> values = reader.ReadMeshDataArray();

            var buffer = new Dictionary<string, MeshData>();
            for (var i = 0; i < count; i++)
                buffer.Add(keys[i], values[i]);
            return buffer;
        }


        private void WriteBuffer (Dictionary<string, string> buffer, BinaryWriter writer) {
            writer.Write(buffer.Count);
            writer.Write(buffer.Keys.ToArray());
            writer.Write(buffer.Values.ToArray());
        }


        private Dictionary<string, string> ReadStringBuffer (BinaryReader reader) {
            var count = reader.Read<int>();
            ReadOnlySpan<string> keys = reader.ReadStringArray();
            ReadOnlySpan<string> values = reader.ReadStringArray();

            var buffer = new Dictionary<string, string>();
            for (var i = 0; i < count; i++)
                buffer.Add(keys[i], values[i]);
            return buffer;
        }

    }

}