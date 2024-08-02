using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Internal.Templates;
using SaveSystemPackage.Serialization;
using HashAlgorithmName = SaveSystemPackage.Security.HashAlgorithmName;
using MemoryStream = System.IO.MemoryStream;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SaveSystemPackage.Verification {

    public sealed class VerificationManager {

        public HashAlgorithmName Algorithm { get; set; }
        public HashStorage Storage { get; set; }


        internal VerificationManager (VerificationSettings settings) {
            SetSettings(settings);
        }


        internal VerificationManager (HashStorage storage, HashAlgorithmName algorithm) {
            Storage = storage;
            Algorithm = algorithm;
        }


        internal void SetSettings (VerificationSettings settings) {
            if (!settings.useCustomStorage)
                Storage = new DefaultHashStorage();
            Algorithm = settings.hashAlgorithm;
        }


        internal async UniTask<byte[]> VerifyData ([NotNull] byte[] bytes) {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(bytes));

            string id;
            byte[] data;

            await using (var reader = new SaveReader(new MemoryStream(bytes))) {
                id = reader.ReadString();
                data = reader.ReadArray<byte>();
            }

            await Storage.Open();
            byte[] storedHash = Storage[id];
            byte[] computedHash = ComputeHash(data, Algorithm);
            await Storage.Close();

            if (!storedHash.EqualsBytes(computedHash))
                throw new SecurityException(Messages.DataIsCorrupted);
            return data;
        }


        internal async UniTask<byte[]> SetChecksum ([NotNull] string id, [NotNull] byte[] data) {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            await Storage.Open();
            Storage[id] = ComputeHash(data, Algorithm);
            await Storage.Close();

            var stream = new MemoryStream();

            await using (var writer = new SaveWriter(stream)) {
                writer.Write(id);
                writer.Write(data);
            }

            return stream.ToArray();
        }


        private byte[] ComputeHash (byte[] data, HashAlgorithmName algorithmName) {
            HashAlgorithm algorithm = algorithmName.SelectAlgorithm();
            return algorithm.ComputeHash(data);
        }

    }

}