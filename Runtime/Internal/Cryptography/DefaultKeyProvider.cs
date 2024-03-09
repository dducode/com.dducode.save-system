using System;
using System.Security.Cryptography;
using System.Text;
using SaveSystem.Cryptography;

namespace SaveSystem.Internal.Cryptography {

    internal class DefaultKeyProvider : IKeyProvider {

        private readonly string m_key;


        internal DefaultKeyProvider (string key) {
            m_key = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key)));
        }


        public byte[] GetKey () {
            return Convert.FromBase64String(m_key);
        }

    }

}