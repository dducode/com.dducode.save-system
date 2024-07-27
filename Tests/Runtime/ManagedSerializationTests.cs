using System;
using System.IO;
using NUnit.Framework;
using SaveSystemPackage.Attributes;
using SaveSystemPackage.BinaryHandlers;

namespace SaveSystemPackage.Tests {

    public class ManagedSerializationTests {

        [Test]
        public void ReadWriteTest () {
            var first = new TestObject {
                intValue = 10,
                floatValue = 2.5f,
                stringValue = "test-object",
                intArray = new[] {1, 2, 3, 4, 5},
                DoubleValue = 5
            };

            var memoryStream = new MemoryStream();

            using (var writer = new SaveWriter(memoryStream)) {
                writer.Write(first);
            }

            TestObject second;

            using (var reader = new SaveReader(new MemoryStream(memoryStream.ToArray()))) {
                second = (TestObject)reader.ReadObject(typeof(TestObject));
            }

            Assert.IsTrue(
                first.intValue == second.intValue &&
                Math.Abs(first.floatValue - second.floatValue) < 0.0001f &&
                string.Equals(first.stringValue, second.stringValue) &&
                Math.Abs(first.DoubleValue - second.DoubleValue) < 0.00001f
            );
        }


        private class TestObject {

            public int intValue;
            public float floatValue;
            public string stringValue;
            public int[] intArray;

            public double DoubleValue {
                get => m_double;
                set => m_double = value;
            }

            [RuntimeSerializedField]
            private double m_double;

        }

    }

}