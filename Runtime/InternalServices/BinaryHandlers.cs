using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace SaveSystem.InternalServices {

    internal static class BinaryHandlers {

        internal static BinaryWriter GetBinaryWriter (string localFilePath, out string fullPath) {
            fullPath = Storage.GetFullPath(localFilePath);
            List<string> directoryPaths = localFilePath.Split(new[] {Path.DirectorySeparatorChar}).ToList();
            directoryPaths.RemoveAt(directoryPaths.Count - 1); // last element is a file, not a folder

            CreateFoldersIfNotExists(directoryPaths);

            return new BinaryWriter(File.Open(fullPath, FileMode.Create));
        }


        internal static BinaryWriter GetBinaryWriterRemote (out string tempPath) {
            tempPath = Storage.GetCachePath();
            return new BinaryWriter(File.Open(tempPath, FileMode.Create));
        }


        [return: MaybeNull]
        internal static BinaryReader GetBinaryReader (string fileName) {
            string persistentPath = Storage.GetFullPath(fileName);
            return File.Exists(persistentPath) ? new BinaryReader(File.Open(persistentPath, FileMode.Open)) : null;
        }


        internal static async UniTask<BinaryReader> GetBinaryReaderRemote (string url) {
            byte[] data = await Storage.GetDataFromRemote(url);
            return data is not null && data.Length > 0 ? new BinaryReader(new MemoryStream(data)) : null;
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