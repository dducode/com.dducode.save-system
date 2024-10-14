using System.Threading;
using System.Threading.Tasks;
using Directory = SaveSystemPackage.Internal.Directory;
using File = SaveSystemPackage.Internal.File;

namespace SaveSystemPackage.Storages {

    public class FileSystemStorage : IDataStorage {

        private readonly Directory m_folder;
        private readonly IDataStorage m_cacheStorage;
        private readonly string m_fileExtension;


        internal FileSystemStorage (Directory directory, string fileExtension, int cacheCapacity = 4096) {
            m_folder = directory;
            m_cacheStorage = new MemoryStorage(cacheCapacity);
            m_fileExtension = fileExtension;
        }


        public async Task Write (string key, byte[] data, CancellationToken token) {
            token.ThrowIfCancellationRequested();
            File file = m_folder.GetOrCreateFile(key, m_fileExtension);
            await file.WriteAllBytesAsync(data, token);
            if (await m_cacheStorage.Exists(key))
                await m_cacheStorage.Write(key, data, token);
        }


        public async Task<byte[]> Read (string key, CancellationToken token) {
            token.ThrowIfCancellationRequested();

            if (await m_cacheStorage.Exists(key)) {
                return await m_cacheStorage.Read(key, token);
            }
            else {
                File file = m_folder.GetOrCreateFile(key, m_fileExtension);
                byte[] bytes = await file.ReadAllBytesAsync(token);
                await m_cacheStorage.Write(key, bytes, token);
                return bytes;
            }
        }


        public async Task Delete (string key) {
            await m_cacheStorage.Delete(key);
            m_folder.DeleteFile(key);
        }


        public async Task Clear () {
            await m_cacheStorage.Clear();
            m_folder.Clear();
        }


        public async Task<bool> Exists (string key) {
            return await m_cacheStorage.Exists(key) || m_folder.ContainsFile(key);
        }

    }

}