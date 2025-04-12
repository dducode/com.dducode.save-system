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

    public virtual async Task SaveData<TData>([NotNull] TData data, string key = null, CancellationToken token = default) where TData : ISaveData {
      try {
        await DataProvider.SaveData(data, key, token);
      }
      catch (OperationCanceledException) {
        SaveSystem.Logger.LogWarning(Name, "Data saving was canceled");
      }
    }

    public virtual async Task<TData> LoadData<TData>(string key = null, CancellationToken token = default) where TData : ISaveData {
      try {
        return await DataProvider.LoadData<TData>(key, token);
      }
      catch (OperationCanceledException) {
        SaveSystem.Logger.LogWarning(Name, "Data loading was canceled");
        return default;
      }
    }

    public virtual async Task DeleteData<TData>(string key = null, CancellationToken token = default) where TData : ISaveData {
      await DataProvider.DeleteData<TData>(key);
    }

    public virtual void RegisterDataSaving<TData>(Func<TData> dataReceiver, string key = null) where TData : ISaveData {
      OnSave += _ => SaveData(dataReceiver(), key);
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