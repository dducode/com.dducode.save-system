using System.Security.Cryptography;
using SaveSystemPackage.Security;

namespace SaveSystemPackage.Internal.Cryptography {

    internal class RandomSessionKeyProvider : IKeyProvider {

        private readonly byte[] m_randomKey;


        public RandomSessionKeyProvider () {
            m_randomKey = new byte[16];
            RandomNumberGenerator.Fill(m_randomKey);
        }


        public byte[] GetKey () {
            return m_randomKey;
        }

    }

}