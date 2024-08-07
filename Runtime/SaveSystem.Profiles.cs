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
        public static TProfile CreateProfile<TProfile> ([NotNull] string name) where TProfile : SaveProfile {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var profile = Activator.CreateInstance<TProfile>();
            string formattedName = name.ToPathFormat();

            if (Storage.InternalDirectory.ContainsFile(formattedName))
                throw new ProfileExistsException($"Profile with name \"{name}\" already exists");

            File file = Storage.InternalDirectory.CreateFile(formattedName, "profile");
            using var writer = new SaveWriter(file.Open());
            InitializeProfile(profile, name);
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
                    InitializeProfile(profile, reader.ReadString());
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


        internal static void ThrowIfProfileExistsWithName (string name) {
            if (Storage.InternalDirectory.ContainsFile(name.ToPathFormat()))
                throw new ProfileExistsException($"Profile with name \"{name}\" already exists");
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

            UpdateProfile(profile);
        }


        internal static void UpdateProfile (SaveProfile profile) {
            File file = Storage.InternalDirectory.GetFile(profile.Name.ToPathFormat());
            using var writer = new SaveWriter(file.Open());
            SerializeProfile(writer, profile);
        }


        private static void InitializeProfile (SaveProfile profile, string name) {
            string formattedName = name.ToPathFormat();
            profile.Initialize(name);
            profile.DataDirectory = Storage.ProfilesDirectory.GetOrCreateDirectory(formattedName);
            profile.DataFile = profile.DataDirectory.GetOrCreateFile(formattedName, "profiledata");
        }


        private static void SerializeProfile (SaveWriter writer, SaveProfile profile) {
            Type type = profile.GetType();
            writer.Write(type.AssemblyQualifiedName);
            writer.Write(profile.Name);
            SerializationManager.SerializeGraph(writer, profile);
        }

    }

}