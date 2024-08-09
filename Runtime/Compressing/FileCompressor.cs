﻿using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global

namespace SaveSystemPackage.Compressing {

    public class FileCompressor : ICloneable<FileCompressor> {

        public CompressionLevel CompressionLevel {
            get => compressionLevel;
            set {
                compressionLevel = value;
                Logger.Log(nameof(FileCompressor), $"Set compression level: {compressionLevel}");
            }
        }

        protected CompressionLevel compressionLevel;


        public FileCompressor (CompressionLevel compressionLevel) {
            this.compressionLevel = compressionLevel;
        }


        internal FileCompressor (CompressionSettings settings) {
            SetSettings(settings);
        }


        public virtual FileCompressor Clone () {
            return new FileCompressor(compressionLevel);
        }


        public virtual async Task Compress (Stream stream, CancellationToken token = default) {
            stream.Position = 0;

            try {
                using TempFile cacheFile = Storage.CacheRoot.CreateTempFile("compress");
                await using FileStream cacheStream = cacheFile.Open();
                await using (var compressor = new DeflateStream(cacheStream, compressionLevel, true))
                    await stream.CopyToAsync(compressor, token);
                stream.SetLength(0);
                cacheStream.Position = 0;
                await cacheStream.CopyToAsync(stream, token);
            }
            finally {
                stream.Position = 0;
            }
        }


        public virtual async Task Decompress (Stream stream, CancellationToken token = default) {
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
            compressionLevel = settings.compressionLevel;
        }

    }

}