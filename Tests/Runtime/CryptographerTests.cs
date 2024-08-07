#if IN_UNITY_PACKAGES_PROJECT
    using System.IO;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using SaveSystemPackage.Internal.Cryptography;
    using SaveSystemPackage.Security;
    using UnityEditor;
    using UnityEngine;

    namespace SaveSystemPackage.Tests {

        public class CryptographerTests {

            private readonly string m_sourcePath =
                Path.Combine(Application.dataPath, @"com.dducode.save-system\Tests\Runtime\TestResources\LoremIpsum.txt");

            private readonly string m_encryptPath =
                Path.Combine(Application.temporaryCachePath, "LoremIpsum_encrypted.txt");

            private readonly string m_decryptPath =
                Path.Combine(Application.temporaryCachePath, "LoremIpsum_decrypted.txt");


            [Test, Order(0)]
            public async Task EncryptLoremIpsum () {
                var cryptographer = Cryptographer.CreateInstance(
                    new DefaultKeyProvider("password"),
                    new DefaultKeyProvider("salt"),
                    KeyGenerationParams.Default
                );
                byte[] encrypted = cryptographer.Encrypt(await File.ReadAllBytesAsync(m_sourcePath));
                await File.WriteAllBytesAsync(m_encryptPath, encrypted);

                EditorUtility.RevealInFinder(m_encryptPath);
            }


            [Test, Order(1)]
            public async Task DecryptLoremIpsum () {
                var cryptographer = Cryptographer.CreateInstance(
                    new DefaultKeyProvider("password"),
                    new DefaultKeyProvider("salt"),
                    KeyGenerationParams.Default
                );
                byte[] decrypted = cryptographer.Decrypt(await File.ReadAllBytesAsync(m_encryptPath));
                await File.WriteAllBytesAsync(m_decryptPath, decrypted);

                EditorUtility.RevealInFinder(m_decryptPath);
            }

        }

    }
#endif