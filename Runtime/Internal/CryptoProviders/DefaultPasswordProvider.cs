using SaveSystem.Cryptography;

namespace SaveSystem.Internal.CryptoProviders {

    internal class DefaultPasswordProvider : IKeyProvider<string> {

        private readonly string m_password;


        internal DefaultPasswordProvider (string password) {
            m_password = password;
        }


        public string GetKey () {
            return m_password;
        }

    }

}