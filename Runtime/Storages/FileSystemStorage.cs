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


        public async Task Write (string key, byte[] data, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            File file = m_folder.GetOrCreateFile(key, m_fileExtension);
            await file.WriteAllBytesAsync(data, token);
        }


        public async Task<byte[]> Read (string key, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            File file = m_folder.GetOrCreateFile(key, m_fileExtension);
            return await file.ReadAllBytesAsync(token);
        }


        public Task Delete (string key) {
            m_folder.DeleteFile(key);
            return Task.CompletedTask;
        }


        public Task Clear () {
            m_folder.Clear();
            return Task.CompletedTask;
        }


        public Task<bool> Exists (string key) {
            return Task.FromResult(m_folder.ContainsFile(key));
        }

    }

}