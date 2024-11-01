using System;
using System.IO.Compression;
using SaveSystemPackage.Compressing;

namespace SaveSystemPackage.Settings {

    [Serializable]
    public class CompressionSettings {

        public bool enableInDebug;
        public bool useCustomCompressor;
        public CompressorReference reference;
        public CompressionLevel compressionLevel = CompressionLevel.NoCompression;


        public override string ToString () {
            string arg = useCustomCompressor ? "enabled" : "disabled";
            return $"Use custom compressor: {arg}, compression level: {compressionLevel}";
        }

    }

}