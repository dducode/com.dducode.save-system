using System.Security.Cryptography;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Internal.Security {

    internal class RandomSessionKeyProvider : IKeyProvider {

        private readonly byte[] m_randomKey;


        internal RandomSessionKeyProvider (int keyLength = 16) {
            m_randomKey = new byte[keyLength];
            RandomNumberGenerator.Fill(m_randomKey);
        }


        private RandomSessionKeyProvider (byte[] key) {
            m_randomKey = new byte[key.Length];
            key.CopyTo(m_randomKey, 0);
        }


        public Key GetKey () {
            return new Key(m_randomKey, true);
        }


        public IKeyProvider Clone () {
            return new RandomSessionKeyProvider(m_randomKey);
        }

    }

}