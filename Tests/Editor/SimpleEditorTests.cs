using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using SaveSystem.Handlers;
using UnityEngine;

namespace SaveSystem.Editor.Tests {

    internal sealed class SimpleEditorTests {

        private const string FileName = "test.bytes";
        private const string FolderName = "TestFolder";


        [Test]
        public void EditorToolsTest () {
            var objectList = new List<EditorObject>();

            for (var i = 0; i < 17_500; i++) {
                var binaryObject = new EditorObject {
                    name = "Binary Object",
                    age = Random.Range(20, 80),
                    isAlive = true,
                    position = new Vector3(10, 0, 15),
                    rotation = new Quaternion(100, 5, 14, 0),
                    color = Color.cyan
                };
                objectList.Add(binaryObject);
            }

            ObjectHandler objectHandler = ObjectHandlersFactory.Create(objectList, FileName);
            objectHandler.Save();

            MethodInfo method =
                typeof(SaveSystemConsole).GetMethod("GetDataSize", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
            
            method = typeof(SaveSystemConsole).GetMethod("RemoveData", BindingFlags.Static | BindingFlags.NonPublic);
            method?.Invoke(null, new object[] { });
        }


        [Test]
        public void HandlerTest () {
            var editorObject = new EditorObject {
                name = "Editor Object",
                age = Random.Range(20, 80),
                isAlive = true,
                position = new Vector3(10, 0, 15),
                rotation = new Quaternion(100, 5, 14, 0),
                color = Color.cyan
            };

            ObjectHandlersFactory.Create(editorObject, Path.Combine(FolderName, FileName)).Save();
        }

    }

}