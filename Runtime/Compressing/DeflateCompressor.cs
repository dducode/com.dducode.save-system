using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Settings;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage.Compressing {

    public sealed class DeflateCompressor : ICompressor {

        public CompressionLevel CompressionLevel {
            get => m_compressionLevel;
            set {
                m_compressionLevel = value;
                Logger.Log(nameof(DeflateCompressor), $"Set compression level: {m_compressionLevel}");
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


        public async Task Compress (Stream stream, CancellationToken token = default) {
            stream.Position = 0;

            try {
                using TempFile cacheFile = Storage.CacheRoot.CreateTempFile("compress");
                await using FileStream cacheStream = cacheFile.Open();
                await using (var compressor = new DeflateStream(cacheStream, m_compressionLevel, true))
                    await stream.CopyToAsync(compressor, token);
                stream.SetLength(0);
                cacheStream.Position = 0;
                await cacheStream.CopyToAsync(stream, token);
            }
            finally {
                stream.Position = 0;
            }
        }


        public async Task Decompress (Stream stream, CancellationToken token = default) {
            stream.Position = 0;

            try {
                using TempFile cacheFile = Storage.CacheRoot.CreateTempFile("decompress");
                await using FileStream cacheStream = cacheFile.Open();
                await stream.CopyToAsync(cacheStream, token);
                stream.SetLength(0);
                cacheStream.Position = 0;
                await using var decompressor = new DeflateStream(cacheStream, CompressionMode.Decompress);
                await decompressor.CopyToAsync(stream, token);
            }
            finally {
                stream.Position = 0;
            }
        }


        internal void SetSettings (CompressionSettings settings) {
            m_compressionLevel = settings.compressionLevel;
        }

    }

}