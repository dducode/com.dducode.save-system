using System;
using SaveSystemPackage.Internal.Cryptography;

namespace SaveSystemPackage.Security {

    public abstract class EncodedObject<TObject> {

        public abstract TObject Value { get; set; }
        protected byte[] bytes;
        private readonly RandomSessionKeyProvider[] m_keyProviders;


        protected EncodedObject (int iterations = 1, int keyLength = 16) {
            m_keyProviders = new RandomSessionKeyProvider[iterations];
            for (var i = 0; i < m_keyProviders.Length; i++)
                m_keyProviders[i] = new RandomSessionKeyProvider(keyLength);
        }


        protected byte[] Encode (byte[] bytes) {
            var encodedBytes = new byte[bytes.Length];
            Array.Copy(bytes, encodedBytes, bytes.Length);

            foreach (RandomSessionKeyProvider provider in m_keyProviders) {
                Key key = provider.GetKey();

                for (int i = 0, j = 0; i < bytes.Length; i++, j++) {
                    if (j == key.value.Length)
                        j = 0;
                    encodedBytes[i] ^= key.value[j];
                }
            }

            return encodedBytes;
        }

    }

}