using System.Security.Cryptography;
using System.Text;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Internal.Security {

    internal class SecurityKeyProvider : ISecurityKeyProvider {

        private readonly byte[] m_key;
        private readonly IEncryptor m_encryptor;


        internal SecurityKeyProvider (string key) {
            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            m_encryptor = new AesEncryptor(
                new RandomSessionKeyProvider(),
                new RandomSessionKeyProvider(),
                KeyGenerationParams.Default
            );
            m_key = m_encryptor.Encrypt(hash);
        }


        public Key GetKey () {
            return new Key(m_encryptor.Decrypt(m_key));
        }

    }

}