using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SaveSystem.Tests.Editor {

    internal sealed class SaveSystemEditorTests {

        private const string FILE_NAME = "test";


        [Test]
        public void BinaryTest () {
            var firstObject = new BinaryObjectEditor {
                name = "Binary Object",
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            DataManager.SaveObject(FILE_NAME, firstObject);
            var secondObject = new BinaryObjectEditor();
            DataManager.LoadObject(FILE_NAME, secondObject);
            Assertion(firstObject, secondObject);

            var method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        [Test]
        public void JsonTest () {
            var firstObject = new JsonObjectEditor {
                name = "Json Object",
                position = new Vector3(109, 24, 0),
                rotation = new Quaternion(0, 1, 19, 1),
                color = Color.green
            };

            DataManager.SaveObject(FILE_NAME, firstObject);
            var secondObject = new JsonObjectEditor();
            DataManager.LoadObject(FILE_NAME, secondObject);
            Assertion(firstObject, secondObject);

            var method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        [Test]
        public void DataSizeTest () {
            var objectList = new List<BinaryObjectEditor>();

            for (var i = 0; i < 17_500; i++) {
                var binaryObject = new BinaryObjectEditor {
                    name = "Binary Object",
                    position = new Vector3(10, 0, 15),
                    rotation = new Quaternion(100, 5, 14, 0),
                    color = Color.cyan
                };
                objectList.Add(binaryObject);
            }

            DataManager.SaveObjects(FILE_NAME, objectList.ToArray());

            var method = typeof(DataManager).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });

            method = typeof(DataManager).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        private static void Assertion (TestObjectEditor firstObjectEditor, TestObjectEditor secondObjectEditor) {
            Assert.IsTrue(firstObjectEditor.name == secondObjectEditor.name);
            Assert.IsTrue(firstObjectEditor.position == secondObjectEditor.position);
            Assert.IsTrue(firstObjectEditor.rotation == secondObjectEditor.rotation);
            Assert.IsTrue(firstObjectEditor.color == secondObjectEditor.color);
        }

    }

}