using System.Security.Cryptography;
using System.Text;
using SaveSystem.Cryptography;

namespace SaveSystem.Internal.CryptoProviders {

    internal class DefaultSaltProvider : IKeyProvider {

        private readonly string m_saltKey;


        internal DefaultSaltProvider (string saltKey) {
            m_saltKey = saltKey;
        }


        public byte[] GetKey () {
            return new SHA1Cng().ComputeHash(Encoding.UTF8.GetBytes(m_saltKey));
        }

    }

}