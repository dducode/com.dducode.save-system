using System.IO;
using SaveSystem.UnityHandlers;

namespace SaveSystem.Internal {

    internal static class UnityHandlersFactory {

        internal static UnityWriter CreateWriter (string localFilePath) {
            Storage.CreateFoldersIfNotExists(localFilePath);
            string fullPath = Storage.GetFullPath(localFilePath);
            return new UnityWriter(new BinaryWriter(new MemoryStream()), fullPath);
        }


        internal static UnityWriter CreateWriter () {
            return new UnityWriter(new BinaryWriter(new MemoryStream()));
        }


        internal static UnityReader CreateReader (string localFilePath) {
            string fullPath = Storage.GetFullPath(localFilePath);
            return new UnityReader(new BinaryReader(new MemoryStream()), fullPath);
        }


        internal static UnityReader CreateReader () {
            return new UnityReader(new BinaryReader(new MemoryStream()));
        }

    }

}