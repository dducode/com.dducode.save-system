using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SaveSystem.Tests.Editor {

    public class SaveSystemTests {

        private const string FILE_NAME = "test";


        [Test]
        public void BinaryTest () {
            var firstObject = new BinaryObject {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            DataManager.SaveObjects(FILE_NAME, firstObject);
            var secondObject = new BinaryObject();
            DataManager.LoadObjects(FILE_NAME, secondObject);
            Assertion(firstObject, secondObject);
            var method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        [Test]
        public void JsonTest () {
            var firstObject = new JsonObject {
                name = "Json Object",
                position = new Vector3(109, 24, 0),
                rotation = new Quaternion(0, 1, 19, 1),
                color = Color.green
            };

            DataManager.SaveObjects(FILE_NAME, firstObject);
            var secondObject = new JsonObject();
            DataManager.LoadObjects(FILE_NAME, secondObject);
            Assertion(firstObject, secondObject);
            var method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        private static void Assertion (TestObject firstObject, TestObject secondObject) {
            Assert.IsTrue(firstObject.name == secondObject.name);
            Assert.IsTrue(firstObject.position == secondObject.position);
            Assert.IsTrue(firstObject.rotation == secondObject.rotation);
            Assert.IsTrue(firstObject.color == secondObject.color);
        }

    }

}