using System.IO;
using System.IO.Compression;
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


        public virtual byte[] Compress (byte[] data) {
            var stream = new MemoryStream();
            CompressionLevel = CompressionLevel.Optimal;
            using (var compressor = new DeflateStream(stream, compressionLevel))
                compressor.Write(data);
            return stream.ToArray();
        }


        public virtual byte[] Decompress (byte[] data) {
            var buffer = new byte[data.Length * 2];
            using var decompressor = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
            // ReSharper disable once MustUseReturnValue
            decompressor.Read(buffer);
            return buffer;
        }


        internal void SetSettings (CompressionSettings settings) {
            compressionLevel = settings.compressionLevel;
        }

    }

}