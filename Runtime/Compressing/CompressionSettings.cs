using System;
using System.IO.Compression;

namespace SaveSystemPackage.Compressing {

    [Serializable]
    public class CompressionSettings {

        public bool useCustomCompressor;
        public FileCompressorReference reference;
        public CompressionLevel compressionLevel = CompressionLevel.NoCompression;


        public override string ToString () {
            string arg = useCustomCompressor ? "enabled" : "disabled";
            return $"Use custom compressor: {arg}, compression level: {compressionLevel}";
        }

    }

}