using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using SaveSystemPackage.BinaryHandlers;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using ArgumentNullException = System.ArgumentNullException;

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
            using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
            var profile = new SaveProfile(name, encrypt, authenticate);
            writer.Write(profile.Name);
            writer.Write(profile.Settings.Data);
            return profile;
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
        public static IEnumerable<SaveProfile> LoadProfiles () {
            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");

            foreach (string path in paths) {
                using var reader = new SaveReader(File.Open(path, FileMode.Open));
                yield return new SaveProfile(reader.ReadString(), reader.ReadDataBuffer());
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


        internal static void UpdateProfile (SaveProfile profile) {
            string path = Path.Combine(InternalFolder, $"{profile.Name}.profile");
            using var writer = new SaveWriter(File.Open(path, FileMode.Open));
            writer.Write(profile.Name);
            writer.Write(profile.Settings.Data);
        }


        internal static void UpdateProfile (SaveProfile profile, string oldName, string newName) {
            string path = Path.Combine(InternalFolder, $"{newName}.profile");
            if (!string.IsNullOrEmpty(oldName))
                File.Move(Path.Combine(InternalFolder, $"{oldName}.profile"), path);

            using var writer = new SaveWriter(File.Open(path, FileMode.Open));
            writer.Write(newName);
            writer.Write(profile.Settings.Data);
        }

    }

}