using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SaveSystemPackage.Providers;
using SaveSystemPackage.SerializableData;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Storages;
using UnityEngine;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace SaveSystemPackage.Tests {

    public class XmlSerializerTests {

        [UnityTest]
        public IEnumerator RigidbodyDataSaveLoadTest () {
            var data = new RigidbodyData {
                position = Random.insideUnitSphere,
                rotation = Random.rotation
            };
            var testScope = new SerializationScope {
                Serializer = new XmlSerializer(),
                KeyProvider = new KeyStore(new Dictionary<Type, string> {
                    {typeof(RigidbodyData), "test-rigidbody-data"}
                }),
                DataStorage = new FileSystemStorage(Storage.TestsDirectory, "xml")
            };
            var completed = false;
            testScope.SaveData(data).ContinueWith(_ => completed = true);
            yield return new WaitWhile(() => !completed);
            completed = false;
            RigidbodyData loadedData = default;
            testScope.LoadData<RigidbodyData>().ContinueWith(rd => {
                completed = true;
                loadedData = rd.Result;
            });
            yield return new WaitWhile(() => !completed);
            Assert.That(data.Equals(loadedData), $"Objects doesn't equal. Data: {data}, loaded data: {loadedData}");
        }

    }

}