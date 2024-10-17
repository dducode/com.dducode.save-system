using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SaveSystemPackage.Exceptions;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Providers;
using SaveSystemPackage.SerializableData;
using SaveSystemPackage.Storages;
using UnityEngine;
using Random = System.Random;

namespace SaveSystemPackage.Profiles {

    public class ProfilesManager {

        private readonly Dictionary<string, string> m_profilesMap;
        private bool m_performed;


        internal ProfilesManager (ProfilesManagerData data) {
            m_profilesMap = data.profilesMap ?? new Dictionary<string, string>();
        }


        public SaveProfile CreateProfile ([NotNull] string name, string iconId = null) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            ThrowIfProfileExistsWithName(name);

            string id;

            do {
                var random = new Random();
                id = $"profile_{random.Next(0, 10_000_000):00000000}";
            } while (m_profilesMap.ContainsKey(id));

            var profileData = new ProfileData {
                id = id,
                name = name,
                iconId = iconId
            };
            m_profilesMap.Add(id, name);
            var managerData = new ProfilesManagerData {profilesMap = new Map<string, string>(m_profilesMap)};
            Task.Run(async () => await SaveData(profileData, managerData));
            return CreateProfileInstance(profileData);
        }


        public async IAsyncEnumerable<SaveProfile> LoadProfiles () {
            foreach (string profileId in m_profilesMap.Keys) {
                var data = await SaveSystem.Game.LoadData<ProfileData>(profileId);
                yield return CreateProfileInstance(data);
            }
        }


        public void DeleteProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            m_profilesMap.Remove(profile.Id);
            var managerData = new ProfilesManagerData {profilesMap = new Map<string, string>(m_profilesMap)};
            Task.Run(async () => await DeleteProfileData(profile, managerData));
        }


        internal void UpdateProfile (SaveProfile profile) {
            m_profilesMap[profile.Id] = profile.Name;
            ProfileData profileData = profile.GetData();
            var managerData = new ProfilesManagerData {profilesMap = new Map<string, string>(m_profilesMap)};
            Task.Run(async () => await SaveData(profileData, managerData));
        }


        internal void ThrowIfProfileExistsWithName (string name) {
            if (m_profilesMap.ContainsValue(name))
                throw new ProfileExistsException($"Profile with name \"{name}\" already exists");
        }


        private SaveProfile CreateProfileInstance (ProfileData profileData) {
            Directory directory = Storage.ProfilesDirectory.CreateDirectory(profileData.id);
            return new SaveProfile(profileData, directory) {
                Serializer = SaveSystem.Settings.SharedSerializer,
                KeyProvider = new KeyDecorator(SaveSystem.Game.KeyProvider, directory.Name),
                DataStorage = new FileSystemStorage(
                    directory,
                    SaveSystem.Settings.SharedSerializer.GetFormatCode(),
                    SaveSystem.Settings.CacheSize
                )
            };
        }


        private async Task SaveData (ProfileData profileData, ProfilesManagerData managerData) {
            while (m_performed)
                await Task.Yield();
            m_performed = true;

            try {
                await SaveSystem.Game.SaveData(profileData.id, profileData);
                await SaveSystem.Game.SaveData(managerData);
            }
            catch (Exception exception) {
                Debug.LogException(exception);
                throw;
            }
            finally {
                m_performed = false;
            }
        }


        private async Task DeleteProfileData (SaveProfile profile, ProfilesManagerData managerData) {
            while (m_performed)
                await Task.Yield();
            m_performed = true;

            try {
                await SaveSystem.Game.DeleteData<ProfileData>(profile.Id);
                await SaveSystem.Game.SaveData(managerData);
                SaveSystem.Logger.Log(nameof(ProfilesManager), $"Profile \"{profile}\" deleted");
            }
            catch (Exception exception) {
                Debug.LogException(exception);
                throw;
            }
            finally {
                m_performed = false;
            }
        }

    }

}