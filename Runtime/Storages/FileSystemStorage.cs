using System.Threading;
using System.Threading.Tasks;
using Directory = SaveSystemPackage.Internal.Directory;
using File = SaveSystemPackage.Internal.File;

namespace SaveSystemPackage.Storages {

    public class FileSystemStorage : IDataStorage {

        private readonly Directory m_folder;
        private readonly string m_fileExtension;


        internal FileSystemStorage (Directory directory, string fileExtension) {
            m_folder = directory;
            m_fileExtension = fileExtension;
        }


        public async Task WriteData (string key, byte[] data, CancellationToken token) {
            File file = m_folder.GetOrCreateFile(key, m_fileExtension);
            await file.WriteAllBytesAsync(data, token);
        }


        public async Task<byte[]> ReadData (string key, CancellationToken token) {
            File file = m_folder.GetOrCreateFile(key, m_fileExtension);
            return await file.ReadAllBytesAsync(token);
        }

    }

}