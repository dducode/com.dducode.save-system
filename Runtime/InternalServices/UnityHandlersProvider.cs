using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using SaveSystem.UnityHandlers;

namespace SaveSystem.InternalServices {

    internal static class UnityHandlersProvider {

        internal static UnityWriter GetWriter (string localFilePath) {
            string fullPath = Storage.GetFullPath(localFilePath);
            List<string> directoryPaths = localFilePath.Split(new[] {Path.DirectorySeparatorChar}).ToList();
            directoryPaths.RemoveAt(directoryPaths.Count - 1); // last element is a file, not a folder

            CreateFoldersIfNotExists(directoryPaths);

            return new UnityWriter(new BinaryWriter(new MemoryStream()), fullPath);
        }


        internal static UnityWriter GetWriterRemote () {
            return new UnityWriter(new BinaryWriter(new MemoryStream()));
        }


        [return: MaybeNull]
        internal static UnityReader GetReader (string fileName) {
            string persistentPath = Storage.GetFullPath(fileName);
            return File.Exists(persistentPath)
                ? new UnityReader(new BinaryReader(File.Open(persistentPath, FileMode.Open)))
                : null;
        }


        internal static async UniTask<UnityReader> GetReaderRemote (string url) {
            byte[] data = await Storage.GetDataFromRemote(url);
            return data is not null && data.Length > 0
                ? new UnityReader(new BinaryReader(new MemoryStream(data)))
                : null;
        }


        private static void CreateFoldersIfNotExists (List<string> directoryPaths) {
            var path = "";

            foreach (string directoryPath in directoryPaths) {
                path = Path.Combine(path, directoryPath);
                string fullPath = Storage.GetFullPath(path);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);
            }
        }

    }

}