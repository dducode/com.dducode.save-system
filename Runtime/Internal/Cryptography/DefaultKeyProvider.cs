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
            return SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(m_key));
        }

    }

}