using System.IO;
using System.IO.Compression;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Logger = SaveSystemPackage.Internal.Logger;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace SaveSystemPackage.Compressing {

    public class FileCompressor : ScriptableObject {

        public CompressionLevel CompressionLevel {
            get => m_compressionLevel;
            set {
                m_compressionLevel = value;
                Logger.Log(nameof(FileCompressor), $"Set compression level: {m_compressionLevel}");
            }
        }

        private CompressionLevel m_compressionLevel;


        public static FileCompressor CreateInstance (CompressionLevel compressionLevel) {
            var fileCompressor = ScriptableObject.CreateInstance<FileCompressor>();
            fileCompressor.m_compressionLevel = compressionLevel;
            return fileCompressor;
        }


        internal static FileCompressor CreateInstance (CompressionSettings settings) {
            var fileCompressor = ScriptableObject.CreateInstance<FileCompressor>();
            fileCompressor.SetSettings(settings);
            return fileCompressor;
        }


        public FileCompressor (CompressionLevel compressionLevel) {
            m_compressionLevel = compressionLevel;
        }


        internal FileCompressor (CompressionSettings settings) {
            SetSettings(settings);
        }


        public virtual byte[] Compress (byte[] data) {
            var stream = new MemoryStream();
            CompressionLevel = CompressionLevel.Optimal;
            using (var compressor = new DeflateStream(stream, m_compressionLevel))
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
            m_compressionLevel = settings.compressionLevel;
        }

    }

}