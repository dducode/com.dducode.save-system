using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Extensions;
using SaveSystemPackage.Internal.Templates;
using HashAlgorithmName = SaveSystemPackage.Security.HashAlgorithmName;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SaveSystemPackage.Verification {

    public class VerificationManager {

        public HashAlgorithmName Algorithm { get; set; }
        public HashStorage Storage { get; set; }


        public VerificationManager (VerificationSettings settings) {
            SetSettings(settings);
        }


        internal VerificationManager (HashStorage storage, HashAlgorithmName algorithm) {
            Storage = storage;
            Algorithm = algorithm;
        }


        public void SetSettings (VerificationSettings settings) {
            if (!settings.useCustomStorage)
                Storage = new DefaultHashStorage();
            Algorithm = settings.hashAlgorithm;
        }


        public virtual async UniTask VerifyData ([NotNull] File file, [NotNull] byte[] data) {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            await Storage.Open();
            byte[] storedHash = Storage.Get(file);
            byte[] computedHash = ComputeHash(data, Algorithm);
            if (!storedHash.EqualsBytes(computedHash))
                throw new SecurityException(Messages.DataIsCorrupted);
            await Storage.Close();
        }


        public virtual async UniTask SetChecksum ([NotNull] File file, [NotNull] byte[] data) {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            await Storage.Open();
            Storage.Add(file, ComputeHash(data, Algorithm));
            await Storage.Close();
        }


        private byte[] ComputeHash (byte[] data, HashAlgorithmName algorithmName) {
            HashAlgorithm algorithm = algorithmName.SelectAlgorithm();
            return algorithm.ComputeHash(data);
        }

    }

}