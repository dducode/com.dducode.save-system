using System.Diagnostics.CodeAnalysis;
using System.IO;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Internal {

    internal static class UnityHandlersFactory {

        /// <summary>
        /// Creates <see cref="UnityWriter"/> with memory stream
        /// </summary>
        internal static UnityWriter CreateBufferingWriter (string filePath) {
            filePath = PathPreparing.PrepareBeforeWriting(filePath);
            return new UnityWriter(new BinaryWriter(new MemoryStream()), filePath);
        }


        /// <summary>
        /// Creates <see cref="UnityWriter"/> with file stream
        /// </summary>
        internal static UnityWriter CreateDirectWriter (string filePath) {
            filePath = PathPreparing.PrepareBeforeWriting(filePath);
            return new UnityWriter(
                new BinaryWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write)), filePath
            );
        }


        /// <summary>
        /// Creates <see cref="UnityReader"/> with memory stream
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        internal static UnityReader CreateBufferingReader (string filePath) {
            filePath = Storage.GetFullPath(filePath);
            return new UnityReader(new BinaryReader(new MemoryStream()), filePath);
        }


        /// <summary>
        /// Creates <see cref="UnityReader"/> with file stream
        /// </summary>
        /// <returns> If file doesn't exist by path, returns null, otherwise new reader </returns>
        [return: MaybeNull]
        internal static UnityReader CreateDirectReader (string filePath) {
            if (!File.Exists(Storage.GetFullPath(filePath)))
                return null;

            filePath = Storage.GetFullPath(filePath);
            return new UnityReader(
                new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)), filePath
            );
        }

    }

}