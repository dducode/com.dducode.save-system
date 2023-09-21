using System.IO;
using SaveSystem.UnityHandlers;

namespace SaveSystem.InternalServices {

    internal static class UnityHandlersProvider {

        internal static UnityWriter GetWriter (string localFilePath) {
            Storage.CreateFoldersIfNotExists(localFilePath);
            string fullPath = Storage.GetFullPath(localFilePath);
            return new UnityWriter(new BinaryWriter(new MemoryStream()), fullPath);
        }


        internal static UnityWriter GetWriter () {
            return new UnityWriter(new BinaryWriter(new MemoryStream()));
        }


        internal static UnityReader GetReader (string localFilePath) {
            string fullPath = Storage.GetFullPath(localFilePath);
            return new UnityReader(new BinaryReader(new MemoryStream()), fullPath);
        }


        internal static UnityReader GetReader () {
            return new UnityReader(new BinaryReader(new MemoryStream()));
        }

    }

}