using System.Security.Cryptography;
using System.Text;
using SaveSystem.Cryptography;

namespace SaveSystem.Internal.CryptoProviders {

    internal class DefaultPasswordProvider : IKeyProvider {

        private readonly string m_password;


        internal DefaultPasswordProvider (string password) {
            m_password = password;
        }


        public byte[] GetKey () {
            return new SHA1Cng().ComputeHash(Encoding.UTF8.GetBytes(m_password));
        }

    }

}