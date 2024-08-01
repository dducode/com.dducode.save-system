using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using SaveSystemPackage.Exceptions;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Serialization;
using ArgumentNullException = System.ArgumentNullException;

namespace SaveSystemPackage {

    public static partial class SaveSystem {

        /// <summary>
        /// Creates new save profile and stores it in the internal storage
        /// </summary>
        public static TProfile CreateProfile<TProfile> (
            [NotNull] string name, bool encrypt = true, bool verify = true
        ) where TProfile : SaveProfile {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var profile = Activator.CreateInstance<TProfile>();
            string formattedName = name.ToPathFormat();

            if (Storage.InternalDirectory.ContainsFile(formattedName))
                throw new ProfileExistsException($"Profile with name \"{name}\" already exists");

            File file = Storage.InternalDirectory.CreateFile(formattedName, "profile");
            using var writer = new SaveWriter(file.Open());
            InitializeProfile(profile, name, encrypt, verify);
            SerializeProfile(writer, profile);
            return profile;
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
        public static IEnumerable<TProfile> LoadProfiles<TProfile> () where TProfile : SaveProfile {
            foreach (File file in Storage.InternalDirectory.EnumerateFiles("profile")) {
                using var reader = new SaveReader(file.Open());
                Type type = typeof(TProfile);

                if (string.Equals(reader.ReadString(), type.AssemblyQualifiedName)) {
                    var profile = Activator.CreateInstance<TProfile>();
                    InitializeProfile(profile, reader.ReadString(), reader.Read<bool>(), reader.Read<bool>());
                    SerializationManager.DeserializeGraph(reader, profile);
                    yield return profile;
                }
            }
        }


        /// <summary>
        /// Removes profile from the internal persistent storage
        /// </summary>
        public static void DeleteProfile ([NotNull] SaveProfile profile) {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            Storage.InternalDirectory.DeleteFile(profile.Name.ToPathFormat());
            profile.DataDirectory.Delete();
            Logger.Log(nameof(SaveSystem), $"Profile \"{profile}\" deleted");
        }


        internal static void UpdateProfile (SaveProfile profile, [NotNull] string oldName, [NotNull] string newName) {
            if (string.IsNullOrEmpty(oldName))
                throw new ArgumentNullException(nameof(oldName));
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentNullException(nameof(newName));
            if (string.Equals(oldName, newName))
                return;

            string formattedName = newName.ToPathFormat();

            Storage.InternalDirectory.GetFile(oldName.ToPathFormat()).Rename(formattedName);
            profile.DataDirectory.Rename(formattedName);
            profile.DataFile.Rename(formattedName);

            if (profile.Settings.VerifyChecksum)
                profile.Settings.VerificationManager.Storage.RenameLink(profile.DataFile);

            UpdateProfile(profile);
        }


        internal static void UpdateProfile (SaveProfile profile) {
            File file = Storage.InternalDirectory.GetFile(profile.Name.ToPathFormat());
            using var writer = new SaveWriter(file.Open());
            SerializeProfile(writer, profile);
        }


        private static void InitializeProfile (SaveProfile profile, string name, bool encrypt, bool verify) {
            string formattedName = name.ToPathFormat();
            profile.Initialize(name, encrypt, verify);
            profile.DataDirectory = Storage.ProfilesDirectory.CreateDirectory(formattedName);
            profile.DataFile = profile.DataDirectory.GetOrCreateFile(formattedName, "profiledata");
        }


        private static void SerializeProfile (SaveWriter writer, SaveProfile profile) {
            Type type = profile.GetType();
            writer.Write(type.AssemblyQualifiedName);
            writer.Write(profile.Name);
            writer.Write(profile.Settings.Encrypt);
            writer.Write(profile.Settings.VerifyChecksum);
            SerializationManager.SerializeGraph(writer, profile);
        }

    }

}