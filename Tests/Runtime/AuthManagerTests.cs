#if IN_UNITY_PACKAGES_PROJECT
    using System.IO;
    using NUnit.Framework;
    using SaveSystem.Security;
    using UnityEngine;

    namespace SaveSystem.Tests {

        public class AuthManagerTests {

            private const string AuthHashKey = "LoremIpsum";

            private readonly string m_sourcePath =
                Path.Combine(Application.dataPath, @"com.dducode.save-system\Tests\Runtime\TestResources\LoremIpsum.txt");


            [Test, Order(0)]
            public void SetLoremIpsumAuthHash () {
                var authManager = new AuthenticationManager(AuthHashKey, HashAlgorithmName.SHA1);
                authManager.SetAuthHash(File.ReadAllBytes(m_sourcePath));
            }


            [Test, Order(1)]
            public void AuthenticateLoremIpsumData () {
                var authManager = new AuthenticationManager(AuthHashKey, HashAlgorithmName.SHA1);
                authManager.AuthenticateData(File.ReadAllBytes(m_sourcePath));
                PlayerPrefs.DeleteKey(AuthHashKey);
            }

        }

    }
#endif