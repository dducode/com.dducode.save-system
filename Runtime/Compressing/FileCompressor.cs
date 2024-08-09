using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using File = SaveSystemPackage.Internal.File;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace SaveSystemPackage.Compressing {

    public class FileCompressor : ScriptableObject, ICloneable<FileCompressor> {

        public CompressionLevel CompressionLevel {
            get => compressionLevel;
            set {
                compressionLevel = value;
                Logger.Log(nameof(FileCompressor), $"Set compression level: {compressionLevel}");
            }
        }

        protected CompressionLevel compressionLevel;


        public static TCompressor CreateInstance<TCompressor> (CompressionLevel compressionLevel)
            where TCompressor : FileCompressor {
            var fileCompressor = ScriptableObject.CreateInstance<TCompressor>();
            fileCompressor.compressionLevel = compressionLevel;
            return fileCompressor;
        }


        internal static FileCompressor CreateInstance (CompressionSettings settings) {
            var fileCompressor = ScriptableObject.CreateInstance<FileCompressor>();
            fileCompressor.SetSettings(settings);
            return fileCompressor;
        }


        public virtual FileCompressor Clone () {
            return CreateInstance<FileCompressor>(compressionLevel);
        }


        public virtual async Task Compress (Stream stream, CancellationToken token = default) {
            stream.Position = 0;
            File cacheFile = Storage.CacheRoot.CreateFile("compress", "temp");

            try {
                await using FileStream cacheStream = cacheFile.Open();
                await using (var compressor = new DeflateStream(cacheStream, compressionLevel, true)) 
                    await stream.CopyToAsync(compressor, token);
                stream.SetLength(0);
                cacheStream.Position = 0;
                await cacheStream.CopyToAsync(stream, token);
            }
            finally {
                cacheFile.Delete();
                stream.Position = 0;
            }
        }


        public virtual async Task Decompress (Stream stream, CancellationToken token = default) {
            stream.Position = 0;
            File cacheFile = Storage.CacheRoot.CreateFile("decompress", "temp");

            try {
                await using FileStream cacheStream = cacheFile.Open();
                await stream.CopyToAsync(cacheStream, token);
                stream.SetLength(0);
                cacheStream.Position = 0;
                await using var decompressor = new DeflateStream(cacheStream, CompressionMode.Decompress);
                await decompressor.CopyToAsync(stream, token);
            }
            finally {
                cacheFile.Delete();
                stream.Position = 0;
            }
        }


        internal void SetSettings (CompressionSettings settings) {
            compressionLevel = settings.compressionLevel;
        }

    }

}