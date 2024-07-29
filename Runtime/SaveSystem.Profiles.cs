﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
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

            string path = Path.Combine(InternalFolder, $"{name.ToPathFormat()}.profile");
            using var writer = new SaveWriter(File.Open(path, FileMode.OpenOrCreate));
            var profile = Activator.CreateInstance<TProfile>();
            profile.Initialize(name, encrypt, verify);
            SerializeProfile(writer, profile);
            return profile;
        }


        /// <summary>
        /// Get all previously created saving profiles
        /// </summary>
        [Pure]
        public static IEnumerable<TProfile> LoadProfiles<TProfile> () where TProfile : SaveProfile {
            string[] paths = Directory.GetFileSystemEntries(InternalFolder, "*.profile");

            foreach (string path in paths) {
                using var reader = new SaveReader(File.Open(path, FileMode.Open));
                Type type = typeof(TProfile);

                if (string.Equals(reader.ReadString(), type.AssemblyQualifiedName)) {
                    var profile = Activator.CreateInstance<TProfile>();
                    profile.Initialize(reader.ReadString(), reader.Read<bool>(), reader.Read<bool>());
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
            SerializeProfile(writer, profile);
        }


        internal static void UpdateProfile (SaveProfile profile, string oldName, string newName) {
            string path = Path.Combine(InternalFolder, $"{newName}.profile");
            if (!string.IsNullOrEmpty(oldName))
                File.Move(Path.Combine(InternalFolder, $"{oldName}.profile"), path);

            using var writer = new SaveWriter(File.Open(path, FileMode.Open));
            SerializeProfile(writer, profile);
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