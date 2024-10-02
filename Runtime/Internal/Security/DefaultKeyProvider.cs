using System.Security.Cryptography;
using System.Text;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Internal.Security {

    internal class DefaultKeyProvider : ISecurityKeyProvider {

        private readonly byte[] m_key;
        private readonly Cryptographer m_cryptographer;


        internal DefaultKeyProvider (string key) {
            byte[] hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(key));
            m_cryptographer = new Cryptographer(
                new RandomSessionKeyProvider(),
                new RandomSessionKeyProvider(),
                KeyGenerationParams.Default
            );
            m_key = m_cryptographer.Encrypt(hash);
        }


        private DefaultKeyProvider (byte[] key, Cryptographer cryptographer) {
            m_key = new byte[key.Length];
            key.CopyTo(m_key, 0);
            m_cryptographer = cryptographer.Clone();
        }


        public Key GetKey () {
            return new Key(m_cryptographer.Decrypt(m_key));
        }


        public ISecurityKeyProvider Clone () {
            return new DefaultKeyProvider(m_key, m_cryptographer);
        }

    }

}