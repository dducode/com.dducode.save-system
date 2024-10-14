using System.Diagnostics.CodeAnalysis;

namespace SaveSystemPackage.Compressing {

    public interface ICompressor {

        public byte[] Compress ([NotNull] byte[] data);
        public byte[] Decompress ([NotNull] byte[] data);

    }

}