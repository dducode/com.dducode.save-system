using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemPackage.Compressing {

    public interface ICompressor {

        public byte[] Compress ([NotNull] byte[] data);
        public Task Compress (Stream stream, CancellationToken token = default);
        public byte[] Decompress ([NotNull] byte[] data);
        public Task Decompress (Stream stream, CancellationToken token = default);

    }

}