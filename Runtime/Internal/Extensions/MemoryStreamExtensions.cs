using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace SaveSystem.Internal.Extensions {

    internal static class MemoryStreamExtensions {

        internal static async UniTask WriteDataToFileAsync (
            this MemoryStream stream, string path, CancellationToken token = default
        ) {
            await File.WriteAllBytesAsync(path, stream.ToArray(), token).AsUniTask();
        }


        internal static async UniTask ReadDataFromFileAsync (
            this MemoryStream stream, string path, CancellationToken token = default
        ) {
            long position = stream.Position;
            await stream.WriteAsync(await File.ReadAllBytesAsync(path, token), token).AsUniTask();
            stream.Position = position;
        }

    }

}