using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
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


        public virtual async Task<byte[]> Compress (byte[] data, CancellationToken token = default) {
            var stream = new MemoryStream();
            await using (var compressor = new DeflateStream(stream, compressionLevel))
                await compressor.WriteAsync(data, token);
            return stream.ToArray();
        }


        public virtual async Task<byte[]> Decompress (byte[] data, CancellationToken token = default) {
            var stream = new MemoryStream();
            await using (var decompressor = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress)) 
                await decompressor.CopyToAsync(stream, token);
            return stream.ToArray();
        }


        internal void SetSettings (CompressionSettings settings) {
            compressionLevel = settings.compressionLevel;
        }

    }

}