using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.Attributes;

namespace SaveSystemPackage.Tests {

    public class ManagedSerializationTests {

        [Test]
        public async Task SerializationScopeTest () {
            var scope = new SerializationScope {
                DataFile = Storage.TestsDirectory.GetOrCreateFile("test-objects", "test")
            };

            for (var i = 0; i < 10; i++) {
                scope.RegisterSerializable($"test-object-{i}", new TestObject {
                    intValue = i,
                    floatValue = i * 1.5f,
                    stringValue = $"test-object-{i}",
                    intArray = new[] {i, i + 1, i + 2},
                    DoubleValue = i * 2.5
                });
            }

            await scope.Serialize(default);
            await scope.Deserialize(default);
        }


        [RuntimeSerializable]
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