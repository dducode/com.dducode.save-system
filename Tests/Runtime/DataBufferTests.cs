using System.IO;
using System.Text;
using NUnit.Framework;
using SaveSystemPackage.BinaryHandlers;
using UnityEngine;

namespace SaveSystemPackage.Tests {

    public class DataBufferTests {

        private const string PositionsKey = "positions";

        private readonly string m_filePath = Storage.GetFullPath(nameof(DataBufferTests) + ".test");


        [Test, Order(0)]
        public void WriteArray () {
            var buffer = new DataBuffer();
            var positions = new Vector3[25];

            for (var i = 0; i < positions.Length; i++)
                positions[i] = Random.insideUnitSphere;

            buffer.Write(PositionsKey, positions);
            using var writer = new SaveWriter(File.Open(m_filePath, FileMode.OpenOrCreate));
            writer.Write(buffer);

            var message = new StringBuilder();
            foreach (Vector3 vector3 in positions)
                message.Append($"value: {vector3}\n");
            Debug.Log(message);
        }


        [Test, Order(1)]
        public void ReadArray () {
            using var reader = new SaveReader(File.Open(m_filePath, FileMode.Open));
            DataBuffer buffer = reader.ReadDataBuffer();
            Vector3[] positions = buffer.GetArray<Vector3>(PositionsKey);

            var message = new StringBuilder();
            foreach (Vector3 vector3 in positions)
                message.Append($"value: {vector3}\n");
            Debug.Log(message);
        }

    }

}