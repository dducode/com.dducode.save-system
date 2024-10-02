using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Security;
using SaveSystemPackage.Storages;
using Directory = SaveSystemPackage.Internal.Directory;
using File = SaveSystemPackage.Internal.File;

// ReSharper disable UnusedMember.Global
// ReSharper disable SuspiciousTypeConversion.Global

namespace SaveSystemPackage {

    public class SerializationScope {

        [NotNull]
        public virtual string Name {
            get => m_name;
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(Name));

                m_name = value;
            }
        }

        public IDataStorage DataStorage { get; set; }

        public SerializationSettings OverriddenSettings =>
            m_overridenSettings ??= SaveSystem.Settings.SerializationSettings.Clone();

        public DataBuffer Data { get; private set; } = new();
        public SecureDataBuffer SecureData { get; private set; } = new();
        public event Func<SaveType, Task> OnSave;
        public event Func<Task> OnReload;

        private string m_name;
        private File m_dataFile;
        private Directory m_folder;
        private SerializationSettings m_overridenSettings;


        public async Task SaveData<TData> (TData data, CancellationToken token = default) where TData : ISaveData {
            SerializationSettings settings = m_overridenSettings ?? SaveSystem.Settings.SerializationSettings;
            string key = settings.KeyProvider.GetKey<TData>();
            byte[] serializedData = await settings.Serializer.Serialize(data, token);
            await DataStorage.WriteData(key, serializedData, token);
        }


        public async Task SaveData<TData> (string key, TData data, CancellationToken token = default)
            where TData : ISaveData {
            SerializationSettings settings = m_overridenSettings ?? SaveSystem.Settings.SerializationSettings;
            string typeKey = settings.KeyProvider.GetKey<TData>();
            byte[] serializedData = await settings.Serializer.Serialize(data, token);
            await DataStorage.WriteData($"{key}_{typeKey}", serializedData, token);
        }


        public async Task<TData> LoadData<TData> (CancellationToken token = default) where TData : ISaveData {
            SerializationSettings settings = m_overridenSettings ?? SaveSystem.Settings.SerializationSettings;
            string key = settings.KeyProvider.GetKey<TData>();
            byte[] data = await DataStorage.ReadData(key, token);
            return await settings.Serializer.Deserialize<TData>(data, token);
        }


        public async Task<TData> LoadData<TData> (string key, CancellationToken token = default)
            where TData : ISaveData {
            SerializationSettings settings = m_overridenSettings ?? SaveSystem.Settings.SerializationSettings;
            string typeKey = settings.KeyProvider.GetKey<TData>();
            byte[] data = await DataStorage.ReadData($"{key}_{typeKey}", token);
            return await settings.Serializer.Deserialize<TData>(data, token);
        }


        internal async Task OnSaveInvoke (SaveType saveType) {
            if (OnSave != null)
                await OnSave.Invoke(saveType);
        }


        internal async Task OnReloadInvoke () {
            if (OnReload != null)
                await OnReload.Invoke();
        }


        internal void Clear () {
            Data.Clear();
            SecureData.Clear();
        }

    }

}