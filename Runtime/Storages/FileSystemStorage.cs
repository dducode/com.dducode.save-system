using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SaveSystemPackage.Internal;

namespace SaveSystemPackage.Storages {

    public class FileSystemStorage : IDataStorage {

        private readonly Directory m_folder;
        private readonly IDataStorage m_cacheStorage;
        private readonly string m_fileExtension;


        public FileSystemStorage (string folderPath, string fileExtension, int cacheCapacity = 64) {
            m_folder = Storage.CreateDirectory(folderPath);
            m_cacheStorage = new MemoryStorage(cacheCapacity);
            m_fileExtension = fileExtension;
        }


        internal FileSystemStorage (Directory directory, string fileExtension, int cacheCapacity = 64) {
            m_folder = directory;
            m_cacheStorage = new MemoryStorage(cacheCapacity);
            m_fileExtension = fileExtension;
        }


        public async Task Write ([NotNull] string key, byte[] data, CancellationToken token) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (data == null || data.Length == 0)
                return;

            token.ThrowIfCancellationRequested();
            File file = m_folder.GetOrCreateFile(key, m_fileExtension);
            await file.WriteAllBytesAsync(data, token);
            if (await m_cacheStorage.Exists(key))
                await m_cacheStorage.Write(key, data, token);
        }


        public async Task<byte[]> Read ([NotNull] string key, CancellationToken token) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
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


        public async Task Delete ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            await m_cacheStorage.Delete(key);
            m_folder.DeleteFile(key);
        }


        public async Task Clear () {
            await m_cacheStorage.Clear();
            m_folder.Clear();
        }


        public async Task<bool> Exists ([NotNull] string key) {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            return await m_cacheStorage.Exists(key) || m_folder.ContainsFile(key);
        }

    }

}