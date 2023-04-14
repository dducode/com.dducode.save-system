using System.Collections.Generic;
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

            DataManager.SaveObject(FILE_NAME, firstObject);
            var secondObject = new BinaryObject();
            DataManager.LoadObject(FILE_NAME, secondObject);
            Assertion(firstObject, secondObject);

            var method = typeof(DataManager).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
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

            DataManager.SaveObject(FILE_NAME, firstObject);
            var secondObject = new JsonObject();
            DataManager.LoadObject(FILE_NAME, secondObject);
            Assertion(firstObject, secondObject);

            var method = typeof(DataManager).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        [Test]
        public void DataSizeTest () {
            var objectList = new List<BinaryObject>();

            for (var i = 0; i < 100_000; i++) {
                var binaryObject = new BinaryObject {
                    name = "Binary Object",
                    position = new Vector3(10, 0, 15),
                    rotation = new Quaternion(100, 5, 14, 0),
                    color = Color.cyan
                };
                objectList.Add(binaryObject);
            }
            
            DataManager.SaveObjects(FILE_NAME, objectList);

            var method = typeof(DataManager).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
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