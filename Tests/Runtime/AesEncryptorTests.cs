#if IN_UNITY_PACKAGES_PROJECT
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.Internal;
using SaveSystemPackage.Internal.Security;
using SaveSystemPackage.Security;
using UnityEditor;
using UnityEngine;
using File = System.IO.File;

namespace SaveSystemPackage.Tests {

    public class AesEncryptorTests {

        private readonly string m_sourcePath =
            Path.Combine(Application.dataPath, @"com.dducode.save-system\Tests\Runtime\TestResources\LoremIpsum.txt");

        private readonly Internal.File m_encryptedFile =
            Storage.TestsDirectory.GetOrCreateFile("LoremIpsum_encrypted", "txt");

        private readonly Internal.File m_decryptedFile =
            Storage.TestsDirectory.GetOrCreateFile("LoremIpsum_decrypted", "txt");


        [Test, Order(0)]
        public async Task EncryptLoremIpsum () {
            var cryptographer = new AesEncryptor(
                new SecurityKeyProvider("password"),
                new SecurityKeyProvider("salt"),
                KeyGenerationParams.Default
            );

            using TempFile cacheFile = Storage.CacheRoot.CreateTempFile("test-encrypt");

            await using (FileStream cacheStream = cacheFile.Open()) {
                await using FileStream stream = File.Open(m_sourcePath, FileMode.Open);
                await using FileStream targetStream = m_encryptedFile.Open();
                await stream.CopyToAsync(cacheStream);
                await cryptographer.Encrypt(cacheStream);
                await cacheStream.CopyToAsync(targetStream);
            }

            EditorUtility.RevealInFinder(m_encryptedFile.Path);
        }


        [Test, Order(1)]
        public async Task DecryptLoremIpsum () {
            var cryptographer = new AesEncryptor(
                new SecurityKeyProvider("password"),
                new SecurityKeyProvider("salt"),
                KeyGenerationParams.Default
            );

            using TempFile cacheFile = Storage.CacheRoot.CreateTempFile("test-decrypt");

            await using (FileStream cacheStream = cacheFile.Open()) {
                await using (FileStream stream = m_encryptedFile.Open())
                    await stream.CopyToAsync(cacheStream);

                await cryptographer.Decrypt(cacheStream);

                await using (FileStream targetStream = m_decryptedFile.Open())
                    await cacheStream.CopyToAsync(targetStream);
            }

            cacheFile.Delete();
            EditorUtility.RevealInFinder(m_decryptedFile.Path);
        }

    }

}
#endif