using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using SaveSystem.BinaryHandlers;
using SaveSystem.Internal.Extensions;
using SaveSystem.Internal.Templates;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SaveSystem.Security {

    public class AuthenticationManager {

        public HashAlgorithmName Algorithm { get; set; }


        public AuthenticationManager (AuthenticationSettings settings) {
            SetSettings(settings);
        }


        internal AuthenticationManager (HashAlgorithmName algorithm) {
            Algorithm = algorithm;
        }


        public void SetSettings (AuthenticationSettings settings) {
            Algorithm = settings.hashAlgorithm;
        }


        public byte[] AuthenticateData ([NotNull] byte[] bytes) {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(bytes));

            var stream = new MemoryStream(bytes);
            using var reader = new SaveReader(stream);
            byte[] storedHash = reader.ReadArray<byte>();
            byte[] data = reader.ReadArray<byte>();
            byte[] computeHash = ComputeHash(data, Algorithm);
            if (storedHash.Length != computeHash.Length)
                throw new SecurityException(Messages.DataIsCorrupted);

            for (var i = 0; i < storedHash.Length; i++)
                if (storedHash[i] != computeHash[i])
                    throw new SecurityException(Messages.DataIsCorrupted);

            return data;
        }


        public byte[] SetAuthHash ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection", nameof(data));

            var stream = new MemoryStream();

            using (var writer = new SaveWriter(stream)) {
                writer.Write(ComputeHash(data, Algorithm));
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