using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Providers;
using SaveSystemPackage.Serialization;
using SaveSystemPackage.Storages;
using Directory = SaveSystemPackage.Internal.Directory;
using File = SaveSystemPackage.Internal.File;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    public class SerializationContext : ISerializationContext {

        [NotNull]
        public virtual string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                m_name = value;
            }
        }

        public ISerializer Serializer { get; set; }
        public IKeyProvider KeyProvider { get; set; }
        public IDataStorage DataStorage { get; set; }
        public event Func<SaveType, Task> OnSave;
        public event Func<Task> OnReload;
        internal Directory directory { get; private protected set; }

        private string m_name;
        private File m_dataFile;
        private Directory m_folder;


        public SerializationContext () { }


        public SerializationContext (ISerializer serializer, IKeyProvider keyProvider, IDataStorage dataStorage) {
            Serializer = serializer;
            KeyProvider = keyProvider;
            DataStorage = dataStorage;
        }


        public virtual async Task SaveData<TData> (TData data, CancellationToken token = default)
            where TData : ISaveData {
            try {
                token.ThrowIfCancellationRequested();
                string key = KeyProvider.Provide<TData>();
                byte[] serializedData = Serializer.Serialize(data);
                await DataStorage.Write(key, serializedData, token);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.LogWarning(Name, "Data saving was canceled");
            }
        }


        public virtual async Task SaveData<TData> (string key, TData data, CancellationToken token = default)
            where TData : ISaveData {
            try {
                token.ThrowIfCancellationRequested();
                string resultKey = KeyProvider.Provide<TData>(key);
                byte[] serializedData = Serializer.Serialize(data);
                await DataStorage.Write(resultKey, serializedData, token);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.LogWarning(Name, "Data saving was canceled");
            }
        }


        public virtual async Task<TData> LoadData<TData> (
            TData @default = default, CancellationToken token = default
        ) where TData : ISaveData {
            try {
                token.ThrowIfCancellationRequested();
                string key = KeyProvider.Provide<TData>();
                if (!await DataStorage.Exists(key))
                    return @default;
                byte[] data = await DataStorage.Read(key, token);
                return Serializer.Deserialize<TData>(data);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.LogWarning(Name, "Data loading was canceled");
                return @default;
            }
        }


        public virtual async Task<TData> LoadData<TData> (
            string key, TData @default = default, CancellationToken token = default
        ) where TData : ISaveData {
            try {
                token.ThrowIfCancellationRequested();
                string resultKey = KeyProvider.Provide<TData>(key);
                if (!await DataStorage.Exists(resultKey))
                    return @default;
                byte[] data = await DataStorage.Read(resultKey, token);
                return Serializer.Deserialize<TData>(data);
            }
            catch (OperationCanceledException) {
                SaveSystem.Logger.LogWarning(Name, "Data loading was canceled");
                return @default;
            }
        }


        public virtual async Task DeleteData<TData> () where TData : ISaveData {
            string key = KeyProvider.Provide<TData>();
            await DataStorage.Delete(key);
        }


        public virtual async Task DeleteData<TData> (string key) where TData : ISaveData {
            string resultKey = KeyProvider.Provide<TData>(key);
            await DataStorage.Delete(resultKey);
        }


        internal async Task OnSaveInvoke (SaveType saveType) {
            if (OnSave != null)
                await OnSave.Invoke(saveType);
        }


        internal async Task OnReloadInvoke () {
            if (OnReload != null)
                await OnReload.Invoke();
        }

    }

}