using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Providers;
using Directory = SaveSystemPackage.Internal.Directory;
using File = SaveSystemPackage.Internal.File;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

  public class SerializationContext : ISerializationContext {

    [NotNull]
    public virtual string Name {
      get => _name;
      set => _name = !string.IsNullOrEmpty(value) ? value : throw new ArgumentNullException(nameof(Name));
    }

    public ISaveDataProvider DataProvider {
      get => _dataProvider;
      set => _dataProvider = value ?? throw new ArgumentNullException(nameof(DataProvider));
    }

    public event Func<SaveType, Task> OnSave;
    public event Func<Task> OnReload;
    internal Directory Directory { get; private protected set; }

    private string _name;
    private ISaveDataProvider _dataProvider;
    private File _dataFile;
    private Directory _folder;

    public SerializationContext() {
    }

    public SerializationContext(ISaveDataProvider dataProvider) {
      DataProvider = dataProvider;
    }

    public virtual async Task SaveData<TData>(string key, [NotNull] TData data, CancellationToken token = default) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));

      try {
        await DataProvider.SaveData(key, data, token);
      }
      catch (OperationCanceledException) {
        SaveSystem.Logger.LogWarning(Name, "Data saving was canceled");
      }
    }

    public virtual async Task<TData> LoadData<TData>(string key, TData defaultData = default, CancellationToken token = default)
      where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));

      try {
        return await DataProvider.LoadData(key, defaultData, token);
      }
      catch (OperationCanceledException) {
        SaveSystem.Logger.LogWarning(Name, "Data loading was canceled");
        return default;
      }
    }

    public virtual async Task DeleteData<TData>(string key, CancellationToken token = default) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));
      await DataProvider.DeleteData<TData>(key);
    }

    public virtual void RegisterDataSaving<TData>([NotNull] string key, Func<TData> dataReceiver) where TData : ISaveData {
      if (string.IsNullOrEmpty(key))
        throw new ArgumentNullException(nameof(key));
      OnSave += _ => SaveData(key, dataReceiver());
    }

    internal async Task OnSaveInvoke(SaveType saveType) {
      if (OnSave != null)
        await OnSave.Invoke(saveType);
    }

    internal async Task OnReloadInvoke() {
      if (OnReload != null)
        await OnReload.Invoke();
    }

  }

}