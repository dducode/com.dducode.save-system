using System.Security.Cryptography;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Internal.Cryptography {

    internal class RandomSessionKeyProvider : IKeyProvider {

        private readonly byte[] m_randomKey;


        internal RandomSessionKeyProvider (int keyLength = 16) {
            m_randomKey = new byte[keyLength];
            RandomNumberGenerator.Fill(m_randomKey);
        }


        private RandomSessionKeyProvider (byte[] key) {
            m_randomKey = new byte[key.Length];
            key.CopyTo(m_randomKey, key.Length);
        }


        public byte[] GetKey () {
            return m_randomKey;
        }


        public IKeyProvider Clone () {
            return new RandomSessionKeyProvider(m_randomKey);
        }

    }

}