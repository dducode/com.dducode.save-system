using System.IO;
using System.IO.Compression;
using SaveSystemPackage.Internal;

// ReSharper disable UnusedMember.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace SaveSystemPackage.Compressing {

    public class FileCompressor {

        public CompressionLevel CompressionLevel {
            get => m_compressionLevel;
            set {
                m_compressionLevel = value;
                Logger.Log(nameof(FileCompressor), $"Set compression level: {m_compressionLevel}");
            }
        }

        private CompressionLevel m_compressionLevel;


        public FileCompressor (CompressionLevel compressionLevel) {
            m_compressionLevel = compressionLevel;
        }


        internal FileCompressor (CompressionSettings settings) {
            SetSettings(settings);
        }


        internal void SetSettings (CompressionSettings settings) {
            m_compressionLevel = settings.compressionLevel;
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

    }

}