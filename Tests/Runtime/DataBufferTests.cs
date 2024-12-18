﻿using System.Text;
using NUnit.Framework;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Serialization;
using UnityEngine;

namespace SaveSystemPackage.Tests {

    public class DataBufferTests {

        private const string PositionsKey = "positions";

        private readonly File m_file = Storage.TestsDirectory.GetOrCreateFile(nameof(DataBufferTests), "test");


        [Test, Order(0)]
        public void WriteArray () {
            var buffer = new DataBuffer();
            var positions = new Vector3[25];

            for (var i = 0; i < positions.Length; i++)
                positions[i] = Random.insideUnitSphere;

            buffer.Write(PositionsKey, positions);
            using var writer = new SaveWriter(m_file.Open());
            writer.Write(buffer);

            var message = new StringBuilder();
            foreach (Vector3 vector3 in positions)
                message.Append($"value: {vector3}\n");
            Debug.Log(message);
        }


        [Test, Order(1)]
        public void ReadArray () {
            using var reader = new SaveReader(m_file.Open());
            DataBuffer buffer = reader.ReadDataBuffer();
            Vector3[] positions = buffer.ReadArray<Vector3>(PositionsKey);

            var message = new StringBuilder();
            foreach (Vector3 vector3 in positions)
                message.Append($"value: {vector3}\n");
            Debug.Log(message);
        }


        [Test]
        public void EncodingTest () {
            const string key = "key";
            const string str = "test-string";
            var buffer = new DataBuffer();
            buffer.Write(key, str);
            string bufferString = buffer.ReadString(key);
            Assert.IsTrue(string.Equals(str, bufferString));
        }

    }

}