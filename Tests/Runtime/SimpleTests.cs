using System;
using NUnit.Framework;
using SaveSystem.InternalServices;
using SaveSystem.Tests.TestObjects;
using UnityEngine;

namespace SaveSystem.Tests {

    internal sealed class SimpleTests {

        private const string FilePath = "test.bytes";


        [Test(Author = "dducode", Description = "Obsolete test")]
        [Obsolete("Test of obsolete logic")]
        public void BinarySaveLoad () {
            var firstObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            DataManager.SaveObject(FilePath, firstObject);
            var secondObject = new BinaryObject();
            DataManager.LoadObject(FilePath, secondObject);
            Assertion(firstObject, secondObject);
        }


        [Test(Author = "dducode", Description = "Obsolete test")]
        [Obsolete("Test of obsolete logic")]
        public void JsonSaveLoad () {
            var firstObject = new JsonObject {
                name = "Json Object",
                position = new Vector3(109, 24, 0),
                rotation = new Quaternion(0, 1, 19, 1),
                color = Color.green
            };

            DataManager.SaveObject(FilePath, firstObject);
            var secondObject = new JsonObject();
            DataManager.LoadObject(FilePath, secondObject);
            Assertion(firstObject, secondObject);
        }


        [TearDown]
        public void EndTest () {
            Storage.DeleteAllData();
        }


        private static void Assertion (TestObject firstObject, TestObject secondObject) {
            Assert.IsTrue(firstObject.name == secondObject.name);
            Assert.IsTrue(firstObject.position == secondObject.position);
            Assert.IsTrue(firstObject.rotation == secondObject.rotation);
            Assert.IsTrue(firstObject.color == secondObject.color);
        }

    }

}