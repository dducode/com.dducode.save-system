using System.Security.Cryptography;
using System.Text;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Internal.Cryptography {

    internal class DefaultKeyProvider : IKeyProvider {

        private readonly byte[] m_key;
        private readonly Cryptographer m_cryptographer;


        internal DefaultKeyProvider (string key) {
            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            m_cryptographer = Cryptographer.CreateInstance(
                new RandomSessionKeyProvider(),
                new RandomSessionKeyProvider(),
                KeyGenerationParams.Default
            );
            m_key = m_cryptographer.Encrypt(hash);
        }


        public byte[] GetKey () {
            return m_cryptographer.Decrypt(m_key);
        }

    }

}