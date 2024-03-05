using System.Text;
using SaveSystem.Cryptography;

namespace SaveSystem.Internal.CryptoProviders {

    internal class DefaultSaltProvider : IKeyProvider<byte[]> {

        private readonly string m_saltKey;


        internal DefaultSaltProvider (string saltKey) {
            m_saltKey = saltKey;
        }


        public byte[] GetKey () {
            return Encoding.UTF8.GetBytes(m_saltKey);
        }

    }

}