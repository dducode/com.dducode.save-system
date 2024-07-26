#if IN_UNITY_PACKAGES_PROJECT
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SaveSystemPackage.Security;
using SaveSystemPackage.Verification;
using UnityEngine;

namespace SaveSystemPackage.Tests {

    public class VerificationManagerTests {

        private readonly string m_sourcePath =
            Path.Combine(Application.dataPath, @"com.dducode.save-system\Tests\Runtime\TestResources\LoremIpsum.txt");


        [Test, Order(0)]
        public async Task SetLoremIpsumAuthHash () {
            var verificationManager = new VerificationManager(new DefaultHashStorage(), HashAlgorithmName.SHA1);
            await verificationManager.SetChecksum(m_sourcePath, await File.ReadAllBytesAsync(m_sourcePath));
        }


        [Test, Order(1)]
        public async Task AuthenticateLoremIpsumData () {
            var verificationManager = new VerificationManager(new DefaultHashStorage(), HashAlgorithmName.SHA1);
            await verificationManager.VerifyData(m_sourcePath, await File.ReadAllBytesAsync(m_sourcePath));
        }

    }

}
#endif