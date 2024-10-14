using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using SaveSystemPackage.Settings;
using CompressionLevel = System.IO.Compression.CompressionLevel;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage.Compressing {

    public sealed class DeflateCompressor : ICompressor {

        public CompressionLevel CompressionLevel {
            get => m_compressionLevel;
            set {
                m_compressionLevel = value;
                SaveSystem.Logger.Log(nameof(DeflateCompressor), $"Set compression level: {m_compressionLevel}");
            }
        }

        private CompressionLevel m_compressionLevel;


        public DeflateCompressor (CompressionLevel compressionLevel) {
            m_compressionLevel = compressionLevel;
        }


        internal DeflateCompressor (CompressionSettings settings) {
            SetSettings(settings);
        }


        public byte[] Compress ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var memoryStream = new MemoryStream();
            using (var compressor = new DeflateStream(memoryStream, m_compressionLevel))
                compressor.Write(data);
            return memoryStream.ToArray();
        }


        public byte[] Decompress ([NotNull] byte[] data) {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var memoryStream = new MemoryStream();
            using (var compressor = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
                compressor.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }


        internal void SetSettings (CompressionSettings settings) {
            m_compressionLevel = settings.compressionLevel;
        }

    }

}