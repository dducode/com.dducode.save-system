using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BinaryReader = SaveSystem.BinaryHandlers.BinaryReader;
using BinaryWriter = SaveSystem.BinaryHandlers.BinaryWriter;

#if SAVE_SYSTEM_UNITASK_SUPPORT
using TaskResult = Cysharp.Threading.Tasks.UniTask<SaveSystem.HandlingResult>;

#else
using TaskResult = System.Threading.Tasks.Task<SaveSystem.HandlingResult>;
#endif

namespace SaveSystem {

    public class SaveProfile : IRuntimeSerializable {

        public string Name { get; private set; }
        public string IconKey { get; private set; }
        public string ProfilePath { get; private set; }

        private readonly List<IRuntimeSerializable> m_serializables = new();

        public bool DontDestroyOnSceneUnload => false;


        public SaveProfile () { }


        public SaveProfile (string name, string iconKey, string profilePath) {
            Name = name;
            IconKey = iconKey;
            ProfilePath = profilePath;
        }


        public void Add (IRuntimeSerializable serializable) {
            m_serializables.Add(serializable);
        }


        public void AddRange (IEnumerable<IRuntimeSerializable> serializables) {
            m_serializables.AddRange(serializables);
        }


        public async TaskResult Save (CancellationToken token) {
            try {
                token.ThrowIfCancellationRequested();
                using var writer = new BinaryWriter(new MemoryStream());

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Serialize(writer);

                await writer.WriteDataToFileAsync(ProfilePath, token);
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        public async TaskResult Load (CancellationToken token) {
            if (!File.Exists(ProfilePath))
                return HandlingResult.FileNotExists;

            try {
                using var reader = new BinaryReader(new MemoryStream());
                await reader.ReadDataFromFileAsync(ProfilePath, token);

                foreach (IRuntimeSerializable serializable in m_serializables)
                    serializable.Deserialize(reader);
                return HandlingResult.Success;
            }
            catch (OperationCanceledException) {
                return HandlingResult.Canceled;
            }
        }


        public void Serialize (BinaryWriter writer) {
            writer.Write(Name);
            writer.Write(IconKey);
            writer.Write(ProfilePath);
        }


        public void Deserialize (BinaryReader reader) {
            Name = reader.ReadString();
            IconKey = reader.ReadString();
            ProfilePath = reader.ReadString();
        }


        public void ClearObjectGroups () {
            var savedGroups = new List<IRuntimeSerializable>();

            foreach (IRuntimeSerializable objectGroup in m_serializables) {
                if (objectGroup.DontDestroyOnSceneUnload)
                    savedGroups.Add(objectGroup);
            }

            m_serializables.Clear();
            foreach (IRuntimeSerializable objectGroup in savedGroups)
                m_serializables.Add(objectGroup);
        }

    }

}