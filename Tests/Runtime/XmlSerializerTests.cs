using System.Collections;
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
    public IEnumerator RigidbodyDataSaveLoadTest() {
      const string key = "rigidbody-data";

      var data = new RigidbodyData {
        position = Random.insideUnitSphere,
        rotation = Random.rotation
      };
      var testScope = new SerializationContext {
        DataProvider = new DataProvider {
          Serializer = new XmlSerializer(),
          DataStorage = new FileSystemStorage(Storage.TestsDirectory, "xml")
        }
      };
      var completed = false;
      testScope.SaveData(key, data).ContinueWith(_ => completed = true);
      yield return new WaitWhile(() => !completed);
      completed = false;
      RigidbodyData loadedData = default;
      testScope.LoadData<RigidbodyData>(key).ContinueWith(rd => {
        completed = true;
        loadedData = rd.Result;
      });
      yield return new WaitWhile(() => !completed);
      Assert.That(data.Equals(loadedData), $"Objects doesn't equal. Data: {data}, loaded data: {loadedData}");
    }

  }

}