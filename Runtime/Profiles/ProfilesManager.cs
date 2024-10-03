using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SaveSystemPackage.Exceptions;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Profiles {

    public class ProfilesManager {

        private readonly Dictionary<string, string> m_profilesMap;


        public ProfilesManager (ProfilesManagerData data) {
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
            var profile = new SaveProfile(profileData);
            m_profilesMap.Add(profile.Id, profile.Name);
            var managerData = new ProfilesManagerData {profilesMap = new Dictionary<string, string>(m_profilesMap)};
            Task.Run(async () => {
                await SaveSystem.Game.SaveData(profile.Id, profileData);
                await SaveSystem.Game.SaveData(managerData);
            });
            return profile;
        }


        public async IAsyncEnumerable<SaveProfile> LoadProfiles () {
            foreach (string profileId in m_profilesMap.Keys) {
                var data = await SaveSystem.Game.LoadData<ProfileData>(profileId);
                yield return new SaveProfile(data);
            }
        }


        public void DeleteProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            m_profilesMap.Remove(profile.Id);
            var managerData = new ProfilesManagerData {profilesMap = new Dictionary<string, string>(m_profilesMap)};
            Task.Run(async () => {
                await SaveSystem.Game.DeleteData<ProfileData>(profile.Id);
                await SaveSystem.Game.SaveData(managerData);
                Logger.Log(nameof(ProfilesManager), $"Profile \"{profile}\" deleted");
            });
        }


        internal void UpdateProfile (SaveProfile profile) {
            m_profilesMap[profile.Id] = profile.Name;
            ProfileData profileData = profile.GetData();
            var managerData = new ProfilesManagerData {profilesMap = new Dictionary<string, string>(m_profilesMap)};
            Task.Run(async () => {
                await SaveSystem.Game.SaveData(profile.Id, profileData);
                await SaveSystem.Game.SaveData(managerData);
            });
        }


        internal void ThrowIfProfileExistsWithName (string name) {
            if (m_profilesMap.ContainsValue(name))
                throw new ProfileExistsException($"Profile with name \"{name}\" already exists");
        }

    }

}