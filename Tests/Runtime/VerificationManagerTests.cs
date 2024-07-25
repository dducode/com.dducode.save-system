﻿#if IN_UNITY_PACKAGES_PROJECT
using System.IO;
using NUnit.Framework;
using SaveSystemPackage.Security;
using UnityEngine;

namespace SaveSystemPackage.Tests {

    public class VerificationManagerTests {

        private readonly string m_sourcePath =
            Path.Combine(Application.dataPath, @"com.dducode.save-system\Tests\Runtime\TestResources\LoremIpsum.txt");


        [Test, Order(0)]
        public void SetLoremIpsumAuthHash () {
            var verificationManager = new VerificationManager(HashAlgorithmName.SHA1);
            verificationManager.SetChecksum(m_sourcePath, File.ReadAllBytes(m_sourcePath));
        }


        [Test, Order(1)]
        public void AuthenticateLoremIpsumData () {
            var verificationManager = new VerificationManager(HashAlgorithmName.SHA1);
            verificationManager.VerifyData(m_sourcePath, File.ReadAllBytes(m_sourcePath));
        }

    }

}
#endif