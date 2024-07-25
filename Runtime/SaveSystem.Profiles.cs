using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Profiles;

namespace SaveSystemPackage {

    public static partial class SaveSystem {

        /// <summary>
        /// Creates new save profile and stores it in the internal storage
        /// </summary>
        public static SaveProfile CreateProfile (
            [NotNull] string name, bool encrypt = true, bool authenticate = true
        ) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            string path = Path.Combine(InternalFolder, $"{name.ToPathFormat()}.profile");
            var profile = new SaveProfile(name, encrypt, authenticate);
            using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
            writer.Write(name);
            writer.Write(encrypt);
            writer.Write(authenticate);
            return profile;
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
        public static async IAsyncEnumerable<SaveProfile> LoadProfiles () {
            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");

            foreach (string path in paths) {
                await using var reader = new SaveReader(File.Open(path, FileMode.Open));
                var profile = new SaveProfile(
                    reader.ReadString(), reader.Read<bool>(), reader.Read<bool>()
                );
                await profile.Load();
                yield return profile;
            }
        }


        /// <summary>
        /// Get saving profile by its name
        /// </summary>
        [Pure]
        public static async UniTask<SaveProfile> LoadProfile ([NotNull] string name) {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");

            foreach (string path in paths) {
                await using var reader = new SaveReader(File.Open(path, FileMode.Open));

                if (string.Equals(reader.ReadString(), name)) {
                    var profile = new SaveProfile(
                        name, reader.Read<bool>(), reader.Read<bool>()
                    );
                    await profile.Load();
                    return profile;
                }
            }

            return null;
        }


        [Pure]
        public static IEnumerable<SaveProfileRef> GetProfileRefs () {
            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");

            foreach (string path in paths) {
                using var reader = new SaveReader(File.Open(path, FileMode.Open));
                yield return new SaveProfileRef(reader.ReadString());
            }
        }


        /// <summary>
        /// Removes profile from the internal persistent storage
        /// </summary>
        public static void DeleteProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string path = Path.Combine(InternalFolder, $"{profile.Name.ToPathFormat()}.profile");
            if (!File.Exists(path))
                return;

            File.Delete(path);
            Directory.Delete(profile.DataFolder, true);
            Logger.Log(nameof(SaveSystem), $"Profile <b>{profile}</b> deleted");
        }

    }

}