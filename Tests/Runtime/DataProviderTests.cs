using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.Providers;
using SaveSystemPackage.SerializableData;
using UnityEngine;

namespace SaveSystemPackage.Tests {

  public class DataProviderTests {

    [Test]
    public async Task DataProviderTestVersions() {
      var provider = new DataProvider { Version = "1.0.0" };
      await provider.SaveData<ColorData>(Color.white);

      provider.Version = "1.0.1";
      await provider.SaveData<ColorData>(Color.green);

      provider.Version = "1.0.2";
      await provider.SaveData<ColorData>(Color.yellow);

      provider.Version = "1.0.0";
      Debug.Log($"1.0.0: {await provider.LoadData<ColorData>()}");

      provider.Version = "1.0.1";
      Debug.Log($"1.0.1: {await provider.LoadData<ColorData>()}");

      provider.Version = "1.0.2";
      Debug.Log($"1.0.2: {await provider.LoadData<ColorData>()}");

      provider.Version = "1.1.0";
      Debug.Log($"1.1.0: {await provider.LoadData<ColorData>()}");
    }

  }

}