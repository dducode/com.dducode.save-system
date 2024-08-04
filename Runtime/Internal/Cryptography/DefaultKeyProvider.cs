using System.Security.Cryptography;
using System.Text;
using SaveSystemPackage.Security;
using HashAlgorithmName = SaveSystemPackage.Security.HashAlgorithmName;

namespace SaveSystemPackage.Internal.Cryptography {

    internal class DefaultKeyProvider : IKeyProvider {

        private readonly byte[] m_key;
        private readonly Cryptographer m_cryptographer;


        internal DefaultKeyProvider (string key) {
            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            m_cryptographer = new Cryptographer(
                new RandomSessionKeyProvider(),
                new RandomSessionKeyProvider(),
                new KeyGenerationParams {
                    hashAlgorithm = HashAlgorithmName.SHA1,
                    keyLength = AESKeyLength._128Bit,
                    iterations = 10
                }
            );
            m_key = m_cryptographer.Encrypt(hash);
        }


        public byte[] GetKey () {
            return m_cryptographer.Decrypt(m_key);
        }

    }

}