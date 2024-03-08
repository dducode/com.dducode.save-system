using System.Security.Cryptography;
using System.Text;
using SaveSystem.Cryptography;

namespace SaveSystem.Internal.Cryptography {

    internal class DefaultKeyProvider : IKeyProvider {

        private readonly string m_key;


        internal DefaultKeyProvider (string key) {
            m_key = key;
        }


        public byte[] GetKey () {
            return new SHA1Cng().ComputeHash(Encoding.UTF8.GetBytes(m_key));
        }

    }

}