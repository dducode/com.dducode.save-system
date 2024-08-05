using NUnit.Framework;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Tests {

    public class EncodedObjectsTests {

        private static int[] s_iterationsArray = {1, 5};


        [Test]
        public void EncodedValueTest ([ValueSource(nameof(s_iterationsArray))] int iterations) {
            var testStruct = new Test {
                intValue = 42,
                doubleValue = 42.5f
            };

            var encodedStruct = new EncodedValue<Test>(iterations) {Value = testStruct};
            Assert.AreEqual(testStruct, encodedStruct.Value);
        }


        [Test]
        public void EncodedStringTest ([ValueSource(nameof(s_iterationsArray))] int iterations) {
            const string str = "test-text";
            var encodedStr = new EncodedString(iterations) {Value = str};
            Assert.IsTrue(string.Equals(str, encodedStr.Value));
        }


        private struct Test {

            public int intValue;
            public double doubleValue;

        }

    }

}